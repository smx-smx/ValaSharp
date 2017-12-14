using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.TypeSymbols;

namespace Vala.Lang.Types {
	/// <summary>
	/// A boolean type.
	/// </summary>
	public class BooleanType : ValaValueType {
		public BooleanType(Struct type_symbol) : base(type_symbol) { }

		public override DataType copy() {
			var result = new BooleanType((Struct)type_symbol);
			result.source_reference = source_reference;
			result.value_owned = value_owned;
			result.nullable = nullable;
			return result;
		}
	}
}
