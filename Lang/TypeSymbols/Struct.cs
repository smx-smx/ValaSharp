using GLibPorts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.CodeNodes;
using Vala.Lang.Methods;
using Vala.Lang.Parser;
using Vala.Lang.Symbols;
using Vala.Lang.Types;

namespace Vala.Lang.TypeSymbols {
	public class Struct : TypeSymbol {
		private List<TypeParameter> type_parameters = new List<TypeParameter>();
		private List<Constant> constants = new List<Constant>();
		private List<Field> fields = new List<Field>();
		private List<Method> methods = new List<Method>();
		private List<Property> properties = new List<Property>();
		private DataType _base_type = null;

		private bool? boolean_type;
		private bool? integer_type;
		private bool? floating_type;
		private bool? decimal_floating_type;
		private bool? simple_type;
		private int? rank;
		private int? _width;
		private bool? _signed;
		private bool? _is_immutable;

		/// <summary>
		/// Specifies the base type.
		/// </summary>
		public DataType base_type {
			get {
				return _base_type;
			}
			set {
				value.parent_node = this;
				_base_type = value;
			}
		}

		/// <summary>
		/// Specifies the base Struct.
		/// </summary>
		public Struct base_struct {
			get {
				if (_base_type != null) {
					return _base_type.data_type as Struct;
				}
				return null;
			}
		}

		/// <summary>
		/// Specifies the default construction method.
		/// </summary>
		public Method default_construction_method { get; set; }

		/// <summary>
		/// Specifies if 'const' should be emitted for input parameters
		/// of this type.
		/// </summary>
		public bool is_immutable {
			get {
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

		public int width {
			get {
				if (_width == null) {
					if (is_integer_type()) {
						_width = get_attribute_integer("IntegerType", "width", 32);
					} else {
						_width = get_attribute_integer("FloatingType", "width", 32);
					}
				}
				return _width.Value;
			}
			set {
				_width = value;
				if (is_integer_type()) {
					set_attribute_integer("IntegerType", "width", value);
				} else {
					set_attribute_integer("FloatingType", "width", value);
				}
			}
		}

		public bool signed {
			get {
				if (_signed == null) {
					_signed = get_attribute_bool("IntegerType", "signed", true);
				}
				return _signed.Value;
			}
			set {
				_signed = value;
				set_attribute_bool("IntegerType", "signed", value);
			}
		}

		/// <summary>
		/// Creates a new struct.
		/// 
		/// <param name="name">type name</param>
		/// <param name="source_reference">reference to source code</param>
		/// <returns>newly created struct</returns>
		/// </summary>
		public Struct(string name, SourceReference source_reference = null, Comment comment = null)
			: base(name, source_reference, comment) { }

		/// <summary>
		/// Appends the specified parameter to the list of type parameters.
		/// 
		/// <param name="p">a type parameter</param>
		/// </summary>
		public void add_type_parameter(TypeParameter p) {
			type_parameters.Add(p);
			scope.add(p.name, p);
		}

		/// <summary>
		/// Returns a copy of the type parameter list.
		/// 
		/// <returns>list of type parameters</returns>
		/// </summary>
		public List<TypeParameter> get_type_parameters() {
			return type_parameters;
		}

		/// <summary>
		/// Adds the specified constant as a member to this struct.
		/// 
		/// <param name="c">a constant</param>
		/// </summary>
		public override void add_constant(Constant c) {
			constants.Add(c);
			scope.add(c.name, c);
		}

		/// <summary>
		/// Adds the specified field as a member to this struct.
		/// 
		/// <param name="f">a field</param>
		/// </summary>
		public override void add_field(Field f) {
			f.access = SymbolAccessibility.PUBLIC;

			fields.Add(f);
			scope.add(f.name, f);
		}

		/// <summary>
		/// Returns a copy of the list of fields.
		/// 
		/// <returns>list of fields</returns>
		/// </summary>
		public List<Field> get_fields() {
			return fields;
		}

		/// <summary>
		/// Returns a copy of the list of constants.
		/// 
		/// <returns>list of constants</returns>
		/// </summary>
		public List<Constant> get_constants() {
			return constants;
		}

		/// <summary>
		/// Adds the specified method as a member to this struct.
		/// 
		/// <param name="m">a method</param>
		/// </summary>
		public override void add_method(Method m) {
			if (m == null) {
				Trace.WriteLine("m != null failed");
				return;
			}

			if (m.binding == MemberBinding.INSTANCE || m is CreationMethod) {
				m.this_parameter = new Parameter("this", SemanticAnalyzer.get_data_type_for_symbol(this));
				m.scope.add(m.this_parameter.name, m.this_parameter);
			}
			if (!(m.return_type is VoidType) && m.get_postconditions().Count > 0) {
				m.result_var = new LocalVariable(m.return_type.copy(), "result", null, source_reference);
				m.result_var.is_result = true;
			}
			if (m is CreationMethod) {
				if (m.name == null) {
					default_construction_method = m;
					m.name = ".new";
				}

				var cm = (CreationMethod)m;
				if (cm.class_name != null && cm.class_name != name) {
					// type_name is null for constructors generated by GIdlParser
					Report.error(m.source_reference, "missing return type in method `%s.%s´".printf(get_full_name(), cm.class_name));
					m.error = true;
					return;
				}
			}

			methods.Add(m);
			scope.add(m.name, m);
		}

		/// <summary>
		/// Returns a copy of the list of methods.
		/// 
		/// <returns>list of methods</returns>
		/// </summary>
		public List<Method> get_methods() {
			return methods;
		}

		/// <summary>
		/// Adds the specified property as a member to this struct.
		/// 
		/// <param name="prop">a property</param>
		/// </summary>
		public override void add_property(Property prop) {
			properties.Add(prop);
			scope.add(prop.name, prop);

			prop.this_parameter = new Parameter("this", SemanticAnalyzer.get_data_type_for_symbol(this));
			prop.scope.add(prop.this_parameter.name, prop.this_parameter);

			if (prop.field != null) {
				add_field(prop.field);
			}
		}

		/// <summary>
		/// Returns a copy of the list of properties.
		/// 
		/// <returns>list of properties</returns>
		/// </summary>
		public List<Property> get_properties() {
			return properties;
		}

		public override void accept(CodeVisitor visitor) {
			visitor.visit_struct(this);
		}

		public override void accept_children(CodeVisitor visitor) {
			if (base_type != null) {
				base_type.accept(visitor);
			}

			foreach (TypeParameter p in type_parameters) {
				p.accept(visitor);
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
		}

		/// <summary>
		/// Returns whether this is a boolean type.
		/// 
		/// <returns>true if this is a boolean type, false otherwise</returns>
		/// </summary>
		public bool is_boolean_type() {
			var st = base_struct;
			if (st != null && st.is_boolean_type()) {
				return true;
			}
			if (boolean_type == null) {
				boolean_type = get_attribute("BooleanType") != null;
			}
			return boolean_type.Value;
		}

		/// <summary>
		/// Returns whether this is an integer type.
		/// 
		/// <returns>true if this is an integer type, false otherwise</returns>
		/// </summary>
		public bool is_integer_type() {
			var st = base_struct;
			if (st != null && st.is_integer_type()) {
				return true;
			}
			if (integer_type == null) {
				integer_type = get_attribute("IntegerType") != null;
			}
			return integer_type.Value;
		}

		/// <summary>
		/// Returns whether this is a floating point type.
		/// 
		/// <returns>true if this is a floating point type, false otherwise</returns>
		/// </summary>
		public bool is_floating_type() {
			var st = base_struct;
			if (st != null && st.is_floating_type()) {
				return true;
			}
			if (floating_type == null) {
				floating_type = get_attribute("FloatingType") != null;
			}
			return floating_type.Value;
		}

		public bool is_decimal_floating_type() {
			var st = base_struct;
			if (st != null && st.is_decimal_floating_type()) {
				return true;
			}
			if (decimal_floating_type == null) {
				decimal_floating_type = get_attribute_bool("FloatingType", "decimal");
			}
			return decimal_floating_type.Value;
		}

		/// <summary>
		/// Returns the rank of this integer or floating point type.
		/// 
		/// <returns>the rank if this is an integer or floating point type</returns>
		/// </summary>
		public int get_rank() {
			if (rank == null) {
				if (is_integer_type() && has_attribute_argument("IntegerType", "rank")) {
					rank = get_attribute_integer("IntegerType", "rank");
				} else if (has_attribute_argument("FloatingType", "rank")) {
					rank = get_attribute_integer("FloatingType", "rank");
				} else {
					var st = base_struct;
					if (st != null) {
						rank = st.get_rank();
					}
				}
			}
			return rank.Value;
		}

		/// <summary>
		/// Sets the rank of this integer or floating point type.
		/// </summary>
		public void set_rank(int rank) {
			this.rank = rank;
			if (is_integer_type()) {
				set_attribute_integer("IntegerType", "rank", rank);
			} else {
				set_attribute_integer("FloatingType", "rank", rank);
			}
		}

		public override int get_type_parameter_index(string name) {
			int i = 0;

			foreach (TypeParameter p in type_parameters) {
				if (p.name == name) {
					return (i);
				}
				i++;
			}

			return -1;
		}

		/// <summary>
		/// Returns whether this struct is a simple type, i.e. whether
		/// instances are passed by value.
		/// </summary>
		public bool is_simple_type() {
			var st = base_struct;
			if (st != null && st.is_simple_type()) {
				return true;
			}
			if (simple_type == null) {
				simple_type = get_attribute("SimpleType") != null || get_attribute("BooleanType") != null || get_attribute("IntegerType") != null || get_attribute("FloatingType") != null;
			}
			return simple_type.Value;
		}

		/// <summary>
		/// Marks this struct as simple type, i.e. instances will be passed by
		/// value.
		/// </summary>
		public void set_simple_type(bool simple_type) {
			this.simple_type = simple_type;
			set_attribute("SimpleType", simple_type);
		}

		public override void replace_type(DataType old_type, DataType new_type) {
			if (base_type == old_type) {
				base_type = new_type;
			}
		}

		public override bool is_subtype_of(TypeSymbol t) {
			if (this == t) {
				return true;
			}

			if (base_type != null) {
				if (base_type.data_type != null && base_type.data_type.is_subtype_of(t)) {
					return true;
				}
			}

			return false;
		}

		public bool is_disposable() {
			if (get_attribute_string("CCode", "destroy_function") != null) {
				return true;
			}

			foreach (Field f in fields) {
				if (f.binding == MemberBinding.INSTANCE
					&& f.variable_type.is_disposable()) {
					return true;
				}
			}

			return false;
		}

		bool is_recursive_value_type(DataType type) {
			var struct_type = type as StructValueType;
			if (struct_type != null && !struct_type.nullable) {
				var st = (Struct)struct_type.type_symbol;
				if (st == this) {
					return true;
				}
				foreach (Field f in st.fields) {
					if (f.binding == MemberBinding.INSTANCE && is_recursive_value_type(f.variable_type)) {
						return true;
					}
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

			if (base_type != null) {
				base_type.check(context);

				if (!(base_type is ValaValueType)) {
					error = true;
					Report.error(source_reference, "The base type `%s` of struct `%s` is not a struct".printf(base_type.ToString(), get_full_name()));
					return false;
				}
			}

			foreach (TypeParameter p in type_parameters) {
				p.check(context);
			}

			foreach (Field f in fields) {
				f.check(context);

				if (f.binding == MemberBinding.INSTANCE && is_recursive_value_type(f.variable_type)) {
					error = true;
					Report.error(f.source_reference, "Recursive value types are not allowed");
					return false;
				}

				if (f.binding == MemberBinding.INSTANCE && f.initializer != null) {
					error = true;
					Report.error(f.source_reference, "Instance field initializers not supported");
					return false;
				}
			}

			foreach (Constant c in constants) {
				c.check(context);
			}

			foreach (Method m in methods) {
				m.check(context);
			}

			foreach (Property prop in properties) {
				prop.check(context);
			}

			if (!external && !external_package) {
				if (base_type == null && get_fields().Count == 0 && !is_boolean_type() && !is_integer_type() && !is_floating_type()) {
					error = true;
					Report.error(source_reference, "structs cannot be empty: %s".printf(name));
				} else if (base_type != null) {
					foreach (Field f in fields) {
						if (f.binding == MemberBinding.INSTANCE) {
							error = true;
							Report.error(source_reference, "derived structs may not have instance fields");
							break;
						}
					}
				}
			}

			context.analyzer.current_source_file = old_source_file;
			context.analyzer.current_symbol = old_symbol;

			return !error;
		}
	}
}
