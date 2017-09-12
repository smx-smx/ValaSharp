using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GLibPorts.Native;

namespace Vala
{
	public static class StringExtensions
	{
		public static string printf(this String format, params VariableArgument[] args) {
			if (!args.Any())
				return format;

			using (var combinedVariables = new CombinedVariables(args)) {
				var bufferCapacity = Win32._vscprintf(format, combinedVariables.GetPtr());
				var stringBuilder = new StringBuilder(bufferCapacity + 1);

				Win32.vsprintf(stringBuilder, format, combinedVariables.GetPtr());

				return stringBuilder.ToString();
			}
		}

		/*
		public static int scanf(this String input, string format, out VariableArgument[] out_args, params VariableArgument[] args) {
			out_args = new VariableArgument[args.Length];

			if (!args.Any())
				return 0;

			using (var combinedVariables = new CombinedVariables(args)) {
				int count = Win32.vscanf_s(format, combinedVariables.GetPtr());

				args.CopyTo(out_args, 0);
				return count;
			}
		}
		*/

		public static string escape(this String @this, string exceptions) {
			char[] find = new char[] { '\b', '\f', '\n', '\r', '\t', '\v', '\\' };
			foreach (char ch in find) {
				string chs = ch.ToString();
				@this.Replace(chs, "\\" + chs);
			}
			return @this;
		}

		public static string compress(this String @this) {
			StringBuilder sb = new StringBuilder();
			bool unescapeNext = false;
			foreach (char ch in @this) {
				if (ch == '\\') {
					unescapeNext = true;
					continue;
				} else {
					if (unescapeNext) {
						switch (ch) {
							case '0':
							case '1':
							case '2':
							case '3':
							case '4':
							case '5':
							case '6':
							case '7':
								throw new NotSupportedException();
							case 'b':
								sb.Append('\b');
								break;
							case 'f':
								sb.Append('\f');
								break;
							case 'n':
								sb.Append('\n');
								break;
							case 'r':
								sb.Append('\r');
								break;
							case 't':
								sb.Append('\t');
								break;
							case 'v':
								sb.Append('\v');
								break;
							default:            /* Also handles \" and \\ */
								sb.Append(ch);
								break;
						}
					}
					unescapeNext = false;
				}
			}
			return sb.ToString();
		}

		public static bool validate(this String @this) {
			try {
				byte[] bytes = Encoding.UTF8.GetBytes(@this);
				return Encoding.UTF8.GetString(bytes) == @this;
			} catch (Exception) {
				return false;
			}
		}

		public static void canon(this String @this, string valid_chars, char substitutor) {
			char[] validChars = valid_chars.ToCharArray();
			StringBuilder sb = new StringBuilder(@this);
			for (int i = 0; i < sb.Length; i++) {
				if (!validChars.Contains(sb[i]))
					sb[i] = substitutor;
			}
			@this = sb.ToString();
		}
	}
}
