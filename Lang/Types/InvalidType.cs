using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vala.Lang.Types {
	public class InvalidType : DataType {
		public InvalidType() {
			error = true;
		}

		public override DataType copy() {
			return new InvalidType();
		}
	}
}
