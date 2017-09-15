using System;
using System.Collections.Generic;
using CommandLine;
using Vala.Lang;

namespace ValaCompiler
{
	class CompilerOptions
	{
		[Option("path",
			Required = false,
			HelpText = "Look for required binaries in DIRECTORY",
			MetaValue = "DIRECTORY..."
		)]
		public string path { get; set; }

		[OptionList("vapidir",
			Required = false,
			HelpText = "Look for package bindings in DIRECTORY",
			MetaValue = "DIRECTORY...",
			DefaultValue = null
		)]
		public IList<string> vapi_directories { get; set; }

		[OptionList("girdir",
			Required = false,
			HelpText = "Look for .gir files in DIRECTORY",
			MetaValue = "DIRECTORY...",
			DefaultValue = null
		)]
		public IList<string> gir_directories { get; set; }

		[OptionList("metadatadir",
			Required = false,
			HelpText = "Look for GIR .metadata files in DIRECTORY",
			MetaValue = "DIRECTORY...",
			DefaultValue = null
		)]
		public IList<string> metadata_directories { get; set; }

		[OptionList("pkg",
			Required = false,
			HelpText = "Include binding for PACKAGE",
			MetaValue = "PACKAGE...",
			DefaultValue = null
		)]
		public IList<string> packages { get; set; }

		[Option("vapi",
			HelpText = "Output VAPI file name",
			MetaValue = "FILE"
		)]
		public string vapi_filename { get; set; }

		[Option("library",
			Required = false,
			HelpText = "Library name",
			MetaValue = "NAME"
		)]
		public string library { get; set; }

		[Option("shared-library",
			Required = false,
			HelpText = "Shared library name used in generated gir",
			MetaValue = "NAME"
		)]
		public string shared_library { get; set; }

		[Option("gir",
			Required = false,
			HelpText = "GObject-Introspection repository file name",
			MetaValue = "NAME-VERSION.gir"
		)]
		public string gir { get; set; }

		[Option('b', "basedir",
			Required = false,
			HelpText = "Base source directory",
			MetaValue = "DIRECTORY"
		)]
		public string basedir { get; set; }

		[Option('d', "directory",
			HelpText = "Change output directory from current working directory",
			MetaValue = "DIRECTORY"
		)]
		public string directory { get; set; }

		[Option("version",
			Required = false,
			HelpText = "Display version number"
		)]
		public bool version { get; set; }

		[Option("api-version",
			Required = false,
			HelpText = "Display API version number"
		)]
		public bool api_version { get; set; }

		[Option('C', "ccode",
			Required = false,
			HelpText = "Output C code"
		)]
		public bool ccode_only { get; set; }

		[Option('H', "header",
			Required = false,
			HelpText = "Output C header file",
			MetaValue = "FILE"
		)]
		public string header_filename { get; set; }

		[Option("use-header",
			Required = false,
			HelpText = "Use C header file"
		)]
		public bool use_header { get; set; }

		[Option("includedir",
			Required = false,
			HelpText = "Directory used to include the C header file",
			MetaValue = "DIRECTORY"
		)]
		public string includedir { get; set; }

		[Option('h', "internal-header",
			Required = false,
			HelpText = "Output internal C header file",
			MetaValue = "FILE"
		)]
		public string internal_header_filename { get; set; }

		[Option("internal-vapi",
			Required = false,
			HelpText = "Output vapi with internal api",
			MetaValue = "FILE"
		)]
		public string internal_vapi_filename { get; set; }

		[Option("fast-vapi",
			Required = false,
			HelpText = "Output vapi without performing symbol resolution"
		)]
		public string fast_vapi_filename { get; set; }

		[OptionList("use-fast-vapi",
			Required = false,
			HelpText = "Use --fast-vapi output during this compile",
			DefaultValue = null
		)]
		public IList<string> fast_vapis { get; set; }

		[Option("vapi-comments",
			Required = false,
			HelpText = "Include comments in generated vapi"
		)]
		public bool vapi_comments { get; set; }

		[Option("deps",
			Required = false,
			HelpText = "Write make-style dependency information to this file"
		)]
		public string dependencies { get; set; }

		[Option("symbols",
			Required = false,
			HelpText = "Output symbols file",
			MetaValue = "FILE"
		)]
		public string symbols_filename { get; set; }

		[Option('c', "compile",
			Required = false,
			HelpText = "Compile but do not link"
		)]
		public bool compile_only { get; set; }

		[Option('o', "output",
			Required = false,
			HelpText = "Place output in file FILE",
			MetaValue = "FILE"
		)]
		public string output { get; set; }

		[Option('g', "debug",
			Required = false,
			HelpText = "Produce debug information"
		)]
		public bool debug { get; set; }

		[Option("threads",
			Required = false,
			HelpText = "Enable multithreading support (DEPRECATED AND IGNORED)"
		)]
		public bool thread { get; set; }

		[Option("enable-mem-profiler",
			Required = false,
			HelpText = "Enable GLib memory profiler"
		)]
		public bool mem_profiler { get; set; }

		[Option('D', "define",
			Required = false,
			HelpText = "Define SYMBOL",
			MetaValue = "SYMBOL...",
			DefaultValue = null
		)]
		public IList<string> defines { get; set; }

		[Option("main",
			Required = false,
			HelpText = "Use SYMBOL as entry point",
			MetaValue = "SYMBOL..."
		)]
		public string entry_point { get; set; }

		[Option("nostdpkg",
			Required = false,
			HelpText = "Do not include standard packages"
		)]
		public bool nostdpkg { get; set; }

		[Option("disable-assert",
			Required = false,
			HelpText = "Disable assertions"
		)]
		public bool disable_assert { get; set; }

		[Option("enable-checking",
			Required = false,
			HelpText = "Enable additional run-time checks"
		)]
		public bool enable_checking { get; set; }

		[Option("enable-deprecated",
			Required = false,
			HelpText = "Enable deprecated features"
		)]
		public bool deprecated { get; set; }

		[Option("hide-internal",
			Required = false,
			HelpText = "Hide symbols marked as internal"
		)]
		public bool hide_internal { get; set; }

		[Option("enable-experimental",
			Required = false,
			HelpText = "Enable experimental features"
		)]
		public bool experimental { get; set; }

		[Option("disable-warnings",
			Required = false,
			HelpText = "Disable warnings"
		)]
		public bool disable_warnings { get; set; }

		[Option("fatal-warnings",
			Required = false,
			HelpText = "Treat warnings as fatal"
		)]
		public bool fatal_warnings { get; set; }

		[Option("disable-since-check",
			Required = false,
			HelpText = "Do not check whether used symbols exist in local packages"
		)]
		public bool disable_since_check { get; set; }

		[Option("enable-experimental-non-null",
			Required = false,
			HelpText = "Enable experimental enhancements for non-null types"
		)]
		public bool experimental_non_null { get; set; }

		[Option("enable-gobject-tracing",
			Required = false,
			HelpText = "Enable GObject creation tracing"
		)]
		public bool gobject_tracing { get; set; }

		[Option("cc",
			Required = false,
			HelpText = "Use COMMAND as C compiler command",
			MetaValue = "COMMAND"
		)]
		public string cc_command { get; set; }

		[OptionList('X', "Xcc",
			Required = false,
			HelpText = "Pass OPTION to the C compiler",
			MetaValue = "OPTION...",
			DefaultValue = null
		)]
		public IList<string> cc_options { get; set; }

		[Option("pkg-config",
			Required = false,
			HelpText = "Use COMMAND as pkg-config command",
			MetaValue = "COMMAND"
		)]
		public string pkg_config_command { get; set; }

		[Option("dump-tree",
			Required = false,
			HelpText = "Write code tree to FILE",
			MetaValue = "FILE"
		)]
		public string dump_tree { get; set; }

		[Option("save-temps",
			Required = false,
			HelpText = "Keep temporary files"
		)]
		public bool save_temps { get; set; }

		[Option("profile",
			Required = false,
			HelpText = "Use the given profile instead of the default",
			MetaValue = "PROFILE"
		)]
		public string profile { get; set; }

		[Option('q', "quiet",
			Required = false,
			HelpText = "Do not print messages to the console"
		)]
		public bool quiet_mode { get; set; }

		[Option('v', "verbose",
			Required = false,
			HelpText = "Print additional messages to the console"
		)]
		public bool verbose_mode { get; set; }

		[Option("no-color",
			Required = false,
			HelpText = "Disable colored output, alias for --color=never"
		)]
		public bool disable_colored_output { get; set; }

		[Option("color",
			Required = false,
			HelpText = "Enable color output, options are 'always', 'never', or 'auto'",
			MetaValue = "WHEN"
		)]
		public Report.Colored colored_output { get; set; }

		[Option("target-glib",
			Required = false,
			HelpText = "Target version of glib for code generation",
			MetaValue = "MAJOR.MINOR"
		)]
		public string target_glib { get; set; }

		[OptionList("gresources",
			Required = false,
			HelpText = "XML of gresources",
			MetaValue = "FILE...",
			DefaultValue = null
		)]
		public IList<string> gresources { get; set; }

		[OptionList("gresourcesdir",
			Required = false,
			HelpText = "Look for resources in DIRECTORY",
			MetaValue = "DIRECTORY...",
			DefaultValue = null
		)]
		public IList<string> gresources_directories { get; set; }

		[Option("enable-version-header",
			Required = false,
			HelpText = "Write vala build version in generated files"
		)]
		public bool enable_version_header { get; set; }

		[Option("disable-version-header",
			Required = false,
			HelpText = "Do not write vala build version in generated files"
		)]
		public bool disable_version_header { get; set; }

		/*[Option(
			Required = true,
			MetaValue = "FILE..."
		)]*/
		[ValueList(typeof(List<string>))]
		public IList<string> unparsed { get; set; }

		public IList<string> sources {
			get { return unparsed; }
			set { unparsed = value; }
		}
	}
}