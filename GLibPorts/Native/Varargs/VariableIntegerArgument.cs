using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GLibPorts.Native.Varargs {
	sealed class VariableIntegerArgument : VariableArgument {
		readonly int _value;

		public VariableIntegerArgument(int value) {
			_value = value;
		}

		public override IDisposable Write(IntPtr buffer) {
			Marshal.Copy(new[] { _value }, 0, buffer, 1);
			return SentinelDisposable;
		}
	}
}
