using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.TypeSymbols;

namespace Vala.Lang.Types {
	/**
	 * An interface type.
	 */
	public class InterfaceType : ReferenceType {
		private WeakReference<Interface> interface_symbol_weak = new WeakReference<Interface>(null);

		/**
		 * The referred interface.
		 */
		public Interface interface_symbol {
			get {
				return interface_symbol_weak.GetTarget();
			}
			set {
				interface_symbol_weak.SetTarget(value);
			}
		}

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
