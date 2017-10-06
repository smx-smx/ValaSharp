using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GLibPorts.Native;
using File = System.IO.File;

namespace GLibPorts
{
	public partial class GLib
	{
		public class FileUtils
		{
			public static bool get_contents(string filename, out string contents) {
				var fileStream = new System.IO.FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
				var textReader = new StreamReader(fileStream);
				contents = textReader.ReadToEnd();
				return true;
			}

			public static bool get_contents(string filename, out string contents, out ulong length) {
				get_contents(filename, out contents);
				length = (ulong)contents.Length;
				return true;
			}

			public static void close(int outputfd)
			{
				Win32._close(outputfd);
			}

			public static int mkstemp(string template, out string filename) {
				filename = Win32.mktemp(template);
				if (filename == null)
					return -1;
				return Win32._open(filename, Win32._O_RDWR | Win32._O_CREAT, 0700);
			}
		}
	}
}
