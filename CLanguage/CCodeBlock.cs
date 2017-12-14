using CLanguage.Statements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLanguage {
	/// <summary>
	/// Represents a C code block.
	/// </summary>
	public class CCodeBlock : CCodeStatement {
		/// <summary>
		/// Specifies whether a newline at the end of the block should be
		/// suppressed.
		/// </summary>
		public bool suppress_newline { get; set; }

		private List<CCodeNode> statements = new List<CCodeNode>();

		/// <summary>
		/// Prepend the specified statement to the list of statements.
		/// </summary>
		public void prepend_statement(CCodeNode statement) {
			statements.Insert(0, statement);
		}

		/// <summary>
		/// Append the specified statement to the list of statements.
		/// </summary>
		public void add_statement(CCodeNode statement) {
			/* allow generic nodes to include comments */
			statements.Add(statement);
		}

		public override void write(CCodeWriter writer) {
			// the last reachable statement
			CCodeNode last_statement = null;

			writer.write_begin_block();
			foreach (CCodeNode statement in statements) {
				statement.write_declaration(writer);

				// determine last reachable statement
				if (statement is CCodeLabel || statement is CCodeCaseStatement) {
					last_statement = null;
				} else if (statement is CCodeReturnStatement || statement is CCodeGotoStatement
				|| statement is CCodeContinueStatement || statement is CCodeBreakStatement) {
					last_statement = statement;
				}
			}

			foreach (CCodeNode statement in statements) {
				statement.write(writer);

				// only output reachable code
				if (statement == last_statement) {
					break;
				}
			}

			writer.write_end_block();

			if (!suppress_newline) {
				writer.write_newline();
			}
		}
	}
}
