using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.Code;
using Vala.Lang.Parser;
using Vala.Lang.Symbols;
using Vala.Lang.Types;

namespace Vala.Lang.CodeNodes {
	/// <summary>
	/// Represents a catch clause in a try statement in the source code.
	/// </summary>
	public class CatchClause : CodeNode {
		/// <summary>
		/// Specifies the error type.
		/// </summary>
		public DataType error_type {
			get { return _data_type; }
			set {
				_data_type = value;
				if (_data_type != null) {
					_data_type.parent_node = this;
				}
			}
		}

		/// <summary>
		/// Specifies the error variable name.
		/// </summary>
		public string variable_name { get; set; }

		/// <summary>
		/// Specifies the error handler body.
		/// </summary>
		public Block body {
			get { return _body; }
			set {
				_body = value;
				_body.parent_node = this;
			}
		}

		/// <summary>
		/// Specifies the declarator for the generated error variable.
		/// </summary>
		public LocalVariable error_variable {
			get { return _error_variable; }
			set {
				_error_variable = value;
				_error_variable.parent_node = this;
			}
		}

		/// <summary>
		/// Specifies the label used for this catch clause in the C code.
		/// </summary>
		public string clabel_name { get; set; }

		private DataType _data_type;

		private Block _body;
		private LocalVariable _error_variable;

		/// <summary>
		/// Creates a new catch
		/// 
		/// <param name="error_type">error type</param>
		/// <param name="variable_name">error variable name</param>
		/// <param name="body">error handler body</param>
		/// <param name="source_reference">reference to source code</param>
		/// <returns>newly created catch clause</returns>
		/// </summary>
		public CatchClause(DataType error_type, string variable_name, Block body, SourceReference source_reference = null) {
			this.error_type = error_type;
			this.variable_name = variable_name;
			this.body = body;
			this.source_reference = source_reference;
		}

		public override void accept(CodeVisitor visitor) {
			visitor.visit_catch_clause(this);
		}

		public override void accept_children(CodeVisitor visitor) {
			if (error_type != null) {
				error_type.accept(visitor);
			}

			body.accept(visitor);
		}

		public override void replace_type(DataType old_type, DataType new_type) {
			if (error_type == old_type) {
				error_type = new_type;
			}
		}

		public override bool check(CodeContext context) {
			if (is_checked) {
				return !error;
			}

			is_checked = true;

			if (error_type != null) {
				if (!(error_type is ErrorType)) {
					Report.error(source_reference, "clause must catch a valid error type, found `%s' instead".printf(error_type.to_string()));
					error = true;
				}

				if (variable_name != null) {
					error_variable = new LocalVariable(error_type.copy(), variable_name);

					body.scope.add(variable_name, error_variable);
					body.add_local_variable(error_variable);

					error_variable.is_checked = true;
				}
			} else {
				// generic catch clause
				error_type = new ErrorType(null, null, source_reference);
			}

			error_type.check(context);
			body.check(context);

			return !error;
		}

		public override void emit(CodeGenerator codegen) {
			if (error_variable != null) {
				error_variable.active = true;
			}

			codegen.visit_catch_clause(this);
		}

		public override void get_defined_variables(ICollection<Variable> collection) {
			if (error_variable != null) {
				collection.Add(error_variable);
			}
		}
	}
}
