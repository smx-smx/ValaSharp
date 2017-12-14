using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.Parser;
using Vala.Lang.Types;

namespace Vala.Lang.Methods {
	/// <summary>
	/// Represents the Array.move method.
	/// </summary>
	public class ArrayMoveMethod : Method {
		/// <summary>
		/// Creates a new array move method.
		/// 
		/// <returns>newly created method</returns>
		/// </summary>
		public ArrayMoveMethod(SourceReference source_reference)
			: base("move", new VoidType(), source_reference) {
			external = true;
		}
	}

}
