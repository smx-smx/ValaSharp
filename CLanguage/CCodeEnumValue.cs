using CLanguage.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLanguage {
	/// <summary>
	/// Represents an enum value in the C code.
	/// </summary>
	public class CCodeEnumValue : CCodeNode {
		/// <summary>
		/// The name of this enum value.
		/// </summary>
		public string name { get; set; }

		/// <summary>
		/// The numerical representation of this enum value.
		/// </summary>
		public CCodeExpression value { get; set; }

		public CCodeEnumValue(string name, CCodeExpression value = null) {
			this.name = name;
			this.value = value;
		}

		public override void write(CCodeWriter writer) {
			writer.write_string(name);
			if (modifiers.HasFlag(CCodeModifiers.DEPRECATED)) {
				// FIXME Requires GCC 6.0 to work at this place
				// https://gcc.gnu.org/bugzilla/show_bug.cgi?id=47043
				//writer.write_string (" G_GNUC_DEPRECATED");
			}
			if (value != null) {
				writer.write_string(" = ");
				value.write(writer);
			}
		}
	}

}
