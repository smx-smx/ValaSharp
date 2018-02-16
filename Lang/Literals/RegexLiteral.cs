﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Vala.Lang.Code;
using Vala.Lang.CodeNodes;
using Vala.Lang.Parser;

namespace Vala.Lang.Literals {
	/// <summary>
	/// Represents a regular expression literal in the source code.
	/// </summary>
	public class RegexLiteral : Literal {
		/// <summary>
		/// The literal value.
		/// </summary>
		public string value { get; set; }

		/// <summary>
		/// Creates a new regular expression literal.
		/// 
		/// <param name="value">the literal value</param>
		/// <param name="source_reference">reference to source code</param>
		/// <returns>newly created string literal</returns>
		/// </summary>
		public RegexLiteral(string value, SourceReference source_reference = null) {
			this.value = value;
			this.source_reference = source_reference;
		}

		public override void accept(CodeVisitor visitor) {
			visitor.visit_regex_literal(this);

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

			try {
				var regex = new Regex(value);
				if (regex != null) { /* Regex is valid. */ }
			} catch (Exception) {
				error = true;
				Report.error(source_reference, "Invalid regular expression `%s'.".printf(value));
				return false;
			}

			value_type = context.analyzer.regex_type.copy();

			return !error;
		}

		public override void emit(CodeGenerator codegen) {
			codegen.visit_regex_literal(this);

			codegen.visit_expression(this);
		}
	}
}
