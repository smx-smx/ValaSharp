using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.CodeNodes;
using Vala.Lang.Parser;

namespace Vala.Lang.Types
{
	public class NullType : ReferenceType
	{
		public NullType(SourceReference source_reference) {
			this.nullable = true;
			this.source_reference = source_reference;
		}

		public override bool compatible(DataType target_type) {
			if (CodeContext.get().experimental_non_null) {
				return target_type.nullable;
			}

			if (!(target_type is PointerType) && (target_type is NullType || (target_type.data_type == null && target_type.type_parameter == null))) {
				return true;
			}

			/* null can be cast to any reference or array type or pointer type */
			if (target_type.type_parameter != null ||
				target_type is PointerType ||
				target_type.nullable ||
				target_type.data_type.get_attribute("PointerType") != null) {
				return true;
			}

			if (target_type.data_type.is_reference_type() ||
				target_type is ArrayType ||
				target_type is DelegateType) {
				return true;
			}

			/* null is not compatible with any other type (i.e. value types) */
			return false;
		}

		public override DataType copy() {
			return new NullType(source_reference);
		}

		public override bool is_disposable() {
			return false;
		}

		public override string to_qualified_string(Scope scope = null) {
			return "null";
		}
	}

}
