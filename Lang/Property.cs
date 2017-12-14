﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.CodeNodes;
using Vala.Lang.Expressions;
using Vala.Lang.Parser;
using Vala.Lang.Symbols;
using Vala.Lang.Types;
using Vala.Lang.TypeSymbols;

namespace Vala.Lang {
	public class Property : Symbol, Lockable {
		/// <summary>
		/// The property type.
		/// </summary>
		public DataType property_type {
			get { return _data_type; }
			set {
				_data_type = value;
				if (value != null) {
					_data_type.parent_node = this;
				}
			}
		}

		/// <summary>
		/// The get accessor of this property if available.
		/// </summary>
		public PropertyAccessor get_accessor {
			get { return _get_accessor; }
			set {
				_get_accessor = value;
				if (value != null) {
					value.owner = scope;
				}
			}
		}

		/// <summary>
		/// The set/construct accessor of this property if available.
		/// </summary>
		public PropertyAccessor set_accessor {
			get { return _set_accessor; }
			set {
				_set_accessor = value;
				if (value != null) {
					value.owner = scope;
				}
			}
		}

		/// <summary>
		/// Represents the generated `this` parameter in this property.
		/// </summary>
		public Parameter this_parameter { get; set; }

		/// <summary>
		/// Specifies whether automatic accessor code generation should be
		/// disabled.
		/// </summary>
		public bool interface_only { get; set; }

		/// <summary>
		/// Specifies whether this property is abstract. Abstract properties have
		/// no accessor bodies, may only be specified within abstract classes and
		/// interfaces, and must be overriden by derived non-abstract classes.
		/// </summary>
		public bool is_abstract { get; set; }

		/// <summary>
		/// Specifies whether this property is virtual. Virtual properties may be
		/// overridden by derived classes.
		/// </summary>
		public bool is_virtual { get; set; }

		/// <summary>
		/// Specifies whether this property overrides a virtual or abstract
		/// property of a base type.
		/// </summary>
		public bool overrides { get; set; }

		/// <summary>
		/// Reference the the Field that holds this property
		/// </summary>
		public Field field { get; set; }

		/// <summary>
		/// Specifies whether this field may only be accessed with an instance of
		/// the contained type.
		/// </summary>
		public MemberBinding binding { get; set; } = MemberBinding.INSTANCE;

		/// <summary>
		/// The nick of this property
		/// </summary>
		public string nick {
			get {
				if (_nick == null) {
					_nick = get_attribute_string("Description", "nick");
					if (_nick == null) {
						_nick = name.Replace("_", "-");
					}
				}
				return _nick;
			}
		}

		/// <summary>
		/// The blurb of this property
		/// </summary>
		public string blurb {
			get {
				if (_blurb == null) {
					_blurb = get_attribute_string("Description", "blurb");
					if (_blurb == null) {
						_blurb = name.Replace("_", "-");
					}
				}
				return _blurb;
			}
		}

		/// <summary>
		/// Specifies whether this a property triggers a notify.
		/// </summary>
		public bool notify {
			get {
				if (_notify == null) {
					_notify = get_attribute_bool("CCode", "notify", true);
				}
				return _notify.Value;
			}
		}

		/// <summary>
		/// Specifies the virtual or abstract property this property overrides.
		/// Reference must be weak as virtual properties set base_property to
		/// themselves.
		/// </summary>
		public Property base_property {
			get {
				find_base_properties();
				return _base_property;
			}
		}

		/// <summary>
		/// Specifies the abstract interface property this property implements.
		/// </summary>
		public Property base_interface_property {
			get {
				find_base_properties();
				return _base_interface_property;
			}
		}

		/// <summary>
		/// Specifies the default value of this property.
		/// </summary>
		public Expression initializer {
			get {
				return _initializer;
			}
			set {
				_initializer = value;
				_initializer.parent_node = this;
			}
		}

		private Expression _initializer;

		private bool lock_used = false;

		private DataType _data_type;

		private WeakReference<Property> _base_property_weak = new WeakReference<Property>(null);

		private Property _base_property {
			get {
				return _base_property_weak.GetTarget();
			}
			set {
				_base_property_weak.SetTarget(value);
			}
		}
		private Property _base_interface_property;
		private bool base_properties_valid;
		PropertyAccessor _get_accessor;
		PropertyAccessor _set_accessor;
		private string _nick;
		private string _blurb;
		private bool? _notify;

		/// <summary>
		/// Creates a new property.
		/// 
		/// <param name="name">property name</param>
		/// <param name="property_type">property type</param>
		/// <param name="get_accessor">get accessor</param>
		/// <param name="set_accessor">set/construct accessor</param>
		/// <param name="source_reference">reference to source code</param>
		/// <returns>newly created property</returns>
		/// </summary>
		public Property(
			string name, DataType property_type, PropertyAccessor get_accessor,
			PropertyAccessor set_accessor, SourceReference source_reference = null, Comment comment = null
		) : base(name, source_reference, comment) {
			this.property_type = property_type;
			this.get_accessor = get_accessor;
			this.set_accessor = set_accessor;
		}

		public override void accept(CodeVisitor visitor) {
			visitor.visit_property(this);
		}

		public override void accept_children(CodeVisitor visitor) {
			property_type.accept(visitor);

			if (get_accessor != null) {
				get_accessor.accept(visitor);
			}
			if (set_accessor != null) {
				set_accessor.accept(visitor);
			}

			if (initializer != null) {
				initializer.accept(visitor);
			}
		}

		public bool get_lock_used() {
			return lock_used;
		}

		public void set_lock_used(bool used) {
			lock_used = used;
		}

		/// <summary>
		/// Checks whether the accessors of this property are compatible
		/// with the specified base property.
		/// 
		/// <param name="base_property">a property</param>
		/// <param name="invalid_match">error string about which check failed</param>
		/// <returns>true if the specified property is compatible to this property</returns>
		/// </summary>
		public bool compatible(Property base_property, out string invalid_match) {
			if ((get_accessor == null && base_property.get_accessor != null) ||
				(get_accessor != null && base_property.get_accessor == null)) {
				invalid_match = "incompatible get accessor";
				return false;
			}

			if ((set_accessor == null && base_property.set_accessor != null) ||
				(set_accessor != null && base_property.set_accessor == null)) {
				invalid_match = "incompatible set accessor";
				return false;
			}

			var object_type = SemanticAnalyzer.get_data_type_for_symbol((TypeSymbol)parent_symbol);

			if (get_accessor != null) {
				// check accessor value_type instead of property_type
				// due to possible ownership differences
				var actual_base_type = base_property.get_accessor.value_type.get_actual_type(object_type, null, this);
				if (!actual_base_type.equals(get_accessor.value_type)) {
					invalid_match = "incompatible get accessor type";
					return false;
				}
			}

			if (set_accessor != null) {
				// check accessor value_type instead of property_type
				// due to possible ownership differences
				var actual_base_type = base_property.set_accessor.value_type.get_actual_type(object_type, null, this);
				if (!actual_base_type.equals(set_accessor.value_type)) {
					invalid_match = "incompatible set accessor type";
					return false;
				}

				if (set_accessor.writable != base_property.set_accessor.writable) {
					invalid_match = "incompatible set accessor";
					return false;
				}
				if (set_accessor.construction != base_property.set_accessor.construction) {
					invalid_match = "incompatible set accessor";
					return false;
				}
			}

			invalid_match = null;
			return true;
		}

		public override void replace_type(DataType old_type, DataType new_type) {
			if (property_type == old_type) {
				property_type = new_type;
			}
		}

		public override void replace_expression(Expression old_node, Expression new_node) {
			if (initializer == old_node) {
				initializer = new_node;
			}
		}

		private void find_base_properties() {
			if (base_properties_valid) {
				return;
			}

			if (parent_symbol is Class) {
				find_base_interface_property((Class)parent_symbol);
				if (is_virtual || overrides) {
					find_base_class_property((Class)parent_symbol);
				}
			} else if (parent_symbol is Interface) {
				if (is_virtual || is_abstract) {
					_base_interface_property = this;
				}
			}

			base_properties_valid = true;
		}

		private void find_base_class_property(Class cl) {
			var sym = cl.scope.lookup(name);
			if (sym is Property) {
				var base_property = (Property)sym;
				if (base_property.is_abstract || base_property.is_virtual) {
					string invalid_match;
					if (!compatible(base_property, out invalid_match)) {
						error = true;
						Report.error(source_reference, "Type and/or accessors of overriding property `%s' do not match overridden property `%s': %s.".printf(get_full_name(), base_property.get_full_name(), invalid_match));
						return;
					}

					_base_property = base_property;
					return;
				}
			}

			if (cl.base_class != null) {
				find_base_class_property(cl.base_class);
			}
		}

		private void find_base_interface_property(Class cl) {
			// FIXME report error if multiple possible base properties are found
			foreach (DataType type in cl.get_base_types()) {
				if (type.data_type is Interface) {
					var sym = type.data_type.scope.lookup(name);
					if (sym is Property) {
						var base_property = (Property)sym;
						if (base_property.is_abstract || base_property.is_virtual) {
							string invalid_match;
							if (!compatible(base_property, out invalid_match)) {
								error = true;
								Report.error(source_reference, "Type and/or accessors of overriding property `%s' do not match overridden property `%s': %s.".printf(get_full_name(), base_property.get_full_name(), invalid_match));
								return;
							}

							_base_interface_property = base_property;
							return;
						}
					}
				}
			}
		}

		public override bool check(CodeContext context) {
			if (is_checked) {
				return !error;
			}

			is_checked = true;

			if (is_abstract) {
				if (parent_symbol is Class) {
					var cl = (Class)parent_symbol;
					if (!cl.is_abstract) {
						error = true;
						Report.error(source_reference, "Abstract properties may not be declared in non-abstract classes");
						return false;
					}
				} else if (!(parent_symbol is Interface)) {
					error = true;
					Report.error(source_reference, "Abstract properties may not be declared outside of classes and interfaces");
					return false;
				}
			} else if (is_virtual) {
				if (!(parent_symbol is Class) && !(parent_symbol is Interface)) {
					error = true;
					Report.error(source_reference, "Virtual properties may not be declared outside of classes and interfaces");
					return false;
				}

				if (parent_symbol is Class) {
					var cl = (Class)parent_symbol;
					if (cl.is_compact) {
						error = true;
						Report.error(source_reference, "Virtual properties may not be declared in compact classes");
						return false;
					}
				}
			} else if (overrides) {
				if (!(parent_symbol is Class)) {
					error = true;
					Report.error(source_reference, "Properties may not be overridden outside of classes");
					return false;
				}
			} else if (access == SymbolAccessibility.PROTECTED) {
				if (!(parent_symbol is Class) && !(parent_symbol is Interface)) {
					error = true;
					Report.error(source_reference, "Protected properties may not be declared outside of classes and interfaces");
					return false;
				}
			}

			var old_source_file = context.analyzer.current_source_file;
			var old_symbol = context.analyzer.current_symbol;

			if (source_reference != null) {
				context.analyzer.current_source_file = source_reference.file;
			}
			context.analyzer.current_symbol = this;

			if (property_type is VoidType) {
				error = true;
				Report.error(source_reference, "'void' not supported as property type");
				return false;
			}

			property_type.check(context);

			if (get_accessor == null && set_accessor == null) {
				error = true;
				Report.error(source_reference, "Property `%s' must have a `get' accessor and/or a `set' mutator".printf(get_full_name()));
				return false;
			}

			if (get_accessor != null) {
				get_accessor.check(context);
			}
			if (set_accessor != null) {
				set_accessor.check(context);
			}

			if (initializer != null) {
				initializer.check(context);
			}

			// check whether property type is at least as accessible as the property
			if (!context.analyzer.is_type_accessible(this, property_type)) {
				error = true;
				Report.error(source_reference, "property type `%s` is less accessible than property `%s`".printf(property_type.ToString(), get_full_name()));
			}

			if (overrides && base_property == null) {
				Report.error(source_reference, "%s: no suitable property found to override".printf(get_full_name()));
			}

			if (!external_package && !overrides && !hides && get_hidden_member() != null) {
				Report.warning(source_reference, "%s hides inherited property `%s'. Use the `new' keyword if hiding was intentional".printf(get_full_name(), get_hidden_member().get_full_name()));
			}

			/* construct properties must be public */
			if (set_accessor != null && set_accessor.construction) {
				if (access != SymbolAccessibility.PUBLIC) {
					error = true;
					Report.error(source_reference, "%s: construct properties must be public".printf(get_full_name()));
				}
			}

			if (initializer != null && !initializer.error && initializer.value_type != null && !(initializer.value_type.compatible(property_type))) {
				error = true;
				Report.error(initializer.source_reference, "Expected initializer of type `%s' but got `%s'".printf(property_type.ToString(), initializer.value_type.ToString()));
			}

			context.analyzer.current_source_file = old_source_file;
			context.analyzer.current_symbol = old_symbol;

			return !error;
		}
	}
}
