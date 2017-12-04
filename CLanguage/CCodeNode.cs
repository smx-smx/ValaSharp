using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLanguage {
	/**
	 * Represents a node in the C code tree.
	 */
	public abstract class CCodeNode {
		/**
		 * The source file name and line number to be presumed for this code
		 * node.
		 */
		public CCodeLineDirective line { get; set; }

		/**
		 * The modifiers for this code node which will be handled as needed
		 * in every subclass.
		 */
		public CCodeModifiers modifiers { get; set; }

		/**
		 * Writes this code node and all children with the specified C code
		 * writer.
		 *
		 * @param writer a C code writer
		 */
		public abstract void write(CCodeWriter writer);

		/**
		 * Writes declaration for this code node with the specified C code
		 * writer if necessary.
		 *
		 * @param writer a C code writer
		 */
		public virtual void write_declaration(CCodeWriter writer) {
		}

		/**
		 * Writes declaration and implementation combined for this code node and
		 * all children with the specified C code writer.
		 *
		 * @param writer a C code writer
		 */
		public virtual void write_combined(CCodeWriter writer) {
			write_declaration(writer);
			write(writer);
		}
	}
}
