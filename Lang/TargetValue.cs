using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.Types;

namespace Vala.Lang {
	public class TargetValue {
		public DataType value_type { get; set; }
		public DataType actual_value_type { get; set; }

		protected TargetValue(DataType value_type) {
			this.value_type = value_type;
		}
	}
}
