using GLibPorts.Native.Varargs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace GLibPorts.Native.Unix {
	public class UnixFileStream : IFileStream {
		private IntPtr stream;

		internal UnixFileStream(IntPtr streamHandle) {
			stream = streamHandle;
		}

		public static void InitilizeStatic() {
			GLib.FileStream.stdin = new UnixFileStream(GLib.File.stdin);
			GLib.FileStream.stderr = new UnixFileStream(GLib.File.stderr);
			GLib.FileStream.stdout = new UnixFileStream(GLib.File.stdout);
		}

		public void Dispose() {
			if(stream != IntPtr.Zero) {
				NativeImports.fclose(stream);
				stream = IntPtr.Zero;
			}
		}

		public int fileno() {
			return NativeImports.fileno(stream);
		}

		public int printf(string format, params VariableArgument[] args) {
			return UnixFile.fprintf(stream, format, args);
		}

		public int putc(int c) {
			return NativeImports.putc(c, stream);
		}

		public int puts(string str) {
			return NativeImports.fputs(str, stream);
		}

		public static IFileStream open(string filePath, string mode) {
			return new UnixFileStream(NativeImports.fopen(filePath, mode));
		}
	}
}
