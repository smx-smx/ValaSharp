using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLibPorts {
	public static class CharExtensions {
		public static bool IsXDigit(this char @this) {
			return GChar.isxdigit(@this);
		}

		public static bool IsDigit(this char @this) {
			return Char.IsDigit(@this);
		}

		public static char ToLower(this char @this) {
			return Char.ToLower(@this);
		}

		public static char ToUpper(this char @this) {
			return Char.ToUpper(@this);
		}

		public static bool IsLetter(this char @this) {
			return Char.IsLetter(@this);
		}

		public static bool IsLetterOrDigit(this char @this) {
			return Char.IsLetterOrDigit(@this);
		}
	}
}
