using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GLibPorts.GLib;

namespace CLanguage {
	/// <summary>
	/// Represents a section that should be processed on condition.
	/// </summary>
	public class CCodeGGnucSection : CCodeFragment {
		/// <summary>
		/// The expression
		/// </summary>
		public GGnucSectionType section_type { get; set; }

		public CCodeGGnucSection(GGnucSectionType t) {
			section_type = t;
		}

		public override void write(CCodeWriter writer) {
			writer.write_string("G_GNUC_BEGIN_");
			writer.write_string(section_type.ToString());
			writer.write_newline();
			foreach (CCodeNode node in get_children()) {
				node.write_combined(writer);
			}
			writer.write_string("G_GNUC_END_");
			writer.write_string(section_type.ToString());
			writer.write_newline();
		}

		public override void write_declaration(CCodeWriter writer) {
		}
	}

	public enum GGnucSectionType {
		IGNORE_DEPRECATIONS
	}

	public static class GGnucSectionTypeExtensions {
		public static string ToString(this GGnucSectionType @this) {
			switch (@this) {
			case GGnucSectionType.IGNORE_DEPRECATIONS:
				return "IGNORE_DEPRECATIONS";
			default:
				assert_not_reached();
				return null;
			}
		}
	}
}
