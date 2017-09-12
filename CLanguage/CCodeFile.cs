using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLanguage
{
	public class CCodeFile
	{
		public bool is_header { get; set; }

		HashSet<string> declarations = new HashSet<string>();
		HashSet<string> includes = new HashSet<string>();
		CCodeFragment comments = new CCodeFragment();
		CCodeFragment include_directives = new CCodeFragment();
		CCodeFragment type_declaration = new CCodeFragment();
		CCodeFragment type_definition = new CCodeFragment();
		CCodeFragment type_member_declaration = new CCodeFragment();
		CCodeFragment constant_declaration = new CCodeFragment();
		CCodeFragment type_member_definition = new CCodeFragment();

		public bool add_declaration(string name) {
			if (declarations.Contains(name)) {
				return true;
			}
			declarations.Add(name);
			return false;
		}

		public void add_comment(CCodeComment comment) {
			comments.append(comment);
		}

		public void add_include(string filename, bool local = false) {
			if (!(includes.Contains(filename))) {
				include_directives.append(new CCodeIncludeDirective(filename, local));
				includes.Add(filename);
			}
		}

		public void add_type_declaration(CCodeNode node) {
			type_declaration.append(node);
		}

		public void add_type_definition(CCodeNode node) {
			type_definition.append(node);
		}

		public void add_type_member_declaration(CCodeNode node) {
			type_member_declaration.append(node);
		}

		public void add_constant_declaration(CCodeNode node) {
			constant_declaration.append(node);
		}

		public void add_type_member_definition(CCodeNode node) {
			type_member_definition.append(node);
		}

		public void add_function_declaration(CCodeFunction func) {
			var decl = func.copy();
			decl.is_declaration = true;
			type_member_declaration.append(decl);
		}

		public void add_function(CCodeFunction func) {
			type_member_definition.append(func);
		}

		public List<string> get_symbols() {
			var symbols = new List<string>();
			get_symbols_from_fragment(symbols, type_member_declaration);
			return symbols;
		}

		void get_symbols_from_fragment(List<string> symbols, CCodeFragment fragment) {
			foreach (CCodeNode node in fragment.get_children()) {
				if (node is CCodeFragment) {
					get_symbols_from_fragment(symbols, (CCodeFragment)node);
				} else {
					var func = node as CCodeFunction;
					if (func != null) {
						symbols.Add(func.name);
					}
				}
			}
		}

		static string get_define_for_filename(string filename) {
			var define = new StringBuilder("__");

			var i = filename.GetEnumerator();
			int length = filename.Length;
			while (length > 0) {
				var c = i.Current;
				if (Char.IsLetterOrDigit(c) && c < 0x80) {
					define.Append(Char.ToUpper(c));
				} else {
					define.Append('_');
				}

				i.MoveNext();
			}

			define.Append("__");

			return define.ToString();
		}

		public bool store(string filename, string source_filename, bool write_version, bool line_directives, string begin_decls = null, string end_decls = null) {
			var writer = new CCodeWriter(filename, source_filename);
			if (!writer.open(write_version)) {
				return false;
			}

			if (!is_header) {
				writer.line_directives = line_directives;

				comments.write(writer);
				writer.write_newline();
				include_directives.write(writer);
				writer.write_newline();
				type_declaration.write_combined(writer);
				writer.write_newline();
				type_definition.write_combined(writer);
				writer.write_newline();
				type_member_declaration.write_declaration(writer);
				writer.write_newline();
				type_member_declaration.write(writer);
				writer.write_newline();
				constant_declaration.write_combined(writer);
				writer.write_newline();
				type_member_definition.write(writer);
				writer.write_newline();
			} else {
				writer.write_newline();

				var once = new CCodeOnceSection(get_define_for_filename(writer.filename));
				once.append(new CCodeNewline());
				once.append(include_directives);
				once.append(new CCodeNewline());

				if (begin_decls != null) {
					once.append(new CCodeIdentifier(begin_decls));
					once.append(new CCodeNewline());
				}

				once.append(new CCodeNewline());
				once.append(type_declaration);
				once.append(new CCodeNewline());
				once.append(type_definition);
				once.append(new CCodeNewline());
				once.append(type_member_declaration);
				once.append(new CCodeNewline());
				once.append(constant_declaration);
				once.append(new CCodeNewline());

				if (begin_decls != null) {
					once.append(new CCodeIdentifier(end_decls));
					once.append(new CCodeNewline());
				}

				once.append(new CCodeNewline());
				once.write(writer);
			}

			writer.close();

			return true;
		}
	}
}
