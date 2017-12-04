using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLanguage {
	/**
	 * Represents a struct declaration in the C code.
	 */
	public class CCodeStruct : CCodeNode {
		/**
		 * The struct name.
		 */
		public string name { get; set; }

		public bool is_empty { get { return declarations.Count == 0; } }

		private List<CCodeDeclaration> declarations = new List<CCodeDeclaration>();

		public CCodeStruct(string name) {
			this.name = name;
		}

		/**
		 * Adds the specified declaration as member to this struct.
		 *
		 * @param decl a variable declaration
		 */
		public void add_declaration(CCodeDeclaration decl) {
			declarations.Add(decl);
		}

		/**
		 * Adds a variable with the specified type and name to this struct.
		 *
		 * @param type_name field type
		 * @param name      member name
		 */
		public void add_field(string type_name, string name, CCodeModifiers modifiers = 0, CCodeDeclaratorSuffix declarator_suffix = null) {
			var decl = new CCodeDeclaration(type_name);
			decl.add_declarator(new CCodeVariableDeclarator(name, null, declarator_suffix));
			decl.modifiers = modifiers;
			add_declaration(decl);
		}

		public override void write(CCodeWriter writer) {
			writer.write_string("struct ");
			writer.write_string(name);
			writer.write_begin_block();
			foreach (CCodeDeclaration decl in declarations) {
				decl.write_declaration(writer);
			}

			writer.write_end_block();
			if (modifiers.HasFlag(CCodeModifiers.DEPRECATED)) {
				writer.write_string(" G_GNUC_DEPRECATED");
			}
			writer.write_string(";");
			writer.write_newline();
			writer.write_newline();
		}
	}
}
