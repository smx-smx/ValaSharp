using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLibPorts {
	public delegate string TranslateFunc(string str, IntPtr data);
	public delegate bool OptionParseFunc(OptionContext context, OptionGroup group, IntPtr data);
	public delegate void OptionErrorfunc(OptionContext context, OptionGroup group, IntPtr data);
	public delegate bool OptionArgFunc(string option_name, string value, IntPtr data);

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

	public class OptionContext {
		public bool help_enabled { get; set; } = false;
		public string description { get; set; }
		public string summary { get; set; }
		public bool ignore_unknown_options { get; set; } = false;

		public string parameter_string { get; set; }

		public bool strict_posix { get; set; } = false;

		private List<OptionGroup> groups = new List<OptionGroup>();
		public OptionGroup main_group { get; set; } = new OptionGroup();

		public TranslateFunc translate_func { get; set; }
		public string translation_domain { get; set; }

		private static string getProgramName() {
			return AppDomain.CurrentDomain.FriendlyName;
		}

		public OptionContext(string parameter_string = null) {
			this.parameter_string = parameter_string;
		}

		public void add_group(OptionGroup group) {
			this.groups.Add(group);
		}

		public void add_main_entries(IEnumerable<OptionEntry> entries, string translation_domain) {
			main_group.add_entries(entries);
			main_group.translation_domain = translation_domain;

			main_group.post_parse_hook = (context, group, data) => {
				foreach(OptionEntry entry in group.entries) {
					switch (entry.arg) {
						case OptionArg.NONE:
							((OptionEntry<bool>)entry).notify();
							break;
						case OptionArg.FILENAME:
						case OptionArg.STRING:
							((OptionEntry<string>)entry).notify();
							break;
						case OptionArg.FILENAME_ARRAY:
						case OptionArg.STRING_ARRAY:
							((OptionEntry<IList<string>>)entry).notify();
							break;
						case OptionArg.INT:
							((OptionEntry<int>)entry).notify();
							break;
						case OptionArg.INT64:
							((OptionEntry<Int64>)entry).notify();
							break;
						case OptionArg.DOUBLE:
							((OptionEntry<double>)entry).notify();
							break;
					}
				}
				return true;
			};
		}

		public string get_help(bool main_help, OptionGroup group) {
			return null;
		}

		private void print_help(bool main_help, OptionGroup group) {
			string help = get_help(main_help, group);
			Console.Write(help);
		}

		private bool NO_ARG(OptionEntry entry) {
			switch (entry.arg) {
				case OptionArg.NONE:
				case OptionArg.CALLBACK:
					return true;
				default:
					return entry.flags.HasFlag(OptionFlags.NO_ARG);
			}
		}

		private bool OPTIONAL_ARG(OptionEntry entry) {
			return entry.arg == OptionArg.CALLBACK && entry.flags.HasFlag(OptionFlags.OPTIONAL_ARG);
		}

		private bool parse_arg(
			OptionGroup group,
			OptionEntry entry,
			string value,
			string option_name
		) {
			Debug.Assert(value != null || OPTIONAL_ARG(entry) || NO_ARG(entry));

			switch (entry.arg) {
				case OptionArg.NONE: {
						OptionEntry<bool> centry = entry as OptionEntry<bool>;
						centry.arg_data = !centry.flags.HasFlag(OptionFlags.REVERSE);
						break;
					}
				case OptionArg.STRING:
				case OptionArg.FILENAME: {
						OptionEntry<string> centry = entry as OptionEntry<string>;
						centry.arg_data = value;
						break;
					}
				case OptionArg.STRING_ARRAY:
				case OptionArg.FILENAME_ARRAY: {
						OptionEntry<IList<string>> centry = entry as OptionEntry<IList<string>>;
						if (centry.arg_data == null)
							centry.arg_data = new List<string>();
						centry.arg_data.Add(value);
						break;
					}
				case OptionArg.INT: {
						OptionEntry<int> centry = entry as OptionEntry<int>;
						centry.arg_data = int.Parse(value);
						break;
					}
				case OptionArg.CALLBACK: {
						OptionEntry<OptionArgFunc> centry = entry as OptionEntry<OptionArgFunc>;
						string data = null;
						if (
							centry.flags.HasFlag(OptionFlags.OPTIONAL_ARG) ||
							centry.flags.HasFlag(OptionFlags.NO_ARG)
						) {
							data = null;
						} else if (centry.flags.HasFlag(OptionFlags.FILENAME)) {
							data = value;
						}
						if (centry.flags.HasFlag(OptionFlags.NO_ARG | OptionFlags.OPTIONAL_ARG) && data == null) {
							return false;
						}

						bool retval = centry.arg_data(option_name, data, group.user_data);
						if (!retval) {
							throw new Exception("Error parsing option " + option_name);
						}
						return retval;
					}
				case OptionArg.DOUBLE: {
						OptionEntry<double> centry = entry as OptionEntry<double>;
						centry.arg_data = double.Parse(value);
						break;
					}
				case OptionArg.INT64: {
						OptionEntry<Int64> centry = entry as OptionEntry<Int64>;
						centry.arg_data = Int64.Parse(value);
						break;
					}
				default:
					Debug.Assert(false);
					break;
			}
			return true;
		}

		private bool parse_short_option(
			OptionGroup group,
			int idx,
			ref int new_idx,
			char arg,
			ref int argc,
			ref string[] argv,
			out bool parsed
		) {
			parsed = false;

			for (int j = 0; j < group.entries.Count; j++) {
				if (arg == group.entries[j].short_name) {
					string value;
					string option_name = group.entries[j].short_name.ToString();

					if (NO_ARG(group.entries[j])) {
						value = null;
					} else {
						if (new_idx > idx) {
							throw new Exception("Error parsing option " + option_name);
						}

						if (idx < argc - 1) {
							if (!OPTIONAL_ARG(group.entries[j])) {
								value = argv[idx + 1];
								new_idx = idx + 1;
							} else {
								if (argv[idx + 1][0] == '-') {
									value = null;
								} else {
									value = argv[idx + 1];
									new_idx = idx + 1;
								}
							}
						} else if (idx >= argc - 1 && OPTIONAL_ARG(group.entries[j])) {
							value = null;
						} else {
							throw new Exception("Missing argument for " + option_name);
						}
					}

					if (!parse_arg(group, group.entries[j], value, option_name)) {
						return false;
					}

					parsed = true;
				}
			}
			return true;
		}

		private bool parse_remaining_arg(
			OptionGroup group,
			ref int idx,
			ref int argc, ref string[] argv,
			out bool parsed
		) {
			parsed = false;

			int j;
			for (j = 0; j < group.entries.Count; j++) {
				if (idx >= argc) {
					return true;
				}
				if (group.entries[j].long_name.Length > 0)
					continue;

				switch (group.entries[j].arg) {
					case OptionArg.CALLBACK:
					case OptionArg.STRING_ARRAY:
					case OptionArg.FILENAME_ARRAY:
						break;
					default:
						return false;
				}

				if (!parse_arg(group, group.entries[j], argv[idx], ""))
					return false;

				parsed = true;
				return true;
			}
			return true;
		}

		private bool parse_long_option(
			OptionGroup group,
			ref int idx,
			string arg,
			bool aliased,
			ref int argc,
			ref string[] argv,
			out bool parsed
		) {
			parsed = false;

			int j;
			for (j = 0; j < group.entries.Count; j++) {
				if (idx >= argc) {
					return true;
				}

				if (aliased && (group.entries[j].flags.HasFlag(OptionFlags.NOALIAS)))
					continue;

				if (NO_ARG(group.entries[j]) && group.entries[j].long_name == arg) {
					string option_name;
					bool retval;

					option_name = "--" + group.entries[j].long_name;
					retval = parse_arg(group, group.entries[j], null, option_name);
					parsed = true;
					return retval;
				} else {
					int len = group.entries[j].long_name.Length;
					if (group.entries[j].long_name == arg && (
						arg.Length <= len || arg[len] == '='
					)) {
						string value = null;
						string option_name = "--" + group.entries[j].long_name;

						if (arg.Length > len && arg[len] == '=') {
							value = arg.Substring(len + 1);
						} else if (idx < argc - 1) {
							if (!OPTIONAL_ARG(group.entries[j])) {
								value = argv[idx + 1];
								idx++;
							} else {
								if (argv[idx + 1][0] == '=') {
									bool retval = parse_arg(group, group.entries[j], null, option_name);
									parsed = true;
									return retval;
								} else {
									value = argv[idx + 1];
									idx++;
								}
							}
						} else if (idx >= argc - 1 && OPTIONAL_ARG(group.entries[j])) {
							bool retval = parse_arg(group, group.entries[j], null, option_name);
							parsed = true;
							return retval;
						} else {
							throw new Exception("Missing argument for " + option_name);
						}

						if (!parse_arg(group, group.entries[j], value, option_name)) {
							return false;
						}

						parsed = true;
					}
				}
			}
			return true;
		}

		private bool has_h_entry() {
			foreach (OptionEntry entry in this.main_group.entries) {
				if (entry.short_name == 'h')
					return true;
			}

			foreach (OptionGroup group in this.groups) {
				foreach (OptionEntry entry in group.entries) {
					if (entry.short_name == 'h')
						return true;
				}
			}

			return false;
		}

		public bool parse(string[] argv) {
			foreach (OptionGroup group in groups) {
				if (group.pre_parse_hook != null) {
					if (!group.pre_parse_hook(this, group, group.user_data)) {
						throw new Exception("pre_parse_hook failed");
					}
				}
			}

			if (main_group.pre_parse_hook != null) {
				if (!main_group.pre_parse_hook(this, main_group, main_group.user_data)) {
					throw new Exception("pre_parse_hook failed (main_group)");
				}
			}

			int argc = argv.Length;
			if (argc > 0) {
				bool stop_parsing = false;
				bool has_unknown = false;
				int separator_pos = 0;

				for (int i = 0; i < argc; i++) {
					bool parsed = false;
					int arglen = argv[i].Length;
					string arg;

					if (argv[i][0] == '-' && arglen > 1 && !stop_parsing) {
						if (argv[i][1] == '-') {
							/* -- option */
							arg = argv[i].Substring(2);

							/* '--' terminates list of arguments */
							if (arg.Length == 0) {
								separator_pos = i;
								stop_parsing = true;
								continue;
							}

							/* Handle help options */
							if (help_enabled) {
								if (arg == "help") {
									print_help(true, null);
								} else if (arg == "help-all") {
									print_help(false, null);
								} else if (arg.StartsWith("help-")) {
									string rest = arg.Substring(5);
									foreach (OptionGroup group in this.groups) {
										if (group.name == rest) {
											print_help(false, group);
										}
									}
								}
							}

							if (!parse_long_option(
									main_group,
									ref i,
									arg, false,
									ref argc,
									ref argv,
									out parsed
								)
							) {
								throw new Exception("fail");
							}

							if (parsed)
								continue;

							foreach (OptionGroup group in this.groups) {
								if (!parse_long_option(
									group,
									ref i,
									arg,
									false,
									ref argc,
									ref argv,
									out parsed
								)) {
									throw new Exception("fail");
								}

								if (parsed)
									break;
							}

							if (parsed)
								continue;

							/* Now look for --<group>-<option> */
							int dashPos = arg.IndexOf('-');
							if (dashPos > -1) {
								string dash = arg.Substring(dashPos);
								foreach (OptionGroup group in this.groups) {
									string cmp = arg.Substring(0, dashPos);
									if (group.name == cmp) {
										if (!parse_long_option(
											group,
											ref i,
											dash.Substring(1),
											true,
											ref argc,
											ref argv,
											out parsed
										)) {
											throw new Exception("fail");
										}

										if (parsed)
											break;
									}
								}
							}

							if (ignore_unknown_options)
								continue;
						} else {
							/* short option */
							int new_i = i;
							int arg_length;

							arg = argv[i].Substring(1);
							arg_length = arg.Length;

							bool has_h_entry = this.has_h_entry();
							bool[] nulled_out = new bool[arg_length];

							for (int j = 0; j < arg_length; j++) {
								if (help_enabled && arg[j] == '?' ||
									(arg[j] == 'h' && !has_h_entry)
								) {
									print_help(true, null);
								}
								parsed = false;
								if (!parse_short_option(
									main_group,
									i,
									ref new_i,
									arg[j],
									ref argc, ref argv,
									out parsed
								)) {
									throw new Exception("fail");
								}

								if (!parsed) {
									/* Try the groups */
									foreach(OptionGroup group in this.groups) {
										if(!parse_short_option(
											group, i, ref new_i, arg[j], ref argc, ref argv,
											out parsed
										)) {
											throw new Exception("fail");
										}
										if (parsed) {
											break;
										}
									}
								}

								if (ignore_unknown_options && parsed)
									nulled_out[j] = true;
								else if (ignore_unknown_options) {
									continue;
								} else if (!parsed) {
									break;
								}
								/* !context->ignore_unknown && parsed */
							}
							if (ignore_unknown_options) {
								string new_arg = null;
								StringBuilder sb = new StringBuilder();
								int arg_index = 0;
								for(int j=0; j<arg_length; j++) {
									if (!nulled_out[j]) {
										if (new_arg == null) {
											sb[arg_index++] = arg[j];
										}
									}
								}
								new_arg = sb.ToString();
								i = new_i;
							} else if (parsed) {
								i = new_i;
							}
						}
						if (!parsed) {
							has_unknown = true;
						}

						if(!parsed && !ignore_unknown_options) {
							throw new Exception("Unknown option " + argv[i]);
						}

					} else {
						if (strict_posix) {
							stop_parsing = true;
						}

						/* Collect remaining args */
						if (!parse_remaining_arg(main_group, ref i, ref argc, ref argv, out parsed)) {
							throw new Exception("fail");
						}

						if (!parsed && (has_unknown || argv[i][0] == '-'))
							separator_pos = 0;
					}
				}
				if(separator_pos > 0) {

				}
			}

			/* Call post-parse hooks */
			foreach (OptionGroup group in this.groups) {
				if (group.post_parse_hook != null) {
					if (!group.post_parse_hook(this, group, group.user_data)) {
						throw new Exception("fail in post_hook");
					}
				}
			}
			if (main_group.post_parse_hook != null) {
				if (!main_group.post_parse_hook(this, main_group, main_group.user_data)) {
					throw new Exception("fail in post_hook (main)");
				}
			}

			if (argc > 0) {
				int j;
				int k;
				for (int i = 0; i < argc; i++) {
					for (k = i; k < argc; k++) {
						if (argv[k] != null)
							break;
					}
					if (k > i) {
						k -= i;
						for (j = i + k; k < argc; j++) {
							argv[j - k] = argv[j];
							argv[j] = null;
						}
						argc -= k;
					}
				}
			}
			return true;
		}

		public bool parse_strv(IEnumerable<string> argv, out IEnumerable<string> out_argv) {
			out_argv = new string[] { };
			return false;
		}
	}
}
