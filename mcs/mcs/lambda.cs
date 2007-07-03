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
		// The parameters can either be:
		//    A list of Parameters (explicitly typed parameters)
		//    An ImplicitLambdaParameter
		//
		public LambdaExpression (AnonymousMethodExpression parent,
					 GenericMethod generic, TypeContainer host,
					 Parameters parameters, Block container,
					 Location loc)
			: base (parent, generic, host, parameters, container, loc)
		{
			explicit_parameters = parameters.FixedParameters [0].TypeName != null;
		}

		public bool HasExplicitParameters {
			get {
				return explicit_parameters;
			}
		}
		
		public override Expression DoResolve (EmitContext ec)
		{
			//
			// Only explicit parameters can be resolved at this point
			//
			if (explicit_parameters) {
				if (!Parameters.Resolve (ec))
					return null;
			}

			eclass = ExprClass.Value;
			type = TypeManager.anonymous_method_type;						
			return this;
		}

		public override bool ImplicitStandardConversionExists (Type delegate_type)
		{
			EmitContext ec = EmitContext.TempEc;

			bool result;

			try {
				Report.DisableErrors ();
				result = DoCompatibleTest (ec, delegate_type, true) != null;
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
				if (!VerifyExplicitParameterCompatibility (delegate_type, invoke_pd, false))
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

			return CoreCompatibilityTest (ec, clone, invoke_mb.ReturnType, delegate_type);
		}

		Expression CoreCompatibilityTest (EmitContext ec, bool clone, Type return_type, Type delegate_type)
		{
			//
			// The return type of the delegate must be compatible with 
			// the anonymous type.   Instead of doing a pass to examine the block
			// we satisfy the rule by setting the return type on the EmitContext
			// to be the delegate type return type.
			//

			ToplevelBlock b = clone ? (ToplevelBlock) Block.PerformClone () : Block;
			
			anonymous = new AnonymousMethod (
				Parent != null ? Parent.AnonymousMethod : null, RootScope, Host,
				GenericMethod, Parameters, Container, b, return_type,
				delegate_type, loc);

			bool r;
			if (clone)
				r = anonymous.ResolveNoDefine (ec);
			else
				r = anonymous.Resolve (ec);

			// Resolution failed.
			if (!r)
				return null;

			return anonymous.AnonymousDelegate;
		}
		
		//
		// TryBuild: tries to compile this LambdaExpression with the given
		// types as the lambda expression parameter types.   
		//
		// If the lambda expression successfully builds with those types, the
		// return value will be the inferred type for the lambda expression,
		// otherwise the result will be null.
		//
		public Type TryBuild (EmitContext ec, Type [] types)
		{
			for (int i = 0; i < types.Length; i++)
				Parameters [i].TypeName = new TypeExpression (types [i], Parameters [i].Location);

			Expression e;
			try {
				Report.DisableErrors ();
				e = CoreCompatibilityTest (ec, true, null, null);
			} finally {
				Report.EnableErrors ();
			}
			
			if (e == null)
				return null;
			
			return e.Type;
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
				ec.ReturnType = Expr.Type;
			} else {
				if (Expr.Type != ec.ReturnType) {
					Expression nExpr = Convert.ImplicitConversionRequired (
						ec, Expr, ec.ReturnType, loc);
					if (nExpr == null){
						Report.Error (1662, loc, "Could not implicitly convert from {0} to {1}",
							      TypeManager.CSharpName (Expr.Type),
							      TypeManager.CSharpName (ec.ReturnType));
						return false;
					}
					Expr = nExpr;
				}
			}

			int errors = Report.Errors;
			unwind_protect = ec.CurrentBranching.AddReturnOrigin (ec.CurrentBranching.CurrentUsageVector, loc);
			if (unwind_protect)
				ec.NeedReturnLabel ();
			ec.CurrentBranching.CurrentUsageVector.Goto ();
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
