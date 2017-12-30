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
			return fdopen(fd, mode);
		}

		public static void InitializeStatic() {
			DisposeStatic();
			GLib.File.stdin = openFile(STDIN_FILENO, "r");
			GLib.File.stdout = openFile(STDOUT_FILENO, "w");
			GLib.File.stderr = openFile(STDERR_FILENO, "w");
		}

		public static void DisposeStatic() {
			if (GLib.File.stdin != IntPtr.Zero)
				fclose(GLib.File.stdin);

			if (GLib.File.stderr != IntPtr.Zero)
				fclose(GLib.File.stderr);

			if (GLib.File.stdout != IntPtr.Zero)
				fclose(GLib.File.stdout);

			GLib.File.stdin = IntPtr.Zero;
			GLib.File.stderr = IntPtr.Zero;
			GLib.File.stdout = IntPtr.Zero;

		}

		public static int fprintf(IntPtr handle, string format, params VariableArgument[] args) {
			using (var combinedVariables = Platform.MakeVariableCombiner(args)) {
				return vfprintf(handle, format, combinedVariables.GetPtr());
			}
		}
	}
}
