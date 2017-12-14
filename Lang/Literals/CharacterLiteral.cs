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
	/// <summary>
	/// Represents a single literal character.
	/// </summary>
	public class CharacterLiteral : Literal {
		/// <summary>
		/// The literal value.
		/// </summary>
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

		/// <summary>
		/// Creates a new character literal.
		/// 
		/// <param name="c">character</param>
		/// <param name="source">reference to source code</param>
		/// <returns>newly created character literal</returns>
		/// </summary>
		public CharacterLiteral(string c, SourceReference source = null) {
			value = c;
			source_reference = source;
		}

		public override void accept(CodeVisitor visitor) {
			visitor.visit_character_literal(this);

			visitor.visit_expression(this);
		}

		/// <summary>
		/// Returns the unicode character value this character literal
		/// represents.
		/// 
		/// <returns>unicode character value</returns>
		/// </summary>
		public char get_char() {
			return value[1];
		}

		public override bool is_pure() {
			return true;
		}

		public override string ToString() {
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
