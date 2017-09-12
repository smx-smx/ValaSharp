using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.TypeSymbols;

namespace Vala.Lang.Types
{
	public class StructValueType : ValaValueType
	{
		public StructValueType(Struct type_symbol) : base(type_symbol) {
		}

		public override bool is_invokable() {
			var st = type_symbol as Struct;
			if (st != null && st.default_construction_method != null) {
				return true;
			} else {
				return false;
			}
		}

		public override DataType get_return_type() {
			var st = type_symbol as Struct;
			if (st != null && st.default_construction_method != null) {
				return st.default_construction_method.return_type;
			} else {
				return null;
			}
		}

		public override List<Parameter> get_parameters() {
			var st = type_symbol as Struct;
			if (st != null && st.default_construction_method != null) {
				return st.default_construction_method.get_parameters();
			} else {
				return null;
			}
		}

		public override DataType copy() {
			var result = new StructValueType((Struct)type_symbol);
			result.source_reference = source_reference;
			result.value_owned = value_owned;
			result.nullable = nullable;

			foreach (DataType arg in get_type_arguments()) {
				result.add_type_argument(arg.copy());
			}

			return result;
		}
	}
}
