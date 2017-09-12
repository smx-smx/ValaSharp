using CLanguage.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLanguage
{
	/**
	 * Represents a function call in the C code.
	 */
	public class CCodeFunctionCall : CCodeExpression
	{
		/**
		 * The function to be called.
		 */
		public CCodeExpression call { get; set; }

		private List<CCodeExpression> arguments = new List<CCodeExpression>();

		public CCodeFunctionCall(CCodeExpression call = null) {
			this.call = call;
		}

		/**
		 * Appends the specified expression to the list of arguments.
		 *
		 * @param expr a C code expression
		 */
		public void add_argument(CCodeExpression expr) {
			arguments.Add(expr);
		}

		public void insert_argument(int index, CCodeExpression expr) {
			arguments.Insert(index, expr);
		}

		/**
		 * Returns a copy of the list of arguments.
		 *
		 * @return list of arguments
		 */
		public List<CCodeExpression> get_arguments() {
			return arguments;
		}

		public override void write(CCodeWriter writer) {
			call.write_inner(writer);
			writer.write_string(" (");

			bool first = true;
			foreach (CCodeExpression expr in arguments) {
				if (!first) {
					writer.write_string(", ");
				} else {
					first = false;
				}

				if (expr != null) {
					expr.write(writer);
				}
			}

			writer.write_string(")");
		}
	}
}
