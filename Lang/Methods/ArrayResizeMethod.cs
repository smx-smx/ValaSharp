using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.Parser;
using Vala.Lang.Types;

namespace Vala.Lang.Methods {
	/**
	 * Represents the Array.resize method.
	 */
	public class ArrayResizeMethod : Method {
		/**
		 * Creates a new array resize method.
		 *
		 * @return newly created method
		 */
		public ArrayResizeMethod(SourceReference source_reference)
			: base("resize", new VoidType(), source_reference) {
			external = true;
			set_attribute_double("CCode", "instance_pos", 0.1);
		}
	}

}
