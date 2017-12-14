using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GLibPorts.GLib;

namespace CLanguage.Expressions {
	/// <summary>
	/// Represents an invalid expression.
	/// </summary>
	public class CCodeInvalidExpression : CCodeExpression {
		public CCodeInvalidExpression() {
		}

		public override void write(CCodeWriter writer) {
			assert_not_reached();
		}
	}

}
