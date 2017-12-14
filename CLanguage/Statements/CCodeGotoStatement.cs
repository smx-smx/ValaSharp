using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLanguage.Statements {
	/// <summary>
	/// Represents a goto statement in the C code.
	/// </summary>
	public class CCodeGotoStatement : CCodeStatement {
		/// <summary>
		/// The name of the target label.
		/// </summary>
		public string name { get; set; }

		public CCodeGotoStatement(string name) {
			this.name = name;
		}

		public override void write(CCodeWriter writer) {
			writer.write_indent();
			writer.write_string("goto ");
			writer.write_string(name);
			writer.write_string(";");
			writer.write_newline();
		}
	}
}
