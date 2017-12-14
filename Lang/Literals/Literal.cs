using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.Expressions;

namespace Vala.Lang.Literals {
	/// <summary>
	/// Base class for all literals in the source code.
	/// </summary>
	public abstract class Literal : Expression {
		public override bool is_constant() {
			return true;
		}

		public override bool is_pure() {
			return true;
		}
	}
}
