using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.CodeNodes;
using Vala.Lang.Parser;

namespace Vala.Lang.Statements
{
	/**
	* Interface for all statement types.
	*/
	public class Statement : CodeNode {
		public Block block;
		public Statement(SourceReference source_reference) {
			this.source_reference = source_reference;
			block = new Block(source_reference);
		}
	}
}
