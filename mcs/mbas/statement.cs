//
// statement.cs: Statement representation for the IL tree.
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Martin Baulig (martin@gnome.org)
//	 Anirban Bhattacharjee (banirban@novell.com)
//
// (C) 2001, 2002 Ximian, Inc.
//

using System;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics;

namespace Mono.MonoBASIC {

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
			ec.Mark (loc);
			Report.Debug (8, "MARK", this, loc);
			return DoEmit (ec);
		}
		
		public static Expression ResolveBoolean (EmitContext ec, Expression e, Location loc)
		{
			e = e.Resolve (ec);
			if (e == null)
				return null;
			
			if (e.Type != TypeManager.bool_type){
				e = Expression.ConvertImplicit (ec, e, TypeManager.bool_type, Location.Null);
			}

			if (e == null){
				Report.Error (
					31, loc, "Can not convert the expression to a boolean");
			}

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

			expr = ResolveBoolean (ec, expr, loc);
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

	public enum DoOptions {
		WHILE,
		UNTIL,
		TEST_BEFORE,
		TEST_AFTER
	};

	public class Do : Statement {
		public Expression expr;
		public readonly Statement  EmbeddedStatement;
		//public DoOptions type;
		public DoOptions test;
		bool infinite, may_return;

		
		public Do (Statement statement, Expression boolExpr, DoOptions do_test, Location l)
		{
			expr = boolExpr;
			EmbeddedStatement = statement;
//			type = do_type;
			test = do_test;
			loc = l;
		}

		public override bool Resolve (EmitContext ec)
		{
			bool ok = true;

			ec.StartFlowBranching (FlowBranchingType.LOOP_BLOCK, loc);

			if (!EmbeddedStatement.Resolve (ec))
				ok = false;

			expr = ResolveBoolean (ec, expr, loc);
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

			if (test == DoOptions.TEST_AFTER) {
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
			}
			else
			{
				ig.MarkLabel (loop);
				ig.MarkLabel (ec.LoopBegin);

				//
				// Dead code elimination
				//
				if (expr is BoolConstant){
					bool res = ((BoolConstant) expr).Value;

					if (res)
						ec.ig.Emit (OpCodes.Br, ec.LoopEnd);
				} else
					EmitBoolExpression (ec, expr, ec.LoopEnd, true);

				EmbeddedStatement.Emit (ec);
				ec.ig.Emit (OpCodes.Br, loop);
				ig.MarkLabel (ec.LoopEnd);
			}
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

			expr = ResolveBoolean (ec, expr, loc);
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
				Test = ResolveBoolean (ec, Test, loc);
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

			if (Increment != null){
				if (!Increment.Resolve (ec))
					ok = false;
			}

			ec.StartFlowBranching (FlowBranchingType.LOOP_BLOCK, loc);
			if (!infinite)
				ec.CurrentBranching.CreateSibling ();

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
			if (Test != null)
				EmitBoolExpression (ec, Test, loop, true);
			else
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
		public Expression expr;
		
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
				Report.Error (157,loc,"Control can not leave the body of the finally block");
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
						      TypeManager.MonoBASIC_Name (ec.ReturnType) + "' is " +
						      "expected for the return statement");
					return true;
				}

				if (Expr.Type != ec.ReturnType)
					Expr = Expression.ConvertImplicitRequired (
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
			} else
				ec.ig.Emit (OpCodes.Ret);

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
			ec.CurrentBranching.CurrentUsageVector.Breaks = FlowReturns.UNREACHABLE;
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
			}

			label = sl.ILLabelCode;

			ec.CurrentBranching.CurrentUsageVector.Breaks = FlowReturns.UNREACHABLE;
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
					expr.Error118 ("value, variable, property or indexer access ");
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
	
	public enum ExitType {
		DO, 
		FOR, 
		WHILE,
		SELECT,
		SUB,
		FUNCTION,
		PROPERTY,
		TRY			
	};
	
	public class Exit : Statement {
		public readonly ExitType type;
		public Exit (ExitType t, Location l)
		{
			loc = l;
			type = t;
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

			if (type != ExitType.SUB && type != ExitType.FUNCTION && 
				type != ExitType.PROPERTY && type != ExitType.TRY) {
				if (ec.InLoop == false && ec.Switch == null){
					Report.Error (139, loc, "No enclosing loop or switch to exit from");
					return false;
				}

				if (ec.InTry || ec.InCatch)
					ig.Emit (OpCodes.Leave, ec.LoopEnd);
				else
					ig.Emit (OpCodes.Br, ec.LoopEnd);
			} else {			
				if (ec.InFinally){
					Report.Error (157,loc,"Control can not leave the body of the finally block");
					return false;
				}
			
				if (ec.InTry || ec.InCatch) {
					if (!ec.HasReturnLabel) {
						ec.ReturnLabel = ec.ig.DefineLabel ();
						ec.HasReturnLabel = true;
					}
					ec.ig.Emit (OpCodes.Leave, ec.ReturnLabel);
				} else
					ec.ig.Emit (OpCodes.Ret);

				return true; 
			}
			
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

				for (int i = 0; i < System.Math.Min (vector.Count, value.Count); i++)
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
		public ArrayList Siblings;

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
		InternalParameters param_info;
		int[] param_map;
		MyStructInfo[] struct_params;
		int num_params;
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

			// 
			// State of parameter `number'.
			//
			public bool this [int number]
			{
				get {
					if (number == -1)
						return true;
					else if (number == 0)
						throw new ArgumentException ();

					return parameters [number - 1];
				}

				set {
					if (number == -1)
						return;
					else if (number == 0)
						throw new ArgumentException ();

					parameters [number - 1] = value;
				}
			}

			//
			// State of the local variable `vi'.
			// If the local variable is a struct, use a non-zero `field_idx'
			// to check an individual field in it.
			//
			public bool this [VariableInfo vi, int field_idx]
			{
				get {
					if (vi.Number == -1)
						return true;
					else if (vi.Number == 0)
						throw new ArgumentException ();

					return locals [vi.Number + field_idx - 1];
				}

				set {
					if (vi.Number == -1)
						return;
					else if (vi.Number == 0)
						throw new ArgumentException ();

					locals [vi.Number + field_idx - 1] = value;
				}
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
			public FlowReturns MergeChildren (FlowBranching branching, ICollection children)
			{
				MyBitVector new_locals = null;
				MyBitVector new_params = null;

				FlowReturns new_returns = FlowReturns.NEVER;
				FlowReturns new_breaks = FlowReturns.NEVER;
				bool new_returns_set = false, new_breaks_set = false;

				Report.Debug (2, "MERGING CHILDREN", branching, branching.Type,
					      this, children.Count);

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
						} else if (children.Count == 1) {
							new_locals = locals.Clone ();
							new_locals.Or (child.locals);
						}

						// An `out' parameter must be assigned in all branches which do
						// not always throw an exception.
						if (parameters != null) {
							if (child.Breaks != FlowReturns.EXCEPTION) {
								if (new_params != null)
									new_params.And (child.parameters);
								else {
									new_params = parameters.Clone ();
									new_params.Or (child.parameters);
								}
							} else if (children.Count == 1) {
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
				    (children.Count == 1)) {
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
			this.Siblings = new ArrayList ();
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
		public FlowBranching (Block block, InternalParameters ip, Location loc)
			: this (FlowBranchingType.BLOCK, loc)
		{
			Block = block;
			Parent = null;

			int count = (ip != null) ? ip.Count : 0;

			param_info = ip;
			param_map = new int [count];
			struct_params = new MyStructInfo [count];
			num_params = 0;

			for (int i = 0; i < count; i++) {
				Parameter.Modifier mod = param_info.ParameterModifier (i);

				if ((mod & Parameter.Modifier.OUT) == 0)
					continue;

				param_map [i] = ++num_params;

				Type param_type = param_info.ParameterType (i);

				struct_params [i] = MyStructInfo.GetStructInfo (param_type);
				if (struct_params [i] != null)
					num_params += struct_params [i].Count;
			}

			Siblings = new ArrayList ();
			Siblings.Add (new UsageVector (null, num_params, block.CountVariables));
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

			if (parent != null) {
				param_info = parent.param_info;
				param_map = parent.param_map;
				struct_params = parent.struct_params;
				num_params = parent.num_params;
			}

			UsageVector vector;
			if (Block != null)
				vector = new UsageVector (parent.CurrentUsageVector, num_params,
							  Block.CountVariables);
			else
				vector = new UsageVector (Parent.CurrentUsageVector);

			Siblings.Add (vector);

			switch (Type) {
			case FlowBranchingType.EXCEPTION:
				finally_vectors = new ArrayList ();
				break;

			default:
				break;
			}
		}

		// <summary>
		//   Returns the branching's current usage vector.
		// </summary>
		public UsageVector CurrentUsageVector
		{
			get {
				return (UsageVector) Siblings [Siblings.Count - 1];
			}
		}

		// <summary>
		//   Creates a sibling of the current usage vector.
		// </summary>
		public void CreateSibling ()
		{
			Siblings.Add (new UsageVector (Parent.CurrentUsageVector));

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

			for (int i = 0; i < param_map.Length; i++) {
				int index = param_map [i];

				if (index == 0)
					continue;

				if (parameters [index - 1])
					continue;

				// If it's a struct, we must ensure that all its fields have
				// been assigned.  If the struct has any non-public fields, this
				// can only be done by assigning the whole struct.

				MyStructInfo struct_info = struct_params [index - 1];
				if ((struct_info == null) || struct_info.HasNonPublicFields) {
					Report.Error (
						177, loc, "The out parameter `" +
						param_info.ParameterName (i) + "' must be " +
						"assigned before control leave the current method.");
					param_map [i] = 0;
					continue;
				}


				for (int j = 0; j < struct_info.Count; j++) {
					if (!parameters [index + j]) {
						Report.Error (
							177, loc, "The out parameter `" +
							param_info.ParameterName (i) + "' must be " +
							"assigned before control leave the current method.");
						param_map [i] = 0;
						break;
					}
				}
			}
		}

		// <summary>
		//   Merge a child branching.
		// </summary>
		public FlowReturns MergeChild (FlowBranching child)
		{
			FlowReturns returns = CurrentUsageVector.MergeChildren (child, child.Siblings);

			if (child.Type != FlowBranchingType.LOOP_BLOCK)
				MayLeaveLoop |= child.MayLeaveLoop;
			else
				MayLeaveLoop = false;

			return returns;
 		}
 
		// <summary>
		//   Does the toplevel merging.
		// </summary>
		public FlowReturns MergeTopBlock ()
		{
			if ((Type != FlowBranchingType.BLOCK) || (Block == null))
				throw new NotSupportedException ();

			UsageVector vector = new UsageVector (null, num_params, Block.CountVariables);

			Report.Debug (1, "MERGING TOP BLOCK", Location, vector);

			vector.MergeChildren (this, Siblings);

			Siblings.Clear ();
			Siblings.Add (vector);

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

		public bool IsVariableAssigned (VariableInfo vi)
		{
			if (CurrentUsageVector.AlwaysBreaks)
				return true;
			else
				return CurrentUsageVector [vi, 0];
		}

		public bool IsVariableAssigned (VariableInfo vi, int field_idx)
		{
			if (CurrentUsageVector.AlwaysBreaks)
				return true;
			else
				return CurrentUsageVector [vi, field_idx];
		}

		public void SetVariableAssigned (VariableInfo vi)
		{
			if (CurrentUsageVector.AlwaysBreaks)
				return;

			CurrentUsageVector [vi, 0] = true;
		}

		public void SetVariableAssigned (VariableInfo vi, int field_idx)
		{
			if (CurrentUsageVector.AlwaysBreaks)
				return;

			CurrentUsageVector [vi, field_idx] = true;
		}

		public bool IsParameterAssigned (int number)
		{
			int index = param_map [number];

			if (index == 0)
				return true;

			if (CurrentUsageVector [index])
				return true;

			// Parameter is not assigned, so check whether it's a struct.
			// If it's either not a struct or a struct which non-public
			// fields, return false.
			MyStructInfo struct_info = struct_params [number];
			if ((struct_info == null) || struct_info.HasNonPublicFields)
				return false;

			// Ok, so each field must be assigned.
			for (int i = 0; i < struct_info.Count; i++)
				if (!CurrentUsageVector [index + i])
					return false;

			return true;
		}

		public bool IsParameterAssigned (int number, string field_name)
		{
			int index = param_map [number];

			if (index == 0)
				return true;

			MyStructInfo info = (MyStructInfo) struct_params [number];
			if (info == null)
				return true;

			int field_idx = info [field_name];

			return CurrentUsageVector [index + field_idx];
		}

		public void SetParameterAssigned (int number)
		{
			if (param_map [number] == 0)
				return;

			if (!CurrentUsageVector.AlwaysBreaks)
				CurrentUsageVector [param_map [number]] = true;
		}

		public void SetParameterAssigned (int number, string field_name)
		{
			int index = param_map [number];

			if (index == 0)
				return;

			MyStructInfo info = (MyStructInfo) struct_params [number];
			if (info == null)
				return;

			int field_idx = info [field_name];

			if (!CurrentUsageVector.AlwaysBreaks)
				CurrentUsageVector [index + field_idx] = true;
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
				reachable = !CurrentUsageVector.AlwaysBreaks &&
					!CurrentUsageVector.AlwaysReturns;
				break;
			}

			Report.Debug (1, "REACHABLE", Type, CurrentUsageVector.Returns,
				      CurrentUsageVector.Breaks, CurrentUsageVector, reachable);

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
			sb.Append (Siblings.Count);
			sb.Append (" - ");
			sb.Append (CurrentUsageVector);
			sb.Append (")");
			return sb.ToString ();
		}
	}

	public class MyStructInfo {
		public readonly Type Type;
		public readonly FieldInfo[] Fields;
		public readonly FieldInfo[] NonPublicFields;
		public readonly int Count;
		public readonly int CountNonPublic;
		public readonly bool HasNonPublicFields;

		private static Hashtable field_type_hash = new Hashtable ();
		private Hashtable field_hash;

		// Private constructor.  To save memory usage, we only need to create one instance
		// of this class per struct type.
		private MyStructInfo (Type type)
		{
			this.Type = type;

			if (type is TypeBuilder) {
				TypeContainer tc = TypeManager.LookupTypeContainer (type);

				ArrayList fields = tc.Fields;
				if (fields != null) {
					foreach (Field field in fields) {
						if ((field.ModFlags & Modifiers.STATIC) != 0)
							continue;
						if ((field.ModFlags & Modifiers.PUBLIC) != 0)
							++Count;
						else
							++CountNonPublic;
					}
				}

				Fields = new FieldInfo [Count];
				NonPublicFields = new FieldInfo [CountNonPublic];

				Count = CountNonPublic = 0;
				if (fields != null) {
					foreach (Field field in fields) {
						if ((field.ModFlags & Modifiers.STATIC) != 0)
							continue;
						if ((field.ModFlags & Modifiers.PUBLIC) != 0)
							Fields [Count++] = field.FieldBuilder;
						else
							NonPublicFields [CountNonPublic++] =
								field.FieldBuilder;
					}
				}
				
			} else {
				Fields = type.GetFields (BindingFlags.Instance|BindingFlags.Public);
				Count = Fields.Length;

				NonPublicFields = type.GetFields (BindingFlags.Instance|BindingFlags.NonPublic);
				CountNonPublic = NonPublicFields.Length;
			}

			Count += NonPublicFields.Length;

			int number = 0;
			field_hash = new Hashtable ();
			foreach (FieldInfo field in Fields)
				field_hash.Add (field.Name, ++number);

			if (NonPublicFields.Length != 0)
				HasNonPublicFields = true;

			foreach (FieldInfo field in NonPublicFields)
				field_hash.Add (field.Name, ++number);
		}

		public int this [string name] {
			get {
				if (field_hash.Contains (name))
					return (int) field_hash [name];
				else
					return 0;
			}
		}

		public FieldInfo this [int index] {
			get {
				if (index >= Fields.Length)
					return NonPublicFields [index - Fields.Length];
				else
					return Fields [index];
			}
		}		       

		public static MyStructInfo GetStructInfo (Type type)
		{
			if (!TypeManager.IsValueType (type) || TypeManager.IsEnumType (type))
				return null;

			if (!(type is TypeBuilder) && TypeManager.IsBuiltinType (type))
				return null;

			MyStructInfo info = (MyStructInfo) field_type_hash [type];
			if (info != null)
				return info;

			info = new MyStructInfo (type);
			field_type_hash.Add (type, info);
			return info;
		}

		public static MyStructInfo GetStructInfo (TypeContainer tc)
		{
			MyStructInfo info = (MyStructInfo) field_type_hash [tc.TypeBuilder];
			if (info != null)
				return info;

			info = new MyStructInfo (tc.TypeBuilder);
			field_type_hash.Add (tc.TypeBuilder, info);
			return info;
		}
	}
	
	public class VariableInfo : IVariable {
		public Expression Type;
		public LocalBuilder LocalBuilder;
		public Type VariableType;
		public readonly string Name;
		public readonly Location Location;
		public readonly int Block;

		public int Number;
		
		public bool Used;
		public bool Assigned;
		public bool ReadOnly;
		
		public VariableInfo (Expression type, string name, int block, Location l)
		{
			Type = type;
			Name = name;
			Block = block;
			LocalBuilder = null;
			Location = l;
		}

		public VariableInfo (TypeContainer tc, int block, Location l)
		{
			VariableType = tc.TypeBuilder;
			struct_info = MyStructInfo.GetStructInfo (tc);
			Block = block;
			LocalBuilder = null;
			Location = l;
		}

		MyStructInfo struct_info;
		public MyStructInfo StructInfo {
			get {
				return struct_info;
			}
		}

		public bool IsAssigned (EmitContext ec, Location loc)
		{/* FIXME: we shouldn't just skip this!!!
			if (!ec.DoFlowAnalysis || ec.CurrentBranching.IsVariableAssigned (this))
				return true;

			MyStructInfo struct_info = StructInfo;
			if ((struct_info == null) || (struct_info.HasNonPublicFields && (Name != null))) {
				Report.Error (165, loc, "Use of unassigned local variable `" + Name + "'");
				ec.CurrentBranching.SetVariableAssigned (this);
				return false;
			}

			int count = struct_info.Count;

			for (int i = 0; i < count; i++) {
				if (!ec.CurrentBranching.IsVariableAssigned (this, i+1)) {
					if (Name != null) {
						Report.Error (165, loc,
							      "Use of unassigned local variable `" +
							      Name + "'");
						ec.CurrentBranching.SetVariableAssigned (this);
						return false;
					}

					FieldInfo field = struct_info [i];
					Report.Error (171, loc,
						      "Field `" + TypeManager.MonoBASIC_Name (VariableType) +
						      "." + field.Name + "' must be fully initialized " +
						      "before control leaves the constructor");
					return false;
				}
			}
*/
			return true;
		}

		public bool IsFieldAssigned (EmitContext ec, string name, Location loc)
		{
			if (!ec.DoFlowAnalysis || ec.CurrentBranching.IsVariableAssigned (this) ||
			    (struct_info == null))
				return true;

			int field_idx = StructInfo [name];
			if (field_idx == 0)
				return true;

			if (!ec.CurrentBranching.IsVariableAssigned (this, field_idx)) {
				Report.Error (170, loc,
					      "Use of possibly unassigned field `" + name + "'");
				ec.CurrentBranching.SetVariableAssigned (this, field_idx);
				return false;
			}

			return true;
		}

		public void SetAssigned (EmitContext ec)
		{
			if (ec.DoFlowAnalysis)
				ec.CurrentBranching.SetVariableAssigned (this);
		}

		public void SetFieldAssigned (EmitContext ec, string name)
		{
			if (ec.DoFlowAnalysis && (struct_info != null))
				ec.CurrentBranching.SetVariableAssigned (this, StructInfo [name]);
		}

		public bool Resolve (DeclSpace decl)
		{
			if (struct_info != null)
				return true;

			if (VariableType == null)
				VariableType = decl.ResolveType (Type, false, Location);

			if (VariableType == null)
				return false;

			struct_info = MyStructInfo.GetStructInfo (VariableType);

			return true;
		}

		public void MakePinned ()
		{
			TypeManager.MakePinned (LocalBuilder);
		}

		public override string ToString ()
		{
			return "VariableInfo (" + Number + "," + Type + "," + Location + ")";
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
		public ArrayList statements;

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
		CaseInsensitiveHashtable labels;

		//
		// Keeps track of (name, type) pairs
		//
		CaseInsensitiveHashtable variables;

		//
		// Keeps track of constants
		CaseInsensitiveHashtable constants;

		//
		// Maps variable names to ILGenerator.LocalBuilders
		//
		CaseInsensitiveHashtable local_builders;

		bool used = false;

		static int id;

		int this_id;
		
		public Block (Block parent)
			: this (parent, false, Location.Null, Location.Null)
		{ }

		public Block (Block parent, bool implicit_block)
			: this (parent, implicit_block, Location.Null, Location.Null)
		{ }

		public Block (Block parent, bool implicit_block, Parameters parameters)
			: this (parent, implicit_block, parameters, Location.Null, Location.Null)
		{ }

		public Block (Block parent, Location start, Location end)
			: this (parent, false, start, end)
		{ }

		public Block (Block parent, Parameters parameters, Location start, Location end)
			: this (parent, false, parameters, start, end)
		{ }

		public Block (Block parent, bool implicit_block, Location start, Location end)
			: this (parent, implicit_block, Parameters.EmptyReadOnlyParameters,
				start, end)
		{ }

		public Block (Block parent, bool implicit_block, Parameters parameters,
			      Location start, Location end)
		{
			if (parent != null)
				parent.AddChild (this);
			
			this.Parent = parent;
			this.Implicit = implicit_block;
			this.parameters = parameters;
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
				labels = new CaseInsensitiveHashtable ();
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

		VariableInfo this_variable = null;

		// <summary>
		//   Returns the "this" instance variable of this block.
		//   See AddThisVariable() for more information.
		// </summary>
		public VariableInfo ThisVariable {
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
				child_variable_names = new CaseInsensitiveHashtable ();

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

			foreach (Block child in block.children) {
				if (child.Variables != null) {
					foreach (string name in child.Variables.Keys)
						AddChildVariableName (name);
				}
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
		public VariableInfo AddThisVariable (TypeContainer tc, Location l)
		{
			if (this_variable != null)
				return this_variable;

			this_variable = new VariableInfo (tc, ID, l);

			if (variables == null)
				variables = new CaseInsensitiveHashtable ();
			variables.Add ("this", this_variable);

			return this_variable;
		}

		public VariableInfo AddVariable (Expression type, string name, Parameters pars, Location l)
		{
			if (variables == null)
				variables = new CaseInsensitiveHashtable ();

			VariableInfo vi = GetVariableInfo (name);
			if (vi != null) {
				if (vi.Block != ID)
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
			
			vi = new VariableInfo (type, name, ID, l);

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
				constants = new CaseInsensitiveHashtable ();

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

		bool variables_initialized = false;
		int count_variables = 0, first_variable = 0;

		void UpdateVariableInfo (EmitContext ec)
		{
			DeclSpace ds = ec.DeclSpace;

			first_variable = 0;

			if (Parent != null)
				first_variable += Parent.CountVariables;

			count_variables = first_variable;
			if (variables != null) {
				foreach (VariableInfo vi in variables.Values) {
					if (!vi.Resolve (ds)) {
						vi.Number = -1;
						continue;
					}

					vi.Number = ++count_variables;

					if (vi.StructInfo != null)
						count_variables += vi.StructInfo.Count;
				}
			}

			variables_initialized = true;
		}

		//
		// <returns>
		//   The number of local variables in this block
		// </returns>
		public int CountVariables
		{
			get {
				if (!variables_initialized)
					throw new Exception ();

				return count_variables;
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
		public void EmitMeta (EmitContext ec, Block toplevel)
		{
			DeclSpace ds = ec.DeclSpace;
			ILGenerator ig = ec.ig;

			if (!variables_initialized)
				UpdateVariableInfo (ec);

			//
			// Process this block variables
			//
			if (variables != null){
				local_builders = new CaseInsensitiveHashtable ();
				
				foreach (DictionaryEntry de in variables){
					string name = (string) de.Key;
					VariableInfo vi = (VariableInfo) de.Value;

					if (vi.VariableType == null)
						continue;

					vi.LocalBuilder = ig.DeclareLocal (vi.VariableType);

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

		bool has_ret = false;

		public override bool Resolve (EmitContext ec)
		{
			Block prev_block = ec.CurrentBlock;
			bool ok = true;

			ec.CurrentBlock = this;
			ec.StartFlowBranching (this);

			Report.Debug (1, "RESOLVE BLOCK", StartLocation, ec.CurrentBranching);

			if (!variables_initialized)
				UpdateVariableInfo (ec);

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
			    !this_variable.IsAssigned (ec, loc))
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
			Block prev_block = ec.CurrentBlock;

			ec.CurrentBlock = this;

			ec.Mark (StartLocation);
			foreach (Statement s in statements)
				s.Emit (ec);
				
			ec.Mark (EndLocation); 
			
			ec.CurrentBlock = prev_block;
			return has_ret;
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
				
				e = Expression.ImplicitUserConversion (ec, Expr, tt, loc);
				if (e == null)
					continue;

				if (converted != null){
					Report.Error (-12, loc, "More than one conversion to an integral " +
						      " type exists for type `" +
						      TypeManager.MonoBASIC_Name (Expr.Type)+"'");
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
			Elements = new CaseInsensitiveHashtable ();
				
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
				if (label_count != 1)
					ig.Emit (OpCodes.Br, next_test);
				
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
					      TypeManager.MonoBASIC_Name (type) + "'");
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
			public VariableInfo vi;
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
			expr_type = ec.DeclSpace.ResolveType (type, false, loc);
			if (expr_type == null)
				return false;

			data = new FixedData [declarators.Count];

			int i = 0;
			foreach (Pair p in declarators){
				VariableInfo vi = (VariableInfo) p.First;
				Expression e = (Expression) p.Second;

				vi.Number = -1;

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
					
					Expression converted = Expression.ConvertImplicitRequired (
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

			for (int i = 0; i < data.Length; i++) {
				VariableInfo vi = data [i].vi;

				//
				// Case 1: & object.
				//
				if (data [i].is_object) {
					//
					// Store pointer in pinned location
					//
					data [i].expr.Emit (ec);
					ig.Emit (OpCodes.Stloc, vi.LocalBuilder);

					is_ret = statement.Emit (ec);

					// Clear the pinned variable.
					ig.Emit (OpCodes.Ldc_I4_0);
					ig.Emit (OpCodes.Conv_U);
					ig.Emit (OpCodes.Stloc, vi.LocalBuilder);

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
				if (data [i].expr.Type == TypeManager.string_type){
					LocalBuilder pinned_string = ig.DeclareLocal (TypeManager.string_type);
					TypeManager.MakePinned (pinned_string);
					
					data [i].expr.Emit (ec);
					ig.Emit (OpCodes.Stloc, pinned_string);

					Expression sptr = new StringPtr (pinned_string, loc);
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
					VariableInfo vi = c.Block.GetVariableInfo (c.Name);
					if (vi == null)
						throw new Exception ();

					vi.Number = -1;
				}

				bool old_in_catch = ec.InCatch;
				ec.InCatch = true;

				if (!c.Resolve (ec))
					ok = false;

				ec.InCatch = old_in_catch;

				FlowBranching.UsageVector current = ec.CurrentBranching.CurrentUsageVector;

				if (!current.AlwaysReturns && !current.AlwaysBreaks)
					vector.AndLocals (current);
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
			}

			Report.Debug (1, "END OF GENERAL CATCH BLOCKS", ec.CurrentBranching);

			if (Fini != null) {
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
				VariableInfo vi;
				
				ig.BeginCatchBlock (c.CatchType);

				if (c.Name != null){
					vi = c.Block.GetVariableInfo (c.Name);
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

			if (!returns || ec.InTry || ec.InCatch)
				return returns;

			// Unfortunately, System.Reflection.Emit automatically emits a leave
			// to the end of the finally block.  This is a problem if `returns'
			// is true since we may jump to a point after the end of the method.
			// As a workaround, emit an explicit ret here.

			if (ec.ReturnType != null)
				ec.ig.Emit (OpCodes.Ldloc, ec.TemporaryReturn ());
			ec.ig.Emit (OpCodes.Ret);

			return true;
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
					
					converted_vars [i] = Expression.ConvertImplicitRequired (
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
				conv = Expression.ConvertImplicitRequired (
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
			if (type != null) {
				this.type = type;
			}
			else
			{
				VariableInfo vi = var.VariableInfo;
				this.type = vi.Type;
			}
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
			conv = Expression.ConvertExplicit (ec, empty, var_type, false, loc);
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

			hm.element_type = hm.get_current.ReturnType;
			hm.enumerator_type = return_type;
			hm.is_disposable = TypeManager.ImplementsInterface (
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
	
	/// <summary>
	///   AddHandler statement
	/// </summary>
	public class AddHandler : Statement {
		Expression EvtId;
		Expression EvtHandler;

		//
		// keeps track whether EvtId is already resolved
		//
		bool resolved;

		public AddHandler (Expression evt_id, Expression evt_handler, Location l)
		{
			EvtId = evt_id;
			EvtHandler = evt_handler;
			loc = l;
			resolved = false;
			//Console.WriteLine ("Adding handler '" + evt_handler + "' for Event '" + evt_id +"'");
		}

		public override bool Resolve (EmitContext ec)
		{
			//
			// if EvetId is of EventExpr type that means
			// this is already resolved 
			//
			if (EvtId is EventExpr)	{
				resolved = true;
				return true;
			}

			EvtId = EvtId.Resolve(ec);
			EvtHandler = EvtHandler.Resolve(ec,ResolveFlags.MethodGroup);
			if (EvtId == null || (!(EvtId is EventExpr))) {
				Report.Error (30676, "Need an event designator.");
				return false;
			}

			if (EvtHandler == null) 
			{
				Report.Error (999, "'AddHandler' statement needs an event handler.");
				return false;
			}

			return true;
		}

		protected override bool DoEmit (EmitContext ec)
		{
			//
			// Already resolved and emitted don't do anything
			//
			if (resolved)
				return true;

			Expression e, d;
			ArrayList args = new ArrayList();
			Argument arg = new Argument (EvtHandler, Argument.AType.Expression);
			args.Add (arg);
			
			

			// The even type was already resolved to a delegate, so
			// we must un-resolve its name to generate a type expression
			string ts = (EvtId.Type.ToString()).Replace ('+','.');
			Expression dtype = Mono.MonoBASIC.Parser.DecomposeQI (ts, Location.Null);

			// which we can use to declare a new event handler
			// of the same type
			d = new New (dtype, args, Location.Null);
			d = d.Resolve(ec);
			e = new CompoundAssign(Binary.Operator.Addition, EvtId, d, Location.Null);

			// we resolve it all and emit the code
			e = e.Resolve(ec);
			if (e != null) 
			{
				e.Emit(ec);
				return true;
			}

			return false;
		}
	}

	/// <summary>
	///   RemoveHandler statement
	/// </summary>
	public class RemoveHandler : Statement 
	{
		Expression EvtId;
		Expression EvtHandler;

		public RemoveHandler (Expression evt_id, Expression evt_handler, Location l)
		{
			EvtId = evt_id;
			EvtHandler = evt_handler;
			loc = l;
		}

		public override bool Resolve (EmitContext ec)
		{
			EvtId = EvtId.Resolve(ec);
			EvtHandler = EvtHandler.Resolve(ec,ResolveFlags.MethodGroup);
			if (EvtId == null || (!(EvtId is EventExpr))) 
			{
				Report.Error (30676, "Need an event designator.");
				return false;
			}

			if (EvtHandler == null) 
			{
				Report.Error (999, "'AddHandler' statement needs an event handler.");
				return false;
			}
			return true;
		}

		protected override bool DoEmit (EmitContext ec)
		{
			Expression e, d;
			ArrayList args = new ArrayList();
			Argument arg = new Argument (EvtHandler, Argument.AType.Expression);
			args.Add (arg);
			
			// The even type was already resolved to a delegate, so
			// we must un-resolve its name to generate a type expression
			string ts = (EvtId.Type.ToString()).Replace ('+','.');
			Expression dtype = Mono.MonoBASIC.Parser.DecomposeQI (ts, Location.Null);

			// which we can use to declare a new event handler
			// of the same type
			d = new New (dtype, args, Location.Null);
			d = d.Resolve(ec);
			// detach the event
			e = new CompoundAssign(Binary.Operator.Subtraction, EvtId, d, Location.Null);

			// we resolve it all and emit the code
			e = e.Resolve(ec);
			if (e != null) 
			{
				e.Emit(ec);
				return true;
			}

			return false;
		}
	}

	public class RedimClause {
		public Expression Expr;
		public ArrayList NewIndexes;
		
		public RedimClause (Expression e, ArrayList args)
		{
			Expr = e;
			NewIndexes = args;
		}
	}

	public class ReDim : Statement {
		ArrayList RedimTargets;
		Type BaseType;
		bool Preserve;

		private StatementExpression ReDimExpr;

		public ReDim (ArrayList targets, bool opt_preserve, Location l)
		{
			loc = l;
			RedimTargets = targets;
			Preserve = opt_preserve;
		}

		public override bool Resolve (EmitContext ec)
		{
			Expression RedimTarget;
			ArrayList NewIndexes;

			foreach (RedimClause rc in RedimTargets) {
				RedimTarget = rc.Expr;
				NewIndexes = rc.NewIndexes;

				RedimTarget = RedimTarget.Resolve (ec);
				if (!RedimTarget.Type.IsArray)
					Report.Error (49, "'ReDim' statement requires an array");

				ArrayList args = new ArrayList();
				foreach (Argument a in NewIndexes) {
					if (a.Resolve(ec, loc))
						args.Add (a.Expr);
				}

				for (int x = 0; x < args.Count; x++) {
					args[x] = new Binary (Binary.Operator.Addition,
								(Expression) args[x], new IntLiteral (1), Location.Null);	
				}

				NewIndexes = args;
				if (RedimTarget.Type.GetArrayRank() != args.Count)
					Report.Error (415, "'ReDim' cannot change the number of dimensions of an array.");

				BaseType = RedimTarget.Type.GetElementType();
				Expression BaseTypeExpr = MonoBASIC.Parser.DecomposeQI(BaseType.FullName.ToString(), Location.Null);
				ArrayCreation acExpr = new ArrayCreation (BaseTypeExpr, NewIndexes, "", null, Location.Null); 	
				// TODO: we are in a foreach we probably can't reuse ReDimExpr, must turn it into an array(list)
				if (Preserve)
				{
					// TODO: Generate call to copying code, which has to make lots of verifications
					//PreserveExpr = (ExpressionStatement) new Preserve(RedimTarget, acExpr, loc);
					//ReDimExpr = (StatementExpression) new StatementExpression ((ExpressionStatement) new Assign (RedimTarget, PreserveExpr, loc), loc);
					ReDimExpr = (StatementExpression) new StatementExpression ((ExpressionStatement) new Assign (RedimTarget, acExpr, loc), loc);
				}
				else
					ReDimExpr = (StatementExpression) new StatementExpression ((ExpressionStatement) new Assign (RedimTarget, acExpr, loc), loc);
				ReDimExpr.Resolve(ec);
			}
			return true;
		}
				
		protected override bool DoEmit (EmitContext ec)
		{
			ReDimExpr.Emit(ec);
			return false;
		}		
		
	}
	
	public class Erase : Statement {
		Expression EraseTarget;
		
		private StatementExpression EraseExpr;
		
		public Erase (Expression expr, Location l)
		{
			loc = l;
			EraseTarget = expr;
		}
		
		public override bool Resolve (EmitContext ec)
		{
			EraseTarget = EraseTarget.Resolve (ec);
			if (!EraseTarget.Type.IsArray) 
				Report.Error (49, "'Erase' statement requires an array");

			EraseExpr = (StatementExpression) new StatementExpression ((ExpressionStatement) new Assign (EraseTarget, NullLiteral.Null, loc), loc);
			EraseExpr.Resolve(ec);
			
			return true;
		}
				
		protected override bool DoEmit (EmitContext ec)
		{
			EraseExpr.Emit(ec);
			return false;
		}		
		
	}
	
	
}
