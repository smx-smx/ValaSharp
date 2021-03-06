﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.Code;
using Vala.Lang.CodeNodes;
using Vala.Lang.Expressions;
using Vala.Lang.Literals;
using Vala.Lang.Methods;
using Vala.Lang.Parser;
using Vala.Lang.Symbols;
using Vala.Lang.Types;

namespace Vala.Lang.Statements {
	/// <summary>
	/// Represents a foreach statement in the source code. Foreach statements iterate
	/// over the elements of a collection.
	/// </summary>
	public class ForeachStatement : Block {
		/// <summary>
		/// Specifies the element type.
		/// </summary>
		public DataType type_reference {
			get { return _data_type; }
			set {
				_data_type = value;
				if (_data_type != null) {
					_data_type.parent_node = this;
				}
			}
		}

		/// <summary>
		/// Specifies the element variable name.
		/// </summary>
		public string variable_name { get; set; }

		/// <summary>
		/// Specifies the container.
		/// </summary>
		public Expression collection {
			get {
				return _collection;
			}
			set {
				_collection = value;
				_collection.parent_node = this;
			}
		}

		/// <summary>
		/// Specifies the loop body.
		/// </summary>
		public Block body {
			get {
				return _body;
			}
			set {
				_body = value;
				_body.parent_node = this;
			}
		}

		public bool use_iterator { get; private set; }

		/// <summary>
		/// Specifies the declarator for the generated element variable.
		/// </summary>
		public LocalVariable element_variable { get; set; }

		/// <summary>
		/// Specifies the declarator for the generated collection variable.
		/// </summary>
		public LocalVariable collection_variable { get; set; }

		/// <summary>
		/// Specifies the declarator for the generated iterator variable.
		/// </summary>
		public LocalVariable iterator_variable { get; set; }

		private Expression _collection;
		private Block _body;

		private DataType _data_type;

		/// <summary>
		/// Creates a new foreach statement.
		/// 
		/// <param name="type_reference">element type</param>
		/// <param name="variable_name">element variable name</param>
		/// <param name="collection">container</param>
		/// <param name="body">loop body</param>
		/// <param name="source_reference">reference to source code</param>
		/// <returns>newly created foreach statement</returns>
		/// </summary>
		public ForeachStatement(DataType type_reference, string variable_name, Expression collection, Block body, SourceReference source_reference) : base(source_reference) {
			this.variable_name = variable_name;
			this.collection = collection;
			this.body = body;
			this.type_reference = type_reference;
		}

		public override void accept(CodeVisitor visitor) {
			if (use_iterator) {
				base.accept(visitor);
				return;
			}

			visitor.visit_foreach_statement(this);
		}

		public override void accept_children(CodeVisitor visitor) {
			if (use_iterator) {
				base.accept_children(visitor);
				return;
			}

			collection.accept(visitor);
			visitor.visit_end_full_expression(collection);

			if (type_reference != null) {
				type_reference.accept(visitor);
			}

			body.accept(visitor);
		}

		public override void replace_expression(Expression old_node, Expression new_node) {
			if (collection == old_node) {
				collection = new_node;
			}
		}

		public override void replace_type(DataType old_type, DataType new_type) {
			if (type_reference == old_type) {
				type_reference = new_type;
			}
		}

		public override bool check(CodeContext context) {
			if (is_checked) {
				return !error;
			}

			is_checked = true;

			// analyze collection expression first, used for type inference
			if (!collection.check(context)) {
				// ignore inner error
				error = true;
				return false;
			} else if (collection.value_type == null) {
				Report.error(collection.source_reference, "invalid collection expression");
				error = true;
				return false;
			}

			var collection_type = collection.value_type.copy();
			collection.target_type = collection_type.copy();

			if (collection_type.is_array()) {
				var array_type = (ArrayType)collection_type;

				// can't use inline-allocated array for temporary variable
				array_type.inline_allocated = false;

				return check_without_iterator(context, collection_type, array_type.element_type);
			} else if (collection_type.compatible(context.analyzer.glist_type) || collection_type.compatible(context.analyzer.gslist_type)) {
				if (collection_type.get_type_arguments().Count != 1) {
					error = true;
					Report.error(collection.source_reference, "missing type argument for collection");
					return false;
				}

				return check_without_iterator(context, collection_type, collection_type.get_type_arguments()[0]);
			} else if (collection_type.compatible(context.analyzer.gvaluearray_type)) {
				return check_without_iterator(context, collection_type, context.analyzer.gvalue_type);
			} else {
				return check_with_iterator(context, collection_type);
			}
		}

		bool check_with_index(CodeContext context, DataType collection_type) {
			var get_method = collection_type.get_member("get") as Method;
			if (get_method == null) {
				return false;
			}
			if (get_method.get_parameters().Count != 1) {
				return false;
			}
			var size_property = collection_type.get_member("size") as Property;
			if (size_property == null) {
				return false;
			}

			add_statement(new DeclarationStatement(new LocalVariable(null, "_%s_list".printf(variable_name), collection, source_reference), source_reference));
			add_statement(new DeclarationStatement(new LocalVariable(null, "_%s_size".printf(variable_name), new MemberAccess(MemberAccess.simple("_%s_list".printf(variable_name), source_reference), "size", source_reference), source_reference), source_reference));
			add_statement(new DeclarationStatement(new LocalVariable(null, "_%s_index".printf(variable_name), new UnaryExpression(UnaryOperator.MINUS, new IntegerLiteral("1", source_reference), source_reference), source_reference), source_reference));
			var next = new UnaryExpression(UnaryOperator.INCREMENT, MemberAccess.simple("_%s_index".printf(variable_name), source_reference), source_reference);
			var conditional = new BinaryExpression(BinaryOperator.LESS_THAN, next, MemberAccess.simple("_%s_size".printf(variable_name), source_reference), source_reference);
			var loop = new WhileStatement(conditional, body, source_reference);
			add_statement(loop);

			var get_call = new MethodCall(new MemberAccess(MemberAccess.simple("_%s_list".printf(variable_name), source_reference), "get", source_reference), source_reference);
			get_call.add_argument(MemberAccess.simple("_%s_index".printf(variable_name), source_reference));
			body.insert_statement(0, new DeclarationStatement(new LocalVariable(type_reference, variable_name, get_call, source_reference), source_reference));

			is_checked = false;
			return base.check(context);
		}

		bool check_with_iterator(CodeContext context, DataType collection_type) {
			use_iterator = true;

			if (check_with_index(context, collection_type)) {
				return true;
			}

			var iterator_method = collection_type.get_member("iterator") as Method;
			if (iterator_method == null) {
				Report.error(collection.source_reference, "`%s' does not have an `iterator' method".printf(collection_type.ToString()));
				error = true;
				return false;
			}
			if (iterator_method.get_parameters().Count != 0) {
				Report.error(collection.source_reference, "`%s' must not have any parameters".printf(iterator_method.get_full_name()));
				error = true;
				return false;
			}
			var iterator_type = iterator_method.return_type.get_actual_type(collection_type, null, this);
			if (iterator_type is VoidType) {
				Report.error(collection.source_reference, "`%s' must return an iterator".printf(iterator_method.get_full_name()));
				error = true;
				return false;
			}

			var iterator_call = new MethodCall(new MemberAccess(collection, "iterator", source_reference), source_reference);
			add_statement(new DeclarationStatement(new LocalVariable(iterator_type, "_%s_it".printf(variable_name), iterator_call, source_reference), source_reference));

			var next_value_method = iterator_type.get_member("next_value") as Method;
			var next_method = iterator_type.get_member("next") as Method;
			if (next_value_method != null) {
				if (next_value_method.get_parameters().Count != 0) {
					Report.error(collection.source_reference, "`%s' must not have any parameters".printf(next_value_method.get_full_name()));
					error = true;
					return false;
				}
				var element_type = next_value_method.return_type.get_actual_type(iterator_type, null, this);
				if (!element_type.nullable) {
					Report.error(collection.source_reference, "return type of `%s' must be nullable".printf(next_value_method.get_full_name()));
					error = true;
					return false;
				}

				if (!analyze_element_type(element_type)) {
					return false;
				}

				add_statement(new DeclarationStatement(new LocalVariable(type_reference, variable_name, null, source_reference), source_reference));

				var next_value_call = new MethodCall(new MemberAccess(MemberAccess.simple("_%s_it".printf(variable_name), source_reference), "next_value", source_reference), source_reference);
				var assignment = new Assignment(new MemberAccess(null, variable_name, source_reference), next_value_call, AssignmentOperator.SIMPLE, source_reference);
				var conditional = new BinaryExpression(BinaryOperator.INEQUALITY, assignment, new NullLiteral(source_reference), source_reference);
				var loop = new WhileStatement(conditional, body, source_reference);
				add_statement(loop);
			} else if (next_method != null) {
				if (next_method.get_parameters().Count != 0) {
					Report.error(collection.source_reference, "`%s' must not have any parameters".printf(next_method.get_full_name()));
					error = true;
					return false;
				}
				if (!next_method.return_type.compatible(context.analyzer.bool_type)) {
					Report.error(collection.source_reference, "`%s' must return a boolean value".printf(next_method.get_full_name()));
					error = true;
					return false;
				}
				var get_method = iterator_type.get_member("get") as Method;
				if (get_method == null) {
					Report.error(collection.source_reference, "`%s' does not have a `get' method".printf(iterator_type.ToString()));
					error = true;
					return false;
				}
				if (get_method.get_parameters().Count != 0) {
					Report.error(collection.source_reference, "`%s' must not have any parameters".printf(get_method.get_full_name()));
					error = true;
					return false;
				}
				var element_type = get_method.return_type.get_actual_type(iterator_type, null, this);
				if (element_type is VoidType) {
					Report.error(collection.source_reference, "`%s' must return an element".printf(get_method.get_full_name()));
					error = true;
					return false;
				}

				if (!analyze_element_type(element_type)) {
					return false;
				}

				var next_call = new MethodCall(new MemberAccess(MemberAccess.simple("_%s_it".printf(variable_name), source_reference), "next", source_reference), source_reference);
				var loop = new WhileStatement(next_call, body, source_reference);
				add_statement(loop);

				var get_call = new MethodCall(new MemberAccess(MemberAccess.simple("_%s_it".printf(variable_name), source_reference), "get", source_reference), source_reference);
				body.insert_statement(0, new DeclarationStatement(new LocalVariable(type_reference, variable_name, get_call, source_reference), source_reference));
			} else {
				Report.error(collection.source_reference, "`%s' does not have a `next_value' or `next' method".printf(iterator_type.ToString()));
				error = true;
				return false;
			}

			is_checked = false;
			return base.check(context);
		}

		bool analyze_element_type(DataType element_type) {
			// analyze element type
			if (type_reference == null) {
				// var type
				type_reference = element_type.copy();
			} else if (!element_type.compatible(type_reference)) {
				error = true;
				Report.error(source_reference, "Foreach: Cannot convert from `%s' to `%s'".printf(element_type.ToString(), type_reference.ToString()));
				return false;
			} else if (element_type.is_disposable() && element_type.value_owned && !type_reference.value_owned) {
				error = true;
				Report.error(source_reference, "Foreach: Invalid assignment from owned expression to unowned variable");
				return false;
			}

			return true;
		}

		bool check_without_iterator(CodeContext context, DataType collection_type, DataType element_type) {
			// analyze element type
			if (type_reference == null) {
				// var type
				type_reference = element_type.copy();
			} else if (!element_type.compatible(type_reference)) {
				error = true;
				Report.error(source_reference, "Foreach: Cannot convert from `%s' to `%s'".printf(element_type.ToString(), type_reference.ToString()));
				return false;
			}

			element_variable = new LocalVariable(type_reference, variable_name, null, source_reference);

			body.scope.add(variable_name, element_variable);

			body.add_local_variable(element_variable);
			element_variable.active = true;
			element_variable.is_checked = true;

			// analyze body
			owner = context.analyzer.current_symbol.scope;
			context.analyzer.current_symbol = this;

			// call add_local_variable to check for shadowed variable
			add_local_variable(element_variable);
			remove_local_variable(element_variable);

			body.check(context);

			foreach (LocalVariable local in get_local_variables()) {
				local.active = false;
			}

			context.analyzer.current_symbol = context.analyzer.current_symbol.parent_symbol;

			collection_variable = new LocalVariable(collection_type.copy(), "%s_collection".printf(variable_name));

			add_local_variable(collection_variable);
			collection_variable.active = true;

			add_error_types(collection.get_error_types());
			add_error_types(body.get_error_types());

			return !error;
		}

		public override void emit(CodeGenerator codegen) {
			if (use_iterator) {
				base.emit(codegen);
				return;
			}

			collection.emit(codegen);
			codegen.visit_end_full_expression(collection);

			element_variable.active = true;
			collection_variable.active = true;
			if (iterator_variable != null) {
				iterator_variable.active = true;
			}

			codegen.visit_foreach_statement(this);
		}

		public override void get_defined_variables(ICollection<Variable> collection) {
			collection.Add(element_variable);
		}
	}
}
