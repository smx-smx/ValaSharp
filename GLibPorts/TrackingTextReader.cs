using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLibPorts {
	public class TrackingTextReader : TextReader {
		private StringReader _baseReader;
		private int _position;

		public TrackingTextReader(StringReader baseReader) {
			_baseReader = baseReader;
		}

		public override int Read() {
			_position++;
			return _baseReader.Read();
		}

		public override int Peek() {
			return _baseReader.Peek();
		}

		public int Position {
			get { return _position; }
		}
	}
}
