using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLanguage {
	/// <summary>
	/// Represents a typedef in the C code.
	/// </summary>
	public class CCodeTypeDefinition : CCodeNode {
		/// <summary>
		/// The type name.
		/// </summary>
		public string type_name { get; set; }

		/// <summary>
		/// The type declarator.
		/// </summary>
		public CCodeDeclarator declarator { get; set; }

		public CCodeTypeDefinition(string type, CCodeDeclarator decl) {
			type_name = type;
			declarator = decl;
		}

		public override void write(CCodeWriter writer) {
		}

		public override void write_declaration(CCodeWriter writer) {
			writer.write_indent();
			writer.write_string("typedef ");

			writer.write_string(type_name);

			writer.write_string(" ");

			declarator.write_declaration(writer);

			if (modifiers.HasFlag(CCodeModifiers.DEPRECATED)) {
				writer.write_string(" G_GNUC_DEPRECATED");
			}

			writer.write_string(";");
			writer.write_newline();
		}
	}
}
