﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.CodeNodes;
using Vala.Lang.Parser;
using Vala.Lang.Statements;
using Vala.Lang.Symbols;

namespace Vala.Lang.Expressions {
	/// <summary>
	/// Represents a conditional expression in the source code.
	/// </summary>
	public class ConditionalExpression : Expression {
		/// <summary>
		/// The condition.
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
		/// The expression to be evaluated if the condition holds.
		/// </summary>
		public Expression true_expression {
			get {
				return _true_expression;
			}
			set {
				_true_expression = value;
				_true_expression.parent_node = this;
			}
		}

		/// <summary>
		/// The expression to be evaluated if the condition doesn't hold.
		/// </summary>
		public Expression false_expression {
			get {
				return _false_expression;
			}
			set {
				_false_expression = value;
				_false_expression.parent_node = this;
			}
		}

		Expression _condition;
		Expression _true_expression;
		Expression _false_expression;

		/// <summary>
		/// Creates a new conditional expression.
		/// 
		/// <param name="cond">a condition</param>
		/// <param name="true_expr">expression to be evaluated if condition is true</param>
		/// <param name="false_expr">expression to be evaluated if condition is false</param>
		/// <returns>newly created conditional expression</returns>
		/// </summary>
		public ConditionalExpression(Expression cond, Expression true_expr, Expression false_expr, SourceReference source) {
			condition = cond;
			true_expression = true_expr;
			false_expression = false_expr;
			source_reference = source;
		}

		public override void accept(CodeVisitor visitor) {
			visitor.visit_conditional_expression(this);

			visitor.visit_expression(this);
		}

		public override void accept_children(CodeVisitor visitor) {
			condition.accept(visitor);
			true_expression.accept(visitor);
			false_expression.accept(visitor);
		}

		public override bool is_pure() {
			return condition.is_pure() && true_expression.is_pure() && false_expression.is_pure();
		}

		public override bool is_accessible(Symbol sym) {
			return condition.is_accessible(sym) && true_expression.is_accessible(sym) && false_expression.is_accessible(sym);
		}

		public override bool check(CodeContext context) {
			if (is_checked) {
				return !error;
			}

			is_checked = true;

			if (!(context.analyzer.current_symbol is Block)) {
				Report.error(source_reference, "Conditional expressions may only be used in blocks");
				error = true;
				return false;
			}

			// convert ternary expression into if statement
			// required for flow analysis and exception handling

			string temp_name = get_temp_name();

			true_expression.target_type = target_type;
			false_expression.target_type = target_type;

			var local = new LocalVariable(null, temp_name, null, source_reference);
			var decl = new DeclarationStatement(local, source_reference);

			var true_local = new LocalVariable(null, temp_name, true_expression, true_expression.source_reference);
			var true_block = new Block(true_expression.source_reference);
			var true_decl = new DeclarationStatement(true_local, true_expression.source_reference);
			true_block.add_statement(true_decl);

			var false_local = new LocalVariable(null, temp_name, false_expression, false_expression.source_reference);
			var false_block = new Block(false_expression.source_reference);
			var false_decl = new DeclarationStatement(false_local, false_expression.source_reference);
			false_block.add_statement(false_decl);

			var if_stmt = new IfStatement(condition, true_block, false_block, source_reference);

			insert_statement(context.analyzer.insert_block, decl);
			insert_statement(context.analyzer.insert_block, if_stmt);

			if (!if_stmt.check(context) || true_expression.error || false_expression.error) {
				error = true;
				return false;
			}

			true_expression = true_local.initializer;
			false_expression = false_local.initializer;

			true_block.remove_local_variable(true_local);
			false_block.remove_local_variable(false_local);

			if (false_expression.value_type.compatible(true_expression.value_type)) {
				value_type = true_expression.value_type.copy();
			} else if (true_expression.value_type.compatible(false_expression.value_type)) {
				value_type = false_expression.value_type.copy();
			} else {
				error = true;
				Report.error(condition.source_reference, "Incompatible expressions");
				return false;
			}

			value_type.value_owned = (true_expression.value_type.value_owned || false_expression.value_type.value_owned);

			local.variable_type = value_type;
			decl.check(context);

			true_expression.target_type = value_type;
			false_expression.target_type = value_type;

			var true_stmt = new ExpressionStatement(new Assignment(MemberAccess.simple(local.name, true_expression.source_reference), true_expression, AssignmentOperator.SIMPLE, true_expression.source_reference), true_expression.source_reference);
			true_stmt.check(context);

			var false_stmt = new ExpressionStatement(new Assignment(MemberAccess.simple(local.name, false_expression.source_reference), false_expression, AssignmentOperator.SIMPLE, false_expression.source_reference), false_expression.source_reference);
			false_stmt.check(context);

			true_block.replace_statement(true_decl, true_stmt);
			false_block.replace_statement(false_decl, false_stmt);

			var ma = MemberAccess.simple(local.name, source_reference);
			ma.formal_target_type = formal_target_type;
			ma.target_type = target_type;
			ma.check(context);

			parent_node.replace_expression(this, ma);

			return true;
		}
	}
}
