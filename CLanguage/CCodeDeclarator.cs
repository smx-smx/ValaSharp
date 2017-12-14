using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLanguage {
	/// <summary>
	/// Represents a variable or function pointer declarator in the C code.
	/// </summary>
	public abstract class CCodeDeclarator : CCodeNode {
		/// <summary>
		/// Writes initialization statements for this declarator with the
		/// specified C code writer if necessary.
		/// 
		/// <param name="writer">a C code writer</param>
		/// </summary>
		public virtual void write_initialization(CCodeWriter writer) {
		}
	}

}
