//
// complete.cs: Expression that are used for completion suggestions.
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Marek Safar (marek.safar@gmail.com)
//
// Copyright 2001, 2002, 2003 Ximian, Inc.
// Copyright 2003-2009 Novell, Inc.
//
// Completion* classes derive from ExpressionStatement as this allows
// them to pass through the parser in many conditions that require
// statements even when the expression is incomplete (for example
// completing inside a lambda
//
namespace Mono.CSharp {
	using System;
	using System.Collections;
	using System.Reflection;
	using System.Reflection.Emit;
	using System.Text;

	//
	// A common base class for Completing expressions, it
	// is just a very simple ExpressionStatement
	//
	public abstract class CompletingExpression : ExpressionStatement {
		public override void EmitStatement (EmitContext ec)
		{
			// Do nothing
		}

		public override void Emit (EmitContext ec)
		{
			// Do nothing
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			return null;
		}
	}
	
	public class CompletionSimpleName : CompletingExpression {
		public string Prefix;
		
		public CompletionSimpleName (string prefix, Location l)
		{
			this.loc = l;
			this.Prefix = prefix;
		}

		public static void AppendResults (ArrayList results, string prefix, IEnumerable names)
		{
			foreach (string name in names){
				if (name == null || prefix == null)
					continue;

				if (!name.StartsWith (prefix))
					continue;

				if (results.Contains (name))
					continue;

				if (prefix != null)
					results.Add (name.Substring (prefix.Length));
				else
					results.Add (name);
			}

		}
		
		public override Expression DoResolve (ResolveContext ec)
		{
			ArrayList results = new ArrayList ();

			AppendResults (results, Prefix, Evaluator.GetVarNames ());
			AppendResults (results, Prefix, ec.CurrentTypeDefinition.NamespaceEntry.CompletionGetTypesStartingWith (Prefix));
			AppendResults (results, Prefix, Evaluator.GetUsingList ());
			
			throw new CompletionResult (Prefix, (string []) results.ToArray (typeof (string)));
		}

		protected override void CloneTo (CloneContext clonectx, Expression t)
		{
			// Nothing
		}
	}
	
	public class CompletionMemberAccess : CompletingExpression {
		Expression expr;
		string partial_name;
		TypeArguments targs;

		internal static MemberFilter CollectingFilter = new MemberFilter (Match);

		static bool Match (MemberInfo m, object filter_criteria)
		{
			if (m is FieldInfo){
				if (((FieldInfo) m).IsSpecialName)
					return false;
				
			}
			if (m is MethodInfo){
				if (((MethodInfo) m).IsSpecialName)
					return false;
			}

			if (filter_criteria == null)
				return true;
			
			string n = (string) filter_criteria;
			if (m.Name.StartsWith (n))
				return true;
			
			return false;
		}
		
		public CompletionMemberAccess (Expression e, string partial_name, Location l)
		{
			this.expr = e;
			this.loc = l;
			this.partial_name = partial_name;
		}

		public CompletionMemberAccess (Expression e, string partial_name, TypeArguments targs, Location l)
		{
			this.expr = e;
			this.loc = l;
			this.partial_name = partial_name;
			this.targs = targs;
		}
		
		public override Expression DoResolve (ResolveContext ec)
		{
			Expression expr_resolved = expr.Resolve (ec,
				ResolveFlags.VariableOrValue | ResolveFlags.Type |
				ResolveFlags.Intermediate | ResolveFlags.DisableStructFlowAnalysis);

			if (expr_resolved == null)
				return null;

			Type expr_type = expr_resolved.Type;
			if (expr_type.IsPointer || expr_type == TypeManager.void_type || expr_type == TypeManager.null_type || expr_type == InternalType.AnonymousMethod) {
				Unary.Error_OperatorCannotBeApplied (ec, loc, ".", expr_type);
				return null;
			}

			if (targs != null) {
				if (!targs.Resolve (ec))
					return null;
			}

			ArrayList results = new ArrayList ();
			if (expr_resolved is Namespace){
				Namespace nexpr = expr_resolved as Namespace;
				string namespaced_partial;

				if (partial_name == null)
					namespaced_partial = nexpr.Name;
				else
					namespaced_partial = nexpr.Name + "." + partial_name;

#if false
				Console.WriteLine ("Workign with: namespaced partial {0}", namespaced_partial);
				foreach (var x in ec.TypeContainer.NamespaceEntry.CompletionGetTypesStartingWith (ec.TypeContainer, namespaced_partial)){
					Console.WriteLine ("    {0}", x);
				}
#endif

				CompletionSimpleName.AppendResults (
					results,
					partial_name, 
					ec.CurrentTypeDefinition.NamespaceEntry.CompletionGetTypesStartingWith (namespaced_partial));
			} else {
				MemberInfo [] result = expr_type.FindMembers (
					MemberTypes.All, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public,
					CollectingFilter, partial_name);

				foreach (MemberInfo r in result){
					string name;
					
					MethodBase rasb = r as MethodBase;
					if (rasb != null && rasb.IsSpecialName)
						continue;
					
					if (partial_name == null)
						name = r.Name;
					else 
						name = r.Name.Substring (partial_name.Length);
					
					if (results.Contains (name))
						continue;
					results.Add (name);
				}
			}

			throw new CompletionResult (partial_name == null ? "" : partial_name, (string []) results.ToArray (typeof (string)));
		}

		protected override void CloneTo (CloneContext clonectx, Expression t)
		{
			CompletionMemberAccess target = (CompletionMemberAccess) t;

			if (targs != null)
				target.targs = targs.Clone ();

			target.expr = expr.Clone (clonectx);
		}
	}

	public class CompletionElementInitializer : CompletingExpression {
		string partial_name;
		
		public CompletionElementInitializer (string partial_name, Location l)
		{
			this.partial_name = partial_name;
			this.loc = l;
		}
		
		public override Expression DoResolve (ResolveContext ec)
		{
			MemberList members = TypeManager.FindMembers (
				ec.CurrentInitializerVariable.Type,
				MemberTypes.Field | MemberTypes.Property,
				BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public,
				CompletionMemberAccess.CollectingFilter, partial_name);

			string [] result = new string [members.Count];
			int i = 0;
			foreach (MemberInfo mi in members){
				string name;
				
				if (partial_name == null)
					name = mi.Name;
				else
					name = mi.Name.Substring (partial_name.Length);
				
				result [i++] = name;
			}

			if (partial_name != null && i > 0 && result [0] == "")
				throw new CompletionResult ("", new string [] { "=" });
			
			throw new CompletionResult (partial_name == null ? "" : partial_name, result);
		}

		protected override void CloneTo (CloneContext clonectx, Expression t)
		{
			// Nothing
		}
	}
}
