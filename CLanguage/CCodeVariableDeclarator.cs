using CLanguage;
using CLanguage.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLanguage
{
	/**
	 * Represents a variable declarator in the C code.
	 */
	public class CCodeVariableDeclarator : CCodeDeclarator
	{
		/**
		 * The variable name.
		 */
		public string name { get; set; }

		/**
		 * The optional initializer expression.
		 */
		public CCodeExpression initializer { get; set; }

		/**
		 * The optional declarator suffix.
		 */
		public CCodeDeclaratorSuffix declarator_suffix { get; set; }

		/**
		 * Initializer only used to zero memory, safe to initialize as part
		 * of declaration at beginning of block instead of separate assignment.
		 */
		public bool init0 { get; set; }

		public CCodeVariableDeclarator(string name, CCodeExpression initializer = null, CCodeDeclaratorSuffix declarator_suffix = null) {
			this.name = name;
			this.initializer = initializer;
			this.declarator_suffix = declarator_suffix;
		}

		public static CCodeVariableDeclarator zero(string name, CCodeExpression initializer, CCodeDeclaratorSuffix declarator_suffix = null) {
			CCodeVariableDeclarator @this = new CCodeVariableDeclarator(name, initializer, declarator_suffix);
			@this.init0 = true;
			return @this;
		}

		public override void write(CCodeWriter writer) {
			writer.write_string(name);

			if (declarator_suffix != null) {
				declarator_suffix.write(writer);
			}

			if (initializer != null) {
				writer.write_string(" = ");
				initializer.write(writer);
			}
		}

		public override void write_declaration(CCodeWriter writer) {
			writer.write_string(name);

			if (declarator_suffix != null) {
				declarator_suffix.write(writer);
			}

			if (initializer != null && init0) {
				writer.write_string(" = ");
				initializer.write(writer);
			}
		}

		public override void write_initialization(CCodeWriter writer) {
			if (initializer != null && !init0) {
				writer.write_indent(line);

				writer.write_string(name);
				writer.write_string(" = ");
				initializer.write(writer);

				writer.write_string(";");
				writer.write_newline();
			}
		}
	}

	public class CCodeDeclaratorSuffix
	{
		bool array;
		CCodeExpression array_length;

		public static CCodeDeclaratorSuffix with_array(CCodeExpression array_length = null) {
			CCodeDeclaratorSuffix @this = new CCodeDeclaratorSuffix();
			@this.array_length = array_length;
			@this.array = true;
			return @this;
		}

		public void write(CCodeWriter writer) {
			if (array) {
				writer.write_string("[");
				if (array_length != null) {
					array_length.write(writer);
				}
				writer.write_string("]");
			}
		}
	}
}