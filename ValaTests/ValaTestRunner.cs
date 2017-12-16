using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using GLibPorts;
using NUnit.Framework;
using ValaCompilerLib;

namespace ValaTests {
	public class ValaTestRunner {
		private string baseDir;
		private string projDir;
		private string vapiDir;

		static ValaTestRunner() {
			GLib.GLibInitialize();
		}

		~ValaTestRunner() {
			GLib.GLibDispose();
		}

		public ValaTestRunner() {
			baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			projDir = Path.GetFullPath(baseDir + "/../../");
			vapiDir = Path.GetFullPath(projDir + "../vapi");
		}

		public int RunValaTest(IList<string> sources) {
			sources = sources.Select(s => Path.GetFullPath(s)).ToList();

			string outExePath = Path.GetFullPath(
				Path.GetTempPath() +
				Path.DirectorySeparatorChar +
				Path.GetFileNameWithoutExtension(sources[0])
			);

			CompilerOptions opts = new CompilerOptions {
				path = @"C:\msys64\mingw64\bin",
				basedir = baseDir,
				debug = false,
				verbose_mode = true,
				sources = sources,
				vapi_directories = new List<string> { vapiDir },
				output = outExePath,
				packages = new List<string> {
					"gio-2.0"
				},
				cc_options = new List<string>() {
					"-DGETTEXT_PACKAGE=\\\"valac\\\"",
					"-Werror=init-self",
					"-Werror=implicit",
					"-Werror=sequence-point",
					"-Werror=return-type",
					"-Werror=uninitialized",
					"-Werror=pointer-arith",
					"-Werror=int-to-pointer-cast",
					"-Werror=pointer-to-int-cast",
					"-Wformat",
					"-Werror=format-security",
					"-Werror=format-nonliteral",
					"-Werror=redundant-decls",
					"-Werror=int-conversion"
				},
				entry_point = "main",
				disable_warnings = true
			};

			int result;
			using (Compiler compiler = new Compiler(opts)) {
				result = compiler.run();
			}
			if (File.Exists(outExePath) && new FileInfo(outExePath).Length > 0) {

				ProcessStartInfo testProc = new ProcessStartInfo {
					FileName = outExePath,
					CreateNoWindow = true,
					UseShellExecute = true
				};

				int exitCode;

				using (Process proc = Process.Start(testProc)) {
					proc.WaitForExit();
					exitCode = proc.ExitCode;
				}
				File.Delete(outExePath);

				return exitCode;
			}
			return result;
		}

		public int RunValaTest(string source) {
			return RunValaTest(new List<string> {
				projDir + source,
			});
		}

	}
}
