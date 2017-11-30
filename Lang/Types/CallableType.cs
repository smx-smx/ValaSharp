using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.Symbols;

namespace Vala.Lang.Types {
	public abstract class CallableType : DataType {
		public override string to_prototype_string(string override_name = null) {
			StringBuilder builder = new StringBuilder();

			// Append return-type
			var return_type = get_return_type();
			if (return_type.is_weak()) {
				builder.Append("unowned ");
			}
			builder.Append(return_type.to_qualified_string());

			// Append name
			builder.Append(' ');
			builder.Append(override_name ?? this.to_string());
			builder.Append(' ');

			// Append parameter-list
			builder.Append('(');
			int i = 1;
			// add sender parameter for internal signal-delegates
			var delegate_type = this as DelegateType;
			if (delegate_type != null) {
				var delegate_symbol = delegate_type.delegate_symbol;
				if (delegate_symbol.parent_symbol is Signal && delegate_symbol.sender_type != null) {
					builder.Append(delegate_symbol.sender_type.to_qualified_string());
					i++;
				}
			}
			foreach (Parameter param in get_parameters()) {
				if (i > 1) {
					builder.Append(", ");
				}

				if (param.ellipsis) {
					builder.Append("...");
					continue;
				}

				if (param.direction == ParameterDirection.IN) {
					if (param.variable_type.value_owned) {
						builder.Append("owned ");
					}
				} else {
					if (param.direction == ParameterDirection.REF) {
						builder.Append("ref ");
					} else if (param.direction == ParameterDirection.OUT) {
						builder.Append("out ");
					}
					if (!param.variable_type.value_owned && param.variable_type is ReferenceType) {
						builder.Append("weak ");
					}
				}

				builder.Append(param.variable_type.to_qualified_string());

				if (param.initializer != null) {
					builder.Append(" = ");
					builder.Append(param.initializer.to_string());
				}

				i++;
			}
			builder.Append(')');

			// Append error-types
			var error_types = get_error_types();
			if (error_types.Count > 0) {
				builder.Append(" throws ");

				bool first = true;
				foreach (DataType type in error_types) {
					if (!first) {
						builder.Append(", ");
					} else {
						first = false;
					}

					builder.Append(type.to_string());
				}
			}

			return builder.ToString();
		}
	}
}
