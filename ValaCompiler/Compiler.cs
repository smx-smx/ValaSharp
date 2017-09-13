using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang;
using Vala.Lang.CodeNodes;
using System.IO;
using System.Reflection;
using Vala;
using Vala.Lang.Parser;
using CCodeGen;
using CCodeGen.Modules;
using Vala.Lang.Code;
using ValaConfig;

using static GLibPorts.GLib;

namespace ValaCompiler
{
	class Compiler
	{
		private const string DEFAULT_COLORS = "error=01;31:warning=01;35:note=01;36:caret=01;32:locus=01:quote=01";

		static string basedir;
		static string directory;
		static bool version;
		static bool api_version;

		static string[] sources;
		static string[] vapi_directories;
		static string[] gir_directories;
		static string[] metadata_directories;
		static string vapi_filename;
		static string library;
		static string shared_library;
		static string gir;

		static string[] packages;
		static string[] fast_vapis;
		static string target_glib;
		static string[] gresources;
		static string[] gresources_directories;

		static bool ccode_only;
		static string header_filename;
		static bool use_header;
		static string internal_header_filename;
		static string internal_vapi_filename;
		static string fast_vapi_filename;
		static bool vapi_comments;
		static string symbols_filename;
		static string includedir;
		static bool compile_only;
		static string output;
		static bool debug;
		static bool thread;
		static bool mem_profiler;
		static bool disable_assert;
		static bool enable_checking;
		static bool deprecated;
		static bool hide_internal;
		static bool experimental;
		static bool experimental_non_null;
		static bool gobject_tracing;
		static bool disable_since_check;
		static bool disable_warnings;
		static string cc_command;

		static string[] cc_options;
		static string pkg_config_command;
		static string dump_tree;
		static bool save_temps;

		static string[] defines;
		static bool quiet_mode;
		static bool verbose_mode;
		static string profile;
		static bool nostdpkg;
		static bool enable_version_header;
		static bool disable_version_header;
		static bool fatal_warnings;
		static bool disable_colored_output;
		static Report.Colored colored_output = Report.Colored.AUTO;
		static string dependencies;

		static string entry_point;
		static bool run_output;
		private CodeContext context;


		static OptionEntry[] options = new OptionEntry[] {
			new OptionEntry<string[]>("vapidir", 0, 0, OptionArg.FILENAME_ARRAY, ref vapi_directories, "Look for package bindings in DIRECTORY", "DIRECTORY..."),
			new OptionEntry<string[]>("girdir", 0, 0, OptionArg.FILENAME_ARRAY, ref gir_directories, "Look for .gir files in DIRECTORY", "DIRECTORY..."),
			new OptionEntry<string[]>("metadatadir", 0, 0, OptionArg.FILENAME_ARRAY, ref metadata_directories, "Look for GIR .metadata files in DIRECTORY", "DIRECTORY..."),
			new OptionEntry<string[]>("pkg", 0, 0, OptionArg.STRING_ARRAY, ref packages, "Include binding for PACKAGE", "PACKAGE..."),
			new OptionEntry<string>("vapi", 0, 0, OptionArg.FILENAME, ref vapi_filename, "Output VAPI file name", "FILE"),
			new OptionEntry<string>("library", 0, 0, OptionArg.STRING, ref library, "Library name", "NAME"),
			new OptionEntry<string>("shared-library", 0, 0, OptionArg.STRING, ref shared_library, "Shared library name used in generated gir", "NAME"),
			new OptionEntry<string>("gir", 0, 0, OptionArg.STRING, ref gir, "GObject-Introspection repository file name", "NAME-VERSION.gir"),
			new OptionEntry<string>("basedir", 'b', 0, OptionArg.FILENAME, ref basedir, "Base source directory", "DIRECTORY"),
			new OptionEntry<string>("directory", 'd', 0, OptionArg.FILENAME, ref directory, "Change output directory from current working directory", "DIRECTORY"),
			new OptionEntry<bool>("version", 0, 0, OptionArg.NONE, ref version, "Display version number", null),
			new OptionEntry<bool>("api-version", 0, 0, OptionArg.NONE, ref api_version, "Display API version number", null),
			new OptionEntry<bool>("ccode", 'C', 0, OptionArg.NONE, ref ccode_only, "Output C code", null),
			new OptionEntry<string>("header", 'H', 0, OptionArg.FILENAME, ref header_filename, "Output C header file", "FILE"),
			new OptionEntry<bool>("use-header", 0, 0, OptionArg.NONE, ref use_header, "Use C header file", null),
			new OptionEntry<string>("includedir", 0, 0, OptionArg.FILENAME, ref includedir, "Directory used to include the C header file", "DIRECTORY"),
			new OptionEntry<string>("internal-header", 'h', 0, OptionArg.FILENAME, ref internal_header_filename, "Output internal C header file", "FILE"),
			new OptionEntry<string>("internal-vapi", 0, 0, OptionArg.FILENAME, ref internal_vapi_filename, "Output vapi with internal api", "FILE"),
			new OptionEntry<string>("fast-vapi", 0, 0, OptionArg.STRING, ref fast_vapi_filename, "Output vapi without performing symbol resolution", null),
			new OptionEntry<string[]>("use-fast-vapi", 0, 0, OptionArg.STRING_ARRAY, ref fast_vapis, "Use --fast-vapi output during this compile", null),
			new OptionEntry<bool>("vapi-comments", 0, 0, OptionArg.NONE, ref vapi_comments, "Include comments in generated vapi", null),
			new OptionEntry<string>("deps", 0, 0, OptionArg.STRING, ref dependencies, "Write make-style dependency information to this file", null),
			new OptionEntry<string>("symbols", 0, 0, OptionArg.FILENAME, ref symbols_filename, "Output symbols file", "FILE"),
			new OptionEntry<bool>("compile", 'c', 0, OptionArg.NONE, ref compile_only, "Compile but do not link", null),
			new OptionEntry<string>("output", 'o', 0, OptionArg.FILENAME, ref output, "Place output in file FILE", "FILE"),
			new OptionEntry<bool>("debug", 'g', 0, OptionArg.NONE, ref debug, "Produce debug information", null),
			new OptionEntry<bool>("thread", 0, 0, OptionArg.NONE, ref thread, "Enable multithreading support (DEPRECATED AND IGNORED)", null),
			new OptionEntry<bool>("enable-mem-profiler", 0, 0, OptionArg.NONE, ref mem_profiler, "Enable GLib memory profiler", null),
			new OptionEntry<string[]>("define", 'D', 0, OptionArg.STRING_ARRAY, ref defines, "Define SYMBOL", "SYMBOL..."),
			new OptionEntry<string>("main", 0, 0, OptionArg.STRING, ref entry_point, "Use SYMBOL as entry point", "SYMBOL..."),
			new OptionEntry<bool>("nostdpkg", 0, 0, OptionArg.NONE, ref nostdpkg, "Do not include standard packages", null),
			new OptionEntry<bool>("disable-assert", 0, 0, OptionArg.NONE, ref disable_assert, "Disable assertions", null),
			new OptionEntry<bool>("enable-checking", 0, 0, OptionArg.NONE, ref enable_checking, "Enable additional run-time checks", null),
			new OptionEntry<bool>("enable-deprecated", 0, 0, OptionArg.NONE, ref deprecated, "Enable deprecated features", null),
			new OptionEntry<bool>("hide-internal", 0, 0, OptionArg.NONE, ref hide_internal, "Hide symbols marked as internal", null),
			new OptionEntry<bool>("enable-experimental", 0, 0, OptionArg.NONE, ref experimental, "Enable experimental features", null),
			new OptionEntry<bool>("disable-warnings", 0, 0, OptionArg.NONE, ref disable_warnings, "Disable warnings", null),
			new OptionEntry<bool>("fatal-warnings", 0, 0, OptionArg.NONE, ref fatal_warnings, "Treat warnings as fatal", null),
			new OptionEntry<bool>("disable-since-check", 0, 0, OptionArg.NONE, ref disable_since_check, "Do not check whether used symbols exist in local packages", null),
			new OptionEntry<bool>("enable-experimental-non-null", 0, 0, OptionArg.NONE, ref experimental_non_null, "Enable experimental enhancements for non-null types", null),
			new OptionEntry<bool>("enable-gobject-tracing", 0, 0, OptionArg.NONE, ref gobject_tracing, "Enable GObject creation tracing", null),
			new OptionEntry<string>("cc", 0, 0, OptionArg.STRING, ref cc_command, "Use COMMAND as C compiler command", "COMMAND"),
			new OptionEntry<string[]>("Xcc", 'X', 0, OptionArg.STRING_ARRAY, ref cc_options, "Pass OPTION to the C compiler", "OPTION..."),
			new OptionEntry<string>("pkg-config", 0, 0, OptionArg.STRING, ref pkg_config_command, "Use COMMAND as pkg-config command", "COMMAND"),
			new OptionEntry<string>("dump-tree", 0, 0, OptionArg.FILENAME, ref dump_tree, "Write code tree to FILE", "FILE"),
			new OptionEntry<bool>("save-temps", 0, 0, OptionArg.NONE, ref save_temps, "Keep temporary files", null),
			new OptionEntry<string>("profile", 0, 0, OptionArg.STRING, ref profile, "Use the given profile instead of the default", "PROFILE"),
			new OptionEntry<bool>("quiet", 'q', 0, OptionArg.NONE, ref quiet_mode, "Do not print messages to the console", null),
			new OptionEntry<bool>("verbose", 'v', 0, OptionArg.NONE, ref verbose_mode, "Print additional messages to the console", null),
			new OptionEntry<bool>("no-color", 0, 0, OptionArg.NONE, ref disable_colored_output, "Disable colored output, alias for --color=never", null),
			new OptionEntry<OptionDelegate>("color", 0, OptionFlags.OPTIONAL_ARG, OptionArg.CALLBACK, new OptionDelegate(option_parse_color), "Enable color output, options are 'always', 'never', or 'auto'", "WHEN"),
			new OptionEntry<string>("target-glib", 0, 0, OptionArg.STRING, ref target_glib, "Target version of glib for code generation", "MAJOR.MINOR"),
			new OptionEntry<string[]>("gresources", 0, 0, OptionArg.FILENAME_ARRAY, ref gresources, "XML of gresources", "FILE..."),
			new OptionEntry<string[]>("gresourcesdir", 0, 0, OptionArg.FILENAME_ARRAY, ref gresources_directories, "Look for resources in DIRECTORY", "DIRECTORY..."),
			new OptionEntry<bool>("enable-version-header", 0, 0, OptionArg.NONE, ref enable_version_header, "Write vala build version in generated files", null),
			new OptionEntry<bool>("disable-version-header", 0, 0, OptionArg.NONE, ref disable_version_header, "Do not write vala build version in generated files", null),
			new OptionEntry<string[]>("", 0, 0, OptionArg.FILENAME_ARRAY, ref sources, null, "FILE..."),
		};

		static bool option_parse_color(string option_name, string val) {
			switch (val) {
				case "auto": colored_output = Report.Colored.AUTO; break;
				case "never": colored_output = Report.Colored.NEVER; break;
				case null:
				case "always": colored_output = Report.Colored.ALWAYS; break;
				default: throw new OptionError_Failed("Invalid --color argument '%s'", val);
			}
			return true;
		}

		private int quit() {
			if (context.report.get_errors() == 0 && context.report.get_warnings() == 0) {
				return 0;
			}
			if (context.report.get_errors() == 0 && (!fatal_warnings || context.report.get_warnings() == 0)) {
				if (!quiet_mode) {
					stdout.printf("Compilation succeeded - %d warning(s)\n", context.report.get_warnings());
				}
				return 0;
			} else {
				if (!quiet_mode) {
					stdout.printf("Compilation failed: %d error(s), %d warning(s)\n", context.report.get_errors(), context.report.get_warnings());
				}
				return 1;
			}
		}

		private int run() {
			context = new CodeContext();
			CodeContext.push(context);

			if (disable_colored_output) {
				colored_output = Report.Colored.NEVER;
			}

			if (colored_output != Report.Colored.NEVER) {
				string env_colors = Environment.GetEnvironmentVariable("VALA_COLORS");
				if (env_colors != null) {
					context.report.set_colors(env_colors, colored_output);
				} else {
					context.report.set_colors(DEFAULT_COLORS, colored_output);
				}
			}


			// default to build executable
			if (!ccode_only && !compile_only && output == null) {
				// strip extension if there is one
				// else we use the default output file of the C compiler
				if (sources[0].LastIndexOf('.') != -1) {
					int dot = sources[0].LastIndexOf('.');
					output = Path.GetFileName(sources[0].Substring(0, dot));
				}
			}

			context.assert = !disable_assert;
			context.checking = enable_checking;
			context.deprecated = deprecated;
			context.since_check = !disable_since_check;
			context.hide_internal = hide_internal;
			context.experimental = experimental;
			context.experimental_non_null = experimental_non_null;
			context.gobject_tracing = gobject_tracing;
			context.report.enable_warnings = !disable_warnings;
			context.report.set_verbose_errors(!quiet_mode);
			context.verbose_mode = verbose_mode;
			context.version_header = !disable_version_header;

			context.ccode_only = ccode_only;
			if (ccode_only && cc_options != null) {
				Report.warning(null, "-X has no effect when -C or --ccode is set");
			}
			context.compile_only = compile_only;
			context.header_filename = header_filename;
			if (header_filename == null && use_header) {
				Report.error(null, "--use-header may only be used in combination with --header");
			}
			context.use_header = use_header;
			context.internal_header_filename = internal_header_filename;
			context.symbols_filename = symbols_filename;
			context.includedir = includedir;
			context.output = output;
			if (output != null && ccode_only) {
				Report.warning(null, "--output and -o have no effect when -C or --ccode is set");
			}
			if (basedir == null) {
				context.basedir = Path.GetFullPath(".");
			} else {
				context.basedir = Path.GetFullPath(basedir);
			}
			if (directory != null) {
				context.directory = Path.GetFullPath(directory);
			} else {
				context.directory = context.basedir;
			}
			context.vapi_directories = vapi_directories;
			context.vapi_comments = vapi_comments;
			context.gir_directories = gir_directories;
			context.metadata_directories = metadata_directories;
			context.debug = debug;
			context.mem_profiler = mem_profiler;
			context.save_temps = save_temps;
			if (ccode_only && save_temps) {
				Report.warning(null, "--save-temps has no effect when -C or --ccode is set");
			}
			if (profile == "gobject-2.0" || profile == "gobject" || profile == null) {
				// default profile
				context.profile = Profile.GOBJECT;
				context.add_define("GOBJECT");
			} else {
				Report.error(null, "Unknown profile %s".printf(profile));
			}
			nostdpkg |= fast_vapi_filename != null;
			context.nostdpkg = nostdpkg;

			context.entry_point_name = entry_point;

			context.run_output = run_output;

			if (defines != null) {
				foreach (string define in defines) {
					context.add_define(define);
				}
			}

			for (int i = 2; i <= 38; i += 2) {
				context.add_define("VALA_0_%d".printf(i));
			}

			int glib_major = 2;
			int glib_minor = 40;

			if (target_glib != null) {
				try {
					glib_major = int.Parse(target_glib);
					string _target_glib = target_glib.Substring(target_glib.IndexOf('.'));
					glib_minor = int.Parse(_target_glib);
				} catch (Exception) {
					Report.error(null, "Invalid format for --target-glib");
				}
			}

			context.target_glib_major = glib_major;
			context.target_glib_minor = glib_minor;
			if (context.target_glib_major != 2) {
				Report.error(null, "This version of valac only supports GLib 2");
			}

			for (int i = 16; i <= glib_minor; i += 2) {
				context.add_define("GLIB_2_%d".printf(i));
			}

			if (!nostdpkg) {
				/* default packages */
				context.add_external_package("glib-2.0");
				context.add_external_package("gobject-2.0");
			}

			if (packages != null) {
				foreach (string package in packages) {
					context.add_external_package(package);
				}
				packages = null;
			}

			if (fast_vapis != null) {
				foreach (string vapi in fast_vapis) {
					var rpath = CodeContext.realpath(vapi);
					var source_file = new SourceFile(context, SourceFileType.FAST, rpath);
					context.add_source_file(source_file);
				}
				context.use_fast_vapi = true;
			}

			context.gresources = gresources;
			context.gresources_directories = gresources_directories;

			if (context.report.get_errors() > 0 || (fatal_warnings && context.report.get_warnings() > 0)) {
				return quit();
			}

			context.codegen = new GDBusServerModule();

			bool has_c_files = false;
			bool has_h_files = false;

			foreach (string source in sources) {
				if (context.add_source_filename(source, run_output, true)) {
					if (source.EndsWith(".c")) {
						has_c_files = true;
					} else if (source.EndsWith(".h")) {
						has_h_files = true;
					}
				}
			}
			sources = null;
			if (ccode_only && (has_c_files || has_h_files)) {
				Report.warning(null, "C header and source files are ignored when -C or --ccode is set");
			}

			if (context.report.get_errors() > 0 || (fatal_warnings && context.report.get_warnings() > 0)) {
				return quit();
			}

			var parser = new Parser();
			parser.parse(context);

#if false
			var genie_parser = new Genie.Parser();
			genie_parser.parse(context);

			var gir_parser = new GirParser();
			gir_parser.parse(context);
#endif

			if (context.report.get_errors() > 0 || (fatal_warnings && context.report.get_warnings() > 0)) {
				return quit();
			}

			if (fast_vapi_filename != null) {
				var interface_writer = new CodeWriter(CodeWriterType.FAST);
				interface_writer.write_file(context, fast_vapi_filename);
				return quit();
			}

			context.check();

			if (context.report.get_errors() > 0 || (fatal_warnings && context.report.get_warnings() > 0)) {
				return quit();
			}

			if (!ccode_only && !compile_only && library == null) {
				// building program, require entry point
				if (!has_c_files && context.entry_point == null) {
					Report.error(null, "program does not contain a static `main' method");
				}
			}

			if (dump_tree != null) {
				var code_writer = new CodeWriter(CodeWriterType.DUMP);
				code_writer.write_file(context, dump_tree);
			}

			if (context.report.get_errors() > 0 || (fatal_warnings && context.report.get_warnings() > 0)) {
				return quit();
			}

			context.codegen.emit(context);

			if (context.report.get_errors() > 0 || (fatal_warnings && context.report.get_warnings() > 0)) {
				return quit();
			}

			if (vapi_filename == null && library != null) {
				// keep backward compatibility with --library option
				vapi_filename = "%s.vapi".printf(library);
			}

			if (library != null) {
				if (gir != null) {
					string gir_base = Path.GetFileName(gir);
					long gir_len = gir_base.Length;
					int last_hyphen = gir_base.LastIndexOf('-');

					if (last_hyphen == -1 || !gir_base.EndsWith(".gir")) {
						Report.error(null, "GIR file name `%s' is not well-formed, expected NAME-VERSION.gir".printf(gir));
					} else {
						string gir_namespace = gir_base.Substring(0, last_hyphen);
						string gir_version = gir_base.Substring(last_hyphen + 1, (int)(gir_len - last_hyphen - 5));
						gir_version.canon("0123456789.", '?');
						if (gir_namespace == "" || gir_version == "" || !Char.IsDigit(gir_version[0]) || gir_version.Contains("?")) {
							Report.error(null, "GIR file name `%s' is not well-formed, expected NAME-VERSION.gir".printf(gir));
						} else {
#if false
							var gir_writer = new GIRWriter();

							// put .gir file in current directory unless -d has been explicitly specified
							string gir_directory = ".";
							if (directory != null) {
								gir_directory = context.directory;
							}

							gir_writer.write_file(context, gir_directory, gir, gir_namespace, gir_version, library, shared_library);
#endif
						}
					}

					gir = null;
				}

				library = null;
			}

			// The GIRWriter places the gir_namespace and gir_version into the top namespace, so write the vapi after that stage
			if (vapi_filename != null) {
				var interface_writer = new CodeWriter();

				// put .vapi file in current directory unless -d has been explicitly specified
				if (directory != null && !GPath.is_absolute(vapi_filename)) {
					vapi_filename = "%s%c%s".printf(context.directory, Path.DirectorySeparatorChar, vapi_filename);
				}

				interface_writer.write_file(context, vapi_filename);
			}

			if (internal_vapi_filename != null) {
				if (internal_header_filename == null ||
					header_filename == null) {
					Report.error(null, "--internal-vapi may only be used in combination with --header and --internal-header");
					return quit();
				}

				var interface_writer = new CodeWriter(CodeWriterType.INTERNAL);
				interface_writer.set_cheader_override(header_filename, internal_header_filename);
				string vapi_filename = internal_vapi_filename;

				// put .vapi file in current directory unless -d has been explicitly specified
				if (directory != null && !GPath.is_absolute(vapi_filename)) {
					vapi_filename = "%s%c%s".printf(context.directory, Path.DirectorySeparatorChar, vapi_filename);
				}

				interface_writer.write_file(context, vapi_filename);

				internal_vapi_filename = null;
			}

			if (dependencies != null) {
				context.write_dependencies(dependencies);
			}

			if (context.report.get_errors() > 0 || (fatal_warnings && context.report.get_warnings() > 0)) {
				return quit();
			}

			if (!ccode_only) {
				var ccompiler = new CCodeCompiler();
				if (cc_command == null && Environment.GetEnvironmentVariable("CC") != null) {
					cc_command = Environment.GetEnvironmentVariable("CC");
				}
				if (pkg_config_command == null && Environment.GetEnvironmentVariable("PKG_CONFIG") != null) {
					pkg_config_command = Environment.GetEnvironmentVariable("PKG_CONFIG");
				}
				if (cc_options == null) {
					ccompiler.compile(context, cc_command, new string[] { }, pkg_config_command);
				} else {
					ccompiler.compile(context, cc_command, cc_options, pkg_config_command);
				}
			}

			return quit();
		}

		static int run_source(string[] args) {
			int i = 1;
			if (args[i] != null && args[i].StartsWith("-")) {
				try {
					string[] compile_args = new string[args.Length + 1];
					compile_args[0] = "valac";
					args.CopyTo(compile_args, 1);

					//Shell.parse_argv("valac " + args[1], out compile_args);

					var opt_context = new OptionContext("- Vala");
					opt_context.set_help_enabled(true);
					opt_context.add_main_entries(options, null);
					string[] temp_args = compile_args;
					opt_context.parse(ref temp_args);
				} catch (Exception e) {
					stdout.printf("%s\n", e.Message);
					return 1;
				}/* catch (OptionError e) {
					stdout.printf("%s\n", e.Message);
					stdout.printf("Run '%s --help' to see a full list of available command line options.\n", args[0]);
					return 1;
				}*/

				i++;
			}

			if (version) {
				stdout.printf("Vala %s\n", Config.BUILD_VERSION);
				return 0;
			} else if (api_version) {
				stdout.printf("%s\n", Config.API_VERSION);
				return 0;
			}

			if (args[i] == null) {
				stderr.printf("No source file specified.\n");
				return 1;
			}

			sources = new string[]{ args[i] };
			output = "%s/%s.XXXXXX".printf(Path.GetTempPath(), Path.GetFileName(args[i]));

			/*int outputfd = FileUtils.mkstemp(output);
			if (outputfd < 0) {
				return 1;
			}*/

			run_output = true;
			disable_warnings = true;
			quiet_mode = true;

			var compiler = new Compiler();
			int ret = compiler.run();
			if (ret != 0) {
				return ret;
			}

			//FileUtils.close(outputfd);
			/*if (FileUtils.chmod(output, 0700) != 0) {
				FileUtils.unlink(output);
				return 1;
			}*/

			List<string> target_args = new List<string>();
			while (i < args.Length) {
				target_args.Add(args[i]);
				i++;
			}

			try {
				int pid;
				//var loop = new MainLoop();
				int child_status = 0;

				Process.Start(new ProcessStartInfo
				{
					FileName = output,
					Arguments = string.Join(" ", target_args.ToArray()),
					UseShellExecute = true
				});

				System.IO.File.Delete(output);
				
				/*ChildWatch.add(pid, (pid, status) => {
					child_status = (status & 0xff00) >> 8;
					loop.quit();
				})*/

				//loop.run();

				return child_status;
			} catch (Exception e) {
				stdout.printf("%s\n", e.Message);
				return 1;
			}
		}

		static int Main(string[] args) {
			// initialize locale
			//Intl.setlocale(LocaleCategory.ALL, "");

#if false
			if (Path.GetFileName(args[0]) == "vala" || Path.GetFileName(args[0]) == "vala" + Config.PACKAGE_SUFFIX) {
				return run_source(args);
			}
#endif
			//SMX
			string[] new_args = new string[args.Length + 1];
			args.CopyTo(new_args, 1);
			new_args[0] = "valac";
			return run_source(new_args);

			try {
				var opt_context = new OptionContext("- Vala Compiler");
				opt_context.set_help_enabled(true);
				opt_context.add_main_entries(options, null);
				opt_context.parse(ref args);
			} catch (OptionError e) {
				stdout.printf("%s\n", e.Message);
				stdout.printf("Run '%s --help' to see a full list of available command line options.\n", args[0]);
				return 1;
			}

			if (version) {
				stdout.printf("Vala %s\n", Config.BUILD_VERSION);
				return 0;
			} else if (api_version) {
				stdout.printf("%s\n", Config.API_VERSION);
				return 0;
			}

			if (sources == null && fast_vapis == null) {
				stderr.printf("No source file specified.\n");
				return 1;
			}

			var compiler = new Compiler();
			return compiler.run();
		}
	}
}
