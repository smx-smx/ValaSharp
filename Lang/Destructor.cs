using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.CodeNodes;
using Vala.Lang.Parser;
using Vala.Lang.Symbols;

namespace Vala.Lang {
	/// <summary>
	/// Represents a class or instance destructor.
	/// </summary>
	public class Destructor : Subroutine {
		/// <summary>
		/// Specifies the generated `this` parameter for instance methods.
		/// </summary>
		public Parameter this_parameter { get; set; }

		/// <summary>
		/// Specifies whether this is an instance or a class destructor.
		/// </summary>
		public MemberBinding binding { get; set; } = MemberBinding.INSTANCE;

		public override bool has_result {
			get { return false; }
		}

		/// <summary>
		/// Creates a new destructor.
		/// 
		/// <param name="source_reference">reference to source code</param>
		/// <returns>newly created destructor</returns>
		/// </summary>
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
