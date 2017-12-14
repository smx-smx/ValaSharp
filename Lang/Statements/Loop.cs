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
	/// Represents an endless loop.
	/// </summary>
	public class Loop : CodeNode, Statement {
		public CodeNode node {
			get { return this; }
		}

		/// <summary>
		/// Specifies the loop body.
		/// </summary>
		public Block body {
			get {
				return _body;
			}
			set {
				_body = value;
				_body.parent_node = this;
			}
		}

		private Block _body;

		/// <summary>
		/// Creates a new loop.
		/// 
		/// <param name="body">loop body</param>
		/// <param name="source_reference">reference to source code</param>
		/// <returns>newly created while statement</returns>
		/// </summary>
		public Loop(Block body, SourceReference source_reference = null) {
			this.body = body;
			this.source_reference = source_reference;
		}

		public override void accept(CodeVisitor visitor) {
			visitor.visit_loop(this);
		}

		public override void accept_children(CodeVisitor visitor) {
			body.accept(visitor);
		}

		public override bool check(CodeContext context) {
			if (is_checked) {
				return !error;
			}

			is_checked = true;

			body.check(context);

			add_error_types(body.get_error_types());

			return !error;
		}

		public override void emit(CodeGenerator codegen) {
			codegen.visit_loop(this);
		}
	}

}
