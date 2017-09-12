using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.Types;

namespace Vala.Lang.CodeNodes
{
	public interface Callable
	{
		/**
	 * The return type of this callable.
	 */
		DataType return_type { get; set; }

		/**
		 * Appends parameter to this callable.
		 *
		 * @param param a formal parameter
		 */
		void add_parameter(Parameter param);

		/**
		 * Returns the parameter list.
		 */
		List<Parameter> get_parameters();
	}
}
