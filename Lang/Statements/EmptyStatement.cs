using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.Code;
using Vala.Lang.CodeNodes;
using Vala.Lang.Parser;

namespace Vala.Lang.Statements {
	public class EmptyStatement : CodeNode, Statement {
		public CodeNode node {
			get { return this; }
		}

		/// <summary>
		/// Creates a new empty statement.
		/// 
		/// <param name="source">reference to source code</param>
		/// <returns>newly created empty statement</returns>
		/// </summary>
		public EmptyStatement(SourceReference source) {
			source_reference = source;
		}

		public override void accept(CodeVisitor visitor) {
			visitor.visit_empty_statement(this);
		}

		public override void emit(CodeGenerator codegen) {
			codegen.visit_empty_statement(this);
		}
	}
}
