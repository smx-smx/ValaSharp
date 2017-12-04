using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLanguage {
	/**
 * Represents an include preprocessor directive in the C code.
 */
	public class CCodeIncludeDirective : CCodeNode {
		/**
		 * The file to be included.
		 */
		public string filename { get; set; }

		/**
		 * Specifies whether the specified file should be searched in the local
		 * directory.
		 */
		public bool local { get; set; }

		public CCodeIncludeDirective(string _filename, bool _local = false) {
			filename = _filename;
			local = _local;
		}

		public override void write(CCodeWriter writer) {
			writer.write_indent();
			writer.write_string("#include ");
			if (local) {
				writer.write_string("\"");
				writer.write_string(filename);
				writer.write_string("\"");
			} else {
				writer.write_string("<");
				writer.write_string(filename);
				writer.write_string(">");
			}
			writer.write_newline();
		}
	}

}
