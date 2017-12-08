using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.CodeNodes;
using Vala.Lang.Parser;
using Vala.Lang.Symbols;
using Vala.Lang.Types;

namespace Vala.Lang {
	/**
	 * Represents a class or instance constructor.
	 */
	public class Constructor : Subroutine {
		/**
		 * Specifies the generated `this` parameter for instance methods.
		 */
		public Parameter this_parameter { get; set; }

		/**
		 * Specifies whether this is an instance or a class constructor.
		 */
		public MemberBinding binding { get; set; } = MemberBinding.INSTANCE;

		public override bool has_result {
			get { return false; }
		}

		/**
		 * Creates a new constructor.
		 *
		 * @param source reference to source code
		 * @return       newly created constructor
		 */
		public Constructor(SourceReference source) : base(null, source) { }

		public override void accept(CodeVisitor visitor) {
			visitor.visit_constructor(this);
		}

		public override void accept_children(CodeVisitor visitor) {
			if (body != null) {
				body.accept(visitor);
			}
		}

		public override bool check(CodeContext context) {
			if (is_checked) {
				return !error;
			}

			is_checked = true;

			this_parameter = new Parameter("this", new ObjectType(context.analyzer.current_class));
			scope.add(this_parameter.name, this_parameter);

			owner = context.analyzer.current_symbol.scope;
			context.analyzer.current_symbol = this;

			if (body != null) {
				body.check(context);
			}

			foreach (DataType body_error_type in body.get_error_types()) {
				if (!((ErrorType)body_error_type).dynamic_error) {
					Report.warning(body_error_type.source_reference, "unhandled error `%s'".printf(body_error_type.ToString()));
				}
			}

			context.analyzer.current_symbol = context.analyzer.current_symbol.parent_symbol;

			return !error;
		}
	}

}
