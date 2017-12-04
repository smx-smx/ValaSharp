using CLanguage.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLanguage.Statements {
	/**
	 * Represents a case block in a switch statement in C code.
	 */
	public class CCodeCaseStatement : CCodeStatement {
		/**
		 * The case expression.
		 */
		public CCodeExpression expression { get; set; }

		public CCodeCaseStatement(CCodeExpression expression) {
			this.expression = expression;
		}

		public override void write(CCodeWriter writer) {
			writer.write_indent(line);
			writer.write_string("case ");
			expression.write(writer);
			writer.write_string(":");
			writer.write_newline();
		}
	}

}
