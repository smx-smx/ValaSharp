using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.CodeNodes;
using Vala.Lang.Expressions;
using Vala.Lang.Parser;
using Vala.Lang.Types;

namespace Vala.Lang.Symbols {
	public class Variable : Symbol {
		/**
	 * The optional initializer expression.
	 */
		public Expression initializer {
			get {
				return _initializer;
			}
			set {
				_initializer = value;
				if (_initializer != null) {
					_initializer.parent_node = this; ;
				}
			}
		}

		/**
		 * The variable type.
		 */
		public DataType variable_type {
			get { return _variable_type; }
			set {
				_variable_type = value;
				if (_variable_type != null) {
					_variable_type.parent_node = this;
				}
			}
		}

		public bool single_assignment { get; set; }

		Expression _initializer;
		DataType _variable_type;

		public Variable(DataType variable_type, string name, Expression initializer = null, SourceReference source_reference = null, Comment comment = null)
			: base(name, source_reference, comment) {
			this.variable_type = variable_type;
			this.initializer = initializer;
		}
	}
}
