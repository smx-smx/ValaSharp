using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.Code;
using Vala.Lang.CodeNodes;
using Vala.Lang.Parser;

namespace Vala.Lang.Expressions {
	public class ValaTuple : Expression {
		private List<Expression> expression_list = new List<Expression>();

		public ValaTuple(SourceReference source_reference = null) {
			this.source_reference = source_reference;
		}

		public override void accept_children(CodeVisitor visitor) {
			foreach (Expression expr in expression_list) {
				expr.accept(visitor);
			}
		}

		public override void accept(CodeVisitor visitor) {
			visitor.visit_tuple(this);

			visitor.visit_expression(this);
		}

		public void add_expression(Expression expr) {
			expression_list.Add(expr);
		}

		public List<Expression> get_expressions() {
			return expression_list;
		}

		public override bool is_pure() {
			return false;
		}

		public override void replace_expression(Expression old_node, Expression new_node) {
			for (int i = 0; i < expression_list.Count; i++) {
				if (expression_list[i] == old_node) {
					expression_list[i] = new_node;
				}
			}
		}

		public override bool check(CodeContext context) {
			if (is_checked) {
				return !error;
			}

			is_checked = true;

			Report.error(source_reference, "tuples are not supported");
			error = true;
			return false;
		}

		public override void emit(CodeGenerator codegen) {
			foreach (Expression expr in expression_list) {
				expr.emit(codegen);
			}

			codegen.visit_tuple(this);

			codegen.visit_expression(this);
		}
	}
}
