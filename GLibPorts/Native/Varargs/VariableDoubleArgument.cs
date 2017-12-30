using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GLibPorts.Native.Varargs {
	sealed class VariableDoubleArgument : VariableArgument {
		readonly double _value;

		public VariableDoubleArgument(double value) {
			_value = value;
		}

		public override int GetSize() {
			return 8;
		}

		public override IDisposable Write(IntPtr buffer) {
			Marshal.Copy(new[] { _value }, 0, buffer, 1);
			return SentinelDisposable;
		}
	}
}
