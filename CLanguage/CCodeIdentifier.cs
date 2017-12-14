using CLanguage.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLanguage {
	/// <summary>
	/// Represents a identifier in the C code.
	/// </summary>
	public class CCodeIdentifier : CCodeExpression {
		/// <summary>
		/// The name of this identifier.
		/// </summary>
		public string name { get; set; }

		public CCodeIdentifier(string _name) {
			name = _name;
		}

		public override void write(CCodeWriter writer) {
			writer.write_string(name);
		}
	}

}
