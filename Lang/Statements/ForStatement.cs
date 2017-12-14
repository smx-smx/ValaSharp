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
	/// Represents a for iteration statement in the source code.
	/// </summary>
	public class ForStatement : CodeNode, Statement {
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
				if (_condition != null) {
					_condition.parent_node = this;
				}
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

		private List<Expression> initializer = new List<Expression>();
		private List<Expression> iterator = new List<Expression>();

		private Expression _condition;
		private Block _body;

		/// <summary>
		/// Creates a new for statement.
		/// 
		/// <param name="condition">loop condition</param>
		/// <param name="body">loop body</param>
		/// <param name="source_reference">reference to source code</param>
		/// <returns>newly created for statement</returns>
		/// </summary>
		public ForStatement(Expression condition, Block body, SourceReference source_reference = null) {
			this.condition = condition;
			this.body = body;
			this.source_reference = source_reference;
		}

		/// <summary>
		/// Appends the specified expression to the list of initializers.
		/// 
		/// <param name="init">an initializer expression</param>
		/// </summary>
		public void add_initializer(Expression init) {
			init.parent_node = this;
			initializer.Add(init);
		}

		/// <summary>
		/// Returns a copy of the list of initializers.
		/// 
		/// <returns>initializer list</returns>
		/// </summary>
		public List<Expression> get_initializer() {
			return initializer;
		}

		/// <summary>
		/// Appends the specified expression to the iterator.
		/// 
		/// <param name="iter">an iterator expression</param>
		/// </summary>
		public void add_iterator(Expression iter) {
			iter.parent_node = this;
			iterator.Add(iter);
		}

		/// <summary>
		/// Returns a copy of the iterator.
		/// 
		/// <returns>iterator</returns>
		/// </summary>
		public List<Expression> get_iterator() {
			return iterator;
		}

		public override void accept(CodeVisitor visitor) {
			visitor.visit_for_statement(this);
		}

		public override void accept_children(CodeVisitor visitor) {
			foreach (Expression init_expr in initializer) {
				init_expr.accept(visitor);
				visitor.visit_end_full_expression(init_expr);
			}

			if (condition != null) {
				condition.accept(visitor);

				visitor.visit_end_full_expression(condition);
			}

			foreach (Expression it_expr in iterator) {
				it_expr.accept(visitor);
				visitor.visit_end_full_expression(it_expr);
			}

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

			var block = new Block(source_reference);

			// initializer
			foreach (var init_expr in initializer) {
				block.add_statement(new ExpressionStatement(init_expr, init_expr.source_reference));
			}

			// do not generate if block if condition is always true
			if (condition == null || always_true(condition)) {
			} else if (always_false(condition)) {
				// do not generate if block if condition is always false
				body.insert_statement(0, new BreakStatement(condition.source_reference));
			} else {
				// condition
				var if_condition = new UnaryExpression(UnaryOperator.LOGICAL_NEGATION, condition, condition.source_reference);
				var true_block = new Block(condition.source_reference);
				true_block.add_statement(new BreakStatement(condition.source_reference));
				var if_stmt = new IfStatement(if_condition, true_block, null, condition.source_reference);
				body.insert_statement(0, if_stmt);
			}

			// iterator
			var first_local = new LocalVariable(context.analyzer.bool_type.copy(), get_temp_name(), new BooleanLiteral(true, source_reference), source_reference);
			block.add_statement(new DeclarationStatement(first_local, source_reference));

			var iterator_block = new Block(source_reference);
			foreach (var it_expr in iterator) {
				iterator_block.add_statement(new ExpressionStatement(it_expr, it_expr.source_reference));
			}

			var first_if = new IfStatement(new UnaryExpression(UnaryOperator.LOGICAL_NEGATION, MemberAccess.simple(first_local.name, source_reference), source_reference), iterator_block, null, source_reference);
			body.insert_statement(0, first_if);
			body.insert_statement(1, new ExpressionStatement(new Assignment(MemberAccess.simple(first_local.name, source_reference), new BooleanLiteral(false, source_reference), AssignmentOperator.SIMPLE, source_reference), source_reference));

			block.add_statement(new Loop(body, source_reference));

			var parent_block = (Block)parent_node;
			parent_block.replace_statement(this, block);

			return block.check(context);
		}
	}

}
