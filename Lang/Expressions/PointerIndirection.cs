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
	/// <summary>
	/// Represents a pointer indirection in the source code, e.g. `*pointer`.
	/// </summary>
	public class PointerIndirection : Expression {
		/// <summary>
		/// The pointer to dereference.
		/// </summary>
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

		/// <summary>
		/// Creates a new pointer indirection.
		/// 
		/// <param name="inner">pointer to be dereferenced</param>
		/// <returns>newly created pointer indirection</returns>
		/// </summary>
		public PointerIndirection(Expression inner, SourceReference source_reference = null) {
			this.source_reference = source_reference;
			this.inner = inner;
		}

		public override void accept(CodeVisitor visitor) {
			inner.accept(visitor);

			visitor.visit_pointer_indirection(this);

			visitor.visit_expression(this);
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

			if (!inner.check(context)) {
				return false;
			}
			if (inner.value_type == null) {
				error = true;
				Report.error(source_reference, "internal error: unknown type of inner expression");
				return false;
			}
			if (inner.value_type is PointerType) {
				var pointer_type = (PointerType)inner.value_type;
				if (pointer_type.base_type is ReferenceType || pointer_type.base_type is VoidType) {
					error = true;
					Report.error(source_reference, "Pointer indirection not supported for this expression");
					return false;
				}
				value_type = pointer_type.base_type;
			} else {
				error = true;
				Report.error(source_reference, "Pointer indirection not supported for this expression");
				return false;
			}

			return !error;
		}

		public override void emit(CodeGenerator codegen) {
			inner.emit(codegen);

			codegen.visit_pointer_indirection(this);

			codegen.visit_expression(this);
		}

		public override void get_defined_variables(ICollection<Variable> collection) {
			inner.get_defined_variables(collection);
		}

		public override void get_used_variables(ICollection<Variable> collection) {
			inner.get_used_variables(collection);
		}
	}
}
