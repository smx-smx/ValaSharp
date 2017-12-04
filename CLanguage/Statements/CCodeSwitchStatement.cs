using CLanguage.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLanguage.Statements {
	/**
 * Represents a switch selection statement in the C code.
 */
	public class CCodeSwitchStatement : CCodeBlock {
		/**
		 * The switch expression.
		 */
		public CCodeExpression expression { get; set; }

		public CCodeSwitchStatement(CCodeExpression expression) {
			this.expression = expression;
		}

		public override void write(CCodeWriter writer) {
			writer.write_indent(line);
			writer.write_string("switch (");
			expression.write(writer);
			writer.write_string(")");

			base.write(writer);
		}
	}

}
