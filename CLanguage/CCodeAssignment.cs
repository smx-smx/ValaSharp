using CLanguage.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLanguage {
	/**
	 * Represents an assignment expression in the C code.
	 */
	public class CCodeAssignment : CCodeExpression {
		/**
		 * Left hand side of the assignment.
		 */
		public CCodeExpression left { get; set; }

		/**
		 * Assignment Operator.
		 */
		public CCodeAssignmentOperator Operator { get; set; }

		/**
		 * Right hand side of the assignment.
		 */
		public CCodeExpression right { get; set; }

		public CCodeAssignment(CCodeExpression l, CCodeExpression r, CCodeAssignmentOperator op = CCodeAssignmentOperator.SIMPLE) {
			left = l;
			Operator = op;
			right = r;
		}

		public override void write(CCodeWriter writer) {
			left.write(writer);

			writer.write_string(" ");

			if (Operator == CCodeAssignmentOperator.BITWISE_OR) {
				writer.write_string("|");
			} else if (Operator == CCodeAssignmentOperator.BITWISE_AND) {
				writer.write_string("&");
			} else if (Operator == CCodeAssignmentOperator.BITWISE_XOR) {
				writer.write_string("^");
			} else if (Operator == CCodeAssignmentOperator.ADD) {
				writer.write_string("+");
			} else if (Operator == CCodeAssignmentOperator.SUB) {
				writer.write_string("-");
			} else if (Operator == CCodeAssignmentOperator.MUL) {
				writer.write_string("*");
			} else if (Operator == CCodeAssignmentOperator.DIV) {
				writer.write_string("/");
			} else if (Operator == CCodeAssignmentOperator.PERCENT) {
				writer.write_string("%");
			} else if (Operator == CCodeAssignmentOperator.SHIFT_LEFT) {
				writer.write_string("<<");
			} else if (Operator == CCodeAssignmentOperator.SHIFT_RIGHT) {
				writer.write_string(">>");
			}

			writer.write_string("= ");

			right.write(writer);
		}

		public override void write_inner(CCodeWriter writer) {
			writer.write_string("(");
			this.write(writer);
			writer.write_string(")");
		}
	}

	public enum CCodeAssignmentOperator {
		SIMPLE,
		BITWISE_OR,
		BITWISE_AND,
		BITWISE_XOR,
		ADD,
		SUB,
		MUL,
		DIV,
		PERCENT,
		SHIFT_LEFT,
		SHIFT_RIGHT
	}

}
