using GLibPorts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.Types;

namespace Vala.Lang.Parser
{
	/**
	 * Lexical scanner for Vala source files.
	 */
	public class Scanner
	{
		public SourceFile source_file { get; private set; }

		TokenType previous;
		MemoryStream current;

		long begin = 0;
		long end = 0;

		int line;
		int column;

		Comment _comment;

		List<Conditional> conditional_stack = new List<Conditional>();

		struct Conditional
		{
			public bool matched;
			public bool else_found;
			public bool skip_section;
		}

		List<State> state_stack = new List<State>();

		enum State
		{
			PARENS,
			BRACE,
			BRACKET,
			TEMPLATE,
			TEMPLATE_PART,
			REGEX_LITERAL
		}

		public Scanner(SourceFile source_file) {
			this.source_file = source_file;

			begin = 0;
			byte[] contents = source_file.get_mapped_contents(out end);
			MemoryStream mem = new MemoryStream(contents);

			current = mem;

			line = 1;
			column = 1;
		}

		public void seek(SourceLocation location)
		{
			current.Seek(location.pos, SeekOrigin.Begin);
			line = location.line;
			column = location.column;

			conditional_stack = new List<Conditional>();
			state_stack = new List<State>();
		}

		bool in_template() {
			return (state_stack.Count > 0 && state_stack[state_stack.Count - 1] == State.TEMPLATE);
		}

		bool in_template_part() {
			return (state_stack.Count > 0 && state_stack[state_stack.Count - 1] == State.TEMPLATE_PART);
		}

		bool in_regex_literal() {
			return (state_stack.Count > 0 && state_stack[state_stack.Count - 1] == State.REGEX_LITERAL);
		}

		bool is_ident_char(char c) {
			return (Char.IsLetterOrDigit(c) || c == '_');
		}

		SourceReference get_source_reference(int offset, long length = 0) {
			return new SourceReference(source_file,
				new SourceLocation(current, line, column + offset),
				new SourceLocation(current, current.Position + length, line, (int)(column + offset + length))
			);
		}

		public TokenType read_regex_token(out SourceLocation token_begin, out SourceLocation token_end) {
			TokenType type;
			MemoryStream begin = current.Clone();
			token_begin = new SourceLocation(current, current.Position, line, column);

			int token_length_in_chars = -1;

			if (begin.Position >= end) {
				type = TokenType.EOF;
			} else {
				switch (current.PeekChar()) {
					case '/':
						type = TokenType.CLOSE_REGEX_LITERAL;
						current.Position++;
						state_stack.RemoveAt(state_stack.Count - 1);
						var fl_i = false;
						var fl_s = false;
						var fl_m = false;
						var fl_x = false;
						while (current.PeekChar() == 'i' || current.PeekChar() == 's' || current.PeekChar() == 'm' || current.PeekChar() == 'x') {
							switch (current.PeekChar()) {
								case 'i':
									if (fl_i) {
										Report.error(get_source_reference(token_length_in_chars), "modifier 'i' used more than once");
									}
									fl_i = true;
									break;
								case 's':
									if (fl_s) {
										Report.error(get_source_reference(token_length_in_chars), "modifier 's' used more than once");
									}
									fl_s = true;
									break;
								case 'm':
									if (fl_m) {
										Report.error(get_source_reference(token_length_in_chars), "modifier 'm' used more than once");
									}
									fl_m = true;
									break;
								case 'x':
									if (fl_x) {
										Report.error(get_source_reference(token_length_in_chars), "modifier 'x' used more than once");
									}
									fl_x = true;
									break;
							}
							current.Position++;
							token_length_in_chars++;
						}
						break;
					default:
						type = TokenType.REGEX_LITERAL;
						token_length_in_chars = 0;
						while (current.Position < end && current.PeekChar() != '/') {
							if (current.PeekChar() == '\\') {
								current.Position++;
								token_length_in_chars++;
								if (current.Position >= end) {
									break;
								}

								switch (current.PeekChar()) {
									case '\'':
									case '"':
									case '\\':
									case '/':
									case '^':
									case '$':
									case '.':
									case '[':
									case ']':
									case '{':
									case '}':
									case '(':
									case ')':
									case '?':
									case '*':
									case '+':
									case '-':
									case '#':
									case '&':
									case '~':
									case ':':
									case ';':
									case '<':
									case '>':
									case '|':
									case '%':
									case '=':
									case '@':
									case '0':
									case 'b':
									case 'B':
									case 'f':
									case 'n':
									case 'r':
									case 't':
									case 'v':
									case 'a':
									case 'A':
									case 'p':
									case 'P':
									case 'e':
									case 'd':
									case 'D':
									case 's':
									case 'S':
									case 'w':
									case 'W':
									case 'G':
									case 'z':
									case 'Z':
										current.Position++;
										token_length_in_chars++;
										break;
									case 'u':
										// u escape character has four hex digits
										current.Position++;
										token_length_in_chars++;
										int digit_length;
										for (digit_length = 0; digit_length < 4 && current.Position < end && GChar.IsXDigit(current.PeekChar()); digit_length++) {
											current.Position++;
											token_length_in_chars++;
										}
										if (digit_length != 4) {
											Report.error(get_source_reference(token_length_in_chars), "\\u requires four hex digits");
										}
										break;
									case 'x':
										// hexadecimal escape character requires two hex digits
										current.Position++;
										token_length_in_chars++;
										int _digit_length;
										for (_digit_length = 0; current.Position < end && GChar.IsXDigit(current.PeekChar()); _digit_length++) {
											current.Position++;
											token_length_in_chars++;
										}
										if (_digit_length < 1) {
											Report.error(get_source_reference(token_length_in_chars), "\\x requires at least one hex digit");
										}
										break;
									default:
										Report.error(get_source_reference(token_length_in_chars), "invalid escape sequence");
										break;
								}
							} else if (current.PeekChar() == '\n') {
								break;
							} else {
								char u = current.PeekCharAt((int)(end - current.Position));
								//TODO: check unicode valid character
								current.Position++;
								token_length_in_chars++;

								/*
								char u = ((string)current).get_char_validated((long)(end - current));
								if (u != (char)(-1)) {
									current.Position += u.to_utf8(null);
									token_length_in_chars++;
								} else {
									current.Position++;
									Report.error(get_source_reference(token_length_in_chars), "invalid UTF-8 character");
								}*/
							}
						}
						if (current.Position >= end || current.PeekChar() == '\n') {
							Report.error(get_source_reference(token_length_in_chars), "syntax error, expected \"");
							state_stack.RemoveAt(state_stack.Count - 1);
							return read_token(out token_begin, out token_end);
						}
						break;
				}
			}

			if (token_length_in_chars < 0) {
				column += (int)(current.Position - begin.Position);
			} else {
				column += token_length_in_chars;
			}

			token_end = new SourceLocation(current, line, column - 1);

			return type;
		}

		public static TokenType get_identifier_or_keyword(MemoryStream begin, int len) {
			switch (len) {
				case 2:
					switch (begin.PeekChar()) {
						case 'a':
							if (matches(begin, "as")) return TokenType.AS;
							break;
						case 'd':
							if (matches(begin, "do")) return TokenType.DO;
							break;
						case 'i':
							switch (begin.PeekCharAt(1)) {
								case 'f':
									return TokenType.IF;
								case 'n':
									return TokenType.IN;
								case 's':
									return TokenType.IS;
							}
							break;
					}
					break;
				case 3:
					switch (begin.PeekChar()) {
						case 'f':
							if (matches(begin, "for")) return TokenType.FOR;
							break;
						case 'g':
							if (matches(begin, "get")) return TokenType.GET;
							break;
						case 'n':
							if (matches(begin, "new")) return TokenType.NEW;
							break;
						case 'o':
							if (matches(begin, "out")) return TokenType.OUT;
							break;
						case 'r':
							if (matches(begin, "ref")) return TokenType.REF;
							break;
						case 's':
							if (matches(begin, "set")) return TokenType.SET;
							break;
						case 't':
							if (matches(begin, "try")) return TokenType.TRY;
							break;
						case 'v':
							if (matches(begin, "var")) return TokenType.VAR;
							break;
					}
					break;
				case 4:
					switch (begin.PeekChar()) {
						case 'b':
							if (matches(begin, "base")) return TokenType.BASE;
							break;
						case 'c':
							if (matches(begin, "case")) return TokenType.CASE;
							break;
						case 'e':
							switch (begin.PeekCharAt(1)) {
								case 'l':
									if (matches(begin, "else")) return TokenType.ELSE;
									break;
								case 'n':
									if (matches(begin, "enum")) return TokenType.ENUM;
									break;
							}
							break;
						case 'l':
							if (matches(begin, "lock")) return TokenType.LOCK;
							break;
						case 'n':
							if (matches(begin, "null")) return TokenType.NULL;
							break;
						case 't':
							switch (begin.PeekCharAt(1)) {
								case 'h':
									if (matches(begin, "this")) return TokenType.THIS;
									break;
								case 'r':
									if (matches(begin, "true")) return TokenType.TRUE;
									break;
							}
							break;
						case 'v':
							if (matches(begin, "void")) return TokenType.VOID;
							break;
						case 'w':
							if (matches(begin, "weak")) return TokenType.WEAK;
							break;
					}
					break;
				case 5:
					switch (begin.PeekChar()) {
						case 'a':
							if (matches(begin, "async")) return TokenType.ASYNC;
							break;
						case 'b':
							if (matches(begin, "break")) return TokenType.BREAK;
							break;
						case 'c':
							switch (begin.PeekCharAt(1)) {
								case 'a':
									if (matches(begin, "catch")) return TokenType.CATCH;
									break;
								case 'l':
									if (matches(begin, "class")) return TokenType.CLASS;
									break;
								case 'o':
									if (matches(begin, "const")) return TokenType.CONST;
									break;
							}
							break;
						case 'f':
							if (matches(begin, "false")) return TokenType.FALSE;
							break;
						case 'o':
							if (matches(begin, "owned")) return TokenType.OWNED;
							break;
						case 't':
							if (matches(begin, "throw")) return TokenType.THROW;
							break;
						case 'u':
							if (matches(begin, "using")) return TokenType.USING;
							break;
						case 'w':
							if (matches(begin, "while")) return TokenType.WHILE;
							break;
						case 'y':
							if (matches(begin, "yield")) return TokenType.YIELD;
							break;
					}
					break;
				case 6:
					switch (begin.PeekChar()) {
						case 'd':
							if (matches(begin, "delete")) return TokenType.DELETE;
							break;
						case 'e':
							if (matches(begin, "extern")) return TokenType.EXTERN;
							break;
						case 'i':
							if (matches(begin, "inline")) return TokenType.INLINE;
							break;
						case 'p':
							switch (begin.PeekCharAt(1)) {
								case 'a':
									if (matches(begin, "params")) return TokenType.PARAMS;
									break;
								case 'u':
									if (matches(begin, "public")) return TokenType.PUBLIC;
									break;
							}
							break;
						case 'r':
							if (matches(begin, "return")) return TokenType.RETURN;
							break;
						case 's':
							switch (begin.PeekCharAt(1)) {
								case 'e':
									if (matches(begin, "sealed")) return TokenType.SEALED;
									break;
								case 'i':
									switch (begin.PeekCharAt(2)) {
										case 'g':
											if (matches(begin, "signal")) return TokenType.SIGNAL;
											break;
										case 'z':
											if (matches(begin, "sizeof")) return TokenType.SIZEOF;
											break;
									}
									break;
								case 't':
									switch (begin.PeekCharAt(2)) {
										case 'a':
											if (matches(begin, "static")) return TokenType.STATIC;
											break;
										case 'r':
											if (matches(begin, "struct")) return TokenType.STRUCT;
											break;
									}
									break;
								case 'w':
									if (matches(begin, "switch")) return TokenType.SWITCH;
									break;
							}
							break;
						case 't':
							switch (begin.PeekCharAt(1)) {
								case 'h':
									if (matches(begin, "throws")) return TokenType.THROWS;
									break;
								case 'y':
									if (matches(begin, "typeof")) return TokenType.TYPEOF;
									break;
							}
							break;
					}
					break;
				case 7:
					switch (begin.PeekChar()) {
						case 'd':
							switch (begin.PeekCharAt(1)) {
								case 'e':
									if (matches(begin, "default")) return TokenType.DEFAULT;
									break;
								case 'y':
									if (matches(begin, "dynamic")) return TokenType.DYNAMIC;
									break;
							}
							break;
						case 'e':
							if (matches(begin, "ensures")) return TokenType.ENSURES;
							break;
						case 'f':
							switch (begin.PeekCharAt(1)) {
								case 'i':
									if (matches(begin, "finally")) return TokenType.FINALLY;
									break;
								case 'o':
									if (matches(begin, "foreach")) return TokenType.FOREACH;
									break;
							}
							break;
						case 'p':
							if (matches(begin, "private")) return TokenType.PRIVATE;
							break;
						case 'u':
							if (matches(begin, "unowned")) return TokenType.UNOWNED;
							break;
						case 'v':
							if (matches(begin, "virtual")) return TokenType.VIRTUAL;
							break;
					}
					break;
				case 8:
					switch (begin.PeekChar()) {
						case 'a':
							if (matches(begin, "abstract")) return TokenType.ABSTRACT;
							break;
						case 'c':
							if (matches(begin, "continue")) return TokenType.CONTINUE;
							break;
						case 'd':
							if (matches(begin, "delegate")) return TokenType.DELEGATE;
							break;
						case 'i':
							if (matches(begin, "internal")) return TokenType.INTERNAL;
							break;
						case 'o':
							if (matches(begin, "override")) return TokenType.OVERRIDE;
							break;
						case 'r':
							if (matches(begin, "requires")) return TokenType.REQUIRES;
							break;
						case 'v':
							if (matches(begin, "volatile")) return TokenType.VOLATILE;
							break;
					}
					break;
				case 9:
					switch (begin.PeekChar()) {
						case 'c':
							if (matches(begin, "construct")) return TokenType.CONSTRUCT;
							break;
						case 'i':
							if (matches(begin, "interface")) return TokenType.INTERFACE;
							break;
						case 'n':
							if (matches(begin, "namespace")) return TokenType.NAMESPACE;
							break;
						case 'p':
							if (matches(begin, "protected")) return TokenType.PROTECTED;
							break;
					}
					break;
				case 11:
					if (matches(begin, "errordomain")) return TokenType.ERRORDOMAIN;
					break;
			}
			return TokenType.IDENTIFIER;
		}

		TokenType read_number() {
			var type = TokenType.INTEGER_LITERAL;

			// integer part
			if (current.Position < end - 2 && current.PeekChar() == '0'
				&& current.PeekCharAt(1) == 'x' && GChar.IsXDigit(current.PeekCharAt(2))) {
				// hexadecimal integer literal
				current.Position += 2;
				while (current.Position < end && GChar.IsXDigit(current.PeekChar())) {
					current.Position++;
				}
			} else {
				// decimal number
				while (current.Position < end && GChar.IsXDigit(current.PeekChar())) {
					current.Position++;
				}
			}

			// fractional part
			if (current.Position < end - 1 && current.PeekChar() == '.' && Char.IsDigit(current.PeekCharAt(1))) {
				type = TokenType.REAL_LITERAL;
				current.Position++;
				while (current.Position < end && Char.IsDigit(current.PeekChar())) {
					current.Position++;
				}
			}

			// exponent part
			if (current.Position < end && Char.ToLower(current.PeekChar()) == 'e') {
				type = TokenType.REAL_LITERAL;
				current.Position++;
				if (current.Position < end && (current.PeekChar() == '+' || current.PeekChar() == '-')) {
					current.Position++;
				}
				while (current.Position < end && Char.IsDigit(current.PeekChar())) {
					current.Position++;
				}
			}

			// type suffix
			if (current.Position < end) {
				bool real_literal = (type == TokenType.REAL_LITERAL);

				switch (current.PeekChar()) {
					case 'l':
					case 'L':
						if (type == TokenType.INTEGER_LITERAL) {
							current.Position++;
							if (current.Position < end && Char.ToLower(current.PeekChar()) == 'l') {
								current.Position++;
							}
						}
						break;
					case 'u':
					case 'U':
						if (type == TokenType.INTEGER_LITERAL) {
							current.Position++;
							if (current.Position < end && Char.ToLower(current.PeekChar()) == 'l') {
								current.Position++;
								if (current.Position < end && Char.ToLower(current.PeekChar()) == 'l') {
									current.Position++;
								}
							}
						}
						break;
					case 'f':
					case 'F':
					case 'd':
					case 'D':
						type = TokenType.REAL_LITERAL;
						current.Position++;
						break;
				}

				if (!real_literal && is_ident_char(current.PeekChar())) {
					// allow identifiers to start with a digit
					// as long as they contain at least one char
					while (current.Position < end && is_ident_char(current.PeekChar())) {
						current.Position++;
					}
					type = TokenType.IDENTIFIER;
				}
			}

			return type;
		}

		public TokenType read_template_token(out SourceLocation token_begin, out SourceLocation token_end) {
			TokenType type;
			MemoryStream begin = current.Clone();
			token_begin = new SourceLocation(begin, line, column);

			int token_length_in_chars = -1;

			if (current.Position >= end) {
				type = TokenType.EOF;
			} else {
				switch (current.PeekChar()) {
					case '"':
						type = TokenType.CLOSE_TEMPLATE;
						current.Position++;
						state_stack.RemoveAt(state_stack.Count - 1);
						break;
					case '$':
						token_begin.pos++; // $ is not part of following token
						current.Position++;
						if (Char.IsLetter(current.PeekChar()) || current.PeekChar() == '_') {
							int len = 0;
							while (current.Position < end && is_ident_char(current.PeekChar())) {
								current.Position++;
								len++;
							}
							type = TokenType.IDENTIFIER;
							state_stack.Add(State.TEMPLATE_PART);
						} else if (current.PeekChar() == '(') {
							current.Position++;
							column += 2;
							state_stack.Add(State.PARENS);
							return read_token(out token_begin, out token_end);
						} else if (current.PeekChar() == '$') {
							type = TokenType.TEMPLATE_STRING_LITERAL;
							current.Position++;
							state_stack.Add(State.TEMPLATE_PART);
						} else {
							Report.error(get_source_reference(1), "unexpected character");
							return read_template_token(out token_begin, out token_end);
						}
						break;
					default:
						type = TokenType.TEMPLATE_STRING_LITERAL;
						token_length_in_chars = 0;
						while (current.Position < end && current.PeekChar() != '"' && current.PeekChar() != '$') {
							if (current.PeekChar() == '\\') {
								current.Position++;
								token_length_in_chars++;
								if (current.Position >= end) {
									break;
								}

								switch (current.PeekChar()) {
									case '\'':
									case '"':
									case '\\':
									case '0':
									case 'b':
									case 'f':
									case 'n':
									case 'r':
									case 't':
									case 'v':
										current.Position++;
										token_length_in_chars++;
										break;
									case 'u':
										// u escape character has four hex digits
										current.Position++;
										token_length_in_chars++;
										int digit_length;
										for (digit_length = 0; digit_length < 4 && current.Position < end && GChar.IsXDigit(current.PeekChar()); digit_length++) {
											current.Position++;
											token_length_in_chars++;
										}
										if (digit_length != 4) {
											Report.error(get_source_reference(token_length_in_chars), "\\u requires four hex digits");
										}
										break;
									case 'x':
										// hexadecimal escape character requires two hex digits
										current.Position++;
										token_length_in_chars++;
										int _digit_length;
										for (_digit_length = 0; current.Position < end && GChar.IsXDigit(current.PeekChar()); _digit_length++) {
											current.Position++;
											token_length_in_chars++;
										}
										if (_digit_length < 1) {
											Report.error(get_source_reference(token_length_in_chars), "\\x requires at least one hex digit");
										}
										break;
									default:
										Report.error(get_source_reference(token_length_in_chars), "invalid escape sequence");
										break;
								}
							} else if (current.PeekChar() == '\n') {
								current.Position++;
								line++;
								column = 1;
								token_length_in_chars = 1;
							} else {
								char u = current.PeekCharAt((int)(end - current.Position));
								/*
								if (u != (char)(-1)) {
									current.Position += u.to_utf8(null);
									token_length_in_chars++;
								} else {
									current.Position++;
									Report.error(get_source_reference(token_length_in_chars), "invalid UTF-8 character");
								}*/
								current.Position++;
								token_length_in_chars++;
							}
						}
						if (current.Position >= end) {
							Report.error(get_source_reference(token_length_in_chars), "syntax error, expected \"");
							state_stack.RemoveAt(state_stack.Count - 1);
							return read_token(out token_begin, out token_end);
						}
						state_stack.Add(State.TEMPLATE_PART);
						break;
				}
			}

			if (token_length_in_chars < 0) {
				column += (int)(current.Position - begin.Position);
			} else {
				column += token_length_in_chars;
			}

			token_end = new SourceLocation(current, line, column - 1);

			return type;
		}

		public TokenType read_token(out SourceLocation token_begin, out SourceLocation token_end) {
			if (in_template()) {
				return read_template_token(out token_begin, out token_end);
			} else if (in_template_part()) {
				state_stack.RemoveAt(state_stack.Count - 1);

				token_begin = new SourceLocation(current, line, column);
				token_end = new SourceLocation(current, line, column - 1);

				return TokenType.COMMA;
			} else if (in_regex_literal()) {
				return read_regex_token(out token_begin, out token_end);
			}

			space();

			TokenType type;
			MemoryStream begin = current.Clone();
			token_begin = new SourceLocation(current, line, column);

			int token_length_in_chars = -1;

			if (current.Position >= end) {
				type = TokenType.EOF;
			} else if (Char.IsLetter(current.PeekChar()) || current.PeekChar() == '_') {
				int len = 0;
				while (current.Position < end && is_ident_char(current.PeekChar())) {
					current.Position++;
					len++;
				}
				type = get_identifier_or_keyword(current, len);
			} else if (current.PeekChar() == '@') {
				if (current.Position < end - 1 && current.PeekCharAt(1) == '"') {
					type = TokenType.OPEN_TEMPLATE;
					current.Position += 2;
					state_stack.Add(State.TEMPLATE);
				} else {
					token_begin.pos++; // @ is not part of the identifier
					current.Position++;
					int len = 0;
					while (current.Position < end && is_ident_char(current.PeekChar())) {
						current.Position++;
						len++;
					}
					type = TokenType.IDENTIFIER;
				}
			} else if (Char.IsDigit(current.PeekChar())) {
				type = read_number();
			} else {
				switch (current.PeekChar()) {
					case '{':
						type = TokenType.OPEN_BRACE;
						current.Position++;
						state_stack.Add(State.BRACE);
						break;
					case '}':
						type = TokenType.CLOSE_BRACE;
						current.Position++;
						if (state_stack.Count > 0) {
							state_stack.RemoveAt(state_stack.Count - 1);
						}
						break;
					case '(':
						type = TokenType.OPEN_PARENS;
						current.Position++;
						state_stack.Add(State.PARENS);
						break;
					case ')':
						type = TokenType.CLOSE_PARENS;
						current.Position++;
						if (state_stack.Count > 0) {
							state_stack.RemoveAt(state_stack.Count - 1);
						}
						if (in_template()) {
							type = TokenType.COMMA;
						}
						break;
					case '[':
						type = TokenType.OPEN_BRACKET;
						current.Position++;
						state_stack.Add(State.BRACKET);
						break;
					case ']':
						type = TokenType.CLOSE_BRACKET;
						current.Position++;
						if (state_stack.Count > 0) {
							state_stack.RemoveAt(state_stack.Count - 1);
						}
						break;
					case '.':
						type = TokenType.DOT;
						current.Position++;
						if (current.Position < end - 1) {
							if (current.PeekChar() == '.' && current.PeekCharAt(1) == '.') {
								type = TokenType.ELLIPSIS;
								current.Position += 2;
							}
						}
						break;
					case ':':
						type = TokenType.COLON;
						current.Position++;
						if (current.Position < end && current.PeekChar() == ':') {
							type = TokenType.DOUBLE_COLON;
							current.Position++;
						}
						break;
					case ',':
						type = TokenType.COMMA;
						current.Position++;
						break;
					case ';':
						type = TokenType.SEMICOLON;
						current.Position++;
						break;
					case '#':
						type = TokenType.HASH;
						current.Position++;
						break;
					case '?':
						type = TokenType.INTERR;
						current.Position++;
						if (current.Position < end && current.PeekChar() == '?') {
							type = TokenType.OP_COALESCING;
							current.Position++;
						}
						break;
					case '|':
						type = TokenType.BITWISE_OR;
						current.Position++;
						if (current.Position < end) {
							switch (current.PeekChar()) {
								case '=':
									type = TokenType.ASSIGN_BITWISE_OR;
									current.Position++;
									break;
								case '|':
									type = TokenType.OP_OR;
									current.Position++;
									break;
							}
						}
						break;
					case '&':
						type = TokenType.BITWISE_AND;
						current.Position++;
						if (current.Position < end) {
							switch (current.PeekChar()) {
								case '=':
									type = TokenType.ASSIGN_BITWISE_AND;
									current.Position++;
									break;
								case '&':
									type = TokenType.OP_AND;
									current.Position++;
									break;
							}
						}
						break;
					case '^':
						type = TokenType.CARRET;
						current.Position++;
						if (current.Position < end && current.PeekChar() == '=') {
							type = TokenType.ASSIGN_BITWISE_XOR;
							current.Position++;
						}
						break;
					case '~':
						type = TokenType.TILDE;
						current.Position++;
						break;
					case '=':
						type = TokenType.ASSIGN;
						current.Position++;
						if (current.Position < end) {
							switch (current.PeekChar()) {
								case '=':
									type = TokenType.OP_EQ;
									current.Position++;
									break;
								case '>':
									type = TokenType.LAMBDA;
									current.Position++;
									break;
							}
						}
						break;
					case '<':
						type = TokenType.OP_LT;
						current.Position++;
						if (current.Position < end) {
							switch (current.PeekChar()) {
								case '=':
									type = TokenType.OP_LE;
									current.Position++;
									break;
								case '<':
									type = TokenType.OP_SHIFT_LEFT;
									current.Position++;
									if (current.Position < end && current.PeekChar() == '=') {
										type = TokenType.ASSIGN_SHIFT_LEFT;
										current.Position++;
									}
									break;
							}
						}
						break;
					case '>':
						type = TokenType.OP_GT;
						current.Position++;
						if (current.Position < end && current.PeekChar() == '=') {
							type = TokenType.OP_GE;
							current.Position++;
						}
						break;
					case '!':
						type = TokenType.OP_NEG;
						current.Position++;
						if (current.Position < end && current.PeekChar() == '=') {
							type = TokenType.OP_NE;
							current.Position++;
						}
						break;
					case '+':
						type = TokenType.PLUS;
						current.Position++;
						if (current.Position < end) {
							switch (current.PeekChar()) {
								case '=':
									type = TokenType.ASSIGN_ADD;
									current.Position++;
									break;
								case '+':
									type = TokenType.OP_INC;
									current.Position++;
									break;
							}
						}
						break;
					case '-':
						type = TokenType.MINUS;
						current.Position++;
						if (current.Position < end) {
							switch (current.PeekChar()) {
								case '=':
									type = TokenType.ASSIGN_SUB;
									current.Position++;
									break;
								case '-':
									type = TokenType.OP_DEC;
									current.Position++;
									break;
								case '>':
									type = TokenType.OP_PTR;
									current.Position++;
									break;
							}
						}
						break;
					case '*':
						type = TokenType.STAR;
						current.Position++;
						if (current.Position < end && current.PeekChar() == '=') {
							type = TokenType.ASSIGN_MUL;
							current.Position++;
						}
						break;
					case '/':
						switch (previous) {
							case TokenType.ASSIGN:
							case TokenType.COMMA:
							case TokenType.MINUS:
							case TokenType.OP_AND:
							case TokenType.OP_COALESCING:
							case TokenType.OP_EQ:
							case TokenType.OP_GE:
							case TokenType.OP_GT:
							case TokenType.OP_LE:
							case TokenType.OP_LT:
							case TokenType.OP_NE:
							case TokenType.OP_NEG:
							case TokenType.OP_OR:
							case TokenType.OPEN_BRACE:
							case TokenType.OPEN_PARENS:
							case TokenType.PLUS:
							case TokenType.RETURN:
								type = TokenType.OPEN_REGEX_LITERAL;
								state_stack.Add(State.REGEX_LITERAL);
								current.Position++;
								break;
							default:
								type = TokenType.DIV;
								current.Position++;
								if (current.Position < end && current.PeekChar() == '=') {
									type = TokenType.ASSIGN_DIV;
									current.Position++;
								}
								break;
						}
						break;
					case '%':
						type = TokenType.PERCENT;
						current.Position++;
						if (current.Position < end && current.PeekChar() == '=') {
							type = TokenType.ASSIGN_PERCENT;
							current.Position++;
						}
						break;
					case '\'':
					case '"':
						if (begin.PeekChar() == '\'') {
							type = TokenType.CHARACTER_LITERAL;
						} else if (current.Position < end - 6 && begin.PeekCharAt(1) == '"' && begin.PeekCharAt(2) == '"') {
							type = TokenType.VERBATIM_STRING_LITERAL;
							token_length_in_chars = 6;
							current.Position += 3;
							while (current.Position < end - 4) {
								if (current.PeekChar() == '"' && current.PeekCharAt(1) == '"' && current.PeekCharAt(2) == '"' && current.PeekCharAt(3) != '"') {
									break;
								} else if (current.PeekChar() == '\n') {
									current.Position++;
									line++;
									column = 1;
									token_length_in_chars = 3;
								} else {
									char _u = current.PeekCharAt((int)(end - current.Position));
									/*if (u != (char)(-1)) {
										current.Position += u.to_utf8(null);
										token_length_in_chars++;
									} else {
										Report.error(get_source_reference(token_length_in_chars), "invalid UTF-8 character");
									}*/
									current.Position++;
									token_length_in_chars++;
								}
							}
							if (current.PeekChar() == '"' && current.PeekCharAt(1) == '"' && current.PeekCharAt(2) == '"') {
								current.Position += 3;
							} else {
								Report.error(get_source_reference(token_length_in_chars), "syntax error, expected \"\"\"");
							}
							break;
						} else {
							type = TokenType.STRING_LITERAL;
						}
						token_length_in_chars = 2;
						current.Position++;
						while (current.Position < end && current.PeekChar() != begin.PeekChar()) {
							if (current.PeekChar() == '\\') {
								current.Position++;
								token_length_in_chars++;
								if (current.Position >= end) {
									break;
								}

								switch (current.PeekChar()) {
									case '\'':
									case '"':
									case '\\':
									case '0':
									case 'b':
									case 'f':
									case 'n':
									case 'r':
									case 't':
									case 'v':
									case '$':
										current.Position++;
										token_length_in_chars++;
										break;
									case 'u':
										// u escape character has four hex digits
										current.Position++;
										token_length_in_chars++;
										int digit_length;
										for (digit_length = 0; digit_length < 4 && current.Position < end && GChar.IsXDigit(current.PeekChar()); digit_length++) {
											current.Position++;
											token_length_in_chars++;
										}
										if (digit_length != 4) {
											Report.error(get_source_reference(token_length_in_chars), "\\u requires four hex digits");
										}
										break;
									case 'x':
										// hexadecimal escape character requires two hex digits
										current.Position++;
										token_length_in_chars++;
										int _digit_length;
										for (_digit_length = 0; current.Position < end && GChar.IsXDigit(current.PeekChar()); _digit_length++) {
											current.Position++;
											token_length_in_chars++;
										}
										if (_digit_length < 1) {
											Report.error(get_source_reference(token_length_in_chars), "\\x requires at least one hex digit");
										}
										break;
									default:
										Report.error(get_source_reference(token_length_in_chars), "invalid escape sequence");
										break;
								}
							} else if (current.PeekChar() == '\n') {
								current.Position++;
								line++;
								column = 1;
								token_length_in_chars = 1;
							} else {
								//char _u = current.PeekCharAt((int)(end - current.Position), SeekOrigin.Begin);
								/*if (u != (char)(-1)) {
									current.Position += u.to_utf8(null);
									token_length_in_chars++;
								} else {
									current.Position++;
									Report.error(get_source_reference(token_length_in_chars), "invalid UTF-8 character");
								}*/
								current.Position++;
								token_length_in_chars++;
							}
							if (current.Position < end && begin.PeekChar() == '\'' && current.PeekChar() != '\'') {
								// multiple characters in single character literal
								Report.error(get_source_reference(token_length_in_chars), "invalid character literal");
							}
						}
						if (current.Position < end) {
							current.Position++;
						} else {
							Report.error(get_source_reference(token_length_in_chars), "syntax error, expected %c".printf(begin.PeekChar()));
						}
						break;
					default:
						//char u = ((string)char).get_char_validated((long)(end - current));
						char u = current.PeekCharAt((int)(end - current.Position));
						/*if (u != (char)(-1)) {
							current.Position += u.to_utf8(null);
							Report.error(get_source_reference(0), "syntax error, unexpected character");
						} else {
							current.Position++;
							Report.error(get_source_reference(0), "invalid UTF-8 character");
						}*/
						current.Position++;


						column++;
						return read_token(out token_begin, out token_end);
				}
			}

			if (token_length_in_chars < 0) {
				column += (int)(current.Position - begin.Position);
			} else {
				column += token_length_in_chars;
			}

			token_end = new SourceLocation(current, line, column - 1);
			previous = type;

			return type;
		}

		static bool matches(MemoryStream begin, string keyword) {
			string text = begin.PeekString(keyword.Length);
			return text == keyword;
		}

		bool pp_whitespace() {
			bool found = false;
			while (current.Position < end && Char.IsWhiteSpace(current.PeekChar()) && current.PeekChar() != '\n') {
				found = true;
				current.Position++;
				column++;
			}
			return found;
		}

		void pp_space() {
			while (pp_whitespace() || comment()) {
			}
		}

		void pp_directive() {
			// hash sign
			current.Position++;
			column++;

			if (line == 1 && column == 2 && current.Position < end && current.PeekChar() == '!') {
				// hash bang: #!
				// skip until end of line or end of file
				while (current.Position < end && current.PeekChar() != '\n') {
					current.Position++;
				}
				return;
			}

			pp_space();

			MemoryStream begin = current.Clone();

			int len = 0;
			while (current.Position < end && Char.IsLetterOrDigit(current.PeekChar())) {
				current.Position++;
				column++;
				len++;
			}

			if (len == 2 && matches(begin, "if")) {
				parse_pp_if();
			} else if (len == 4 && matches(begin, "elif")) {
				parse_pp_elif();
			} else if (len == 4 && matches(begin, "else")) {
				parse_pp_else();
			} else if (len == 5 && matches(begin, "endif")) {
				parse_pp_endif();
			} else {
				Report.error(get_source_reference(-len, len), "syntax error, invalid preprocessing directive");
			}

			if (conditional_stack.Count > 0
				&& conditional_stack[conditional_stack.Count - 1].skip_section) {
				// skip lines until next preprocessing directive
				bool bol = false;
				while (current.Position < end) {
					if (bol && current.PeekChar() == '#') {
						// go back to begin of line
						current.Position -= (column - 1);
						column = 1;
						return;
					}
					if (current.PeekChar() == '\n') {
						line++;
						column = 0;
						bol = true;
					} else if (!Char.IsWhiteSpace(current.PeekChar())) {
						bol = false;
					}
					current.Position++;
					column++;
				}
			}
		}

		void pp_eol() {
			pp_space();
			if (current.Position >= end || current.PeekChar() != '\n') {
				Report.error(get_source_reference(0), "syntax error, expected newline");
			}
		}

		void parse_pp_if() {
			pp_space();

			bool condition = parse_pp_expression();

			pp_eol();

			conditional_stack.Add(new Conditional());

			if (condition && (conditional_stack.Count == 1 || !conditional_stack[conditional_stack.Count - 2].skip_section)) {
				// condition true => process code within if
				var cond = conditional_stack[conditional_stack.Count - 1];
				cond.matched = true;
				conditional_stack[conditional_stack.Count - 1] = cond;
			} else {
				// skip lines until next preprocessing directive
				var cond = conditional_stack[conditional_stack.Count - 1];
				cond.skip_section = true;
				conditional_stack[conditional_stack.Count - 1] = cond;
			}
		}

		void parse_pp_elif() {
			pp_space();

			bool condition = parse_pp_expression();

			pp_eol();

			if (conditional_stack.Count == 0 || conditional_stack[conditional_stack.Count - 1].else_found) {
				Report.error(get_source_reference(0), "syntax error, unexpected #elif");
				return;
			}

			if (condition && !conditional_stack[conditional_stack.Count - 1].matched
				&& (conditional_stack.Count == 1 || !conditional_stack[conditional_stack.Count - 2].skip_section)) {
				// condition true => process code within if
				var cond = conditional_stack[conditional_stack.Count - 1];
				cond.matched = true;
				cond.skip_section = false;
				conditional_stack[conditional_stack.Count - 1] = cond;
			} else {
				// skip lines until next preprocessing directive
				var cond = conditional_stack[conditional_stack.Count - 1];
				cond.skip_section = true;
				conditional_stack[conditional_stack.Count - 1] = cond;
			}
		}

		void parse_pp_else() {
			pp_eol();

			if (conditional_stack.Count == 0 || conditional_stack[conditional_stack.Count - 1].else_found) {
				Report.error(get_source_reference(0), "syntax error, unexpected #else");
				return;
			}

			if (!conditional_stack[conditional_stack.Count - 1].matched
				&& (conditional_stack.Count == 1 || !conditional_stack[conditional_stack.Count - 2].skip_section)) {
				// condition true => process code within if
				var cond = conditional_stack[conditional_stack.Count - 1];
				cond.matched = true;
				cond.skip_section = false;
				conditional_stack[conditional_stack.Count - 1] = cond;
			} else {
				// skip lines until next preprocessing directive
				var cond = conditional_stack[conditional_stack.Count - 1];
				cond.skip_section = true;
				conditional_stack[conditional_stack.Count - 1] = cond;
			}
		}

		void parse_pp_endif() {
			pp_eol();

			if (conditional_stack.Count == 0) {
				Report.error(get_source_reference(0), "syntax error, unexpected #endif");
				return;
			}

			conditional_stack.RemoveAt(conditional_stack.Count - 1);
		}

		bool parse_pp_symbol() {
			int len = 0;
			while (current.Position < end && is_ident_char(current.PeekChar())) {
				current.Position++;
				column++;
				len++;
			}

			if (len == 0) {
				Report.error(get_source_reference(0), "syntax error, expected identifier");
				return false;
			}

			current.Seek(-len, SeekOrigin.Current);
			string identifier = current.PeekString(len);
			current.Seek(len, SeekOrigin.Current);

			bool defined;
			if (identifier == "true") {
				defined = true;
			} else if (identifier == "false") {
				defined = false;
			} else {
				defined = source_file.context.is_defined(identifier);
			}

			return defined;
		}

		bool parse_pp_primary_expression() {
			if (current.Position >= end) {
				Report.error(get_source_reference(0), "syntax error, expected identifier");
			} else if (is_ident_char(current.PeekChar())) {
				return parse_pp_symbol();
			} else if (current.PeekChar() == '(') {
				current.Position++;
				column++;
				pp_space();
				bool result = parse_pp_expression();
				pp_space();
				if (current.Position < end && current.PeekChar() == ')') {
					current.Position++;
					column++;
				} else {
					Report.error(get_source_reference(0), "syntax error, expected `)'");
				}
				return result;
			} else {
				Report.error(get_source_reference(0), "syntax error, expected identifier");
			}
			return false;
		}

		bool parse_pp_unary_expression() {
			if (current.Position < end && current.PeekChar() == '!') {
				current.Position++;
				column++;
				pp_space();
				return !parse_pp_unary_expression();
			}

			return parse_pp_primary_expression();
		}

		bool parse_pp_equality_expression() {
			bool left = parse_pp_unary_expression();
			pp_space();
			while (true) {
				if (current.Position < end - 1 && current.PeekChar() == '=' && current.PeekCharAt(1) == '=') {
					current.Position += 2;
					column += 2;
					pp_space();
					bool right = parse_pp_unary_expression();
					left = (left == right);
				} else if (current.Position < end - 1 && current.PeekChar() == '!' && current.PeekCharAt(1) == '=') {
					current.Position += 2;
					column += 2;
					pp_space();
					bool right = parse_pp_unary_expression();
					left = (left != right);
				} else {
					break;
				}
			}
			return left;
		}

		bool parse_pp_and_expression() {
			bool left = parse_pp_equality_expression();
			pp_space();
			while (current.Position < end - 1 && current.PeekChar() == '&' && current.PeekCharAt(1) == '&') {
				current.Position += 2;
				column += 2;
				pp_space();
				bool right = parse_pp_equality_expression();
				left = left && right;
			}
			return left;
		}

		bool parse_pp_or_expression() {
			bool left = parse_pp_and_expression();
			pp_space();
			while (current.Position < end - 1 && current.PeekChar() == '|' && current.PeekCharAt(1) == '|') {
				current.Position += 2;
				column += 2;
				pp_space();
				bool right = parse_pp_and_expression();
				left = left || right;
			}
			return left;
		}

		bool parse_pp_expression() {
			return parse_pp_or_expression();
		}

		bool whitespace() {
			bool found = false;
			bool bol = (column == 1);
			while (current.Position < end && Char.IsWhiteSpace(current.PeekChar())) {
				if (current.PeekChar() == '\n') {
					line++;
					column = 0;
					bol = true;
				}
				found = true;
				current.Position++;
				column++;
			}
			if (bol && current.Position < end && current.PeekChar() == '#') {
				pp_directive();
				return true;
			}
			return found;
		}

		bool comment(bool file_comment = false) {
			if (current == null
				|| current.Position > end - 2
				|| current.PeekChar() != '/'
				|| (current.PeekCharAt(1) != '/' && current.PeekCharAt(1) != '*')) {
				return false;
			}

			if (current.PeekCharAt(1) == '/') {
				SourceReference source_reference = null;
				if (file_comment) {
					source_reference = get_source_reference(0);
				}

				// single-line comment
				current.Position += 2;
				MemoryStream begin = current.Clone();

				// skip until end of line or end of file
				while (current.Position < end && current.PeekChar() != '\n') {
					current.Position++;
				}

				if (source_reference != null) {
					string comment = begin.PeekString((int)(current.Position - begin.Position));
					push_comment(comment, source_reference, file_comment);
				}
			} else {
				SourceReference source_reference = null;

				if (file_comment && current.PeekCharAt(2) == '*') {
					return false;
				}

				if (current.PeekCharAt(2) == '*' || file_comment) {
					source_reference = get_source_reference(0);
				}

				current.Position += 2;
				column += 2;

				MemoryStream begin = current.Clone();
				while (current.Position < end - 1
					   && (current.PeekChar() != '*' || current.PeekCharAt(1) != '/')) {
					if (current.PeekChar() == '\n') {
						line++;
						column = 0;
					}
					current.Position++;
					column++;
				}

				if (current.Position == end - 1) {
					Report.error(get_source_reference(0), "syntax error, expected */");
					return true;
				}

				if (source_reference != null) {
					string comment = begin.PeekString((int)(current.Position - begin.Position));
					push_comment(comment, source_reference, file_comment);
				}

				current.Position += 2;
				column += 2;
			}

			return true;
		}

		void space() {
			while (whitespace() || comment()) {
			}
		}

		public void parse_file_comments() {
			while (whitespace() || comment(true)) {
			}
		}

		void push_comment(string comment_item, SourceReference source_reference, bool file_comment) {
			if (comment_item[0] == '*') {
				if (_comment != null) {
					// extra doc comment, add it to source file comments
					source_file.add_comment(_comment);
				}
				_comment = new Comment(comment_item, source_reference);
			}

			if (file_comment) {
				source_file.add_comment(new Comment(comment_item, source_reference));
				_comment = null;
			}
		}

		/**
		 * Clears and returns the content of the comment stack.
		 *
		 * @return saved comment
		 */
		public Comment pop_comment() {
			if (_comment == null) {
				return null;
			}

			var comment = _comment;
			_comment = null;
			return comment;
		}
	}


}
