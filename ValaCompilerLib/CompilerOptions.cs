using GLibPorts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang;

namespace ValaCompilerLib {
	public class CompilerOptions {
		public string path;

		public string basedir;
		public string directory;
		public bool version;
		public bool api_version;

		public IList<string> sources;
		public IList<string> vapi_directories;
		public IList<string> gir_directories;
		public IList<string> metadata_directories;
		public string vapi_filename;
		public string library;
		public string shared_library;
		public string gir;

		public IList<string> packages;

		public IList<string> fast_vapis;
		public string target_glib;

		public IList<string> gresources;
		public IList<string> gresources_directories;

		public bool ccode_only;
		public bool dry_run;

		public string header_filename;
		public bool use_header;
		public string internal_header_filename;
		public string internal_vapi_filename;
		public string fast_vapi_filename;
		public bool vapi_comments;
		public string symbols_filename;
		public string includedir;
		public bool compile_only;
		public string output;

		public bool valac_debug;
		public bool debug;

		public bool thread;
		public bool mem_profiler;
		public bool disable_assert;
		public bool enable_checking;
		public bool deprecated;
		public bool hide_internal;
		public bool experimental;
		public bool experimental_non_null;
		public bool gobject_tracing;
		public bool disable_since_check;
		public bool disable_warnings;
		public string cc_command;

		public IList<string> cc_options;
		public string pkg_config_command;
		public string dump_tree;
		public bool save_temps;

		public IList<string> defines;
		public bool quiet_mode;
		public bool verbose_mode;
		public string profile;
		public bool nostdpkg;
		public bool enable_version_header;
		public bool disable_version_header;
		public bool fatal_warnings;
		public bool disable_colored_output;
		public Report.Colored colored_output = Report.Colored.AUTO;

		public string dependencies;
		public string entry_point;
		public bool run_output;

		private bool option_parse_color(string option_name, string val, IntPtr data) {
			switch (val) {
			case "auto": colored_output = Report.Colored.AUTO; break;
			case "never": colored_output = Report.Colored.NEVER; break;
			case null:
			case "always": colored_output = Report.Colored.ALWAYS; break;
			default: throw new Exception($"Invalid --color argument '{val}'");
			}
			return true;
		}

		public void parse_args(string[] args) {
			OptionEntry[] options = new OptionEntry[]{
				new OptionEntry<string>(
					"path", 0, 0, OptionArg.STRING,
					"Use the gcc/toolchain binaries located in DIRECTORY", "DIRECTORY",
					(value) => { path = value; }
				),
				new OptionEntry<IList<string>>(
					"vapidir", 0, 0, OptionArg.FILENAME_ARRAY,
					"Look for package bindings in DIRECTORY", "DIRECTORY...",
					(value) => { vapi_directories = value; }
				),
				new OptionEntry<IList<string>>(
					"girdir", 0, 0, OptionArg.FILENAME_ARRAY,
					"Look for .gir files in DIRECTORY", "DIRECTORY...",
					(value) => { gir_directories = value; }
				),
				new OptionEntry<IList<string>>(
					"metadatadir", 0, 0, OptionArg.FILENAME_ARRAY,
					"Look for GIR .metadata files in DIRECTORY", "DIRECTORY...",
					(value) => { metadata_directories = value; }
				),
				new OptionEntry<IList<string>>(
					"pkg", 0, 0, OptionArg.STRING_ARRAY,
					"Include binding for PACKAGE", "PACKAGE...",
					(value) => { packages = value; }
				),
				new OptionEntry<string>(
					"vapi", 0, 0, OptionArg.FILENAME,
					"Output VAPI file name", "FILE",
					(value) => { vapi_filename = value; }
				),
				new OptionEntry<string>(
					"library", 0, 0, OptionArg.STRING,
					"Library name", "NAME",
					(value) => { library = value; }
				),
				new OptionEntry<string>(
					"shared-library", 0, 0, OptionArg.STRING,
					"Shared library name used in generated gir", "NAME",
					(value) => { shared_library = value; }
				),
				new OptionEntry<string>(
					"gir", 0, 0, OptionArg.STRING,
					"GObject-Introspection repository file name", "NAME-VERSION.gir",
					(value) => { gir = value; }
				),
				new OptionEntry<string>(
					"basedir", 'b', 0, OptionArg.FILENAME,
					"Base source directory", "DIRECTORY",
					(value) => { basedir = value; }
				),
				new OptionEntry<string>(
					"directory", 'd', 0, OptionArg.FILENAME,
					"Change output directory from current working directory", "DIRECTORY",
					(value) => { directory = value; }
				),
				new OptionEntry<bool>(
					"version", 0, 0, OptionArg.NONE,
					"Display version number", null,
					(value) => { version = value; }
				),
				new OptionEntry<bool>(
					"api-version", 0, 0, OptionArg.NONE,
					"Display API version number", null,
					(value) => { api_version = value; }
				),
				new OptionEntry<bool>(
					"ccode", 'C', 0, OptionArg.NONE,
					"Output C code", null,
					(value) => { ccode_only = value; }
				),
				new OptionEntry<bool>(
					null, 'n', 0, OptionArg.NONE,
					"Dry Run", null,
					(value) => { dry_run = value; }
				),
				new OptionEntry<string>(
					"header", 'H', 0, OptionArg.FILENAME,
					"Output C header file", "FILE",
					(value) => { header_filename = value; }
				),
				new OptionEntry<bool>(
					"use-header", 0, 0, OptionArg.NONE,
					"Use C header file", null,
					(value) => { use_header = value; }
				),
				new OptionEntry<string>(
					"includedir", 0, 0, OptionArg.FILENAME,
					"Directory used to include the C header file", "DIRECTORY",
					(value) => { includedir = value; }
				),
				new OptionEntry<string>(
					"internal-header", 'h', 0, OptionArg.FILENAME,
					"Output internal C header file", "FILE",
					(value) => { internal_header_filename = value; }
				),
				new OptionEntry<string>(
					"internal-vapi", 0, 0, OptionArg.FILENAME,
					"Output vapi with internal api", "FILE",
					(value) => { internal_vapi_filename = value; }
				),
				new OptionEntry<string>(
					"fast-vapi", 0, 0, OptionArg.STRING,
					"Output vapi without performing symbol resolution", null,
					(value) => { fast_vapi_filename = value; }
				),
				new OptionEntry<IList<string>>(
					"use-fast-vapi", 0, 0, OptionArg.STRING_ARRAY,
					"Use --fast-vapi output during this compile", null,
					(value) => { fast_vapis = value; }
				),
				new OptionEntry<bool>(
					"vapi-comments", 0, 0, OptionArg.NONE,
					"Include comments in generated vapi", null,
					(value) => { vapi_comments = value; }
				),
				new OptionEntry<string>(
					"deps", 0, 0, OptionArg.STRING,
					"Write make-style dependency information to this file", null,
					(value) => { dependencies = value; }
				),
				new OptionEntry<string>(
					"symbols", 0, 0, OptionArg.FILENAME,
					"Output symbols file", "FILE",
					(value) => { symbols_filename = value; }
				),
				new OptionEntry<bool>(
					"compile", 'c', 0, OptionArg.NONE,
					"Compile but do not link", null,
					(value) => { compile_only = value; }
				),
				new OptionEntry<string>(
					"output", 'o', 0, OptionArg.FILENAME,
					"Place output in file FILE", "FILE",
					(value) => { output = value; }
				),
				new OptionEntry<bool>(
					"debug", 'g', 0, OptionArg.NONE,
					"Produce debug information", null,
					(value) => { debug = value; }
				),
				new OptionEntry<bool>(
					"valac-debug", 0, 0, OptionArg.NONE,
					"Start the debugger", null,
					(value) => { valac_debug = value; }
				),
				new OptionEntry<bool>(
					"thread", 0, 0, OptionArg.NONE,
					"Enable multithreading support (DEPRECATED AND IGNORED)", null,
					(value) => { thread = value; }
				),
				new OptionEntry<bool>(
					"enable-mem-profiler", 0, 0, OptionArg.NONE,
					"Enable GLib memory profiler", null,
					(value) => { mem_profiler = value; }
				),
				new OptionEntry<IList<string>>(
					"define", 'D', 0, OptionArg.STRING_ARRAY,
					"Define SYMBOL", "SYMBOL...",
					(value) => { defines = value; }
				),
				new OptionEntry<string>(
					"main", 0, 0, OptionArg.STRING,
					"Use SYMBOL as entry point", "SYMBOL...",
					(value) => { entry_point = value; }
				),
				new OptionEntry<bool>(
					"nostdpkg", 0, 0, OptionArg.NONE,
					"Do not include standard packages", null,
					(value) => { nostdpkg = value; }
				),
				new OptionEntry<bool>(
					"disable-assert", 0, 0, OptionArg.NONE,
					"Disable assertions", null,
					(value) => { disable_assert = value; }
				),
				new OptionEntry<bool>(
					"enable-checking", 0, 0, OptionArg.NONE,
					"Enable additional run-time checks", null,
					(value) => { enable_checking = value; }
				),
				new OptionEntry<bool>(
					"enable-deprecated", 0, 0, OptionArg.NONE,
					"Enable deprecated features", null,
					(value) => { deprecated = value; }
				),
				new OptionEntry<bool>(
					"hide-internal", 0, 0, OptionArg.NONE,
					"Hide symbols marked as internal", null,
					(value) => { hide_internal = value; }
				),
				new OptionEntry<bool>(
					"enable-experimental", 0, 0, OptionArg.NONE,
					"Enable experimental features", null,
					(value) => { experimental = value; }
				),
				new OptionEntry<bool>(
					"disable-warnings", 0, 0, OptionArg.NONE,
					"Disable warnings", null,
					(value) => { disable_warnings = value; }
				),
				new OptionEntry<bool>(
					"fatal-warnings", 0, 0, OptionArg.NONE,
					"Treat warnings as fatal", null,
					(value) => { fatal_warnings = value; }
				),
				new OptionEntry<bool>(
					"disable-since-check", 0, 0, OptionArg.NONE,
					"Do not check whether used symbols exist in local packages", null,
					(value) => { disable_since_check = value; }
				),
				new OptionEntry<bool>(
					"enable-experimental-non-null", 0, 0, OptionArg.NONE,
					"Enable experimental enhancements for non-null types", null,
					(value) => { experimental_non_null = value; }
				),
				new OptionEntry<bool>(
					"enable-gobject-tracing", 0, 0, OptionArg.NONE,
					"Enable GObject creation tracing", null,
					(value) => { gobject_tracing = value; }
				),
				new OptionEntry<string>(
					"cc", 0, 0, OptionArg.STRING,
					"Use COMMAND as C compiler command", "COMMAND",
					(value) => { cc_command = value; }
				),
				new OptionEntry<IList<string>>(
					"Xcc", 'X', 0, OptionArg.STRING_ARRAY,
					"Pass OPTION to the C compiler", "OPTION...",
					(value) => { cc_options = value; }
				),
				new OptionEntry<string>(
					"pkg-config", 0, 0, OptionArg.STRING,
					"Use COMMAND as pkg-config command", "COMMAND",
					(value) => { pkg_config_command = value; }
				),
				new OptionEntry<string>(
					"dump-tree", 0, 0, OptionArg.FILENAME,
					"Write code tree to FILE", "FILE",
					(value) => { dump_tree = value; }
				),
				new OptionEntry<bool>(
					"save-temps", 0, 0, OptionArg.NONE,
					"Keep temporary files", null,
					(value) => { save_temps = value; }
				),
				new OptionEntry<string>(
					"profile", 0, 0, OptionArg.STRING,
					"Use the given profile instead of the default", "PROFILE",
					(value) => { profile = value; }
				),
				new OptionEntry<bool>(
					"quiet", 'q', 0, OptionArg.NONE,
					"Do not print messages to the console", null,
					(value) => { quiet_mode = value; }
				),
				new OptionEntry<bool>(
					"verbose", 'v', 0, OptionArg.NONE,
					"Print additional messages to the console", null,
					(value) => { verbose_mode = value; }
				),
				new OptionEntry<bool>(
					"no-color", 0, 0, OptionArg.NONE,
					"Disable colored output, alias for --color=never", null,
					(value) => { disable_colored_output = value; }
				),
				new OptionEntry<OptionArgFunc>(
					"color", 0, OptionFlags.OPTIONAL_ARG, OptionArg.CALLBACK,
					"Enable color output, options are 'always', 'never', or 'auto'", "WHEN",
					(value) => { }
				){
					arg_data = new OptionArgFunc(option_parse_color)
				},
				new OptionEntry<string>(
					"target-glib", 0, 0, OptionArg.STRING,
					"Target version of glib for code generation", "MAJOR.MINOR",
					(value) => { target_glib = value; }
				),
				new OptionEntry<IList<string>>(
					"gresources", 0, 0, OptionArg.FILENAME_ARRAY,
					"XML of gresources", "FILE...",
					(value) => { gresources = value; }
				),
				new OptionEntry<IList<string>>(
					"gresourcesdir", 0, 0, OptionArg.FILENAME_ARRAY,
					"Look for resources in DIRECTORY", "DIRECTORY...",
					(value) => { gresources_directories = value; }
				),
				new OptionEntry<bool>(
					"enable-version-header", 0, 0, OptionArg.NONE,
					"Write vala build version in generated files", null,
					(value) => { enable_version_header = value; }
				),
				new OptionEntry<bool>(
					"disable-version-header", 0, 0, OptionArg.NONE,
					"Do not write vala build version in generated files", null,
					(value) => { disable_version_header = value; }
				),
				new OptionEntry<IList<string>>(
					"", 0, 0, OptionArg.FILENAME_ARRAY,
					 null, "FILE...",
					 (value) => { sources = value; }
				)
			};

			var opt_context = new OptionContext("- Vala Interpreter");
			opt_context.help_enabled = true;
			opt_context.add_main_entries(options, null);
			opt_context.parse(args);
		}
	}
}
