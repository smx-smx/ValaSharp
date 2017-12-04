using System;
using System.Collections.Generic;
using System.Text;
using Vala.Lang.CodeNodes;

namespace Vala.Lang.Parser {
	/**
	 * Represents a reference to a location in a source file.
	 */
	public class SourceReference : IDisposable {
		private WeakReference<SourceFile> file_weak = new WeakReference<SourceFile>(null);

		/**
		 * The source file to be referenced.
		 */
		public SourceFile file {
			get {
				return file_weak.GetTarget();
			}
			set {
				file_weak.SetTarget(value);
			}
		}

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
		public SourceReference(SourceFile _file, SourceLocation begin, SourceLocation end) {
			file = _file;
			this.begin = begin;
			this.end = end;
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

		public void Dispose() {
			begin?.Dispose();
			end?.Dispose();
		}
	}
}
