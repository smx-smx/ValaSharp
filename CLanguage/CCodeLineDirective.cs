﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala;

namespace CLanguage {
	/// <summary>
	/// Represents a line directive in the C code.
	/// </summary>
	public class CCodeLineDirective : CCodeNode {
		/// <summary>
		/// The name of the source file to be presumed.
		/// </summary>
		public string filename { get; set; }

		/// <summary>
		/// The line number in the source file to be presumed.
		/// </summary>
		public int line_number { get; set; }

		public CCodeLineDirective(string _filename, int _line) {
			filename = _filename;
			filename = filename.Replace("\\", "/");

			line_number = _line;
		}

		public override void write(CCodeWriter writer) {
			if (!writer.bol) {
				writer.write_newline();
			}
			writer.write_string("#line %d \"%s\"".printf(line_number, filename));
			writer.write_newline();
		}
	}

}
