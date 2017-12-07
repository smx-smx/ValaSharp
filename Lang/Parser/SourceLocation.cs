using GLibPorts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Utils;

namespace Vala.Lang.Parser {
	public class SourceLocation : IDisposable {
		public FastMView view;
		public int line;
		public int column;

		public long pos;

		public SourceLocation(FastMView view, long offset, int _line, int _column) {
			this.view = view;
			pos = offset;
			line = _line;
			column = _column;
		}

		public SourceLocation(FastMView view, int _line, int _column) {
			this.view = view;
			pos = view.Position;
			line = _line;
			column = _column;
		}

		public string ReadString(int length) {
			var saved_pos = view.Position;
			view.Position = pos;

			string result = view.ReadString(length);

			view.Position = saved_pos;
			return result;
		}

		public void Dispose() {
			view?.Dispose();
		}
	}
}
