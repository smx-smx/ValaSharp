using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang;
using Vala.Lang.CodeNodes;
using Vala.Lang.Expressions;
using Vala.Lang.Methods;
using Vala.Lang.Parser;
using Vala.Lang.Statements;
using Vala.Lang.Symbols;
using Vala.Lang.Types;
using Vala.Lang.TypeSymbols;

namespace ValaLanguageServer {
	/// <summary>
	/// A visitor that traverses all elements
	/// </summary>
	public abstract class CodeTraversalVisitor : CodeVisitor {
		public override void visit_block(Block b) {
			b.accept_children(this);
		}

		public override void visit_namespace(Namespace ns) {
			ns.accept_children(this);
		}

		public override void visit_class(Class cl) {
			cl.accept_children(this);
		}

		public override void visit_interface(Interface iface) {
			iface.accept_children(this);
		}

		public override void visit_enum_value(EnumValue ev) {
			ev.accept_children(this);
		}

		public override void visit_formal_parameter(Parameter p) {
			p.accept_children(this);
		}

		public override void visit_field(Field f) {
			f.accept_children(this);
		}

		public override void visit_local_variable(LocalVariable local) {
			local.accept_children(this);
		}

		public override void visit_property(Property prop) {
			prop.accept_children(this);
		}

		public override void visit_catch_clause(CatchClause clause) {
			clause.accept_children(this);
		}

		public override void visit_switch_label(SwitchLabel label) {
			label.accept_children(this);
		}

		public override void visit_switch_section(SwitchSection section) {
			section.accept_children(this);
		}

		public override void visit_constructor(Constructor c) {
			c.accept_children(this);
		}

		public override void visit_property_accessor(PropertyAccessor acc) {
			acc.accept_children(this);
		}

		public override void visit_destructor(Destructor d) {
			d.accept_children(this);
		}

		public override void visit_struct(Struct st) {
			st.accept_children(this);
		}

		public override void visit_error_domain(ErrorDomain edomain) {
			edomain.accept_children(this);
		}

		public override void visit_enum(ValaEnum en) {
			en.accept_children(this);
		}

		public override void visit_delegate(ValaDelegate d) {
			d.accept_children(this);
		}

		public override void visit_constant(Constant c) {
			c.accept_children(this);
		}

		public override void visit_typeof_expression(TypeofExpression expr) {
			expr.accept_children(this);
		}

		public override void visit_template(Template tmpl) {
			tmpl.accept_children(this);
		}

		public override void visit_initializer_list(InitializerList list) {
			list.accept_children(this);
		}

		public override void visit_signal(Signal sig) {
			sig.accept_children(this);
		}

		public override void visit_reference_transfer_expression(ReferenceTransferExpression expr) {
			expr.accept_children(this);
		}

		public override void visit_object_creation_expression(ObjectCreationExpression expr) {
			expr.accept_children(this);
		}

		public override void visit_named_argument(NamedArgument expr) {
			expr.accept_children(this);
		}

		public override void visit_method(Method m) {
			m.accept_children(this);
		}

		public override void visit_method_call(MethodCall expr) {
			expr.accept_children(this);
		}

		public override void visit_conditional_expression(ConditionalExpression expr) {
			expr.accept_children(this);
		}

		public override void visit_cast_expression(CastExpression expr) {
			expr.accept_children(this);
		}

		public override void visit_type_check(TypeCheck expr) {
			expr.accept_children(this);
		}

		public override void visit_member_access(MemberAccess expr) {
			expr.accept_children(this);
		}

		public override void visit_for_statement(ForStatement stmt) {
			stmt.accept_children(this);
		}

		public override void visit_do_statement(DoStatement stmt) {
			stmt.accept_children(this);
		}

		public override void visit_return_statement(ReturnStatement stmt) {
			stmt.accept_children(this);
		}

		public override void visit_delete_statement(DeleteStatement stmt) {
			stmt.accept_children(this);
		}

		public override void visit_try_statement(TryStatement stmt) {
			stmt.accept_children(this);
		}

		public override void visit_loop(Loop stmt) {
			stmt.accept_children(this);
		}

		public override void visit_declaration_statement(DeclarationStatement stmt) {
			stmt.accept_children(this);
		}

		public override void visit_yield_statement(YieldStatement y) {
			y.accept_children(this);
		}

		public override void visit_throw_statement(ThrowStatement stmt) {
			stmt.accept_children(this);
		}

		public override void visit_data_type(DataType type) {
			type.accept_children(this);
		}

		public override void visit_if_statement(IfStatement stmt) {
			stmt.accept_children(this);
		}

		public override void visit_while_statement(WhileStatement stmt) {
			stmt.accept_children(this);
		}

		public override void visit_postfix_expression(PostfixExpression expr) {
			expr.accept_children(this);
		}

		public override void visit_sizeof_expression(SizeofExpression expr) {
			expr.accept_children(this);
		}

		public override void visit_addressof_expression(AddressofExpression expr) {
			expr.accept_children(this);
		}

		public override void visit_assignment(Assignment a) {
			a.accept_children(this);
		}

		public override void visit_element_access(ElementAccess expr) {
			expr.accept_children(this);
		}

		public override void visit_tuple(ValaTuple tuple) {
			tuple.accept_children(this);
		}

		public override void visit_slice_expression(SliceExpression expr) {
			expr.accept_children(this);
		}

		public override void visit_unary_expression(UnaryExpression expr) {
			expr.accept_children(this);
		}

		public override void visit_lambda_expression(LambdaExpression expr) {
			expr.accept_children(this);
		}

		public override void visit_foreach_statement(ForeachStatement stmt) {
			stmt.accept_children(this);
		}

		public override void visit_creation_method(CreationMethod m) {
			m.accept_children(this);
		}

		public override void visit_source_file(SourceFile source_file) {
			source_file.accept_children(this);
		}

		public override void visit_expression_statement(ExpressionStatement stmt) {
			stmt.accept_children(this);
		}

		public override void visit_switch_statement(SwitchStatement stmt) {
			stmt.accept_children(this);
		}

		public override void visit_array_creation_expression(ArrayCreationExpression expr) {
			expr.accept_children(this);
		}

		public override void visit_binary_expression(BinaryExpression expr) {
			expr.accept_children(this);
		}
	}
}
