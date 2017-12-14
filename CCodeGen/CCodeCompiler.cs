using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala;
using Vala.Lang;
using Vala.Lang.CodeNodes;
using Vala.Lang.Parser;
using static GLibPorts.GLib;

namespace CCodeGen {

	/// <summary>
	/// Interface to the C compiler.
	/// </summary>
	public class CCodeCompiler {
		public string path { get; set; }

		public CCodeCompiler() {
		}

		static bool package_exists(string package_name, string pkg_config_command = "pkg-config") {
			string pc = " --exists " + package_name;
			int exit_status;

			try {
				using (Process pkg_config = Process.Start(new ProcessStartInfo {
					UseShellExecute = false,
					FileName = pkg_config_command,
					Arguments = pc,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					WorkingDirectory = Path.GetDirectoryName(pkg_config_command)
				})) {
					pkg_config.WaitForExit();
					exit_status = pkg_config.ExitCode;
				}
				return (0 == exit_status);
			} catch (Exception e) {
				Report.error(null, e.Message);
				return false;
			}
		}

		/// <summary>
		/// Compile generated C code to object code and optionally link object
		/// files.
		/// 
		/// <param name="context">a code context</param>
		/// </summary>
		public void compile(CodeContext context, string cc_command, string[] cc_options, string pkg_config_command = null) {
			bool use_pkgconfig = false;

			if (pkg_config_command == null) {
				pkg_config_command = path + "pkg-config" + GProcess.get_executable_suffix();
			}

			string pc = " --cflags";
			if (!context.compile_only) {
				pc += " --libs";
			}
			use_pkgconfig = true;
			pc += " gobject-2.0";
			foreach (string pkg in context.get_packages()) {
				if (package_exists(pkg, pkg_config_command)) {
					use_pkgconfig = true;
					pc += " " + pkg;
				}
			}
			string pkgflags = "";
			if (use_pkgconfig) {
				try {
					int exit_status;
					using (Process pkg_config = Process.Start(new ProcessStartInfo {
						UseShellExecute = false,
						FileName = pkg_config_command,
						Arguments = pc,
						WorkingDirectory = path,
						RedirectStandardOutput = true,
						RedirectStandardError = true
					})) {
						pkgflags = pkg_config.StandardOutput.ReadToEnd();
						pkg_config.WaitForExit();
						exit_status = pkg_config.ExitCode;
					}
					if (exit_status != 0) {
						Report.error(null, "pkg-config exited with status %d".printf(exit_status));
						return;
					}
				} catch (Exception e) {
					Report.error(null, e.Message);
					return;
				}
			}

			// TODO compile the C code files in parallel

			if (cc_command == null) {
				cc_command = context.path + "cc" + GProcess.get_executable_suffix();
			}
			string cmdline = "";
			if (context.debug) {
				cmdline += " -g";
			}
			if (context.compile_only) {
				cmdline += " -c";
			} else if (context.output != null) {
				string output = context.output;
				if (context.directory != null && context.directory != "" && !GPath.is_absolute(context.output)) {
					output = "%s%c%s".printf(context.directory, Path.DirectorySeparatorChar, context.output);
				}
				cmdline += " -o " + Shell.quote(output);
			}

			/* we're only interested in non-pkg source files */
			var source_files = context.get_source_files();
			foreach (SourceFile file in source_files) {
				if (file.file_type == SourceFileType.SOURCE) {
					cmdline += " " + Shell.quote(file.get_csource_filename());
				}
			}
			var c_source_files = context.get_c_source_files();
			foreach (string file in c_source_files) {
				cmdline += " " + Shell.quote(file);
			}

			// add libraries after source files to fix linking
			// with --as-needed and on Windows
			cmdline += " " + pkgflags.Trim();
			foreach (string cc_option in cc_options) {
				cmdline += " " + Shell.quote(cc_option);
			}

			if (context.verbose_mode) {
				stdout.printf("%s\n", cmdline);
			}

			try {
				int exit_status;
				using (Process cc = Process.Start(new ProcessStartInfo {
					UseShellExecute = false,
					FileName = cc_command,
					Arguments = cmdline,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					WorkingDirectory = path
				})) {
					//stdout.puts(cc.StandardOutput.ReadToEnd());
					stderr.puts(cc.StandardError.ReadToEnd());
					cc.WaitForExit();
					exit_status = cc.ExitCode;
				}
				if (exit_status != 0) {
					Report.error(null, "cc exited with status %d".printf(exit_status));
				}
			} catch (Exception e) {
				Report.error(null, e.Message);
			}

			/* remove generated C source and header files */
			foreach (SourceFile file in source_files) {
				if (file.file_type == SourceFileType.SOURCE) {
					if (!context.save_csources) {
						File.Delete(file.get_csource_filename());
					}
				}
			}
		}
	}
}
