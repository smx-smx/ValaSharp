using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.Parser;

namespace Vala.Lang {
	public class Comment {
		public Comment(string comment, SourceReference _source_reference) {
			source_reference = _source_reference;
			content = comment;
		}

		/// <summary>
		/// The text describing the referenced source code.
		/// </summary>
		public string content { set; get; }

		/// <summary>
		/// References the location in the source file where this code node has
		/// been written.
		/// </summary>
		public SourceReference source_reference { get; set; }
	}
}
