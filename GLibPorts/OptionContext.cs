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
			private bool help_enabled = false;
			private Dictionary<string, OptionEntry> entries = new Dictionary<string, OptionEntry>();

			public OptionContext(string parameter_string) { }

			public void set_help_enabled(bool help_enabled)
			{
				this.help_enabled = help_enabled;
			}

			public void add_main_entries(object[] entries, string translation_domain)
			{
				foreach (object entry in entries)
				{
					if(!(entry is OptionEntry))
						throw new ArgumentException("Invalid type");
					Type t = entry.GetType().GetGenericTypeDefinition();
				}
			}

			public void parse(ref string[] args) {
				foreach (string arg in args)
				{
				}
			}
		}
	}
}
