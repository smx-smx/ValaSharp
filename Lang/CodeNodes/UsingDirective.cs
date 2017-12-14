using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.Parser;
using Vala.Lang.Symbols;

namespace Vala.Lang.CodeNodes {
	public class UsingDirective : CodeNode {
		/// <summary>
		/// The symbol of the namespace this using directive is referring to.
		/// </summary>
		public Symbol namespace_symbol { get; set; }

		/// <summary>
		/// Creates a new using directive.
		/// 
		/// <param name="namespace_symbol">namespace symbol</param>
		/// <returns>newly created using directive</returns>
		/// </summary>
		public UsingDirective(Symbol namespace_symbol, SourceReference source_reference = null) {
			this.namespace_symbol = namespace_symbol;
			this.source_reference = source_reference;
		}

		public override void accept(CodeVisitor visitor) {
			visitor.visit_using_directive(this);
		}
	}
}
