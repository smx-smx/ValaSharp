using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.Code;
using Vala.Lang.CodeNodes;
using Vala.Lang.Parser;
using Vala.Lang.Symbols;
using Vala.Lang.Types;

namespace Vala.Lang.Expressions {
	/**
 * Represents a postfix increment or decrement expression.
 */
	public class PostfixExpression : Expression {
		/**
		 * The operand, must be a variable or a property.
		 */
		public Expression inner {
			get { return _inner; }
			set {
				_inner = value;
				_inner.parent_node = this;
			}
		}

		/**
		 * Specifies whether value should be incremented or decremented.
		 */
		public bool increment { get; set; }

		private Expression _inner;

		/**
		 * Creates a new postfix expression.
		 *
		 * @param _inner  operand expression
		 * @param inc     true for increment, false for decrement
		 * @param source  reference to source code
		 * @return newly  created postfix expression
		 */
		public PostfixExpression(Expression _inner, bool inc, SourceReference source) {
			inner = _inner;
			increment = inc;
			source_reference = source;
		}

		public override void accept(CodeVisitor visitor) {
			visitor.visit_postfix_expression(this);

			visitor.visit_expression(this);
		}

		public override void accept_children(CodeVisitor visitor) {
			inner.accept(visitor);
		}

		public override bool is_pure() {
			return false;
		}

		public override bool is_accessible(Symbol sym) {
			return inner.is_accessible(sym);
		}

		public override void get_defined_variables(ICollection<Variable> collection) {
			inner.get_defined_variables(collection);
			var local = inner.symbol_reference as LocalVariable;
			var param = inner.symbol_reference as Parameter;
			if (local != null) {
				collection.Add(local);
			} else if (param != null && param.direction == ParameterDirection.OUT) {
				collection.Add(param);
			}
		}

		public override void get_used_variables(ICollection<Variable> collection) {
			inner.get_used_variables(collection);
		}

		public override bool check(CodeContext context) {
			if (is_checked) {
				return !error;
			}

			is_checked = true;

			inner.lvalue = true;
			if (!inner.check(context)) {
				error = true;
				return false;
			}

			if (!(inner.value_type is IntegerType) && !(inner.value_type is FloatingType) && !(inner.value_type is PointerType)) {
				error = true;
				Report.error(source_reference, "unsupported lvalue in postfix expression");
				return false;
			}

			if (inner is MemberAccess) {
				var ma = (MemberAccess)inner;

				if (ma.prototype_access) {
					error = true;
					Report.error(source_reference, "Access to instance member `%s' denied".printf(ma.symbol_reference.get_full_name()));
					return false;
				}

				if (ma.error || ma.symbol_reference == null) {
					error = true;
					/* if no symbol found, skip this check */
					return false;
				}
			} else if (inner is ElementAccess) {
				var ea = (ElementAccess)inner;
				if (!(ea.container.value_type is ArrayType)) {
					error = true;
					Report.error(source_reference, "unsupported lvalue in postfix expression");
					return false;
				}
			} else {
				error = true;
				Report.error(source_reference, "unsupported lvalue in postfix expression");
				return false;
			}

			if (inner is MemberAccess) {
				var ma = (MemberAccess)inner;

				if (ma.symbol_reference is Property) {
					var prop = (Property)ma.symbol_reference;

					if (prop.set_accessor == null || !prop.set_accessor.writable) {
						ma.error = true;
						Report.error(ma.source_reference, "Property `%s' is read-only".printf(prop.get_full_name()));
						return false;
					}
				}
			}

			value_type = inner.value_type;

			return !error;
		}

		public override void emit(CodeGenerator codegen) {
			inner.emit(codegen);

			codegen.visit_postfix_expression(this);

			codegen.visit_expression(this);
		}
	}
}
