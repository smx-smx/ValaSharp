using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace GLibPorts {
	public abstract class OptionEntry {
		public string long_name;
		public char short_name;

		public OptionFlags flags;
		public OptionArg arg;

		public string arg_description;
		public string description;
	}

	public class OptionEntry<T> : OptionEntry {
		public delegate void OptionEntryFunc(T value);
		
		public T arg_data;
		public OptionEntryFunc onValue = null;

		public void notify() {
			if(onValue != null) {
				onValue(this.arg_data);
			}
		}

		public OptionEntry(string long_name, int short_name, OptionFlags flags, OptionArg arg, string description, string arg_desc, OptionEntryFunc onValue)
		{
			this.long_name = long_name;
			this.short_name = Convert.ToChar(short_name);

			this.flags = flags;
			this.arg = arg;
			this.description = description;

			this.onValue = onValue;
		}
	}
}