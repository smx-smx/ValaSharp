using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.Code;
using Vala.Lang.CodeNodes;
using Vala.Lang.Parser;
using Vala.Lang.Types;
using Vala.Lang.TypeSymbols;

namespace Vala.Lang.Literals {
	/**
	 * Represents an integer literal in the source code.
	 */
	public class IntegerLiteral : Literal {
		/**
		 * The literal value.
		 */
		public string value { get; set; }

		public string type_suffix { get; set; }

		/**
		 * Creates a new integer literal.
		 *
		 * @param i      literal value
		 * @param source reference to source code
		 * @return       newly created integer literal
		 */
		public IntegerLiteral(string i, SourceReference source = null) {
			value = i;
			source_reference = source;
		}

		public override void accept(CodeVisitor visitor) {
			visitor.visit_integer_literal(this);

			visitor.visit_expression(this);
		}

		public override string to_string() {
			return value;
		}

		public override bool is_pure() {
			return true;
		}

		public override bool check(CodeContext context) {
			if (is_checked) {
				return !error;
			}

			is_checked = true;

			int l = 0;
			while (value.EndsWith("l") || value.EndsWith("L")) {
				l++;
				value = value.Substring(0, value.Length - 1);
			}

			bool u = false;
			if (value.EndsWith("u") || value.EndsWith("U")) {
				u = true;
				value = value.Substring(0, value.Length - 1);
			}

			Int64 n;
			try {
				n = Convert.ToInt64(value);
			} catch (FormatException) {
				string tmp = value;
				if (value[0] == '-') {
					tmp = value.Substring(1);
				}
				n = Convert.ToInt64(tmp, 16);
				if (value[0] == '-') {
					n = -n;
				}
			}

			if (!u && (n > int.MaxValue || n < int.MinValue)) {
				// value doesn't fit into signed 32-bit
				l = 2;
			} else if (u && n > uint.MaxValue) {
				// value doesn't fit into unsigned 32-bit
				l = 2;
			}

			string type_name;
			if (l == 0) {
				if (u) {
					type_suffix = "U";
					type_name = "uint";
				} else {
					type_suffix = "";
					type_name = "int";
				}
			} else if (l == 1) {
				if (u) {
					type_suffix = "UL";
					type_name = "ulong";
				} else {
					type_suffix = "L";
					type_name = "long";
				}
			} else {
				if (u) {
					type_suffix = "ULL";
					type_name = "uint64";
				} else {
					type_suffix = "LL";
					type_name = "int64";
				}
			}

			var st = (Struct)context.analyzer.root_symbol.scope.lookup(type_name);
			// ensure attributes are already processed
			st.check(context);

			value_type = new IntegerType(st, value, type_name);

			return !error;
		}

		public override void emit(CodeGenerator codegen) {
			codegen.visit_integer_literal(this);

			codegen.visit_expression(this);
		}
	}
}
