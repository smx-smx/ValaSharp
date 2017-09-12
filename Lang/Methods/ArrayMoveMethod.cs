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
	 * Represents the Array.move method.
	 */
	public class ArrayMoveMethod : Method
	{
		/**
		 * Creates a new array move method.
		 *
		 * @return newly created method
		 */
		public ArrayMoveMethod(SourceReference source_reference)
			: base("move", new VoidType(), source_reference)
		{
			external = true;
		}
	}

}
