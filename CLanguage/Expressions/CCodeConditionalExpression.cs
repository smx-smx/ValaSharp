using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLanguage.Expressions
{
	/**
 * Represents a conditional expression in C code.
 */
	public class CCodeConditionalExpression : CCodeExpression
	{
		/**
		 * The condition.
		 */
		public CCodeExpression condition { get; set; }

		/**
		 * The expression to be evaluated if the condition holds.
		 */
		public CCodeExpression true_expression { get; set; }

		/**
		 * The expression to be evaluated if the condition doesn't hold.
		 */
		public CCodeExpression false_expression { get; set; }

		public CCodeConditionalExpression(CCodeExpression cond, CCodeExpression true_expr, CCodeExpression false_expr) {
			condition = cond;
			true_expression = true_expr;
			false_expression = false_expr;
		}

		public override void write(CCodeWriter writer) {
			condition.write_inner(writer);
			writer.write_string(" ? ");
			true_expression.write_inner(writer);
			writer.write_string(" : ");
			false_expression.write_inner(writer);
		}

		public override void write_inner(CCodeWriter writer) {
			writer.write_string("(");
			this.write(writer);
			writer.write_string(")");
		}
	}

}
