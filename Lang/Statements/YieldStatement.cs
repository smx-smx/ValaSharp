using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.Code;
using Vala.Lang.CodeNodes;
using Vala.Lang.Expressions;
using Vala.Lang.Parser;

namespace Vala.Lang.Statements
{
	/**
 * Represents a yield statement in the source code.
 */
	public class YieldStatement : CodeNode, Statement
	{
		public CodeNode node {
			get { return this; }
		}

		/**
		 * The expression to yield or the method call to yield to.
		 */
		public Expression yield_expression {
			get { return _yield_expression; }
			set {
				_yield_expression = value;
				if (_yield_expression != null) {
					_yield_expression.parent_node = this;
				}
			}
		}

		private Expression _yield_expression;

		/**
		 * Creates a new yield statement.
		 *
		 * @param yield_expression the yield expression
		 * @param source_reference reference to source code
		 * @return                 newly created yield statement
		 */
		public YieldStatement(Expression yield_expression, SourceReference source_reference = null) {
			this.yield_expression = yield_expression;
			this.source_reference = source_reference;
		}

		public override void accept(CodeVisitor visitor) {
			visitor.visit_yield_statement(this);
		}

		public override void accept_children(CodeVisitor visitor) {
			if (yield_expression != null) {
				yield_expression.accept(visitor);

				visitor.visit_end_full_expression(yield_expression);
			}
		}

		public override void replace_expression(Expression old_node, Expression new_node) {
			if (yield_expression == old_node) {
				yield_expression = new_node;
			}
		}

		public override bool check(CodeContext context) {
			if (yield_expression != null) {
				yield_expression.check(context);
				error = yield_expression.error;
			}

			return !error;
		}

		public override void emit(CodeGenerator codegen) {
			if (yield_expression != null) {
				yield_expression.emit(codegen);

				codegen.visit_end_full_expression(yield_expression);
			}

			codegen.visit_yield_statement(this);
		}
	}


}
