using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GLibPorts
{
	public partial class GLib {
		public class Shell {
			public static string quote(string str) {
				return "\"" + Regex.Replace(str, @"(\\+)$", @"$1$1") + "\"";
			}
		}
	}
}
