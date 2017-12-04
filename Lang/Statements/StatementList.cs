﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang.Code;
using Vala.Lang.CodeNodes;
using Vala.Lang.Parser;

namespace Vala.Lang.Statements {
	public class StatementList : CodeNode, Statement {
		public CodeNode node {
			get { return this; }
		}

		private List<Statement> list = new List<Statement>();

		public int length {
			get { return list.Count; }
		}

		public StatementList(SourceReference source_reference) {
			this.source_reference = source_reference;
		}

		public Statement get(int index) {
			return list[index];
		}

		public void set(int index, Statement stmt) {
			list[index] = stmt;
		}

		public void add(Statement stmt) {
			list.Add(stmt);
		}

		public void insert(int index, Statement stmt) {
			list.Insert(index, stmt);
		}

		public override void accept(CodeVisitor visitor) {
			foreach (Statement stmt in list) {
				stmt.node.accept(visitor);
			}
		}

		public override void emit(CodeGenerator codegen) {
			foreach (Statement stmt in list) {
				stmt.node.emit(codegen);
			}
		}
	}
}
