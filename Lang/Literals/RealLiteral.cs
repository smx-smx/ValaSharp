using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.Code;
using Vala.Lang.CodeNodes;
using Vala.Lang.Parser;
using Vala.Lang.Types;
using Vala.Lang.TypeSymbols;

namespace Vala.Lang.Literals
{
	/**
	 * Represents a real literal in the source code.
	 */
	public class RealLiteral : Literal
	{
		/**
		 * The literal value.
		 */
		public string value { get; set; }

		/**
		 * Creates a new real literal.
		 *
		 * @param r      literal value
		 * @param source reference to source code
		 * @return       newly created real literal
		 */
		public RealLiteral(string r, SourceReference source = null) {
			value = r;
			source_reference = source;
		}

		public override void accept(CodeVisitor visitor) {
			visitor.visit_real_literal(this);

			visitor.visit_expression(this);
		}

		/**
		 * Returns the type name of the value this literal represents.
		 *
		 * @return the name of literal type
		 */
		public string get_type_name() {
			if (value.EndsWith("f") || value.EndsWith("F")) {
				return "float";
			}

			return "double";
		}

		public override bool is_pure() {
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

			var st = (Struct)context.analyzer.root_symbol.scope.lookup(get_type_name());
			// ensure attributes are already processed
			st.check(context);

			value_type = new FloatingType(st);

			return !error;
		}

		public override void emit(CodeGenerator codegen) {
			codegen.visit_real_literal(this);

			codegen.visit_expression(this);
		}
	}

}
