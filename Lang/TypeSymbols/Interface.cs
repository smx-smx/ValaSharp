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
	public class Interface : ObjectTypeSymbol {
		private List<DataType> prerequisites = new List<DataType>();

		private List<Method> methods = new List<Method>();
		private List<Field> fields = new List<Field>();
		private List<Constant> constants = new List<Constant>();
		private List<Property> properties = new List<Property>();
		private List<Signal> signals = new List<Signal>();
		private List<Symbol> virtuals = new List<Symbol>();

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
		 * Creates a new interface.
		 *
		 * @param name              type name
		 * @param source_reference  reference to source code
		 * @return                  newly created interface
		 */
		public Interface(string name, SourceReference source_reference = null, Comment comment = null) : base(name, source_reference, comment) { }

		/**
		 * Adds the specified interface or class to the list of prerequisites of
		 * this interface.
		 *
		 * @param type an interface or class reference
		 */
		public void add_prerequisite(DataType type) {
			prerequisites.Add(type);
			type.parent_node = this;
		}

		/**
		 * Prepends the specified interface or class to the list of
		 * prerequisites of this interface.
		 *
		 * @param type an interface or class reference
		 */
		public void prepend_prerequisite(DataType type) {
			prerequisites.Insert(0, type);
		}

		/**
		 * Returns a copy of the base type list.
		 *
		 * @return list of base types
		 */
		public List<DataType> get_prerequisites() {
			return prerequisites;
		}

		/**
		 * Adds the specified method as a member to this interface.
		 *
		 * @param m a method
		 */
		public override void add_method(Method m) {
			if (m is CreationMethod) {
				Report.error(m.source_reference, "construction methods may only be declared within classes and structs");

				m.error = true;
				return;
			}
			if (m.binding == MemberBinding.INSTANCE) {
				m.this_parameter = new Parameter("this", get_this_type());
				m.scope.add(m.this_parameter.name, m.this_parameter);
			}
			if (!(m.return_type is VoidType) && m.get_postconditions().Count > 0) {
				m.result_var = new LocalVariable(m.return_type.copy(), "result", null, source_reference);
				m.result_var.is_result = true;
			}

			methods.Add(m);
			scope.add(m.name, m);
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
		 * Adds the specified field as a member to this interface. The field
		 * must be private and static.
		 *
		 * @param f a field
		 */
		public override void add_field(Field f) {
			fields.Add(f);
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
		 * Adds the specified constant as a member to this interface.
		 *
		 * @param c a constant
		 */
		public override void add_constant(Constant c) {
			constants.Add(c);
			scope.add(c.name, c);
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
		 * Adds the specified property as a member to this interface.
		 *
		 * @param prop a property
		 */
		public override void add_property(Property prop) {
			if (prop.field != null) {
				Report.error(prop.source_reference, "automatic properties are not allowed in interfaces");

				prop.error = true;
				return;
			}

			properties.Add(prop);
			scope.add(prop.name, prop);

			prop.this_parameter = new Parameter("this", new ObjectType(this));
			prop.scope.add(prop.this_parameter.name, prop.this_parameter);
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
		 * Adds the specified signal as a member to this interface.
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

		public virtual List<Symbol> get_virtuals() {
			return virtuals;
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

		public override void accept(CodeVisitor visitor) {
			visitor.visit_interface(this);
		}

		public override void accept_children(CodeVisitor visitor) {
			foreach (DataType type in prerequisites.ToList()) {
				type.accept(visitor);
			}

			foreach (TypeParameter p in get_type_parameters()) {
				p.accept(visitor);
			}

			/* process enums first to avoid order problems in C code */
			foreach (ValaEnum en in enums) {
				en.accept(visitor);
			}

			foreach (Method m in methods) {
				m.accept(visitor);
			}

			foreach (Field f in fields) {
				f.accept(visitor);
			}

			foreach (Constant c in constants) {
				c.accept(visitor);
			}

			foreach (Property prop in properties) {
				prop.accept(visitor);
			}

			foreach (Signal sig in signals) {
				sig.accept(visitor);
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

		public override bool is_subtype_of(TypeSymbol t) {
			if (this == t) {
				return true;
			}

			foreach (DataType prerequisite in prerequisites) {
				if (prerequisite.data_type != null && prerequisite.data_type.is_subtype_of(t)) {
					return true;
				}
			}

			return false;
		}

		public override void replace_type(DataType old_type, DataType new_type) {
			for (int i = 0; i < prerequisites.Count; i++) {
				if (prerequisites[i] == old_type) {
					prerequisites[i] = new_type;
					new_type.parent_node = this;
					return;
				}
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
			context.analyzer.current_symbol = this;

			foreach (DataType prerequisite_reference in get_prerequisites()) {
				// check whether prerequisite is at least as accessible as the interface
				if (!context.analyzer.is_type_accessible(this, prerequisite_reference)) {
					error = true;
					Report.error(source_reference, "prerequisite `%s` is less accessible than interface `%s`".printf(prerequisite_reference.ToString(), get_full_name()));
					return false;
				}
			}

			/* check prerequisites */
			Class prereq_class = null;
			foreach (DataType prereq in get_prerequisites()) {
				TypeSymbol class_or_interface = prereq.data_type;
				/* skip on previous errors */
				if (class_or_interface == null) {
					error = true;
					continue;
				}

				if (!(class_or_interface is ObjectTypeSymbol)) {
					error = true;
					Report.error(source_reference, "Prerequisite `%s` of interface `%s` is not a class or interface".printf(get_full_name(), class_or_interface.ToString()));
					return false;
				}

				/* interfaces are not allowed to have multiple instantiable prerequisites */
				if (class_or_interface is Class) {
					if (prereq_class != null) {
						error = true;
						Report.error(source_reference, "%s: Interfaces cannot have multiple instantiable prerequisites (`%s' and `%s')".printf(get_full_name(), class_or_interface.get_full_name(), prereq_class.get_full_name()));
						return false;
					}

					prereq_class = (Class)class_or_interface;
				}
			}

			foreach (DataType type in prerequisites) {
				type.check(context);
			}

			foreach (TypeParameter p in get_type_parameters()) {
				p.check(context);
			}

			foreach (ValaEnum en in enums) {
				en.check(context);
			}

			foreach (Method m in methods) {
				m.check(context);
				if (m.is_virtual || m.is_abstract) {
					virtuals.Add(m);
				}
			}

			foreach (Field f in fields) {
				f.check(context);
			}

			foreach (Constant c in constants) {
				c.check(context);
			}

			foreach (Signal sig in signals) {
				sig.check(context);
				if (sig.is_virtual) {
					virtuals.Add(sig);
				}
			}

			foreach (Property prop in properties) {
				prop.check(context);
				if (prop.is_virtual || prop.is_abstract) {
					virtuals.Add(prop);
				}
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

			Dictionary<int, Symbol> positions = new Dictionary<int, Symbol>();
			bool ordered_seen = false;
			bool unordered_seen = false;
			foreach (Symbol sym in virtuals) {
				int ordering = sym.get_attribute_integer("CCode", "ordering", -1);
				if (ordering < -1) {
					Report.error(sym.source_reference, "%s: Invalid ordering".printf(sym.get_full_name()));
					// Mark state as invalid
					error = true;
					ordered_seen = true;
					unordered_seen = true;
					continue;
				}
				bool ordered = ordering != -1;
				if (ordered && unordered_seen && !ordered_seen) {
					Report.error(sym.source_reference, "%s: Cannot mix ordered and unordered virtuals".printf(sym.get_full_name()));
					error = true;
				}
				ordered_seen = ordered_seen || ordered;
				if (!ordered && !unordered_seen && ordered_seen) {
					Report.error(sym.source_reference, "%s: Cannot mix ordered and unordered virtuals".printf(sym.get_full_name()));
					error = true;
				}
				unordered_seen = unordered_seen || !ordered;
				if (!ordered_seen || !unordered_seen) {
					if (ordered) {
						positions.TryGetValue(ordering, out Symbol prev);
						if (prev != null) {
							Report.error(sym.source_reference, "%s: Duplicate ordering (previous virtual with the same position is %s)".printf(sym.get_full_name(), prev.name));
							error = true;
						}
						positions[ordering] = sym;
					}
				}
			}
			if (ordered_seen) {
				for (int i = 0; i < virtuals.Count; i++) {
					Symbol sym = positions[i];
					if (sym == null) {
						Report.error(source_reference, "%s: Gap in ordering in position %d".printf(get_full_name(), i));
						error = true;
					}
					if (!error) {
						virtuals[i] = sym;
					}
				}
			}

			context.analyzer.current_source_file = old_source_file;
			context.analyzer.current_symbol = old_symbol;

			return !error;
		}
	}
}
