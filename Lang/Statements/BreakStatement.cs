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
	 * Represents a break statement in the source code.
	 */
	public class BreakStatement : Statement
	{
		/**
		 * Creates a new break statement.
		 *
		 * @param source reference to source code
		 * @return       newly created break statement
		 */
		public BreakStatement(SourceReference source) : base(source) {
		}

		public override void accept(CodeVisitor visitor) {
			visitor.visit_break_statement(this);
		}

		public override void emit(CodeGenerator codegen) {
			codegen.visit_break_statement(this);
		}
	}
}
