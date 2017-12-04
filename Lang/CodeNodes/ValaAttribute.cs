using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.Parser;

namespace Vala.Lang.CodeNodes {
	public class ValaAttribute : CodeNode {
		/**
	 * The name of the attribute type.
	 */
		public string name { get; set; }

		/**
		 * Contains all specified attribute arguments.
		 */
		public Dictionary<string, string> args = new Dictionary<string, string>();

		/**
		 * Creates a new attribute.
		 *
		 * @param name             attribute type name
		 * @param source_reference reference to source code
		 * @return                 newly created attribute
		 */
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

		/**
		 * Adds an attribute argument.
		 *
		 * @param key    argument name
		 * @param value  argument value
		 */
		public void add_argument(string key, string value) {
			args[key] = value;
		}

		/**
		 * Returns whether this attribute has the specified named argument.
		 *
		 * @param name argument name
		 * @return     true if the argument has been found, false otherwise
		 */
		public bool has_argument(string name) {
			return args.ContainsKey(name);
		}

		/**
		 * Returns the string value of the specified named argument.
		 *
		 * @param name argument name
		 * @return     string value
		 */
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

		/**
		 * Returns the integer value of the specified named argument.
		 *
		 * @param name argument name
		 * @return     integer value
		 */
		public int get_integer(string name, int default_value = 0) {
			string value;
			args.TryGetValue(name, out value);

			if (value == null) {
				return default_value;
			}

			return int.Parse(value);
		}

		/**
		 * Returns the double value of the specified named argument.
		 *
		 * @param name argument name
		 * @return     double value
		 */
		public double get_double(string name, double default_value = 0) {
			string value;
			args.TryGetValue(name, out value);

			if (value == null) {
				return default_value;
			}

			return double.Parse(value);
		}

		/**
		 * Returns the boolean value of the specified named argument.
		 *
		 * @param name argument name
		 * @return     boolean value
		 */
		public bool get_bool(string name, bool default_value = false) {
			string value;
			args.TryGetValue(name, out value);

			if (value == null) {
				return default_value;
			}

			return bool.Parse(value);
		}
	}
}
