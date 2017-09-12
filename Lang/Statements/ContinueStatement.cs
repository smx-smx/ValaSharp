using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.Code;
using Vala.Lang.CodeNodes;
using Vala.Lang.Parser;

namespace Vala.Lang.Statements
{
	/**
 * Represents a continue statement in the source code.
 */
	public class ContinueStatement : Statement
	{
		/**
		 * Creates a new continue statement.
		 *
		 * @param source reference to source code
		 * @return       newly created continue statement
		 */
		public ContinueStatement(SourceReference source) : base(source) {
		}

		public override void accept(CodeVisitor visitor) {
			visitor.visit_continue_statement(this);
		}

		public override void emit(CodeGenerator codegen) {
			codegen.visit_continue_statement(this);
		}
	}
}
