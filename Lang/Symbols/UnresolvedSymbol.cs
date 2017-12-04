using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.Expressions;
using Vala.Lang.Parser;

namespace Vala.Lang.Symbols {
	/**
	 * An unresolved reference to a symbol.
	 */
	public class UnresolvedSymbol : Symbol {
		/**
		 * The parent of the symbol or null.
		 */
		public UnresolvedSymbol inner { get; set; }

		/**
		 * Qualified access to global symbol.
		 */
		public bool qualified { get; set; }

		public UnresolvedSymbol(UnresolvedSymbol inner, string name, SourceReference source_reference = null)
			: base(name, source_reference) {
			this.inner = inner;
		}

		public static UnresolvedSymbol new_from_expression(Expression expr) {
			var ma = expr as MemberAccess;
			if (ma != null) {
				if (ma.inner != null) {
					return new UnresolvedSymbol(new_from_expression(ma.inner), ma.member_name, ma.source_reference);
				} else {
					return new UnresolvedSymbol(null, ma.member_name, ma.source_reference);
				}
			}

			Report.error(expr.source_reference, "Type reference must be simple name or member access expression");
			return null;
		}

		public override string to_string() {
			if (inner == null) {
				return name;
			} else {
				return "%s.%s".printf(inner.to_string(), name);
			}
		}

		public UnresolvedSymbol copy() {
			return new UnresolvedSymbol(inner, name, source_reference);
		}
	}

}
