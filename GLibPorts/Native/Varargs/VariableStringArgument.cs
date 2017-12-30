using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GLibPorts.Native.Varargs {
	sealed class VariableStringArgument : VariableArgument {
		readonly string _value;

		public VariableStringArgument(string value) {
			_value = value;
		}

		public override IDisposable Write(IntPtr buffer) {
			var ptr = Marshal.StringToHGlobalAnsi(_value);

			Marshal.Copy(new[] { ptr }, 0, buffer, 1);

			return new StringArgumentDisposable(ptr);
		}

		#region StringArgumentDisposable

		class StringArgumentDisposable : IDisposable {
			IntPtr _ptr;

			public StringArgumentDisposable(IntPtr ptr) {
				_ptr = ptr;
			}

			public void Dispose() {
				if (_ptr != IntPtr.Zero) {
					Marshal.FreeHGlobal(_ptr);
					_ptr = IntPtr.Zero;
				}
			}
		}

		#endregion
	}
}