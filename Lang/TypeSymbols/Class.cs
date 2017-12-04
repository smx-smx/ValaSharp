﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.CodeNodes;
using Vala.Lang.Methods;
using Vala.Lang.Parser;
using Vala.Lang.Symbols;
using Vala.Lang.Types;

namespace Vala.Lang.TypeSymbols {
	public class Class : ObjectTypeSymbol {
		/**
	 * Specifies the base class.
	 */
		public Class base_class { get; set; }

		/**
		 * Specifies whether this class is abstract. Abstract classes may not be
		 * instantiated.
		 */
		public bool is_abstract { get; set; }

		/**
		 * Instances of compact classes are fast to create and have a
		 * compact memory layout. Compact classes don't support runtime
		 * type information or virtual methods.
		 */
		public bool is_compact {
			get {
				if (_is_compact == null) {
					if (base_class != null) {
						_is_compact = base_class.is_compact;
					} else {
						_is_compact = get_attribute("Compact") != null;
					}
				}
				if (_is_compact == null) {
					_is_compact = get_attribute("Compact") != null;
				}
				return _is_compact.Value;
			}
			set {
				_is_compact = value;
				set_attribute("Compact", value);
			}
		}

		/**
		 * Instances of immutable classes are immutable after construction.
		 */
		public bool is_immutable {
			get {
				if (_is_immutable == null) {
					if (base_class != null) {
						_is_immutable = base_class.is_immutable;
					} else {
						_is_immutable = get_attribute("Immutable") != null;
					}
				}
				if (_is_immutable == null) {
					_is_immutable = get_attribute("Immutable") != null;
				}
				return _is_immutable.Value;
			}
			set {
				_is_immutable = value;
				set_attribute("Immutable", value);
			}
		}

		/**
		 * Specifies whether this class has private fields.
		 */
		public bool has_private_fields { get; set; }

		/**
		 * Specifies whether this class has class fields.
		 */
		public bool has_class_private_fields { get; private set; }

		private bool? _is_compact;
		private bool? _is_immutable;

		private List<DataType> base_types = new List<DataType>();

		private List<Constant> constants = new List<Constant>();
		private List<Field> fields = new List<Field>();
		private List<Method> methods = new List<Method>();
		private List<Property> properties = new List<Property>();
		private List<Signal> signals = new List<Signal>();

		// inner types
		private List<Class> classes = new List<Class>();
		private List<Struct> structs = new List<Struct>();
		private List<ValaEnum> enums = new List<ValaEnum>();
		private List<ValaDelegate> delegates = new List<ValaDelegate>();

		/**
		 * Returns a copy of the list of classes.
		 *
		 * @return list of classes
		 */
		public List<Class> get_classes() {
			return classes;
		}

		/**
		 * Returns a copy of the list of structs.
		 *
		 * @return list of structs
		 */
		public List<Struct> get_structs() {
			return structs;
		}

		/**
		 * Returns a copy of the list of enums.
		 *
		 * @return list of enums
		 */
		public List<ValaEnum> get_enums() {
			return enums;
		}

		/**
		 * Returns a copy of the list of delegates.
		 *
		 * @return list of delegates
		 */
		public List<ValaDelegate> get_delegates() {
			return delegates;
		}

		/**
		 * Specifies the default construction method.
		 */
		public CreationMethod default_construction_method { get; set; }

		/**
		 * Specifies the instance constructor.
		 */
		public Constructor constructor { get; set; }

		/**
		 * Specifies the class constructor.
		 */
		public Constructor class_constructor { get; set; }

		/**
		 * Specifies the static class constructor.
		 */
		public Constructor static_constructor { get; set; }

		/**
		 * Specifies the instance destructor.
		 */
		public Destructor destructor {
			get { return _destructor; }
			set {
				_destructor = value;
				if (_destructor != null) {
					if (_destructor.this_parameter != null) {
						_destructor.scope.remove(_destructor.this_parameter.name);
					}
					_destructor.this_parameter = new Parameter("this", get_this_type());
					_destructor.scope.add(_destructor.this_parameter.name, _destructor.this_parameter);
				}
			}
		}

		/**
		 * Specifies the class destructor.
		 */
		public Destructor static_destructor { get; set; }

		/**
		 * Specifies the class destructor.
		 */
		public Destructor class_destructor { get; set; }

		/**
		 * Specifies whether this class denotes an error base.
		 */
		public bool is_error_base {
			get {
				return get_attribute("ErrorBase") != null;
			}
		}

		Destructor _destructor;

		/**
		 * Creates a new class.
		 *
		 * @param name             type name
		 * @param source_reference reference to source code
		 * @param comment          class documentation
		 * @return                 newly created class
		 */
		public Class(string name, SourceReference source_reference = null, Comment comment = null) : base(name, source_reference, comment) {
		}

		/**
		 * Adds the specified class or interface to the list of base types of
		 * this class.
		 *
		 * @param type a class or interface reference
		 */
		public void add_base_type(DataType type) {
			base_types.Add(type);
			type.parent_node = this;
		}

		/**
		 * Returns a copy of the base type list.
		 *
		 * @return list of base types
		 */
		public List<DataType> get_base_types() {
			return base_types;
		}

		/**
		 * Adds the specified constant as a member to this class.
		 *
		 * @param c a constant
		 */
		public override void add_constant(Constant c) {
			constants.Add(c);
			scope.add(c.name, c);
		}

		/**
		 * Adds the specified field as a member to this class.
		 *
		 * @param f a field
		 */
		public override void add_field(Field f) {
			fields.Add(f);
			if (f.access == SymbolAccessibility.PRIVATE && f.binding == MemberBinding.INSTANCE) {
				has_private_fields = true;
			} else if (f.access == SymbolAccessibility.PRIVATE && f.binding == MemberBinding.CLASS) {
				has_class_private_fields = true;
			}
			scope.add(f.name, f);
		}

		/**
		 * Returns a copy of the list of fields.
		 *
		 * @return list of fields
		 */
		public List<Field> get_fields() {
			return fields;
		}

		/**
		 * Returns a copy of the list of constants.
		 *
		 * @return list of constants
		 */
		public List<Constant> get_constants() {
			return constants;
		}

		/**
		 * Adds the specified method as a member to this class.
		 *
		 * @param m a method
		 */
		public override void add_method(Method m) {
			if (m.binding == MemberBinding.INSTANCE || m is CreationMethod) {
				if (m.this_parameter != null) {
					m.scope.remove(m.this_parameter.name);
				}
				m.this_parameter = new Parameter("this", get_this_type());
				m.scope.add(m.this_parameter.name, m.this_parameter);
			}
			if (!(m.return_type is VoidType) && m.get_postconditions().Count > 0) {
				if (m.result_var != null) {
					m.scope.remove(m.result_var.name);
				}
				m.result_var = new LocalVariable(m.return_type.copy(), "result", null, source_reference);
				m.result_var.is_result = true;
			}
			if (m is CreationMethod) {
				if (m.name == null) {
					default_construction_method = (CreationMethod)m;
					m.name = ".new";
				}

				var cm = (CreationMethod)m;
				if (cm.class_name != null && cm.class_name != name) {
					// class_name is null for constructors generated by GIdlParser
					Report.error(m.source_reference, "missing return type in method `%s.%s´".printf(get_full_name(), cm.class_name));
					m.error = true;
					return;
				}
			}

			methods.Add(m);
			if (m.base_interface_type == null) {
				scope.add(m.name, m);
			} else {
				// explicit interface method implementation
				scope.add(null, m);
			}
		}

		/**
		 * Returns a copy of the list of methods.
		 *
		 * @return list of methods
		 */
		public override List<Method> get_methods() {
			return methods;
		}

		/**
		 * Adds the specified property as a member to this class.
		 *
		 * @param prop a property
		 */
		public override void add_property(Property prop) {
			properties.Add(prop);
			scope.add(prop.name, prop);

			prop.this_parameter = new Parameter("this", get_this_type());
			prop.scope.add(prop.this_parameter.name, prop.this_parameter);

			if (prop.field != null) {
				add_field(prop.field);
			}
		}

		/**
		 * Returns a copy of the list of properties.
		 *
		 * @return list of properties
		 */
		public override List<Property> get_properties() {
			return properties;
		}

		/**
		 * Adds the specified signal as a member to this class.
		 *
		 * @param sig a signal
		 */
		public override void add_signal(Signal sig) {
			signals.Add(sig);
			scope.add(sig.name, sig);
		}

		/**
		 * Returns a copy of the list of signals.
		 *
		 * @return list of signals
		 */
		public override List<Signal> get_signals() {
			return signals;
		}

		/**
		 * Adds the specified class as an inner class.
		 *
		 * @param cl a class
		 */
		public override void add_class(Class cl) {
			classes.Add(cl);
			scope.add(cl.name, cl);
		}

		/**
		 * Adds the specified struct as an inner struct.
		 *
		 * @param st a struct
		 */
		public override void add_struct(Struct st) {
			structs.Add(st);
			scope.add(st.name, st);
		}

		/**
		 * Adds the specified enum as an inner enum.
		 *
		 * @param en an enum
		 */
		public override void add_enum(ValaEnum en) {
			enums.Add(en);
			scope.add(en.name, en);
		}

		/**
		 * Adds the specified delegate as an inner delegate.
		 *
		 * @param d a delegate
		 */
		public override void add_delegate(ValaDelegate d) {
			delegates.Add(d);
			scope.add(d.name, d);
		}

		public override void add_constructor(Constructor c) {
			if (c.binding == MemberBinding.INSTANCE) {
				if (constructor != null) {
					Report.error(c.source_reference, "class already contains a constructor");
				}
				constructor = c;
			} else if (c.binding == MemberBinding.CLASS) {
				if (class_constructor != null) {
					Report.error(c.source_reference, "class already contains a class constructor");
				}
				class_constructor = c;
			} else {
				if (static_constructor != null) {
					Report.error(c.source_reference, "class already contains a static constructor");
				}
				static_constructor = c;
			}
		}

		public override void add_destructor(Destructor d) {
			if (d.binding == MemberBinding.INSTANCE) {
				if (destructor != null) {
					Report.error(d.source_reference, "class already contains a destructor");
				}
				destructor = d;
			} else if (d.binding == MemberBinding.CLASS) {
				if (class_destructor != null) {
					Report.error(d.source_reference, "class already contains a class destructor");
				}
				class_destructor = d;
			} else {
				if (static_destructor != null) {
					Report.error(d.source_reference, "class already contains a static destructor");
				}
				static_destructor = d;
			}
		}

		public override void accept(CodeVisitor visitor) {
			visitor.visit_class(this);
		}

		public override void accept_children(CodeVisitor visitor) {
			foreach (DataType type in base_types.ToList()) {
				type.accept(visitor);
			}

			foreach (TypeParameter p in get_type_parameters()) {
				p.accept(visitor);
			}

			/* process enums first to avoid order problems in C code */
			foreach (ValaEnum en in enums) {
				en.accept(visitor);
			}

			foreach (Field f in fields) {
				f.accept(visitor);
			}

			foreach (Constant c in constants) {
				c.accept(visitor);
			}

			foreach (Method m in methods) {
				m.accept(visitor);
			}

			foreach (Property prop in properties) {
				prop.accept(visitor);
			}

			foreach (Signal sig in signals) {
				sig.accept(visitor);
			}

			if (constructor != null) {
				constructor.accept(visitor);
			}

			if (class_constructor != null) {
				class_constructor.accept(visitor);
			}

			if (static_constructor != null) {
				static_constructor.accept(visitor);
			}

			if (destructor != null) {
				destructor.accept(visitor);
			}

			if (static_destructor != null) {
				static_destructor.accept(visitor);
			}

			if (class_destructor != null) {
				class_destructor.accept(visitor);
			}

			foreach (Class cl in classes) {
				cl.accept(visitor);
			}

			foreach (Struct st in structs) {
				st.accept(visitor);
			}

			foreach (ValaDelegate d in delegates) {
				d.accept(visitor);
			}
		}

		public override bool is_reference_type() {
			return true;
		}

		public bool is_fundamental() {
			if (!is_compact && base_class == null) {
				return true;
			}
			return false;
		}

		public override bool is_subtype_of(TypeSymbol t) {
			if (this == t) {
				return true;
			}

			foreach (DataType base_type in base_types) {
				if (base_type.data_type != null && base_type.data_type.is_subtype_of(t)) {
					return true;
				}
			}

			return false;
		}

		public override void replace_type(DataType old_type, DataType new_type) {
			for (int i = 0; i < base_types.Count; i++) {
				if (base_types[i] == old_type) {
					base_types[i] = new_type;
					new_type.parent_node = this;
					return;
				}
			}
		}

		private void get_all_prerequisites(Interface iface, List<TypeSymbol> list) {
			foreach (DataType prereq in iface.get_prerequisites()) {
				TypeSymbol type = prereq.data_type;
				/* skip on previous errors */
				if (type == null) {
					continue;
				}

				list.Add(type);
				if (type is Interface) {
					get_all_prerequisites((Interface)type, list);

				}
			}
		}

		private bool class_is_a(Class cl, TypeSymbol t) {
			if (cl == t) {
				return true;
			}

			foreach (DataType base_type in cl.get_base_types()) {
				if (base_type.data_type is Class) {
					if (class_is_a((Class)base_type.data_type, t)) {
						return true;
					}
				} else if (base_type.data_type == t) {
					return true;
				}
			}

			return false;
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
			context.analyzer.current_symbol = this;

			foreach (DataType base_type_reference in get_base_types()) {
				if (!base_type_reference.check(context)) {
					error = true;
					return false;
				}

				if (!(base_type_reference is ObjectType)) {
					error = true;
					Report.error(source_reference, "base type `%s` of class `%s` is not an object type".printf(base_type_reference.to_string(), get_full_name()));
					return false;
				}

				// check whether base type is at least as accessible as the class
				if (!context.analyzer.is_type_accessible(this, base_type_reference)) {
					error = true;
					Report.error(source_reference, "base type `%s` is less accessible than class `%s`".printf(base_type_reference.to_string(), get_full_name()));
					return false;
				}

				int n_type_args = base_type_reference.get_type_arguments().Count;
				int n_type_params = ((ObjectTypeSymbol)base_type_reference.data_type).get_type_parameters().Count;
				if (n_type_args < n_type_params) {
					error = true;
					Report.error(base_type_reference.source_reference, "too few type arguments");
					return false;
				} else if (n_type_args > n_type_params) {
					error = true;
					Report.error(base_type_reference.source_reference, "too many type arguments");
					return false;
				}
			}

			foreach (DataType type in base_types) {
				type.check(context);
			}

			foreach (TypeParameter p in get_type_parameters()) {
				p.check(context);
			}

			/* process enums first to avoid order problems in C code */
			foreach (ValaEnum en in enums) {
				en.check(context);
			}

			foreach (Field f in fields) {
				f.check(context);
			}

			foreach (Constant c in constants) {
				c.check(context);
			}

			foreach (Method m in methods) {
				m.check(context);
			}

			foreach (Property prop in properties) {
				if (prop.get_attribute("NoAccessorMethod") != null && !is_subtype_of(context.analyzer.object_type)) {
					error = true;
					Report.error(prop.source_reference, "NoAccessorMethod is only allowed for properties in classes derived from GLib.Object");
					return false;
				}
				prop.check(context);
			}

			foreach (Signal sig in signals) {
				sig.check(context);
			}

			if (constructor != null) {
				constructor.check(context);
			}

			if (class_constructor != null) {
				class_constructor.check(context);
			}

			if (static_constructor != null) {
				static_constructor.check(context);
			}

			if (destructor != null) {
				destructor.check(context);
			}

			if (static_destructor != null) {
				static_destructor.check(context);
			}

			if (class_destructor != null) {
				class_destructor.check(context);
			}

			foreach (Class cl in classes) {
				cl.check(context);
			}

			foreach (Struct st in structs) {
				st.check(context);
			}

			foreach (ValaDelegate d in delegates) {
				d.check(context);
			}

			/* compact classes cannot implement interfaces */
			if (is_compact) {
				foreach (DataType base_type in get_base_types()) {
					if (base_type.data_type is Interface) {
						error = true;
						Report.error(source_reference, "compact classes `%s` may not implement interfaces".printf(get_full_name()));
					}
				}

				if (!external && !external_package && base_class != null && base_class != context.analyzer.gsource_type) {
					foreach (Field f in fields) {
						if (f.binding == MemberBinding.INSTANCE) {
							error = true;
							Report.error(source_reference, "derived compact classes may not have instance fields");
							break;
						}
					}
				}
			}

			/* gather all prerequisites */
			List<TypeSymbol> prerequisites = new List<TypeSymbol>();
			foreach (DataType base_type in get_base_types()) {
				if (base_type.data_type is Interface) {
					get_all_prerequisites((Interface)base_type.data_type, prerequisites);
				}
			}
			/* check whether all prerequisites are met */
			List<string> missing_prereqs = new List<string>();
			foreach (TypeSymbol prereq in prerequisites) {
				if (!class_is_a(this, prereq)) {
					missing_prereqs.Insert(0, prereq.get_full_name());
				}
			}
			/* report any missing prerequisites */
			if (missing_prereqs.Count > 0) {
				error = true;

				string error_string = "%s: some prerequisites (".printf(get_full_name());
				bool first = true;
				foreach (string s in missing_prereqs) {
					if (first) {
						error_string = "%s`%s'".printf(error_string, s);
						first = false;
					} else {
						error_string = "%s, `%s'".printf(error_string, s);
					}
				}
				error_string += ") are not met";
				Report.error(source_reference, error_string);
			}

			/* VAPI classes don't have to specify overridden methods */
			if (source_type == SourceFileType.SOURCE) {
				/* all abstract symbols defined in base types have to be at least defined (or implemented) also in this type */
				foreach (DataType base_type in get_base_types()) {
					if (base_type.data_type is Interface) {
						Interface iface = (Interface)base_type.data_type;

						if (base_class != null && base_class.is_subtype_of(iface)) {
							// reimplementation of interface, class is not required to reimplement all methods
							break;
						}

						/* We do not need to do expensive equality checking here since this is done
						 * already. We only need to guarantee the symbols are present.
						 */

						/* check methods */
						foreach (Method m in iface.get_methods()) {
							if (m.is_abstract) {
								var implemented = false;
								var base_class = this;
								while (base_class != null) {
									foreach (var impl in base_class.get_methods()) {
										if (impl.name == m.name && (impl.base_interface_type == null || impl.base_interface_type.data_type == iface)) {
											// method is used as interface implementation, so it is not unused
											impl.version.check(source_reference);
											impl.used = true;
											implemented = true;
											break;
										}
									}
									base_class = base_class.base_class;
								}
								if (!implemented) {
									error = true;
									Report.error(source_reference, "`%s' does not implement interface method `%s'".printf(get_full_name(), m.get_full_name()));
								}
							}
						}

						/* check properties */
						foreach (Property prop in iface.get_properties()) {
							if (prop.is_abstract) {
								Symbol sym = null;
								var base_class = this;
								while (base_class != null && !(sym is Property)) {
									sym = base_class.scope.lookup(prop.name);
									base_class = base_class.base_class;
								}
								if (sym is Property) {
									var base_prop = (Property)sym;
									string invalid_match = null;
									// No check at all for "new" classified properties, really?
									if (!base_prop.hides && !base_prop.compatible(prop, out invalid_match)) {
										error = true;
										Report.error(source_reference, "Type and/or accessors of inherited properties `%s' and `%s' do not match: %s.".printf(prop.get_full_name(), base_prop.get_full_name(), invalid_match));
									}
									// property is used as interface implementation, so it is not unused
									sym.version.check(source_reference);
									sym.used = true;
								} else {
									error = true;
									Report.error(source_reference, "`%s' does not implement interface property `%s'".printf(get_full_name(), prop.get_full_name()));
								}
							}
						}
					}
				}

				/* all abstract symbols defined in base classes have to be implemented in non-abstract classes */
				if (!is_abstract) {
					var base_class = this.base_class;
					while (base_class != null && base_class.is_abstract) {
						foreach (Method base_method in base_class.get_methods()) {
							if (base_method.is_abstract) {
								var override_method = SemanticAnalyzer.symbol_lookup_inherited(this, base_method.name) as Method;
								if (override_method == null || !override_method.overrides) {
									error = true;
									Report.error(source_reference, "`%s' does not implement abstract method `%s'".printf(get_full_name(), base_method.get_full_name()));
								}
							}
						}
						foreach (Property base_property in base_class.get_properties()) {
							if (base_property.is_abstract) {
								var override_property = SemanticAnalyzer.symbol_lookup_inherited(this, base_property.name) as Property;
								if (override_property == null || !override_property.overrides) {
									error = true;
									Report.error(source_reference, "`%s' does not implement abstract property `%s'".printf(get_full_name(), base_property.get_full_name()));
								}
							}
						}
						base_class = base_class.base_class;
					}
				}
			}

			context.analyzer.current_source_file = old_source_file;
			context.analyzer.current_symbol = old_symbol;

			return !error;
		}
	}
}
