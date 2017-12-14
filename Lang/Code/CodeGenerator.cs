using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.CodeNodes;
using Vala.Lang.Parser;

namespace Vala.Lang.Code {
	public abstract class CodeGenerator : CodeVisitor {
		/// <summary>
		/// Generate and emit C code for the specified code context.
		/// 
		/// <param name="context">a code context</param>
		/// </summary>
		public virtual void emit(CodeContext context) {
		}

		public abstract TargetValue load_local(LocalVariable local);

		public abstract void store_local(LocalVariable local, TargetValue value, bool initializer, SourceReference source_reference = null);

		public abstract TargetValue load_parameter(Parameter param);

		public abstract void store_parameter(Parameter param, TargetValue value, bool capturing_parameter = false, SourceReference source_reference = null);

		public abstract TargetValue load_field(Field field, TargetValue instance);

		public abstract void store_field(Field field, TargetValue instance, TargetValue value, SourceReference source_reference = null);
	}
}
