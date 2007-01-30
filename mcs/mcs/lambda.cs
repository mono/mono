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

namespace Mono.CSharp {
	public class LambdaExpression : AnonymousMethodExpression {
		bool explicit_parameters;

		//
		// If set, this was a lambda expression with an expression
		// argument.  And if so, we have a pointer to it, so we can
		// change it if needed.
		//
		Expression lambda_expr;
		
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
		}

		public void SetExpression (Expression expr)
		{
			lambda_expr = expr;
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
			MethodGroupExpr invoke_mg = Delegate.GetInvokeMethod (
				Host.TypeBuilder, delegate_type, loc);
			MethodInfo invoke_mb = (MethodInfo) invoke_mg.Methods [0];
			ParameterData invoke_pd = TypeManager.GetParameterData (invoke_mb);

			if (Parameters.Count != invoke_pd.Count)
				return false;

			if (explicit_parameters){
				for (int i = 0; i < Parameters.Count; ++i) {
					if (invoke_pd.ParameterType (i) != Parameters.ParameterType (i))
						return false;
				}
			} else {
#if false
				//
				// Although the spec requires this, csc
				// allows for REF and OUT implicit parameters.
				//
				// If implicit, D has no ref or out parameters
				//
				for (int i = 0; i < Parameters.Count; ++i) {
					if (invoke_pd.ParameterModifier (i) != Parameter.Modifier.NONE)
						return false;
				}
#endif

			}
			return false;
		}
	}
}
