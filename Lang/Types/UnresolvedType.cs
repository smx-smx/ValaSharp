using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.Expressions;
using Vala.Lang.Parser;
using Vala.Lang.Symbols;

namespace Vala.Lang.Types {
	/**
	 * An unresolved reference to a data type.
	 */
	public class UnresolvedType : DataType {
		/**
		 * The unresolved reference to a type symbol.
		 */
		public UnresolvedSymbol unresolved_symbol { get; set; }

		public UnresolvedType() {
		}

		/**
		 * Creates a new type reference.
		 *
		 * @param symbol    unresolved type symbol
		 * @param source    reference to source code
		 * @return          newly created type reference
		 */
		public static UnresolvedType from_symbol(UnresolvedSymbol symbol, SourceReference source = null) {
			UnresolvedType @this = new UnresolvedType();
			@this.unresolved_symbol = symbol;
			@this.source_reference = source;
			return @this;
		}

		/**
		 * Creates a new type reference from a code expression.
		 *
		 * @param expr   member access expression
		 * @return       newly created type reference
		 */
		public static UnresolvedType new_from_expression(Expression expr) {
			var sym = UnresolvedSymbol.new_from_expression(expr);

			if (sym != null) {
				var type_ref = UnresolvedType.from_symbol(sym, expr.source_reference);
				type_ref.value_owned = true;

				var ma = (MemberAccess)expr;
				foreach (DataType arg in ma.get_type_arguments()) {
					type_ref.add_type_argument(arg);
				}

				return type_ref;
			}

			return null;
		}

		public override DataType copy() {
			var result = new UnresolvedType();
			result.source_reference = source_reference;
			result.value_owned = value_owned;
			result.nullable = nullable;
			result.is_dynamic = is_dynamic;
			result.unresolved_symbol = unresolved_symbol.copy();

			foreach (DataType arg in get_type_arguments()) {
				result.add_type_argument(arg.copy());
			}

			return result;
		}

		public override string to_qualified_string(Scope scope) {
			var s = unresolved_symbol.to_string();

			var type_args = get_type_arguments();
			if (type_args.Count > 0) {
				s += "<";
				bool first = true;
				foreach (DataType type_arg in type_args) {
					if (!first) {
						s += ",";
					} else {
						first = false;
					}
					if (!type_arg.value_owned) {
						s += "weak ";
					}
					s += type_arg.to_qualified_string(scope);
				}
				s += ">";
			}
			if (nullable) {
				s += "?";
			}

			return s;
		}

		public override bool is_disposable() {
			return value_owned;
		}
	}
}
