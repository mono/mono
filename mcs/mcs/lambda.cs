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
			if (parameters == null)
				Parameters = new Parameters (new Parameter [0]);
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

		//
		// Returns true if this anonymous method can be implicitly
		// converted to the delegate type `delegate_type'
		//
		public override Expression Compatible (EmitContext ec, Type delegate_type)
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

			Parameters parameters_copy = Parameters.Clone ();
			if (explicit_parameters){
				//
				// If L has an explicitly typed parameter list, each parameter
				// in D has the same type and modifiers as the corresponding
				// parameter in L.
				//
				if (!VerifyExplicitParameterCompatibility (delegate_type, invoke_pd))
					return null;

				parameters_copy = Parameters.Clone ();
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
					parameters_copy [i].TypeName = new TypeExpression (
						invoke_pd.ParameterType (i),
						parameters_copy [i].Location);
			}
			
				
			if (invoke_mb.ReturnType == TypeManager.void_type){
				
			}
			
			if (lambda_expr == null){
			} else {
			}

#if false
			//
			// Second: the return type of the delegate must be compatible with 
			// the anonymous type.   Instead of doing a pass to examine the block
			// we satisfy the rule by setting the return type on the EmitContext
			// to be the delegate type return type.
			//

			//MethodBuilder builder = method_data.MethodBuilder;
			//ILGenerator ig = builder.GetILGenerator ();

			Report.Debug (64, "COMPATIBLE", this, Parent, GenericMethod, Host,
				      Container, Block, invoke_mb.ReturnType, delegate_type,
				      TypeManager.IsGenericType (delegate_type), loc);

			anonymous = new AnonymousMethod (
				Parent != null ? Parent.AnonymousMethod : null, RootScope, Host,
				GenericMethod, parameters, Container, Block, invoke_mb.ReturnType,
				delegate_type, loc);

			if (!anonymous.Resolve (ec))
				return null;

			return anonymous.AnonymousDelegate;
#endif
			return null;
		}
	}
}
