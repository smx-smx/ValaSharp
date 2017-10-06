using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using ValaCompilerLib;

namespace ValaTests
{
	public class ValaTestRunner {
		private string baseDir;
		private string projDir;
		private string vapiDir;

		public ValaTestRunner() {
			baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			projDir = Path.GetFullPath(baseDir + "/../../");
			vapiDir = @"C:\msys64\mingw64\share\vala-0.38\vapi";
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
				vapi_directories = new List<string>{ vapiDir },
				output = outExePath,
				packages = new List<string> {
					"gio-2.0"
				}
			};
			
			Compiler compiler = new Compiler(opts);
			int result = compiler.run();
			if (File.Exists(outExePath) && new FileInfo(outExePath).Length > 0) {
				File.Delete(outExePath);
				return 0;
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
