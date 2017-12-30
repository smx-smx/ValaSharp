using GLibPorts.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLibPorts {
	public partial class GLib {
		/// <summary>
		/// Clears up unmanaged resources used through GLib's lifetime
		/// </summary>
		public static void GLibDispose() {
			FileStream.DisposeStatic();
			File.DisposeStatic();
		}

		public static void GLibInitialize() {
			File.InitializeStatic();
			FileStream.InitializeStatic();
		}

		public static IFileStream stdin => FileStream.stdin;
		public static IFileStream stderr => FileStream.stderr;
		public static IFileStream stdout => FileStream.stdout;
	}
}
