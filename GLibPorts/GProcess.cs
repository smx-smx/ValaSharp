using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLibPorts
{
	public partial class GLib {
		public class GProcess {
			public static string get_executable_suffix() {
				if (Utils.IsUnix())
					return string.Empty;
				return ".exe";
			}
		}
	}
}
