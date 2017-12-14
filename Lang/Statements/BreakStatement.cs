using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.Code;
using Vala.Lang.CodeNodes;
using Vala.Lang.Parser;

namespace Vala.Lang.Statements {
	/// <summary>
	/// Represents a break statement in the source code.
	/// </summary>
	public class BreakStatement : CodeNode, Statement {
		public CodeNode node {
			get { return this; }
		}

		/// <summary>
		/// Creates a new break statement.
		/// 
		/// <param name="source">reference to source code</param>
		/// <returns>newly created break statement</returns>
		/// </summary>
		public BreakStatement(SourceReference source) {
			source_reference = source;
		}

		public override void accept(CodeVisitor visitor) {
			visitor.visit_break_statement(this);
		}

		public override void emit(CodeGenerator codegen) {
			codegen.visit_break_statement(this);
		}
	}
}
