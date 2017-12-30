using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLibPorts.Native.Win32 {
	public class Win32Strings : IStrings {
		public void sprintf(StringBuilder sb, string format, IntPtr args) {
			NativeImports.vsprintf(sb, format, args);
		}

		public int vscprintf(string format, IntPtr args) {
			return NativeImports._vscprintf(format, args);
		}
	}
}
