using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.Parser;

namespace Vala.Lang
{
	/**
	 * Simple reader for a subset of XML.
	 */
	public class MarkupReader
	{
		public string filename { get; private set; }

		public string name { get; private set; }

		public string content { get; private set; }

		MemoryMappedFile mapped_file;
		private MemoryStream stream;

		long begin;
		long current;
		long end;

		int line;
		int column;

		Dictionary<string, string> attributes = new Dictionary<string, string>();
		bool empty_element;

		public MarkupReader(string filename) {
			this.filename = filename;

			try {
				mapped_file = MemoryMappedFile.OpenExisting(filename);
				begin = 0;
				end = mapped_file.CreateViewStream().Length;

				current = begin;

				line = 1;
				column = 1;
			} catch (Exception e) {
				Report.error(null, "Unable to map file `%s': %s".printf(filename, e.Message));
			}
		}

		public string get_attribute(string attr) {
			return attributes[attr];
		}

		/*
		 * Returns a copy of the current attributes.
		 *
		 * @return map of current attributes
		 */
		public Dictionary<string, string> get_attributes() {
			var result = new Dictionary<string, string>();
			foreach (var key in attributes.Keys) {
				result[key] = attributes[key];
			}
			return result;
		}

		string read_name() {
			long begin = current;
			while (current < end) {
				if (current[0] == ' ' || current[0] == '\t' || current[0] == '>'
					|| current[0] == '/' || current[0] == '=' || current[0] == '\n') {
					break;
				}
				char u = ((string)current).get_char_validated((long)(end - current));
				if (u != (char)(-1)) {
					current += u.to_utf8(null);
				} else {
					Report.error(null, "invalid UTF-8 character");
				}
			}
			if (current == begin) {
				// syntax error: invalid name
			}
			return ((string)begin).Substring(0, (int)(current - begin));
		}

		public MarkupTokenType read_token(out SourceLocation token_begin, out SourceLocation token_end) {
			attributes.Clear();

			if (empty_element) {
				empty_element = false;
				token_begin = SourceLocation(begin, line, column);
				token_end = SourceLocation(begin, line, column);
				return MarkupTokenType.END_ELEMENT;
			}

			space();

			MarkupTokenType type = MarkupTokenType.NONE;
			char* begin = current;
			token_begin = SourceLocation(begin, line, column);

			if (current >= end) {
				type = MarkupTokenType.EOF;
			} else if (current[0] == '<') {
				current++;
				if (current >= end) {
					// error
				} else if (current[0] == '?') {
					// processing instruction
				} else if (current[0] == '!') {
					// comment or doctype
					current++;
					if (current < end - 1 && current[0] == '-' && current[1] == '-') {
						// comment
						current += 2;
						while (current < end - 2) {
							if (current[0] == '-' && current[1] == '-' && current[2] == '>') {
								// end of comment
								current += 3;
								break;
							} else if (current[0] == '\n') {
								line++;
								column = 0;
							}
							current++;
						}

						// ignore comment, read next token
						return read_token(out token_begin, out token_end);
					}
				} else if (current[0] == '/') {
					type = MarkupTokenType.END_ELEMENT;
					current++;
					name = read_name();
					if (current >= end || current[0] != '>') {
						// error
					}
					current++;
				} else {
					type = MarkupTokenType.START_ELEMENT;
					name = read_name();
					space();
					while (current < end && current[0] != '>' && current[0] != '/') {
						string attr_name = read_name();
						if (current >= end || current[0] != '=') {
							// error
						}
						current++;
						if (current >= end || current[0] != '"' || current[0] != '\'') {
							// error
						}
						char quote = current[0];
						current++;

						string attr_value = text(quote, false);

						if (current >= end || current[0] != quote) {
							// error
						}
						current++;
						attributes[attr_name] = attr_value;
						space();
					}
					if (current[0] == '/') {
						empty_element = true;
						current++;
						space();
					} else {
						empty_element = false;
					}
					if (current >= end || current[0] != '>') {
						// error
					}
					current++;
				}
			} else {
				space();

				if (current[0] != '<') {
					content = text('<', true);
				} else {
					// no text
					// read next token
					return read_token(out token_begin, out token_end);
				}

				type = MarkupTokenType.TEXT;
			}

			token_end = new SourceLocation(current, line, column - 1);

			return type;
		}

		public MarkupTokenType read_token() {
			SourceLocation begin;
			SourceLocation end;
			return read_token(out begin, out end);
		}

		string text(char end_char, bool rm_trailing_whitespace) {
			StringBuilder content = new StringBuilder();
			long text_begin = current;
			long last_linebreak = current;

			while (current < end && current[0] != end_char) {
				char u = ((string)current).get_char_validated((long)(end - current));
				if (u == (char)(-1)) {
					Report.error(null, "invalid UTF-8 character");
				} else if (u == '&') {
					char* next_pos = current + u.to_utf8(null);
					if (((string)next_pos).has_prefix("amp;")) {
						content.append(((string)text_begin).substring(0, (int)(current - text_begin)));
						content.append_c('&');
						current += 5;
						text_begin = current;
					} else if (((string)next_pos).has_prefix("quot;")) {
						content.append(((string)text_begin).substring(0, (int)(current - text_begin)));
						content.append_c('"');
						current += 6;
						text_begin = current;
					} else if (((string)next_pos).has_prefix("apos;")) {
						content.append(((string)text_begin).substring(0, (int)(current - text_begin)));
						content.append_c('\'');
						current += 6;
						text_begin = current;
					} else if (((string)next_pos).has_prefix("lt;")) {
						content.append(((string)text_begin).substring(0, (int)(current - text_begin)));
						content.append_c('<');
						current += 4;
						text_begin = current;
					} else if (((string)next_pos).has_prefix("gt;")) {
						content.append(((string)text_begin).substring(0, (int)(current - text_begin)));
						content.append_c('>');
						current += 4;
						text_begin = current;
					} else {
						current += u.to_utf8(null);
					}
				} else {
					if (u == '\n') {
						line++;
						column = 0;
						last_linebreak = current;
					}

					current += u.to_utf8(null);
					column++;
				}
			}

			if (text_begin != current) {
				content.append(((string)text_begin).substring(0, (int)(current - text_begin)));
			}

			column += (int)(current - last_linebreak);

			// Removes trailing whitespace
			if (rm_trailing_whitespace) {
				char* str_pos = ((char*)content.str) + content.len;
				for (str_pos--; str_pos > ((char*)content.str) && str_pos[0].isspace(); str_pos--) ;
				content.erase((ssize_t)(str_pos - ((char*)content.str) + 1), -1);
			}

			return content.str;
		}

		void space() {
			while (current < end && current[0].isspace()) {
				if (current[0] == '\n') {
					line++;
					column = 0;
				}
				current++;
				column++;
			}
		}
	}

	public enum MarkupTokenType
	{
		NONE,
		START_ELEMENT,
		END_ELEMENT,
		TEXT,
		EOF
	}

	public static class MarkupTokenTypeExtensions
	{
		public static string ToString(this MarkupTokenType @this) {
			switch (@this) {
				case MarkupTokenType.START_ELEMENT: return "start element";
				case MarkupTokenType.END_ELEMENT: return "end element";
				case MarkupTokenType.TEXT: return "text";
				case MarkupTokenType.EOF: return "end of file";
				default: return "unknown token type";
			}
		}
	}
}
