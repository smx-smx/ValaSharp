using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.Code;
using Vala.Lang.Expressions;
using Vala.Lang.Parser;
using Vala.Lang.Symbols;

namespace Vala.Lang.CodeNodes {
	/**
	 * Represents a member initializer, i.e. an element of an object initializer, in
	 * the source code.
	 */
	public class MemberInitializer : CodeNode {
		/**
		 * Member name.
		 */
		public string name { get; set; }

		/**
		 * Initializer expression.
		 */
		public Expression initializer {
			get { return _initializer; }
			set {
				_initializer = value;
				_initializer.parent_node = this;
			}
		}

		private WeakReference<Symbol> symbol_reference_weak = new WeakReference<Symbol>(null);

		/**
		 * The symbol this expression refers to.
		 */
		public Symbol symbol_reference {
			get {
				return symbol_reference_weak.GetTarget();
			}
			set {
				symbol_reference_weak.SetTarget(value);
			}
		}

		Expression _initializer;

		/**
		 * Creates a new member initializer.
		 *
		 * @param name             member name
		 * @param initializer      initializer expression
		 * @param source_reference reference to source code
		 * @return                 newly created member initializer
		 */
		public MemberInitializer(string name, Expression initializer, SourceReference source_reference = null) {
			this.initializer = initializer;
			this.source_reference = source_reference;
			this.name = name;
		}

		public override void accept(CodeVisitor visitor) {
			initializer.accept(visitor);
		}

		public override bool check(CodeContext context) {
			return initializer.check(context);
		}

		public override void emit(CodeGenerator codegen) {
			initializer.emit(codegen);
		}

		public override void get_used_variables(ICollection<Variable> collection) {
			initializer.get_used_variables(collection);
		}

		public override void replace_expression(Expression old_node, Expression new_node) {
			if (initializer == old_node) {
				initializer = new_node;
			}
		}
	}
}
