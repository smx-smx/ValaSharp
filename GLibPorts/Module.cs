using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GLibPorts.Native;

namespace GLibPorts {
	public partial class GLib {
		public class Module : IDisposable {
			private IntPtr handle;
			private Module(IntPtr handle) {
				if (handle == IntPtr.Zero) {
					throw new ArgumentNullException("handle");
				}
				this.handle = handle;
			}

			private static IModuleLoader loader {
				get {
					return Platform.ModuleLoader;
				}
			}

			public static Module open(string file_name, ModuleFlags flags) {
				IntPtr handle = IntPtr.Zero;
				if (file_name == null) {
					handle = loader.GetModuleHandle(null);
				} else {
					handle = loader.LoadLibrary(file_name);
				}

				if (handle == IntPtr.Zero)
					return null;

				return new Module(handle);
			}

			public void symbol(string symbol_name, out IntPtr symbol) {
				symbol = loader.GetProcAddress(handle, symbol_name);
			}

			public void Dispose() {
				if (handle != IntPtr.Zero) {
					loader.FreeLibrary(handle);
					handle = IntPtr.Zero;
				}
			}
		}
	}
}
