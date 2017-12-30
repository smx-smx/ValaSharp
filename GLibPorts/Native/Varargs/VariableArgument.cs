using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLibPorts.Native.Varargs {
	public abstract class VariableArgument {
		#region SentinelDispose

		protected static readonly IDisposable SentinelDisposable =
				new SentinelDispose();

		class SentinelDispose : IDisposable {
			public void Dispose() {

			}
		}

		#endregion

		public abstract IDisposable Write(IntPtr buffer);

		public virtual int GetSize() {
			return IntPtr.Size;
		}

		public static implicit operator VariableArgument(int input) {
			return new VariableIntegerArgument(input);
		}

		public static implicit operator VariableArgument(string input) {
			return new VariableStringArgument(input);
		}

		public static implicit operator VariableArgument(double input) {
			return new VariableDoubleArgument(input);
		}
	}
}
