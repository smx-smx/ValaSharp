using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.Code;
using Vala.Lang.CodeNodes;
using Vala.Lang.Expressions;
using Vala.Lang.Parser;

namespace Vala.Lang.Literals {
	/// <summary>
	/// Represents a string literal in the source code.
	/// </summary>
	public class StringLiteral : Literal {
		/// <summary>
		/// The literal value.
		/// </summary>
		public string value { get; set; }

		public bool translate { get; set; }

		/// <summary>
		/// Creates a new string literal.
		/// 
		/// <param name="value">the literal value</param>
		/// <param name="source_reference">reference to source code</param>
		/// <returns>newly created string literal</returns>
		/// </summary>
		public StringLiteral(string value, SourceReference source_reference = null) {
			this.value = value;
			this.source_reference = source_reference;
		}

		/// <summary>
		/// Evaluates the literal string value.
		/// 
		/// <returns>the unescaped string</returns>
		/// </summary>
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
