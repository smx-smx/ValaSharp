using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.CodeNodes;
using Vala.Lang.Methods;
using Vala.Lang.Parser;
using Vala.Lang.Symbols;
using Vala.Lang.Types;

namespace Vala.Lang.TypeSymbols {
	public class ErrorDomain : TypeSymbol {
		private List<ErrorCode> codes = new List<ErrorCode>();
		private List<Method> methods = new List<Method>();

		/// <summary>
		/// Creates a new error domain.
		/// 
		/// <param name="name">type name</param>
		/// <param name="source_reference">reference to source code</param>
		/// <returns>newly created error domain</returns>
		/// </summary>
		public ErrorDomain(string name, SourceReference source_reference = null, Comment comment = null) : base(name, source_reference, comment) {
		}

		/// <summary>
		/// Appends the specified code to the list of error codes.
		/// 
		/// <param name="ecode">an error code</param>
		/// </summary>
		public void add_code(ErrorCode ecode) {
			codes.Add(ecode);
			scope.add(ecode.name, ecode);
		}

		/// <summary>
		/// Adds the specified method as a member to this error domain.
		/// 
		/// <param name="m">a method</param>
		/// </summary>
		public override void add_method(Method m) {
			if (m is CreationMethod) {
				Report.error(m.source_reference, "construction methods may only be declared within classes and structs");

				m.error = true;
				return;
			}
			if (m.binding == MemberBinding.INSTANCE) {
				m.this_parameter = new Parameter("this", new ErrorType(this, null));
				m.scope.add(m.this_parameter.name, m.this_parameter);
			}

			methods.Add(m);
			scope.add(m.name, m);
		}

		/// <summary>
		/// Returns a copy of the list of error codes.
		/// 
		/// <returns>list of error codes</returns>
		/// </summary>
		public List<ErrorCode> get_codes() {
			return codes;
		}

		/// <summary>
		/// Returns a copy of the list of methods.
		/// 
		/// <returns>list of methods</returns>
		/// </summary>
		public List<Method> get_methods() {
			return methods;
		}

		public override void accept(CodeVisitor visitor) {
			visitor.visit_error_domain(this);
		}

		public override void accept_children(CodeVisitor visitor) {
			foreach (ErrorCode ecode in codes) {
				ecode.accept(visitor);
			}

			foreach (Method m in methods) {
				m.accept(visitor);
			}
		}

		public override bool is_reference_type() {
			return false;
		}

		public override bool check(CodeContext context) {
			if (is_checked) {
				return !error;
			}

			is_checked = true;

			foreach (ErrorCode ecode in codes) {
				ecode.check(context);
			}

			foreach (Method m in methods) {
				m.check(context);
			}

			return !error;
		}
	}
}
