using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vala.Lang {
	/// <summary>
	/// Represents a lockable object.
	/// </summary>
	public interface Lockable {
		/// <summary>
		/// Indicates a specific lockable object beeing actually locked somewhere.
		/// </summary>
		bool get_lock_used();

		/// <summary>
		/// Set this lockable object as beeing locked somewhere.
		/// </summary>
		void set_lock_used(bool used);
	}
}
