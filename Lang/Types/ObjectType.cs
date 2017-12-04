﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.CodeNodes;
using Vala.Lang.Symbols;
using Vala.Lang.TypeSymbols;

namespace Vala.Lang.Types {

	/**
	 * A class type.
	 */
	public class ObjectType : ReferenceType {
		private WeakReference<ObjectTypeSymbol> type_symbol_weak = new WeakReference<ObjectTypeSymbol>(null);
		/**
		 * The referred class or interface.
		 */
		public ObjectTypeSymbol type_symbol {
			get {
				return type_symbol_weak.GetTarget();
			}
			set {
				type_symbol_weak.SetTarget(value);
			}
		}

		public ObjectType(ObjectTypeSymbol type_symbol) {
			this.type_symbol = type_symbol;
			data_type = type_symbol;
		}

		public override DataType copy() {
			var result = new ObjectType(type_symbol);
			result.source_reference = source_reference;
			result.value_owned = value_owned;
			result.nullable = nullable;
			result.is_dynamic = is_dynamic;
			result.floating_reference = floating_reference;

			foreach (DataType arg in get_type_arguments()) {
				result.add_type_argument(arg.copy());
			}

			return result;
		}

		public override bool stricter(DataType target_type) {
			var obj_target_type = target_type as ObjectType;
			if (obj_target_type == null) {
				return false;
			}

			if (value_owned != target_type.value_owned) {
				return false;
			}

			if (nullable && !target_type.nullable) {
				return false;
			}

			return type_symbol.is_subtype_of(obj_target_type.type_symbol);
		}

		public override bool is_invokable() {
			var cl = type_symbol as Class;
			if (cl != null && cl.default_construction_method != null) {
				return true;
			} else {
				return false;
			}
		}

		public override DataType get_return_type() {
			var cl = type_symbol as Class;
			if (cl != null && cl.default_construction_method != null) {
				return cl.default_construction_method.return_type;
			} else {
				return null;
			}
		}

		public override List<Parameter> get_parameters() {
			var cl = type_symbol as Class;
			if (cl != null && cl.default_construction_method != null) {
				return cl.default_construction_method.get_parameters();
			} else {
				return null;
			}
		}

		public override bool check(CodeContext context) {
			if (!type_symbol.check(context)) {
				return false;
			}

			int n_type_args = get_type_arguments().Count;
			if (n_type_args > 0 && n_type_args < type_symbol.get_type_parameters().Count) {
				Report.error(source_reference, "too few type arguments");
				return false;
			} else if (n_type_args > 0 && n_type_args > type_symbol.get_type_parameters().Count) {
				Report.error(source_reference, "too many type arguments");
				return false;
			}

			foreach (DataType type in get_type_arguments()) {
				if (!type.check(context)) {
					return false;
				}
			}

			return true;
		}
	}

}
