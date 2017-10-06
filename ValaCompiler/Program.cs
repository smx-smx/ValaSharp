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
using Vala;
using Vala.Lang.Parser;
using CCodeGen;
using CCodeGen.Modules;
using CommandLine.Text;
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
			CompilerOptions opts = new CompilerOptions();
			if (!CommandLine.Parser.Default.ParseArguments(args, opts))
				return 1;

			//int result = run_source(opts);
			//return result;
			var compiler = new Compiler(opts);
			return compiler.run();
		}
	}
}
