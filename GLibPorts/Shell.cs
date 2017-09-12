using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLibPorts
{
	public partial class GLib {
		public class Shell {
			public static string quote(string str) {
				StringBuilder sb = new StringBuilder("'");
				foreach(char ch in str) {
					if (ch == '\'')
						sb.Append("'\\''");
					else
						sb.Append(ch);
				}
				sb.Append('\'');
				return sb.ToString();
			}
		}
	}
}
