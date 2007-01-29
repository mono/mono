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
	public class LambdaExpression : Expression {
		public LambdaExpression (ArrayList parameters, object body, Location loc)
		{
			this.loc = loc;
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
