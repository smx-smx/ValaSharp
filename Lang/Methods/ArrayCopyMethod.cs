using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.Parser;
using Vala.Lang.Types;

namespace Vala.Lang.Methods {
	/// <summary>
	/// Represents the Array.copy method.
	/// </summary>
	public class ArrayCopyMethod : Method {
		/// <summary>
		/// Creates a new array copy method.
		/// 
		/// <returns>newly created method</returns>
		/// </summary>
		public ArrayCopyMethod(SourceReference source_reference)
			: base("copy", new InvalidType(), source_reference) {
			external = true;
		}
	}
}
