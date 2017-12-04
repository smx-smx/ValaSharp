using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.Code;
using Vala.Lang.CodeNodes;
using Vala.Lang.Parser;
using Vala.Lang.Types;
using Vala.Lang.TypeSymbols;

namespace Vala.Lang.Literals {
	/**
 * Represents a single literal character.
 */
	public class CharacterLiteral : Literal {
		/**
		 * The literal value.
		 */
		public string value {
			get {
				return _value;
			}
			set {
				_value = value;

				if (!value.validate()) {
					error = true;
				}
			}
		}

		private string _value;

		/**
		 * Creates a new character literal.
		 *
		 * @param c      character
		 * @param source reference to source code
		 * @return       newly created character literal
		 */
		public CharacterLiteral(string c, SourceReference source = null) {
			value = c;
			source_reference = source;
		}

		public override void accept(CodeVisitor visitor) {
			visitor.visit_character_literal(this);

			visitor.visit_expression(this);
		}

		/**
		 * Returns the unicode character value this character literal
		 * represents.
		 *
		 * @return unicode character value
		 */
		public char get_char() {
			return value[1];
		}

		public override bool is_pure() {
			return true;
		}

		public override string to_string() {
			return value;
		}

		public override bool check(CodeContext context) {
			if (is_checked) {
				return !error;
			}

			is_checked = true;

			if (get_char() < 128) {
				value_type = new IntegerType((Struct)context.analyzer.root_symbol.scope.lookup("char"));
			} else {
				value_type = new IntegerType((Struct)context.analyzer.root_symbol.scope.lookup("unichar"));
			}

			return !error;
		}

		public override void emit(CodeGenerator codegen) {
			codegen.visit_character_literal(this);

			codegen.visit_expression(this);
		}
	}

}
