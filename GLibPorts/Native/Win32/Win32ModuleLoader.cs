using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLibPorts.Native.Win32 {
	public class Win32ModuleLoader : IModuleLoader {
		public void FreeLibrary(IntPtr handle) {
			NativeImports.FreeLibrary(handle);
		}

		public IntPtr GetModuleHandle(string moduleName) {
			return NativeImports.GetModuleHandle(moduleName);
		}

		public IntPtr GetProcAddress(IntPtr handle, string symbol_name) {
			return NativeImports.GetProcAddress(handle, symbol_name);
		}

		public IntPtr LoadLibrary(string library_path) {
			return NativeImports.LoadLibrary(library_path);
		}
	}
}
