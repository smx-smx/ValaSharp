using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLanguage {
	/**
	 * Represents a section that should only to processed once.
	 */
	public class CCodeOnceSection : CCodeFragment {
		/**
		 * The name of the guarding define.
		 */
		public string define { get; set; }

		public CCodeOnceSection(string def) {
			define = def;
		}

		public override void write(CCodeWriter writer) {
			writer.write_indent();
			writer.write_string("#ifndef ");
			writer.write_string(define);
			writer.write_newline();
			writer.write_string("#define ");
			writer.write_string(define);
			writer.write_newline();
			foreach (CCodeNode node in get_children()) {
				node.write_combined(writer);
			}
			writer.write_indent();
			writer.write_string("#endif");
			writer.write_newline();
		}

		public override void write_declaration(CCodeWriter writer) {
		}
	}

}
