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
	/// C function to register a struct at runtime.
	/// </summary>
	public class StructRegisterFunction : TypeRegisterFunction {
		/// <summary>
		/// Specifies the struct to be registered.
		/// </summary>
		public Struct struct_reference { get; set; }

		/// <summary>
		/// Creates a new C function to register the specified struct at runtime.
		/// 
		/// <param name="st">a struct</param>
		/// <returns>newly created struct register function</returns>
		/// </summary>
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
