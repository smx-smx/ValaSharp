using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.Code;
using Vala.Lang.CodeNodes;
using Vala.Lang.Parser;
using Vala.Lang.Symbols;
using Vala.Lang.Types;

namespace Vala.Lang.Expressions {
	public class ReferenceTransferExpression : Expression {
		/**
	 * The variable whose reference is to be transferred.
	 */
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

		/**
		 * Creates a new reference transfer expression.
		 *
		 * @param inner variable whose reference is to be transferred
		 * @return      newly created reference transfer expression
		 */
		public ReferenceTransferExpression(Expression inner, SourceReference source_reference = null) {
			this.inner = inner;
			this.source_reference = source_reference;
		}

		public override void accept(CodeVisitor visitor) {
			visitor.visit_reference_transfer_expression(this);

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

		public override bool is_pure() {
			return false;
		}

		public override bool is_accessible(Symbol sym) {
			return inner.is_accessible(sym);
		}

		public override bool check(CodeContext context) {
			if (is_checked) {
				return !error;
			}

			is_checked = true;

			inner.lvalue = true;

			inner.check(context);

			if (inner.error) {
				/* if there was an error in the inner expression, skip type check */
				error = true;
				return false;
			}

			if (!(inner is MemberAccess || inner is ElementAccess)) {
				error = true;
				Report.error(source_reference, "Reference transfer not supported for this expression");
				return false;
			}

			var is_owned_delegate = inner.value_type is DelegateType && inner.value_type.value_owned;
			if (!inner.value_type.is_disposable()
				&& !(inner.value_type is PointerType)
				&& !is_owned_delegate) {
				error = true;
				Report.error(source_reference, "No reference to be transferred");
				return false;
			}

			value_type = inner.value_type.copy();
			value_type.value_owned = true;

			return !error;
		}

		public override void emit(CodeGenerator codegen) {
			inner.emit(codegen);

			codegen.visit_reference_transfer_expression(this);

			codegen.visit_expression(this);
		}

		public override void get_defined_variables(ICollection<Variable> collection) {
			inner.get_defined_variables(collection);
			var local = inner.symbol_reference as LocalVariable;
			var param = inner.symbol_reference as Parameter;
			if (local != null) {
				collection.Add(local);
			} else if (param != null && param.direction == ParameterDirection.OUT) {
				collection.Add(param);
			}
		}

		public override void get_used_variables(ICollection<Variable> collection) {
			inner.get_used_variables(collection);
			var local = inner.symbol_reference as LocalVariable;
			var param = inner.symbol_reference as Parameter;
			if (local != null) {
				collection.Add(local);
			} else if (param != null && param.direction == ParameterDirection.OUT) {
				collection.Add(param);
			}
		}
	}
}
