using CLanguage.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLanguage.Statements {
	/**
	 * Represents an if selection statement in the C code.
	 */
	public class CCodeIfStatement : CCodeStatement {
		/**
		 * The boolean condition to evaluate.
		 */
		public CCodeExpression condition { get; set; }

		/**
		 * The statement to be evaluated if the condition holds.
		 */
		public CCodeStatement true_statement { get; set; }

		/**
		 * The optional statement to be evaluated if the condition doesn't hold.
		 */
		public CCodeStatement false_statement { get; set; }

		public CCodeIfStatement(CCodeExpression cond, CCodeStatement true_stmt, CCodeStatement false_stmt = null) {
			condition = cond;
			true_statement = true_stmt;
			false_statement = false_stmt;
		}

		/**
		 * Specifies whether this if statement is part of an else if statement.
		 * This only affects the output formatting.
		 */
		public bool else_if { get; set; }

		public override void write(CCodeWriter writer) {
			if (!else_if) {
				writer.write_indent(line);
			} else {
				writer.write_string(" ");
			}
			writer.write_string("if (");
			if (condition != null) {
				condition.write(writer);
			}
			writer.write_string(")");

			/* else shouldn't be on a separate line */
			if (false_statement != null && true_statement is CCodeBlock) {
				var cblock = (CCodeBlock)true_statement;
				cblock.suppress_newline = true;
			}

			true_statement.write(writer);
			if (false_statement != null) {
				if (writer.bol) {
					writer.write_indent();
					writer.write_string("else");
				} else {
					writer.write_string(" else");
				}

				/* else if should be on one line */
				if (false_statement is CCodeIfStatement) {
					var cif = (CCodeIfStatement)false_statement;
					cif.else_if = true;
				}

				false_statement.write(writer);
			}
		}
	}

}
