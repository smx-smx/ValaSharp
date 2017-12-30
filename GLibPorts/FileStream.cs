using GLibPorts.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLibPorts {
	public partial class GLib {
		public class FileStream {
			public static IFileStream stdin { get; internal set; }
			public static IFileStream stderr { get; internal set; }
			public static IFileStream stdout { get; internal set; }

			private IntPtr stream;

			internal FileStream(IntPtr streamHandle) {
				stream = streamHandle;
			}

			public static void InitializeStatic() {
				DisposeStatic();
				if (Utils.IsUnix()) {
					Native.Unix.UnixFileStream.InitilizeStatic();
				} else {
					Native.Win32.Win32FileStream.InitializeStatic();
				}
			}

			public static void DisposeStatic() {
				stdin?.Dispose();
				stderr?.Dispose();
				stdout?.Dispose();
			}

			public static IFileStream open(string filePath, string mode) {
				if (Utils.IsUnix())
					return Native.Unix.UnixFileStream.open(filePath, mode);
				else
					return Native.Win32.Win32FileStream.open(filePath, mode);
			}
		}
	}
}
