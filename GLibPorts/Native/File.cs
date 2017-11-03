using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static GLibPorts.Native.Win32;

namespace GLibPorts.Native
{
	public class File
	{
		public static IntPtr stdin;
		public static IntPtr stderr;
		public static IntPtr stdout;

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
			stdin = openFile(STD_INPUT_HANDLE, "r");
			stdout = openFile(STD_OUTPUT_HANDLE, "w");
			stderr = openFile(STD_ERROR_HANDLE, "w");
		}

		public static void DisposeStatic() {
			if(stdin != IntPtr.Zero)
				fclose(stdin);

			if (stderr != IntPtr.Zero)
				fclose(stderr);

			if (stdout != IntPtr.Zero)
				fclose(stdout);

			stdin = IntPtr.Zero;
			stderr = IntPtr.Zero;
			stdout = IntPtr.Zero;

		}

		public static int fprintf(IntPtr handle, string format, params VariableArgument[] args) {
			using (var combinedVariables = new CombinedVariables(args)) {
				return vfprintf_s(handle, format, combinedVariables.GetPtr());
			}
		}
	}
}
