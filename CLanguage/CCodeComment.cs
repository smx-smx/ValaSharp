using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLanguage {
	/// <summary>
	/// Represents a comment in the C code.
	/// </summary>
	public class CCodeComment : CCodeNode {
		/// <summary>
		/// The text content of the comment.
		/// </summary>
		public string text { get; set; }

		public CCodeComment(string _text) {
			text = _text;
		}

		public override void write(CCodeWriter writer) {
			writer.write_comment(text);
		}
	}
}
