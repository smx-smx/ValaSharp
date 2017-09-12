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
	 * C function to register a struct at runtime.
	 */
	public class StructRegisterFunction : TypeRegisterFunction
	{
		/**
		 * Specifies the struct to be registered.
		 */
		public Struct struct_reference { get; set; }

		/**
		 * Creates a new C function to register the specified struct at runtime.
		 *
		 * @param st a struct
		 * @return   newly created struct register function
		 */
		public StructRegisterFunction(Struct st, CodeContext context) {
			struct_reference = st;
			this.context = context;
		}

		public override TypeSymbol get_type_declaration() {
			return struct_reference;
		}

		public override SymbolAccessibility get_accessibility() {
			return struct_reference.access;
		}
	}

}
