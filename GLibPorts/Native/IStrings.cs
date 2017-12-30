using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLibPorts.Native {
	public interface IStrings {
		void sprintf(StringBuilder sb, string format, IntPtr args);
		int vscprintf(string format, IntPtr args);
	}
}
