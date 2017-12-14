using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.Types;

namespace CCodeGen {
	/// <summary>
	/// A C type, used only for code generation purposes.
	/// </summary>
	public class CType : DataType {
		/// <summary>
		/// The name of the C type.
		/// </summary>
		public string ctype_name { get; set; }

		public CType(string ctype_name) {
			this.ctype_name = ctype_name;
		}

		public override DataType copy() {
			return new CType(ctype_name);
		}
	}

}
