using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLanguage.Statements {
	/**
 * Represents a continue statement in the C code.
 */
	public class CCodeContinueStatement : CCodeStatement {
		public override void write(CCodeWriter writer) {
			writer.write_indent(line);
			writer.write_string("continue;");
			writer.write_newline();
		}
	}
}
