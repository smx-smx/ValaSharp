﻿using Vala;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.Symbols;
using Vala.Lang.Parser;
using Vala.Lang.CodeNodes;
using System.IO;

namespace Vala.Lang {
	public class VersionAttribute {
		private WeakReference<Symbol> symbol_weak = new WeakReference<Symbol>(null);

		private Symbol symbol {
			get {
				return symbol_weak.GetTarget();
			}
			set {
				symbol_weak.SetTarget(value);
			}
		}

		private bool? _deprecated;
		private bool? _experimental;

		/**
		 * Constructs a new VersionAttribute.
		 *
		 * @param symbol the owner
		 * @return a new VersionAttribute
		 * @see Vala.Symbol
		 */
		public VersionAttribute(Symbol symbol) {
			this.symbol = symbol;
		}



		/**
		 * Specifies whether this symbol has been deprecated.
		 */
		public bool deprecated {
			get {
				if (_deprecated == null) {
					_deprecated = symbol.get_attribute_bool("Version", "deprecated", false)
						|| symbol.get_attribute_string("Version", "deprecated_since") != null
						|| symbol.get_attribute_string("Version", "replacement") != null
						// [Deprecated] is deprecated
						|| symbol.get_attribute("Deprecated") != null;
				}
				return _deprecated.Value;
			}
			set {
				_deprecated = value;
				symbol.set_attribute_bool("Version", "deprecated", _deprecated.Value);
			}
		}

		/**
		 * Specifies what version this symbol has been deprecated since.
		 */
		public string deprecated_since {
			get {
				return symbol.get_attribute_string("Version", "deprecated_since")
					// [Deprecated] is deprecated
					?? symbol.get_attribute_string("Deprecated", "since");
			}
			set {
				symbol.set_attribute_string("Version", "deprecated_since", value);
			}
		}

		/**
		 * Specifies the replacement if this symbol has been deprecated.
		 */
		public string replacement {
			get {
				return symbol.get_attribute_string("Version", "replacement")
					// [Deprecated] is deprecated
					?? symbol.get_attribute_string("Deprecated", "replacement");
			}
			set {
				symbol.set_attribute_string("Version", "replacement", value);
			}
		}



		/**
		 * Specifies whether this symbol is experimental.
		 */
		public bool experimental {
			get {
				if (_experimental == null) {
					_experimental = symbol.get_attribute_bool("Version", "experimental", false)
						|| symbol.get_attribute_string("Version", "experimental_until") != null
						|| symbol.get_attribute("Experimental") != null;
				}
				return _experimental.Value;
			}
			set {
				_experimental = value;
				symbol.set_attribute_bool("Version", "experimental", value);
			}
		}

		/**
		 * Specifies until which version this symbol is experimental.
		 */
		public string experimental_until {
			get {
				return symbol.get_attribute_string("Version", "experimental_until");
			}
			set {
				symbol.set_attribute_string("Version", "experimental_until", value);
			}
		}



		/**
		 * The minimum version for {@link Vala.VersionAttribute.symbol}
		 */
		public string since {
			get {
				return symbol.get_attribute_string("Version", "since");
			}
			set {
				symbol.set_attribute_string("Version", "since", value);
			}
		}



		/**
		 * Check to see if the symbol is experimental, deprecated or not available
		 * and emit a warning if it is.
		 */
		public bool check(SourceReference source_ref = null) {
			bool result = false;

			// deprecation:
			if (symbol.external_package && deprecated) {
				string package_version = symbol.source_reference.file.installed_version;

				if (!CodeContext.get().deprecated && (package_version == null || deprecated_since == null || VersionAttribute.cmp_versions(package_version, deprecated_since) >= 0)) {
					Report.deprecated(source_ref, "%s %s%s".printf(symbol.get_full_name(), (deprecated_since == null) ? "is deprecated" : "has been deprecated since %s".printf(deprecated_since), (replacement == null) ? "" : ". Use %s".printf(replacement)));
				}
				result = true;
			}

			// availability:
			if (symbol.external_package && since != null) {
				string package_version = symbol.source_reference.file.installed_version;

				if (CodeContext.get().since_check && package_version != null && VersionAttribute.cmp_versions(package_version, since) < 0) {
					string filename = symbol.source_reference.file.filename;
					string pkg = Path.GetFileName(filename.Substring(0, filename.LastIndexOf('.')));
					Report.error(source_ref, "%s is not available in %s %s. Use %s >= %s".printf(symbol.get_full_name(), pkg, package_version, pkg, since));
				}
				result = true;
			}

			// experimental:
			if (symbol.external_package && experimental) {
				if (!CodeContext.get().experimental) {
					string package_version = symbol.source_reference.file.installed_version;
					string experimental_until = this.experimental_until;

					if (experimental_until == null || package_version == null || VersionAttribute.cmp_versions(package_version, experimental_until) < 0) {
						Report.experimental(source_ref, "%s is experimental%s".printf(symbol.get_full_name(), (experimental_until != null) ? " until %s".printf(experimental_until) : ""));
					}
				}
				result = true;
			}

			return result;
		}


		/**
		 * A simple version comparison function.
		 *
		 * @param v1str a version number
		 * @param v2str a version number
		 * @return an integer less than, equal to, or greater than zero, if v1str is <, == or > than v2str
		 * @see GLib.CompareFunc
		 */
		public static int cmp_versions(string v1str, string v2str) {
			string[] v1arr = v1str.Split('.');
			string[] v2arr = v2str.Split('.');
			int i = 0;

			while (v1arr[i] != null && v2arr[i] != null) {
				int v1num = int.Parse(v1arr[i]);
				int v2num = int.Parse(v2arr[i]);

				if (v1num < 0 || v2num < 0) {
					// invalid format
					return 0;
				}

				if (v1num > v2num) {
					return 1;
				}

				if (v1num < v2num) {
					return -1;
				}

				i++;
			}

			if (v1arr[i] != null && v2arr[i] == null) {
				return 1;
			}

			if (v1arr[i] == null && v2arr[i] != null) {
				return -1;
			}

			return 0;
		}
	}
}
