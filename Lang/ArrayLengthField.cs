using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.Parser;
using Vala.Lang.Types;

namespace Vala.Lang
{
	/**
	 * Represents the Array.length field.
	 */
	public class ArrayLengthField : Field
	{
		/**
		 * Creates a new array length field.
		 *
		 * @return newly created field
		 */
		public ArrayLengthField(SourceReference source_reference) : base("length", new InvalidType(), null, source_reference) {
			external = true;
		}
	}
}
