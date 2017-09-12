using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.TypeSymbols;

namespace Vala.Lang.Types
{
	/**
	 * An interface type.
	 */
	public class InterfaceType : ReferenceType
	{
		/**
		 * The referred interface.
		 */
		public Interface interface_symbol { get; set; }

		public InterfaceType(Interface interface_symbol) {
			this.interface_symbol = interface_symbol;
		}

		public override DataType copy() {
			var result = new InterfaceType(interface_symbol);
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
	}

}
