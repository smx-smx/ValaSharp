using System;
using System.Collections.Generic;

namespace GLibPorts {
	public class OptionGroup {
		public readonly List<OptionEntry> entries = new List<OptionEntry>();

		public OptionParseFunc pre_parse_hook { get; set; }
		public OptionParseFunc post_parse_hook { get; set; }

		public OptionErrorfunc error_hook { get; set; }
		public TranslateFunc translate_func { get; set; }
		public string translation_domain { get; set; }

		public string name { get; set; }

		public IntPtr user_data;

		public void add_entries(IEnumerable<OptionEntry> entries) {
			this.entries.AddRange(entries);
		}
	}
}