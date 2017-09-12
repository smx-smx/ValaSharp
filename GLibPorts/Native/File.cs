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
		public static readonly IntPtr stdin;
		public static readonly IntPtr stderr;
		public static readonly IntPtr stdout;

		private static IntPtr openFile(int handle, string mode) {
			IntPtr handlePtr = GetStdHandle(handle);
			Debug.Assert(handlePtr != IntPtr.Zero);

			int fd = _open_osfhandle(handlePtr, _O_TEXT);
			Debug.Assert(fd > 0);

			return _fdopen(fd, mode);
		}

		static File() {
			stdin = openFile(STD_INPUT_HANDLE, "r");
			stdout = openFile(STD_OUTPUT_HANDLE, "w");
			stderr = openFile(STD_ERROR_HANDLE, "w");
		}

		public static int fprintf(IntPtr handle, string format, params VariableArgument[] args) {
			var combinedVariables = new CombinedVariables(args);
			return vfprintf_s(handle, format, combinedVariables.GetPtr());
		}
	}
}
