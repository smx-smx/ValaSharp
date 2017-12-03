using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CCodeGen.Modules;
using CLanguage;
using CLanguage.Statements;
using Vala;
using Vala.Lang.CodeNodes;
using Vala.Lang.Symbols;
using Vala.Lang.Types;
using Vala.Lang.TypeSymbols;

namespace CCodeGen
{
	/**
	 * C function to register an interface at runtime.
	 */
	public class InterfaceRegisterFunction : TypeRegisterFunction
	{
		/**
		 * Specifies the interface to be registered.
		 */
		public Interface interface_reference { get; set; }

		public InterfaceRegisterFunction(Interface iface, CodeContext context) {
			interface_reference = iface;
			this.context = context;
		}

		public override TypeSymbol get_type_declaration() {
			return interface_reference;
		}

		public override string get_type_struct_name() {
			return CCodeBaseModule.get_ccode_type_name(interface_reference);
		}

		public override string get_base_init_func_name() {
			return "%s_base_init".printf(CCodeBaseModule.get_ccode_lower_case_name(interface_reference));
		}

		public override string get_class_finalize_func_name() {
			return "NULL";
		}

		public override string get_base_finalize_func_name() {
			return "NULL";
		}

		public override string get_class_init_func_name() {
			return "NULL";
		}

		public override string get_instance_struct_size() {
			return "0";
		}

		public override string get_instance_init_func_name() {
			return "NULL";
		}

		public override string get_parent_type_name() {
			return "G_TYPE_INTERFACE";
		}

		public override SymbolAccessibility get_accessibility() {
			return interface_reference.access;
		}

		public override void get_type_interface_init_statements(CCodeBlock block, bool plugin) {
			/* register all prerequisites */
			foreach (DataType prereq_ref in interface_reference.get_prerequisites()) {
				var prereq = prereq_ref.data_type;

				var func = new CCodeFunctionCall(new CCodeIdentifier("g_type_interface_add_prerequisite"));
				func.add_argument(new CCodeIdentifier("%s_type_id".printf(CCodeBaseModule.get_ccode_lower_case_name(interface_reference))));
				func.add_argument(new CCodeIdentifier(CCodeBaseModule.get_ccode_type_id(prereq)));

				block.add_statement(new CCodeExpressionStatement(func));
			}

			((CCodeBaseModule)context.codegen).register_dbus_info(block, interface_reference);
		}
	}
}
