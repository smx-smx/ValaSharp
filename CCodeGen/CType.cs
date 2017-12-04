using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.Types;

namespace CCodeGen {
	/**
 * A C type, used only for code generation purposes.
 */
	public class CType : DataType {
		/**
		 * The name of the C type.
		 */
		public string ctype_name { get; set; }

		public CType(string ctype_name) {
			this.ctype_name = ctype_name;
		}

		public override DataType copy() {
			return new CType(ctype_name);
		}
	}

}
