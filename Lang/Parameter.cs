using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.CodeNodes;
using Vala.Lang.Expressions;
using Vala.Lang.Literals;
using Vala.Lang.Methods;
using Vala.Lang.Parser;
using Vala.Lang.Symbols;
using Vala.Lang.Types;

namespace Vala.Lang
{
	public class Parameter : Variable
	{
		public ParameterDirection direction { get; set; } = ParameterDirection.IN;

		/**
		 * Specifies whether the methods accepts an indefinite number of
		 * parameters.
		 */
		public bool ellipsis { get; set; }

		/**
		 * Specifies whether the methods accepts an indefinite number of
		 * parameters.
		 */
		public bool params_array { get; set; }

		public bool captured { get; set; }

		public bool format_arg {
			get {
				return get_attribute("FormatArg") != null;
			}
		}

		/**
		 * The base parameter of this parameter relative to the base method.
		 */
		public Parameter base_parameter { get; set; }

		/**
		 * Creates a new formal parameter.
		 *
		 * @param name              parameter name
		 * @param variable_type     parameter type
		 * @param source_reference  reference to source code
		 * @return                  newly created formal parameter
		 */
		public Parameter(string name, DataType variable_type, SourceReference source_reference = null)
			: base(variable_type, name, null, source_reference) {
			access = SymbolAccessibility.PUBLIC;
		}

		/**
		 * Creates a new ellipsis parameter representing an indefinite number of
		 * parameters.
		 */
		public static Parameter with_ellipsis(SourceReference source_reference = null) {
			Parameter param = new Parameter(null, null, source_reference);
			param.ellipsis = true;
			param.access = SymbolAccessibility.PUBLIC;
			return param;
		}

		public override void accept(CodeVisitor visitor) {
			visitor.visit_formal_parameter(this);
		}

		public override void accept_children(CodeVisitor visitor) {
			if (!ellipsis) {
				variable_type.accept(visitor);

				if (initializer != null) {
					initializer.accept(visitor);
				}
			}
		}

		public override void replace_type(DataType old_type, DataType new_type) {
			if (variable_type == old_type) {
				variable_type = new_type;
			}
		}

		public override void replace_expression(Expression old_node, Expression new_node) {
			if (initializer == old_node) {
				initializer = new_node;
			}
		}

		public Parameter copy() {
			if (!ellipsis) {
				var result = new Parameter(name, variable_type.copy(), source_reference);
				result.params_array = params_array;
				result.direction = this.direction;
				result.initializer = this.initializer;

				// cannot use List.copy()
				// as it returns a list of unowned elements
				foreach (ValaAttribute a in this.attributes) {
					result.attributes.Add(a);
				}

				return result;
			} else {
				return Parameter.with_ellipsis();
			}
		}

		public override bool check(CodeContext context) {
			if (is_checked) {
				return !error;
			}

			is_checked = true;

			var old_source_file = context.analyzer.current_source_file;
			var old_symbol = context.analyzer.current_symbol;

			if (source_reference != null) {
				context.analyzer.current_source_file = source_reference.file;
			}
			context.analyzer.current_symbol = parent_symbol;

			if (variable_type != null) {
				if (variable_type is VoidType) {
					error = true;
					Report.error(source_reference, "'void' not supported as parameter type");
					return false;
				}
				variable_type.check(context);
			}

			if (!ellipsis) {
				variable_type.check(context);

				if (params_array && !(variable_type is ArrayType)) {
					error = true;
					Report.error(source_reference, "parameter array expected");
					return false;
				}

				if (has_attribute_argument("CCode", "scope") && variable_type is DelegateType) {
					var delegate_type = (DelegateType)variable_type;
					delegate_type.is_called_once = get_attribute_string("CCode", "scope") == "async";
				}

				if (initializer != null) {
					initializer.target_type = variable_type.copy();
					initializer.check(context);
				}
			}

			if (initializer != null) {
				if (initializer is NullLiteral
					&& !variable_type.nullable
					&& direction != ParameterDirection.OUT) {
					Report.warning(source_reference, "`null' incompatible with parameter type `%s`".printf(variable_type.to_string()));
				} else if (!(initializer is NullLiteral) && direction == ParameterDirection.OUT) {
					Report.error(source_reference, "only `null' is allowed as default value for out parameters");
				} else if (direction == ParameterDirection.IN && !initializer.value_type.compatible(variable_type)) {
					Report.error(initializer.source_reference, "Cannot convert from `%s' to `%s'".printf(initializer.value_type.to_string(), variable_type.to_string()));
				} else if (direction == ParameterDirection.REF) {
					Report.error(source_reference, "default value not allowed for ref parameter");
				} else if (!initializer.is_accessible(this)) {
					Report.error(initializer.source_reference, "default value is less accessible than method `%s'".printf(parent_symbol.get_full_name()));
				}
			}

			if (!ellipsis) {
				// check whether parameter type is at least as accessible as the method
				if (!context.analyzer.is_type_accessible(this, variable_type)) {
					error = true;
					Report.error(source_reference, "parameter type `%s` is less accessible than method `%s`".printf(variable_type.to_string(), parent_symbol.get_full_name()));
				}
			}

			var m = parent_symbol as Method;
			if (m != null) {
				Method base_method = m.base_method != null ? m.base_method : m.base_interface_method;
				if (base_method != null && base_method != m) {
					int index = m.get_parameters().IndexOf(this);
					if (index >= 0) {
						base_parameter = base_method.get_parameters()[index];
					}
				}
			}

			context.analyzer.current_source_file = old_source_file;
			context.analyzer.current_symbol = old_symbol;

			return !error;
		}
	}

	public enum ParameterDirection
	{
		IN,
		OUT,
		REF
	}
}
