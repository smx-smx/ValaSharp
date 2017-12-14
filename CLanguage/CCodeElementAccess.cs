using CLanguage.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLanguage {
	/// <summary>
	/// Represents an access to an array member in the C code.
	/// </summary>
	public class CCodeElementAccess : CCodeExpression {
		/// <summary>
		/// Expression representing the container on which we want to access.
		/// </summary>
		public CCodeExpression container { get; set; }

		/// <summary>
		/// Expression representing the index we want to access inside the
		/// container.
		/// </summary>
		public CCodeExpression index { get; set; }

		public CCodeElementAccess(CCodeExpression cont, CCodeExpression i) {
			container = cont;
			index = i;
		}

		public override void write(CCodeWriter writer) {
			container.write_inner(writer);
			writer.write_string("[");
			index.write(writer);
			writer.write_string("]");
		}
	}

}
