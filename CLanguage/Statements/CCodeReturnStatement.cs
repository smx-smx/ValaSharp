using CLanguage.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLanguage.Statements {

	/**
	 * Represents a return statement in the C code.
	 */
	public class CCodeReturnStatement : CCodeStatement {
		/**
		 * The optional expression to return.
		 */
		public CCodeExpression return_expression { get; set; }

		public CCodeReturnStatement(CCodeExpression expr = null) {
			return_expression = expr;
		}

		public override void write(CCodeWriter writer) {
			writer.write_indent(line);
			writer.write_string("return");

			if (return_expression != null) {
				writer.write_string(" ");
				return_expression.write(writer);
			}

			writer.write_string(";");
			writer.write_newline();
		}
	}

}
