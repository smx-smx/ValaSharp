using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vala.Lang.Types {
	public enum TokenType {
		NONE,
		ABSTRACT,
		AS,
		ASSIGN,
		ASSIGN_ADD,
		ASSIGN_BITWISE_AND,
		ASSIGN_BITWISE_OR,
		ASSIGN_BITWISE_XOR,
		ASSIGN_DIV,
		ASSIGN_MUL,
		ASSIGN_PERCENT,
		ASSIGN_SHIFT_LEFT,
		ASSIGN_SUB,
		ASYNC,
		BASE,
		BITWISE_AND,
		BITWISE_OR,
		BREAK,
		CARRET,
		CASE,
		CATCH,
		CHARACTER_LITERAL,
		CLASS,
		CLOSE_BRACE,
		CLOSE_BRACKET,
		CLOSE_PARENS,
		CLOSE_REGEX_LITERAL,
		CLOSE_TEMPLATE,
		COLON,
		COMMA,
		CONST,
		CONSTRUCT,
		CONTINUE,
		DEFAULT,
		DELEGATE,
		DELETE,
		DIV,
		DO,
		DOUBLE_COLON,
		DOT,
		DYNAMIC,
		ELLIPSIS,
		ELSE,
		ENUM,
		ENSURES,
		ERRORDOMAIN,
		EOF,
		EXTERN,
		FALSE,
		FINALLY,
		FOR,
		FOREACH,
		GET,
		HASH,
		IDENTIFIER,
		IF,
		IN,
		INLINE,
		INTEGER_LITERAL,
		INTERFACE,
		INTERNAL,
		INTERR,
		IS,
		LAMBDA,
		LOCK,
		MINUS,
		NAMESPACE,
		NEW,
		NULL,
		OUT,
		OP_AND,
		OP_COALESCING,
		OP_DEC,
		OP_EQ,
		OP_GE,
		OP_GT,
		OP_INC,
		OP_LE,
		OP_LT,
		OP_NE,
		OP_NEG,
		OP_OR,
		OP_PTR,
		OP_SHIFT_LEFT,
		OPEN_BRACE,
		OPEN_BRACKET,
		OPEN_PARENS,
		OPEN_REGEX_LITERAL,
		OPEN_TEMPLATE,
		OVERRIDE,
		OWNED,
		PARAMS,
		PERCENT,
		PLUS,
		PRIVATE,
		PROTECTED,
		PUBLIC,
		REAL_LITERAL,
		REF,
		REGEX_LITERAL,
		REQUIRES,
		RETURN,
		SEALED,
		SEMICOLON,
		SET,
		SIGNAL,
		SIZEOF,
		STAR,
		STATIC,
		STRING_LITERAL,
		STRUCT,
		SWITCH,
		TEMPLATE_STRING_LITERAL,
		THIS,
		THROW,
		THROWS,
		TILDE,
		TRUE,
		TRY,
		TYPEOF,
		UNOWNED,
		USING,
		VAR,
		VERBATIM_STRING_LITERAL,
		VIRTUAL,
		VOID,
		VOLATILE,
		WEAK,
		WHILE,
		YIELD
	}

	public static class TokenTypeExtensions {
		public static string ToString(this TokenType @this) {
			switch (@this) {
				case TokenType.ABSTRACT: return "`abstract'";
				case TokenType.AS: return "`as'";
				case TokenType.ASSIGN: return "`='";
				case TokenType.ASSIGN_ADD: return "`+='";
				case TokenType.ASSIGN_BITWISE_AND: return "`&='";
				case TokenType.ASSIGN_BITWISE_OR: return "`|='";
				case TokenType.ASSIGN_BITWISE_XOR: return "`^='";
				case TokenType.ASSIGN_DIV: return "`/='";
				case TokenType.ASSIGN_MUL: return "`*='";
				case TokenType.ASSIGN_PERCENT: return "`%='";
				case TokenType.ASSIGN_SHIFT_LEFT: return "`<<='";
				case TokenType.ASSIGN_SUB: return "`-='";
				case TokenType.ASYNC: return "`async'";
				case TokenType.BASE: return "`base'";
				case TokenType.BITWISE_AND: return "`&'";
				case TokenType.BITWISE_OR: return "`|'";
				case TokenType.BREAK: return "`break'";
				case TokenType.CARRET: return "`^'";
				case TokenType.CASE: return "`case'";
				case TokenType.CATCH: return "`catch'";
				case TokenType.CHARACTER_LITERAL: return "character literal";
				case TokenType.CLASS: return "`class'";
				case TokenType.CLOSE_BRACE: return "`}'";
				case TokenType.CLOSE_BRACKET: return "`]'";
				case TokenType.CLOSE_PARENS: return "`)'";
				case TokenType.CLOSE_REGEX_LITERAL: return "`/'";
				case TokenType.CLOSE_TEMPLATE: return "close template";
				case TokenType.COLON: return "`:'";
				case TokenType.COMMA: return "`,'";
				case TokenType.CONST: return "`const'";
				case TokenType.CONSTRUCT: return "`construct'";
				case TokenType.CONTINUE: return "`continue'";
				case TokenType.DEFAULT: return "`default'";
				case TokenType.DELEGATE: return "`delegate'";
				case TokenType.DELETE: return "`delete'";
				case TokenType.DIV: return "`/'";
				case TokenType.DO: return "`do'";
				case TokenType.DOUBLE_COLON: return "`::'";
				case TokenType.DOT: return "`.'";
				case TokenType.DYNAMIC: return "`dynamic'";
				case TokenType.ELLIPSIS: return "`...'";
				case TokenType.ELSE: return "`else'";
				case TokenType.ENUM: return "`enum'";
				case TokenType.ENSURES: return "`ensures'";
				case TokenType.ERRORDOMAIN: return "`errordomain'";
				case TokenType.EOF: return "end of file";
				case TokenType.EXTERN: return "`extern'";
				case TokenType.FALSE: return "`false'";
				case TokenType.FINALLY: return "`finally'";
				case TokenType.FOR: return "`for'";
				case TokenType.FOREACH: return "`foreach'";
				case TokenType.GET: return "`get'";
				case TokenType.HASH: return "`hash'";
				case TokenType.IDENTIFIER: return "identifier";
				case TokenType.IF: return "`if'";
				case TokenType.IN: return "`in'";
				case TokenType.INLINE: return "`inline'";
				case TokenType.INTEGER_LITERAL: return "integer literal";
				case TokenType.INTERFACE: return "`interface'";
				case TokenType.INTERNAL: return "`internal'";
				case TokenType.INTERR: return "`?'";
				case TokenType.IS: return "`is'";
				case TokenType.LAMBDA: return "`=>'";
				case TokenType.LOCK: return "`lock'";
				case TokenType.MINUS: return "`-'";
				case TokenType.NAMESPACE: return "`namespace'";
				case TokenType.NEW: return "`new'";
				case TokenType.NULL: return "`null'";
				case TokenType.OUT: return "`out'";
				case TokenType.OP_AND: return "`&&'";
				case TokenType.OP_COALESCING: return "`??'";
				case TokenType.OP_DEC: return "`--'";
				case TokenType.OP_EQ: return "`=='";
				case TokenType.OP_GE: return "`>='";
				case TokenType.OP_GT: return "`>'";
				case TokenType.OP_INC: return "`++'";
				case TokenType.OP_LE: return "`<='";
				case TokenType.OP_LT: return "`<'";
				case TokenType.OP_NE: return "`!='";
				case TokenType.OP_NEG: return "`!'";
				case TokenType.OP_OR: return "`||'";
				case TokenType.OP_PTR: return "`->'";
				case TokenType.OP_SHIFT_LEFT: return "`<<'";
				case TokenType.OPEN_BRACE: return "`{'";
				case TokenType.OPEN_BRACKET: return "`['";
				case TokenType.OPEN_PARENS: return "`('";
				case TokenType.OPEN_REGEX_LITERAL: return "`/'";
				case TokenType.OPEN_TEMPLATE: return "open template";
				case TokenType.OVERRIDE: return "`override'";
				case TokenType.OWNED: return "`owned'";
				case TokenType.PARAMS: return "`params'";
				case TokenType.PERCENT: return "`%'";
				case TokenType.PLUS: return "`+'";
				case TokenType.PRIVATE: return "`private'";
				case TokenType.PROTECTED: return "`protected'";
				case TokenType.PUBLIC: return "`public'";
				case TokenType.REAL_LITERAL: return "real literal";
				case TokenType.REF: return "`ref'";
				case TokenType.REGEX_LITERAL: return "regex literal";
				case TokenType.REQUIRES: return "`requires'";
				case TokenType.RETURN: return "`return'";
				case TokenType.SEALED: return "`sealed'";
				case TokenType.SEMICOLON: return "`;'";
				case TokenType.SET: return "`set'";
				case TokenType.SIGNAL: return "`signal'";
				case TokenType.SIZEOF: return "`sizeof'";
				case TokenType.STAR: return "`*'";
				case TokenType.STATIC: return "`static'";
				case TokenType.STRING_LITERAL: return "string literal";
				case TokenType.STRUCT: return "`struct'";
				case TokenType.SWITCH: return "`switch'";
				case TokenType.TEMPLATE_STRING_LITERAL: return "template string literal";
				case TokenType.THIS: return "`this'";
				case TokenType.THROW: return "`throw'";
				case TokenType.THROWS: return "`throws'";
				case TokenType.TILDE: return "`~'";
				case TokenType.TRUE: return "`true'";
				case TokenType.TRY: return "`try'";
				case TokenType.TYPEOF: return "`typeof'";
				case TokenType.UNOWNED: return "`unowned'";
				case TokenType.USING: return "`using'";
				case TokenType.VAR: return "`var'";
				case TokenType.VERBATIM_STRING_LITERAL: return "verbatim string literal";
				case TokenType.VIRTUAL: return "`virtual'";
				case TokenType.VOID: return "`void'";
				case TokenType.VOLATILE: return "`volatile'";
				case TokenType.WEAK: return "`weak'";
				case TokenType.WHILE: return "`while'";
				case TokenType.YIELD: return "`yield'";
				default: return "unknown token";
			}
		}
	}
}
