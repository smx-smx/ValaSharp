using System;
using System.Collections.Generic;
using System.Text;
using Vala.Lang.CodeNodes;

namespace Vala.Lang.Parser {
	/// <summary>
	/// Represents a reference to a location in a source file.
	/// </summary>
	public class SourceReference : IDisposable {
		private WeakReference<SourceFile> file_weak = new WeakReference<SourceFile>(null);

		/// <summary>
		/// The source file to be referenced.
		/// </summary>
		public SourceFile file {
			get {
				return file_weak.GetTarget();
			}
			set {
				file_weak.SetTarget(value);
			}
		}

		/// <summary>
		/// The begin of the referenced source code.
		/// </summary>
		public SourceLocation begin { get; set; }

		/// <summary>
		/// The end of the referenced source code.
		/// </summary>
		public SourceLocation end { get; set; }

		public List<UsingDirective> using_directives { get; private set; }

		/// <summary>
		/// Creates a new source reference.
		/// 
		/// <param name="_file">a source file</param>
		/// <param name="begin">the begin of the referenced source code</param>
		/// <param name="end">the end of the referenced source code</param>
		/// <returns>newly created source reference</returns>
		/// </summary>
		public SourceReference(SourceFile _file, SourceLocation begin, SourceLocation end) {
			file = _file;
			this.begin = begin;
			this.end = end;
			using_directives = file.current_using_directives;
		}

		/// <summary>
		/// Returns a string representation of this source reference.
		/// 
		/// <returns>human-readable string</returns>
		/// </summary>
		public override string ToString() {
			return ("%s:%d.%d-%d.%d".printf(file.get_relative_filename(), begin.line, begin.column, end.line, end.column));
		}

		public void Dispose() {
			begin?.Dispose();
			end?.Dispose();
		}
	}
}
