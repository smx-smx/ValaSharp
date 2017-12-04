using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.CodeNodes;
using Vala.Lang.Expressions;
using Vala.Lang.Parser;
using Vala.Lang.Symbols;

namespace Vala.Lang {
	public class EnumValue : Constant {
		/**
	 * Creates a new enum value with the specified numerical representation.
	 *
	 * @param name  enum value name
	 * @param value numerical representation
	 * @return      newly created enum value
	 */
		public EnumValue(string name, Expression value, SourceReference source_reference = null, Comment comment = null)
			: base(name, null, value, source_reference, comment) {
		}

		public override void accept(CodeVisitor visitor) {
			visitor.visit_enum_value(this);
		}

		public override void accept_children(CodeVisitor visitor) {
			if (value != null) {
				value.accept(visitor);
			}
		}

		public override bool check(CodeContext context) {
			if (is_checked) {
				return !error;
			}

			is_checked = true;

			if (value != null) {
				value.check(context);
			}

			return !error;
		}
	}
}
