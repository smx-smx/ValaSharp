using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLanguage {
	/// <summary>
	/// Represents a node in the C code tree.
	/// </summary>
	public abstract class CCodeNode {
		/// <summary>
		/// The source file name and line number to be presumed for this code
		/// node.
		/// </summary>
		public CCodeLineDirective line { get; set; }

		/// <summary>
		/// The modifiers for this code node which will be handled as needed
		/// in every subclass.
		/// </summary>
		public CCodeModifiers modifiers { get; set; }

		/// <summary>
		/// Writes this code node and all children with the specified C code
		/// writer.
		/// 
		/// <param name="writer">a C code writer</param>
		/// </summary>
		public abstract void write(CCodeWriter writer);

		/// <summary>
		/// Writes declaration for this code node with the specified C code
		/// writer if necessary.
		/// 
		/// <param name="writer">a C code writer</param>
		/// </summary>
		public virtual void write_declaration(CCodeWriter writer) {
		}

		/// <summary>
		/// Writes declaration and implementation combined for this code node and
		/// all children with the specified C code writer.
		/// 
		/// <param name="writer">a C code writer</param>
		/// </summary>
		public virtual void write_combined(CCodeWriter writer) {
			write_declaration(writer);
			write(writer);
		}
	}
}
