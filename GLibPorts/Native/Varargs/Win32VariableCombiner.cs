using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GLibPorts.Native.Varargs {
	public class Win32VariableCombiner : IVariableCombiner, IDisposable {
		protected readonly IList<IDisposable> _disposables = new List<IDisposable>();
		protected IntPtr _ptr = IntPtr.Zero;

		public VariableArgument[] args;
		protected bool _disposed;

		public Win32VariableCombiner(VariableArgument[] args) {
			this.args = args;
		}

		private void _build() {
			_disposables.Clear();

			_ptr = Marshal.AllocHGlobal(args.Sum(arg => arg.GetSize()));
			var curPtr = _ptr;

			foreach (var arg in args) {
				_disposables.Add(arg.Write(curPtr));
				curPtr += arg.GetSize();
			}
		}

		public virtual void Build() {
			_build();
		}

		public virtual IntPtr GetPtr() {
			if (_ptr == IntPtr.Zero)
				_build();

			if (_disposed)
				throw new InvalidOperationException("Disposed already.");

			return _ptr;
		}

		public virtual void Dispose() {
			if (!_disposed) {
				_disposed = true;

				foreach (var disposable in _disposables)
					disposable.Dispose();

				if(_ptr != IntPtr.Zero)
					Marshal.FreeHGlobal(_ptr);
			}
		}
	}
}
