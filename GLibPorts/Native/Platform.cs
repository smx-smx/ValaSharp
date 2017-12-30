using GLibPorts.Native.Varargs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLibPorts.Native {
	public static class Platform {
		public static IModuleLoader ModuleLoader;
		public static IStrings Strings;
		
		public static IVariableCombiner MakeVariableCombiner(params VariableArgument[] args) {
			if (Utils.IsUnix())
				return new UnixVariableCombiner(args);
			else
				return new Win32VariableCombiner(args);
		}

		static Platform() {
			if (Utils.IsUnix()) {
				ModuleLoader = new Unix.UnixModuleLoader();
				Strings = new Unix.UnixStrings();
			} else {
				ModuleLoader = new Win32.Win32ModuleLoader();
				Strings = new Win32.Win32Strings();
			}
		}
	}
}
