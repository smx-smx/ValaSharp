using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLanguage.Statements {
	/**
	 * Represents a break statement in the C code.
	 */
	public class CCodeBreakStatement : CCodeStatement {
		public override void write(CCodeWriter writer) {
			writer.write_indent(line);
			writer.write_string("break;");
			writer.write_newline();
		}
	}
}
