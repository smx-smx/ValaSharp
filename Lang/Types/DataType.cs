﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.CodeNodes;
using Vala.Lang.Symbols;
using Vala.Lang.TypeSymbols;

namespace Vala.Lang.Types {
	public abstract class DataType : CodeNode {
		/// <summary>
		/// Specifies that the expression or variable owns the value.
		/// </summary>
		public bool value_owned { get; set; }

		/// <summary>
		/// Specifies that the expression may be null.
		/// </summary>
		public bool nullable { get; set; }

		private WeakReference<TypeSymbol> data_type_weak = new WeakReference<TypeSymbol>(null);
		/// <summary>
		/// The referred data type.
		/// </summary>
		public TypeSymbol data_type {
			get {
				return data_type_weak.GetTarget();
			}
			set {
				data_type_weak.SetTarget(value);
			}
		}

		/// <summary>
		/// The referred generic type parameter.
		/// </summary>
		public TypeParameter type_parameter { get; set; }

		/// <summary>
		/// Specifies that the expression transfers a floating reference.
		/// </summary>
		public bool floating_reference { get; set; }

		/// <summary>
		/// Specifies that the type supports dynamic lookup.
		/// </summary>
		public bool is_dynamic { get; set; }


		private List<DataType> type_argument_list;
		private static List<DataType> _empty_type_list;

		/// <summary>
		/// Appends the specified type as generic type argument.
		/// 
		/// <param name="arg">a type reference</param>
		/// </summary>
		public void add_type_argument(DataType arg) {
			if (type_argument_list == null) {
				type_argument_list = new List<DataType>();
			}
			type_argument_list.Add(arg);
			arg.parent_node = this;
		}

		/// <summary>
		/// Returns a copy of the list of generic type arguments.
		/// 
		/// <returns>type argument list</returns>
		/// </summary>
		public List<DataType> get_type_arguments() {
			if (type_argument_list != null) {
				return type_argument_list;
			}
			if (_empty_type_list == null) {
				_empty_type_list = new List<DataType>();
			}
			return _empty_type_list;
		}

		public bool has_type_arguments() {
			if (type_argument_list == null) {
				return false;
			}

			return type_argument_list.Count > 0;
		}

		/// <summary>
		/// Removes all generic type arguments.
		/// </summary>
		public void remove_all_type_arguments() {
			type_argument_list = null;
		}

		public override void accept(CodeVisitor visitor) {
			visitor.visit_data_type(this);
		}

		public override void accept_children(CodeVisitor visitor) {
			if (type_argument_list != null && type_argument_list.Count > 0) {
				foreach (DataType type_arg in type_argument_list.ToList()) {
					type_arg.accept(visitor);
				}
			}
		}

		public override string ToString() {
			return to_qualified_string(null);
		}

		public virtual string to_qualified_string(Scope scope = null) {
			// logic temporarily duplicated in DelegateType class

			string s;

			if (data_type != null) {
				Symbol global_symbol = data_type;
				while (global_symbol.parent_symbol.name != null) {
					global_symbol = global_symbol.parent_symbol;
				}

				Symbol sym = null;
				Scope parent_scope = scope;
				while (sym == null && parent_scope != null) {
					sym = parent_scope.lookup(global_symbol.name);
					parent_scope = parent_scope.parent_scope;
				}

				if (sym != null && global_symbol != sym) {
					s = "global::" + data_type.get_full_name();
				} else {
					s = data_type.get_full_name();
				}
			} else {
				s = "null";
			}

			var type_args = get_type_arguments();
			if (type_args.Count > 0) {
				s += "<";
				bool first = true;
				foreach (DataType type_arg in type_args) {
					if (!first) {
						s += ",";
					} else {
						first = false;
					}
					if (!type_arg.value_owned) {
						s += "weak ";
					}
					s += type_arg.to_qualified_string(scope);
				}
				s += ">";
			}
			if (nullable) {
				s += "?";
			}

			return s;
		}

		/// <summary>
		/// Creates a shallow copy of this type reference.
		/// 
		/// <returns>copy of this type reference</returns>
		/// </summary>
		public abstract DataType copy();

		/// <summary>
		/// Checks two type references for equality. May only be used with
		/// resolved type references.
		/// 
		/// <param name="type2">a type reference</param>
		/// <returns>true if this type reference is equal to type2, false</returns>
		/// otherwise
		/// </summary>
		public virtual bool equals(DataType type2) {
			if (type2.is_disposable() != is_disposable()) {
				return false;
			}
			if (type2.nullable != nullable) {
				return false;
			}
			if (type2.data_type != data_type) {
				return false;
			}
			if (type2.type_parameter != null || type_parameter != null) {
				if (type2.type_parameter == null || type_parameter == null) {
					return false;
				}
				if (!type2.type_parameter.equals(type_parameter)) {
					return false;
				}
			}
			if (type2.floating_reference != floating_reference) {
				return false;
			}

			var type_args = get_type_arguments();
			var type2_args = type2.get_type_arguments();
			if (type2_args.Count != type_args.Count) {
				return false;
			}

			for (int i = 0; i < type_args.Count; i++) {
				if (!type2_args[i].equals(type_args[i]))
					return false;
			}

			return true;
		}

		/// <summary>
		/// Checks whether this type reference is at least as strict as the
		/// specified type reference type2.
		/// 
		/// <param name="type2">a type reference</param>
		/// <returns>true if this type reference is stricter or equal</returns>
		/// </summary>
		public virtual bool stricter(DataType type2) {
			if (type2.is_disposable() != is_disposable()) {
				return false;
			}

			if (!type2.nullable && nullable) {
				return false;
			}

			/* temporarily ignore type parameters */
			if (type_parameter != null || type2.type_parameter != null) {
				return true;
			}

			if (type2.data_type != data_type) {
				// FIXME: allow this type reference to refer to a
				//        subtype of the type type2 is referring to
				return false;
			}

			if (type2.floating_reference != floating_reference) {
				return false;
			}

			return true;
		}

		public override void replace_type(DataType old_type, DataType new_type) {
			if (type_argument_list != null) {
				for (int i = 0; i < type_argument_list.Count; i++) {
					if (type_argument_list[i] == old_type) {
						type_argument_list[i] = new_type;
						return;
					}
				}
			}
		}

		public virtual bool compatible(DataType target_type) {
			if (CodeContext.get().experimental_non_null && nullable && !target_type.nullable) {
				return false;
			}

			if (target_type.data_type != null) {
				if (target_type.data_type.is_subtype_of(CodeContext.get().analyzer.gvalue_type.data_type)) {
					// allow implicit conversion to GValue
					return true;
				}

				if (target_type.data_type.is_subtype_of(CodeContext.get().analyzer.gvariant_type.data_type)) {
					// allow implicit conversion to GVariant
					return true;
				}
			}

			if (target_type is DelegateType && this is DelegateType) {
				return ((DelegateType)target_type).delegate_symbol == ((DelegateType)this).delegate_symbol;
			}

			if (target_type is PointerType) {
				/* any reference or array type or pointer type can be cast to a generic pointer */
				if (type_parameter != null ||
					(data_type != null && (
						data_type.is_reference_type() ||
						this is DelegateType))) {
					return true;
				}

				return false;
			}

			/* temporarily ignore type parameters */
			if (target_type.type_parameter != null) {
				return true;
			}

			if (this is ArrayType != target_type is ArrayType) {
				return false;
			}

			if (data_type is ValaEnum && target_type.data_type is Struct && ((Struct)target_type.data_type).is_integer_type()) {
				return true;
			}

			if (data_type != null && target_type.data_type != null && data_type.is_subtype_of(target_type.data_type)) {
				var base_type = SemanticAnalyzer.get_instance_base_type_for_member(this, target_type.data_type, this);
				// check compatibility of generic type arguments
				var base_type_args = base_type.get_type_arguments();
				var target_type_args = target_type.get_type_arguments();
				if (base_type_args.Count == target_type_args.Count) {
					for (int i = 0; i < base_type_args.Count; i++) {
						// mutable generic types require type argument equality,
						// not just one way compatibility
						// as we do not currently have immutable generic container types,
						// the additional check would be very inconvenient, so we
						// skip the additional check for now
						if (!base_type_args[i].compatible(target_type_args[i])) {
							return false;
						}
					}
				}
				return true;
			}

			if (data_type is Struct && target_type.data_type is Struct) {
				var expr_struct = (Struct)data_type;
				var expect_struct = (Struct)target_type.data_type;

				/* integer types may be implicitly cast to floating point types */
				if (expr_struct.is_integer_type() && expect_struct.is_floating_type()) {
					return true;
				}

				if ((expr_struct.is_integer_type() && expect_struct.is_integer_type()) ||
					(expr_struct.is_floating_type() && expect_struct.is_floating_type())) {
					if (expr_struct.get_rank() <= expect_struct.get_rank()) {
						return true;
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Returns whether instances of this type are invokable.
		/// 
		/// <returns>true if invokable, false otherwise</returns>
		/// </summary>
		public virtual bool is_invokable() {
			return false;
		}

		/// <summary>
		/// Returns the return type of this invokable.
		/// 
		/// <returns>return type</returns>
		/// </summary>
		public virtual DataType get_return_type() {
			return null;
		}

		/// <summary>
		/// Returns copy of the list of invocation parameters.
		/// 
		/// <returns>parameter list</returns>
		/// </summary>
		public virtual List<Parameter> get_parameters() {
			return null;
		}

		public virtual bool is_reference_type_or_type_parameter() {
			return (data_type != null &&
					data_type.is_reference_type()) ||
				   type_parameter != null;
		}

		public virtual bool is_array() {
			return false;
		}

		// check whether this type is at least as accessible as the specified symbol
		public virtual bool is_accessible(Symbol sym) {
			foreach (var type_arg in get_type_arguments()) {
				if (!type_arg.is_accessible(sym)) {
					return false;
				}
			}
			if (data_type != null) {
				return data_type.is_accessible(sym);
			}
			return true;
		}

		public virtual Symbol get_member(string member_name) {
			if (data_type != null) {
				return SemanticAnalyzer.symbol_lookup_inherited(data_type, member_name);
			}
			return null;
		}

		public virtual Symbol get_pointer_member(string member_name) {
			return null;
		}

		/// <summary>
		/// Checks whether this data type references a real struct. A real struct
		/// is a struct which is not a simple (fundamental) type.
		/// </summary>
		public virtual bool is_real_struct_type() {
			var s = data_type as Struct;
			if (s != null && !s.is_simple_type()) {
				return true;
			}
			return false;
		}

		public bool is_real_non_null_struct_type() {
			return is_real_struct_type() && !nullable;
		}

		/// <summary>
		/// Returns whether the value needs to be disposed, i.e. whether
		/// allocated memory or other resources need to be released when
		/// the value is no longer needed.
		/// </summary>
		public virtual bool is_disposable() {
			if (!value_owned) {
				return false;
			}

			if (is_reference_type_or_type_parameter()) {
				return true;
			}
			return false;
		}

		public virtual DataType get_actual_type(DataType derived_instance_type, List<DataType> method_type_arguments, CodeNodes.CodeNode node_reference) {
			DataType result = this.copy();

			if (derived_instance_type == null && method_type_arguments == null) {
				return result;
			}

			if (result is GenericType) {
				result = SemanticAnalyzer.get_actual_type(derived_instance_type, method_type_arguments, (GenericType)result, node_reference);
				// don't try to resolve type arguments of returned actual type
				// they can never be resolved and are not related to the instance type
			} else if (result.type_argument_list != null) {
				// recursely get actual types for type arguments
				for (int i = 0; i < result.type_argument_list.Count; i++) {
					result.type_argument_list[i] = result.type_argument_list[i].get_actual_type(derived_instance_type, method_type_arguments, node_reference);
				}
			}

			return result;
		}

		/// <summary>
		/// Search for the type parameter in this formal type and match it in
		/// value_type.
		/// </summary>
		public virtual DataType infer_type_argument(TypeParameter type_param, DataType value_type) {
			var value_type_arg_it = value_type.get_type_arguments().GetEnumerator();
			foreach (var formal_type_arg in this.get_type_arguments()) {
				if (value_type_arg_it.MoveNext()) {
					var inferred_type = formal_type_arg.infer_type_argument(type_param, value_type_arg_it.Current);
					if (inferred_type != null) {
						return inferred_type;
					}
				}
			}

			return null;
		}

		public bool is_weak() {
			if (this.value_owned) {
				return false;
			} else if (this is VoidType || this is PointerType) {
				return false;
			} else if (this is ValaValueType) {
				if (this.nullable) {
					// nullable structs are heap allocated
					return true;
				}

				// TODO return true for structs with destroy
				return false;
			}

			return true;
		}
	}
}
