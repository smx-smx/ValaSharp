using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.CodeNodes;
using Vala.Lang.Symbols;
using Vala.Lang.TypeSymbols;

namespace Vala.Lang.Types
{
	public abstract class ValaValueType : DataType
	{
		/**
		* The referred struct or enum.
		*/
		public TypeSymbol type_symbol { get; set; }

		public ValaValueType(TypeSymbol type_symbol) {
			this.type_symbol = type_symbol;
			data_type = type_symbol;
		}

		public override bool is_disposable() {
			if (!value_owned) {
				return false;
			}

			// nullable structs are heap allocated
			if (nullable) {
				return true;
			}

			var st = type_symbol as Struct;
			if (st != null) {
				return st.is_disposable();
			}

			return false;
		}

		public override bool check(CodeContext context) {
			return type_symbol.check(context);
		}
	}
}
