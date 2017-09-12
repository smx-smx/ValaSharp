using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vala.Lang
{
	/**
	 * Represents a lockable object.
	 */
	public interface Lockable {
		/**
		 * Indicates a specific lockable object beeing actually locked somewhere.
		 */
		bool get_lock_used();

		/**
		 * Set this lockable object as beeing locked somewhere.
		 */
		void set_lock_used(bool used);
	}
}
