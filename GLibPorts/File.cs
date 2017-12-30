using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLibPorts {
	public partial class GLib {
		internal class File {
			public static IntPtr stdin { get; internal set; }
			public static IntPtr stderr { get; internal set; }
			public static IntPtr stdout { get; internal set; }

			public static void InitializeStatic() {
				if (Utils.IsUnix()) {
					Native.Unix.UnixFile.InitializeStatic();
				} else {
					Native.Win32.Win32File.InitializeStatic();
				}
			}

			public static void DisposeStatic() {
				if (Utils.IsUnix()) {
					Native.Unix.UnixFile.DisposeStatic();
				} else {
					Native.Win32.Win32File.DisposeStatic();
				}
			}
		}
	}
}
