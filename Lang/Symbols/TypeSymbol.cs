﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.Parser;

namespace Vala.Lang.Symbols {
	public abstract class TypeSymbol : Symbol {
		public TypeSymbol(string name, SourceReference source_reference = null, Comment comment = null) : base(name, source_reference, comment) {
		}

		/// <summary>
		/// Checks whether this data type has value or reference type semantics.
		/// 
		/// <returns>true if this data type has reference type semantics</returns>
		/// </summary>
		public virtual bool is_reference_type() {
			return false;
		}

		/// <summary>
		/// Checks whether this data type is equal to or a subtype of the
		/// specified data type.
		/// 
		/// <param name="t">a data type</param>
		/// <returns>true if t is a supertype of this data type, false otherwise</returns>
		/// </summary>
		public virtual bool is_subtype_of(TypeSymbol t) {
			return (this == t);
		}

		/// <summary>
		/// Return the index of the specified type parameter name.
		/// </summary>
		public virtual int get_type_parameter_index(string name) {
			return -1;
		}
	}
}
