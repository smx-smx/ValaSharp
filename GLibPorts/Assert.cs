using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLibPorts {
	public partial class GLib {
		public static void assert_not_reached() {
			StackTrace stackTrace = new StackTrace();
			StackFrame parent = stackTrace.GetFrame(1);

			Debug.Assert(true, string.Format("Assertion failed in {0} ({1}:{2})",
				parent.GetMethod().Name,
				parent.GetFileName(),
				parent.GetFileLineNumber()
			));
		}
	}
}
