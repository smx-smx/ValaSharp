using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLibPorts.Native.Unix {
	public class UnixModuleLoader : IModuleLoader {
		public void FreeLibrary(IntPtr handle) {
			NativeImports.dlclose(handle);
		}

		public IntPtr GetModuleHandle(string moduleName) {
			if(moduleName == null) {
				return NativeImports.dlopen(null, NativeImports.RTLD_LAZY);
			} else {
				throw new NotImplementedException();
			}
		}

		public IntPtr GetProcAddress(IntPtr handle, string symbol_name) {
			return NativeImports.dlsym(handle, symbol_name);
		}

		public IntPtr LoadLibrary(string library_path) {
			return NativeImports.dlopen(library_path, NativeImports.RTLD_LAZY);
		}
	}
}
