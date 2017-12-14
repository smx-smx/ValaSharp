using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLanguage.Expressions {
	/// <summary>
	/// Represents an expression node in the C code tree.
	/// </summary>
	public abstract class CCodeExpression : CCodeNode {
		public virtual void write_inner(CCodeWriter writer) {
			this.write(writer);
		}
	}
}
