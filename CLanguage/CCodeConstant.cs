using CLanguage.Expressions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GLibPorts;

namespace CLanguage
{
	/**
	 * A constant C expression.
	 */
	public class CCodeConstant : CCodeExpression
	{
		/**
		 * The name of this constant.
		 */
		public string name { get; set; }

		public CCodeConstant(string _name) {
			name = _name;
		}

		const int LINE_LENGTH = 70;

		public static CCodeConstant from_string(string _name) {
			Debug.Assert(_name[0] == '\"');

			CCodeConstant @this = new CCodeConstant(_name);

			if (_name.Length <= LINE_LENGTH) {
				@this.name = _name;
				return @this;
			}

			var builder = new StringBuilder("\"");

			int p = 0;
			long end = _name.Length;

			// remove quotes
			p++;
			end--;

			int col = 0;
			while (p < end) {
				if (col >= LINE_LENGTH) {
					builder.Append("\" \\\n\"");
					col = 0;
				}
				if (_name[p] == '\\') {
					int begin_of_char = p;

					builder.Append(_name[p]);
					builder.Append(_name[p + 1]);
					p += 2;
					switch (_name[p -1]) {
						case 'x':
							// hexadecimal character
							while (p < end && GChar.IsXDigit(_name[p])) {
								builder.Append(_name[p]);
								p++;
							}
							break;
						case '0':
						case '1':
						case '2':
						case '3':
						case '4':
						case '5':
						case '6':
						case '7':
							// octal character
							while (p < end && p - begin_of_char <= 3 && _name[p] >= '0' && _name[p] <= '7') {
								builder.Append(_name[p]);
								p++;
							}
							break;
						case 'n':
							// break line at \n
							col = LINE_LENGTH;
							break;
					}
					col += (int)(p - begin_of_char);
				} else {
					builder.Append(_name[p]);
					p++;
					//p += ((char*)((string)p).next_char() - p);
					col++;
				}
			}

			builder.Append('"');

			@this.name = builder.ToString();
			return @this;
		}

		public override void write(CCodeWriter writer) {
			writer.write_string(name);
		}
	}

}
