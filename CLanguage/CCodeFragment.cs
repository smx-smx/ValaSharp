using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLanguage
{
	/**
 * Represents a container for C code nodes.
 */
	public class CCodeFragment : CCodeNode
	{
		private List<CCodeNode> children = new List<CCodeNode>();

		/**
		 * Appends the specified code node to this code fragment.
		 *
		 * @param node a C code node
		 */
		public void append(CCodeNode node) {
			children.Add(node);
		}

		/**
		 * Returns a copy of the list of children.
		 *
		 * @return children list
		 */
		public List<CCodeNode> get_children() {
			return children;
		}

		public override void write(CCodeWriter writer) {
			foreach (CCodeNode node in children) {
				node.write(writer);
			}
		}

		public override void write_declaration(CCodeWriter writer) {
			foreach (CCodeNode node in children) {
				node.write_declaration(writer);
			}
		}

		public override void write_combined(CCodeWriter writer) {
			foreach (CCodeNode node in children) {
				node.write_combined(writer);
			}
		}
	}
}
