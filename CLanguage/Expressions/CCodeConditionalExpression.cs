using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLanguage.Expressions {
	/// <summary>
	/// Represents a conditional expression in C code.
	/// </summary>
	public class CCodeConditionalExpression : CCodeExpression {
		/// <summary>
		/// The condition.
		/// </summary>
		public CCodeExpression condition { get; set; }

		/// <summary>
		/// The expression to be evaluated if the condition holds.
		/// </summary>
		public CCodeExpression true_expression { get; set; }

		/// <summary>
		/// The expression to be evaluated if the condition doesn't hold.
		/// </summary>
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
