﻿using CLanguage;
using CLanguage.Expressions;
using CLanguage.Statements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala;
using Vala.Lang;
using Vala.Lang.Methods;
using Vala.Lang.Parser;
using Vala.Lang.Statements;
using Vala.Lang.Symbols;
using Vala.Lang.Types;
using Vala.Lang.TypeSymbols;
using static CCodeGen.CCode;

namespace CCodeGen.Modules
{
	public class GAsyncModule : GtkModule
	{
		CCodeStruct generate_data_struct(Method m) {
			string dataname = Symbol.lower_case_to_camel_case(get_ccode_name(m)) + "Data";
			var data = new CCodeStruct("_" + dataname);

			data.add_field("int", "_state_");
			data.add_field("GObject*", "_source_object_");
			data.add_field("GAsyncResult*", "_res_");

			data.add_field("GTask*", "_async_result");
			if (!context.require_glib_version(2, 44)) {
				data.add_field("GAsyncReadyCallback", "_callback_");
				data.add_field("gboolean", "_task_complete_");
			}

			if (m is CreationMethod) {
				data.add_field("GType", "object_type");
			}

			if (m.binding == MemberBinding.INSTANCE) {
				var type_sym = (TypeSymbol)m.parent_symbol;
				if (type_sym is ObjectTypeSymbol) {
					data.add_field(get_ccode_name(type_sym) + "*", "self");
				} else {
					data.add_field(get_ccode_name(type_sym), "self");
				}
			}

			foreach (Parameter param in m.get_parameters()) {
				bool is_unowned_delegate = param.variable_type is DelegateType && !param.variable_type.value_owned;

				var param_type = param.variable_type.copy();
				param_type.value_owned = true;
				data.add_field(get_ccode_name(param_type), get_variable_cname(param.name));

				if (param.variable_type is ArrayType) {
					var array_type = (ArrayType)param.variable_type;
					if (get_ccode_array_length(param)) {
						for (int dim = 1; dim <= array_type.rank; dim++) {
							data.add_field("gint", get_parameter_array_length_cname(param, dim));
						}
					}
				} else if (param.variable_type is DelegateType) {
					var deleg_type = (DelegateType)param.variable_type;
					if (deleg_type.delegate_symbol.has_target) {
						data.add_field("gpointer", get_ccode_delegate_target_name(param));
						if (!is_unowned_delegate) {
							data.add_field("GDestroyNotify", get_delegate_target_destroy_notify_cname(get_variable_cname(param.name)));
						}
					}
				}
			}

			foreach (var type_param in m.get_type_parameters()) {
				data.add_field("GType", "%s_type".printf(type_param.name.ToLower()));
				data.add_field("GBoxedCopyFunc", "%s_dup_func".printf(type_param.name.ToLower()));
				data.add_field("GDestroyNotify", "%s_destroy_func".printf(type_param.name.ToLower()));
			}

			if (!(m.return_type is VoidType)) {
				data.add_field(get_ccode_name(m.return_type), "result");
				if (m.return_type is ArrayType) {
					var array_type = (ArrayType)m.return_type;
					if (get_ccode_array_length(m)) {
						for (int dim = 1; dim <= array_type.rank; dim++) {
							data.add_field("gint", get_array_length_cname("result", dim));
						}
					}
				} else if (m.return_type is DelegateType) {
					var deleg_type = (DelegateType)m.return_type;
					if (deleg_type.delegate_symbol.has_target) {
						data.add_field("gpointer", get_delegate_target_cname("result"));
						data.add_field("GDestroyNotify", get_delegate_target_destroy_notify_cname("result"));
					}
				}
			}

			return data;
		}

		CCodeFunction generate_free_function(Method m) {
			var dataname = Symbol.lower_case_to_camel_case(get_ccode_name(m)) + "Data";

			var freefunc = new CCodeFunction(get_ccode_real_name(m) + "_data_free", "void");
			freefunc.modifiers = CCodeModifiers.STATIC;
			freefunc.add_parameter(new CCodeParameter("_data", "gpointer"));

			push_context(new EmitContext(m));
			push_function(freefunc);

			ccode.add_declaration(dataname + "*", new CCodeVariableDeclarator("_data_", new CCodeIdentifier("_data")));

			foreach (Parameter param in m.get_parameters()) {
				if (!param.captured && param.direction != ParameterDirection.OUT) {
					var param_type = param.variable_type.copy();
					if (!param_type.value_owned) {
						param_type.value_owned = !no_implicit_copy(param_type);
					}

					if (requires_destroy(param_type)) {
						ccode.add_expression(destroy_parameter(param));
					}
				}
			}

			if (requires_destroy(m.return_type)) {
				if (get_ccode_array_length(m) || !(m.return_type is ArrayType)) {
					/* this is very evil. */
					var v = new LocalVariable(m.return_type, ".result");
					ccode.add_expression(destroy_local(v));
				} else {
					var v = new GLibValue(m.return_type, new CCodeIdentifier("_data_->result"), true);
					v.array_null_terminated = get_ccode_array_null_terminated(m);
					ccode.add_expression(destroy_value(v));
				}
			}

			if (m.binding == MemberBinding.INSTANCE) {
				var this_type = m.this_parameter.variable_type.copy();
				this_type.value_owned = true;

				if (requires_destroy(this_type)) {
					ccode.add_expression(destroy_parameter(m.this_parameter));
				}
			}

			var freecall = new CCodeFunctionCall(new CCodeIdentifier("g_slice_free"));
			freecall.add_argument(new CCodeIdentifier(dataname));
			freecall.add_argument(new CCodeIdentifier("_data_"));
			ccode.add_expression(freecall);

			pop_context();

			cfile.add_function_declaration(freefunc);
			cfile.add_function(freefunc);

			return freefunc;
		}

		void generate_async_ready_callback_wrapper(Method m, string function_name) {
			var function = new CCodeFunction(function_name, "void");
			function.modifiers = CCodeModifiers.STATIC;

			function.add_parameter(new CCodeParameter("*source_object", "GObject"));
			function.add_parameter(new CCodeParameter("*res", "GAsyncResult"));
			function.add_parameter(new CCodeParameter("*user_data", "void"));

			push_function(function);

			// Set _task_complete_ to false after calling back to the real func
			var async_result_cast = new CCodeFunctionCall(new CCodeIdentifier("G_TASK"));
			async_result_cast.add_argument(new CCodeIdentifier("res"));

			var dataname = Symbol.lower_case_to_camel_case(get_ccode_name(m)) + "Data";
			ccode.add_declaration(dataname + "*", new CCodeVariableDeclarator("_task_data_"));

			var get_data_call = new CCodeFunctionCall(new CCodeIdentifier("g_task_get_task_data"));
			get_data_call.add_argument(async_result_cast);

			var data_var = new CCodeIdentifier("_task_data_");
			ccode.add_assignment(data_var, get_data_call);

			var task_inner_callback = CCodeMemberAccess.pointer(data_var, "_callback_");
			var callback_is_nonnull = new CCodeBinaryExpression(CCodeBinaryOperator.INEQUALITY, task_inner_callback, new CCodeConstant("NULL"));

			ccode.open_if(callback_is_nonnull);
			var nested_callback = new CCodeFunctionCall(task_inner_callback);
			nested_callback.add_argument(new CCodeIdentifier("source_object"));
			nested_callback.add_argument(new CCodeIdentifier("res"));
			nested_callback.add_argument(new CCodeIdentifier("user_data"));
			ccode.add_expression(nested_callback);
			ccode.close();

			ccode.add_assignment(CCodeMemberAccess.pointer(data_var, "_task_complete_"), new CCodeConstant("TRUE"));

			pop_function();

			cfile.add_function_declaration(function);
			cfile.add_function(function);
		}

		void generate_async_function(Method m) {
			push_context(new EmitContext());

			string callback_wrapper = null;

			if (!context.require_glib_version(2, 44)) {
				callback_wrapper = get_ccode_real_name(m) + "_async_ready_wrapper";
				generate_async_ready_callback_wrapper(m, callback_wrapper);
			}

			var dataname = Symbol.lower_case_to_camel_case(get_ccode_name(m)) + "Data";
			var asyncfunc = new CCodeFunction(get_ccode_real_name(m), "void");
			var cparam_map = new Dictionary<int, CCodeParameter>();

			cparam_map[get_param_pos(-1)] = new CCodeParameter("_callback_", "GAsyncReadyCallback");
			cparam_map[get_param_pos(-0.9)] = new CCodeParameter("_user_data_", "gpointer");

			generate_cparameters(m, cfile, cparam_map, asyncfunc, null, null, null, 1);

			if (m.base_method != null || m.base_interface_method != null) {
				// declare *_real_* function
				asyncfunc.modifiers |= CCodeModifiers.STATIC;
				cfile.add_function_declaration(asyncfunc);
			} else if (m.is_private_symbol()) {
				asyncfunc.modifiers |= CCodeModifiers.STATIC;
			} else if (context.hide_internal && m.is_internal_symbol()) {
				asyncfunc.modifiers |= CCodeModifiers.INTERNAL;
			}

			push_function(asyncfunc);

			// logic copied from valaccodemethodmodule
			if (m.overrides || (m.base_interface_method != null && !m.is_abstract && !m.is_virtual)) {
				Method base_method;

				if (m.overrides) {
					base_method = m.base_method;
				} else {
					base_method = m.base_interface_method;
				}

				var base_expression_type = new ObjectType((ObjectTypeSymbol)base_method.parent_symbol);
				var type_symbol = m.parent_symbol as ObjectTypeSymbol;

				var self_target_type = new ObjectType(type_symbol);
				var cself = get_cvalue_(transform_value(new GLibValue(base_expression_type, new CCodeIdentifier("base"), true), self_target_type, m));
				ccode.add_declaration("%s *".printf(get_ccode_name(type_symbol)), new CCodeVariableDeclarator("self"));
				ccode.add_assignment(new CCodeIdentifier("self"), cself);
			}

			var dataalloc = new CCodeFunctionCall(new CCodeIdentifier("g_slice_new0"));
			dataalloc.add_argument(new CCodeIdentifier(dataname));

			var data_var = new CCodeIdentifier("_data_");

			ccode.add_declaration(dataname + "*", new CCodeVariableDeclarator("_data_"));
			ccode.add_assignment(data_var, dataalloc);

			if (!context.require_glib_version(2, 44)) {
				ccode.add_assignment(CCodeMemberAccess.pointer(data_var, "_callback_"), new CCodeConstant("_callback_"));
			}

			var create_result = new CCodeFunctionCall(new CCodeIdentifier("g_task_new"));

			var t = m.parent_symbol as TypeSymbol;
			if (!(m is CreationMethod) && m.binding == MemberBinding.INSTANCE &&
				t != null && t.is_subtype_of(gobject_type)) {
				var gobject_cast = new CCodeFunctionCall(new CCodeIdentifier("G_OBJECT"));
				gobject_cast.add_argument(new CCodeIdentifier("self"));

				create_result.add_argument(gobject_cast);
			} else {
				create_result.add_argument(new CCodeConstant("NULL"));
			}

			Parameter cancellable_param = null;

			foreach (Parameter param in m.get_parameters()) {
				if (param.variable_type is ObjectType && param.variable_type.data_type.get_full_name() == "GLib.Cancellable") {
					cancellable_param = param;
					break;
				}
			}

			if (cancellable_param == null) {
				create_result.add_argument(new CCodeConstant("NULL"));
			} else {
				create_result.add_argument(new CCodeIdentifier(get_variable_cname(cancellable_param.name)));
			}

			if (context.require_glib_version(2, 44)) {
				create_result.add_argument(new CCodeIdentifier("_callback_"));
			} else {
				create_result.add_argument(new CCodeIdentifier(callback_wrapper));
			}
			create_result.add_argument(new CCodeIdentifier("_user_data_"));

			ccode.add_assignment(CCodeMemberAccess.pointer(data_var, "_async_result"), create_result);

			if (!context.require_glib_version(2, 44)) {
				var task_completed_var = CCodeMemberAccess.pointer(data_var, "_task_complete_");
				var callback = new CCodeIdentifier("_callback_");
				var callback_is_null = new CCodeBinaryExpression(CCodeBinaryOperator.EQUALITY, callback, new CCodeConstant("NULL"));

				ccode.open_if(callback_is_null);
				ccode.add_assignment(task_completed_var, new CCodeConstant("TRUE"));
				ccode.close();
			}

			var attach_data_call = new CCodeFunctionCall(new CCodeIdentifier("g_task_set_task_data"));

			attach_data_call.add_argument(CCodeMemberAccess.pointer(data_var, "_async_result"));
			attach_data_call.add_argument(data_var);
			attach_data_call.add_argument(new CCodeIdentifier(get_ccode_real_name(m) + "_data_free"));
			ccode.add_expression(attach_data_call);

			if (m is CreationMethod) {
				ccode.add_assignment(CCodeMemberAccess.pointer(data_var, "object_type"), new CCodeIdentifier("object_type"));
			} else if (m.binding == MemberBinding.INSTANCE) {
				var this_type = m.this_parameter.variable_type.copy();
				this_type.value_owned = true;

				// create copy if necessary as variables in async methods may need to be kept alive
				CCodeExpression cself = new CCodeIdentifier("self");
				if (this_type.is_real_non_null_struct_type()) {
					cself = new CCodeUnaryExpression(CCodeUnaryOperator.POINTER_INDIRECTION, cself);
				}
				if (requires_copy(this_type)) {
					cself = get_cvalue_(copy_value(new GLibValue(m.this_parameter.variable_type, cself, true), m.this_parameter));
				}

				ccode.add_assignment(CCodeMemberAccess.pointer(data_var, "self"), cself);
			}

			emit_context.push_symbol(m);
			foreach (Parameter param in m.get_parameters()) {
				if (param.direction != ParameterDirection.OUT) {
					// create copy if necessary as variables in async methods may need to be kept alive
					var old_captured = param.captured;
					param.captured = false;
					current_method.coroutine = false;

					TargetValue value;
					if (param.variable_type.value_owned) {
						// do not use load_parameter for reference/ownership transfer
						// otherwise delegate destroy notify will not be moved
						value = get_parameter_cvalue(param);
					} else {
						value = load_parameter(param);
					}

					current_method.coroutine = true;

					store_parameter(param, value);

					param.captured = old_captured;
				}
			}
			emit_context.pop_symbol();

			foreach (var type_param in m.get_type_parameters()) {
				var type = "%s_type".printf(type_param.name.ToLower());
				var dup_func = "%s_dup_func".printf(type_param.name.ToLower());
				var destroy_func = "%s_destroy_func".printf(type_param.name.ToLower());
				ccode.add_assignment(CCodeMemberAccess.pointer(data_var, type), new CCodeIdentifier(type));
				ccode.add_assignment(CCodeMemberAccess.pointer(data_var, dup_func), new CCodeIdentifier(dup_func));
				ccode.add_assignment(CCodeMemberAccess.pointer(data_var, destroy_func), new CCodeIdentifier(destroy_func));
			}

			var ccall = new CCodeFunctionCall(new CCodeIdentifier(get_ccode_real_name(m) + "_co"));
			ccall.add_argument(data_var);
			ccode.add_expression(ccall);

			cfile.add_function(asyncfunc);

			pop_context();
		}

		void append_struct(CCodeStruct structure) {
			var typename = new CCodeVariableDeclarator(structure.name.Substring(1));
			var typedef = new CCodeTypeDefinition("struct " + structure.name, typename);
			cfile.add_type_declaration(typedef);
			cfile.add_type_definition(structure);
		}

		public override void generate_method_declaration(Method m, CCodeFile decl_space) {
			if (m.coroutine) {
				if (add_symbol_declaration(decl_space, m, get_ccode_name(m))) {
					return;
				}

				var cl = m.parent_symbol as Class;

				var asyncfunc = new CCodeFunction(get_ccode_name(m), "void");
				var cparam_map = new Dictionary<int, CCodeParameter>();
				var carg_map = new Dictionary<int, CCodeExpression>();

				if (m.is_private_symbol()) {
					asyncfunc.modifiers |= CCodeModifiers.STATIC;
				} else if (context.hide_internal && m.is_internal_symbol()) {
					asyncfunc.modifiers |= CCodeModifiers.INTERNAL;
				}

				// do not generate _new functions for creation methods of abstract classes
				if (!(m is CreationMethod && cl != null && cl.is_abstract)) {
					generate_cparameters(m, decl_space, cparam_map, asyncfunc, null, carg_map, new CCodeFunctionCall(new CCodeIdentifier("fake")), 1);

					decl_space.add_function_declaration(asyncfunc);
				}

				var finishfunc = new CCodeFunction(get_ccode_finish_name(m));
				cparam_map = new Dictionary<int, CCodeParameter>();
				carg_map = new Dictionary<int, CCodeExpression>();

				if (m.is_private_symbol()) {
					finishfunc.modifiers |= CCodeModifiers.STATIC;
				} else if (context.hide_internal && m.is_internal_symbol()) {
					finishfunc.modifiers |= CCodeModifiers.INTERNAL;
				}

				// do not generate _new functions for creation methods of abstract classes
				if (!(m is CreationMethod && cl != null && cl.is_abstract)) {
					generate_cparameters(m, decl_space, cparam_map, finishfunc, null, carg_map, new CCodeFunctionCall(new CCodeIdentifier("fake")), 2);

					decl_space.add_function_declaration(finishfunc);
				}

				if (m is CreationMethod && cl != null) {
					// _construct function
					var function = new CCodeFunction(get_ccode_real_name(m));

					if (m.is_private_symbol()) {
						function.modifiers |= CCodeModifiers.STATIC;
					} else if (context.hide_internal && m.is_internal_symbol()) {
						function.modifiers |= CCodeModifiers.INTERNAL;
					}

					cparam_map = new Dictionary<int, CCodeParameter>();
					generate_cparameters(m, decl_space, cparam_map, function, null, null, null, 1);

					decl_space.add_function_declaration(function);

					function = new CCodeFunction(get_ccode_finish_real_name(m));

					if (m.is_private_symbol()) {
						function.modifiers |= CCodeModifiers.STATIC;
					} else if (context.hide_internal && m.is_internal_symbol()) {
						function.modifiers |= CCodeModifiers.INTERNAL;
					}

					cparam_map = new Dictionary<int, CCodeParameter>();
					generate_cparameters(m, decl_space, cparam_map, function, null, null, null, 2);

					decl_space.add_function_declaration(function);
				}
			} else {
				base.generate_method_declaration(m, decl_space);
			}
		}

		public override void visit_method(Method m) {
			if (m.coroutine) {
				cfile.add_include("gio/gio.h");
				if (!m.is_internal_symbol()) {
					header_file.add_include("gio/gio.h");
				}

				if (!m.is_abstract && m.body != null) {
					var data = generate_data_struct(m);

					closure_struct = data;

					generate_free_function(m);
					generate_async_function(m);
					generate_finish_function(m);

					// append the _co function
					base.visit_method(m);
					closure_struct = null;

					// only append data struct here to make sure all struct member
					// types are declared before the struct definition
					append_struct(data);
				} else {
					generate_method_declaration(m, cfile);

					if (!m.is_internal_symbol()) {
						generate_method_declaration(m, header_file);
					}
					if (!m.is_private_symbol()) {
						generate_method_declaration(m, internal_header_file);
					}
				}

				if (m.is_abstract || m.is_virtual) {
					// generate virtual function wrappers
					var cparam_map = new Dictionary<int, CCodeParameter>();
					var carg_map = new Dictionary<int, CCodeExpression>();
					generate_vfunc(m, new VoidType(), cparam_map, carg_map, "", 1);

					cparam_map = new Dictionary<int, CCodeParameter>();
					carg_map = new Dictionary<int, CCodeExpression>();
					generate_vfunc(m, m.return_type, cparam_map, carg_map, "_finish", 2);
				}
			} else {
				base.visit_method(m);
			}
		}

		public override void visit_creation_method(CreationMethod m) {
			if (!m.coroutine) {
				base.visit_creation_method(m);
			} else {
				push_line(m.source_reference);

				bool visible = !m.is_private_symbol();

				visit_method(m);

				if (m.source_type == SourceFileType.FAST) {
					return;
				}

				// do not generate _new functions for creation methods of abstract classes
				if (current_type_symbol is Class && !current_class.is_compact && !current_class.is_abstract) {
					var vfunc = new CCodeFunction(get_ccode_name(m));

					var cparam_map = new Dictionary<int, CCodeParameter>();
					var carg_map = new Dictionary<int, CCodeExpression>();

					push_function(vfunc);

					var vcall = new CCodeFunctionCall(new CCodeIdentifier(get_ccode_real_name(m)));
					vcall.add_argument(new CCodeIdentifier(get_ccode_type_id(current_class)));

					generate_cparameters(m, cfile, cparam_map, vfunc, null, carg_map, vcall, 1);
					ccode.add_expression(vcall);

					if (!visible) {
						vfunc.modifiers |= CCodeModifiers.STATIC;
					}

					pop_function();

					cfile.add_function(vfunc);


					vfunc = new CCodeFunction(get_ccode_finish_name(m));

					cparam_map = new Dictionary<int, CCodeParameter>();
					carg_map = new Dictionary<int, CCodeExpression>();

					push_function(vfunc);

					vcall = new CCodeFunctionCall(new CCodeIdentifier(get_ccode_finish_real_name(m)));

					generate_cparameters(m, cfile, cparam_map, vfunc, null, carg_map, vcall, 2);
					ccode.add_return(vcall);

					if (!visible) {
						vfunc.modifiers |= CCodeModifiers.STATIC;
					}

					pop_function();

					cfile.add_function(vfunc);
				}

				pop_line();
			}
		}

		void generate_finish_function(Method m) {
			push_context(new EmitContext());

			string dataname = Symbol.lower_case_to_camel_case(get_ccode_name(m)) + "Data";

			var finishfunc = new CCodeFunction(get_ccode_finish_real_name(m));

			var cparam_map = new Dictionary<int, CCodeParameter>();

			cparam_map[get_param_pos(0.1)] = new CCodeParameter("_res_", "GAsyncResult*");

			generate_cparameters(m, cfile, cparam_map, finishfunc, null, null, null, 2);

			if (m.is_private_symbol() || m.base_method != null || m.base_interface_method != null) {
				finishfunc.modifiers |= CCodeModifiers.STATIC;
			} else if (context.hide_internal && m.is_internal_symbol()) {
				finishfunc.modifiers |= CCodeModifiers.INTERNAL;
			}

			push_function(finishfunc);

			var return_type = m.return_type;
			if (m is CreationMethod) {
				var type_sym = (TypeSymbol)m.parent_symbol;
				if (type_sym is ObjectTypeSymbol) {
					ccode.add_declaration(get_ccode_name(type_sym) + "*", new CCodeVariableDeclarator("result"));
				}
			} else if (!(return_type is VoidType) && !return_type.is_real_non_null_struct_type()) {
				ccode.add_declaration(get_ccode_name(m.return_type), new CCodeVariableDeclarator("result"));
			}

			var data_var = new CCodeIdentifier("_data_");

			ccode.add_declaration(dataname + "*", new CCodeVariableDeclarator("_data_"));

			var async_result_cast = new CCodeFunctionCall(new CCodeIdentifier("G_TASK"));
			async_result_cast.add_argument(new CCodeIdentifier("_res_"));

			var ccall = new CCodeFunctionCall(new CCodeIdentifier("g_task_propagate_pointer"));
			ccall.add_argument(async_result_cast);

			if (m.get_error_types().Count > 0) {
				ccall.add_argument(new CCodeIdentifier("error"));
			} else {
				ccall.add_argument(new CCodeConstant("NULL"));
			}

			ccode.add_assignment(data_var, ccall);

			bool has_cancellable = false;

			foreach (Parameter param in m.get_parameters()) {
				if (param.variable_type is ObjectType && param.variable_type.data_type.get_full_name() == "GLib.Cancellable") {
					has_cancellable = true;
					break;
				}
			}

			// If a task is cancelled, g_task_propagate_pointer returns NULL
			if (m.get_error_types().Count > 0 || has_cancellable) {
				var is_null = new CCodeBinaryExpression(CCodeBinaryOperator.EQUALITY, new CCodeConstant("NULL"), data_var);

				ccode.open_if(is_null);
				return_default_value(return_type);
				ccode.close();
			}

			emit_context.push_symbol(m);
			foreach (Parameter param in m.get_parameters()) {
				if (param.direction != ParameterDirection.IN) {
					return_out_parameter(param);
					if (!(param.variable_type is ValaValueType) || param.variable_type.nullable) {
						ccode.add_assignment(CCodeMemberAccess.pointer(data_var, get_variable_cname(param.name)), new CCodeConstant("NULL"));
					}
				}
			}
			emit_context.pop_symbol();

			if (m is CreationMethod) {
				ccode.add_assignment(new CCodeIdentifier("result"), CCodeMemberAccess.pointer(data_var, "self"));
				ccode.add_assignment(CCodeMemberAccess.pointer(data_var, "self"), new CCodeConstant("NULL"));
				ccode.add_return(new CCodeIdentifier("result"));
			} else if (return_type.is_real_non_null_struct_type()) {
				// structs are returned via out parameter
				CCodeExpression cexpr = CCodeMemberAccess.pointer(data_var, "result");
				if (requires_copy(return_type)) {
					cexpr = get_cvalue_(copy_value(new GLibValue(return_type, cexpr, true), return_type));
				}
				ccode.add_assignment(new CCodeUnaryExpression(CCodeUnaryOperator.POINTER_INDIRECTION, new CCodeIdentifier("result")), cexpr);
			} else if (!(return_type is VoidType)) {
				ccode.add_assignment(new CCodeIdentifier("result"), CCodeMemberAccess.pointer(data_var, "result"));
				if (return_type is ArrayType) {
					var array_type = (ArrayType)return_type;
					if (get_ccode_array_length(m)) {
						for (int dim = 1; dim <= array_type.rank; dim++) {
							ccode.add_assignment(new CCodeUnaryExpression(CCodeUnaryOperator.POINTER_INDIRECTION, new CCodeIdentifier(get_array_length_cname("result", dim))), CCodeMemberAccess.pointer(data_var, get_array_length_cname("result", dim)));
						}
					}
				} else if (return_type is DelegateType && ((DelegateType)return_type).delegate_symbol.has_target) {
					ccode.add_assignment(new CCodeUnaryExpression(CCodeUnaryOperator.POINTER_INDIRECTION, new CCodeIdentifier(get_delegate_target_cname("result"))), CCodeMemberAccess.pointer(data_var, get_delegate_target_cname("result")));
				}
				if (!(return_type is ValaValueType) || return_type.nullable) {
					ccode.add_assignment(CCodeMemberAccess.pointer(data_var, "result"), new CCodeConstant("NULL"));
				}
				ccode.add_return(new CCodeIdentifier("result"));
			}

			pop_function();

			cfile.add_function(finishfunc);

			pop_context();
		}

		public override string generate_ready_function(Method m) {
			// generate ready callback handler

			var dataname = Symbol.lower_case_to_camel_case(get_ccode_name(m)) + "Data";

			var readyfunc = new CCodeFunction(get_ccode_name(m) + "_ready", "void");

			if (!add_wrapper(readyfunc.name)) {
				// wrapper already defined
				return readyfunc.name;
			}

			readyfunc.add_parameter(new CCodeParameter("source_object", "GObject*"));
			readyfunc.add_parameter(new CCodeParameter("_res_", "GAsyncResult*"));
			readyfunc.add_parameter(new CCodeParameter("_user_data_", "gpointer"));

			push_function(readyfunc);

			var data_var = new CCodeIdentifier("_data_");

			ccode.add_declaration(dataname + "*", new CCodeVariableDeclarator("_data_"));
			ccode.add_assignment(data_var, new CCodeIdentifier("_user_data_"));
			ccode.add_assignment(CCodeMemberAccess.pointer(data_var, "_source_object_"), new CCodeIdentifier("source_object"));
			ccode.add_assignment(CCodeMemberAccess.pointer(data_var, "_res_"), new CCodeIdentifier("_res_"));

			if (!context.require_glib_version(2, 44)) {
				ccode.add_assignment(CCodeMemberAccess.pointer(data_var, "_task_complete_"), new CCodeConstant("TRUE"));
			}

			var ccall = new CCodeFunctionCall(new CCodeIdentifier(get_ccode_real_name(m) + "_co"));
			ccall.add_argument(data_var);
			ccode.add_expression(ccall);

			readyfunc.modifiers |= CCodeModifiers.STATIC;

			pop_function();

			cfile.add_function_declaration(readyfunc);
			cfile.add_function(readyfunc);

			return readyfunc.name;
		}

		public override void generate_virtual_method_declaration(Method m, CCodeFile decl_space, CCodeStruct type_struct) {
			if (!m.coroutine) {
				base.generate_virtual_method_declaration(m, decl_space, type_struct);
				return;
			}

			if (!m.is_abstract && !m.is_virtual) {
				return;
			}

			var creturn_type = m.return_type;
			if (m.return_type.is_real_non_null_struct_type()) {
				// structs are returned via out parameter
				creturn_type = new VoidType();
			}

			// add vfunc field to the type struct
			var vdeclarator = new CCodeFunctionDeclarator(get_ccode_vfunc_name(m));
			var cparam_map = new Dictionary<int, CCodeParameter>();

			generate_cparameters(m, decl_space, cparam_map, new CCodeFunction("fake"), vdeclarator, null, null, 1);

			var vdecl = new CCodeDeclaration("void");
			vdecl.add_declarator(vdeclarator);
			type_struct.add_declaration(vdecl);

			// add vfunc field to the type struct
			vdeclarator = new CCodeFunctionDeclarator(get_ccode_finish_vfunc_name(m));
			cparam_map = new Dictionary<int, CCodeParameter>();

			generate_cparameters(m, decl_space, cparam_map, new CCodeFunction("fake"), vdeclarator, null, null, 2);

			vdecl = new CCodeDeclaration(get_ccode_name(creturn_type));
			vdecl.add_declarator(vdeclarator);
			type_struct.add_declaration(vdecl);
		}

		public override void visit_yield_statement(YieldStatement stmt) {
			if (!is_in_coroutine()) {
				return;
			}

			if (stmt.yield_expression == null) {
				int state = emit_context.next_coroutine_state++;

				ccode.add_assignment(CCodeMemberAccess.pointer(new CCodeIdentifier("_data_"), "_state_"), new CCodeConstant(state.ToString()));
				ccode.add_return(new CCodeConstant("FALSE"));
				ccode.add_label("_state_%d".printf(state));
				ccode.add_statement(new CCodeEmptyStatement());

				return;
			}

			if (stmt.yield_expression.error) {
				stmt.error = true;
				return;
			}

			ccode.add_expression(get_cvalue(stmt.yield_expression));

			if (stmt.tree_can_fail && stmt.yield_expression.tree_can_fail) {
				// simple case, no node breakdown necessary

				add_simple_check(stmt.yield_expression);
			}

			/* free temporary objects */

			foreach (var value in temp_ref_values) {
				ccode.add_expression(destroy_value(value));
			}

			temp_ref_values.Clear();
		}

		public override void return_with_exception(CCodeExpression error_expr) {
			if (!is_in_coroutine()) {
				base.return_with_exception(error_expr);
				return;
			}

			var async_result_expr = CCodeMemberAccess.pointer(new CCodeIdentifier("_data_"), "_async_result");
			CCodeFunctionCall set_error = null;

			set_error = new CCodeFunctionCall(new CCodeIdentifier("g_task_return_error"));
			set_error.add_argument(async_result_expr);
			set_error.add_argument(error_expr);
			ccode.add_expression(set_error);

			append_local_free(current_symbol, false);

			// We already returned the error above, we must not return anything else here.
			var unref = new CCodeFunctionCall(new CCodeIdentifier("g_object_unref"));
			unref.add_argument(async_result_expr);
			ccode.add_expression(unref);

			ccode.add_return(new CCodeConstant("FALSE"));
		}

		public override void visit_return_statement(ReturnStatement stmt) {
			base.visit_return_statement(stmt);

			if (!is_in_coroutine()) {
				return;
			}

			complete_async();
		}

		public override void generate_cparameters(Method m, CCodeFile decl_space, Dictionary<int, CCodeParameter> cparam_map, CCodeFunction func, CCodeFunctionDeclarator vdeclarator = null, Dictionary<int, CCodeExpression>carg_map = null, CCodeFunctionCall vcall = null, int direction = 3) {
			if (m.coroutine) {
				decl_space.add_include("gio/gio.h");

				if (direction == 1) {
					cparam_map[get_param_pos(-1)] = new CCodeParameter("_callback_", "GAsyncReadyCallback");
					cparam_map[get_param_pos(-0.9)] = new CCodeParameter("_user_data_", "gpointer");
					if (carg_map != null) {
						carg_map[get_param_pos(-1)] = new CCodeIdentifier("_callback_");
						carg_map[get_param_pos(-0.9)] = new CCodeIdentifier("_user_data_");
					}
				} else if (direction == 2) {
					cparam_map[get_param_pos(0.1)] = new CCodeParameter("_res_", "GAsyncResult*");
					if (carg_map != null) {
						carg_map[get_param_pos(0.1)] = new CCodeIdentifier("_res_");
					}
				}
			}
			base.generate_cparameters(m, decl_space, cparam_map, func, vdeclarator, carg_map, vcall, direction);
		}

		public string generate_async_callback_wrapper() {
			string async_callback_wrapper_func = "_vala_g_async_ready_callback";

			if (!add_wrapper(async_callback_wrapper_func)) {
				return async_callback_wrapper_func;
			}

			var function = new CCodeFunction(async_callback_wrapper_func, "void");
			function.modifiers = CCodeModifiers.STATIC;

			function.add_parameter(new CCodeParameter("*source_object", "GObject"));
			function.add_parameter(new CCodeParameter("*res", "GAsyncResult"));
			function.add_parameter(new CCodeParameter("*user_data", "void"));

			push_function(function);

			var res_ref = new CCodeFunctionCall(new CCodeIdentifier("g_object_ref"));
			res_ref.add_argument(new CCodeIdentifier("res"));

			CCodeFunctionCall ccall = null;

			// store reference to async result of inner async function in out async result
			ccall = new CCodeFunctionCall(new CCodeIdentifier("g_task_return_pointer"));
			ccall.add_argument(new CCodeIdentifier("user_data"));
			ccall.add_argument(res_ref);
			ccall.add_argument(new CCodeIdentifier("g_object_unref"));
			ccode.add_expression(ccall);

			// free async result
			ccall = new CCodeFunctionCall(new CCodeIdentifier("g_object_unref"));
			ccall.add_argument(new CCodeIdentifier("user_data"));
			ccode.add_expression(ccall);

			pop_function();

			cfile.add_function_declaration(function);
			cfile.add_function(function);

			return async_callback_wrapper_func;
		}
	}

}
