﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.Code;
using Vala.Lang.CodeNodes;
using Vala.Lang.Literals;
using Vala.Lang.Methods;
using Vala.Lang.Parser;
using Vala.Lang.Statements;
using Vala.Lang.Symbols;
using Vala.Lang.Types;
using Vala.Lang.TypeSymbols;

namespace Vala.Lang.Expressions {
	/// <summary>
	/// Represents an invocation expression in the source code.
	/// </summary>
	public class MethodCall : Expression {
		/// <summary>
		/// The method to call.
		/// </summary>
		public Expression call {
			get { return _call; }
			set {
				_call = value;
				_call.parent_node = this;
			}
		}

		public bool is_yield_expression { get; set; }

		public bool is_assert { get; private set; }

		/// <summary>
		/// Whether this chain up uses the constructv function with va_list.
		/// </summary>
		public bool is_constructv_chainup { get; private set; }

		public bool is_chainup { get; private set; }

		private Expression _call;

		private List<Expression> argument_list = new List<Expression>();

		/// <summary>
		/// Creates a new invocation expression.
		/// 
		/// <param name="call">method to call</param>
		/// <param name="source_reference">reference to source code</param>
		/// <returns>newly created invocation expression</returns>
		/// </summary>
		public MethodCall(Expression call, SourceReference source_reference = null) {
			this.source_reference = source_reference;
			this.call = call;
		}

		/// <summary>
		/// Appends the specified expression to the list of arguments.
		/// 
		/// <param name="arg">an argument</param>
		/// </summary>
		public void add_argument(Expression arg) {
			argument_list.Add(arg);
			arg.parent_node = this;
		}

		/// <summary>
		/// Returns a copy of the argument list.
		/// 
		/// <returns>argument list</returns>
		/// </summary>
		public List<Expression> get_argument_list() {
			return argument_list;
		}

		public override void accept(CodeVisitor visitor) {
			visitor.visit_method_call(this);

			visitor.visit_expression(this);
		}

		public override void accept_children(CodeVisitor visitor) {
			call.accept(visitor);

			foreach (Expression expr in argument_list) {
				expr.accept(visitor);
			}
		}

		public override void replace_expression(Expression old_node, Expression new_node) {
			if (call == old_node) {
				call = new_node;
			}

			int index = argument_list.IndexOf(old_node);
			if (index >= 0 && new_node.parent_node == null) {
				argument_list[index] = new_node;
				new_node.parent_node = this;
			}
		}

		public override bool is_constant() {
			var method_type = call.value_type as MethodType;

			if (method_type != null) {
				// N_ and NC_ do not have any effect on the C code,
				// they are only interpreted by xgettext
				// this means that it is ok to use them in constant initializers
				if (method_type.method_symbol.get_full_name() == "GLib.N_") {
					// first argument is string
					return argument_list[0].is_constant();
				} else if (method_type.method_symbol.get_full_name() == "GLib.NC_") {
					// second argument is string
					return argument_list[1].is_constant();
				}
			}

			return false;
		}

		public override bool is_pure() {
			return false;
		}

		public override bool is_accessible(Symbol sym) {
			foreach (var arg in argument_list) {
				if (!arg.is_accessible(sym)) {
					return false;
				}
			}

			return call.is_accessible(sym);
		}

		public override bool check(CodeContext context) {
			if (is_checked) {
				return !error;
			}

			is_checked = true;

			if (!call.check(context)) {
				/* if method resolving didn't succeed, skip this check */
				error = true;
				return false;
			}

			// type of target object
			DataType target_object_type = null;

			List<DataType> method_type_args = null;

			if (call.value_type is DelegateType) {
				// delegate invocation, resolve generic types relative to delegate
				target_object_type = call.value_type;
			} else if (call is MemberAccess) {
				var ma = (MemberAccess)call;
				if (ma.prototype_access) {
					error = true;
					Report.error(source_reference, "Access to instance member `%s' denied".printf(call.symbol_reference.get_full_name()));
					return false;
				}

				method_type_args = ma.get_type_arguments();

				if (ma.inner != null) {
					target_object_type = ma.inner.value_type;

					// foo is relevant instance in foo.bar.connect (on_bar)
					if (ma.inner.symbol_reference is Signal) {
						var sig = ma.inner as MemberAccess;
						if (sig != null) {
							target_object_type = sig.inner.value_type;
						}
					}

					// foo is relevant instance in foo.bar.begin (bar_ready) and foo.bar.end (result)
					var m = ma.symbol_reference as Method;
					if (m != null && m.coroutine) {
						// begin or end call of async method
						if (ma.member_name == "begin" || ma.member_name == "end") {
							var method_access = ma.inner as MemberAccess;
							if (method_access != null && method_access.inner != null) {
								target_object_type = method_access.inner.value_type;
							} else {
								// static method
								target_object_type = null;
							}
						}
					}
				}

				if (ma.symbol_reference != null && ma.symbol_reference.get_attribute("Assert") != null) {
					this.is_assert = true;

					var _args = get_argument_list();
					if (_args.Count == 1) {
						this.source_reference = _args[0].source_reference;
					}
				}
			}

			var mtype = call.value_type;
			var gobject_chainup = call.symbol_reference == context.analyzer.object_type;
			is_chainup = gobject_chainup;

			if (!gobject_chainup) {
				var expr = call;
				var ma = expr as MemberAccess;
				if (ma != null && ma.symbol_reference is CreationMethod) {
					expr = ma.inner;
					ma = expr as MemberAccess;
				}
				if (ma != null && ma.member_name == "this") {
					// this[.with_foo] ()
					is_chainup = true;
				} else if (expr is BaseAccess) {
					// base[.with_foo] ()
					is_chainup = true;
				}
			}

			CreationMethod base_cm = null;

			if (is_chainup) {
				var cm = context.analyzer.find_current_method() as CreationMethod;
				if (cm == null) {
					error = true;
					Report.error(source_reference, "invocation not supported in this context");
					return false;
				} else if (cm.chain_up) {
					error = true;
					Report.error(source_reference, "Multiple constructor calls in the same constructor are not permitted");
					return false;
				}
				cm.chain_up = true;

				if (mtype is ObjectType) {
					var otype = (ObjectType)mtype;
					var cl = (Class)otype.type_symbol;
					base_cm = cl.default_construction_method;
					if (base_cm == null) {
						error = true;
						Report.error(source_reference, "chain up to `%s' not supported".printf(cl.get_full_name()));
						return false;
					} else if (!base_cm.has_construct_function) {
						error = true;
						Report.error(source_reference, "chain up to `%s' not supported".printf(base_cm.get_full_name()));
						return false;
					}
				} else if (call.symbol_reference is CreationMethod && call.symbol_reference.parent_symbol is Class) {
					base_cm = (CreationMethod)call.symbol_reference;
					if (!base_cm.has_construct_function) {
						error = true;
						Report.error(source_reference, "chain up to `%s' not supported".printf(base_cm.get_full_name()));
						return false;
					}
				} else if (gobject_chainup) {
					var cl = cm.parent_symbol as Class;
					if (cl == null || !cl.is_subtype_of(context.analyzer.object_type)) {
						error = true;
						Report.error(source_reference, "chain up to `GLib.Object' not supported");
						return false;
					}
					call.value_type = new ObjectType(context.analyzer.object_type);
					mtype = call.value_type;
				}
			}

			// check for struct construction
			if (call is MemberAccess &&
				((call.symbol_reference is CreationMethod
				  && call.symbol_reference.parent_symbol is Struct)
				 || call.symbol_reference is Struct)) {
				var st = call.symbol_reference as Struct;
				if (st != null && st.default_construction_method == null && (st.is_boolean_type() || st.is_integer_type() || st.is_floating_type())) {
					error = true;
					Report.error(source_reference, "invocation not supported in this context");
					return false;
				}

				var struct_creation_expression = new ObjectCreationExpression((MemberAccess)call, source_reference);
				struct_creation_expression.struct_creation = true;
				foreach (Expression arg in get_argument_list()) {
					struct_creation_expression.add_argument(arg);
				}
				struct_creation_expression.target_type = target_type;
				context.analyzer.replaced_nodes.Add(this);
				parent_node.replace_expression(this, struct_creation_expression);
				struct_creation_expression.check(context);
				return true;
			} else if (!is_chainup && call is MemberAccess && call.symbol_reference is CreationMethod) {
				error = true;
				Report.error(source_reference, "use `new' operator to create new objects");
				return false;
			}

			if (!is_chainup && mtype is ObjectType) {
				// prevent funny stuff like (new Object ()) ()
				error = true;
				Report.error(source_reference, "invocation not supported in this context");
				return false;
			} else if (mtype != null && mtype.is_invokable()) {
				// call ok, expression is invokable
			} else if (call.symbol_reference is Class) {
				error = true;
				Report.error(source_reference, "use `new' operator to create new objects");
				return false;
			} else {
				error = true;
				Report.error(source_reference, "invocation not supported in this context");
				return false;
			}

			var ret_type = mtype.get_return_type();
			var _params = mtype.get_parameters();

			if (mtype is MethodType) {
				var m = ((MethodType)mtype).method_symbol;
				if (m != null && m.coroutine) {
					var ma = (MemberAccess)call;
					if (!is_yield_expression) {
						// begin or end call of async method
						if (ma.member_name != "end") {
							// begin (possibly implicit)
							if (ma.member_name != "begin") {
								Report.deprecated(ma.source_reference, "implicit .begin is deprecated");
							}
							_params = m.get_async_begin_parameters();
							ret_type = new VoidType();
						} else {
							// end
							_params = m.get_async_end_parameters();
						}
					} else if (ma.member_name == "begin" || ma.member_name == "end") {
						error = true;
						Report.error(ma.source_reference, "use of `%s' not allowed in yield statement".printf(ma.member_name));
					}
				}

				if (m != null) {
					var ma = (MemberAccess)call;
					int n_type_params = m.get_type_parameters().Count;
					int n_type_args = ma.get_type_arguments().Count;
					if (n_type_args > 0 && n_type_args < n_type_params) {
						error = true;
						Report.error(ma.source_reference, "too few type arguments");
						return false;
					} else if (n_type_args > 0 && n_type_args > n_type_params) {
						error = true;
						Report.error(ma.source_reference, "too many type arguments");
						return false;
					}
				}
			}

			// FIXME partial code duplication in ObjectCreationExpression.check

			Expression last_arg = null;

			var args = get_argument_list();
			IEnumerator<Expression> arg_it = args.GetEnumerator();
			foreach (Parameter param in _params) {
				if (param.ellipsis) {
					break;
				}

				if (param.params_array) {
					var array_type = (ArrayType)param.variable_type;
					while (arg_it.MoveNext()) {
						Expression arg = arg_it.Current;

						/* store expected type for callback parameters */
						arg.target_type = array_type.element_type;
						arg.target_type.value_owned = array_type.value_owned;
					}
					break;
				}

				if (arg_it.MoveNext()) {
					Expression arg = arg_it.Current;

					/* store expected type for callback parameters */
					arg.formal_target_type = param.variable_type;
					arg.target_type = arg.formal_target_type.get_actual_type(target_object_type, method_type_args, this);

					last_arg = arg;
				}
			}

			// concatenate stringified arguments for methods with attribute [Print]
			if (mtype is MethodType && ((MethodType)mtype).method_symbol.get_attribute("Print") != null) {
				var template = new Template(source_reference);
				foreach (Expression arg in argument_list) {
					arg.parent_node = null;
					template.add_expression(arg);
				}
				argument_list.Clear();
				add_argument(template);
			}

			// printf arguments
			if (mtype is MethodType && ((MethodType)mtype).method_symbol.printf_format) {
				StringLiteral format_literal = null;
				if (last_arg != null) {
					// use last argument as format string
					format_literal = StringLiteral.get_format_literal(last_arg);
					if (format_literal == null && args.Count == _params.Count - 1) {
						// insert "%s" to avoid issues with embedded %
						format_literal = new StringLiteral("\"%s\"");
						format_literal.target_type = context.analyzer.string_type.copy();
						argument_list.Insert(args.Count - 1, format_literal);

						// recreate iterator and skip to right position
						arg_it = argument_list.GetEnumerator();
						foreach (Parameter param in _params) {
							if (param.ellipsis) {
								break;
							}
							arg_it.MoveNext();
						}
					}
				} else {
					// use instance as format string for string.printf (...)
					var ma = call as MemberAccess;
					if (ma != null) {
						format_literal = StringLiteral.get_format_literal(ma.inner);
					}
				}
				if (format_literal != null) {
					string format = format_literal.eval();
					if (!context.analyzer.check_print_format(format, arg_it, source_reference)) {
						return false;
					}
				}
			}

			foreach (Expression arg in get_argument_list().ToList()) {
				arg.check(context);
			}

			if (ret_type is VoidType) {
				// void return type
				if (!(parent_node is ExpressionStatement)
					&& !(parent_node is ForStatement)
					&& !(parent_node is YieldStatement)) {
					// A void method invocation can be in the initializer or
					// iterator of a for statement
					error = true;
					Report.error(source_reference, "invocation of void method not allowed as expression");
					return false;
				}
			}

			formal_value_type = ret_type.copy();
			value_type = formal_value_type.get_actual_type(target_object_type, method_type_args, this);

			bool may_throw = false;

			if (mtype is MethodType) {
				var m = ((MethodType)mtype).method_symbol;
				if (is_yield_expression) {
					if (!m.coroutine) {
						error = true;
						Report.error(source_reference, "yield expression requires async method");
					}
					if (context.analyzer.current_method == null || !context.analyzer.current_method.coroutine) {
						error = true;
						Report.error(source_reference, "yield expression not available outside async method");
					}
				}
				if (m != null && m.coroutine && !is_yield_expression && ((MemberAccess)call).member_name != "end") {
					// .begin call of async method, no error can happen here
				} else {
					foreach (DataType error_type in m.get_error_types()) {
						may_throw = true;

						// ensure we can trace back which expression may throw errors of this type
						var call_error_type = error_type.copy();
						call_error_type.source_reference = source_reference;

						add_error_type(call_error_type);
					}
				}
				if (m.returns_floating_reference) {
					value_type.floating_reference = true;
				}
				if (m.returns_modified_pointer) {
					((MemberAccess)call).inner.lvalue = true;
				}

				var dynamic_sig = m.parent_symbol as DynamicSignal;
				if (dynamic_sig != null && dynamic_sig.handler != null) {
					dynamic_sig.return_type = dynamic_sig.handler.value_type.get_return_type().copy();
					bool first = true;
					foreach (Parameter param in dynamic_sig.handler.value_type.get_parameters()) {
						if (first) {
							// skip sender parameter
							first = false;
						} else {
							dynamic_sig.add_parameter(param.copy());
						}
					}
					dynamic_sig.handler.target_type = new DelegateType(dynamic_sig.get_delegate(new ObjectType((ObjectTypeSymbol)dynamic_sig.parent_symbol), this));
				}

				if (m != null && m.get_type_parameters().Count > 0) {
					var ma = (MemberAccess)call;
					if (ma.get_type_arguments().Count == 0) {
						// infer type arguments
						foreach (var type_param in m.get_type_parameters()) {
							DataType type_arg = null;

							// infer type arguments from arguments
							arg_it = args.GetEnumerator();
							foreach (Parameter param in _params) {
								if (param.ellipsis || param.params_array) {
									break;
								}

								if (arg_it.MoveNext()) {
									Expression arg = arg_it.Current;

									type_arg = param.variable_type.infer_type_argument(type_param, arg.value_type);
									if (type_arg != null) {
										break;
									}

									arg.target_type = arg.formal_target_type.get_actual_type(target_object_type, method_type_args, this);
								}
							}

							// infer type arguments from expected return type
							if (type_arg == null && target_type != null) {
								type_arg = m.return_type.infer_type_argument(type_param, target_type);
							}

							if (type_arg == null) {
								error = true;
								Report.error(ma.source_reference, "cannot infer generic type argument for type parameter `%s'".printf(type_param.get_full_name()));
								return false;
							}

							ma.add_type_argument(type_arg);
						}

						// recalculate argument target types with new information
						arg_it = args.GetEnumerator();
						foreach (Parameter param in _params) {
							if (param.ellipsis || param.params_array) {
								break;
							}

							if (arg_it.MoveNext()) {
								Expression arg = arg_it.Current;

								arg.target_type = arg.formal_target_type.get_actual_type(target_object_type, method_type_args, this);
							}
						}

						// recalculate return value type with new information
						value_type = formal_value_type.get_actual_type(target_object_type, method_type_args, this);
					}
				}
				// replace method-type if needed for proper argument-check in semantic-analyser
				if (m != null && m.coroutine) {
					var ma = (MemberAccess)call;
					if (ma.member_name == "end") {
						mtype = new MethodType(m.get_end_method());
					}
				}
			} else if (mtype is ObjectType) {
				// constructor
				var cl = (Class)((ObjectType)mtype).type_symbol;
				var m = cl.default_construction_method;
				foreach (DataType error_type in m.get_error_types()) {
					may_throw = true;

					// ensure we can trace back which expression may throw errors of this type
					var call_error_type = error_type.copy();
					call_error_type.source_reference = source_reference;

					add_error_type(call_error_type);
				}
			} else if (mtype is DelegateType) {
				var d = ((DelegateType)mtype).delegate_symbol;
				foreach (DataType error_type in d.get_error_types()) {
					may_throw = true;

					// ensure we can trace back which expression may throw errors of this type
					var call_error_type = error_type.copy();
					call_error_type.source_reference = source_reference;

					add_error_type(call_error_type);
				}
			}

			if (!context.analyzer.check_arguments(this, mtype, _params, get_argument_list())) {
				error = true;
				return false;
			}

			/* Check for constructv chain up */
			if (base_cm != null && base_cm.is_variadic() && args.Count == base_cm.get_parameters().Count) {
				var this_last_arg = args[args.Count - 1];
				if (this_last_arg.value_type is StructValueType && this_last_arg.value_type.data_type == context.analyzer.va_list_type.data_type) {
					is_constructv_chainup = true;
				}
			}

			if (may_throw) {
				if (parent_node is LocalVariable || parent_node is ExpressionStatement) {
					// simple statements, no side effects after method call
				} else if (!(context.analyzer.current_symbol is Block)) {
					// can't handle errors in field initializers
					Report.error(source_reference, "Field initializers must not throw errors");
				} else {
					// store parent_node as we need to replace the expression in the old parent node later on
					var old_parent_node = parent_node;

					var local = new LocalVariable(value_type.copy(), get_temp_name(), null, source_reference);
					var decl = new DeclarationStatement(local, source_reference);

					insert_statement(context.analyzer.insert_block, decl);

					var temp_access = SemanticAnalyzer.create_temp_access(local, target_type);

					// don't set initializer earlier as this changes parent_node and parent_statement
					local.initializer = this;
					decl.check(context);


					// move temp variable to insert block to ensure the
					// variable is in the same block as the declaration
					// otherwise there will be scoping issues in the generated code
					var block = (Block)context.analyzer.current_symbol;
					block.remove_local_variable(local);
					context.analyzer.insert_block.add_local_variable(local);

					old_parent_node.replace_expression(this, temp_access);
					temp_access.check(context);
				}
			}

			return !error;
		}

		public override void emit(CodeGenerator codegen) {
			var method_type = call.value_type as MethodType;

			if (method_type != null && method_type.method_symbol.parent_symbol is Signal) {
				var signal_access = ((MemberAccess)call).inner;
				signal_access.emit(codegen);
			} else {
				call.emit(codegen);
			}

			foreach (Expression expr in argument_list) {
				expr.emit(codegen);
			}

			codegen.visit_method_call(this);

			codegen.visit_expression(this);
		}

		public override void get_defined_variables(ICollection<Variable> collection) {
			call.get_defined_variables(collection);

			foreach (Expression arg in argument_list) {
				arg.get_defined_variables(collection);
			}
		}

		public override void get_used_variables(ICollection<Variable> collection) {
			call.get_used_variables(collection);

			foreach (Expression arg in argument_list) {
				arg.get_used_variables(collection);
			}
		}

		public StringLiteral get_format_literal() {
			var mtype = this.call.value_type as MethodType;
			if (mtype != null) {
				int format_arg = mtype.method_symbol.get_format_arg_index();
				if (format_arg >= 0 && format_arg < argument_list.Count) {
					return StringLiteral.get_format_literal(argument_list[format_arg]);
				}
			}

			return null;
		}
	}

}
