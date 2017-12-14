using CLanguage;
using CLanguage.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang;
using Vala.Lang.Expressions;
using Vala.Lang.Parser;
using Vala.Lang.Symbols;
using Vala.Lang.Types;
using Vala.Lang.TypeSymbols;
using static GLibPorts.GLib;

namespace CCodeGen.Modules {
	/// <summary>
	/// The link between an assignment and generated code.
	/// </summary>
	public class CCodeAssignmentModule : CCodeMemberAccessModule {
		TargetValue emit_simple_assignment(Assignment assignment) {
			Variable variable = (Variable)assignment.left.symbol_reference;

			if (requires_destroy(assignment.left.value_type)) {
				/* unref old value */
				ccode.add_expression(destroy_value(assignment.left.target_value));
			}

			if (assignment.Operator == AssignmentOperator.SIMPLE) {
				store_value(assignment.left.target_value, assignment.right.target_value, assignment.source_reference);
			} else {
				CCodeAssignmentOperator cop = (CCodeAssignmentOperator)0;
				if (assignment.Operator == AssignmentOperator.BITWISE_OR) {
					cop = CCodeAssignmentOperator.BITWISE_OR;
				} else if (assignment.Operator == AssignmentOperator.BITWISE_AND) {
					cop = CCodeAssignmentOperator.BITWISE_AND;
				} else if (assignment.Operator == AssignmentOperator.BITWISE_XOR) {
					cop = CCodeAssignmentOperator.BITWISE_XOR;
				} else if (assignment.Operator == AssignmentOperator.ADD) {
					cop = CCodeAssignmentOperator.ADD;
				} else if (assignment.Operator == AssignmentOperator.SUB) {
					cop = CCodeAssignmentOperator.SUB;
				} else if (assignment.Operator == AssignmentOperator.MUL) {
					cop = CCodeAssignmentOperator.MUL;
				} else if (assignment.Operator == AssignmentOperator.DIV) {
					cop = CCodeAssignmentOperator.DIV;
				} else if (assignment.Operator == AssignmentOperator.PERCENT) {
					cop = CCodeAssignmentOperator.PERCENT;
				} else if (assignment.Operator == AssignmentOperator.SHIFT_LEFT) {
					cop = CCodeAssignmentOperator.SHIFT_LEFT;
				} else if (assignment.Operator == AssignmentOperator.SHIFT_RIGHT) {
					cop = CCodeAssignmentOperator.SHIFT_RIGHT;
				} else {
					assert_not_reached();
				}

				CCodeExpression codenode = new CCodeAssignment(get_cvalue(assignment.left), get_cvalue(assignment.right), cop);
				ccode.add_expression(codenode);
			}

			if (assignment.left.value_type is ArrayType && (((ArrayType)assignment.left.value_type).inline_allocated)) {
				return load_variable(variable, assignment.left.target_value);
			} else {
				return store_temp_value(assignment.left.target_value, assignment);
			}
		}

		public override void visit_assignment(Assignment assignment) {
			if (assignment.left.error || assignment.right.error) {
				assignment.error = true;
				return;
			}

			if (assignment.left.symbol_reference is Property) {
				var ma = assignment.left as MemberAccess;
				var prop = (Property)assignment.left.symbol_reference;

				store_property(prop, ma.inner, assignment.right.target_value);
				assignment.target_value = assignment.right.target_value;
			} else if (assignment.left.symbol_reference is Variable && is_simple_struct_creation((Variable)assignment.left.symbol_reference, assignment.right)) {
				// delegate to visit_object_creation_expression
			} else {
				assignment.target_value = emit_simple_assignment(assignment);
			}
		}

		public override void store_value(TargetValue lvalue, TargetValue value, SourceReference source_reference = null) {
			var array_type = lvalue.value_type as ArrayType;

			if (array_type != null && array_type.fixed_length) {
				cfile.add_include("string.h");

				// it is necessary to use memcpy for fixed-length (stack-allocated) arrays
				// simple assignments do not work in C
				var sizeof_call = new CCodeFunctionCall(new CCodeIdentifier("sizeof"));
				sizeof_call.add_argument(new CCodeIdentifier(get_ccode_name(array_type.element_type)));
				var size = new CCodeBinaryExpression(CCodeBinaryOperator.MUL, get_ccodenode(array_type.length), sizeof_call);

				var ccopy = new CCodeFunctionCall(new CCodeIdentifier("memcpy"));
				ccopy.add_argument(get_cvalue_(lvalue));
				ccopy.add_argument(get_cvalue_(value));
				ccopy.add_argument(size);
				ccode.add_expression(ccopy);

				return;
			}

			var cexpr = get_cvalue_(value);
			if (get_ctype(lvalue) != null) {
				cexpr = new CCodeCastExpression(cexpr, get_ctype(lvalue));
			}

			/*
			 * If this is a SimpleType struct being passed by value
			 * and the user specified a custom user function
			 * use the specified function instead to do assignments rather than dest = source
			 */
			var st_left = lvalue.value_type.data_type as Struct;
			var st_right = value.value_type.data_type as Struct;
			if (
				st_left != null &&
				st_right != null &&
				lvalue.value_type.compatible(value.value_type) &&
				st_left.is_simple_type() &&
				lvalue.value_type is StructValueType &&
				st_left.has_attribute_argument("CCode", "copy_function")
			) {
				//copy_function (src, dest)
				var copy_call = new CCodeFunctionCall(new CCodeIdentifier(get_ccode_copy_function(st_left)));
				copy_call.add_argument(get_cvalue_(value));
				copy_call.add_argument(get_cvalue_(lvalue));
				ccode.add_expression(copy_call);
				return;
			}

			ccode.add_assignment(get_cvalue_(lvalue), cexpr);

			if (array_type != null && ((GLibValue)lvalue).array_length_cvalues != null) {
				var glib_value = (GLibValue)value;
				if (glib_value.array_length_cvalues != null) {
					for (int dim = 1; dim <= array_type.rank; dim++) {
						ccode.add_assignment(get_array_length_cvalue(lvalue, dim), get_array_length_cvalue(value, dim));
					}
				} else if (glib_value.array_null_terminated) {
					requires_array_length = true;
					var len_call = new CCodeFunctionCall(new CCodeIdentifier("_vala_array_length"));
					len_call.add_argument(get_cvalue_(value));

					ccode.add_assignment(get_array_length_cvalue(lvalue, 1), len_call);
				} else {
					for (int dim = 1; dim <= array_type.rank; dim++) {
						ccode.add_assignment(get_array_length_cvalue(lvalue, dim), new CCodeConstant("-1"));
					}
				}

				if (array_type.rank == 1 && get_array_size_cvalue(lvalue) != null) {
					ccode.add_assignment(get_array_size_cvalue(lvalue), get_array_length_cvalue(lvalue, 1));
				}
			}

			var delegate_type = lvalue.value_type as DelegateType;
			if (delegate_type != null && delegate_type.delegate_symbol.has_target) {
				var lvalue_target = get_delegate_target_cvalue(lvalue);
				var rvalue_target = get_delegate_target_cvalue(value);
				if (lvalue_target != null) {
					if (rvalue_target != null) {
						ccode.add_assignment(lvalue_target, rvalue_target);
					} else {
						Report.error(source_reference, "Assigning delegate without required target in scope");
						ccode.add_assignment(lvalue_target, new CCodeInvalidExpression());
					}
					var lvalue_destroy_notify = get_delegate_target_destroy_notify_cvalue(lvalue);
					var rvalue_destroy_notify = get_delegate_target_destroy_notify_cvalue(value);
					if (lvalue_destroy_notify != null) {
						if (rvalue_destroy_notify != null) {
							ccode.add_assignment(lvalue_destroy_notify, rvalue_destroy_notify);
						} else {
							ccode.add_assignment(lvalue_destroy_notify, new CCodeConstant("NULL"));
						}
					}
				}
			}
		}

		public override void store_local(LocalVariable local, TargetValue value, bool initializer, SourceReference source_reference = null) {
			if (!initializer && requires_destroy(local.variable_type)) {
				/* unref old value */
				ccode.add_expression(destroy_local(local));
			}

			store_value(get_local_cvalue(local), value, source_reference);
		}

		public override void store_parameter(Parameter param, TargetValue _value, bool capturing_parameter = false, SourceReference source_reference = null) {
			var value = _value;

			bool capturing_parameter_in_coroutine = capturing_parameter && is_in_coroutine();

			var param_type = param.variable_type.copy();
			if (param.captured || is_in_coroutine()) {
				if (!param_type.value_owned && !no_implicit_copy(param_type)) {
					// parameter value has been implicitly copied into a heap data structure
					// treat parameter as owned
					param_type.value_owned = true;

					var old_coroutine = is_in_coroutine();
					if (old_coroutine) {
						current_method.coroutine = false;
					}

					if (requires_copy(param_type) && !capturing_parameter_in_coroutine) {
						// do not copy value when capturing parameter in coroutine
						// as the value was already copied on coroutine initialization
						value = copy_value(value, param);
					}

					if (old_coroutine) {
						current_method.coroutine = true;
					}
				}
			}

			if (requires_destroy(param_type)) {
				/* unref old value */
				ccode.add_expression(destroy_parameter(param));
			}

			store_value(get_parameter_cvalue(param), value, source_reference);
		}

		public override void store_field(Field field, TargetValue instance, TargetValue value, SourceReference source_reference = null) {
			var lvalue = get_field_cvalue(field, instance);
			var type = lvalue.value_type;
			if (lvalue.actual_value_type != null) {
				type = lvalue.actual_value_type;
			}
			if (requires_destroy(type)) {
				/* unref old value */
				ccode.add_expression(destroy_field(field, instance));
			}

			store_value(lvalue, value, source_reference);
		}
	}
}
