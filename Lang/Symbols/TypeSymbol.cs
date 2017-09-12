using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.Parser;

namespace Vala.Lang.Symbols
{
	public abstract class TypeSymbol : Symbol
	{
		public TypeSymbol(string name, SourceReference source_reference = null, Comment comment = null) : base(name, source_reference, comment) {
		}

		/**
		 * Checks whether this data type has value or reference type semantics.
		 *
		 * @return true if this data type has reference type semantics
		 */
		public virtual bool is_reference_type() {
			return false;
		}

		/**
		 * Checks whether this data type is equal to or a subtype of the
		 * specified data type.
		 *
		 * @param t a data type
		 * @return  true if t is a supertype of this data type, false otherwise
		 */
		public virtual bool is_subtype_of(TypeSymbol t) {
			return (this == t);
		}

		/**
		 * Return the index of the specified type parameter name.
		 */
		public virtual int get_type_parameter_index(string name) {
			return -1;
		}
	}
}
