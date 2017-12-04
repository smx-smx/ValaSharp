using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.TypeSymbols;

namespace Vala.Lang.Types {
	/**
	 * A floating-point type.
	 */
	public class FloatingType : ValaValueType {
		public FloatingType(Struct type_symbol) : base(type_symbol) {
		}

		public override DataType copy() {
			var result = new FloatingType((Struct)type_symbol);
			result.source_reference = source_reference;
			result.value_owned = value_owned;
			result.nullable = nullable;
			return result;
		}
	}
}
