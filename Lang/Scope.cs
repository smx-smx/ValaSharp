using Vala;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.Symbols;

namespace Vala.Lang
{
	public class Scope
	{
		private WeakReference<Symbol> owner_weak = new WeakReference<Symbol>(null);
		private WeakReference<Scope> parent_scope_weak = new WeakReference<Scope>(null);

		/**
	 * The symbol that owns this scope.
	 */
		public Symbol owner {
			get {
				return owner_weak.GetTarget();
			}
			set {
				owner_weak.SetTarget(value);
			}
		}

		/**
		 * The parent of this scope.
		 */
		public Scope parent_scope {
			get {
				return parent_scope_weak.GetTarget();
			}
			set {
				parent_scope_weak.SetTarget(value);
			}
		}

		private Dictionary<string, Symbol> symbol_table;
		private List<Symbol> anonymous_members;

		/**
		 * Creates a new scope.
		 *
		 * @return newly created scope
		 */
		public Scope(Symbol owner = null) {
			this.owner = owner;
		}

		/**
		 * Adds the specified symbol with the specified name to the symbol table
		 * of this scope.
		 *
		 * @param name name for the specified symbol
		 * @param sym  a symbol
		 */
		public void add(string name, Symbol sym) {
			if (name != null) {
				if (symbol_table == null) {
					symbol_table = new Dictionary<string, Symbol>();
				} else if (lookup(name) != null) {
					owner.error = true;
					if (owner.name == null && owner.parent_symbol == null) {
						Report.error(sym.source_reference, "The root namespace already contains a definition for `%s'".printf(name));
					} else {
						Report.error(sym.source_reference, "`%s' already contains a definition for `%s'".printf(owner.get_full_name(), name));
					}
					Report.notice(lookup(name).source_reference, "previous definition of `%s' was here".printf(name));
					return;
				}

				symbol_table[(string)name] = sym;
			} else {
				if (anonymous_members == null) {
					anonymous_members = new List<Symbol>();
				}

				anonymous_members.Add(sym);
			}
			sym.owner = this;
		}

		public void remove(string name) {
			symbol_table.Remove(name);
		}

		/**
		 * Returns the symbol stored in the symbol table with the specified
		 * name.
		 *
		 * @param name name of the symbol to be returned
		 * @return     found symbol or null
		 */
		public Symbol lookup(string name) {
			if (symbol_table == null) {
				return null;
			}
			Symbol sym;
			symbol_table.TryGetValue(name, out sym);
			if (sym != null && !sym.active) {
				sym = null;
			}
			return sym;
		}

		/**
		 * Returns whether the specified scope is an ancestor of this scope.
		 *
		 * @param scope a scope or null for the root scope
		 * @return      true if this scope is a subscope of the specified
		 *              scope, false otherwise
		 */
		public bool is_subscope_of(Scope scope) {
			if (scope == this) {
				return true;
			}

			// null scope is the root scope
			if (scope == null) {
				return true;
			}

			if (parent_scope != null) {
				return parent_scope.is_subscope_of(scope);
			}

			return false;
		}

		public Dictionary<string, Symbol> get_symbol_table() {
			return symbol_table;
		}
	}
}
