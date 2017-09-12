using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.Parser;

namespace Vala.Lang.Types
{
	public class VoidType : DataType
	{
		public VoidType(SourceReference source_reference = null) {
			this.source_reference = source_reference;
		}

		public override bool stricter(DataType type2) {
			return (type2 is VoidType);
		}

		public override string to_qualified_string(Scope scope) {
			return "void";
		}

		public override DataType copy() {
			return new VoidType(source_reference);
		}
	}
}
