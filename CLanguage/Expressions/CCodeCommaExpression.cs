using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLanguage.Expressions {
	/**
	 * Represents a comma separated expression list in the C code.
	 */
	public class CCodeCommaExpression : CCodeExpression {
		private List<CCodeExpression> inner = new List<CCodeExpression>();

		/**
		 * Appends the specified expression to the expression list.
		 *
		 * @param expr a C code expression
		 */
		public void append_expression(CCodeExpression expr) {
			inner.Add(expr);
		}

		public void set_expression(int index, CCodeExpression expr) {
			inner[index] = expr;
		}

		public List<CCodeExpression> get_inner() {
			return inner;
		}

		public override void write(CCodeWriter writer) {
			bool first = true;

			writer.write_string("(");
			foreach (CCodeExpression expr in inner) {
				if (!first) {
					writer.write_string(", ");
				} else {
					first = false;
				}
				expr.write(writer);
			}
			writer.write_string(")");
		}
	}

}
