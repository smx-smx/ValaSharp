using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLanguage {
	/// <summary>
	/// Represents a container for C code nodes.
	/// </summary>
	public class CCodeFragment : CCodeNode {
		private List<CCodeNode> children = new List<CCodeNode>();

		/// <summary>
		/// Appends the specified code node to this code fragment.
		/// 
		/// <param name="node">a C code node</param>
		/// </summary>
		public void append(CCodeNode node) {
			children.Add(node);
		}

		/// <summary>
		/// Returns a copy of the list of children.
		/// 
		/// <returns>children list</returns>
		/// </summary>
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
