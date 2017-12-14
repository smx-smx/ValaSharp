using CLanguage.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLanguage.Statements {
	/// <summary>
	/// Represents a while iteration statement in the C code.
	/// </summary>
	public class CCodeWhileStatement : CCodeStatement {
		/// <summary>
		/// The loop condition.
		/// </summary>
		public CCodeExpression condition { get; set; }

		/// <summary>
		/// The loop body.
		/// </summary>
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
