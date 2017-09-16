using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.Code;
using Vala.Lang.CodeNodes;
using Vala.Lang.Parser;
using Vala.Lang.Statements;

namespace Vala.Lang
{
	/**
 * Represents a switch section in the source code.
 */
	public class SwitchSection : Block
	{
		private List<SwitchLabel> labels = new List<SwitchLabel>();

		/**
		 * Creates a new switch section.
		 *
		 * @param source_reference reference to source code
		 * @return                 newly created switch section
		 */
		public SwitchSection(SourceReference source_reference) : base(source_reference) { }

		/**
		 * Appends the specified label to the list of switch labels.
		 *
		 * @param label a switch label
		 */
		public void add_label(SwitchLabel label) {
			if (labels.Count == 0) {
				this.source_reference = label.source_reference;
			}

			labels.Add(label);
			label.section = this;
		}

		/**
		 * Returns a copy of the list of switch labels.
		 *
		 * @return switch label list
		 */
		public List<SwitchLabel> get_labels() {
			return labels;
		}

		public bool has_default_label() {
			foreach (SwitchLabel label in labels) {
				if (label.expression == null) {
					return true;
				}
			}

			return false;
		}

		public override void accept(CodeVisitor visitor) {
			visitor.visit_switch_section(this);
		}

		public override void accept_children(CodeVisitor visitor) {
			foreach (SwitchLabel label in labels) {
				label.accept(visitor);
			}

			foreach (Statement st in get_statements()) {
				st.node.accept(visitor);
			}
		}

		public override bool check(CodeContext context) {
			if (is_checked) {
				return !error;
			}

			is_checked = true;

			foreach (SwitchLabel label in get_labels()) {
				label.check(context);
			}

			owner = context.analyzer.current_symbol.scope;

			var old_symbol = context.analyzer.current_symbol;
			var old_insert_block = context.analyzer.insert_block;
			context.analyzer.current_symbol = this;
			context.analyzer.insert_block = this;

			foreach (Statement st in get_statements()) {
				st.node.check(context);
			}

			foreach (LocalVariable local in get_local_variables()) {
				local.active = false;
			}

			// use get_statements () instead of statement_list to not miss errors within StatementList objects
			foreach (Statement stmt in get_statements()) {
				add_error_types(stmt.node.get_error_types());
			}

			context.analyzer.current_symbol = old_symbol;
			context.analyzer.insert_block = old_insert_block;

			return !error;
		}

		public override void emit(CodeGenerator codegen) {
			foreach (SwitchLabel label in labels) {
				label.emit(codegen);
			}

			base.emit(codegen);
		}
	}
}
