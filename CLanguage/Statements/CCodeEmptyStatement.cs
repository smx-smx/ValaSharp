using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLanguage.Statements {
	/// <summary>
	/// Represents an empty statement in the C code.
	/// </summary>
	public class CCodeEmptyStatement : CCodeStatement {
		public override void write(CCodeWriter writer) {
			writer.write_indent();
			writer.write_string(";");
			writer.write_newline();
		}
	}
}
