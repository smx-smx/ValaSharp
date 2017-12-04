using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLanguage.Expressions {
	/**
	 * Represents a type cast in the generated C code.
	 */
	public class CCodeCastExpression : CCodeExpression {
		/**
		 * The expression to be cast.
		 */
		public CCodeExpression inner { get; set; }

		/**
		 * The target type.
		 */
		public string type_name { get; set; }

		public CCodeCastExpression(CCodeExpression expr, string type) {
			inner = expr;
			type_name = type;
		}

		public override void write(CCodeWriter writer) {
			writer.write_string("(");
			writer.write_string(type_name);
			writer.write_string(") ");

			inner.write_inner(writer);
		}

		public override void write_inner(CCodeWriter writer) {
			writer.write_string("(");
			this.write(writer);
			writer.write_string(")");
		}
	}
}
