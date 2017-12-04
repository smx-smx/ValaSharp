using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GLibPorts.GLib;

namespace CLanguage.Expressions {
	/**
	 * Represents an expression with one operand in the C code.
	 */
	public class CCodeUnaryExpression : CCodeExpression {
		/**
		 * The unary operator.
		 */
		public CCodeUnaryOperator Operator { get; set; }

		/**
		 * The operand.
		 */
		public CCodeExpression inner { get; set; }

		public CCodeUnaryExpression(CCodeUnaryOperator op, CCodeExpression expr) {
			Operator = op;
			inner = expr;
		}

		public override void write(CCodeWriter writer) {
			switch (Operator) {
				case CCodeUnaryOperator.PLUS: writer.write_string("+"); inner.write_inner(writer); break;
				case CCodeUnaryOperator.MINUS: writer.write_string("-"); inner.write_inner(writer); break;
				case CCodeUnaryOperator.LOGICAL_NEGATION: writer.write_string("!"); inner.write_inner(writer); break;
				case CCodeUnaryOperator.BITWISE_COMPLEMENT: writer.write_string("~"); inner.write_inner(writer); break;
				case CCodeUnaryOperator.POINTER_INDIRECTION:
					var inner_unary = inner as CCodeUnaryExpression;
					if (inner_unary != null && inner_unary.Operator == CCodeUnaryOperator.ADDRESS_OF) {
						// simplify expression
						inner_unary.inner.write(writer);
						return;
					}
					writer.write_string("*");
					inner.write_inner(writer);
					break;
				case CCodeUnaryOperator.ADDRESS_OF:
					var _inner_unary_ = inner as CCodeUnaryExpression;
					if (_inner_unary_ != null && _inner_unary_.Operator == CCodeUnaryOperator.POINTER_INDIRECTION) {
						// simplify expression
						_inner_unary_.inner.write(writer);
						return;
					}
					writer.write_string("&");
					inner.write_inner(writer);
					break;
				case CCodeUnaryOperator.PREFIX_INCREMENT: writer.write_string("++"); break;
				case CCodeUnaryOperator.PREFIX_DECREMENT: writer.write_string("--"); break;
				case CCodeUnaryOperator.POSTFIX_INCREMENT: inner.write_inner(writer); writer.write_string("++"); break;
				case CCodeUnaryOperator.POSTFIX_DECREMENT: inner.write_inner(writer); writer.write_string("--"); break;
				default: assert_not_reached(); break;
			}
		}

		public override void write_inner(CCodeWriter writer) {
			writer.write_string("(");
			this.write(writer);
			writer.write_string(")");
		}
	}

	public enum CCodeUnaryOperator {
		PLUS,
		MINUS,
		LOGICAL_NEGATION,
		BITWISE_COMPLEMENT,
		POINTER_INDIRECTION,
		ADDRESS_OF,
		PREFIX_INCREMENT,
		PREFIX_DECREMENT,
		POSTFIX_INCREMENT,
		POSTFIX_DECREMENT
	}
}
