﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.CodeNodes;
using Vala.Lang.Expressions;
using Vala.Lang.Literals;
using Vala.Lang.Parser;
using Vala.Lang.Symbols;
using Vala.Lang.Types;
using Vala.Lang.TypeSymbols;

namespace Vala.Lang.Methods {
	public class Method : Subroutine, Callable {
		List<TypeParameter> type_parameters;

		/**
		 * The return type of this method.
		 */
		public DataType return_type {
			get { return _return_type; }
			set {
				_return_type = value;
				_return_type.parent_node = this;
			}
		}

		public override bool has_result {
			get { return !(return_type is VoidType); }
		}

		/**
		 * Specifies whether this method may only be called with an instance of
		 * the contained type.
		 */
		public MemberBinding binding { get; set; } = MemberBinding.INSTANCE;

		/**
		 * Specifies whether this method is abstract. Abstract methods have no
		 * body, may only be specified within abstract classes, and must be
		 * overriden by derived non-abstract classes.
		 */
		public bool is_abstract { get; set; }

		/**
		 * Specifies whether this method is virtual. Virtual methods may be
		 * overridden by derived classes.
		 */
		public bool is_virtual { get; set; }

		/**
		 * Specifies whether this method overrides a virtual or abstract method
		 * of a base type.
		 */
		public bool overrides { get; set; }

		/**
		 * Specifies whether this method should be inlined.
		 */
		public bool is_inline { get; set; }

		public bool returns_floating_reference {
			get {
				return get_attribute_bool("CCode", "returns_floating_reference");
			}
			set {
				set_attribute_bool("CCode", "returns_floating_reference", value);
			}
		}

		/*
		 * Specifies whether the C method returns a new instance pointer which
		 * may be different from the previous instance pointer. Only valid for
		 * imported methods.
		 */
		public bool returns_modified_pointer {
			get {
				return get_attribute("ReturnsModifiedPointer") != null;
			}
			set {
				set_attribute("ReturnsModifiedPointer", value);
			}
		}

		/**
		 * Specifies the virtual or abstract method this method overrides.
		 * Reference must be weak as virtual and abstract methods set 
		 * base_method to themselves.
		 */
		public Method base_method {
			get {
				find_base_methods();
				return _base_method;
			}
		}

		/**
		 * Specifies the abstract interface method this method implements.
		 */
		public Method base_interface_method {
			get {
				find_base_methods();
				return _base_interface_method;
			}
		}

		/**
		 * Specifies the explicit interface containing the method this method implements.
		 */
		public DataType base_interface_type {
			get { return _base_interface_type; }
			set {
				_base_interface_type = value;
				_base_interface_type.parent_node = this;
			}
		}

		public bool entry_point { get; private set; }

		/**
		 * Specifies the generated `this` parameter for instance methods.
		 */
		public Parameter this_parameter { get; set; }

		/**
		 * Specifies whether this method expects printf-style format arguments.
		 */
		public bool printf_format {
			get {
				return get_attribute("PrintfFormat") != null;
			}
			set {
				set_attribute("PrintfFormat", value);
			}
		}

		/**
		 * Specifies whether this method expects scanf-style format arguments.
		 */
		public bool scanf_format {
			get {
				return get_attribute("ScanfFormat") != null;
			}
			set {
				set_attribute("ScanfFormat", value);
			}
		}

		/**
		 * Specifies whether a construct function with a GType parameter is
		 * available. This is only applicable to creation methods.
		 */
		public bool has_construct_function {
			get {
				return get_attribute_bool("CCode", "has_construct_function", true);
			}
			set {
				set_attribute_bool("CCode", "has_construct_function", value);
			}
		}

		private WeakReference<Signal> signal_reference_weak = new WeakReference<Signal>(null);

		public Signal signal_reference {
			get {
				return signal_reference_weak.GetTarget();
			}
			set {
				signal_reference_weak.SetTarget(value);
			}
		}

		public bool closure { get; set; }

		public bool coroutine { get; set; }

		public bool is_async_callback { get; set; }

		private List<Parameter> parameters = new List<Parameter>();
		private List<Expression> preconditions;
		private List<Expression> postconditions;
		private DataType _return_type;

		private WeakReference<Method> _base_method_weak = new WeakReference<Method>(null);
		private WeakReference<Method> _base_interface_method_weak = new WeakReference<Method>(null);

		private Method _base_method {
			get {
				return _base_method_weak.GetTarget();
			}
			set {
				_base_method_weak.SetTarget(value);
			}
		}

		private Method _base_interface_method {
			get {
				return _base_interface_method_weak.GetTarget();
			}
			set {
				_base_interface_method_weak.SetTarget(value);
			}
		}
		private DataType _base_interface_type;
		private bool base_methods_valid;

		Method callback_method;
		Method end_method;

		// only valid for closures
		List<LocalVariable> captured_variables;

		static List<Expression> _empty_expression_list;
		static List<TypeParameter> _empty_type_parameter_list;

		/**
		 * Creates a new method.
		 *
		 * @param name              method name
		 * @param return_type       method return type
		 * @param source_reference  reference to source code
		 * @return                  newly created method
		 */
		public Method(string name, DataType return_type, SourceReference source_reference = null, Comment comment = null)
			: base(name, source_reference, comment) {
			this.return_type = return_type;
		}

		/**
		 * Appends parameter to this method.
		 *
		 * @param param a formal parameter
		 */
		public void add_parameter(Parameter param) {
			// default C parameter position
			parameters.Add(param);
			scope.add(param.name, param);
		}

		public List<Parameter> get_parameters() {
			return parameters;
		}

		/**
		 * Remove all parameters from this method.
		 */
		public void clear_parameters() {
			foreach (Parameter param in parameters) {
				if (!param.ellipsis) {
					scope.remove(param.name);
				}
			}
			parameters.Clear();
		}

		public bool is_variadic() {
			foreach (Parameter param in parameters) {
				if (param.ellipsis) {
					return true;
				}
			}
			return false;
		}

		public override void accept(CodeVisitor visitor) {
			visitor.visit_method(this);
		}

		public override void accept_children(CodeVisitor visitor) {
			foreach (TypeParameter p in get_type_parameters()) {
				p.accept(visitor);
			}

			if (base_interface_type != null) {
				base_interface_type.accept(visitor);
			}

			if (return_type != null) {
				return_type.accept(visitor);
			}

			foreach (Parameter param in parameters) {
				param.accept(visitor);
			}

			foreach (DataType error_type in get_error_types().ToList()) {
				error_type.accept(visitor);
			}

			if (result_var != null) {
				result_var.accept(visitor);
			}

			if (preconditions != null) {
				foreach (Expression precondition in preconditions) {
					precondition.accept(visitor);
				}
			}

			if (postconditions != null) {
				foreach (Expression postcondition in postconditions) {
					postcondition.accept(visitor);
				}
			}

			if (body != null) {
				body.accept(visitor);
			}
		}

		/**
		 * Checks whether the parameters and return type of this method are
		 * compatible with the specified method
		 *
		 * @param base_method a method
		 * @param invalid_match error string about which check failed
		 * @return true if the specified method is compatible to this method
		 */
		public bool compatible(Method base_method, out string invalid_match) {
			// method is always compatible to itself
			if (this == base_method) {
				invalid_match = null;
				return true;
			}

			if (binding != base_method.binding) {
				invalid_match = "incompatible binding";
				return false;
			}

			ObjectType object_type = null;
			if (parent_symbol is ObjectTypeSymbol) {
				object_type = new ObjectType((ObjectTypeSymbol)parent_symbol);
				foreach (TypeParameter type_parameter in object_type.type_symbol.get_type_parameters()) {
					var type_arg = new GenericType(type_parameter);
					type_arg.value_owned = true;
					object_type.add_type_argument(type_arg);
				}
			}

			if (this.get_type_parameters().Count < base_method.get_type_parameters().Count) {
				invalid_match = "too few type parameters";
				return false;
			} else if (this.get_type_parameters().Count > base_method.get_type_parameters().Count) {
				invalid_match = "too many type parameters";
				return false;
			}

			List<DataType> method_type_args = null;
			if (this.get_type_parameters().Count > 0) {
				method_type_args = new List<DataType>();
				foreach (TypeParameter type_parameter in this.get_type_parameters()) {
					var type_arg = new GenericType(type_parameter);
					type_arg.value_owned = true;
					method_type_args.Add(type_arg);
				}
			}

			var actual_base_type = base_method.return_type.get_actual_type(object_type, method_type_args, this);
			if (!return_type.equals(actual_base_type)) {
				invalid_match = "Base method expected return type `%s', but `%s' was provided".printf(actual_base_type.to_qualified_string(), return_type.to_qualified_string());
				return false;
			}

			IEnumerator<Parameter> method_params_it = parameters.GetEnumerator();
			int param_index = 1;
			foreach (Parameter base_param in base_method.parameters) {
				/* this method may not expect less arguments */
				if (!method_params_it.MoveNext()) {
					invalid_match = "too few parameters";
					return false;
				}

				var param = method_params_it.Current;
				if (base_param.ellipsis != param.ellipsis) {
					invalid_match = "ellipsis parameter mismatch";
					return false;
				}
				if (!base_param.ellipsis) {
					if (base_param.direction != param.direction) {
						invalid_match = "incompatible direction of parameter %d".printf(param_index);
						return false;
					}

					actual_base_type = base_param.variable_type.get_actual_type(object_type, method_type_args, this);
					if (!actual_base_type.equals(param.variable_type)) {
						invalid_match = "incompatible type of parameter %d".printf(param_index);
						return false;
					}
				}
				param_index++;
			}

			/* this method may not expect more arguments */
			if (method_params_it.MoveNext()) {
				invalid_match = "too many parameters";
				return false;
			}

			/* this method may throw less but not more errors than the base method */
			foreach (DataType method_error_type in get_error_types()) {
				bool match = false;
				foreach (DataType base_method_error_type in base_method.get_error_types()) {
					if (method_error_type.compatible(base_method_error_type)) {
						match = true;
						break;
					}
				}

				if (!match) {
					invalid_match = "incompatible error type `%s'".printf(method_error_type.to_string());
					return false;
				}
			}
			if (base_method.coroutine != this.coroutine) {
				invalid_match = "async mismatch";
				return false;
			}

			invalid_match = null;
			return true;
		}

		/**
		 * Appends the specified parameter to the list of type parameters.
		 *
		 * @param p a type parameter
		 */
		public void add_type_parameter(TypeParameter p) {
			if (type_parameters == null) {
				type_parameters = new List<TypeParameter>();
			}
			type_parameters.Add(p);
			scope.add(p.name, p);
		}

		/**
		 * Returns a copy of the type parameter list.
		 *
		 * @return list of type parameters
		 */
		public List<TypeParameter> get_type_parameters() {
			if (type_parameters != null) {
				return type_parameters;
			}
			if (_empty_type_parameter_list == null) {
				_empty_type_parameter_list = new List<TypeParameter>();
			}
			return _empty_type_parameter_list;
		}

		public int get_type_parameter_index(string name) {
			if (type_parameters == null) {
				return -1;
			}

			int i = 0;
			foreach (TypeParameter parameter in type_parameters) {
				if (parameter.name == name) {
					return i;
				}
				i++;
			}
			return -1;
		}

		/**
		 * Adds a precondition to this method.
		 *
		 * @param precondition a boolean precondition expression
		 */
		public void add_precondition(Expression precondition) {
			if (preconditions == null) {
				preconditions = new List<Expression>();
			}
			preconditions.Add(precondition);
			precondition.parent_node = this;
		}

		/**
		 * Returns a copy of the list of preconditions of this method.
		 *
		 * @return list of preconditions
		 */
		public List<Expression> get_preconditions() {
			if (preconditions != null) {
				return preconditions;
			}
			if (_empty_expression_list == null) {
				_empty_expression_list = new List<Expression>();
			}
			return _empty_expression_list;
		}

		/**
		 * Adds a postcondition to this method.
		 *
		 * @param postcondition a boolean postcondition expression
		 */
		public void add_postcondition(Expression postcondition) {
			if (postconditions == null) {
				postconditions = new List<Expression>();
			}
			postconditions.Add(postcondition);
			postcondition.parent_node = this;
		}

		/**
		 * Returns a copy of the list of postconditions of this method.
		 *
		 * @return list of postconditions
		 */
		public List<Expression> get_postconditions() {
			if (postconditions != null) {
				return postconditions;
			}
			if (_empty_expression_list == null) {
				_empty_expression_list = new List<Expression>();
			}
			return _empty_expression_list;
		}

		public override void replace_type(DataType old_type, DataType new_type) {
			if (base_interface_type == old_type) {
				base_interface_type = new_type;
				return;
			}
			if (return_type == old_type) {
				return_type = new_type;
				return;
			}
			var error_types = get_error_types();
			for (int i = 0; i < error_types.Count; i++) {
				if (error_types[i] == old_type) {
					error_types[i] = new_type;
					return;
				}
			}
		}

		private void find_base_methods() {
			if (base_methods_valid) {
				return;
			}

			if (parent_symbol is Class) {
				if (!(this is CreationMethod)) {
					find_base_interface_method((Class)parent_symbol);
					if (is_virtual || is_abstract || overrides) {
						find_base_class_method((Class)parent_symbol);
					}
				}
			} else if (parent_symbol is Interface) {
				if (is_virtual || is_abstract) {
					_base_interface_method = this;
				}
			}

			base_methods_valid = true;
		}

		private void find_base_class_method(Class cl) {
			var sym = cl.scope.lookup(name);
			if (sym is Signal) {
				var sig = (Signal)sym;
				sym = sig.default_handler;
			}
			if (sym is Method) {
				var base_method = (Method)sym;
				if (base_method.is_abstract || base_method.is_virtual) {
					string invalid_match;
					if (!compatible(base_method, out invalid_match)) {
						error = true;
						Report.error(source_reference, "overriding method `%s' is incompatible with base method `%s': %s.".printf(get_full_name(), base_method.get_full_name(), invalid_match));
						return;
					}

					_base_method = base_method;
					return;
				}
			}

			if (cl.base_class != null) {
				find_base_class_method(cl.base_class);
			}
		}

		private void find_base_interface_method(Class cl) {
			foreach (DataType type in cl.get_base_types()) {
				if (type.data_type is Interface) {
					if (base_interface_type != null && base_interface_type.data_type != type.data_type) {
						continue;
					}

					var sym = type.data_type.scope.lookup(name);
					if (sym is Signal) {
						var sig = (Signal)sym;
						sym = sig.default_handler;
					}
					if (sym is Method) {
						var base_method = (Method)sym;
						if (base_method.is_abstract || base_method.is_virtual) {
							if (base_interface_type == null) {
								// check for existing explicit implementation
								var has_explicit_implementation = false;
								foreach (var m in cl.get_methods()) {
									if (m.base_interface_type != null && base_method == m.base_interface_method) {
										has_explicit_implementation = true;
										break;
									}
								}
								if (has_explicit_implementation) {
									continue;
								}
							}

							string invalid_match = null;
							if (!compatible(base_method, out invalid_match)) {
								error = true;
								Report.error(source_reference, "overriding method `%s' is incompatible with base method `%s': %s.".printf(get_full_name(), base_method.get_full_name(), invalid_match));
								return;
							}

							_base_interface_method = base_method;
							return;
						}
					}
				}
			}

			if (base_interface_type != null) {
				Report.error(source_reference, "%s: no suitable interface method found to implement".printf(get_full_name()));
			}
		}

		public override bool check(CodeContext context) {
			if (is_checked) {
				return !error;
			}

			is_checked = true;

			if (get_attribute("DestroysInstance") != null) {
				this_parameter.variable_type.value_owned = true;
			}
			if (get_attribute("NoThrow") != null) {
				get_error_types().Clear();
			}

			if (is_abstract) {
				if (parent_symbol is Class) {
					var cl = (Class)parent_symbol;
					if (!cl.is_abstract) {
						error = true;
						Report.error(source_reference, "Abstract methods may not be declared in non-abstract classes");
						return false;
					}
				} else if (!(parent_symbol is Interface)) {
					error = true;
					Report.error(source_reference, "Abstract methods may not be declared outside of classes and interfaces");
					return false;
				}
			} else if (is_virtual) {
				if (!(parent_symbol is Class) && !(parent_symbol is Interface)) {
					error = true;
					Report.error(source_reference, "Virtual methods may not be declared outside of classes and interfaces");
					return false;
				}

				if (parent_symbol is Class) {
					var cl = (Class)parent_symbol;
					if (cl.is_compact && cl != context.analyzer.gsource_type) {
						Report.error(source_reference, "Virtual methods may not be declared in compact classes");
						return false;
					}
				}
			} else if (overrides) {
				if (!(parent_symbol is Class)) {
					error = true;
					Report.error(source_reference, "Methods may not be overridden outside of classes");
					return false;
				}
			} else if (access == SymbolAccessibility.PROTECTED) {
				if (!(parent_symbol is Class) && !(parent_symbol is Interface)) {
					error = true;
					Report.error(source_reference, "Protected methods may not be declared outside of classes and interfaces");
					return false;
				}
			}

			if (is_abstract && body != null) {
				Report.error(source_reference, "Abstract methods cannot have bodies");
			} else if ((is_abstract || is_virtual) && external && !external_package && !parent_symbol.external) {
				Report.error(source_reference, "Extern methods cannot be abstract or virtual");
			} else if (external && body != null) {
				Report.error(source_reference, "Extern methods cannot have bodies");
			} else if (!is_abstract && !external && source_type == SourceFileType.SOURCE && body == null) {
				Report.error(source_reference, "Non-abstract, non-extern methods must have bodies");
			}

			if (coroutine && !external_package && !context.has_package("gio-2.0")) {
				error = true;
				Report.error(source_reference, "gio-2.0 package required for async methods");
				return false;
			}

			var old_source_file = context.analyzer.current_source_file;
			var old_symbol = context.analyzer.current_symbol;

			if (source_reference != null) {
				context.analyzer.current_source_file = source_reference.file;
			}
			context.analyzer.current_symbol = this;

			return_type.check(context);

			var init_attr = get_attribute("ModuleInit");
			if (init_attr != null) {
				source_reference.file.context.module_init_method = this;
			}

			if (return_type != null) {
				return_type.check(context);
			}

			if (parameters.Count == 1 && parameters[0].ellipsis && body != null && binding != MemberBinding.INSTANCE) {
				// accept just `...' for external methods and instance methods
				error = true;
				Report.error(parameters[0].source_reference, "Named parameter required before `...'");
			}

			if (!coroutine) {
				// TODO: begin and end parameters must be checked separately for coroutines
				var optional_param = false;
				foreach (Parameter parameter in parameters) {
					parameter.check(context);
					if (coroutine && parameter.direction == ParameterDirection.REF) {
						error = true;
						Report.error(parameter.source_reference, "Reference parameters are not supported for async methods");
					}
					if (optional_param && parameter.initializer == null && !parameter.ellipsis) {
						Report.warning(parameter.source_reference, "parameter without default follows parameter with default");
					} else if (parameter.initializer != null) {
						optional_param = true;
					}
				}
			}

			foreach (DataType error_type in get_error_types()) {
				error_type.check(context);

				// check whether error type is at least as accessible as the method
				if (!context.analyzer.is_type_accessible(this, error_type)) {
					error = true;
					Report.error(source_reference, "error type `%s` is less accessible than method `%s`".printf(error_type.to_string(), get_full_name()));
					return false;
				}
			}

			if (result_var != null) {
				result_var.check(context);
			}

			if (preconditions != null) {
				foreach (Expression precondition in preconditions) {
					precondition.check(context);
				}
			}

			if (postconditions != null) {
				foreach (Expression postcondition in postconditions) {
					postcondition.check(context);
				}
			}

			if (body != null) {
				body.check(context);
			}

			if (context.analyzer.current_struct != null) {
				if (is_abstract || is_virtual || overrides) {
					error = true;
					Report.error(source_reference, "A struct member `%s' cannot be marked as override, virtual, or abstract".printf(get_full_name()));
					return false;
				}
			} else if (overrides && base_method == null) {
				Report.error(source_reference, "%s: no suitable method found to override".printf(get_full_name()));
			} else if ((is_abstract || is_virtual || overrides) && access == SymbolAccessibility.PRIVATE) {
				error = true;
				Report.error(source_reference, "Private member `%s' cannot be marked as override, virtual, or abstract".printf(get_full_name()));
				return false;
			}

			if (base_interface_type != null && base_interface_method != null && parent_symbol is Class) {
				var cl = (Class)parent_symbol;
				foreach (var m in cl.get_methods()) {
					if (m != this && m.base_interface_method == base_interface_method) {
						m.is_checked = true;
						m.error = true;
						error = true;
						Report.error(source_reference, "`%s' already contains an implementation for `%s'".printf(cl.get_full_name(), base_interface_method.get_full_name()));
						Report.notice(m.source_reference, "previous implementation of `%s' was here".printf(base_interface_method.get_full_name()));
						return false;
					}
				}
			}

			context.analyzer.current_source_file = old_source_file;
			context.analyzer.current_symbol = old_symbol;

			if (!external_package && !overrides && !hides && get_hidden_member() != null) {
				Report.warning(source_reference, "%s hides inherited method `%s'. Use the `new' keyword if hiding was intentional".printf(get_full_name(), get_hidden_member().get_full_name()));
			}

			// check whether return type is at least as accessible as the method
			if (!context.analyzer.is_type_accessible(this, return_type)) {
				error = true;
				Report.error(source_reference, "return type `%s` is less accessible than method `%s`".printf(return_type.to_string(), get_full_name()));
				return false;
			}

			foreach (Expression precondition in get_preconditions()) {
				if (precondition.error) {
					// if there was an error in the precondition, skip this check
					error = true;
					return false;
				}

				if (!precondition.value_type.compatible(context.analyzer.bool_type)) {
					error = true;
					Report.error(precondition.source_reference, "Precondition must be boolean");
					return false;
				}
			}

			foreach (Expression postcondition in get_postconditions()) {
				if (postcondition.error) {
					// if there was an error in the postcondition, skip this check
					error = true;
					return false;
				}

				if (!postcondition.value_type.compatible(context.analyzer.bool_type)) {
					error = true;
					Report.error(postcondition.source_reference, "Postcondition must be boolean");
					return false;
				}
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
					bool is_dynamic_error = body_error_type is ErrorType && ((ErrorType)body_error_type).dynamic_error;
					if (!can_propagate_error && !is_dynamic_error) {
						Report.warning(body_error_type.source_reference, "unhandled error `%s'".printf(body_error_type.to_string()));
					}
				}
			}

			if (is_possible_entry_point(context)) {
				if (context.entry_point != null) {
					error = true;
					Report.error(source_reference, "program already has an entry point `%s'".printf(context.entry_point.get_full_name()));
					return false;
				}
				entry_point = true;
				context.entry_point = this;

				if (tree_can_fail) {
					Report.error(source_reference, "\"main\" method cannot throw errors");
				}

				if (is_inline) {
					Report.error(source_reference, "\"main\" method cannot be inline");
				}

				if (coroutine) {
					Report.error(source_reference, "\"main\" method cannot be async");
				}
			}

			if (get_attribute("GtkCallback") != null) {
				used = true;
			}

			return !error;
		}

		bool is_possible_entry_point(CodeContext context) {
			if (external_package) {
				return false;
			}

			if (context.entry_point_name == null) {
				if (name == null || name != "main") {
					// method must be called "main"
					return false;
				}
			} else {
				// custom entry point name
				if (get_full_name() != context.entry_point_name) {
					return false;
				}
			}

			if (binding == MemberBinding.INSTANCE) {
				// method must be static
				return false;
			}

			if (return_type is VoidType) {
			} else if (return_type.data_type == context.analyzer.int_type.data_type) {
			} else {
				// return type must be void or int
				return false;
			}

			var _params = get_parameters();
			if (_params.Count == 0) {
				// method may have no parameters
				return true;
			}

			if (_params.Count > 1) {
				// method must not have more than one parameter
				return false;
			}

			IEnumerator<Parameter> params_it = _params.GetEnumerator();
			params_it.MoveNext();
			var param = params_it.Current;

			if (param.direction == ParameterDirection.OUT) {
				// parameter must not be an out parameter
				return false;
			}

			if (!(param.variable_type is ArrayType)) {
				// parameter must be an array
				return false;
			}

			var array_type = (ArrayType)param.variable_type;
			if (array_type.element_type.data_type != context.analyzer.string_type.data_type) {
				// parameter must be an array of strings
				return false;
			}

			return true;
		}

		public int get_required_arguments() {
			int n = 0;
			foreach (var param in parameters) {
				if (param.initializer != null || param.ellipsis) {
					// optional argument
					break;
				}
				n++;
			}
			return n;
		}

		public Method get_end_method() {
			Debug.Assert(this.coroutine);

			if (end_method == null) {
				end_method = new Method("end", return_type, source_reference);
				end_method.access = SymbolAccessibility.PUBLIC;
				end_method.external = true;
				end_method.owner = scope;
				foreach (var param in get_async_end_parameters()) {
					end_method.add_parameter(param.copy());
				}
				foreach (var param in get_type_parameters()) {
					end_method.add_type_parameter(param);
				}
			}
			return end_method;
		}

		public Method get_callback_method() {
			Debug.Assert(this.coroutine);

			if (callback_method == null) {
				var bool_type = new BooleanType((Struct)CodeContext.get().root.scope.lookup("bool"));
				bool_type.value_owned = true;
				callback_method = new Method("callback", bool_type, source_reference);
				callback_method.access = SymbolAccessibility.PUBLIC;
				callback_method.external = true;
				callback_method.binding = MemberBinding.INSTANCE;
				callback_method.owner = scope;
				callback_method.is_async_callback = true;
			}
			return callback_method;
		}

		public List<Parameter> get_async_begin_parameters() {
			Debug.Assert(this.coroutine);

			var glib_ns = CodeContext.get().root.scope.lookup("GLib");

			var _params = new List<Parameter>();
			Parameter ellipsis = null;
			foreach (var param in parameters) {
				if (param.ellipsis) {
					ellipsis = param;
				} else if (param.direction == ParameterDirection.IN) {
					_params.Add(param);
				}
			}

			var callback_type = new DelegateType((ValaDelegate)glib_ns.scope.lookup("AsyncReadyCallback"));
			callback_type.nullable = true;
			callback_type.value_owned = true;
			callback_type.is_called_once = true;

			var callback_param = new Parameter("_callback_", callback_type);
			callback_param.initializer = new NullLiteral(source_reference);
			callback_param.initializer.target_type = callback_type.copy();
			callback_param.set_attribute_double("CCode", "pos", -1);
			callback_param.set_attribute_double("CCode", "delegate_target_pos", -0.9);

			_params.Add(callback_param);

			if (ellipsis != null) {
				_params.Add(ellipsis);
			}

			return _params;
		}

		public List<Parameter> get_async_end_parameters() {
			Debug.Assert(this.coroutine);

			var _params = new List<Parameter>();

			var glib_ns = CodeContext.get().root.scope.lookup("GLib");
			var result_type = new ObjectType((ObjectTypeSymbol)glib_ns.scope.lookup("AsyncResult"));

			var result_param = new Parameter("_res_", result_type);
			result_param.set_attribute_double("CCode", "pos", 0.1);
			_params.Add(result_param);

			foreach (var param in parameters) {
				if (param.direction == ParameterDirection.OUT) {
					_params.Add(param);
				}
			}

			return _params;
		}

		public void add_captured_variable(LocalVariable local) {
			Debug.Assert(this.closure);

			if (captured_variables == null) {
				captured_variables = new List<LocalVariable>();
			}
			captured_variables.Add(local);
		}

		public void get_captured_variables(ICollection<LocalVariable> variables) {
			if (captured_variables != null) {
				foreach (var local in captured_variables) {
					variables.Add(local);
				}
			}
		}

		public override void get_defined_variables(ICollection<Variable> collection) {
			// capturing variables is only supported if they are initialized
			// therefore assume that captured variables are initialized
			if (closure) {
				Debug.Assert(collection.Count <= 0);
				ICollection<LocalVariable> localVariables = new List<LocalVariable>();
				get_captured_variables(localVariables);

				foreach (LocalVariable v in localVariables) {
					collection.Add(v);
				}
			}
		}

		public int get_format_arg_index() {
			for (int i = 0; i < parameters.Count; i++) {
				if (parameters[i].format_arg) {
					return i;
				}
			}
			return -1;
		}
	}
}
