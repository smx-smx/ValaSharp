using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.Code;
using Vala.Lang.CodeNodes;
using Vala.Lang.Expressions;
using Vala.Lang.Parser;

namespace Vala.Lang.Statements
{
	/**
 * Represents an unlock statement e.g. {{{ unlock (a); }}}.
 */
	public class UnlockStatement : CodeNode, Statement
	{
		public CodeNode node {
			get { return this; }
		}

		/**
		 * Expression representing the resource to be unlocked.
		 */
		public Expression resource { get; set; }

		public UnlockStatement(Expression resource, SourceReference source_reference = null){
			this.resource = resource;
			this.source_reference = source_reference;
		}

		public override void accept(CodeVisitor visitor) {
			resource.accept(visitor);
			visitor.visit_unlock_statement(this);
		}

		public override bool check(CodeContext context) {
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
			codegen.visit_unlock_statement(this);
		}
	}
}
