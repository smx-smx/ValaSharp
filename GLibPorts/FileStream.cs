using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GLibPorts.Native;

namespace GLibPorts
{
	public partial class GLib
	{
		public static readonly FileStream stdin = new FileStream(File.stdin);
		public static readonly FileStream stderr = new FileStream(File.stderr);
		public static readonly FileStream stdout = new FileStream(File.stdout);

		public class FileStream : IDisposable
		{
			private IntPtr stream;

			internal FileStream(IntPtr streamHandle) {
				stream = streamHandle;

				Win32.setvbuf(stream, null, Win32._IONBF, 0);
			}

			~FileStream() {
				Dispose();
			}

			public void Dispose() {
				if (stream != IntPtr.Zero) {
					Win32.fclose(stream);
					stream = IntPtr.Zero;
				}
			}

			public int fileno() {
				return Win32._fileno(stream);
			}

			public int printf(string format, params VariableArgument[] args) {
				return File.fprintf(stream, format, args);
			}

			public int puts(string str) {
				return Win32.fputs(str, stream);
			}

			public int putc(int c) {
				return Win32.putc(c, stream);
			}

			public static FileStream fdopen(int fd, string mode) {
				return new FileStream(Win32._fdopen(fd, mode));
			}

			public static FileStream open(string filename, string mode) {
				return new FileStream(Win32.fopen(filename, mode));
			}
		}
	}
}
