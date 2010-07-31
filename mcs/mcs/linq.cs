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
	public class QueryExpression : AQueryClause
	{
		public QueryExpression (AQueryClause start)
			: base (null, null, Location.Null)
		{
			this.next = start;
		}

		public override Expression BuildQueryClause (ResolveContext ec, Expression lSide, Parameter parentParameter)
		{
			return next.BuildQueryClause (ec, lSide, parentParameter);
		}

		protected override Expression DoResolve (ResolveContext ec)
		{
			int counter = QueryBlock.TransparentParameter.Counter;

			Expression e = BuildQueryClause (ec, null, null);
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

	public abstract class AQueryClause : ShimExpression
	{
		protected class QueryExpressionAccess : MemberAccess
		{
			public QueryExpressionAccess (Expression expr, string methodName, Location loc)
				: base (expr, methodName, loc)
			{
			}

			public QueryExpressionAccess (Expression expr, string methodName, TypeArguments typeArguments, Location loc)
				: base (expr, methodName, typeArguments, loc)
			{
			}

			protected override void Error_TypeDoesNotContainDefinition (ResolveContext ec, TypeSpec type, string name)
			{
				ec.Report.Error (1935, loc, "An implementation of `{0}' query expression pattern could not be found. " +
					"Are you missing `System.Linq' using directive or `System.Core.dll' assembly reference?",
					name);
			}
		}

		protected class QueryExpressionInvocation : Invocation, OverloadResolver.IErrorHandler
		{
			public QueryExpressionInvocation (QueryExpressionAccess expr, Arguments arguments)
				: base (expr, arguments)
			{
			}

			protected override MethodGroupExpr DoResolveOverload (ResolveContext ec)
			{
				MethodGroupExpr rmg = mg.OverloadResolve (ec, ref arguments, this, OverloadResolver.Restrictions.None);
				return rmg;
			}

			#region IErrorHandler Members

			bool OverloadResolver.IErrorHandler.AmbiguousCandidates (ResolveContext ec, MemberSpec best, MemberSpec ambiguous)
			{
				ec.Report.SymbolRelatedToPreviousError (best);
				ec.Report.SymbolRelatedToPreviousError (ambiguous);
				ec.Report.Error (1940, loc, "Ambiguous implementation of the query pattern `{0}' for source type `{1}'",
					best.Name, mg.InstanceExpression.GetSignatureForError ());
				return true;
			}

			bool OverloadResolver.IErrorHandler.ArgumentMismatch (ResolveContext rc, MemberSpec best, Argument arg, int index)
			{
				return false;
			}

			bool OverloadResolver.IErrorHandler.NoArgumentMatch (ResolveContext rc, MemberSpec best)
			{
				return false;
			}

			bool OverloadResolver.IErrorHandler.TypeInferenceFailed (ResolveContext rc, MemberSpec best)
			{
				var ms = (MethodSpec) best;
				TypeSpec source_type = ms.Parameters.ExtensionMethodType;
				if (source_type != null) {
					Argument a = arguments[0];

					if (TypeManager.IsGenericType (source_type) && TypeManager.ContainsGenericParameters (source_type)) {
						TypeInferenceContext tic = new TypeInferenceContext (source_type.TypeArguments);
						tic.OutputTypeInference (rc, a.Expr, source_type);
						if (tic.FixAllTypes (rc)) {
							source_type = source_type.GetDefinition ().MakeGenericType (tic.InferredTypeArguments);
						}
					}

					if (!Convert.ImplicitConversionExists (rc, a.Expr, source_type)) {
						rc.Report.Error (1936, loc, "An implementation of `{0}' query expression pattern for source type `{1}' could not be found",
							best.Name, TypeManager.CSharpName (a.Type));
						return true;
					}
				}

				if (best.Name == "SelectMany") {
					rc.Report.Error (1943, loc,
						"An expression type is incorrect in a subsequent `from' clause in a query expression with source type `{0}'",
						arguments[0].GetSignatureForError ());
				} else {
					rc.Report.Error (1942, loc,
						"An expression type in `{0}' clause is incorrect. Type inference failed in the call to `{1}'",
						best.Name.ToLowerInvariant (), best.Name);
				}

				return true;
			}

			#endregion
		}

		public AQueryClause next;
		public QueryBlock block;

		protected AQueryClause (QueryBlock block, Expression expr, Location loc)
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
				t.block = (QueryBlock) clonectx.LookupBlock (block);

			if (next != null)
				t.next = (AQueryClause) next.Clone (clonectx);
		}

		protected override Expression DoResolve (ResolveContext ec)
		{
			return expr.Resolve (ec);
		}

		public virtual Expression BuildQueryClause (ResolveContext ec, Expression lSide, Parameter parameter)
		{
			Arguments args = null;
			CreateArguments (ec, parameter, ref args);
			lSide = CreateQueryExpression (lSide, args);
			if (next != null) {
				parameter = CreateChildrenParameters (parameter);

				Select s = next as Select;
				if (s == null || s.IsRequired (parameter))
					return next.BuildQueryClause (ec, lSide, parameter);
					
				// Skip transparent select clause if any clause follows
				if (next.next != null)
					return next.next.BuildQueryClause (ec, lSide, parameter);
			}

			return lSide;
		}

		protected virtual Parameter CreateChildrenParameters (Parameter parameter)
		{
			return parameter;
		}

		protected virtual void CreateArguments (ResolveContext ec, Parameter parameter, ref Arguments args)
		{
			args = new Arguments (2);

			LambdaExpression selector = new LambdaExpression (loc);

			block.SetParameter (parameter.Clone ());
			selector.Block = block;
			selector.Block.AddStatement (new ContextualReturn (expr));

			args.Add (new Argument (selector));
		}

		protected Invocation CreateQueryExpression (Expression lSide, Arguments arguments)
		{
			return new QueryExpressionInvocation (
				new QueryExpressionAccess (lSide, MethodName, loc), arguments);
		}

		protected abstract string MethodName { get; }

		public AQueryClause Next {
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
	public abstract class ARangeVariableQueryClause : AQueryClause
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

		protected SimpleMemberName range_variable;

		protected ARangeVariableQueryClause (QueryBlock block, SimpleMemberName identifier, Expression expr, Location loc)
			: base (block, expr, loc)
		{
			range_variable = identifier;
		}

		public FullNamedExpression IdentifierType { get; set; }

		protected Invocation CreateCastExpression (Expression lSide)
		{
			return new QueryExpressionInvocation (
				new QueryExpressionAccess (lSide, "Cast", new TypeArguments (IdentifierType), loc), null);
		}

		protected override Parameter CreateChildrenParameters (Parameter parameter)
		{
			return new QueryBlock.TransparentParameter (parameter, GetIntoVariable ());
		}

		protected static Expression CreateRangeVariableType (ResolveContext rc, Parameter parameter, SimpleMemberName name, Expression init)
		{
			var args = new List<AnonymousTypeParameter> (2);
			args.Add (new AnonymousTypeParameter (parameter));
			args.Add (new RangeAnonymousTypeParameter (init, name));
			return new NewAnonymousType (args, rc.MemberContext.CurrentMemberDefinition.Parent, name.Location);
		}

		protected virtual SimpleMemberName GetIntoVariable ()
		{
			return range_variable;
		}
	}

	class QueryStartClause : ARangeVariableQueryClause
	{
		public QueryStartClause (QueryBlock block, Expression expr, SimpleMemberName identifier, Location loc)
			: base (block, identifier, expr, loc)
		{
			block.AddRangeVariable (identifier);
		}

		public override Expression BuildQueryClause (ResolveContext ec, Expression lSide, Parameter parameter)
		{
/*
			expr = expr.Resolve (ec);
			if (expr == null)
				return null;

			if (expr.Type == InternalType.Dynamic || expr.Type == TypeManager.void_type) {
				ec.Report.Error (1979, expr.Location,
					"Query expression with a source or join sequence of type `{0}' is not allowed",
					TypeManager.CSharpName (expr.Type));
				return null;
			}
*/

			if (IdentifierType != null)
				expr = CreateCastExpression (expr);

			if (parameter == null)
				lSide = expr;

			return next.BuildQueryClause (ec, lSide, new ImplicitLambdaParameter (range_variable.Value, range_variable.Location));
		}

		protected override Expression DoResolve (ResolveContext ec)
		{
			Expression e = BuildQueryClause (ec, null, null);
			return e.Resolve (ec);
		}

		protected override string MethodName {
			get { throw new NotSupportedException (); }
		}
	}

	public class GroupBy : AQueryClause
	{
		Expression element_selector;
		QueryBlock element_block;

		public GroupBy (QueryBlock block, Expression elementSelector, QueryBlock elementBlock, Expression keySelector, Location loc)
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

		protected override void CreateArguments (ResolveContext ec, Parameter parameter, ref Arguments args)
		{
			base.CreateArguments (ec, parameter, ref args);

			if (element_selector != null) {
				LambdaExpression lambda = new LambdaExpression (element_selector.Location);

				element_block.SetParameter (parameter.Clone ());
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
				t.element_block = (QueryBlock) element_block.Clone (clonectx);
			}

			base.CloneTo (clonectx, t);
		}

		protected override string MethodName {
			get { return "GroupBy"; }
		}
	}

	public class Join : SelectMany
	{
		QueryBlock inner_selector, outer_selector;

		public Join (QueryBlock block, SimpleMemberName lt, Expression inner, QueryBlock outerSelector, QueryBlock innerSelector, Location loc)
			: base (block, lt, inner, loc)
		{
			this.outer_selector = outerSelector;
			this.inner_selector = innerSelector;
		}

		protected override void CreateArguments (ResolveContext ec, Parameter parameter, ref Arguments args)
		{
			args = new Arguments (4);

			if (IdentifierType != null)
				expr = CreateCastExpression (expr);

			args.Add (new Argument (expr));

			outer_selector.SetParameter (parameter.Clone ());
			var lambda = new LambdaExpression (outer_selector.StartLocation);
			lambda.Block = outer_selector;
			args.Add (new Argument (lambda));

			inner_selector.SetParameter (new ImplicitLambdaParameter (range_variable.Value, range_variable.Location));
			lambda = new LambdaExpression (inner_selector.StartLocation);
			lambda.Block = inner_selector;
			args.Add (new Argument (lambda));

			base.CreateArguments (ec, parameter, ref args);
		}

		protected override void CloneTo (CloneContext clonectx, Expression target)
		{
			Join t = (Join) target;
			t.inner_selector = (QueryBlock) inner_selector.Clone (clonectx);
			t.outer_selector = (QueryBlock) outer_selector.Clone (clonectx);
			base.CloneTo (clonectx, t);
		}	

		protected override string MethodName {
			get { return "Join"; }
		}
	}

	public class GroupJoin : Join
	{
		readonly SimpleMemberName into;

		public GroupJoin (QueryBlock block, SimpleMemberName lt, Expression inner,
			QueryBlock outerSelector, QueryBlock innerSelector, SimpleMemberName into, Location loc)
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

	public class Let : ARangeVariableQueryClause
	{
		public Let (QueryBlock block, SimpleMemberName identifier, Expression expr, Location loc)
			: base (block, identifier, expr, loc)
		{
		}

		protected override void CreateArguments (ResolveContext ec, Parameter parameter, ref Arguments args)
		{
			expr = CreateRangeVariableType (ec, parameter, range_variable, expr);
			base.CreateArguments (ec, parameter, ref args);
		}

		protected override string MethodName {
			get { return "Select"; }
		}
	}

	public class Select : AQueryClause
	{
		public Select (QueryBlock block, Expression expr, Location loc)
			: base (block, expr, loc)
		{
		}
		
		//
		// For queries like `from a orderby a select a'
		// the projection is transparent and select clause can be safely removed 
		//
		public bool IsRequired (Parameter parameter)
		{
			SimpleName sn = expr as SimpleName;
			if (sn == null)
				return true;

			return sn.Name != parameter.Name;
		}

		protected override string MethodName {
			get { return "Select"; }
		}
	}

	public class SelectMany : ARangeVariableQueryClause
	{
		public SelectMany (QueryBlock block, SimpleMemberName identifier, Expression expr, Location loc)
			: base (block, identifier, expr, loc)
		{
		}

		protected override void CreateArguments (ResolveContext ec, Parameter parameter, ref Arguments args)
		{
			if (args == null) {
				if (IdentifierType != null)
					expr = CreateCastExpression (expr);

				base.CreateArguments (ec, parameter, ref args);
			}

			Expression result_selector_expr;
			QueryBlock result_block;

			var target = GetIntoVariable ();
			var target_param = new ImplicitLambdaParameter (target.Value, target.Location);

			//
			// When select follows use it as a result selector
			//
			if (next is Select) {
				result_selector_expr = next.Expr;

				result_block = next.block;
				result_block.SetParameters (parameter, target_param);

				next = next.next;
			} else {
				result_selector_expr = CreateRangeVariableType (ec, parameter, target, new SimpleName (target.Value, target.Location));

				result_block = new QueryBlock (ec.Compiler, block.Parent, block.StartLocation);
				result_block.SetParameters (parameter, target_param);
			}

			LambdaExpression result_selector = new LambdaExpression (Location);
			result_selector.Block = result_block;
			result_selector.Block.AddStatement (new ContextualReturn (result_selector_expr));

			args.Add (new Argument (result_selector));
		}

		protected override string MethodName {
			get { return "SelectMany"; }
		}
	}

	public class Where : AQueryClause
	{
		public Where (QueryBlock block, BooleanExpression expr, Location loc)
			: base (block, expr, loc)
		{
		}

		protected override string MethodName {
			get { return "Where"; }
		}

		protected override void CreateArguments (ResolveContext ec, Parameter parameter, ref Arguments args)
		{
			base.CreateArguments (ec, parameter, ref args);
		}
	}

	public class OrderByAscending : AQueryClause
	{
		public OrderByAscending (QueryBlock block, Expression expr)
			: base (block, expr, expr.Location)
		{
		}

		protected override string MethodName {
			get { return "OrderBy"; }
		}
	}

	public class OrderByDescending : AQueryClause
	{
		public OrderByDescending (QueryBlock block, Expression expr)
			: base (block, expr, expr.Location)
		{
		}

		protected override string MethodName {
			get { return "OrderByDescending"; }
		}
	}

	public class ThenByAscending : OrderByAscending
	{
		public ThenByAscending (QueryBlock block, Expression expr)
			: base (block, expr)
		{
		}

		protected override string MethodName {
			get { return "ThenBy"; }
		}
	}

	public class ThenByDescending : OrderByDescending
	{
		public ThenByDescending (QueryBlock block, Expression expr)
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
	public class QueryBlock : ToplevelBlock
	{
		//
		// Transparent parameters are used to package up the intermediate results
		// and pass them onto next clause
		//
		public sealed class TransparentParameter : ImplicitLambdaParameter
		{
			public static int Counter;
			const string ParameterNamePrefix = "<>__TranspIdent";

			public readonly Parameter Parent;
			public readonly string Identifier;

			public TransparentParameter (Parameter parent, SimpleMemberName identifier)
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

		sealed class RangeVariable : IKnownVariable
		{
			public RangeVariable (QueryBlock block, Location loc)
			{
				Block = block;
				Location = loc;
			}

			public Block Block { get; private set; }

			public Location Location { get; private set; }
		}

		List<SimpleMemberName> range_variables;

		public QueryBlock (CompilerContext ctx, Block parent, Location start)
			: base (ctx, parent, ParametersCompiled.EmptyReadOnlyParameters, start)
		{
		}

		public void AddRangeVariable (SimpleMemberName name)
		{
			if (!CheckParentConflictName (this, name.Value, name.Location))
				return;

			if (range_variables == null)
				range_variables = new List<SimpleMemberName> ();

			range_variables.Add (name);
			AddKnownVariable (name.Value, new RangeVariable (this, name.Location));
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

				TransparentParameter tp_next = tp.Parent as TransparentParameter;
				if (tp_next == null && tp.Parent.Name == name)
					break;

				tp = tp_next;
			}

			if (tp != null) {
				expr = new SimpleName (parameters[0].Name, loc);
				TransparentParameter tp_cursor = (TransparentParameter) parameters[0];
				while (tp_cursor != tp) {
					tp_cursor = (TransparentParameter) tp_cursor.Parent;
					expr = new TransparentMemberAccess (expr, tp_cursor.Name);
				}

				return new TransparentMemberAccess (expr, name);
			}

			return null;
		}

		protected override bool HasParameterWithName (string name)
		{
			return range_variables != null && range_variables.Exists (l => l.Value == name);
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
		
		public override void Error_AlreadyDeclaredTypeParameter (Location loc, string name, string conflict)
		{
			Report.Error (1948, loc, "A range variable `{0}' conflicts with a method type parameter",
				name);
		}

		public void SetParameter (Parameter parameter)
		{
			base.parameters = new ParametersCompiled (null, parameter);
			base.parameter_info = new ToplevelParameterInfo [] {
				new ToplevelParameterInfo (this, 0)
			};
		}

		public void SetParameters (Parameter first, Parameter second)
		{
			base.parameters = new ParametersCompiled (null, first, second);
			base.parameter_info = new ToplevelParameterInfo[] {
				new ToplevelParameterInfo (this, 0),
				new ToplevelParameterInfo (this, 1)
			};
		}
	}

	sealed class TransparentMemberAccess : MemberAccess
	{
		public TransparentMemberAccess (Expression expr, string name)
			: base (expr, name)
		{
		}
	}
}
