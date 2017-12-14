using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLanguage {
	/// <summary>
	/// Represents a line break in the C code.
	/// </summary>
	public class CCodeNewline : CCodeNode {
		public override void write(CCodeWriter writer) {
			writer.write_newline();
		}
	}
}
