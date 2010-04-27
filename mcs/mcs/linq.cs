//
// linq.cs: support for query expressions
//
// Authors: Marek Safar (marek.safar@gmail.com)
//
// Dual licensed under the terms of the MIT X11 or GNU GPL
//
// Copyright 2007-2008 Novell, Inc
//

using System;
using System.Reflection;
using System.Collections.Generic;

namespace Mono.CSharp.Linq
{
	// NOTES:
	// Expression should be IExpression to save some memory and make a few things
	// easier to read
	//
	//

	class QueryExpression : AQueryClause
	{
		public QueryExpression (Block block, AQueryClause query)
			: base (null, null, query.Location)
		{
			this.next = query;
		}

		public override Expression BuildQueryClause (ResolveContext ec, Expression lSide)
		{
			return next.BuildQueryClause (ec, lSide);
		}

		protected override Expression DoResolve (ResolveContext ec)
		{
			int counter = QueryBlock.TransparentParameter.Counter;

			Expression e = BuildQueryClause (ec, null);
			if (e != null)
				e = e.Resolve (ec);

			//
			// Reset counter in probing mode to ensure that all transparent
			// identifier anonymous types are created only once
			//
			if (ec.IsInProbingMode)
				QueryBlock.TransparentParameter.Counter = counter;

			return e;
		}

		protected override string MethodName {
			get { throw new NotSupportedException (); }
		}
	}

	abstract class AQueryClause : ShimExpression
	{
		class QueryExpressionAccess : MemberAccess
		{
			public QueryExpressionAccess (Expression expr, string methodName, Location loc)
				: base (expr, methodName, loc)
			{
			}

			public QueryExpressionAccess (Expression expr, string methodName, TypeArguments typeArguments, Location loc)
				: base (expr, methodName, typeArguments, loc)
			{
			}

			protected override Expression Error_MemberLookupFailed (ResolveContext ec, TypeSpec container_type, TypeSpec qualifier_type,
				TypeSpec queried_type, string name, int arity, string class_name, MemberKind mt, BindingRestriction bf)
			{
				ec.Report.Error (1935, loc, "An implementation of `{0}' query expression pattern could not be found. " +
					"Are you missing `System.Linq' using directive or `System.Core.dll' assembly reference?",
					name);
				return null;
			}
		}

		class QueryExpressionInvocation : Invocation, MethodGroupExpr.IErrorHandler
		{
			public QueryExpressionInvocation (QueryExpressionAccess expr, Arguments arguments)
				: base (expr, arguments)
			{
			}

			protected override MethodGroupExpr DoResolveOverload (ResolveContext ec)
			{
				mg.CustomErrorHandler = this;
				MethodGroupExpr rmg = mg.OverloadResolve (ec, ref arguments, false, loc);
				return rmg;
			}

			public bool AmbiguousCall (ResolveContext ec, MethodSpec ambiguous)
			{
				ec.Report.SymbolRelatedToPreviousError (mg.BestCandidate);
				ec.Report.SymbolRelatedToPreviousError (ambiguous);
				ec.Report.Error (1940, loc, "Ambiguous implementation of the query pattern `{0}' for source type `{1}'",
					mg.Name, mg.InstanceExpression.GetSignatureForError ());
				return true;
			}

			public bool NoExactMatch (ResolveContext ec, MethodSpec method)
			{
				var pd = method.Parameters;
				TypeSpec source_type = pd.ExtensionMethodType;
				if (source_type != null) {
					Argument a = arguments [0];

					if (TypeManager.IsGenericType (source_type) && TypeManager.ContainsGenericParameters (source_type)) {
						TypeInferenceContext tic = new TypeInferenceContext (source_type.TypeArguments);
						tic.OutputTypeInference (ec, a.Expr, source_type);
						if (tic.FixAllTypes (ec)) {
							source_type = source_type.GetDefinition ().MakeGenericType (tic.InferredTypeArguments);
						}
					}

					if (!Convert.ImplicitConversionExists (ec, a.Expr, source_type)) {
						ec.Report.Error (1936, loc, "An implementation of `{0}' query expression pattern for source type `{1}' could not be found",
							mg.Name, TypeManager.CSharpName (a.Type));
						return true;
					}
				}

				if (!method.IsGeneric)
					return false;

				if (mg.Name == "SelectMany") {
					ec.Report.Error (1943, loc,
						"An expression type is incorrect in a subsequent `from' clause in a query expression with source type `{0}'",
						arguments [0].GetSignatureForError ());
				} else {
					ec.Report.Error (1942, loc,
						"An expression type in `{0}' clause is incorrect. Type inference failed in the call to `{1}'",
						mg.Name.ToLower (), mg.Name);
				}

				return true;
			}
		}

		// TODO: protected
		public AQueryClause next;
		protected ToplevelBlock block;

		protected AQueryClause (ToplevelBlock block, Expression expr, Location loc)
			 : base (expr)
		{
			this.block = block;
			this.loc = loc;
		}
		
		protected override void CloneTo (CloneContext clonectx, Expression target)
		{
			base.CloneTo (clonectx, target);

			AQueryClause t = (AQueryClause) target;

			if (block != null)
				t.block = (ToplevelBlock) block.Clone (clonectx);

			if (next != null)
				t.next = (AQueryClause) next.Clone (clonectx);
		}

		protected override Expression DoResolve (ResolveContext ec)
		{
			return expr.Resolve (ec);
		}

		public virtual Expression BuildQueryClause (ResolveContext ec, Expression lSide)
		{
			Arguments args;
			CreateArguments (ec, out args);
			lSide = CreateQueryExpression (lSide, args);
			if (next != null) {
				Select s = next as Select;
				if (s == null || s.IsRequired)
					return next.BuildQueryClause (ec, lSide);
					
				// Skip transparent select clause if any clause follows
				if (next.next != null)
					return next.next.BuildQueryClause (ec, lSide);
			}

			return lSide;
		}

		protected virtual void CreateArguments (ResolveContext ec, out Arguments args)
		{
			args = new Arguments (2);

			LambdaExpression selector = new LambdaExpression (loc);
			selector.Block = block;
			selector.Block.AddStatement (new ContextualReturn (expr));

			args.Add (new Argument (selector));
		}

		protected Invocation CreateQueryExpression (Expression lSide, Arguments arguments)
		{
			return new QueryExpressionInvocation (
				new QueryExpressionAccess (lSide, MethodName, loc), arguments);
		}

		protected Invocation CreateQueryExpression (Expression lSide, TypeArguments typeArguments, Arguments arguments)
		{
			return new QueryExpressionInvocation (
				new QueryExpressionAccess (lSide, MethodName, typeArguments, loc), arguments);
		}

		protected abstract string MethodName { get; }

		public virtual AQueryClause Next {
			set {
				next = value;
			}
		}

		public AQueryClause Tail {
			get {
				return next == null ? this : next.Tail;
			}
		}
	}

	//
	// A query clause with an identifier (range variable)
	//
	abstract class ARangeVariableQueryClause : AQueryClause
	{
		sealed class RangeAnonymousTypeParameter : AnonymousTypeParameter
		{
			public RangeAnonymousTypeParameter (Expression initializer, SimpleMemberName parameter)
				: base (initializer, parameter.Value, parameter.Location)
			{
			}

			protected override void Error_InvalidInitializer (ResolveContext ec, string initializer)
			{
				ec.Report.Error (1932, loc, "A range variable `{0}' cannot be initialized with `{1}'",
					Name, initializer);
			}
		}

		protected ARangeVariableQueryClause (ToplevelBlock block, Expression expr)
			: base (block, expr, expr.Location)
		{
		}

		protected static Expression CreateRangeVariableType (ToplevelBlock block, TypeContainer container, SimpleMemberName name, Expression init)
		{
			var args = new List<AnonymousTypeParameter> (2);
			args.Add (new AnonymousTypeParameter (block.Parameters [0]));
			args.Add (new RangeAnonymousTypeParameter (init, name));
			return new NewAnonymousType (args, container, name.Location);
		}
	}

	class QueryStartClause : AQueryClause
	{
		public QueryStartClause (Expression expr)
			: base (null, expr, expr.Location)
		{
		}

		public override Expression BuildQueryClause (ResolveContext ec, Expression lSide)
		{
			expr = expr.Resolve (ec);
			if (expr == null)
				return null;

			if (expr.Type == InternalType.Dynamic || expr.Type == TypeManager.void_type) {
				ec.Report.Error (1979, expr.Location,
					"Query expression with a source or join sequence of type `{0}' is not allowed",
					TypeManager.CSharpName (expr.Type));
				return null;
			}

			return next.BuildQueryClause (ec, expr);
		}

		protected override Expression DoResolve (ResolveContext ec)
		{
			Expression e = BuildQueryClause (ec, null);
			return e.Resolve (ec);
		}

		protected override string MethodName {
			get { throw new NotSupportedException (); }
		}
	}

	class Cast : QueryStartClause
	{
		// We don't have to clone cast type
		readonly FullNamedExpression type_expr;

		public Cast (FullNamedExpression type, Expression expr)
			: base (expr)
		{
			this.type_expr = type;
		}
		
		public override Expression BuildQueryClause (ResolveContext ec, Expression lSide)
		{
			lSide = CreateQueryExpression (expr, new TypeArguments (type_expr), null);
			if (next != null)
				return next.BuildQueryClause (ec, lSide);

			return lSide;
		}

		protected override string MethodName {
			get { return "Cast"; }
		}
	}

	class GroupBy : AQueryClause
	{
		Expression element_selector;
		ToplevelBlock element_block;
		
		public GroupBy (ToplevelBlock block, Expression elementSelector, ToplevelBlock elementBlock, Expression keySelector, Location loc)
			: base (block, keySelector, loc)
		{
			//
			// Optimizes clauses like `group A by A'
			//
			if (!elementSelector.Equals (keySelector)) {
				this.element_selector = elementSelector;
				this.element_block = elementBlock;
			}
		}

		protected override void CreateArguments (ResolveContext ec, out Arguments args)
		{
			base.CreateArguments (ec, out args);

			if (element_selector != null) {
				LambdaExpression lambda = new LambdaExpression (element_selector.Location);
				lambda.Block = element_block;
				lambda.Block.AddStatement (new ContextualReturn (element_selector));
				args.Add (new Argument (lambda));
			}
		}

		protected override void CloneTo (CloneContext clonectx, Expression target)
		{
			GroupBy t = (GroupBy) target;
			if (element_selector != null) {
				t.element_selector = element_selector.Clone (clonectx);
				t.element_block = (ToplevelBlock) element_block.Clone (clonectx);
			}

			base.CloneTo (clonectx, t);
		}

		protected override string MethodName {
			get { return "GroupBy"; }
		}
	}

	class Join : ARangeVariableQueryClause
	{
		readonly SimpleMemberName lt;
		ToplevelBlock inner_selector, outer_selector;

		public Join (ToplevelBlock block, SimpleMemberName lt, Expression inner, ToplevelBlock outerSelector, ToplevelBlock innerSelector, Location loc)
			: base (block, inner)
		{
			this.lt = lt;
			this.outer_selector = outerSelector;
			this.inner_selector = innerSelector;
		}

		protected override void CreateArguments (ResolveContext ec, out Arguments args)
		{
			args = new Arguments (4);

			args.Add (new Argument (expr));

			LambdaExpression lambda = new LambdaExpression (outer_selector.StartLocation);
			lambda.Block = outer_selector;
			args.Add (new Argument (lambda));

			lambda = new LambdaExpression (inner_selector.StartLocation);
			lambda.Block = inner_selector;
			args.Add (new Argument (lambda));

			Expression result_selector_expr;
			SimpleMemberName into_variable = GetIntoVariable ();
			//
			// When select follows use is as result selector
			//
			if (next is Select) {
				result_selector_expr = next.Expr;
				next = next.next;
			} else {
				result_selector_expr = CreateRangeVariableType (block, ec.MemberContext.CurrentMemberDefinition.Parent, into_variable,
					new SimpleName (into_variable.Value, into_variable.Location));
			}

			LambdaExpression result_selector = new LambdaExpression (lt.Location);
			result_selector.Block = new QueryBlock (ec.Compiler, block.Parent, block.Parameters, into_variable, block.StartLocation);
			result_selector.Block.AddStatement (new ContextualReturn (result_selector_expr));

			args.Add (new Argument (result_selector));
		}

		protected virtual SimpleMemberName GetIntoVariable ()
		{
			return lt;
		}

		protected override void CloneTo (CloneContext clonectx, Expression target)
		{
			Join t = (Join) target;
			t.inner_selector = (ToplevelBlock) inner_selector.Clone (clonectx);
			t.outer_selector = (ToplevelBlock) outer_selector.Clone (clonectx);
			base.CloneTo (clonectx, t);
		}	

		protected override string MethodName {
			get { return "Join"; }
		}
	}

	class GroupJoin : Join
	{
		readonly SimpleMemberName into;

		public GroupJoin (ToplevelBlock block, SimpleMemberName lt, Expression inner,
			ToplevelBlock outerSelector, ToplevelBlock innerSelector, SimpleMemberName into, Location loc)
			: base (block, lt, inner, outerSelector, innerSelector, loc)
		{
			this.into = into;
		}

		protected override SimpleMemberName GetIntoVariable ()
		{
			return into;
		}

		protected override string MethodName {
			get { return "GroupJoin"; }
		}
	}

	class Let : ARangeVariableQueryClause
	{
		public Let (ToplevelBlock block, TypeContainer container, SimpleMemberName identifier, Expression expr)
			: base (block, CreateRangeVariableType (block, container, identifier, expr))
		{
		}

		protected override string MethodName {
			get { return "Select"; }
		}
	}

	class Select : AQueryClause
	{
		public Select (ToplevelBlock block, Expression expr, Location loc)
			: base (block, expr, loc)
		{
		}
		
		//
		// For queries like `from a orderby a select a'
		// the projection is transparent and select clause can be safely removed 
		//
		public bool IsRequired {
			get {
				SimpleName sn = expr as SimpleName;
				if (sn == null)
					return true;

				return sn.Name != block.Parameters.FixedParameters [0].Name;
			}
		}

		protected override string MethodName {
			get { return "Select"; }
		}
	}

	class SelectMany : ARangeVariableQueryClause
	{
		SimpleMemberName lt;

		public SelectMany (ToplevelBlock block, SimpleMemberName lt, Expression expr)
			: base (block, expr)
		{
			this.lt = lt;
		}

		protected override void CreateArguments (ResolveContext ec, out Arguments args)
		{
			base.CreateArguments (ec, out args);

			Expression result_selector_expr;
			//
			// When select follow use is as result selector
			//
			if (next is Select) {
				result_selector_expr = next.Expr;
				next = next.next;
			} else {
				result_selector_expr = CreateRangeVariableType (block, ec.MemberContext.CurrentMemberDefinition.Parent, lt, new SimpleName (lt.Value, lt.Location));
			}

			LambdaExpression result_selector = new LambdaExpression (lt.Location);
			result_selector.Block = new QueryBlock (ec.Compiler, block.Parent, block.Parameters, lt, block.StartLocation);
			result_selector.Block.AddStatement (new ContextualReturn (result_selector_expr));

			args.Add (new Argument (result_selector));
		}

		protected override string MethodName {
			get { return "SelectMany"; }
		}
	}

	class Where : AQueryClause
	{
		public Where (ToplevelBlock block, BooleanExpression expr, Location loc)
			: base (block, expr, loc)
		{
		}

		protected override string MethodName {
			get { return "Where"; }
		}
	}

	class OrderByAscending : AQueryClause
	{
		public OrderByAscending (ToplevelBlock block,Expression expr)
			: base (block, expr, expr.Location)
		{
		}

		protected override string MethodName {
			get { return "OrderBy"; }
		}
	}

	class OrderByDescending : AQueryClause
	{
		public OrderByDescending (ToplevelBlock block, Expression expr)
			: base (block, expr, expr.Location)
		{
		}

		protected override string MethodName {
			get { return "OrderByDescending"; }
		}
	}

	class ThenByAscending : OrderByAscending
	{
		public ThenByAscending (ToplevelBlock block, Expression expr)
			: base (block, expr)
		{
		}

		protected override string MethodName {
			get { return "ThenBy"; }
		}
	}

	class ThenByDescending : OrderByDescending
	{
		public ThenByDescending (ToplevelBlock block, Expression expr)
			: base (block, expr)
		{
		}

		protected override string MethodName {
			get { return "ThenByDescending"; }
		}
	}

	//
	// Implicit query block
	//
	class QueryBlock : ToplevelBlock
	{
		//
		// Transparent parameters are used to package up the intermediate results
		// and pass them onto next clause
		//
		public sealed class TransparentParameter : ImplicitLambdaParameter
		{
			public static int Counter;
			const string ParameterNamePrefix = "<>__TranspIdent";

			public readonly ParametersCompiled Parent;
			public readonly string Identifier;

			public TransparentParameter (ParametersCompiled parent, SimpleMemberName identifier)
				: base (ParameterNamePrefix + Counter++, identifier.Location)
			{
				Parent = parent;
				Identifier = identifier.Value;
			}

			public new static void Reset ()
			{
				Counter = 0;
			}
		}

		public sealed class ImplicitQueryParameter : ImplicitLambdaParameter
		{
			public ImplicitQueryParameter (string name, Location loc)
				: base (name, loc)
			{
			}
		}

		public QueryBlock (CompilerContext ctx, Block parent, SimpleMemberName lt, Location start)
			: base (ctx, parent, new ParametersCompiled (ctx, new ImplicitQueryParameter (lt.Value, lt.Location)), start)
		{
			if (parent != null)
				base.CheckParentConflictName (parent.Toplevel, lt.Value, lt.Location);
		}

		public QueryBlock (CompilerContext ctx, Block parent, ParametersCompiled parameters, SimpleMemberName lt, Location start)
			: base (ctx, parent, new ParametersCompiled (ctx, parameters [0].Clone (), new ImplicitQueryParameter (lt.Value, lt.Location)), start)
		{
		}

		public QueryBlock (CompilerContext ctx, Block parent, Location start)
			: base (ctx, parent, parent.Toplevel.Parameters.Clone (), start)
		{
		}

		public void AddTransparentParameter (CompilerContext ctx, SimpleMemberName name)
		{
			base.CheckParentConflictName (this, name.Value, name.Location);

			parameters = new ParametersCompiled (ctx, new TransparentParameter (parameters, name));
		}

		protected override bool CheckParentConflictName (ToplevelBlock block, string name, Location l)
		{
			return true;
		}

		// 
		// Query parameter reference can include transparent parameters
		//
		protected override Expression GetParameterReferenceExpression (string name, Location loc)
		{
			Expression expr = base.GetParameterReferenceExpression (name, loc);
			if (expr != null)
				return expr;

			TransparentParameter tp = parameters [0] as TransparentParameter;
			while (tp != null) {
				if (tp.Identifier == name)
					break;

				TransparentParameter tp_next = tp.Parent [0] as TransparentParameter;
				if (tp_next == null) {
					if (tp.Parent.GetParameterIndexByName (name) >= 0)
						break;
				}

				tp = tp_next;
			}

			if (tp != null) {
				expr = new SimpleName (parameters[0].Name, loc);
				TransparentParameter tp_cursor = (TransparentParameter) parameters[0];
				while (tp_cursor != tp) {
					tp_cursor = (TransparentParameter) tp_cursor.Parent[0];
					expr = new MemberAccess (expr, tp_cursor.Name);
				}

				return new MemberAccess (expr, name);
			}

			return null;
		}

		protected override void Error_AlreadyDeclared (Location loc, string var, string reason)
		{
			Report.Error (1931, loc, "A range variable `{0}' conflicts with a previous declaration of `{0}'",
				var);
		}
		
		protected override void Error_AlreadyDeclared (Location loc, string var)
		{
			Report.Error (1930, loc, "A range variable `{0}' has already been declared in this scope",
				var);		
		}
		
		public override void Error_AlreadyDeclaredTypeParameter (Report r, Location loc, string name, string conflict)
		{
			r.Error (1948, loc, "A range variable `{0}' conflicts with a method type parameter",
				name);
		}
	}
}
