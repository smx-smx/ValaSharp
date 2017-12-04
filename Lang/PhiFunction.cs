using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.Symbols;

namespace Vala.Lang {
	public class PhiFunction {
		public Variable original_variable { get; private set; }

		public List<Variable> operands { get; private set; }

		public PhiFunction(Variable variable, int num_of_ops) {
			this.original_variable = variable;
			this.operands = new List<Variable>();
			for (int i = 0; i < num_of_ops; i++) {
				this.operands.Add((Variable)null);
			}
		}
	}
}
