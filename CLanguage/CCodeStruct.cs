using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLanguage {
	/// <summary>
	/// Represents a struct declaration in the C code.
	/// </summary>
	public class CCodeStruct : CCodeNode {
		/// <summary>
		/// The struct name.
		/// </summary>
		public string name { get; set; }

		public bool is_empty { get { return declarations.Count == 0; } }

		private List<CCodeDeclaration> declarations = new List<CCodeDeclaration>();

		public CCodeStruct(string name) {
			this.name = name;
		}

		/// <summary>
		/// Adds the specified declaration as member to this struct.
		/// 
		/// <param name="decl">a variable declaration</param>
		/// </summary>
		public void add_declaration(CCodeDeclaration decl) {
			declarations.Add(decl);
		}

		/// <summary>
		/// Adds a variable with the specified type and name to this struct.
		/// 
		/// <param name="type_name">field type</param>
		/// <param name="name">member name</param>
		/// </summary>
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
