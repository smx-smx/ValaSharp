using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLibPorts
{
	public class GChar
	{
		/// <summary>
		/// Is a character 0-9 a-f A-F ?
		/// </summary>
		public static bool IsXDigit(char c) {
			if ('0' <= c && c <= '9') return true;
			if ('a' <= c && c <= 'f') return true;
			if ('A' <= c && c <= 'F') return true;
			return false;
		}
	}
}
