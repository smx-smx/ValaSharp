using CLanguage.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLanguage {
	/// <summary>
	/// Represents a function call in the C code.
	/// </summary>
	public class CCodeFunctionCall : CCodeExpression {
		/// <summary>
		/// The function to be called.
		/// </summary>
		public CCodeExpression call { get; set; }

		private List<CCodeExpression> arguments = new List<CCodeExpression>();

		public CCodeFunctionCall(CCodeExpression call = null) {
			this.call = call;
		}

		/// <summary>
		/// Appends the specified expression to the list of arguments.
		/// 
		/// <param name="expr">a C code expression</param>
		/// </summary>
		public void add_argument(CCodeExpression expr) {
			arguments.Add(expr);
		}

		public void insert_argument(int index, CCodeExpression expr) {
			arguments.Insert(index, expr);
		}

		/// <summary>
		/// Returns a copy of the list of arguments.
		/// 
		/// <returns>list of arguments</returns>
		/// </summary>
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
