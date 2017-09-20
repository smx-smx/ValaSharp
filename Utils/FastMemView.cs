using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
	public unsafe class FastMView
	{
		public FastMemoryMappedFile mf { get; private set; }
		private byte* ptr;

		public long Position {
			get {
				return ptr - mf.Data;
			}
			set {
				ptr = mf.Data + value;
			}
		}

		public FastMView(FastMView source) : this(source.mf) {
			this.Position = source.Position;
		}

		public FastMView(FastMemoryMappedFile mf) {
			this.mf = mf;
			this.Position = 0;
		}

		public char PeekChar() {
			return Convert.ToChar(PeekByte());
		}

		public char PeekCharAt(long offset, SeekOrigin loc = SeekOrigin.Current) {
			return Convert.ToChar(PeekByteAt(offset, loc));
		}

		public byte PeekByteAt(long offset, SeekOrigin loc = SeekOrigin.Current) {
			long prev_pos = Position;
			Seek(offset, loc);

			byte b = PeekByte();

			Seek(prev_pos, SeekOrigin.Begin);
			return b;
		}

		public byte PeekByte() {
			if(Position >= mf.Size) {
				throw new EndOfStreamException();
			}

			return *(mf.Data + Position);
		}

		public string ReadString(int length) {
			return new string((sbyte*)mf.Data + Position, 0, length);
		}

		public void Seek(long pos, SeekOrigin begin) {
			switch (begin) {
				case SeekOrigin.Begin:
					Position = pos;
					break;
				case SeekOrigin.Current:
					Position += pos;
					break;
				case SeekOrigin.End:
					Position = mf.Size - pos;
					break;
			}
		}
	}
}
