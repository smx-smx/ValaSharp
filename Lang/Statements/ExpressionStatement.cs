using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.Code;
using Vala.Lang.CodeNodes;
using Vala.Lang.Expressions;
using Vala.Lang.Parser;
using Vala.Lang.Symbols;

namespace Vala.Lang.Statements {
	public class ExpressionStatement : CodeNode, Statement {
		public CodeNode node {
			get { return this; }
		}

		/**
	 * Specifies the expression to evaluate.
	 */
		public Expression expression {
			get {
				return _expression;
			}
			set {
				_expression = value;
				_expression.parent_node = this;
			}
		}

		private Expression _expression;

		/**
		 * Creates a new expression statement.
		 *
		 * @param expression        expression to evaluate
		 * @param source_reference  reference to source code
		 * @return                  newly created expression statement
		 */
		public ExpressionStatement(Expression expression, SourceReference source_reference = null) {
			this.source_reference = source_reference;
			this.expression = expression;
		}

		public override void accept(CodeVisitor visitor) {
			visitor.visit_expression_statement(this);
		}

		public override void accept_children(CodeVisitor visitor) {
			expression.accept(visitor);
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

			if (!expression.check(context)) {
				// ignore inner error
				error = true;
				return false;
			}

			add_error_types(expression.get_error_types());

			return !error;
		}

		public override void emit(CodeGenerator codegen) {
			expression.emit(codegen);

			codegen.visit_expression_statement(this);
		}

		public override void get_defined_variables(ICollection<Variable> collection) {
			expression.get_defined_variables(collection);
		}

		public override void get_used_variables(ICollection<Variable> collection) {
			expression.get_used_variables(collection);
		}
	}
}
