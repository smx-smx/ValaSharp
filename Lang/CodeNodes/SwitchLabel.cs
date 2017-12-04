using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.Code;
using Vala.Lang.Expressions;
using Vala.Lang.Parser;
using Vala.Lang.Statements;

namespace Vala.Lang.CodeNodes {
	/**
 * Represents a switch label in the source code.
 */
	public class SwitchLabel : CodeNode {
		/**
		 * Specifies the label expression.
		 */
		public Expression expression { get; set; }

		private WeakReference<SwitchSection> section_weak = new WeakReference<SwitchSection>(null);

		public SwitchSection section {
			get {
				return section_weak.GetTarget();
			}
			set {
				section_weak.SetTarget(value);
			}
		}

		/**
		 * Creates a new switch case label.
		 *
		 * @param expr   label expression
		 * @param source reference to source code
		 * @return       newly created switch case label
		 */
		public SwitchLabel(Expression expr, SourceReference source = null) {
			expression = expr;
			source_reference = source;
		}

		/**
		 * Creates a new switch default label.
		 *
		 * @param source reference to source code
		 * @return       newly created switch default label
		 */
		public static SwitchLabel with_default(SourceReference source = null) {
			return new SwitchLabel(null, source);
		}

		public override void accept(CodeVisitor visitor) {
			visitor.visit_switch_label(this);
		}

		public override void accept_children(CodeVisitor visitor) {
			if (expression != null) {
				expression.accept(visitor);

				visitor.visit_end_full_expression(expression);
			}
		}

		public override bool check(CodeContext context) {
			if (expression != null) {
				expression.check(context);

				var switch_statement = (SwitchStatement)section.parent_node;
				if (!expression.is_constant()) {
					error = true;
					Report.error(expression.source_reference, "Expression must be constant");
					return false;
				}
				if (!expression.value_type.compatible(switch_statement.expression.value_type)) {
					error = true;
					Report.error(expression.source_reference, "Cannot convert from `%s' to `%s'".printf(expression.value_type.to_string(), switch_statement.expression.value_type.to_string()));
					return false;
				}
			}

			return true;
		}

		public override void emit(CodeGenerator codegen) {
			codegen.visit_switch_label(this);
		}
	}
}
