using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.Code;
using Vala.Lang.Expressions;
using Vala.Lang.Parser;
using Vala.Lang.Symbols;
using Vala.Lang.Types;

namespace Vala.Lang.CodeNodes {
	/// <summary>
	/// Represents a part of the parsed source code.
	/// 
	/// Code nodes get created by the parser and are used throughout the whole
	/// compilation process.
	/// </summary>
	public abstract class CodeNode {

		private WeakReference<CodeNode> parent_node_weak = new WeakReference<CodeNode>(null);

		/**
	 * Parent of this code node.
	 */
		public CodeNode parent_node {
			get {
				return parent_node_weak.GetTarget();
			}
			set {
				parent_node_weak.SetTarget(value);
			}
		}

		/**
		 * References the location in the source file where this code node has
		 * been written.
		 */
		public SourceReference source_reference { get; set; }

		public bool unreachable { get; set; }

		/**
		 * Contains all attributes that have been specified for this code node.
		 */
		public List<ValaAttribute> attributes = new List<ValaAttribute>();

		public string type_name {
			get { return this.GetType().Name; }
		}

		public bool is_checked { get; set; }

		/**
		 * Specifies whether a fatal error has been detected in this code node.
		 */
		public bool error { get; set; }

		/**
		 * Specifies that this node or a child node may throw an exception.
		 */
		public bool tree_can_fail {
			get { return _error_types != null && _error_types.Count > 0; }
		}

		private List<DataType> _error_types;
		private static List<DataType> _empty_type_list;
		private AttributeCache[] attributes_cache = { };

		static int last_temp_nr = 0;
		static int next_attribute_cache_index = 0;

		/**
		 * Specifies the exceptions that can be thrown by this node or a child node
		 */
		public List<DataType> get_error_types() {
			if (_error_types != null) {
				return _error_types;
			}
			if (_empty_type_list == null) {
				_empty_type_list = new List<DataType>();
			}
			return _empty_type_list;
		}

		/**
		 * Adds an error type to the exceptions that can be thrown by this node
		 * or a child node 
		 */
		public void add_error_type(DataType error_type) {
			if (_error_types == null) {
				_error_types = new List<DataType>();
			}
			_error_types.Add(error_type);
			error_type.parent_node = this;
		}

		/**
		 * Adds a collection of error types to the exceptions that can be thrown by this node
		 * or a child node 
		 */
		public void add_error_types(List<DataType> error_types) {
			foreach (DataType error_type in error_types) {
				add_error_type(error_type);
			}
		}

		/**
		 * Visits this code node with the specified CodeVisitor.
		 *
		 * @param visitor the visitor to be called while traversing
		 */
		public virtual void accept(CodeVisitor visitor) {
		}

		/**
		 * Visits all children of this code node with the specified CodeVisitor.
		 *
		 * @param visitor the visitor to be called while traversing
		 */
		public virtual void accept_children(CodeVisitor visitor) {
		}

		public virtual bool check(CodeContext context) {
			return true;
		}

		public virtual void emit(CodeGenerator codegen) {
		}

		public virtual void replace_type(DataType old_type, DataType new_type) {
		}

		public virtual void replace_expression(Expression old_node, Expression new_node) {
		}

		/**
		 * Returns the specified attribute.
		 *
		 * @param name attribute name
		 * @return     attribute
		 */
		public ValaAttribute get_attribute(string name) {
			// FIXME: use hash table
			foreach (ValaAttribute a in attributes) {
				if (a.name == name) {
					return a;
				}
			}

			return null;
		}

		/**
		 * Returns true if the specified attribute argument is set.
		 *
		 * @param  attribute attribute name
		 * @param  argument  argument name
		 * @return           true if the attribute has the given argument
		 */
		public bool has_attribute_argument(string attribute, string argument) {
			var a = get_attribute(attribute);
			if (a == null) {
				return false;
			}
			return a.has_argument(argument);
		}

		/**
		 * Sets the specified named attribute to this code node.
		 *
		 * @param name  attribute name
		 * @param value true to add the attribute, false to remove it
		 */
		public void set_attribute(string name, bool value, SourceReference source_reference = null) {
			var a = get_attribute(name);
			if (value && a == null) {
				attributes.Add(new ValaAttribute(name, source_reference));
			} else if (!value && a != null) {
				attributes.Remove(a);
			}
		}

		/**
		 * Remove the specified named attribute argument
		 *
		 * @param attribute attribute name
		 * @param argument  argument name
		 */
		public void remove_attribute_argument(string attribute, string argument) {
			var a = get_attribute(attribute);
			if (a != null) {
				a.args.Remove(argument);
				if (a.args.Count == 0) {
					attributes.Remove(a);
				}
			}
		}

		/**
		 * Returns the string value of the specified attribute argument.
		 *
		 * @param attribute attribute name
		 * @param argument  argument name
		 * @return          string value
		 */
		public string get_attribute_string(string attribute, string argument, string default_value = null) {
			var a = get_attribute(attribute);
			if (a == null) {
				return default_value;
			}
			return a.get_string(argument, default_value);
		}

		/**
		 * Returns the integer value of the specified attribute argument.
		 *
		 * @param attribute attribute name
		 * @param argument  argument name
		 * @return          integer value
		 */
		public int get_attribute_integer(string attribute, string argument, int default_value = 0) {
			var a = get_attribute(attribute);
			if (a == null) {
				return default_value;
			}
			return a.get_integer(argument, default_value);
		}

		/**
		 * Returns the double value of the specified attribute argument.
		 *
		 * @param attribute attribute name
		 * @param argument  argument name
		 * @return          double value
		 */
		public double get_attribute_double(string attribute, string argument, double default_value = 0) {
			if (attributes == null) {
				return default_value;
			}
			var a = get_attribute(attribute);
			if (a == null) {
				return default_value;
			}
			return a.get_double(argument, default_value);
		}

		/**
		 * Returns the bool value of the specified attribute argument.
		 *
		 * @param attribute attribute name
		 * @param argument  argument name
		 * @return          bool value
		 */
		public bool get_attribute_bool(string attribute, string argument, bool default_value = false) {
			if (attributes == null) {
				return default_value;
			}
			var a = get_attribute(attribute);
			if (a == null) {
				return default_value;
			}
			return a.get_bool(argument, default_value);
		}

		/**
		 * Sets the string value of the specified attribute argument.
		 *
		 * @param attribute attribute name
		 * @param argument  argument name
		 * @param value     string value
		 */
		public void set_attribute_string(string attribute, string argument, string value, SourceReference source_reference = null) {
			if (value == null) {
				remove_attribute_argument(attribute, argument);
				return;
			}

			var a = get_attribute(attribute);
			if (a == null) {
				a = new ValaAttribute(attribute, source_reference);
				attributes.Add(a);
			}
			a.add_argument(argument, "\"%s\"".printf(value));
		}

		/**
		 * Sets the integer value of the specified attribute argument.
		 *
		 * @param attribute attribute name
		 * @param argument  argument name
		 * @param value     integer value
		 */
		public void set_attribute_integer(string attribute, string argument, int value, SourceReference source_reference = null) {
			var a = get_attribute(attribute);
			if (a == null) {
				a = new ValaAttribute(attribute, source_reference);
				attributes.Add(a);
			}
			a.add_argument(argument, value.ToString());
		}

		/**
		 * Sets the integer value of the specified attribute argument.
		 *
		 * @param attribute attribute name
		 * @param argument  argument name
		 * @param value     double value
		 */
		public void set_attribute_double(string attribute, string argument, double value, SourceReference source_reference = null) {
			var a = get_attribute(attribute);
			if (a == null) {
				a = new ValaAttribute(attribute, source_reference);
				attributes.Add(a);
			}

			a.add_argument(argument, value.ToString("0.00", CultureInfo.InvariantCulture));
		}

		/**
		 * Sets the boolean value of the specified attribute argument.
		 *
		 * @param attribute attribute name
		 * @param argument  argument name
		 * @param value     bool value
		 */
		public void set_attribute_bool(string attribute, string argument, bool value, SourceReference source_reference = null) {
			var a = get_attribute(attribute);
			if (a == null) {
				a = new ValaAttribute(attribute, source_reference);
				attributes.Add(a);
			}
			a.add_argument(argument, value.ToString());
		}

		/**
		 * Returns the attribute cache at the specified index.
		 *
		 * @param index attribute cache index
		 * @return      attribute cache
		 */
		public AttributeCache get_attribute_cache(int index) {
			if (index >= attributes_cache.Length) {
				return null;
			}
			return attributes_cache[index];
		}

		/**
		 * Sets the specified attribute cache to this code node.
		 *
		 * @param index attribute cache index
		 * @param cache attribute cache
		 */
		public void set_attribute_cache(int index, AttributeCache cache) {
			if (index >= attributes_cache.Length) {
				Array.Resize(ref attributes_cache, index * 2 + 1);
			}
			attributes_cache[index] = cache;
		}

		/**
		 * Returns a string that represents this code node.
		 *
		 * @return a string representation
		 */
		public virtual string to_string() {
			var str = new StringBuilder();

			str.Append("/* ");

			if (source_reference != null) {
				str.Append("@").Append(source_reference.ToString());
			}

			return str.Append(" */").ToString();
		}

		public virtual void get_defined_variables(ICollection<Variable> collection) {
		}

		public virtual void get_used_variables(ICollection<Variable> collection) {
		}

		public static string get_temp_name() {
			return "." + (++last_temp_nr).ToString();
		}

		/**
		 * Returns a new cache index for accessing the attributes cache of code nodes
		 *
		 * @return a new cache index
		 */
		public static int get_attribute_cache_index() {
			return next_attribute_cache_index++;
		}
	}

	public class AttributeCache { }
}
