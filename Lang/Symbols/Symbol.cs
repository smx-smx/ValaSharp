﻿using Vala;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.CodeNodes;
using Vala.Lang.Parser;
using Vala.Lang.Methods;
using Vala.Lang.TypeSymbols;
using GLibPorts;

namespace Vala.Lang.Symbols {
	/// <summary>
	/// Represents a node in the symbol tree.
	/// </summary>
	public abstract class Symbol : CodeNode {

		/// <summary>
		/// The parent of this symbol.
		/// </summary>
		public Symbol parent_symbol {
			get {
				if (owner == null) {
					return null;
				} else {
					return owner.owner;
				}
			}
		}

		/// <summary>
		/// The scope this symbol is a part of
		/// </summary>
		public Scope owner {
			get {
				return _owner;
			}
			set {
				_owner = value;
				_scope.parent_scope = value;
			}
		}

		/// <summary>
		/// The symbol name.
		/// </summary>
		public string name { get; set; }

		/// <summary>
		/// Specifies whether this symbol is active.
		/// 
		/// Symbols may become inactive when they only apply to a part of a
		/// scope. This is used for local variables not declared at the beginning
		/// of the block to determine which variables need to be freed before
		/// jump statements.
		/// </summary>
		public bool active { get; set; } = true;

		/// <summary>
		/// Specifies whether this symbol has been accessed.
		/// </summary>
		public bool used { get; set; }

		/// <summary>
		/// Specifies the accessibility of this symbol. Public accessibility
		/// doesn't limit access. Default accessibility limits access to this
		/// program or library. Private accessibility limits access to instances
		/// of the contained type.
		/// </summary>
		public SymbolAccessibility access { get; set; }

		public Comment comment { get; set; }


		private VersionAttribute _version;

		/// <summary>
		/// The associated [Version] attribute
		/// </summary>
		public VersionAttribute version {
			get {
				if (_version == null) {
					_version = new VersionAttribute(this);
				}

				return _version;
			}
		}

		/// <summary>
		/// Specifies whether this method explicitly hides a member of a base
		/// type.
		/// </summary>
		public bool hides { get; set; }

		/// <summary>
		/// Check if this symbol is just internal API (and therefore doesn't need
		/// to be listed in header files for instance) by traversing parent symbols
		/// and checking their accessibility.
		/// </summary>
		public bool is_internal_symbol() {
			if (!external && external_package) {
				// non-external symbols in VAPI files are internal symbols
				return true;
			}

			for (Symbol sym = this; null != sym; sym = sym.parent_symbol) {
				if (sym.access == SymbolAccessibility.PRIVATE
					|| sym.access == SymbolAccessibility.INTERNAL) {
					return true;
				}
			}

			return false;
		}

		public bool is_private_symbol() {
			if (!external && external_package) {
				// non-external symbols in VAPI files are private symbols
				return true;
			}

			for (Symbol sym = this; null != sym; sym = sym.parent_symbol) {
				if (sym.access == SymbolAccessibility.PRIVATE) {
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// The scope this symbol opens.
		/// </summary>
		public Scope scope {
			get { return _scope; }
		}

		/// <summary>
		/// Specifies whether the implementation is external, for example in
		/// a separate C source file or in an external library.
		/// </summary>
		public bool external { get; set; }

		/// <summary>
		/// Specifies whether the implementation is in an external library.
		/// </summary>
		public bool external_package {
			get {
				return source_type == SourceFileType.PACKAGE;
			}
		}

		/// <summary>
		/// Specifies whether the implementation came from the commandline.
		/// </summary>
		public bool from_commandline {
			get {
				if (source_reference != null) {
					return source_reference.file.from_commandline;
				} else {
					return false;
				}
			}
		}

		/// <summary>
		/// Gets the SourceFileType of the source file that this symbol
		/// came from, or SourceFileType.NONE.
		/// </summary>
		public SourceFileType source_type {
			get {
				if (source_reference != null) {
					return source_reference.file.file_type;
				} else {
					return SourceFileType.NONE;
				}
			}
		}

		private WeakReference<Scope> _owner_weak = new WeakReference<Scope>(null);

		private Scope _owner {
			get {
				return _owner_weak.GetTarget();
			}
			set {
				_owner_weak.SetTarget(value);
			}
		}
		private Scope _scope;


		public Symbol(string name, SourceReference source_reference, Comment comment = null) {
			this.name = name;
			this.source_reference = source_reference;
			this.comment = comment;
			_scope = new Scope(this);
		}

		/// <summary>
		/// Returns the fully expanded name of this symbol for use in
		/// human-readable messages.
		/// 
		/// <returns>full name</returns>
		/// </summary>
		public string get_full_name() {
			if (parent_symbol == null) {
				return name;
			}

			if (name == null) {
				return parent_symbol.get_full_name();
			}

			if (parent_symbol.get_full_name() == null) {
				return name;
			}

			if (name.StartsWith(".")) {
				return "%s%s".printf(parent_symbol.get_full_name(), name);
			} else {
				return "%s.%s".printf(parent_symbol.get_full_name(), name);
			}
		}

		/// <summary>
		/// Converts a string from CamelCase to lower_case.
		/// 
		/// <param name="camel_case">a string in camel case</param>
		/// <returns>the specified string converted to lower case</returns>
		/// </summary>
		public static string camel_case_to_lower_case(string camel_case) {
			if (camel_case.Contains('_')) {
				// do not insert additional underscores if input is not real camel case
				return camel_case.ToLower();
			}

			var result_builder = new StringBuilder("");

			string i = camel_case;
			int index = 0;

			bool first = true;
			while (i.Length > 0) {
				char c = i[0];
				if (Char.IsUpper(c) && !first) {
					/* current character is upper case and
					 * we're not at the beginning */
					string t = camel_case.Substring(index - 1);
					bool prev_upper = Char.IsUpper(t[0]);
					t = i.Substring(1);

					bool next_upper = t.Length > 0 && Char.IsUpper(t[0]);
					if (!prev_upper || (i.Length >= 2 && !next_upper)) {
						/* previous character wasn't upper case or
						 * next character isn't upper case*/
						int len = result_builder.ToString().Length;
						if (len != 1 && result_builder.ToString()[len - 2] != '_') {
							/* we're not creating 1 character words */
							result_builder.Append('_');
						}
					}
				}

				result_builder.Append(Char.ToLower(c));

				first = false;
				i = i.Substring(1);
				index++;
			}

			return result_builder.ToString();
		}

		/// <summary>
		/// Converts a string from lower_case to CamelCase.
		/// 
		/// <param name="lower_case">a string in lower case</param>
		/// <returns>the specified string converted to camel case</returns>
		/// </summary>
		public static string lower_case_to_camel_case(string lower_case) {
			var result_builder = new StringBuilder("");

			ITwoWayEnumerator<char> i = lower_case.GetTwoWayEnumerator();
			i.MoveNext();
			int length = lower_case.Length;


			bool last_underscore = true;
			while (true) {
				char c = i.Current;
				if (c == '_') {
					last_underscore = true;
				} else if (Char.IsUpper(c)) {
					// original string is not lower_case, don't apply transformation
					return lower_case;
				} else if (last_underscore) {
					result_builder.Append(Char.ToUpper(c));
					last_underscore = false;
				} else {
					result_builder.Append(c);
				}

				if (!i.MoveNext())
					break;
			}

			return result_builder.ToString();
		}

		// get the top scope from where this symbol is still accessible
		public Scope get_top_accessible_scope(bool is_internal = false) {
			if (access == SymbolAccessibility.PRIVATE) {
				// private symbols are accessible within the scope where the symbol has been declared
				return owner;
			}

			if (access == SymbolAccessibility.INTERNAL) {
				is_internal = true;
			}

			if (parent_symbol == null) {
				// this is the root symbol
				if (is_internal) {
					// only accessible within the same library
					// return root scope
					return scope;
				} else {
					// unlimited access
					return null;
				}
			}

			// if this is a public symbol, it's equally accessible as the parent symbol
			return parent_symbol.get_top_accessible_scope(is_internal);
		}

		public virtual bool is_instance_member() {
			bool instance = true;
			if (this is Field) {
				var f = (Field)this;
				instance = (f.binding == MemberBinding.INSTANCE);
			} else if (this is Method) {
				var m = (Method)this;
				if (!(m is CreationMethod)) {
					instance = (m.binding == MemberBinding.INSTANCE);
				}
			} else if (this is Property) {
				var prop = (Property)this;
				instance = (prop.binding == MemberBinding.INSTANCE);
			} else if (this is EnumValue) {
				instance = false;
			} else if (this is ErrorCode) {
				instance = false;
			}

			return instance;
		}

		public virtual bool is_class_member() {
			bool isclass = true;
			if (this is Field) {
				var f = (Field)this;
				isclass = (f.binding == MemberBinding.CLASS);
			} else if (this is Method) {
				var m = (Method)this;
				if (!(m is CreationMethod)) {
					isclass = (m.binding == MemberBinding.CLASS);
				}
			} else if (this is Property) {
				var prop = (Property)this;
				isclass = (prop.binding == MemberBinding.CLASS);
			} else if (this is EnumValue) {
				isclass = false;
			} else if (this is ErrorCode) {
				isclass = false;
			}

			return isclass;
		}

		public Symbol get_hidden_member() {
			Symbol sym = null;

			if (parent_symbol is Class) {
				var cl = ((Class)parent_symbol).base_class;
				while (cl != null) {
					sym = cl.scope.lookup(name);
					if (sym != null && sym.access != SymbolAccessibility.PRIVATE) {
						return sym;
					}
					cl = cl.base_class;
				}
			} else if (parent_symbol is Struct) {
				var st = ((Struct)parent_symbol).base_struct;
				while (st != null) {
					sym = st.scope.lookup(name);
					if (sym != null && sym.access != SymbolAccessibility.PRIVATE) {
						return sym;
					}
					st = st.base_struct;
				}
			}

			return null;
		}

		// check whether this symbol is at least as accessible as the specified symbol
		public bool is_accessible(Symbol sym) {
			Scope sym_scope = sym.get_top_accessible_scope();
			Scope this_scope = this.get_top_accessible_scope();
			if ((sym_scope == null && this_scope != null)
				|| (sym_scope != null && !sym_scope.is_subscope_of(this_scope))) {
				return false;
			}

			return true;
		}

		public virtual void add_namespace(Namespace ns) {
			Report.error(ns.source_reference, "unexpected declaration");
		}

		public virtual void add_class(Class cl) {
			Report.error(cl.source_reference, "unexpected declaration");
		}

		public virtual void add_interface(Interface iface) {
			Report.error(iface.source_reference, "unexpected declaration");
		}

		public virtual void add_struct(Struct st) {
			Report.error(st.source_reference, "unexpected declaration");
		}

		public virtual void add_enum(ValaEnum en) {
			Report.error(en.source_reference, "unexpected declaration");
		}

		public virtual void add_error_domain(ErrorDomain edomain) {
			Report.error(edomain.source_reference, "unexpected declaration");
		}

		public virtual void add_delegate(ValaDelegate d) {
			Report.error(d.source_reference, "unexpected declaration");
		}

		public virtual void add_constant(Constant constant) {
			Report.error(constant.source_reference, "unexpected declaration");
		}

		public virtual void add_field(Field f) {
			Report.error(f.source_reference, "unexpected declaration");
		}

		public virtual void add_method(Method m) {
			Report.error(m.source_reference, "unexpected declaration");
		}

		public virtual void add_property(Property prop) {
			Report.error(prop.source_reference, "unexpected declaration");
		}

		public virtual void add_signal(Signal sig) {
			Report.error(sig.source_reference, "unexpected declaration");
		}

		public virtual void add_constructor(Constructor c) {
			Report.error(c.source_reference, "unexpected declaration");
		}

		public virtual void add_destructor(Destructor d) {
			Report.error(d.source_reference, "unexpected declaration");
		}
	}
}
