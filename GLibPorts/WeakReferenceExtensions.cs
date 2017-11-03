using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vala
{
	public static class WeakReferenceExtensions
	{
		public static T GetTarget<T>(this WeakReference<T> @this) where T : class {
			T target;
			if(!@this.TryGetTarget(out target)) {
				//throw new Exception("Failed to get target for WeakReference");
				return null;
			}
			return target;
		}
	}
}
