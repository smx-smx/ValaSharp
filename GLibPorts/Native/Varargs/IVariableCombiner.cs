using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLibPorts.Native.Varargs {
	public interface IVariableCombiner : IDisposable {
		void Build();
		IntPtr GetPtr();
	}
}
