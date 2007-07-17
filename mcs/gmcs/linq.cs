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
		Expression from;

		public QueryExpression (TypeContainer host, Block block, Expression from, AQueryClause query)
		{
			this.Host = host;
			this.Block = block;
			this.from = from;
			this.query = query;
		}

		public override Expression DoResolve (EmitContext ec)
		{
			from = from.DoResolve (ec);
			if (from == null)
				return null;

			ICollection values = Block.Variables.Values;
			if (values.Count != 1)
				throw new NotImplementedException ("Count != 1");

			IEnumerator enumerator = values.GetEnumerator ();
			enumerator.MoveNext ();
			LocalInfo li = (LocalInfo)enumerator.Current;

			VarExpr var = li.Type as VarExpr;
			if (var != null) {
				li.Type = var.ResolveLValue (ec, from, var.Location);
				if (li.Type == null)
					return null;
				li.VariableType = li.Type.Type;
			}

			Expression e = query.BuildQueryClause (ec, this, from, li);
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

		// TODO: Linq methods are context specific
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

				method_group = new MethodGroupExpr (ArrayList.Adapter (ml), loc);
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
			// TODO: An anonymous method is not enough to infer implicitly typed arguments,
			// we need lambda expression here
			Parameters parameters = new Parameters (new Parameter (li.Type, li.Name, Parameter.Modifier.NONE, null, loc));
			AnonymousMethodExpression ame = new AnonymousMethodExpression (
				null, null, top.Host,
				parameters,
				top.Block, loc);
			ame.Block = new ToplevelBlock (parameters, loc);
			ame.Block.AddStatement (new Return (expr, loc));

			expr = new Invocation (MethodGroup, CreateArguments (ame, from));
			if (Next != null)
				return Next.BuildQueryClause (ec, top, this, li);

			return expr;
		}
			                       
		protected virtual ArrayList CreateArguments (AnonymousMethodExpression ame, Expression from)
		{
			ArrayList args = new ArrayList (2);
			args.Add (new Argument (from));
			args.Add (new Argument (ame));
			return args;
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

	public class GroupBy : AQueryClause
	{
		readonly Expression element_selector;
		
		public GroupBy (Expression elementSelector, Expression keySelector, Location loc)
			: base (keySelector, loc)
		{
			this.element_selector = elementSelector;
		}

		protected override ArrayList CreateArguments (AnonymousMethodExpression ame, Expression from)
		{
			ArrayList args = base.CreateArguments (ame, from);
			
			// A query can be optimized when selector is not group by specific
			if (!element_selector.Equals (from)) {
				AnonymousMethodExpression am_element = new AnonymousMethodExpression (
					null, null, ame.Host, ame.Parameters, ame.Container, loc);
				am_element.Block = new ToplevelBlock (ame.Parameters, loc);
				am_element.Block.AddStatement (new Return (element_selector, loc));
				
				args.Add (new Argument (am_element));
			}
			return args;
		}

		protected override string MethodName {
			get { return "GroupBy"; }
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
	
	public class ImplicitArgument : Expression
	{
		public static ImplicitArgument Instance = new ImplicitArgument ();

		private ImplicitArgument ()
		{
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
