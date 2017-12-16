using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.CodeNodes;
using Vala.Lang.Literals;
using Vala.Lang.Parser;

namespace Vala.Lang.Expressions {
	public class Template : Expression {
		private List<Expression> expression_list = new List<Expression>();

		public Template(SourceReference source_reference = null) {
			this.source_reference = source_reference;
		}

		public override void accept(CodeVisitor visitor) {
			visitor.visit_template(this);
		}

		public override void accept_children(CodeVisitor visitor) {
			foreach (var expr in expression_list) {
				expr.accept(visitor);
			}
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

		Expression stringify(Expression expr) {
			if (expr is StringLiteral) {
				return expr;
			} else {
				return new MethodCall(new MemberAccess(expr, "to_string", expr.source_reference), expr.source_reference);
			}
		}

		public override bool check(CodeContext context) {
			if (is_checked) {
				return !error;
			}

			is_checked = true;

			Expression expr;

			if (expression_list.Count == 0) {
				expr = new StringLiteral("\"\"", source_reference);
			} else {
				expr = stringify(expression_list[0]);
				if (expression_list.Count > 1) {
					var concat = new MethodCall(new MemberAccess(expr, "concat", source_reference), source_reference);
					for (int i = 1; i < expression_list.Count; i++) {
						concat.add_argument(stringify(expression_list[i]));
					}
					expr = concat;
				}
			}
			expr.target_type = target_type;

			context.analyzer.replaced_nodes.Add(this);
			parent_node.replace_expression(this, expr);
			return expr.check(context);
		}
	}
}
