using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala;

namespace CLanguage {
	/// <summary>
	/// Represents a function pointer declarator in the C code.
	/// </summary>
	public class CCodeFunctionDeclarator : CCodeDeclarator {
		/// <summary>
		/// The declarator name.
		/// </summary>
		public string name { get; set; }

		private List<CCodeParameter> parameters = new List<CCodeParameter>();

		public CCodeFunctionDeclarator(string name) {
			this.name = name;
		}

		/// <summary>
		/// Appends the specified parameter to the list of function parameters.
		/// 
		/// <param name="param">a formal parameter</param>
		/// </summary>
		public void add_parameter(CCodeParameter param) {
			parameters.Add(param);
		}

		public override void write(CCodeWriter writer) {
			write_declaration(writer);
		}

		public override void write_declaration(CCodeWriter writer) {
			writer.write_string("(*");
			writer.write_string(name);
			writer.write_string(") (");

			bool has_args = (modifiers.HasFlag(CCodeModifiers.PRINTF) || modifiers.HasFlag(CCodeModifiers.SCANF));
			int i = 0;
			int format_arg_index = -1;
			int args_index = -1;
			foreach (CCodeParameter param in parameters) {
				if (i > 0) {
					writer.write_string(", ");
				}
				param.write(writer);
				if (param.modifiers.HasFlag(CCodeModifiers.FORMAT_ARG)) {
					format_arg_index = i;
				}
				if (has_args && param.ellipsis) {
					args_index = i;
				} else if (has_args && param.type_name == "va_list" && format_arg_index < 0) {
					format_arg_index = i - 1;
				}
				i++;
			}

			writer.write_string(")");

			if (modifiers.HasFlag(CCodeModifiers.DEPRECATED)) {
				writer.write_string(" G_GNUC_DEPRECATED");
			}

			if (modifiers.HasFlag(CCodeModifiers.PRINTF)) {
				format_arg_index = (format_arg_index >= 0 ? format_arg_index + 1 : args_index);
				writer.write_string(" G_GNUC_PRINTF(%d,%d)".printf(format_arg_index, args_index + 1));
			} else if (modifiers.HasFlag(CCodeModifiers.SCANF)) {
				format_arg_index = (format_arg_index >= 0 ? format_arg_index + 1 : args_index);
				writer.write_string(" G_GNUC_SCANF(%d,%d)".printf(format_arg_index, args_index + 1));
			} else if (format_arg_index >= 0) {
				writer.write_string(" G_GNUC_FORMAT(%d)".printf(format_arg_index + 1));
			}
		}
	}

}
