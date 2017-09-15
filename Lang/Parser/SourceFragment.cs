using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vala.Lang.Parser
{
	public class SourceFragment
	{
		public SourceLocation begin;
		public SourceLocation end;

		public long length {
			get { return end.pos - begin.pos; }
		}

		public string content {
			get
			{
				long currentPos = begin.rdr.Position;
				begin.rdr.Seek(begin.pos, SeekOrigin.Begin);

				byte[] chars = new byte[length];
				begin.rdr.Read(chars, 0, (int)length);

				begin.rdr.Seek(currentPos, SeekOrigin.Begin);
				return Encoding.Default.GetString(chars);
			}
		}

		public static string get_content(SourceLocation begin, SourceLocation end)
		{
			return new SourceFragment(begin, end).content;
		}

		public SourceFragment(SourceLocation begin, SourceLocation end)
		{
			this.begin = begin;
			this.end = end;
		}
	}
}
