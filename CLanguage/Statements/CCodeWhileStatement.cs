using CLanguage.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLanguage.Statements {
	/**
 * Represents a while iteration statement in the C code.
 */
	public class CCodeWhileStatement : CCodeStatement {
		/**
		 * The loop condition.
		 */
		public CCodeExpression condition { get; set; }

		/**
		 * The loop body.
		 */
		public CCodeStatement body { get; set; }

		public CCodeWhileStatement(CCodeExpression cond, CCodeStatement stmt = null) {
			condition = cond;
			body = stmt;
		}

		public override void write(CCodeWriter writer) {
			writer.write_indent(line);
			writer.write_string("while (");

			condition.write(writer);

			writer.write_string(")");

			body.write(writer);
		}
	}
}
