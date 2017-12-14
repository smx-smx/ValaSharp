using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.Code;
using Vala.Lang.CodeNodes;
using Vala.Lang.Literals;
using Vala.Lang.Parser;
using Vala.Lang.Symbols;
using Vala.Lang.Types;
using Vala.Lang.TypeSymbols;
using static GLibPorts.GLib;

namespace Vala.Lang.Expressions {
	public class UnaryExpression : Expression {
		/// <summary>
		/// The unary operator.
		/// </summary>
		public UnaryOperator Operator { get; set; }

		/// <summary>
		/// The operand.
		/// </summary>
		public Expression inner {
			get {
				return _inner;
			}
			set {
				_inner = value;
				_inner.parent_node = this;
			}
		}

		private Expression _inner;

		/// <summary>
		/// Creates a new unary expression.
		/// 
		/// <param name="op">unary operator</param>
		/// <param name="_inner">operand</param>
		/// <param name="source">reference to source code</param>
		/// <returns>newly created binary expression</returns>
		/// </summary>
		public UnaryExpression(UnaryOperator op, Expression _inner, SourceReference source) {
			Operator = op;
			inner = _inner;
			source_reference = source;
		}

		public override void accept(CodeVisitor visitor) {
			visitor.visit_unary_expression(this);

			visitor.visit_expression(this);
		}

		public override void accept_children(CodeVisitor visitor) {
			inner.accept(visitor);
		}

		public override void replace_expression(Expression old_node, Expression new_node) {
			if (inner == old_node) {
				inner = new_node;
			}
		}

		private string get_operator_string() {
			switch (Operator) {
			case UnaryOperator.PLUS: return "+";
			case UnaryOperator.MINUS: return "-";
			case UnaryOperator.LOGICAL_NEGATION: return "!";
			case UnaryOperator.BITWISE_COMPLEMENT: return "~";
			case UnaryOperator.INCREMENT: return "++";
			case UnaryOperator.DECREMENT: return "--";
			case UnaryOperator.REF: return "ref ";
			case UnaryOperator.OUT: return "out ";
			}
			assert_not_reached();
			return null;
		}

		public override string ToString() {
			return get_operator_string() + _inner.ToString();
		}

		public override bool is_constant() {
			if (Operator == UnaryOperator.INCREMENT || Operator == UnaryOperator.DECREMENT) {
				return false;
			}

			if (Operator == UnaryOperator.REF || Operator == UnaryOperator.OUT) {
				var field = inner.symbol_reference as Field;
				if (field != null && field.binding == MemberBinding.STATIC) {
					return true;
				} else {
					return false;
				}
			}

			return inner.is_constant();
		}

		public override bool is_pure() {
			if (Operator == UnaryOperator.INCREMENT || Operator == UnaryOperator.DECREMENT) {
				return false;
			}

			return inner.is_pure();
		}

		public override bool is_accessible(Symbol sym) {
			return inner.is_accessible(sym);
		}

		bool is_numeric_type(DataType type) {
			if (!(type.data_type is Struct)) {
				return false;
			}

			var st = (Struct)type.data_type;
			return st.is_integer_type() || st.is_floating_type();
		}

		bool is_integer_type(DataType type) {
			if (!(type.data_type is Struct)) {
				return false;
			}

			var st = (Struct)type.data_type;
			return st.is_integer_type();
		}

		MemberAccess find_member_access(Expression expr) {
			if (expr is MemberAccess) {
				return (MemberAccess)expr;
			}

			return null;
		}

		public override bool check(CodeContext context) {
			if (is_checked) {
				return !error;
			}

			is_checked = true;

			if (Operator == UnaryOperator.REF || Operator == UnaryOperator.OUT) {
				inner.lvalue = true;
				inner.target_type = target_type;
			} else if (Operator == UnaryOperator.INCREMENT || Operator == UnaryOperator.DECREMENT) {
				inner.lvalue = true;
			}

			if (!inner.check(context)) {
				/* if there was an error in the inner expression, skip type check */
				error = true;
				return false;
			}

			if (inner.value_type is FieldPrototype) {
				error = true;
				Report.error(inner.source_reference, "Access to instance member `%s' denied".printf(inner.symbol_reference.get_full_name()));
				return false;
			}

			if (Operator == UnaryOperator.PLUS || Operator == UnaryOperator.MINUS) {
				// integer or floating point type
				if (!is_numeric_type(inner.value_type)) {
					error = true;
					Report.error(source_reference, "Operator not supported for `%s'".printf(inner.value_type.ToString()));
					return false;
				}

				value_type = inner.value_type;
			} else if (Operator == UnaryOperator.LOGICAL_NEGATION) {
				// boolean type
				if (!inner.value_type.compatible(context.analyzer.bool_type)) {
					error = true;
					Report.error(source_reference, "Operator not supported for `%s'".printf(inner.value_type.ToString()));
					return false;
				}

				value_type = inner.value_type;
			} else if (Operator == UnaryOperator.BITWISE_COMPLEMENT) {
				// integer type
				if (!is_integer_type(inner.value_type) && !(inner.value_type is EnumValueType)) {
					error = true;
					Report.error(source_reference, "Operator not supported for `%s'".printf(inner.value_type.ToString()));
					return false;
				}

				value_type = inner.value_type;
			} else if (Operator == UnaryOperator.INCREMENT ||
						   Operator == UnaryOperator.DECREMENT) {
				// integer type
				if (!is_integer_type(inner.value_type)) {
					error = true;
					Report.error(source_reference, "Operator not supported for `%s'".printf(inner.value_type.ToString()));
					return false;
				}

				var ma = find_member_access(inner);
				if (ma == null) {
					error = true;
					Report.error(source_reference, "Prefix operators not supported for this expression");
					return false;
				}

				var old_value = new MemberAccess(ma.inner, ma.member_name, inner.source_reference);
				var bin = new BinaryExpression(Operator == UnaryOperator.INCREMENT ? BinaryOperator.PLUS : BinaryOperator.MINUS, old_value, new IntegerLiteral("1"), source_reference);

				var assignment = new Assignment(ma, bin, AssignmentOperator.SIMPLE, source_reference);
				assignment.target_type = target_type;
				context.analyzer.replaced_nodes.Add(this);
				parent_node.replace_expression(this, assignment);
				assignment.check(context);
				return true;
			} else if (Operator == UnaryOperator.REF || Operator == UnaryOperator.OUT) {
				var ea = inner as ElementAccess;
				if (inner.symbol_reference is Field || inner.symbol_reference is Parameter || inner.symbol_reference is LocalVariable ||
					(ea != null && ea.container.value_type is ArrayType)) {
					// ref and out can only be used with fields, parameters, local variables, and array element access
					lvalue = true;
					value_type = inner.value_type;
				} else {
					error = true;
					Report.error(source_reference, "ref and out method arguments can only be used with fields, parameters, local variables, and array element access");
					return false;
				}
			} else {
				error = true;
				Report.error(source_reference, "internal error: unsupported unary operator");
				return false;
			}

			return !error;
		}

		public override void emit(CodeGenerator codegen) {
			inner.emit(codegen);

			codegen.visit_unary_expression(this);

			codegen.visit_expression(this);
		}

		public override void get_defined_variables(ICollection<Variable> collection) {
			inner.get_defined_variables(collection);
			if (Operator == UnaryOperator.OUT || Operator == UnaryOperator.REF) {
				var local = inner.symbol_reference as LocalVariable;
				var param = inner.symbol_reference as Parameter;
				if (local != null) {
					collection.Add(local);
				}
				if (param != null && param.direction == ParameterDirection.OUT) {
					collection.Add(param);
				}
			}
		}

		public override void get_used_variables(ICollection<Variable> collection) {
			if (Operator != UnaryOperator.OUT) {
				inner.get_used_variables(collection);
			}
		}
	}

	public enum UnaryOperator {
		NONE,
		PLUS,
		MINUS,
		LOGICAL_NEGATION,
		BITWISE_COMPLEMENT,
		INCREMENT,
		DECREMENT,
		REF,
		OUT
	}
}
