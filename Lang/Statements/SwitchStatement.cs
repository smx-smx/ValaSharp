using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.Code;
using Vala.Lang.CodeNodes;
using Vala.Lang.Expressions;
using Vala.Lang.Literals;
using Vala.Lang.Parser;
using Vala.Lang.Types;

namespace Vala.Lang.Statements {
	/// <summary>
	/// Represents a switch selection statement in the source code.
	/// </summary>
	public class SwitchStatement : CodeNode, Statement {
		public CodeNode node {
			get { return this; }
		}

		/// <summary>
		/// Specifies the switch expression.
		/// </summary>
		public Expression expression {
			get {
				return _expression;
			}
			set {
				_expression = value;
				_expression.parent_node = this;
			}
		}

		private Expression _expression;
		private List<SwitchSection> sections = new List<SwitchSection>();

		/// <summary>
		/// Creates a new switch statement.
		/// 
		/// <param name="expression">switch expression</param>
		/// <param name="source_reference">reference to source code</param>
		/// <returns>newly created switch statement</returns>
		/// </summary>
		public SwitchStatement(Expression expression, SourceReference source_reference) {
			this.source_reference = source_reference;
			this.expression = expression;
		}

		/// <summary>
		/// Appends the specified section to the list of switch sections.
		/// 
		/// <param name="section">a switch section</param>
		/// </summary>
		public void add_section(SwitchSection section) {
			section.parent_node = this;
			sections.Add(section);
		}

		/// <summary>
		/// Returns a copy of the list of switch sections.
		/// 
		/// <returns>section list</returns>
		/// </summary>
		public List<SwitchSection> get_sections() {
			return sections;
		}

		public override void accept(CodeVisitor visitor) {
			visitor.visit_switch_statement(this);
		}

		public override void accept_children(CodeVisitor visitor) {
			expression.accept(visitor);

			visitor.visit_end_full_expression(expression);

			foreach (SwitchSection section in sections) {
				section.accept(visitor);
			}
		}

		public override void replace_expression(Expression old_node, Expression new_node) {
			if (expression == old_node) {
				expression = new_node;
			}
		}

		public override bool check(CodeContext context) {
			if (is_checked) {
				return !error;
			}

			is_checked = true;

			if (!expression.check(context)) {
				error = true;
				return false;
			}

			if (expression.value_type == null ||
				(!(expression.value_type is IntegerType) &&
				 !(expression.value_type is EnumValueType) &&
				 !expression.value_type.compatible(context.analyzer.string_type))) {
				Report.error(expression.source_reference, "Integer or string expression expected");
				error = true;
				return false;
			}

			// ensure that possibly owned (string) expression stays alive
			expression.target_type = expression.value_type.copy();
			expression.target_type.nullable = false;

			var labelset = new HashSet<string>();
			foreach (SwitchSection section in sections) {
				section.check(context);

				// check for duplicate literal case labels
				foreach (SwitchLabel label in section.get_labels()) {
					if (label.expression != null) {
						string value = null;
						if (label.expression is StringLiteral) {
							value = ((StringLiteral)label.expression).eval();
						} else if (label.expression is Literal) {
							value = ((Literal)label.expression).ToString();
						} else if (label.expression.is_constant()) {
							value = label.expression.ToString();
						}

						if (value != null && !labelset.Add(value)) {
							error = true;
							Report.error(label.expression.source_reference, "Switch statement already contains this label");
						}
					}
				}
				add_error_types(section.get_error_types());
			}

			return !error;
		}

		public override void emit(CodeGenerator codegen) {
			expression.emit(codegen);

			codegen.visit_end_full_expression(expression);

			codegen.visit_switch_statement(this);
		}
	}
}
