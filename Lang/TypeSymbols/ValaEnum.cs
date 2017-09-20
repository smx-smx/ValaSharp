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

namespace Vala.Lang.TypeSymbols
{
	public class ValaEnum : TypeSymbol
	{
		/**
	 * Specifies whether this is a flags enum.
	 */
		public bool is_flags {
			get {
				if (_is_flags == null) {
					_is_flags = get_attribute("Flags") != null;
				}
				return _is_flags.Value;
			}
		}

		private List<EnumValue> values = new List<EnumValue>();
		private List<Method> methods = new List<Method>();
		private List<Constant> constants = new List<Constant>();

		private bool? _is_flags;

		/**
		 * Creates a new enum.
		 *
		 * @param name             type name
		 * @param source_reference reference to source code
		 * @return                 newly created enum
		 */
		public ValaEnum(string name, SourceReference source_reference = null, Comment comment = null) : base(name, source_reference, comment) {
		}

		/**
		 * Appends the specified enum value to the list of values.
		 *
		 * @param value an enum value
		 */
		public void add_value(EnumValue value) {
			value.access = SymbolAccessibility.PUBLIC;

			values.Add(value);
			scope.add(value.name, value);
		}

		/**
		 * Adds the specified method as a member to this enum.
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
				m.this_parameter = new Parameter("this", new EnumValueType(this));
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
		 * Adds the specified constant as a member to this enum.
		 *
		 * @param c a constant
		 */
		public override void add_constant(Constant c) {
			constants.Add(c);
			scope.add(c.name, c);
		}

		/**
		 * Returns a copy of the list of enum values.
		 *
		 * @return list of enum values
		 */
		public List<EnumValue> get_values() {
			return values;
		}

		// used by vapigen
		public void remove_all_values() {
			values.Clear();
		}

		/**
		 * Returns a copy of the list of methods.
		 *
		 * @return list of methods
		 */
		public List<Method> get_methods() {
			return methods;
		}

		/**
		 * Returns a copy of the list of constants.
		 *
		 * @return list of constants
		 */
		public List<Constant> get_constants() {
			return constants;
		}

		public override void accept(CodeVisitor visitor) {
			visitor.visit_enum(this);
		}

		public override void accept_children(CodeVisitor visitor) {
			foreach (EnumValue value in values) {
				value.accept(visitor);
			}

			foreach (Method m in methods) {
				m.accept(visitor);
			}

			foreach (Constant c in constants) {
				c.accept(visitor);
			}
		}

		public override bool is_reference_type() {
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

			if (values.Count <= 0) {
				Report.error(source_reference, "Enum `%s' requires at least one value".printf(get_full_name()));
				error = true;
				return false;
			}

			foreach (EnumValue value in values) {
				value.check(context);
			}

			foreach (Method m in methods) {
				m.check(context);
			}

			foreach (Constant c in constants) {
				c.check(context);
			}

			context.analyzer.current_source_file = old_source_file;
			context.analyzer.current_symbol = old_symbol;

			return !error;
			}
		}
}
