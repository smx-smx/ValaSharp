using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
	public unsafe class FastMemoryMappedFile
	{
		public readonly MemoryMappedFile mf;
		private MemoryMappedViewAccessor view;

		private byte* ptr = null;
		public readonly long Size;

		public byte* Data { get { return ptr; } }

		public static FastMemoryMappedFile OpenExisting(string filePath) {
			return new FastMemoryMappedFile(filePath);
		}

		public long GetOffset(void* mem) {
			Debug.Assert((byte*)mem >= Data);
			return (byte*)mem - Data;
		}

		public FastMemoryMappedFile(MemoryMappedFile mf, long size) {
			this.mf = mf;
			this.Size = size;
			this.GetPointer();
		}

		private void GetPointer() {
			view = mf.CreateViewAccessor();
			view.SafeMemoryMappedViewHandle.AcquirePointer(ref ptr);
		}

		private FastMemoryMappedFile(string filePath) {
			this.Size = new FileInfo(filePath).Length;
			this.mf = MemoryMappedFile.CreateFromFile(filePath, FileMode.Open);
			this.GetPointer();
		}

		private FastMemoryMappedFile(FastMemoryMappedFile other) {
			this.mf = other.mf;
			this.Size = other.Size;
			this.ptr = other.Data;
		}

		public FastMemoryMappedFile Clone() {
			return new FastMemoryMappedFile(this);
		}

		~FastMemoryMappedFile() {
			ptr = null;
			view.Dispose();
			mf.Dispose();
		}
	}
}
