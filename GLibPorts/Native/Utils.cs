using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLibPorts.Native {
	public class Utils {
		/// <summary>
		/// Clears up unmanaged resources used through GLib's lifetime
		/// </summary>
		public static void GLibDispose() {
			GLib.FileStream.DisposeStatic();
			File.DisposeStatic();
		}

		public static void GLibInitialize() {
			File.InitializeStatic();
			GLib.FileStream.InitializeStatic();
		}
	}
}
