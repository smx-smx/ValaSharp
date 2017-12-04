using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.CodeNodes;
using Vala.Lang.Expressions;
using Vala.Lang.Parser;
using Vala.Lang.Symbols;

namespace Vala.Lang.TypeSymbols {
	public class ErrorCode : TypeSymbol {
		/**
	 * Specifies the numerical representation of this enum value.
	 */
		public Expression value { get; set; }

		/**
		 * Creates a new enum value.
		 *
		 * @param name enum value name
		 * @return     newly created enum value
		 */
		public ErrorCode(string name, SourceReference source_reference = null, Comment comment = null) : base(name, source_reference, comment) {
		}

		/**
		 * Creates a new enum value with the specified numerical representation.
		 *
		 * @param name  enum value name
		 * @param value numerical representation
		 * @return      newly created enum value
		 */
		public static ErrorCode with_value(string name, Expression value, SourceReference source_reference = null) {
			ErrorCode err = new ErrorCode(name, source_reference);
			err.value = value;
			return err;
		}

		public override void accept(CodeVisitor visitor) {
			visitor.visit_error_code(this);
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
