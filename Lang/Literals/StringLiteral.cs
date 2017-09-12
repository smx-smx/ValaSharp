using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.Code;
using Vala.Lang.CodeNodes;
using Vala.Lang.Expressions;
using Vala.Lang.Parser;

namespace Vala.Lang.Literals
{
	/**
	 * Represents a string literal in the source code.
	 */
	public class StringLiteral : Literal
	{
		/**
		 * The literal value.
		 */
		public string value { get; set; }

		public bool translate { get; set; }

		/**
		 * Creates a new string literal.
		 *
		 * @param value             the literal value
		 * @param source_reference  reference to source code
		 * @return                  newly created string literal
		 */
		public StringLiteral(string value, SourceReference source_reference = null) {
			this.value = value;
			this.source_reference = source_reference;
		}

		/**
		 * Evaluates the literal string value.
		 *
		 * @return the unescaped string
		 */
		public string eval() {
			if (value == null) {
				return null;
			}

			/* remove quotes */
			var noquotes = value.Substring(1, (int)(value.Length - 2));
			/* unescape string */
			return noquotes.compress();
		}

		public override void accept(CodeVisitor visitor) {
			visitor.visit_string_literal(this);

			visitor.visit_expression(this);
		}

		public override bool is_pure() {
			return true;
		}

		public override bool is_non_null() {
			return true;
		}

		public override string to_string() {
			return value;
		}

		public override bool check(CodeContext context) {
			if (is_checked) {
				return !error;
			}

			is_checked = true;

			value_type = context.analyzer.string_type.copy();

			return !error;
		}

		public override void emit(CodeGenerator codegen) {
			codegen.visit_string_literal(this);

			codegen.visit_expression(this);
		}

		public static StringLiteral get_format_literal(Expression expr) {
			var format_literal = expr as StringLiteral;
			if (format_literal != null) {
				return format_literal;
			}

			var call = expr as MethodCall;
			if (call != null) {
				return call.get_format_literal();
			}

			return null;
		}
	}

}
