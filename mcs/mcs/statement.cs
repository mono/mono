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

		//
		// Return value indicates whether the last instruction
		// was a return instruction
		//
		public abstract bool Emit (EmitContext ec);

		// <remarks>
		//    Emits a bool expression.  Generates a jump to the `t' label if true
		//    if defined, or to `f' if defined.
		//
		//    t and f can not be both non-null
		// </remarks>
		public static bool EmitBoolExpression (EmitContext ec, Expression e, Label l, bool isTrue)
		{
			e = e.Resolve (ec);

			if (e == null)
				return false;

			if (e.Type != TypeManager.bool_type)
				e = Expression.ConvertImplicit (ec, e, TypeManager.bool_type,
								new Location (-1));

			if (e == null){
				Report.Error (
					31, "Can not convert the expression to a boolean");
				return false;
			}

			bool invert = false;
			if (e is Unary){
				Unary u = (Unary) e;
				
				if (u.Oper == Unary.Operator.LogicalNot){
					invert = true;

					u.EmitLogicalNot (ec);
				}
			} 

			if (!invert)
				e.Emit (ec);

			if (isTrue){
				if (invert)
					ec.ig.Emit (OpCodes.Brfalse, l);
				else
					ec.ig.Emit (OpCodes.Brtrue, l);
			} else {
				if (invert)
					ec.ig.Emit (OpCodes.Brtrue, l);
				else
					ec.ig.Emit (OpCodes.Brfalse, l);
			}
			
			return true;
		}

	}

	public class EmptyStatement : Statement {
		public override bool Emit (EmitContext ec)
		{
			return false;
		}
	}
	
	public class If : Statement {
		public readonly Expression  Expr;
		public readonly Statement   TrueStatement;
		public readonly Statement   FalseStatement;
		
		public If (Expression expr, Statement trueStatement)
		{
			Expr = expr;
			TrueStatement = trueStatement;
		}

		public If (Expression expr,
			   Statement trueStatement,
			   Statement falseStatement)
		{
			Expr = expr;
			TrueStatement = trueStatement;
			FalseStatement = falseStatement;
		}

		public override bool Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			Label false_target = ig.DefineLabel ();
			Label end;
			bool is_ret;
			
			if (!EmitBoolExpression (ec, Expr, false_target, false))
				return false;
			
			is_ret = TrueStatement.Emit (ec);

			if (FalseStatement != null){
				bool branch_emitted = false;
				
				end = ig.DefineLabel ();
				if (!is_ret){
					ig.Emit (OpCodes.Br, end);
					branch_emitted = true;
				}
			
				ig.MarkLabel (false_target);
				is_ret = FalseStatement.Emit (ec);

				if (branch_emitted)
					ig.MarkLabel (end);
			} else
				ig.MarkLabel (false_target);

			return is_ret;
		}
	}

	public class Do : Statement {
		public readonly Expression Expr;
		public readonly Statement  EmbeddedStatement;
		
		public Do (Statement statement, Expression boolExpr)
		{
			Expr = boolExpr;
			EmbeddedStatement = statement;
		}

		public override bool Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			Label loop = ig.DefineLabel ();
			Label old_begin = ec.LoopBegin;
			Label old_end = ec.LoopEnd;
			bool  old_inloop = ec.InLoop;
			
			ec.LoopBegin = ig.DefineLabel ();
			ec.LoopEnd = ig.DefineLabel ();
			ec.InLoop = true;
				
			ig.MarkLabel (loop);
			EmbeddedStatement.Emit (ec);
			ig.MarkLabel (ec.LoopBegin);
			EmitBoolExpression (ec, Expr, loop, true);
			ig.MarkLabel (ec.LoopEnd);

			ec.LoopBegin = old_begin;
			ec.LoopEnd = old_end;
			ec.InLoop = old_inloop;
			
			return false;
		}
	}

	public class While : Statement {
		public readonly Expression Expr;
		public readonly Statement Statement;
		
		public While (Expression boolExpr, Statement statement)
		{
			Expr = boolExpr;
			Statement = statement;
		}

		public override bool Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			Label old_begin = ec.LoopBegin;
			Label old_end = ec.LoopEnd;
			bool old_inloop = ec.InLoop;
			
			ec.LoopBegin = ig.DefineLabel ();
			ec.LoopEnd = ig.DefineLabel ();
			ec.InLoop = true;
			
			ig.MarkLabel (ec.LoopBegin);
			EmitBoolExpression (ec, Expr, ec.LoopEnd, false);
			Statement.Emit (ec);
			ig.Emit (OpCodes.Br, ec.LoopBegin);
			ig.MarkLabel (ec.LoopEnd);

			ec.LoopBegin = old_begin;
			ec.LoopEnd = old_end;
			ec.InLoop = old_inloop;
			
			return false;
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

		public override bool Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			Label old_begin = ec.LoopBegin;
			Label old_end = ec.LoopEnd;
			bool old_inloop = ec.InLoop;
			Label loop = ig.DefineLabel ();

			if (InitStatement != null)
				if (! (InitStatement is EmptyStatement))
					InitStatement.Emit (ec);

			ec.LoopBegin = ig.DefineLabel ();
			ec.LoopEnd = ig.DefineLabel ();
			ec.InLoop = true;

			ig.MarkLabel (loop);
			EmitBoolExpression (ec, Test, ec.LoopEnd, false);
			Statement.Emit (ec);
			ig.MarkLabel (ec.LoopBegin);
			if (!(Increment is EmptyStatement))
				Increment.Emit (ec);
			ig.Emit (OpCodes.Br, loop);
			ig.MarkLabel (ec.LoopEnd);

			ec.LoopBegin = old_begin;
			ec.LoopEnd = old_end;
			ec.InLoop = old_inloop;
			return false;
		}
	}
	
	public class StatementExpression : Statement {
		public readonly ExpressionStatement Expr;
		
		public StatementExpression (ExpressionStatement expr)
		{
			Expr = expr;
		}

		public override bool Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			Expression ne;
			
			ne = Expr.Resolve (ec);
			if (ne != null){
				if (ne is ExpressionStatement)
					((ExpressionStatement) ne).EmitStatement (ec);
				else {
					ne.Emit (ec);
					ig.Emit (OpCodes.Pop);
				}
			}

			return false;
		}
	}

	public class Return : Statement {
		public Expression Expr;
		public readonly Location loc;
		
		public Return (Expression expr, Location l)
		{
			Expr = expr;
			loc = l;
		}

		public override bool Emit (EmitContext ec)
		{
			if (ec.ReturnType == null){
				if (Expr != null){
					Report.Error (127, loc, "Return with a value not allowed here");
					return false;
				}
			} else {
				if (Expr == null){
					Report.Error (126, loc, "An object of type `" +
						      TypeManager.CSharpName (ec.ReturnType) + "' is " +
						      "expected for the return statement");
					return false;
				}

				Expr = Expr.Resolve (ec);
				if (Expr == null)
					return false;

				if (Expr.Type != ec.ReturnType)
					Expr = Expression.ConvertImplicitRequired (
						ec, Expr, ec.ReturnType, loc);

				if (Expr == null)
					return false;

				Expr.Emit (ec);
			}

			ec.ig.Emit (OpCodes.Ret);

			return true; 
		}
	}

	public class Goto : Statement {
		string target;
		Location loc;
			
		public Goto (string label, Location l)
		{
			loc = l;
			target = label;
		}

		public string Target {
			get {
				return target;
			}
		}

		public override bool Emit (EmitContext ec)
		{
			Console.WriteLine ("Attempting to goto to: " + target);
			
			return false;
		}
	}

	public class Throw : Statement {
		public readonly Expression Expr;
		
		public Throw (Expression expr)
		{
			Expr = expr;
		}

		public override bool Emit (EmitContext ec)
		{
			Expression e = Expr.Resolve (ec);

			if (e == null)
				return false;

			e.Emit (ec);
			ec.ig.Emit (OpCodes.Throw);

			return false;
		}
	}

	public class Break : Statement {
		Location loc;
		
		public Break (Location l)
		{
			loc = l;
		}

		public override bool Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;

			if (!ec.InLoop){
				Report.Error (139, loc, "No enclosing loop to continue to");
				return false;
			}
			
			ig.Emit (OpCodes.Br, ec.LoopEnd);
			return false;
		}
	}

	public class Continue : Statement {
		Location loc;
		
		public Continue (Location l)
		{
			loc = l;
		}

		public override bool Emit (EmitContext ec)
		{
			Label begin = ec.LoopBegin;
			
			if (!ec.InLoop){
				Report.Error (139, loc, "No enclosing loop to continue to");
				return false;
			} 

			ec.ig.Emit (OpCodes.Br, begin);
			return false;
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
		// An array of Blocks.  We keep track of children just
		// to generate the local variable declarations.
		//
		// Statements and child statements are handled through the
		// statements.
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

		public override bool Emit (EmitContext ec)
		{
			bool is_ret = false;
			Block prev_block = ec.CurrentBlock;
			
			ec.CurrentBlock = this;
			foreach (Statement s in Statements)
				is_ret = s.Emit (ec);

			ec.CurrentBlock = prev_block;
			return is_ret;
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

		public override bool Emit (EmitContext ec)
		{
			throw new Exception ("Unimplemented");
		}
	}

	public class Lock : Statement {
		public readonly Expression Expr;
		public readonly Statement Statement;
		Location loc;
			
		public Lock (Expression expr, Statement stmt, Location l)
		{
			Expr = expr;
			Statement = stmt;
			loc = l;
		}

		public override bool Emit (EmitContext ec)
		{
			Expression e = Expr.Resolve (ec);
			if (e == null)
				return false;

			Type type = e.Type;
			
			if (type.IsValueType){
				Report.Error (185, loc, "lock statement requires the expression to be " +
					      " a reference type (type is: `" +
					      TypeManager.CSharpName (type) + "'");
				return false;
			}

			LocalBuilder temp = ec.GetTemporaryStorage (type);
			ILGenerator ig = ec.ig;
				
			e.Emit (ec);
			ig.Emit (OpCodes.Dup);
			ig.Emit (OpCodes.Stloc, temp);
			ig.Emit (OpCodes.Call, TypeManager.void_monitor_enter_object);

			// try
			Label end = ig.BeginExceptionBlock ();
			Label finish = ig.DefineLabel ();
			Statement.Emit (ec);
			// ig.Emit (OpCodes.Leave, finish);

			ig.MarkLabel (finish);
			
			// finally
			ig.BeginFinallyBlock ();
			ig.Emit (OpCodes.Ldloc, temp);
			ig.Emit (OpCodes.Call, TypeManager.void_monitor_exit_object);
			ig.EndExceptionBlock ();
			
			return false;
		}
	}

	public class Unchecked : Statement {
		public readonly Block Block;
		
		public Unchecked (Block b)
		{
			Block = b;
		}

		public override bool Emit (EmitContext ec)
		{
			bool previous_state = ec.CheckState;
			bool val;
			
			ec.CheckState = false;
			val = Block.Emit (ec);
			ec.CheckState = previous_state;

			return val;
		}
	}

	public class Checked : Statement {
		public readonly Block Block;
		
		public Checked (Block b)
		{
			Block = b;
		}

		public override bool Emit (EmitContext ec)
		{
			bool previous_state = ec.CheckState;
			bool val;
			
			ec.CheckState = true;
			val = Block.Emit (ec);
			ec.CheckState = previous_state;

			return val;
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

		public override bool Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			Label end;
			Label finish = ig.DefineLabel ();;

			end = ig.BeginExceptionBlock ();
			Block.Emit (ec);
			ig.Emit (OpCodes.Leave, finish);
			
			foreach (Catch c in Specific){
				Type catch_type = ec.TypeContainer.LookupType (c.Type, false);
				VariableInfo vi;
				
				if (catch_type == null)
					return false;

				ig.BeginCatchBlock (catch_type);

				if (c.Name != null){
					vi = c.Block.GetVariableInfo (c.Name);
					if (vi == null){
						Console.WriteLine ("This should not happen! variable does not exist in this block");
						Environment.Exit (0);
					}
				
					ig.Emit (OpCodes.Stloc, vi.LocalBuilder);
				} else
					ig.Emit (OpCodes.Pop);
				
				c.Block.Emit (ec);
			}

			if (General != null){
				ig.BeginCatchBlock (TypeManager.object_type);
				ig.Emit (OpCodes.Pop);
			}

			ig.MarkLabel (finish);
			if (Fini != null){
				ig.BeginFinallyBlock ();
				Fini.Emit (ec);
			}
			
			ig.EndExceptionBlock ();

			return false;
		}
	}

	public class Using : Statement {
		object expression_or_block;
		Statement Statement;
		Location loc;
		
		public Using (object expression_or_block, Statement stmt, Location l)
		{
			this.expression_or_block = expression_or_block;
			Statement = stmt;
			loc = l;
		}

		public override bool Emit (EmitContext ec)
		{
			//
			// Expressions are simple. 
			// The problem is with blocks, blocks might contain
			// more than one variable, ie like this:
			//
			// using (a = new X (), b = new Y ()) stmt;
			//
			// which is turned into:
			// using (a = new X ()) using (b = new Y ()) stmt;
			//
			// The trick is that the block will contain a bunch
			// of potential Assign expressions
			//
			//
			// We need to signal an error if a variable lacks
			// an assignment. (210).
			//
			// This is one solution.  Another is to set a flag
			// when we get the USING token, and have declare_local_variables
			// do something *different* that we can better cope with
			//
			throw new Exception ("Implement me!");
		}
	}
	
	public class Foreach : Statement {
		string type;
		LocalVariableReference variable;
		Expression expr;
		Statement statement;
		Location loc;
		
		public Foreach (string type, LocalVariableReference var, Expression expr,
				Statement stmt, Location l)
		{
			this.type = type;
			this.variable = var;
			this.expr = expr;
			statement = stmt;
			loc = l;
		}

		static bool GetEnumeratorFilter (MemberInfo m, object criteria)
		{
			if (m == null)
				return false;
			
			if (!(m is MethodInfo))
				return false;
			
			if (m.Name != "GetEnumerator")
				return false;
			
			MethodInfo mi = (MethodInfo) m;
			
			if (mi.ReturnType != TypeManager.ienumerator_type)
				return false;
			
			Type [] args = TypeManager.GetArgumentTypes (mi);
			if (args == null)
				return true;
			
			if (args.Length == 0)
				return true;
			
			return false;
		}
		
		// <summary>
		//   This filter is used to find the GetEnumerator method
		//   on which IEnumerator operates
		// </summary>
		static MemberFilter FilterEnumerator;
		
		static Foreach ()
		{
			FilterEnumerator = new MemberFilter (GetEnumeratorFilter);
		}

                void error1579 (Type t)
                {
                        Report.Error (1579, loc,
                                      "foreach statement cannot operate on variables of type `" +
                                      t.FullName + "' because that class does not provide a " +
                                      " GetEnumerator method or it is inaccessible");
                }

		MethodInfo ProbeCollectionType (Type t)
		{
			MemberInfo [] mi;

			mi = TypeContainer.FindMembers (t, MemberTypes.Method,
							BindingFlags.Public,
							FilterEnumerator, null);

			if (mi == null){
				error1579 (t);
				return null;
			}

			if (mi.Length == 0){
				error1579 (t);
				return null;
			}

			return (MethodInfo) mi [0];
		}

		//
		// FIXME: possible optimization.
		// We might be able to avoid creating `empty' if the type is the sam
		//
		bool EmitCollectionForeach (EmitContext ec, Type var_type, MethodInfo get_enum)
		{
			ILGenerator ig = ec.ig;
			LocalBuilder enumerator, disposable;
			Expression empty = new EmptyExpression ();
			Expression conv;

			//
			// FIXME: maybe we can apply the same trick we do in the
			// array handling to avoid creating empty and conv in some cases.
			//
			// Although it is not as important in this case, as the type
			// will not likely be object (what the enumerator will return).
			//
			conv = Expression.ConvertExplicit (ec, empty, var_type, loc);
			if (conv == null)
				return false;
			
			enumerator = ig.DeclareLocal (TypeManager.ienumerator_type);
			disposable = ig.DeclareLocal (TypeManager.idisposable_type);
			
			//
			// Instantiate the enumerator

			Label old_begin = ec.LoopBegin, old_end = ec.LoopEnd;
			Label end_try = ig.DefineLabel ();
			bool old_inloop = ec.InLoop;
			ec.LoopBegin = ig.DefineLabel ();
			ec.LoopEnd = ig.DefineLabel ();
			ec.InLoop = true;
			
			//
			// FIXME: This code does not work for cases like:
			// foreach (int a in ValueTypeVariable){
			// }
			//
			// The code should emit an ldarga instruction
			// for the ValueTypeVariable rather than a ldarg
			//
			if (expr.Type.IsValueType){
				ig.Emit (OpCodes.Call, get_enum);
			} else {
				expr.Emit (ec);
				ig.Emit (OpCodes.Callvirt, get_enum);
			}
			ig.Emit (OpCodes.Stloc, enumerator);

			//
			// Protect the code in a try/finalize block, so that
			// if the beast implement IDisposable, we get rid of it
			//
			Label l = ig.BeginExceptionBlock ();
			ig.MarkLabel (ec.LoopBegin);
			ig.Emit (OpCodes.Ldloc, enumerator);
			ig.Emit (OpCodes.Callvirt, TypeManager.bool_movenext_void);
			ig.Emit (OpCodes.Brfalse, end_try);
			ig.Emit (OpCodes.Ldloc, enumerator);
			ig.Emit (OpCodes.Callvirt, TypeManager.object_getcurrent_void);
			variable.EmitAssign (ec, conv);
			statement.Emit (ec);
			ig.Emit (OpCodes.Br, ec.LoopBegin);
			ig.MarkLabel (end_try);

			// The runtime provides this for us.
			// ig.Emit (OpCodes.Leave, end);

			//
			// Now the finally block
			//
			Label end_finally = ig.DefineLabel ();
			
			ig.BeginFinallyBlock ();
			ig.Emit (OpCodes.Ldloc, enumerator);
			ig.Emit (OpCodes.Isinst, TypeManager.idisposable_type);
			ig.Emit (OpCodes.Stloc, disposable);
			ig.Emit (OpCodes.Ldloc, disposable);
			ig.Emit (OpCodes.Brfalse, end_finally);
			ig.Emit (OpCodes.Ldloc, disposable);
			ig.Emit (OpCodes.Callvirt, TypeManager.void_dispose_void);
			ig.MarkLabel (end_finally);

			// The runtime generates this anyways.
			// ig.Emit (OpCodes.Endfinally);

			ig.EndExceptionBlock ();

			ig.MarkLabel (ec.LoopEnd);
			
			ec.LoopBegin = old_begin;
			ec.LoopEnd = old_end;
			ec.InLoop = old_inloop;

			return false;
		}

		//
		// FIXME: possible optimization.
		// We might be able to avoid creating `empty' if the type is the sam
		//
		bool EmitArrayForeach (EmitContext ec, Type var_type)
		{
			Type array_type = expr.Type;
			Type element_type = array_type.GetElementType ();
			Expression conv = null;
			Expression empty = new EmptyExpression (var_type);
			
			conv = Expression.ConvertExplicit (ec, empty, var_type, loc);
			if (conv == null)
					return false;

			int rank = array_type.GetArrayRank ();
			ILGenerator ig = ec.ig;

			Console.WriteLine ("Rank= " + rank);
			if (rank == 1){
				LocalBuilder counter = ec.GetTemporaryStorage (TypeManager.int32_type);
				LocalBuilder copy = ec.GetTemporaryStorage (array_type);

				Label loop, test;
				
				//
				// Make our copy of the array
				//
				expr.Emit (ec);
				ig.Emit (OpCodes.Stloc, copy);
				
				ig.Emit (OpCodes.Ldc_I4_0);
				ig.Emit (OpCodes.Stloc, counter);
				test = ig.DefineLabel ();
				ig.Emit (OpCodes.Br, test);

				loop = ig.DefineLabel ();
				ig.MarkLabel (loop);

				ArrayAccess.EmitLoadOpcode (ig, var_type);

				variable.EmitAssign (ec, conv);

				statement.Emit (ec);

				ig.Emit (OpCodes.Ldloc, counter);
				ig.Emit (OpCodes.Ldc_I4_1);
				ig.Emit (OpCodes.Add);
				ig.Emit (OpCodes.Stloc, counter);

				ig.MarkLabel (test);
				ig.Emit (OpCodes.Ldloc, counter);
				ig.Emit (OpCodes.Ldloc, copy);
				ig.Emit (OpCodes.Ldlen);
				ig.Emit (OpCodes.Conv_I4);
				ig.Emit (OpCodes.Blt, loop);
			} else {
				throw new Exception ("Unimplemented");
			}

			return false;
		}
		
		public override bool Emit (EmitContext ec)
		{
			Type var_type;
			
			expr = expr.Resolve (ec);
			if (expr == null)
				return false;

			var_type = ec.TypeContainer.LookupType (type, false);
			if (var_type == null)
				return false;
			
			//
			// We need an instance variable.  Not sure this is the best
			// way of doing this.
			//
			// FIXME: When we implement propertyaccess, will those turn
			// out to return values in ExprClass?  I think they should.
			//
			if (!(expr.ExprClass == ExprClass.Variable || expr.ExprClass == ExprClass.Value)){
				error1579 (expr.Type);
				return false;
			}

			if (expr.Type.IsArray)
				return EmitArrayForeach (ec, var_type);
			else {
				MethodInfo get_enum;
				
				if ((get_enum = ProbeCollectionType (expr.Type)) == null)
					return false;

				return EmitCollectionForeach (ec, var_type, get_enum);
			}

		}

	}
}

