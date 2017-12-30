using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GLibPorts.Native;
using File = System.IO.File;

namespace GLibPorts {
	public partial class GLib {
		public class FileUtils {
			public static bool get_contents(string filename, out string contents) {
				var fileStream = new System.IO.FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
				var textReader = new StreamReader(fileStream);
				contents = textReader.ReadToEnd();
				return true;
			}

			public static bool get_contents(string filename, out string contents, out ulong length) {
				get_contents(filename, out contents);
				length = (ulong)contents.Length;
				return true;
			}
			
			//TODO(Smx) make an interface for the calls below
			private static void close_unix(int outputfd) {
				Native.Unix.NativeImports.close(outputfd);
			}

			private static void close_win32(int outputfd) {
				Native.Win32.NativeImports._close(outputfd);
			}

			private static int mkstemp_unix(string template, out string filename) {
				filename = Native.Unix.NativeImports.mktemp(template);
				if (filename == null)
					return -1;
				return Native.Unix.NativeImports.open(
					filename,
					Native.Unix.NativeImports.O_RDWR | Native.Unix.NativeImports.O_CREAT,
					0700
				);
			}

			private static int mkstemp_win32(string template, out string filename) {
				filename = Native.Win32.NativeImports.mktemp(template);
				if (filename == null)
					return -1;
				return Native.Win32.NativeImports._open(
					filename,
					Native.Win32.NativeImports._O_RDWR | Native.Win32.NativeImports._O_CREAT,
					0700
				);
			}

			public static void close(int outputfd) {
				if (Utils.IsUnix())
					close_unix(outputfd);
				else
					close_win32(outputfd);
			}

			public static int mkstemp(string template, out string filename) {
				if (Utils.IsUnix())
					return mkstemp_unix(template, out filename);
				else
					return mkstemp_win32(template, out filename);
			}
		}
	}
}
