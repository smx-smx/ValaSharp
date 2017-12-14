using System;
using Vala.Lang.Parser;

namespace Vala.Lang {
	public enum ReportType {
		Notice,
		Deprecated,
		Experimental,
		Warning,
		Error
	}

	public class ReportEventArgs : EventArgs {
		public readonly SourceReference Source;
		public readonly string Message;
		public readonly ReportType Type;

		public ReportEventArgs(ReportType type, string message, SourceReference source) : base() {
			Type = type;
			Source = source;
			Message = message;
		}
	}
}