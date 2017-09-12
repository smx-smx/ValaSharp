using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLanguage
{
	/**
	 * Represents a line break in the C code.
	 */
	public class CCodeNewline : CCodeNode
	{
		public override void write(CCodeWriter writer) {
			writer.write_newline();
		}
	}
}
