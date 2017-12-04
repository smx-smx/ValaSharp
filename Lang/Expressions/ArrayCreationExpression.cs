using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.Code;
using Vala.Lang.CodeNodes;
using Vala.Lang.Literals;
using Vala.Lang.Parser;
using Vala.Lang.Symbols;
using Vala.Lang.Types;

namespace Vala.Lang.Expressions {
	/**
	 * Represents an array creation expression e.g. {{{ new int[] {1,2,3} }}}.
	 */
	public class ArrayCreationExpression : Expression {
		/**
		 * The type of the elements of the array.
		 */
		public DataType element_type {
			get { return _element_type; }
			set {
				_element_type = value;
				_element_type.parent_node = this;
			}
		}

		/**
		 * The rank of the array.
		 */
		public int rank { get; set; }

		/**
		 * The size for each dimension ascending from left to right.
		 */
		private List<Expression> sizes = new List<Expression>();

		/**
		 * The root array initializer list.
		 */
		public InitializerList initializer_list {
			get { return _initializer_list; }
			set {
				_initializer_list = value;
				if (_initializer_list != null) {
					_initializer_list.parent_node = this;
				}
			}
		}

		private DataType _element_type;
		private InitializerList _initializer_list;

		/**
		 * Add a size expression.
		 */
		public void append_size(Expression size) {
			sizes.Add(size);
			if (size != null) {
				size.parent_node = this;
			}
		}

		/**
		 * Get the sizes for all dimensions ascending from left to right.
		 */
		public List<Expression> get_sizes() {
			return sizes;
		}

		public ArrayCreationExpression(DataType element_type, int rank, InitializerList initializer_list, SourceReference source_reference) {
			this.element_type = element_type;
			this.rank = rank;
			this.initializer_list = initializer_list;
			this.source_reference = source_reference;
		}

		public override void accept_children(CodeVisitor visitor) {
			if (element_type != null) {
				element_type.accept(visitor);
			}

			foreach (Expression e in sizes) {
				e.accept(visitor);
			}

			if (initializer_list != null) {
				initializer_list.accept(visitor);
			}
		}

		public override void accept(CodeVisitor visitor) {
			visitor.visit_array_creation_expression(this);

			visitor.visit_expression(this);
		}

		public override bool is_pure() {
			return false;
		}

		public override bool is_accessible(Symbol sym) {
			foreach (Expression e in sizes) {
				if (!e.is_accessible(sym)) {
					return false;
				}
			}

			if (initializer_list != null) {
				return initializer_list.is_accessible(sym);
			}

			return true;
		}

		public override void replace_expression(Expression old_node, Expression new_node) {
			for (int i = 0; i < sizes.Count; i++) {
				if (sizes[i] == old_node) {
					sizes[i] = new_node;
					return;
				}
			}
		}

		public override void replace_type(DataType old_type, DataType new_type) {
			if (element_type == old_type) {
				element_type = new_type;
			}
		}

		private int create_sizes_from_initializer_list(CodeContext context, InitializerList il, int rank, List<Literal> sl) {
			if (sl.Count == (this.rank - rank)) {
				// only add size if this is the first initializer list of the current dimension
				var init = new IntegerLiteral(il.size.ToString(), il.source_reference);
				init.check(context);
				sl.Add(init);
			}

			int subsize = -1;
			foreach (Expression e in il.get_initializers()) {
				if (e is InitializerList) {
					if (rank == 1) {
						il.error = true;
						e.error = true;
						Report.error(e.source_reference, "Expected array element, got array initializer list");
						return -1;
					}
					int size = create_sizes_from_initializer_list(context, (InitializerList)e, rank - 1, sl);
					if (size == -1) {
						return -1;
					}
					if (subsize >= 0 && subsize != size) {
						il.error = true;
						Report.error(il.source_reference, "Expected initializer list of size %d, got size %d".printf(subsize, size));
						return -1;
					} else {
						subsize = size;
					}
				} else {
					if (rank != 1) {
						il.error = true;
						e.error = true;
						Report.error(e.source_reference, "Expected array initializer list, got array element");
						return -1;
					}
				}
			}
			return il.size;
		}

		public override bool check(CodeContext context) {
			if (is_checked) {
				return !error;
			}

			is_checked = true;

			List<Expression> sizes = get_sizes();
			var initlist = initializer_list;

			if (element_type != null) {
				element_type.check(context);
			}

			foreach (Expression e in sizes) {
				e.check(context);
			}

			var calc_sizes = new List<Literal>();
			if (initlist != null) {
				initlist.target_type = new ArrayType(element_type, rank, source_reference);

				if (!initlist.check(context)) {
					error = true;
				}

				var ret = create_sizes_from_initializer_list(context, initlist, rank, calc_sizes);
				if (ret == -1) {
					error = true;
				}
			}

			if (sizes.Count > 0) {
				/* check for errors in the size list */
				foreach (Expression e in sizes) {
					if (e.value_type == null) {
						/* return on previous error */
						return false;
					} else if (!(e.value_type is IntegerType || e.value_type is EnumValueType)) {
						error = true;
						Report.error(e.source_reference, "Expression of integer type expected");
					}
				}
			} else {
				if (initlist == null) {
					error = true;
					/* this is an internal error because it is already handeld by the parser */
					Report.error(source_reference, "internal error: initializer list expected");
				} else {
					foreach (Expression size in calc_sizes) {
						append_size(size);
					}
				}
			}

			if (error) {
				return false;
			}

			/* check for wrong elements inside the initializer */
			if (initializer_list != null && initializer_list.value_type == null) {
				return false;
			}

			/* try to construct the type of the array */
			if (element_type == null) {
				error = true;
				Report.error(source_reference, "Cannot determine the element type of the created array");
				return false;
			}

			value_type = new ArrayType(element_type, rank, source_reference);
			value_type.value_owned = true;

			return !error;
		}

		public override void emit(CodeGenerator codegen) {
			foreach (Expression e in sizes) {
				e.emit(codegen);
			}

			if (initializer_list != null) {
				initializer_list.emit(codegen);
			}

			codegen.visit_array_creation_expression(this);

			codegen.visit_expression(this);
		}

		public override void get_used_variables(ICollection<Variable> collection) {
			foreach (Expression e in sizes) {
				e.get_used_variables(collection);
			}

			if (initializer_list != null) {
				initializer_list.get_used_variables(collection);
			}
		}
	}

}
