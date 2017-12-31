using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.Code;
using Vala.Lang.CodeNodes;
using Vala.Lang.Parser;
using Vala.Lang.Symbols;
using Vala.Lang.TypeSymbols;

namespace Vala.Lang.Expressions {
	/// <summary>
	/// Represents an access to base class members in the source code.
	/// </summary>
	public class BaseAccess : Expression {
		/// <summary>
		/// Creates a new base access expression.
		/// 
		/// <param name="source">reference to source code</param>
		/// <returns>newly created base access expression</returns>
		/// </summary>
		public BaseAccess(SourceReference source = null) {
			source_reference = source;
		}

		public override void accept(CodeVisitor visitor) {
			visitor.visit_base_access(this);

			visitor.visit_expression(this);
		}

		public override string to_string() {
			return "base";
		}

		public override bool is_pure() {
			return true;
		}

		public override bool check(CodeContext context) {
			if (is_checked) {
				return !error;
			}

			is_checked = true;

			if (!context.analyzer.is_in_instance_method()) {
				error = true;
				Report.error(source_reference, "Base access invalid outside of instance methods");
				return false;
			}

			if (context.analyzer.current_class == null) {
				if (context.analyzer.current_struct == null) {
					error = true;
					Report.error(source_reference, "Base access invalid outside of class and struct");
					return false;
				} else if (context.analyzer.current_struct.base_type == null) {
					error = true;
					Report.error(source_reference, "Base access invalid without base type");
					return false;
				}
				value_type = context.analyzer.current_struct.base_type;
			} else if (context.analyzer.current_class.base_class == null) {
				error = true;
				Report.error(source_reference, "Base access invalid without base class");
				return false;
			} else {
				foreach (var base_type in context.analyzer.current_class.get_base_types()) {
					if (base_type.data_type is Class) {
						value_type = base_type.copy();
						value_type.value_owned = false;
					}
				}
			}

			symbol_reference = value_type.data_type;

			return !error;
		}

		public override void emit(CodeGenerator codegen) {
			codegen.visit_base_access(this);

			codegen.visit_expression(this);
		}
	}

}
