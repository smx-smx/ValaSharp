using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.CodeNodes;

namespace ValaLanguageServer {
	public class ScopedNode<Tnode, Tscope>
		where Tnode : CodeNode
		where Tscope: CodeNode
	{
		public Tnode Node;
		public Tscope Scope;
	}
}
