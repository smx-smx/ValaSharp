using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils {
	public unsafe class FastMemoryMappedFile : IDisposable {
		public MemoryMappedFile mf;
		private MemoryMappedViewAccessor view;
		private FileStream fs;

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
			if (view != null)
				DisposeView();

			// Read access, to avoid creating a file lock
			view = mf.CreateViewAccessor(0L, 0L, MemoryMappedFileAccess.Read);
			view.SafeMemoryMappedViewHandle.AcquirePointer(ref ptr);
		}

		public string GetContents() {
			// Read access, to avoid creating a file lock
			using (var stream = mf.CreateViewStream(0L, 0L, MemoryMappedFileAccess.Read)) {
				byte[] buf = new byte[stream.Length];
				stream.Read(buf, 0, (int)stream.Length);

				return Encoding.Default.GetString(buf);
			}
		}

		private FastMemoryMappedFile(string filePath) {
			this.Size = new FileInfo(filePath).Length;

			this.mf = MemoryMappedFile.CreateFromFile(
				// Read access, to avoid creating a file lock
				File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read),
				//not mapping to a name
				null,
				//use the file's actual size
				0L,
				MemoryMappedFileAccess.Read,
				HandleInheritability.None,
				//close the previously passed in stream when disposed
				false);

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

		private void DisposeView() {
			view.Flush();
			if (ptr != null) {
				view.SafeMemoryMappedViewHandle.ReleasePointer();
				ptr = null;
			}
			if (!view.SafeMemoryMappedViewHandle.IsClosed) {
				view.SafeMemoryMappedViewHandle.Close();
			}
			view.SafeMemoryMappedViewHandle.Dispose();
			view.Dispose();
		}

		public void Dispose() {
			if (view != null) {
				DisposeView();
			}
			if (mf != null) {
				mf.Dispose();
			}

			view = null;
			mf = null;
		}
	}
}
