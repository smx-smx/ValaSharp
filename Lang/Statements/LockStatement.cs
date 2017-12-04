using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.Code;
using Vala.Lang.CodeNodes;
using Vala.Lang.Expressions;
using Vala.Lang.Parser;

namespace Vala.Lang.Statements {
	/**
 * Represents a lock statement e.g. {{{ lock (a); }}} or {{{ lock (a) { f(a); } }}}.
 *
 * If the statement is empty, the mutex remains locked until a corresponding UnlockStatement
 * occurs. Otherwise it's translated into a try/finally statement which unlocks the mutex
 * after the block is finished.
 */
	public class LockStatement : CodeNode, Statement {
		public CodeNode node {
			get { return this; }
		}

		/**
		 * Expression representing the resource to be locked.
		 */
		public Expression resource { get; set; }

		/**
		 * The statement during its execution the resource is locked.
		 */
		public Block body { get; set; }

		public LockStatement(Expression resource, Block body, SourceReference source_reference = null) {
			this.body = body;
			this.source_reference = source_reference;
			this.resource = resource;
		}

		public override void accept(CodeVisitor visitor) {
			resource.accept(visitor);
			if (body != null) {
				body.accept(visitor);
			}
			visitor.visit_lock_statement(this);
		}

		public override bool check(CodeContext context) {
			if (body != null) {
				// if the statement isn't empty, it is converted into a try statement

				var fin_body = new Block(source_reference);
				fin_body.add_statement(new UnlockStatement(resource, source_reference));

				var block = new Block(source_reference);
				block.add_statement(new LockStatement(resource, null, source_reference));
				block.add_statement(new TryStatement(body, fin_body, source_reference));

				var parent_block = (Block)parent_node;
				parent_block.replace_statement(this, block);

				return block.check(context);
			}

			if (is_checked) {
				return !error;
			}

			is_checked = true;

			resource.check(context);

			/* resource must be a member access and denote a Lockable */
			if (!(resource is MemberAccess && resource.symbol_reference is Lockable)) {
				error = true;
				resource.error = true;
				Report.error(resource.source_reference, "Expression is either not a member access or does not denote a lockable member");
				return false;
			}

			/* parent symbol must be the current class */
			if (resource.symbol_reference.parent_symbol != context.analyzer.current_class) {
				error = true;
				resource.error = true;
				Report.error(resource.source_reference, "Only members of the current class are lockable");
			}

			((Lockable)resource.symbol_reference).set_lock_used(true);

			return !error;
		}

		public override void emit(CodeGenerator codegen) {
			resource.emit(codegen);
			codegen.visit_lock_statement(this);
		}
	}
}
