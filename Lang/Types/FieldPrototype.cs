using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vala.Lang.Types
{
	/**
 * A reference to an instance field without a specific instance.
 */
	public class FieldPrototype : DataType
	{
		public Field field_symbol { get; set; }

		public FieldPrototype(Field field_symbol) {
			this.field_symbol = field_symbol;
		}

		public override DataType copy() {
			var result = new FieldPrototype(field_symbol);
			return result;
		}

		public override string to_qualified_string(Scope scope) {
			return field_symbol.get_full_name();
		}
	}
}
