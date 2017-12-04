using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLibPorts {
	public class gstring {
		public static string nfill(int length, char fill_char) {
			StringBuilder sb = new StringBuilder();
			sb.Append(fill_char, length);
			return sb.ToString();
		}
	}
}
