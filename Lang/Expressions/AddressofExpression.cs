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

namespace Vala.Lang.Expressions
{
	/**
 * Represents an address-of expression in the source code, e.g. `&foo`.
 */
	public class AddressofExpression : Expression
	{
		/**
		 * The variable whose address is to be computed.
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
		 * Creates a new address-of expression.
		 *
		 * @param inner variable whose address is to be computed
		 * @return      newly created address-of expression
		 */
		public AddressofExpression(Expression inner, SourceReference source_reference = null) {
			this.source_reference = source_reference;
			this.inner = inner;
		}

		public override void accept(CodeVisitor visitor) {
			visitor.visit_addressof_expression(this);

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
			return inner.is_pure();
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

			if (!inner.check(context)) {
				error = true;
				return false;
			}
			var ea = inner as ElementAccess;
			if (inner is MemberAccess && inner.symbol_reference is Variable) {
				// address of variable is always possible
			} else if (ea != null &&
					   (ea.container.value_type is ArrayType || ea.container.value_type is PointerType)) {
				// address of element of regular array or pointer is always possible
			} else {
				error = true;
				Report.error(source_reference, "Address-of operator not supported for this expression");
				return false;
			}

			if (inner.value_type.is_reference_type_or_type_parameter()) {
				value_type = new PointerType(new PointerType(inner.value_type));
			} else {
				value_type = new PointerType(inner.value_type);
			}

			return !error;
		}

		public override void emit(CodeGenerator codegen) {
			inner.emit(codegen);

			codegen.visit_addressof_expression(this);

			codegen.visit_expression(this);
		}
	}

}
