using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GLibPorts.Native;
using GLibPorts.Native.Varargs;

namespace GLibPorts.Native.Win32 {
	public class Win32FileStream : IFileStream, IDisposable {
		private IntPtr stream;

		internal Win32FileStream(IntPtr streamHandle) {
			stream = streamHandle;
			NativeImports.setvbuf(stream, null, NativeImports._IONBF, 0);
		}

		public static void InitializeStatic() {
			GLib.FileStream.stdin = new Win32FileStream(GLib.File.stdin);
			GLib.FileStream.stderr = new Win32FileStream(GLib.File.stderr);
			GLib.FileStream.stdout = new Win32FileStream(GLib.File.stdout);
		}

		public void Dispose() {
			if (stream != IntPtr.Zero) {
				NativeImports.fclose(stream);
				stream = IntPtr.Zero;
			}
		}

		public int fileno() {
			return NativeImports._fileno(stream);
		}

		public int printf(string format, params VariableArgument[] args) {
			return Win32File.fprintf(stream, format, args);
		}

		public int puts(string str) {
			return NativeImports.fputs(str, stream);
		}

		public int putc(int c) {
			return NativeImports.putc(c, stream);
		}

		public static Win32FileStream fdopen(int fd, string mode) {
			return new Win32FileStream(NativeImports._fdopen(fd, mode));
		}

		public static Win32FileStream open(string filename, string mode) {
			return new Win32FileStream(NativeImports.fopen(filename, mode));
		}
	}
}
