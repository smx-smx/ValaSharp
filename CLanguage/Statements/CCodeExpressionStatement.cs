using CLanguage.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLanguage.Statements {
	/// <summary>
	/// Represents a C code statement that evaluates a given expression.
	/// </summary>
	public class CCodeExpressionStatement : CCodeStatement {
		/// <summary>
		/// The expression to evaluate.
		/// </summary>
		public CCodeExpression expression { get; set; }

		public CCodeExpressionStatement(CCodeExpression expr) {
			expression = expr;
		}

		public override void write(CCodeWriter writer) {
			if (expression is CCodeCommaExpression) {
				// expand comma expression into multiple statements
				// to improve code readability
				var ccomma = expression as CCodeCommaExpression;

				foreach (CCodeExpression expr in ccomma.get_inner()) {
					write_expression(writer, expr);
				}
			} else if (expression is CCodeParenthesizedExpression) {
				var cpar = expression as CCodeParenthesizedExpression;

				write_expression(writer, cpar.inner);
			} else {
				write_expression(writer, expression);
			}
		}

		private void write_expression(CCodeWriter writer, CCodeExpression expr) {
			writer.write_indent(line);
			if (expr != null) {
				expr.write(writer);
			}
			writer.write_string(";");
			writer.write_newline();
		}
	}
}
