using GLibPorts.Native.Varargs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static GLibPorts.Native.Win32.NativeImports;

namespace GLibPorts.Native.Win32 {
	public class Win32File {
		private static IntPtr openFile(int handle, string mode) {
			// Get Handle
			IntPtr handlePtr = GetStdHandle(handle);
			Debug.Assert(handlePtr != IntPtr.Zero);

			// Copy Handle
			IntPtr handleCopy;
			IntPtr procHandle = GetCurrentProcess();
			DuplicateHandle(procHandle, handlePtr, procHandle, out handleCopy, 0, true, DUPLICATE_SAME_ACCESS);

			// Open Copy
			int fd = _open_osfhandle(handleCopy, _O_TEXT);
			Debug.Assert(fd > 0);

			return _fdopen(fd, mode);
		}

		public static void InitializeStatic() {
			DisposeStatic();
			GLib.File.stdin = openFile(STD_INPUT_HANDLE, "r");
			GLib.File.stdout = openFile(STD_OUTPUT_HANDLE, "w");
			GLib.File.stderr = openFile(STD_ERROR_HANDLE, "w");
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
				return vfprintf_s(handle, format, combinedVariables.GetPtr());
			}
		}
	}
}
