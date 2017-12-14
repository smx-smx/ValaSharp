using Vala;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.Symbols;

namespace Vala.Lang {
	public class Scope {
		private WeakReference<Symbol> owner_weak = new WeakReference<Symbol>(null);
		private WeakReference<Scope> parent_scope_weak = new WeakReference<Scope>(null);

		/// <summary>
		/// The symbol that owns this scope.
		/// </summary>
		public Symbol owner {
			get {
				return owner_weak.GetTarget();
			}
			set {
				owner_weak.SetTarget(value);
			}
		}

		/// <summary>
		/// The parent of this scope.
		/// </summary>
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

		/// <summary>
		/// Creates a new scope.
		/// 
		/// <returns>newly created scope</returns>
		/// </summary>
		public Scope(Symbol owner = null) {
			this.owner = owner;
		}

		/// <summary>
		/// Adds the specified symbol with the specified name to the symbol table
		/// of this scope.
		/// 
		/// <param name="name">name for the specified symbol</param>
		/// <param name="sym">a symbol</param>
		/// </summary>
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

		/// <summary>
		/// Returns the symbol stored in the symbol table with the specified
		/// name.
		/// 
		/// <param name="name">name of the symbol to be returned</param>
		/// <returns>found symbol or null</returns>
		/// </summary>
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

		/// <summary>
		/// Returns whether the specified scope is an ancestor of this scope.
		/// 
		/// <param name="scope">a scope or null for the root scope</param>
		/// <returns>true if this scope is a subscope of the specified</returns>
		/// scope, false otherwise
		/// </summary>
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
