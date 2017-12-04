using CLanguage.Expressions;
using CLanguage.Statements;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala;

namespace CLanguage {
	/**
 * Represents a function declaration in the C code.
 */
	public class CCodeFunction : CCodeNode {
		/**
		 * The name of this function.
		 */
		public string name { get; set; }

		/**
		 * The function return type.
		 */
		public string return_type { get; set; }

		public bool is_declaration { get; set; }

		/**
		 * The function body.
		 */
		public CCodeBlock block { get; set; }

		/**
		 * The current line directive.
		 */
		public CCodeLineDirective current_line { get; set; }

		/**
		 * The current block to be written into.
		 */
		public CCodeBlock current_block { get; set; }

		private List<CCodeParameter> parameters = new List<CCodeParameter>();

		List<CCodeStatement> statement_stack = new List<CCodeStatement>();

		public CCodeFunction(string name, string return_type = "void") {
			this.name = name;
			this.return_type = return_type;
			this.block = new CCodeBlock();
			current_block = block;
		}

		/**
		 * Appends the specified parameter to the list of function parameters.
		 *
		 * @param param a formal parameter
		 */
		public void add_parameter(CCodeParameter param) {
			parameters.Add(param);
		}

		public void insert_parameter(int position, CCodeParameter param) {
			parameters.Insert(position, param);
		}

		public int get_parameter_count() {
			return parameters.Count;
		}

		public CCodeParameter get_parameter(int position) {
			return parameters[position];
		}

		/**
		 * Returns a copy of this function.
		 *
		 * @return copied function
		 */
		public CCodeFunction copy() {
			var func = new CCodeFunction(name, return_type);
			func.modifiers = modifiers;

			/* no deep copy for lists available yet
			 * func.parameters = parameters.copy ();
			 */
			foreach (CCodeParameter param in parameters) {
				func.parameters.Add(param);
			}

			func.is_declaration = is_declaration;
			func.block = block;
			return func;
		}

		public override void write(CCodeWriter writer) {
			writer.write_indent(line);
			if (modifiers.HasFlag(CCodeModifiers.INTERNAL)) {
				writer.write_string("G_GNUC_INTERNAL ");
			}
			if (modifiers.HasFlag(CCodeModifiers.STATIC)) {
				writer.write_string("static ");
			}
			if (modifiers.HasFlag(CCodeModifiers.INLINE)) {
				writer.write_string("inline ");
			}
			writer.write_string(return_type);
			writer.write_string(" ");
			writer.write_string(name);
			writer.write_string(" (");

			bool has_args = (modifiers.HasFlag(CCodeModifiers.PRINTF) || modifiers.HasFlag(CCodeModifiers.SCANF));
			int i = 0;
			int format_arg_index = -1;
			int args_index = -1;
			foreach (CCodeParameter param in parameters) {
				if (i > 0) {
					writer.write_string(", ");
				}
				param.write(writer);
				if (param.modifiers.HasFlag(CCodeModifiers.FORMAT_ARG)) {
					format_arg_index = i;
				}
				if (has_args && param.ellipsis) {
					args_index = i;
				} else if (has_args && param.type_name == "va_list" && format_arg_index < 0) {
					format_arg_index = i - 1;
				}
				i++;
			}
			if (i == 0) {
				writer.write_string("void");
			}

			writer.write_string(")");

			if (is_declaration) {
				if (modifiers.HasFlag(CCodeModifiers.DEPRECATED)) {
					writer.write_string(" G_GNUC_DEPRECATED");
				}

				if (modifiers.HasFlag(CCodeModifiers.PRINTF)) {
					format_arg_index = (format_arg_index >= 0 ? format_arg_index + 1 : args_index);
					writer.write_string(" G_GNUC_PRINTF(%d,%d)".printf(format_arg_index, args_index + 1));
				} else if (modifiers.HasFlag(CCodeModifiers.SCANF)) {
					format_arg_index = (format_arg_index >= 0 ? format_arg_index + 1 : args_index);
					writer.write_string(" G_GNUC_SCANF(%d,%d)".printf(format_arg_index, args_index + 1));
				} else if (format_arg_index >= 0) {
					writer.write_string(" G_GNUC_FORMAT(%d)".printf(format_arg_index + 1));
				}

				if (modifiers.HasFlag(CCodeModifiers.CONST)) {
					writer.write_string(" G_GNUC_CONST");
				}
				if (modifiers.HasFlag(CCodeModifiers.UNUSED)) {
					writer.write_string(" G_GNUC_UNUSED");
				}

				if (modifiers.HasFlag(CCodeModifiers.CONSTRUCTOR)) {
					writer.write_string(" __attribute__((constructor))");
				} else if (modifiers.HasFlag(CCodeModifiers.DESTRUCTOR)) {
					writer.write_string(" __attribute__((destructor))");
				}

				writer.write_string(";");
			} else {
				block.write(writer);
				writer.write_newline();
			}
			writer.write_newline();
		}

		public void add_statement(CCodeNode stmt) {
			stmt.line = current_line;
			current_block.add_statement(stmt);
		}

		public void open_block() {
			statement_stack.Add(current_block);
			var parent_block = current_block;

			current_block = new CCodeBlock();

			parent_block.add_statement(current_block);
		}

		public void open_if(CCodeExpression condition) {
			statement_stack.Add(current_block);
			var parent_block = current_block;

			current_block = new CCodeBlock();

			var cif = new CCodeIfStatement(condition, current_block);
			cif.line = current_line;
			statement_stack.Add(cif);

			parent_block.add_statement(cif);
		}

		public void add_else() {
			current_block = new CCodeBlock();

			var cif = (CCodeIfStatement)statement_stack[statement_stack.Count - 1];
			cif.line = current_line;
			Debug.Assert(cif.false_statement == null);
			cif.false_statement = current_block;
		}

		public void else_if(CCodeExpression condition) {
			var parent_if = (CCodeIfStatement)statement_stack[statement_stack.Count - 1];
			statement_stack.RemoveAt(statement_stack.Count - 1);
			Debug.Assert(parent_if.false_statement == null);

			current_block = new CCodeBlock();

			var cif = new CCodeIfStatement(condition, current_block);
			cif.line = current_line;
			parent_if.false_statement = cif;
			statement_stack.Add(cif);
		}

		public void open_while(CCodeExpression condition) {
			statement_stack.Add(current_block);
			var parent_block = current_block;

			current_block = new CCodeBlock();

			var cwhile = new CCodeWhileStatement(condition, current_block);
			cwhile.line = current_line;
			parent_block.add_statement(cwhile);
		}

		public void open_for(CCodeExpression initializer, CCodeExpression condition, CCodeExpression iterator) {
			statement_stack.Add(current_block);
			var parent_block = current_block;

			current_block = new CCodeBlock();

			var cfor = new CCodeForStatement(condition, current_block);
			cfor.line = current_line;
			if (initializer != null) {
				cfor.add_initializer(initializer);
			}
			if (iterator != null) {
				cfor.add_iterator(iterator);
			}

			parent_block.add_statement(cfor);
		}

		public void open_switch(CCodeExpression expression) {
			statement_stack.Add(current_block);
			var parent_block = current_block;

			var cswitch = new CCodeSwitchStatement(expression);
			cswitch.line = current_line;
			current_block = cswitch;

			parent_block.add_statement(cswitch);
		}

		public void add_label(string label) {
			add_statement(new CCodeLabel(label));
		}

		public void add_case(CCodeExpression expression) {
			add_statement(new CCodeCaseStatement(expression));
		}

		public void add_default() {
			add_statement(new CCodeLabel("default"));
		}

		public void add_goto(string target) {
			add_statement(new CCodeGotoStatement(target));
		}

		public void add_expression(CCodeExpression expression) {
			add_statement(new CCodeExpressionStatement(expression));
		}

		public void add_assignment(CCodeExpression left, CCodeExpression right) {
			add_expression(new CCodeAssignment(left, right));
		}

		public void add_return(CCodeExpression expression = null) {
			add_statement(new CCodeReturnStatement(expression));
		}

		public void add_break() {
			add_statement(new CCodeBreakStatement());
		}

		public void add_continue() {
			add_statement(new CCodeContinueStatement());
		}

		public void add_declaration(string type_name, CCodeDeclarator declarator, CCodeModifiers modifiers = 0) {
			var stmt = new CCodeDeclaration(type_name);
			stmt.add_declarator(declarator);
			stmt.modifiers = modifiers;
			add_statement(stmt);
		}

		public void close() {
			do {
				var top = statement_stack[statement_stack.Count - 1];
				statement_stack.RemoveAt(statement_stack.Count - 1);
				current_block = top as CCodeBlock;
			} while (current_block == null);
		}
	}

}
