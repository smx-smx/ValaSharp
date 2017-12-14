using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.Parser;

namespace Vala.Lang.CodeNodes {
	public class ValaAttribute : CodeNode, IComparable {
		/// <summary>
		/// The name of the attribute type.
		/// </summary>
		public string name { get; set; }

		/// <summary>
		/// Contains all specified attribute arguments.
		/// </summary>
		public Dictionary<string, string> args = new Dictionary<string, string>();

		/// <summary>
		/// Creates a new attribute.
		/// 
		/// <param name="name">attribute type name</param>
		/// <param name="source_reference">reference to source code</param>
		/// <returns>newly created attribute</returns>
		/// </summary>
		public ValaAttribute(string name, SourceReference source_reference = null) {
			this.name = name;
			this.source_reference = source_reference;

			if (!CodeContext.get().deprecated) {
				if (name == "Deprecated") {
					Report.deprecated(source_reference, "[Deprecated] is deprecated. Use [Version (deprecated = true, deprecated_since = \"\", replacement = \"\")]");
				} else if (name == "Experimental") {
					Report.deprecated(source_reference, "[Experimental] is deprecated. Use [Version (experimental = true, experimental_until = \"\")]");
				}
			}
		}

		/// <summary>
		/// Adds an attribute argument.
		/// 
		/// <param name="key">argument name</param>
		/// <param name="value">argument value</param>
		/// </summary>
		public void add_argument(string key, string value) {
			args[key] = value;
		}

		/// <summary>
		/// Returns whether this attribute has the specified named argument.
		/// 
		/// <param name="name">argument name</param>
		/// <returns>true if the argument has been found, false otherwise</returns>
		/// </summary>
		public bool has_argument(string name) {
			return args.ContainsKey(name);
		}

		/// <summary>
		/// Returns the string value of the specified named argument.
		/// 
		/// <param name="name">argument name</param>
		/// <returns>string value</returns>
		/// </summary>
		public string get_string(string name, string default_value = null) {
			string value;
			args.TryGetValue(name, out value);

			if (value == null) {
				return default_value;
			}

			/* remove quotes */
			var noquotes = value.Substring(1, (int)(value.Length - 2));
			/* unescape string */
			return noquotes.compress();
		}

		/// <summary>
		/// Returns the integer value of the specified named argument.
		/// 
		/// <param name="name">argument name</param>
		/// <returns>integer value</returns>
		/// </summary>
		public int get_integer(string name, int default_value = 0) {
			string value;
			args.TryGetValue(name, out value);

			if (value == null) {
				return default_value;
			}

			return int.Parse(value);
		}

		/// <summary>
		/// Returns the double value of the specified named argument.
		/// 
		/// <param name="name">argument name</param>
		/// <returns>double value</returns>
		/// </summary>
		public double get_double(string name, double default_value = 0) {
			string value;
			args.TryGetValue(name, out value);

			if (value == null) {
				return default_value;
			}

			return double.Parse(value);
		}

		/// <summary>
		/// Returns the boolean value of the specified named argument.
		/// 
		/// <param name="name">argument name</param>
		/// <returns>boolean value</returns>
		/// </summary>
		public bool get_bool(string name, bool default_value = false) {
			string value;
			args.TryGetValue(name, out value);

			if (value == null) {
				return default_value;
			}

			return bool.Parse(value);
		}

		public int CompareTo(object obj) {
			if (obj == null)
				return 1;

			ValaAttribute other = obj as ValaAttribute;
			if (other == null)
				throw new ArgumentException("Object is not a ValaAttribute");

			return this.name.CompareTo(other.name);
		}
	}
}
