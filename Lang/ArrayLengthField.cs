using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.Parser;
using Vala.Lang.Types;

namespace Vala.Lang {
	/// <summary>
	/// Represents the Array.length field.
	/// </summary>
	public class ArrayLengthField : Field {
		/// <summary>
		/// Creates a new array length field.
		/// 
		/// <returns>newly created field</returns>
		/// </summary>
		public ArrayLengthField(SourceReference source_reference) : base("length", new InvalidType(), null, source_reference) {
			external = true;
		}
	}
}
