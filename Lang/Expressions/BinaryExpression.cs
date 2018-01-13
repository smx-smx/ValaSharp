using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.Code;
using Vala.Lang.CodeNodes;
using Vala.Lang.Literals;
using Vala.Lang.Methods;
using Vala.Lang.Parser;
using Vala.Lang.Statements;
using Vala.Lang.Symbols;
using Vala.Lang.Types;
using Vala.Lang.TypeSymbols;
using static GLibPorts.GLib;

namespace Vala.Lang.Expressions {
	/// <summary>
	/// Represents an expression with two operands in the source code.
	/// 
	/// Supports +, -, *, /, %, <<, >>, <, >, <=, >=, ==, !=, &, |, ^, &&, ||, ??.
	/// </summary>
	public class BinaryExpression : Expression {
		/// <summary>
		/// The binary operator.
		/// </summary>
		public BinaryOperator Operator { get; set; }

		/// <summary>
		/// The left operand.
		/// </summary>
		public Expression left {
			get {
				return _left;
			}
			set {
				_left = value;
				_left.parent_node = this;
			}
		}

		/// <summary>
		/// The right operand.
		/// </summary>
		public Expression right {
			get {
				return _right;
			}
			set {
				_right = value;
				_right.parent_node = this;
			}
		}

		public bool chained;

		private Expression _left;
		private Expression _right;

		/// <summary>
		/// Creates a new binary expression.
		/// 
		/// <param name="op">binary operator</param>
		/// <param name="_left">left operand</param>
		/// <param name="_right">right operand</param>
		/// <param name="source">reference to source code</param>
		/// <returns>newly created binary expression</returns>
		/// </summary>
		public BinaryExpression(BinaryOperator op, Expression _left, Expression _right, SourceReference source = null) {
			Operator = op;
			left = _left;
			right = _right;
			source_reference = source;
		}

		public override void accept(CodeVisitor visitor) {
			visitor.visit_binary_expression(this);

			visitor.visit_expression(this);
		}

		public override void accept_children(CodeVisitor visitor) {
			left.accept(visitor);
			right.accept(visitor);
		}

		public override void replace_expression(Expression old_node, Expression new_node) {
			if (left == old_node) {
				left = new_node;
			}
			if (right == old_node) {
				right = new_node;
			}
		}

		private string get_operator_string() {
			switch (Operator) {
			case BinaryOperator.PLUS: return "+";
			case BinaryOperator.MINUS: return "-";
			case BinaryOperator.MUL: return "*";
			case BinaryOperator.DIV: return "/";
			case BinaryOperator.MOD: return "%";
			case BinaryOperator.SHIFT_LEFT: return "<<";
			case BinaryOperator.SHIFT_RIGHT: return ">>";
			case BinaryOperator.LESS_THAN: return "<";
			case BinaryOperator.GREATER_THAN: return ">";
			case BinaryOperator.LESS_THAN_OR_EQUAL: return "<=";
			case BinaryOperator.GREATER_THAN_OR_EQUAL: return ">=";
			case BinaryOperator.EQUALITY: return "==";
			case BinaryOperator.INEQUALITY: return "!=";
			case BinaryOperator.BITWISE_AND: return "&";
			case BinaryOperator.BITWISE_OR: return "|";
			case BinaryOperator.BITWISE_XOR: return "^";
			case BinaryOperator.AND: return "&&";
			case BinaryOperator.OR: return "||";
			case BinaryOperator.IN: return "in";
			case BinaryOperator.COALESCE: return "??";
			}
			assert_not_reached();
			return null;
		}

		public override string ToString() {
			return _left.ToString() + get_operator_string() + _right.ToString();
		}

		public override bool is_constant() {
			return left.is_constant() && right.is_constant();
		}

		public override bool is_pure() {
			return left.is_pure() && right.is_pure();
		}

		public override bool is_non_null() {
			return left.is_non_null() && right.is_non_null();
		}

		public override bool is_accessible(Symbol sym) {
			return left.is_accessible(sym) && right.is_accessible(sym);
		}

		public override bool check(CodeContext context) {
			if (is_checked) {
				return !error;
			}

			is_checked = true;

			// some expressions are not in a block,
			// for example, expressions in method contracts
			if (context.analyzer.current_symbol is Block
				&& (Operator == BinaryOperator.AND || Operator == BinaryOperator.OR)) {
				// convert conditional expression into if statement
				// required for flow analysis and exception handling

				var local = new LocalVariable(context.analyzer.bool_type.copy(), get_temp_name(), null, source_reference);
				var decl = new DeclarationStatement(local, source_reference);
				decl.check(context);

				var right_stmt = new ExpressionStatement(
					new Assignment(
						MemberAccess.simple(local.name, right.source_reference),
						right,
						AssignmentOperator.SIMPLE,
						right.source_reference
					), right.source_reference
				);

				var stmt = new ExpressionStatement(
					new Assignment(
						MemberAccess.simple(local.name, left.source_reference),
						new BooleanLiteral((Operator == BinaryOperator.OR), left.source_reference),
						AssignmentOperator.SIMPLE, left.source_reference
					), left.source_reference
				);

				var true_block = new Block(source_reference);
				var false_block = new Block(source_reference);

				if (Operator == BinaryOperator.AND) {
					true_block.add_statement(right_stmt);
					false_block.add_statement(stmt);
				} else {
					true_block.add_statement(stmt);
					false_block.add_statement(right_stmt);
				}

				var if_stmt = new IfStatement(left, true_block, false_block, source_reference);

				insert_statement(context.analyzer.insert_block, decl);
				insert_statement(context.analyzer.insert_block, if_stmt);

				if (!if_stmt.check(context)) {
					error = true;
					return false;
				}

				var ma = MemberAccess.simple(local.name, source_reference);
				ma.target_type = target_type;
				ma.formal_target_type = formal_target_type;
				ma.check(context);

				parent_node.replace_expression(this, ma);

				return true;
			}

			if (Operator == BinaryOperator.COALESCE) {
				if (!left.check(context)) {
					error = true;
					return false;
				}

				if (!right.check(context)) {
					error = true;
					return false;
				}

				DataType local_type = null;
				bool cast_non_null = false;
				if (left.value_type is NullType && right.value_type != null) {
					Report.warning(left.source_reference, "left operand is always null");
					local_type = right.value_type.copy();
					local_type.nullable = true;
					if (!right.value_type.nullable) {
						cast_non_null = true;
					}
				} else if (left.value_type != null) {
					local_type = left.value_type.copy();
					if (right.value_type != null && right.value_type.value_owned) {
						// value owned if either left or right is owned
						local_type.value_owned = true;
					}
					if (context.experimental_non_null) {
						if (!local_type.nullable) {
							Report.warning(left.source_reference, "left operand is never null");
							if (right.value_type != null && right.value_type.nullable) {
								local_type.nullable = true;
								cast_non_null = true;
							}
						} else if (right.value_type != null && !right.value_type.nullable) {
							cast_non_null = true;
						}
					}
				} else if (right.value_type != null) {
					local_type = right.value_type.copy();
				}

				var local = new LocalVariable(local_type, get_temp_name(), left, source_reference);
				var decl = new DeclarationStatement(local, source_reference);

				var right_stmt = new ExpressionStatement(new Assignment(MemberAccess.simple(local.name, right.source_reference), right, AssignmentOperator.SIMPLE, right.source_reference), right.source_reference);

				var true_block = new Block(source_reference);

				true_block.add_statement(right_stmt);

				var cond = new BinaryExpression(BinaryOperator.EQUALITY, MemberAccess.simple(local.name, left.source_reference), new NullLiteral(source_reference), source_reference);

				var if_stmt = new IfStatement(cond, true_block, null, source_reference);

				insert_statement(context.analyzer.insert_block, decl);
				insert_statement(context.analyzer.insert_block, if_stmt);

				if (!decl.check(context)) {
					error = true;
					return false;
				}

				if (!if_stmt.check(context)) {
					error = true;
					return false;
				}

				var replace_expr = SemanticAnalyzer.create_temp_access(local, target_type);
				if (cast_non_null && replace_expr.target_type != null) {
					var cast = CastExpression.non_null(replace_expr, source_reference);
					cast.target_type = replace_expr.target_type.copy();
					cast.target_type.nullable = false;
					replace_expr = cast;
				}
				replace_expr.check(context);

				parent_node.replace_expression(this, replace_expr);

				return true;
			}

			if (!left.check(context) || !right.check(context)) {
				/* if there were any errors in inner expressions, skip type check */
				error = true;
				return false;
			}

			if (left.value_type == null) {
				Report.error(left.source_reference, "invalid left operand");
				error = true;
				return false;
			}

			if (Operator != BinaryOperator.IN && right.value_type == null) {
				Report.error(right.source_reference, "invalid right operand");
				error = true;
				return false;
			}

			if (left.value_type is FieldPrototype) {
				error = true;
				Report.error(left.source_reference, "Access to instance member `%s' denied".printf(left.symbol_reference.get_full_name()));
				return false;
			}
			if (right.value_type is FieldPrototype) {
				error = true;
				Report.error(right.source_reference, "Access to instance member `%s' denied".printf(right.symbol_reference.get_full_name()));
				return false;
			}

			left.target_type = left.value_type.copy();
			left.target_type.value_owned = false;
			right.target_type = right.value_type.copy();
			right.target_type.value_owned = false;

			if (left.value_type.data_type == context.analyzer.string_type.data_type
				&& Operator == BinaryOperator.PLUS) {
				// string concatenation

				if (right.value_type == null || right.value_type.data_type != context.analyzer.string_type.data_type) {
					error = true;
					Report.error(source_reference, "Operands must be strings");
					return false;
				}

				value_type = context.analyzer.string_type.copy();
				if (left.is_constant() && right.is_constant()) {
					value_type.value_owned = false;
				} else {
					value_type.value_owned = true;
				}
			} else if (left.value_type is ArrayType && Operator == BinaryOperator.PLUS) {
				// array concatenation

				var array_type = (ArrayType)left.value_type;

				if (right.value_type == null || !right.value_type.compatible(array_type.element_type)) {
					error = true;
					Report.error(source_reference, "Incompatible operand");
					return false;
				}

				right.target_type = array_type.element_type.copy();

				value_type = array_type.copy();
				value_type.value_owned = true;
			} else if (
				Operator == BinaryOperator.PLUS ||
				Operator == BinaryOperator.MINUS ||
				Operator == BinaryOperator.MUL ||
				Operator == BinaryOperator.DIV
			) {
				// check for pointer arithmetic
				if (left.value_type is PointerType) {
					var pointer_type = (PointerType)left.value_type;
					if (pointer_type.base_type is VoidType) {
						error = true;
						Report.error(source_reference, "Pointer arithmetic not supported for `void*'");
						return false;
					}

					var offset_type = right.value_type.data_type as Struct;
					if (offset_type != null && offset_type.is_integer_type()) {
						if (Operator == BinaryOperator.PLUS
							|| Operator == BinaryOperator.MINUS) {
							// pointer arithmetic: pointer +/- offset
							value_type = left.value_type.copy();
						}
					} else if (right.value_type is PointerType) {
						// pointer arithmetic: pointer - pointer
						value_type = context.analyzer.size_t_type;
					}
				} else {
					left.target_type.nullable = false;
					right.target_type.nullable = false;
				}

				if (value_type == null) {
					value_type = context.analyzer.get_arithmetic_result_type(left.target_type, right.target_type);
				}

				if (value_type == null) {
					error = true;
					Report.error(source_reference, "Arithmetic operation not supported for types `%s' and `%s'".printf(left.value_type.ToString(), right.value_type.ToString()));
					return false;
				}
			} else if (
				Operator == BinaryOperator.MOD ||
				Operator == BinaryOperator.SHIFT_LEFT ||
				Operator == BinaryOperator.SHIFT_RIGHT
			) {
				left.target_type.nullable = false;
				right.target_type.nullable = false;

				value_type = context.analyzer.get_arithmetic_result_type(left.target_type, right.target_type);

				if (value_type == null) {
					error = true;
					Report.error(source_reference, "Arithmetic operation not supported for types `%s' and `%s'".printf(left.value_type.ToString(), right.value_type.ToString()));
					return false;
				}
			} else if (Operator == BinaryOperator.LESS_THAN ||
				Operator == BinaryOperator.GREATER_THAN ||
				Operator == BinaryOperator.LESS_THAN_OR_EQUAL ||
				Operator == BinaryOperator.GREATER_THAN_OR_EQUAL
			) {
				if (left.value_type.compatible(context.analyzer.string_type)
					&& right.value_type.compatible(context.analyzer.string_type)) {
					// string comparison
				} else if (left.value_type is PointerType && right.value_type is PointerType) {
					// pointer arithmetic
				} else {
					DataType resulting_type;

					if (chained) {
						var lbe = (BinaryExpression)left;
						resulting_type = context.analyzer.get_arithmetic_result_type(lbe.right.target_type, right.target_type);
					} else {
						resulting_type = context.analyzer.get_arithmetic_result_type(left.target_type, right.target_type);
					}

					if (resulting_type == null) {
						error = true;
						Report.error(source_reference, "Relational operation not supported for types `%s' and `%s'".printf(left.value_type.ToString(), right.value_type.ToString()));
						return false;
					}

					if (!chained) {
						left.target_type = resulting_type.copy();
					}
					right.target_type = resulting_type.copy();
					left.target_type.nullable = false;
					right.target_type.nullable = false;
				}

				value_type = context.analyzer.bool_type;
			} else if (
				Operator == BinaryOperator.EQUALITY ||
				Operator == BinaryOperator.INEQUALITY
			) {
				/* relational operation */

				if (!right.value_type.compatible(left.value_type)
					&& !left.value_type.compatible(right.value_type)) {
					Report.error(source_reference, "Equality operation: `%s' and `%s' are incompatible".printf(right.value_type.ToString(), left.value_type.ToString()));
					error = true;
					return false;
				}

				var resulting_type = context.analyzer.get_arithmetic_result_type(left.target_type, right.target_type);
				if (resulting_type != null) {
					// numeric operation
					left.target_type = resulting_type.copy();
					right.target_type = resulting_type.copy();
				}

				left.target_type.value_owned = false;
				right.target_type.value_owned = false;

				if (left.value_type.nullable != right.value_type.nullable) {
					// if only one operand is nullable, make sure the other
					// operand is promoted to nullable as well,
					// reassign both, as get_arithmetic_result_type doesn't
					// take nullability into account
					left.target_type.nullable = true;
					right.target_type.nullable = true;
				}

				value_type = context.analyzer.bool_type;
			} else if (
				Operator == BinaryOperator.BITWISE_AND ||
				Operator == BinaryOperator.BITWISE_OR ||
				Operator == BinaryOperator.BITWISE_XOR
			) {
				// integer type or flags type
				left.target_type.nullable = false;
				right.target_type.nullable = false;

				value_type = left.target_type.copy();
			} else if (
				Operator == BinaryOperator.AND ||
				Operator == BinaryOperator.OR
			) {
				if (!left.value_type.compatible(context.analyzer.bool_type) || !right.value_type.compatible(context.analyzer.bool_type)) {
					error = true;
					Report.error(source_reference, "Operands must be boolean");
				}
				left.target_type.nullable = false;
				right.target_type.nullable = false;

				value_type = context.analyzer.bool_type;
			} else if (Operator == BinaryOperator.IN) {
				if (left.value_type.compatible(context.analyzer.int_type)
					&& right.value_type.compatible(context.analyzer.int_type)) {
					// integers or enums
					left.target_type.nullable = false;
					right.target_type.nullable = false;
				} else if (right.value_type is ArrayType) {
					if (!left.value_type.compatible(((ArrayType)right.value_type).element_type)) {
						Report.error(source_reference, "Cannot look for `%s' in `%s'".printf(left.value_type.ToString(), right.value_type.ToString()));
					}
				} else {
					// otherwise require a bool contains () method
					var contains_method = right.value_type.get_member("contains") as Method;
					if (contains_method == null) {
						Report.error(source_reference, "`%s' does not have a `contains' method".printf(right.value_type.ToString()));
						error = true;
						return false;
					}
					if (contains_method.get_parameters().Count != 1) {
						Report.error(source_reference, "`%s' must have one parameter".printf(contains_method.get_full_name()));
						error = true;
						return false;
					}
					if (!contains_method.return_type.compatible(context.analyzer.bool_type)) {
						Report.error(source_reference, "`%s' must return a boolean value".printf(contains_method.get_full_name()));
						error = true;
						return false;
					}

					var contains_call = new MethodCall(new MemberAccess(right, "contains", source_reference), source_reference);
					contains_call.add_argument(left);
					parent_node.replace_expression(this, contains_call);
					return contains_call.check(context);
				}

				value_type = context.analyzer.bool_type;

			} else {
				assert_not_reached();
			}

			return !error;
		}

		public override void emit(CodeGenerator codegen) {
			left.emit(codegen);
			right.emit(codegen);

			codegen.visit_binary_expression(this);

			codegen.visit_expression(this);
		}

		public override void get_defined_variables(ICollection<Variable> collection) {
			left.get_defined_variables(collection);
			right.get_defined_variables(collection);
		}

		public override void get_used_variables(ICollection<Variable> collection) {
			left.get_used_variables(collection);
			right.get_used_variables(collection);
		}
	}

	public enum BinaryOperator {
		NONE,
		PLUS,
		MINUS,
		MUL,
		DIV,
		MOD,
		SHIFT_LEFT,
		SHIFT_RIGHT,
		LESS_THAN,
		GREATER_THAN,
		LESS_THAN_OR_EQUAL,
		GREATER_THAN_OR_EQUAL,
		EQUALITY,
		INEQUALITY,
		BITWISE_AND,
		BITWISE_OR,
		BITWISE_XOR,
		AND,
		OR,
		IN,
		COALESCE
	}
}
