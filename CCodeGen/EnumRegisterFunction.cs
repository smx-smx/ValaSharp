using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.CodeNodes;
using Vala.Lang.Symbols;
using Vala.Lang.TypeSymbols;

namespace CCodeGen
{
	/**
	 * C function to register an enum at runtime.
	 */
	public class EnumRegisterFunction : TypeRegisterFunction
	{
		/**
		 * Specifies the enum to be registered.
		 */
		public ValaEnum enum_reference { get; set; }

		/**
		 * Creates a new C function to register the specified enum at runtime.
		 *
		 * @param en an enum
		 * @return   newly created enum register function
		 */
		public EnumRegisterFunction(ValaEnum en, CodeContext context) {
			enum_reference = en;
			this.context = context;
		}

		public override TypeSymbol get_type_declaration() {
			return enum_reference;
		}

		public override SymbolAccessibility get_accessibility() {
			return enum_reference.access;
		}
	}

}
