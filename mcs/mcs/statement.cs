//
// statement.cs: Statement representation for the IL tree.
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Martin Baulig (martin@gnome.org)
//
// (C) 2001, 2002, 2003 Ximian, Inc.
//

using System;
using System.Text;
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
		protected abstract bool DoEmit (EmitContext ec);

		/// <summary>
		///   Return value indicates whether all code paths emitted return.
		/// </summary>
		public virtual bool Emit (EmitContext ec)
		{
			ec.Mark (loc, true);
			return DoEmit (ec);
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
		
		protected override bool DoEmit (EmitContext ec)
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
			Report.Debug (1, "START IF BLOCK", loc);

			expr = Expression.ResolveBoolean (ec, expr, loc);
			if (expr == null){
				return false;
			}
			
			ec.StartFlowBranching (FlowBranchingType.BLOCK, loc);
			
			if (!TrueStatement.Resolve (ec)) {
				ec.KillFlowBranching ();
				return false;
			}

			ec.CurrentBranching.CreateSibling ();

			if ((FalseStatement != null) && !FalseStatement.Resolve (ec)) {
				ec.KillFlowBranching ();
				return false;
			}
					
			ec.EndFlowBranching ();

			Report.Debug (1, "END IF BLOCK", loc);

			return true;
		}
		
		protected override bool DoEmit (EmitContext ec)
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
		bool infinite, may_return;
		
		public Do (Statement statement, Expression boolExpr, Location l)
		{
			expr = boolExpr;
			EmbeddedStatement = statement;
			loc = l;
		}

		public override bool Resolve (EmitContext ec)
		{
			bool ok = true;

			ec.StartFlowBranching (FlowBranchingType.LOOP_BLOCK, loc);

			if (!EmbeddedStatement.Resolve (ec))
				ok = false;

			expr = Expression.ResolveBoolean (ec, expr, loc);
			if (expr == null)
				ok = false;
			else if (expr is BoolConstant){
				bool res = ((BoolConstant) expr).Value;

				if (res)
					infinite = true;
			}

			ec.CurrentBranching.Infinite = infinite;
			FlowReturns returns = ec.EndFlowBranching ();
			may_return = returns != FlowReturns.NEVER;

			return ok;
		}
		
		protected override bool DoEmit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			Label loop = ig.DefineLabel ();
			Label old_begin = ec.LoopBegin;
			Label old_end = ec.LoopEnd;
			bool  old_inloop = ec.InLoop;
			int old_loop_begin_try_catch_level = ec.LoopBeginTryCatchLevel;
			
			ec.LoopBegin = ig.DefineLabel ();
			ec.LoopEnd = ig.DefineLabel ();
			ec.InLoop = true;
			ec.LoopBeginTryCatchLevel = ec.TryCatchLevel;
				
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
				EmitBoolExpression (ec, expr, loop, true);
			
			ig.MarkLabel (ec.LoopEnd);

			ec.LoopBeginTryCatchLevel = old_loop_begin_try_catch_level;
			ec.LoopBegin = old_begin;
			ec.LoopEnd = old_end;
			ec.InLoop = old_inloop;

			if (infinite)
				return may_return == false;
			else
				return false;
		}
	}

	public class While : Statement {
		public Expression expr;
		public readonly Statement Statement;
		bool may_return, empty, infinite;
		
		public While (Expression boolExpr, Statement statement, Location l)
		{
			this.expr = boolExpr;
			Statement = statement;
			loc = l;
		}

		public override bool Resolve (EmitContext ec)
		{
			bool ok = true;

			expr = Expression.ResolveBoolean (ec, expr, loc);
			if (expr == null)
				return false;

			ec.StartFlowBranching (FlowBranchingType.LOOP_BLOCK, loc);

			//
			// Inform whether we are infinite or not
			//
			if (expr is BoolConstant){
				BoolConstant bc = (BoolConstant) expr;

				if (bc.Value == false){
					Warning_DeadCodeFound (Statement.loc);
					empty = true;
				} else
					infinite = true;
			} else {
				//
				// We are not infinite, so the loop may or may not be executed.
				//
				ec.CurrentBranching.CreateSibling ();
			}

			if (!Statement.Resolve (ec))
				ok = false;

			if (empty)
				ec.KillFlowBranching ();
			else {
				ec.CurrentBranching.Infinite = infinite;
				FlowReturns returns = ec.EndFlowBranching ();
				may_return = returns != FlowReturns.NEVER;
			}

			return ok;
		}
		
		protected override bool DoEmit (EmitContext ec)
		{
			if (empty)
				return false;

			ILGenerator ig = ec.ig;
			Label old_begin = ec.LoopBegin;
			Label old_end = ec.LoopEnd;
			bool old_inloop = ec.InLoop;
			int old_loop_begin_try_catch_level = ec.LoopBeginTryCatchLevel;
			bool ret;
			
			ec.LoopBegin = ig.DefineLabel ();
			ec.LoopEnd = ig.DefineLabel ();
			ec.InLoop = true;
			ec.LoopBeginTryCatchLevel = ec.TryCatchLevel;

			//
			// Inform whether we are infinite or not
			//
			if (expr is BoolConstant){
				BoolConstant bc = (BoolConstant) expr;

				ig.MarkLabel (ec.LoopBegin);
				Statement.Emit (ec);
				ig.Emit (OpCodes.Br, ec.LoopBegin);
					
				//
				// Inform that we are infinite (ie, `we return'), only
				// if we do not `break' inside the code.
				//
				ret = may_return == false;
				ig.MarkLabel (ec.LoopEnd);
			} else {
				Label while_loop = ig.DefineLabel ();

				ig.Emit (OpCodes.Br, ec.LoopBegin);
				ig.MarkLabel (while_loop);

				Statement.Emit (ec);
			
				ig.MarkLabel (ec.LoopBegin);

				EmitBoolExpression (ec, expr, while_loop, true);
				ig.MarkLabel (ec.LoopEnd);

				ret = false;
			}	

			ec.LoopBegin = old_begin;
			ec.LoopEnd = old_end;
			ec.InLoop = old_inloop;
			ec.LoopBeginTryCatchLevel = old_loop_begin_try_catch_level;

			return ret;
		}
	}

	public class For : Statement {
		Expression Test;
		readonly Statement InitStatement;
		readonly Statement Increment;
		readonly Statement Statement;
		bool may_return, infinite, empty;
		
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
						Warning_DeadCodeFound (Statement.loc);
						empty = true;
					} else
						infinite = true;
				}
			} else
				infinite = true;

			ec.StartFlowBranching (FlowBranchingType.LOOP_BLOCK, loc);
			if (!infinite)
				ec.CurrentBranching.CreateSibling ();

			if (!Statement.Resolve (ec))
				ok = false;

			if (Increment != null){
				if (!Increment.Resolve (ec))
					ok = false;
			}

			if (empty)
				ec.KillFlowBranching ();
			else {
				ec.CurrentBranching.Infinite = infinite;
				FlowReturns returns = ec.EndFlowBranching ();
				may_return = returns != FlowReturns.NEVER;
			}

			return ok;
		}
		
		protected override bool DoEmit (EmitContext ec)
		{
			if (empty)
				return false;

			ILGenerator ig = ec.ig;
			Label old_begin = ec.LoopBegin;
			Label old_end = ec.LoopEnd;
			bool old_inloop = ec.InLoop;
			int old_loop_begin_try_catch_level = ec.LoopBeginTryCatchLevel;
			Label loop = ig.DefineLabel ();
			Label test = ig.DefineLabel ();
			
			if (InitStatement != null)
				if (! (InitStatement is EmptyStatement))
					InitStatement.Emit (ec);

			ec.LoopBegin = ig.DefineLabel ();
			ec.LoopEnd = ig.DefineLabel ();
			ec.InLoop = true;
			ec.LoopBeginTryCatchLevel = ec.TryCatchLevel;

			ig.Emit (OpCodes.Br, test);
			ig.MarkLabel (loop);
			Statement.Emit (ec);

			ig.MarkLabel (ec.LoopBegin);
			if (!(Increment is EmptyStatement))
				Increment.Emit (ec);

			ig.MarkLabel (test);
			//
			// If test is null, there is no test, and we are just
			// an infinite loop
			//
			if (Test != null){
				//
				// The Resolve code already catches the case for Test == BoolConstant (false)
				// so we know that this is true
				//
				if (Test is BoolConstant)
					ig.Emit (OpCodes.Br, loop);
				else
					EmitBoolExpression (ec, Test, loop, true);
			} else
				ig.Emit (OpCodes.Br, loop);
			ig.MarkLabel (ec.LoopEnd);

			ec.LoopBegin = old_begin;
			ec.LoopEnd = old_end;
			ec.InLoop = old_inloop;
			ec.LoopBeginTryCatchLevel = old_loop_begin_try_catch_level;
			
			//
 			// Inform whether we are infinite or not
			//
			if (Test != null){
				if (Test is BoolConstant){
					BoolConstant bc = (BoolConstant) Test;

					if (bc.Value)
						return may_return == false;
				}
				return false;
			} else
				return may_return == false;
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
		
		protected override bool DoEmit (EmitContext ec)
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

			if (ec.InIterator){
				Report.Error (-206, loc, "Return statement not allowed inside iterators");
				return false;
			}
				
			FlowBranching.UsageVector vector = ec.CurrentBranching.CurrentUsageVector;

			if (ec.CurrentBranching.InTryBlock ())
				ec.CurrentBranching.AddFinallyVector (vector);
			else
				vector.CheckOutParameters (ec.CurrentBranching);

			vector.Returns = FlowReturns.ALWAYS;
			vector.Breaks = FlowReturns.ALWAYS;
			return true;
		}
		
		protected override bool DoEmit (EmitContext ec)
		{
			if (ec.InFinally){
				Report.Error (157, loc, "Control can not leave the body of the finally block");
				return false;
			}
			
			if (ec.ReturnType == null){
				if (Expr != null){
					Report.Error (127, loc, "Return with a value not allowed here");
					return true;
				}
			} else {
				if (Expr == null){
					Report.Error (126, loc, "An object of type `" +
						      TypeManager.CSharpName (ec.ReturnType) + "' is " +
						      "expected for the return statement");
					return true;
				}

				if (Expr.Type != ec.ReturnType)
					Expr = Convert.ImplicitConversionRequired (
						ec, Expr, ec.ReturnType, loc);

				if (Expr == null)
					return true;

				Expr.Emit (ec);

				if (ec.InTry || ec.InCatch)
					ec.ig.Emit (OpCodes.Stloc, ec.TemporaryReturn ());
			}

			if (ec.InTry || ec.InCatch) {
				if (!ec.HasReturnLabel) {
					ec.ReturnLabel = ec.ig.DefineLabel ();
					ec.HasReturnLabel = true;
				}
				ec.ig.Emit (OpCodes.Leave, ec.ReturnLabel);
			} else {
				ec.ig.Emit (OpCodes.Ret);
				ec.NeedExplicitReturn = false;
			}

			return true; 
		}
	}

	public class Goto : Statement {
		string target;
		Block block;
		LabeledStatement label;
		
		public override bool Resolve (EmitContext ec)
		{
			label = block.LookupLabel (target);
			if (label == null){
				Report.Error (
					159, loc,
					"No such label `" + target + "' in this scope");
				return false;
			}

			// If this is a forward goto.
			if (!label.IsDefined)
				label.AddUsageVector (ec.CurrentBranching.CurrentUsageVector);

			ec.CurrentBranching.CurrentUsageVector.Breaks = FlowReturns.ALWAYS;
			ec.CurrentBranching.CurrentUsageVector.Returns = FlowReturns.ALWAYS;

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

		protected override bool DoEmit (EmitContext ec)
		{
			Label l = label.LabelTarget (ec);
			ec.ig.Emit (OpCodes.Br, l);
			
			return false;
		}
	}

	public class LabeledStatement : Statement {
		public readonly Location Location;
		string label_name;
		bool defined;
		bool referenced;
		Label label;

		ArrayList vectors;
		
		public LabeledStatement (string label_name, Location l)
		{
			this.label_name = label_name;
			this.Location = l;
		}

		public Label LabelTarget (EmitContext ec)
		{
			if (defined)
				return label;
			label = ec.ig.DefineLabel ();
			defined = true;

			return label;
		}

		public bool IsDefined {
			get {
				return defined;
			}
		}

		public bool HasBeenReferenced {
			get {
				return referenced;
			}
		}

		public void AddUsageVector (FlowBranching.UsageVector vector)
		{
			if (vectors == null)
				vectors = new ArrayList ();

			vectors.Add (vector.Clone ());
		}

		public override bool Resolve (EmitContext ec)
		{
			if (vectors != null)
				ec.CurrentBranching.CurrentUsageVector.MergeJumpOrigins (vectors);
			else {
				ec.CurrentBranching.CurrentUsageVector.Breaks = FlowReturns.NEVER;
				ec.CurrentBranching.CurrentUsageVector.Returns = FlowReturns.NEVER;
			}

			referenced = true;

			return true;
		}

		protected override bool DoEmit (EmitContext ec)
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

		public override bool Resolve (EmitContext ec)
		{
			ec.CurrentBranching.CurrentUsageVector.Breaks = FlowReturns.ALWAYS;
			ec.CurrentBranching.CurrentUsageVector.Returns = FlowReturns.ALWAYS;
			return true;
		}

		protected override bool DoEmit (EmitContext ec)
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
		Label label;
		
		public GotoCase (Expression e, Location l)
		{
			expr = e;
			loc = l;
		}

		public override bool Resolve (EmitContext ec)
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
				return false;
			}

			label = sl.ILLabelCode;

			ec.CurrentBranching.CurrentUsageVector.Breaks = FlowReturns.UNREACHABLE;
			ec.CurrentBranching.CurrentUsageVector.Returns = FlowReturns.ALWAYS;
			return true;
		}

		protected override bool DoEmit (EmitContext ec)
		{
			ec.ig.Emit (OpCodes.Br, label);
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

				ExprClass eclass = expr.eclass;

				if (!(eclass == ExprClass.Variable || eclass == ExprClass.PropertyAccess ||
				      eclass == ExprClass.Value || eclass == ExprClass.IndexerAccess)) {
					expr.Error_UnexpectedKind ("value, variable, property or indexer access ");
					return false;
				}

				Type t = expr.Type;
				
				if ((t != TypeManager.exception_type) &&
				    !t.IsSubclassOf (TypeManager.exception_type) &&
				    !(expr is NullLiteral)) {
					Report.Error (155, loc,
						      "The type caught or thrown must be derived " +
						      "from System.Exception");
					return false;
				}
			}

			ec.CurrentBranching.CurrentUsageVector.Returns = FlowReturns.EXCEPTION;
			ec.CurrentBranching.CurrentUsageVector.Breaks = FlowReturns.EXCEPTION;
			return true;
		}
			
		protected override bool DoEmit (EmitContext ec)
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

		public override bool Resolve (EmitContext ec)
		{
			ec.CurrentBranching.MayLeaveLoop = true;
			ec.CurrentBranching.CurrentUsageVector.Breaks = FlowReturns.ALWAYS;
			return true;
		}

		protected override bool DoEmit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;

			if (ec.InLoop == false && ec.Switch == null){
				Report.Error (139, loc, "No enclosing loop or switch to continue to");
				return false;
			}

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

		public override bool Resolve (EmitContext ec)
		{
			ec.CurrentBranching.CurrentUsageVector.Breaks = FlowReturns.ALWAYS;
			return true;
		}

		protected override bool DoEmit (EmitContext ec)
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
			if (ec.TryCatchLevel > ec.LoopBeginTryCatchLevel)
				ec.ig.Emit (OpCodes.Leave, begin);
			else if (ec.TryCatchLevel < ec.LoopBeginTryCatchLevel)
				throw new Exception ("Should never happen");
			else
				ec.ig.Emit (OpCodes.Br, begin);
			return false;
		}
	}

	// <summary>
	//   This is used in the control flow analysis code to specify whether the
	//   current code block may return to its enclosing block before reaching
	//   its end.
	// </summary>
	public enum FlowReturns {
		// It can never return.
		NEVER,

		// This means that the block contains a conditional return statement
		// somewhere.
		SOMETIMES,

		// The code always returns, ie. there's an unconditional return / break
		// statement in it.
		ALWAYS,

		// The code always throws an exception.
		EXCEPTION,

		// The current code block is unreachable.  This happens if it's immediately
		// following a FlowReturns.ALWAYS block.
		UNREACHABLE
	}

	// <summary>
	//   This is a special bit vector which can inherit from another bit vector doing a
	//   copy-on-write strategy.  The inherited vector may have a smaller size than the
	//   current one.
	// </summary>
	public class MyBitVector {
		public readonly int Count;
		public readonly MyBitVector InheritsFrom;

		bool is_dirty;
		BitArray vector;

		public MyBitVector (int Count)
			: this (null, Count)
		{ }

		public MyBitVector (MyBitVector InheritsFrom, int Count)
		{
			this.InheritsFrom = InheritsFrom;
			this.Count = Count;
		}

		// <summary>
		//   Checks whether this bit vector has been modified.  After setting this to true,
		//   we won't use the inherited vector anymore, but our own copy of it.
		// </summary>
		public bool IsDirty {
			get {
				return is_dirty;
			}

			set {
				if (!is_dirty)
					initialize_vector ();
			}
		}

		// <summary>
		//   Get/set bit `index' in the bit vector.
		// </summary>
		public bool this [int index]
		{
			get {
				if (index > Count)
					throw new ArgumentOutOfRangeException ();

				// We're doing a "copy-on-write" strategy here; as long
				// as nobody writes to the array, we can use our parent's
				// copy instead of duplicating the vector.

				if (vector != null)
					return vector [index];
				else if (InheritsFrom != null) {
					BitArray inherited = InheritsFrom.Vector;

					if (index < inherited.Count)
						return inherited [index];
					else
						return false;
				} else
					return false;
			}

			set {
				if (index > Count)
					throw new ArgumentOutOfRangeException ();

				// Only copy the vector if we're actually modifying it.

				if (this [index] != value) {
					initialize_vector ();

					vector [index] = value;
				}
			}
		}

		// <summary>
		//   If you explicitly convert the MyBitVector to a BitArray, you will get a deep
		//   copy of the bit vector.
		// </summary>
		public static explicit operator BitArray (MyBitVector vector)
		{
			vector.initialize_vector ();
			return vector.Vector;
		}

		// <summary>
		//   Performs an `or' operation on the bit vector.  The `new_vector' may have a
		//   different size than the current one.
		// </summary>
		public void Or (MyBitVector new_vector)
		{
			BitArray new_array = new_vector.Vector;

			initialize_vector ();

			int upper;
			if (vector.Count < new_array.Count)
				upper = vector.Count;
			else
				upper = new_array.Count;

			for (int i = 0; i < upper; i++)
				vector [i] = vector [i] | new_array [i];
		}

		// <summary>
		//   Perfonrms an `and' operation on the bit vector.  The `new_vector' may have
		//   a different size than the current one.
		// </summary>
		public void And (MyBitVector new_vector)
		{
			BitArray new_array = new_vector.Vector;

			initialize_vector ();

			int lower, upper;
			if (vector.Count < new_array.Count)
				lower = upper = vector.Count;
			else {
				lower = new_array.Count;
				upper = vector.Count;
			}

			for (int i = 0; i < lower; i++)
				vector [i] = vector [i] & new_array [i];

			for (int i = lower; i < upper; i++)
				vector [i] = false;
		}

		// <summary>
		//   This does a deep copy of the bit vector.
		// </summary>
		public MyBitVector Clone ()
		{
			MyBitVector retval = new MyBitVector (Count);

			retval.Vector = Vector;

			return retval;
		}

		BitArray Vector {
			get {
				if (vector != null)
					return vector;
				else if (!is_dirty && (InheritsFrom != null))
					return InheritsFrom.Vector;

				initialize_vector ();

				return vector;
			}

			set {
				initialize_vector ();

				for (int i = 0; i < Math.Min (vector.Count, value.Count); i++)
					vector [i] = value [i];
			}
		}

		void initialize_vector ()
		{
			if (vector != null)
				return;

			vector = new BitArray (Count, false);
			if (InheritsFrom != null)
				Vector = InheritsFrom.Vector;

			is_dirty = true;
		}

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ("MyBitVector (");

			BitArray vector = Vector;
			sb.Append (Count);
			sb.Append (",");
			if (!IsDirty)
				sb.Append ("INHERITED - ");
			for (int i = 0; i < vector.Count; i++) {
				if (i > 0)
					sb.Append (",");
				sb.Append (vector [i]);
			}
			
			sb.Append (")");
			return sb.ToString ();
		}
	}

	// <summary>
	//   The type of a FlowBranching.
	// </summary>
	public enum FlowBranchingType {
		// Normal (conditional or toplevel) block.
		BLOCK,

		// A loop block.
		LOOP_BLOCK,

		// Try/Catch block.
		EXCEPTION,

		// Switch block.
		SWITCH,

		// Switch section.
		SWITCH_SECTION
	}

	// <summary>
	//   A new instance of this class is created every time a new block is resolved
	//   and if there's branching in the block's control flow.
	// </summary>
	public class FlowBranching {
		// <summary>
		//   The type of this flow branching.
		// </summary>
		public readonly FlowBranchingType Type;

		// <summary>
		//   The block this branching is contained in.  This may be null if it's not
		//   a top-level block and it doesn't declare any local variables.
		// </summary>
		public readonly Block Block;

		// <summary>
		//   The parent of this branching or null if this is the top-block.
		// </summary>
		public readonly FlowBranching Parent;

		// <summary>
		//   Start-Location of this flow branching.
		// </summary>
		public readonly Location Location;

		// <summary>
		//   A list of UsageVectors.  A new vector is added each time control flow may
		//   take a different path.
		// </summary>
		public UsageVector[] Siblings;

		// <summary>
		//   If this is an infinite loop.
		// </summary>
		public bool Infinite;

		// <summary>
		//   If we may leave the current loop.
		// </summary>
		public bool MayLeaveLoop;

		//
		// Private
		//
		VariableMap param_map, local_map;
		ArrayList finally_vectors;

		static int next_id = 0;
		int id;

		// <summary>
		//   Performs an `And' operation on the FlowReturns status
		//   (for instance, a block only returns ALWAYS if all its siblings
		//   always return).
		// </summary>
		public static FlowReturns AndFlowReturns (FlowReturns a, FlowReturns b)
		{
			if (b == FlowReturns.UNREACHABLE)
				return a;

			switch (a) {
			case FlowReturns.NEVER:
				if (b == FlowReturns.NEVER)
					return FlowReturns.NEVER;
				else
					return FlowReturns.SOMETIMES;

			case FlowReturns.SOMETIMES:
				return FlowReturns.SOMETIMES;

			case FlowReturns.ALWAYS:
				if ((b == FlowReturns.ALWAYS) || (b == FlowReturns.EXCEPTION))
					return FlowReturns.ALWAYS;
				else
					return FlowReturns.SOMETIMES;

			case FlowReturns.EXCEPTION:
				if (b == FlowReturns.EXCEPTION)
					return FlowReturns.EXCEPTION;
				else if (b == FlowReturns.ALWAYS)
					return FlowReturns.ALWAYS;
				else
					return FlowReturns.SOMETIMES;
			}

			return b;
		}

		// <summary>
		//   The vector contains a BitArray with information about which local variables
		//   and parameters are already initialized at the current code position.
		// </summary>
		public class UsageVector {
			// <summary>
			//   If this is true, then the usage vector has been modified and must be
			//   merged when we're done with this branching.
			// </summary>
			public bool IsDirty;

			// <summary>
			//   The number of parameters in this block.
			// </summary>
			public readonly int CountParameters;

			// <summary>
			//   The number of locals in this block.
			// </summary>
			public readonly int CountLocals;

			// <summary>
			//   If not null, then we inherit our state from this vector and do a
			//   copy-on-write.  If null, then we're the first sibling in a top-level
			//   block and inherit from the empty vector.
			// </summary>
			public readonly UsageVector InheritsFrom;

			//
			// Private.
			//
			MyBitVector locals, parameters;
			FlowReturns real_returns, real_breaks;
			bool is_finally;

			static int next_id = 0;
			int id;

			//
			// Normally, you should not use any of these constructors.
			//
			public UsageVector (UsageVector parent, int num_params, int num_locals)
			{
				this.InheritsFrom = parent;
				this.CountParameters = num_params;
				this.CountLocals = num_locals;
				this.real_returns = FlowReturns.NEVER;
				this.real_breaks = FlowReturns.NEVER;

				if (parent != null) {
					locals = new MyBitVector (parent.locals, CountLocals);
					if (num_params > 0)
						parameters = new MyBitVector (parent.parameters, num_params);
					real_returns = parent.Returns;
					real_breaks = parent.Breaks;
				} else {
					locals = new MyBitVector (null, CountLocals);
					if (num_params > 0)
						parameters = new MyBitVector (null, num_params);
				}

				id = ++next_id;
			}

			public UsageVector (UsageVector parent)
				: this (parent, parent.CountParameters, parent.CountLocals)
			{ }

			// <summary>
			//   This does a deep copy of the usage vector.
			// </summary>
			public UsageVector Clone ()
			{
				UsageVector retval = new UsageVector (null, CountParameters, CountLocals);

				retval.locals = locals.Clone ();
				if (parameters != null)
					retval.parameters = parameters.Clone ();
				retval.real_returns = real_returns;
				retval.real_breaks = real_breaks;

				return retval;
			}

			public bool IsAssigned (VariableInfo var)
			{
				if (!var.IsParameter && AlwaysBreaks)
					return true;

				return var.IsAssigned (var.IsParameter ? parameters : locals);
			}

			public void SetAssigned (VariableInfo var)
			{
				if (!var.IsParameter && AlwaysBreaks)
					return;

				var.SetAssigned (var.IsParameter ? parameters : locals);
			}

			public bool IsFieldAssigned (VariableInfo var, string name)
			{
				if (!var.IsParameter && AlwaysBreaks)
					return true;

				return var.IsFieldAssigned (var.IsParameter ? parameters : locals, name);
			}

			public void SetFieldAssigned (VariableInfo var, string name)
			{
				if (!var.IsParameter && AlwaysBreaks)
					return;

				var.SetFieldAssigned (var.IsParameter ? parameters : locals, name);
			}

			// <summary>
			//   Specifies when the current block returns.
			//   If this is FlowReturns.UNREACHABLE, then control can never reach the
			//   end of the method (so that we don't need to emit a return statement).
			//   The same applies for FlowReturns.EXCEPTION, but in this case the return
			//   value will never be used.
			// </summary>
			public FlowReturns Returns {
				get {
					return real_returns;
				}

				set {
					real_returns = value;
				}
			}

			// <summary>
			//   Specifies whether control may return to our containing block
			//   before reaching the end of this block.  This happens if there
			//   is a break/continue/goto/return in it.
			//   This can also be used to find out whether the statement immediately
			//   following the current block may be reached or not.
			// </summary>
			public FlowReturns Breaks {
				get {
					return real_breaks;
				}

				set {
					real_breaks = value;
				}
			}

			public bool AlwaysBreaks {
				get {
					return (Breaks == FlowReturns.ALWAYS) ||
						(Breaks == FlowReturns.EXCEPTION) ||
						(Breaks == FlowReturns.UNREACHABLE);
				}
			}

			public bool MayBreak {
				get {
					return Breaks != FlowReturns.NEVER;
				}
			}

			public bool AlwaysReturns {
				get {
					return (Returns == FlowReturns.ALWAYS) ||
						(Returns == FlowReturns.EXCEPTION);
				}
			}

			public bool MayReturn {
				get {
					return (Returns == FlowReturns.SOMETIMES) ||
						(Returns == FlowReturns.ALWAYS);
				}
			}

			// <summary>
			//   Merge a child branching.
			// </summary>
			public FlowReturns MergeChildren (FlowBranching branching, UsageVector[] children)
			{
				MyBitVector new_locals = null;
				MyBitVector new_params = null;

				FlowReturns new_returns = FlowReturns.NEVER;
				FlowReturns new_breaks = FlowReturns.NEVER;
				bool new_returns_set = false, new_breaks_set = false;

				Report.Debug (2, "MERGING CHILDREN", branching, branching.Type,
					      this, children.Length);

				foreach (UsageVector child in children) {
					Report.Debug (2, "  MERGING CHILD", child, child.is_finally);
					
					if (!child.is_finally) {
						if (child.Breaks != FlowReturns.UNREACHABLE) {
							// If Returns is already set, perform an
							// `And' operation on it, otherwise just set just.
							if (!new_returns_set) {
								new_returns = child.Returns;
								new_returns_set = true;
							} else
								new_returns = AndFlowReturns (
									new_returns, child.Returns);
						}

						// If Breaks is already set, perform an
						// `And' operation on it, otherwise just set just.
						if (!new_breaks_set) {
							new_breaks = child.Breaks;
							new_breaks_set = true;
						} else
							new_breaks = AndFlowReturns (
								new_breaks, child.Breaks);
					}

					// Ignore unreachable children.
					if (child.Returns == FlowReturns.UNREACHABLE)
						continue;

					// A local variable is initialized after a flow branching if it
					// has been initialized in all its branches which do neither
					// always return or always throw an exception.
					//
					// If a branch may return, but does not always return, then we
					// can treat it like a never-returning branch here: control will
					// only reach the code position after the branching if we did not
					// return here.
					//
					// It's important to distinguish between always and sometimes
					// returning branches here:
					//
					//    1   int a;
					//    2   if (something) {
					//    3      return;
					//    4      a = 5;
					//    5   }
					//    6   Console.WriteLine (a);
					//
					// The if block in lines 3-4 always returns, so we must not look
					// at the initialization of `a' in line 4 - thus it'll still be
					// uninitialized in line 6.
					//
					// On the other hand, the following is allowed:
					//
					//    1   int a;
					//    2   if (something)
					//    3      a = 5;
					//    4   else
					//    5      return;
					//    6   Console.WriteLine (a);
					//
					// Here, `a' is initialized in line 3 and we must not look at
					// line 5 since it always returns.
					// 
					if (child.is_finally) {
						if (new_locals == null)
							new_locals = locals.Clone ();
						new_locals.Or (child.locals);

						if (parameters != null) {
							if (new_params == null)
								new_params = parameters.Clone ();
							new_params.Or (child.parameters);
						}
					} else {
						if (!child.AlwaysReturns && !child.AlwaysBreaks) {
							if (new_locals != null)
								new_locals.And (child.locals);
							else {
								new_locals = locals.Clone ();
								new_locals.Or (child.locals);
							}
						} else if (children.Length == 1) {
							new_locals = locals.Clone ();
							new_locals.Or (child.locals);
						}

						// An `out' parameter must be assigned in all branches which do
						// not always throw an exception.
						if (parameters != null) {
							bool and_params = child.Breaks != FlowReturns.EXCEPTION;
							if (branching.Type == FlowBranchingType.EXCEPTION)
								and_params &= child.Returns != FlowReturns.NEVER;
							if (and_params) {
								if (new_params != null)
									new_params.And (child.parameters);
								else {
									new_params = parameters.Clone ();
									new_params.Or (child.parameters);
								}
							} else if ((children.Length == 1) || (new_params == null)) {
								new_params = parameters.Clone ();
								new_params.Or (child.parameters);
							}
						}
					}
				}

				Returns = new_returns;
				if ((branching.Type == FlowBranchingType.BLOCK) ||
				    (branching.Type == FlowBranchingType.EXCEPTION) ||
				    (new_breaks == FlowReturns.UNREACHABLE) ||
				    (new_breaks == FlowReturns.EXCEPTION))
					Breaks = new_breaks;
				else if (branching.Type == FlowBranchingType.SWITCH_SECTION)
					Breaks = new_returns;
				else if (branching.Type == FlowBranchingType.SWITCH){
					if (new_breaks == FlowReturns.ALWAYS)
						Breaks = FlowReturns.ALWAYS;
				}

				//
				// We've now either reached the point after the branching or we will
				// never get there since we always return or always throw an exception.
				//
				// If we can reach the point after the branching, mark all locals and
				// parameters as initialized which have been initialized in all branches
				// we need to look at (see above).
				//

				if (((new_breaks != FlowReturns.ALWAYS) &&
				     (new_breaks != FlowReturns.EXCEPTION) &&
				     (new_breaks != FlowReturns.UNREACHABLE)) ||
				    (children.Length == 1)) {
					if (new_locals != null)
						locals.Or (new_locals);

					if (new_params != null)
						parameters.Or (new_params);
				}

				Report.Debug (2, "MERGING CHILDREN DONE", branching.Type,
					      new_params, new_locals, new_returns, new_breaks,
					      branching.Infinite, branching.MayLeaveLoop, this);

				if (branching.Type == FlowBranchingType.SWITCH_SECTION) {
					if ((new_breaks != FlowReturns.ALWAYS) &&
					    (new_breaks != FlowReturns.EXCEPTION) &&
					    (new_breaks != FlowReturns.UNREACHABLE))
						Report.Error (163, branching.Location,
							      "Control cannot fall through from one " +
							      "case label to another");
				}

				if (branching.Infinite && !branching.MayLeaveLoop) {
					Report.Debug (1, "INFINITE", new_returns, new_breaks,
						      Returns, Breaks, this);

					// We're actually infinite.
					if (new_returns == FlowReturns.NEVER) {
						Breaks = FlowReturns.UNREACHABLE;
						return FlowReturns.UNREACHABLE;
					}

					// If we're an infinite loop and do not break, the code after
					// the loop can never be reached.  However, if we may return
					// from the loop, then we do always return (or stay in the loop
					// forever).
					if ((new_returns == FlowReturns.SOMETIMES) ||
					    (new_returns == FlowReturns.ALWAYS)) {
						Returns = FlowReturns.ALWAYS;
						return FlowReturns.ALWAYS;
					}
				}

				if (branching.Type == FlowBranchingType.LOOP_BLOCK) {
					Report.Debug (2, "MERGING LOOP BLOCK DONE", branching,
						      branching.Infinite, branching.MayLeaveLoop,
						      new_breaks, new_returns);

					// If we may leave the loop, then we do not always return.
					if (branching.MayLeaveLoop && (new_returns == FlowReturns.ALWAYS)) {
						Returns = FlowReturns.SOMETIMES;
						return FlowReturns.SOMETIMES;
					}

					// A `break' in a loop does not "break" in the outer block.
					Breaks = FlowReturns.NEVER;
				}

				return new_returns;
			}

			// <summary>
			//   Tells control flow analysis that the current code position may be reached with
			//   a forward jump from any of the origins listed in `origin_vectors' which is a
			//   list of UsageVectors.
			//
			//   This is used when resolving forward gotos - in the following example, the
			//   variable `a' is uninitialized in line 8 becase this line may be reached via
			//   the goto in line 4:
			//
			//      1     int a;
			//
			//      3     if (something)
			//      4        goto World;
			//
			//      6     a = 5;
			//
			//      7  World:
			//      8     Console.WriteLine (a);
			//
			// </summary>
			public void MergeJumpOrigins (ICollection origin_vectors)
			{
				Report.Debug (1, "MERGING JUMP ORIGIN", this);

				real_breaks = FlowReturns.NEVER;
				real_returns = FlowReturns.NEVER;

				foreach (UsageVector vector in origin_vectors) {
					Report.Debug (1, "  MERGING JUMP ORIGIN", vector);

					locals.And (vector.locals);
					if (parameters != null)
						parameters.And (vector.parameters);
					Breaks = AndFlowReturns (Breaks, vector.Breaks);
					Returns = AndFlowReturns (Returns, vector.Returns);
				}

				Report.Debug (1, "MERGING JUMP ORIGIN DONE", this);
			}

			// <summary>
			//   This is used at the beginning of a finally block if there were
			//   any return statements in the try block or one of the catch blocks.
			// </summary>
			public void MergeFinallyOrigins (ICollection finally_vectors)
			{
				Report.Debug (1, "MERGING FINALLY ORIGIN", this);

				real_breaks = FlowReturns.NEVER;

				foreach (UsageVector vector in finally_vectors) {
					Report.Debug (1, "  MERGING FINALLY ORIGIN", vector);

					if (parameters != null)
						parameters.And (vector.parameters);
					Breaks = AndFlowReturns (Breaks, vector.Breaks);
				}

				is_finally = true;

				Report.Debug (1, "MERGING FINALLY ORIGIN DONE", this);
			}

			public void CheckOutParameters (FlowBranching branching)
			{
				if (parameters != null)
					branching.CheckOutParameters (parameters, branching.Location);
			}

			// <summary>
			//   Performs an `or' operation on the locals and the parameters.
			// </summary>
			public void Or (UsageVector new_vector)
			{
				locals.Or (new_vector.locals);
				if (parameters != null)
					parameters.Or (new_vector.parameters);
			}

			// <summary>
			//   Performs an `and' operation on the locals.
			// </summary>
			public void AndLocals (UsageVector new_vector)
			{
				locals.And (new_vector.locals);
			}

			// <summary>
			//   Returns a deep copy of the parameters.
			// </summary>
			public MyBitVector Parameters {
				get {
					if (parameters != null)
						return parameters.Clone ();
					else
						return null;
				}
			}

			// <summary>
			//   Returns a deep copy of the locals.
			// </summary>
			public MyBitVector Locals {
				get {
					return locals.Clone ();
				}
			}

			//
			// Debugging stuff.
			//

			public override string ToString ()
			{
				StringBuilder sb = new StringBuilder ();

				sb.Append ("Vector (");
				sb.Append (id);
				sb.Append (",");
				sb.Append (Returns);
				sb.Append (",");
				sb.Append (Breaks);
				if (parameters != null) {
					sb.Append (" - ");
					sb.Append (parameters);
				}
				sb.Append (" - ");
				sb.Append (locals);
				sb.Append (")");

				return sb.ToString ();
			}
		}

		FlowBranching (FlowBranchingType type, Location loc)
		{
			this.Block = null;
			this.Location = loc;
			this.Type = type;
			id = ++next_id;
		}

		// <summary>
		//   Creates a new flow branching for `block'.
		//   This is used from Block.Resolve to create the top-level branching of
		//   the block.
		// </summary>
		public FlowBranching (Block block, Location loc)
			: this (FlowBranchingType.BLOCK, loc)
		{
			Block = block;
			Parent = null;

			param_map = block.ParameterMap;
			local_map = block.LocalMap;

			UsageVector vector = new UsageVector (null, param_map.Length, local_map.Length);

			AddSibling (vector);
		}

		// <summary>
		//   Creates a new flow branching which is contained in `parent'.
		//   You should only pass non-null for the `block' argument if this block
		//   introduces any new variables - in this case, we need to create a new
		//   usage vector with a different size than our parent's one.
		// </summary>
		public FlowBranching (FlowBranching parent, FlowBranchingType type,
				      Block block, Location loc)
			: this (type, loc)
		{
			Parent = parent;
			Block = block;

			UsageVector vector;
			if (Block != null) {
				param_map = Block.ParameterMap;
				local_map = Block.LocalMap;

				vector = new UsageVector (parent.CurrentUsageVector, param_map.Length,
							  local_map.Length);
			} else {
				param_map = Parent.param_map;
				local_map = Parent.local_map;
				vector = new UsageVector (Parent.CurrentUsageVector);
			}

			AddSibling (vector);

			switch (Type) {
			case FlowBranchingType.EXCEPTION:
				finally_vectors = new ArrayList ();
				break;

			default:
				break;
			}
		}

		void AddSibling (UsageVector uv)
		{
			if (Siblings != null) {
				UsageVector[] ns = new UsageVector [Siblings.Length + 1];
				for (int i = 0; i < Siblings.Length; ++i)
					ns [i] = Siblings [i];
				Siblings = ns;
			} else {
				Siblings = new UsageVector [1];
			}
			Siblings [Siblings.Length - 1] = uv;
		}

		// <summary>
		//   Returns the branching's current usage vector.
		// </summary>
		public UsageVector CurrentUsageVector
		{
			get {
				return Siblings [Siblings.Length - 1];
			}
		}

		// <summary>
		//   Creates a sibling of the current usage vector.
		// </summary>
		public void CreateSibling ()
		{
			AddSibling (new UsageVector (Parent.CurrentUsageVector));

			Report.Debug (1, "CREATED SIBLING", CurrentUsageVector);
		}

		// <summary>
		//   Creates a sibling for a `finally' block.
		// </summary>
		public void CreateSiblingForFinally ()
		{
			if (Type != FlowBranchingType.EXCEPTION)
				throw new NotSupportedException ();

			CreateSibling ();

			CurrentUsageVector.MergeFinallyOrigins (finally_vectors);
		}

		// <summary>
		//   Check whether all `out' parameters have been assigned.
		// </summary>
		public void CheckOutParameters (MyBitVector parameters, Location loc)
		{
			if (InTryBlock ())
				return;

			for (int i = 0; i < param_map.Count; i++) {
				VariableInfo var = param_map [i];

				if (var == null)
					continue;

				if (var.IsAssigned (parameters))
					continue;

				Report.Error (177, loc, "The out parameter `" +
					      param_map.VariableNames [i] + "' must be " +
					      "assigned before control leave the current method.");
			}
		}

		// <summary>
		//   Merge a child branching.
		// </summary>
		public FlowReturns MergeChild (FlowBranching child)
		{
			FlowReturns returns = CurrentUsageVector.MergeChildren (child, child.Siblings);

			if ((child.Type != FlowBranchingType.LOOP_BLOCK) &&
			    (child.Type != FlowBranchingType.SWITCH_SECTION))
				MayLeaveLoop |= child.MayLeaveLoop;

			return returns;
 		}
 
		// <summary>
		//   Does the toplevel merging.
		// </summary>
		public FlowReturns MergeTopBlock ()
		{
			if ((Type != FlowBranchingType.BLOCK) || (Block == null))
				throw new NotSupportedException ();

			UsageVector vector = new UsageVector (null, param_map.Length, local_map.Length);

			Report.Debug (1, "MERGING TOP BLOCK", Location, vector);

			vector.MergeChildren (this, Siblings);

			if (Siblings.Length == 1)
				Siblings [0] = vector;
			else {
				Siblings = null;
				AddSibling (vector);
			}

			Report.Debug (1, "MERGING TOP BLOCK DONE", Location, vector);

			if (vector.Breaks != FlowReturns.EXCEPTION) {
				if (!vector.AlwaysBreaks)
					CheckOutParameters (CurrentUsageVector.Parameters, Location);
				return vector.AlwaysBreaks ? FlowReturns.ALWAYS : vector.Returns;
			} else
				return FlowReturns.EXCEPTION;
		}

		public bool InTryBlock ()
		{
			if (finally_vectors != null)
				return true;
			else if (Parent != null)
				return Parent.InTryBlock ();
			else
				return false;
		}

		public void AddFinallyVector (UsageVector vector)
		{
			if (finally_vectors != null) {
				finally_vectors.Add (vector.Clone ());
				return;
			}

			if (Parent != null)
				Parent.AddFinallyVector (vector);
			else
				throw new NotSupportedException ();
		}

		public bool IsAssigned (VariableInfo vi)
		{
			return CurrentUsageVector.IsAssigned (vi);
		}

		public bool IsFieldAssigned (VariableInfo vi, string field_name)
		{
			if (CurrentUsageVector.IsAssigned (vi))
				return true;

			return CurrentUsageVector.IsFieldAssigned (vi, field_name);
		}

		public void SetAssigned (VariableInfo vi)
		{
			CurrentUsageVector.SetAssigned (vi);
		}

		public void SetFieldAssigned (VariableInfo vi, string name)
		{
			CurrentUsageVector.SetFieldAssigned (vi, name);
		}

		public bool IsReachable ()
		{
			bool reachable;

			switch (Type) {
			case FlowBranchingType.SWITCH_SECTION:
				// The code following a switch block is reachable unless the switch
				// block always returns.
				reachable = !CurrentUsageVector.AlwaysReturns;
				break;

			case FlowBranchingType.LOOP_BLOCK:
				// The code following a loop is reachable unless the loop always
				// returns or it's an infinite loop without any `break's in it.
				reachable = !CurrentUsageVector.AlwaysReturns &&
					(CurrentUsageVector.Breaks != FlowReturns.UNREACHABLE);
				break;

			default:
				// The code following a block or exception is reachable unless the
				// block either always returns or always breaks.
				if (MayLeaveLoop)
					reachable = true;
				else
					reachable = !CurrentUsageVector.AlwaysBreaks &&
						!CurrentUsageVector.AlwaysReturns;
				break;
			}

			Report.Debug (1, "REACHABLE", this, Type, CurrentUsageVector.Returns,
				      CurrentUsageVector.Breaks, CurrentUsageVector, MayLeaveLoop,
				      reachable);

			return reachable;
		}

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ("FlowBranching (");

			sb.Append (id);
			sb.Append (",");
			sb.Append (Type);
			if (Block != null) {
				sb.Append (" - ");
				sb.Append (Block.ID);
				sb.Append (" - ");
				sb.Append (Block.StartLocation);
			}
			sb.Append (" - ");
			sb.Append (Siblings.Length);
			sb.Append (" - ");
			sb.Append (CurrentUsageVector);
			sb.Append (")");
			return sb.ToString ();
		}
	}

	// <summary>
	//   This is used by the flow analysis code to keep track of the type of local variables
	//   and variables.
	//
	//   The flow code uses a BitVector to keep track of whether a variable has been assigned
	//   or not.  This is easy for fundamental types (int, char etc.) or reference types since
	//   you can only assign the whole variable as such.
	//
	//   For structs, we also need to keep track of all its fields.  To do this, we allocate one
	//   bit for the struct itself (it's used if you assign/access the whole struct) followed by
	//   one bit for each of its fields.
	//
	//   This class computes this `layout' for each type.
	// </summary>
	public class TypeInfo
	{
		public readonly Type Type;

		// <summary>
		//   Total number of bits a variable of this type consumes in the flow vector.
		// </summary>
		public readonly int TotalLength;

		// <summary>
		//   Number of bits the simple fields of a variable of this type consume
		//   in the flow vector.
		// </summary>
		public readonly int Length;

		// <summary>
		//   This is only used by sub-structs.
		// </summary>
		public readonly int Offset;

		// <summary>
		//   If this is a struct.
		// </summary>
		public readonly bool IsStruct;	     

		// <summary>
		//   If this is a struct, all fields which are structs theirselves.
		// </summary>
		public TypeInfo[] SubStructInfo;

		protected readonly StructInfo struct_info;
		private static Hashtable type_hash = new Hashtable ();

		public static TypeInfo GetTypeInfo (Type type)
		{
			TypeInfo info = (TypeInfo) type_hash [type];
			if (info != null)
				return info;

			info = new TypeInfo (type);
			type_hash.Add (type, info);
			return info;
		}

		public static TypeInfo GetTypeInfo (TypeContainer tc)
		{
			TypeInfo info = (TypeInfo) type_hash [tc.TypeBuilder];
			if (info != null)
				return info;

			info = new TypeInfo (tc);
			type_hash.Add (tc.TypeBuilder, info);
			return info;
		}

		private TypeInfo (Type type)
		{
			this.Type = type;

			struct_info = StructInfo.GetStructInfo (type);
			if (struct_info != null) {
				Length = struct_info.Length;
				TotalLength = struct_info.TotalLength;
				SubStructInfo = struct_info.StructFields;
				IsStruct = true;
			} else {
				Length = 0;
				TotalLength = 1;
				IsStruct = false;
			}
		}

		private TypeInfo (TypeContainer tc)
		{
			this.Type = tc.TypeBuilder;

			struct_info = StructInfo.GetStructInfo (tc);
			if (struct_info != null) {
				Length = struct_info.Length;
				TotalLength = struct_info.TotalLength;
				SubStructInfo = struct_info.StructFields;
				IsStruct = true;
			} else {
				Length = 0;
				TotalLength = 1;
				IsStruct = false;
			}
		}

		protected TypeInfo (StructInfo struct_info, int offset)
		{
			this.struct_info = struct_info;
			this.Offset = offset;
			this.Length = struct_info.Length;
			this.TotalLength = struct_info.TotalLength;
			this.SubStructInfo = struct_info.StructFields;
			this.Type = struct_info.Type;
			this.IsStruct = true;
		}

		public int GetFieldIndex (string name)
		{
			if (struct_info == null)
				return 0;

			return struct_info [name];
		}

		public TypeInfo GetSubStruct (string name)
		{
			if (struct_info == null)
				return null;

			return struct_info.GetStructField (name);
		}

		// <summary>
		//   A struct's constructor must always assign all fields.
		//   This method checks whether it actually does so.
		// </summary>
		public bool IsFullyInitialized (FlowBranching branching, VariableInfo vi, Location loc)
		{
			if (struct_info == null)
				return true;

			bool ok = true;
			for (int i = 0; i < struct_info.Count; i++) {
				FieldInfo field = struct_info.Fields [i];

				if (!branching.IsFieldAssigned (vi, field.Name)) {
					Report.Error (171, loc,
						      "Field `" + TypeManager.CSharpName (Type) +
						      "." + field.Name + "' must be fully initialized " +
						      "before control leaves the constructor");
					ok = false;
				}
			}

			return ok;
		}

		public override string ToString ()
		{
			return String.Format ("TypeInfo ({0}:{1}:{2}:{3})",
					      Type, Offset, Length, TotalLength);
		}

		protected class StructInfo {
			public readonly Type Type;
			public readonly FieldInfo[] Fields;
			public readonly TypeInfo[] StructFields;
			public readonly int Count;
			public readonly int CountPublic;
			public readonly int CountNonPublic;
			public readonly int Length;
			public readonly int TotalLength;
			public readonly bool HasStructFields;

			private static Hashtable field_type_hash = new Hashtable ();
			private Hashtable struct_field_hash;
			private Hashtable field_hash;

			protected bool InTransit = false;

			// Private constructor.  To save memory usage, we only need to create one instance
			// of this class per struct type.
			private StructInfo (Type type)
			{
				this.Type = type;

				field_type_hash.Add (type, this);

				if (type is TypeBuilder) {
					TypeContainer tc = TypeManager.LookupTypeContainer (type);

					ArrayList fields = tc.Fields;

					ArrayList public_fields = new ArrayList ();
					ArrayList non_public_fields = new ArrayList ();

					if (fields != null) {
						foreach (Field field in fields) {
							if ((field.ModFlags & Modifiers.STATIC) != 0)
								continue;
							if ((field.ModFlags & Modifiers.PUBLIC) != 0)
								public_fields.Add (field.FieldBuilder);
							else
								non_public_fields.Add (field.FieldBuilder);
						}
					}

					CountPublic = public_fields.Count;
					CountNonPublic = non_public_fields.Count;
					Count = CountPublic + CountNonPublic;

					Fields = new FieldInfo [Count];
					public_fields.CopyTo (Fields, 0);
					non_public_fields.CopyTo (Fields, CountPublic);
				} else {
					FieldInfo[] public_fields = type.GetFields (
						BindingFlags.Instance|BindingFlags.Public);
					FieldInfo[] non_public_fields = type.GetFields (
						BindingFlags.Instance|BindingFlags.NonPublic);

					CountPublic = public_fields.Length;
					CountNonPublic = non_public_fields.Length;
					Count = CountPublic + CountNonPublic;

					Fields = new FieldInfo [Count];
					public_fields.CopyTo (Fields, 0);
					non_public_fields.CopyTo (Fields, CountPublic);
				}

				struct_field_hash = new Hashtable ();
				field_hash = new Hashtable ();

				Length = 0;
				StructFields = new TypeInfo [Count];
				StructInfo[] sinfo = new StructInfo [Count];

				InTransit = true;

				for (int i = 0; i < Count; i++) {
					FieldInfo field = (FieldInfo) Fields [i];

					sinfo [i] = GetStructInfo (field.FieldType);
					if (sinfo [i] == null)
						field_hash.Add (field.Name, ++Length);
					else if (sinfo [i].InTransit) {
						Report.Error (523, String.Format (
								      "Struct member '{0}.{1}' of type '{2}' causes " +
								      "a cycle in the structure layout",
								      type, field.Name, sinfo [i].Type));
						sinfo [i] = null;
						return;
					}
				}

				InTransit = false;

				TotalLength = Length + 1;
				for (int i = 0; i < Count; i++) {
					FieldInfo field = (FieldInfo) Fields [i];

					if (sinfo [i] == null)
						continue;

					field_hash.Add (field.Name, TotalLength);

					HasStructFields = true;
					StructFields [i] = new TypeInfo (sinfo [i], TotalLength);
					struct_field_hash.Add (field.Name, StructFields [i]);
					TotalLength += sinfo [i].TotalLength;
				}
			}

			public int this [string name] {
				get {
					if (field_hash.Contains (name))
						return (int) field_hash [name];
					else
						return 0;
				}
			}

			public TypeInfo GetStructField (string name)
			{
				return (TypeInfo) struct_field_hash [name];
			}

			public static StructInfo GetStructInfo (Type type)
			{
				if (!TypeManager.IsValueType (type) || TypeManager.IsEnumType (type) ||
				    TypeManager.IsBuiltinType (type))
					return null;

				StructInfo info = (StructInfo) field_type_hash [type];
				if (info != null)
					return info;

				return new StructInfo (type);
			}

			public static StructInfo GetStructInfo (TypeContainer tc)
			{
				StructInfo info = (StructInfo) field_type_hash [tc.TypeBuilder];
				if (info != null)
					return info;

				return new StructInfo (tc.TypeBuilder);
			}
		}
	}

	// <summary>
	//   This is used by the flow analysis code to store information about a single local variable
	//   or parameter.  Depending on the variable's type, we need to allocate one or more elements
	//   in the BitVector - if it's a fundamental or reference type, we just need to know whether
	//   it has been assigned or not, but for structs, we need this information for each of its fields.
	// </summary>
	public class VariableInfo {
		public readonly string Name;
		public readonly TypeInfo TypeInfo;

		// <summary>
		//   The bit offset of this variable in the flow vector.
		// </summary>
		public readonly int Offset;

		// <summary>
		//   The number of bits this variable needs in the flow vector.
		//   The first bit always specifies whether the variable as such has been assigned while
		//   the remaining bits contain this information for each of a struct's fields.
		// </summary>
		public readonly int Length;

		// <summary>
		//   If this is a parameter of local variable.
		// </summary>
		public readonly bool IsParameter;

		public readonly LocalInfo LocalInfo;
		public readonly int ParameterIndex;

		readonly VariableInfo Parent;
		VariableInfo[] sub_info;

		protected VariableInfo (string name, Type type, int offset)
		{
			this.Name = name;
			this.Offset = offset;
			this.TypeInfo = TypeInfo.GetTypeInfo (type);

			Length = TypeInfo.TotalLength;

			Initialize ();
		}

		protected VariableInfo (VariableInfo parent, TypeInfo type)
		{
			this.Name = parent.Name;
			this.TypeInfo = type;
			this.Offset = parent.Offset + type.Offset;
			this.Parent = parent;
			this.Length = type.TotalLength;

			this.IsParameter = parent.IsParameter;
			this.LocalInfo = parent.LocalInfo;
			this.ParameterIndex = parent.ParameterIndex;

			Initialize ();
		}

		protected void Initialize ()
		{
			TypeInfo[] sub_fields = TypeInfo.SubStructInfo;
			if (sub_fields != null) {
				sub_info = new VariableInfo [sub_fields.Length];
				for (int i = 0; i < sub_fields.Length; i++) {
					if (sub_fields [i] != null)
						sub_info [i] = new VariableInfo (this, sub_fields [i]);
				}
			} else
				sub_info = new VariableInfo [0];
		}

		public VariableInfo (LocalInfo local_info, int offset)
			: this (local_info.Name, local_info.VariableType, offset)
		{
			this.LocalInfo = local_info;
			this.IsParameter = false;
		}

		public VariableInfo (string name, Type type, int param_idx, int offset)
			: this (name, type, offset)
		{
			this.ParameterIndex = param_idx;
			this.IsParameter = true;
		}

		public bool IsAssigned (EmitContext ec)
		{
			return !ec.DoFlowAnalysis || ec.CurrentBranching.IsAssigned (this);
		}

		public bool IsAssigned (EmitContext ec, Location loc)
		{
			if (IsAssigned (ec))
				return true;

			Report.Error (165, loc,
				      "Use of unassigned local variable `" + Name + "'");
			ec.CurrentBranching.SetAssigned (this);
			return false;
		}

		public bool IsAssigned (MyBitVector vector)
		{
			if (vector [Offset])
				return true;

			for (VariableInfo parent = Parent; parent != null; parent = parent.Parent)
				if (vector [parent.Offset])
					return true;

			// Return unless this is a struct.
			if (!TypeInfo.IsStruct)
				return false;

			// Ok, so each field must be assigned.
			for (int i = 0; i < TypeInfo.Length; i++) {
				if (!vector [Offset + i + 1])
					return false;
			}

			// Ok, now check all fields which are structs.
			for (int i = 0; i < sub_info.Length; i++) {
				VariableInfo sinfo = sub_info [i];
				if (sinfo == null)
					continue;

				if (!sinfo.IsAssigned (vector))
					return false;
			}

			vector [Offset] = true;
			return true;
		}

		public void SetAssigned (EmitContext ec)
		{
			if (ec.DoFlowAnalysis)
				ec.CurrentBranching.SetAssigned (this);
		}

		public void SetAssigned (MyBitVector vector)
		{
			vector [Offset] = true;
		}

		public bool IsFieldAssigned (EmitContext ec, string name, Location loc)
		{
			if (!ec.DoFlowAnalysis || ec.CurrentBranching.IsFieldAssigned (this, name))
				return true;

			Report.Error (170, loc,
				      "Use of possibly unassigned field `" + name + "'");
			ec.CurrentBranching.SetFieldAssigned (this, name);
			return false;
		}

		public bool IsFieldAssigned (MyBitVector vector, string field_name)
		{
			int field_idx = TypeInfo.GetFieldIndex (field_name);

			if (field_idx == 0)
				return true;

			return vector [Offset + field_idx];
		}

		public void SetFieldAssigned (EmitContext ec, string name)
		{
			if (ec.DoFlowAnalysis)
				ec.CurrentBranching.SetFieldAssigned (this, name);
		}

		public void SetFieldAssigned (MyBitVector vector, string field_name)
		{
			int field_idx = TypeInfo.GetFieldIndex (field_name);

			if (field_idx == 0)
				return;

			vector [Offset + field_idx] = true;
		}

		public VariableInfo GetSubStruct (string name)
		{
			TypeInfo type = TypeInfo.GetSubStruct (name);

			if (type == null)
				return null;

			return new VariableInfo (this, type);
		}

		public override string ToString ()
		{
			return String.Format ("VariableInfo ({0}:{1}:{2}:{3}:{4})",
					      Name, TypeInfo, Offset, Length, IsParameter);
		}
	}

	// <summary>
	//   This is used by the flow code to hold the `layout' of the flow vector for
	//   all locals and all parameters (ie. we create one instance of this class for the
	//   locals and another one for the params).
	// </summary>
	public class VariableMap {
		// <summary>
		//   The number of variables in the map.
		// </summary>
		public readonly int Count;

		// <summary>
		//   Total length of the flow vector for this map.
		// <summary>
		public readonly int Length;

		// <summary>
		//   Type and name of all the variables.
		//   Note that this is null for variables for which we do not need to compute
		//   assignment info.
		// </summary>
		public readonly Type[] VariableTypes;
		public readonly string[] VariableNames;

		VariableInfo[] map;

		public VariableMap (InternalParameters ip)
		{
			Count = ip != null ? ip.Count : 0;
			map = new VariableInfo [Count];
			VariableNames = new string [Count];
			VariableTypes = new Type [Count];
			Length = 0;

			for (int i = 0; i < Count; i++) {
				Parameter.Modifier mod = ip.ParameterModifier (i);

				if ((mod & Parameter.Modifier.OUT) == 0)
					continue;

				VariableNames [i] = ip.ParameterName (i);
				VariableTypes [i] = ip.ParameterType (i).GetElementType ();

				map [i] = new VariableInfo (VariableNames [i], VariableTypes [i], i, Length);
				Length += map [i].Length;
			}
		}

		public VariableMap (LocalInfo[] locals)
			: this (null, locals)
		{ }

		public VariableMap (VariableMap parent, LocalInfo[] locals)
		{
			int offset = 0, start = 0;
			if (parent != null) {
				offset = parent.Length;
				start = parent.Count;
			}

			Count = locals.Length + start;
			map = new VariableInfo [Count];
			VariableNames = new string [Count];
			VariableTypes = new Type [Count];
			Length = offset;

			if (parent != null) {
				parent.map.CopyTo (map, 0);
				parent.VariableNames.CopyTo (VariableNames, 0);
				parent.VariableTypes.CopyTo (VariableTypes, 0);
			}

			for (int i = start; i < Count; i++) {
				LocalInfo li = locals [i-start];

				if (li.VariableType == null)
					continue;

				VariableNames [i] = li.Name;
				VariableTypes [i] = li.VariableType;

				map [i] = li.VariableInfo = new VariableInfo (li, Length);
				Length += map [i].Length;
			}
		}

		// <summary>
		//   Returns the VariableInfo for variable @index or null if we don't need to
		//   compute assignment info for this variable.
		// </summary>
		public VariableInfo this [int index] {
			get {
				return map [index];
			}
		}

		public override string ToString ()
		{
			return String.Format ("VariableMap ({0}:{1})", Count, Length);
		}
	}

	public class LocalInfo {
		public Expression Type;

		//
		// Most of the time a variable will be stored in a LocalBuilder
		//
		// But sometimes, it will be stored in a field.  The context of the field will
		// be stored in the EmitContext
		//
		//
		public LocalBuilder LocalBuilder;
		public FieldBuilder FieldBuilder;

		public Type VariableType;
		public readonly string Name;
		public readonly Location Location;
		public readonly Block Block;

		public VariableInfo VariableInfo;

		public bool Used;
		public bool Assigned;
		public bool ReadOnly;
		
		public LocalInfo (Expression type, string name, Block block, Location l)
		{
			Type = type;
			Name = name;
			Block = block;
			LocalBuilder = null;
			Location = l;
		}

		public LocalInfo (TypeContainer tc, Block block, Location l)
		{
			VariableType = tc.TypeBuilder;
			Block = block;
			LocalBuilder = null;
			Location = l;
		}

		public bool IsThisAssigned (EmitContext ec, Location loc)
		{
			VariableInfo vi = Block.GetVariableInfo (this);
			if (vi == null)
				throw new Exception ();

			if (!ec.DoFlowAnalysis || ec.CurrentBranching.IsAssigned (vi))
				return true;

			return vi.TypeInfo.IsFullyInitialized (ec.CurrentBranching, vi, loc);
		}

		public bool Resolve (DeclSpace decl)
		{
			if (VariableType == null)
				VariableType = decl.ResolveType (Type, false, Location);

			if (VariableType == null)
				return false;

			return true;
		}

		public void MakePinned ()
		{
			TypeManager.MakePinned (LocalBuilder);
		}

		public override string ToString ()
		{
			return String.Format ("LocalInfo ({0},{1},{2},{3})",
					      Name, Type, VariableInfo, Location);
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
		public readonly Location  StartLocation;
		public Location           EndLocation = Location.Null;

		[Flags]
		public enum Flags : byte {
			Implicit  = 1,
			Unchecked = 2
		}
		Flags flags;

		public bool Implicit {
			get {
				return (flags & Flags.Implicit) != 0;
			}
		}

		public bool Unchecked {
			get {
				return (flags & Flags.Unchecked) != 0;
			}
			set {
				flags |= Flags.Unchecked;
			}
		}

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
		// If this is a switch section, the enclosing switch block.
		//
		Block switch_block;

		bool used = false;

		static int id;

		int this_id;
		
		public Block (Block parent)
			: this (parent, (Flags) 0, Location.Null, Location.Null)
		{ }

		public Block (Block parent, Flags flags)
			: this (parent, flags, Location.Null, Location.Null)
		{ }

		public Block (Block parent, Flags flags, Parameters parameters)
			: this (parent, flags, parameters, Location.Null, Location.Null)
		{ }

		public Block (Block parent, Location start, Location end)
			: this (parent, (Flags) 0, start, end)
		{ }

		public Block (Block parent, Parameters parameters, Location start, Location end)
			: this (parent, (Flags) 0, parameters, start, end)
		{ }

		public Block (Block parent, Flags flags, Location start, Location end)
			: this (parent, flags, Parameters.EmptyReadOnlyParameters, start, end)
		{ }

		public Block (Block parent, Flags flags, Parameters parameters,
			      Location start, Location end)
		{
			if (parent != null)
				parent.AddChild (this);
			
			this.Parent = parent;
			this.flags = flags;
			this.parameters = parameters;
			this.StartLocation = start;
			this.EndLocation = end;
			this.loc = start;
			this_id = id++;
			statements = new ArrayList ();
		}

		public Block CreateSwitchBlock (Location start)
		{
			Block new_block = new Block (this, start, start);
			new_block.switch_block = this;
			return new_block;
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
			if (switch_block != null)
				return switch_block.AddLabel (name, target);

			if (labels == null)
				labels = new Hashtable ();
			if (labels.Contains (name))
				return false;
			
			labels.Add (name, target);
			return true;
		}

		public LabeledStatement LookupLabel (string name)
		{
			if (switch_block != null)
				return switch_block.LookupLabel (name);

			if (labels != null){
				if (labels.Contains (name))
					return ((LabeledStatement) labels [name]);
			}

			if (Parent != null)
				return Parent.LookupLabel (name);

			return null;
		}

		LocalInfo this_variable = null;

		// <summary>
		//   Returns the "this" instance variable of this block.
		//   See AddThisVariable() for more information.
		// </summary>
		public LocalInfo ThisVariable {
			get {
				if (this_variable != null)
					return this_variable;
				else if (Parent != null)
					return Parent.ThisVariable;
				else
					return null;
			}
		}

		Hashtable child_variable_names;

		// <summary>
		//   Marks a variable with name @name as being used in a child block.
		//   If a variable name has been used in a child block, it's illegal to
		//   declare a variable with the same name in the current block.
		// </summary>
		public void AddChildVariableName (string name)
		{
			if (child_variable_names == null)
				child_variable_names = new Hashtable ();

			if (!child_variable_names.Contains (name))
				child_variable_names.Add (name, true);
		}

		// <summary>
		//   Marks all variables from block @block and all its children as being
		//   used in a child block.
		// </summary>
		public void AddChildVariableNames (Block block)
		{
			if (block.Variables != null) {
				foreach (string name in block.Variables.Keys)
					AddChildVariableName (name);
			}

			if (block.children != null) {
				foreach (Block child in block.children)
					AddChildVariableNames (child);
			}

			if (block.child_variable_names != null) {
				foreach (string name in block.child_variable_names.Keys)
					AddChildVariableName (name);
			}
		}

		// <summary>
		//   Checks whether a variable name has already been used in a child block.
		// </summary>
		public bool IsVariableNameUsedInChildBlock (string name)
		{
			if (child_variable_names == null)
				return false;

			return child_variable_names.Contains (name);
		}

		// <summary>
		//   This is used by non-static `struct' constructors which do not have an
		//   initializer - in this case, the constructor must initialize all of the
		//   struct's fields.  To do this, we add a "this" variable and use the flow
		//   analysis code to ensure that it's been fully initialized before control
		//   leaves the constructor.
		// </summary>
		public LocalInfo AddThisVariable (TypeContainer tc, Location l)
		{
			if (this_variable != null)
				return this_variable;

			if (variables == null)
				variables = new Hashtable ();

			this_variable = new LocalInfo (tc, this, l);

			variables.Add ("this", this_variable);

			return this_variable;
		}

		public LocalInfo AddVariable (Expression type, string name, Parameters pars, Location l)
		{
			if (variables == null)
				variables = new Hashtable ();

			LocalInfo vi = GetLocalInfo (name);
			if (vi != null) {
				if (vi.Block != this)
					Report.Error (136, l, "A local variable named `" + name + "' " +
						      "cannot be declared in this scope since it would " +
						      "give a different meaning to `" + name + "', which " +
						      "is already used in a `parent or current' scope to " +
						      "denote something else");
				else
					Report.Error (128, l, "A local variable `" + name + "' is already " +
						      "defined in this scope");
				return null;
			}

			if (IsVariableNameUsedInChildBlock (name)) {
				Report.Error (136, l, "A local variable named `" + name + "' " +
					      "cannot be declared in this scope since it would " +
					      "give a different meaning to `" + name + "', which " +
					      "is already used in a `child' scope to denote something " +
					      "else");
				return null;
			}

			if (pars != null) {
				int idx = 0;
				Parameter p = pars.GetParameterByName (name, out idx);
				if (p != null) {
					Report.Error (136, l, "A local variable named `" + name + "' " +
						      "cannot be declared in this scope since it would " +
						      "give a different meaning to `" + name + "', which " +
						      "is already used in a `parent or current' scope to " +
						      "denote something else");
					return null;
				}
			}
			
			vi = new LocalInfo (type, name, this, l);

			variables.Add (name, vi);

			if (variables_initialized)
				throw new Exception ();

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

		public LocalInfo GetLocalInfo (string name)
		{
			if (variables != null) {
				object temp;
				temp = variables [name];

				if (temp != null){
					return (LocalInfo) temp;
				}
			}

			if (Parent != null)
				return Parent.GetLocalInfo (name);

			return null;
		}

		public VariableInfo GetVariableInfo (LocalInfo li)
		{
			return li.VariableInfo;
		}

		public Expression GetVariableType (string name)
		{
			LocalInfo vi = GetLocalInfo (name);

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

		Parameters parameters = null;
		public Parameters Parameters {
			get {
				if (Parent != null)
					return Parent.Parameters;

				return parameters;
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

		VariableMap param_map, local_map;
		bool variables_initialized = false;

		public VariableMap ParameterMap {
			get {
				if (!variables_initialized)
					throw new Exception ();

				return param_map;
			}
		}

		public VariableMap LocalMap {
			get {
				if (!variables_initialized)
					throw new Exception ();

				return local_map;
			}
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
		public void EmitMeta (EmitContext ec, InternalParameters ip, Block toplevel)
		{
			DeclSpace ds = ec.DeclSpace;
			ILGenerator ig = ec.ig;

			//
			// Compute the VariableMap's.
			//
			// Unfortunately, we don't know the type when adding variables with
			// AddVariable(), so we need to compute this info here.
			//

			LocalInfo[] locals;
			if (variables != null) {
				foreach (LocalInfo li in variables.Values)
					li.Resolve (ec.DeclSpace);

				locals = new LocalInfo [variables.Count];
				variables.Values.CopyTo (locals, 0);
			} else
				locals = new LocalInfo [0];

			if (Parent != null)
				local_map = new VariableMap (Parent.LocalMap, locals);
			else
				local_map = new VariableMap (locals);

			param_map = new VariableMap (ip);
			variables_initialized = true;

			bool old_check_state = ec.ConstantCheckState;
			ec.ConstantCheckState = (flags & Flags.Unchecked) == 0;
			bool remap_locals = ec.RemapToProxy;
				
			//
			// Process this block variables
			//
			if (variables != null){
				foreach (DictionaryEntry de in variables){
					string name = (string) de.Key;
					LocalInfo vi = (LocalInfo) de.Value;
					
					if (vi.VariableType == null)
						continue;

					Type variable_type = vi.VariableType;

					if (variable_type.IsPointer){
						//
						// Am not really convinced that this test is required (Microsoft does it)
						// but the fact is that you would not be able to use the pointer variable
						// *anyways*
						//
						if (!TypeManager.VerifyUnManaged (variable_type.GetElementType (), vi.Location))
							continue;
					}

					if (remap_locals)
						vi.FieldBuilder = ec.MapVariable (name, vi.VariableType);
					else
						vi.LocalBuilder = ig.DeclareLocal (vi.VariableType);

					if (constants == null)
						continue;

					Expression cv = (Expression) constants [name];
					if (cv == null)
						continue;

					ec.CurrentBlock = this;
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
			ec.ConstantCheckState = old_check_state;

			//
			// Now, handle the children
			//
			if (children != null){
				foreach (Block b in children)
					b.EmitMeta (ec, ip, toplevel);
			}
		}

		public void UsageWarning ()
		{
			string name;
			
			if (variables != null){
				foreach (DictionaryEntry de in variables){
					LocalInfo vi = (LocalInfo) de.Value;
					
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

		bool has_ret = false;

		public override bool Resolve (EmitContext ec)
		{
			Block prev_block = ec.CurrentBlock;
			bool ok = true;

			ec.CurrentBlock = this;
			ec.StartFlowBranching (this);

			Report.Debug (1, "RESOLVE BLOCK", StartLocation, ec.CurrentBranching);

			ArrayList new_statements = new ArrayList ();
			bool unreachable = false, warning_shown = false;

 			foreach (Statement s in statements){
				if (unreachable && !(s is LabeledStatement)) {
					if (!warning_shown && !(s is EmptyStatement)) {
						warning_shown = true;
						Warning_DeadCodeFound (s.loc);
					}

					continue;
				}

				if (s.Resolve (ec) == false) {
 					ok = false;
					continue;
				}

				if (s is LabeledStatement)
					unreachable = false;
				else
					unreachable = ! ec.CurrentBranching.IsReachable ();

				new_statements.Add (s);
			}

			statements = new_statements;

			Report.Debug (1, "RESOLVE BLOCK DONE", StartLocation, ec.CurrentBranching);

			FlowReturns returns = ec.EndFlowBranching ();
			ec.CurrentBlock = prev_block;

			// If we're a non-static `struct' constructor which doesn't have an
			// initializer, then we must initialize all of the struct's fields.
			if ((this_variable != null) && (returns != FlowReturns.EXCEPTION) &&
			    !this_variable.IsThisAssigned (ec, loc))
				ok = false;

			if ((labels != null) && (RootContext.WarningLevel >= 2)) {
				foreach (LabeledStatement label in labels.Values)
					if (!label.HasBeenReferenced)
						Report.Warning (164, label.Location,
								"This label has not been referenced");
			}

			if ((returns == FlowReturns.ALWAYS) ||
			    (returns == FlowReturns.EXCEPTION) ||
			    (returns == FlowReturns.UNREACHABLE))
				has_ret = true;

			return ok;
		}
		
		protected override bool DoEmit (EmitContext ec)
		{
			foreach (Statement s in statements)
				s.Emit (ec);

			return has_ret;
		}

		public override bool Emit (EmitContext ec)
		{
			Block prev_block = ec.CurrentBlock;

			ec.CurrentBlock = this;

			bool emit_debug_info = (CodeGen.SymbolWriter != null);
			bool is_lexical_block = !Implicit && (Parent != null);

			if (emit_debug_info) {
				if (is_lexical_block)
					ec.ig.BeginScope ();

				if (variables != null) {
					foreach (DictionaryEntry de in variables) {
						string name = (string) de.Key;
						LocalInfo vi = (LocalInfo) de.Value;

						if (vi.LocalBuilder == null)
							continue;

						vi.LocalBuilder.SetLocalSymInfo (name);
					}
				}
			}

			ec.Mark (StartLocation, true);
			bool retval = DoEmit (ec);
			ec.Mark (EndLocation, true); 

			if (emit_debug_info && is_lexical_block)
				ec.ig.EndScope ();

			ec.CurrentBlock = prev_block;

			return retval;
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
				Report.Error (150, loc, "A constant value is expected, got: " + e);
				return false;
			}

			if (e is StringConstant || e is NullLiteral){
				if (required_type == TypeManager.string_type){
					converted = e;
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
		Expression new_expr;

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
				
				e = Convert.ImplicitUserConversion (ec, Expr, tt, loc);
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
				rgKeyBlocks.Add (new KeyBlock (System.Convert.ToInt64 (key)));

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
					bool fNextBlock = (key is UInt64) ? (ulong) key > (ulong) kbCurr.nLast :
						System.Convert.ToInt64 (key) > kbCurr.nLast;
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
						EmitObjectInteger (ig, System.Convert.ChangeType (kb.nFirst, typeKeys));
						ig.Emit (OpCodes.Blt, lblDefault);
						ig.Emit (OpCodes.Ldloc, val);
						EmitObjectInteger (ig, System.Convert.ChangeType (kb.nFirst, typeKeys));
						ig.Emit (OpCodes.Bgt, lblDefault);

						// normalize range
						ig.Emit (OpCodes.Ldloc, val);
						if (kb.nFirst != 0)
						{
							EmitObjectInteger (ig, System.Convert.ChangeType (kb.nFirst, typeKeys));
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
						if (System.Convert.ToInt64 (key) == kb.nFirst + iJump)
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
				bool returns = ss.Block.Emit (ec);
				fAllReturn &= returns;
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
			bool null_found;
			
			ig.Emit (OpCodes.Ldloc, val);
			
			if (Elements.Contains (NullLiteral.Null)){
				ig.Emit (OpCodes.Brfalse, null_target);
			} else
				ig.Emit (OpCodes.Brfalse, default_target);
			
			ig.Emit (OpCodes.Ldloc, val);
			ig.Emit (OpCodes.Call, TypeManager.string_isinterneted_string);
			ig.Emit (OpCodes.Stloc, val);
		
			int section_count = Sections.Count;
			for (int section = 0; section < section_count; section++){
				SwitchSection ss = (SwitchSection) Sections [section];
				Label sec_begin = ig.DefineLabel ();

				if (pending_goto_end)
					ig.Emit (OpCodes.Br, end_of_switch);

				int label_count = ss.Labels.Count;
				null_found = false;
				for (int label = 0; label < label_count; label++){
					SwitchLabel sl = (SwitchLabel) ss.Labels [label];
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
						StringConstant str = (StringConstant) lit;
						
						ig.Emit (OpCodes.Ldloc, val);
						ig.Emit (OpCodes.Ldstr, str.Value);
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
				if (null_found)
					ig.MarkLabel (null_target);
				ig.MarkLabel (sec_begin);
				foreach (SwitchLabel sl in ss.Labels)
					ig.MarkLabel (sl.ILLabelCode);

				bool returns = ss.Block.Emit (ec);
				if (returns)
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
			Expr = Expr.Resolve (ec);
			if (Expr == null)
				return false;

			new_expr = SwitchGoverningType (ec, Expr.Type);
			if (new_expr == null){
				Report.Error (151, loc, "An integer type or string was expected for switch");
				return false;
			}

			// Validate switch.
			SwitchType = new_expr.Type;

			if (!CheckSwitch (ec))
				return false;

			Switch old_switch = ec.Switch;
			ec.Switch = this;
			ec.Switch.SwitchType = SwitchType;

			ec.StartFlowBranching (FlowBranchingType.SWITCH, loc);

			bool first = true;
			foreach (SwitchSection ss in Sections){
				if (!first)
					ec.CurrentBranching.CreateSibling ();
				else
					first = false;

				if (ss.Block.Resolve (ec) != true)
					return false;
			}


			if (!got_default)
				ec.CurrentBranching.CreateSibling ();

			ec.EndFlowBranching ();
			ec.Switch = old_switch;

			return true;
		}
		
		protected override bool DoEmit (EmitContext ec)
		{
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
		
		protected override bool DoEmit (EmitContext ec)
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
			b.Unchecked = true;
		}

		public override bool Resolve (EmitContext ec)
		{
			bool previous_state = ec.CheckState;
			bool previous_state_const = ec.ConstantCheckState;

			ec.CheckState = false;
			ec.ConstantCheckState = false;
			bool ret = Block.Resolve (ec);
			ec.CheckState = previous_state;
			ec.ConstantCheckState = previous_state_const;

			return ret;
		}
		
		protected override bool DoEmit (EmitContext ec)
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
			b.Unchecked = false;
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

		protected override bool DoEmit (EmitContext ec)
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
		
		protected override bool DoEmit (EmitContext ec)
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
		Type expr_type;
		FixedData[] data;

		struct FixedData {
			public bool is_object;
			public LocalInfo vi;
			public Expression expr;
			public Expression converted;
		}			

		public Fixed (Expression type, ArrayList decls, Statement stmt, Location l)
		{
			this.type = type;
			declarators = decls;
			statement = stmt;
			loc = l;
		}

		public override bool Resolve (EmitContext ec)
		{
			if (!ec.InUnsafe){
				Expression.UnsafeError (loc);
				return false;
			}
			
			expr_type = ec.DeclSpace.ResolveType (type, false, loc);
			if (expr_type == null)
				return false;

			if (ec.RemapToProxy){
				Report.Error (-210, loc, "Fixed statement not allowed in iterators");
				return false;
			}
			
			data = new FixedData [declarators.Count];

			if (!expr_type.IsPointer){
				Report.Error (209, loc, "Variables in a fixed statement must be pointers");
				return false;
			}
			
			int i = 0;
			foreach (Pair p in declarators){
				LocalInfo vi = (LocalInfo) p.First;
				Expression e = (Expression) p.Second;

				vi.VariableInfo = null;

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
						return false;
					}
					
					e = e.Resolve (ec);
					if (e == null)
						return false;

					child = ((Unary) e).Expr;
					
					if (!TypeManager.VerifyUnManaged (child.Type, loc))
						return false;

					data [i].is_object = true;
					data [i].expr = e;
					data [i].converted = null;
					data [i].vi = vi;
					i++;

					continue;
				}

				e = e.Resolve (ec);
				if (e == null)
					return false;

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
						return false;

					//
					// and T* is implicitly convertible to the
					// pointer type given in the fixed statement.
					//
					ArrayPtr array_ptr = new ArrayPtr (e, loc);
					
					Expression converted = Convert.ImplicitConversionRequired (
						ec, array_ptr, vi.VariableType, loc);
					if (converted == null)
						return false;

					data [i].is_object = false;
					data [i].expr = e;
					data [i].converted = converted;
					data [i].vi = vi;
					i++;

					continue;
				}

				//
				// Case 3: string
				//
				if (e.Type == TypeManager.string_type){
					data [i].is_object = false;
					data [i].expr = e;
					data [i].converted = null;
					data [i].vi = vi;
					i++;
				}
			}

			return statement.Resolve (ec);
		}
		
		protected override bool DoEmit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;

			bool is_ret = false;
			LocalBuilder [] clear_list = new LocalBuilder [data.Length];
			
			for (int i = 0; i < data.Length; i++) {
				LocalInfo vi = data [i].vi;

				//
				// Case 1: & object.
				//
				if (data [i].is_object) {
					//
					// Store pointer in pinned location
					//
					data [i].expr.Emit (ec);
					ig.Emit (OpCodes.Stloc, vi.LocalBuilder);
					clear_list [i] = vi.LocalBuilder;
					continue;
				}

				//
				// Case 2: Array
				//
				if (data [i].expr.Type.IsArray){
					//
					// Store pointer in pinned location
					//
					data [i].converted.Emit (ec);
					
					ig.Emit (OpCodes.Stloc, vi.LocalBuilder);
					clear_list [i] = vi.LocalBuilder;
					continue;
				}

				//
				// Case 3: string
				//
				if (data [i].expr.Type == TypeManager.string_type){
					LocalBuilder pinned_string = ig.DeclareLocal (TypeManager.string_type);
					TypeManager.MakePinned (pinned_string);
					clear_list [i] = pinned_string;
					
					data [i].expr.Emit (ec);
					ig.Emit (OpCodes.Stloc, pinned_string);

					Expression sptr = new StringPtr (pinned_string, loc);
					Expression converted = Convert.ImplicitConversionRequired (
						ec, sptr, vi.VariableType, loc);
					
					if (converted == null)
						continue;

					converted.Emit (ec);
					ig.Emit (OpCodes.Stloc, vi.LocalBuilder);
				}
			}

			is_ret = statement.Emit (ec);

			if (is_ret)
				return is_ret;
			//
			// Clear the pinned variable
			//
			for (int i = 0; i < data.Length; i++) {
				LocalInfo vi = data [i].vi;

				if (data [i].is_object || data [i].expr.Type.IsArray) {
					ig.Emit (OpCodes.Ldc_I4_0);
					ig.Emit (OpCodes.Conv_U);
					ig.Emit (OpCodes.Stloc, clear_list [i]);
				} else if (data [i].expr.Type == TypeManager.string_type){
					ig.Emit (OpCodes.Ldnull);
					ig.Emit (OpCodes.Stloc, clear_list [i]);
				}
			}

			return is_ret;
		}
	}
	
	public class Catch {
		public readonly string Name;
		public readonly Block  Block;
		public readonly Location Location;

		Expression type_expr;
		Type type;
		
		public Catch (Expression type, string name, Block block, Location l)
		{
			type_expr = type;
			Name = name;
			Block = block;
			Location = l;
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

		public bool Resolve (EmitContext ec)
		{
			if (type_expr != null) {
				type = ec.DeclSpace.ResolveType (type_expr, false, Location);
				if (type == null)
					return false;

				if (type != TypeManager.exception_type && !type.IsSubclassOf (TypeManager.exception_type)){
					Report.Error (155, Location,
						      "The type caught or thrown must be derived " +
						      "from System.Exception");
					return false;
				}
			} else
				type = null;

			if (!Block.Resolve (ec))
				return false;

			return true;
		}
	}

	public class Try : Statement {
		public readonly Block Fini, Block;
		public readonly ArrayList Specific;
		public readonly Catch General;
		
		//
		// specific, general and fini might all be null.
		//
		public Try (Block block, ArrayList specific, Catch general, Block fini, Location l)
		{
			if (specific == null && general == null){
				Console.WriteLine ("CIR.Try: Either specific or general have to be non-null");
			}
			
			this.Block = block;
			this.Specific = specific;
			this.General = general;
			this.Fini = fini;
			loc = l;
		}

		public override bool Resolve (EmitContext ec)
		{
			bool ok = true;
			
			ec.StartFlowBranching (FlowBranchingType.EXCEPTION, Block.StartLocation);

			Report.Debug (1, "START OF TRY BLOCK", Block.StartLocation);

			bool old_in_try = ec.InTry;
			ec.InTry = true;

			if (!Block.Resolve (ec))
				ok = false;

			ec.InTry = old_in_try;

			FlowBranching.UsageVector vector = ec.CurrentBranching.CurrentUsageVector;

			Report.Debug (1, "START OF CATCH BLOCKS", vector);

			foreach (Catch c in Specific){
				ec.CurrentBranching.CreateSibling ();
				Report.Debug (1, "STARTED SIBLING FOR CATCH", ec.CurrentBranching);

				if (c.Name != null) {
					LocalInfo vi = c.Block.GetLocalInfo (c.Name);
					if (vi == null)
						throw new Exception ();

					vi.VariableInfo = null;
				}

				bool old_in_catch = ec.InCatch;
				ec.InCatch = true;

				if (!c.Resolve (ec))
					ok = false;

				ec.InCatch = old_in_catch;

				FlowBranching.UsageVector current = ec.CurrentBranching.CurrentUsageVector;

				if (!current.AlwaysReturns && !current.AlwaysBreaks)
					vector.AndLocals (current);
				else
					vector.Or (current);
			}

			Report.Debug (1, "END OF CATCH BLOCKS", ec.CurrentBranching);

			if (General != null){
				ec.CurrentBranching.CreateSibling ();
				Report.Debug (1, "STARTED SIBLING FOR GENERAL", ec.CurrentBranching);

				bool old_in_catch = ec.InCatch;
				ec.InCatch = true;

				if (!General.Resolve (ec))
					ok = false;

				ec.InCatch = old_in_catch;

				FlowBranching.UsageVector current = ec.CurrentBranching.CurrentUsageVector;

				if (!current.AlwaysReturns && !current.AlwaysBreaks)
					vector.AndLocals (current);
				else	
					vector.Or (current);
			}

			Report.Debug (1, "END OF GENERAL CATCH BLOCKS", ec.CurrentBranching);

			if (Fini != null) {
				if (ok)
					ec.CurrentBranching.CreateSiblingForFinally ();
				Report.Debug (1, "STARTED SIBLING FOR FINALLY", ec.CurrentBranching, vector);

				bool old_in_finally = ec.InFinally;
				ec.InFinally = true;

				if (!Fini.Resolve (ec))
					ok = false;

				ec.InFinally = old_in_finally;
			}

			FlowReturns returns = ec.EndFlowBranching ();

			FlowBranching.UsageVector f_vector = ec.CurrentBranching.CurrentUsageVector;

			Report.Debug (1, "END OF FINALLY", ec.CurrentBranching, returns, vector, f_vector);

			if ((returns == FlowReturns.SOMETIMES) || (returns == FlowReturns.ALWAYS)) {
				ec.CurrentBranching.CheckOutParameters (f_vector.Parameters, loc);
			}

			ec.CurrentBranching.CurrentUsageVector.Or (vector);

			Report.Debug (1, "END OF TRY", ec.CurrentBranching);

			if (returns != FlowReturns.ALWAYS) {
				// Unfortunately, System.Reflection.Emit automatically emits a leave
				// to the end of the finally block.  This is a problem if `returns'
				// is true since we may jump to a point after the end of the method.
				// As a workaround, emit an explicit ret here.
				ec.NeedExplicitReturn = true;
			}

			return ok;
		}
		
		protected override bool DoEmit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			Label end;
			Label finish = ig.DefineLabel ();;
			bool returns;

			ec.TryCatchLevel++;
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
				LocalInfo vi;
				
				ig.BeginCatchBlock (c.CatchType);

				if (c.Name != null){
					vi = c.Block.GetLocalInfo (c.Name);
					if (vi == null)
						throw new Exception ("Variable does not exist in this block");

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
			ec.TryCatchLevel--;

			return returns;
		}
	}

	public class Using : Statement {
		object expression_or_block;
		Statement Statement;
		ArrayList var_list;
		Expression expr;
		Type expr_type;
		Expression conv;
		Expression [] converted_vars;
		ExpressionStatement [] assign;
		
		public Using (object expression_or_block, Statement stmt, Location l)
		{
			this.expression_or_block = expression_or_block;
			Statement = stmt;
			loc = l;
		}

		//
		// Resolves for the case of using using a local variable declaration.
		//
		bool ResolveLocalVariableDecls (EmitContext ec)
		{
			bool need_conv = false;
			expr_type = ec.DeclSpace.ResolveType (expr, false, loc);
			int i = 0;

			if (expr_type == null)
				return false;

			//
			// The type must be an IDisposable or an implicit conversion
			// must exist.
			//
			converted_vars = new Expression [var_list.Count];
			assign = new ExpressionStatement [var_list.Count];
			if (!TypeManager.ImplementsInterface (expr_type, TypeManager.idisposable_type)){
				foreach (DictionaryEntry e in var_list){
					Expression var = (Expression) e.Key;

					var = var.ResolveLValue (ec, new EmptyExpression ());
					if (var == null)
						return false;
					
					converted_vars [i] = Convert.ImplicitConversionRequired (
						ec, var, TypeManager.idisposable_type, loc);

					if (converted_vars [i] == null)
						return false;
					i++;
				}
				need_conv = true;
			}

			i = 0;
			foreach (DictionaryEntry e in var_list){
				LocalVariableReference var = (LocalVariableReference) e.Key;
				Expression new_expr = (Expression) e.Value;
				Expression a;

				a = new Assign (var, new_expr, loc);
				a = a.Resolve (ec);
				if (a == null)
					return false;

				if (!need_conv)
					converted_vars [i] = var;
				assign [i] = (ExpressionStatement) a;
				i++;
			}

			return true;
		}

		bool ResolveExpression (EmitContext ec)
		{
			if (!TypeManager.ImplementsInterface (expr_type, TypeManager.idisposable_type)){
				conv = Convert.ImplicitConversionRequired (
					ec, expr, TypeManager.idisposable_type, loc);

				if (conv == null)
					return false;
			}

			return true;
		}
		
		//
		// Emits the code for the case of using using a local variable declaration.
		//
		bool EmitLocalVariableDecls (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			int i = 0;

			bool old_in_try = ec.InTry;
			ec.InTry = true;
			for (i = 0; i < assign.Length; i++) {
				assign [i].EmitStatement (ec);
				
				ig.BeginExceptionBlock ();
			}
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

		bool EmitExpression (EmitContext ec)
		{
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
			if (expression_or_block is DictionaryEntry){
				expr = (Expression) ((DictionaryEntry) expression_or_block).Key;
				var_list = (ArrayList)((DictionaryEntry)expression_or_block).Value;

				if (!ResolveLocalVariableDecls (ec))
					return false;

			} else if (expression_or_block is Expression){
				expr = (Expression) expression_or_block;

				expr = expr.Resolve (ec);
				if (expr == null)
					return false;

				expr_type = expr.Type;

				if (!ResolveExpression (ec))
					return false;
			}			

			return Statement.Resolve (ec);
		}
		
		protected override bool DoEmit (EmitContext ec)
		{
			if (expression_or_block is DictionaryEntry)
				return EmitLocalVariableDecls (ec);
			else if (expression_or_block is Expression)
				return EmitExpression (ec);

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
		ForeachHelperMethods hm;
		Expression empty, conv;
		Type array_type, element_type;
		Type var_type;
		
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
			if (expr == null)
				return false;

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
			      expr.eclass == ExprClass.PropertyAccess || expr.eclass == ExprClass.IndexerAccess)){
				error1579 (expr.Type);
				return false;
			}

			if (expr.Type.IsArray) {
				array_type = expr.Type;
				element_type = array_type.GetElementType ();

				empty = new EmptyExpression (element_type);
			} else {
				hm = ProbeCollectionType (ec, expr.Type);
				if (hm == null){
					error1579 (expr.Type);
					return false;
				}

				array_type = expr.Type;
				element_type = hm.element_type;

				empty = new EmptyExpression (hm.element_type);
			}

			ec.StartFlowBranching (FlowBranchingType.LOOP_BLOCK, loc);
			ec.CurrentBranching.CreateSibling ();

 			//
			//
			// FIXME: maybe we can apply the same trick we do in the
			// array handling to avoid creating empty and conv in some cases.
			//
			// Although it is not as important in this case, as the type
			// will not likely be object (what the enumerator will return).
			//
			conv = Convert.ExplicitConversion (ec, empty, var_type, loc);
			if (conv == null)
				return false;

			if (variable.ResolveLValue (ec, empty) == null)
				return false;

			if (!statement.Resolve (ec))
				return false;

			FlowReturns returns = ec.EndFlowBranching ();

			return true;
		}
		
		//
		// Retrieves a `public bool MoveNext ()' method from the Type `t'
		//
		static MethodInfo FetchMethodMoveNext (Type t)
		{
			MemberList move_next_list;
			
			move_next_list = TypeContainer.FindMembers (
				t, MemberTypes.Method,
				BindingFlags.Public | BindingFlags.Instance,
				Type.FilterName, "MoveNext");
			if (move_next_list.Count == 0)
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
			MemberList move_next_list;
			
			move_next_list = TypeContainer.FindMembers (
				t, MemberTypes.Method,
				BindingFlags.Public | BindingFlags.Instance,
				Type.FilterName, "get_Current");
			if (move_next_list.Count == 0)
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
			public Type element_type;
			public Type enumerator_type;
			public bool is_disposable;

			public ForeachHelperMethods (EmitContext ec)
			{
				this.ec = ec;
				this.element_type = TypeManager.object_type;
				this.enumerator_type = TypeManager.ienumerator_type;
				this.is_disposable = true;
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

			if ((mi.ReturnType == TypeManager.ienumerator_type) && (declaring == TypeManager.string_type))
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

			if (mi.ReturnType == TypeManager.ienumerator_type ||
			    TypeManager.ienumerator_type.IsAssignableFrom (mi.ReturnType) ||
			    (!RootContext.StdLib && TypeManager.ImplementsInterface (mi.ReturnType, TypeManager.ienumerator_type))) {
				if (declaring != TypeManager.string_type) {
					hm.move_next = TypeManager.bool_movenext_void;
					hm.get_current = TypeManager.object_getcurrent_void;
					return true;
				}
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

			hm.element_type = hm.get_current.ReturnType;
			hm.enumerator_type = return_type;
			hm.is_disposable = !hm.enumerator_type.IsSealed ||
				TypeManager.ImplementsInterface (
					hm.enumerator_type, TypeManager.idisposable_type);

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
			MemberList mi;
			
			mi = TypeContainer.FindMembers (t, MemberTypes.Method,
							BindingFlags.Public | BindingFlags.NonPublic |
							BindingFlags.Instance,
							FilterEnumerator, hm);

			if (mi.Count == 0)
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
		bool EmitCollectionForeach (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			LocalBuilder enumerator, disposable;

			enumerator = ig.DeclareLocal (hm.enumerator_type);
			if (hm.is_disposable)
				disposable = ig.DeclareLocal (TypeManager.idisposable_type);
			else
				disposable = null;
			
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
			Label l;
			bool old_in_try = ec.InTry;

			if (hm.is_disposable) {
				l = ig.BeginExceptionBlock ();
				ec.InTry = true;
			}
			
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
			if (hm.is_disposable) {
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
			}

			ig.MarkLabel (ec.LoopEnd);
			return false;
		}

		//
		// FIXME: possible optimization.
		// We might be able to avoid creating `empty' if the type is the sam
		//
		bool EmitArrayForeach (EmitContext ec)
		{
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

				//
				// Load the value, we load the value using the underlying type,
				// then we use the variable.EmitAssign to load using the proper cast.
				//
				ArrayAccess.EmitLoadOpcode (ig, element_type);
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
		
		protected override bool DoEmit (EmitContext ec)
		{
			bool ret_val;
			
			ILGenerator ig = ec.ig;
			
			Label old_begin = ec.LoopBegin, old_end = ec.LoopEnd;
			bool old_inloop = ec.InLoop;
			int old_loop_begin_try_catch_level = ec.LoopBeginTryCatchLevel;
			ec.LoopBegin = ig.DefineLabel ();
			ec.LoopEnd = ig.DefineLabel ();
			ec.InLoop = true;
			ec.LoopBeginTryCatchLevel = ec.TryCatchLevel;
			
			if (hm != null)
				ret_val = EmitCollectionForeach (ec);
			else
				ret_val = EmitArrayForeach (ec);
			
			ec.LoopBegin = old_begin;
			ec.LoopEnd = old_end;
			ec.InLoop = old_inloop;
			ec.LoopBeginTryCatchLevel = old_loop_begin_try_catch_level;

			return ret_val;
		}
	}
}
