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

namespace Vala.Lang.TypeSymbols
{
	public class ErrorDomain : TypeSymbol
	{
		private List<ErrorCode> codes = new List<ErrorCode>();
		private List<Method> methods = new List<Method>();

		/**
		 * Creates a new error domain.
		 *
		 * @param name             type name
		 * @param source_reference reference to source code
		 * @return                 newly created error domain
		 */
		public ErrorDomain(string name, SourceReference source_reference = null, Comment comment = null) : base(name, source_reference, comment) {
		}

		/**
		 * Appends the specified code to the list of error codes.
		 *
		 * @param ecode an error code
		 */
		public void add_code(ErrorCode ecode) {
			codes.Add(ecode);
			scope.add(ecode.name, ecode);
		}

		/**
		 * Adds the specified method as a member to this error domain.
		 *
		 * @param m a method
		 */
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

		/**
		 * Returns a copy of the list of error codes.
		 *
		 * @return list of error codes
		 */
		public List<ErrorCode> get_codes() {
			return codes;
		}

		/**
		 * Returns a copy of the list of methods.
		 *
		 * @return list of methods
		 */
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
