using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GLibPorts.GLib;

namespace CLanguage.Expressions {
	/// <summary>
	/// Represents an expression with two operands in C code.
	/// </summary>
	public class CCodeBinaryExpression : CCodeExpression {
		/// <summary>
		/// The binary operator.
		/// </summary>
		public CCodeBinaryOperator Operator { get; set; }

		/// <summary>
		/// The left operand.
		/// </summary>
		public CCodeExpression left { get; set; }

		/// <summary>
		/// The right operand.
		/// </summary>
		public CCodeExpression right { get; set; }

		public CCodeBinaryExpression(CCodeBinaryOperator op, CCodeExpression l, CCodeExpression r) {
			Operator = op;
			left = l;
			right = r;
		}

		public override void write(CCodeWriter writer) {
			left.write_inner(writer);

			switch (Operator) {
			case CCodeBinaryOperator.PLUS: writer.write_string(" + "); break;
			case CCodeBinaryOperator.MINUS: writer.write_string(" - "); break;
			case CCodeBinaryOperator.MUL: writer.write_string(" * "); break;
			case CCodeBinaryOperator.DIV: writer.write_string(" / "); break;
			case CCodeBinaryOperator.MOD: writer.write_string(" % "); break;
			case CCodeBinaryOperator.SHIFT_LEFT: writer.write_string(" << "); break;
			case CCodeBinaryOperator.SHIFT_RIGHT: writer.write_string(" >> "); break;
			case CCodeBinaryOperator.LESS_THAN: writer.write_string(" < "); break;
			case CCodeBinaryOperator.GREATER_THAN: writer.write_string(" > "); break;
			case CCodeBinaryOperator.LESS_THAN_OR_EQUAL: writer.write_string(" <= "); break;
			case CCodeBinaryOperator.GREATER_THAN_OR_EQUAL: writer.write_string(" >= "); break;
			case CCodeBinaryOperator.EQUALITY: writer.write_string(" == "); break;
			case CCodeBinaryOperator.INEQUALITY: writer.write_string(" != "); break;
			case CCodeBinaryOperator.BITWISE_AND: writer.write_string(" & "); break;
			case CCodeBinaryOperator.BITWISE_OR: writer.write_string(" | "); break;
			case CCodeBinaryOperator.BITWISE_XOR: writer.write_string(" ^ "); break;
			case CCodeBinaryOperator.AND: writer.write_string(" && "); break;
			case CCodeBinaryOperator.OR: writer.write_string(" || "); break;
			default: assert_not_reached(); break;
			}

			right.write_inner(writer);
		}

		public override void write_inner(CCodeWriter writer) {
			writer.write_string("(");
			this.write(writer);
			writer.write_string(")");
		}
	}

	public enum CCodeBinaryOperator {
		PLUS,
		MINUS,
		MUL,
		DIV,
		MOD,
		SHIFT_LEFT,
		SHIFT_RIGHT,
		LESS_THAN,
		GREATER_THAN,
		LESS_THAN_OR_EQUAL,
		GREATER_THAN_OR_EQUAL,
		EQUALITY,
		INEQUALITY,
		BITWISE_AND,
		BITWISE_OR,
		BITWISE_XOR,
		AND,
		OR
	}

}
