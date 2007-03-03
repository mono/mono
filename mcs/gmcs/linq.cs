//
// linq.cs: support for query expressions
//
// Authors: Marek Safar (marek.safar@gmail.com)
//
// Licensed under the terms of the GNU GPL
//
// (C) 2007 Novell, Inc
//

using System;

namespace Mono.CSharp
{
	public class QueryExpression : Expression
	{
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

