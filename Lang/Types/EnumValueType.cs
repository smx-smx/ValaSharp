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
		private Method ToString_method;

		public EnumValueType(ValaEnum type_symbol) : base(type_symbol) { }

		public override DataType copy() {
			var result = new EnumValueType((ValaEnum)type_symbol);
			result.source_reference = source_reference;
			result.value_owned = value_owned;
			result.nullable = nullable;

			return result;
		}

		public Method get_ToString_method() {
			if (ToString_method == null) {
				var string_type = new ObjectType((Class)CodeContext.get().root.scope.lookup("string"));
				string_type.value_owned = false;
				ToString_method = new Method("to_string", string_type);
				ToString_method.access = SymbolAccessibility.PUBLIC;
				ToString_method.external = true;
				ToString_method.owner = type_symbol.scope;
				ToString_method.this_parameter = new Parameter("this", this);
				ToString_method.scope.add(ToString_method.this_parameter.name, ToString_method.this_parameter);
			}
			return ToString_method;
		}

		public override Symbol get_member(string member_name) {
			var result = base.get_member(member_name);
			if (result == null && member_name == "to_string") {
				return get_ToString_method();
			}
			return result;
		}
	}
}
