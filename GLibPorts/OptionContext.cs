using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLibPorts
{
	public partial class GLib
	{
		public class OptionContext
		{
			public OptionContext(string parameter_string) { }

			public void set_help_enabled(bool help_enabled) {
				throw new NotImplementedException();
			}

			public void add_main_entries(OptionEntry[] entries, string translation_domain) {
				throw new NotImplementedException();
			}

			public void parse(ref string[] args) {
				throw new NotImplementedException();
			}
		}
	}
}
