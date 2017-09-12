using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLibPorts
{
	public partial class GLib
	{
		public class OptionError : Exception
		{
		}

		public class OptionError_Failed : Exception
		{
			private string val;

			public OptionError_Failed(string message, string val) : base(message) {
				this.val = val;
			}
		}
	}
}
