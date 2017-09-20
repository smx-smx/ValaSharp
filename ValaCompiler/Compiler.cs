﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang;
using Vala.Lang.CodeNodes;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using Vala;
using Vala.Lang.Parser;
using CCodeGen;
using CCodeGen.Modules;
using CommandLine.Text;
using Vala.Lang.Code;
using ValaConfig;

using static GLibPorts.GLib;
using Parser = Vala.Lang.Parser.Parser;

namespace ValaCompiler
{
	class Compiler
	{
		private const string DEFAULT_COLORS = "error=01;31:warning=01;35:note=01;36:caret=01;32:locus=01:quote=01";

		static bool run_output;
		private CodeContext context;
		private CompilerOptions opts;

		public Compiler(CompilerOptions opts) {
			this.opts = opts;
		}

		public Compiler() {
		}

		/*
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
		*/

		private int quit() {
			if (context.report.get_errors() == 0 && context.report.get_warnings() == 0) {
				return 0;
			}
			if (context.report.get_errors() == 0 && (!opts.fatal_warnings || context.report.get_warnings() == 0)) {
				if (!opts.quiet_mode) {
					stdout.printf("Compilation succeeded - %d warning(s)\n", context.report.get_warnings());
				}
				return 0;
			} else {
				if (!opts.quiet_mode) {
					stdout.printf("Compilation failed: %d error(s), %d warning(s)\n", context.report.get_errors(), context.report.get_warnings());
				}
				return 1;
			}
		}

		private int run() {
			context = new CodeContext();
			CodeContext.push(context);

			if (opts.disable_colored_output) {
				opts.colored_output = Report.Colored.NEVER;
			}

			if (opts.colored_output != Report.Colored.NEVER) {
				string env_colors = Environment.GetEnvironmentVariable("VALA_COLORS");
				if (env_colors != null) {
					context.report.set_colors(env_colors, opts.colored_output);
				} else {
					context.report.set_colors(DEFAULT_COLORS, opts.colored_output);
				}
			}


			// default to build executable
			if (!opts.ccode_only && !opts.compile_only && opts.output == null) {
				// strip extension if there is one
				// else we use the default output file of the C compiler
				if (opts.sources[0].LastIndexOf('.') != -1) {
					int dot = opts.sources[0].LastIndexOf('.');
					opts.output = Path.GetFileName(opts.sources[0].Substring(0, dot));
				}
			}

			context.assert = !opts.disable_assert;
			context.checking = opts.enable_checking;
			context.deprecated = opts.deprecated;
			context.since_check = !opts.disable_since_check;
			context.hide_internal = opts.hide_internal;
			context.experimental = opts.experimental;
			context.experimental_non_null = opts.experimental_non_null;
			context.gobject_tracing = opts.gobject_tracing;
			context.report.enable_warnings = !opts.disable_warnings;
			context.report.set_verbose_errors(!opts.quiet_mode);
			context.verbose_mode = opts.verbose_mode;
			context.version_header = !opts.disable_version_header;

			context.path = opts.path;
			context.ccode_only = opts.ccode_only;
			if (opts.ccode_only && opts.cc_options != null) {
				Report.warning(null, "-X has no effect when -C or --ccode is set");
			}
			context.compile_only = opts.compile_only;
			context.header_filename = opts.header_filename;
			if (opts.header_filename == null && opts.use_header) {
				Report.error(null, "--use-header may only be used in combination with --header");
			}
			context.use_header = opts.use_header;
			context.internal_header_filename = opts.internal_header_filename;
			context.symbols_filename = opts.symbols_filename;
			context.includedir = opts.includedir;
			context.output = opts.output;
			if (opts.output != null && opts.ccode_only) {
				Report.warning(null, "--output and -o have no effect when -C or --ccode is set");
			}
			if (opts.basedir == null) {
				context.basedir = Path.GetFullPath(".");
			} else {
				context.basedir = Path.GetFullPath(opts.basedir);
			}
			if (opts.directory != null) {
				context.directory = Path.GetFullPath(opts.directory);
			} else {
				context.directory = context.basedir;
			}
			context.vapi_directories = opts.vapi_directories?.ToArray();
			context.vapi_comments = opts.vapi_comments;
			context.gir_directories = opts.gir_directories?.ToArray();
			context.metadata_directories = opts.metadata_directories?.ToArray();
			context.debug = opts.debug;
			context.mem_profiler = opts.mem_profiler;
			context.save_temps = opts.save_temps;
			if (opts.ccode_only && opts.save_temps) {
				Report.warning(null, "--save-temps has no effect when -C or --ccode is set");
			}
			if (opts.profile == "gobject-2.0" || opts.profile == "gobject" || opts.profile == null) {
				// default profile
				context.profile = Profile.GOBJECT;
				context.add_define("GOBJECT");
			} else {
				Report.error(null, "Unknown profile %s".printf(opts.profile));
			}
			opts.nostdpkg |= opts.fast_vapi_filename != null;
			context.nostdpkg = opts.nostdpkg;

			context.entry_point_name = opts.entry_point;

			context.run_output = run_output;

			if (opts.defines != null) {
				foreach (string define in opts.defines) {
					context.add_define(define);
				}
			}

			for (int i = 2; i <= 38; i += 2) {
				context.add_define("VALA_0_%d".printf(i));
			}

			int glib_major = 2;
			int glib_minor = 40;

			if (opts.target_glib != null) {
				try {
					glib_major = int.Parse(opts.target_glib);
					string _target_glib = opts.target_glib.Substring(opts.target_glib.IndexOf('.'));
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

			if (!opts.nostdpkg) {
				/* default packages */
				context.add_external_package("glib-2.0");
				context.add_external_package("gobject-2.0");
			}

			if (opts.packages != null) {
				foreach (string package in opts.packages) {
					context.add_external_package(package);
				}
				opts.packages = null;
			}

			if (opts.fast_vapis != null) {
				foreach (string vapi in opts.fast_vapis) {
					var rpath = CodeContext.realpath(vapi);
					var source_file = new SourceFile(context, SourceFileType.FAST, rpath);
					context.add_source_file(source_file);
				}
				context.use_fast_vapi = true;
			}

			context.gresources = opts.gresources?.ToArray();
			context.gresources_directories = opts.gresources_directories?.ToArray();

			if (context.report.get_errors() > 0 || (opts.fatal_warnings && context.report.get_warnings() > 0)) {
				return quit();
			}

			context.codegen = new GDBusServerModule();

			bool has_c_files = false;
			bool has_h_files = false;

			foreach (string source in opts.sources) {
				if (context.add_source_filename(source, run_output, true)) {
					if (source.EndsWith(".c")) {
						has_c_files = true;
					} else if (source.EndsWith(".h")) {
						has_h_files = true;
					}
				}
			}
			opts.sources = null;
			if (opts.ccode_only && (has_c_files || has_h_files)) {
				Report.warning(null, "C header and source files are ignored when -C or --ccode is set");
			}

			if (context.report.get_errors() > 0 || (opts.fatal_warnings && context.report.get_warnings() > 0)) {
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

			if (context.report.get_errors() > 0 || (opts.fatal_warnings && context.report.get_warnings() > 0)) {
				return quit();
			}

			if (opts.fast_vapi_filename != null) {
				var interface_writer = new CodeWriter(CodeWriterType.FAST);
				interface_writer.write_file(context, opts.fast_vapi_filename);
				return quit();
			}

			context.check();

			if (context.report.get_errors() > 0 || (opts.fatal_warnings && context.report.get_warnings() > 0)) {
				return quit();
			}

			if (!opts.ccode_only && !opts.compile_only && opts.library == null) {
				// building program, require entry point
				if (!has_c_files && context.entry_point == null) {
					Report.error(null, "program does not contain a static `main' method");
				}
			}

			if (opts.dump_tree != null) {
				var code_writer = new CodeWriter(CodeWriterType.DUMP);
				code_writer.write_file(context, opts.dump_tree);
			}

			if (context.report.get_errors() > 0 || (opts.fatal_warnings && context.report.get_warnings() > 0)) {
				return quit();
			}

			context.codegen.emit(context);

			if (context.report.get_errors() > 0 || (opts.fatal_warnings && context.report.get_warnings() > 0)) {
				return quit();
			}

			if (opts.vapi_filename == null && opts.library != null) {
				// keep backward compatibility with --library option
				opts.vapi_filename = "%s.vapi".printf(opts.library);
			}

			if (opts.library != null) {
				if (opts.gir != null) {
					string gir_base = Path.GetFileName(opts.gir);
					long gir_len = gir_base.Length;
					int last_hyphen = gir_base.LastIndexOf('-');

					if (last_hyphen == -1 || !gir_base.EndsWith(".gir")) {
						Report.error(null, "GIR file name `%s' is not well-formed, expected NAME-VERSION.gir".printf(opts.gir));
					} else {
						string gir_namespace = gir_base.Substring(0, last_hyphen);
						string gir_version = gir_base.Substring(last_hyphen + 1, (int)(gir_len - last_hyphen - 5));
						gir_version.canon("0123456789.", '?');
						if (gir_namespace == "" || gir_version == "" || !Char.IsDigit(gir_version[0]) || gir_version.Contains("?")) {
							Report.error(null, "GIR file name `%s' is not well-formed, expected NAME-VERSION.gir".printf(opts.gir));
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

					opts.gir = null;
				}

				opts.library = null;
			}

			// The GIRWriter places the gir_namespace and gir_version into the top namespace, so write the vapi after that stage
			if (opts.vapi_filename != null) {
				var interface_writer = new CodeWriter();

				// put .vapi file in current directory unless -d has been explicitly specified
				if (opts.directory != null && !GPath.is_absolute(opts.vapi_filename)) {
					opts.vapi_filename = "%s%c%s".printf(context.directory, Path.DirectorySeparatorChar, opts.vapi_filename);
				}

				interface_writer.write_file(context, opts.vapi_filename);
			}

			if (opts.internal_vapi_filename != null) {
				if (opts.internal_header_filename == null ||
				    opts.header_filename == null) {
					Report.error(null, "--internal-vapi may only be used in combination with --header and --internal-header");
					return quit();
				}

				var interface_writer = new CodeWriter(CodeWriterType.INTERNAL);
				interface_writer.set_cheader_override(opts.header_filename, opts.internal_header_filename);
				string vapi_filename = opts.internal_vapi_filename;

				// put .vapi file in current directory unless -d has been explicitly specified
				if (opts.directory != null && !GPath.is_absolute(vapi_filename)) {
					vapi_filename = "%s%c%s".printf(context.directory, Path.DirectorySeparatorChar, vapi_filename);
				}

				interface_writer.write_file(context, vapi_filename);

				opts.internal_vapi_filename = null;
			}

			if (opts.dependencies != null) {
				context.write_dependencies(opts.dependencies);
			}

			if (context.report.get_errors() > 0 || (opts.fatal_warnings && context.report.get_warnings() > 0)) {
				return quit();
			}

			if (!opts.ccode_only) {
				var ccompiler = new CCodeCompiler();
				if (opts.cc_command == null && Environment.GetEnvironmentVariable("CC") != null) {
					opts.cc_command = Environment.GetEnvironmentVariable("CC");
				}
				if (opts.pkg_config_command == null && Environment.GetEnvironmentVariable("PKG_CONFIG") != null) {
					opts.pkg_config_command = Environment.GetEnvironmentVariable("PKG_CONFIG");
				}
				if (opts.cc_options == null) {
					ccompiler.compile(context, opts.cc_command, new string[] { }, opts.pkg_config_command);
				} else {
					ccompiler.compile(context, opts.cc_command, opts.cc_options.ToArray(), opts.pkg_config_command);
				}
			}

			return quit();
		}

		static int run_source(CompilerOptions opts) {
#if false
			int i = 1;
			if (args[i] != null && args[i].StartsWith("-")) {
				try {
					string[] compile_args = new string[args.Length + 1];
					compile_args[0] = "valac";
					args.CopyTo(compile_args, 1);

					//Shell.parse_argv("valac " + args[1], out compile_args);

					/*var opt_context = new OptionContext("- Vala");
					opt_context.set_help_enabled(true);
					opt_context.add_main_entries(options, null);
					string[] temp_args = compile_args;
					opt_context.parse(ref temp_args);*/

					var options = new CompilerOptions();
					CommandLine.Parser.Default.ParseArgumentsStrict(args, options);

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
#endif

			if (opts.version) {
				stdout.printf("Vala %s\n", Config.BUILD_VERSION);
				return 0;
			} else if (opts.api_version) {
				stdout.printf("%s\n", Config.API_VERSION);
				return 0;
			}

			if (opts.unparsed.Count == 0) {
				stderr.printf("No source file specified.\n");
				return 1;
			}

			opts.output = "%s/%s.XXXXXX".printf(Path.GetTempPath(), Path.GetFileName(opts.unparsed[0]));

			/*int outputfd = FileUtils.mkstemp(output);
			if (outputfd < 0) {
				return 1;
			}*/

			run_output = true;
			opts.disable_warnings = true;
			opts.quiet_mode = true;

			var compiler = new Compiler(opts);
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
			/*while (i < args.Length) {
				target_args.Add(args[i]);
				i++;
			}*/

			if(opts.unparsed.Count > 1)
				target_args.AddRange(opts.unparsed.Take(1));

			try {
				int pid;
				//var loop = new MainLoop();
				int child_status = 0;

				Process.Start(new ProcessStartInfo
				{
					FileName = opts.output,
					Arguments = string.Join(" ", target_args.ToArray()),
					UseShellExecute = true
				});

				System.IO.File.Delete(opts.output);
				
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
			var opts = new CompilerOptions();
			CommandLine.Parser.Default.ParseArguments(args, opts);
			/*string[] new_args = new string[args.Length + 1];
			args.CopyTo(new_args, 1);
			new_args[0] = "valac";*/
			int result = run_source(opts);
			return result;

			try {
#if false
				var opt_context = new OptionContext("- Vala Compiler");
				opt_context.set_help_enabled(true);
				opt_context.add_main_entries(options, null);
				opt_context.parse(ref args);
#endif
			} catch (OptionError e) {
				stdout.printf("%s\n", e.Message);
				stdout.printf("Run '%s --help' to see a full list of available command line options.\n", args[0]);
				return 1;
			}

			if (opts.version) {
				stdout.printf("Vala %s\n", Config.BUILD_VERSION);
				return 0;
			} else if (opts.api_version) {
				stdout.printf("%s\n", Config.API_VERSION);
				return 0;
			}

			if (opts.sources == null && opts.fast_vapis == null) {
				stderr.printf("No source file specified.\n");
				return 1;
			}

			var compiler = new Compiler();
			return compiler.run();
		}
	}
}
