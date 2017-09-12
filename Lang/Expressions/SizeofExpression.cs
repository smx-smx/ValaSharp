using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.Code;
using Vala.Lang.CodeNodes;
using Vala.Lang.Parser;
using Vala.Lang.Types;

namespace Vala.Lang.Expressions
{
	/**
	 * Represents a sizeof expression in the source code.
	 */
	public class SizeofExpression : Expression
	{
		/**
		 * The type whose size to be retrieved.
		 */
		public DataType type_reference {
			get { return _data_type; }
			set {
				_data_type = value;
				_data_type.parent_node = this;
			}
		}

		private DataType _data_type;

		/**
		 * Creates a new sizeof expression.
		 *
		 * @param type   a data type
		 * @param source reference to source code
		 * @return       newly created sizeof expression
		 */
		public SizeofExpression(DataType type, SourceReference source) {
			type_reference = type;
			source_reference = source;
		}

		public override void accept(CodeVisitor visitor) {
			visitor.visit_sizeof_expression(this);

			visitor.visit_expression(this);
		}

		public override void accept_children(CodeVisitor visitor) {
			type_reference.accept(visitor);
		}

		public override bool is_pure() {
			return true;
		}

		public override bool is_constant() {
			return true;
		}

		public override void replace_type(DataType old_type, DataType new_type) {
			if (type_reference == old_type) {
				type_reference = new_type;
			}
		}

		public override bool check(CodeContext context) {
			if (is_checked) {
				return !error;
			}

			is_checked = true;

			type_reference.check(context);

			value_type = context.analyzer.ulong_type;

			return !error;
		}

		public override void emit(CodeGenerator codegen) {
			codegen.visit_sizeof_expression(this);

			codegen.visit_expression(this);
		}
	}
}
