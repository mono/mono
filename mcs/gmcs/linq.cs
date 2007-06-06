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

namespace Mono.CSharp.Linq
{
	// NOTES:
	// Expression should be IExpression to save some memory and make a few things
	// easier to read
	//
	//

	public class QueryExpression : Expression
	{
		public readonly Block Block;
		public readonly TypeContainer Host;

		AQueryClause query;
		public Expression From;

		public QueryExpression (TypeContainer host, Block block, Expression from, AQueryClause query)
		{
			this.Host = host;
			this.Block = block;
			this.From = from;
			this.query = query;
		}

		public override Expression DoResolve (EmitContext ec)
		{
			LocalInfo li = null;
			foreach (LocalInfo li_temp in Block.Variables.Values)
			{
				li = li_temp;
				break;
			}

			Expression e = query.BuildQueryClause (ec, this, From, li);
			return e.Resolve (ec);
		}

		public override void Emit (EmitContext ec)
		{
			throw new NotSupportedException ();
		}
	}

	public abstract class ALinqExpression : Expression
	{
		// Dictionary of method name -> MethodGroupExpr
		static Hashtable methods = new Hashtable ();
		static Type enumerable_class;

		protected abstract string MethodName { get; }

		protected MethodGroupExpr MethodGroup {
			get {
				MethodGroupExpr method_group = (MethodGroupExpr)methods [MethodName];
				if (method_group != null)
					return method_group;

				if (enumerable_class == null)
					enumerable_class = TypeManager.CoreLookupType ("System.Linq", "Enumerable");

				MemberList ml = TypeManager.FindMembers (enumerable_class,
					MemberTypes.Method, BindingFlags.Static | BindingFlags.Public,
					Type.FilterName, MethodName);

				// TODO: implement correct selection
				MethodInfo[] mi = new MethodInfo[] { (MethodInfo)ml[0] };

				method_group = new MethodGroupExpr (mi, loc);
				methods.Add (MethodName, method_group);
				return method_group;
			}
		}
	}

	public abstract class AQueryClause : ALinqExpression
	{
		public AQueryClause Next;
		protected Expression expr;

		protected AQueryClause (Expression expr, Location loc)
		{
			this.expr = expr;
			this.loc = loc;
		}

		public override Expression DoResolve (EmitContext ec)
		{
			return expr.DoResolve (ec);
		}

		public override void Emit (EmitContext ec)
		{
			throw new NotSupportedException ();
		}

		public Expression BuildQueryClause (EmitContext ec, QueryExpression top, Expression from, LocalInfo li)
		{
			Parameters parameters = new Parameters (new Parameter (li.Type, li.Name, Parameter.Modifier.NONE, null, loc));
			AnonymousMethodExpression ame = new AnonymousMethodExpression (
				null, null, top.Host,
				parameters,
				top.Block, loc);
			ame.Block = new ToplevelBlock (parameters, loc);
			ame.Block.AddStatement (new Return (expr, loc));

			ArrayList args = new ArrayList (2);
			args.Add (new Argument (from));
			args.Add (new Argument (ame));

			expr = new Invocation (MethodGroup, args);
			if (Next != null)
				return Next.BuildQueryClause (ec, top, this, li);

			return expr;
		}
	}

	public class Cast : ALinqExpression
	{
		readonly Expression expr;
		readonly Expression cast_type;

		public Cast (Expression type, Expression expr, Location loc)
		{
			this.cast_type = type;
			this.expr = expr;
			this.loc = loc;
		}

		protected override string MethodName {
			get { return "Cast"; }
		}

		public override Expression DoResolve (EmitContext ec)
		{
			TypeArguments type_arguments = new TypeArguments (loc, cast_type);
			Expression cast = MethodGroup.ResolveGeneric (ec, type_arguments);

			ArrayList args = new ArrayList (1);
			args.Add (new Argument (expr));
			return new Invocation (cast, args).DoResolve (ec);
		}

		public override void Emit (EmitContext ec)
		{
			throw new NotSupportedException ();
		}
	}

	public class Select : AQueryClause
	{
		public Select (Expression expr, Location loc)
			: base (expr, loc)
		{
		}

		protected override string MethodName {
			get { return "Select"; }
		}
	}

	public class Where : AQueryClause
	{
		public Where (Expression expr, Location loc)
			: base (expr, loc)
		{
		}

		protected override string MethodName {
			get { return "Where"; }
		}
	}
}
