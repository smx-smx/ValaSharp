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

		public long start_pos;
		public long end_pos;

		public long length {
			get {
				return end_pos - start_pos;
			}
		}

		public string content {
			get {
				rdr.Seek(start_pos, SeekOrigin.Begin);
				byte[] chars = new byte[length];
				rdr.Read(chars, 0, (int)length);
				return Encoding.Default.GetString(chars);
			}
		}

		public SourceLocation(MemoryStream rdr, long offset, int _line, int _column) : this(rdr, _line, _column) {
			start_pos = offset;
		}

		public SourceLocation(MemoryStream rdr, int _line, int _column) {
			this.rdr = rdr;
			start_pos = rdr.Position;
			line = _line;
			column = _column;
		}
	}
}
