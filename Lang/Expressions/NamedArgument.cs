using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.Code;
using Vala.Lang.CodeNodes;
using Vala.Lang.Parser;
using Vala.Lang.Symbols;

namespace Vala.Lang.Expressions
{
	public class NamedArgument : Expression
	{
		public string name { get; set; }

		public Expression inner {
			get {
				return _inner;
			}
			set {
				_inner = value;
				_inner.parent_node = this;
			}
		}

		private Expression _inner;

		public NamedArgument(string name, Expression inner, SourceReference source_reference = null) {
			this.name = name;
			this.inner = inner;
			this.source_reference = source_reference;
		}

		public override void accept(CodeVisitor visitor) {
			visitor.visit_named_argument(this);

			visitor.visit_expression(this);
		}

		public override void accept_children(CodeVisitor visitor) {
			inner.accept(visitor);
		}

		public override void replace_expression(Expression old_node, Expression new_node) {
			if (inner == old_node) {
				inner = new_node;
			}
		}

		public override bool is_pure() {
			return inner.is_pure();
		}

		public override bool check(CodeContext context) {
			if (is_checked) {
				return !error;
			}

			is_checked = true;

			inner.target_type = target_type;

			if (!inner.check(context)) {
				error = true;
				return false;
			}

			inner.target_type = inner.value_type;
			value_type = inner.value_type;

			return !error;
		}

		public override void emit(CodeGenerator codegen) {
			inner.emit(codegen);

			codegen.visit_named_argument(this);

			codegen.visit_expression(this);
		}

		public override void get_defined_variables(ICollection<Variable> collection) {
			inner.get_defined_variables(collection);
		}

		public override void get_used_variables(ICollection<Variable> collection) {
			inner.get_used_variables(collection);
		}
	}
}
