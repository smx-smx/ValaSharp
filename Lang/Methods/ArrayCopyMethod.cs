using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.Parser;
using Vala.Lang.Types;

namespace Vala.Lang.Methods
{
	/**
	 * Represents the Array.copy method.
	 */
	public class ArrayCopyMethod : Method
	{
		/**
		 * Creates a new array copy method.
		 *
		 * @return newly created method
		 */
		public ArrayCopyMethod(SourceReference source_reference)
			: base("copy", new InvalidType(), source_reference)
		{
			external = true;
		}
	}
}
