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

namespace Mono.CSharp {
	public class LambdaExpression : AnonymousMethodExpression {
		Expression expression;
		Block block;

		//
		// The parameter list can either be:
		//    null: no parameters
		//    arraylist of Parameter (explicitly typed parameters)
		//    arraylist of strings (implicitly typed parameters)
		//
		public LambdaExpression (ArrayList parameter_list, object expression_or_block_body, Location l)
			: base (null, null, null, null, null, l)
		{
			expression = expression_or_block_body as Expression;
			block = expression_or_block_body as Block;

			if (expression == null && block == null)
				throw new Exception ("Internal compiler error: only Expression or Block is allowed");

			if (RootContext.Version < LanguageVersion.LINQ){
				Report.FeatureRequiresLINQ (l, "lambda expressions");
				return;
			}
		}

		public override Expression DoResolve (EmitContext ec)
		{
			throw new NotImplementedException ();
		}

		public override void Emit (EmitContext ec)
		{
			throw new NotImplementedException ();
		}
	}
}
