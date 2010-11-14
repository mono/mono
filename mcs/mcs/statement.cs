//
// statement.cs: Statement representation for the IL tree.
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Martin Baulig (martin@ximian.com)
//   Marek Safar (marek.safar@seznam.cz)
//
// Copyright 2001, 2002, 2003 Ximian, Inc.
// Copyright 2003, 2004 Novell, Inc.
//

using System;
using System.Text;
using System.Reflection.Emit;
using System.Diagnostics;
using System.Collections.Generic;

namespace Mono.CSharp {
	
	public abstract class Statement {
		public Location loc;
		
		/// <summary>
		///   Resolves the statement, true means that all sub-statements
		///   did resolve ok.
		//  </summary>
		public virtual bool Resolve (BlockContext ec)
		{
			return true;
		}

		/// <summary>
		///   We already know that the statement is unreachable, but we still
		///   need to resolve it to catch errors.
		/// </summary>
		public virtual bool ResolveUnreachable (BlockContext ec, bool warn)
		{
			//
			// This conflicts with csc's way of doing this, but IMHO it's
			// the right thing to do.
			//
			// If something is unreachable, we still check whether it's
			// correct.  This means that you cannot use unassigned variables
			// in unreachable code, for instance.
			//

			if (warn)
				ec.Report.Warning (162, 2, loc, "Unreachable code detected");

			ec.StartFlowBranching (FlowBranching.BranchingType.Block, loc);
			bool ok = Resolve (ec);
			ec.KillFlowBranching ();

			return ok;
		}
				
		/// <summary>
		///   Return value indicates whether all code paths emitted return.
		/// </summary>
		protected abstract void DoEmit (EmitContext ec);

		public virtual void Emit (EmitContext ec)
		{
			ec.Mark (loc);
			DoEmit (ec);
		}

		//
		// This routine must be overrided in derived classes and make copies
		// of all the data that might be modified if resolved
		// 
		protected abstract void CloneTo (CloneContext clonectx, Statement target);

		public Statement Clone (CloneContext clonectx)
		{
			Statement s = (Statement) this.MemberwiseClone ();
			CloneTo (clonectx, s);
			return s;
		}

		public virtual Expression CreateExpressionTree (ResolveContext ec)
		{
			ec.Report.Error (834, loc, "A lambda expression with statement body cannot be converted to an expresion tree");
			return null;
		}

		public Statement PerformClone ()
		{
			CloneContext clonectx = new CloneContext ();

			return Clone (clonectx);
		}
	}

	public sealed class EmptyStatement : Statement
	{
		public EmptyStatement (Location loc)
		{
			this.loc = loc;
		}
		
		public override bool Resolve (BlockContext ec)
		{
			return true;
		}

		public override bool ResolveUnreachable (BlockContext ec, bool warn)
		{
			return true;
		}

		public override void Emit (EmitContext ec)
		{
		}

		protected override void DoEmit (EmitContext ec)
		{
			throw new NotSupportedException ();
		}

		protected override void CloneTo (CloneContext clonectx, Statement target)
		{
			// nothing needed.
		}
	}
	
	public class If : Statement {
		Expression expr;
		public Statement TrueStatement;
		public Statement FalseStatement;

		bool is_true_ret;

		public If (Expression bool_expr, Statement true_statement, Location l)
			: this (bool_expr, true_statement, null, l)
		{
		}

		public If (Expression bool_expr,
			   Statement true_statement,
			   Statement false_statement,
			   Location l)
		{
			this.expr = bool_expr;
			TrueStatement = true_statement;
			FalseStatement = false_statement;
			loc = l;
		}

		public override bool Resolve (BlockContext ec)
		{
			bool ok = true;

			Report.Debug (1, "START IF BLOCK", loc);

			expr = expr.Resolve (ec);
			if (expr == null) {
				ok = false;
			} else {
				//
				// Dead code elimination
				//
				if (expr is Constant) {
					bool take = !((Constant) expr).IsDefaultValue;

					if (take) {
						if (!TrueStatement.Resolve (ec))
							return false;

						if ((FalseStatement != null) &&
							!FalseStatement.ResolveUnreachable (ec, true))
							return false;
						FalseStatement = null;
					} else {
						if (!TrueStatement.ResolveUnreachable (ec, true))
							return false;
						TrueStatement = null;

						if ((FalseStatement != null) &&
							!FalseStatement.Resolve (ec))
							return false;
					}

					return true;
				}
			}

			ec.StartFlowBranching (FlowBranching.BranchingType.Conditional, loc);
			
			ok &= TrueStatement.Resolve (ec);

			is_true_ret = ec.CurrentBranching.CurrentUsageVector.IsUnreachable;

			ec.CurrentBranching.CreateSibling ();

			if (FalseStatement != null)
				ok &= FalseStatement.Resolve (ec);
					
			ec.EndFlowBranching ();

			Report.Debug (1, "END IF BLOCK", loc);

			return ok;
		}
		
		protected override void DoEmit (EmitContext ec)
		{
			Label false_target = ec.DefineLabel ();
			Label end;

			//
			// If we're a boolean constant, Resolve() already
			// eliminated dead code for us.
			//
			Constant c = expr as Constant;
			if (c != null){
				c.EmitSideEffect (ec);

				if (!c.IsDefaultValue)
					TrueStatement.Emit (ec);
				else if (FalseStatement != null)
					FalseStatement.Emit (ec);

				return;
			}			
			
			expr.EmitBranchable (ec, false_target, false);
			
			TrueStatement.Emit (ec);

			if (FalseStatement != null){
				bool branch_emitted = false;
				
				end = ec.DefineLabel ();
				if (!is_true_ret){
					ec.Emit (OpCodes.Br, end);
					branch_emitted = true;
				}

				ec.MarkLabel (false_target);
				FalseStatement.Emit (ec);

				if (branch_emitted)
					ec.MarkLabel (end);
			} else {
				ec.MarkLabel (false_target);
			}
		}

		protected override void CloneTo (CloneContext clonectx, Statement t)
		{
			If target = (If) t;

			target.expr = expr.Clone (clonectx);
			target.TrueStatement = TrueStatement.Clone (clonectx);
			if (FalseStatement != null)
				target.FalseStatement = FalseStatement.Clone (clonectx);
		}
	}

	public class Do : Statement {
		public Expression expr;
		public Statement  EmbeddedStatement;

		public Do (Statement statement, BooleanExpression bool_expr, Location l)
		{
			expr = bool_expr;
			EmbeddedStatement = statement;
			loc = l;
		}

		public override bool Resolve (BlockContext ec)
		{
			bool ok = true;

			ec.StartFlowBranching (FlowBranching.BranchingType.Loop, loc);

			bool was_unreachable = ec.CurrentBranching.CurrentUsageVector.IsUnreachable;

			ec.StartFlowBranching (FlowBranching.BranchingType.Embedded, loc);
			if (!EmbeddedStatement.Resolve (ec))
				ok = false;
			ec.EndFlowBranching ();

			if (ec.CurrentBranching.CurrentUsageVector.IsUnreachable && !was_unreachable)
				ec.Report.Warning (162, 2, expr.Location, "Unreachable code detected");

			expr = expr.Resolve (ec);
			if (expr == null)
				ok = false;
			else if (expr is Constant){
				bool infinite = !((Constant) expr).IsDefaultValue;
				if (infinite)
					ec.CurrentBranching.CurrentUsageVector.Goto ();
			}

			ec.EndFlowBranching ();

			return ok;
		}
		
		protected override void DoEmit (EmitContext ec)
		{
			Label loop = ec.DefineLabel ();
			Label old_begin = ec.LoopBegin;
			Label old_end = ec.LoopEnd;
			
			ec.LoopBegin = ec.DefineLabel ();
			ec.LoopEnd = ec.DefineLabel ();
				
			ec.MarkLabel (loop);
			EmbeddedStatement.Emit (ec);
			ec.MarkLabel (ec.LoopBegin);

			//
			// Dead code elimination
			//
			if (expr is Constant){
				bool res = !((Constant) expr).IsDefaultValue;

				expr.EmitSideEffect (ec);
				if (res)
					ec.Emit (OpCodes.Br, loop); 
			} else
				expr.EmitBranchable (ec, loop, true);
			
			ec.MarkLabel (ec.LoopEnd);

			ec.LoopBegin = old_begin;
			ec.LoopEnd = old_end;
		}

		protected override void CloneTo (CloneContext clonectx, Statement t)
		{
			Do target = (Do) t;

			target.EmbeddedStatement = EmbeddedStatement.Clone (clonectx);
			target.expr = expr.Clone (clonectx);
		}
	}

	public class While : Statement {
		public Expression expr;
		public Statement Statement;
		bool infinite, empty;

		public While (BooleanExpression bool_expr, Statement statement, Location l)
		{
			this.expr = bool_expr;
			Statement = statement;
			loc = l;
		}

		public override bool Resolve (BlockContext ec)
		{
			bool ok = true;

			expr = expr.Resolve (ec);
			if (expr == null)
				ok = false;

			//
			// Inform whether we are infinite or not
			//
			if (expr is Constant){
				bool value = !((Constant) expr).IsDefaultValue;

				if (value == false){
					if (!Statement.ResolveUnreachable (ec, true))
						return false;
					empty = true;
					return true;
				} else
					infinite = true;
			}

			ec.StartFlowBranching (FlowBranching.BranchingType.Loop, loc);
			if (!infinite)
				ec.CurrentBranching.CreateSibling ();

			ec.StartFlowBranching (FlowBranching.BranchingType.Embedded, loc);
			if (!Statement.Resolve (ec))
				ok = false;
			ec.EndFlowBranching ();

			// There's no direct control flow from the end of the embedded statement to the end of the loop
			ec.CurrentBranching.CurrentUsageVector.Goto ();

			ec.EndFlowBranching ();

			return ok;
		}
		
		protected override void DoEmit (EmitContext ec)
		{
			if (empty) {
				expr.EmitSideEffect (ec);
				return;
			}

			Label old_begin = ec.LoopBegin;
			Label old_end = ec.LoopEnd;
			
			ec.LoopBegin = ec.DefineLabel ();
			ec.LoopEnd = ec.DefineLabel ();

			//
			// Inform whether we are infinite or not
			//
			if (expr is Constant){
				// expr is 'true', since the 'empty' case above handles the 'false' case
				ec.MarkLabel (ec.LoopBegin);
				expr.EmitSideEffect (ec);
				Statement.Emit (ec);
				ec.Emit (OpCodes.Br, ec.LoopBegin);
					
				//
				// Inform that we are infinite (ie, `we return'), only
				// if we do not `break' inside the code.
				//
				ec.MarkLabel (ec.LoopEnd);
			} else {
				Label while_loop = ec.DefineLabel ();

				ec.Emit (OpCodes.Br, ec.LoopBegin);
				ec.MarkLabel (while_loop);

				Statement.Emit (ec);
			
				ec.MarkLabel (ec.LoopBegin);
				ec.Mark (loc);

				expr.EmitBranchable (ec, while_loop, true);
				
				ec.MarkLabel (ec.LoopEnd);
			}	

			ec.LoopBegin = old_begin;
			ec.LoopEnd = old_end;
		}

		public override void Emit (EmitContext ec)
		{
			DoEmit (ec);
		}

		protected override void CloneTo (CloneContext clonectx, Statement t)
		{
			While target = (While) t;

			target.expr = expr.Clone (clonectx);
			target.Statement = Statement.Clone (clonectx);
		}
	}

	public class For : Statement {
		Expression Test;
		Statement InitStatement;
		Statement Increment;
		public Statement Statement;
		bool infinite, empty;
		
		public For (Statement init_statement,
			    BooleanExpression test,
			    Statement increment,
			    Statement statement,
			    Location l)
		{
			InitStatement = init_statement;
			Test = test;
			Increment = increment;
			Statement = statement;
			loc = l;
		}

		public override bool Resolve (BlockContext ec)
		{
			bool ok = true;

			if (InitStatement != null){
				if (!InitStatement.Resolve (ec))
					ok = false;
			}

			if (Test != null){
				Test = Test.Resolve (ec);
				if (Test == null)
					ok = false;
				else if (Test is Constant){
					bool value = !((Constant) Test).IsDefaultValue;

					if (value == false){
						if (!Statement.ResolveUnreachable (ec, true))
							return false;
						if ((Increment != null) &&
						    !Increment.ResolveUnreachable (ec, false))
							return false;
						empty = true;
						return true;
					} else
						infinite = true;
				}
			} else
				infinite = true;

			ec.StartFlowBranching (FlowBranching.BranchingType.Loop, loc);
			if (!infinite)
				ec.CurrentBranching.CreateSibling ();

			bool was_unreachable = ec.CurrentBranching.CurrentUsageVector.IsUnreachable;

			ec.StartFlowBranching (FlowBranching.BranchingType.Embedded, loc);
			if (!Statement.Resolve (ec))
				ok = false;
			ec.EndFlowBranching ();

			if (Increment != null){
				if (ec.CurrentBranching.CurrentUsageVector.IsUnreachable) {
					if (!Increment.ResolveUnreachable (ec, !was_unreachable))
						ok = false;
				} else {
					if (!Increment.Resolve (ec))
						ok = false;
				}
			}

			// There's no direct control flow from the end of the embedded statement to the end of the loop
			ec.CurrentBranching.CurrentUsageVector.Goto ();

			ec.EndFlowBranching ();

			return ok;
		}

		protected override void DoEmit (EmitContext ec)
		{
			if (InitStatement != null)
				InitStatement.Emit (ec);

			if (empty) {
				Test.EmitSideEffect (ec);
				return;
			}

			Label old_begin = ec.LoopBegin;
			Label old_end = ec.LoopEnd;
			Label loop = ec.DefineLabel ();
			Label test = ec.DefineLabel ();

			ec.LoopBegin = ec.DefineLabel ();
			ec.LoopEnd = ec.DefineLabel ();

			ec.Emit (OpCodes.Br, test);
			ec.MarkLabel (loop);
			Statement.Emit (ec);

			ec.MarkLabel (ec.LoopBegin);
			Increment.Emit (ec);

			ec.MarkLabel (test);
			//
			// If test is null, there is no test, and we are just
			// an infinite loop
			//
			if (Test != null){
				//
				// The Resolve code already catches the case for
				// Test == Constant (false) so we know that
				// this is true
				//
				if (Test is Constant) {
					Test.EmitSideEffect (ec);
					ec.Emit (OpCodes.Br, loop);
				} else {
					Test.EmitBranchable (ec, loop, true);
				}
				
			} else
				ec.Emit (OpCodes.Br, loop);
			ec.MarkLabel (ec.LoopEnd);

			ec.LoopBegin = old_begin;
			ec.LoopEnd = old_end;
		}

		protected override void CloneTo (CloneContext clonectx, Statement t)
		{
			For target = (For) t;

			if (InitStatement != null)
				target.InitStatement = InitStatement.Clone (clonectx);
			if (Test != null)
				target.Test = Test.Clone (clonectx);
			if (Increment != null)
				target.Increment = Increment.Clone (clonectx);
			target.Statement = Statement.Clone (clonectx);
		}
	}
	
	public class StatementExpression : Statement {
		ExpressionStatement expr;
		
		public StatementExpression (ExpressionStatement expr)
		{
			this.expr = expr;
			loc = expr.Location;
		}

		public override bool Resolve (BlockContext ec)
		{
			expr = expr.ResolveStatement (ec);
			return expr != null;
		}
		
		protected override void DoEmit (EmitContext ec)
		{
			expr.EmitStatement (ec);
		}

		public override string ToString ()
		{
			return "StatementExpression (" + expr + ")";
		}

		protected override void CloneTo (CloneContext clonectx, Statement t)
		{
			StatementExpression target = (StatementExpression) t;

			target.expr = (ExpressionStatement) expr.Clone (clonectx);
		}
	}

	//
	// Simple version of statement list not requiring a block
	//
	public class StatementList : Statement
	{
		List<Statement> statements;

		public StatementList (Statement first, Statement second)
		{
			statements = new List<Statement> () { first, second };
		}

		#region Properties
		public IList<Statement> Statements {
			get {
				return statements;
			}
		}
		#endregion

		public void Add (Statement statement)
		{
			statements.Add (statement);
		}

		public override bool Resolve (BlockContext ec)
		{
			foreach (var s in statements)
				s.Resolve (ec);

			return true;
		}

		protected override void DoEmit (EmitContext ec)
		{
			foreach (var s in statements)
				s.Emit (ec);
		}

		protected override void CloneTo (CloneContext clonectx, Statement target)
		{
			StatementList t = (StatementList) target;

			t.statements = new List<Statement> (statements.Count);
			foreach (Statement s in statements)
				t.statements.Add (s.Clone (clonectx));
		}
	}

	// A 'return' or a 'yield break'
	public abstract class ExitStatement : Statement
	{
		protected bool unwind_protect;
		protected abstract bool DoResolve (BlockContext ec);

		public virtual void Error_FinallyClause (Report Report)
		{
			Report.Error (157, loc, "Control cannot leave the body of a finally clause");
		}

		public sealed override bool Resolve (BlockContext ec)
		{
			if (!DoResolve (ec))
				return false;

			unwind_protect = ec.CurrentBranching.AddReturnOrigin (ec.CurrentBranching.CurrentUsageVector, this);
			if (unwind_protect)
				ec.NeedReturnLabel ();
			ec.CurrentBranching.CurrentUsageVector.Goto ();
			return true;
		}
	}

	/// <summary>
	///   Implements the return statement
	/// </summary>
	public class Return : ExitStatement
	{
		protected Expression Expr;
		public Return (Expression expr, Location l)
		{
			Expr = expr;
			loc = l;
		}

		#region Properties
		public Expression Expression {
			get {
				return Expr;
			}
		}
		#endregion

		protected override bool DoResolve (BlockContext ec)
		{
			if (Expr == null) {
				if (ec.ReturnType == TypeManager.void_type)
					return true;

				if (ec.CurrentIterator != null) {
					Error_ReturnFromIterator (ec);
				} else {
					ec.Report.Error (126, loc,
						"An object of a type convertible to `{0}' is required for the return statement",
						ec.ReturnType.GetSignatureForError ());
				}

				return false;
			}

			Expr = Expr.Resolve (ec);

			AnonymousExpression am = ec.CurrentAnonymousMethod;
			if (am == null) {
				if (ec.ReturnType == TypeManager.void_type) {
					ec.Report.Error (127, loc,
						"`{0}': A return keyword must not be followed by any expression when method returns void",
						ec.GetSignatureForError ());
				}
			} else {
				if (am.IsIterator) {
					Error_ReturnFromIterator (ec);
					return false;
				}

				var l = am as AnonymousMethodBody;
				if (l != null && l.ReturnTypeInference != null && Expr != null) {
					l.ReturnTypeInference.AddCommonTypeBound (Expr.Type);
					return true;
				}
			}

			if (Expr == null)
				return false;

			if (Expr.Type != ec.ReturnType) {
				Expr = Convert.ImplicitConversionRequired (ec, Expr, ec.ReturnType, loc);

				if (Expr == null) {
					if (am != null) {
						ec.Report.Error (1662, loc,
							"Cannot convert `{0}' to delegate type `{1}' because some of the return types in the block are not implicitly convertible to the delegate return type",
							am.ContainerType, am.GetSignatureForError ());
					}
					return false;
				}
			}

			return true;			
		}
		
		protected override void DoEmit (EmitContext ec)
		{
			if (Expr != null) {
				Expr.Emit (ec);

				if (unwind_protect)
					ec.Emit (OpCodes.Stloc, ec.TemporaryReturn ());
			}

			if (unwind_protect)
				ec.Emit (OpCodes.Leave, ec.ReturnLabel);
			else
				ec.Emit (OpCodes.Ret);
		}

		void Error_ReturnFromIterator (ResolveContext rc)
		{
			rc.Report.Error (1622, loc,
				"Cannot return a value from iterators. Use the yield return statement to return a value, or yield break to end the iteration");
		}

		protected override void CloneTo (CloneContext clonectx, Statement t)
		{
			Return target = (Return) t;
			// It's null for simple return;
			if (Expr != null)
				target.Expr = Expr.Clone (clonectx);
		}
	}

	public class Goto : Statement {
		string target;
		LabeledStatement label;
		bool unwind_protect;

		public override bool Resolve (BlockContext ec)
		{
			unwind_protect = ec.CurrentBranching.AddGotoOrigin (ec.CurrentBranching.CurrentUsageVector, this);
			ec.CurrentBranching.CurrentUsageVector.Goto ();
			return true;
		}
		
		public Goto (string label, Location l)
		{
			loc = l;
			target = label;
		}

		public string Target {
			get { return target; }
		}

		public void SetResolvedTarget (LabeledStatement label)
		{
			this.label = label;
			label.AddReference ();
		}

		protected override void CloneTo (CloneContext clonectx, Statement target)
		{
			// Nothing to clone
		}

		protected override void DoEmit (EmitContext ec)
		{
			if (label == null)
				throw new InternalErrorException ("goto emitted before target resolved");
			Label l = label.LabelTarget (ec);
			ec.Emit (unwind_protect ? OpCodes.Leave : OpCodes.Br, l);
		}
	}

	public class LabeledStatement : Statement {
		string name;
		bool defined;
		bool referenced;
		Label label;
		Block block;

		FlowBranching.UsageVector vectors;
		
		public LabeledStatement (string name, Block block, Location l)
		{
			this.name = name;
			this.block = block;
			this.loc = l;
		}

		public Label LabelTarget (EmitContext ec)
		{
			if (defined)
				return label;

			label = ec.DefineLabel ();
			defined = true;
			return label;
		}

		public Block Block {
			get {
				return block;
			}
		}

		public string Name {
			get { return name; }
		}

		public bool IsDefined {
			get { return defined; }
		}

		public bool HasBeenReferenced {
			get { return referenced; }
		}

		public FlowBranching.UsageVector JumpOrigins {
			get { return vectors; }
		}

		public void AddUsageVector (FlowBranching.UsageVector vector)
		{
			vector = vector.Clone ();
			vector.Next = vectors;
			vectors = vector;
		}

		protected override void CloneTo (CloneContext clonectx, Statement target)
		{
			// nothing to clone
		}

		public override bool Resolve (BlockContext ec)
		{
			// this flow-branching will be terminated when the surrounding block ends
			ec.StartFlowBranching (this);
			return true;
		}

		protected override void DoEmit (EmitContext ec)
		{
			if (!HasBeenReferenced)
				ec.Report.Warning (164, 2, loc, "This label has not been referenced");

			LabelTarget (ec);
			ec.MarkLabel (label);
		}

		public void AddReference ()
		{
			referenced = true;
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

		protected override void CloneTo (CloneContext clonectx, Statement target)
		{
			// nothing to clone
		}

		public override bool Resolve (BlockContext ec)
		{
			ec.CurrentBranching.CurrentUsageVector.Goto ();

			if (ec.Switch == null) {
				ec.Report.Error (153, loc, "A goto case is only valid inside a switch statement");
				return false;
			}

			if (!ec.Switch.GotDefault) {
				FlowBranchingBlock.Error_UnknownLabel (loc, "default", ec.Report);
				return false;
			}

			return true;
		}

		protected override void DoEmit (EmitContext ec)
		{
			ec.Emit (OpCodes.Br, ec.Switch.DefaultTarget);
		}
	}

	/// <summary>
	///   `goto case' statement
	/// </summary>
	public class GotoCase : Statement {
		Expression expr;
		SwitchLabel sl;
		
		public GotoCase (Expression e, Location l)
		{
			expr = e;
			loc = l;
		}

		public override bool Resolve (BlockContext ec)
		{
			if (ec.Switch == null){
				ec.Report.Error (153, loc, "A goto case is only valid inside a switch statement");
				return false;
			}

			ec.CurrentBranching.CurrentUsageVector.Goto ();

			expr = expr.Resolve (ec);
			if (expr == null)
				return false;

			Constant c = expr as Constant;
			if (c == null) {
				ec.Report.Error (150, expr.Location, "A constant value is expected");
				return false;
			}

			TypeSpec type = ec.Switch.SwitchType;
			Constant res = c.TryReduce (ec, type, c.Location);
			if (res == null) {
				c.Error_ValueCannotBeConverted (ec, loc, type, true);
				return false;
			}

			if (!Convert.ImplicitStandardConversionExists (c, type))
				ec.Report.Warning (469, 2, loc,
					"The `goto case' value is not implicitly convertible to type `{0}'",
					TypeManager.CSharpName (type));

			object val = res.GetValue ();
			if (val == null)
				val = SwitchLabel.NullStringCase;
					
			if (!ec.Switch.Elements.TryGetValue (val, out sl)) {
				FlowBranchingBlock.Error_UnknownLabel (loc, "case " + 
					(c.GetValue () == null ? "null" : val.ToString ()), ec.Report);
				return false;
			}

			return true;
		}

		protected override void DoEmit (EmitContext ec)
		{
			ec.Emit (OpCodes.Br, sl.GetILLabelCode (ec));
		}

		protected override void CloneTo (CloneContext clonectx, Statement t)
		{
			GotoCase target = (GotoCase) t;

			target.expr = expr.Clone (clonectx);
		}
	}
	
	public class Throw : Statement {
		Expression expr;
		
		public Throw (Expression expr, Location l)
		{
			this.expr = expr;
			loc = l;
		}

		public override bool Resolve (BlockContext ec)
		{
			if (expr == null) {
				ec.CurrentBranching.CurrentUsageVector.Goto ();
				return ec.CurrentBranching.CheckRethrow (loc);
			}

			expr = expr.Resolve (ec, ResolveFlags.Type | ResolveFlags.VariableOrValue);
			ec.CurrentBranching.CurrentUsageVector.Goto ();

			if (expr == null)
				return false;

			if (Convert.ImplicitConversionExists (ec, expr, TypeManager.exception_type))
				expr = Convert.ImplicitConversion (ec, expr, TypeManager.exception_type, loc);
			else
				ec.Report.Error (155, expr.Location, "The type caught or thrown must be derived from System.Exception");

			return true;
		}
			
		protected override void DoEmit (EmitContext ec)
		{
			if (expr == null)
				ec.Emit (OpCodes.Rethrow);
			else {
				expr.Emit (ec);

				ec.Emit (OpCodes.Throw);
			}
		}

		protected override void CloneTo (CloneContext clonectx, Statement t)
		{
			Throw target = (Throw) t;

			if (expr != null)
				target.expr = expr.Clone (clonectx);
		}
	}

	public class Break : Statement {
		
		public Break (Location l)
		{
			loc = l;
		}

		bool unwind_protect;

		public override bool Resolve (BlockContext ec)
		{
			unwind_protect = ec.CurrentBranching.AddBreakOrigin (ec.CurrentBranching.CurrentUsageVector, loc);
			ec.CurrentBranching.CurrentUsageVector.Goto ();
			return true;
		}

		protected override void DoEmit (EmitContext ec)
		{
			ec.Emit (unwind_protect ? OpCodes.Leave : OpCodes.Br, ec.LoopEnd);
		}

		protected override void CloneTo (CloneContext clonectx, Statement t)
		{
			// nothing needed
		}
	}

	public class Continue : Statement {
		
		public Continue (Location l)
		{
			loc = l;
		}

		bool unwind_protect;

		public override bool Resolve (BlockContext ec)
		{
			unwind_protect = ec.CurrentBranching.AddContinueOrigin (ec.CurrentBranching.CurrentUsageVector, loc);
			ec.CurrentBranching.CurrentUsageVector.Goto ();
			return true;
		}

		protected override void DoEmit (EmitContext ec)
		{
			ec.Emit (unwind_protect ? OpCodes.Leave : OpCodes.Br, ec.LoopBegin);
		}

		protected override void CloneTo (CloneContext clonectx, Statement t)
		{
			// nothing needed.
		}
	}

	public interface ILocalVariable
	{
		void Emit (EmitContext ec);
		void EmitAssign (EmitContext ec);
		void EmitAddressOf (EmitContext ec);
	}

	public interface INamedBlockVariable
	{
		Block Block { get; }
		Expression CreateReferenceExpression (ResolveContext rc, Location loc);
		bool IsDeclared { get; }
		Location Location { get; }
	}

	public class BlockVariableDeclaration : Statement
	{
		public class Declarator
		{
			LocalVariable li;
			Expression initializer;

			public Declarator (LocalVariable li, Expression initializer)
			{
				if (li.Type != null)
					throw new ArgumentException ("Expected null variable type");

				this.li = li;
				this.initializer = initializer;
			}

			public Declarator (Declarator clone, Expression initializer)
			{
				this.li = clone.li;
				this.initializer = initializer;
			}

			#region Properties

			public LocalVariable Variable {
				get {
					return li;
				}
			}

			public Expression Initializer {
				get {
					return initializer;
				}
				set {
					initializer = value;
				}
			}

			#endregion
		}

		Expression initializer;
		protected FullNamedExpression type_expr;
		protected LocalVariable li;
		protected List<Declarator> declarators;

		public BlockVariableDeclaration (FullNamedExpression type, LocalVariable li)
		{
			this.type_expr = type;
			this.li = li;
			this.loc = type_expr.Location;
		}

		protected BlockVariableDeclaration (LocalVariable li)
		{
			this.li = li;
		}

		#region Properties

		public List<Declarator> Declarators {
			get {
				return declarators;
			}
		}

		public Expression Initializer {
			get {
				return initializer;
			}
			set {
				initializer = value;
			}
		}

		public FullNamedExpression TypeExpression {
			get {
				return type_expr;
			}
		}

		public LocalVariable Variable {
			get {
				return li;
			}
		}

		#endregion

		public void AddDeclarator (Declarator decl)
		{
			if (declarators == null)
				declarators = new List<Declarator> ();

			declarators.Add (decl);
		}

		void CreateEvaluatorVariable (BlockContext bc, LocalVariable li)
		{
			var container = bc.CurrentMemberDefinition.Parent;

			Field f = new Field (container, new TypeExpression (li.Type, li.Location), Modifiers.PUBLIC | Modifiers.STATIC,
				new MemberName (li.Name, li.Location), null);

			container.AddField (f);
			f.Define ();
			Evaluator.QueueField (f);

			li.HoistedVariant = new HoistedEvaluatorVariable (f);
			li.SetIsUsed ();
		}

		public override bool Resolve (BlockContext bc)
		{
			if (li.Type == null) {
				TypeSpec type = null;
				if (type_expr is VarExpr) {
					//
					// C# 3.0 introduced contextual keywords (var) which behaves like a type if type with
					// same name exists or as a keyword when no type was found
					// 
					var texpr = type_expr.ResolveAsTypeTerminal (bc, true);
					if (texpr == null) {
						if (RootContext.Version < LanguageVersion.V_3)
							bc.Report.FeatureIsNotAvailable (loc, "implicitly typed local variable");

						if (li.IsFixed) {
							bc.Report.Error (821, loc, "A fixed statement cannot use an implicitly typed local variable");
							return false;
						}

						if (li.IsConstant) {
							bc.Report.Error (822, loc, "An implicitly typed local variable cannot be a constant");
							return false;
						}

						if (Initializer == null) {
							bc.Report.Error (818, loc, "An implicitly typed local variable declarator must include an initializer");
							return false;
						}

						if (declarators != null) {
							bc.Report.Error (819, loc, "An implicitly typed local variable declaration cannot include multiple declarators");
							declarators = null;
						}

						Initializer = Initializer.Resolve (bc);
						if (Initializer != null) {
							((VarExpr) type_expr).InferType (bc, Initializer);
							type = type_expr.Type;
						}
					}
				}

				if (type == null) {
					var texpr = type_expr.ResolveAsTypeTerminal (bc, false);
					if (texpr == null)
						return false;

					type = texpr.Type;

					if (li.IsConstant && !type.IsConstantCompatible) {
						Const.Error_InvalidConstantType (type, loc, bc.Report);
					}
				}

				if (type.IsStatic)
					FieldBase.Error_VariableOfStaticClass (loc, li.Name, type, bc.Report);

				if (type.IsPointer && !bc.IsUnsafe)
					Expression.UnsafeError (bc, loc);

				li.Type = type;
			}

			bool eval_global = RootContext.StatementMode && bc.CurrentBlock is ToplevelBlock;
			if (eval_global) {
				CreateEvaluatorVariable (bc, li);
			} else {
				li.PrepareForFlowAnalysis (bc);
			}

			if (initializer != null) {
				initializer = ResolveInitializer (bc, li, initializer);
				// li.Variable.DefinitelyAssigned 
			}

			if (declarators != null) {
				foreach (var d in declarators) {
					d.Variable.Type = li.Type;
					if (eval_global) {
						CreateEvaluatorVariable (bc, d.Variable);
					} else {
						d.Variable.PrepareForFlowAnalysis (bc);
					}

					if (d.Initializer != null) {
						d.Initializer = ResolveInitializer (bc, d.Variable, d.Initializer);
						// d.Variable.DefinitelyAssigned 
					}
				}
			}

			return true;
		}

		protected virtual Expression ResolveInitializer (BlockContext bc, LocalVariable li, Expression initializer)
		{
			var a = new SimpleAssign (li.CreateReferenceExpression (bc, li.Location), initializer, li.Location);
			return a.ResolveStatement (bc);
		}

		protected override void DoEmit (EmitContext ec)
		{
			if (li.IsConstant)
				return;

			li.CreateBuilder (ec);

			if (Initializer != null)
				((ExpressionStatement) Initializer).EmitStatement (ec);

			if (declarators != null) {
				foreach (var d in declarators) {
					d.Variable.CreateBuilder (ec);
					if (d.Initializer != null)
						((ExpressionStatement) d.Initializer).EmitStatement (ec);
				}
			}
		}

		protected override void CloneTo (CloneContext clonectx, Statement target)
		{
			BlockVariableDeclaration t = (BlockVariableDeclaration) target;

			if (type_expr != null)
				t.type_expr = (FullNamedExpression) type_expr.Clone (clonectx);

			if (initializer != null)
				t.initializer = initializer.Clone (clonectx);

			if (declarators != null) {
				t.declarators = null;
				foreach (var d in declarators)
					t.AddDeclarator (new Declarator (d, d.Initializer == null ? null : d.Initializer.Clone (clonectx)));
			}
		}
	}

	class BlockConstantDeclaration : BlockVariableDeclaration
	{
		public BlockConstantDeclaration (FullNamedExpression type, LocalVariable li)
			: base (type, li)
		{
		}

		protected override Expression ResolveInitializer (BlockContext bc, LocalVariable li, Expression initializer)
		{
			initializer = initializer.Resolve (bc);
			if (initializer == null)
				return null;

			var c = initializer as Constant;
			if (c == null) {
				initializer.Error_ExpressionMustBeConstant (bc, initializer.Location, li.Name);
				return null;
			}

			c = c.ConvertImplicitly (bc, li.Type);
			if (c == null) {
				if (TypeManager.IsReferenceType (li.Type))
					initializer.Error_ConstantCanBeInitializedWithNullOnly (bc, li.Type, initializer.Location, li.Name);
				else
					initializer.Error_ValueCannotBeConverted (bc, initializer.Location, li.Type, false);

				return null;
			}

			li.ConstantValue = c;
			return initializer;
		}
	}

	//
	// The information about a user-perceived local variable
	//
	public class LocalVariable : INamedBlockVariable, ILocalVariable
	{
		[Flags]
		public enum Flags
		{
			Used = 1,
			IsThis = 1 << 1,
			AddressTaken = 1 << 2,
			CompilerGenerated = 1 << 3,
			Constant = 1 << 4,
			ForeachVariable = 1 << 5,
			FixedVariable = 1 << 6,
			UsingVariable = 1 << 7,
//			DefinitelyAssigned = 1 << 8,
			IsLocked = 1 << 9,

			ReadonlyMask = ForeachVariable | FixedVariable | UsingVariable
		}

		TypeSpec type;
		readonly string name;
		readonly Location loc;
		readonly Block block;
		Flags flags;
		Constant const_value;

		public VariableInfo VariableInfo;
		HoistedVariable hoisted_variant;

		LocalBuilder builder;

		public LocalVariable (Block block, string name, Location loc)
		{
			this.block = block;
			this.name = name;
			this.loc = loc;
		}

		public LocalVariable (Block block, string name, Flags flags, Location loc)
			: this (block, name, loc)
		{
			this.flags = flags;
		}

		//
		// Used by variable declarators
		//
		public LocalVariable (LocalVariable li, string name, Location loc)
			: this (li.block, name, li.flags, loc)
		{
		}

		#region Properties

		public bool AddressTaken {
			get { return (flags & Flags.AddressTaken) != 0; }
			set { flags |= Flags.AddressTaken; }
		}

		public Block Block {
			get {
				return block;
			}
		}

		public Constant ConstantValue {
			get {
				return const_value;
			}
			set {
				const_value = value;
			}
		}

		//
		// Hoisted local variable variant
		//
		public HoistedVariable HoistedVariant {
			get {
				return hoisted_variant;
			}
			set {
				hoisted_variant = value;
			}
		}

		public bool IsDeclared {
			get {
				return type != null;
			}
		}

		public bool IsConstant {
			get {
				return (flags & Flags.Constant) != 0;
			}
		}

		public bool IsLocked {
			get {
				return (flags & Flags.IsLocked) != 0;
			}
			set {
				flags = value ? flags | Flags.IsLocked : flags & ~Flags.IsLocked;
			}
		}

		public bool IsThis {
			get {
				return (flags & Flags.IsThis) != 0;
			}
		}

		public bool IsFixed {
			get {
				return (flags & Flags.FixedVariable) != 0;
			}
		}

		public bool IsReadonly {
			get {
				return (flags & Flags.ReadonlyMask) != 0;
			}
		}

		public Location Location {
			get {
				return loc;
			}
		}

		public string Name {
			get {
				return name;
			}
		}

		public TypeSpec Type {
		    get {
				return type;
			}
		    set {
				type = value;
			}
		}

		#endregion

		public void CreateBuilder (EmitContext ec)
		{
			if ((flags & Flags.Used) == 0) {
				if (VariableInfo == null) {
					// Missing flow analysis or wrong variable flags
					throw new InternalErrorException ("VariableInfo is null and the variable `{0}' is not used", name);
				}

				if (VariableInfo.IsEverAssigned)
					ec.Report.Warning (219, 3, Location, "The variable `{0}' is assigned but its value is never used", Name);
				else
					ec.Report.Warning (168, 3, Location, "The variable `{0}' is declared but never used", Name);
			}

			if (HoistedVariant != null)
				return;

			if (builder != null) {
				if ((flags & Flags.CompilerGenerated) != 0)
					return;

				// To avoid Used warning duplicates
				throw new InternalErrorException ("Already created variable `{0}'", name);
			}

			//
			// All fixed variabled are pinned, a slot has to be alocated
			//
			builder = ec.DeclareLocal (Type, IsFixed);
			if (SymbolWriter.HasSymbolWriter)
				ec.DefineLocalVariable (name, builder);
		}

		public static LocalVariable CreateCompilerGenerated (TypeSpec type, Block block, Location loc)
		{
			LocalVariable li = new LocalVariable (block, "<$$>", Flags.CompilerGenerated | Flags.Used, loc);
			li.Type = type;
			return li;
		}

		public Expression CreateReferenceExpression (ResolveContext rc, Location loc)
		{
			if (IsConstant && const_value != null)
				return Constant.CreateConstantFromValue (Type, const_value.GetValue (), loc).Resolve (rc);

			return new LocalVariableReference (this, loc);
		}

		public void Emit (EmitContext ec)
		{
			// TODO: Need something better for temporary variables
			if ((flags & Flags.CompilerGenerated) != 0)
				CreateBuilder (ec);

			ec.Emit (OpCodes.Ldloc, builder);
		}

		public void EmitAssign (EmitContext ec)
		{
			// TODO: Need something better for temporary variables
			if ((flags & Flags.CompilerGenerated) != 0)
				CreateBuilder (ec);

			ec.Emit (OpCodes.Stloc, builder);
		}

		public void EmitAddressOf (EmitContext ec)
		{
			ec.Emit (OpCodes.Ldloca, builder);
		}

		public string GetReadOnlyContext ()
		{
			switch (flags & Flags.ReadonlyMask) {
			case Flags.FixedVariable:
				return "fixed variable";
			case Flags.ForeachVariable:
				return "foreach iteration variable";
			case Flags.UsingVariable:
				return "using variable";
			}

			throw new InternalErrorException ("Variable is not readonly");
		}

		public bool IsThisAssigned (BlockContext ec, Block block)
		{
			if (VariableInfo == null)
				throw new Exception ();

			if (!ec.DoFlowAnalysis || ec.CurrentBranching.IsAssigned (VariableInfo))
				return true;

			return VariableInfo.TypeInfo.IsFullyInitialized (ec, VariableInfo, block.StartLocation);
		}

		public bool IsAssigned (BlockContext ec)
		{
			if (VariableInfo == null)
				throw new Exception ();

			return !ec.DoFlowAnalysis || ec.CurrentBranching.IsAssigned (VariableInfo);
		}

		public void PrepareForFlowAnalysis (BlockContext bc)
		{
			//
			// No need for definitely assigned check for these guys
			//
			if ((flags & (Flags.Constant | Flags.ReadonlyMask | Flags.CompilerGenerated)) != 0)
				return;

			VariableInfo = new VariableInfo (this, bc.FlowOffset);
			bc.FlowOffset += VariableInfo.Length;
		}

		//
		// Mark the variables as referenced in the user code
		//
		public void SetIsUsed ()
		{
			flags |= Flags.Used;
		}

		public override string ToString ()
		{
			return string.Format ("LocalInfo ({0},{1},{2},{3})", name, type, VariableInfo, Location);
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
	///
	///   Top-level blocks derive from Block, and they are called ToplevelBlock
	///   they contain extra information that is not necessary on normal blocks.
	/// </remarks>
	public class Block : Statement {
		[Flags]
		public enum Flags
		{
			Unchecked = 1,
			HasRet = 8,
			Unsafe = 16,
			IsIterator = 32,
			HasCapturedVariable = 64,
			HasCapturedThis = 1 << 7,
			IsExpressionTree = 1 << 8,
			CompilerGenerated = 1 << 9
		}

		public Block Parent;
		public Location StartLocation;
		public Location EndLocation;

		public ExplicitBlock Explicit;
		public ParametersBlock ParametersBlock;

		protected Flags flags;

		//
		// The statements in this block
		//
		protected List<Statement> statements;

		protected List<Statement> scope_initializers;

		int? resolving_init_idx;

		Block original;

#if DEBUG
		static int id;
		public int ID = id++;

		static int clone_id_counter;
		int clone_id;
#endif

//		int assignable_slots;
		bool unreachable_shown;
		bool unreachable;
		
		public Block (Block parent, Location start, Location end)
			: this (parent, 0, start, end)
		{
		}

		public Block (Block parent, Flags flags, Location start, Location end)
		{
			if (parent != null) {
				// the appropriate constructors will fixup these fields
				ParametersBlock = parent.ParametersBlock;
				Explicit = parent.Explicit;
			}
			
			this.Parent = parent;
			this.flags = flags;
			this.StartLocation = start;
			this.EndLocation = end;
			this.loc = start;
			statements = new List<Statement> (4);

			this.original = this;
		}

		#region Properties

		public bool HasRet {
			get { return (flags & Flags.HasRet) != 0; }
		}

		public Block Original {
			get {
				return original;
			}
		}

		public bool IsCompilerGenerated {
			get { return (flags & Flags.CompilerGenerated) != 0; }
			set { flags = value ? flags | Flags.CompilerGenerated : flags & ~Flags.CompilerGenerated; }
		}

		public bool Unchecked {
			get { return (flags & Flags.Unchecked) != 0; }
			set { flags = value ? flags | Flags.Unchecked : flags & ~Flags.Unchecked; }
		}

		public bool Unsafe {
			get { return (flags & Flags.Unsafe) != 0; }
			set { flags |= Flags.Unsafe; }
		}

		#endregion

		public Block CreateSwitchBlock (Location start)
		{
			// FIXME: Only explicit block should be created
			var new_block = new Block (this, start, start);
			new_block.IsCompilerGenerated = true;
			return new_block;
		}

		public void SetEndLocation (Location loc)
		{
			EndLocation = loc;
		}

		public void AddLabel (LabeledStatement target)
		{
			ParametersBlock.TopBlock.AddLabel (target.Name, target);
		}

		public void AddLocalName (LocalVariable li)
		{
			AddLocalName (li.Name, li);
		}

		public virtual void AddLocalName (string name, INamedBlockVariable li)
		{
			ParametersBlock.TopBlock.AddLocalName (name, li);
		}

		public virtual void Error_AlreadyDeclared (string name, INamedBlockVariable variable, string reason)
		{
			if (reason == null) {
				Error_AlreadyDeclared (name, variable);
				return;
			}

			ParametersBlock.TopBlock.Report.Error (136, variable.Location,
				"A local variable named `{0}' cannot be declared in this scope because it would give a different meaning " +
				"to `{0}', which is already used in a `{1}' scope to denote something else",
				name, reason);
		}

		public virtual void Error_AlreadyDeclared (string name, INamedBlockVariable variable)
		{
			var pi = variable as ParametersBlock.ParameterInfo;
			if (pi != null) {
				var p = pi.Parameter;
				if (p is AnonymousTypeClass.GeneratedParameter) {
					ParametersBlock.TopBlock.Report.Error (833, p.Location, "`{0}': An anonymous type cannot have multiple properties with the same name",
						p.Name);
				} else {
					ParametersBlock.TopBlock.Report.Error (100, p.Location, "The parameter name `{0}' is a duplicate", p.Name);
				}

				return;
			}

			ParametersBlock.TopBlock.Report.Error (128, variable.Location,
				"A local variable named `{0}' is already defined in this scope", name);
		}
					
		public virtual void Error_AlreadyDeclaredTypeParameter (string name, Location loc)
		{
			ParametersBlock.TopBlock.Report.Error (412, loc,
				"The type parameter name `{0}' is the same as local variable or parameter name",
				name);
		}

		//
		// It should be used by expressions which require to
		// register a statement during resolve process.
		//
		public void AddScopeStatement (Statement s)
		{
			if (scope_initializers == null)
				scope_initializers = new List<Statement> ();

			//
			// Simple recursive helper, when resolve scope initializer another
			// new scope initializer can be added, this ensures it's initialized
			// before existing one. For now this can happen with expression trees
			// in base ctor initializer only
			//
			if (resolving_init_idx.HasValue) {
				scope_initializers.Insert (resolving_init_idx.Value, s);
				++resolving_init_idx;
			} else {
				scope_initializers.Add (s);
			}
		}
		
		public void AddStatement (Statement s)
		{
			statements.Add (s);
		}

		public int AssignableSlots {
			get {
				// FIXME: HACK, we don't know the block available variables count now, so set this high enough
				return 4096;
//				return assignable_slots;
			}
		}

		public LabeledStatement LookupLabel (string name)
		{
			return ParametersBlock.TopBlock.GetLabel (name, this);
		}

		public override bool Resolve (BlockContext ec)
		{
			Block prev_block = ec.CurrentBlock;
			bool ok = true;

			ec.CurrentBlock = this;
			ec.StartFlowBranching (this);

			Report.Debug (4, "RESOLVE BLOCK", StartLocation, ec.CurrentBranching);

			//
			// Compiler generated scope statements
			//
			if (scope_initializers != null) {
				for (resolving_init_idx = 0; resolving_init_idx < scope_initializers.Count; ++resolving_init_idx) {
					scope_initializers[resolving_init_idx.Value].Resolve (ec);
				}

				resolving_init_idx = null;
			}

			//
			// This flag is used to notate nested statements as unreachable from the beginning of this block.
			// For the purposes of this resolution, it doesn't matter that the whole block is unreachable 
			// from the beginning of the function.  The outer Resolve() that detected the unreachability is
			// responsible for handling the situation.
			//
			int statement_count = statements.Count;
			for (int ix = 0; ix < statement_count; ix++){
				Statement s = statements [ix];

				//
				// Warn if we detect unreachable code.
				//
				if (unreachable) {
					if (s is EmptyStatement)
						continue;

					if (!unreachable_shown && !(s is LabeledStatement)) {
						ec.Report.Warning (162, 2, s.loc, "Unreachable code detected");
						unreachable_shown = true;
					}

					Block c_block = s as Block;
					if (c_block != null)
						c_block.unreachable = c_block.unreachable_shown = true;
				}

				//
				// Note that we're not using ResolveUnreachable() for unreachable
				// statements here.  ResolveUnreachable() creates a temporary
				// flow branching and kills it afterwards.  This leads to problems
				// if you have two unreachable statements where the first one
				// assigns a variable and the second one tries to access it.
				//

				if (!s.Resolve (ec)) {
					ok = false;
					if (ec.IsInProbingMode)
						break;

					statements [ix] = new EmptyStatement (s.loc);
					continue;
				}

				if (unreachable && !(s is LabeledStatement) && !(s is Block))
					statements [ix] = new EmptyStatement (s.loc);

				unreachable = ec.CurrentBranching.CurrentUsageVector.IsUnreachable;
				if (unreachable && s is LabeledStatement)
					throw new InternalErrorException ("should not happen");
			}

			Report.Debug (4, "RESOLVE BLOCK DONE", StartLocation,
				      ec.CurrentBranching, statement_count);

			while (ec.CurrentBranching is FlowBranchingLabeled)
				ec.EndFlowBranching ();

			bool flow_unreachable = ec.EndFlowBranching ();

			ec.CurrentBlock = prev_block;

			if (flow_unreachable)
				flags |= Flags.HasRet;

			// If we're a non-static `struct' constructor which doesn't have an
			// initializer, then we must initialize all of the struct's fields.
			if (this == ParametersBlock.TopBlock && !ParametersBlock.TopBlock.IsThisAssigned (ec) && !flow_unreachable)
				ok = false;

			return ok;
		}

		public override bool ResolveUnreachable (BlockContext ec, bool warn)
		{
			unreachable_shown = true;
			unreachable = true;

			if (warn)
				ec.Report.Warning (162, 2, loc, "Unreachable code detected");

			ec.StartFlowBranching (FlowBranching.BranchingType.Block, loc);
			bool ok = Resolve (ec);
			ec.KillFlowBranching ();

			return ok;
		}
		
		protected override void DoEmit (EmitContext ec)
		{
			for (int ix = 0; ix < statements.Count; ix++){
				statements [ix].Emit (ec);
			}
		}

		public override void Emit (EmitContext ec)
		{
			if (scope_initializers != null)
				EmitScopeInitializers (ec);

			ec.Mark (StartLocation);
			DoEmit (ec);

			if (SymbolWriter.HasSymbolWriter)
				EmitSymbolInfo (ec);
		}

		protected void EmitScopeInitializers (EmitContext ec)
		{
			SymbolWriter.OpenCompilerGeneratedBlock (ec);

			using (ec.With (EmitContext.Options.OmitDebugInfo, true)) {
				foreach (Statement s in scope_initializers)
					s.Emit (ec);
			}

			SymbolWriter.CloseCompilerGeneratedBlock (ec);
		}

		protected virtual void EmitSymbolInfo (EmitContext ec)
		{
		}

#if DEBUG
		public override string ToString ()
		{
			return String.Format ("{0} ({1}:{2})", GetType (), ID, StartLocation);
		}
#endif

		protected override void CloneTo (CloneContext clonectx, Statement t)
		{
			Block target = (Block) t;
#if DEBUG
			target.clone_id = clone_id_counter++;
#endif

			clonectx.AddBlockMap (this, target);

			target.ParametersBlock = (ParametersBlock) (ParametersBlock == this ? target : clonectx.RemapBlockCopy (ParametersBlock));
			target.Explicit = (ExplicitBlock) (Explicit == this ? target : clonectx.LookupBlock (Explicit));

			if (Parent != null)
				target.Parent = clonectx.RemapBlockCopy (Parent);

			target.statements = new List<Statement> (statements.Count);
			foreach (Statement s in statements)
				target.statements.Add (s.Clone (clonectx));
		}
	}

	public class ExplicitBlock : Block
	{
		protected AnonymousMethodStorey am_storey;

		public ExplicitBlock (Block parent, Location start, Location end)
			: this (parent, (Flags) 0, start, end)
		{
		}

		public ExplicitBlock (Block parent, Flags flags, Location start, Location end)
			: base (parent, flags, start, end)
		{
			this.Explicit = this;
		}

		#region Properties

		public AnonymousMethodStorey AnonymousMethodStorey {
			get {
				return am_storey;
			}
		}

		public bool HasCapturedThis {
			set { flags = value ? flags | Flags.HasCapturedThis : flags & ~Flags.HasCapturedThis; }
			get {
				return (flags & Flags.HasCapturedThis) != 0;
			}
		}

		public bool HasCapturedVariable {
			set { flags = value ? flags | Flags.HasCapturedVariable : flags & ~Flags.HasCapturedVariable; }
			get {
				return (flags & Flags.HasCapturedVariable) != 0;
			}
		}

		#endregion

		//
		// Creates anonymous method storey in current block
		//
		public AnonymousMethodStorey CreateAnonymousMethodStorey (ResolveContext ec)
		{
			//
			// An iterator has only 1 storey block
			//
			if (ec.CurrentIterator != null)
			    return ec.CurrentIterator.Storey;

			//
			// When referencing a variable in iterator storey from children anonymous method
			//
			if (ParametersBlock.am_storey is IteratorStorey) {
				return ParametersBlock.am_storey;
			}

			if (am_storey == null) {
				MemberBase mc = ec.MemberContext as MemberBase;

				//
				// Creates anonymous method storey for this block
				//
				am_storey = new AnonymousMethodStorey (this, ec.CurrentMemberDefinition.Parent.PartialContainer, mc, ec.CurrentTypeParameters, "AnonStorey");
			}

			return am_storey;
		}

		public override void Emit (EmitContext ec)
		{
			if (am_storey != null) {
				DefineAnonymousStorey (ec);
				am_storey.EmitStoreyInstantiation (ec, this);
			}

			bool emit_debug_info = SymbolWriter.HasSymbolWriter && Parent != null && !(am_storey is IteratorStorey);
			if (emit_debug_info)
				ec.BeginScope ();

			base.Emit (ec);

			if (emit_debug_info)
				ec.EndScope ();
		}

		void DefineAnonymousStorey (EmitContext ec)
		{
			//
			// Creates anonymous method storey
			//
			if (ec.CurrentAnonymousMethod != null && ec.CurrentAnonymousMethod.Storey != null) {
				//
				// Creates parent storey reference when hoisted this is accessible
				//
				if (am_storey.OriginalSourceBlock.Explicit.HasCapturedThis) {
					ExplicitBlock parent = am_storey.OriginalSourceBlock.Explicit.Parent.Explicit;

					//
					// Hoisted this exists in top-level parent storey only
					//
					while (parent.am_storey == null || parent.am_storey.Parent is AnonymousMethodStorey)
						parent = parent.Parent.Explicit;

					am_storey.AddParentStoreyReference (ec, parent.am_storey);
				}

				am_storey.SetNestedStoryParent (ec.CurrentAnonymousMethod.Storey);

				// TODO MemberCache: Review
				am_storey.Mutator = ec.CurrentAnonymousMethod.Storey.Mutator;
			}

			am_storey.CreateType ();
			if (am_storey.Mutator == null && ec.CurrentTypeParameters != null)
				am_storey.Mutator = new TypeParameterMutator (ec.CurrentTypeParameters, am_storey.CurrentTypeParameters);

			am_storey.DefineType ();
			am_storey.ResolveTypeParameters ();

			var ref_blocks = am_storey.ReferencesFromChildrenBlock;
			if (ref_blocks != null) {
				foreach (ExplicitBlock ref_block in ref_blocks) {
					for (ExplicitBlock b = ref_block.Explicit; b.am_storey != am_storey; b = b.Parent.Explicit) {
						if (b.am_storey != null) {
							b.am_storey.AddParentStoreyReference (ec, am_storey);

							// Stop propagation inside same top block
							if (b.ParametersBlock.am_storey == ParametersBlock.am_storey)
								break;

							b = b.ParametersBlock;
						}

						b.HasCapturedVariable = true;
					}
				}
			}

			am_storey.Define ();
			am_storey.Parent.PartialContainer.AddCompilerGeneratedClass (am_storey);
		}

		public void WrapIntoDestructor (TryFinally tf, ExplicitBlock tryBlock)
		{
			tryBlock.statements = statements;
			statements = new List<Statement> (1);
			statements.Add (tf);
		}
	}

	//
	// ParametersBlock was introduced to support anonymous methods
	// and lambda expressions
	// 
	public class ParametersBlock : ExplicitBlock
	{
		public class ParameterInfo : INamedBlockVariable
		{
			readonly ParametersBlock block;
			readonly int index;
			public VariableInfo VariableInfo;
			bool is_locked;

			public ParameterInfo (ParametersBlock block, int index)
			{
				this.block = block;
				this.index = index;
			}

			#region Properties

			public Block Block {
				get {
					return block;
				}
			}

			public bool IsDeclared {
				get {
					return true;
				}
			}

			public bool IsLocked {
				get {
					return is_locked;
				}
				set {
					is_locked = value;
				}
			}

			public Location Location {
				get {
					return Parameter.Location;
				}
			}

			public Parameter Parameter {
				get {
					return block.Parameters [index];
				}
			}

			public TypeSpec ParameterType {
				get {
					return Parameter.Type;
				}
			}

			#endregion

			public Expression CreateReferenceExpression (ResolveContext rc, Location loc)
			{
				return new ParameterReference (this, loc);
			}
		}

		// 
		// Block is converted to an expression
		//
		sealed class BlockScopeExpression : Expression
		{
			Expression child;
			readonly ParametersBlock block;

			public BlockScopeExpression (Expression child, ParametersBlock block)
			{
				this.child = child;
				this.block = block;
			}

			public override Expression CreateExpressionTree (ResolveContext ec)
			{
				throw new NotSupportedException ();
			}

			protected override Expression DoResolve (ResolveContext ec)
			{
				if (child == null)
					return null;

				child = child.Resolve (ec);
				if (child == null)
					return null;

				eclass = child.eclass;
				type = child.Type;
				return this;
			}

			public override void Emit (EmitContext ec)
			{
				block.EmitScopeInitializers (ec);
				child.Emit (ec);
			}
		}

		protected ParametersCompiled parameters;
		protected ParameterInfo[] parameter_info;
		bool resolved;
		protected bool unreachable;
		protected ToplevelBlock top_block;

		public ParametersBlock (Block parent, ParametersCompiled parameters, Location start)
			: base (parent, 0, start, start)
		{
			if (parameters == null)
				throw new ArgumentNullException ("parameters");

			this.parameters = parameters;
			ParametersBlock = this;

			this.top_block = parent.ParametersBlock.top_block;
			ProcessParameters ();
		}

		protected ParametersBlock (ParametersCompiled parameters, Location start)
			: base (null, 0, start, start)
		{
			if (parameters == null)
				throw new ArgumentNullException ("parameters");

			this.parameters = parameters;
			ParametersBlock = this;
		}

		protected ParametersBlock (ParametersBlock source, ParametersCompiled parameters)
			: base (null, 0, source.StartLocation, source.EndLocation)
		{
			this.parameters = parameters;
			this.statements = source.statements;
			this.scope_initializers = source.scope_initializers;

			this.resolved = true;
			this.unreachable = source.unreachable;
			this.am_storey = source.am_storey;

			ParametersBlock = this;
		}

		#region Properties

		//
		// Block has been converted to expression tree
		//
		public bool IsExpressionTree {
			get {
				return (flags & Flags.IsExpressionTree) != 0;
			}
		}

		//
		// The parameters for the block.
		//
		public ParametersCompiled Parameters {
			get {
				return parameters;
			}
		}

		public ToplevelBlock TopBlock {
			get {
				return top_block;
			}
		}

		public bool Resolved {
			get {
				return resolved;
			}
		}

		#endregion

		// <summary>
		//   Check whether all `out' parameters have been assigned.
		// </summary>
		public void CheckOutParameters (FlowBranching.UsageVector vector, Location loc)
		{
			if (vector.IsUnreachable)
				return;

			int n = parameter_info == null ? 0 : parameter_info.Length;

			for (int i = 0; i < n; i++) {
				VariableInfo var = parameter_info[i].VariableInfo;

				if (var == null)
					continue;

				if (vector.IsAssigned (var, false))
					continue;

				TopBlock.Report.Error (177, loc, "The out parameter `{0}' must be assigned to before control leaves the current method",
					var.Name);
			}
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			if (statements.Count == 1) {
				Expression expr = ((Statement) statements[0]).CreateExpressionTree (ec);
				if (scope_initializers != null)
					expr = new BlockScopeExpression (expr, this);

				return expr;
			}

			return base.CreateExpressionTree (ec);
		}

		public ParameterInfo GetParameterInfo (Parameter p)
		{
			for (int i = 0; i < parameters.Count; ++i) {
				if (parameters[i] == p)
					return parameter_info[i];
			}

			throw new ArgumentException ("Invalid parameter");
		}

		public Expression GetParameterReference (int index, Location loc)
		{
			return new ParameterReference (parameter_info[index], loc);
		}

		protected void ProcessParameters ()
		{
			if (parameters.Count == 0)
				return;

			parameter_info = new ParameterInfo[parameters.Count];
			for (int i = 0; i < parameter_info.Length; ++i) {
				var p = parameters.FixedParameters[i];
				if (p == null)
					continue;

				// TODO: Should use Parameter only and more block there
				parameter_info[i] = new ParameterInfo (this, i);
				AddLocalName (p.Name, parameter_info[i]);
			}
		}

		public bool Resolve (FlowBranching parent, BlockContext rc, IMethodData md)
		{
			if (resolved)
				return true;

			resolved = true;

			if (rc.HasSet (ResolveContext.Options.ExpressionTreeConversion))
				flags |= Flags.IsExpressionTree;

			try {
				ResolveMeta (rc);

				using (rc.With (ResolveContext.Options.DoFlowAnalysis, true)) {
					FlowBranchingToplevel top_level = rc.StartFlowBranching (this, parent);

					if (!Resolve (rc))
						return false;

					unreachable = top_level.End ();
				}
			} catch (Exception e) {
				if (e is CompletionResult || rc.Report.IsDisabled)
					throw;

				if (rc.CurrentBlock != null) {
					rc.Report.Error (584, rc.CurrentBlock.StartLocation, "Internal compiler error: {0}", e.Message);
				} else {
					rc.Report.Error (587, "Internal compiler error: {0}", e.Message);
				}

				if (Report.DebugFlags > 0)
					throw;
			}

			if (rc.ReturnType != TypeManager.void_type && !unreachable) {
				if (rc.CurrentAnonymousMethod == null) {
					// FIXME: Missing FlowAnalysis for generated iterator MoveNext method
					if (md is IteratorMethod) {
						unreachable = true;
					} else {
						rc.Report.Error (161, md.Location, "`{0}': not all code paths return a value", md.GetSignatureForError ());
						return false;
					}
				} else {
					rc.Report.Error (1643, rc.CurrentAnonymousMethod.Location, "Not all code paths return a value in anonymous method of type `{0}'",
							  rc.CurrentAnonymousMethod.GetSignatureForError ());
					return false;
				}
			}

			return true;
		}

		void ResolveMeta (BlockContext ec)
		{
			int orig_count = parameters.Count;

			for (int i = 0; i < orig_count; ++i) {
				Parameter.Modifier mod = parameters.FixedParameters[i].ModFlags;

				if ((mod & Parameter.Modifier.OUT) != Parameter.Modifier.OUT)
					continue;

				VariableInfo vi = new VariableInfo (parameters, i, ec.FlowOffset);
				parameter_info[i].VariableInfo = vi;
				ec.FlowOffset += vi.Length;
			}
		}

		public void WrapIntoIterator (IMethodData method, TypeContainer host, TypeSpec iterator_type, bool is_enumerable)
		{
			ParametersBlock pb = new ParametersBlock (this, ParametersCompiled.EmptyReadOnlyParameters, StartLocation);
			pb.EndLocation = EndLocation;
			pb.statements = statements;

			var iterator = new Iterator (pb, method, host, iterator_type, is_enumerable);
			am_storey = new IteratorStorey (iterator);

			statements = new List<Statement> (1);
			AddStatement (new Return (iterator, iterator.Location));
		}
	}

	//
	//
	//
	public class ToplevelBlock : ParametersBlock
	{
		LocalVariable this_variable;
		CompilerContext compiler;
		Dictionary<string, object> names;
		Dictionary<string, object> labels;

		public HoistedVariable HoistedThisVariable;

		public Report Report {
			get { return compiler.Report; }
		}

		public ToplevelBlock (CompilerContext ctx, Location loc)
			: this (ctx, ParametersCompiled.EmptyReadOnlyParameters, loc)
		{
		}

		public ToplevelBlock (CompilerContext ctx, ParametersCompiled parameters, Location start)
			: base (parameters, start)
		{
			this.compiler = ctx;
			top_block = this;

			ProcessParameters ();
		}

		//
		// Recreates a top level block from parameters block. Used for
		// compiler generated methods where the original block comes from
		// explicit child block. This works for already resolved blocks
		// only to ensure we resolve them in the correct flow order
		//
		public ToplevelBlock (ParametersBlock source, ParametersCompiled parameters)
			: base (source, parameters)
		{
			this.compiler = source.TopBlock.compiler;
			top_block = this;
		}

		public override void AddLocalName (string name, INamedBlockVariable li)
		{
			if (names == null)
				names = new Dictionary<string, object> ();

			object value;
			if (!names.TryGetValue (name, out value)) {
				names.Add (name, li);
				return;
			}

			INamedBlockVariable existing = value as INamedBlockVariable;
			List<INamedBlockVariable> existing_list;
			if (existing != null) {
				existing_list = new List<INamedBlockVariable> ();
				existing_list.Add (existing);
				names[name] = existing_list;
			} else {
				existing_list = (List<INamedBlockVariable>) value;
			}

			//
			// A collision checking between local names
			//
			for (int i = 0; i < existing_list.Count; ++i) {
				existing = existing_list[i];
				Block b = existing.Block;

				// Collision at same level
				if (li.Block == b) {
					li.Block.Error_AlreadyDeclared (name, li);
					break;
				}

				// Collision with parent
				b = li.Block;
				while ((b = b.Parent) != null) {
					if (existing.Block == b) {
						li.Block.Error_AlreadyDeclared (name, li, "parent or current");
						i = existing_list.Count;
						break;
					}
				}

				// Collision with with children
				b = existing.Block;
				while ((b = b.Parent) != null) {
					if (li.Block == b) {
						li.Block.Error_AlreadyDeclared (name, li, "child");
						i = existing_list.Count;
						break;
					}
				}
			}

			existing_list.Add (li);
		}

		public void AddLabel (string name, LabeledStatement label)
		{
			if (labels == null)
				labels = new Dictionary<string, object> ();

			object value;
			if (!labels.TryGetValue (name, out value)) {
				labels.Add (name, label);
				return;
			}

			LabeledStatement existing = value as LabeledStatement;
			List<LabeledStatement> existing_list;
			if (existing != null) {
				existing_list = new List<LabeledStatement> ();
				existing_list.Add (existing);
				labels[name] = existing_list;
			} else {
				existing_list = (List<LabeledStatement>) value;
			}

			//
			// A collision checking between labels
			//
			for (int i = 0; i < existing_list.Count; ++i) {
				existing = existing_list[i];
				Block b = existing.Block;

				// Collision at same level
				if (label.Block == b) {
					Report.SymbolRelatedToPreviousError (existing.loc, name);
					Report.Error (140, label.loc, "The label `{0}' is a duplicate", name);
					break;
				}

				// Collision with parent
				b = label.Block;
				while ((b = b.Parent) != null) {
					if (existing.Block == b) {
						Report.Error (158, label.loc,
							"The label `{0}' shadows another label by the same name in a contained scope", name);
						i = existing_list.Count;
						break;
					}
				}

				// Collision with with children
				b = existing.Block;
				while ((b = b.Parent) != null) {
					if (label.Block == b) {
						Report.Error (158, label.loc,
							"The label `{0}' shadows another label by the same name in a contained scope", name);
						i = existing_list.Count;
						break;
					}
				}
			}

			existing_list.Add (label);
		}

		//
		// Creates an arguments set from all parameters, useful for method proxy calls
		//
		public Arguments GetAllParametersArguments ()
		{
			int count = parameters.Count;
			Arguments args = new Arguments (count);
			for (int i = 0; i < count; ++i) {
				var arg_expr = GetParameterReference (i, parameter_info[i].Location);
				args.Add (new Argument (arg_expr));
			}

			return args;
		}

		//
		// Lookup inside a block, the returned value can represent 3 states
		//
		// true+variable: A local name was found and it's valid
		// false+variable: A local name was found in a child block only
		// false+null: No local name was found
		//
		public bool GetLocalName (string name, Block block, ref INamedBlockVariable variable)
		{
			if (names == null)
				return false;

			object value;
			if (!names.TryGetValue (name, out value))
				return false;

			variable = value as INamedBlockVariable;
			Block b = block;
			if (variable != null) {
				do {
					if (variable.Block == b.Original)
						return true;

					b = b.Parent;
				} while (b != null);

				b = variable.Block;
				do {
					if (block == b)
						return false;

					b = b.Parent;
				} while (b != null);
			} else {
				List<INamedBlockVariable> list = (List<INamedBlockVariable>) value;
				for (int i = 0; i < list.Count; ++i) {
					variable = list[i];
					do {
						if (variable.Block == b.Original)
							return true;

						b = b.Parent;
					} while (b != null);

					b = variable.Block;
					do {
						if (block == b)
							return false;

						b = b.Parent;
					} while (b != null);

					b = block;
				}
			}

			variable = null;
			return false;
		}

		public LabeledStatement GetLabel (string name, Block block)
		{
			if (labels == null)
				return null;

			object value;
			if (!labels.TryGetValue (name, out value)) {
				return null;
			}

			var label = value as LabeledStatement;
			Block b = block;
			if (label != null) {
				if (label.Block == b.Original)
					return label;

				// TODO: Temporary workaround for the switch block implicit label block
				if (label.Block.IsCompilerGenerated && label.Block.Parent == b.Original)
					return label;
			} else {
				List<LabeledStatement> list = (List<LabeledStatement>) value;
				for (int i = 0; i < list.Count; ++i) {
					label = list[i];
					if (label.Block == b.Original)
						return label;

					// TODO: Temporary workaround for the switch block implicit label block
					if (label.Block.IsCompilerGenerated && label.Block.Parent == b.Original)
						return label;
				}
			}
				
			return null;
		}

		// <summary>
		//   Returns the "this" instance variable of this block.
		//   See AddThisVariable() for more information.
		// </summary>
		public LocalVariable ThisVariable {
			get { return this_variable; }
		}

		// <summary>
		//   This is used by non-static `struct' constructors which do not have an
		//   initializer - in this case, the constructor must initialize all of the
		//   struct's fields.  To do this, we add a "this" variable and use the flow
		//   analysis code to ensure that it's been fully initialized before control
		//   leaves the constructor.
		// </summary>
		public LocalVariable AddThisVariable (BlockContext bc, TypeContainer ds, Location l)
		{
			if (this_variable == null) {
				this_variable = new LocalVariable (this, "this", LocalVariable.Flags.IsThis | LocalVariable.Flags.Used, l);
				this_variable.Type = ds.CurrentType;
				this_variable.PrepareForFlowAnalysis (bc);
			}

			return this_variable;
		}

		public bool IsIterator {
			get { return (flags & Flags.IsIterator) != 0; }
			set { flags = value ? flags | Flags.IsIterator : flags & ~Flags.IsIterator; }
		}

		public bool IsThisAssigned (BlockContext ec)
		{
			return this_variable == null || this_variable.IsThisAssigned (ec, this);
		}

		public override void Emit (EmitContext ec)
		{
			if (Report.Errors > 0)
				return;

#if PRODUCTION
			try {
#endif
			if (ec.HasReturnLabel)
				ec.ReturnLabel = ec.DefineLabel ();

			base.Emit (ec);

			ec.Mark (EndLocation);

			if (ec.HasReturnLabel)
				ec.MarkLabel (ec.ReturnLabel);

			if (ec.return_value != null) {
				ec.Emit (OpCodes.Ldloc, ec.return_value);
				ec.Emit (OpCodes.Ret);
			} else {
				//
				// If `HasReturnLabel' is set, then we already emitted a
				// jump to the end of the method, so we must emit a `ret'
				// there.
				//
				// Unfortunately, System.Reflection.Emit automatically emits
				// a leave to the end of a finally block.  This is a problem
				// if no code is following the try/finally block since we may
				// jump to a point after the end of the method.
				// As a workaround, we're always creating a return label in
				// this case.
				//

				if (ec.HasReturnLabel || !unreachable) {
					if (ec.ReturnType != TypeManager.void_type)
						ec.Emit (OpCodes.Ldloc, ec.TemporaryReturn ());
					ec.Emit (OpCodes.Ret);
				}
			}

#if PRODUCTION
			} catch (Exception e){
				Console.WriteLine ("Exception caught by the compiler while emitting:");
				Console.WriteLine ("   Block that caused the problem begin at: " + block.loc);
					
				Console.WriteLine (e.GetType ().FullName + ": " + e.Message);
				throw;
			}
#endif
		}

		protected override void EmitSymbolInfo (EmitContext ec)
		{
			AnonymousExpression ae = ec.CurrentAnonymousMethod;
			if ((ae != null) && (ae.Storey != null))
				SymbolWriter.DefineScopeVariable (ae.Storey.ID);

			base.EmitSymbolInfo (ec);
		}
	}
	
	public class SwitchLabel {
		Expression label;
		object converted;
		Location loc;

		Label il_label;
		bool  il_label_set;
		Label il_label_code;
		bool  il_label_code_set;

		public static readonly object NullStringCase = new object ();

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

		public Location Location {
			get { return loc; }
		}

		public object Converted {
			get {
				return converted;
			}
		}

		public Label GetILLabel (EmitContext ec)
		{
			if (!il_label_set){
				il_label = ec.DefineLabel ();
				il_label_set = true;
			}
			return il_label;
		}

		public Label GetILLabelCode (EmitContext ec)
		{
			if (!il_label_code_set){
				il_label_code = ec.DefineLabel ();
				il_label_code_set = true;
			}
			return il_label_code;
		}				
		
		//
		// Resolves the expression, reduces it to a literal if possible
		// and then converts it to the requested type.
		//
		public bool ResolveAndReduce (ResolveContext ec, TypeSpec required_type, bool allow_nullable)
		{	
			Expression e = label.Resolve (ec);

			if (e == null)
				return false;

			Constant c = e as Constant;
			if (c == null){
				ec.Report.Error (150, loc, "A constant value is expected");
				return false;
			}

			if (required_type == TypeManager.string_type && c.GetValue () == null) {
				converted = NullStringCase;
				return true;
			}

			if (allow_nullable && c.GetValue () == null) {
				converted = NullStringCase;
				return true;
			}
			
			c = c.ImplicitConversionRequired (ec, required_type, loc);
			if (c == null)
				return false;

			converted = c.GetValue ();
			return true;
		}

		public void Error_AlreadyOccurs (ResolveContext ec, TypeSpec switch_type, SwitchLabel collision_with)
		{
			string label;
			if (converted == null)
				label = "default";
			else if (converted == NullStringCase)
				label = "null";
			else
				label = converted.ToString ();
			
			ec.Report.SymbolRelatedToPreviousError (collision_with.loc, null);
			ec.Report.Error (152, loc, "The label `case {0}:' already occurs in this switch statement", label);
		}

		public SwitchLabel Clone (CloneContext clonectx)
		{
			return new SwitchLabel (label.Clone (clonectx), loc);
		}
	}

	public class SwitchSection {
		public readonly List<SwitchLabel> Labels;
		public readonly Block Block;
		
		public SwitchSection (List<SwitchLabel> labels, Block block)
		{
			Labels = labels;
			Block = block;
		}

		public SwitchSection Clone (CloneContext clonectx)
		{
			var cloned_labels = new List<SwitchLabel> ();

			foreach (SwitchLabel sl in cloned_labels)
				cloned_labels.Add (sl.Clone (clonectx));
			
			return new SwitchSection (cloned_labels, clonectx.LookupBlock (Block));
		}
	}
	
	public class Switch : Statement {
		public List<SwitchSection> Sections;
		public Expression Expr;

		/// <summary>
		///   Maps constants whose type type SwitchType to their  SwitchLabels.
		/// </summary>
		public IDictionary<object, SwitchLabel> Elements;

		/// <summary>
		///   The governing switch type
		/// </summary>
		public TypeSpec SwitchType;

		//
		// Computed
		//
		Label default_target;
		Label null_target;
		Expression new_expr;
		bool is_constant;
		bool has_null_case;
		SwitchSection constant_section;
		SwitchSection default_section;

		ExpressionStatement string_dictionary;
		FieldExpr switch_cache_field;
		static int unique_counter;
		ExplicitBlock block;

		//
		// Nullable Types support
		//
		Nullable.Unwrap unwrap;

		protected bool HaveUnwrap {
			get { return unwrap != null; }
		}

		//
		// The types allowed to be implicitly cast from
		// on the governing type
		//
		static TypeSpec [] allowed_types;

		public Switch (Expression e, ExplicitBlock block, List<SwitchSection> sects, Location l)
		{
			Expr = e;
			this.block = block;
			Sections = sects;
			loc = l;
		}

		public ExplicitBlock Block {
			get {
				return block;
			}
		}

		public bool GotDefault {
			get {
				return default_section != null;
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
		Expression SwitchGoverningType (ResolveContext ec, Expression expr)
		{
			TypeSpec t = expr.Type;

			if (t == TypeManager.byte_type ||
			    t == TypeManager.sbyte_type ||
			    t == TypeManager.ushort_type ||
			    t == TypeManager.short_type ||
			    t == TypeManager.uint32_type ||
			    t == TypeManager.int32_type ||
			    t == TypeManager.uint64_type ||
			    t == TypeManager.int64_type ||
			    t == TypeManager.char_type ||
			    t == TypeManager.string_type ||
			    t == TypeManager.bool_type ||
			    TypeManager.IsEnumType (t))
				return expr;

			if (allowed_types == null){
				allowed_types = new TypeSpec [] {
					TypeManager.sbyte_type,
					TypeManager.byte_type,
					TypeManager.short_type,
					TypeManager.ushort_type,
					TypeManager.int32_type,
					TypeManager.uint32_type,
					TypeManager.int64_type,
					TypeManager.uint64_type,
					TypeManager.char_type,
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
			foreach (TypeSpec tt in allowed_types){
				Expression e;
				
				e = Convert.ImplicitUserConversion (ec, expr, tt, loc);
				if (e == null)
					continue;

				//
				// Ignore over-worked ImplicitUserConversions that do
				// an implicit conversion in addition to the user conversion.
				// 
				if (!(e is UserCast))
					continue;

				if (converted != null){
					ec.Report.ExtraInformation (loc, "(Ambiguous implicit user defined conversion in previous ");
					return null;
				}

				converted = e;
			}
			return converted;
		}

		//
		// Performs the basic sanity checks on the switch statement
		// (looks for duplicate keys and non-constant expressions).
		//
		// It also returns a hashtable with the keys that we will later
		// use to compute the switch tables
		//
		bool CheckSwitch (ResolveContext ec)
		{
			bool error = false;
			Elements = new Dictionary<object, SwitchLabel> ();
				
			foreach (SwitchSection ss in Sections){
				foreach (SwitchLabel sl in ss.Labels){
					if (sl.Label == null){
						if (default_section != null){
							sl.Error_AlreadyOccurs (ec, SwitchType, (SwitchLabel)default_section.Labels [0]);
							error = true;
						}
						default_section = ss;
						continue;
					}

					if (!sl.ResolveAndReduce (ec, SwitchType, HaveUnwrap)) {
						error = true;
						continue;
					}
					
					object key = sl.Converted;
					if (key == SwitchLabel.NullStringCase)
						has_null_case = true;

					try {
						Elements.Add (key, sl);
					} catch (ArgumentException) {
						sl.Error_AlreadyOccurs (ec, SwitchType, Elements [key]);
						error = true;
					}
				}
			}
			return !error;
		}

		void EmitObjectInteger (EmitContext ec, object k)
		{
			if (k is int)
				ec.EmitInt ((int) k);
			else if (k is Constant) {
				EmitObjectInteger (ec, ((Constant) k).GetValue ());
			} 
			else if (k is uint)
				ec.EmitInt (unchecked ((int) (uint) k));
			else if (k is long)
			{
				if ((long) k >= int.MinValue && (long) k <= int.MaxValue)
				{
					ec.EmitInt ((int) (long) k);
					ec.Emit (OpCodes.Conv_I8);
				}
				else
					ec.EmitLong ((long) k);
			}
			else if (k is ulong)
			{
				ulong ul = (ulong) k;
				if (ul < (1L<<32))
				{
					ec.EmitInt (unchecked ((int) ul));
					ec.Emit (OpCodes.Conv_U8);
				}
				else
				{
					ec.EmitLong (unchecked ((long) ul));
				}
			}
			else if (k is char)
				ec.EmitInt ((int) ((char) k));
			else if (k is sbyte)
				ec.EmitInt ((int) ((sbyte) k));
			else if (k is byte)
				ec.EmitInt ((int) ((byte) k));
			else if (k is short)
				ec.EmitInt ((int) ((short) k));
			else if (k is ushort)
				ec.EmitInt ((int) ((ushort) k));
			else if (k is bool)
				ec.EmitInt (((bool) k) ? 1 : 0);
			else
				throw new Exception ("Unhandled case");
		}
		
		// structure used to hold blocks of keys while calculating table switch
		class KeyBlock : IComparable
		{
			public KeyBlock (long _first)
			{
				first = last = _first;
			}
			public long first;
			public long last;
			public List<object> element_keys;
			// how many items are in the bucket
			public int Size = 1;
			public int Length
			{
				get { return (int) (last - first + 1); }
			}
			public static long TotalLength (KeyBlock kb_first, KeyBlock kb_last)
			{
				return kb_last.last - kb_first.first + 1;
			}
			public int CompareTo (object obj)
			{
				KeyBlock kb = (KeyBlock) obj;
				int nLength = Length;
				int nLengthOther = kb.Length;
				if (nLengthOther == nLength)
					return (int) (kb.first - first);
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
		void TableSwitchEmit (EmitContext ec, Expression val)
		{
			int element_count = Elements.Count;
			object [] element_keys = new object [element_count];
			Elements.Keys.CopyTo (element_keys, 0);
			Array.Sort (element_keys);

			// initialize the block list with one element per key
			var key_blocks = new List<KeyBlock> (element_count);
			foreach (object key in element_keys)
				key_blocks.Add (new KeyBlock (System.Convert.ToInt64 (key)));

			KeyBlock current_kb;
			// iteratively merge the blocks while they are at least half full
			// there's probably a really cool way to do this with a tree...
			while (key_blocks.Count > 1)
			{
				var key_blocks_new = new List<KeyBlock> ();
				current_kb = (KeyBlock) key_blocks [0];
				for (int ikb = 1; ikb < key_blocks.Count; ikb++)
				{
					KeyBlock kb = (KeyBlock) key_blocks [ikb];
					if ((current_kb.Size + kb.Size) * 2 >=  KeyBlock.TotalLength (current_kb, kb))
					{
						// merge blocks
						current_kb.last = kb.last;
						current_kb.Size += kb.Size;
					}
					else
					{
						// start a new block
						key_blocks_new.Add (current_kb);
						current_kb = kb;
					}
				}
				key_blocks_new.Add (current_kb);
				if (key_blocks.Count == key_blocks_new.Count)
					break;
				key_blocks = key_blocks_new;
			}

			// initialize the key lists
			foreach (KeyBlock kb in key_blocks)
				kb.element_keys = new List<object> ();

			// fill the key lists
			int iBlockCurr = 0;
			if (key_blocks.Count > 0) {
				current_kb = (KeyBlock) key_blocks [0];
				foreach (object key in element_keys)
				{
					bool next_block = (key is UInt64) ? (ulong) key > (ulong) current_kb.last :
						System.Convert.ToInt64 (key) > current_kb.last;
					if (next_block)
						current_kb = (KeyBlock) key_blocks [++iBlockCurr];
					current_kb.element_keys.Add (key);
				}
			}

			// sort the blocks so we can tackle the largest ones first
			key_blocks.Sort ();

			// okay now we can start...
			Label lbl_end = ec.DefineLabel ();	// at the end ;-)
			Label lbl_default = default_target;

			Type type_keys = null;
			if (element_keys.Length > 0)
				type_keys = element_keys [0].GetType ();	// used for conversions

			TypeSpec compare_type;
			
			if (TypeManager.IsEnumType (SwitchType))
				compare_type = EnumSpec.GetUnderlyingType (SwitchType);
			else
				compare_type = SwitchType;
			
			for (int iBlock = key_blocks.Count - 1; iBlock >= 0; --iBlock)
			{
				KeyBlock kb = ((KeyBlock) key_blocks [iBlock]);
				lbl_default = (iBlock == 0) ? default_target : ec.DefineLabel ();
				if (kb.Length <= 2)
				{
					foreach (object key in kb.element_keys) {
						SwitchLabel sl = (SwitchLabel) Elements [key];
						if (key is int && (int) key == 0) {
							val.EmitBranchable (ec, sl.GetILLabel (ec), false);
						} else {
							val.Emit (ec);
							EmitObjectInteger (ec, key);
							ec.Emit (OpCodes.Beq, sl.GetILLabel (ec));
						}
					}
				}
				else
				{
					// TODO: if all the keys in the block are the same and there are
					//       no gaps/defaults then just use a range-check.
					if (compare_type == TypeManager.int64_type ||
						compare_type == TypeManager.uint64_type)
					{
						// TODO: optimize constant/I4 cases

						// check block range (could be > 2^31)
						val.Emit (ec);
						EmitObjectInteger (ec, System.Convert.ChangeType (kb.first, type_keys));
						ec.Emit (OpCodes.Blt, lbl_default);
						val.Emit (ec);
						EmitObjectInteger (ec, System.Convert.ChangeType (kb.last, type_keys));
						ec.Emit (OpCodes.Bgt, lbl_default);

						// normalize range
						val.Emit (ec);
						if (kb.first != 0)
						{
							EmitObjectInteger (ec, System.Convert.ChangeType (kb.first, type_keys));
							ec.Emit (OpCodes.Sub);
						}
						ec.Emit (OpCodes.Conv_I4);	// assumes < 2^31 labels!
					}
					else
					{
						// normalize range
						val.Emit (ec);
						int first = (int) kb.first;
						if (first > 0)
						{
							ec.EmitInt (first);
							ec.Emit (OpCodes.Sub);
						}
						else if (first < 0)
						{
							ec.EmitInt (-first);
							ec.Emit (OpCodes.Add);
						}
					}

					// first, build the list of labels for the switch
					int iKey = 0;
					int cJumps = kb.Length;
					Label [] switch_labels = new Label [cJumps];
					for (int iJump = 0; iJump < cJumps; iJump++)
					{
						object key = kb.element_keys [iKey];
						if (System.Convert.ToInt64 (key) == kb.first + iJump)
						{
							SwitchLabel sl = (SwitchLabel) Elements [key];
							switch_labels [iJump] = sl.GetILLabel (ec);
							iKey++;
						}
						else
							switch_labels [iJump] = lbl_default;
					}
					// emit the switch opcode
					ec.Emit (OpCodes.Switch, switch_labels);
				}

				// mark the default for this block
				if (iBlock != 0)
					ec.MarkLabel (lbl_default);
			}

			// TODO: find the default case and emit it here,
			//       to prevent having to do the following jump.
			//       make sure to mark other labels in the default section

			// the last default just goes to the end
			if (element_keys.Length > 0)
				ec.Emit (OpCodes.Br, lbl_default);

			// now emit the code for the sections
			bool found_default = false;

			foreach (SwitchSection ss in Sections) {
				foreach (SwitchLabel sl in ss.Labels) {
					if (sl.Converted == SwitchLabel.NullStringCase) {
						ec.MarkLabel (null_target);
					} else if (sl.Label == null) {
						ec.MarkLabel (lbl_default);
						found_default = true;
						if (!has_null_case)
							ec.MarkLabel (null_target);
					}
					ec.MarkLabel (sl.GetILLabel (ec));
					ec.MarkLabel (sl.GetILLabelCode (ec));
				}
				ss.Block.Emit (ec);
			}
			
			if (!found_default) {
				ec.MarkLabel (lbl_default);
				if (!has_null_case) {
					ec.MarkLabel (null_target);
				}
			}
			
			ec.MarkLabel (lbl_end);
		}

		SwitchSection FindSection (SwitchLabel label)
		{
			foreach (SwitchSection ss in Sections){
				foreach (SwitchLabel sl in ss.Labels){
					if (label == sl)
						return ss;
				}
			}

			return null;
		}

		public static void Reset ()
		{
			unique_counter = 0;
			allowed_types = null;
		}

		public override bool Resolve (BlockContext ec)
		{
			Expr = Expr.Resolve (ec);
			if (Expr == null)
				return false;

			new_expr = SwitchGoverningType (ec, Expr);

			if ((new_expr == null) && TypeManager.IsNullableType (Expr.Type)) {
				unwrap = Nullable.Unwrap.Create (Expr, false);
				if (unwrap == null)
					return false;

				new_expr = SwitchGoverningType (ec, unwrap);
			}

			if (new_expr == null){
				ec.Report.Error (151, loc,
					"A switch expression of type `{0}' cannot be converted to an integral type, bool, char, string, enum or nullable type",
					TypeManager.CSharpName (Expr.Type));
				return false;
			}

			// Validate switch.
			SwitchType = new_expr.Type;

			if (RootContext.Version == LanguageVersion.ISO_1 && SwitchType == TypeManager.bool_type) {
				ec.Report.FeatureIsNotAvailable (loc, "switch expression of boolean type");
				return false;
			}

			if (!CheckSwitch (ec))
				return false;

			if (HaveUnwrap)
				Elements.Remove (SwitchLabel.NullStringCase);

			Switch old_switch = ec.Switch;
			ec.Switch = this;
			ec.Switch.SwitchType = SwitchType;

			Report.Debug (1, "START OF SWITCH BLOCK", loc, ec.CurrentBranching);
			ec.StartFlowBranching (FlowBranching.BranchingType.Switch, loc);

			var constant = new_expr as Constant;
			if (constant != null) {
				is_constant = true;
				object key = constant.GetValue ();
				SwitchLabel label;
				if (Elements.TryGetValue (key, out label))
					constant_section = FindSection (label);

				if (constant_section == null)
					constant_section = default_section;
			}

			bool first = true;
			bool ok = true;
			foreach (SwitchSection ss in Sections){
				if (!first)
					ec.CurrentBranching.CreateSibling (
						null, FlowBranching.SiblingType.SwitchSection);
				else
					first = false;

				if (is_constant && (ss != constant_section)) {
					// If we're a constant switch, we're only emitting
					// one single section - mark all the others as
					// unreachable.
					ec.CurrentBranching.CurrentUsageVector.Goto ();
					if (!ss.Block.ResolveUnreachable (ec, true)) {
						ok = false;
					}
				} else {
					if (!ss.Block.Resolve (ec))
						ok = false;
				}
			}

			if (default_section == null)
				ec.CurrentBranching.CreateSibling (
					null, FlowBranching.SiblingType.SwitchSection);

			ec.EndFlowBranching ();
			ec.Switch = old_switch;

			Report.Debug (1, "END OF SWITCH BLOCK", loc, ec.CurrentBranching);

			if (!ok)
				return false;

			if (SwitchType == TypeManager.string_type && !is_constant) {
				// TODO: Optimize single case, and single+default case
				ResolveStringSwitchMap (ec);
			}

			return true;
		}

		void ResolveStringSwitchMap (ResolveContext ec)
		{
			FullNamedExpression string_dictionary_type;
			if (TypeManager.generic_ienumerable_type != null) {
				MemberAccess system_collections_generic = new MemberAccess (new MemberAccess (
					new QualifiedAliasMember (QualifiedAliasMember.GlobalAlias, "System", loc), "Collections", loc), "Generic", loc);

				string_dictionary_type = new MemberAccess (system_collections_generic, "Dictionary",
					new TypeArguments (
						new TypeExpression (TypeManager.string_type, loc),
						new TypeExpression (TypeManager.int32_type, loc)), loc);
			} else {
				MemberAccess system_collections_generic = new MemberAccess (
					new QualifiedAliasMember (QualifiedAliasMember.GlobalAlias, "System", loc), "Collections", loc);

				string_dictionary_type = new MemberAccess (system_collections_generic, "Hashtable", loc);
			}

			var ctype = ec.CurrentMemberDefinition.Parent.PartialContainer;
			Field field = new Field (ctype, string_dictionary_type,
				Modifiers.STATIC | Modifiers.PRIVATE | Modifiers.COMPILER_GENERATED,
				new MemberName (CompilerGeneratedClass.MakeName (null, "f", "switch$map", unique_counter++), loc), null);
			if (!field.Define ())
				return;
			ctype.AddField (field);

			var init = new List<Expression> ();
			int counter = 0;
			Elements.Clear ();
			string value = null;
			foreach (SwitchSection section in Sections) {
				int last_count = init.Count;
				foreach (SwitchLabel sl in section.Labels) {
					if (sl.Label == null || sl.Converted == SwitchLabel.NullStringCase)
						continue;

					value = (string) sl.Converted;
					var init_args = new List<Expression> (2);
					init_args.Add (new StringLiteral (value, sl.Location));
					init_args.Add (new IntConstant (counter, loc));
					init.Add (new CollectionElementInitializer (init_args, loc));
				}

				//
				// Don't add empty sections
				//
				if (last_count == init.Count)
					continue;

				Elements.Add (counter, section.Labels [0]);
				++counter;
			}

			Arguments args = new Arguments (1);
			args.Add (new Argument (new IntConstant (init.Count, loc)));
			Expression initializer = new NewInitialize (string_dictionary_type, args,
				new CollectionOrObjectInitializers (init, loc), loc);

			switch_cache_field = new FieldExpr (field, loc);
			string_dictionary = new SimpleAssign (switch_cache_field, initializer.Resolve (ec));
		}

		void DoEmitStringSwitch (LocalTemporary value, EmitContext ec)
		{
			Label l_initialized = ec.DefineLabel ();

			//
			// Skip initialization when value is null
			//
			value.EmitBranchable (ec, null_target, false);

			//
			// Check if string dictionary is initialized and initialize
			//
			switch_cache_field.EmitBranchable (ec, l_initialized, true);
			string_dictionary.EmitStatement (ec);
			ec.MarkLabel (l_initialized);

			LocalTemporary string_switch_variable = new LocalTemporary (TypeManager.int32_type);

			ResolveContext rc = new ResolveContext (ec.MemberContext);

			if (TypeManager.generic_ienumerable_type != null) {
				Arguments get_value_args = new Arguments (2);
				get_value_args.Add (new Argument (value));
				get_value_args.Add (new Argument (string_switch_variable, Argument.AType.Out));
				Expression get_item = new Invocation (new MemberAccess (switch_cache_field, "TryGetValue", loc), get_value_args).Resolve (rc);
				if (get_item == null)
					return;

				//
				// A value was not found, go to default case
				//
				get_item.EmitBranchable (ec, default_target, false);
			} else {
				Arguments get_value_args = new Arguments (1);
				get_value_args.Add (new Argument (value));

				Expression get_item = new ElementAccess (switch_cache_field, get_value_args, loc).Resolve (rc);
				if (get_item == null)
					return;

				LocalTemporary get_item_object = new LocalTemporary (TypeManager.object_type);
				get_item_object.EmitAssign (ec, get_item, true, false);
				ec.Emit (OpCodes.Brfalse, default_target);

				ExpressionStatement get_item_int = (ExpressionStatement) new SimpleAssign (string_switch_variable,
					new Cast (new TypeExpression (TypeManager.int32_type, loc), get_item_object, loc)).Resolve (rc);

				get_item_int.EmitStatement (ec);
				get_item_object.Release (ec);
			}

			TableSwitchEmit (ec, string_switch_variable);
			string_switch_variable.Release (ec);
		}
		
		protected override void DoEmit (EmitContext ec)
		{
			//
			// Needed to emit anonymous storey initialization
			// Otherwise it does not contain any statements for now
			//
			block.Emit (ec);

			default_target = ec.DefineLabel ();
			null_target = ec.DefineLabel ();

			// Store variable for comparission purposes
			// TODO: Don't duplicate non-captured VariableReference
			LocalTemporary value;
			if (HaveUnwrap) {
				value = new LocalTemporary (SwitchType);
				unwrap.EmitCheck (ec);
				ec.Emit (OpCodes.Brfalse, null_target);
				new_expr.Emit (ec);
				value.Store (ec);
			} else if (!is_constant) {
				value = new LocalTemporary (SwitchType);
				new_expr.Emit (ec);
				value.Store (ec);
			} else
				value = null;

			//
			// Setup the codegen context
			//
			Label old_end = ec.LoopEnd;
			Switch old_switch = ec.Switch;
			
			ec.LoopEnd = ec.DefineLabel ();
			ec.Switch = this;

			// Emit Code.
			if (is_constant) {
				if (constant_section != null)
					constant_section.Block.Emit (ec);
			} else if (string_dictionary != null) {
				DoEmitStringSwitch (value, ec);
			} else {
				TableSwitchEmit (ec, value);
			}

			if (value != null)
				value.Release (ec);

			// Restore context state. 
			ec.MarkLabel (ec.LoopEnd);

			//
			// Restore the previous context
			//
			ec.LoopEnd = old_end;
			ec.Switch = old_switch;
		}

		protected override void CloneTo (CloneContext clonectx, Statement t)
		{
			Switch target = (Switch) t;

			target.Expr = Expr.Clone (clonectx);
			target.Sections = new List<SwitchSection> ();
			foreach (SwitchSection ss in Sections){
				target.Sections.Add (ss.Clone (clonectx));
			}
		}
	}

	// A place where execution can restart in an iterator
	public abstract class ResumableStatement : Statement
	{
		bool prepared;
		protected Label resume_point;

		public Label PrepareForEmit (EmitContext ec)
		{
			if (!prepared) {
				prepared = true;
				resume_point = ec.DefineLabel ();
			}
			return resume_point;
		}

		public virtual Label PrepareForDispose (EmitContext ec, Label end)
		{
			return end;
		}
		public virtual void EmitForDispose (EmitContext ec, Iterator iterator, Label end, bool have_dispatcher)
		{
		}
	}

	// Base class for statements that are implemented in terms of try...finally
	public abstract class ExceptionStatement : ResumableStatement
	{
		bool code_follows;
		Iterator iter;
		List<ResumableStatement> resume_points;
		int first_resume_pc;
		protected Statement stmt;
		Label dispose_try_block;
		bool prepared_for_dispose, emitted_dispose;

		protected ExceptionStatement (Statement stmt, Location loc)
		{
			this.stmt = stmt;
			this.loc = loc;
		}

		#region Properties

		public Statement Statement {
			get {
				return stmt;
			}
		}

		#endregion

		protected abstract void EmitPreTryBody (EmitContext ec);
		protected abstract void EmitTryBody (EmitContext ec);
		protected abstract void EmitFinallyBody (EmitContext ec);

		protected sealed override void DoEmit (EmitContext ec)
		{
			EmitPreTryBody (ec);

			if (resume_points != null) {
				ec.EmitInt ((int) Iterator.State.Running);
				ec.Emit (OpCodes.Stloc, iter.CurrentPC);
			}

			ec.BeginExceptionBlock ();

			if (resume_points != null) {
				ec.MarkLabel (resume_point);

				// For normal control flow, we want to fall-through the Switch
				// So, we use CurrentPC rather than the $PC field, and initialize it to an outside value above
				ec.Emit (OpCodes.Ldloc, iter.CurrentPC);
				ec.EmitInt (first_resume_pc);
				ec.Emit (OpCodes.Sub);

				Label [] labels = new Label [resume_points.Count];
				for (int i = 0; i < resume_points.Count; ++i)
					labels [i] = resume_points [i].PrepareForEmit (ec);
				ec.Emit (OpCodes.Switch, labels);
			}

			EmitTryBody (ec);

			ec.BeginFinallyBlock ();

			Label start_finally = ec.DefineLabel ();
			if (resume_points != null) {
				ec.Emit (OpCodes.Ldloc, iter.SkipFinally);
				ec.Emit (OpCodes.Brfalse_S, start_finally);
				ec.Emit (OpCodes.Endfinally);
			}

			ec.MarkLabel (start_finally);
			EmitFinallyBody (ec);

			ec.EndExceptionBlock ();
		}

		public void SomeCodeFollows ()
		{
			code_follows = true;
		}

		public override bool Resolve (BlockContext ec)
		{
			// System.Reflection.Emit automatically emits a 'leave' at the end of a try clause
			// So, ensure there's some IL code after this statement.
			if (!code_follows && resume_points == null && ec.CurrentBranching.CurrentUsageVector.IsUnreachable)
				ec.NeedReturnLabel ();

			iter = ec.CurrentIterator;
			return true;
		}

		public void AddResumePoint (ResumableStatement stmt, int pc)
		{
			if (resume_points == null) {
				resume_points = new List<ResumableStatement> ();
				first_resume_pc = pc;
			}

			if (pc != first_resume_pc + resume_points.Count)
				throw new InternalErrorException ("missed an intervening AddResumePoint?");

			resume_points.Add (stmt);
		}

		public override Label PrepareForDispose (EmitContext ec, Label end)
		{
			if (!prepared_for_dispose) {
				prepared_for_dispose = true;
				dispose_try_block = ec.DefineLabel ();
			}
			return dispose_try_block;
		}

		public override void EmitForDispose (EmitContext ec, Iterator iterator, Label end, bool have_dispatcher)
		{
			if (emitted_dispose)
				return;

			emitted_dispose = true;

			Label end_of_try = ec.DefineLabel ();

			// Ensure that the only way we can get into this code is through a dispatcher
			if (have_dispatcher)
				ec.Emit (OpCodes.Br, end);

			ec.BeginExceptionBlock ();

			ec.MarkLabel (dispose_try_block);

			Label [] labels = null;
			for (int i = 0; i < resume_points.Count; ++i) {
				ResumableStatement s = (ResumableStatement) resume_points [i];
				Label ret = s.PrepareForDispose (ec, end_of_try);
				if (ret.Equals (end_of_try) && labels == null)
					continue;
				if (labels == null) {
					labels = new Label [resume_points.Count];
					for (int j = 0; j < i; ++j)
						labels [j] = end_of_try;
				}
				labels [i] = ret;
			}

			if (labels != null) {
				int j;
				for (j = 1; j < labels.Length; ++j)
					if (!labels [0].Equals (labels [j]))
						break;
				bool emit_dispatcher = j < labels.Length;

				if (emit_dispatcher) {
					//SymbolWriter.StartIteratorDispatcher (ec.ig);
					ec.Emit (OpCodes.Ldloc, iterator.CurrentPC);
					ec.EmitInt (first_resume_pc);
					ec.Emit (OpCodes.Sub);
					ec.Emit (OpCodes.Switch, labels);
					//SymbolWriter.EndIteratorDispatcher (ec.ig);
				}

				foreach (ResumableStatement s in resume_points)
					s.EmitForDispose (ec, iterator, end_of_try, emit_dispatcher);
			}

			ec.MarkLabel (end_of_try);

			ec.BeginFinallyBlock ();

			EmitFinallyBody (ec);

			ec.EndExceptionBlock ();
		}
	}

	public class Lock : ExceptionStatement {
		Expression expr;
		TemporaryVariableReference expr_copy;
		TemporaryVariableReference lock_taken;
			
		public Lock (Expression expr, Statement stmt, Location loc)
			: base (stmt, loc)
		{
			this.expr = expr;
		}

		public override bool Resolve (BlockContext ec)
		{
			expr = expr.Resolve (ec);
			if (expr == null)
				return false;

			if (!TypeManager.IsReferenceType (expr.Type)){
				ec.Report.Error (185, loc,
					"`{0}' is not a reference type as required by the lock statement",
					expr.Type.GetSignatureForError ());
			}

			if (expr.Type.IsGenericParameter) {
				expr = Convert.ImplicitTypeParameterConversion (expr, TypeManager.object_type);
			}

			VariableReference lv = expr as VariableReference;
			bool locked;
			if (lv != null) {
				locked = lv.IsLockedByStatement;
				lv.IsLockedByStatement = true;
			} else {
				lv = null;
				locked = false;
			}

			ec.StartFlowBranching (this);
			Statement.Resolve (ec);
			ec.EndFlowBranching ();

			if (lv != null) {
				lv.IsLockedByStatement = locked;
			}

			base.Resolve (ec);

			//
			// Have to keep original lock value around to unlock same location
			// in the case the original has changed or is null
			//
			expr_copy = TemporaryVariableReference.Create (TypeManager.object_type, ec.CurrentBlock.Parent, loc);
			expr_copy.Resolve (ec);

			//
			// Ensure Monitor methods are available
			//
			if (ResolvePredefinedMethods (ec) > 1) {
				lock_taken = TemporaryVariableReference.Create (TypeManager.bool_type, ec.CurrentBlock.Parent, loc);
				lock_taken.Resolve (ec);
			}

			return true;
		}
		
		protected override void EmitPreTryBody (EmitContext ec)
		{
			expr_copy.EmitAssign (ec, expr);

			if (lock_taken != null) {
				//
				// Initialize ref variable
				//
				lock_taken.EmitAssign (ec, new BoolLiteral (false, loc));
			} else {
				//
				// Monitor.Enter (expr_copy)
				//
				expr_copy.Emit (ec);
				ec.Emit (OpCodes.Call, TypeManager.void_monitor_enter_object);
			}
		}

		protected override void EmitTryBody (EmitContext ec)
		{
			//
			// Monitor.Enter (expr_copy, ref lock_taken)
			//
			if (lock_taken != null) {
				expr_copy.Emit (ec);
				lock_taken.LocalInfo.CreateBuilder (ec);
				lock_taken.AddressOf (ec, AddressOp.Load);
				ec.Emit (OpCodes.Call, TypeManager.void_monitor_enter_object);
			}

			Statement.Emit (ec);
		}

		protected override void EmitFinallyBody (EmitContext ec)
		{
			//
			// if (lock_taken) Monitor.Exit (expr_copy)
			//
			Label skip = ec.DefineLabel ();

			if (lock_taken != null) {
				lock_taken.Emit (ec);
				ec.Emit (OpCodes.Brfalse_S, skip);
			}

			expr_copy.Emit (ec);
			ec.Emit (OpCodes.Call, TypeManager.void_monitor_exit_object);
			ec.MarkLabel (skip);
		}

		int ResolvePredefinedMethods (ResolveContext rc)
		{
			if (TypeManager.void_monitor_enter_object == null || TypeManager.void_monitor_exit_object == null) {
				TypeSpec monitor_type = TypeManager.CoreLookupType (rc.Compiler, "System.Threading", "Monitor", MemberKind.Class, true);

				if (monitor_type == null)
					return 0;

				// Try 4.0 Monitor.Enter (object, ref bool) overload first
				var filter = MemberFilter.Method ("Enter", 0, new ParametersImported (
					new[] {
							new ParameterData (null, Parameter.Modifier.NONE),
							new ParameterData (null, Parameter.Modifier.REF)
						},
					new[] {
							TypeManager.object_type,
							TypeManager.bool_type
						}, false), null);

				TypeManager.void_monitor_enter_object = TypeManager.GetPredefinedMethod (monitor_type, filter, true, loc);
				if (TypeManager.void_monitor_enter_object == null) {
					TypeManager.void_monitor_enter_object = TypeManager.GetPredefinedMethod (
						monitor_type, "Enter", loc, TypeManager.object_type);
				}

				TypeManager.void_monitor_exit_object = TypeManager.GetPredefinedMethod (
					monitor_type, "Exit", loc, TypeManager.object_type);
			}

			return TypeManager.void_monitor_enter_object.Parameters.Count;
		}

		protected override void CloneTo (CloneContext clonectx, Statement t)
		{
			Lock target = (Lock) t;

			target.expr = expr.Clone (clonectx);
			target.stmt = Statement.Clone (clonectx);
		}
	}

	public class Unchecked : Statement {
		public Block Block;
		
		public Unchecked (Block b, Location loc)
		{
			Block = b;
			b.Unchecked = true;
			this.loc = loc;
		}

		public override bool Resolve (BlockContext ec)
		{
			using (ec.With (ResolveContext.Options.AllCheckStateFlags, false))
				return Block.Resolve (ec);
		}
		
		protected override void DoEmit (EmitContext ec)
		{
			using (ec.With (EmitContext.Options.AllCheckStateFlags, false))
				Block.Emit (ec);
		}

		protected override void CloneTo (CloneContext clonectx, Statement t)
		{
			Unchecked target = (Unchecked) t;

			target.Block = clonectx.LookupBlock (Block);
		}
	}

	public class Checked : Statement {
		public Block Block;
		
		public Checked (Block b, Location loc)
		{
			Block = b;
			b.Unchecked = false;
			this.loc = loc;
		}

		public override bool Resolve (BlockContext ec)
		{
			using (ec.With (ResolveContext.Options.AllCheckStateFlags, true))
				return Block.Resolve (ec);
		}

		protected override void DoEmit (EmitContext ec)
		{
			using (ec.With (EmitContext.Options.AllCheckStateFlags, true))
				Block.Emit (ec);
		}

		protected override void CloneTo (CloneContext clonectx, Statement t)
		{
			Checked target = (Checked) t;

			target.Block = clonectx.LookupBlock (Block);
		}
	}

	public class Unsafe : Statement {
		public Block Block;

		public Unsafe (Block b, Location loc)
		{
			Block = b;
			Block.Unsafe = true;
			this.loc = loc;
		}

		public override bool Resolve (BlockContext ec)
		{
			if (ec.CurrentIterator != null)
				ec.Report.Error (1629, loc, "Unsafe code may not appear in iterators");

			using (ec.Set (ResolveContext.Options.UnsafeScope))
				return Block.Resolve (ec);
		}
		
		protected override void DoEmit (EmitContext ec)
		{
			Block.Emit (ec);
		}

		protected override void CloneTo (CloneContext clonectx, Statement t)
		{
			Unsafe target = (Unsafe) t;

			target.Block = clonectx.LookupBlock (Block);
		}
	}

	// 
	// Fixed statement
	//
	public class Fixed : Statement
	{
		abstract class Emitter : ShimExpression
		{
			protected LocalVariable vi;

			protected Emitter (Expression expr, LocalVariable li)
				: base (expr)
			{
				vi = li;
			}

			public abstract void EmitExit (EmitContext ec);
		}

		class ExpressionEmitter : Emitter {
			public ExpressionEmitter (Expression converted, LocalVariable li) :
				base (converted, li)
			{
			}

			protected override Expression DoResolve (ResolveContext rc)
			{
				throw new NotImplementedException ();
			}

			public override void Emit (EmitContext ec) {
				//
				// Store pointer in pinned location
				//
				expr.Emit (ec);
				vi.EmitAssign (ec);
			}

			public override void EmitExit (EmitContext ec)
			{
				ec.Emit (OpCodes.Ldc_I4_0);
				ec.Emit (OpCodes.Conv_U);
				vi.EmitAssign (ec);
			}
		}

		class StringEmitter : Emitter
		{
			LocalVariable pinned_string;

			public StringEmitter (Expression expr, LocalVariable li, Location loc)
				: base (expr, li)
			{
			}

			protected override Expression DoResolve (ResolveContext rc)
			{
				pinned_string = new LocalVariable (vi.Block, "$pinned",
					LocalVariable.Flags.FixedVariable | LocalVariable.Flags.CompilerGenerated | LocalVariable.Flags.Used,
					vi.Location);

				pinned_string.Type = TypeManager.string_type;

				if (TypeManager.int_get_offset_to_string_data == null) {
					TypeManager.int_get_offset_to_string_data = TypeManager.GetPredefinedProperty (
						TypeManager.runtime_helpers_type, "OffsetToStringData", pinned_string.Location, TypeManager.int32_type);
				}

				eclass = ExprClass.Variable;
				type = TypeManager.int32_type;
				return this;
			}

			public override void Emit (EmitContext ec)
			{
				pinned_string.CreateBuilder (ec);

				expr.Emit (ec);
				pinned_string.EmitAssign (ec);

				// TODO: Should use Binary::Add
				pinned_string.Emit (ec);
				ec.Emit (OpCodes.Conv_I);

				PropertyExpr pe = new PropertyExpr (TypeManager.int_get_offset_to_string_data, pinned_string.Location);
				//pe.InstanceExpression = pinned_string;
				pe.Resolve (new ResolveContext (ec.MemberContext)).Emit (ec);

				ec.Emit (OpCodes.Add);
				vi.EmitAssign (ec);
			}

			public override void EmitExit (EmitContext ec)
			{
				ec.Emit (OpCodes.Ldnull);
				pinned_string.EmitAssign (ec);
			}
		}

		public class VariableDeclaration : BlockVariableDeclaration
		{
			public VariableDeclaration (FullNamedExpression type, LocalVariable li)
				: base (type, li)
			{
			}

			protected override Expression ResolveInitializer (BlockContext bc, LocalVariable li, Expression initializer)
			{
				if (!Variable.Type.IsPointer && li == Variable) {
					bc.Report.Error (209, TypeExpression.Location,
						"The type of locals declared in a fixed statement must be a pointer type");
					return null;
				}

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

				if (initializer is Cast) {
					bc.Report.Error (254, initializer.Location, "The right hand side of a fixed statement assignment may not be a cast expression");
					return null;
				}

				initializer = initializer.Resolve (bc);

				if (initializer == null)
					return null;

				//
				// Case 1: Array
				//
				if (initializer.Type.IsArray) {
					TypeSpec array_type = TypeManager.GetElementType (initializer.Type);

					//
					// Provided that array_type is unmanaged,
					//
					if (!TypeManager.VerifyUnmanaged (bc.Compiler, array_type, loc))
						return null;

					//
					// and T* is implicitly convertible to the
					// pointer type given in the fixed statement.
					//
					ArrayPtr array_ptr = new ArrayPtr (initializer, array_type, loc);

					Expression converted = Convert.ImplicitConversionRequired (
						bc, array_ptr, li.Type, loc);
					if (converted == null)
						return null;

					//
					// fixed (T* e_ptr = (e == null || e.Length == 0) ? null : converted [0])
					//
					converted = new Conditional (new BooleanExpression (new Binary (Binary.Operator.LogicalOr,
						new Binary (Binary.Operator.Equality, initializer, new NullLiteral (loc), loc),
						new Binary (Binary.Operator.Equality, new MemberAccess (initializer, "Length"), new IntConstant (0, loc), loc), loc)),
							new NullPointer (loc),
							converted, loc);

					converted = converted.Resolve (bc);

					return new ExpressionEmitter (converted, li);
				}

				//
				// Case 2: string
				//
				if (initializer.Type == TypeManager.string_type) {
					return new StringEmitter (initializer, li, loc).Resolve (bc);
				}

				// Case 3: fixed buffer
				if (initializer is FixedBufferPtr) {
					return new ExpressionEmitter (initializer, li);
				}

				//
				// Case 4: & object.
				//
				bool already_fixed = true;
				Unary u = initializer as Unary;
				if (u != null && u.Oper == Unary.Operator.AddressOf) {
					IVariableReference vr = u.Expr as IVariableReference;
					if (vr == null || !vr.IsFixed) {
						already_fixed = false;
					}
				}

				if (already_fixed) {
					bc.Report.Error (213, loc, "You cannot use the fixed statement to take the address of an already fixed expression");
				}

				initializer = Convert.ImplicitConversionRequired (bc, initializer, li.Type, loc);
				return new ExpressionEmitter (initializer, li);
			}
		}


		VariableDeclaration decl;
		Statement statement;
		bool has_ret;

		public Fixed (VariableDeclaration decl, Statement stmt, Location l)
		{
			this.decl = decl;
			statement = stmt;
			loc = l;
		}

		#region Properties

		public Statement Statement {
			get {
				return statement;
			}
		}

		public BlockVariableDeclaration Variables {
			get {
				return decl;
			}
		}

		#endregion

		public override bool Resolve (BlockContext ec)
		{
			using (ec.Set (ResolveContext.Options.FixedInitializerScope)) {
				if (!decl.Resolve (ec))
					return false;
			}

			ec.StartFlowBranching (FlowBranching.BranchingType.Conditional, loc);
			bool ok = statement.Resolve (ec);
			bool flow_unreachable = ec.EndFlowBranching ();
			has_ret = flow_unreachable;

			return ok;
		}
		
		protected override void DoEmit (EmitContext ec)
		{
			decl.Variable.CreateBuilder (ec);
			decl.Initializer.Emit (ec);
			if (decl.Declarators != null) {
				foreach (var d in decl.Declarators) {
					d.Variable.CreateBuilder (ec);
					d.Initializer.Emit (ec);
				}
			}

			statement.Emit (ec);

			if (has_ret)
				return;

			//
			// Clear the pinned variable
			//
			((Emitter) decl.Initializer).EmitExit (ec);
			if (decl.Declarators != null) {
				foreach (var d in decl.Declarators) {
					((Emitter)d.Initializer).EmitExit (ec);
				}
			}
		}

		protected override void CloneTo (CloneContext clonectx, Statement t)
		{
			Fixed target = (Fixed) t;

			target.decl = (VariableDeclaration) decl.Clone (clonectx);
			target.statement = statement.Clone (clonectx);
		}
	}

	public class Catch : Statement
	{
		Block block;
		LocalVariable li;
		FullNamedExpression type_expr;
		CompilerAssign assign;
		TypeSpec type;
		
		public Catch (Block block, Location loc)
		{
			this.block = block;
			this.loc = loc;
		}

		#region Properties

		public Block Block {
			get {
				return block;
			}
		}

		public TypeSpec CatchType {
			get {
				return type;
			}
		}

		public bool IsGeneral {
			get {
				return type_expr == null;
			}
		}

		public FullNamedExpression TypeExpression {
			get {
				return type_expr;
			}
			set {
				type_expr = value;
			}
		}

		public LocalVariable Variable {
			get {
				return li;
			}
			set {
				li = value;
			}
		}

		#endregion

		protected override void DoEmit (EmitContext ec)
		{
			if (IsGeneral)
				ec.BeginCatchBlock (TypeManager.object_type);
			else
				ec.BeginCatchBlock (CatchType);

			if (li != null) {
				li.CreateBuilder (ec);

				//
				// Special case hoisted catch variable, we have to use a temporary variable
				// to pass via anonymous storey initialization with the value still on top
				// of the stack
				//
				if (li.HoistedVariant != null) {
					LocalTemporary lt = new LocalTemporary (li.Type);
					SymbolWriter.OpenCompilerGeneratedBlock (ec);
					lt.Store (ec);
					SymbolWriter.CloseCompilerGeneratedBlock (ec);

					// switch to assigning from the temporary variable and not from top of the stack
					assign.UpdateSource (lt);
				}
			} else {
				SymbolWriter.OpenCompilerGeneratedBlock (ec);
				ec.Emit (OpCodes.Pop);
				SymbolWriter.CloseCompilerGeneratedBlock (ec);
			}

			Block.Emit (ec);
		}

		public override bool Resolve (BlockContext ec)
		{
			using (ec.With (ResolveContext.Options.CatchScope, true)) {
				if (type_expr != null) {
					TypeExpr te = type_expr.ResolveAsTypeTerminal (ec, false);
					if (te == null)
						return false;

					type = te.Type;
					if (type != TypeManager.exception_type && !TypeSpec.IsBaseClass (type, TypeManager.exception_type, false)) {
						ec.Report.Error (155, loc, "The type caught or thrown must be derived from System.Exception");
					} else if (li != null) {
						li.Type = type;
						li.PrepareForFlowAnalysis (ec);

						// source variable is at the top of the stack
						Expression source = new EmptyExpression (li.Type);
						if (li.Type.IsGenericParameter)
							source = new UnboxCast (source, li.Type);

						assign = new CompilerAssign (new LocalVariableReference (li, loc), source, loc);
						Block.AddScopeStatement (new StatementExpression (assign));
					}
				}

				return Block.Resolve (ec);
			}
		}

		protected override void CloneTo (CloneContext clonectx, Statement t)
		{
			Catch target = (Catch) t;

			if (type_expr != null)
				target.type_expr = (FullNamedExpression) type_expr.Clone (clonectx);

			target.block = clonectx.LookupBlock (block);
		}
	}

	public class TryFinally : ExceptionStatement {
		Block fini;

		public TryFinally (Statement stmt, Block fini, Location loc)
			 : base (stmt, loc)
		{
			this.fini = fini;
		}

		public override bool Resolve (BlockContext ec)
		{
			bool ok = true;

			ec.StartFlowBranching (this);

			if (!stmt.Resolve (ec))
				ok = false;

			if (ok)
				ec.CurrentBranching.CreateSibling (fini, FlowBranching.SiblingType.Finally);
			using (ec.With (ResolveContext.Options.FinallyScope, true)) {
				if (!fini.Resolve (ec))
					ok = false;
			}

			ec.EndFlowBranching ();

			ok &= base.Resolve (ec);

			return ok;
		}

		protected override void EmitPreTryBody (EmitContext ec)
		{
		}

		protected override void EmitTryBody (EmitContext ec)
		{
			stmt.Emit (ec);
		}

		protected override void EmitFinallyBody (EmitContext ec)
		{
			fini.Emit (ec);
		}

		protected override void CloneTo (CloneContext clonectx, Statement t)
		{
			TryFinally target = (TryFinally) t;

			target.stmt = (Statement) stmt.Clone (clonectx);
			if (fini != null)
				target.fini = clonectx.LookupBlock (fini);
		}
	}

	public class TryCatch : Statement {
		public Block Block;
		public List<Catch> Specific;
		public Catch General;
		bool inside_try_finally, code_follows;

		public TryCatch (Block block, List<Catch> catch_clauses, Location l, bool inside_try_finally)
		{
			this.Block = block;
			this.Specific = catch_clauses;
			this.inside_try_finally = inside_try_finally;

			Catch c = catch_clauses [0];
			if (c.IsGeneral) {
				this.General = c;			
				catch_clauses.RemoveAt (0);
			}

			loc = l;
		}

		public override bool Resolve (BlockContext ec)
		{
			bool ok = true;

			ec.StartFlowBranching (this);

			if (!Block.Resolve (ec))
				ok = false;

			TypeSpec[] prev_catches = new TypeSpec [Specific.Count];
			int last_index = 0;
			foreach (Catch c in Specific){
				ec.CurrentBranching.CreateSibling (c.Block, FlowBranching.SiblingType.Catch);

				if (!c.Resolve (ec)) {
					ok = false;
					continue;
				}

				TypeSpec resolved_type = c.CatchType;
				for (int ii = 0; ii < last_index; ++ii) {
					if (resolved_type == prev_catches[ii] || TypeSpec.IsBaseClass (resolved_type, prev_catches[ii], true)) {
						ec.Report.Error (160, c.loc,
							"A previous catch clause already catches all exceptions of this or a super type `{0}'",
							TypeManager.CSharpName (prev_catches [ii]));
						ok = false;
					}
				}

				prev_catches [last_index++] = resolved_type;
			}

			if (General != null) {
				if (CodeGen.Assembly.WrapNonExceptionThrows) {
					foreach (Catch c in Specific){
						if (c.CatchType == TypeManager.exception_type && ec.Compiler.PredefinedAttributes.RuntimeCompatibility.IsDefined) {
							ec.Report.Warning (1058, 1, c.loc,
								"A previous catch clause already catches all exceptions. All non-exceptions thrown will be wrapped in a `System.Runtime.CompilerServices.RuntimeWrappedException'");
						}
					}
				}

				ec.CurrentBranching.CreateSibling (General.Block, FlowBranching.SiblingType.Catch);

				if (!General.Resolve (ec))
					ok = false;
			}

			ec.EndFlowBranching ();

			// System.Reflection.Emit automatically emits a 'leave' at the end of a try/catch clause
			// So, ensure there's some IL code after this statement
			if (!inside_try_finally && !code_follows && ec.CurrentBranching.CurrentUsageVector.IsUnreachable)
				ec.NeedReturnLabel ();

			return ok;
		}

		public void SomeCodeFollows ()
		{
			code_follows = true;
		}
		
		protected override void DoEmit (EmitContext ec)
		{
			if (!inside_try_finally)
				ec.BeginExceptionBlock ();

			Block.Emit (ec);

			foreach (Catch c in Specific)
				c.Emit (ec);

			if (General != null)
				General.Emit (ec);

			if (!inside_try_finally)
				ec.EndExceptionBlock ();
		}

		protected override void CloneTo (CloneContext clonectx, Statement t)
		{
			TryCatch target = (TryCatch) t;

			target.Block = clonectx.LookupBlock (Block);
			if (General != null)
				target.General = (Catch) General.Clone (clonectx);
			if (Specific != null){
				target.Specific = new List<Catch> ();
				foreach (Catch c in Specific)
					target.Specific.Add ((Catch) c.Clone (clonectx));
			}
		}
	}

	public class Using : ExceptionStatement
	{
		public class VariableDeclaration : BlockVariableDeclaration
		{
			Statement dispose_call;

			public VariableDeclaration (FullNamedExpression type, LocalVariable li)
				: base (type, li)
			{
			}

			public VariableDeclaration (LocalVariable li, Location loc)
				: base (li)
			{
				this.loc = loc;
			}

			public VariableDeclaration (Expression expr)
				: base (null)
			{
				loc = expr.Location;
				Initializer = expr;
			}

			#region Properties

			public bool IsNested { get; private set; }

			#endregion

			public void EmitDispose (EmitContext ec)
			{
				dispose_call.Emit (ec);
			}

			public override bool Resolve (BlockContext bc)
			{
				if (IsNested)
					return true;

				return base.Resolve (bc);
			}

			public Expression ResolveExpression (BlockContext bc)
			{
				var e = Initializer.Resolve (bc);
				if (e == null)
					return null;

				li = LocalVariable.CreateCompilerGenerated (e.Type, bc.CurrentBlock, loc);
				Initializer = ResolveInitializer (bc, Variable, e);
				return e;
			}

			protected override Expression ResolveInitializer (BlockContext bc, LocalVariable li, Expression initializer)
			{
				if (li.Type == InternalType.Dynamic) {
					initializer = initializer.Resolve (bc);
					if (initializer == null)
						return null;

					// Once there is dynamic used defer conversion to runtime even if we know it will never succeed
					Arguments args = new Arguments (1);
					args.Add (new Argument (initializer));
					initializer = new DynamicConversion (TypeManager.idisposable_type, 0, args, initializer.Location).Resolve (bc);
					if (initializer == null)
						return null;

					var var = LocalVariable.CreateCompilerGenerated (TypeManager.idisposable_type, bc.CurrentBlock, loc);
					dispose_call = CreateDisposeCall (bc, var);
					dispose_call.Resolve (bc);

					return base.ResolveInitializer (bc, li, new SimpleAssign (var.CreateReferenceExpression (bc, loc), initializer, loc));
				}

				if (li == Variable) {
					CheckIDiposableConversion (bc, li, initializer);
					dispose_call = CreateDisposeCall (bc, li);
					dispose_call.Resolve (bc);
				}

				return base.ResolveInitializer (bc, li, initializer);
			}

			protected virtual void CheckIDiposableConversion (BlockContext bc, LocalVariable li, Expression initializer)
			{
				var type = li.Type;

				if (type != TypeManager.idisposable_type && !type.ImplementsInterface (TypeManager.idisposable_type, false)) {
					if (TypeManager.IsNullableType (type)) {
						// it's handled in CreateDisposeCall
						return;
					}

					bc.Report.SymbolRelatedToPreviousError (type);
					var loc = type_expr == null ? initializer.Location : type_expr.Location;
					bc.Report.Error (1674, loc, "`{0}': type used in a using statement must be implicitly convertible to `System.IDisposable'",
						type.GetSignatureForError ());

					return;
				}
			}

			protected virtual Statement CreateDisposeCall (BlockContext bc, LocalVariable lv)
			{
				var lvr = lv.CreateReferenceExpression (bc, lv.Location);
				var type = lv.Type;
				var loc = lv.Location;

				if (TypeManager.void_dispose_void == null) {
					TypeManager.void_dispose_void = TypeManager.GetPredefinedMethod (
						TypeManager.idisposable_type, "Dispose", loc, TypeSpec.EmptyTypes);
				}

				var dispose_mg = MethodGroupExpr.CreatePredefined (TypeManager.void_dispose_void, TypeManager.idisposable_type, loc);
				dispose_mg.InstanceExpression = TypeManager.IsNullableType (type) ?
					new Cast (new TypeExpression (TypeManager.idisposable_type, loc), lvr, loc).Resolve (bc) :
					lvr;

				Statement dispose = new StatementExpression (new Invocation (dispose_mg, null));

				// Add conditional call when disposing possible null variable
				if (!type.IsStruct || TypeManager.IsNullableType (type))
					dispose = new If (new Binary (Binary.Operator.Inequality, lvr, new NullLiteral (loc), loc), dispose, loc);

				return dispose;
			}

			public Statement RewriteForDeclarators (BlockContext bc, Statement stmt)
			{
				for (int i = declarators.Count - 1; i >= 0; --i) {
					var d = declarators [i];
					var vd = new VariableDeclaration (d.Variable, type_expr.Location);
					vd.Initializer = d.Initializer;
					vd.IsNested = true;
					vd.dispose_call = CreateDisposeCall (bc, d.Variable);
					vd.dispose_call.Resolve (bc);

					stmt = new Using (vd, stmt, d.Variable.Location);
				}

				declarators = null;
				return stmt;
			}
		}

		VariableDeclaration decl;

		public Using (VariableDeclaration decl, Statement stmt, Location loc)
			: base (stmt, loc)
		{
			this.decl = decl;
		}

		public Using (Expression expr, Statement stmt, Location loc)
			: base (stmt, loc)
		{
			this.decl = new VariableDeclaration (expr);
		}

		#region Properties

		public Expression Expression {
			get {
				return decl.Variable == null ? decl.Initializer : null;
			}
		}

		public BlockVariableDeclaration Variables {
			get {
				return decl;
			}
		}

		#endregion

		protected override void EmitPreTryBody (EmitContext ec)
		{
			decl.Emit (ec);
		}

		protected override void EmitTryBody (EmitContext ec)
		{
			stmt.Emit (ec);
		}

		protected override void EmitFinallyBody (EmitContext ec)
		{
			decl.EmitDispose (ec);
		}

		public override bool Resolve (BlockContext ec)
		{
			VariableReference vr;
			bool vr_locked = false;

			using (ec.Set (ResolveContext.Options.UsingInitializerScope)) {
				if (decl.Variable == null) {
					vr = decl.ResolveExpression (ec) as VariableReference;
					if (vr != null) {
						vr_locked = vr.IsLockedByStatement;
						vr.IsLockedByStatement = true;
					}
				} else {
					if (!decl.Resolve (ec))
						return false;

					if (decl.Declarators != null) {
						stmt = decl.RewriteForDeclarators (ec, stmt);
					}

					vr = null;
				}
			}

			ec.StartFlowBranching (this);

			stmt.Resolve (ec);

			ec.EndFlowBranching ();

			if (vr != null)
				vr.IsLockedByStatement = vr_locked;

			base.Resolve (ec);

			return true;
		}

		protected override void CloneTo (CloneContext clonectx, Statement t)
		{
			Using target = (Using) t;

			target.decl = (VariableDeclaration) decl.Clone (clonectx);
			target.stmt = stmt.Clone (clonectx);
		}
	}

	/// <summary>
	///   Implementation of the foreach C# statement
	/// </summary>
	public class Foreach : Statement {

		sealed class ArrayForeach : Statement
		{
			readonly Foreach for_each;
			readonly Statement statement;

			Expression conv;
			TemporaryVariableReference[] lengths;
			Expression [] length_exprs;
			StatementExpression[] counter;
			TemporaryVariableReference[] variables;

			TemporaryVariableReference copy;
			Expression access;
			LocalVariableReference variable;

			public ArrayForeach (Foreach @foreach, int rank)
			{
				for_each = @foreach;
				statement = for_each.statement;
				loc = @foreach.loc;
				variable = new LocalVariableReference (for_each.variable, loc);

				counter = new StatementExpression[rank];
				variables = new TemporaryVariableReference[rank];
				length_exprs = new Expression [rank];

				//
				// Only use temporary length variables when dealing with
				// multi-dimensional arrays
				//
				if (rank > 1)
					lengths = new TemporaryVariableReference [rank];
			}

			protected override void CloneTo (CloneContext clonectx, Statement target)
			{
				throw new NotImplementedException ();
			}

			public override bool Resolve (BlockContext ec)
			{
				Block variables_block = variable.local_info.Block;
				copy = TemporaryVariableReference.Create (for_each.expr.Type, variables_block, loc);
				copy.Resolve (ec);

				int rank = length_exprs.Length;
				Arguments list = new Arguments (rank);
				for (int i = 0; i < rank; i++) {
					var v = TemporaryVariableReference.Create (TypeManager.int32_type, variables_block, loc);
					variables[i] = v;
					counter[i] = new StatementExpression (new UnaryMutator (UnaryMutator.Mode.PostIncrement, v, loc));
					counter[i].Resolve (ec);

					if (rank == 1) {
						length_exprs [i] = new MemberAccess (copy, "Length").Resolve (ec);
					} else {
						lengths[i] = TemporaryVariableReference.Create (TypeManager.int32_type, variables_block, loc);
						lengths[i].Resolve (ec);

						Arguments args = new Arguments (1);
						args.Add (new Argument (new IntConstant (i, loc)));
						length_exprs [i] = new Invocation (new MemberAccess (copy, "GetLength"), args).Resolve (ec);
					}

					list.Add (new Argument (v));
				}

				access = new ElementAccess (copy, list, loc).Resolve (ec);
				if (access == null)
					return false;

				Expression var_type = for_each.type;
				VarExpr ve = var_type as VarExpr;
				if (ve != null) {
					// Infer implicitly typed local variable from foreach array type
					var_type = new TypeExpression (access.Type, ve.Location);
				}

				var_type = var_type.ResolveAsTypeTerminal (ec, false);
				if (var_type == null)
					return false;

				conv = Convert.ExplicitConversion (ec, access, var_type.Type, loc);
				if (conv == null)
					return false;

				bool ok = true;

				ec.StartFlowBranching (FlowBranching.BranchingType.Loop, loc);
				ec.CurrentBranching.CreateSibling ();

				variable.local_info.Type = conv.Type;
				variable.Resolve (ec);

				ec.StartFlowBranching (FlowBranching.BranchingType.Embedded, loc);
				if (!statement.Resolve (ec))
					ok = false;
				ec.EndFlowBranching ();

				// There's no direct control flow from the end of the embedded statement to the end of the loop
				ec.CurrentBranching.CurrentUsageVector.Goto ();

				ec.EndFlowBranching ();

				return ok;
			}

			protected override void DoEmit (EmitContext ec)
			{
				copy.EmitAssign (ec, for_each.expr);

				int rank = length_exprs.Length;
				Label[] test = new Label [rank];
				Label[] loop = new Label [rank];

				for (int i = 0; i < rank; i++) {
					test [i] = ec.DefineLabel ();
					loop [i] = ec.DefineLabel ();

					if (lengths != null)
						lengths [i].EmitAssign (ec, length_exprs [i]);
				}

				IntConstant zero = new IntConstant (0, loc);
				for (int i = 0; i < rank; i++) {
					variables [i].EmitAssign (ec, zero);

					ec.Emit (OpCodes.Br, test [i]);
					ec.MarkLabel (loop [i]);
				}

				variable.local_info.CreateBuilder (ec);
				variable.EmitAssign (ec, conv, false, false);

				statement.Emit (ec);

				ec.MarkLabel (ec.LoopBegin);

				for (int i = rank - 1; i >= 0; i--){
					counter [i].Emit (ec);

					ec.MarkLabel (test [i]);
					variables [i].Emit (ec);

					if (lengths != null)
						lengths [i].Emit (ec);
					else
						length_exprs [i].Emit (ec);

					ec.Emit (OpCodes.Blt, loop [i]);
				}

				ec.MarkLabel (ec.LoopEnd);
			}
		}

		sealed class CollectionForeach : Statement, OverloadResolver.IErrorHandler
		{
			class Body : Statement
			{
				TypeSpec type;
				LocalVariableReference variable;
				Expression current, conv;
				Statement statement;

				public Body (TypeSpec type, LocalVariable variable,
								   Expression current, Statement statement,
								   Location loc)
				{
					this.type = type;
					this.variable = new LocalVariableReference (variable, loc);
					this.current = current;
					this.statement = statement;
					this.loc = loc;
				}

				protected override void CloneTo (CloneContext clonectx, Statement target)
				{
					throw new NotImplementedException ();
				}

				public override bool Resolve (BlockContext ec)
				{
					current = current.Resolve (ec);
					if (current == null)
						return false;

					conv = Convert.ExplicitConversion (ec, current, type, loc);
					if (conv == null)
						return false;

					variable.local_info.Type = conv.Type;
					variable.Resolve (ec);

					if (!statement.Resolve (ec))
						return false;

					return true;
				}

				protected override void DoEmit (EmitContext ec)
				{
					variable.local_info.CreateBuilder (ec);
					variable.EmitAssign (ec, conv, false, false);

					statement.Emit (ec);
				}
			}

			class RuntimeDispose : Using.VariableDeclaration
			{
				public RuntimeDispose (LocalVariable lv, Location loc)
					: base (lv, loc)
				{
				}

				protected override void CheckIDiposableConversion (BlockContext bc, LocalVariable li, Expression initializer)
				{
					// Defered to runtime check
				}

				protected override Statement CreateDisposeCall (BlockContext bc, LocalVariable lv)
				{
					if (TypeManager.void_dispose_void == null) {
						TypeManager.void_dispose_void = TypeManager.GetPredefinedMethod (
							TypeManager.idisposable_type, "Dispose", loc, TypeSpec.EmptyTypes);
					}

					//
					// Fabricates code like
					//
					// if ((temp = vr as IDisposable) != null) temp.Dispose ();
					//

					var dispose_variable = LocalVariable.CreateCompilerGenerated (TypeManager.idisposable_type, bc.CurrentBlock, loc);

					var idisaposable_test = new Binary (Binary.Operator.Inequality, new CompilerAssign (
						dispose_variable.CreateReferenceExpression (bc, loc),
						new As (lv.CreateReferenceExpression (bc, loc), new TypeExpression (dispose_variable.Type, loc), loc),
						loc), new NullLiteral (loc), loc);

					var dispose_mg = MethodGroupExpr.CreatePredefined (TypeManager.void_dispose_void, TypeManager.idisposable_type, loc);
					dispose_mg.InstanceExpression = dispose_variable.CreateReferenceExpression (bc, loc);

					Statement dispose = new StatementExpression (new Invocation (dispose_mg, null));
					return new If (idisaposable_test, dispose, loc);
				}
			}

			LocalVariable variable;
			Expression expr;
			Statement statement;
			Expression var_type;
			ExpressionStatement init;
			TemporaryVariableReference enumerator_variable;
			bool ambiguous_getenumerator_name;

			public CollectionForeach (Expression var_type, LocalVariable var, Expression expr, Statement stmt, Location l)
			{
				this.var_type = var_type;
				this.variable = var;
				this.expr = expr;
				statement = stmt;
				loc = l;
			}

			protected override void CloneTo (CloneContext clonectx, Statement target)
			{
				throw new NotImplementedException ();
			}

			void Error_WrongEnumerator (ResolveContext rc, MethodSpec enumerator)
			{
				rc.Report.SymbolRelatedToPreviousError (enumerator);
				rc.Report.Error (202, loc,
					"foreach statement requires that the return type `{0}' of `{1}' must have a suitable public MoveNext method and public Current property",
						enumerator.ReturnType.GetSignatureForError (), enumerator.GetSignatureForError ());
			}

			MethodGroupExpr ResolveGetEnumerator (ResolveContext rc)
			{
				//
				// Option 1: Try to match by name GetEnumerator first
				//
				var mexpr = Expression.MemberLookup (rc, rc.CurrentType, expr.Type,
					"GetEnumerator", 0, Expression.MemberLookupRestrictions.ExactArity, loc);		// TODO: What if CS0229 ?

				var mg = mexpr as MethodGroupExpr;
				if (mg != null) {
					mg.InstanceExpression = expr;
					Arguments args = new Arguments (0);
					mg = mg.OverloadResolve (rc, ref args, this, OverloadResolver.Restrictions.None);

					// For ambiguous GetEnumerator name warning CS0278 was reported, but Option 2 could still apply
					if (ambiguous_getenumerator_name)
						mg = null;

					if (mg != null && args.Count == 0 && !mg.BestCandidate.IsStatic && mg.BestCandidate.IsPublic) {
						return mg;
					}
				}

				//
				// Option 2: Try to match using IEnumerable interfaces with preference of generic version
				//
				TypeSpec iface_candidate = null;
				for (TypeSpec t = expr.Type; t != null && t != TypeManager.object_type; t = t.BaseType) {
					var ifaces = t.Interfaces;
					if (ifaces != null) {
						foreach (var iface in ifaces) {
							if (TypeManager.generic_ienumerable_type != null && iface.MemberDefinition == TypeManager.generic_ienumerable_type.MemberDefinition) {
								if (iface_candidate != null && iface_candidate != TypeManager.ienumerable_type) {
									rc.Report.SymbolRelatedToPreviousError (expr.Type);
									rc.Report.Error (1640, loc,
										"foreach statement cannot operate on variables of type `{0}' because it contains multiple implementation of `{1}'. Try casting to a specific implementation",
										expr.Type.GetSignatureForError (), TypeManager.generic_ienumerable_type.GetSignatureForError ());

									return null;
								}

								iface_candidate = iface;
								continue;
							}

							if (iface == TypeManager.ienumerable_type && iface_candidate == null) {
								iface_candidate = iface;
							}
						}
					}
				}

				if (iface_candidate == null) {
					rc.Report.Error (1579, loc,
						"foreach statement cannot operate on variables of type `{0}' because it does not contain a definition for `{1}' or is inaccessible",
						expr.Type.GetSignatureForError (), "GetEnumerator");

					return null;
				}

				var method = TypeManager.GetPredefinedMethod (iface_candidate, 
					MemberFilter.Method ("GetEnumerator", 0, ParametersCompiled.EmptyReadOnlyParameters, null), loc);

				if (method == null)
					return null;

				mg = MethodGroupExpr.CreatePredefined (method, expr.Type, loc);
				mg.InstanceExpression = expr;
				return mg;
			}

			MethodGroupExpr ResolveMoveNext (ResolveContext rc, MethodSpec enumerator)
			{
				var ms = MemberCache.FindMember (enumerator.ReturnType,
					MemberFilter.Method ("MoveNext", 0, ParametersCompiled.EmptyReadOnlyParameters, TypeManager.bool_type),
					BindingRestriction.InstanceOnly) as MethodSpec;

				if (ms == null || !ms.IsPublic) {
					Error_WrongEnumerator (rc, enumerator);
					return null;
				}

				return MethodGroupExpr.CreatePredefined (ms, enumerator.ReturnType, loc);
			}

			PropertySpec ResolveCurrent (ResolveContext rc, MethodSpec enumerator)
			{
				var ps = MemberCache.FindMember (enumerator.ReturnType,
					MemberFilter.Property ("Current", null),
					BindingRestriction.InstanceOnly) as PropertySpec;

				if (ps == null || !ps.IsPublic) {
					Error_WrongEnumerator (rc, enumerator);
					return null;
				}

				return ps;
			}

			public override bool Resolve (BlockContext ec)
			{
				bool is_dynamic = expr.Type == InternalType.Dynamic;

				if (is_dynamic) {
					expr = Convert.ImplicitConversionRequired (ec, expr, TypeManager.ienumerable_type, loc);
				} else if (TypeManager.IsNullableType (expr.Type)) {
					expr = new Nullable.UnwrapCall (expr).Resolve (ec);
				}

				var get_enumerator_mg = ResolveGetEnumerator (ec);
				if (get_enumerator_mg == null) {
					return false;
				}

				var get_enumerator = get_enumerator_mg.BestCandidate;
				enumerator_variable = TemporaryVariableReference.Create (get_enumerator.ReturnType, variable.Block, loc);
				enumerator_variable.Resolve (ec);

				// Prepare bool MoveNext ()
				var move_next_mg = ResolveMoveNext (ec, get_enumerator);
				if (move_next_mg == null) {
					return false;
				}

				move_next_mg.InstanceExpression = enumerator_variable;

				// Prepare ~T~ Current { get; }
				var current_prop = ResolveCurrent (ec, get_enumerator);
				if (current_prop == null) {
					return false;
				}

				var current_pe = new PropertyExpr (current_prop, loc) { InstanceExpression = enumerator_variable }.Resolve (ec);
				if (current_pe == null)
					return false;

				VarExpr ve = var_type as VarExpr;
				if (ve != null) {
					if (is_dynamic) {
						// Source type is dynamic, set element type to dynamic too
						var_type = new TypeExpression (InternalType.Dynamic, var_type.Location);
					} else {
						// Infer implicitly typed local variable from foreach enumerable type
						var_type = new TypeExpression (current_pe.Type, var_type.Location);
					}
				} else if (is_dynamic) {
					// Explicit cast of dynamic collection elements has to be done at runtime
					current_pe = EmptyCast.Create (current_pe, InternalType.Dynamic);
				}

				var_type = var_type.ResolveAsTypeTerminal (ec, false);
				if (var_type == null)
					return false;

				variable.Type = var_type.Type;

				var init = new Invocation (get_enumerator_mg, null);

				statement = new While (new BooleanExpression (new Invocation (move_next_mg, null)),
					new Body (var_type.Type, variable, current_pe, statement, loc), loc);

				var enum_type = enumerator_variable.Type;

				//
				// Add Dispose method call when enumerator can be IDisposable
				//
				if (!enum_type.ImplementsInterface (TypeManager.idisposable_type, false)) {
					if (!enum_type.IsSealed && !TypeManager.IsValueType (enum_type)) {
						//
						// Runtime Dispose check
						//
						var vd = new RuntimeDispose (enumerator_variable.LocalInfo, loc);
						vd.Initializer = init;
						statement = new Using (vd, statement, loc);
					} else {
						//
						// No Dispose call needed
						//
						this.init = new SimpleAssign (enumerator_variable, init);
						this.init.Resolve (ec);
					}
				} else {
					//
					// Static Dispose check
					//
					var vd = new Using.VariableDeclaration (enumerator_variable.LocalInfo, loc);
					vd.Initializer = init;
					statement = new Using (vd, statement, loc);
				}

				return statement.Resolve (ec);
			}

			protected override void DoEmit (EmitContext ec)
			{
				enumerator_variable.LocalInfo.CreateBuilder (ec);

				if (init != null)
					init.EmitStatement (ec);

				statement.Emit (ec);
			}

			#region IErrorHandler Members

			bool OverloadResolver.IErrorHandler.AmbiguousCandidates (ResolveContext ec, MemberSpec best, MemberSpec ambiguous)
			{
				ec.Report.SymbolRelatedToPreviousError (best);
				ec.Report.Warning (278, 2, loc,
					"`{0}' contains ambiguous implementation of `{1}' pattern. Method `{2}' is ambiguous with method `{3}'",
					expr.Type.GetSignatureForError (), "enumerable",
					best.GetSignatureForError (), ambiguous.GetSignatureForError ());

				ambiguous_getenumerator_name = true;
				return true;
			}

			bool OverloadResolver.IErrorHandler.ArgumentMismatch (ResolveContext rc, MemberSpec best, Argument arg, int index)
			{
				return false;
			}

			bool OverloadResolver.IErrorHandler.NoArgumentMatch (ResolveContext rc, MemberSpec best)
			{
				return false;
			}

			bool OverloadResolver.IErrorHandler.TypeInferenceFailed (ResolveContext rc, MemberSpec best)
			{
				return false;
			}

			#endregion
		}

		Expression type;
		LocalVariable variable;
		Expression expr;
		Statement statement;

		public Foreach (Expression type, LocalVariable var, Expression expr, Statement stmt, Location l)
		{
			this.type = type;
			this.variable = var;
			this.expr = expr;
			statement = stmt;
			loc = l;
		}

		public Statement Statement {
			get { return statement; }
		}

		public override bool Resolve (BlockContext ec)
		{
			expr = expr.Resolve (ec);
			if (expr == null)
				return false;

			if (expr.IsNull) {
				ec.Report.Error (186, loc, "Use of null is not valid in this context");
				return false;
			}

			if (expr.Type == TypeManager.string_type) {
				statement = new ArrayForeach (this, 1);
			} else if (expr.Type is ArrayContainer) {
				statement = new ArrayForeach (this, ((ArrayContainer) expr.Type).Rank);
			} else {
				if (expr.eclass == ExprClass.MethodGroup || expr is AnonymousMethodExpression) {
					ec.Report.Error (446, expr.Location, "Foreach statement cannot operate on a `{0}'",
						expr.ExprClassName);
					return false;
				}

				statement = new CollectionForeach (type, variable, expr, statement, loc);
			}

			return statement.Resolve (ec);
		}

		protected override void DoEmit (EmitContext ec)
		{
			Label old_begin = ec.LoopBegin, old_end = ec.LoopEnd;
			ec.LoopBegin = ec.DefineLabel ();
			ec.LoopEnd = ec.DefineLabel ();

			statement.Emit (ec);

			ec.LoopBegin = old_begin;
			ec.LoopEnd = old_end;
		}

		protected override void CloneTo (CloneContext clonectx, Statement t)
		{
			Foreach target = (Foreach) t;

			target.type = type.Clone (clonectx);
			target.expr = expr.Clone (clonectx);
			target.statement = statement.Clone (clonectx);
		}
	}
}
