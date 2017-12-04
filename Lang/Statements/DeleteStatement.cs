using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.Code;
using Vala.Lang.CodeNodes;
using Vala.Lang.Expressions;
using Vala.Lang.Parser;
using Vala.Lang.Types;

namespace Vala.Lang.Statements {
	/**
 * Represents a delete statement e.g. "delete a".
 */
	public class DeleteStatement : CodeNode, Statement {
		public CodeNode node {
			get { return this; }
		}

		/**
		 * Expression representing the instance to be freed.
		 */
		public Expression expression {
			get { return _expression; }
			set {
				_expression = value;
				_expression.parent_node = this;
			}
		}

		private Expression _expression;

		public DeleteStatement(Expression expression, SourceReference source_reference = null) {
			this.expression = expression;
			this.source_reference = source_reference;
		}

		public override void accept(CodeVisitor visitor) {
			visitor.visit_delete_statement(this);
		}

		public override void accept_children(CodeVisitor visitor) {
			expression.accept(visitor);
		}

		public override bool check(CodeContext context) {
			if (is_checked) {
				return !error;
			}

			is_checked = true;

			if (!expression.check(context)) {
				// if there was an error in the inner expression, skip this check
				return false;
			}

			if (!(expression.value_type is PointerType) && !(expression.value_type is ArrayType)) {
				error = true;
				Report.error(source_reference, "delete operator not supported for `%s'".printf(expression.value_type.to_string()));
			}

			return !error;
		}

		public override void emit(CodeGenerator codegen) {
			expression.emit(codegen);

			codegen.visit_delete_statement(this);
		}
	}
}
