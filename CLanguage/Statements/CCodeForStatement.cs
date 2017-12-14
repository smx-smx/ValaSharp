using CLanguage.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLanguage.Statements {
	/// <summary>
	/// Represents a for iteration statement in the C code.
	/// </summary>
	public class CCodeForStatement : CCodeStatement {
		/// <summary>
		/// The loop condition.
		/// </summary>
		public CCodeExpression condition { get; set; }

		/// <summary>
		/// The loop body.
		/// </summary>
		public CCodeStatement body { get; set; }

		private List<CCodeExpression> initializer = new List<CCodeExpression>();
		private List<CCodeExpression> iterator = new List<CCodeExpression>();

		public CCodeForStatement(CCodeExpression condition, CCodeStatement body = null) {
			this.condition = condition;
			this.body = body;
		}

		/// <summary>
		/// Appends the specified expression to the list of initializers.
		/// 
		/// <param name="expr">an initializer expression</param>
		/// </summary>
		public void add_initializer(CCodeExpression expr) {
			initializer.Add(expr);
		}

		/// <summary>
		/// Appends the specified expression to the iterator.
		/// 
		/// <param name="expr">an iterator expression</param>
		/// </summary>
		public void add_iterator(CCodeExpression expr) {
			iterator.Add(expr);
		}

		public override void write(CCodeWriter writer) {
			bool first;

			writer.write_indent(line);
			writer.write_string("for (");

			first = true;
			foreach (CCodeExpression init_expr in initializer) {
				if (!first) {
					writer.write_string(", ");
				} else {
					first = false;
				}
				if (init_expr != null) {
					init_expr.write(writer);
				}
			}

			writer.write_string("; ");
			if (condition != null) {
				condition.write(writer);
			}
			writer.write_string("; ");

			first = true;
			foreach (CCodeExpression it_expr in iterator) {
				if (!first) {
					writer.write_string(", ");
				} else {
					first = false;
				}
				if (it_expr != null) {
					it_expr.write(writer);
				}
			}

			writer.write_string(")");
			body.write(writer);
		}
	}
}
