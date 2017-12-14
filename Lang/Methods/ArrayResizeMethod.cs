using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.Parser;
using Vala.Lang.Types;

namespace Vala.Lang.Methods {
	/// <summary>
	/// Represents the Array.resize method.
	/// </summary>
	public class ArrayResizeMethod : Method {
		/// <summary>
		/// Creates a new array resize method.
		/// 
		/// <returns>newly created method</returns>
		/// </summary>
		public ArrayResizeMethod(SourceReference source_reference)
			: base("resize", new VoidType(), source_reference) {
			external = true;
			set_attribute_double("CCode", "instance_pos", 0.1);
		}
	}

}
