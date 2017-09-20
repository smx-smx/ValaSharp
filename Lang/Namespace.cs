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
using Vala.Lang.TypeSymbols;

namespace Vala.Lang
{
	/// <summary>
	/// Represents a namespace declaration in the source code.
	/// </summary>
	public class Namespace : Symbol
	{
		private List<Class> classes = new List<Class>();
		private List<Interface> interfaces = new List<Interface>();
		private List<Struct> structs = new List<Struct>();
		private List<ValaEnum> enums = new List<ValaEnum>();
		private List<ErrorDomain> error_domains = new List<ErrorDomain>();
		private List<ValaDelegate> delegates = new List<ValaDelegate>();
		private List<Constant> constants = new List<Constant>();
		private List<Field> fields = new List<Field>();
		private List<Method> methods = new List<Method>();

		private List<Comment> comments = new List<Comment>();

		private List<Namespace> namespaces = new List<Namespace>();

		private List<UsingDirective> using_directives = new List<UsingDirective>();

		/**
		 * Creates a new namespace.
		 *
		 * @param name             namespace name
		 * @param source_reference reference to source code
		 * @return                 newly created namespace
		 */
		public Namespace(string name, SourceReference source_reference = null) : base(name, source_reference) {
			access = SymbolAccessibility.PUBLIC;
		}

		/**
		 * Adds a new using directive with the specified namespace.
		 *
		 * @param ns reference to namespace
		 */
		public void add_using_directive(UsingDirective ns) {
			using_directives.Add(ns);
		}

		public void add_comment(Comment comment) {
			comments.Add(comment);
		}

		/**
		 * Returns a copy of the list of namespaces.
		 *
		 * @return comment list
		 */
		public List<Comment> get_comments() {
			return comments;
		}

		/**
		 * Adds the specified namespace to this source file.
		 *
		 * @param ns a namespace
		 */
		public override void add_namespace(Namespace ns) {
			if (ns.owner == null) {
				ns.source_reference.file.add_node(ns);
			}

			if (scope.lookup(ns.name) is Namespace) {
				// merge if namespace already exists
				var old_ns = (Namespace)scope.lookup(ns.name);
				if (old_ns.external_package && !ns.external_package) {
					old_ns.source_reference = ns.source_reference;
				}

				foreach (var using_directive in ns.using_directives) {
					old_ns.add_using_directive(using_directive);
				}
				foreach (Namespace sub_ns in ns.get_namespaces()) {
					old_ns.add_namespace(sub_ns);
				}
				foreach (Class cl in ns.get_classes()) {
					old_ns.add_class(cl);
				}
				foreach (Struct st in ns.get_structs()) {
					old_ns.add_struct(st);
				}
				foreach (Interface iface in ns.get_interfaces()) {
					old_ns.add_interface(iface);
				}
				foreach (ValaDelegate d in ns.get_delegates()) {
					old_ns.add_delegate(d);
				}
				foreach (ValaEnum en in ns.get_enums()) {
					old_ns.add_enum(en);
				}
				foreach (ErrorDomain ed in ns.get_error_domains()) {
					old_ns.add_error_domain(ed);
				}
				foreach (Constant c in ns.get_constants()) {
					old_ns.add_constant(c);
				}
				foreach (Field f in ns.get_fields()) {
					old_ns.add_field(f);
				}
				foreach (Method m in ns.get_methods()) {
					old_ns.add_method(m);
				}
				foreach (Comment c in ns.get_comments()) {
					old_ns.add_comment(c);
				}
				foreach (ValaAttribute a in ns.attributes) {
					if (old_ns.get_attribute(a.name) == null) {
						old_ns.attributes.Add(a);
					}
				}
			} else {
				namespaces.Add(ns);
				scope.add(ns.name, ns);
			}
		}

		/**
		 * Returns a copy of the list of namespaces.
		 *
		 * @return namespace list
		 */
		public List<Namespace> get_namespaces() {
			return namespaces;
		}

		/**
		 * Adds the specified class to this namespace.
		 *
		 * @param cl a class
		 */
		public override void add_class(Class cl) {
			// namespaces do not support private memebers
			if (cl.access == SymbolAccessibility.PRIVATE) {
				cl.access = SymbolAccessibility.INTERNAL;
			}

			if (cl.owner == null) {
				cl.source_reference.file.add_node(cl);
			}

			classes.Add(cl);
			scope.add(cl.name, cl);
		}

		/**
		 * Adds the specified interface to this namespace.
		 *
		 * @param iface an interface
		 */
		public override void add_interface(Interface iface) {
			// namespaces do not support private memebers
			if (iface.access == SymbolAccessibility.PRIVATE) {
				iface.access = SymbolAccessibility.INTERNAL;
			}

			if (iface.owner == null) {
				iface.source_reference.file.add_node(iface);
			}

			interfaces.Add(iface);
			scope.add(iface.name, iface);

		}

		/**
		 * Adds the specified struct to this namespace.
		 *
		 * @param st a struct
		 */
		public override void add_struct(Struct st) {
			// namespaces do not support private memebers
			if (st.access == SymbolAccessibility.PRIVATE) {
				st.access = SymbolAccessibility.INTERNAL;
			}

			if (st.owner == null) {
				st.source_reference.file.add_node(st);
			}

			structs.Add(st);
			scope.add(st.name, st);
		}

		/**
		 * Removes the specified struct from this namespace.
		 *
		 * @param st a struct
		 */
		public void remove_struct(Struct st) {
			structs.Remove(st);
			scope.remove(st.name);
		}

		/**
		 * Adds the specified enum to this namespace.
		 *
		 * @param en an enum
		 */
		public override void add_enum(ValaEnum en) {
			// namespaces do not support private memebers
			if (en.access == SymbolAccessibility.PRIVATE) {
				en.access = SymbolAccessibility.INTERNAL;
			}

			if (en.owner == null) {
				en.source_reference.file.add_node(en);
			}

			enums.Add(en);
			scope.add(en.name, en);
		}

		/**
		 * Adds the specified error domain to this namespace.
		 *
		 * @param edomain an error domain
		 */
		public override void add_error_domain(ErrorDomain edomain) {
			// namespaces do not support private memebers
			if (edomain.access == SymbolAccessibility.PRIVATE) {
				edomain.access = SymbolAccessibility.INTERNAL;
			}

			if (edomain.owner == null) {
				edomain.source_reference.file.add_node(edomain);
			}

			error_domains.Add(edomain);
			scope.add(edomain.name, edomain);
		}

		/**
		 * Adds the specified delegate to this namespace.
		 *
		 * @param d a delegate
		 */
		public override void add_delegate(ValaDelegate d) {
			// namespaces do not support private memebers
			if (d.access == SymbolAccessibility.PRIVATE) {
				d.access = SymbolAccessibility.INTERNAL;
			}

			if (d.owner == null) {
				d.source_reference.file.add_node(d);
			}

			delegates.Add(d);
			scope.add(d.name, d);
		}

		/**
		 * Returns a copy of the list of structs.
		 *
		 * @return struct list
		 */
		public List<Struct> get_structs() {
			return structs;
		}

		/**
		 * Returns a copy of the list of classes.
		 *
		 * @return class list
		 */
		public List<Class> get_classes() {
			return classes;
		}

		/**
		 * Returns a copy of the list of interfaces.
		 *
		 * @return interface list
		 */
		public List<Interface> get_interfaces() {
			return interfaces;
		}

		/**
		 * Returns a copy of the list of enums.
		 *
		 * @return enum list
		 */
		public List<ValaEnum> get_enums() {
			return enums;
		}

		/**
		 * Returns a copy of the list of error domains.
		 *
		 * @return error domain list
		 */
		public List<ErrorDomain> get_error_domains() {
			return error_domains;
		}

		/**
		 * Returns a copy of the list of fields.
		 *
		 * @return field list
		 */
		public List<Field> get_fields() {
			return fields;
		}

		/**
		 * Returns a copy of the list of constants.
		 *
		 * @return constant list
		 */
		public List<Constant> get_constants() {
			return constants;
		}

		/**
		 * Returns a copy of the list of delegates.
		 *
		 * @return delegate list
		 */
		public List<ValaDelegate> get_delegates() {
			return delegates;
		}

		/**
		 * Returns a copy of the list of methods.
		 *
		 * @return method list
		 */
		public List<Method> get_methods() {
			return methods;
		}

		/**
		 * Adds the specified constant to this namespace.
		 *
		 * @param constant a constant
		 */
		public override void add_constant(Constant constant) {
			// namespaces do not support private memebers
			if (constant.access == SymbolAccessibility.PRIVATE) {
				constant.access = SymbolAccessibility.INTERNAL;
			}

			if (constant.owner == null) {
				constant.source_reference.file.add_node(constant);
			}

			constants.Add(constant);
			scope.add(constant.name, constant);
		}

		/**
		 * Adds the specified field to this namespace.
		 *
		 * @param f a field
		 */
		public override void add_field(Field f) {
			if (f.binding == MemberBinding.INSTANCE) {
				// default to static member binding
				f.binding = MemberBinding.STATIC;
			}

			// namespaces do not support private memebers
			if (f.access == SymbolAccessibility.PRIVATE) {
				f.access = SymbolAccessibility.INTERNAL;
			}

			if (f.binding == MemberBinding.INSTANCE) {
				Report.error(f.source_reference, "instance members are not allowed outside of data types");
				f.error = true;
				return;
			} else if (f.binding == MemberBinding.CLASS) {
				Report.error(f.source_reference, "class members are not allowed outside of classes");
				f.error = true;
				return;
			}

			if (f.owner == null) {
				f.source_reference.file.add_node(f);
			}

			fields.Add(f);
			scope.add(f.name, f);
		}

		/**
		 * Adds the specified method to this namespace.
		 *
		 * @param m a method
		 */
		public override void add_method(Method m) {
			if (m.binding == MemberBinding.INSTANCE) {
				// default to static member binding
				m.binding = MemberBinding.STATIC;
			}

			// namespaces do not support private memebers
			if (m.access == SymbolAccessibility.PRIVATE) {
				m.access = SymbolAccessibility.INTERNAL;
			}

			if (m is CreationMethod) {
				Report.error(m.source_reference, "construction methods may only be declared within classes and structs");
				m.error = true;
				return;
			}
			if (m.binding == MemberBinding.INSTANCE) {
				Report.error(m.source_reference, "instance members are not allowed outside of data types");
				m.error = true;
				return;
			} else if (m.binding == MemberBinding.CLASS) {
				Report.error(m.source_reference, "class members are not allowed outside of classes");
				m.error = true;
				return;
			}
			if (!(m.return_type is VoidType) && m.get_postconditions().Count > 0) {
				m.result_var = new LocalVariable(m.return_type.copy(), "result", null, source_reference);
				m.result_var.is_result = true;
			}

			if (m.owner == null) {
				m.source_reference.file.add_node(m);
			}

			methods.Add(m);
			scope.add(m.name, m);
		}

		public override void accept(CodeVisitor visitor) {
			visitor.visit_namespace(this);
		}

		public override void accept_children(CodeVisitor visitor) {
			foreach (UsingDirective ns_ref in using_directives) {
				ns_ref.accept(visitor);
			}

			foreach (Namespace ns in namespaces) {
				ns.accept(visitor);
			}

			/* process enums first to avoid order problems in C code */
			foreach (ValaEnum en in enums) {
				en.accept(visitor);
			}

			foreach (ErrorDomain edomain in error_domains) {
				edomain.accept(visitor);
			}

			foreach (Class cl in classes) {
				cl.accept(visitor);
			}

			foreach (Interface iface in interfaces) {
				iface.accept(visitor);
			}

			foreach (Struct st in structs) {
				st.accept(visitor);
			}

			foreach (ValaDelegate d in delegates) {
				d.accept(visitor);
			}

			foreach (Constant c in constants) {
				c.accept(visitor);
			}

			foreach (Field f in fields) {
				f.accept(visitor);
			}

			foreach (Method m in methods) {
				m.accept(visitor);
			}
		}

		public override bool check(CodeContext context) {
			if (is_checked) {
				return !error;
			}

			is_checked = true;

			var a = get_attribute("CCode");
			if (a != null && a.has_argument("gir_namespace")) {
				source_reference.file.gir_namespace = a.get_string("gir_namespace");
			}
			if (a != null && a.has_argument("gir_version")) {
				source_reference.file.gir_version = a.get_string("gir_version");
			}

			foreach (Namespace ns in namespaces) {
				ns.check(context);
			}

			return !error;
			}
		}
}
