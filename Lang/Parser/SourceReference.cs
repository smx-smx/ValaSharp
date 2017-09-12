using System;
using System.Collections.Generic;
using System.Text;
using Vala.Lang.CodeNodes;

namespace Vala.Lang.Parser
{
	/**
	 * Represents a reference to a location in a source file.
	 */
	public class SourceReference
	{
		/**
		 * The source file to be referenced.
		 */
		public SourceFile file { get; set; }

		/**
		 * The begin of the referenced source code.
		 */
		public SourceLocation begin { get; set; }

		/**
		 * The end of the referenced source code.
		 */
		public SourceLocation end { get; set; }

		public List<UsingDirective> using_directives { get; private set; }

		/**
		 * Creates a new source reference.
		 *
		 * @param _file        a source file
		 * @param begin        the begin of the referenced source code
		 * @param end          the end of the referenced source code
		 * @return             newly created source reference
		 */
		public SourceReference(SourceFile _file, SourceLocation location) {
			file = _file;
			this.begin = this.begin;
			using_directives = file.current_using_directives;
		}

		/**
		 * Returns a string representation of this source reference.
		 *
		 * @return human-readable string
		 */
		public override string ToString() {
			return ("%s:%d.%d-%d.%d".printf(file.get_relative_filename(), begin.line, begin.column, end.line, end.column));
		}
	}
}
