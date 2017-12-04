using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.TypeSymbols;

namespace Vala.Lang.Types {
	/**
	 * A class type.
	 */
	public class ClassType : ReferenceType {
		private WeakReference<Class> class_symbol_weak = new WeakReference<Class>(null);

		/**
		 * The referred class.
		 */
		public Class class_symbol {
			get {
				return class_symbol_weak.GetTarget();
			}
			set {
				class_symbol_weak.SetTarget(value);
			}
		}

		public ClassType(Class class_symbol) {
			this.class_symbol = class_symbol;
		}

		public override DataType copy() {
			var result = new ClassType(class_symbol);
			result.source_reference = source_reference;
			result.value_owned = value_owned;
			result.nullable = nullable;
			result.is_dynamic = is_dynamic;
			result.floating_reference = floating_reference;

			foreach (DataType arg in get_type_arguments()) {
				result.add_type_argument(arg.copy());
			}

			return result;
		}
	}

}
