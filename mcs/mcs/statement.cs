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

		public static bool EmitBoolExpression (EmitContext ec, Expression e)
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
			
			e.Emit (ec);

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
			
			if (!EmitBoolExpression (ec, Expr))
				return false;
			
			ig.Emit (OpCodes.Brfalse, false_target);
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

			ig.MarkLabel (loop);
			EmbeddedStatement.Emit (ec);
			EmitBoolExpression (ec, Expr);
			ig.Emit (OpCodes.Brtrue, loop);

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
			Label while_eval = ig.DefineLabel ();
			Label exit = ig.DefineLabel ();
			
			ig.MarkLabel (while_eval);
			EmitBoolExpression (ec, Expr);
			ig.Emit (OpCodes.Brfalse, exit);
			Statement.Emit (ec);
			ig.Emit (OpCodes.Br, while_eval);
			ig.MarkLabel (exit);

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
			Label loop = ig.DefineLabel ();
			Label exit = ig.DefineLabel ();
			
			if (! (InitStatement is EmptyStatement))
				InitStatement.Emit (ec);

			ig.MarkLabel (loop);
			EmitBoolExpression (ec, Test);
			ig.Emit (OpCodes.Brfalse, exit);
			Statement.Emit (ec);
			if (!(Increment is EmptyStatement))
				Increment.Emit (ec);
			ig.Emit (OpCodes.Br, loop);
			ig.MarkLabel (exit);

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
		
		public Goto (string label)
		{
			target = label;
		}

		public string Target {
			get {
				return target;
			}
		}

		public override bool Emit (EmitContext ec)
		{
			throw new Exception ("Unimplemented");
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
		public Break ()
		{
		}

		public override bool Emit (EmitContext ec)
		{
			throw new Exception ("Unimplemented");
		}
	}

	public class Continue : Statement {
		public Continue ()
		{
		}

		public override bool Emit (EmitContext ec)
		{
			throw new Exception ("Unimplemented");
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

		public override bool Emit (EmitContext ec)
		{
			bool is_ret = false;

			foreach (Statement s in Statements)
				is_ret = s.Emit (ec);

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
		
		public Lock (Expression expr, Statement stmt)
		{
			Expr = expr;
			Statement = stmt;
		}

		public override bool Emit (EmitContext ec)
		{
			throw new Exception ("Unimplemented");
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
                        Report.Error (1579, Location,
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
		
		public override bool Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			Expression e = Expr;
			MethodInfo get_enum;
			LocalBuilder enumerator, disposable;
			Type var_type;
			
			e = e.Resolve (ec);
			if (e == null)
				return false;

			var_type = ec.TypeContainer.LookupType (Type, false);
			if (var_type == null)
				return false;
			
			//
			// We need an instance variable.  Not sure this is the best
			// way of doing this.
			//
			// FIXME: When we implement propertyaccess, will those turn
			// out to return values in ExprClass?  I think they should.
			//
			if (!(e.ExprClass == ExprClass.Variable || e.ExprClass == ExprClass.Value)){
				error1579 (e.Type);
				return false;
			}
			
			if ((get_enum = ProbeCollectionType (e.Type)) == null)
				return false;

			Expression empty = new EmptyExpression ();
			Expression conv;

			conv = Expression.ConvertExplicit (ec, empty, var_type, Location);
			if (conv == null)
				return false;
			
			enumerator = ig.DeclareLocal (TypeManager.ienumerator_type);
			disposable = ig.DeclareLocal (TypeManager.idisposable_type);
			
			//
			// Instantiate the enumerator

			Label end = ig.DefineLabel ();
			Label end_try = ig.DefineLabel ();
			Label loop = ig.DefineLabel ();

			//
			// FIXME: This code does not work for cases like:
			// foreach (int a in ValueTypeVariable){
			// }
			//
			// The code should emit an ldarga instruction
			// for the ValueTypeVariable rather than a ldarg
			//
			if (e.Type.IsValueType){
				ig.Emit (OpCodes.Call, get_enum);
			} else {
				e.Emit (ec);
				ig.Emit (OpCodes.Callvirt, get_enum);
			}
			ig.Emit (OpCodes.Stloc, enumerator);

			//
			// Protect the code in a try/finalize block, so that
			// if the beast implement IDisposable, we get rid of it
			//
			Label l = ig.BeginExceptionBlock ();
			ig.MarkLabel (loop);
			ig.Emit (OpCodes.Ldloc, enumerator);
			ig.Emit (OpCodes.Callvirt, TypeManager.bool_movenext_void);
			ig.Emit (OpCodes.Brfalse, end_try);
			ig.Emit (OpCodes.Ldloc, enumerator);
			ig.Emit (OpCodes.Callvirt, TypeManager.object_getcurrent_void);
			conv.Emit (ec);
			Variable.Store (ec);
			Statement.Emit (ec);
			ig.Emit (OpCodes.Br, loop);
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
			ig.MarkLabel (end);

			return false;
		}

	}
}

