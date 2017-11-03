using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.CodeNodes;
using Vala.Lang.Statements;
using Vala.Lang.Symbols;
using Vala.Lang.Types;

namespace Vala.Lang.Expressions
{
	public abstract class Expression : CodeNode
	{
		/**
	 * The static type of the value of this expression.
	 * 
	 * The semantic analyzer computes this value.
	 */
		public DataType value_type { get; set; }

		public DataType formal_value_type { get; set; }

		/*
		 * The static type this expression is expected to have.
		 *
		 * The semantic analyzer computes this value, lambda expressions use it.
		 */
		public DataType target_type { get; set; }

		public DataType formal_target_type { get; set; }

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

		/**
		 * Specifies that this expression is used as lvalue, i.e. the
		 * left hand side of an assignment.
		 */
		public bool lvalue { get; set; }

		public TargetValue target_value { get; set; }

		/**
		 * Returns whether this expression is constant, i.e. whether this
		 * expression only consists of literals and other constants.
		 */
		public virtual bool is_constant() {
			return false;
		}

		/**
		 * Returns whether this expression is pure, i.e. whether this expression
		 * is free of side-effects.
		 */
		public abstract bool is_pure();

		/**
		 * Returns whether this expression is guaranteed to be non-null.
		 */
		public virtual bool is_non_null() {
			return false;
		}

		/**
		 * Check whether symbol_references in this expression are at least
		 * as accessible as the specified symbol.
		 */
		public virtual bool is_accessible(Symbol sym) {
			return true;
		}

		public Statement parent_statement {
			get {
				var expr = parent_node as Expression;
				var stmt = parent_node as Statement;
				var local = parent_node as LocalVariable;
				var initializer = parent_node as MemberInitializer;
				if (stmt != null) {
					return (Statement)parent_node;
				} else if (expr != null) {
					return expr.parent_statement;
				} else if (local != null) {
					return (Statement)local.parent_node;
				} else if (initializer != null) {
					return ((Expression)initializer.parent_node).parent_statement;
				} else {
					return null;
				}
			}
		}

		public void insert_statement(Block block, Statement stmt) {
			block.insert_before(parent_statement, stmt);
		}
	}
}
