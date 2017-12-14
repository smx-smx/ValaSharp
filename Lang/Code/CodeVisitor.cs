using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.Parser;
using Vala.Lang.Methods;
using Vala.Lang.Symbols;
using Vala.Lang.TypeSymbols;
using Vala.Lang.Types;
using Vala.Lang.Expressions;
using Vala.Lang.Statements;
using Vala.Lang.Literals;

namespace Vala.Lang.CodeNodes {
	/// <summary>
	/// Abstract code node visitor for traversing source code tree.
	/// </summary>
	public abstract class CodeVisitor {
		/// <summary>
		/// Visit operation called for source files.
		/// 
		/// <param name="source_file">a source file</param>
		/// </summary>
		public virtual void visit_source_file(SourceFile source_file) {
		}

		/// <summary>
		/// Visit operation called for namespaces.
		/// 
		/// <param name="ns">a namespace</param>
		/// </summary>
		public virtual void visit_namespace(Namespace ns) {
		}

		/// <summary>
		/// Visit operation called for classes.
		/// 
		/// <param name="cl">a class</param>
		/// </summary>
		public virtual void visit_class(Class cl) {
		}

		/// <summary>
		/// Visit operation called for structs.
		/// 
		/// <param name="st">a struct</param>
		/// </summary>
		public virtual void visit_struct(Struct st) {
		}

		/// <summary>
		/// Visit operation called for interfaces.
		/// 
		/// <param name="iface">an interface</param>
		/// </summary>
		public virtual void visit_interface(Interface iface) {
		}

		/// <summary>
		/// Visit operation called for enums.
		/// 
		/// <param name="en">an enum</param>
		/// </summary>
		public virtual void visit_enum(ValaEnum en) {
		}

		/// <summary>
		/// Visit operation called for enum values.
		/// 
		/// <param name="ev">an enum value</param>
		/// </summary>
		public virtual void visit_enum_value(EnumValue ev) {
		}

		/// <summary>
		/// Visit operation called for error domains.
		/// 
		/// <param name="edomain">an error domain</param>
		/// </summary>
		public virtual void visit_error_domain(ErrorDomain edomain) {
		}

		/// <summary>
		/// Visit operation called for error codes.
		/// 
		/// <param name="ecode">an error code</param>
		/// </summary>
		public virtual void visit_error_code(ErrorCode ecode) {
		}

		/// <summary>
		/// Visit operation called for delegates.
		/// 
		/// <param name="d">a delegate</param>
		/// </summary>
		public virtual void visit_delegate(ValaDelegate d) {
		}

		/// <summary>
		/// Visit operation called for constants.
		/// 
		/// <param name="c">a constant</param>
		/// </summary>
		public virtual void visit_constant(Constant c) {
		}

		/// <summary>
		/// Visit operation called for fields.
		/// 
		/// <param name="f">a field</param>
		/// </summary>
		public virtual void visit_field(Field f) {
		}

		/// <summary>
		/// Visit operation called for methods.
		/// 
		/// <param name="m">a method</param>
		/// </summary>
		public virtual void visit_method(Method m) {
		}

		/// <summary>
		/// Visit operation called for creation methods.
		/// 
		/// <param name="m">a method</param>
		/// </summary>
		public virtual void visit_creation_method(CreationMethod m) {
		}

		/// <summary>
		/// Visit operation called for formal parameters.
		/// 
		/// <param name="p">a formal parameter</param>
		/// </summary>
		public virtual void visit_formal_parameter(Parameter p) {
		}

		/// <summary>
		/// Visit operation called for properties.
		/// 
		/// <param name="prop">a property</param>
		/// </summary>
		public virtual void visit_property(Property prop) {
		}

		/// <summary>
		/// Visit operation called for property accessors.
		/// 
		/// <param name="acc">a property accessor</param>
		/// </summary>
		public virtual void visit_property_accessor(PropertyAccessor acc) {
		}

		/// <summary>
		/// Visit operation called for signals.
		/// 
		/// <param name="sig">a signal</param>
		/// </summary>
		public virtual void visit_signal(Signal sig) {
		}

		/// <summary>
		/// Visit operation called for constructors.
		/// 
		/// <param name="c">a constructor</param>
		/// </summary>
		public virtual void visit_constructor(Constructor c) {
		}

		/// <summary>
		/// Visit operation called for destructors.
		/// 
		/// <param name="d">a destructor</param>
		/// </summary>
		public virtual void visit_destructor(Destructor d) {
		}

		/// <summary>
		/// Visit operation called for type parameters.
		/// 
		/// <param name="p">a type parameter</param>
		/// </summary>
		public virtual void visit_type_parameter(TypeParameter p) {
		}

		/// <summary>
		/// Visit operation called for using directives.
		/// 
		/// <param name="ns">a using directive</param>
		/// </summary>
		public virtual void visit_using_directive(UsingDirective ns) {
		}

		/// <summary>
		/// Visit operation called for type references.
		/// 
		/// <param name="type">a type reference</param>
		/// </summary>
		public virtual void visit_data_type(DataType type) {
		}

		/// <summary>
		/// Visit operation called for blocks.
		/// 
		/// <param name="b">a block</param>
		/// </summary>
		public virtual void visit_block(Block b) {
		}

		/// <summary>
		/// Visit operation called for empty statements.
		/// 
		/// <param name="stmt">an empty statement</param>
		/// </summary>
		public virtual void visit_empty_statement(EmptyStatement stmt) {
		}

		/// <summary>
		/// Visit operation called for declaration statements.
		/// 
		/// <param name="stmt">a declaration statement</param>
		/// </summary>
		public virtual void visit_declaration_statement(DeclarationStatement stmt) {
		}

		/// <summary>
		/// Visit operation called for local variables.
		/// 
		/// <param name="local">a local variable</param>
		/// </summary>
		public virtual void visit_local_variable(LocalVariable local) {
		}

		/// <summary>
		/// Visit operation called for initializer lists
		/// 
		/// <param name="list">an initializer list</param>
		/// </summary>
		public virtual void visit_initializer_list(InitializerList list) {
		}

		/// <summary>
		/// Visit operation called for expression statements.
		/// 
		/// <param name="stmt">an expression statement</param>
		/// </summary>
		public virtual void visit_expression_statement(ExpressionStatement stmt) {
		}

		/// <summary>
		/// Visit operation called for if statements.
		/// 
		/// <param name="stmt">an if statement</param>
		/// </summary>
		public virtual void visit_if_statement(IfStatement stmt) {
		}

		/// <summary>
		/// Visit operation called for switch statements.
		/// 
		/// <param name="stmt">a switch statement</param>
		/// </summary>
		public virtual void visit_switch_statement(SwitchStatement stmt) {
		}

		/// <summary>
		/// Visit operation called for switch sections.
		/// 
		/// <param name="section">a switch section</param>
		/// </summary>
		public virtual void visit_switch_section(SwitchSection section) {
		}

		/// <summary>
		/// Visit operation called for switch label.
		/// 
		/// <param name="label">a switch label</param>
		/// </summary>
		public virtual void visit_switch_label(SwitchLabel label) {
		}

		/// <summary>
		/// Visit operation called for loops.
		/// 
		/// <param name="stmt">a loop</param>
		/// </summary>
		public virtual void visit_loop(Loop stmt) {
		}

		/// <summary>
		/// Visit operation called for while statements.
		/// 
		/// <param name="stmt">an while statement</param>
		/// </summary>
		public virtual void visit_while_statement(WhileStatement stmt) {
		}

		/// <summary>
		/// Visit operation called for do statements.
		/// 
		/// <param name="stmt">a do statement</param>
		/// </summary>
		public virtual void visit_do_statement(DoStatement stmt) {
		}

		/// <summary>
		/// Visit operation called for for statements.
		/// 
		/// <param name="stmt">a for statement</param>
		/// </summary>
		public virtual void visit_for_statement(ForStatement stmt) {
		}

		/// <summary>
		/// Visit operation called for foreach statements.
		/// 
		/// <param name="stmt">a foreach statement</param>
		/// </summary>
		public virtual void visit_foreach_statement(ForeachStatement stmt) {
		}

		/// <summary>
		/// Visit operation called for break statements.
		/// 
		/// <param name="stmt">a break statement</param>
		/// </summary>
		public virtual void visit_break_statement(BreakStatement stmt) {
		}

		/// <summary>
		/// Visit operation called for continue statements.
		/// 
		/// <param name="stmt">a continue statement</param>
		/// </summary>
		public virtual void visit_continue_statement(ContinueStatement stmt) {
		}

		/// <summary>
		/// Visit operation called for return statements.
		/// 
		/// <param name="stmt">a return statement</param>
		/// </summary>
		public virtual void visit_return_statement(ReturnStatement stmt) {
		}

		/// <summary>
		/// Visit operation called for yield statement.
		/// 
		/// <param name="y">a yield statement</param>
		/// </summary>
		public virtual void visit_yield_statement(YieldStatement y) {
		}

		/// <summary>
		/// Visit operation called for throw statements.
		/// 
		/// <param name="stmt">a throw statement</param>
		/// </summary>
		public virtual void visit_throw_statement(ThrowStatement stmt) {
		}

		/// <summary>
		/// Visit operation called for try statements.
		/// 
		/// <param name="stmt">a try statement</param>
		/// </summary>
		public virtual void visit_try_statement(TryStatement stmt) {
		}

		/// <summary>
		/// Visit operation called for catch clauses.
		/// 
		/// <param name="clause">a catch cluase</param>
		/// </summary>
		public virtual void visit_catch_clause(CatchClause clause) {
		}

		/// <summary>
		/// Visit operation called for lock statements before the body has been visited.
		/// 
		/// <param name="stmt">a lock statement</param>
		/// </summary>
		public virtual void visit_lock_statement(LockStatement stmt) {
		}

		/// <summary>
		/// Visit operation called for unlock statements.
		/// 
		/// <param name="stmt">an unlock statement</param>
		/// </summary>
		public virtual void visit_unlock_statement(UnlockStatement stmt) {
		}

		/// <summary>
		/// Visit operation called for delete statements.
		/// 
		/// <param name="stmt">a delete statement</param>
		/// </summary>
		public virtual void visit_delete_statement(DeleteStatement stmt) {
		}

		/// <summary>
		/// Visit operations called for expresions.
		/// 
		/// <param name="expr">an expression</param>
		/// </summary>
		public virtual void visit_expression(Expression expr) {
		}

		/// <summary>
		/// Visit operations called for array creation expresions.
		/// 
		/// <param name="expr">an array creation expression</param>
		/// </summary>
		public virtual void visit_array_creation_expression(ArrayCreationExpression expr) {
		}

		/// <summary>
		/// Visit operation called for boolean literals.
		/// 
		/// <param name="lit">a boolean literal</param>
		/// </summary>
		public virtual void visit_boolean_literal(BooleanLiteral lit) {
		}

		/// <summary>
		/// Visit operation called for character literals.
		/// 
		/// <param name="lit">a character literal</param>
		/// </summary>
		public virtual void visit_character_literal(CharacterLiteral lit) {
		}

		/// <summary>
		/// Visit operation called for integer literals.
		/// 
		/// <param name="lit">an integer literal</param>
		/// </summary>
		public virtual void visit_integer_literal(IntegerLiteral lit) {
		}

		/// <summary>
		/// Visit operation called for real literals.
		/// 
		/// <param name="lit">a real literal</param>
		/// </summary>
		public virtual void visit_real_literal(RealLiteral lit) {
		}

		/// <summary>
		/// Visit operation called for regex literals.
		/// 
		/// <param name="lit">a regex literal</param>
		/// </summary>
		public virtual void visit_regex_literal(RegexLiteral lit) {
		}


		/// <summary>
		/// Visit operation called for string literals.
		/// 
		/// <param name="lit">a string literal</param>
		/// </summary>
		public virtual void visit_string_literal(StringLiteral lit) {
		}

		/// <summary>
		/// Visit operation called for string templates.
		/// 
		/// <param name="tmpl">a string template</param>
		/// </summary>
		public virtual void visit_template(Template tmpl) {
		}

		/// <summary>
		/// Visit operation called for tuples.
		/// 
		/// <param name="tuple">a tuple</param>
		/// </summary>
		public virtual void visit_tuple(ValaTuple tuple) {
		}

		/// <summary>
		/// Visit operation called for null literals.
		/// 
		/// <param name="lit">a null literal</param>
		/// </summary>
		public virtual void visit_null_literal(NullLiteral lit) {
		}

		/// <summary>
		/// Visit operation called for member access expressions.
		/// 
		/// <param name="expr">a member access expression</param>
		/// </summary>
		public virtual void visit_member_access(MemberAccess expr) {
		}

		/// <summary>
		/// Visit operation called for invocation expressions.
		/// 
		/// <param name="expr">an invocation expression</param>
		/// </summary>
		public virtual void visit_method_call(MethodCall expr) {
		}

		/// <summary>
		/// Visit operation called for element access expressions.
		/// 
		/// <param name="expr">an element access expression</param>
		/// </summary>
		public virtual void visit_element_access(ElementAccess expr) {
		}

		/// <summary>
		/// Visit operation called for array slice expressions.
		/// 
		/// <param name="expr">an array slice expression</param>
		/// </summary>
		public virtual void visit_slice_expression(SliceExpression expr) {
		}

		/// <summary>
		/// Visit operation called for base access expressions.
		/// 
		/// <param name="expr">a base access expression</param>
		/// </summary>
		public virtual void visit_base_access(BaseAccess expr) {
		}

		/// <summary>
		/// Visit operation called for postfix expressions.
		/// 
		/// <param name="expr">a postfix expression</param>
		/// </summary>
		public virtual void visit_postfix_expression(PostfixExpression expr) {
		}

		/// <summary>
		/// Visit operation called for object creation expressions.
		/// 
		/// <param name="expr">an object creation expression</param>
		/// </summary>
		public virtual void visit_object_creation_expression(ObjectCreationExpression expr) {
		}

		/// <summary>
		/// Visit operation called for sizeof expressions.
		/// 
		/// <param name="expr">a sizeof expression</param>
		/// </summary>
		public virtual void visit_sizeof_expression(SizeofExpression expr) {
		}

		/// <summary>
		/// Visit operation called for typeof expressions.
		/// 
		/// <param name="expr">a typeof expression</param>
		/// </summary>
		public virtual void visit_typeof_expression(TypeofExpression expr) {
		}

		/// <summary>
		/// Visit operation called for unary expressions.
		/// 
		/// <param name="expr">an unary expression</param>
		/// </summary>
		public virtual void visit_unary_expression(UnaryExpression expr) {
		}

		/// <summary>
		/// Visit operation called for call expressions.
		/// 
		/// <param name="expr">a call expression</param>
		/// </summary>
		public virtual void visit_cast_expression(CastExpression expr) {
		}

		/// <summary>
		/// Visit operation called for named arguments.
		/// 
		/// <param name="expr">a named argument</param>
		/// </summary>
		public virtual void visit_named_argument(NamedArgument expr) {
		}

		/// <summary>
		/// Visit operation called for pointer indirections.
		/// 
		/// <param name="expr">a pointer indirection</param>
		/// </summary>
		public virtual void visit_pointer_indirection(PointerIndirection expr) {
		}

		/// <summary>
		/// Visit operation called for address-of expressions.
		/// 
		/// <param name="expr">an address-of expression</param>
		/// </summary>
		public virtual void visit_addressof_expression(AddressofExpression expr) {
		}

		/// <summary>
		/// Visit operation called for reference transfer expressions.
		/// 
		/// <param name="expr">a reference transfer expression</param>
		/// </summary>
		public virtual void visit_reference_transfer_expression(ReferenceTransferExpression expr) {
		}

		/// <summary>
		/// Visit operation called for binary expressions.
		/// 
		/// <param name="expr">a binary expression</param>
		/// </summary>
		public virtual void visit_binary_expression(BinaryExpression expr) {
		}

		/// <summary>
		/// Visit operation called for type checks.
		/// 
		/// <param name="expr">a type check expression</param>
		/// </summary>
		public virtual void visit_type_check(TypeCheck expr) {
		}

		/// <summary>
		/// Visit operation called for conditional expressions.
		/// 
		/// <param name="expr">a conditional expression</param>
		/// </summary>
		public virtual void visit_conditional_expression(ConditionalExpression expr) {
		}

		/// <summary>
		/// Visit operation called for lambda expressions.
		/// 
		/// <param name="expr">a lambda expression</param>
		/// </summary>
		public virtual void visit_lambda_expression(LambdaExpression expr) {
		}

		/// <summary>
		/// Visit operation called for assignments.
		/// 
		/// <param name="a">an assignment</param>
		/// </summary>
		public virtual void visit_assignment(Assignment a) {
		}

		/// <summary>
		/// Visit operation called at end of full expressions.
		/// 
		/// <param name="expr">a full expression</param>
		/// </summary>
		public virtual void visit_end_full_expression(Expression expr) {
		}
	}
}
