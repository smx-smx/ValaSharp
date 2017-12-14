using CLanguage.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLanguage {
	/// <summary>
	/// Represents an access to a struct member in the C code.
	/// </summary>
	public class CCodeMemberAccess : CCodeExpression {
		/// <summary>
		/// The parent of the member.
		/// </summary>
		public CCodeExpression inner { get; set; }

		/// <summary>
		/// The name of the member.
		/// </summary>
		public string member_name { get; set; }

		/// <summary>
		/// Specifies whether the member access happens by pointer dereferencing.
		/// </summary>
		public bool is_pointer { get; set; }

		public CCodeMemberAccess(CCodeExpression container, string member, bool pointer = false) {
			inner = container;
			member_name = member;
			is_pointer = pointer;
		}

		public static CCodeMemberAccess pointer(CCodeExpression container, string member) {
			CCodeMemberAccess @this = new CCodeMemberAccess(container, member, true);
			return @this;
		}

		public override void write(CCodeWriter writer) {
			inner.write_inner(writer);
			if (is_pointer) {
				writer.write_string("->");
			} else {
				writer.write_string(".");
			}
			writer.write_string(member_name);
		}
	}
}
