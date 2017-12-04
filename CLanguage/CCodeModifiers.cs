using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLanguage {
	[Flags]
	public enum CCodeModifiers {
		NONE = 0,
		STATIC = 1 << 0,
		REGISTER = 1 << 1,
		EXTERN = 1 << 2,
		INLINE = 1 << 3,
		VOLATILE = 1 << 4,
		DEPRECATED = 1 << 5,
		THREAD_LOCAL = 1 << 6,
		INTERNAL = 1 << 7,
		CONST = 1 << 8,
		UNUSED = 1 << 9,
		CONSTRUCTOR = 1 << 10,
		DESTRUCTOR = 1 << 11,
		FORMAT_ARG = 1 << 12,
		PRINTF = 1 << 13,
		SCANF = 1 << 14
	}

}
