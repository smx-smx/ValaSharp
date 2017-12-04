using CLanguage.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLanguage {
	/**
	 * Represents an access to an array member in the C code.
	 */
	public class CCodeElementAccess : CCodeExpression {
		/**
		 * Expression representing the container on which we want to access.
		 */
		public CCodeExpression container { get; set; }

		/**
		 * Expression representing the index we want to access inside the
		 * container.
		 */
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
