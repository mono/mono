//
// statement.cs: Statement representation for the IL tree.
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001, 2002 Ximian, Inc.
//

using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics;

namespace Mono.CSharp {

	using System.Collections;
	
	public abstract class Statement {
		public Location loc;
		
		///
		/// Resolves the statement, true means that all sub-statements
		/// did resolve ok.
		//
		public virtual bool Resolve (EmitContext ec)
		{
			return true;
		}
		
		/// <summary>
		///   Return value indicates whether all code paths emitted return.
		/// </summary>
		public abstract bool Emit (EmitContext ec);
		
		public static Expression ResolveBoolean (EmitContext ec, Expression e, Location loc)
		{
			e = e.Resolve (ec);
			if (e == null)
				return null;
			
			if (e.Type != TypeManager.bool_type){
				e = Expression.ConvertImplicit (ec, e, TypeManager.bool_type,
								new Location (-1));
			}

			if (e == null){
				Report.Error (
					31, loc, "Can not convert the expression to a boolean");
			}

			if (CodeGen.SymbolWriter != null)
				ec.Mark (loc);

			return e;
		}
		
		/// <remarks>
		///    Encapsulates the emission of a boolean test and jumping to a
		///    destination.
		///
		///    This will emit the bool expression in `bool_expr' and if
		///    `target_is_for_true' is true, then the code will generate a 
		///    brtrue to the target.   Otherwise a brfalse. 
		/// </remarks>
		public static void EmitBoolExpression (EmitContext ec, Expression bool_expr,
						       Label target, bool target_is_for_true)
		{
			ILGenerator ig = ec.ig;
			
			bool invert = false;
			if (bool_expr is Unary){
				Unary u = (Unary) bool_expr;
				
				if (u.Oper == Unary.Operator.LogicalNot){
					invert = true;

					u.EmitLogicalNot (ec);
				}
			} else if (bool_expr is Binary){
				Binary b = (Binary) bool_expr;

				if (b.EmitBranchable (ec, target, target_is_for_true))
					return;
			}

			if (!invert)
				bool_expr.Emit (ec);

			if (target_is_for_true){
				if (invert)
					ig.Emit (OpCodes.Brfalse, target);
				else
					ig.Emit (OpCodes.Brtrue, target);
			} else {
				if (invert)
					ig.Emit (OpCodes.Brtrue, target);
				else
					ig.Emit (OpCodes.Brfalse, target);
			}
		}

		public static void Warning_DeadCodeFound (Location loc)
		{
			Report.Warning (162, loc, "Unreachable code detected");
		}
	}

	public class EmptyStatement : Statement {
		public override bool Resolve (EmitContext ec)
		{
			return true;
		}
		
		public override bool Emit (EmitContext ec)
		{
			return false;
		}
	}
	
	public class If : Statement {
		Expression expr;
		public Statement TrueStatement;
		public Statement FalseStatement;
		
		public If (Expression expr, Statement trueStatement, Location l)
		{
			this.expr = expr;
			TrueStatement = trueStatement;
			loc = l;
		}

		public If (Expression expr,
			   Statement trueStatement,
			   Statement falseStatement,
			   Location l)
		{
			this.expr = expr;
			TrueStatement = trueStatement;
			FalseStatement = falseStatement;
			loc = l;
		}

		public override bool Resolve (EmitContext ec)
		{
			expr = ResolveBoolean (ec, expr, loc);
			if (expr == null){
				return false;
			}
			
			if (TrueStatement.Resolve (ec)){
				if (FalseStatement != null){
					if (FalseStatement.Resolve (ec))
						return true;
					
					return false;
				}
				return true;
			}
			return false;
		}
		
		public override bool Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			Label false_target = ig.DefineLabel ();
			Label end;
			bool is_true_ret, is_false_ret;

			//
			// Dead code elimination
			//
			if (expr is BoolConstant){
				bool take = ((BoolConstant) expr).Value;

				if (take){
					if (FalseStatement != null){
						Warning_DeadCodeFound (FalseStatement.loc);
					}
					return TrueStatement.Emit (ec);
				} else {
					Warning_DeadCodeFound (TrueStatement.loc);
					if (FalseStatement != null)
						return FalseStatement.Emit (ec);
				}
			}
			
			EmitBoolExpression (ec, expr, false_target, false);
			
			is_true_ret = TrueStatement.Emit (ec);
			is_false_ret = is_true_ret;

			if (FalseStatement != null){
				bool branch_emitted = false;
				
				end = ig.DefineLabel ();
				if (!is_true_ret){
					ig.Emit (OpCodes.Br, end);
					branch_emitted = true;
				}
			
				ig.MarkLabel (false_target);
				is_false_ret = FalseStatement.Emit (ec);

				if (branch_emitted)
					ig.MarkLabel (end);
			} else {
				ig.MarkLabel (false_target);
				is_false_ret = false;
			}

			return is_true_ret && is_false_ret;
		}
	}

	public class Do : Statement {
		public Expression expr;
		public readonly Statement  EmbeddedStatement;
		
		public Do (Statement statement, Expression boolExpr, Location l)
		{
			expr = boolExpr;
			EmbeddedStatement = statement;
			loc = l;
		}

		public override bool Resolve (EmitContext ec)
		{
			expr = ResolveBoolean (ec, expr, loc);
			if (expr == null)
				return false;
			
			return EmbeddedStatement.Resolve (ec);
		}
		
		public override bool Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			Label loop = ig.DefineLabel ();
			Label old_begin = ec.LoopBegin;
			Label old_end = ec.LoopEnd;
			bool  old_inloop = ec.InLoop;
			bool old_breaks = ec.Breaks;
			
			ec.LoopBegin = ig.DefineLabel ();
			ec.LoopEnd = ig.DefineLabel ();
			ec.InLoop = true;
				
			ig.MarkLabel (loop);
			ec.Breaks = false;
			EmbeddedStatement.Emit (ec);
			bool breaks = ec.Breaks;
			ig.MarkLabel (ec.LoopBegin);

			//
			// Dead code elimination
			//
			if (expr is BoolConstant){
				bool res = ((BoolConstant) expr).Value;

				if (res)
					ec.ig.Emit (OpCodes.Br, loop); 
			} else
				EmitBoolExpression (ec, expr, loop, true);
			
			ig.MarkLabel (ec.LoopEnd);

			ec.LoopBegin = old_begin;
			ec.LoopEnd = old_end;
			ec.InLoop = old_inloop;
			ec.Breaks = old_breaks;

			//
			// Inform whether we are infinite or not
			//
			if (expr is BoolConstant){
				BoolConstant bc = (BoolConstant) expr;

				if (bc.Value == true)
					return breaks == false;
			}
			
			return false;
		}
	}

	public class While : Statement {
		public Expression expr;
		public readonly Statement Statement;
		
		public While (Expression boolExpr, Statement statement, Location l)
		{
			this.expr = boolExpr;
			Statement = statement;
			loc = l;
		}

		public override bool Resolve (EmitContext ec)
		{
			expr = ResolveBoolean (ec, expr, loc);
			if (expr == null)
				return false;
			
			return Statement.Resolve (ec);
		}
		
		public override bool Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			Label old_begin = ec.LoopBegin;
			Label old_end = ec.LoopEnd;
			bool old_inloop = ec.InLoop;
			bool old_breaks = ec.Breaks;
			Label while_loop = ig.DefineLabel ();
			bool ret;
			
			ec.LoopBegin = ig.DefineLabel ();
			ec.LoopEnd = ig.DefineLabel ();
			ec.InLoop = true;

			ig.Emit (OpCodes.Br, ec.LoopBegin);
			ig.MarkLabel (while_loop);

			//
			// Inform whether we are infinite or not
			//
			if (expr is BoolConstant){
				BoolConstant bc = (BoolConstant) expr;

				ig.MarkLabel (ec.LoopBegin);
				if (bc.Value == false){
					Warning_DeadCodeFound (Statement.loc);
					ret = false;
				} else {
					bool breaks;
					
					ec.Breaks = false;
					Statement.Emit (ec);
					breaks = ec.Breaks;
					ig.Emit (OpCodes.Br, ec.LoopBegin);
					
					//
					// Inform that we are infinite (ie, `we return'), only
					// if we do not `break' inside the code.
					//
					ret = breaks == false;
				}
				ig.MarkLabel (ec.LoopEnd);
			} else {
				Statement.Emit (ec);
			
				ig.MarkLabel (ec.LoopBegin);

				EmitBoolExpression (ec, expr, while_loop, true);
				ig.MarkLabel (ec.LoopEnd);

				ret = false;
			}	

			ec.LoopBegin = old_begin;
			ec.LoopEnd = old_end;
			ec.InLoop = old_inloop;
			ec.Breaks = old_breaks;

			return ret;
		}
	}

	public class For : Statement {
		Expression Test;
		readonly Statement InitStatement;
		readonly Statement Increment;
		readonly Statement Statement;
		
		public For (Statement initStatement,
			    Expression test,
			    Statement increment,
			    Statement statement,
			    Location l)
		{
			InitStatement = initStatement;
			Test = test;
			Increment = increment;
			Statement = statement;
			loc = l;
		}

		public override bool Resolve (EmitContext ec)
		{
			bool ok = true;

			if (Test != null){
				Test = ResolveBoolean (ec, Test, loc);
				if (Test == null)
					ok = false;
			}

			if (InitStatement != null){
				if (!InitStatement.Resolve (ec))
					ok = false;
			}

			if (Increment != null){
				if (!Increment.Resolve (ec))
					ok = false;
			}
			
			return Statement.Resolve (ec) && ok;
		}
		
		public override bool Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			Label old_begin = ec.LoopBegin;
			Label old_end = ec.LoopEnd;
			bool old_inloop = ec.InLoop;
			bool old_breaks = ec.Breaks;
			Label loop = ig.DefineLabel ();
			Label test = ig.DefineLabel ();
			
			if (InitStatement != null)
				if (! (InitStatement is EmptyStatement))
					InitStatement.Emit (ec);

			ec.LoopBegin = ig.DefineLabel ();
			ec.LoopEnd = ig.DefineLabel ();
			ec.InLoop = true;

			ig.Emit (OpCodes.Br, test);
			ig.MarkLabel (loop);
			ec.Breaks = false;
			Statement.Emit (ec);
			bool breaks = ec.Breaks;

			ig.MarkLabel (ec.LoopBegin);
			if (!(Increment is EmptyStatement))
				Increment.Emit (ec);

			ig.MarkLabel (test);
			//
			// If test is null, there is no test, and we are just
			// an infinite loop
			//
			if (Test != null)
				EmitBoolExpression (ec, Test, loop, true);
			else
				ig.Emit (OpCodes.Br, loop);
			ig.MarkLabel (ec.LoopEnd);

			ec.LoopBegin = old_begin;
			ec.LoopEnd = old_end;
			ec.InLoop = old_inloop;
			ec.Breaks = old_breaks;
			
			//
			// Inform whether we are infinite or not
			//
			if (Test != null){
				if (Test is BoolConstant){
					BoolConstant bc = (BoolConstant) Test;

					if (bc.Value)
						return breaks == false;
				}
				return false;
			} else
				return true;
		}
	}
	
	public class StatementExpression : Statement {
		Expression expr;
		
		public StatementExpression (ExpressionStatement expr, Location l)
		{
			this.expr = expr;
			loc = l;
		}

		public override bool Resolve (EmitContext ec)
		{
			expr = (Expression) expr.Resolve (ec);
			return expr != null;
		}
		
		public override bool Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			
			if (expr is ExpressionStatement)
				((ExpressionStatement) expr).EmitStatement (ec);
			else {
				expr.Emit (ec);
				ig.Emit (OpCodes.Pop);
			}

			return false;
		}

		public override string ToString ()
		{
			return "StatementExpression (" + expr + ")";
		}
	}

	/// <summary>
	///   Implements the return statement
	/// </summary>
	public class Return : Statement {
		public Expression Expr;
		
		public Return (Expression expr, Location l)
		{
			Expr = expr;
			loc = l;
		}

		public override bool Resolve (EmitContext ec)
		{
			if (Expr != null){
				Expr = Expr.Resolve (ec);
				if (Expr == null)
					return false;
			}
			return true;
		}
		
		public override bool Emit (EmitContext ec)
		{
			if (ec.InFinally){
				Report.Error (157,loc,"Control can not leave the body of the finally block");
				return false;
			}
			
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

				if (Expr.Type != ec.ReturnType)
					Expr = Expression.ConvertImplicitRequired (
						ec, Expr, ec.ReturnType, loc);

				if (Expr == null)
					return false;

				Expr.Emit (ec);

				if (ec.InTry || ec.InCatch)
					ec.ig.Emit (OpCodes.Stloc, ec.TemporaryReturn ());
			}

			if (ec.InTry || ec.InCatch)
				ec.ig.Emit (OpCodes.Leave, ec.ReturnLabel);
			else
				ec.ig.Emit (OpCodes.Ret);

			return true; 
		}
	}

	public class Goto : Statement {
		string target;
		Block block;
		
		public override bool Resolve (EmitContext ec)
		{
			return true;
		}
		
		public Goto (Block parent_block, string label, Location l)
		{
			block = parent_block;
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
			LabeledStatement label = block.LookupLabel (target);

			if (label == null){
				//
				// Maybe we should catch this before?
				//
				Report.Error (
					159, loc,
					"No such label `" + target + "' in this scope");
				return false;
			}
			Label l = label.LabelTarget (ec);
			ec.ig.Emit (OpCodes.Br, l);
			
			return false;
		}
	}

	public class LabeledStatement : Statement {
		string label_name;
		bool defined;
		Label label;
		
		public LabeledStatement (string label_name)
		{
			this.label_name = label_name;
		}

		public Label LabelTarget (EmitContext ec)
		{
			if (defined)
				return label;
			label = ec.ig.DefineLabel ();
			defined = true;

			return label;
		}

		public override bool Emit (EmitContext ec)
		{
			LabelTarget (ec);
			ec.ig.MarkLabel (label);

			return false;
		}
	}
	

	/// <summary>
	///   `goto default' statement
	/// </summary>
	public class GotoDefault : Statement {
		
		public GotoDefault (Location l)
		{
			loc = l;
		}

		public override bool Emit (EmitContext ec)
		{
			if (ec.Switch == null){
				Report.Error (153, loc, "goto default is only valid in a switch statement");
				return false;
			}

			if (!ec.Switch.GotDefault){
				Report.Error (159, loc, "No default target on switch statement");
				return false;
			}
			ec.ig.Emit (OpCodes.Br, ec.Switch.DefaultTarget);
			return false;
		}
	}

	/// <summary>
	///   `goto case' statement
	/// </summary>
	public class GotoCase : Statement {
		Expression expr;
		
		public GotoCase (Expression e, Location l)
		{
			expr = e;
			loc = l;
		}

		public override bool Emit (EmitContext ec)
		{
			if (ec.Switch == null){
				Report.Error (153, loc, "goto case is only valid in a switch statement");
				return false;
			}

			expr = expr.Resolve (ec);
			if (expr == null)
				return false;

			if (!(expr is Constant)){
				Report.Error (159, loc, "Target expression for goto case is not constant");
				return false;
			}

			object val = Expression.ConvertIntLiteral (
				(Constant) expr, ec.Switch.SwitchType, loc);

			if (val == null)
				return false;
					
			SwitchLabel sl = (SwitchLabel) ec.Switch.Elements [val];

			if (sl == null){
				Report.Error (
					159, loc,
					"No such label 'case " + val + "': for the goto case");
			}

			ec.ig.Emit (OpCodes.Br, sl.ILLabelCode);
			return true;
		}
	}
	
	public class Throw : Statement {
		Expression expr;
		
		public Throw (Expression expr, Location l)
		{
			this.expr = expr;
			loc = l;
		}

		public override bool Resolve (EmitContext ec)
		{
			if (expr != null){
				expr = expr.Resolve (ec);
				if (expr == null)
					return false;
			}
			return true;
		}
			
		public override bool Emit (EmitContext ec)
		{
			if (expr == null){
				if (ec.InCatch)
					ec.ig.Emit (OpCodes.Rethrow);
				else {
					Report.Error (
						156, loc,
						"A throw statement with no argument is only " +
						"allowed in a catch clause");
				}
				return false;
			}
			
			expr.Emit (ec);

			ec.ig.Emit (OpCodes.Throw);

			return true;
		}
	}

	public class Break : Statement {
		
		public Break (Location l)
		{
			loc = l;
		}

		public override bool Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;

			if (ec.InLoop == false && ec.Switch == null){
				Report.Error (139, loc, "No enclosing loop or switch to continue to");
				return false;
			}

			ec.Breaks = true;
			if (ec.InTry || ec.InCatch)
				ig.Emit (OpCodes.Leave, ec.LoopEnd);
			else
				ig.Emit (OpCodes.Br, ec.LoopEnd);

			return false;
		}
	}

	public class Continue : Statement {
		
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

			//
			// UGH: Non trivial.  This Br might cross a try/catch boundary
			// How can we tell?
			//
			// while () {
			//   try { ... } catch { continue; }
			// }
			//
			// From:
			// try {} catch { while () { continue; }}
			//
			ec.ig.Emit (OpCodes.Br, begin);
			return false;
		}
	}
	
	public class VariableInfo {
		public Expression Type;
		public LocalBuilder LocalBuilder;
		public Type VariableType;
		public readonly Location Location;
		
		public bool Used;
		public bool Assigned;
		public bool ReadOnly;
		
		public VariableInfo (Expression type, Location l)
		{
			Type = type;
			LocalBuilder = null;
			Location = l;
		}

		public void MakePinned ()
		{
			TypeManager.MakePinned (LocalBuilder);
		}				
	}
		
	/// <summary>
	///   Block represents a C# block.
	/// </summary>
	///
	/// <remarks>
	///   This class is used in a number of places: either to represent
	///   explicit blocks that the programmer places or implicit blocks.
	///
	///   Implicit blocks are used as labels or to introduce variable
	///   declarations.
	/// </remarks>
	public class Block : Statement {
		public readonly Block     Parent;
		public readonly bool      Implicit;
		public readonly Location  StartLocation;
		public Location           EndLocation;

		//
		// The statements in this block
		//
		ArrayList statements;

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
		// Keeps track of constants
		Hashtable constants;

		//
		// Maps variable names to ILGenerator.LocalBuilders
		//
		Hashtable local_builders;

		bool used = false;

		static int id;

		int this_id;
		
		public Block (Block parent)
			: this (parent, false, Location.Null, Location.Null)
		{ }

		public Block (Block parent, bool implicit_block)
			: this (parent, implicit_block, Location.Null, Location.Null)
		{ }

		public Block (Block parent, Location start, Location end)
			: this (parent, false, start, end)
		{ }

		public Block (Block parent, bool implicit_block, Location start, Location end)
		{
			if (parent != null)
				parent.AddChild (this);
			
			this.Parent = parent;
			this.Implicit = implicit_block;
			this.StartLocation = start;
			this.EndLocation = end;
			this.loc = start;
			this_id = id++;
			statements = new ArrayList ();
		}

		public int ID {
			get {
				return this_id;
			}
		}
		
		void AddChild (Block b)
		{
			if (children == null)
				children = new ArrayList ();
			
			children.Add (b);
		}

		public void SetEndLocation (Location loc)
		{
			EndLocation = loc;
		}

		/// <summary>
		///   Adds a label to the current block. 
		/// </summary>
		///
		/// <returns>
		///   false if the name already exists in this block. true
		///   otherwise.
		/// </returns>
		///
		public bool AddLabel (string name, LabeledStatement target)
		{
			if (labels == null)
				labels = new Hashtable ();
			if (labels.Contains (name))
				return false;
			
			labels.Add (name, target);
			return true;
		}

		public LabeledStatement LookupLabel (string name)
		{
			if (labels != null){
				if (labels.Contains (name))
					return ((LabeledStatement) labels [name]);
			}

			if (Parent != null)
				return Parent.LookupLabel (name);

			return null;
		}

		public VariableInfo AddVariable (Expression type, string name, Parameters pars, Location l)
		{
			if (variables == null)
				variables = new Hashtable ();

			if (GetVariableType (name) != null)
				return null;

			if (pars != null) {
				int idx = 0;
				Parameter p = pars.GetParameterByName (name, out idx);
				if (p != null) 
					return null;
			}
			
			VariableInfo vi = new VariableInfo (type, l);

			variables.Add (name, vi);

			// Console.WriteLine ("Adding {0} to {1}", name, ID);
			return vi;
		}

		public bool AddConstant (Expression type, string name, Expression value, Parameters pars, Location l)
		{
			if (AddVariable (type, name, pars, l) == null)
				return false;
			
			if (constants == null)
				constants = new Hashtable ();

			constants.Add (name, value);
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

				if (temp != null){
					return (VariableInfo) temp;
				}
			}

			if (Parent != null)
				return Parent.GetVariableInfo (name);

			return null;
		}
		
		public Expression GetVariableType (string name)
		{
			VariableInfo vi = GetVariableInfo (name);

			if (vi != null)
				return vi.Type;

			return null;
		}

		public Expression GetConstantExpression (string name)
		{
			if (constants != null) {
				object temp;
				temp = constants [name];
				
				if (temp != null)
					return (Expression) temp;
			}
			
			if (Parent != null)
				return Parent.GetConstantExpression (name);

			return null;
		}
		
		/// <summary>
		///   True if the variable named @name has been defined
		///   in this block
		/// </summary>
		public bool IsVariableDefined (string name)
		{
			// Console.WriteLine ("Looking up {0} in {1}", name, ID);
			if (variables != null) {
				if (variables.Contains (name))
					return true;
			}
			
			if (Parent != null)
				return Parent.IsVariableDefined (name);

			return false;
		}

		/// <summary>
		///   True if the variable named @name is a constant
		///  </summary>
		public bool IsConstant (string name)
		{
			Expression e = null;
			
			e = GetConstantExpression (name);
			
			return e != null;
		}
		
		/// <summary>
		///   Use to fetch the statement associated with this label
		/// </summary>
		public Statement this [string name] {
			get {
				return (Statement) labels [name];
			}
		}

		/// <returns>
		///   A list of labels that were not used within this block
		/// </returns>
		public string [] GetUnreferenced ()
		{
			// FIXME: Implement me
			return null;
		}

		public void AddStatement (Statement s)
		{
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
		
		/// <summary>
		///   Emits the variable declarations and labels.
		/// </summary>
		/// <remarks>
		///   tc: is our typecontainer (to resolve type references)
		///   ig: is the code generator:
		///   toplevel: the toplevel block.  This is used for checking 
		///   		that no two labels with the same name are used.
		/// </remarks>
		public void EmitMeta (EmitContext ec, Block toplevel)
		{
			DeclSpace ds = ec.DeclSpace;
			ILGenerator ig = ec.ig;
				
			//
			// Process this block variables
			//
			if (variables != null){
				local_builders = new Hashtable ();
				
				foreach (DictionaryEntry de in variables){
					string name = (string) de.Key;
					VariableInfo vi = (VariableInfo) de.Value;
					Type t;

					t = ds.ResolveType (vi.Type, false, vi.Location);
					if (t == null)
						continue;

					vi.VariableType = t;
					vi.LocalBuilder = ig.DeclareLocal (t);

					if (CodeGen.SymbolWriter != null)
						vi.LocalBuilder.SetLocalSymInfo (name);

					if (constants == null)
						continue;

					Expression cv = (Expression) constants [name];
					if (cv == null)
						continue;

					Expression e = cv.Resolve (ec);
					if (e == null)
						continue;

					if (!(e is Constant)){
						Report.Error (133, vi.Location,
							      "The expression being assigned to `" +
							      name + "' must be constant (" + e + ")");
						continue;
					}

					constants.Remove (name);
					constants.Add (name, e);
				}
			}

			//
			// Now, handle the children
			//
			if (children != null){
				foreach (Block b in children)
					b.EmitMeta (ec, toplevel);
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

		public override bool Resolve (EmitContext ec)
		{
			Block prev_block = ec.CurrentBlock;
			bool ok = true;

			ec.CurrentBlock = this;
			foreach (Statement s in statements){
				if (s.Resolve (ec) == false)
					ok = false;
			}

			ec.CurrentBlock = prev_block;
			return ok;
		}
		
		public override bool Emit (EmitContext ec)
		{
			bool is_ret = false;
			Block prev_block = ec.CurrentBlock;
			
			ec.CurrentBlock = this;

			if (CodeGen.SymbolWriter != null) {
				ec.Mark (StartLocation);
				
				foreach (Statement s in statements) {
					ec.Mark (s.loc);
					
					is_ret = s.Emit (ec);
				}

				ec.Mark (EndLocation); 
			} else {
				foreach (Statement s in statements)
					is_ret = s.Emit (ec);
			}
			
			ec.CurrentBlock = prev_block;
			return is_ret;
		}
	}

	public class SwitchLabel {
		Expression label;
		object converted;
		public Location loc;
		public Label ILLabel;
		public Label ILLabelCode;
		
		//
		// if expr == null, then it is the default case.
		//
		public SwitchLabel (Expression expr, Location l)
		{
			label = expr;
			loc = l;
		}

		public Expression Label {
			get {
				return label;
			}
		}

		public object Converted {
			get {
				return converted;
			}
		}
		
		//
		// Resolves the expression, reduces it to a literal if possible
		// and then converts it to the requested type.
		//
		public bool ResolveAndReduce (EmitContext ec, Type required_type)
		{
			ILLabel = ec.ig.DefineLabel ();
			ILLabelCode = ec.ig.DefineLabel ();

			if (label == null)
				return true;
			
			Expression e = label.Resolve (ec);

			if (e == null)
				return false;

			if (!(e is Constant)){
				Console.WriteLine ("Value is: " + label);
				Report.Error (150, loc, "A constant value is expected");
				return false;
			}

			if (e is StringConstant || e is NullLiteral){
				if (required_type == TypeManager.string_type){
					converted = label;
					ILLabel = ec.ig.DefineLabel ();
					return true;
				}
			}

			converted = Expression.ConvertIntLiteral ((Constant) e, required_type, loc);
			if (converted == null)
				return false;

			return true;
		}
	}

	public class SwitchSection {
		// An array of SwitchLabels.
		public readonly ArrayList Labels;
		public readonly Block Block;
		
		public SwitchSection (ArrayList labels, Block block)
		{
			Labels = labels;
			Block = block;
		}
	}
	
	public class Switch : Statement {
		public readonly ArrayList Sections;
		public Expression Expr;

		/// <summary>
		///   Maps constants whose type type SwitchType to their  SwitchLabels.
		/// </summary>
		public Hashtable Elements;

		/// <summary>
		///   The governing switch type
		/// </summary>
		public Type SwitchType;

		//
		// Computed
		//
		bool got_default;
		Label default_target;
		
		//
		// The types allowed to be implicitly cast from
		// on the governing type
		//
		static Type [] allowed_types;
		
		public Switch (Expression e, ArrayList sects, Location l)
		{
			Expr = e;
			Sections = sects;
			loc = l;
		}

		public bool GotDefault {
			get {
				return got_default;
			}
		}

		public Label DefaultTarget {
			get {
				return default_target;
			}
		}

		//
		// Determines the governing type for a switch.  The returned
		// expression might be the expression from the switch, or an
		// expression that includes any potential conversions to the
		// integral types or to string.
		//
		Expression SwitchGoverningType (EmitContext ec, Type t)
		{
			if (t == TypeManager.int32_type ||
			    t == TypeManager.uint32_type ||
			    t == TypeManager.char_type ||
			    t == TypeManager.byte_type ||
			    t == TypeManager.sbyte_type ||
			    t == TypeManager.ushort_type ||
			    t == TypeManager.short_type ||
			    t == TypeManager.uint64_type ||
			    t == TypeManager.int64_type ||
			    t == TypeManager.string_type ||
				t == TypeManager.bool_type ||
				t.IsSubclassOf (TypeManager.enum_type))
				return Expr;

			if (allowed_types == null){
				allowed_types = new Type [] {
					TypeManager.sbyte_type,
					TypeManager.byte_type,
					TypeManager.short_type,
					TypeManager.ushort_type,
					TypeManager.int32_type,
					TypeManager.uint32_type,
					TypeManager.int64_type,
					TypeManager.uint64_type,
					TypeManager.char_type,
					TypeManager.bool_type,
					TypeManager.string_type
				};
			}

			//
			// Try to find a *user* defined implicit conversion.
			//
			// If there is no implicit conversion, or if there are multiple
			// conversions, we have to report an error
			//
			Expression converted = null;
			foreach (Type tt in allowed_types){
				Expression e;
				
				e = Expression.ImplicitUserConversion (ec, Expr, tt, loc);
				if (e == null)
					continue;

				if (converted != null){
					Report.Error (-12, loc, "More than one conversion to an integral " +
						      " type exists for type `" +
						      TypeManager.CSharpName (Expr.Type)+"'");
					return null;
				} else
					converted = e;
			}
			return converted;
		}

		void error152 (string n)
		{
			Report.Error (
				152, "The label `" + n + ":' " +
				"is already present on this switch statement");
		}
		
		//
		// Performs the basic sanity checks on the switch statement
		// (looks for duplicate keys and non-constant expressions).
		//
		// It also returns a hashtable with the keys that we will later
		// use to compute the switch tables
		//
		bool CheckSwitch (EmitContext ec)
		{
			Type compare_type;
			bool error = false;
			Elements = new Hashtable ();
				
			got_default = false;

			if (TypeManager.IsEnumType (SwitchType)){
				compare_type = TypeManager.EnumToUnderlying (SwitchType);
			} else
				compare_type = SwitchType;
			
			foreach (SwitchSection ss in Sections){
				foreach (SwitchLabel sl in ss.Labels){
					if (!sl.ResolveAndReduce (ec, SwitchType)){
						error = true;
						continue;
					}

					if (sl.Label == null){
						if (got_default){
							error152 ("default");
							error = true;
						}
						got_default = true;
						continue;
					}
					
					object key = sl.Converted;

					if (key is Constant)
						key = ((Constant) key).GetValue ();

					if (key == null)
						key = NullLiteral.Null;
					
					string lname = null;
					if (compare_type == TypeManager.uint64_type){
						ulong v = (ulong) key;

						if (Elements.Contains (v))
							lname = v.ToString ();
						else
							Elements.Add (v, sl);
					} else if (compare_type == TypeManager.int64_type){
						long v = (long) key;

						if (Elements.Contains (v))
							lname = v.ToString ();
						else
							Elements.Add (v, sl);
					} else if (compare_type == TypeManager.uint32_type){
						uint v = (uint) key;

						if (Elements.Contains (v))
							lname = v.ToString ();
						else
							Elements.Add (v, sl);
					} else if (compare_type == TypeManager.char_type){
						char v = (char) key;
						
						if (Elements.Contains (v))
							lname = v.ToString ();
						else
							Elements.Add (v, sl);
					} else if (compare_type == TypeManager.byte_type){
						byte v = (byte) key;
						
						if (Elements.Contains (v))
							lname = v.ToString ();
						else
							Elements.Add (v, sl);
					} else if (compare_type == TypeManager.sbyte_type){
						sbyte v = (sbyte) key;
						
						if (Elements.Contains (v))
							lname = v.ToString ();
						else
							Elements.Add (v, sl);
					} else if (compare_type == TypeManager.short_type){
						short v = (short) key;
						
						if (Elements.Contains (v))
							lname = v.ToString ();
						else
							Elements.Add (v, sl);
					} else if (compare_type == TypeManager.ushort_type){
						ushort v = (ushort) key;
						
						if (Elements.Contains (v))
							lname = v.ToString ();
						else
							Elements.Add (v, sl);
					} else if (compare_type == TypeManager.string_type){
						if (key is NullLiteral){
							if (Elements.Contains (NullLiteral.Null))
								lname = "null";
							else
								Elements.Add (NullLiteral.Null, null);
						} else {
							string s = (string) key;

							if (Elements.Contains (s))
								lname = s;
							else
								Elements.Add (s, sl);
						}
					} else if (compare_type == TypeManager.int32_type) {
						int v = (int) key;

						if (Elements.Contains (v))
							lname = v.ToString ();
						else
							Elements.Add (v, sl);
					} else if (compare_type == TypeManager.bool_type) {
						bool v = (bool) key;

						if (Elements.Contains (v))
							lname = v.ToString ();
						else
							Elements.Add (v, sl);
					}
					else
					{
						throw new Exception ("Unknown switch type!" +
								     SwitchType + " " + compare_type);
					}

					if (lname != null){
						error152 ("case + " + lname);
						error = true;
					}
				}
			}
			if (error)
				return false;
			
			return true;
		}

		void EmitObjectInteger (ILGenerator ig, object k)
		{
			if (k is int)
				IntConstant.EmitInt (ig, (int) k);
			else if (k is Constant) {
				EmitObjectInteger (ig, ((Constant) k).GetValue ());
			} 
			else if (k is uint)
				IntConstant.EmitInt (ig, unchecked ((int) (uint) k));
			else if (k is long)
			{
				if ((long) k >= int.MinValue && (long) k <= int.MaxValue)
				{
					IntConstant.EmitInt (ig, (int) (long) k);
					ig.Emit (OpCodes.Conv_I8);
				}
				else
					LongConstant.EmitLong (ig, (long) k);
			}
			else if (k is ulong)
			{
				if ((ulong) k < (1L<<32))
				{
					IntConstant.EmitInt (ig, (int) (long) k);
					ig.Emit (OpCodes.Conv_U8);
				}
				else
				{
					LongConstant.EmitLong (ig, unchecked ((long) (ulong) k));
				}
			}
			else if (k is char)
				IntConstant.EmitInt (ig, (int) ((char) k));
			else if (k is sbyte)
				IntConstant.EmitInt (ig, (int) ((sbyte) k));
			else if (k is byte)
				IntConstant.EmitInt (ig, (int) ((byte) k));
			else if (k is short)
				IntConstant.EmitInt (ig, (int) ((short) k));
			else if (k is ushort)
				IntConstant.EmitInt (ig, (int) ((ushort) k));
			else if (k is bool)
				IntConstant.EmitInt (ig, ((bool) k) ? 1 : 0);
			else
				throw new Exception ("Unhandled case");
		}
		
		// structure used to hold blocks of keys while calculating table switch
		class KeyBlock : IComparable
		{
			public KeyBlock (long _nFirst)
			{
				nFirst = nLast = _nFirst;
			}
			public long nFirst;
			public long nLast;
			public ArrayList rgKeys = null;
			public int Length
			{
				get { return (int) (nLast - nFirst + 1); }
			}
			public static long TotalLength (KeyBlock kbFirst, KeyBlock kbLast)
			{
				return kbLast.nLast - kbFirst.nFirst + 1;
			}
			public int CompareTo (object obj)
			{
				KeyBlock kb = (KeyBlock) obj;
				int nLength = Length;
				int nLengthOther = kb.Length;
				if (nLengthOther == nLength)
					return (int) (kb.nFirst - nFirst);
				return nLength - nLengthOther;
			}
		}

		/// <summary>
		/// This method emits code for a lookup-based switch statement (non-string)
		/// Basically it groups the cases into blocks that are at least half full,
		/// and then spits out individual lookup opcodes for each block.
		/// It emits the longest blocks first, and short blocks are just
		/// handled with direct compares.
		/// </summary>
		/// <param name="ec"></param>
		/// <param name="val"></param>
		/// <returns></returns>
		bool TableSwitchEmit (EmitContext ec, LocalBuilder val)
		{
			int cElements = Elements.Count;
			object [] rgKeys = new object [cElements];
			Elements.Keys.CopyTo (rgKeys, 0);
			Array.Sort (rgKeys);

			// initialize the block list with one element per key
			ArrayList rgKeyBlocks = new ArrayList ();
			foreach (object key in rgKeys)
				rgKeyBlocks.Add (new KeyBlock (Convert.ToInt64 (key)));

			KeyBlock kbCurr;
			// iteratively merge the blocks while they are at least half full
			// there's probably a really cool way to do this with a tree...
			while (rgKeyBlocks.Count > 1)
			{
				ArrayList rgKeyBlocksNew = new ArrayList ();
				kbCurr = (KeyBlock) rgKeyBlocks [0];
				for (int ikb = 1; ikb < rgKeyBlocks.Count; ikb++)
				{
					KeyBlock kb = (KeyBlock) rgKeyBlocks [ikb];
					if ((kbCurr.Length + kb.Length) * 2 >=  KeyBlock.TotalLength (kbCurr, kb))
					{
						// merge blocks
						kbCurr.nLast = kb.nLast;
					}
					else
					{
						// start a new block
						rgKeyBlocksNew.Add (kbCurr);
						kbCurr = kb;
					}
				}
				rgKeyBlocksNew.Add (kbCurr);
				if (rgKeyBlocks.Count == rgKeyBlocksNew.Count)
					break;
				rgKeyBlocks = rgKeyBlocksNew;
			}

			// initialize the key lists
			foreach (KeyBlock kb in rgKeyBlocks)
				kb.rgKeys = new ArrayList ();

			// fill the key lists
			int iBlockCurr = 0;
			if (rgKeyBlocks.Count > 0) {
				kbCurr = (KeyBlock) rgKeyBlocks [0];
				foreach (object key in rgKeys)
				{
					bool fNextBlock = (key is UInt64) ? (ulong) key > (ulong) kbCurr.nLast : Convert.ToInt64 (key) > kbCurr.nLast;
					if (fNextBlock)
						kbCurr = (KeyBlock) rgKeyBlocks [++iBlockCurr];
					kbCurr.rgKeys.Add (key);
				}
			}

			// sort the blocks so we can tackle the largest ones first
			rgKeyBlocks.Sort ();

			// okay now we can start...
			ILGenerator ig = ec.ig;
			Label lblEnd = ig.DefineLabel ();	// at the end ;-)
			Label lblDefault = ig.DefineLabel ();

			Type typeKeys = null;
			if (rgKeys.Length > 0)
				typeKeys = rgKeys [0].GetType ();	// used for conversions

			for (int iBlock = rgKeyBlocks.Count - 1; iBlock >= 0; --iBlock)
			{
				KeyBlock kb = ((KeyBlock) rgKeyBlocks [iBlock]);
				lblDefault = (iBlock == 0) ? DefaultTarget : ig.DefineLabel ();
				if (kb.Length <= 2)
				{
					foreach (object key in kb.rgKeys)
					{
						ig.Emit (OpCodes.Ldloc, val);
						EmitObjectInteger (ig, key);
						SwitchLabel sl = (SwitchLabel) Elements [key];
						ig.Emit (OpCodes.Beq, sl.ILLabel);
					}
				}
				else
				{
					// TODO: if all the keys in the block are the same and there are
					//       no gaps/defaults then just use a range-check.
					if (SwitchType == TypeManager.int64_type ||
						SwitchType == TypeManager.uint64_type)
					{
						// TODO: optimize constant/I4 cases

						// check block range (could be > 2^31)
						ig.Emit (OpCodes.Ldloc, val);
						EmitObjectInteger (ig, Convert.ChangeType (kb.nFirst, typeKeys));
						ig.Emit (OpCodes.Blt, lblDefault);
						ig.Emit (OpCodes.Ldloc, val);
						EmitObjectInteger (ig, Convert.ChangeType (kb.nFirst, typeKeys));
						ig.Emit (OpCodes.Bgt, lblDefault);

						// normalize range
						ig.Emit (OpCodes.Ldloc, val);
						if (kb.nFirst != 0)
						{
							EmitObjectInteger (ig, Convert.ChangeType (kb.nFirst, typeKeys));
							ig.Emit (OpCodes.Sub);
						}
						ig.Emit (OpCodes.Conv_I4);	// assumes < 2^31 labels!
					}
					else
					{
						// normalize range
						ig.Emit (OpCodes.Ldloc, val);
						int nFirst = (int) kb.nFirst;
						if (nFirst > 0)
						{
							IntConstant.EmitInt (ig, nFirst);
							ig.Emit (OpCodes.Sub);
						}
						else if (nFirst < 0)
						{
							IntConstant.EmitInt (ig, -nFirst);
							ig.Emit (OpCodes.Add);
						}
					}

					// first, build the list of labels for the switch
					int iKey = 0;
					int cJumps = kb.Length;
					Label [] rgLabels = new Label [cJumps];
					for (int iJump = 0; iJump < cJumps; iJump++)
					{
						object key = kb.rgKeys [iKey];
						if (Convert.ToInt64 (key) == kb.nFirst + iJump)
						{
							SwitchLabel sl = (SwitchLabel) Elements [key];
							rgLabels [iJump] = sl.ILLabel;
							iKey++;
						}
						else
							rgLabels [iJump] = lblDefault;
					}
					// emit the switch opcode
					ig.Emit (OpCodes.Switch, rgLabels);
				}

				// mark the default for this block
				if (iBlock != 0)
					ig.MarkLabel (lblDefault);
			}

			// TODO: find the default case and emit it here,
			//       to prevent having to do the following jump.
			//       make sure to mark other labels in the default section

			// the last default just goes to the end
			ig.Emit (OpCodes.Br, lblDefault);

			// now emit the code for the sections
			bool fFoundDefault = false;
			bool fAllReturn = true;
			foreach (SwitchSection ss in Sections)
			{
				foreach (SwitchLabel sl in ss.Labels)
				{
					ig.MarkLabel (sl.ILLabel);
					ig.MarkLabel (sl.ILLabelCode);
					if (sl.Label == null)
					{
						ig.MarkLabel (lblDefault);
						fFoundDefault = true;
					}
				}
				fAllReturn &= ss.Block.Emit (ec);
				//ig.Emit (OpCodes.Br, lblEnd);
			}
			
			if (!fFoundDefault) {
				ig.MarkLabel (lblDefault);
				fAllReturn = false;
			}
			ig.MarkLabel (lblEnd);

			return fAllReturn;
		}
		//
		// This simple emit switch works, but does not take advantage of the
		// `switch' opcode. 
		// TODO: remove non-string logic from here
		// TODO: binary search strings?
		//
		bool SimpleSwitchEmit (EmitContext ec, LocalBuilder val)
		{
			ILGenerator ig = ec.ig;
			Label end_of_switch = ig.DefineLabel ();
			Label next_test = ig.DefineLabel ();
			Label null_target = ig.DefineLabel ();
			bool default_found = false;
			bool first_test = true;
			bool pending_goto_end = false;
			bool all_return = true;
			bool is_string = false;
			bool null_found;
			
			//
			// Special processing for strings: we cant compare
			// against null.
			//
			if (SwitchType == TypeManager.string_type){
				ig.Emit (OpCodes.Ldloc, val);
				is_string = true;
				
				if (Elements.Contains (NullLiteral.Null)){
					ig.Emit (OpCodes.Brfalse, null_target);
				} else
					ig.Emit (OpCodes.Brfalse, default_target);

				ig.Emit (OpCodes.Ldloc, val);
				ig.Emit (OpCodes.Call, TypeManager.string_isinterneted_string);
				ig.Emit (OpCodes.Stloc, val);
			}

			SwitchSection last_section;
			last_section = (SwitchSection) Sections [Sections.Count-1];
			
			foreach (SwitchSection ss in Sections){
				Label sec_begin = ig.DefineLabel ();

				if (pending_goto_end)
					ig.Emit (OpCodes.Br, end_of_switch);

				int label_count = ss.Labels.Count;
				null_found = false;
				foreach (SwitchLabel sl in ss.Labels){
					ig.MarkLabel (sl.ILLabel);
					
					if (!first_test){
						ig.MarkLabel (next_test);
						next_test = ig.DefineLabel ();
					}
					//
					// If we are the default target
					//
					if (sl.Label == null){
						ig.MarkLabel (default_target);
						default_found = true;
					} else {
						object lit = sl.Converted;

						if (lit is NullLiteral){
							null_found = true;
							if (label_count == 1)
								ig.Emit (OpCodes.Br, next_test);
							continue;
									      
						}
						if (is_string){
							StringConstant str = (StringConstant) lit;

							ig.Emit (OpCodes.Ldloc, val);
							ig.Emit (OpCodes.Ldstr, str.Value);
							if (label_count == 1)
								ig.Emit (OpCodes.Bne_Un, next_test);
							else
								ig.Emit (OpCodes.Beq, sec_begin);
						} else {
							ig.Emit (OpCodes.Ldloc, val);
							EmitObjectInteger (ig, lit);
							ig.Emit (OpCodes.Ceq);
							if (label_count == 1)
								ig.Emit (OpCodes.Brfalse, next_test);
							else
								ig.Emit (OpCodes.Brtrue, sec_begin);
						}
					}
				}
				if (label_count != 1 && ss != last_section)
					ig.Emit (OpCodes.Br, next_test);
				
				if (null_found)
					ig.MarkLabel (null_target);
				ig.MarkLabel (sec_begin);
				foreach (SwitchLabel sl in ss.Labels)
					ig.MarkLabel (sl.ILLabelCode);
				if (ss.Block.Emit (ec))
					pending_goto_end = false;
				else {
					all_return = false;
					pending_goto_end = true;
				}
				first_test = false;
			}
			if (!default_found){
				ig.MarkLabel (default_target);
				all_return = false;
			}
			ig.MarkLabel (next_test);
			ig.MarkLabel (end_of_switch);
			
			return all_return;
		}

		public override bool Resolve (EmitContext ec)
		{
			foreach (SwitchSection ss in Sections){
				if (ss.Block.Resolve (ec) != true)
					return false;
			}

			return true;
		}
		
		public override bool Emit (EmitContext ec)
		{
			Expr = Expr.Resolve (ec);
			if (Expr == null)
				return false;

			Expression new_expr = SwitchGoverningType (ec, Expr.Type);
			if (new_expr == null){
				Report.Error (151, loc, "An integer type or string was expected for switch");
				return false;
			}

			// Validate switch.
			SwitchType = new_expr.Type;

			if (!CheckSwitch (ec))
				return false;

			// Store variable for comparission purposes
			LocalBuilder value = ec.ig.DeclareLocal (SwitchType);
			new_expr.Emit (ec);
			ec.ig.Emit (OpCodes.Stloc, value);

			ILGenerator ig = ec.ig;

			default_target = ig.DefineLabel ();

			//
			// Setup the codegen context
			//
			Label old_end = ec.LoopEnd;
			Switch old_switch = ec.Switch;
			
			ec.LoopEnd = ig.DefineLabel ();
			ec.Switch = this;

			// Emit Code.
			bool all_return;
			if (SwitchType == TypeManager.string_type)
				all_return = SimpleSwitchEmit (ec, value);
			else
				all_return = TableSwitchEmit (ec, value);

			// Restore context state. 
			ig.MarkLabel (ec.LoopEnd);

			//
			// Restore the previous context
			//
			ec.LoopEnd = old_end;
			ec.Switch = old_switch;
			
			return all_return;
		}
	}

	public class Lock : Statement {
		Expression expr;
		Statement Statement;
			
		public Lock (Expression expr, Statement stmt, Location l)
		{
			this.expr = expr;
			Statement = stmt;
			loc = l;
		}

		public override bool Resolve (EmitContext ec)
		{
			expr = expr.Resolve (ec);
			return Statement.Resolve (ec) && expr != null;
		}
		
		public override bool Emit (EmitContext ec)
		{
			Type type = expr.Type;
			bool val;
			
			if (type.IsValueType){
				Report.Error (185, loc, "lock statement requires the expression to be " +
					      " a reference type (type is: `" +
					      TypeManager.CSharpName (type) + "'");
				return false;
			}

			ILGenerator ig = ec.ig;
			LocalBuilder temp = ig.DeclareLocal (type);
				
			expr.Emit (ec);
			ig.Emit (OpCodes.Dup);
			ig.Emit (OpCodes.Stloc, temp);
			ig.Emit (OpCodes.Call, TypeManager.void_monitor_enter_object);

			// try
			Label end = ig.BeginExceptionBlock ();
			bool old_in_try = ec.InTry;
			ec.InTry = true;
			Label finish = ig.DefineLabel ();
			val = Statement.Emit (ec);
			ec.InTry = old_in_try;
			// ig.Emit (OpCodes.Leave, finish);

			ig.MarkLabel (finish);
			
			// finally
			ig.BeginFinallyBlock ();
			ig.Emit (OpCodes.Ldloc, temp);
			ig.Emit (OpCodes.Call, TypeManager.void_monitor_exit_object);
			ig.EndExceptionBlock ();
			
			return val;
		}
	}

	public class Unchecked : Statement {
		public readonly Block Block;
		
		public Unchecked (Block b)
		{
			Block = b;
		}

		public override bool Resolve (EmitContext ec)
		{
			return Block.Resolve (ec);
		}
		
		public override bool Emit (EmitContext ec)
		{
			bool previous_state = ec.CheckState;
			bool previous_state_const = ec.ConstantCheckState;
			bool val;
			
			ec.CheckState = false;
			ec.ConstantCheckState = false;
			val = Block.Emit (ec);
			ec.CheckState = previous_state;
			ec.ConstantCheckState = previous_state_const;

			return val;
		}
	}

	public class Checked : Statement {
		public readonly Block Block;
		
		public Checked (Block b)
		{
			Block = b;
		}

		public override bool Resolve (EmitContext ec)
		{
			bool previous_state = ec.CheckState;
			bool previous_state_const = ec.ConstantCheckState;
			
			ec.CheckState = true;
			ec.ConstantCheckState = true;
			bool ret = Block.Resolve (ec);
			ec.CheckState = previous_state;
			ec.ConstantCheckState = previous_state_const;

			return ret;
		}

		public override bool Emit (EmitContext ec)
		{
			bool previous_state = ec.CheckState;
			bool previous_state_const = ec.ConstantCheckState;
			bool val;
			
			ec.CheckState = true;
			ec.ConstantCheckState = true;
			val = Block.Emit (ec);
			ec.CheckState = previous_state;
			ec.ConstantCheckState = previous_state_const;

			return val;
		}
	}

	public class Unsafe : Statement {
		public readonly Block Block;

		public Unsafe (Block b)
		{
			Block = b;
		}

		public override bool Resolve (EmitContext ec)
		{
			bool previous_state = ec.InUnsafe;
			bool val;
			
			ec.InUnsafe = true;
			val = Block.Resolve (ec);
			ec.InUnsafe = previous_state;

			return val;
		}
		
		public override bool Emit (EmitContext ec)
		{
			bool previous_state = ec.InUnsafe;
			bool val;
			
			ec.InUnsafe = true;
			val = Block.Emit (ec);
			ec.InUnsafe = previous_state;

			return val;
		}
	}

	// 
	// Fixed statement
	//
	public class Fixed : Statement {
		Expression type;
		ArrayList declarators;
		Statement statement;

		public Fixed (Expression type, ArrayList decls, Statement stmt, Location l)
		{
			this.type = type;
			declarators = decls;
			statement = stmt;
			loc = l;
		}

		public override bool Resolve (EmitContext ec)
		{
			return statement.Resolve (ec);
		}
		
		public override bool Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			Type t;
			
			t = ec.DeclSpace.ResolveType (type, false, loc);
			if (t == null)
				return false;

			bool is_ret = false;

			foreach (Pair p in declarators){
				VariableInfo vi = (VariableInfo) p.First;
				Expression e = (Expression) p.Second;

				//
				// The rules for the possible declarators are pretty wise,
				// but the production on the grammar is more concise.
				//
				// So we have to enforce these rules here.
				//
				// We do not resolve before doing the case 1 test,
				// because the grammar is explicit in that the token &
				// is present, so we need to test for this particular case.
				//

				//
				// Case 1: & object.
				//
				if (e is Unary && ((Unary) e).Oper == Unary.Operator.AddressOf){
					Expression child = ((Unary) e).Expr;

					vi.MakePinned ();
					if (child is ParameterReference || child is LocalVariableReference){
						Report.Error (
							213, loc, 
							"No need to use fixed statement for parameters or " +
							"local variable declarations (address is already " +
							"fixed)");
						continue;
					}
					
					e = e.Resolve (ec);
					if (e == null)
						continue;

					child = ((Unary) e).Expr;
					
					if (!TypeManager.VerifyUnManaged (child.Type, loc))
						continue;

					//
					// Store pointer in pinned location
					//
					e.Emit (ec);
					ig.Emit (OpCodes.Stloc, vi.LocalBuilder);

					is_ret = statement.Emit (ec);

					// Clear the pinned variable.
					ig.Emit (OpCodes.Ldc_I4_0);
					ig.Emit (OpCodes.Conv_U);
					ig.Emit (OpCodes.Stloc, vi.LocalBuilder);

					continue;
				}

				e = e.Resolve (ec);
				if (e == null)
					continue;

				//
				// Case 2: Array
				//
				if (e.Type.IsArray){
					Type array_type = e.Type.GetElementType ();
					
					vi.MakePinned ();
					//
					// Provided that array_type is unmanaged,
					//
					if (!TypeManager.VerifyUnManaged (array_type, loc))
						continue;

					//
					// and T* is implicitly convertible to the
					// pointer type given in the fixed statement.
					//
					ArrayPtr array_ptr = new ArrayPtr (e);
					
					Expression converted = Expression.ConvertImplicitRequired (
						ec, array_ptr, vi.VariableType, loc);
					if (converted == null)
						continue;

					//
					// Store pointer in pinned location
					//
					converted.Emit (ec);
					
					ig.Emit (OpCodes.Stloc, vi.LocalBuilder);

					is_ret = statement.Emit (ec);
					
					// Clear the pinned variable.
					ig.Emit (OpCodes.Ldc_I4_0);
					ig.Emit (OpCodes.Conv_U);
					ig.Emit (OpCodes.Stloc, vi.LocalBuilder);

					continue;
				}

				//
				// Case 3: string
				//
				if (e.Type == TypeManager.string_type){
					LocalBuilder pinned_string = ig.DeclareLocal (TypeManager.string_type);
					TypeManager.MakePinned (pinned_string);
					
					e.Emit (ec);
					ig.Emit (OpCodes.Stloc, pinned_string);

					Expression sptr = new StringPtr (pinned_string);
					Expression converted = Expression.ConvertImplicitRequired (
						ec, sptr, vi.VariableType, loc);
					
					if (converted == null)
						continue;

					converted.Emit (ec);
					ig.Emit (OpCodes.Stloc, vi.LocalBuilder);
					
					is_ret = statement.Emit (ec);

					// Clear the pinned variable
					ig.Emit (OpCodes.Ldnull);
					ig.Emit (OpCodes.Stloc, pinned_string);
				}
			}

			return is_ret;
		}
	}
	
	public class Catch {
		public readonly Expression Type;
		public readonly string Name;
		public readonly Block  Block;
		public readonly Location Location;
		
		public Catch (Expression type, string name, Block block, Location l)
		{
			Type = type;
			Name = name;
			Block = block;
			Location = l;
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

		public override bool Resolve (EmitContext ec)
		{
			bool ok = true;
			
			if (General != null)
				if (!General.Block.Resolve (ec))
					ok = false;

			foreach (Catch c in Specific){
				if (!c.Block.Resolve (ec))
					ok = false;
			}

			if (!Block.Resolve (ec))
				ok = false;

			if (Fini != null)
				if (!Fini.Resolve (ec))
					ok = false;
			
			return ok;
		}
		
		public override bool Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			Label end;
			Label finish = ig.DefineLabel ();;
			bool returns;
			
			end = ig.BeginExceptionBlock ();
			bool old_in_try = ec.InTry;
			ec.InTry = true;
			returns = Block.Emit (ec);
			ec.InTry = old_in_try;

			//
			// System.Reflection.Emit provides this automatically:
			// ig.Emit (OpCodes.Leave, finish);

			bool old_in_catch = ec.InCatch;
			ec.InCatch = true;
			DeclSpace ds = ec.DeclSpace;

			foreach (Catch c in Specific){
				Type catch_type = ds.ResolveType (c.Type, false, c.Location);
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
				
				if (!c.Block.Emit (ec))
					returns = false;
			}

			if (General != null){
				ig.BeginCatchBlock (TypeManager.object_type);
				ig.Emit (OpCodes.Pop);
				if (!General.Block.Emit (ec))
					returns = false;
			}
			ec.InCatch = old_in_catch;

			ig.MarkLabel (finish);
			if (Fini != null){
				ig.BeginFinallyBlock ();
				bool old_in_finally = ec.InFinally;
				ec.InFinally = true;
				Fini.Emit (ec);
				ec.InFinally = old_in_finally;
			}
			
			ig.EndExceptionBlock ();

			//
			// FIXME: Is this correct?
			// Replace with `returns' and check test-18, maybe we can
			// perform an optimization here.
			//
			return returns;
		}
	}

	//
	// FIXME: We still do not support the expression variant of the using
	// statement.
	//
	public class Using : Statement {
		object expression_or_block;
		Statement Statement;
		
		public Using (object expression_or_block, Statement stmt, Location l)
		{
			this.expression_or_block = expression_or_block;
			Statement = stmt;
			loc = l;
		}

		//
		// Emits the code for the case of using using a local variable declaration.
		//
		bool EmitLocalVariableDecls (EmitContext ec, Expression expr_type, ArrayList var_list)
		{
			ILGenerator ig = ec.ig;
			Expression [] converted_vars;
			bool need_conv = false;
			Type type = ec.DeclSpace.ResolveType (expr_type, false, loc);
			int i = 0;

			if (type == null)
				return false;
			
			//
			// The type must be an IDisposable or an implicit conversion
			// must exist.
			//
			converted_vars = new Expression [var_list.Count];
			if (!TypeManager.ImplementsInterface (type, TypeManager.idisposable_type)){
				foreach (DictionaryEntry e in var_list){
					Expression var = (Expression) e.Key;

					var = var.Resolve (ec);
					if (var == null)
						return false;
					
					converted_vars [i] = Expression.ConvertImplicit (
						ec, var, TypeManager.idisposable_type, loc);

					if (converted_vars [i] == null)
						return false;
					i++;
				}
				need_conv = true;
			}
			
			i = 0;
			bool old_in_try = ec.InTry;
			ec.InTry = true;
			bool error = false;
			foreach (DictionaryEntry e in var_list){
				LocalVariableReference var = (LocalVariableReference) e.Key;
				Expression expr = (Expression) e.Value;
				Expression a;

				a = new Assign (var, expr, loc);
				a = a.Resolve (ec);
				if (!need_conv)
					converted_vars [i] = var;
				i++;
				if (a == null){
					error = true;
					continue;
				}
				((ExpressionStatement) a).EmitStatement (ec);
				
				ig.BeginExceptionBlock ();

			}
			if (error)
				return false;
			Statement.Emit (ec);
			ec.InTry = old_in_try;

			bool old_in_finally = ec.InFinally;
			ec.InFinally = true;
			var_list.Reverse ();
			foreach (DictionaryEntry e in var_list){
				LocalVariableReference var = (LocalVariableReference) e.Key;
				Label skip = ig.DefineLabel ();
				i--;
				
				ig.BeginFinallyBlock ();
				
				var.Emit (ec);
				ig.Emit (OpCodes.Brfalse, skip);
				converted_vars [i].Emit (ec);
				ig.Emit (OpCodes.Callvirt, TypeManager.void_dispose_void);
				ig.MarkLabel (skip);
				ig.EndExceptionBlock ();
			}
			ec.InFinally = old_in_finally;

			return false;
		}

		bool EmitExpression (EmitContext ec, Expression expr)
		{
			Type expr_type = expr.Type;
			Expression conv = null;
			
			if (!TypeManager.ImplementsInterface (expr_type, TypeManager.idisposable_type)){
				conv = Expression.ConvertImplicit (
					ec, expr, TypeManager.idisposable_type, loc);

				if (conv == null)
					return false;
			}

			//
			// Make a copy of the expression and operate on that.
			//
			ILGenerator ig = ec.ig;
			LocalBuilder local_copy = ig.DeclareLocal (expr_type);
			if (conv != null)
				conv.Emit (ec);
			else
				expr.Emit (ec);
			ig.Emit (OpCodes.Stloc, local_copy);

			bool old_in_try = ec.InTry;
			ec.InTry = true;
			ig.BeginExceptionBlock ();
			Statement.Emit (ec);
			ec.InTry = old_in_try;
			
			Label skip = ig.DefineLabel ();
			bool old_in_finally = ec.InFinally;
			ig.BeginFinallyBlock ();
			ig.Emit (OpCodes.Ldloc, local_copy);
			ig.Emit (OpCodes.Brfalse, skip);
			ig.Emit (OpCodes.Ldloc, local_copy);
			ig.Emit (OpCodes.Callvirt, TypeManager.void_dispose_void);
			ig.MarkLabel (skip);
			ec.InFinally = old_in_finally;
			ig.EndExceptionBlock ();

			return false;
		}
		
		public override bool Resolve (EmitContext ec)
		{
			return Statement.Resolve (ec);
		}
		
		public override bool Emit (EmitContext ec)
		{
			if (expression_or_block is DictionaryEntry){
				Expression expr_type = (Expression) ((DictionaryEntry) expression_or_block).Key;
				ArrayList var_list = (ArrayList)((DictionaryEntry)expression_or_block).Value;

				return EmitLocalVariableDecls (ec, expr_type, var_list);
			} if (expression_or_block is Expression){
				Expression e = (Expression) expression_or_block;

				e = e.Resolve (ec);
				if (e == null)
					return false;

				return EmitExpression (ec, e);
			}
			return false;
		}
	}

	/// <summary>
	///   Implementation of the foreach C# statement
	/// </summary>
	public class Foreach : Statement {
		Expression type;
		LocalVariableReference variable;
		Expression expr;
		Statement statement;
		
		public Foreach (Expression type, LocalVariableReference var, Expression expr,
				Statement stmt, Location l)
		{
			this.type = type;
			this.variable = var;
			this.expr = expr;
			statement = stmt;
			loc = l;
		}
		
		public override bool Resolve (EmitContext ec)
		{
			expr = expr.Resolve (ec);
			return statement.Resolve (ec) && expr != null;
		}
		
		//
		// Retrieves a `public bool MoveNext ()' method from the Type `t'
		//
		static MethodInfo FetchMethodMoveNext (Type t)
		{
			MemberInfo [] move_next_list;
			
			move_next_list = TypeContainer.FindMembers (
				t, MemberTypes.Method,
				BindingFlags.Public | BindingFlags.Instance,
				Type.FilterName, "MoveNext");
			if (move_next_list == null || move_next_list.Length == 0)
				return null;

			foreach (MemberInfo m in move_next_list){
				MethodInfo mi = (MethodInfo) m;
				Type [] args;
				
				args = TypeManager.GetArgumentTypes (mi);
				if (args != null && args.Length == 0){
					if (mi.ReturnType == TypeManager.bool_type)
						return mi;
				}
			}
			return null;
		}
		
		//
		// Retrieves a `public T get_Current ()' method from the Type `t'
		//
		static MethodInfo FetchMethodGetCurrent (Type t)
		{
			MemberInfo [] move_next_list;
			
			move_next_list = TypeContainer.FindMembers (
				t, MemberTypes.Method,
				BindingFlags.Public | BindingFlags.Instance,
				Type.FilterName, "get_Current");
			if (move_next_list == null || move_next_list.Length == 0)
				return null;

			foreach (MemberInfo m in move_next_list){
				MethodInfo mi = (MethodInfo) m;
				Type [] args;

				args = TypeManager.GetArgumentTypes (mi);
				if (args != null && args.Length == 0)
					return mi;
			}
			return null;
		}

		// 
		// This struct records the helper methods used by the Foreach construct
		//
		class ForeachHelperMethods {
			public EmitContext ec;
			public MethodInfo get_enumerator;
			public MethodInfo move_next;
			public MethodInfo get_current;

			public ForeachHelperMethods (EmitContext ec)
			{
				this.ec = ec;
			}
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
			Type [] args = TypeManager.GetArgumentTypes (mi);
			if (args != null){
				if (args.Length != 0)
					return false;
			}
			ForeachHelperMethods hm = (ForeachHelperMethods) criteria;
			EmitContext ec = hm.ec;

			//
			// Check whether GetEnumerator is accessible to us
			//
			MethodAttributes prot = mi.Attributes & MethodAttributes.MemberAccessMask;

			Type declaring = mi.DeclaringType;
			if (prot == MethodAttributes.Private){
				if (declaring != ec.ContainerType)
					return false;
			} else if (prot == MethodAttributes.FamANDAssem){
				// If from a different assembly, false
				if (!(mi is MethodBuilder))
					return false;
				//
				// Are we being invoked from the same class, or from a derived method?
				//
				if (ec.ContainerType != declaring){
					if (!ec.ContainerType.IsSubclassOf (declaring))
						return false;
				}
			} else if (prot == MethodAttributes.FamORAssem){
				if (!(mi is MethodBuilder ||
				      ec.ContainerType == declaring ||
				      ec.ContainerType.IsSubclassOf (declaring)))
					return false;
			} if (prot == MethodAttributes.Family){
				if (!(ec.ContainerType == declaring ||
				      ec.ContainerType.IsSubclassOf (declaring)))
					return false;
			}

			//
			// Ok, we can access it, now make sure that we can do something
			// with this `GetEnumerator'
			//

			if (mi.ReturnType == TypeManager.ienumerator_type ||
			    TypeManager.ienumerator_type.IsAssignableFrom (mi.ReturnType) ||
			    (!RootContext.StdLib && TypeManager.ImplementsInterface (mi.ReturnType, TypeManager.ienumerator_type))) {
				hm.move_next = TypeManager.bool_movenext_void;
				hm.get_current = TypeManager.object_getcurrent_void;
				return true;
			}

			//
			// Ok, so they dont return an IEnumerable, we will have to
			// find if they support the GetEnumerator pattern.
			//
			Type return_type = mi.ReturnType;

			hm.move_next = FetchMethodMoveNext (return_type);
			if (hm.move_next == null)
				return false;
			hm.get_current = FetchMethodGetCurrent (return_type);
			if (hm.get_current == null)
				return false;

			return true;
		}
		
		/// <summary>
		///   This filter is used to find the GetEnumerator method
		///   on which IEnumerator operates
		/// </summary>
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

		static bool TryType (Type t, ForeachHelperMethods hm)
		{
			MemberInfo [] mi;
			
			mi = TypeContainer.FindMembers (t, MemberTypes.Method,
							BindingFlags.Public | BindingFlags.NonPublic |
							BindingFlags.Instance,
							FilterEnumerator, hm);

			if (mi == null || mi.Length == 0)
				return false;

			hm.get_enumerator = (MethodInfo) mi [0];
			return true;	
		}
		
		//
		// Looks for a usable GetEnumerator in the Type, and if found returns
		// the three methods that participate: GetEnumerator, MoveNext and get_Current
		//
		ForeachHelperMethods ProbeCollectionType (EmitContext ec, Type t)
		{
			ForeachHelperMethods hm = new ForeachHelperMethods (ec);

			if (TryType (t, hm))
				return hm;

			//
			// Now try to find the method in the interfaces
			//
			while (t != null){
				Type [] ifaces = t.GetInterfaces ();

				foreach (Type i in ifaces){
					if (TryType (i, hm))
						return hm;
				}
				
				//
				// Since TypeBuilder.GetInterfaces only returns the interface
				// types for this type, we have to keep looping, but once
				// we hit a non-TypeBuilder (ie, a Type), then we know we are
				// done, because it returns all the types
				//
				if ((t is TypeBuilder))
					t = t.BaseType;
				else
					break;
			} 

			return null;
		}

		//
		// FIXME: possible optimization.
		// We might be able to avoid creating `empty' if the type is the sam
		//
		bool EmitCollectionForeach (EmitContext ec, Type var_type, ForeachHelperMethods hm)
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
			//
			if (expr.Type.IsValueType){
				if (expr is IMemoryLocation){
					IMemoryLocation ml = (IMemoryLocation) expr;

					ml.AddressOf (ec, AddressOp.Load);
				} else
					throw new Exception ("Expr " + expr + " of type " + expr.Type +
							     " does not implement IMemoryLocation");
				ig.Emit (OpCodes.Call, hm.get_enumerator);
			} else {
				expr.Emit (ec);
				ig.Emit (OpCodes.Callvirt, hm.get_enumerator);
			}
			ig.Emit (OpCodes.Stloc, enumerator);

			//
			// Protect the code in a try/finalize block, so that
			// if the beast implement IDisposable, we get rid of it
			//
			Label l = ig.BeginExceptionBlock ();
			bool old_in_try = ec.InTry;
			ec.InTry = true;
			
			Label end_try = ig.DefineLabel ();
			
			ig.MarkLabel (ec.LoopBegin);
			ig.Emit (OpCodes.Ldloc, enumerator);
			ig.Emit (OpCodes.Callvirt, hm.move_next);
			ig.Emit (OpCodes.Brfalse, end_try);
			ig.Emit (OpCodes.Ldloc, enumerator);
			ig.Emit (OpCodes.Callvirt, hm.get_current);
			variable.EmitAssign (ec, conv);
			statement.Emit (ec);
			ig.Emit (OpCodes.Br, ec.LoopBegin);
			ig.MarkLabel (end_try);
			ec.InTry = old_in_try;
			
			// The runtime provides this for us.
			// ig.Emit (OpCodes.Leave, end);

			//
			// Now the finally block
			//
			Label end_finally = ig.DefineLabel ();
			bool old_in_finally = ec.InFinally;
			ec.InFinally = true;
			ig.BeginFinallyBlock ();
			
			ig.Emit (OpCodes.Ldloc, enumerator);
			ig.Emit (OpCodes.Isinst, TypeManager.idisposable_type);
			ig.Emit (OpCodes.Stloc, disposable);
			ig.Emit (OpCodes.Ldloc, disposable);
			ig.Emit (OpCodes.Brfalse, end_finally);
			ig.Emit (OpCodes.Ldloc, disposable);
			ig.Emit (OpCodes.Callvirt, TypeManager.void_dispose_void);
			ig.MarkLabel (end_finally);
			ec.InFinally = old_in_finally;

			// The runtime generates this anyways.
			// ig.Emit (OpCodes.Endfinally);

			ig.EndExceptionBlock ();

			ig.MarkLabel (ec.LoopEnd);
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
			Expression empty = new EmptyExpression (element_type);
			
			conv = Expression.ConvertExplicit (ec, empty, var_type, loc);
			if (conv == null)
				return false;

			int rank = array_type.GetArrayRank ();
			ILGenerator ig = ec.ig;

			LocalBuilder copy = ig.DeclareLocal (array_type);
			
			//
			// Make our copy of the array
			//
			expr.Emit (ec);
			ig.Emit (OpCodes.Stloc, copy);
			
			if (rank == 1){
				LocalBuilder counter = ig.DeclareLocal (TypeManager.int32_type);

				Label loop, test;
				
				ig.Emit (OpCodes.Ldc_I4_0);
				ig.Emit (OpCodes.Stloc, counter);
				test = ig.DefineLabel ();
				ig.Emit (OpCodes.Br, test);

				loop = ig.DefineLabel ();
				ig.MarkLabel (loop);

				ig.Emit (OpCodes.Ldloc, copy);
				ig.Emit (OpCodes.Ldloc, counter);
				ArrayAccess.EmitLoadOpcode (ig, var_type);

				variable.EmitAssign (ec, conv);

				statement.Emit (ec);

				ig.MarkLabel (ec.LoopBegin);
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
				LocalBuilder [] dim_len   = new LocalBuilder [rank];
				LocalBuilder [] dim_count = new LocalBuilder [rank];
				Label [] loop = new Label [rank];
				Label [] test = new Label [rank];
				int dim;
				
				for (dim = 0; dim < rank; dim++){
					dim_len [dim] = ig.DeclareLocal (TypeManager.int32_type);
					dim_count [dim] = ig.DeclareLocal (TypeManager.int32_type);
					test [dim] = ig.DefineLabel ();
					loop [dim] = ig.DefineLabel ();
				}
					
				for (dim = 0; dim < rank; dim++){
					ig.Emit (OpCodes.Ldloc, copy);
					IntLiteral.EmitInt (ig, dim);
					ig.Emit (OpCodes.Callvirt, TypeManager.int_getlength_int);
					ig.Emit (OpCodes.Stloc, dim_len [dim]);
				}

				for (dim = 0; dim < rank; dim++){
					ig.Emit (OpCodes.Ldc_I4_0);
					ig.Emit (OpCodes.Stloc, dim_count [dim]);
					ig.Emit (OpCodes.Br, test [dim]);
					ig.MarkLabel (loop [dim]);
				}

				ig.Emit (OpCodes.Ldloc, copy);
				for (dim = 0; dim < rank; dim++)
					ig.Emit (OpCodes.Ldloc, dim_count [dim]);

				//
				// FIXME: Maybe we can cache the computation of `get'?
				//
				Type [] args = new Type [rank];
				MethodInfo get;

				for (int i = 0; i < rank; i++)
					args [i] = TypeManager.int32_type;

				ModuleBuilder mb = CodeGen.ModuleBuilder;
				get = mb.GetArrayMethod (
					array_type, "Get",
					CallingConventions.HasThis| CallingConventions.Standard,
					var_type, args);
				ig.Emit (OpCodes.Call, get);
				variable.EmitAssign (ec, conv);
				statement.Emit (ec);
				ig.MarkLabel (ec.LoopBegin);
				for (dim = rank - 1; dim >= 0; dim--){
					ig.Emit (OpCodes.Ldloc, dim_count [dim]);
					ig.Emit (OpCodes.Ldc_I4_1);
					ig.Emit (OpCodes.Add);
					ig.Emit (OpCodes.Stloc, dim_count [dim]);

					ig.MarkLabel (test [dim]);
					ig.Emit (OpCodes.Ldloc, dim_count [dim]);
					ig.Emit (OpCodes.Ldloc, dim_len [dim]);
					ig.Emit (OpCodes.Blt, loop [dim]);
				}
			}
			ig.MarkLabel (ec.LoopEnd);
			
			return false;
		}
		
		public override bool Emit (EmitContext ec)
		{
			Type var_type;
			bool ret_val;
			
			var_type = ec.DeclSpace.ResolveType (type, false, loc);
			if (var_type == null)
				return false;
			
			//
			// We need an instance variable.  Not sure this is the best
			// way of doing this.
			//
			// FIXME: When we implement propertyaccess, will those turn
			// out to return values in ExprClass?  I think they should.
			//
			if (!(expr.eclass == ExprClass.Variable || expr.eclass == ExprClass.Value ||
			      expr.eclass == ExprClass.PropertyAccess)){
				error1579 (expr.Type);
				return false;
			}

			ILGenerator ig = ec.ig;
			
			Label old_begin = ec.LoopBegin, old_end = ec.LoopEnd;
			bool old_inloop = ec.InLoop;
			ec.LoopBegin = ig.DefineLabel ();
			ec.LoopEnd = ig.DefineLabel ();
			ec.InLoop = true;
			
			if (expr.Type.IsArray)
				ret_val = EmitArrayForeach (ec, var_type);
			else {
				ForeachHelperMethods hm;
				
				hm = ProbeCollectionType (ec, expr.Type);
				if (hm == null){
					error1579 (expr.Type);
					return false;
				}

				ret_val = EmitCollectionForeach (ec, var_type, hm);
			}
			
			ec.LoopBegin = old_begin;
			ec.LoopEnd = old_end;
			ec.InLoop = old_inloop;

			return ret_val;
		}
	}
}

