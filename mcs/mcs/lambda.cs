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

		//
		// Returns true if the body of lambda expression can be implicitly
		// converted to the delegate of type `delegate_type'
		//
		public override bool ImplicitStandardConversionExists (Type delegate_type)
		{
			EmitContext ec = EmitContext.TempEc;

			using (ec.Set (EmitContext.Flags.ProbingMode)) {
				bool r = DoImplicitStandardConversion (ec, delegate_type) != null;

				// Ignore the result
				anonymous = null;

				return r;
			}
		}
		
		//
		// Returns true if this anonymous method can be implicitly
		// converted to the delegate type `delegate_type'
		//
		public override Expression Compatible (EmitContext ec, Type delegate_type)
		{
			if (anonymous != null)
				return anonymous.AnonymousDelegate;

			return DoImplicitStandardConversion (ec, delegate_type);
		}

		Expression DoImplicitStandardConversion (EmitContext ec, Type delegate_type)
		{
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
				// If L has an implicitly typed parameter list
				//
				for (int i = 0; i < invoke_pd.Count; i++) {
					// D has no ref or out parameters
					if ((invoke_pd.ParameterModifier (i) & Parameter.Modifier.ISBYREF) != 0)
						return null;

					//
					// Makes implicit parameters explicit
					// Set each parameter of L is given the type of the corresponding parameter in D
					//
					Parameters[i].ParameterType = invoke_pd.Types[i];
					
				}
			}

			return CoreCompatibilityTest (ec, invoke_mb.ReturnType, delegate_type);
		}

		Expression CoreCompatibilityTest (EmitContext ec, Type return_type, Type delegate_type)
		{
			//
			// The return type of the delegate must be compatible with 
			// the anonymous type.   Instead of doing a pass to examine the block
			// we satisfy the rule by setting the return type on the EmitContext
			// to be the delegate type return type.
			//

			ToplevelBlock b = ec.IsInProbingMode ? (ToplevelBlock) Block.PerformClone () : Block;

			anonymous = new AnonymousMethod (
				Parent != null ? Parent.AnonymousMethod : null, RootScope, Host,
				GenericMethod, Parameters, Container, b, return_type,
				delegate_type, loc);

			bool r;
			if (ec.IsInProbingMode)
				r = anonymous.ResolveNoDefine (ec);
			else
				r = anonymous.Resolve (ec);

			// Resolution failed.
			if (!r)
				return null;

			return anonymous.AnonymousDelegate;
		}
		
		public override string GetSignatureForError ()
		{
			return "lambda expression";
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

			// TODO: temporary hack
			ec.InferReturnType = true;
			
			Expression e;
			using (ec.Set (EmitContext.Flags.ProbingMode)) {
				e = CoreCompatibilityTest (ec, null, null);
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

	public class ContextualReturn : Return
	{
		public ContextualReturn (Expression expr)
			: base (expr, expr.Location)
		{
		}
	}
}
