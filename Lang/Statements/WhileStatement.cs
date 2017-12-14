using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.CodeNodes;
using Vala.Lang.Expressions;
using Vala.Lang.Literals;
using Vala.Lang.Parser;

namespace Vala.Lang.Statements {
	/// <summary>
	/// Represents a while iteration statement in the source code.
	/// </summary>
	public class WhileStatement : CodeNode, Statement {
		public CodeNode node {
			get { return this; }
		}

		/// <summary>
		/// Specifies the loop condition.
		/// </summary>
		public Expression condition {
			get {
				return _condition;
			}
			set {
				_condition = value;
				_condition.parent_node = this;
			}
		}

		/// <summary>
		/// Specifies the loop body.
		/// </summary>
		public Block body {
			get {
				return _body;
			}
			set {
				_body = value;
				_body.parent_node = this;
			}
		}

		private Expression _condition;
		private Block _body;

		/// <summary>
		/// Creates a new while statement.
		/// 
		/// <param name="condition">loop condition</param>
		/// <param name="body">loop body</param>
		/// <param name="source_reference">reference to source code</param>
		/// <returns>newly created while statement</returns>
		/// </summary>
		public WhileStatement(Expression condition, Block body, SourceReference source_reference = null) {
			this.body = body;
			this.source_reference = source_reference;
			this.condition = condition;
		}

		public override void accept(CodeVisitor visitor) {
			visitor.visit_while_statement(this);
		}

		public override void accept_children(CodeVisitor visitor) {
			condition.accept(visitor);

			visitor.visit_end_full_expression(condition);

			body.accept(visitor);
		}

		bool always_true(Expression condition) {
			var literal = condition as BooleanLiteral;
			return (literal != null && literal.value);
		}

		bool always_false(Expression condition) {
			var literal = condition as BooleanLiteral;
			return (literal != null && !literal.value);
		}

		public override bool check(CodeContext context) {
			// convert to simple loop

			if (always_true(condition)) {
				// do not generate if block if condition is always true
			} else if (always_false(condition)) {
				// do not generate if block if condition is always false
				body.insert_statement(0, new BreakStatement(condition.source_reference));
			} else {
				var if_condition = new UnaryExpression(UnaryOperator.LOGICAL_NEGATION, condition, condition.source_reference);
				var true_block = new Block(condition.source_reference);
				true_block.add_statement(new BreakStatement(condition.source_reference));
				var if_stmt = new IfStatement(if_condition, true_block, null, condition.source_reference);
				body.insert_statement(0, if_stmt);
			}

			var loop = new Loop(body, source_reference);

			var parent_block = (Block)parent_node;
			parent_block.replace_statement(this, loop);

			return loop.check(context);
		}
	}
}
