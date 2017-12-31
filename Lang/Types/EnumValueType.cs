using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.CodeNodes;
using Vala.Lang.Methods;
using Vala.Lang.Symbols;
using Vala.Lang.TypeSymbols;

namespace Vala.Lang.Types {
	public class EnumValueType : ValaValueType {
		private Method to_string_method;

		public EnumValueType(ValaEnum type_symbol) : base(type_symbol) { }

		public override DataType copy() {
			var result = new EnumValueType((ValaEnum)type_symbol);
			result.source_reference = source_reference;
			result.value_owned = value_owned;
			result.nullable = nullable;

			return result;
		}

		public Method get_to_string_method() {
			if (to_string_method == null) {
				var string_type = new ObjectType((Class)CodeContext.get().root.scope.lookup("string"));
				string_type.value_owned = false;
				to_string_method = new Method("to_string", string_type);
				to_string_method.access = SymbolAccessibility.PUBLIC;
				to_string_method.external = true;
				to_string_method.owner = type_symbol.scope;
				to_string_method.this_parameter = new Parameter("this", this);
				to_string_method.scope.add(to_string_method.this_parameter.name, to_string_method.this_parameter);
			}
			return to_string_method;
		}

		public override Symbol get_member(string member_name) {
			var result = base.get_member(member_name);
			if (result == null && member_name == "to_string") {
				return get_to_string_method();
			}
			return result;
		}
	}
}
