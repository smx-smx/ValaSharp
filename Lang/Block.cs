﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.Code;
using Vala.Lang.CodeNodes;
using Vala.Lang.Methods;
using Vala.Lang.Parser;
using Vala.Lang.Statements;
using Vala.Lang.Symbols;

namespace Vala.Lang {
	public class Block : Symbol, Statement {
		public CodeNode node {
			get { return this; }
		}

		/// <summary>
		/// Specifies whether this block contains a jump statement. This
		/// information can be used to remove unreachable block cleanup code.
		/// </summary>
		public bool contains_jump_statement { get; set; }

		public bool captured { get; set; }

		private List<Statement> statement_list = new List<Statement>();
		private List<LocalVariable> local_variables = new List<LocalVariable>();
		private List<Constant> local_constants = new List<Constant>();

		/// <summary>
		/// Creates a new block.
		/// 
		/// <param name="source_reference">reference to source code</param>
		/// </summary>
		public Block(SourceReference source_reference) : base(null, source_reference) {
		}

		/// <summary>
		/// Append a statement to this block.
		/// 
		/// <param name="stmt">a statement</param>
		/// </summary>
		public void add_statement(Statement stmt) {
			stmt.node.parent_node = this;
			statement_list.Add(stmt);
		}

		public void insert_statement(int index, Statement stmt) {
			stmt.node.parent_node = this;
			statement_list.Insert(index, stmt);
		}

		/// <summary>
		/// Returns a copy of the list of statements.
		/// 
		/// <returns>statement list</returns>
		/// </summary>
		public List<Statement> get_statements() {
			var list = new List<Statement>();
			foreach (Statement stmt in statement_list) {
				var stmt_list = stmt as StatementList;
				if (stmt_list != null) {
					for (int i = 0; i < stmt_list.length; i++) {
						list.Add(stmt_list.get(i));
					}
				} else {
					list.Add(stmt);
				}
			}
			return list;
		}

		/// <summary>
		/// Add a local variable to this block.
		/// 
		/// <param name="local">a variable declarator</param>
		/// </summary>
		public void add_local_variable(LocalVariable local) {
			var parent_block = parent_symbol;
			while (parent_block is Block || parent_block is Method || parent_block is PropertyAccessor) {
				if (parent_block.scope.lookup(local.name) != null) {
					Report.error(local.source_reference, "Local variable `%s' conflicts with a local variable or constant declared in a parent scope".printf(local.name));
					break;
				}
				parent_block = parent_block.parent_symbol;
			}
			local_variables.Add(local);
		}

		public void remove_local_variable(LocalVariable local) {
			local_variables.Remove(local);
		}

		/// <summary>
		/// Returns a copy of the list of local variables.
		/// 
		/// <returns>variable declarator list</returns>
		/// </summary>
		public List<LocalVariable> get_local_variables() {
			return local_variables;
		}

		public void add_local_constant(Constant constant) {
			var parent_block = parent_symbol;
			while (parent_block is Block || parent_block is Method || parent_block is PropertyAccessor) {
				if (parent_block.scope.lookup(constant.name) != null) {
					Report.error(constant.source_reference, "Local constant `%s' conflicts with a local variable or constant declared in a parent scope".printf(constant.name));
					break;
				}
				parent_block = parent_block.parent_symbol;
			}
			local_constants.Add(constant);
			scope.add(constant.name, constant);
		}

		public override void accept(CodeVisitor visitor) {
			visitor.visit_block(this);
		}

		public override void accept_children(CodeVisitor visitor) {
			foreach (Statement stmt in statement_list) {
				stmt.node.accept(visitor);
			}
		}

		public override bool check(CodeContext context) {
			if (is_checked) {
				return !error;
			}

			is_checked = true;

			owner = context.analyzer.current_symbol.scope;

			var old_symbol = context.analyzer.current_symbol;
			var old_insert_block = context.analyzer.insert_block;
			context.analyzer.current_symbol = this;
			context.analyzer.insert_block = this;

			for (int i = 0; i < statement_list.Count; i++) {
				statement_list[i].node.check(context);
			}

			foreach (LocalVariable local in get_local_variables()) {
				local.active = false;
			}

			foreach (Constant constant in local_constants) {
				constant.active = false;
			}

			// use get_statements () instead of statement_list to not miss errors within StatementList objects
			foreach (Statement stmt in get_statements()) {
				add_error_types(stmt.node.get_error_types());
			}

			context.analyzer.current_symbol = old_symbol;
			context.analyzer.insert_block = old_insert_block;

			return !error;
		}

		public override void emit(CodeGenerator codegen) {
			codegen.visit_block(this);
		}

		public void insert_before(Statement stmt, Statement new_stmt) {
			for (int i = 0; i < statement_list.Count; i++) {
				var stmt_list = statement_list[i] as StatementList;
				if (stmt_list != null) {
					for (int j = 0; j < stmt_list.length; j++) {
						if (stmt_list.get(j) == stmt) {
							stmt_list.insert(j, new_stmt);
							new_stmt.node.parent_node = this;
							break;
						}
					}
				} else if (statement_list[i] == stmt) {
					stmt_list = new StatementList(source_reference);
					stmt_list.add(new_stmt);
					stmt_list.add(stmt);
					statement_list[i] = stmt_list;
					new_stmt.node.parent_node = this;
				}
			}
		}

		public void replace_statement(Statement old_stmt, Statement new_stmt) {
			for (int i = 0; i < statement_list.Count; i++) {
				var stmt_list = statement_list[i] as StatementList;
				if (stmt_list != null) {
					for (int j = 0; j < stmt_list.length; j++) {
						if (stmt_list.get(j) == old_stmt) {
							stmt_list.set(j, new_stmt);
							new_stmt.node.parent_node = this;
							break;
						}
					}
				} else if (statement_list[i] == old_stmt) {
					statement_list[i] = new_stmt;
					new_stmt.node.parent_node = this;
					break;
				}
			}
		}
	}
}
