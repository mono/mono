//
// statement.cs: Statement representation for the IL tree.
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

using System;
using System.Reflection;
using System.Reflection.Emit;

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
		public readonly Statement InitStatement;
		public readonly Expression Test;
		public readonly Statement Increment;
		public readonly Statement Statement;
		
		public For (Statement initStatement,
			    Expression test,
			    Statement increment,
			    Statement statement)
		{
			InitStatement = initStatement;
			Test = test;
			Increment = increment;
			Statement = statement;
		}
	}
	
	public class StatementExpression : Statement {
		ExpressionStatement expr;
		
		public StatementExpression (ExpressionStatement expr)
		{
			this.expr = expr;
		}

		public ExpressionStatement Expr {
			get {
				return expr;
			}
		}
	}

	public class Return : Statement {
		public readonly Expression Expr;
		
		public Return (Expression expr)
		{
			Expr = expr;
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
		public readonly Expression Expr;
		
		public Throw (Expression expr)
		{
			Expr = expr;
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
	
	public class VariableInfo {
		public readonly string Type;
		public LocalBuilder LocalBuilder;
		public Type VariableType;
		public readonly Location Location;
		
		int  idx;
		public bool Used;
		public bool Assigned; 
		
		public VariableInfo (string type, Location l)
		{
			Type = type;
			LocalBuilder = null;
			idx = -1;
			Location = l;
		}

		public int Idx {
			get {
				if (idx == -1)
					throw new Exception ("Unassigned idx for variable");
				
				return idx;
			}

			set {
				idx = value;
			}
		}

	}
		
	// <summary>
	//   Used for Label management
	// </summary>
	//
	public class Block : Statement {
		public readonly Block  Parent;
		public readonly bool   Implicit;
		public readonly string Label;

		//
		// The statements in this block
		//
		StatementCollection statements;

		//
		// An array of Blocks
		//
		ArrayList children;
		
		//
		// Labels.  (label, block) pairs.
		//
		Hashtable labels;

		//
		// Keeps track of (name, type) pairs
		//
		Hashtable variables;

		//
		// Maps variable names to ILGenerator.LocalBuilders
		//
		Hashtable local_builders;

		bool used = false;

		public Block (Block parent)
		{
			if (parent != null)
				parent.AddChild (this);
			
			this.Parent = parent;
			this.Implicit = false;
		}

		public Block (Block parent, bool implicit_block)
		{
			if (parent != null)
				parent.AddChild (this);
			
			this.Parent = parent;
			this.Implicit = true;
		}

		public Block (Block parent, string labeled)
		{
			if (parent != null)
				parent.AddChild (this);
			
			this.Parent = parent;
			this.Implicit = true;
			Label = labeled;
		}

		public void AddChild (Block b)
		{
			if (children == null)
				children = new ArrayList ();
			
			children.Add (b);
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

		public bool AddVariable (string type, string name, Location l)
		{
			if (variables == null)
				variables = new Hashtable ();

			if (GetVariableType (name) != null)
				return false;

			VariableInfo vi = new VariableInfo (type, l);
			
			variables.Add (name, vi);
			return true;
		}

		public Hashtable Variables {
			get {
				return variables;
			}
		}

		public VariableInfo GetVariableInfo (string name)
		{
			if (variables != null) {
				object temp;
				temp = variables [name];

				if (temp != null)
					return (VariableInfo) temp;
			}

			if (Parent != null){
				return Parent.GetVariableInfo (name);
			}

			return null;
		}
		
		public string GetVariableType (string name)
		{
			VariableInfo vi = GetVariableInfo (name);

			if (vi != null)
				return vi.Type;

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

		// <returns>
		//   A list of labels that were not used within this block
		// </returns>
		public string [] GetUnreferenced ()
		{
			// FIXME: Implement me
			return null;
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

		public void Use ()
		{
			used = true;
		}
		
		// <summary>
		//   Creates a compiler-internal identifier, this is
		//   used to create temporary variables that should not
		//   be seen by the application
		// </summary
		int internal_id_serial;
		public string MakeInternalID () {
			string ret = internal_id_serial.ToString ();

			internal_id_serial++;
			return "0_" + ret;
		}

		// <summary>
		//   Emits the variable declarations and labels.
		// </summary>
		//
		// tc: is our typecontainer (to resolve type references)
		// ig: is the code generator:
		// toplevel: the toplevel block.  This is used for checking 
		//           that no two labels with the same name are used.
		//
		public void EmitMeta (TypeContainer tc, ILGenerator ig, Block toplevel, int count)
		{
			//
			// Process this block variables
			//
			if (variables != null){
				local_builders = new Hashtable ();
				
				foreach (DictionaryEntry de in variables){
					string name = (string) de.Key;
					VariableInfo vi = (VariableInfo) de.Value;
					Type t;
					
					t = tc.LookupType (vi.Type, false);
					if (t == null)
						continue;

					vi.VariableType = t;
					vi.LocalBuilder = ig.DeclareLocal (t);
					vi.Idx = count++;
				}
			}

			//
			// Now, handle the children
			//
			if (children != null){
				foreach (Block b in children)
					b.EmitMeta (tc, ig, toplevel, count);
			}
		}

		public void UsageWarning ()
		{
			string name;
			
			if (variables != null){
				foreach (DictionaryEntry de in variables){
					VariableInfo vi = (VariableInfo) de.Value;
					
					if (vi.Used)
						continue;
					
					name = (string) de.Key;
						
					if (vi.Assigned){
						Report.Warning (
							219, vi.Location, "The variable `" + name +
							"' is assigned but its value is never used");
					} else {
						Report.Warning (
							168, vi.Location, "The variable `" +
							name +
							"' is declared but never used");
					} 
				}
			}

			if (children != null)
				foreach (Block b in children)
					b.UsageWarning ();
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
		public readonly Block Fini, Block;
		public readonly ArrayList Specific;
		public readonly Catch General;
		
		//
		// specific, general and fini might all be null.
		//
		public Try (Block block, ArrayList specific, Catch general, Block fini)
		{
			if (specific == null && general == null){
				Console.WriteLine ("CIR.Try: Either specific or general have to be non-null");
			}
			
			this.Block = block;
			this.Specific = specific;
			this.General = general;
			this.Fini = fini;
		}
	}
	
	public class Catch {
		public readonly string Type;
		public readonly string Name;
		public readonly Block  Block;
		
		public Catch (string type, string name, Block block)
		{
			Type = type;
			Name = name;
			Block = block;
		}
	}

	public class Foreach : Statement {
		public readonly string Type;
		public readonly LocalVariableReference Variable;
		public readonly Expression Expr;
		public readonly Statement Statement;
		public readonly Location Location;
		
		public Foreach (string type, LocalVariableReference var, Expression expr,
				Statement stmt, Location l)
		{
			Type = type;
			Variable = var;
			Expr = expr;
			Statement = stmt;
			Location = l;
		}
	}
}

