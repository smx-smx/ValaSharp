using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLibPorts {
	[Flags]
	public enum OptionFlags {
		NONE = 0,
		HIDDEN = 1 << 0,
		IN_MAIN = 1 << 1,
		REVERSE = 1 << 2,
		NO_ARG = 1 << 3,
		FILENAME = 1 << 4,
		OPTIONAL_ARG = 1 << 5,
		NOALIAS = 1 << 6
	}
}
