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
			if (parameters.FixedParameters.Length > 0)
				explicit_parameters = parameters.FixedParameters [0].TypeName != null;
		}

		public override bool HasExplicitParameters {
			get {
				return explicit_parameters;
			}
		}

		protected override Parameters CreateParameters (EmitContext ec, Type delegateType, ParameterData delegateParameters)
		{
			Parameters p = base.CreateParameters (ec, delegateType, delegateParameters);
			if (explicit_parameters)
				return p;

			//
			// If L has an implicitly typed parameter list
			//
			for (int i = 0; i < delegateParameters.Count; i++) {
				// D has no ref or out parameters
				//if ((invoke_pd.ParameterModifier (i) & Parameter.Modifier.ISBYREF) != 0)
				//	return null;

				//
				// Makes implicit parameters explicit
				// Set each parameter of L is given the type of the corresponding parameter in D
				//
				p[i].ParameterType = delegateParameters.Types[i];
			}
			return p;
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
				bool r = Compatible (ec, delegate_type) != null;

				// Ignore the result
				anonymous = null;

				return r;
			}
		}

		//
		// Resolves a body of lambda expression.
		//
		protected override Expression ResolveMethod (EmitContext ec, Parameters parameters, Type returnType, Type delegateType)
		{
			ToplevelBlock b = ec.IsInProbingMode ? (ToplevelBlock) Block.PerformClone () : Block;

			anonymous = new LambdaMethod (
				Parent != null ? Parent.AnonymousMethod : null, RootScope, Host,
				GenericMethod, Parameters, Container, b, returnType,
				delegateType, loc);

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
				e = ResolveMethod (ec, Parameters, typeof (LambdaExpression), null);
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
