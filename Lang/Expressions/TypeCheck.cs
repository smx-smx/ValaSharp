using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.Code;
using Vala.Lang.CodeNodes;
using Vala.Lang.Parser;
using Vala.Lang.Types;

namespace Vala.Lang.Expressions {
	/// <summary>
	/// Represents a type check (`is`) expression in the source code.
	/// </summary>
	public class TypeCheck : Expression {
		/// <summary>
		/// The expression to be checked.
		/// </summary>
		public Expression expression {
			get { return _expression; }
			set {
				_expression = value;
				_expression.parent_node = this;
			}
		}

		/// <summary>
		/// The type to be matched against.
		/// </summary>
		public DataType type_reference {
			get { return _data_type; }
			set {
				_data_type = value;
				_data_type.parent_node = this;
			}
		}

		Expression _expression;
		private DataType _data_type;

		/// <summary>
		/// Creates a new type check expression.
		/// 
		/// <param name="expr">an expression</param>
		/// <param name="type">a data type</param>
		/// <param name="source">reference to source code</param>
		/// <returns>newly created type check expression</returns>
		/// </summary>
		public TypeCheck(Expression expr, DataType type, SourceReference source) {
			expression = expr;
			type_reference = type;
			source_reference = source;
		}

		public override void accept(CodeVisitor visitor) {
			visitor.visit_type_check(this);

			visitor.visit_expression(this);
		}

		public override void accept_children(CodeVisitor visitor) {
			expression.accept(visitor);

			type_reference.accept(visitor);
		}

		public override bool is_pure() {
			return expression.is_pure();
		}

		public override void replace_type(DataType old_type, DataType new_type) {
			if (type_reference == old_type) {
				type_reference = new_type;
			}
		}

		public override void replace_expression(Expression old_node, Expression new_node) {
			if (expression == old_node) {
				expression = new_node;
			}
		}

		public override bool check(CodeContext context) {
			if (is_checked) {
				return !error;
			}

			is_checked = true;

			expression.check(context);

			type_reference.check(context);

			if (expression.value_type == null) {
				Report.error(expression.source_reference, "invalid left operand");
				error = true;
				return false;
			}

			if (type_reference.data_type == null) {
				/* if type resolving didn't succeed, skip this check */
				error = true;
				return false;
			}

			if (type_reference.get_type_arguments().Count > 0) {
				Report.warning(_data_type.source_reference, "Type argument list has no effect");
			}

			value_type = context.analyzer.bool_type;

			return !error;
		}

		public override void emit(CodeGenerator codegen) {
			expression.emit(codegen);

			codegen.visit_type_check(this);

			codegen.visit_expression(this);
		}
	}
}
