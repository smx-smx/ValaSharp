using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLibPorts
{
	public partial class GLib
	{
		public class GPath
		{
			public static bool is_absolute(string file_name) {
				if (file_name == null || file_name.Length < 1)
					return false;

				if (file_name[0] == System.IO.Path.DirectorySeparatorChar)
					return true;

#if !UNIX
				if (Char.IsLetter(file_name[0]) &&
					file_name[1] == ':' && file_name[3] == System.IO.Path.DirectorySeparatorChar)
					return true;
#endif
				return false;
			}

			public static string build_path(string separator, params string[] parts) {
				return string.Join(separator, parts);
			}

			public static bool is_dir_separator(char ch) {
				return ch == Path.DirectorySeparatorChar || ch == Path.AltDirectorySeparatorChar;
			}
		}
	}
}
