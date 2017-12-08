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
	/**
	 * Represents a literal `null` in the source code.
	 */
	public class NullLiteral : Literal {
		/**
		 * Creates a new null literal.
		 *
		 * @param source reference to source code
		 * @return       newly created null literal
		 */
		public NullLiteral(SourceReference source = null) {
			source_reference = source;
		}

		public override void accept(CodeVisitor visitor) {
			visitor.visit_null_literal(this);

			visitor.visit_expression(this);
		}

		public override string ToString() {
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
