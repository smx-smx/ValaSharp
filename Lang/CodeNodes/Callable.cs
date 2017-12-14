using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.Types;

namespace Vala.Lang.CodeNodes {
	public interface Callable {
		/// <summary>
		/// The return type of this callable.
		/// </summary>
		DataType return_type { get; set; }

		/// <summary>
		/// Appends parameter to this callable.
		/// 
		/// <param name="param">a formal parameter</param>
		/// </summary>
		void add_parameter(Parameter param);

		/// <summary>
		/// Returns the parameter list.
		/// </summary>
		List<Parameter> get_parameters();
	}
}
