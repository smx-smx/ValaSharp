using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.Code;
using Vala.Lang.CodeNodes;
using Vala.Lang.Parser;
using Vala.Lang.Types;

namespace Vala.Lang.Literals {
	/// <summary>
	/// Represents a literal `null` in the source code.
	/// </summary>
	public class NullLiteral : Literal {
		/// <summary>
		/// Creates a new null literal.
		/// 
		/// <param name="source">reference to source code</param>
		/// <returns>newly created null literal</returns>
		/// </summary>
		public NullLiteral(SourceReference source = null) {
			source_reference = source;
		}

		public override void accept(CodeVisitor visitor) {
			visitor.visit_null_literal(this);

			visitor.visit_expression(this);
		}

		public override string to_string() {
			return "null";
		}

		public override bool is_pure() {
			return true;
		}

		public override bool check(CodeContext context) {
			if (is_checked) {
				return !error;
			}

			is_checked = true;

			value_type = new NullType(source_reference);

			return !error;
		}

		public override void emit(CodeGenerator codegen) {
			codegen.visit_null_literal(this);

			codegen.visit_expression(this);
		}
	}
}
