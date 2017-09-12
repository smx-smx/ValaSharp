using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.Parser;
using Vala.Lang.Symbols;

namespace Vala.Lang.CodeNodes
{
	public class UsingDirective : CodeNode
	{
		/**
	 * The symbol of the namespace this using directive is referring to.
	 */
		public Symbol namespace_symbol { get; set; }

		/**
		 * Creates a new using directive.
		 *
		 * @param namespace_symbol namespace symbol
		 * @return                 newly created using directive
		 */
		public UsingDirective(Symbol namespace_symbol, SourceReference source_reference = null) {
			this.namespace_symbol = namespace_symbol;
			this.source_reference = source_reference;
		}

		public override void accept(CodeVisitor visitor) {
			visitor.visit_using_directive(this);
		}
	}
}
