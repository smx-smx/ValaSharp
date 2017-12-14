using CLanguage.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLanguage {

	/// <summary>
	/// Represents a struct or array initializer list in the C code.
	/// </summary>
	public class CCodeInitializerList : CCodeExpression {
		private List<CCodeExpression> initializers = new List<CCodeExpression>();

		/// <summary>
		/// Appends the specified expression to this initializer list.
		/// 
		/// <param name="expr">an expression</param>
		/// </summary>
		public void append(CCodeExpression expr) {
			initializers.Add(expr);
		}

		public override void write(CCodeWriter writer) {
			writer.write_string("{");

			bool first = true;
			foreach (CCodeExpression expr in initializers) {
				if (!first) {
					writer.write_string(", ");
				} else {
					first = false;
				}

				if (expr != null) {
					expr.write(writer);
				}
			}

			writer.write_string("}");
		}
	}

}
