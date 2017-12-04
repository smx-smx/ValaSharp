using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.CodeNodes;
using Vala.Lang.Expressions;
using Vala.Lang.Parser;
using Vala.Lang.Types;

namespace Vala.Lang.Methods {
	/**
	 * Represents a late bound method.
	 */
	public class DynamicMethod : Method {
		public DataType dynamic_type { get; set; }

		public MethodCall invocation { get; set; }

		public DynamicMethod(DataType dynamic_type, string name, DataType return_type, SourceReference source_reference = null, Comment comment = null)
			: base(name, return_type, source_reference, comment) {
			this.dynamic_type = dynamic_type;
		}

		public override bool check(CodeContext context) {
			return true;
		}
	}

}
