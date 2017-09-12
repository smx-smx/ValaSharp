using System;
using System.Collections.Generic;
using System.Text;

namespace Vala.Lang.Parser
{
	public class SourceReference
	{
		/**
		 * The source file to be referenced.
		 */
		public WeakReference<SourceFile> file { get; set; }

		/**
		 * The begin of the referenced source code.
		 */
		public SourceLocation begin { get; set; }

		/**
		 * The end of the referenced source code.
		 */
		public SourceLocation end { get; set; }
	}
}
