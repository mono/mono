//
// statement.cs: Statement representation for the IL tree.
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

using System;

namespace CIR {

	using System.Collections;
	
	public abstract class Statement {
	}

	public class EmptyStatement : Statement {
	}
	
	public class If : Statement {
		Expression expr;
		Statement trueStatement;
		Statement falseStatement;
		
		public If (Expression expr, Statement trueStatement)
		{
			this.expr = expr;
			this.trueStatement = trueStatement;
		}

		public If (Expression expr,
			   Statement trueStatement,
			   Statement falseStatement)
		{
			this.expr = expr;
			this.trueStatement = trueStatement;
			this.falseStatement = falseStatement;
		}

		public Statement TrueStatement {
			get {
				return trueStatement;
			}
		}

		public Statement FalseStatement {
			get {
				return falseStatement;
			}
		}

		public Expression Expr {
			get {
				return expr;
			}
		}
	}

	public class Do : Statement {
		Expression boolExpr;
		Statement statement;
		
		public Do (Statement statement, Expression boolExpr)
		{
			this.boolExpr = boolExpr;
			this.statement = statement;
		}

		public Statement EmbeddedStatement {
			get {
				return statement;
			}
		}

		public Expression Expr {
			get {
				return boolExpr;
			}
		}
	}

	public class While : Statement {
		Expression boolExpr;
		Statement statement;
		
		public While (Expression boolExpr, Statement statement)
		{
			this.boolExpr = boolExpr;
			this.statement = statement;
		}

		public Statement Statement {
			get {
				return statement;
			}
		}

		public Expression Expr {
			get {
				return boolExpr;
			}
		}
	}

	public class For : Statement {
		Statement initStatement;
		Expression test;
		Statement increment;
		Statement statement;
		
		public For (Statement initStatement,
			    Expression test,
			    Statement increment,
			    Statement statement)
		{
			this.initStatement = initStatement;
			this.test = test;
			this.increment = increment;
			this.statement = statement;
		}

		public Statement InitStatement {
			get {
				return initStatement;
			}
		}

		public Expression Test {
			get {
				return test;
			}
		}
		
		public Statement Increment {
			get {
				return increment;
			}
		}

		public Statement Statement {
			get {
				return statement;
			}
		}

	}
	
	public class StatementExpression : Statement {
		Expression expr;
		
		public StatementExpression (Expression expr)
		{
			this.expr = expr;
		}

		public Expression Expr {
			get {
				return expr;
			}
		}
	}

	public class Return : Statement {
		Expression expr;
		
		public Return (Expression expr)
		{
			this.expr = expr;
		}

		public Expression Expr {
			get {
				return expr;
			}
		}
	}

	public class Goto : Statement {
		string target;
		
		public Goto (string label)
		{
			target = label;
		}

		public string Target {
			get {
				return target;
			}
		}
	}

	public class Throw : Statement {
		Expression expr;
		
		public Throw (Expression expr)
		{
			this.expr = expr;
		}
	}

	public class Break : Statement {
		public Break ()
		{
		}
	}

	public class Continue : Statement {
		public Continue ()
		{
		}
	}
	
	// <summary>
	//   Used for Label management
	// </summary>
	//
	public class Block : Statement {
		Block parent;
		StatementCollection statements;
		Hashtable labels;
		Hashtable labels_referenced;
		bool implicit_block;
		Hashtable variables;
		string label;
		int internal_id_serial;
		bool used = false;
		
		public Block (Block parent)
		{
			this.parent = parent;
			this.implicit_block = false;
		}

		public Block (Block parent, bool implicit_block)
		{
			this.parent = parent;
			this.implicit_block = true;
		}

		public Block (Block parent, string labeled)
		{
			this.parent = parent;
			this.implicit_block = true;
			label = labeled;
		}
		
		public Block Parent {
			get {
				return parent;
			}
		}

		public bool Implicit {
			get {
				return implicit_block;
			}
		}

		public string Label {
			get {
				return label;
			}
		}
		
		// <summary>
		//   Adds a label to the current block. 
		// </summary>
		//
		// <returns>
		//   false if the name already exists in this block. true
		//   otherwise.
		// </returns>
		//
		public bool AddLabel (string name, Block block)
		{
			if (labels == null)
				labels = new Hashtable ();
			if (labels.Contains (name))
				return false;
			
			labels.Add (name, block);
			return true;
		}

		public bool AddVariable (string type, string name)
		{
			if (variables == null)
				variables = new Hashtable ();

			if (GetVariableType (name) != null)
				return false;
			
			variables.Add (name, type);
			return true;
		}

		public Hashtable Variables {
			get {
				return variables;
			}
		}

		public string GetVariableType (string name)
		{
			string type = null;

			if (variables != null)
				type = (string) variables [name];
			if (type != null)
				return type;
			else if (parent != null)
				return parent.GetVariableType (name);
			return null;
		}

		// <summary>
		//   True if the variable named @name has been defined
		//   in this block
		// </summary>
		public bool IsVariableDefined (string name)
		{
			return GetVariableType (name) != null;
		}

		// <summary>
		//   Use to fetch the statement associated with this label
		// </summary>
		public Statement this [string name] {
			get {
				return (Statement) labels [name];
			}
		}

		// <summary>
		//   Call this to label the label as used in a block
		// </summary>
		public void Reference (string name)
		{
			if (labels_referenced == null)
				labels_referenced = new Hashtable ();
			labels_referenced.Add (name, null);
		}

		// <returns>
		//   A list of labels that were not used within this block
		// </returns>
		public string [] GetUnreferenced ()
		{
			ArrayList unrefs = new ArrayList ();

			foreach (string s in (string []) labels.Keys) {
				if (labels_referenced [s] == null)
					unrefs.Add (s);
			}

			string [] result = new string [unrefs.Count];
			unrefs.CopyTo (result);
			return result;
		}

		public StatementCollection Statements {
			get {
				if (statements == null)
					statements = new StatementCollection ();

				return statements;
			}
		}

		public void AddStatement (Statement s)
		{
			if (statements == null)
				statements = new StatementCollection ();

			statements.Add (s);
			used = true;
		}

		public bool Used {
			get {
				return used;
			}
		}
		
		// <summary>
		//   Creates a compiler-internal identifier, this is
		//   used to create temporary variables that should not
		//   be seen by the application
		// </summary
		public string MakeInternalID () {
			string ret = internal_id_serial.ToString ();

			internal_id_serial++;
			return "0_" + ret;
		}
	}

	public class SwitchLabel {
		Expression label;

		//
		// if expr == null, then it is the default case.
		//
		public SwitchLabel (Expression expr)
		{
			label = expr;
		}
		
		public Expression Label {
			get {
				return label;
			}
		}
	}

	public class SwitchSection {
		// An array of SwitchLabels.
		ArrayList labels;
		Block block;
		
		public SwitchSection (ArrayList labels, Block block)
		{
			this.labels = labels;
			this.block = block;
		}

		public Block Block {
			get {
				return block;
			}
		}

		public ArrayList Labels {
			get {
				return labels;
			}
		}
	}
	
	public class Switch : Statement {
		ArrayList sections;
		Expression expr;
		
		public Switch (Expression expr, ArrayList sections)
		{
			this.expr = expr;
			this.sections = sections;
		}

		public Expression Expr {
			get {
				return expr;
			}
		}

		public ArrayList Sections {
			get {
				return sections;
			}
		}
	}

	public class Lock : Statement {
		Expression expr;
		Statement stmt;
		
		public Lock (Expression expr, Statement stmt)
		{
			this.expr = expr;
			this.stmt = stmt;
		}

		public Statement Statement {
			get {
				return stmt;
			}
		}

		public Expression Expr {
			get {
				return expr;
			}
		}
		
	}

	public class Unchecked : Statement {
		Block b;
		
		public Unchecked (Block b)
		{
			this.b = b;
		}

		public Block Block {
			get {
				return b;
			}
		}
	}

	public class Checked : Statement {
		Block b;
		
		public Checked (Block b)
		{
			this.b = b;
		}

		public Block Block {
			get {
				return b;
			}
		}
	}

	public class Try : Statement {
		Block fini, block;
		ArrayList specific;
		Catch general;
		
		//
		// specific, general and fini might all be null.
		//
		public Try (Block block, ArrayList specific, Catch general, Block fini)
		{
			if (specific == null && general == null){
				Console.WriteLine ("CIR.Try: Either specific or general have to be non-null");
			}
			
			this.block = block;
			this.specific = specific;
			this.general = general;
			this.fini = fini;
		}

		public Block Block {
			get {
				return block;
			}
		}

		public ArrayList Specific {
			get {
				return specific;
			}
		}

		public Catch General {
			get {
				return general;
			}
		}

		public Block Fini {
			get {
				return fini;
			}
		}
	}
	
	public class Catch {
		string type;
		string name;
		Block block;
		
		public Catch (string type, string name, Block block)
		{
			this.type = type;
			this.name = name;
			this.block = block;
		}

		public Block Block {
			get {
				return block;
			}
		}

		public string Name {
			get {
				return name;
			}
		}

		public string Type {
			get {
				return type;
			}
		}
	}
}

