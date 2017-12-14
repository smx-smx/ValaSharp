using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.CodeNodes;
using Vala.Lang.Symbols;
using Vala.Lang.TypeSymbols;

namespace CCodeGen {
	/// <summary>
	/// C function to register an enum at runtime.
	/// </summary>
	public class EnumRegisterFunction : TypeRegisterFunction {
		/// <summary>
		/// Specifies the enum to be registered.
		/// </summary>
		public ValaEnum enum_reference { get; set; }

		/// <summary>
		/// Creates a new C function to register the specified enum at runtime.
		/// 
		/// <param name="en">an enum</param>
		/// <returns>newly created enum register function</returns>
		/// </summary>
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
