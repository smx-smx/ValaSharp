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
	 * Represents a type cast in the source code.
	 */
	public class CastExpression : Expression
	{
		/**
		 * The expression to be cast.
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

		/**
		 * The target type.
		 */
		public DataType type_reference {
			get { return _data_type; }
			set {
				_data_type = value;
				_data_type.parent_node = this;
			}
		}

		/**
		 * Checked casts return NULL instead of raising an error.
		 */
		public bool is_silent_cast { get; set; }

		public bool is_non_null_cast { get; set; }

		private Expression _inner;

		private DataType _data_type;

		/**
		 * Creates a new cast expression.
		 *
		 * @param inner           expression to be cast
		 * @param type_reference  target type
		 * @return                newly created cast expression
		 */
		public CastExpression(Expression inner, DataType type_reference, SourceReference source_reference, bool is_silent_cast) {
			this.type_reference = type_reference;
			this.source_reference = source_reference;
			this.is_silent_cast = is_silent_cast;
			this.inner = inner;
		}

		public static CastExpression non_null(Expression inner, SourceReference source_reference) {
			CastExpression @this = new CastExpression(inner, null, source_reference, false);
			@this.is_non_null_cast = true;
			return @this;
		}

		public override void accept(CodeVisitor visitor) {
			visitor.visit_cast_expression(this);

			visitor.visit_expression(this);
		}

		public override void accept_children(CodeVisitor visitor) {
			inner.accept(visitor);
			if (!is_non_null_cast) {
				type_reference.accept(visitor);
			}
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

		public override void replace_type(DataType old_type, DataType new_type) {
			if (type_reference == old_type) {
				type_reference = new_type;
			}
		}

		public override bool check(CodeContext context) {
			if (is_checked) {
				return !error;
			}

			is_checked = true;

			if (!inner.check(context)) {
				error = true;
				return false;
			}

			if (inner.value_type == null) {
				Report.error(source_reference, "Invalid cast expression");
				error = true;
				return false;
			}

			if (is_non_null_cast) {
				// (!) non-null cast
				type_reference = inner.value_type.copy();
				type_reference.nullable = false;
			}

			type_reference.check(context);

			// FIXME: check whether cast is allowed

			if (type_reference is DelegateType && inner.value_type is MethodType) {
				if (target_type != null) {
					inner.value_type.value_owned = target_type.value_owned;
				} else {
					inner.value_type.value_owned = true;
				}
			}

			value_type = type_reference;
			value_type.value_owned = inner.value_type.value_owned;

			if (is_silent_cast) {
				value_type.nullable = true;
			}

			if (is_gvariant(context, inner.value_type) && !is_gvariant(context, value_type)) {
				// GVariant unboxing returns owned value
				value_type.value_owned = true;
			}

			inner.target_type = inner.value_type.copy();

			return !error;
		}

		bool is_gvariant(CodeContext context, DataType type) {
			return type.data_type != null && type.data_type.is_subtype_of(context.analyzer.gvariant_type.data_type);
		}

		public override void emit(CodeGenerator codegen) {
			inner.emit(codegen);

			codegen.visit_cast_expression(this);

			codegen.visit_expression(this);
		}

		public override void get_defined_variables(ICollection<Variable> collection) {
			inner.get_defined_variables(collection);
		}

		public override void get_used_variables(ICollection<Variable> collection) {
			inner.get_used_variables(collection);
		}

		public override bool is_constant() {
			return inner.is_constant();
		}
	}

}
