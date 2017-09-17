using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GLibPorts.GLib;

namespace Vala.Lang.Symbols
{
	public enum SymbolAccessibility
	{
		PRIVATE,
		INTERNAL,
		PROTECTED,
		PUBLIC
	}

	public static class SymbolAccessibilityExtensions
	{
		public static string ToString(this SymbolAccessibility @this) {
			switch (@this) {
				case SymbolAccessibility.PROTECTED:
					return "protected";

				case SymbolAccessibility.INTERNAL:
					return "internal";

				case SymbolAccessibility.PRIVATE:
					return "private";

				case SymbolAccessibility.PUBLIC:
					return "public";
			}
			assert_not_reached();
			return null;
		}
	}
}
