using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ValaLanguageServer {
	class Program {
		static void Main(string[] args) {
			Console.OutputEncoding = Encoding.UTF8;
			var app = new App(Console.OpenStandardInput(), Console.OpenStandardOutput());
			try {
				app.Listen().Wait();
			} catch (AggregateException ex) {
				Console.Error.WriteLine(ex.InnerExceptions[0]);
				Environment.Exit(1);
			}
		}
	}
}
