using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLanguage {
	/// <summary>
	/// Represents a formal parameter in a C method signature.
	/// </summary>
	public class CCodeParameter : CCodeNode {
		/// <summary>
		/// The parameter name.
		/// </summary>
		public string name { get; set; }

		/// <summary>
		/// The parameter type.
		/// </summary>
		public string type_name { get; set; }

		/// <summary>
		/// Specifies whether the function accepts an indefinite number of
		/// arguments.
		/// </summary>
		public bool ellipsis { get; set; }

		public CCodeParameter(string n, string type) {
			name = n;
			type_name = type;
		}

		public static CCodeParameter with_ellipsis() {
			CCodeParameter @this = new CCodeParameter(null, null);
			@this.ellipsis = true;
			return @this;
		}

		public override void write(CCodeWriter writer) {
			if (!ellipsis) {
				writer.write_string(type_name);
				writer.write_string(" ");
				writer.write_string(name);
			} else {
				writer.write_string("...");
			}
		}
	}

}
