using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLanguage
{
	/**
	 * Represents a label declaration in the C code.
	 */
	public class CCodeLabel : CCodeStatement
	{
		/**
		 * The name of this label.
		 */
		public string name { get; set; }

		public CCodeLabel(string name) {
			this.name = name;
		}

		public override void write(CCodeWriter writer) {
			writer.write_indent();
			writer.write_string(name);
			writer.write_string(":");
			writer.write_newline();
		}
	}
}
