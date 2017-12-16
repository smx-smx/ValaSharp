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
using System.Runtime.CompilerServices;
using System.Threading;
using Vala;
using Vala.Lang.Parser;
using CCodeGen;
using CCodeGen.Modules;
using GLibPorts;
using Vala.Lang.Code;
using ValaConfig;

using static GLibPorts.GLib;
using Parser = Vala.Lang.Parser.Parser;
using ValaCompilerLib;

namespace ValaCompiler {
	public class Program {
		private const string DEFAULT_COLORS = "error=01;31:warning=01;35:note=01;36:caret=01;32:locus=01:quote=01";

		private CodeContext context;

		static int Main(string[] args) {
			GLibInitialize();

			CompilerOptions opts = new CompilerOptions();
			opts.parse_args(args);

			if (opts.cc_options != null) {
				for (int i = 0; i < opts.cc_options.Count; i++) {
					string cc_opt = opts.cc_options[i];
					if (cc_opt.Length > 0 && cc_opt[0] == '\'') {
						cc_opt = cc_opt.Substring(1, cc_opt.Length - 2).Trim();
						opts.cc_options[i] = cc_opt;
					}
				}
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

			//int result = run_source(opts);
			//return result;
			var compiler = new Compiler(opts);
			int result = compiler.run();

			GLibDispose();
			return result;
		}
	}
}
