using LanguageServer.Parameters;
using LanguageServer.Parameters.TextDocument;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang;
using Vala.Lang.CodeNodes;
using Vala.Lang.Expressions;
using Vala.Lang.Methods;
using Vala.Lang.Parser;
using Vala.Lang.Symbols;
using Vala.Lang.TypeSymbols;

namespace ValaLanguageServer {
	public class CodeSearchVisitor : CodeTraversalVisitor {
		private string documentFile;
		private Position documentPos;

		private Dictionary<SourceReference, CompletionItem> results = new Dictionary<SourceReference, CompletionItem>();

		private Dictionary<SourceReference, ScopedNode<LocalVariable, Subroutine>> variables = new Dictionary<SourceReference, ScopedNode<LocalVariable, Subroutine>>();

		public CodeSearchVisitor(string docFile, Position pos) {
			this.documentFile = docFile;
			this.documentPos = pos;
		}

		public override void visit_local_variable(LocalVariable local) {
			if (
				!local.source_reference.MatchesPosition(documentPos) ||
				variables.ContainsKey(local.source_reference)
			) {
				return;
			}

			Logger.Get.Log("Found at " + local.source_reference);

			variables.Add(local.source_reference, new ScopedNode<LocalVariable, Subroutine> {
				Node = local,
				Scope = local.parent_symbol as Subroutine
			});

			base.visit_local_variable(local);
		}

		public List<CompletionItem> Search(CodeContext ctx) {
			ctx.accept(this);

			return results.Values.ToList();
		}
	}
}
