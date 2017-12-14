using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLanguage {
	/// <summary>
	/// Represents a label declaration in the C code.
	/// </summary>
	public class CCodeLabel : CCodeStatement {
		/// <summary>
		/// The name of this label.
		/// </summary>
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
