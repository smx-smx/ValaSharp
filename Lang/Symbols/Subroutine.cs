using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.CodeNodes;
using Vala.Lang.Parser;

namespace Vala.Lang.Symbols
{
	public abstract class Subroutine : Symbol
	{
		Block _body;

		public BasicBlock entry_block { get; set; }

		public BasicBlock return_block { get; set; }

		public BasicBlock exit_block { get; set; }

		/**
		 * Specifies the generated `result` variable for postconditions.
		 */
		public LocalVariable result_var { get; set; }

		public abstract bool has_result { get; }

		protected Subroutine(string name, SourceReference source_reference, Comment comment = null) 
			: base(name, source_reference, comment) { }

		public Block body {
			get { return _body; }
			set {
				_body = value;
				if (_body != null) {
					_body.owner = scope;
					_body.parent_node = this;
				}
			}
		}
	}
}
