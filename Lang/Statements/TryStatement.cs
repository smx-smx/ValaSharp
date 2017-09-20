﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.Code;
using Vala.Lang.CodeNodes;
using Vala.Lang.Parser;
using Vala.Lang.Types;

namespace Vala.Lang.Statements
{
	/**
 * Represents a try statement in the source code.
 */
	public class TryStatement : Statement
	{
		/**
		 * Specifies the body of the try statement.
		 */
		public Block body {
			get { return _body; }
			set {
				_body = value;
				_body.parent_node = this;
			}
		}

		/**
		 * Specifies the body of the optional finally clause.
		 */
		public Block finally_body {
			get { return _finally_body; }
			set {
				_finally_body = value;
				if (_finally_body != null)
					_finally_body.parent_node = this;
			}
		}

		public bool after_try_block_reachable { get; set; } = true;

		private Block _body;
		private Block _finally_body;
		private List<CatchClause> catch_clauses = new List<CatchClause>();

		/**
		 * Creates a new try statement.
		 *
		 * @param body             body of the try statement
		 * @param finally_body     body of the optional finally clause
		 * @param source_reference reference to source code
		 * @return                 newly created try statement
		 */
		public TryStatement(Block body, Block finally_body, SourceReference source_reference = null) : base(source_reference){
			this.body = body;
			this.finally_body = finally_body;
		}

		/**
		 * Appends the specified clause to the list of catch clauses.
		 *
		 * @param clause a catch clause
		 */
		public void add_catch_clause(CatchClause clause) {
			clause.parent_node = this;
			catch_clauses.Add(clause);
		}

		/**
		 * Returns a copy of the list of catch clauses.
		 *
		 * @return list of catch clauses
		 */
		public List<CatchClause> get_catch_clauses() {
			return catch_clauses;
		}

		public override void accept(CodeVisitor visitor) {
			visitor.visit_try_statement(this);
		}

		public override void accept_children(CodeVisitor visitor) {
			body.accept(visitor);

			foreach (CatchClause clause in catch_clauses) {
				clause.accept(visitor);
			}

			if (finally_body != null) {
				finally_body.accept(visitor);
			}
		}

		public override bool check(CodeContext context) {
			if (is_checked) {
				return !error;
			}

			is_checked = true;

			body.check(context);

			var error_types = new List<DataType>();
			foreach (DataType body_error_type in body.get_error_types()) {
				error_types.Add(body_error_type);
			}

			var handled_error_types = new List<DataType>();
			foreach (CatchClause clause in catch_clauses) {
				foreach (DataType body_error_type in error_types) {
					if (clause.error_type == null || body_error_type.compatible(clause.error_type)) {
						handled_error_types.Add(body_error_type);
					}
				}
				foreach (DataType handled_error_type in handled_error_types) {
					error_types.Remove(handled_error_type);
				}
				handled_error_types.Clear();

				clause.check(context);
				foreach (DataType body_error_type in clause.body.get_error_types()) {
					error_types.Add(body_error_type);
				}
			}

			if (finally_body != null) {
				finally_body.check(context);
				foreach (DataType body_error_type in finally_body.get_error_types()) {
					error_types.Add(body_error_type);
				}
			}

			add_error_types(error_types);

			return !error;
		}

		public override void emit(CodeGenerator codegen) {
			codegen.visit_try_statement(this);
		}
	}
}
