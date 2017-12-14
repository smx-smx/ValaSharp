using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.CodeNodes;
using Vala.Lang.Expressions;
using Vala.Lang.Methods;
using Vala.Lang.Parser;
using Vala.Lang.Statements;
using Vala.Lang.Types;
using Vala.Lang.TypeSymbols;

namespace Vala.Lang.Symbols {
	public class Signal : Symbol, Lockable, Callable {
		/// <summary>
		/// The return type of handlers of this signal.
		/// </summary>
		public DataType return_type {
			get { return _return_type; }
			set {
				_return_type = value;
				_return_type.parent_node = this;
			}
		}

		public Block body {
			get { return _body; }
			set {
				_body = value;
				if (_body != null) {
					_body.owner = scope;
				}
			}
		}

		/// <summary>
		/// Specifies whether this signal has virtual method handler.
		/// </summary>
		public bool is_virtual { get; set; }

		private List<Parameter> parameters = new List<Parameter>();
		/// <summary>
		/// Refers to the default signal handler, which is an anonymous
		/// function in the scope.
		/// 
		/// </summary>
		public Method default_handler { get; private set; }

		/// <summary>
		/// Refers to the public signal emitter method, which is an anonymous
		/// function in the scope.
		/// 
		/// </summary>
		public Method emitter { get; private set; }

		private bool lock_used = false;

		private DataType _return_type;

		private Block _body;

		/// <summary>
		/// Creates a new signal.
		/// 
		/// <param name="name">signal name</param>
		/// <param name="return_type">signal return type</param>
		/// <param name="source_reference">reference to source code</param>
		/// <returns>newly created signal</returns>
		/// </summary>
		public Signal(string name, DataType return_type, SourceReference source_reference = null, Comment comment = null) : base(name, source_reference, comment) {
			this.return_type = return_type;
		}

		/// <summary>
		/// Appends parameter to signal handler.
		/// 
		/// <param name="param">a formal parameter</param>
		/// </summary>
		public void add_parameter(Parameter param) {
			parameters.Add(param);
			scope.add(param.name, param);
		}

		public List<Parameter> get_parameters() {
			return parameters;
		}

		/// <summary>
		/// Returns generated delegate to be used for signal handlers.
		/// 
		/// <returns>delegate</returns>
		/// </summary>
		public ValaDelegate get_delegate(DataType sender_type, CodeNode node_reference) {
			var actual_return_type = return_type.get_actual_type(sender_type, null, node_reference);

			var generated_delegate = new ValaDelegate(null, actual_return_type);
			generated_delegate.access = SymbolAccessibility.PUBLIC;
			generated_delegate.owner = scope;

			// sender parameter is never null and doesn't own its value
			var sender_param_type = sender_type.copy();
			sender_param_type.value_owned = false;
			sender_param_type.nullable = false;

			generated_delegate.sender_type = sender_param_type;

			bool is_generic = false;

			foreach (Parameter param in parameters) {
				var actual_param = param.copy();
				actual_param.variable_type = actual_param.variable_type.get_actual_type(sender_type, null, node_reference);
				generated_delegate.add_parameter(actual_param);

				if (actual_param.variable_type is GenericType) {
					is_generic = true;
				}
			}

			if (is_generic) {
				var cl = (ObjectTypeSymbol)parent_symbol;
				foreach (var type_param in cl.get_type_parameters()) {
					generated_delegate.add_type_parameter(new TypeParameter(type_param.name, type_param.source_reference));
				}

				// parameter types must refer to the delegate type parameters
				// instead of to the class type parameters
				foreach (var param in generated_delegate.get_parameters()) {
					if (param.variable_type is GenericType) {
						param.variable_type.type_parameter = generated_delegate.get_type_parameters()[generated_delegate.get_type_parameter_index(param.variable_type.type_parameter.name)];
					}
				}
			}

			scope.add(null, generated_delegate);

			return generated_delegate;
		}

		public override void accept(CodeVisitor visitor) {
			visitor.visit_signal(this);
		}

		public override void accept_children(CodeVisitor visitor) {
			return_type.accept(visitor);

			foreach (Parameter param in parameters) {
				param.accept(visitor);
			}
			if (default_handler == null && body != null) {
				body.accept(visitor);
			} else if (default_handler != null) {
				default_handler.accept(visitor);
			}
			if (emitter != null) {
				emitter.accept(visitor);
			}
		}

		public bool get_lock_used() {
			return lock_used;
		}

		public void set_lock_used(bool used) {
			lock_used = used;
		}

		public override void replace_type(DataType old_type, DataType new_type) {
			if (return_type == old_type) {
				return_type = new_type;
			}
		}

		public override bool check(CodeContext context) {
			if (is_checked) {
				return !error;
			}

			is_checked = true;

			return_type.check(context);

			foreach (Parameter param in parameters) {
				if (param.ellipsis) {
					Report.error(param.source_reference, "Signals with variable argument lists are not supported");
					return false;
				}

				param.check(context);
			}

			if (!is_virtual && body != null) {
				Report.error(source_reference, "Only virtual signals can have a default signal handler body");
			}


			if (is_virtual) {
				default_handler = new Method(name, return_type, source_reference);

				default_handler.owner = owner;
				default_handler.access = access;
				default_handler.external = external;
				default_handler.hides = hides;
				default_handler.is_virtual = true;
				default_handler.signal_reference = this;
				default_handler.body = body;


				foreach (Parameter param in parameters) {
					default_handler.add_parameter(param);
				}

				var cl = parent_symbol as ObjectTypeSymbol;

				cl.add_hidden_method(default_handler);
				default_handler.check(context);
			}

			if (!external_package && get_attribute("HasEmitter") != null) {
				emitter = new Method(name, return_type, source_reference);

				emitter.owner = owner;
				emitter.access = access;

				var body = new Block(source_reference);
				var call = new MethodCall(MemberAccess.simple(name, source_reference), source_reference);

				foreach (Parameter param in parameters) {
					emitter.add_parameter(param);
					call.add_argument(MemberAccess.simple(param.name, source_reference));
				}

				if (return_type is VoidType) {
					body.add_statement(new ExpressionStatement(call, source_reference));
				} else {
					body.add_statement(new ReturnStatement(call, source_reference));
				}
				emitter.body = body;

				var cl = parent_symbol as ObjectTypeSymbol;

				cl.add_hidden_method(emitter);
				emitter.check(context);
			}


			if (!external_package && !hides && get_hidden_member() != null) {
				Report.warning(source_reference, "%s hides inherited signal `%s'. Use the `new' keyword if hiding was intentional".printf(get_full_name(), get_hidden_member().get_full_name()));
			}

			return !error;
		}
	}
}

