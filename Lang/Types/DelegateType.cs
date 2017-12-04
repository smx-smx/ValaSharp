using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.CodeNodes;
using Vala.Lang.Symbols;
using Vala.Lang.TypeSymbols;

namespace Vala.Lang.Types {
	public class DelegateType : DataType {
		public ValaDelegate delegate_symbol { get; set; }

		public bool is_called_once { get; set; }

		public DelegateType(ValaDelegate delegate_symbol) {
			this.delegate_symbol = delegate_symbol;
			this.is_called_once = (delegate_symbol.get_attribute_string("CCode", "scope") == "async");
		}

		public override bool is_invokable() {
			return true;
		}

		public override DataType get_return_type() {
			return delegate_symbol.return_type;
		}

		public override List<Parameter> get_parameters() {
			return delegate_symbol.get_parameters();
		}

		public override string to_qualified_string(Scope scope) {
			// logic temporarily duplicated from DataType class

			Symbol global_symbol = delegate_symbol;
			while (global_symbol.parent_symbol != null && global_symbol.parent_symbol.name != null) {
				global_symbol = global_symbol.parent_symbol;
			}

			Symbol sym = null;
			Scope parent_scope = scope;
			while (sym == null && parent_scope != null) {
				sym = parent_scope.lookup(global_symbol.name);
				parent_scope = parent_scope.parent_scope;
			}

			string s;

			if (sym != null && global_symbol != sym) {
				s = "global::" + delegate_symbol.get_full_name(); ;
			} else {
				s = delegate_symbol.get_full_name();
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

		public override DataType copy() {
			var result = new DelegateType(delegate_symbol);
			result.source_reference = source_reference;
			result.value_owned = value_owned;
			result.nullable = nullable;

			foreach (DataType arg in get_type_arguments()) {
				result.add_type_argument(arg.copy());
			}

			result.is_called_once = is_called_once;

			return result;
		}

		public override bool is_accessible(Symbol sym) {
			return delegate_symbol.is_accessible(sym);
		}

		public override bool check(CodeContext context) {
			if (is_called_once && !value_owned) {
				Report.warning(source_reference, "delegates with scope=\"async\" must be owned");
			}

			if (!delegate_symbol.check(context)) {
				return false;
			}

			var n_type_params = delegate_symbol.get_type_parameters().Count;
			var n_type_args = get_type_arguments().Count;
			if (n_type_args > 0 && n_type_args < n_type_params) {
				Report.error(source_reference, "too few type arguments");
				return false;
			} else if (n_type_args > 0 && n_type_args > n_type_params) {
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

		public override bool is_disposable() {
			return delegate_symbol.has_target && value_owned && !is_called_once;
		}
	}
}
