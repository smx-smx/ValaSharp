using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.CodeNodes;

namespace Vala.Lang {
	public class BasicBlock {
		private List<CodeNode> nodes = new List<CodeNode>();

		// control flow graph
		private List<BasicBlock> predecessors = new List<BasicBlock>();
		private List<BasicBlock> successors = new List<BasicBlock>();

		// dominator tree
		public BasicBlock parent { get; private set; }
		List<BasicBlock> children = new List<BasicBlock>();
		HashSet<BasicBlock> df = new HashSet<BasicBlock>();

		HashSet<PhiFunction> phi_functions = new HashSet<PhiFunction>();

		public bool postorder_visited { get; set; }
		public int postorder_number { get; set; }

		public BasicBlock() {
		}

		public static BasicBlock entry() {
			return new BasicBlock();
		}

		public static BasicBlock exit() {
			return new BasicBlock();
		}

		public void add_node(CodeNode node) {
			nodes.Add(node);
		}

		public List<CodeNode> get_nodes() {
			return nodes;
		}

		public void connect(BasicBlock target) {
			if (!successors.Contains(target)) {
				successors.Add(target);
			}
			if (!target.predecessors.Contains(this)) {
				target.predecessors.Add(this);
			}
		}

		public List<BasicBlock> get_predecessors() {
			return predecessors;
		}

		public List<BasicBlock> get_successors() {
			return successors;
		}

		public void add_child(BasicBlock block) {
			children.Add(block);
			block.parent = this;
		}

		public List<BasicBlock> get_children() {
			return children;
		}

		public void add_dominator_frontier(BasicBlock block) {
			df.Add(block);
		}

		public HashSet<BasicBlock> get_dominator_frontier() {
			return df;
		}

		public void add_phi_function(PhiFunction phi) {
			phi_functions.Add(phi);
		}

		public HashSet<PhiFunction> get_phi_functions() {
			return phi_functions;
		}
	}
}
