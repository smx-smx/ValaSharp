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
	/// Represents a continue statement in the source code.
	/// </summary>
	public class ContinueStatement : CodeNode, Statement {
		public CodeNode node {
			get { return this; }
		}

		/// <summary>
		/// Creates a new continue statement.
		/// 
		/// <param name="source">reference to source code</param>
		/// <returns>newly created continue statement</returns>
		/// </summary>
		public ContinueStatement(SourceReference source) {
			source_reference = source;
		}

		public override void accept(CodeVisitor visitor) {
			visitor.visit_continue_statement(this);
		}

		public override void emit(CodeGenerator codegen) {
			codegen.visit_continue_statement(this);
		}
	}
}
