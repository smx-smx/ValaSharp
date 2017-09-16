using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.CodeNodes;
using Vala.Lang.Parser;

namespace Vala.Lang.Statements
{
	/**
	* Interface for all statement types.
	*/
	public interface Statement {
		CodeNode node { get; }
	}
}
