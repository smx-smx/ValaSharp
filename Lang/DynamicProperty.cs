using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.CodeNodes;
using Vala.Lang.Parser;
using Vala.Lang.Types;

namespace Vala.Lang
{
	/**
	 * Represents a late bound property.
	 */
	public class DynamicProperty : Property
	{
		public DataType dynamic_type { get; set; }

		public DynamicProperty(DataType dynamic_type, string name, SourceReference source_reference = null, Comment comment = null)
			: base(name, null, null, null, source_reference, comment)
		{
			this.dynamic_type = dynamic_type;
		}

		public override bool check(CodeContext context) {
			return true;
		}
	}

}
