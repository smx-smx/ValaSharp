using CLanguage.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLanguage {
	/**
 * Represents a preprocessor macro replacement definition in the C code.
 */
	public class CCodeMacroReplacement : CCodeNode {
		/**
		 * The name of this macro.
		 */
		public string name { get; set; }

		/**
		 * The replacement of this macro.
		 */
		public string replacement { get; set; }

		/**
		 * The replacement expression of this macro.
		 */
		public CCodeExpression replacement_expression { get; set; }

		public CCodeMacroReplacement(string name, string replacement) {
			this.replacement = replacement;
			this.name = name;
		}

		public static CCodeMacroReplacement with_expression(string name, CCodeExpression replacement_expression) {
			CCodeMacroReplacement @this = new CCodeMacroReplacement(name, null);
			@this.replacement_expression = replacement_expression;
			return @this;
		}

		public override void write(CCodeWriter writer) {
			writer.write_indent();
			writer.write_string("#define ");
			writer.write_string(name);
			writer.write_string(" ");
			if (replacement != null) {
				writer.write_string(replacement);
			} else {
				replacement_expression.write_inner(writer);
			}
			writer.write_newline();
		}
	}

}
