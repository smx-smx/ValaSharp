using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.Symbols;

namespace Vala.Lang.Types {
	/// <summary>
	/// The type of a generic type parameter.
	/// </summary>
	public class GenericType : DataType {
		public GenericType(TypeParameter type_parameter) {
			this.type_parameter = type_parameter;
			// type parameters are always considered nullable
			this.nullable = true;
		}

		public override DataType copy() {
			var result = new GenericType(type_parameter);
			result.source_reference = source_reference;
			result.value_owned = value_owned;
			result.nullable = nullable;
			result.floating_reference = floating_reference;

			return result;
		}

		public override DataType infer_type_argument(TypeParameter type_param, DataType value_type) {
			if (type_parameter == type_param) {
				var ret = value_type.copy();
				ret.value_owned = true;
				return ret;
			}

			return null;
		}

		public override string to_qualified_string(Scope scope = null) {
			return type_parameter.name;
		}

		public override Symbol get_member(string member_name) {
			return null;
		}
	}
}
