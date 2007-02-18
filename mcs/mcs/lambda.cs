//
// lambda.cs: support for lambda expressions
//
// Authors: Miguel de Icaza (miguel@gnu.org)
//
// Licensed under the terms of the GNU GPL
//
// (C) 2007 Novell, Inc
//

using System;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;

namespace Mono.CSharp {
	public class LambdaExpression : AnonymousMethodExpression {
		bool explicit_parameters;

		//
		// The parameter list can either be:
		//    null: no parameters
		//    arraylist of Parameter (explicitly typed parameters)
		//    arraylist of strings (implicitly typed parameters)
		//
		public LambdaExpression (AnonymousMethodExpression parent,
					 GenericMethod generic, TypeContainer host,
					 Parameters parameters, Block container,
					 Location loc)
			: base (parent, generic, host, parameters, container, loc)
		{
			explicit_parameters = (parameters != null && parameters.Count > 0 && parameters [0].TypeName != null);
			if (parameters == null)
				Parameters = new Parameters (new Parameter [0]);
		}

		public override Expression DoResolve (EmitContext ec)
		{
			eclass = ExprClass.Value;
			type = TypeManager.anonymous_method_type;

			if (explicit_parameters){
				if (!Parameters.Resolve (ec))
					return null;
			}
			
			// We will resolve parameters later, we do not
			// have information at this point.
			
			return this;
		}

		public override void Emit (EmitContext ec)
		{
			base.Emit (ec);
		}

		public override bool ImplicitStandardConversionExists (Type delegate_type)
		{
			EmitContext ec = EmitContext.TempEc;

			bool result;

			try {
				Report.DisableErrors ();
				result = DoCompatibleTest (ec, delegate_type, true) != null;
				if (result)
					Console.WriteLine ("INFO: Lambda.Compatible Passed for {0}", delegate_type);
				else
					Console.WriteLine ("INFO: Lambda.Compatible Failed for {0}", delegate_type);
			} finally {
				Report.EnableErrors ();
			}
			
			// Ignore the result
			anonymous = null;

			return result;
		}
		
		//
		// Returns true if this anonymous method can be implicitly
		// converted to the delegate type `delegate_type'
		//
		public override Expression Compatible (EmitContext ec, Type delegate_type)
		{
			return DoCompatibleTest (ec, delegate_type, false);
		}

		Expression DoCompatibleTest (EmitContext ec, Type delegate_type, bool clone)
		{
			if (anonymous != null)
				return anonymous.AnonymousDelegate;

			if (CompatibleChecks (ec, delegate_type) == null)
				return null;

			MethodGroupExpr invoke_mg = Delegate.GetInvokeMethod (
				ec.ContainerType, delegate_type, loc);
			MethodInfo invoke_mb = (MethodInfo) invoke_mg.Methods [0];
			ParameterData invoke_pd = TypeManager.GetParameterData (invoke_mb);

			//
			// The lambda expression is compatible with the delegate type,
			// provided that:
			//

			//
			// D and L have the same number of arguments.
			if (Parameters.Count != invoke_pd.Count)
				return null;

			if (explicit_parameters){
				//
				// If L has an explicitly typed parameter list, each parameter
				// in D has the same type and modifiers as the corresponding
				// parameter in L.
				//
				if (!VerifyExplicitParameterCompatibility (delegate_type, invoke_pd))
					return null;
			} else {
				//
				// If L has an implicitly typed parameter list, D has no ref or
				// out parameters
				//
				// Note: We currently do nothing, because the preview does not
				// follow the above rule.

				//
				// each parameter of L is given the type of the corresponding parameter in D
				//

				for (int i = 0; i < invoke_pd.Count; i++)
					Parameters [i].TypeName = new TypeExpression (
						invoke_pd.ParameterType (i),
						Parameters [i].Location);
			}

			//
			// The return type of the delegate must be compatible with 
			// the anonymous type.   Instead of doing a pass to examine the block
			// we satisfy the rule by setting the return type on the EmitContext
			// to be the delegate type return type.
			//

			ToplevelBlock b = clone ? (ToplevelBlock) Block.PerformClone () : Block;
			
			anonymous = new AnonymousMethod (
				Parent != null ? Parent.AnonymousMethod : null, RootScope, Host,
				GenericMethod, Parameters, Container, b, invoke_mb.ReturnType,
				delegate_type, loc);

			if (!anonymous.Resolve (ec))
				return null;

			return anonymous.AnonymousDelegate;
		}
	}

	//
	// This is a return statement that is prepended lambda expression bodies that happen
	// to be expressions.  Depending on the return type of the delegate this will behave
	// as either { expr (); return (); } or { return expr (); }
	//
	public class ContextualReturn : Statement {
		public Expression Expr;
		
		public ContextualReturn (Expression e)
		{
			Expr = e;
			loc = Expr.Location;
		}

		bool unwind_protect;

		public override bool Resolve (EmitContext ec)
		{
			AnonymousContainer am = ec.CurrentAnonymousMethod;
			if ((am != null) && am.IsIterator && ec.InIterator) {
				Report.Error (1622, loc, "Cannot return a value from iterators. Use the yield return " +
					      "statement to return a value, or yield break to end the iteration");
				return false;
			}

			Expr = Expr.Resolve (ec);
			if (Expr == null)
				return false;

			if (ec.ReturnType == null){
				if (!(Expr is ExpressionStatement)){
					Expression.Error_InvalidExpressionStatement (Expr.Location);
					return false;
				}
			} else {
				if (Expr.Type != ec.ReturnType) {
					Expr = Convert.ImplicitConversionRequired (
						ec, Expr, ec.ReturnType, loc);
					if (Expr == null)
						return false;
				}
			}

			int errors = Report.Errors;
			unwind_protect = ec.CurrentBranching.AddReturnOrigin (ec.CurrentBranching.CurrentUsageVector, loc);
			if (unwind_protect)
				ec.NeedReturnLabel ();
			ec.CurrentBranching.CurrentUsageVector.Return ();
			return errors == Report.Errors;
		}
		
		protected override void DoEmit (EmitContext ec)
		{
			Expr.Emit (ec);

			if (unwind_protect){
				ec.ig.Emit (OpCodes.Stloc, ec.TemporaryReturn ());
				ec.ig.Emit (OpCodes.Leave, ec.ReturnLabel);
			} 
			ec.ig.Emit (OpCodes.Ret);
		}

		protected override void CloneTo (CloneContext clonectx, Statement t)
		{
			ContextualReturn cr = (ContextualReturn) t;

			cr.Expr = Expr.Clone (clonectx);
		}
	}
}
