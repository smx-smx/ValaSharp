using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.Code;
using Vala.Lang.CodeNodes;
using Vala.Lang.Parser;
using Vala.Lang.Symbols;
using Vala.Lang.Types;

namespace Vala.Lang.Statements
{
	/**
	 * Represents a local variable or constant declaration statement in the source code.
	 */
	public class DeclarationStatement : Statement
	{
		/**
		 * The local variable or constant declaration.
		 */
		public Symbol declaration {
			get {
				return _declaration;
			}
			set {
				_declaration = value;
				if (_declaration != null) {
					_declaration.parent_node = this;
				}
			}
		}

		Symbol _declaration;

		/**
		 * Creates a new declaration statement.
		 *
		 * @param declaration       local variable declaration
		 * @param source_reference  reference to source code
		 * @return                  newly created declaration statement
		 */
		public DeclarationStatement(Symbol declaration, SourceReference source_reference) : base(source_reference) {
			this.declaration = declaration;
		}

		public override void accept(CodeVisitor visitor) {
			visitor.visit_declaration_statement(this);
		}

		public override void accept_children(CodeVisitor visitor) {
			declaration.accept(visitor);
		}

		public override bool check(CodeContext context) {
			if (is_checked) {
				return !error;
			}

			is_checked = true;

			declaration.check(context);

			var local = declaration as LocalVariable;
			if (local != null && local.initializer != null) {
				foreach (DataType error_type in local.initializer.get_error_types()) {
					// ensure we can trace back which expression may throw errors of this type
					var initializer_error_type = error_type.copy();
					initializer_error_type.source_reference = local.initializer.source_reference;

					add_error_type(initializer_error_type);
				}
			}

			return !error;
		}

		public override void emit(CodeGenerator codegen) {
			codegen.visit_declaration_statement(this);
		}

		public override void get_defined_variables(ICollection<Variable> collection) {
			var local = declaration as LocalVariable;
			if (local != null) {
				var array_type = local.variable_type as ArrayType;
				if (local.initializer != null) {
					local.initializer.get_defined_variables(collection);
					collection.Add(local);
				} else if (array_type != null && array_type.fixed_length) {
					collection.Add(local);
				}
			}
		}

		public override void get_used_variables(ICollection<Variable> collection) {
			var local = declaration as LocalVariable;
			if (local != null && local.initializer != null) {
				local.initializer.get_used_variables(collection);
			}
		}
	}
}
