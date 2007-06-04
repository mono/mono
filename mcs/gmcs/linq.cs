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
using System.Reflection;
using System.Collections;

namespace Mono.CSharp
{
	public class QueryExpression : Expression
	{
		Expression query_body;
		Expression from;

		public QueryExpression (Expression from, Expression queryBody)
		{
			this.from = from;
			this.query_body = queryBody;
		}

		public override Expression DoResolve (EmitContext ec)
		{
			from = from.Resolve (ec);
			((Select)query_body).From = from;
			query_body = query_body.Resolve (ec);

			return query_body;
		}

		public override void Emit (EmitContext ec)
		{
			throw new NotImplementedException ();
		}
	}

	public class Select : Expression
	{
		Expression expr;
		public Expression From;

		TypeContainer host;
		Block block;

		public Select (TypeContainer host, Block block, Expression expr, Location loc)
		{
			this.host = host;
			this.block = block;

			this.expr = expr;
			this.loc = loc;
		}

		public override Expression DoResolve (EmitContext ec)
		{
			MethodInfo[] mi = new MethodInfo[] { TypeManager.enumerable_select };
			MethodGroupExpr select_mg = new MethodGroupExpr (mi, loc);

			// TODO:
			LocalInfo li = null;
			foreach (LocalInfo li_temp in block.Variables.Values) {
				li = li_temp;
				break;
			}

			// TODO: eplicit li.Type means .Cast between IEnumerable and IEnumerable<T>
			Parameters p = new Parameters (new Parameter (li.Type, li.Name, Parameter.Modifier.NONE, null, loc));
			AnonymousMethodExpression ame = new AnonymousMethodExpression (null, null, host, p, block, loc);
			ame.Block = new ToplevelBlock (p, loc);
			ame.Block.AddStatement (new Return (expr, loc));

			ArrayList args = new ArrayList (2);
			args.Add (new Argument (From));
			args.Add (new Argument (ame));

			Expression select = new Invocation (select_mg, args);
			select = select.Resolve (ec);
			return select;
		}

		public override void Emit (EmitContext ec)
		{
			throw new NotImplementedException ();
		}
	}
}

