using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLibPorts
{
	public partial class GLib
	{
		public abstract class OptionEntry
		{
		}

		public delegate bool OptionDelegate(string option_name, string val);

		// Doesn't try to be the same as the original. Just as close as vala
		public class OptionEntry<T> : OptionEntry
		{
			public string arg;
			public dynamic arg_data;
			public char arg_description;
			public OptionArg description;
			public int flags;
			public string long_name;
			public string short_name;

			public OptionEntry(
				string arg,
				dynamic arg_data,
				OptionFlags arg_description,
				OptionArg description,
				ref T flags,
				string long_name,
				string short_name
			) {

			}

			public OptionEntry(
				string arg,
				dynamic arg_data,
				OptionFlags arg_description,
				OptionArg description,
				T flags,
				string long_name,
				string short_name
			) {

			}
		}
	}
}
