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
		/// <summary>
		/// Specifies the numerical representation of this enum value.
		/// </summary>
		public Expression value { get; set; }

		/// <summary>
		/// Creates a new enum value.
		/// 
		/// <param name="name">enum value name</param>
		/// <returns>newly created enum value</returns>
		/// </summary>
		public ErrorCode(string name, SourceReference source_reference = null, Comment comment = null) : base(name, source_reference, comment) {
		}

		/// <summary>
		/// Creates a new enum value with the specified numerical representation.
		/// 
		/// <param name="name">enum value name</param>
		/// <param name="value">numerical representation</param>
		/// <returns>newly created enum value</returns>
		/// </summary>
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
