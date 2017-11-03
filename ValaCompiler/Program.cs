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
using CommandLine;
using CommandLine.Text;
using GLibPorts;
using Vala.Lang.Code;
using ValaConfig;

using static GLibPorts.GLib;
using Parser = Vala.Lang.Parser.Parser;
using ValaCompilerLib;

namespace ValaCompiler
{
	public class Program
	{
		static int Main(string[] args) {
			Console.WriteLine("Press Enter to start work");
			Console.ReadLine();

			CompilerOptions opts = new CompilerOptions();
			var parser = new CommandLine.Parser((settings => {
				settings.CaseSensitive = true;
			}));
			if (!parser.ParseArguments(args, opts))
				return 1;

			//int result = run_source(opts);
			//return result;
			var compiler = new Compiler(opts);
			int result = compiler.run();

			compiler = null;
			opts = null;
			parser = null;

			GC.Collect();
			Console.WriteLine("Press Enter to quit");
			Console.ReadLine();

			GLibPorts.Native.Utils.GLibDispose();
			return result;
		}
	}
}
