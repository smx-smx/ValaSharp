using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
	public unsafe class FastMView : IDisposable
	{
		private WeakReference<FastMemoryMappedFile> mf_weak = new WeakReference<FastMemoryMappedFile>(null);

		public FastMemoryMappedFile mf {
			get {
				if (mf_weak.TryGetTarget(out var mf_ref))
					return mf_ref;
				return null;
			}
			private set {
				mf_weak.SetTarget(value);
			}
		}

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
			return Encoding.UTF8.GetString(mf.Data + Position, length);
			//return new string((sbyte*)mf.Data + Position, 0, length);
		}

		public char PeekUniChar(out int length) {
			char ch;
			using (UnmanagedMemoryStream ums = new UnmanagedMemoryStream(mf.Data + Position, mf.Size - Position))
			using (StreamReader sr = new StreamReader(ums, Encoding.UTF8, true)) {
				ch = Convert.ToChar(sr.Read());
			}
			
			length = Encoding.UTF8.GetByteCount(new char[]{ch});
			return ch;
		}

		public char ReadUniChar() {
			char ch = PeekUniChar(out int length);
			Seek(length, SeekOrigin.Current);
			return ch;
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

		/// <summary>
		/// Closes the inner FastMemoryMappedFile
		/// </summary>
		public void Dispose() {
			mf.Dispose();
		}
	}
}
