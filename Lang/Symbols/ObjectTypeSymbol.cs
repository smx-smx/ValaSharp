using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.Methods;
using Vala.Lang.Parser;
using Vala.Lang.Types;

namespace Vala.Lang.Symbols {
	public abstract class ObjectTypeSymbol : TypeSymbol {
		private List<TypeParameter> type_parameters = new List<TypeParameter>();

		public ObjectTypeSymbol(string name, SourceReference source_reference = null, Comment comment = null) : base(name, source_reference, comment) {
		}

		public abstract List<Method> get_methods();
		public abstract List<Signal> get_signals();
		public abstract List<Property> get_properties();

		/**
		 * Appends the specified parameter to the list of type parameters.
		 *
		 * @param p a type parameter
		 */
		public void add_type_parameter(TypeParameter p) {
			type_parameters.Add(p);
			scope.add(p.name, p);
		}

		/**
		 * Returns a copy of the type parameter list.
		 *
		 * @return list of type parameters
		 */
		public List<TypeParameter> get_type_parameters() {
			return type_parameters;
		}

		public override int get_type_parameter_index(string name) {
			int i = 0;
			foreach (TypeParameter parameter in type_parameters) {
				if (parameter.name == name) {
					return i;
				}
				i++;
			}
			return -1;
		}

		public ObjectType get_this_type() {
			var result = new ObjectType(this);
			foreach (var type_parameter in get_type_parameters()) {
				var type_arg = new GenericType(type_parameter);
				type_arg.value_owned = true;
				result.add_type_argument(type_arg);
			}
			return result;
		}

		/**
		 * Adds the specified method as a hidden member to this class,
		 * primarily used for default signal handlers.
		 *
		 * The hidden methods are not part of the `methods` collection.
		 *
		 * There may also be other use cases, eg, convert array.resize() to
		 * this type of method?
		 *
		 * @param m a method
		 */
		public void add_hidden_method(Method m) {
			if (m.binding == MemberBinding.INSTANCE) {
				if (m.this_parameter != null) {
					m.scope.remove(m.this_parameter.name);
				}
				m.this_parameter = new Parameter("this", get_this_type());
				m.scope.add(m.this_parameter.name, m.this_parameter);
			}
			if (!(m.return_type is VoidType) && m.get_postconditions().Count > 0) {
				if (m.result_var != null) {
					m.scope.remove(m.result_var.name);
				}
				m.result_var = new LocalVariable(m.return_type.copy(), "result");
				m.result_var.is_result = true;
			}

			scope.add(null, m);
		}
	}
}
