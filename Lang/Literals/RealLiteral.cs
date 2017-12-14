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

namespace Vala.Lang.Literals {
	/// <summary>
	/// Represents a real literal in the source code.
	/// </summary>
	public class RealLiteral : Literal {
		/// <summary>
		/// The literal value.
		/// </summary>
		public string value { get; set; }

		/// <summary>
		/// Creates a new real literal.
		/// 
		/// <param name="r">literal value</param>
		/// <param name="source">reference to source code</param>
		/// <returns>newly created real literal</returns>
		/// </summary>
		public RealLiteral(string r, SourceReference source = null) {
			value = r;
			source_reference = source;
		}

		public override void accept(CodeVisitor visitor) {
			visitor.visit_real_literal(this);

			visitor.visit_expression(this);
		}

		/// <summary>
		/// Returns the type name of the value this literal represents.
		/// 
		/// <returns>the name of literal type</returns>
		/// </summary>
		public string get_type_name() {
			if (value.EndsWith("f") || value.EndsWith("F")) {
				return "float";
			}

			return "double";
		}

		public override bool is_pure() {
			return true;
		}

		public override string ToString() {
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
