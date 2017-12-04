using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.CodeNodes;
using Vala.Lang.Parser;
using Vala.Lang.Symbols;

namespace Vala.Lang {
	/**
	 * Represents a class or instance destructor.
	 */
	public class Destructor : Subroutine {
		/**
		 * Specifies the generated `this` parameter for instance methods.
		 */
		public Parameter this_parameter { get; set; }

		/**
		 * Specifies whether this is an instance or a class destructor.
		 */
		public MemberBinding binding { get; set; } = MemberBinding.INSTANCE;

		public override bool has_result {
			get { return false; }
		}

		/**
		 * Creates a new destructor.
		 *
		 * @param source_reference reference to source code
		 * @return                 newly created destructor
		 */
		public Destructor(SourceReference source_reference = null) : base(null, source_reference) { }

		public override void accept(CodeVisitor visitor) {
			visitor.visit_destructor(this);
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

			owner = context.analyzer.current_symbol.scope;
			context.analyzer.current_symbol = this;

			if (body != null) {
				body.check(context);
			}

			context.analyzer.current_symbol = context.analyzer.current_symbol.parent_symbol;

			return !error;
		}
	}

}
