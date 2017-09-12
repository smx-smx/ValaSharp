using Vala;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.CodeNodes;
using Vala.Lang.Parser;

namespace Vala.Lang.Symbols
{
	public class TypeParameter : Symbol
	{
		/**
		* Creates a new generic type parameter.
		*
		* @param name              parameter name
		* @param source_reference  reference to source code
		* @return                  newly created generic type parameter
		*/
		public TypeParameter(string name, SourceReference source_reference) : base(name, source_reference) {
		}

		public override void accept(CodeVisitor visitor) {
			visitor.visit_type_parameter(this);
		}

		/**
		* Checks two type parameters for equality.
		*
		* @param param2 a type parameter
		* @return      true if this type parameter is equal to param2, false
		*              otherwise
		*/
		public bool equals(TypeParameter param2) {
			/* only type parameters with a common scope are comparable */
			if (!owner.is_subscope_of(param2.owner) && !param2.owner.is_subscope_of(owner)) {
				Report.error(source_reference, "internal error: comparing type parameters from different scopes");
				return false;
			}

			return name == param2.name && parent_symbol == param2.parent_symbol;
		}
	}
}
