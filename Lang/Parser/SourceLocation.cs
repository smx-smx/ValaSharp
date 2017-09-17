using GLibPorts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Vala.Lang.Parser
{
    public class SourceLocation
    {
		public MemoryStream rdr;
		public int line;
		public int column;

		public long pos;

		public SourceLocation(MemoryStream rdr, long offset, int _line, int _column)
		{
			this.rdr = rdr;
			pos = offset;
			line = _line;
			column = _column;
		}

		public SourceLocation(MemoryStream rdr, int _line, int _column){
			this.rdr = rdr;
			pos = rdr.Position;
			line = _line;
			column = _column;
		}
	}
}
