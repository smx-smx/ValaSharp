using GLibPorts.Native.Varargs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLibPorts.Native {
	public interface IFileStream : IDisposable {
		int fileno();
		int printf(string format, params VariableArgument[] args);
		int puts(string str);
		int putc(int c);
	}
}
