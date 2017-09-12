using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLanguage
{
	/**
 * Represents a formal parameter in a C method signature.
 */
	public class CCodeParameter : CCodeNode
	{
		/**
		 * The parameter name.
		 */
		public string name { get; set; }

		/**
		 * The parameter type.
		 */
		public string type_name { get; set; }

		/**
		 * Specifies whether the function accepts an indefinite number of
		 * arguments.
		 */
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
