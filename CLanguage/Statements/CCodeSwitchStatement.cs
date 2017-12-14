using CLanguage.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLanguage.Statements {
	/// <summary>
	/// Represents a switch selection statement in the C code.
	/// </summary>
	public class CCodeSwitchStatement : CCodeBlock {
		/// <summary>
		/// The switch expression.
		/// </summary>
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
