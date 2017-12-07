using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GLibPorts;
using System.Collections.Generic;

namespace ValaTests {
	[TestClass]
	public class CommandLineParsing {
		[TestMethod]
		public void CommandLineParserTests() {
			IList<string> vapi_directories = null;
			bool foo = false;

			OptionEntry[] options = new OptionEntry[]{
				new OptionEntry<IList<string>>(
					"vapidir", 0, 0, OptionArg.FILENAME_ARRAY,
					"Look for package bindings in DIRECTORY", "DIRECTORY...",
					(value) => { vapi_directories = value; }
				),
				new OptionEntry<bool>(
					"foo", 0, 0, OptionArg.NONE,
					"Unit Test", "",
					(value) => { foo = value; }
				)
			};

			var opt_context = new OptionContext("- Vala Interpreter");
			opt_context.help_enabled = true;
			opt_context.add_main_entries(options, null);

			string[] args = {
				"--vapidir", "foo",
				"--vapidir", "bar",
				"--vapidir=baz",
				"--foo"
			};

			opt_context.parse(args);

			Assert.IsTrue(vapi_directories != null && vapi_directories.Count == 3);
			Assert.AreEqual(vapi_directories[0], "foo");
			Assert.AreEqual(vapi_directories[1], "bar");
			Assert.AreEqual(vapi_directories[2], "baz");
			Assert.IsTrue(foo);
		}
	}
}
