﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.TypeSymbols;

namespace Vala.Lang.Types
{
	public class IntegerType : ValaValueType
	{
		string literal_value;
		string literal_type_name;

		public IntegerType(Struct type_symbol, string literal_value = null, string literal_type_name = null) : base(type_symbol) {
			this.literal_value = literal_value;
			this.literal_type_name = literal_type_name;
		}

		public override DataType copy() {
			var result = new IntegerType((Struct)type_symbol, literal_value, literal_type_name);
			result.source_reference = source_reference;
			result.value_owned = value_owned;
			result.nullable = nullable;
			return result;
		}

		public override bool compatible(DataType target_type) {
			if (target_type.data_type is Struct && literal_type_name == "int") {
				// int literals are implicitly convertible to integer types
				// of a lower rank if the value of the literal is within
				// the range of the target type
				var target_st = (Struct)target_type.data_type;
				if (target_st.is_integer_type()) {
					var int_attr = target_st.get_attribute("IntegerType");
					if (int_attr != null && int_attr.has_argument("min") && int_attr.has_argument("max")) {
						int val;
						try {
							val = Convert.ToInt32(literal_value);
						}
						catch (Exception) {
							val = Convert.ToInt32(literal_value, 16);
						}
						return (val >= int_attr.get_integer("min") && val <= int_attr.get_integer("max"));
					} else {
						// assume to be compatible if the target type doesn't specify limits
						return true;
					}
				}
			} else if (target_type.data_type is ValaEnum && literal_type_name == "int") {
				// allow implicit conversion from 0 to enum and flags types
				if (int.Parse(literal_value) == 0) {
					return true;
				}
			}

			return base.compatible(target_type);
		}
	}
}
