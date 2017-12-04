using CLanguage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vala.Lang {
	/**
	 * Represents an enum in the C code.
	 */
	public class CCodeEnum : CCodeNode {
		/**
		 * The name of this enum.
		 */
		public string name { get; set; }

		private List<CCodeEnumValue> values = new List<CCodeEnumValue>();

		public CCodeEnum(string name = null) {
			this.name = name;
		}

		/**
		 * Adds the specified value to this enum.
		 *
		 * @param value optional numerical value
		 */
		public void add_value(CCodeEnumValue value) {
			values.Add(value);
		}

		public override void write(CCodeWriter writer) {
			if (name != null) {
				writer.write_string("typedef ");
			}
			writer.write_string("enum ");
			writer.write_begin_block();
			bool first = true;
			foreach (CCodeEnumValue value in values) {
				if (!first) {
					writer.write_string(",");
					writer.write_newline();
				}
				writer.write_indent();
				value.write(writer);
				first = false;
			}
			if (!first) {
				writer.write_newline();
			}
			writer.write_end_block();
			if (name != null) {
				writer.write_string(" ");
				writer.write_string(name);
			}
			if (modifiers.HasFlag(CCodeModifiers.DEPRECATED)) {
				writer.write_string(" G_GNUC_DEPRECATED");
			}
			writer.write_string(";");
			writer.write_newline();
		}
	}
}
