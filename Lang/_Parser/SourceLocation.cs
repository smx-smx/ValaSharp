using System;
using System.Collections.Generic;
using System.Text;

namespace Vala.Lang.Parser
{
    public class SourceLocation
    {
		public int line;
		public int column;

		public SourceLocation(int _line, int _column) {
			line = _line;
			column = _column;
		}
	}
}
