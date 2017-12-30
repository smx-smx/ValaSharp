using GLibPorts.Native.Varargs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLibPorts.Native.Unix {
	public class UnixStrings : IStrings {
		public void sprintf(StringBuilder sb, string format, IntPtr args) {
			NativeImports.vsprintf(sb, format, args);
		}

		public int vscprintf(string format, params VariableArgument[] args) {
			using (var combined = Platform.MakeVariableCombiner(args)) {
				return NativeImports.vsnprintf(null, 0, format, combined.GetPtr());
			}
		}

		public int vscprintf(string format, IntPtr args) {
			return NativeImports.vsnprintf(null, 0, format, args);
		}
	}
}
