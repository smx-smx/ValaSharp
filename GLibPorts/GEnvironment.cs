using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLibPorts {
	public class GEnvironment {
		public static string[] get_system_data_dirs() {
			return new string[] {
				Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
				Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
			};
		}
	}
}
