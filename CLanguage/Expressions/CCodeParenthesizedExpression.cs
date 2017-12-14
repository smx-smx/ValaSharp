using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLanguage.Expressions {
	/// <summary>
	/// Represents a parenthesized expression in the C code.
	/// </summary>
	public class CCodeParenthesizedExpression : CCodeExpression {
		/// <summary>
		/// The expression in the parenthesis.
		/// </summary>
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
