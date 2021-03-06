﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.CodeNodes;
using Vala.Lang.Expressions;
using Vala.Lang.Parser;
using Vala.Lang.Statements;
using Vala.Lang.Symbols;
using Vala.Lang.Types;
using Vala.Lang.TypeSymbols;

namespace Vala.Lang.Methods {
	public class CreationMethod : Method {
		/// <summary>
		/// Specifies the name of the type this creation method belongs to.
		/// </summary>
		public string class_name { get; set; }

		/// <summary>
		/// Specifies whether this constructor chains up to a base
		/// constructor or a different constructor of the same class.
		/// </summary>
		public bool chain_up { get; set; }

		/// <summary>
		/// Creates a new method.
		/// 
		/// <param name="name">method name</param>
		/// <param name="source_reference">reference to source code</param>
		/// <returns>newly created method</returns>
		/// </summary>
		public CreationMethod(string class_name, string name, SourceReference source_reference = null, Comment comment = null)
			: base(name, new VoidType(), source_reference, comment) {
			this.class_name = class_name;
		}

		public override void accept(CodeVisitor visitor) {
			visitor.visit_creation_method(this);
		}

		public override void accept_children(CodeVisitor visitor) {
			foreach (Parameter param in get_parameters()) {
				param.accept(visitor);
			}

			foreach (DataType error_type in get_error_types().ToList()) {
				error_type.accept(visitor);
			}

			foreach (Expression precondition in get_preconditions()) {
				precondition.accept(visitor);
			}

			foreach (Expression postcondition in get_postconditions()) {
				postcondition.accept(visitor);
			}

			if (body != null) {
				body.accept(visitor);
			}
		}

		public override bool check(CodeContext context) {
			if (is_checked) {
				return !error;
			}

			is_checked = true;

			if (class_name != null && class_name != parent_symbol.name) {
				// class_name is null for constructors generated by GIdlParser
				Report.error(source_reference, "missing return type in method `%s.%s´".printf(context.analyzer.current_symbol.get_full_name(), class_name));
				error = true;
				return false;
			}

			var old_source_file = context.analyzer.current_source_file;
			var old_symbol = context.analyzer.current_symbol;

			if (source_reference != null) {
				context.analyzer.current_source_file = source_reference.file;
			}
			context.analyzer.current_symbol = this;

			foreach (Parameter param in get_parameters()) {
				param.check(context);
			}

			foreach (DataType error_type in get_error_types()) {
				error_type.check(context);
			}

			foreach (Expression precondition in get_preconditions()) {
				precondition.check(context);
			}

			foreach (Expression postcondition in get_postconditions()) {
				postcondition.check(context);
			}

			if (body != null) {
				body.check(context);

				var cl = parent_symbol as Class;

				// ensure we chain up to base constructor
				if (!chain_up && cl != null && cl.base_class != null) {
					if (cl.base_class.default_construction_method != null
						&& !cl.base_class.default_construction_method.has_construct_function) {
						// directly chain up to Object
						var old_insert_block = context.analyzer.insert_block;
						context.analyzer.current_symbol = body;
						context.analyzer.insert_block = body;

						var stmt = new ExpressionStatement(new MethodCall(new MemberAccess(MemberAccess.simple("GLib", source_reference), "Object", source_reference), source_reference), source_reference);
						body.insert_statement(0, stmt);
						stmt.check(context);

						context.analyzer.current_symbol = this;
						context.analyzer.insert_block = old_insert_block;
					} else if (cl.base_class.default_construction_method == null
						|| cl.base_class.default_construction_method.access == SymbolAccessibility.PRIVATE) {
						Report.error(source_reference, "unable to chain up to private base constructor");
					} else if (cl.base_class.default_construction_method.get_required_arguments() > 0) {
						Report.error(source_reference, "unable to chain up to base constructor requiring arguments");
					} else {
						var old_insert_block = context.analyzer.insert_block;
						context.analyzer.current_symbol = body;
						context.analyzer.insert_block = body;

						var stmt = new ExpressionStatement(new MethodCall(new BaseAccess(source_reference), source_reference), source_reference);
						body.insert_statement(0, stmt);
						stmt.check(context);

						context.analyzer.current_symbol = this;
						context.analyzer.insert_block = old_insert_block;
					}
				}
			}

			context.analyzer.current_source_file = old_source_file;
			context.analyzer.current_symbol = old_symbol;

			if (is_abstract || is_virtual || overrides) {
				Report.error(source_reference, "The creation method `%s' cannot be marked as override, virtual, or abstract".printf(get_full_name()));
				return false;
			}

			// check that all errors that can be thrown in the method body are declared
			if (body != null) {
				foreach (DataType body_error_type in body.get_error_types()) {
					bool can_propagate_error = false;
					foreach (DataType method_error_type in get_error_types()) {
						if (body_error_type.compatible(method_error_type)) {
							can_propagate_error = true;
						}
					}
					if (!can_propagate_error && !((ErrorType)body_error_type).dynamic_error) {
						Report.warning(body_error_type.source_reference, "unhandled error `%s'".printf(body_error_type.ToString()));
					}
				}
			}

			return !error;
		}
	}
}
