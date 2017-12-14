using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.Code;
using Vala.Lang.CodeNodes;
using Vala.Lang.Parser;

namespace Vala.Lang.Literals {
	/// <summary>
	/// Represents a literal boolean, i.e. true or false.
	/// </summary>
	public class BooleanLiteral : Literal {
		/// <summary>
		/// The literal value.
		/// </summary>
		public bool value { get; set; }

		/// <summary>
		/// Creates a new boolean literal.
		/// 
		/// <param name="b">boolean value</param>
		/// <param name="source">reference to source code</param>
		/// <returns>newly created boolean literal</returns>
		/// </summary>
		public BooleanLiteral(bool b, SourceReference source = null) {
			value = b;
			source_reference = source;
		}

		public override void accept(CodeVisitor visitor) {
			visitor.visit_boolean_literal(this);

			visitor.visit_expression(this);
		}

		public override string ToString() {
			if (value) {
				return "true";
			} else {
				return "false";
			}
		}

		public override bool is_pure() {
			return true;
		}

		public override bool check(CodeContext context) {
			if (is_checked) {
				return !error;
			}

			is_checked = true;

			value_type = context.analyzer.bool_type;

			return !error;
		}

		public override void emit(CodeGenerator codegen) {
			codegen.visit_boolean_literal(this);

			codegen.visit_expression(this);
		}
	}
}
