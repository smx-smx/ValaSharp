using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLanguage.Expressions {
	/**
	 * Represents a parenthesized expression in the C code.
	 */
	public class CCodeParenthesizedExpression : CCodeExpression {
		/**
		 * The expression in the parenthesis.
		 */
		public CCodeExpression inner { get; set; }

		public CCodeParenthesizedExpression(CCodeExpression expr) {
			inner = expr;
		}

		public override void write(CCodeWriter writer) {
			writer.write_string("(");

			inner.write(writer);

			writer.write_string(")");
		}
	}
}
