using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLanguage {
	/**
 * Represents a variable or function pointer declarator in the C code.
 */
	public abstract class CCodeDeclarator : CCodeNode {
		/**
		 * Writes initialization statements for this declarator with the
		 * specified C code writer if necessary.
		 *
		 * @param writer a C code writer
		 */
		public virtual void write_initialization(CCodeWriter writer) {
		}
	}

}
