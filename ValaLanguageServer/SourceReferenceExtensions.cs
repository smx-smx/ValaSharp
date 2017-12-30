using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.Parser;
using LanguageServer.Parameters;

namespace ValaLanguageServer {
	public static class SourceReferenceExtensions {
		public static bool MatchesPosition(this SourceReference @this, Position pos) {
			return
				@this.begin.column == (pos.character + 1) &&
				@this.begin.line == (pos.line + 1);
		}

		public static bool BetweenPosition(this SourceReference @this, Position pos) {
			var adjCol = pos.character + 1;
			var adjRow = pos.line + 1;

			return
				(adjRow >= @this.begin.line && adjRow <= @this.end.line) &&
				(adjCol >= @this.begin.column && adjCol <= @this.end.column);

		}
	}
}
