using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLibPorts.Native {
	public interface IModuleLoader {
		IntPtr GetProcAddress(IntPtr handle, string symbol_name);
		IntPtr LoadLibrary(string library_path);
		void FreeLibrary(IntPtr handle);

		IntPtr GetModuleHandle(string moduleName);
	}
}
