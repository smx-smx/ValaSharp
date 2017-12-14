using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLanguage {
	/// <summary>
	/// Represents a local variable declaration in the C code.
	/// </summary>
	public class CCodeDeclaration : CCodeStatement {
		/// <summary>
		/// The type of the local variable.
		/// </summary>
		public string type_name { get; set; }

		private List<CCodeDeclarator> declarators = new List<CCodeDeclarator>();

		public CCodeDeclaration(string type_name) {
			this.type_name = type_name;
		}

		/// <summary>
		/// Adds the specified declarator to this declaration.
		/// 
		/// <param name="decl">a declarator</param>
		/// </summary>
		public void add_declarator(CCodeDeclarator decl) {
			declarators.Add(decl);
		}

		public override void write(CCodeWriter writer) {
			if ((modifiers & (CCodeModifiers.STATIC | CCodeModifiers.INTERNAL | CCodeModifiers.EXTERN)) == 0) {
				foreach (CCodeDeclarator decl in declarators) {
					decl.write_initialization(writer);
				}
			}
		}

		private bool has_initializer() {
			foreach (CCodeDeclarator decl in declarators) {
				var var_decl = decl as CCodeVariableDeclarator;
				if (var_decl != null && var_decl.initializer == null) {
					return false;
				}
			}
			return true;
		}

		public override void write_declaration(CCodeWriter writer) {
			if ((modifiers & (CCodeModifiers.STATIC | CCodeModifiers.INTERNAL | CCodeModifiers.EXTERN)) != 0) {
				// combined declaration and initialization for static and extern variables
				writer.write_indent(line);
				if ((modifiers & CCodeModifiers.INTERNAL) != 0) {
					writer.write_string("G_GNUC_INTERNAL ");
				}
				if ((modifiers & CCodeModifiers.STATIC) != 0) {
					writer.write_string("static ");
				}
				if ((modifiers & CCodeModifiers.VOLATILE) != 0) {
					writer.write_string("volatile ");
				}
				if ((modifiers & CCodeModifiers.EXTERN) != 0 && !has_initializer()) {
					writer.write_string("extern ");
				}
				if ((modifiers & CCodeModifiers.THREAD_LOCAL) != 0) {
					writer.write_string("thread_local ");
				}
				writer.write_string(type_name);
				writer.write_string(" ");

				bool _first = true;
				foreach (CCodeDeclarator decl in declarators) {
					if (!_first) {
						writer.write_string(", ");
					} else {
						_first = false;
					}
					decl.write(writer);
				}

				writer.write_string(";");
				writer.write_newline();
				return;
			}

			writer.write_indent();
			if ((modifiers & CCodeModifiers.REGISTER) == CCodeModifiers.REGISTER) {
				writer.write_string("register ");
			}
			if ((modifiers & CCodeModifiers.VOLATILE) != 0) {
				writer.write_string("volatile ");
			}
			writer.write_string(type_name);
			writer.write_string(" ");

			bool first = true;
			foreach (CCodeDeclarator decl in declarators) {
				if (!first) {
					writer.write_string(", ");
				} else {
					first = false;
				}
				decl.write_declaration(writer);
			}

			if (modifiers.HasFlag(CCodeModifiers.DEPRECATED)) {
				writer.write_string(" G_GNUC_DEPRECATED");
			}

			writer.write_string(";");
			writer.write_newline();
		}
	}

}
