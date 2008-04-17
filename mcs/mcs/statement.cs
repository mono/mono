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
using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics;
using System.Collections;
using System.Collections.Specialized;

namespace Mono.CSharp {
	
	public abstract class Statement {
		public Location loc;
		
		/// <summary>
		///   Resolves the statement, true means that all sub-statements
		///   did resolve ok.
		//  </summary>
		public virtual bool Resolve (EmitContext ec)
		{
			return true;
		}

		/// <summary>
		///   We already know that the statement is unreachable, but we still
		///   need to resolve it to catch errors.
		/// </summary>
		public virtual bool ResolveUnreachable (EmitContext ec, bool warn)
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
				Report.Warning (162, 2, loc, "Unreachable code detected");

			ec.StartFlowBranching (FlowBranching.BranchingType.Block, loc);
			bool ok = Resolve (ec);
			ec.KillFlowBranching ();

			return ok;
		}
				
		/// <summary>
		///   Return value indicates whether all code paths emitted return.
		/// </summary>
		protected abstract void DoEmit (EmitContext ec);

		/// <summary>
		///   Utility wrapper routine for Error, just to beautify the code
		/// </summary>
		public void Error (int error, string format, params object[] args)
		{
			Error (error, String.Format (format, args));
		}

		public void Error (int error, string s)
		{
			if (!loc.IsNull)
				Report.Error (error, loc, s);
			else
				Report.Error (error, s);
		}

		/// <summary>
		///   Return value indicates whether all code paths emitted return.
		/// </summary>
		public virtual void Emit (EmitContext ec)
		{
			ec.Mark (loc, true);
			DoEmit (ec);
		}

		//
		// This routine must be overrided in derived classes and make copies
		// of all the data that might be modified if resolved
		// 
		protected virtual void CloneTo (CloneContext clonectx, Statement target)
		{
			throw new InternalErrorException ("{0} does not implement Statement.CloneTo", this.GetType ());
		}

		public Statement Clone (CloneContext clonectx)
		{
			Statement s = (Statement) this.MemberwiseClone ();
			CloneTo (clonectx, s);
			return s;
		}

		public virtual Expression CreateExpressionTree (EmitContext ec)
		{
			Report.Error (834, loc, "A lambda expression with statement body cannot be converted to an expresion tree");
			return null;
		}

		public Statement PerformClone ()
		{
			CloneContext clonectx = new CloneContext ();

			return Clone (clonectx);
		}

	}

	//
	// This class is used during the Statement.Clone operation
	// to remap objects that have been cloned.
	//
	// Since blocks are cloned by Block.Clone, we need a way for
	// expressions that must reference the block to be cloned
	// pointing to the new cloned block.
	//
	public class CloneContext {
		Hashtable block_map = new Hashtable ();
		Hashtable variable_map;
		
		public void AddBlockMap (Block from, Block to)
		{
			if (block_map.Contains (from))
				return;
			block_map [from] = to;
		}
		
		public Block LookupBlock (Block from)
		{
			Block result = (Block) block_map [from];

			if (result == null){
				result = (Block) from.Clone (this);
				block_map [from] = result;
			}

			return result;
		}

		///
		/// Remaps block to cloned copy if one exists.
		///
		public Block RemapBlockCopy (Block from)
		{
			Block mapped_to = (Block)block_map[from];
			if (mapped_to == null)
				return from;

			return mapped_to;
		}

		public void AddVariableMap (LocalInfo from, LocalInfo to)
		{
			if (variable_map == null)
				variable_map = new Hashtable ();
			
			if (variable_map.Contains (from))
				return;
			variable_map [from] = to;
		}
		
		public LocalInfo LookupVariable (LocalInfo from)
		{
			LocalInfo result = (LocalInfo) variable_map [from];

			if (result == null)
				throw new Exception ("LookupVariable: looking up a variable that has not been registered yet");

			return result;
		}
	}
	
	public sealed class EmptyStatement : Statement {
		
		private EmptyStatement () {}
		
		public static readonly EmptyStatement Value = new EmptyStatement ();
		
		public override bool Resolve (EmitContext ec)
		{
			return true;
		}

		public override bool ResolveUnreachable (EmitContext ec, bool warn)
		{
			return true;
		}

		protected override void DoEmit (EmitContext ec)
		{
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
		
		public If (Expression expr, Statement true_statement, Location l)
		{
			this.expr = expr;
			TrueStatement = true_statement;
			loc = l;
		}

		public If (Expression expr,
			   Statement true_statement,
			   Statement false_statement,
			   Location l)
		{
			this.expr = expr;
			TrueStatement = true_statement;
			FalseStatement = false_statement;
			loc = l;
		}

		public override bool Resolve (EmitContext ec)
		{
			bool ok = true;

			Report.Debug (1, "START IF BLOCK", loc);

			expr = Expression.ResolveBoolean (ec, expr, loc);
			if (expr == null){
				ok = false;
				goto skip;
			}

			Assign ass = expr as Assign;
			if (ass != null && ass.Source is Constant) {
				Report.Warning (665, 3, loc, "Assignment in conditional expression is always constant; did you mean to use == instead of = ?");
			}

			//
			// Dead code elimination
			//
			if (expr is Constant){
				bool take = !((Constant) expr).IsDefaultValue;

				if (take){
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
		skip:
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
			ILGenerator ig = ec.ig;
			Label false_target = ig.DefineLabel ();
			Label end;

			//
			// If we're a boolean constant, Resolve() already
			// eliminated dead code for us.
			//
			if (expr is Constant){
				
				//
				// Simple bool constant
				//
				if (expr is BoolConstant) {
					bool take = ((BoolConstant) expr).Value;

					if (take)
						TrueStatement.Emit (ec);
					else if (FalseStatement != null)
						FalseStatement.Emit (ec);
					
					return;
				}

				//
				// Bool constant with side-effects
				//
				expr.Emit (ec);
				ig.Emit (OpCodes.Pop);

				if (TrueStatement != null)
					TrueStatement.Emit (ec);
				if (FalseStatement != null)
					FalseStatement.Emit (ec);

				return;
			}
			
			expr.EmitBranchable (ec, false_target, false);
			
			TrueStatement.Emit (ec);

			if (FalseStatement != null){
				bool branch_emitted = false;
				
				end = ig.DefineLabel ();
				if (!is_true_ret){
					ig.Emit (OpCodes.Br, end);
					branch_emitted = true;
				}

				ig.MarkLabel (false_target);
				FalseStatement.Emit (ec);

				if (branch_emitted)
					ig.MarkLabel (end);
			} else {
				ig.MarkLabel (false_target);
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
		bool infinite;
		
		public Do (Statement statement, Expression bool_expr, Location l)
		{
			expr = bool_expr;
			EmbeddedStatement = statement;
			loc = l;
		}

		public override bool Resolve (EmitContext ec)
		{
			bool ok = true;

			ec.StartFlowBranching (FlowBranching.BranchingType.Loop, loc);

			bool was_unreachable = ec.CurrentBranching.CurrentUsageVector.IsUnreachable;

			ec.StartFlowBranching (FlowBranching.BranchingType.Embedded, loc);
			if (!EmbeddedStatement.Resolve (ec))
				ok = false;
			ec.EndFlowBranching ();

			if (ec.CurrentBranching.CurrentUsageVector.IsUnreachable && !was_unreachable)
				Report.Warning (162, 2, expr.Location, "Unreachable code detected");

			expr = Expression.ResolveBoolean (ec, expr, loc);
			if (expr == null)
				ok = false;
			else if (expr is BoolConstant){
				bool res = ((BoolConstant) expr).Value;

				if (res)
					infinite = true;
			}
			if (infinite)
				ec.CurrentBranching.CurrentUsageVector.Goto ();

			ec.EndFlowBranching ();

			return ok;
		}
		
		protected override void DoEmit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			Label loop = ig.DefineLabel ();
			Label old_begin = ec.LoopBegin;
			Label old_end = ec.LoopEnd;
			
			ec.LoopBegin = ig.DefineLabel ();
			ec.LoopEnd = ig.DefineLabel ();
				
			ig.MarkLabel (loop);
			EmbeddedStatement.Emit (ec);
			ig.MarkLabel (ec.LoopBegin);

			//
			// Dead code elimination
			//
			if (expr is BoolConstant){
				bool res = ((BoolConstant) expr).Value;

				if (res)
					ec.ig.Emit (OpCodes.Br, loop); 
			} else
				expr.EmitBranchable (ec, loop, true);
			
			ig.MarkLabel (ec.LoopEnd);

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
		
		public While (Expression bool_expr, Statement statement, Location l)
		{
			this.expr = bool_expr;
			Statement = statement;
			loc = l;
		}

		public override bool Resolve (EmitContext ec)
		{
			bool ok = true;

			expr = Expression.ResolveBoolean (ec, expr, loc);
			if (expr == null)
				return false;

			//
			// Inform whether we are infinite or not
			//
			if (expr is BoolConstant){
				BoolConstant bc = (BoolConstant) expr;

				if (bc.Value == false){
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
			if (empty)
				return;

			ILGenerator ig = ec.ig;
			Label old_begin = ec.LoopBegin;
			Label old_end = ec.LoopEnd;
			
			ec.LoopBegin = ig.DefineLabel ();
			ec.LoopEnd = ig.DefineLabel ();

			//
			// Inform whether we are infinite or not
			//
			if (expr is BoolConstant){
				ig.MarkLabel (ec.LoopBegin);
				Statement.Emit (ec);
				ig.Emit (OpCodes.Br, ec.LoopBegin);
					
				//
				// Inform that we are infinite (ie, `we return'), only
				// if we do not `break' inside the code.
				//
				ig.MarkLabel (ec.LoopEnd);
			} else {
				Label while_loop = ig.DefineLabel ();

				ig.Emit (OpCodes.Br, ec.LoopBegin);
				ig.MarkLabel (while_loop);

				Statement.Emit (ec);
			
				ig.MarkLabel (ec.LoopBegin);
				ec.Mark (loc, true);

				expr.EmitBranchable (ec, while_loop, true);
				
				ig.MarkLabel (ec.LoopEnd);
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
			    Expression test,
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

		public override bool Resolve (EmitContext ec)
		{
			bool ok = true;

			if (InitStatement != null){
				if (!InitStatement.Resolve (ec))
					ok = false;
			}

			if (Test != null){
				Test = Expression.ResolveBoolean (ec, Test, loc);
				if (Test == null)
					ok = false;
				else if (Test is BoolConstant){
					BoolConstant bc = (BoolConstant) Test;

					if (bc.Value == false){
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
			if (empty)
				return;

			ILGenerator ig = ec.ig;
			Label old_begin = ec.LoopBegin;
			Label old_end = ec.LoopEnd;
			Label loop = ig.DefineLabel ();
			Label test = ig.DefineLabel ();
			
			if (InitStatement != null && InitStatement != EmptyStatement.Value)
				InitStatement.Emit (ec);

			ec.LoopBegin = ig.DefineLabel ();
			ec.LoopEnd = ig.DefineLabel ();

			ig.Emit (OpCodes.Br, test);
			ig.MarkLabel (loop);
			Statement.Emit (ec);

			ig.MarkLabel (ec.LoopBegin);
			if (Increment != EmptyStatement.Value)
				Increment.Emit (ec);

			ig.MarkLabel (test);
			//
			// If test is null, there is no test, and we are just
			// an infinite loop
			//
			if (Test != null){
				//
				// The Resolve code already catches the case for
				// Test == BoolConstant (false) so we know that
				// this is true
				//
				if (Test is BoolConstant)
					ig.Emit (OpCodes.Br, loop);
				else
					Test.EmitBranchable (ec, loop, true);
				
			} else
				ig.Emit (OpCodes.Br, loop);
			ig.MarkLabel (ec.LoopEnd);

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

		public override bool Resolve (EmitContext ec)
		{
			if (expr != null)
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

	// A 'return' or a 'yield break'
	public abstract class ExitStatement : Statement
	{
		protected bool unwind_protect;
		protected abstract bool DoResolve (EmitContext ec);

		public virtual void Error_FinallyClause ()
		{
			Report.Error (157, loc, "Control cannot leave the body of a finally clause");
		}

		public sealed override bool Resolve (EmitContext ec)
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
	public class Return : ExitStatement {
		protected Expression Expr;
		public Return (Expression expr, Location l)
		{
			Expr = expr;
			loc = l;
		}
		
		protected override bool DoResolve (EmitContext ec)
		{
			if (Expr == null) {
				if (ec.ReturnType == TypeManager.void_type)
					return true;
				
				Error (126, "An object of a type convertible to `{0}' is required " +
					   "for the return statement",
					   TypeManager.CSharpName (ec.ReturnType));
				return false;
			}

			AnonymousContainer am = ec.CurrentAnonymousMethod;
			if ((am != null) && am.IsIterator && ec.InIterator) {
				Report.Error (1622, loc, "Cannot return a value from iterators. Use the yield return " +
						  "statement to return a value, or yield break to end the iteration");
			}

			if (am == null && ec.ReturnType == TypeManager.void_type) {
				MemberCore mc = ec.ResolveContext as MemberCore;
				Report.Error (127, loc, "`{0}': A return keyword must not be followed by any expression when method returns void",
					mc.GetSignatureForError ());
			}

			Expr = Expr.Resolve (ec);
			if (Expr == null)
				return false;

			if (Expr.Type != ec.ReturnType) {
				if (ec.InferReturnType) {
					//
					// void cannot be used in contextual return
					//
					if (Expr.Type == TypeManager.void_type)
						return false;

					ec.ReturnType = Expr.Type;
				} else {
					Expr = Convert.ImplicitConversionRequired (
						ec, Expr, ec.ReturnType, loc);

					if (Expr == null) {
						if (am != null) {
							Report.Error (1662, loc,
								"Cannot convert `{0}' to delegate type `{1}' because some of the return types in the block are not implicitly convertible to the delegate return type",
								am.ContainerType, am.GetSignatureForError ());
						}
						return false;
					}
				}
			}

			return true;			
		}
		
		protected override void DoEmit (EmitContext ec)
		{
			if (Expr != null) {
				Expr.Emit (ec);

				if (unwind_protect)
					ec.ig.Emit (OpCodes.Stloc, ec.TemporaryReturn ());
			}

			if (unwind_protect)
				ec.ig.Emit (OpCodes.Leave, ec.ReturnLabel);
			else
				ec.ig.Emit (OpCodes.Ret);
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
		
		public override bool Resolve (EmitContext ec)
		{
			int errors = Report.Errors;
			unwind_protect = ec.CurrentBranching.AddGotoOrigin (ec.CurrentBranching.CurrentUsageVector, this);
			ec.CurrentBranching.CurrentUsageVector.Goto ();
			return errors == Report.Errors;
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

		protected override void DoEmit (EmitContext ec)
		{
			if (label == null)
				throw new InternalErrorException ("goto emitted before target resolved");
			Label l = label.LabelTarget (ec);
			ec.ig.Emit (unwind_protect ? OpCodes.Leave : OpCodes.Br, l);
		}
	}

	public class LabeledStatement : Statement {
		string name;
		bool defined;
		bool referenced;
		Label label;
		ILGenerator ig;

		FlowBranching.UsageVector vectors;
		
		public LabeledStatement (string name, Location l)
		{
			this.name = name;
			this.loc = l;
		}

		public Label LabelTarget (EmitContext ec)
		{
			if (defined)
				return label;
			ig = ec.ig;
			label = ec.ig.DefineLabel ();
			defined = true;

			return label;
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

		public override bool Resolve (EmitContext ec)
		{
			// this flow-branching will be terminated when the surrounding block ends
			ec.StartFlowBranching (this);
			return true;
		}

		protected override void DoEmit (EmitContext ec)
		{
			if (ig != null && ig != ec.ig)
				throw new InternalErrorException ("cannot happen");
			LabelTarget (ec);
			ec.ig.MarkLabel (label);
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

		public override bool Resolve (EmitContext ec)
		{
			ec.CurrentBranching.CurrentUsageVector.Goto ();
			return true;
		}

		protected override void DoEmit (EmitContext ec)
		{
			if (ec.Switch == null){
				Report.Error (153, loc, "A goto case is only valid inside a switch statement");
				return;
			}

			if (!ec.Switch.GotDefault){
				FlowBranchingBlock.Error_UnknownLabel (loc, "default");
				return;
			}
			ec.ig.Emit (OpCodes.Br, ec.Switch.DefaultTarget);
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

		public override bool Resolve (EmitContext ec)
		{
			if (ec.Switch == null){
				Report.Error (153, loc, "A goto case is only valid inside a switch statement");
				return false;
			}

			expr = expr.Resolve (ec);
			if (expr == null)
				return false;

			Constant c = expr as Constant;
			if (c == null) {
				Error (150, "A constant value is expected");
				return false;
			}

			Type type = ec.Switch.SwitchType;
			if (!Convert.ImplicitStandardConversionExists (c, type))
				Report.Warning (469, 2, loc, "The `goto case' value is not implicitly " +
						"convertible to type `{0}'", TypeManager.CSharpName (type));

			bool fail = false;
			object val = c.GetValue ();
			if ((val != null) && (c.Type != type) && (c.Type != TypeManager.object_type))
				val = TypeManager.ChangeType (val, type, out fail);

			if (fail) {
				Report.Error (30, loc, "Cannot convert type `{0}' to `{1}'",
					      c.GetSignatureForError (), TypeManager.CSharpName (type));
				return false;
			}

			if (val == null)
				val = SwitchLabel.NullStringCase;
					
			sl = (SwitchLabel) ec.Switch.Elements [val];

			if (sl == null){
				FlowBranchingBlock.Error_UnknownLabel (loc, "case " + 
					(c.GetValue () == null ? "null" : val.ToString ()));
				return false;
			}

			ec.CurrentBranching.CurrentUsageVector.Goto ();
			return true;
		}

		protected override void DoEmit (EmitContext ec)
		{
			ec.ig.Emit (OpCodes.Br, sl.GetILLabelCode (ec));
		}

		protected override void CloneTo (CloneContext clonectx, Statement t)
		{
			GotoCase target = (GotoCase) t;

			target.expr = expr.Clone (clonectx);
			target.sl = sl.Clone (clonectx);
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
			ec.CurrentBranching.CurrentUsageVector.Goto ();

			if (expr == null)
				return ec.CurrentBranching.CheckRethrow (loc);

			expr = expr.Resolve (ec);
			if (expr == null)
				return false;

			ExprClass eclass = expr.eclass;

			if (!(eclass == ExprClass.Variable || eclass == ExprClass.PropertyAccess ||
			      eclass == ExprClass.Value || eclass == ExprClass.IndexerAccess)) {
				expr.Error_UnexpectedKind (ec.DeclContainer, "value, variable, property or indexer access ", loc);
				return false;
			}

			Type t = expr.Type;

			if ((t != TypeManager.exception_type) &&
			    !TypeManager.IsSubclassOf (t, TypeManager.exception_type) &&
			    !(expr is NullLiteral)) {
				Error (155, "The type caught or thrown must be derived from System.Exception");
				return false;
			}
			return true;
		}
			
		protected override void DoEmit (EmitContext ec)
		{
			if (expr == null)
				ec.ig.Emit (OpCodes.Rethrow);
			else {
				expr.Emit (ec);

				ec.ig.Emit (OpCodes.Throw);
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

		public override bool Resolve (EmitContext ec)
		{
			int errors = Report.Errors;
			unwind_protect = ec.CurrentBranching.AddBreakOrigin (ec.CurrentBranching.CurrentUsageVector, loc);
			ec.CurrentBranching.CurrentUsageVector.Goto ();
			return errors == Report.Errors;
		}

		protected override void DoEmit (EmitContext ec)
		{
			ec.ig.Emit (unwind_protect ? OpCodes.Leave : OpCodes.Br, ec.LoopEnd);
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

		public override bool Resolve (EmitContext ec)
		{
			int errors = Report.Errors;
			unwind_protect = ec.CurrentBranching.AddContinueOrigin (ec.CurrentBranching.CurrentUsageVector, loc);
			ec.CurrentBranching.CurrentUsageVector.Goto ();
			return errors == Report.Errors;
		}

		protected override void DoEmit (EmitContext ec)
		{
			ec.ig.Emit (unwind_protect ? OpCodes.Leave : OpCodes.Br, ec.LoopBegin);
		}

		protected override void CloneTo (CloneContext clonectx, Statement t)
		{
			// nothing needed.
		}
	}

	public abstract class Variable
	{
		public abstract Type Type {
			get;
		}

		public abstract bool HasInstance {
			get;
		}

		public abstract bool NeedsTemporary {
			get;
		}

		public abstract void EmitInstance (EmitContext ec);

		public abstract void Emit (EmitContext ec);

		public abstract void EmitAssign (EmitContext ec);

		public abstract void EmitAddressOf (EmitContext ec);
	}

	public interface IKnownVariable {
		Block Block { get; }
		Location Location { get; }
	}

	//
	// The information about a user-perceived local variable
	//
	public class LocalInfo : IKnownVariable {
		public Expression Type;

		public Type VariableType;
		public readonly string Name;
		public readonly Location Location;
		public readonly Block Block;

		public VariableInfo VariableInfo;

		Variable var;
		public Variable Variable {
			get { return var; }
		}

		[Flags]
		enum Flags : byte {
			Used = 1,
			ReadOnly = 2,
			Pinned = 4,
			IsThis = 8,
			Captured = 16,
			AddressTaken = 32,
			CompilerGenerated = 64,
			IsConstant = 128
		}

		public enum ReadOnlyContext: byte {
			Using,
			Foreach,
			Fixed
		}

		Flags flags;
		ReadOnlyContext ro_context;
		LocalBuilder builder;
		
		public LocalInfo (Expression type, string name, Block block, Location l)
		{
			Type = type;
			Name = name;
			Block = block;
			Location = l;
		}

		public LocalInfo (DeclSpace ds, Block block, Location l)
		{
			VariableType = ds.IsGeneric ? ds.CurrentType : ds.TypeBuilder;
			Block = block;
			Location = l;
		}

		public void ResolveVariable (EmitContext ec)
		{
			Block theblock = Block;
			if (theblock.ScopeInfo != null)
				var = theblock.ScopeInfo.GetCapturedVariable (this);

			if (var == null) {
				if (Pinned)
					//
					// This is needed to compile on both .NET 1.x and .NET 2.x
					// the later introduced `DeclareLocal (Type t, bool pinned)'
					//
					builder = TypeManager.DeclareLocalPinned (ec.ig, VariableType);
				else
					builder = ec.ig.DeclareLocal (VariableType);

				var = new LocalVariable (this, builder);
			}
		}

		public void EmitSymbolInfo (EmitContext ec, string name)
		{
			if (builder != null)
				ec.DefineLocalVariable (Name, builder);
		}

		public bool IsThisAssigned (EmitContext ec)
		{
			if (VariableInfo == null)
				throw new Exception ();

			if (!ec.DoFlowAnalysis || ec.CurrentBranching.IsAssigned (VariableInfo))
				return true;

			return VariableInfo.TypeInfo.IsFullyInitialized (ec.CurrentBranching, VariableInfo, ec.loc);
		}

		public bool IsAssigned (EmitContext ec)
		{
			if (VariableInfo == null)
				throw new Exception ();

			return !ec.DoFlowAnalysis || ec.CurrentBranching.IsAssigned (VariableInfo);
		}

		public bool Resolve (EmitContext ec)
		{
			if (VariableType == null) {
				TypeExpr texpr = Type.ResolveAsContextualType (ec, false);
				if (texpr == null)
					return false;
				
				VariableType = texpr.Type;
			}

			if (TypeManager.IsGenericParameter (VariableType))
				return true;

			if (VariableType == TypeManager.void_type) {
				Expression.Error_VoidInvalidInTheContext (Location);
				return false;
			}

			if (VariableType.IsAbstract && VariableType.IsSealed) {
				FieldBase.Error_VariableOfStaticClass (Location, Name, VariableType);
				return false;
			}

			if (VariableType.IsPointer && !ec.InUnsafe)
				Expression.UnsafeError (Location);

			return true;
		}

		public bool IsCaptured {
			get { return (flags & Flags.Captured) != 0; }
			set { flags |= Flags.Captured; }
		}

		public bool IsConstant {
			get { return (flags & Flags.IsConstant) != 0; }
			set { flags |= Flags.IsConstant; }
		}

		public bool AddressTaken {
			get { return (flags & Flags.AddressTaken) != 0; }
			set { flags |= Flags.AddressTaken; }
		}

		public bool CompilerGenerated {
			get { return (flags & Flags.CompilerGenerated) != 0; }
			set { flags |= Flags.CompilerGenerated; }
		}

		public override string ToString ()
		{
			return String.Format ("LocalInfo ({0},{1},{2},{3})",
					      Name, Type, VariableInfo, Location);
		}

		public bool Used {
			get { return (flags & Flags.Used) != 0; }
			set { flags = value ? (flags | Flags.Used) : (unchecked (flags & ~Flags.Used)); }
		}

		public bool ReadOnly {
			get { return (flags & Flags.ReadOnly) != 0; }
		}

		public void SetReadOnlyContext (ReadOnlyContext context)
		{
			flags |= Flags.ReadOnly;
			ro_context = context;
		}

		public string GetReadOnlyContext ()
		{
			if (!ReadOnly)
				throw new InternalErrorException ("Variable is not readonly");

			switch (ro_context) {
				case ReadOnlyContext.Fixed:
					return "fixed variable";
				case ReadOnlyContext.Foreach:
					return "foreach iteration variable";
				case ReadOnlyContext.Using:
					return "using variable";
			}
			throw new NotImplementedException ();
		}

		//
		// Whether the variable is pinned, if Pinned the variable has been 
		// allocated in a pinned slot with DeclareLocal.
		//
		public bool Pinned {
			get { return (flags & Flags.Pinned) != 0; }
			set { flags = value ? (flags | Flags.Pinned) : (flags & ~Flags.Pinned); }
		}

		public bool IsThis {
			get { return (flags & Flags.IsThis) != 0; }
			set { flags = value ? (flags | Flags.IsThis) : (flags & ~Flags.IsThis); }
		}

		Block IKnownVariable.Block {
			get { return Block; }
		}

		Location IKnownVariable.Location {
			get { return Location; }
		}

		protected class LocalVariable : Variable
		{
			public readonly LocalInfo LocalInfo;
			LocalBuilder builder;

			public LocalVariable (LocalInfo local, LocalBuilder builder)
			{
				this.LocalInfo = local;
				this.builder = builder;
			}

			public override Type Type {
				get { return LocalInfo.VariableType; }
			}

			public override bool HasInstance {
				get { return false; }
			}

			public override bool NeedsTemporary {
				get { return false; }
			}

			public override void EmitInstance (EmitContext ec)
			{
				// Do nothing.
			}

			public override void Emit (EmitContext ec)
			{
				ec.ig.Emit (OpCodes.Ldloc, builder);
			}

			public override void EmitAssign (EmitContext ec)
			{
				ec.ig.Emit (OpCodes.Stloc, builder);
			}

			public override void EmitAddressOf (EmitContext ec)
			{
				ec.ig.Emit (OpCodes.Ldloca, builder);
			}
		}

		public LocalInfo Clone (CloneContext clonectx)
		{
			//
			// Variables in anonymous block are not resolved yet
			//
			if (VariableType == null)
				return new LocalInfo (Type.Clone (clonectx), Name, clonectx.LookupBlock (Block), Location);

			//
			// Variables in method block are resolved
			//
			LocalInfo li = new LocalInfo (null, Name, clonectx.LookupBlock (Block), Location);
			li.VariableType = VariableType;
			return li;			
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
		public Block    Parent;
		public readonly Location  StartLocation;
		public Location EndLocation = Location.Null;

		public ExplicitBlock Explicit;
		public ToplevelBlock Toplevel;

		[Flags]
		public enum Flags : byte {
			Unchecked = 1,
			BlockUsed = 2,
			VariablesInitialized = 4,
			HasRet = 8,
			IsDestructor = 16,
			Unsafe = 32,
			HasVarargs = 64, // Used in ToplevelBlock
			IsIterator = 128
		}
		protected Flags flags;

		public bool Unchecked {
			get { return (flags & Flags.Unchecked) != 0; }
			set { flags |= Flags.Unchecked; }
		}

		public bool Unsafe {
			get { return (flags & Flags.Unsafe) != 0; }
			set { flags |= Flags.Unsafe; }
		}

		//
		// The statements in this block
		//
		protected ArrayList statements;
		int num_statements;

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
		HybridDictionary labels;

		//
		// Keeps track of (name, type) pairs
		//
		IDictionary variables;

		//
		// Keeps track of constants
		HybridDictionary constants;

		//
		// Temporary variables.
		//
		ArrayList temporary_variables;
		
		//
		// If this is a switch section, the enclosing switch block.
		//
		Block switch_block;

		// TODO: merge with scope_initializers
		ExpressionStatement scope_init;
		ArrayList scope_initializers;

		ArrayList anonymous_children;

		protected static int id;

		int this_id;

		int assignable_slots;
		protected ScopeInfo scope_info;
		bool unreachable_shown;
		bool unreachable;
		
		public Block (Block parent)
			: this (parent, (Flags) 0, Location.Null, Location.Null)
		{ }

		public Block (Block parent, Flags flags)
			: this (parent, flags, Location.Null, Location.Null)
		{ }

		public Block (Block parent, Location start, Location end)
			: this (parent, (Flags) 0, start, end)
		{ }

		public Block (Block parent, Flags flags, Location start, Location end)
		{
			if (parent != null) {
				parent.AddChild (this);

				// the appropriate constructors will fixup these fields
				Toplevel = parent.Toplevel;
				Explicit = parent.Explicit;
			}
			
			this.Parent = parent;
			this.flags = flags;
			this.StartLocation = start;
			this.EndLocation = end;
			this.loc = start;
			this_id = id++;
			statements = new ArrayList (4);
		}

		public Block CreateSwitchBlock (Location start)
		{
			// FIXME: should this be implicit?
			Block new_block = new ExplicitBlock (this, start, start);
			new_block.switch_block = this;
			return new_block;
		}

		public int ID {
			get { return this_id; }
		}

		public IDictionary Variables {
			get {
				if (variables == null)
					variables = new ListDictionary ();
				return variables;
			}
		}

		void AddChild (Block b)
		{
			if (children == null)
				children = new ArrayList (1);
			
			children.Add (b);
		}

		public void SetEndLocation (Location loc)
		{
			EndLocation = loc;
		}

		protected static void Error_158 (string name, Location loc)
		{
			Report.Error (158, loc, "The label `{0}' shadows another label " +
				      "by the same name in a contained scope", name);
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
		public bool AddLabel (LabeledStatement target)
		{
			if (switch_block != null)
				return switch_block.AddLabel (target);

			string name = target.Name;

			Block cur = this;
			while (cur != null) {
				LabeledStatement s = cur.DoLookupLabel (name);
				if (s != null) {
					Report.SymbolRelatedToPreviousError (s.loc, s.Name);
					Report.Error (140, target.loc, "The label `{0}' is a duplicate", name);
					return false;
				}

				if (this == Explicit)
					break;

				cur = cur.Parent;
			}

			while (cur != null) {
				if (cur.DoLookupLabel (name) != null) {
					Error_158 (name, target.loc);
					return false;
				}

				if (children != null) {
					foreach (Block b in children) {
						LabeledStatement s = b.DoLookupLabel (name);
						if (s == null)
							continue;

						Report.SymbolRelatedToPreviousError (s.loc, s.Name);
						Error_158 (name, target.loc);
						return false;
					}
				}

				cur = cur.Parent;
			}

			Toplevel.CheckError158 (name, target.loc);

			if (labels == null)
				labels = new HybridDictionary();

			labels.Add (name, target);
			return true;
		}

		public LabeledStatement LookupLabel (string name)
		{
			LabeledStatement s = DoLookupLabel (name);
			if (s != null)
				return s;

			if (children == null)
				return null;

			foreach (Block child in children) {
				if (Explicit != child.Explicit)
					continue;

				s = child.LookupLabel (name);
				if (s != null)
					return s;
			}

			return null;
		}

		LabeledStatement DoLookupLabel (string name)
		{
			if (switch_block != null)
				return switch_block.LookupLabel (name);

			if (labels != null)
				if (labels.Contains (name))
					return ((LabeledStatement) labels [name]);

			return null;
		}

		public bool CheckInvariantMeaningInBlock (string name, Expression e, Location loc)
		{
			Block b = this;
			IKnownVariable kvi = b.Explicit.GetKnownVariable (name);
			while (kvi == null) {
				b = b.Explicit.Parent;
				if (b == null)
					return true;
				kvi = b.Explicit.GetKnownVariable (name);
			}

			if (kvi.Block == b)
				return true;

			// Is kvi.Block nested inside 'b'
			if (b.Explicit != kvi.Block.Explicit) {
				//
				// If a variable by the same name it defined in a nested block of this
				// block, we violate the invariant meaning in a block.
				//
				if (b == this) {
					Report.SymbolRelatedToPreviousError (kvi.Location, name);
					Report.Error (135, loc, "`{0}' conflicts with a declaration in a child block", name);
					return false;
				}

				//
				// It's ok if the definition is in a nested subblock of b, but not
				// nested inside this block -- a definition in a sibling block
				// should not affect us.
				//
				return true;
			}

			//
			// Block 'b' and kvi.Block are the same textual block.
			// However, different variables are extant.
			//
			// Check if the variable is in scope in both blocks.  We use
			// an indirect check that depends on AddVariable doing its
			// part in maintaining the invariant-meaning-in-block property.
			//
			if (e is VariableReference || (e is Constant && b.GetLocalInfo (name) != null))
				return true;

			//
			// Even though we detected the error when the name is used, we
			// treat it as if the variable declaration was in error.
			//
			Report.SymbolRelatedToPreviousError (loc, name);
			Error_AlreadyDeclared (kvi.Location, name, "parent or current");
			return false;
		}

		public LocalInfo AddVariable (Expression type, string name, Location l)
		{
			LocalInfo vi = GetLocalInfo (name);
			if (vi != null) {
				Report.SymbolRelatedToPreviousError (vi.Location, name);
				if (Explicit == vi.Block.Explicit)
					Error_AlreadyDeclared (l, name, null);
				else
					Error_AlreadyDeclared (l, name, "parent");
				return null;
			}

			ToplevelParameterInfo pi = Toplevel.GetParameterInfo (name);
			if (pi != null) {
				Report.SymbolRelatedToPreviousError (pi.Location, name);
				Error_AlreadyDeclared (loc, name,
					pi.Block == Toplevel ? "method argument" : "parent or current");
				return null;
			}
			
			if (Toplevel.GenericMethod != null) {
				foreach (TypeParameter tp in Toplevel.GenericMethod.CurrentTypeParameters) {
					if (tp.Name == name) {
						Report.SymbolRelatedToPreviousError (tp);
						Error_AlreadyDeclaredTypeParameter (loc, name);
						return null;
					}
				}
			}			

			IKnownVariable kvi = Explicit.GetKnownVariable (name);
			if (kvi != null) {
				Report.SymbolRelatedToPreviousError (kvi.Location, name);
				Error_AlreadyDeclared (l, name, "child");
				return null;
			}

			vi = new LocalInfo (type, name, this, l);
			AddVariable (vi);

			if ((flags & Flags.VariablesInitialized) != 0)
				throw new InternalErrorException ("block has already been resolved");

			return vi;
		}
		
		protected virtual void AddVariable (LocalInfo li)
		{
			Variables.Add (li.Name, li);
			Explicit.AddKnownVariable (li.Name, li);
		}

		protected virtual void Error_AlreadyDeclared (Location loc, string var, string reason)
		{
			if (reason == null) {
				Error_AlreadyDeclared (loc, var);
				return;
			}
			
			Report.Error (136, loc, "A local variable named `{0}' cannot be declared " +
				      "in this scope because it would give a different meaning " +
				      "to `{0}', which is already used in a `{1}' scope " +
				      "to denote something else", var, reason);
		}

		protected virtual void Error_AlreadyDeclared (Location loc, string name)
		{
			Report.Error (128, loc,
				"A local variable named `{0}' is already defined in this scope", name);
		}
					
		protected virtual void Error_AlreadyDeclaredTypeParameter (Location loc, string name)
		{
			GenericMethod.Error_ParameterNameCollision (loc, name, "local variable");
		}					

		public bool AddConstant (Expression type, string name, Expression value, Location l)
		{
			if (AddVariable (type, name, l) == null)
				return false;
			
			if (constants == null)
				constants = new HybridDictionary();

			constants.Add (name, value);

			// A block is considered used if we perform an initialization in a local declaration, even if it is constant.
			Use ();
			return true;
		}

		static int next_temp_id = 0;

		public LocalInfo AddTemporaryVariable (TypeExpr te, Location loc)
		{
			Report.Debug (64, "ADD TEMPORARY", this, Toplevel, loc);

			if (temporary_variables == null)
				temporary_variables = new ArrayList ();

			int id = ++next_temp_id;
			string name = "$s_" + id.ToString ();

			LocalInfo li = new LocalInfo (te, name, this, loc);
			li.CompilerGenerated = true;
			temporary_variables.Add (li);
			return li;
		}

		public LocalInfo GetLocalInfo (string name)
		{
			for (Block b = this; b != null; b = b.Parent) {
				if (b.variables != null) {
					LocalInfo ret = b.variables [name] as LocalInfo;
					if (ret != null)
						return ret;
				}
			}
			return null;
		}

		public Expression GetVariableType (string name)
		{
			LocalInfo vi = GetLocalInfo (name);
			return vi == null ? null : vi.Type;
		}

		public Expression GetConstantExpression (string name)
		{
			for (Block b = this; b != null; b = b.Parent) {
				if (b.constants != null) {
					Expression ret = b.constants [name] as Expression;
					if (ret != null)
						return ret;
				}
			}
			return null;
		}

		//
		// It should be used by expressions which require to
		// register a statement during resolve process.
		//
		public void AddScopeStatement (StatementExpression s)
		{
			if (scope_initializers == null)
				scope_initializers = new ArrayList ();

			scope_initializers.Add (s);
		}
		
		public void AddStatement (Statement s)
		{
			statements.Add (s);
			flags |= Flags.BlockUsed;
		}

		public bool Used {
			get { return (flags & Flags.BlockUsed) != 0; }
		}

		public void Use ()
		{
			flags |= Flags.BlockUsed;
		}

		public bool HasRet {
			get { return (flags & Flags.HasRet) != 0; }
		}

		public bool IsDestructor {
			get { return (flags & Flags.IsDestructor) != 0; }
		}

		public void SetDestructor ()
		{
			flags |= Flags.IsDestructor;
		}

		public int AssignableSlots {
			get {
				if ((flags & Flags.VariablesInitialized) == 0)
					throw new Exception ("Variables have not been initialized yet");
				return assignable_slots;
			}
		}

		public ScopeInfo ScopeInfo {
			get { return scope_info; }
		}

		public ScopeInfo CreateScopeInfo ()
		{
			if (scope_info == null)
				scope_info = ScopeInfo.CreateScope (this);

			return scope_info;
		}

		public ArrayList AnonymousChildren {
			get { return anonymous_children; }
		}

		public void AddAnonymousChild (ToplevelBlock b)
		{
			if (anonymous_children == null)
				anonymous_children = new ArrayList ();

			anonymous_children.Add (b);
		}

		void DoResolveConstants (EmitContext ec)
		{
			if (constants == null)
				return;

			if (variables == null)
				throw new InternalErrorException ("cannot happen");

			foreach (DictionaryEntry de in variables) {
				string name = (string) de.Key;
				LocalInfo vi = (LocalInfo) de.Value;
				Type variable_type = vi.VariableType;

				if (variable_type == null) {
					if (vi.Type is VarExpr)
						Report.Error (822, vi.Type.Location, "An implicitly typed local variable cannot be a constant");

					continue;
				}

				Expression cv = (Expression) constants [name];
				if (cv == null)
					continue;

				// Don't let 'const int Foo = Foo;' succeed.
				// Removing the name from 'constants' ensures that we get a LocalVariableReference below,
				// which in turn causes the 'must be constant' error to be triggered.
				constants.Remove (name);

				if (!Const.IsConstantTypeValid (variable_type)) {
					Const.Error_InvalidConstantType (variable_type, loc);
					continue;
				}

				ec.CurrentBlock = this;
				Expression e;
				using (ec.With (EmitContext.Flags.ConstantCheckState, (flags & Flags.Unchecked) == 0)) {
					e = cv.Resolve (ec);
				}
				if (e == null)
					continue;

				Constant ce = e as Constant;
				if (ce == null) {
					Const.Error_ExpressionMustBeConstant (vi.Location, name);
					continue;
				}

				e = ce.ConvertImplicitly (variable_type);
				if (e == null) {
					if (!variable_type.IsValueType && variable_type != TypeManager.string_type && !ce.IsDefaultValue)
						Const.Error_ConstantCanBeInitializedWithNullOnly (vi.Location, vi.Name);
					else
						ce.Error_ValueCannotBeConverted (null, vi.Location, variable_type, false);
					continue;
				}

				constants.Add (name, e);
				vi.IsConstant = true;
			}
		}

		protected void ResolveMeta (EmitContext ec, int offset)
		{
			Report.Debug (64, "BLOCK RESOLVE META", this, Parent);

			// If some parent block was unsafe, we remain unsafe even if this block
			// isn't explicitly marked as such.
			using (ec.With (EmitContext.Flags.InUnsafe, ec.InUnsafe | Unsafe)) {
				flags |= Flags.VariablesInitialized;

				if (variables != null) {
					foreach (LocalInfo li in variables.Values) {
						if (!li.Resolve (ec))
							continue;
						li.VariableInfo = new VariableInfo (li, offset);
						offset += li.VariableInfo.Length;
					}
				}
				assignable_slots = offset;

				DoResolveConstants (ec);

				if (children == null)
					return;
				foreach (Block b in children)
					b.ResolveMeta (ec, offset);
			}
		}

		//
		// Emits the local variable declarations for a block
		//
		public virtual void EmitMeta (EmitContext ec)
		{
			Report.Debug (64, "BLOCK EMIT META", this, Parent, Toplevel, ScopeInfo, ec);
			if (ScopeInfo != null) {
				scope_init = ScopeInfo.GetScopeInitializer (ec);
				Report.Debug (64, "BLOCK EMIT META #1", this, Toplevel, ScopeInfo,
					      ec, scope_init);
			}

			if (variables != null){
				foreach (LocalInfo vi in variables.Values)
					vi.ResolveVariable (ec);
			}

			if (temporary_variables != null) {
				for (int i = 0; i < temporary_variables.Count; i++)
					((LocalInfo)temporary_variables[i]).ResolveVariable(ec);
			}

			if (children != null) {
				for (int i = 0; i < children.Count; i++)
					((Block)children[i]).EmitMeta(ec);
			}
		}

		void UsageWarning ()
		{
			if (variables == null || Report.WarningLevel < 3)
				return;

			foreach (DictionaryEntry de in variables) {
				LocalInfo vi = (LocalInfo) de.Value;

				if (!vi.Used) {
					string name = (string) de.Key;

					// vi.VariableInfo can be null for 'catch' variables
					if (vi.VariableInfo != null && vi.VariableInfo.IsEverAssigned)
						Report.Warning (219, 3, vi.Location, "The variable `{0}' is assigned but its value is never used", name);
					else
						Report.Warning (168, 3, vi.Location, "The variable `{0}' is declared but never used", name);
				}
			}
		}

		private void CheckPossibleMistakenEmptyStatement (Statement s)
		{
			Statement body;

			// Some statements are wrapped by a Block. Since
			// others' internal could be changed, here I treat
			// them as possibly wrapped by Block equally.
			Block b = s as Block;
			if (b != null && b.statements.Count == 1)
				s = (Statement) b.statements [0];

			if (s is Lock)
				body = ((Lock) s).Statement;
			else if (s is For)
				body = ((For) s).Statement;
			else if (s is Foreach)
				body = ((Foreach) s).Statement;
			else if (s is While)
				body = ((While) s).Statement;
			else if (s is Fixed)
				body = ((Fixed) s).Statement;
			else if (s is Using)
				body = ((Using) s).EmbeddedStatement;
			else if (s is UsingTemporary)
				body = ((UsingTemporary) s).Statement;
			else
				return;

			if (body == null || body is EmptyStatement)
				Report.Warning (642, 3, s.loc, "Possible mistaken empty statement");
		}

		public override bool Resolve (EmitContext ec)
		{
			Block prev_block = ec.CurrentBlock;
			bool ok = true;

			int errors = Report.Errors;

			ec.CurrentBlock = this;
			ec.StartFlowBranching (this);

			Report.Debug (4, "RESOLVE BLOCK", StartLocation, ec.CurrentBranching);

			//
			// This flag is used to notate nested statements as unreachable from the beginning of this block.
			// For the purposes of this resolution, it doesn't matter that the whole block is unreachable 
			// from the beginning of the function.  The outer Resolve() that detected the unreachability is
			// responsible for handling the situation.
			//
			int statement_count = statements.Count;
			for (int ix = 0; ix < statement_count; ix++){
				Statement s = (Statement) statements [ix];
				// Check possible empty statement (CS0642)
				if (Report.WarningLevel >= 3 &&
					ix + 1 < statement_count &&
						statements [ix + 1] is Block)
					CheckPossibleMistakenEmptyStatement (s);

				//
				// Warn if we detect unreachable code.
				//
				if (unreachable) {
					if (s is EmptyStatement)
						continue;

					if (s is Block)
						((Block) s).unreachable = true;

					if (!unreachable_shown && !(s is LabeledStatement)) {
						Report.Warning (162, 2, s.loc, "Unreachable code detected");
						unreachable_shown = true;
					}
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

					statements [ix] = EmptyStatement.Value;
					continue;
				}

				if (unreachable && !(s is LabeledStatement) && !(s is Block))
					statements [ix] = EmptyStatement.Value;

				num_statements = ix + 1;

				unreachable = ec.CurrentBranching.CurrentUsageVector.IsUnreachable;
				if (unreachable && s is LabeledStatement)
					throw new InternalErrorException ("should not happen");
			}

			Report.Debug (4, "RESOLVE BLOCK DONE", StartLocation,
				      ec.CurrentBranching, statement_count, num_statements);

			while (ec.CurrentBranching is FlowBranchingLabeled)
				ec.EndFlowBranching ();

			bool flow_unreachable = ec.EndFlowBranching ();

			ec.CurrentBlock = prev_block;

			if (flow_unreachable)
				flags |= Flags.HasRet;

			// If we're a non-static `struct' constructor which doesn't have an
			// initializer, then we must initialize all of the struct's fields.
			if (this == Toplevel && !Toplevel.IsThisAssigned (ec) && !flow_unreachable)
				ok = false;

			if ((labels != null) && (Report.WarningLevel >= 2)) {
				foreach (LabeledStatement label in labels.Values)
					if (!label.HasBeenReferenced)
						Report.Warning (164, 2, label.loc, "This label has not been referenced");
			}

			if (ok && errors == Report.Errors)
				UsageWarning ();

			return ok;
		}

		public override bool ResolveUnreachable (EmitContext ec, bool warn)
		{
			unreachable_shown = true;
			unreachable = true;

			if (warn)
				Report.Warning (162, 2, loc, "Unreachable code detected");

			ec.StartFlowBranching (FlowBranching.BranchingType.Block, loc);
			bool ok = Resolve (ec);
			ec.KillFlowBranching ();

			return ok;
		}
		
		protected override void DoEmit (EmitContext ec)
		{
			for (int ix = 0; ix < num_statements; ix++){
				Statement s = (Statement) statements [ix];
				s.Emit (ec);
			}
		}

		public override void Emit (EmitContext ec)
		{
			Block prev_block = ec.CurrentBlock;

			ec.CurrentBlock = this;

			bool emit_debug_info = SymbolWriter.HasSymbolWriter;
			bool is_lexical_block = (this == Explicit) && (Parent != null) &&
				((flags & Flags.IsIterator) == 0);

			bool omit_debug_info = ec.OmitDebuggingInfo;

			if (emit_debug_info) {
				if (is_lexical_block)
					ec.BeginScope ();
			}

			if ((scope_init != null) || (scope_initializers != null))
				SymbolWriter.OpenCompilerGeneratedBlock (ec.ig);

			if (scope_init != null) {
				ec.OmitDebuggingInfo = true;
				scope_init.EmitStatement (ec);
				ec.OmitDebuggingInfo = omit_debug_info;
			}
			if (scope_initializers != null) {
				ec.OmitDebuggingInfo = true;
				foreach (StatementExpression s in scope_initializers)
					s.Emit (ec);
				ec.OmitDebuggingInfo = omit_debug_info;
			}

			if ((scope_init != null) || (scope_initializers != null))
				SymbolWriter.CloseCompilerGeneratedBlock (ec.ig);

			ec.Mark (StartLocation, true);
			DoEmit (ec);

			if (emit_debug_info) {
				EmitSymbolInfo (ec);

				if (is_lexical_block)
					ec.EndScope ();
			}

			ec.CurrentBlock = prev_block;
		}

		protected virtual void EmitSymbolInfo (EmitContext ec)
		{
			if (variables != null) {
				foreach (DictionaryEntry de in variables) {
					string name = (string) de.Key;
					LocalInfo vi = (LocalInfo) de.Value;

					vi.EmitSymbolInfo (ec, name);
				}
			}
		}

		public override string ToString ()
		{
			return String.Format ("{0} ({1}:{2})", GetType (),ID, StartLocation);
		}

		protected override void CloneTo (CloneContext clonectx, Statement t)
		{
			Block target = (Block) t;

			clonectx.AddBlockMap (this, target);

			//target.Toplevel = (ToplevelBlock) clonectx.LookupBlock (Toplevel);
			target.Explicit = (ExplicitBlock) clonectx.LookupBlock (Explicit);
			if (Parent != null)
				target.Parent = clonectx.RemapBlockCopy (Parent);

			if (variables != null){
				target.variables = new Hashtable ();

				foreach (DictionaryEntry de in variables){
					LocalInfo newlocal = ((LocalInfo) de.Value).Clone (clonectx);
					target.variables [de.Key] = newlocal;
					clonectx.AddVariableMap ((LocalInfo) de.Value, newlocal);
				}
			}

			target.statements = new ArrayList (statements.Count);
			foreach (Statement s in statements)
				target.statements.Add (s.Clone (clonectx));

			if (target.children != null){
				target.children = new ArrayList (children.Count);
				foreach (Block b in children){
					target.children.Add (clonectx.LookupBlock (b));
				}
			}

			//
			// TODO: labels, switch_block, constants (?), anonymous_children
			//
		}
	}

	public class ExplicitBlock : Block {
		public ExplicitBlock (Block parent, Location start, Location end)
			: this (parent, (Flags) 0, start, end)
		{
		}

		public ExplicitBlock (Block parent, Flags flags, Location start, Location end)
			: base (parent, flags, start, end)
		{
			this.Explicit = this;
		}

		public bool IsIterator {
			get { return (flags & Flags.IsIterator) != 0; }
		}

		HybridDictionary known_variables;

		// <summary>
		//   Marks a variable with name @name as being used in this or a child block.
		//   If a variable name has been used in a child block, it's illegal to
		//   declare a variable with the same name in the current block.
		// </summary>
		internal void AddKnownVariable (string name, IKnownVariable info)
		{
			if (known_variables == null)
				known_variables = new HybridDictionary();

			known_variables [name] = info;

			if (Parent != null)
				Parent.Explicit.AddKnownVariable (name, info);
		}

		internal IKnownVariable GetKnownVariable (string name)
		{
			return known_variables == null ? null : (IKnownVariable) known_variables [name];
		}

		protected override void CloneTo (CloneContext clonectx, Statement t)
		{
			ExplicitBlock target = (ExplicitBlock) t;
			target.known_variables = null;
			base.CloneTo (clonectx, t);
		}
	}

	public class ToplevelParameterInfo : IKnownVariable {
		public readonly ToplevelBlock Block;
		public readonly int Index;
		public VariableInfo VariableInfo;

		Block IKnownVariable.Block {
			get { return Block; }
		}
		public Parameter Parameter {
			get { return Block.Parameters [Index]; }
		}
		public Location Location {
			get { return Parameter.Location; }
		}

		public ToplevelParameterInfo (ToplevelBlock block, int idx)
		{
			this.Block = block;
			this.Index = idx;
		}
	}

	//
	// A toplevel block contains extra information, the split is done
	// only to separate information that would otherwise bloat the more
	// lightweight Block.
	//
	// In particular, this was introduced when the support for Anonymous
	// Methods was implemented. 
	// 
	public class ToplevelBlock : ExplicitBlock {
		GenericMethod generic;
		FlowBranchingToplevel top_level_branching;
		AnonymousContainer anonymous_container;
		RootScopeInfo root_scope;
		Parameters parameters;
		ToplevelParameterInfo[] parameter_info;

		public bool HasVarargs {
			get { return (flags & Flags.HasVarargs) != 0; }
			set { flags |= Flags.HasVarargs; }
		}

		//
		// The parameters for the block.
		//
		public Parameters Parameters {
			get { return parameters; }
		}

		public bool CompleteContexts (EmitContext ec)
		{
			Report.Debug (64, "TOPLEVEL COMPLETE CONTEXTS", this, Parent, root_scope);

			if (root_scope != null)
				root_scope.LinkScopes ();

			if (Parent == null && root_scope != null) {
				Report.Debug (64, "TOPLEVEL COMPLETE CONTEXTS #1", this, root_scope);

				if (root_scope.DefineType () == null)
					return false;
				if (!root_scope.ResolveType ())
					return false;
				if (!root_scope.ResolveMembers ())
					return false;
				if (!root_scope.DefineMembers ())
					return false;
			}

			return true;
		}

		public GenericMethod GenericMethod {
			get { return generic; }
		}

		public ToplevelBlock Container {
			get { return Parent == null ? null : Parent.Toplevel; }
		}

		public AnonymousContainer AnonymousContainer {
			get { return anonymous_container; }
			set { anonymous_container = value; }
		}

		public ToplevelBlock (Block parent, Parameters parameters, Location start) :
			this (parent, (Flags) 0, parameters, start)
		{
		}

		public ToplevelBlock (Block parent, Parameters parameters, GenericMethod generic, Location start) :
			this (parent, parameters, start)
		{
			this.generic = generic;
		}
		
		public ToplevelBlock (Parameters parameters, Location start) :
			this (null, (Flags) 0, parameters, start)
		{
		}

		public ToplevelBlock (Flags flags, Parameters parameters, Location start) :
			this (null, flags, parameters, start)
		{
		}

		// We use 'Parent' to hook up to the containing block, but don't want to register the current block as a child.
		// So, we use a two-stage setup -- first pass a null parent to the base constructor, and then override 'Parent'.
		public ToplevelBlock (Block parent, Flags flags, Parameters parameters, Location start) :
			base (null, flags, start, Location.Null)
		{
			this.Toplevel = this;

			this.parameters = parameters == null ? Parameters.EmptyReadOnlyParameters : parameters;
			this.Parent = parent;
			if (parent != null)
				parent.AddAnonymousChild (this);

			if (this.parameters.Count != 0)
				ProcessParameters ();
		}

		public ToplevelBlock (Location loc) : this (null, (Flags) 0, null, loc)
		{
		}

		protected override void CloneTo (CloneContext clonectx, Statement t)
		{
			ToplevelBlock target = (ToplevelBlock) t;
			base.CloneTo (clonectx, t);

			if (parameters.Count != 0)
				target.parameter_info = new ToplevelParameterInfo [parameters.Count];
			for (int i = 0; i < parameters.Count; ++i)
				target.parameter_info [i] = new ToplevelParameterInfo (target, i);
		}

		public bool CheckError158 (string name, Location loc)
		{
			if (AnonymousChildren != null) {
				foreach (ToplevelBlock child in AnonymousChildren) {
					if (!child.CheckError158 (name, loc))
						return false;
				}
			}

			for (ToplevelBlock c = Container; c != null; c = c.Container) {
				if (!c.DoCheckError158 (name, loc))
					return false;
			}

			return true;
		}

		public virtual Expression GetTransparentIdentifier (string name)
		{
			return null;
		}

		void ProcessParameters ()
		{
			int n = parameters.Count;
			parameter_info = new ToplevelParameterInfo [n];
			for (int i = 0; i < n; ++i) {
				parameter_info [i] = new ToplevelParameterInfo (this, i);

				Parameter p = parameters [i];
				if (p == null)
					continue;

				string name = p.Name;
				LocalInfo vi = GetLocalInfo (name);
				if (vi != null) {
					Report.SymbolRelatedToPreviousError (vi.Location, name);
					Error_AlreadyDeclared (loc, name, "parent or current");
					continue;
				}

				ToplevelParameterInfo pi = Parent == null ? null : Parent.Toplevel.GetParameterInfo (name);
				if (pi != null) {
					Report.SymbolRelatedToPreviousError (pi.Location, name);
					Error_AlreadyDeclared (loc, name, "parent or current");
					continue;
				}

				AddKnownVariable (name, parameter_info [i]);
			}

			// mark this block as "used" so that we create local declarations in a sub-block
			// FIXME: This appears to uncover a lot of bugs
			//this.Use ();
		}

		bool DoCheckError158 (string name, Location loc)
		{
			LabeledStatement s = LookupLabel (name);
			if (s != null) {
				Report.SymbolRelatedToPreviousError (s.loc, s.Name);
				Error_158 (name, loc);
				return false;
			}

			return true;
		}

		public RootScopeInfo CreateRootScope (TypeContainer host)
		{
			if (root_scope != null)
				return root_scope;

			if (Container == null)
				root_scope = new RootScopeInfo (
					this, host, generic, StartLocation);

			if (scope_info != null)
				throw new InternalErrorException ();

			scope_info = root_scope;
			return root_scope;
		}

		public override Expression CreateExpressionTree (EmitContext ec)
		{
			return ((Statement) statements [0]).CreateExpressionTree (ec);
		}

		public void CreateIteratorHost (RootScopeInfo root)
		{
			Report.Debug (64, "CREATE ITERATOR HOST", this, root, Parent, root_scope);

			if (Parent != null || root_scope != null)
				throw new InternalErrorException ();

			scope_info = root_scope = root;
		}

		public RootScopeInfo RootScope {
			get {
				if (root_scope != null)
					return root_scope;
				else if (Container != null)
					return Container.RootScope;
				else
					return null;
			}
		}

		public FlowBranchingToplevel TopLevelBranching {
			get { return top_level_branching; }
		}

		//
		// This is used if anonymous methods are used inside an iterator
		// (see 2test-22.cs for an example).
		//
		// The AnonymousMethod is created while parsing - at a time when we don't
		// know yet that we're inside an iterator, so it's `Container' is initially
		// null.  Later on, when resolving the iterator, we need to move the
		// anonymous method into that iterator.
		//
		public void ReParent (ToplevelBlock new_parent)
		{
			if ((flags & Flags.VariablesInitialized) != 0)
				throw new InternalErrorException ("block has already been resolved");

			Parent = new_parent;
		}

		//
		// Returns a `ParameterReference' for the given name, or null if there
		// is no such parameter
		//
		public ParameterReference GetParameterReference (string name, Location loc)
		{
			ToplevelParameterInfo p = GetParameterInfo (name);
			return p == null ? null : new ParameterReference (this, p, loc);
		}

		public ToplevelParameterInfo GetParameterInfo (string name)
		{
			int idx;
			for (ToplevelBlock t = this; t != null; t = t.Container) {
				Parameter par = t.Parameters.GetParameterByName (name, out idx);
				if (par != null)
					return t.parameter_info [idx];
			}
			return null;
		}

		LocalInfo this_variable = null;

		// <summary>
		//   Returns the "this" instance variable of this block.
		//   See AddThisVariable() for more information.
		// </summary>
		public LocalInfo ThisVariable {
			get { return this_variable; }
		}

		// <summary>
		//   This is used by non-static `struct' constructors which do not have an
		//   initializer - in this case, the constructor must initialize all of the
		//   struct's fields.  To do this, we add a "this" variable and use the flow
		//   analysis code to ensure that it's been fully initialized before control
		//   leaves the constructor.
		// </summary>
		public LocalInfo AddThisVariable (DeclSpace ds, Location l)
		{
			if (this_variable == null) {
				this_variable = new LocalInfo (ds, this, l);
				this_variable.Used = true;
				this_variable.IsThis = true;

				Variables.Add ("this", this_variable);
			}

			return this_variable;
		}

		public bool IsThisAssigned (EmitContext ec)
		{
			return this_variable == null || this_variable.IsThisAssigned (ec);
		}

		public bool ResolveMeta (EmitContext ec, Parameters ip)
		{
			int errors = Report.Errors;
			int orig_count = parameters.Count;

			if (top_level_branching != null)
				return true;

			if (ip != null)
				parameters = ip;

			// Assert: orig_count != parameter.Count => orig_count == 0
			if (orig_count != 0 && orig_count != parameters.Count)
				throw new InternalErrorException ("parameter information mismatch");

			int offset = Parent == null ? 0 : Parent.AssignableSlots;

			for (int i = 0; i < orig_count; ++i) {
				Parameter.Modifier mod = parameters.ParameterModifier (i);

				if ((mod & Parameter.Modifier.OUT) != Parameter.Modifier.OUT)
					continue;

				VariableInfo vi = new VariableInfo (ip, i, offset);
				parameter_info [i].VariableInfo = vi;
				offset += vi.Length;
			}

			ResolveMeta (ec, offset);

			top_level_branching = ec.StartFlowBranching (this);

			return Report.Errors == errors;
		}

		// <summary>
		//   Check whether all `out' parameters have been assigned.
		// </summary>
		public void CheckOutParameters (FlowBranching.UsageVector vector, Location loc)
		{
			if (vector.IsUnreachable)
				return;

			int n = parameter_info == null ? 0 : parameter_info.Length;

			for (int i = 0; i < n; i++) {
				VariableInfo var = parameter_info [i].VariableInfo;

				if (var == null)
					continue;

				if (vector.IsAssigned (var, false))
					continue;

				Report.Error (177, loc, "The out parameter `{0}' must be assigned to before control leaves the current method",
					var.Name);
			}
		}

		public override void EmitMeta (EmitContext ec)
		{
			// Avoid declaring an IL variable for this_variable since it is not accessed
			// from the generated IL
			if (this_variable != null)
				Variables.Remove ("this");
			base.EmitMeta (ec);
			parameters.ResolveVariable (this);
		}

		protected override void EmitSymbolInfo (EmitContext ec)
		{
			if ((AnonymousContainer != null) && (AnonymousContainer.Scope != null))
				SymbolWriter.DefineScopeVariable (AnonymousContainer.Scope.ID);

			base.EmitSymbolInfo (ec);
		}

		public void MakeIterator (Iterator iterator)
		{
			flags |= Flags.IsIterator;

			Block block = new ExplicitBlock (this, flags, StartLocation, EndLocation);
			foreach (Statement stmt in statements)
				block.AddStatement (stmt);
			statements.Clear ();
			statements.Add (new MoveNextStatement (iterator, block));
		}

		protected class MoveNextStatement : Statement {
			Iterator iterator;
			Block block;

			public MoveNextStatement (Iterator iterator, Block block)
			{
				this.iterator = iterator;
				this.block = block;
				this.loc = iterator.Location;
			}

			public override bool Resolve (EmitContext ec)
			{
				ec.StartFlowBranching (iterator);
				bool ok = block.Resolve (ec);
				ec.EndFlowBranching ();
				return ok;
			}

			protected override void DoEmit (EmitContext ec)
			{
				iterator.EmitMoveNext (ec, block);
			}
		}

		public override string ToString ()
		{
			return String.Format ("{0} ({1}:{2}{3}:{4})", GetType (), ID, StartLocation,
					      root_scope, anonymous_container != null ?
					      anonymous_container.Scope : null);
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

		public object Converted {
			get {
				return converted;
			}
		}

		public Label GetILLabel (EmitContext ec)
		{
			if (!il_label_set){
				il_label = ec.ig.DefineLabel ();
				il_label_set = true;
			}
			return il_label;
		}

		public Label GetILLabelCode (EmitContext ec)
		{
			if (!il_label_code_set){
				il_label_code = ec.ig.DefineLabel ();
				il_label_code_set = true;
			}
			return il_label_code;
		}				
		
		//
		// Resolves the expression, reduces it to a literal if possible
		// and then converts it to the requested type.
		//
		public bool ResolveAndReduce (EmitContext ec, Type required_type, bool allow_nullable)
		{	
			Expression e = label.Resolve (ec);

			if (e == null)
				return false;

			Constant c = e as Constant;
			if (c == null){
				Report.Error (150, loc, "A constant value is expected");
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
			
			c = c.ImplicitConversionRequired (required_type, loc);
			if (c == null)
				return false;

			converted = c.GetValue ();
			return true;
		}

		public void Error_AlreadyOccurs (Type switch_type, SwitchLabel collision_with)
		{
			string label;
			if (converted == null)
				label = "default";
			else if (converted == NullStringCase)
				label = "null";
			else if (TypeManager.IsEnumType (switch_type)) 
				label = TypeManager.CSharpEnumValue (switch_type, converted);
			else
				label = converted.ToString ();
			
			Report.SymbolRelatedToPreviousError (collision_with.loc, null);
			Report.Error (152, loc, "The label `case {0}:' already occurs in this switch statement", label);
		}

		public SwitchLabel Clone (CloneContext clonectx)
		{
			return new SwitchLabel (label.Clone (clonectx), loc);
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

		public SwitchSection Clone (CloneContext clonectx)
		{
			ArrayList cloned_labels = new ArrayList ();

			foreach (SwitchLabel sl in cloned_labels)
				cloned_labels.Add (sl.Clone (clonectx));
			
			return new SwitchSection (cloned_labels, clonectx.LookupBlock (Block));
		}
	}
	
	public class Switch : Statement {
		public ArrayList Sections;
		public Expression Expr;

		/// <summary>
		///   Maps constants whose type type SwitchType to their  SwitchLabels.
		/// </summary>
		public IDictionary Elements;

		/// <summary>
		///   The governing switch type
		/// </summary>
		public Type SwitchType;

		//
		// Computed
		//
		Label default_target;
		Label null_target;
		Expression new_expr;
		bool is_constant;
		SwitchSection constant_section;
		SwitchSection default_section;

#if GMCS_SOURCE
		//
		// Nullable Types support for GMCS.
		//
		Nullable.Unwrap unwrap;

		protected bool HaveUnwrap {
			get { return unwrap != null; }
		}
#else
		protected bool HaveUnwrap {
			get { return false; }
		}
#endif

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
		Expression SwitchGoverningType (EmitContext ec, Expression expr)
		{
			Type t = expr.Type;

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
					TypeManager.string_type,
					TypeManager.bool_type
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
					Report.ExtraInformation (
						loc,
						String.Format ("reason: more than one conversion to an integral type exist for type {0}",
							       TypeManager.CSharpName (expr.Type)));
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
		bool CheckSwitch (EmitContext ec)
		{
			bool error = false;
			Elements = Sections.Count > 10 ? 
				(IDictionary)new Hashtable () : 
				(IDictionary)new ListDictionary ();
				
			foreach (SwitchSection ss in Sections){
				foreach (SwitchLabel sl in ss.Labels){
					if (sl.Label == null){
						if (default_section != null){
							sl.Error_AlreadyOccurs (SwitchType, (SwitchLabel)default_section.Labels [0]);
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
					try {
						Elements.Add (key, sl);
					} catch (ArgumentException) {
						sl.Error_AlreadyOccurs (SwitchType, (SwitchLabel)Elements [key]);
						error = true;
					}
				}
			}
			return !error;
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
				ulong ul = (ulong) k;
				if (ul < (1L<<32))
				{
					IntConstant.EmitInt (ig, unchecked ((int) ul));
					ig.Emit (OpCodes.Conv_U8);
				}
				else
				{
					LongConstant.EmitLong (ig, unchecked ((long) ul));
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
			public KeyBlock (long _first)
			{
				first = last = _first;
			}
			public long first;
			public long last;
			public ArrayList element_keys = null;
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
		void TableSwitchEmit (EmitContext ec, LocalBuilder val)
		{
			int element_count = Elements.Count;
			object [] element_keys = new object [element_count];
			Elements.Keys.CopyTo (element_keys, 0);
			Array.Sort (element_keys);

			// initialize the block list with one element per key
			ArrayList key_blocks = new ArrayList ();
			foreach (object key in element_keys)
				key_blocks.Add (new KeyBlock (System.Convert.ToInt64 (key)));

			KeyBlock current_kb;
			// iteratively merge the blocks while they are at least half full
			// there's probably a really cool way to do this with a tree...
			while (key_blocks.Count > 1)
			{
				ArrayList key_blocks_new = new ArrayList ();
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
				kb.element_keys = new ArrayList ();

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
			ILGenerator ig = ec.ig;
			Label lbl_end = ig.DefineLabel ();	// at the end ;-)
			Label lbl_default = ig.DefineLabel ();

			Type type_keys = null;
			if (element_keys.Length > 0)
				type_keys = element_keys [0].GetType ();	// used for conversions

			Type compare_type;
			
			if (TypeManager.IsEnumType (SwitchType))
				compare_type = TypeManager.GetEnumUnderlyingType (SwitchType);
			else
				compare_type = SwitchType;
			
			for (int iBlock = key_blocks.Count - 1; iBlock >= 0; --iBlock)
			{
				KeyBlock kb = ((KeyBlock) key_blocks [iBlock]);
				lbl_default = (iBlock == 0) ? DefaultTarget : ig.DefineLabel ();
				if (kb.Length <= 2)
				{
					foreach (object key in kb.element_keys)
					{
						ig.Emit (OpCodes.Ldloc, val);
						EmitObjectInteger (ig, key);
						SwitchLabel sl = (SwitchLabel) Elements [key];
						ig.Emit (OpCodes.Beq, sl.GetILLabel (ec));
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
						ig.Emit (OpCodes.Ldloc, val);
						EmitObjectInteger (ig, System.Convert.ChangeType (kb.first, type_keys));
						ig.Emit (OpCodes.Blt, lbl_default);
						ig.Emit (OpCodes.Ldloc, val);
						EmitObjectInteger (ig, System.Convert.ChangeType (kb.last, type_keys));
						ig.Emit (OpCodes.Bgt, lbl_default);

						// normalize range
						ig.Emit (OpCodes.Ldloc, val);
						if (kb.first != 0)
						{
							EmitObjectInteger (ig, System.Convert.ChangeType (kb.first, type_keys));
							ig.Emit (OpCodes.Sub);
						}
						ig.Emit (OpCodes.Conv_I4);	// assumes < 2^31 labels!
					}
					else
					{
						// normalize range
						ig.Emit (OpCodes.Ldloc, val);
						int first = (int) kb.first;
						if (first > 0)
						{
							IntConstant.EmitInt (ig, first);
							ig.Emit (OpCodes.Sub);
						}
						else if (first < 0)
						{
							IntConstant.EmitInt (ig, -first);
							ig.Emit (OpCodes.Add);
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
					ig.Emit (OpCodes.Switch, switch_labels);
				}

				// mark the default for this block
				if (iBlock != 0)
					ig.MarkLabel (lbl_default);
			}

			// TODO: find the default case and emit it here,
			//       to prevent having to do the following jump.
			//       make sure to mark other labels in the default section

			// the last default just goes to the end
			ig.Emit (OpCodes.Br, lbl_default);

			// now emit the code for the sections
			bool found_default = false;
			bool found_null = false;
			foreach (SwitchSection ss in Sections)
			{
				foreach (SwitchLabel sl in ss.Labels)
					if (sl.Converted == SwitchLabel.NullStringCase)
						found_null = true;
			}

			foreach (SwitchSection ss in Sections)
			{
				foreach (SwitchLabel sl in ss.Labels)
				{
					ig.MarkLabel (sl.GetILLabel (ec));
					ig.MarkLabel (sl.GetILLabelCode (ec));
					if (sl.Converted == SwitchLabel.NullStringCase)
						ig.MarkLabel (null_target);
					else if (sl.Label == null) {
						ig.MarkLabel (lbl_default);
						found_default = true;
						if (!found_null)
							ig.MarkLabel (null_target);
					}
				}
				ss.Block.Emit (ec);
			}
			
			if (!found_default) {
				ig.MarkLabel (lbl_default);
				if (HaveUnwrap && !found_null) {
					ig.MarkLabel (null_target);
				}
			}
			
			ig.MarkLabel (lbl_end);
		}
		//
		// This simple emit switch works, but does not take advantage of the
		// `switch' opcode. 
		// TODO: remove non-string logic from here
		// TODO: binary search strings?
		//
		void SimpleSwitchEmit (EmitContext ec, LocalBuilder val)
		{
			ILGenerator ig = ec.ig;
			Label end_of_switch = ig.DefineLabel ();
			Label next_test = ig.DefineLabel ();
			bool first_test = true;
			bool pending_goto_end = false;
			bool null_marked = false;
			bool null_found;
			int section_count = Sections.Count;

			// TODO: implement switch optimization for string by using Hashtable
			//if (SwitchType == TypeManager.string_type && section_count > 7)
			//	Console.WriteLine ("Switch optimization possible " + loc);

			ig.Emit (OpCodes.Ldloc, val);
			
			if (Elements.Contains (SwitchLabel.NullStringCase)){
				ig.Emit (OpCodes.Brfalse, null_target);
			} else
				ig.Emit (OpCodes.Brfalse, default_target);
			
			ig.Emit (OpCodes.Ldloc, val);
			ig.Emit (OpCodes.Call, TypeManager.string_isinterned_string);
			ig.Emit (OpCodes.Stloc, val);

			for (int section = 0; section < section_count; section++){
				SwitchSection ss = (SwitchSection) Sections [section];

				if (ss == default_section)
					continue;

				Label sec_begin = ig.DefineLabel ();

				ig.Emit (OpCodes.Nop);

				if (pending_goto_end)
					ig.Emit (OpCodes.Br, end_of_switch);

				int label_count = ss.Labels.Count;
				null_found = false;
				for (int label = 0; label < label_count; label++){
					SwitchLabel sl = (SwitchLabel) ss.Labels [label];
					ig.MarkLabel (sl.GetILLabel (ec));
					
					if (!first_test){
						ig.MarkLabel (next_test);
						next_test = ig.DefineLabel ();
					}
					//
					// If we are the default target
					//
					if (sl.Label != null){
						object lit = sl.Converted;

						if (lit == SwitchLabel.NullStringCase){
							null_found = true;
							if (label + 1 == label_count)
								ig.Emit (OpCodes.Br, next_test);
							continue;
						}
						
						ig.Emit (OpCodes.Ldloc, val);
						ig.Emit (OpCodes.Ldstr, (string)lit);
						if (label_count == 1)
							ig.Emit (OpCodes.Bne_Un, next_test);
						else {
							if (label+1 == label_count)
								ig.Emit (OpCodes.Bne_Un, next_test);
							else
								ig.Emit (OpCodes.Beq, sec_begin);
						}
					}
				}
				if (null_found) {
					ig.MarkLabel (null_target);
					null_marked = true;
				}
				ig.MarkLabel (sec_begin);
				foreach (SwitchLabel sl in ss.Labels)
					ig.MarkLabel (sl.GetILLabelCode (ec));

				ss.Block.Emit (ec);
				pending_goto_end = !ss.Block.HasRet;
				first_test = false;
			}
			ig.MarkLabel (next_test);
			ig.MarkLabel (default_target);
			if (!null_marked)
				ig.MarkLabel (null_target);
			if (default_section != null)
				default_section.Block.Emit (ec);
			ig.MarkLabel (end_of_switch);
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

		public override bool Resolve (EmitContext ec)
		{
			Expr = Expr.Resolve (ec);
			if (Expr == null)
				return false;

			new_expr = SwitchGoverningType (ec, Expr);

#if GMCS_SOURCE
			if ((new_expr == null) && TypeManager.IsNullableType (Expr.Type)) {
				unwrap = Nullable.Unwrap.Create (Expr, ec);
				if (unwrap == null)
					return false;

				new_expr = SwitchGoverningType (ec, unwrap);
			}
#endif

			if (new_expr == null){
				Report.Error (151, loc, "A value of an integral type or string expected for switch");
				return false;
			}

			// Validate switch.
			SwitchType = new_expr.Type;

			if (RootContext.Version == LanguageVersion.ISO_1 && SwitchType == TypeManager.bool_type) {
				Report.FeatureIsNotAvailable (loc, "switch expression of boolean type");
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

			is_constant = new_expr is Constant;
			if (is_constant) {
				object key = ((Constant) new_expr).GetValue ();
				SwitchLabel label = (SwitchLabel) Elements [key];

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

			if (TypeManager.string_isinterned_string == null) {
				TypeManager.string_isinterned_string = TypeManager.GetPredefinedMethod (TypeManager.string_type,
					"IsInterned", loc, TypeManager.string_type);
			}

			return ok;
		}
		
		protected override void DoEmit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;

			default_target = ig.DefineLabel ();
			null_target = ig.DefineLabel ();

			// Store variable for comparission purposes
			LocalBuilder value;
			if (HaveUnwrap) {
				value = ig.DeclareLocal (SwitchType);
#if GMCS_SOURCE
				unwrap.EmitCheck (ec);
				ig.Emit (OpCodes.Brfalse, null_target);
				new_expr.Emit (ec);
				ig.Emit (OpCodes.Stloc, value);
#endif
			} else if (!is_constant) {
				value = ig.DeclareLocal (SwitchType);
				new_expr.Emit (ec);
				ig.Emit (OpCodes.Stloc, value);
			} else
				value = null;

			//
			// Setup the codegen context
			//
			Label old_end = ec.LoopEnd;
			Switch old_switch = ec.Switch;
			
			ec.LoopEnd = ig.DefineLabel ();
			ec.Switch = this;

			// Emit Code.
			if (is_constant) {
				if (constant_section != null)
					constant_section.Block.Emit (ec);
			} else if (SwitchType == TypeManager.string_type)
				SimpleSwitchEmit (ec, value);
			else
				TableSwitchEmit (ec, value);

			// Restore context state. 
			ig.MarkLabel (ec.LoopEnd);

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
			target.Sections = new ArrayList ();
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
				resume_point = ec.ig.DefineLabel ();
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

	public abstract class ExceptionStatement : ResumableStatement
	{
		bool code_follows;

		protected abstract void EmitPreTryBody (EmitContext ec);
		protected abstract void EmitTryBody (EmitContext ec);
		protected abstract void EmitFinallyBody (EmitContext ec);

		protected sealed override void DoEmit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;

			EmitPreTryBody (ec);

			if (resume_points != null) {
				IntConstant.EmitInt (ig, (int) Iterator.State.Running);
				ig.Emit (OpCodes.Stloc, ec.CurrentIterator.CurrentPC);
			}

			ig.BeginExceptionBlock ();

			if (resume_points != null) {
				ig.MarkLabel (resume_point);

				// For normal control flow, we want to fall-through the Switch
				// So, we use CurrentPC rather than the $PC field, and initialize it to an outside value above
				ig.Emit (OpCodes.Ldloc, ec.CurrentIterator.CurrentPC);
				IntConstant.EmitInt (ig, first_resume_pc);
				ig.Emit (OpCodes.Sub);

				Label [] labels = new Label [resume_points.Count];
				for (int i = 0; i < resume_points.Count; ++i)
					labels [i] = ((ResumableStatement) resume_points [i]).PrepareForEmit (ec);
				ig.Emit (OpCodes.Switch, labels);
			}

			EmitTryBody (ec);

			ig.BeginFinallyBlock ();

			Label start_finally = ec.ig.DefineLabel ();
			if (resume_points != null) {
				ig.Emit (OpCodes.Ldloc, ec.CurrentIterator.SkipFinally);
				ig.Emit (OpCodes.Brfalse_S, start_finally);
				ig.Emit (OpCodes.Endfinally);
			}

			ig.MarkLabel (start_finally);
			EmitFinallyBody (ec);

			ig.EndExceptionBlock ();
		}

		public void SomeCodeFollows ()
		{
			code_follows = true;
		}

		protected void ResolveReachability (EmitContext ec)
		{
			// System.Reflection.Emit automatically emits a 'leave' at the end of a try clause
			// So, ensure there's some IL code after this statement.
			if (!code_follows && resume_points == null && ec.CurrentBranching.CurrentUsageVector.IsUnreachable)
				ec.NeedReturnLabel ();

		}

		ArrayList resume_points;
		int first_resume_pc;
		public void AddResumePoint (ResumableStatement stmt, Location loc, int pc)
		{
			if (resume_points == null) {
				resume_points = new ArrayList ();
				first_resume_pc = pc;
			}

			if (pc != first_resume_pc + resume_points.Count)
				throw new InternalErrorException ("missed an intervening AddResumePoint?");

			resume_points.Add (stmt);
		}

		Label dispose_try_block;
		bool prepared_for_dispose, emitted_dispose;
		public override Label PrepareForDispose (EmitContext ec, Label end)
		{
			if (!prepared_for_dispose) {
				prepared_for_dispose = true;
				dispose_try_block = ec.ig.DefineLabel ();
			}
			return dispose_try_block;
		}

		public override void EmitForDispose (EmitContext ec, Iterator iterator, Label end, bool have_dispatcher)
		{
			if (emitted_dispose)
				return;

			emitted_dispose = true;

			ILGenerator ig = ec.ig;

			Label end_of_try = ig.DefineLabel ();

			// Ensure that the only way we can get into this code is through a dispatcher
			if (have_dispatcher)
				ig.Emit (OpCodes.Br, end);

			ig.BeginExceptionBlock ();

			ig.MarkLabel (dispose_try_block);

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
					ig.Emit (OpCodes.Ldloc, iterator.CurrentPC);
					IntConstant.EmitInt (ig, first_resume_pc);
					ig.Emit (OpCodes.Sub);
					ig.Emit (OpCodes.Switch, labels);
					//SymbolWriter.EndIteratorDispatcher (ec.ig);
				}

				foreach (ResumableStatement s in resume_points)
					s.EmitForDispose (ec, iterator, end_of_try, emit_dispatcher);
			}

			ig.MarkLabel (end_of_try);

			ig.BeginFinallyBlock ();

			EmitFinallyBody (ec);

			ig.EndExceptionBlock ();
		}
	}

	public class Lock : ExceptionStatement {
		Expression expr;
		public Statement Statement;
		TemporaryVariable temp;
			
		public Lock (Expression expr, Statement stmt, Location l)
		{
			this.expr = expr;
			Statement = stmt;
			loc = l;
		}

		public override bool Resolve (EmitContext ec)
		{
			expr = expr.Resolve (ec);
			if (expr == null)
				return false;

			if (expr.Type.IsValueType){
				Report.Error (185, loc,
					      "`{0}' is not a reference type as required by the lock statement",
					      TypeManager.CSharpName (expr.Type));
				return false;
			}

			ec.StartFlowBranching (this);
			bool ok = Statement.Resolve (ec);
			ec.EndFlowBranching ();

			ResolveReachability (ec);

			// Avoid creating libraries that reference the internal
			// mcs NullType:
			Type t = expr.Type;
			if (t == TypeManager.null_type)
				t = TypeManager.object_type;
			
			temp = new TemporaryVariable (t, loc);
			temp.Resolve (ec);

			if (TypeManager.void_monitor_enter_object == null || TypeManager.void_monitor_exit_object == null) {
				Type monitor_type = TypeManager.CoreLookupType ("System.Threading", "Monitor", Kind.Class, true);
				TypeManager.void_monitor_enter_object = TypeManager.GetPredefinedMethod (
					monitor_type, "Enter", loc, TypeManager.object_type);
				TypeManager.void_monitor_exit_object = TypeManager.GetPredefinedMethod (
					monitor_type, "Exit", loc, TypeManager.object_type);
			}
			
			return ok;
		}
		
		protected override void EmitPreTryBody (EmitContext ec)
		{
			ILGenerator ig = ec.ig;

			temp.Store (ec, expr);
			temp.Emit (ec);
			ig.Emit (OpCodes.Call, TypeManager.void_monitor_enter_object);
		}

		protected override void EmitTryBody (EmitContext ec)
		{
			Statement.Emit (ec);
		}

		protected override void EmitFinallyBody (EmitContext ec)
		{
			temp.Emit (ec);
			ec.ig.Emit (OpCodes.Call, TypeManager.void_monitor_exit_object);
		}
		
		protected override void CloneTo (CloneContext clonectx, Statement t)
		{
			Lock target = (Lock) t;

			target.expr = expr.Clone (clonectx);
			target.Statement = Statement.Clone (clonectx);
		}
	}

	public class Unchecked : Statement {
		public Block Block;
		
		public Unchecked (Block b)
		{
			Block = b;
			b.Unchecked = true;
		}

		public override bool Resolve (EmitContext ec)
		{
			using (ec.With (EmitContext.Flags.AllCheckStateFlags, false))
				return Block.Resolve (ec);
		}
		
		protected override void DoEmit (EmitContext ec)
		{
			using (ec.With (EmitContext.Flags.AllCheckStateFlags, false))
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
		
		public Checked (Block b)
		{
			Block = b;
			b.Unchecked = false;
		}

		public override bool Resolve (EmitContext ec)
		{
			using (ec.With (EmitContext.Flags.AllCheckStateFlags, true))
			        return Block.Resolve (ec);
		}

		protected override void DoEmit (EmitContext ec)
		{
			using (ec.With (EmitContext.Flags.AllCheckStateFlags, true))
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

		public Unsafe (Block b)
		{
			Block = b;
			Block.Unsafe = true;
		}

		public override bool Resolve (EmitContext ec)
		{
			using (ec.With (EmitContext.Flags.InUnsafe, true))
				return Block.Resolve (ec);
		}
		
		protected override void DoEmit (EmitContext ec)
		{
			using (ec.With (EmitContext.Flags.InUnsafe, true))
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
	public class Fixed : Statement {
		Expression type;
		ArrayList declarators;
		Statement statement;
		Type expr_type;
		Emitter[] data;
		bool has_ret;

		abstract class Emitter
		{
			protected LocalInfo vi;
			protected Expression converted;

			protected Emitter (Expression expr, LocalInfo li)
			{
				converted = expr;
				vi = li;
			}

			public abstract void Emit (EmitContext ec);
			public abstract void EmitExit (EmitContext ec);
		}

		class ExpressionEmitter : Emitter {
			public ExpressionEmitter (Expression converted, LocalInfo li) :
				base (converted, li)
			{
			}

			public override void Emit (EmitContext ec) {
				//
				// Store pointer in pinned location
				//
				converted.Emit (ec);
				vi.Variable.EmitAssign (ec);
			}

			public override void EmitExit (EmitContext ec)
			{
				ec.ig.Emit (OpCodes.Ldc_I4_0);
				ec.ig.Emit (OpCodes.Conv_U);
				vi.Variable.EmitAssign (ec);
			}
		}

		class StringEmitter : Emitter {
			LocalBuilder pinned_string;
			Location loc;

			public StringEmitter (Expression expr, LocalInfo li, Location loc):
				base (expr, li)
			{
				this.loc = loc;
			}

			public override void Emit (EmitContext ec)
			{
				ILGenerator ig = ec.ig;
				pinned_string = TypeManager.DeclareLocalPinned (ig, TypeManager.string_type);
					
				converted.Emit (ec);
				ig.Emit (OpCodes.Stloc, pinned_string);

				Expression sptr = new StringPtr (pinned_string, loc);
				converted = Convert.ImplicitConversionRequired (
					ec, sptr, vi.VariableType, loc);
					
				if (converted == null)
					return;

				converted.Emit (ec);
				vi.Variable.EmitAssign (ec);
			}

			public override void EmitExit (EmitContext ec)
			{
				ec.ig.Emit (OpCodes.Ldnull);
				ec.ig.Emit (OpCodes.Stloc, pinned_string);
			}
		}

		public Fixed (Expression type, ArrayList decls, Statement stmt, Location l)
		{
			this.type = type;
			declarators = decls;
			statement = stmt;
			loc = l;
		}

		public Statement Statement {
			get { return statement; }
		}

		public override bool Resolve (EmitContext ec)
		{
			if (!ec.InUnsafe){
				Expression.UnsafeError (loc);
				return false;
			}
			
			TypeExpr texpr = type.ResolveAsContextualType (ec, false);
			if (texpr == null) {
				if (type is VarExpr)
					Report.Error (821, type.Location, "A fixed statement cannot use an implicitly typed local variable");

				return false;
			}

			expr_type = texpr.Type;

			data = new Emitter [declarators.Count];

			if (!expr_type.IsPointer){
				Report.Error (209, loc, "The type of locals declared in a fixed statement must be a pointer type");
				return false;
			}
			
			int i = 0;
			foreach (Pair p in declarators){
				LocalInfo vi = (LocalInfo) p.First;
				Expression e = (Expression) p.Second;
				
				vi.VariableInfo.SetAssigned (ec);
				vi.SetReadOnlyContext (LocalInfo.ReadOnlyContext.Fixed);

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

				if (e is Cast){
					Report.Error (254, loc, "The right hand side of a fixed statement assignment may not be a cast expression");
					return false;
				}
				
				//
				// Case 1: & object.
				//
				if (e is Unary && ((Unary) e).Oper == Unary.Operator.AddressOf){
					Expression child = ((Unary) e).Expr;

					if (child is ParameterReference || child is LocalVariableReference){
						Report.Error (
							213, loc, 
							"No need to use fixed statement for parameters or " +
							"local variable declarations (address is already " +
							"fixed)");
						return false;
					}

					ec.InFixedInitializer = true;
					e = e.Resolve (ec);
					ec.InFixedInitializer = false;
					if (e == null)
						return false;

					child = ((Unary) e).Expr;
					
					if (!TypeManager.VerifyUnManaged (child.Type, loc))
						return false;

					if (!Convert.ImplicitConversionExists (ec, e, expr_type)) {
						e.Error_ValueCannotBeConverted (ec, e.Location, expr_type, false);
						return false;
					}

					data [i] = new ExpressionEmitter (e, vi);
					i++;

					continue;
				}

				ec.InFixedInitializer = true;
				e = e.Resolve (ec);
				ec.InFixedInitializer = false;
				if (e == null)
					return false;

				//
				// Case 2: Array
				//
				if (e.Type.IsArray){
					Type array_type = TypeManager.GetElementType (e.Type);
					
					//
					// Provided that array_type is unmanaged,
					//
					if (!TypeManager.VerifyUnManaged (array_type, loc))
						return false;

					//
					// and T* is implicitly convertible to the
					// pointer type given in the fixed statement.
					//
					ArrayPtr array_ptr = new ArrayPtr (e, array_type, loc);
					
					Expression converted = Convert.ImplicitConversionRequired (
						ec, array_ptr, vi.VariableType, loc);
					if (converted == null)
						return false;
					
					//
					// fixed (T* e_ptr = (e == null || e.Length == 0) ? null : converted [0])
					//
					converted = new Conditional (new Binary (Binary.Operator.LogicalOr,
						new Binary (Binary.Operator.Equality, e, new NullLiteral (loc)),
						new Binary (Binary.Operator.Equality, new MemberAccess (e, "Length"), new IntConstant (0, loc))),
							NullPointer.Null,
							converted);

					converted = converted.Resolve (ec);					

					data [i] = new ExpressionEmitter (converted, vi);
					i++;

					continue;
				}

				//
				// Case 3: string
				//
				if (e.Type == TypeManager.string_type){
					data [i] = new StringEmitter (e, vi, loc);
					i++;
					continue;
				}

				// Case 4: fixed buffer
				FixedBufferPtr fixed_buffer_ptr = e as FixedBufferPtr;
				if (fixed_buffer_ptr != null) {
					data [i++] = new ExpressionEmitter (fixed_buffer_ptr, vi);
					continue;
				}

				//
				// For other cases, flag a `this is already fixed expression'
				//
				if (e is LocalVariableReference || e is ParameterReference ||
				    Convert.ImplicitConversionExists (ec, e, vi.VariableType)){
				    
					Report.Error (245, loc, "right hand expression is already fixed, no need to use fixed statement ");
					return false;
				}

				Report.Error (245, loc, "Fixed statement only allowed on strings, arrays or address-of expressions");
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
			for (int i = 0; i < data.Length; i++) {
				data [i].Emit (ec);
			}

			statement.Emit (ec);

			if (has_ret)
				return;

			//
			// Clear the pinned variable
			//
			for (int i = 0; i < data.Length; i++) {
				data [i].EmitExit (ec);
			}
		}

		protected override void CloneTo (CloneContext clonectx, Statement t)
		{
			Fixed target = (Fixed) t;

			target.type = type.Clone (clonectx);
			target.declarators = new ArrayList (declarators.Count);
			foreach (Pair p in declarators) {
				LocalInfo vi = (LocalInfo) p.First;
				Expression e = (Expression) p.Second;

				target.declarators.Add (
					new Pair (clonectx.LookupVariable (vi), e.Clone (clonectx)));				
			}
			
			target.statement = statement.Clone (clonectx);
		}
	}
	
	public class Catch : Statement {
		public readonly string Name;
		public Block  Block;
		public Block  VarBlock;

		Expression type_expr;
		Type type;
		
		public Catch (Expression type, string name, Block block, Block var_block, Location l)
		{
			type_expr = type;
			Name = name;
			Block = block;
			VarBlock = var_block;
			loc = l;
		}

		public Type CatchType {
			get {
				return type;
			}
		}

		public bool IsGeneral {
			get {
				return type_expr == null;
			}
		}

		protected override void DoEmit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;

			if (CatchType != null)
				ig.BeginCatchBlock (CatchType);
			else
				ig.BeginCatchBlock (TypeManager.object_type);

			if (VarBlock != null)
				VarBlock.Emit (ec);

			if (Name != null) {
				LocalInfo vi = Block.GetLocalInfo (Name);
				if (vi == null)
					throw new Exception ("Variable does not exist in this block");

				if (vi.Variable.NeedsTemporary) {
					LocalBuilder e = ig.DeclareLocal (vi.VariableType);
					ig.Emit (OpCodes.Stloc, e);

					vi.Variable.EmitInstance (ec);
					ig.Emit (OpCodes.Ldloc, e);
					vi.Variable.EmitAssign (ec);
				} else
					vi.Variable.EmitAssign (ec);
			} else
				ig.Emit (OpCodes.Pop);

			Block.Emit (ec);
		}

		public override bool Resolve (EmitContext ec)
		{
			using (ec.With (EmitContext.Flags.InCatch, true)) {
				if (type_expr != null) {
					TypeExpr te = type_expr.ResolveAsTypeTerminal (ec, false);
					if (te == null)
						return false;

					type = te.Type;

					if (type != TypeManager.exception_type && !TypeManager.IsSubclassOf (type, TypeManager.exception_type)){
						Error (155, "The type caught or thrown must be derived from System.Exception");
						return false;
					}
				} else
					type = null;

				if (!Block.Resolve (ec))
					return false;

				// Even though VarBlock surrounds 'Block' we resolve it later, so that we can correctly
				// emit the "unused variable" warnings.
				if (VarBlock != null)
					return VarBlock.Resolve (ec);

				return true;
			}
		}

		protected override void CloneTo (CloneContext clonectx, Statement t)
		{
			Catch target = (Catch) t;

			if (type_expr != null)
				target.type_expr = type_expr.Clone (clonectx);
			if (VarBlock != null)
				target.VarBlock = clonectx.LookupBlock (VarBlock);			
			target.Block = clonectx.LookupBlock (Block);
		}
	}

	public class TryFinally : ExceptionStatement {
		Statement stmt;
		Block fini;

		public TryFinally (Statement stmt, Block fini, Location l)
		{
			this.stmt = stmt;
			this.fini = fini;
			loc = l;
		}

		public override bool Resolve (EmitContext ec)
		{
			bool ok = true;

			ec.StartFlowBranching (this);

			if (!stmt.Resolve (ec))
				ok = false;

			if (ok)
				ec.CurrentBranching.CreateSibling (fini, FlowBranching.SiblingType.Finally);
			using (ec.With (EmitContext.Flags.InFinally, true)) {
				if (!fini.Resolve (ec))
					ok = false;
			}

			ec.EndFlowBranching ();

			ResolveReachability (ec);

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
		public ArrayList Specific;
		public Catch General;
		bool inside_try_finally, code_follows;

		public TryCatch (Block block, ArrayList catch_clauses, Location l, bool inside_try_finally)
		{
			this.Block = block;
			this.Specific = catch_clauses;
			this.General = null;
			this.inside_try_finally = inside_try_finally;

			for (int i = 0; i < catch_clauses.Count; ++i) {
				Catch c = (Catch) catch_clauses [i];
				if (c.IsGeneral) {
					if (i != catch_clauses.Count - 1)
						Report.Error (1017, c.loc, "Try statement already has an empty catch block");
					this.General = c;
					catch_clauses.RemoveAt (i);
					i--;
				}
			}

			loc = l;
		}

		public override bool Resolve (EmitContext ec)
		{
			bool ok = true;

			ec.StartFlowBranching (this);

			if (!Block.Resolve (ec))
				ok = false;

			Type[] prev_catches = new Type [Specific.Count];
			int last_index = 0;
			foreach (Catch c in Specific){
				ec.CurrentBranching.CreateSibling (c.Block, FlowBranching.SiblingType.Catch);

				if (c.Name != null) {
					LocalInfo vi = c.Block.GetLocalInfo (c.Name);
					if (vi == null)
						throw new Exception ();

					vi.VariableInfo = null;
				}

				if (!c.Resolve (ec))
					ok = false;

				Type resolved_type = c.CatchType;
				for (int ii = 0; ii < last_index; ++ii) {
					if (resolved_type == prev_catches [ii] || TypeManager.IsSubclassOf (resolved_type, prev_catches [ii])) {
						Report.Error (160, c.loc, "A previous catch clause already catches all exceptions of this or a super type `{0}'", prev_catches [ii].FullName);
						ok = false;
					}
				}

				prev_catches [last_index++] = resolved_type;
			}

			if (General != null) {
				if (CodeGen.Assembly.WrapNonExceptionThrows) {
					foreach (Catch c in Specific){
						if (c.CatchType == TypeManager.exception_type) {
							Report.Warning (1058, 1, c.loc, "A previous catch clause already catches all exceptions. All non-exceptions thrown will be wrapped in a `System.Runtime.CompilerServices.RuntimeWrappedException'");
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
			ILGenerator ig = ec.ig;

			if (!inside_try_finally)
				ig.BeginExceptionBlock ();

			Block.Emit (ec);

			foreach (Catch c in Specific)
				c.Emit (ec);

			if (General != null)
				General.Emit (ec);

			if (!inside_try_finally)
				ig.EndExceptionBlock ();
		}

		protected override void CloneTo (CloneContext clonectx, Statement t)
		{
			TryCatch target = (TryCatch) t;

			target.Block = clonectx.LookupBlock (Block);
			if (General != null)
				target.General = (Catch) General.Clone (clonectx);
			if (Specific != null){
				target.Specific = new ArrayList ();
				foreach (Catch c in Specific)
					target.Specific.Add (c.Clone (clonectx));
			}
		}
	}

	public class UsingTemporary : ExceptionStatement {
		TemporaryVariable local_copy;
		public Statement Statement;
		Expression expr;
		Type expr_type;

		public UsingTemporary (Expression expr, Statement stmt, Location l)
		{
			this.expr = expr;
			Statement = stmt;
			loc = l;
		}

		public override bool Resolve (EmitContext ec)
		{
			expr = expr.Resolve (ec);
			if (expr == null)
				return false;

			expr_type = expr.Type;

			if (!TypeManager.ImplementsInterface (expr_type, TypeManager.idisposable_type)) {
				if (Convert.ImplicitConversion (ec, expr, TypeManager.idisposable_type, loc) == null) {
					Using.Error_IsNotConvertibleToIDisposable (expr);
					return false;
				}
			}

			local_copy = new TemporaryVariable (expr_type, loc);
			local_copy.Resolve (ec);

			ec.StartFlowBranching (this);

			bool ok = Statement.Resolve (ec);

			ec.EndFlowBranching ();

			ResolveReachability (ec);

			if (TypeManager.void_dispose_void == null) {
				TypeManager.void_dispose_void = TypeManager.GetPredefinedMethod (
					TypeManager.idisposable_type, "Dispose", loc, Type.EmptyTypes);
			}

			return ok;
		}

		protected override void EmitPreTryBody (EmitContext ec)
		{
			local_copy.Store (ec, expr);
		}

		protected override void EmitTryBody (EmitContext ec)
		{
			Statement.Emit (ec);
		}

	        protected override void EmitFinallyBody (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			if (!expr_type.IsValueType) {
				Label skip = ig.DefineLabel ();
				local_copy.Emit (ec);
				ig.Emit (OpCodes.Brfalse, skip);
				local_copy.Emit (ec);
				ig.Emit (OpCodes.Callvirt, TypeManager.void_dispose_void);
				ig.MarkLabel (skip);
				return;
			}

			Expression ml = Expression.MemberLookup (
				ec.ContainerType, TypeManager.idisposable_type, expr_type,
				"Dispose", Location.Null);

			if (!(ml is MethodGroupExpr)) {
				local_copy.Emit (ec);
				ig.Emit (OpCodes.Box, expr_type);
				ig.Emit (OpCodes.Callvirt, TypeManager.void_dispose_void);
				return;
			}

			MethodInfo mi = null;

			foreach (MethodInfo mk in ((MethodGroupExpr) ml).Methods) {
				if (TypeManager.GetParameterData (mk).Count == 0) {
					mi = mk;
					break;
				}
			}

			if (mi == null) {
				Report.Error(-100, Mono.CSharp.Location.Null, "Internal error: No Dispose method which takes 0 parameters.");
				return;
			}

			local_copy.AddressOf (ec, AddressOp.Load);
			ig.Emit (OpCodes.Call, mi);
		}

		protected override void CloneTo (CloneContext clonectx, Statement t)
		{
			UsingTemporary target = (UsingTemporary) t;

			target.expr = expr.Clone (clonectx);
			target.Statement = Statement.Clone (clonectx);
		}
	}

	public class Using : ExceptionStatement {
		Statement stmt;
		public Statement EmbeddedStatement {
			get { return stmt is Using ? ((Using) stmt).EmbeddedStatement : stmt; }
		}

		Expression var;
		Expression init;

		Expression converted_var;
		ExpressionStatement assign;

		public Using (Expression var, Expression init, Statement stmt, Location l)
		{
			this.var = var;
			this.init = init;
			this.stmt = stmt;
			loc = l;
		}

		bool ResolveVariable (EmitContext ec)
		{
			ExpressionStatement a = new Assign (var, init, loc);
			a = a.ResolveStatement (ec);
			if (a == null)
				return false;

			assign = a;

			if (TypeManager.ImplementsInterface (a.Type, TypeManager.idisposable_type)) {
				converted_var = var;
				return true;
			}

			Expression e = Convert.ImplicitConversionStandard (ec, a, TypeManager.idisposable_type, var.Location);
			if (e == null) {
				Error_IsNotConvertibleToIDisposable (var);
				return false;
			}

			converted_var = e;

			return true;
		}

		static public void Error_IsNotConvertibleToIDisposable (Expression expr)
		{
			Report.SymbolRelatedToPreviousError (expr.Type);
			Report.Error (1674, expr.Location, "`{0}': type used in a using statement must be implicitly convertible to `System.IDisposable'",
				expr.GetSignatureForError ());
		}

		protected override void EmitPreTryBody (EmitContext ec)
		{
			assign.EmitStatement (ec);
		}

		protected override void EmitTryBody (EmitContext ec)
		{
			stmt.Emit (ec);
		}

		protected override void EmitFinallyBody (EmitContext ec)
		{
			ILGenerator ig = ec.ig;

			if (!var.Type.IsValueType) {
				Label skip = ig.DefineLabel ();
				var.Emit (ec);
				ig.Emit (OpCodes.Brfalse, skip);
				converted_var.Emit (ec);
				ig.Emit (OpCodes.Callvirt, TypeManager.void_dispose_void);
				ig.MarkLabel (skip);
			} else {
				Expression ml = Expression.MemberLookup(ec.ContainerType, TypeManager.idisposable_type, var.Type, "Dispose", Mono.CSharp.Location.Null);

				if (!(ml is MethodGroupExpr)) {
					var.Emit (ec);
					ig.Emit (OpCodes.Box, var.Type);
					ig.Emit (OpCodes.Callvirt, TypeManager.void_dispose_void);
				} else {
					MethodInfo mi = null;

					foreach (MethodInfo mk in ((MethodGroupExpr) ml).Methods) {
						if (TypeManager.GetParameterData (mk).Count == 0) {
							mi = mk;
							break;
						}
					}

					if (mi == null) {
						Report.Error(-100, Mono.CSharp.Location.Null, "Internal error: No Dispose method which takes 0 parameters.");
						return;
					}

					IMemoryLocation mloc = (IMemoryLocation) var;

					mloc.AddressOf (ec, AddressOp.Load);
					ig.Emit (OpCodes.Call, mi);
				}
			}
		}

		public override bool Resolve (EmitContext ec)
		{
			if (!ResolveVariable (ec))
				return false;

			ec.StartFlowBranching (this);

			bool ok = stmt.Resolve (ec);

			ec.EndFlowBranching ();

			ResolveReachability (ec);

			if (TypeManager.void_dispose_void == null) {
				TypeManager.void_dispose_void = TypeManager.GetPredefinedMethod (
					TypeManager.idisposable_type, "Dispose", loc, Type.EmptyTypes);
			}

			return ok;
		}

		protected override void CloneTo (CloneContext clonectx, Statement t)
		{
			Using target = (Using) t;

			target.var = var.Clone (clonectx);
			target.init = init.Clone (clonectx);
			target.stmt = stmt.Clone (clonectx);
		}
	}

	/// <summary>
	///   Implementation of the foreach C# statement
	/// </summary>
	public class Foreach : Statement {
		Expression type;
		Expression variable;
		Expression expr;
		Statement statement;
		ArrayForeach array;
		CollectionForeach collection;
		
		public Foreach (Expression type, LocalVariableReference var, Expression expr,
				Statement stmt, Location l)
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

		public override bool Resolve (EmitContext ec)
		{
			expr = expr.Resolve (ec);
			if (expr == null)
				return false;

			if (expr.IsNull) {
				Report.Error (186, loc, "Use of null is not valid in this context");
				return false;
			}

			if (expr.eclass == ExprClass.MethodGroup || expr is AnonymousMethodExpression) {
				Report.Error (446, expr.Location, "Foreach statement cannot operate on a `{0}'",
					expr.ExprClassName);
				return false;
			}

			//
			// We need an instance variable.  Not sure this is the best
			// way of doing this.
			//
			// FIXME: When we implement propertyaccess, will those turn
			// out to return values in ExprClass?  I think they should.
			//
			if (!(expr.eclass == ExprClass.Variable || expr.eclass == ExprClass.Value ||
			      expr.eclass == ExprClass.PropertyAccess || expr.eclass == ExprClass.IndexerAccess)){
				collection.Error_Enumerator ();
				return false;
			}

			if (expr.Type.IsArray) {
				array = new ArrayForeach (type, variable, expr, statement, loc);
				return array.Resolve (ec);
			}
			
			collection = new CollectionForeach (type, variable, expr, statement, loc);
			return collection.Resolve (ec);
		}

		protected override void DoEmit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			
			Label old_begin = ec.LoopBegin, old_end = ec.LoopEnd;
			ec.LoopBegin = ig.DefineLabel ();
			ec.LoopEnd = ig.DefineLabel ();

			if (collection != null)
				collection.Emit (ec);
			else
				array.Emit (ec);
			
			ec.LoopBegin = old_begin;
			ec.LoopEnd = old_end;
		}

		protected class ArrayCounter : TemporaryVariable
		{
			public ArrayCounter (Location loc)
				: base (TypeManager.int32_type, loc)
			{ }

			public void Initialize (EmitContext ec)
			{
				EmitThis (ec);
				ec.ig.Emit (OpCodes.Ldc_I4_0);
				EmitStore (ec);
			}

			public void Increment (EmitContext ec)
			{
				EmitThis (ec);
				Emit (ec);
				ec.ig.Emit (OpCodes.Ldc_I4_1);
				ec.ig.Emit (OpCodes.Add);
				EmitStore (ec);
			}
		}

		protected class ArrayForeach : Statement
		{
			Expression variable, expr, conv;
			Statement statement;
			Type array_type;
			Expression var_type;
			TemporaryVariable[] lengths;
			ArrayCounter[] counter;
			int rank;

			TemporaryVariable copy;
			Expression access;

			public ArrayForeach (Expression var_type, Expression var,
					     Expression expr, Statement stmt, Location l)
			{
				this.var_type = var_type;
				this.variable = var;
				this.expr = expr;
				statement = stmt;
				loc = l;
			}

			public override bool Resolve (EmitContext ec)
			{
				array_type = expr.Type;
				rank = array_type.GetArrayRank ();

				copy = new TemporaryVariable (array_type, loc);
				copy.Resolve (ec);

				counter = new ArrayCounter [rank];
				lengths = new TemporaryVariable [rank];

				ArrayList list = new ArrayList ();
				for (int i = 0; i < rank; i++) {
					counter [i] = new ArrayCounter (loc);
					counter [i].Resolve (ec);

					lengths [i] = new TemporaryVariable (TypeManager.int32_type, loc);
					lengths [i].Resolve (ec);

					list.Add (counter [i]);
				}

				access = new ElementAccess (copy, list).Resolve (ec);
				if (access == null)
					return false;

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

				variable = variable.ResolveLValue (ec, conv, loc);
				if (variable == null)
					ok = false;

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
				ILGenerator ig = ec.ig;

				copy.Store (ec, expr);

				Label[] test = new Label [rank];
				Label[] loop = new Label [rank];

				for (int i = 0; i < rank; i++) {
					test [i] = ig.DefineLabel ();
					loop [i] = ig.DefineLabel ();

					lengths [i].EmitThis (ec);
					((ArrayAccess) access).EmitGetLength (ec, i);
					lengths [i].EmitStore (ec);
				}

				for (int i = 0; i < rank; i++) {
					counter [i].Initialize (ec);

					ig.Emit (OpCodes.Br, test [i]);
					ig.MarkLabel (loop [i]);
				}

				((IAssignMethod) variable).EmitAssign (ec, conv, false, false);

				statement.Emit (ec);

				ig.MarkLabel (ec.LoopBegin);

				for (int i = rank - 1; i >= 0; i--){
					counter [i].Increment (ec);

					ig.MarkLabel (test [i]);
					counter [i].Emit (ec);
					lengths [i].Emit (ec);
					ig.Emit (OpCodes.Blt, loop [i]);
				}

				ig.MarkLabel (ec.LoopEnd);
			}
		}

		protected class CollectionForeach : Statement
		{
			Expression variable, expr;
			Statement statement;

			TemporaryVariable enumerator;
			Expression init;
			Statement loop;
			Statement wrapper;

			MethodGroupExpr get_enumerator;
			PropertyExpr get_current;
			MethodInfo move_next;
			Expression var_type;
			Type enumerator_type;
			bool enumerator_found;

			public CollectionForeach (Expression var_type, Expression var,
						  Expression expr, Statement stmt, Location l)
			{
				this.var_type = var_type;
				this.variable = var;
				this.expr = expr;
				statement = stmt;
				loc = l;
			}

			bool GetEnumeratorFilter (EmitContext ec, MethodInfo mi)
			{
				Type return_type = mi.ReturnType;

				if ((return_type == TypeManager.ienumerator_type) && (mi.DeclaringType == TypeManager.string_type))
					//
					// Apply the same optimization as MS: skip the GetEnumerator
					// returning an IEnumerator, and use the one returning a 
					// CharEnumerator instead. This allows us to avoid the 
					// try-finally block and the boxing.
					//
					return false;

				//
				// Ok, we can access it, now make sure that we can do something
				// with this `GetEnumerator'
				//

				if (return_type == TypeManager.ienumerator_type ||
				    TypeManager.ienumerator_type.IsAssignableFrom (return_type) ||
				    (!RootContext.StdLib && TypeManager.ImplementsInterface (return_type, TypeManager.ienumerator_type))) {
					//
					// If it is not an interface, lets try to find the methods ourselves.
					// For example, if we have:
					// public class Foo : IEnumerator { public bool MoveNext () {} public int Current { get {}}}
					// We can avoid the iface call. This is a runtime perf boost.
					// even bigger if we have a ValueType, because we avoid the cost
					// of boxing.
					//
					// We have to make sure that both methods exist for us to take
					// this path. If one of the methods does not exist, we will just
					// use the interface. Sadly, this complex if statement is the only
					// way I could do this without a goto
					//

					if (TypeManager.bool_movenext_void == null) {
						TypeManager.bool_movenext_void = TypeManager.GetPredefinedMethod (
							TypeManager.ienumerator_type, "MoveNext", loc, Type.EmptyTypes);
					}

					if (TypeManager.ienumerator_getcurrent == null) {
						TypeManager.ienumerator_getcurrent = TypeManager.GetPredefinedProperty (
							TypeManager.ienumerator_type, "Current", loc, TypeManager.object_type);
					}

#if GMCS_SOURCE
					//
					// Prefer a generic enumerator over a non-generic one.
					//
					if (return_type.IsInterface && return_type.IsGenericType) {
						enumerator_type = return_type;
						if (!FetchGetCurrent (ec, return_type))
							get_current = new PropertyExpr (
								ec.ContainerType, TypeManager.ienumerator_getcurrent, loc);
						if (!FetchMoveNext (return_type))
							move_next = TypeManager.bool_movenext_void;
						return true;
					}
#endif

					if (return_type.IsInterface ||
					    !FetchMoveNext (return_type) ||
					    !FetchGetCurrent (ec, return_type)) {
						enumerator_type = return_type;
						move_next = TypeManager.bool_movenext_void;
						get_current = new PropertyExpr (
							ec.ContainerType, TypeManager.ienumerator_getcurrent, loc);
						return true;
					}
				} else {
					//
					// Ok, so they dont return an IEnumerable, we will have to
					// find if they support the GetEnumerator pattern.
					//

					if (TypeManager.HasElementType (return_type) || !FetchMoveNext (return_type) || !FetchGetCurrent (ec, return_type)) {
						Report.Error (202, loc, "foreach statement requires that the return type `{0}' of `{1}' must have a suitable public MoveNext method and public Current property",
							TypeManager.CSharpName (return_type), TypeManager.CSharpSignature (mi));
						return false;
					}
				}

				enumerator_type = return_type;

				return true;
			}

			//
			// Retrieves a `public bool MoveNext ()' method from the Type `t'
			//
			bool FetchMoveNext (Type t)
			{
				MemberList move_next_list;

				move_next_list = TypeContainer.FindMembers (
					t, MemberTypes.Method,
					BindingFlags.Public | BindingFlags.Instance,
					Type.FilterName, "MoveNext");
				if (move_next_list.Count == 0)
					return false;

				foreach (MemberInfo m in move_next_list){
					MethodInfo mi = (MethodInfo) m;
				
					if ((TypeManager.GetParameterData (mi).Count == 0) &&
					    TypeManager.TypeToCoreType (mi.ReturnType) == TypeManager.bool_type) {
						move_next = mi;
						return true;
					}
				}

				return false;
			}
		
			//
			// Retrieves a `public T get_Current ()' method from the Type `t'
			//
			bool FetchGetCurrent (EmitContext ec, Type t)
			{
				PropertyExpr pe = Expression.MemberLookup (
					ec.ContainerType, t, "Current", MemberTypes.Property,
					Expression.AllBindingFlags, loc) as PropertyExpr;
				if (pe == null)
					return false;

				get_current = pe;
				return true;
			}

			// 
			// Retrieves a `public void Dispose ()' method from the Type `t'
			//
			static MethodInfo FetchMethodDispose (Type t)
			{
				MemberList dispose_list;

				dispose_list = TypeContainer.FindMembers (
					t, MemberTypes.Method,
					BindingFlags.Public | BindingFlags.Instance,
					Type.FilterName, "Dispose");
				if (dispose_list.Count == 0)
					return null;

				foreach (MemberInfo m in dispose_list){
					MethodInfo mi = (MethodInfo) m;

					if (TypeManager.GetParameterData (mi).Count == 0){
						if (mi.ReturnType == TypeManager.void_type)
							return mi;
					}
				}
				return null;
			}

			public void Error_Enumerator ()
			{
				if (enumerator_found) {
					return;
				}

			    Report.Error (1579, loc,
					"foreach statement cannot operate on variables of type `{0}' because it does not contain a definition for `GetEnumerator' or is not accessible",
					TypeManager.CSharpName (expr.Type));
			}

			bool IsOverride (MethodInfo m)
			{
				m = (MethodInfo) TypeManager.DropGenericMethodArguments (m);

				if (!m.IsVirtual || ((m.Attributes & MethodAttributes.NewSlot) != 0))
					return false;
				if (m is MethodBuilder)
					return true;

				MethodInfo base_method = m.GetBaseDefinition ();
				return base_method != m;
			}

			bool TryType (EmitContext ec, Type t)
			{
				MethodGroupExpr mg = Expression.MemberLookup (
					ec.ContainerType, t, "GetEnumerator", MemberTypes.Method,
					Expression.AllBindingFlags, loc) as MethodGroupExpr;
				if (mg == null)
					return false;

				MethodInfo result = null;
				MethodInfo tmp_move_next = null;
				PropertyExpr tmp_get_cur = null;
				Type tmp_enumerator_type = enumerator_type;
				foreach (MethodInfo mi in mg.Methods) {
					if (TypeManager.GetParameterData (mi).Count != 0)
						continue;
			
					// Check whether GetEnumerator is public
					if ((mi.Attributes & MethodAttributes.Public) != MethodAttributes.Public)
						continue;

					if (IsOverride (mi))
						continue;

					enumerator_found = true;

					if (!GetEnumeratorFilter (ec, mi))
						continue;

					if (result != null) {
						if (TypeManager.IsGenericType (result.ReturnType)) {
							if (!TypeManager.IsGenericType (mi.ReturnType))
								continue;

							MethodBase mb = TypeManager.DropGenericMethodArguments (mi);
							Report.SymbolRelatedToPreviousError (t);
							Report.Error(1640, loc, "foreach statement cannot operate on variables of type `{0}' " +
								     "because it contains multiple implementation of `{1}'. Try casting to a specific implementation",
								     TypeManager.CSharpName (t), TypeManager.CSharpSignature (mb));
							return false;
						}

						// Always prefer generics enumerators
						if (!TypeManager.IsGenericType (mi.ReturnType)) {
							if (TypeManager.ImplementsInterface (mi.DeclaringType, result.DeclaringType) ||
							    TypeManager.ImplementsInterface (result.DeclaringType, mi.DeclaringType))
								continue;

							Report.SymbolRelatedToPreviousError (result);
							Report.SymbolRelatedToPreviousError (mi);
							Report.Warning (278, 2, loc, "`{0}' contains ambiguous implementation of `{1}' pattern. Method `{2}' is ambiguous with method `{3}'",
									TypeManager.CSharpName (t), "enumerable", TypeManager.CSharpSignature (result), TypeManager.CSharpSignature (mi));
							return false;
						}
					}
					result = mi;
					tmp_move_next = move_next;
					tmp_get_cur = get_current;
					tmp_enumerator_type = enumerator_type;
					if (mi.DeclaringType == t)
						break;
				}

				if (result != null) {
					move_next = tmp_move_next;
					get_current = tmp_get_cur;
					enumerator_type = tmp_enumerator_type;
					MethodInfo[] mi = new MethodInfo[] { (MethodInfo) result };
					get_enumerator = new MethodGroupExpr (mi, enumerator_type, loc);

					if (t != expr.Type) {
						expr = Convert.ExplicitConversion (
							ec, expr, t, loc);
						if (expr == null)
							throw new InternalErrorException ();
					}

					get_enumerator.InstanceExpression = expr;
					get_enumerator.IsBase = t != expr.Type;

					return true;
				}

				return false;
			}		

			bool ProbeCollectionType (EmitContext ec, Type t)
			{
				int errors = Report.Errors;
				for (Type tt = t; tt != null && tt != TypeManager.object_type;){
					if (TryType (ec, tt))
						return true;
					tt = tt.BaseType;
				}

				if (Report.Errors > errors)
					return false;

				//
				// Now try to find the method in the interfaces
				//
				Type [] ifaces = TypeManager.GetInterfaces (t);
				foreach (Type i in ifaces){
					if (TryType (ec, i))
						return true;
				}

				return false;
			}

			public override bool Resolve (EmitContext ec)
			{
				enumerator_type = TypeManager.ienumerator_type;

				if (!ProbeCollectionType (ec, expr.Type)) {
					Error_Enumerator ();
					return false;
				}

				bool is_disposable = !enumerator_type.IsSealed ||
					TypeManager.ImplementsInterface (enumerator_type, TypeManager.idisposable_type);

				VarExpr ve = var_type as VarExpr;
				if (ve != null) {
					// Infer implicitly typed local variable from foreach enumerable type
					var_type = new TypeExpression (get_current.PropertyInfo.PropertyType, var_type.Location);
				}

				var_type = var_type.ResolveAsTypeTerminal (ec, false);
				if (var_type == null)
					return false;
								
				enumerator = new TemporaryVariable (enumerator_type, loc);
				enumerator.Resolve (ec);

				init = new Invocation (get_enumerator, null);
				init = init.Resolve (ec);
				if (init == null)
					return false;

				Expression move_next_expr;
				{
					MemberInfo[] mi = new MemberInfo[] { move_next };
					MethodGroupExpr mg = new MethodGroupExpr (mi, var_type.Type, loc);
					mg.InstanceExpression = enumerator;

					move_next_expr = new Invocation (mg, null);
				}

				get_current.InstanceExpression = enumerator;

				Statement block = new CollectionForeachStatement (
					var_type.Type, variable, get_current, statement, loc);

				loop = new While (move_next_expr, block, loc);

				wrapper = is_disposable ?
					(Statement) new DisposableWrapper (this) :
					(Statement) new NonDisposableWrapper (this);
				return wrapper.Resolve (ec);
			}

			protected override void DoEmit (EmitContext ec)
			{
				wrapper.Emit (ec);
			}

			class NonDisposableWrapper : Statement {
				CollectionForeach parent;

				internal NonDisposableWrapper (CollectionForeach parent)
				{
					this.parent = parent;
				}

				public override bool Resolve (EmitContext ec)
				{
					return parent.ResolveLoop (ec);
				}

				protected override void DoEmit (EmitContext ec)
				{
					parent.EmitLoopInit (ec);
					parent.EmitLoopBody (ec);
				}
			}

			class DisposableWrapper : ExceptionStatement {
				CollectionForeach parent;

				internal DisposableWrapper (CollectionForeach parent)
				{
					this.parent = parent;
				}

				public override bool Resolve (EmitContext ec)
				{
					bool ok = true;

					ec.StartFlowBranching (this);

					if (!parent.ResolveLoop (ec))
						ok = false;

					ec.EndFlowBranching ();

					ResolveReachability (ec);

					if (TypeManager.void_dispose_void == null) {
						TypeManager.void_dispose_void = TypeManager.GetPredefinedMethod (
							TypeManager.idisposable_type, "Dispose", loc, Type.EmptyTypes);
					}
					return ok;
				}

				protected override void EmitPreTryBody (EmitContext ec)
				{
					parent.EmitLoopInit (ec);
				}

				protected override void EmitTryBody (EmitContext ec)
				{
					parent.EmitLoopBody (ec);
				}

				protected override void EmitFinallyBody (EmitContext ec)
				{
					parent.EmitFinallyBody (ec);
				}
			}

			bool ResolveLoop (EmitContext ec)
			{
				return loop.Resolve (ec);
			}

			void EmitLoopInit (EmitContext ec)
			{
				enumerator.Store (ec, init);
			}

			void EmitLoopBody (EmitContext ec)
			{
				loop.Emit (ec);
			}

			void EmitFinallyBody (EmitContext ec)
			{
				ILGenerator ig = ec.ig;

				if (enumerator_type.IsValueType) {
					MethodInfo mi = FetchMethodDispose (enumerator_type);
					if (mi != null) {
						enumerator.EmitLoadAddress (ec);
						ig.Emit (OpCodes.Call, mi);
					} else {
						enumerator.Emit (ec);
						ig.Emit (OpCodes.Box, enumerator_type);
						ig.Emit (OpCodes.Callvirt, TypeManager.void_dispose_void);
					}
				} else {
					Label call_dispose = ig.DefineLabel ();

					enumerator.Emit (ec);
					ig.Emit (OpCodes.Isinst, TypeManager.idisposable_type);
					ig.Emit (OpCodes.Dup);
					ig.Emit (OpCodes.Brtrue_S, call_dispose);

					// 'endfinally' empties the evaluation stack, and can appear anywhere inside a finally block
					// (Partition III, Section 3.35)
					ig.Emit (OpCodes.Endfinally);

					ig.MarkLabel (call_dispose);
					ig.Emit (OpCodes.Callvirt, TypeManager.void_dispose_void);
				}
			}
		}

		protected class CollectionForeachStatement : Statement
		{
			Type type;
			Expression variable, current, conv;
			Statement statement;
			Assign assign;

			public CollectionForeachStatement (Type type, Expression variable,
							   Expression current, Statement statement,
							   Location loc)
			{
				this.type = type;
				this.variable = variable;
				this.current = current;
				this.statement = statement;
				this.loc = loc;
			}

			public override bool Resolve (EmitContext ec)
			{
				current = current.Resolve (ec);
				if (current == null)
					return false;

				conv = Convert.ExplicitConversion (ec, current, type, loc);
				if (conv == null)
					return false;

				assign = new Assign (variable, conv, loc);
				if (assign.Resolve (ec) == null)
					return false;

				if (!statement.Resolve (ec))
					return false;

				return true;
			}

			protected override void DoEmit (EmitContext ec)
			{
				assign.EmitStatement (ec);
				statement.Emit (ec);
			}
		}

		protected override void CloneTo (CloneContext clonectx, Statement t)
		{
			Foreach target = (Foreach) t;

			target.type = type.Clone (clonectx);
			target.variable = variable.Clone (clonectx);
			target.expr = expr.Clone (clonectx);
			target.statement = statement.Clone (clonectx);
		}
	}
}
