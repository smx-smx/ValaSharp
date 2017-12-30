using GLibPorts.Native.Varargs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static GLibPorts.Native.Unix.NativeImports;

namespace GLibPorts.Native.Unix {
	public class UnixFile {
		private static IntPtr openFile(int fd, string mode) {
			// Copy Handle
			int copy_fd = dup(fd);

			// Open Copy
			return fdopen(copy_fd, mode);
		}

		public static void InitializeStatic() {
			DisposeStatic();
			GLib.File.stdin = openFile(STDIN_FILENO, "r");
			GLib.File.stdout = openFile(STDOUT_FILENO, "w");
			GLib.File.stderr = openFile(STDERR_FILENO, "w");
		}

		public static void DisposeStatic() {
			/*
			 * This is a no-op on Unix.
			 * The reason is that on Windows a handle copy is a separate handle
			 * On Unix a handle copy (dup) refers to the same stream.
			 * This means we free stdout/stdin/stderr in UnixFileStream instead
			 */
		}

		public static int fprintf(IntPtr handle, string format, params VariableArgument[] args) {
			using (var combinedVariables = Platform.MakeVariableCombiner(args)) {
				return vfprintf(handle, format, combinedVariables.GetPtr());
			}
		}
	}
}
