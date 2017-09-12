using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLibPorts
{
	public static class MemoryStreamExtensions
	{
		public static char PeekChar(this MemoryStream @this) {
			return Convert.ToChar(@this.PeekByte());
		}

		public static char PeekCharAt(this MemoryStream @this, int offset, SeekOrigin loc = SeekOrigin.Current) {
			return Convert.ToChar(@this.PeekByteAt(offset, loc));
		}

		public static MemoryStream Clone(this MemoryStream @this) {
			var newStream = new MemoryStream();
			@this.CopyTo(newStream);
			newStream.Position = @this.Position;
			return newStream;
		}

		public static byte PeekByteAt(this MemoryStream @this, int offset, SeekOrigin loc = SeekOrigin.Current) {
			long prev_pos = @this.Position;
			@this.Seek((long)offset, loc);

			byte b = @this.PeekByte();

			@this.Seek(prev_pos, SeekOrigin.Begin);
			return b;
		}

		public static string PeekString(this MemoryStream @this, int length) {
			long prev_pos = @this.Position;

			byte[] bytes = new byte[length];
			@this.Read(bytes, 0, length);

			@this.Seek(prev_pos, SeekOrigin.Begin);

			return Encoding.Default.GetString(bytes);
		}

		public static byte PeekByte(this MemoryStream @this) {
			int bint = @this.ReadByte();
			if(bint < 0) {
				throw new EndOfStreamException();
			}

			@this.Position--;
			return (byte)bint;
		}
	}
}
