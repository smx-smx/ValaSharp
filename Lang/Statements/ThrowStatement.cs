﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.Code;
using Vala.Lang.CodeNodes;
using Vala.Lang.Expressions;
using Vala.Lang.Parser;
using Vala.Lang.Symbols;
using Vala.Lang.Types;

namespace Vala.Lang.Statements
{
	/**
 * Represents a throw statement in the source code.
 */
	public class ThrowStatement : CodeNode, Statement
	{
		public CodeNode node {
			get { return this; }
		}

		/**
		 * The error expression to throw.
		 */
		public Expression error_expression {
			get {
				return _error_expression;
			}
			set {
				_error_expression = value;
				if (_error_expression != null) {
					_error_expression.parent_node = this;
				}
			}
		}

		private Expression _error_expression;

		/**
		 * Creates a new throw statement.
		 *
		 * @param error_expression the error expression
		 * @param source_reference reference to source code
		 * @return                 newly created throw statement
		 */
		public ThrowStatement(Expression error_expression, SourceReference source_reference = null){
			this.error_expression = error_expression;
			this.source_reference = source_reference;
		}

		public override void accept(CodeVisitor visitor) {
			visitor.visit_throw_statement(this);
		}

		public override void accept_children(CodeVisitor visitor) {
			if (error_expression != null) {
				error_expression.accept(visitor);

				visitor.visit_end_full_expression(error_expression);
			}
		}

		public override void replace_expression(Expression old_node, Expression new_node) {
			if (error_expression == old_node) {
				error_expression = new_node;
			}
		}

		public override bool check(CodeContext context) {
			if (is_checked) {
				return !error;
			}

			is_checked = true;

			error_expression.target_type.value_owned = true;

			if (error_expression != null) {
				if (!error_expression.check(context)) {
					error = true;
					return false;
				}

				if (error_expression.value_type == null) {
					Report.error(error_expression.source_reference, "invalid error expression");
					error = true;
					return false;
				}

				if (!(error_expression.value_type is ErrorType)) {
					Report.error(error_expression.source_reference, "`%s' is not an error type".printf(error_expression.value_type.to_string()));
					error = true;
					return false;
				}
			}

			var error_type = error_expression.value_type.copy();
			error_type.source_reference = source_reference;

			add_error_type(error_type);

			return !error;
		}

		public override void emit(CodeGenerator codegen) {
			if (error_expression != null) {
				error_expression.emit(codegen);

				codegen.visit_end_full_expression(error_expression);
			}

			codegen.visit_throw_statement(this);
		}

		public override void get_defined_variables(ICollection<Variable> collection) {
			error_expression.get_defined_variables(collection);
		}

		public override void get_used_variables(ICollection<Variable> collection) {
			error_expression.get_used_variables(collection);
		}
	}
}
