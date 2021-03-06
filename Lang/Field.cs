﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.CodeNodes;
using Vala.Lang.Expressions;
using Vala.Lang.Parser;
using Vala.Lang.Symbols;
using Vala.Lang.Types;
using Vala.Lang.TypeSymbols;

namespace Vala.Lang {
	public class Field : Variable, Lockable {
		/// <summary>
		/// Specifies whether this field may only be accessed with an instance of
		/// the contained type.
		/// </summary>
		public MemberBinding binding { get; set; } = MemberBinding.INSTANCE;

		/// <summary>
		/// Specifies whether the field is volatile. Volatile fields are
		/// necessary to allow multi-threaded access.
		/// </summary>
		public bool is_volatile { get; set; }

		private bool lock_used = false;

		/// <summary>
		/// Creates a new field.
		/// 
		/// <param name="name">field name</param>
		/// <param name="variable_type">field type</param>
		/// <param name="initializer">initializer expression</param>
		/// <param name="source_reference">reference to source code</param>
		/// <returns>newly created field</returns>
		/// </summary>
		public Field(string name, DataType variable_type, Expression initializer, SourceReference source_reference = null, Comment comment = null)
			: base(variable_type, name, initializer, source_reference, comment) {
		}

		public override void accept(CodeVisitor visitor) {
			visitor.visit_field(this);
		}

		public override void accept_children(CodeVisitor visitor) {
			variable_type.accept(visitor);

			if (initializer != null) {
				initializer.accept(visitor);
			}
		}

		public bool get_lock_used() {
			return lock_used;
		}

		public void set_lock_used(bool used) {
			lock_used = used;
		}

		public override void replace_expression(Expression old_node, Expression new_node) {
			if (initializer == old_node) {
				initializer = new_node;
			}
		}

		public override void replace_type(DataType old_type, DataType new_type) {
			if (variable_type == old_type) {
				variable_type = new_type;
			}
		}

		public override bool check(CodeContext context) {
			if (is_checked) {
				return !error;
			}

			is_checked = true;

			var old_source_file = context.analyzer.current_source_file;
			var old_symbol = context.analyzer.current_symbol;

			if (source_reference != null) {
				context.analyzer.current_source_file = source_reference.file;
			}
			context.analyzer.current_symbol = this;

			if (variable_type is VoidType) {
				error = true;
				Report.error(source_reference, "'void' not supported as field type");
				return false;
			}

			variable_type.check(context);

			// check whether field type is at least as accessible as the field
			if (!context.analyzer.is_type_accessible(this, variable_type)) {
				error = true;
				Report.error(source_reference, "field type `%s` is less accessible than field `%s`".printf(variable_type.ToString(), get_full_name()));
				return false;
			}

			if (initializer != null) {
				initializer.target_type = variable_type;

				if (!initializer.check(context)) {
					error = true;
					return false;
				}

				if (initializer.value_type == null) {
					error = true;
					Report.error(source_reference, "expression type not allowed as initializer");
					return false;
				}

				if (!initializer.value_type.compatible(variable_type)) {
					error = true;
					Report.error(source_reference, "Cannot convert from `%s' to `%s'".printf(initializer.value_type.ToString(), variable_type.ToString()));
					return false;
				}

				if (initializer.value_type.is_disposable()) {
					/* rhs transfers ownership of the expression */
					if (!(variable_type is PointerType) && !variable_type.value_owned) {
						/* lhs doesn't own the value */
						error = true;
						Report.error(source_reference, "Invalid assignment from owned expression to unowned variable");
						return false;
					}
				}

				if (parent_symbol is Namespace && !initializer.is_constant()) {
					error = true;
					Report.error(source_reference, "Non-constant field initializers not supported in this context");
					return false;
				}

				if (parent_symbol is Namespace && initializer.is_constant() && initializer.is_non_null()) {
					if (variable_type.is_disposable() && variable_type.value_owned) {
						error = true;
						Report.error(source_reference, "Owned namespace fields can only be initialized in a function or method");
						return false;
					}
				}

				if (binding == MemberBinding.STATIC && parent_symbol is Class && ((Class)parent_symbol).is_compact && !initializer.is_constant()) {
					error = true;
					Report.error(source_reference, "Static fields in compact classes cannot have non-constant initializers");
					return false;
				}

				if (external) {
					error = true;
					Report.error(source_reference, "External fields cannot use initializers");
				}
			}

			if (binding == MemberBinding.INSTANCE && parent_symbol is Interface) {
				error = true;
				Report.error(source_reference, "Interfaces may not have instance fields");
				return false;
			}

			bool field_in_header = !is_internal_symbol();
			if (parent_symbol is Class) {
				var cl = (Class)parent_symbol;
				if (cl.is_compact && !cl.is_internal_symbol()) {
					// compact classes don't have priv structs
					field_in_header = true;
				}
			}

			if (!external_package && !hides && get_hidden_member() != null) {
				Report.warning(source_reference, "%s hides inherited field `%s'. Use the `new' keyword if hiding was intentional".printf(get_full_name(), get_hidden_member().get_full_name()));
			}

			context.analyzer.current_source_file = old_source_file;
			context.analyzer.current_symbol = old_symbol;

			return !error;
		}
	}
}
