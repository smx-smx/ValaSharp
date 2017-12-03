using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.Methods;
using Vala.Lang.Symbols;

namespace Vala.Lang.Types
{
	/**
	 * The type of a method referencea.
	 */
	public class MethodType : DataType
	{
		public Method method_symbol { get; set; }

		public MethodType(Method method_symbol) {
			this.method_symbol = method_symbol;
		}

		public override bool is_invokable() {
			return true;
		}

		public override DataType get_return_type() {
			return method_symbol.return_type;
		}

		public override List<Parameter> get_parameters() {
			return method_symbol.get_parameters();
		}

		public override DataType copy() {
			return new MethodType(method_symbol);
		}

		public override bool compatible(DataType target_type) {
			var dt = target_type as DelegateType;
			if (dt == null) {
				// method types incompatible to anything but delegates
				return false;
			}

			return dt.delegate_symbol.matches_method(method_symbol, dt);
		}

		public override string to_qualified_string(Scope scope) {
			return method_symbol.get_full_name();
		}

		public override Symbol get_member(string member_name) {
			if (method_symbol.coroutine && member_name == "begin") {
				return method_symbol;
			} else if (method_symbol.coroutine && member_name == "end") {
				return method_symbol;
			} else if (method_symbol.coroutine && member_name == "callback") {
				return method_symbol.get_callback_method();
			}
			return null;
		}

		public string to_prototype_string(bool with_type_parameters = false) {
			var proto = "%s %s (".printf(get_return_type().to_string(), this.to_string());

			int i = 1;
			foreach (Parameter param in get_parameters()) {
				if (i > 1) {
					proto += ", ";
				}

				if (param.ellipsis) {
					proto += "...";
					continue;
				}

				if (param.direction == ParameterDirection.IN) {
					if (param.variable_type.value_owned) {
						proto += "owned ";
					}
				} else {
					if (param.direction == ParameterDirection.REF) {
						proto += "ref ";
					} else if (param.direction == ParameterDirection.OUT) {
						proto += "out ";
					}
					if (param.variable_type.is_weak()) {
						proto += "unowned ";
					}
				}

				proto = "%s%s %s".printf(proto, param.variable_type.to_qualified_string(), param.name);

				if (param.initializer != null) {
					proto = "%s = %s".printf(proto, param.initializer.to_string());
				}

				i++;
			}

			return proto + ")";
		}
	}
}
