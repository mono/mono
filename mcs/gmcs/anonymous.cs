//
// anonymous.cs: Support for anonymous methods
//
// Author:
//   Miguel de Icaza (miguel@ximain.com)
//
// (C) 2003, 2004 Novell, Inc.
//
// TODO: Ideally, we should have the helper classes emited as a hierarchy to map
// their nesting, and have the visibility set to private, instead of NestedAssembly
//
//
//

using System;
using System.Text;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;

namespace Mono.CSharp {

	public abstract class CompilerGeneratedClass : Class
	{
		GenericMethod generic_method;
		static int next_index = 0;

		private static MemberName MakeProxyName (GenericMethod generic, Location loc)
		{
			string name = String.Format ("<>c__CompilerGenerated{0}", ++next_index);
			if (generic != null) {
				TypeArguments args = new TypeArguments (loc);
				foreach (TypeParameter tparam in generic.CurrentTypeParameters)
					args.Add (new SimpleName (tparam.Name, loc));
				return new MemberName (name, args, loc);
			} else
				return new MemberName (name, loc);
		}

		protected CompilerGeneratedClass (DeclSpace parent, GenericMethod generic,
						  int mod, Location loc)
			: base (parent.NamespaceEntry, parent,
				MakeProxyName (generic, loc), mod, null)
		{
			this.generic_method = generic;

			if (generic != null) {
				ArrayList list = new ArrayList ();
				foreach (TypeParameter tparam in generic.CurrentTypeParameters) {
					if (tparam.Constraints != null)
						list.Add (tparam.Constraints.Clone ());
				}
				SetParameterInfo (list);
			}
		}

		protected override bool DefineNestedTypes ()
		{
			Report.Debug (64, "COMPILER GENERATED NESTED", this, Name, Parent);

			RootContext.RegisterCompilerGeneratedType (TypeBuilder);
			return base.DefineNestedTypes ();
		}

		public GenericMethod GenericMethod {
			get { return generic_method; }
		}

		public abstract ConstructorInfo Constructor {
			get;
		}

		public TypeExpr InflateType (Type it)
		{
			if (generic_method == null)
				return new TypeExpression (it, Location);

			if (it.IsGenericParameter && (it.DeclaringMethod != null)) {
				int pos = it.GenericParameterPosition;
				it = CurrentTypeParameters [pos].Type;
			} else if (it.IsGenericType) {
				Type[] args = it.GetGenericArguments ();

				TypeArguments inflated = new TypeArguments (Location);
				foreach (Type t in args)
					inflated.Add (InflateType (t));

				return new ConstructedType (it, inflated, Location);
			} else if (it.IsArray) {
				TypeExpr et_expr = InflateType (it.GetElementType ());
				int rank = it.GetArrayRank ();

				Type et = et_expr.ResolveAsTypeTerminal (this, false).Type;
				it = et.MakeArrayType (rank);
			}

			return new TypeExpression (it, Location);
		}

		Type parent_type;
		CapturedVariable parent_link;

		public Type ParentType {
			get { return parent_type; }
		}

		public Field ParentLink {
			get { return parent_link; }
		}

		protected override bool DoDefineMembers ()
		{
			Report.Debug (64, "DO DEFINE MEMBERS", this, Name, Parent);

			CompilerGeneratedClass parent = Parent as CompilerGeneratedClass;
			if (parent != null) {
				parent_type = parent.CurrentType != null ?
					parent.CurrentType : parent.TypeBuilder;

				parent_link = new CapturedVariable (this, "<>parent", parent_type);

				Parameter[] ctor_params = new Parameter [1];
				ctor_params [0] = new Parameter (
					parent_type, "parent", Parameter.Modifier.NONE,
					null, Location);

				Constructor ctor = new Constructor (
					this, MemberName.Name, Modifiers.PUBLIC,
					new Parameters (ctor_params),
					new GeneratedBaseInitializer (Location),
					Location);
				AddConstructor (ctor);

				ctor.Block = new ToplevelBlock (null, Location);
				ctor.Block.AddStatement (new TheCtor (parent_link));
			}

			return base.DoDefineMembers ();
		}

		protected class TheCtor : Statement
		{
			Field parent;

			public TheCtor (Field parent)
			{
				this.parent = parent;
			}

			public override bool Resolve (EmitContext ec)
			{
				return true;
			}

			protected override void DoEmit (EmitContext ec)
			{
				ec.ig.Emit (OpCodes.Ldarg_0);
				ec.ig.Emit (OpCodes.Ldarg_1);
				ec.ig.Emit (OpCodes.Stfld, parent.FieldBuilder);
			}
		}

		protected class CapturedVariable : Field
		{
			public CapturedVariable (CompilerGeneratedClass helper, string name, Type type)
				: base (helper, new TypeExpression (type, helper.Location),
					Modifiers.INTERNAL, name, null, helper.Location)
			{
				helper.AddField (this);
			}
		}
	}

	public interface IAnonymousContainer
	{
		bool IsIterator {
			get;
		}

		CompilerGeneratedClass CreateScopeType (ScopeInfo scope);
	}

	public class AnonymousMethodExpression : Expression, IAnonymousContainer
	{
		public readonly AnonymousMethodExpression Parent;
		public readonly GenericMethod GenericMethod;
		public readonly TypeContainer Host;
		public readonly Parameters Parameters;
		public readonly ToplevelBlock Container;

		public ToplevelBlock Block;
		protected AnonymousMethod anonymous;

		public AnonymousMethod AnonymousMethod {
			get { return anonymous; }
		}

		public AnonymousMethodExpression (AnonymousMethodExpression parent,
						  GenericMethod generic, TypeContainer host,
						  Parameters parameters, ToplevelBlock container,
						  Location loc)
		{
			this.Parent = parent;
			this.GenericMethod = generic;
			this.Host = host;
			this.Parameters = parameters;
			this.Container = container;
			this.loc = loc;

			Report.Debug (64, "NEW ANONYMOUS METHOD EXPRESSION", this, parent, host,
				      container, loc);

			if (container != null)
				container.SetHaveAnonymousMethods (loc, this);
		}

		public override string ExprClassName {
			get {
				return "anonymous method";
			}
		}

		void Error_ParameterMismatch (Type t)
		{
			Report.Error (1661, loc, "Anonymous method could not be converted to delegate `" +
				      "{0}' since there is a parameter mismatch",
				      TypeManager.CSharpName (t));
		}

		public bool ImplicitStandardConversionExists (Type delegate_type)
		{
			if (Parameters == null)
				return true;

			MethodGroupExpr invoke_mg = Delegate.GetInvokeMethod (
				Host.TypeBuilder, delegate_type, loc);
			MethodInfo invoke_mb = (MethodInfo) invoke_mg.Methods [0];
			ParameterData invoke_pd = TypeManager.GetParameterData (invoke_mb);

			if (Parameters.Count != invoke_pd.Count)
				return false;

			for (int i = 0; i < Parameters.Count; ++i) {
				if (invoke_pd.ParameterType (i) != Parameters.ParameterType (i))
					return false;
			}
			return true;
		}

		//
		// Returns true if this anonymous method can be implicitly
		// converted to the delegate type `delegate_type'
		//
		public Expression Compatible (EmitContext ec, Type delegate_type)
		{
			if (anonymous != null)
				return anonymous.AnonymousDelegate;

			//
			// At this point its the first time we know the return type that is 
			// needed for the anonymous method.  We create the method here.
			//

			MethodGroupExpr invoke_mg = Delegate.GetInvokeMethod (
				ec.ContainerType, delegate_type, loc);
			MethodInfo invoke_mb = (MethodInfo) invoke_mg.Methods [0];
			ParameterData invoke_pd = TypeManager.GetParameterData (invoke_mb);

			Parameters parameters;
			if (Parameters == null) {
				//
				// We provide a set of inaccessible parameters
				//
				Parameter [] fixedpars = new Parameter [invoke_pd.Count];
								
				for (int i = 0; i < invoke_pd.Count; i++){
					fixedpars [i] = new Parameter (
						invoke_pd.ParameterType (i),
						"+" + i, invoke_pd.ParameterModifier (i), null, loc);
				}
								
				parameters = new Parameters (fixedpars);
				if (!parameters.Resolve (ec))
					return null;
			} else {
				if (Parameters.Count != invoke_pd.Count) {
					Report.SymbolRelatedToPreviousError (delegate_type);
					Report.Error (1593, loc, "Delegate `{0}' does not take `{1}' arguments",
						TypeManager.CSharpName (delegate_type), Parameters.Count.ToString ());
					Error_ParameterMismatch (delegate_type);
					return null;
				}

				for (int i = 0; i < Parameters.Count; ++i) {
					Parameter.Modifier p_mod = invoke_pd.ParameterModifier (i);
					if (Parameters.ParameterModifier (i) != p_mod && p_mod != Parameter.Modifier.PARAMS) {
						if (p_mod == Parameter.Modifier.NONE)
							Report.Error (1677, loc, "Parameter `{0}' should not be declared with the `{1}' keyword",
								(i + 1).ToString (), Parameter.GetModifierSignature (Parameters.ParameterModifier (i)));
						else
							Report.Error (1676, loc, "Parameter `{0}' must be declared with the `{1}' keyword",
								(i+1).ToString (), Parameter.GetModifierSignature (p_mod));
						Error_ParameterMismatch (delegate_type);
						return null;
					}

					if (invoke_pd.ParameterType (i) != Parameters.ParameterType (i)) {
						Report.Error (1678, loc, "Parameter `{0}' is declared as type `{1}' but should be `{2}'",
							(i+1).ToString (),
							TypeManager.CSharpName (Parameters.ParameterType (i)),
							TypeManager.CSharpName (invoke_pd.ParameterType (i)));
						Error_ParameterMismatch (delegate_type);
						return null;
					}
				}

				parameters = Parameters;
			}

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
				      delegate_type.IsGenericType, loc);

			anonymous = new AnonymousMethod (
				Parent != null ? Parent.AnonymousMethod : null, GenericMethod,
				Host, parameters, Container, Block, invoke_mb.ReturnType,
				delegate_type, loc);

			if (!anonymous.Resolve (ec))
				return null;

			return anonymous.AnonymousDelegate;
		}

		public override Expression DoResolve (EmitContext ec)
		{
			//
			// Set class type, set type
			//

			eclass = ExprClass.Value;

			//
			// This hack means `The type is not accessible
			// anywhere', we depend on special conversion
			// rules.
			// 
			type = TypeManager.anonymous_method_type;

			if ((Parameters != null) && !Parameters.Resolve (ec))
				return null;

			return this;
		}

		public override void Emit (EmitContext ec)
		{
			// nothing, as we only exist to not do anything.
		}

		bool IAnonymousContainer.IsIterator {
			get { return false; }
		}

		CompilerGeneratedClass IAnonymousContainer.CreateScopeType (ScopeInfo scope)
		{
			return anonymous.CreateScopeType (scope);
		}
	}

	public abstract class AnonymousContainer : CompilerGeneratedClass, IAnonymousContainer
	{
		// Used to generate unique method names.
		protected static int anonymous_method_count;

		new public readonly TypeContainer Parent;

		// An array list of AnonymousMethodParameter or null
		public Parameters Parameters;

		//
		// The block that makes up the body for the anonymous mehtod
		//
		public ToplevelBlock Block;

		//
		// The implicit method we create
		//
		public Method method;

		// The emit context for the anonymous method
		protected bool unreachable;
		protected readonly Location loc;

		// The method scope
		ScopeInfo method_scope;
		bool computed_method_scope = false;
		
		//
		// The modifiers applied to the method, we aggregate them
		//
		protected int method_modifiers = Modifiers.PRIVATE;
		
		//
		// Track the scopes that this method has used.  At the
		// end this is used to determine the ScopeInfo that will
		// host the method
		//
		ArrayList scopes_used = new ArrayList ();
		
		//
		// Points to our container anonymous method if its present
		//
		public readonly AnonymousContainer ContainerAnonymousMethod;

		protected AnonymousContainer (AnonymousContainer parent, GenericMethod generic,
					      TypeContainer host, Parameters parameters,
					      ToplevelBlock container, ToplevelBlock block,
					      int mod, Location loc)
			: base (parent != null ? parent : host, parent != null ? null : generic,
				mod, loc)
		{
			this.ContainerAnonymousMethod = parent;
			this.Parent = (parent != null) ? parent : host;
			this.Parameters = parameters;
			this.Block = block;
			this.loc = loc;

			Report.Debug (64, "NEW ANONYMOUS CONTAINER", this, parent, host, host.Name,
				      container, parameters);

			Parent.AddCompilerGeneratedClass (this);

			if (container == null)
				return;

			container.RegisterAnonymousMethod (this);
		}

		protected AnonymousContainer (TypeContainer host, GenericMethod generic,
					      Parameters parameters, ToplevelBlock container,
					      int mod, Location loc)
			: this (null, generic, host, parameters, container, null, mod, loc)
		{
			Block = new ToplevelBlock (container, parameters, loc);
			Block.SetHaveAnonymousMethods (loc, this);
		}

		public void RegisterScope (ScopeInfo scope)
		{
			if (scopes_used.Contains (scope))
				return;
			scopes_used.Add (scope);
		}

		// Returns the deepest of two scopes
		ScopeInfo Deepest (ScopeInfo a, ScopeInfo b)
		{
			ScopeInfo p;

			if (a == null)
				return b;
			if (b == null)
				return a;
			if (a == b)
				return a;

			//
			// If they Scopes are on the same CaptureContext, we do the double
			// checks just so if there is an invariant change in the future,
			// we get the exception at the end
			//
			for (p = a; p != null; p = p.ParentScope)
				if (p == b)
					return a;
			
			for (p = b; p != null; p = p.ParentScope)
				if (p == a)
					return b;

			CaptureContext ca = a.CaptureContext;
			CaptureContext cb = b.CaptureContext;

			for (CaptureContext c = ca; c != null; c = c.ParentCaptureContext)
				if (c == cb)
					return a;

			for (CaptureContext c = cb; c != null; c = c.ParentCaptureContext)
				if (c == ca)
					return b;
			throw new Exception ("Should never be reached");
		}

		//
		// Determines the proper host for a method considering the
		// scopes it references
		//
		public void ComputeMethodHost ()
		{
			Report.Debug (64, "COMPUTE METHOD HOST", this, computed_method_scope,
				      method_scope, scopes_used);

			if (computed_method_scope)
				return;
			
			method_scope = null;
			int top = scopes_used.Count;
			computed_method_scope = true;

			if (top == 0)
				return;
			
			method_scope = (ScopeInfo) scopes_used [0];
			if (top == 1)
				return;
			
			for (int i = 1; i < top; i++)
				method_scope = Deepest (method_scope, (ScopeInfo) scopes_used [i]);
		}

		public ScopeInfo Scope {
			get {
				if (computed_method_scope)
					return method_scope;

				//
				// This means that ComputeMethodHost is not being called, most
				// likely by someone who overwrote the CreateMethodHost method
				//
				throw new Exception ("Internal error, AnonymousContainer.Scope is being used before its container is computed");
			}
		}

#if FIXME
		public bool CreateMethod (EmitContext ec)
		{
			if (method != null)
				return true;

			return CreateMethodHost (ec);
		}
#endif

		public abstract bool Resolve (EmitContext ec);

		protected abstract bool CreateMethodHost (EmitContext ec);

		public abstract CompilerGeneratedClass CreateScopeType (ScopeInfo scope);

		public abstract bool IsIterator {
			get;
		}

		public override ConstructorInfo Constructor {
			get { throw new InternalErrorException (); }
		}

		public ExpressionStatement GetScopeInitializer (Location loc)
		{
			return new AnonymousMethodScopeInitializer (this, loc);
		}

		public override string ToString ()
		{
			return String.Format ("{0} ({1})", GetType (), Name);
		}

		protected class AnonymousMethodScopeInitializer : ExpressionStatement
		{
			AnonymousContainer am;

			public AnonymousMethodScopeInitializer (AnonymousContainer am, Location loc)
			{
				this.am = am;
				this.loc = loc;
				eclass = ExprClass.Value;
			}

			public override Expression DoResolve (EmitContext ec)
			{
				ScopeInfo scope = am.Scope;
				if (scope == null) {
					type = am.Parent.TypeBuilder;
					return this;
				}

				AnonymousContainer container = am.ContainerAnonymousMethod;
				if ((container != null) && (scope == container.Scope)) {
					type = scope.ScopeType;
					return this;
				}

				return scope.GetScopeInitializer (ec);
			}

			public override void Emit (EmitContext ec)
			{
				ec.ig.Emit (OpCodes.Nop);

				if (ec.IsStatic)
					ec.ig.Emit (OpCodes.Ldnull);
				else
					ec.ig.Emit (OpCodes.Ldarg_0);
			}

			public override void EmitStatement (EmitContext ec)
			{
				Emit (ec);
			}
		}
	}

	public class AnonymousMethod : AnonymousContainer
	{
		public readonly Type ReturnType;
		public readonly Type DelegateType;

		public EmitContext aec;

		//
		// The value return by the Compatible call, this ensure that
		// the code works even if invoked more than once (Resolve called
		// more than once, due to the way Convert.ImplicitConversion works
		//
		Expression anonymous_delegate;

		public AnonymousMethod (AnonymousMethod parent, GenericMethod generic,
					TypeContainer host, Parameters parameters,
					ToplevelBlock container, ToplevelBlock block,
					Type return_type, Type delegate_type, Location loc)
			: base (parent, generic, host, parameters, container, block, 0, loc)
		{
			this.ReturnType = return_type;
			this.DelegateType = delegate_type;
		}

		public Expression AnonymousDelegate {
			get { return anonymous_delegate; }
		}

		public override bool IsIterator {
			get { return false; }
		}

		//
		// Creates the host for the anonymous method
		//
		protected override bool CreateMethodHost (EmitContext ec)
		{
			ComputeMethodHost ();

			if (method != null)
				return true;

			Report.Debug (64, "CREATE METHOD HOST", this, Scope);

			if ((Scope != null) && (Scope.HelperClass == null))
				throw new InternalErrorException ();

			method_modifiers = (Scope != null) ? Modifiers.INTERNAL : Modifiers.PRIVATE;

			if ((Scope == null) && ec.IsStatic)
				method_modifiers |= Modifiers.STATIC;

			string name = "<>c__AnonymousMethod" + anonymous_method_count++;
			MemberName member_name;

			Report.Debug (64, "CREATE METHOD HOST #1",
				      this, Scope, Scope.HelperClass.Name, name);

			GenericMethod generic_method = null;
			if (DelegateType.IsGenericType) {
				TypeArguments args = new TypeArguments (loc);

				Type[] tparam = TypeManager.GetTypeArguments (DelegateType);
				for (int i = 0; i < tparam.Length; i++)
					args.Add (new SimpleName (tparam [i].Name, loc));

				member_name = new MemberName (name, args, loc);

				generic_method = new GenericMethod (
					NamespaceEntry, this, member_name,
					new TypeExpression (ReturnType, loc), Parameters);

				generic_method.SetParameterInfo (null);
			} else
				member_name = new MemberName (name, loc);

			method = new Method (
				this, generic_method, new TypeExpression (ReturnType, loc),
				method_modifiers, false, member_name, Parameters, null);
			method.Block = Block;

			return method.Define ();
		}

		public override bool Resolve (EmitContext ec)
		{
			if (!ec.IsAnonymousMethodAllowed) {
				Report.Error (1706, loc,
					      "Anonymous methods are not allowed in the " +
					      "attribute declaration");
				return false;
			}

			Report.Debug (64, "RESOLVE ANONYMOUS METHOD", this, ec, aec, Parameters);

			if (DefineType () == null)
				return false;
			if (!ResolveType ())
				return false;

			aec = new EmitContext (
				ec.ResolveContext, ec.TypeContainer, this, loc, null, ReturnType,
				/* REVIEW */ (ec.InIterator ? Modifiers.METHOD_YIELDS : 0) |
				(ec.InUnsafe ? Modifiers.UNSAFE : 0) |
				(ec.IsStatic ? Modifiers.STATIC : 0),
				/* No constructor */ false);

			aec.CurrentAnonymousMethod = this;

			Report.Debug (64, "RESOLVE ANONYMOUS METHOD #1", this, ec, aec,
				      Parent, Parameters, Block, Block.CaptureContext);

			ScopeInfo root = ec.capture_context.CreateRootScope (this);
			RegisterScope (root);

#if FIXME
			ScopeInfo scope = Block.CaptureContext.CreateRootScope (this);
			RegisterScope (scope);

			if (ContainerAnonymousMethod != null) {
				ScopeInfo scope = Block.CaptureContext.CreateRootScope (this);
				ContainerAnonymousMethod.RegisterScope (scope);
			}
#endif

			bool unreachable;
			if (!aec.ResolveTopBlock (ec, Block, Parameters, null, out unreachable))
				return false;

			Report.Debug (64, "RESOLVE ANONYMOUS METHOD #3", this, ec, aec, Block,
				      ec.capture_context);

			if (!CreateMethodHost (ec))
				return false;

			anonymous_delegate = new AnonymousDelegate (this, DelegateType, loc).Resolve (ec);
			if (anonymous_delegate == null)
				return false;

			return true;
		}

		public MethodInfo GetMethodBuilder (EmitContext ec)
		{
			MethodInfo builder = method.MethodBuilder;
			if ((Scope == null) || !IsGeneric)
				return builder;

			Expression init = Scope.GetScopeInitializer (ec);
			MethodGroupExpr mg = (MethodGroupExpr) Expression.MemberLookup (
				ec.ContainerType, init.Type, builder.Name, loc);

			Report.Debug (64, "GET METHOD BUILDER", this, Scope, init, init.Type,
				      builder, mg, loc);

			if (mg == null)
				throw new InternalErrorException ();

			return (MethodInfo) mg.Methods [0];
		}

		public bool EmitMethod (EmitContext ec)
		{
			if (!CreateMethodHost (ec))
				return false;

			MethodBuilder builder = method.MethodBuilder;
			ILGenerator ig = builder.GetILGenerator ();
			aec.ig = ig;

			Parameters.ApplyAttributes (builder);

			Report.Debug (64, "ANONYMOUS EMIT METHOD", this, aec, Scope,
				      aec.capture_context);

			//
			// Adjust based on the computed state of the
			// method from CreateMethodHost
			
			aec.MethodIsStatic = (method_modifiers & Modifiers.STATIC) != 0;
			
			aec.EmitMeta (Block);
			aec.EmitResolvedTopBlock (Block, unreachable);
			return true;
		}

		public override CompilerGeneratedClass CreateScopeType (ScopeInfo scope)
		{
			Report.Debug (64, "ANONYMOUS METHOD CREATE SCOPE TYPE",
				      this, scope, scope.ParentScope);

			if (scope.ParentScope == null) {
				if (!Define ())
					return null;
				if (DefineType () == null)
					return null;
				return this;
			}

			AnonymousHelper helper = new AnonymousHelper (this);

			if (!helper.Define ())
				return null;

			if (helper.DefineType () == null)
				return null;

			AddCompilerGeneratedClass (helper);

			return helper;
		}

		public static void Error_AddressOfCapturedVar (string name, Location loc)
		{
			Report.Error (1686, loc,
				"Local variable `{0}' or its members cannot have their address taken and be used inside an anonymous method block",
				name);
		}

		protected class AnonymousHelper : CompilerGeneratedClass
		{
			ConstructorInfo ctor;

			public AnonymousHelper (AnonymousMethod anonymous)
				: base (anonymous, anonymous.GenericMethod,
					Modifiers.PUBLIC, anonymous.Location)
			{ }

			protected override bool DoDefineMembers ()
			{
				if (!base.DoDefineMembers ())
					return false;

				ctor = default_constructor.ConstructorBuilder;
				return true;
			}

			public override ConstructorInfo Constructor {
				get { return ctor; }
			}
		}
	}

	//
	// This will emit the code for the delegate, as well delegate creation on the host
	//
	public class AnonymousDelegate : DelegateCreation {
		AnonymousMethod am;

		public AnonymousDelegate (AnonymousMethod am, Type target_type, Location l)
		{
			type = target_type;
			loc = l;
			this.am = am;
		}

		public override Expression DoResolve (EmitContext ec)
		{
			eclass = ExprClass.Value;

			return this;
		}
		
		public override void Emit (EmitContext ec)
		{
			Report.Debug (64, "ANONYMOUS DELEGATE", this, am, ec.ContainerType, type,
				      ec, ec.capture_context, loc);

			if (!am.EmitMethod (ec))
				return;

			//
			// Now emit the delegate creation.
			//
			if ((am.method.ModFlags & Modifiers.STATIC) == 0) {
				delegate_instance_expression = am.GetScopeInitializer (loc).Resolve (ec);
				Report.Debug (64, "ANONYMOUS DELEGATE #0", this,
					      delegate_instance_expression);

				if (delegate_instance_expression == null)
					throw new InternalErrorException ();
			}

			Expression ml = Expression.MemberLookup (ec.ContainerType, type, ".ctor", loc);
			constructor_method = ((MethodGroupExpr) ml).Methods [0];
			delegate_method = am.GetMethodBuilder (ec);
			Report.Debug (64, "ANONYMOUS DELEGATE #1", constructor_method, delegate_method,
				      delegate_method.GetType ());
			base.Emit (ec);
		}
	}

	class CapturedVariable : Field
	{
		public CapturedVariable (ScopeInfo scope, string name, Type type)
			: base (scope.HelperClass, scope.HelperClass.InflateType (type),
				Modifiers.INTERNAL, name, null, scope.loc)
		{
			scope.HelperClass.AddField (this);
		}
	}

	class CapturedParameter {
		public readonly Parameter Parameter;
		public readonly Field Field;
		public readonly int Idx;

		public CapturedParameter (ScopeInfo scope, Parameter par, int idx)
		{
			this.Parameter = par;
			Idx = idx;
			Field = new CapturedVariable (scope, "<p:" + par.Name + ">", par.ParameterType);
		}
	}

	public class CapturedParameterReference : Expression, IAssignMethod, IMemoryLocation, IVariable
	{
		ScopeInfo scope;
		CapturedParameter cp;
		VariableInfo vi;
		bool prepared;

		internal CapturedParameterReference (ScopeInfo scope, CapturedParameter cp,
						     Location loc)
		{
			this.scope = scope;
			this.cp = cp;
			this.loc = loc;

			type = cp.Field.MemberType;
			eclass = ExprClass.Variable;
		}

		public VariableInfo VariableInfo {
			get { return vi; }
		}

		public override Expression DoResolve (EmitContext ec)
		{
			// We are born fully resolved.
			return this;
		}

		public override Expression DoResolveLValue (EmitContext ec, Expression right_side)
		{
			// We are born fully resolved.
			return this;
		}

		LocalTemporary temp;

		public override void Emit (EmitContext ec)
		{
			Emit (ec, false);
		}

		public void Emit (EmitContext ec, bool leave_copy)
		{
			scope.CaptureContext.EmitParameter (
				ec, cp.Parameter.Name, leave_copy, prepared, ref temp);
		}

		public void EmitAssign (EmitContext ec, Expression source, bool leave_copy,
					bool prepare_for_load)
		{
			prepared = prepare_for_load;
			scope.CaptureContext.EmitAssignParameter (
				ec, cp.Parameter.Name, source, leave_copy, prepare_for_load, ref temp);
		}

		public void AddressOf (EmitContext ec, AddressOp mode)
		{
			scope.CaptureContext.EmitAddressOfParameter (ec, cp.Parameter.Name);
		}

		public bool VerifyFixed ()
		{
			// A parameter is fixed if it's a value parameter (i.e., no modifier like out, ref, param).
			return cp.Parameter.ModFlags == Parameter.Modifier.NONE;
		}
	}

	//
	// Here we cluster all the variables captured on a given scope, we also
	// keep some extra information that might be required on each scope.
	//
	public class ScopeInfo {
		public CaptureContext CaptureContext;
		public ScopeInfo ParentScope;
		public Block ScopeBlock;
		public bool HostsParameters = false;
		
		// For tracking the number of scopes created.
		public int id;
		static int count;
		
		ArrayList locals = new ArrayList ();
		ArrayList children = new ArrayList ();

		//
		// The types and fields generated
		//
		public readonly Location loc;
		public TypeBuilder ScopeTypeBuilder;
		public Type ScopeType;
		public Field THIS;

		public CompilerGeneratedClass HelperClass;

		ExpressionStatement scope_instance;
		
		public ScopeInfo (CaptureContext cc, Block block)
		{
			CaptureContext = cc;
			ScopeBlock = block;
			loc = cc.loc;
			id = count++;

			Report.Debug (64, "NEW SCOPE", this, cc, block);
			Report.StackTrace ();

			cc.RegisterCaptureContext ();
		}

		public ScopeInfo (CaptureContext cc, Block block, CompilerGeneratedClass helper)
			: this (cc, block)
		{
			HelperClass = helper;
		}

		public Variable AddLocal (LocalInfo local)
		{
			CapturedLocal cl = new CapturedLocal (this, local);
			locals.Add (cl);
			return cl;
		}

		internal void AddChild (ScopeInfo si)
		{
			if (children.Contains (si))
				return;

			//
			// If any of the current children should be a children of `si', move them there
			//
			ArrayList move_queue = null;
			foreach (ScopeInfo child in children){
				if (child.ScopeBlock.IsChildOf (si.ScopeBlock)){
					if (move_queue == null)
						move_queue = new ArrayList ();
					move_queue.Add (child);
					child.ParentScope = si;
					si.AddChild (child);
				}
			}
			
			children.Add (si);

			if (move_queue != null){
				foreach (ScopeInfo child in move_queue){
					children.Remove (child);
				}
			}
		} 

		static int indent = 0;

		void Pad ()
		{
			for (int i = 0; i < indent; i++)
				Console.Write ("    ");
		}

		void EmitDebug ()
		{
			//Console.WriteLine (Environment.StackTrace);
			Pad ();
			Console.WriteLine ("START");
			indent++;
			foreach (LocalInfo li in locals){
				Pad ();
				Console.WriteLine ("var {0}", MakeFieldName (li.Name));
			}
			
			foreach (ScopeInfo si in children)
				si.EmitDebug ();
			indent--;
			Pad ();
			Console.WriteLine ("END");
		}

		protected string MakeFieldName (string local_name)
		{
			return "<" + id + ":" + local_name + ">";
		}

		bool resolved;

		public Field ParentLink {
			get { return HelperClass.ParentLink; }
		}

		public void CreateScopeType ()
		{
			Report.Debug (64, "CREATE SCOPE TYPE", this, HelperClass, ParentScope);

			if (HelperClass != null)
				return;

			if (ParentScope != null)
				ParentScope.CreateScopeType ();

			HelperClass = CaptureContext.Host.CreateScopeType (this);
			ScopeTypeBuilder = HelperClass.TypeBuilder;

			Report.Debug (64, "CREATE SCOPE TYPE #1", this, HelperClass, ParentScope,
				      ScopeTypeBuilder);
		}

		public void EmitScopeType (EmitContext ec)
		{
			// EmitDebug ();

			if (resolved)
				return;

			if (!(CaptureContext.Host.IsIterator))
				HelperClass.ResolveType ();

			ScopeType = HelperClass.IsGeneric ?
				HelperClass.CurrentType : HelperClass.TypeBuilder;

			Report.Debug (64, "EMIT SCOPE TYPE", this, HelperClass, CaptureContext,
				      CaptureContext.NeedThis, ParentScope, ScopeTypeBuilder,
				      ec, ec.DeclContainer, ec.DeclContainer.IsGeneric);

			if (ParentScope != null)
				ParentScope.EmitScopeType (ec);
			else if (CaptureContext.NeedThis) {
				Type container;
				if (ec.TypeContainer.CurrentType != null)
					container = ec.TypeContainer.CurrentType;
				else
					container = ec.TypeContainer.TypeBuilder;

				THIS = new CapturedVariable (this, "<>THIS", container);
			}

#if FIXME
			if (ParentScope != null){
				if (ParentScope.ScopeTypeBuilder == null){
					throw new InternalErrorException (
						"My parent has not been initialized " +
						"{0} and {1}", ParentScope, this);
				}

				if (ParentScope.ScopeTypeBuilder != ScopeTypeBuilder)
					ParentLink = new CapturedVariable (
						this, "<>parent", ParentScope.ScopeType);
			}
#endif

#if FIXME
			foreach (LocalInfo info in locals) {
				info.Field = new CapturedVariable (
					this, MakeFieldName (info.Name), info.VariableType);
			}

			if (HostsParameters){
				Hashtable captured_parameters = CaptureContext.captured_parameters;
				
				foreach (DictionaryEntry de in captured_parameters){
					string name = (string) de.Key;
					CapturedParameter cp = (CapturedParameter) de.Value;

					cp.Field = new CapturedVariable (
						this, "<p:" + name + ">", cp.Type);
				}
			}
#endif

			resolved = true;

			if (!(CaptureContext.Host.IsIterator))
				HelperClass.DefineMembers ();

			Report.Debug (64, "EMIT SCOPE TYPE #1", this, HelperClass, ParentScope,
				      ParentLink);

			foreach (ScopeInfo si in children)
				si.EmitScopeType (ec);
		}

		public ExpressionStatement GetScopeInitializer (EmitContext ec)
		{
			if (scope_instance == null) {
				scope_instance = new ScopeInitializer (this, loc);
				if (scope_instance.Resolve (ec) == null)
					throw new InternalErrorException ();
			}

			return scope_instance;
		}

		public void EmitScopeInstance (EmitContext ec)
		{
			if (CaptureContext.Host.IsIterator)
				ec.ig.Emit (OpCodes.Ldarg_0);
			else {
				if (scope_instance == null) {
					//
					// This is needed if someone overwrites the Emit method
					// of Statement and manually calls Block.Emit without
					// this snippet first:
					// 
					//   ec.EmitScopeInitFromBlock (The_Block);
					//   The_Block.Emit (ec);
					// 
					throw new InternalErrorException ();
				}
				scope_instance.Emit (ec);
			}
		}

		public static void CheckCycles (string msg, ScopeInfo s)
		{
			ArrayList l = new ArrayList ();
			int n = 0;
			
			for (ScopeInfo p = s; p != null; p = p.ParentScope,n++){
				if (l.Contains (p)){
					Console.WriteLine ("Loop detected {0} in {1}", n, msg);
					throw new Exception ();
				}
				l.Add (p);
			}
		}
		
		static void DoPath (StringBuilder sb, ScopeInfo start)
		{
			CheckCycles ("print", start);
			
			if (start.ParentScope != null){
				DoPath (sb, start.ParentScope);
				sb.Append (", ");
			}
			sb.Append ((start.id).ToString ());
		}
		
		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();
			
			sb.Append ("{");
			if (CaptureContext != null){
				sb.Append (CaptureContext.ToString ());
				sb.Append (":");
			}

			DoPath (sb, this);
			sb.Append ("}");

			return sb.ToString ();
		}

		protected class CapturedLocal : Variable {
			public readonly ScopeInfo Scope;
			public readonly LocalInfo Local;
			public readonly Field Field;

			public FieldExpr FieldInstance;

			public CapturedLocal (ScopeInfo scope, LocalInfo local)
			{
				this.Scope = scope;
				this.Local = local;
				Field = new CapturedVariable (
					scope, scope.MakeFieldName (local.Name), local.VariableType);
			}

			public override Type Type {
				get { return Field.MemberType; }
			}

			public override bool NeedsTemporary {
				get { return true; }
			}

			public override void EmitInstance (EmitContext ec)
			{
				ec.EmitCapturedVariableInstance (Local);
			}

			public override void Emit (EmitContext ec)
			{
				if (ec.capture_context == Scope.CaptureContext)
					ec.ig.Emit (OpCodes.Ldfld, FieldInstance.FieldInfo);
				else
					ec.ig.Emit (OpCodes.Ldfld, Field.FieldBuilder);
			}

			public override void EmitAssign (EmitContext ec)
			{
				if (ec.capture_context == Scope.CaptureContext)
					ec.ig.Emit (OpCodes.Stfld, FieldInstance.FieldInfo);
				else
					ec.ig.Emit (OpCodes.Stfld, Field.FieldBuilder);
			}

			public override void EmitAddressOf (EmitContext ec)
			{
				if (ec.capture_context == Scope.CaptureContext)
					ec.ig.Emit (OpCodes.Ldflda, FieldInstance.FieldInfo);
				else
					ec.ig.Emit (OpCodes.Ldflda, Field.FieldBuilder);
			}
		}

#if FIXME
		protected class CapturedLocalInstance : Variable
		{
			public readonly LocalInfo LocalInfo;

			public CapturedLocalInstance (LocalInfo local)
			{
				this.LocalInfo = local;
			}

			public override Type Type {
				get { return LocalInfo.TheField.Type; }
			}

			public override bool NeedsTemporary {
				get { return true; }
			}

			public override void EmitInstance (EmitContext ec)
			{
				ec.EmitCapturedVariableInstance (LocalInfo);
			}

			public override void Emit (EmitContext ec)
			{
				ec.ig.Emit (OpCodes.Ldfld, LocalInfo.TheField.FieldInfo);
			}

			public override void EmitAssign (EmitContext ec)
			{
				ec.ig.Emit (OpCodes.Stfld, LocalInfo.TheField.FieldInfo);
			}

			public override void EmitAddressOf (EmitContext ec)
			{
				ec.ig.Emit (OpCodes.Ldflda, LocalInfo.TheField.FieldInfo);
			}
		}
#endif

		protected sealed class ScopeInitializer : ExpressionStatement
		{
			ScopeInfo scope;
			TypeExpr scope_type;
			LocalTemporary scope_instance;
			Expression scope_ctor;

			FieldExpr this_field;
			Hashtable captured_params;

			ExpressionStatement parent_init;

			public ScopeInitializer (ScopeInfo scope, Location loc)
			{
				this.scope = scope;
				this.loc = loc;
				eclass = ExprClass.Value;
			}

			public LocalTemporary ScopeInstance {
				get { return scope_instance; }
			}

			public ScopeInfo Scope {
				get { return scope; }
			}

			public bool IsIterator {
				get { return scope.CaptureContext.Host.IsIterator; }
			}

			public override Expression DoResolve (EmitContext ec)
			{
				if (scope_type != null)
					return this;

				Scope.EmitScopeType (ec);

				if (Scope.HelperClass == null)
					throw new InternalErrorException (
						"HelperClass is null for " + Scope.ToString ());

				Report.Debug (64, "RESOLVE SCOPE INITIALIZER", this, Scope,
					      Scope.ParentScope, ec, ec.TypeContainer.Name,
					      ec.DeclContainer, ec.DeclContainer.IsGeneric);

				if (ec.DeclContainer.IsGeneric)
					scope_type = new ConstructedType (
						Scope.ScopeType, ec.DeclContainer.TypeParameters, loc);
				else
					scope_type = new TypeExpression (Scope.ScopeType, loc);

				scope_type = scope_type.ResolveAsTypeTerminal (ec, false);
				if ((scope_type == null) || (scope_type.Type == null))
					throw new InternalErrorException ();
				type = scope_type.Type;

				if (!IsIterator) {
					scope_instance = new LocalTemporary (type);
					ArrayList args = new ArrayList ();
					if (Scope.HelperClass.ParentType != null) {
						args.Add (new Argument (new SimpleThis (
							Scope.HelperClass.ParentType, loc)));
					}
					scope_ctor = new New (scope_type, args, loc).Resolve (ec);
					if (scope_ctor == null)
						throw new InternalErrorException ();
				}

				if (Scope.THIS != null) {
					this_field = (FieldExpr) Expression.MemberLookup (
						ec.ContainerType, type, "<>THIS", loc);
					Report.Debug (64, "RESOLVE SCOPE INITIALIZER #1", this, Scope, ec,
						      ec.ContainerType, type, this_field,
						      Scope.ParentScope);
					if (this_field == null)
						throw new InternalErrorException ();
				}

				if ((Scope.ParentScope != null) &&
				    (Scope.ParentScope.ScopeTypeBuilder != Scope.ScopeTypeBuilder)) {
					parent_init = Scope.ParentScope.GetScopeInitializer (ec);
				}

				foreach (CapturedLocal local in Scope.locals) {
					FieldExpr fe = (FieldExpr) Expression.MemberLookup (
						ec.ContainerType, type, local.Field.Name, loc);
					if (fe == null)
						throw new InternalErrorException ();

					fe.InstanceExpression = this;
					local.FieldInstance = fe;
				}

				//
				// Copy the parameter values, if any
				//
				int extra = ec.IsStatic ? 0 : 1;
				if (IsIterator)
					extra++;
				if (Scope.HostsParameters){
					Hashtable hash = Scope.CaptureContext.captured_parameters;
					captured_params = new Hashtable ();
					foreach (CapturedParameter cp in hash.Values) {
						FieldExpr fe = (FieldExpr) Expression.MemberLookup (
							ec.ContainerType, type, cp.Field.Name, loc);
						if (fe == null)
							throw new InternalErrorException ();

						captured_params.Add (cp.Idx + extra, fe);
					}
				}

				return this;
			}

			bool initialized;

			protected void EmitScopeInstance (EmitContext ec)
			{
				if (IsIterator)
					ec.ig.Emit (OpCodes.Ldarg_0);
				else {
					try {
						scope_instance.Emit (ec);
					} catch {
						Report.Debug (64, "EMIT SCOPE INSTANCE FUCK",
							      this, Scope, ec);
						Report.StackTrace ();

						ec.ig.Emit (OpCodes.Ldarg_0);

						ec.ig.Emit (OpCodes.Neg);
						ec.ig.Emit (OpCodes.Not);
						ec.ig.Emit (OpCodes.Neg);
					}
				}
			}

			static int next_id;
			int id = ++next_id;

			protected void DoEmit (EmitContext ec)
			{
				if (initialized)
					return;
				initialized = true;

				Report.Debug (64, "EMIT SCOPE INIT", this, id,
					      Scope, IsIterator, scope_instance, ec);

				ec.ig.Emit (OpCodes.Nop);
				ec.ig.Emit (OpCodes.Ldc_I4, id);
				ec.ig.Emit (OpCodes.Pop);
				ec.ig.Emit (OpCodes.Nop);

				if (!IsIterator) {
					scope_ctor.Emit (ec);
					scope_instance.Store (ec);
				}

				if (Scope.THIS != null) {
					if (IsIterator) {
						ec.ig.Emit (OpCodes.Ldarg_0);
						ec.ig.Emit (OpCodes.Ldarg_1);
					} else {
						scope_instance.Emit (ec);
						ec.ig.Emit (OpCodes.Ldarg_0);
					}
					ec.ig.Emit (OpCodes.Stfld, this_field.FieldInfo);
				}

				if (Scope.HostsParameters) {
					foreach (DictionaryEntry de in captured_params) {
						FieldExpr fe = (FieldExpr) de.Value;
						int idx = (int) de.Key;

						EmitScopeInstance (ec);
						ParameterReference.EmitLdArg (ec.ig, idx);
						ec.ig.Emit (OpCodes.Stfld, fe.FieldInfo);
					}
				}

				if (parent_init != null) {
					Report.Debug (64, "EMIT SCOPE INIT PARENT", this, id, Scope,
						      Scope.ParentScope, parent_init, ec);
					parent_init.EmitStatement (ec);
				}
			}

			public override void Emit (EmitContext ec)
			{
				DoEmit (ec);
				EmitScopeInstance (ec);
			}

			public override void EmitStatement (EmitContext ec)
			{
				DoEmit (ec);
			}

			protected class SimpleThis : Expression
			{
				public SimpleThis (Type type, Location loc)
				{
					this.type = type;
					this.loc = loc;
					eclass = ExprClass.Value;
				}

				public override Expression DoResolve (EmitContext ec)
				{
					return this;
				}

				public override void Emit (EmitContext ec)
				{
					ec.ig.Emit (OpCodes.Ldarg_0);
				}
			}
		}
	}

	//
	// CaptureContext objects are created on demand if a method has
	// anonymous methods and kept on the ToplevelBlock.
	//
	// If they exist, all ToplevelBlocks in the containing block are
	// linked together (children pointing to their parents).
	//
	public class CaptureContext {
		public static int count;
		public int cc_id;
		public Location loc;
		
		//
		// Points to the toplevel block that owns this CaptureContext
		//
		ToplevelBlock toplevel_owner;

		//
		// All the scopes we capture
		//
		Hashtable scopes = new Hashtable ();

		//
		// All the root scopes
		//
		ArrayList roots = new ArrayList ();
		
		bool have_captured_vars = false;
		bool referenced_this = false;

		//
		// Captured fields
		//
		Hashtable captured_fields = new Hashtable ();
		Hashtable captured_variables = new Hashtable ();
		public Hashtable captured_parameters = new Hashtable ();
		public IAnonymousContainer Host;

		public CaptureContext (ToplevelBlock toplevel_owner, Location loc,
				       IAnonymousContainer host)
		{
			cc_id = count++;
			this.toplevel_owner = toplevel_owner;
			this.Host = host;
			this.loc = loc;

			Report.Debug (64, "NEW CAPTURE CONTEXT", this, toplevel_owner, loc);
		}

		void DoPath (StringBuilder sb, CaptureContext cc)
		{
			if (cc.ParentCaptureContext != null){
				DoPath (sb, cc.ParentCaptureContext);
				sb.Append (".");
			}
			sb.Append (cc.cc_id.ToString ());
		}

		public void ReParent (ToplevelBlock new_toplevel, AnonymousContainer new_host)
		{
			toplevel_owner = new_toplevel;
			Host = new_host;

			for (CaptureContext cc = ParentCaptureContext; cc != null;
			     cc = cc.ParentCaptureContext) {
				cc.Host = new_host;
			}
		}

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();
			sb.Append ("[");
			DoPath (sb, this);
			sb.Append ("]");
			return sb.ToString ();
		}

		public ToplevelBlock ParentToplevel {
			get {
				return toplevel_owner.Container;
			}
		}

		public CaptureContext ParentCaptureContext {
			get {
				ToplevelBlock parent = ParentToplevel;
				
				return (parent == null) ? null : parent.CaptureContext;
			}
		}

		internal ScopeInfo CreateRootScope (AnonymousContainer host)
		{
			this.Host = host;

			Report.Debug (64, "CREATE ROOT SCOPE", this, toplevel_owner,
				      toplevel_owner.ScopeInfo, host);

			if (toplevel_owner.ScopeInfo != null)
				return toplevel_owner.ScopeInfo;

			ScopeInfo si = new ScopeInfo (this, toplevel_owner, host);
			si.CreateScopeType ();

			toplevel_owner.ScopeInfo = si;
			scopes.Add (toplevel_owner.ID, si);
			return si;
		}

		internal ScopeInfo GetScopeForBlock (Block block)
		{
			ScopeInfo si = (ScopeInfo) scopes [block.ID];
			Report.Debug (64, "GET SCOPE FOR BLOCK", this, block,
				      block.ScopeInfo, block.Parent, si);
			if (si != null)
				return si;

			block.ScopeInfo = si = new ScopeInfo (this, block);
			si.CreateScopeType ();
			scopes [block.ID] = si;
			return si;
		}

		public Variable AddLocal (AnonymousContainer am, LocalInfo li)
		{
			Report.Debug (64, "ADD LOCAL", this, li.Name, loc, li.Block,
				      li.Block.Toplevel, toplevel_owner);

			if (li.Block.Toplevel != toplevel_owner)
				return ParentCaptureContext.AddLocal (am, li);

			ScopeInfo scope = GetScopeForBlock (li.Block);

#if FIXME
			//
			// Adjust the owner
			//
			if (Host != null)
				Host.RegisterScope (scope);
#endif

			//
			// Adjust the user
			//
			am.RegisterScope (scope);

			Report.Debug (64, "ADD LOCAL #1", this, li.Name, scope);

			Variable var = (Variable) captured_variables [li];
			if (var == null) {
				var = scope.AddLocal (li);
				captured_variables.Add (li, var);
			}

			have_captured_vars = true;
			return var;
		}

		//
		// Retursn the CaptureContext for the block that defines the parameter `name'
		//
		static CaptureContext _ContextForParameter (ToplevelBlock current, string name)
		{
			ToplevelBlock container = current.Container;
			if (container != null){
				CaptureContext cc = _ContextForParameter (container, name);
				if (cc != null)
					return cc;
			}
			if (current.IsParameterReference (name))
				return current.ToplevelBlockCaptureContext;
			return null;
		}

		static CaptureContext ContextForParameter (ToplevelBlock current, string name)
		{
			CaptureContext cc = _ContextForParameter (current, name);
			if (cc == null)
				throw new Exception (String.Format ("request for parameteter {0} failed: not found", name));
			return cc;
		}
		
		//
		// Records the captured parameter at the appropriate CaptureContext
		//
		public Expression AddParameter (EmitContext ec, Parameter par, int idx, Location loc)
		{
			CaptureContext cc = ContextForParameter (ec.CurrentBlock.Toplevel, par.Name);
			return cc.AddParameterToContext (ec.CurrentAnonymousMethod, par, idx, loc);
		}

		//
		// Records the parameters in the context
		//
		public Expression AddParameterToContext (AnonymousContainer am, Parameter par,
							 int idx, Location loc)
		{
			if (captured_parameters == null)
				captured_parameters = new Hashtable ();

			ScopeInfo scope = GetScopeForBlock (toplevel_owner);
			scope.HostsParameters = true;
			am.RegisterScope (scope);

			CapturedParameter cp = (CapturedParameter) captured_parameters [par.Name];
			if (cp == null) {
				cp = new CapturedParameter (scope, par, idx);
				captured_parameters.Add (par.Name, cp);
			}

			return new CapturedParameterReference (scope, cp, loc);
		}

		//
		// Captured fields are only recorded on the topmost CaptureContext, because that
		// one is the one linked to the owner of instance fields
		//
		public void AddField (EmitContext ec, AnonymousContainer am, FieldExpr fe)
		{
			if (fe.FieldInfo.IsStatic)
				throw new Exception ("Attempt to register a static field as a captured field");
			CaptureContext parent = ParentCaptureContext;
			if (parent != null) {
				parent.AddField (ec, am, fe);
				return;
			}

			ScopeInfo scope = GetScopeForBlock (toplevel_owner);
			am.RegisterScope (scope);
		}

		public void CaptureThis (AnonymousContainer am)
		{
			if (am == null)
				throw new Exception ("Internal Compiler error: Capturethis called with a null method");
			CaptureContext parent = ParentCaptureContext;
			if (parent != null) {
				parent.CaptureThis (am);
				return;
			}
			referenced_this = true;

			ScopeInfo scope = GetScopeForBlock (toplevel_owner);
			am.RegisterScope (scope);
		}

		public bool HaveCapturedVariables {
			get {
				return have_captured_vars;
			}
		}
		
		public bool HaveCapturedFields {
			get {
				CaptureContext parent = ParentCaptureContext;
				if (parent != null)
					return parent.HaveCapturedFields;
				return captured_fields.Count > 0;
			}
		}

		public bool NeedThis {
			get { return HaveCapturedFields || referenced_this; }
		}

		public Variable GetCapturedVariable (LocalInfo local)
		{
			return (Variable) captured_variables [local];
		}

		//
		// Returns whether the parameter is captured
		//
		public bool IsParameterCaptured (string name)
		{
			if (ParentCaptureContext != null && ParentCaptureContext.IsParameterCaptured (name))
				return true;
			
			if (captured_parameters != null)
				return captured_parameters [name] != null;
			return false;
		}

		public void EmitAnonymousHelperClasses (EmitContext ec)
		{
			Report.Debug (64, "EMIT ANONYMOUS HELPERS");

			if (roots.Count != 0) {
				foreach (ScopeInfo root in roots)
					root.EmitScopeType (ec);
			} 
		}

		public ExpressionStatement GetScopeInitializerForBlock (EmitContext ec, Block b)
		{
			Report.Debug (64, "GET SCOPE INIT FOR BLOCK", this, Host, b);
			ScopeInfo si = (ScopeInfo) scopes [b.ID];
			if (si == null)
				return null;

			Report.Debug (64, "GET SCOPE INIT FOR BLOCK #1", this, Host, b, si);
			return si.GetScopeInitializer (ec);
		}
		
		//
		// Emits the opcodes necessary to load the instance of the captured
		// variable in `li'
		//
		public void EmitCapturedVariableInstance (EmitContext ec, LocalInfo li,
							  AnonymousContainer am)
		{
			ILGenerator ig = ec.ig;
			ScopeInfo si;

			Report.Debug (64, "EMIT CAPTURED VARIABLE INSTANCE", this, li.Name,
				      am, li.Block.Toplevel, toplevel_owner);

			if (li.Block.Toplevel == toplevel_owner){
				si = (ScopeInfo) scopes [li.Block.ID];
				si.EmitScopeInstance (ec);
				return;
			}

			Report.Debug (64, "EMIT CAPTURED VARIABLE INSTANCE #1", this, li.Name,
				      am, am.IsIterator, li.Block.Toplevel, toplevel_owner, am.Scope);

			si = am.Scope;
			ig.Emit (OpCodes.Ldarg_0);
			if (si != null){
				if (am.IsIterator && (si.ScopeBlock.Toplevel == li.Block.Toplevel)) {
					return;
				}

			Report.Debug (64, "EMIT CAPTURED VARIABLE INSTANCE #2", this, li.Name,
				      si, si.ParentLink, si.ScopeBlock, li.Block);

				while (si.ScopeBlock.ID != li.Block.ID){
					if (si.ParentLink != null)
						ig.Emit (OpCodes.Ldfld, si.ParentLink.FieldBuilder);
					si = si.ParentScope;
					if (si == null) {
						si = am.Scope;
						Console.WriteLine ("Target: {0} {1}", li.Block, li.Name);
						while (si.ScopeBlock.ID != li.Block.ID){
							Console.WriteLine ("Trying: {0} {1}",
									   si, si.ScopeBlock);
							si = si.ParentScope;
						}

						throw new Exception (
							     String.Format ("Never found block {0} starting at {1} while looking up {2}",
									    li.Block.ID, am.Scope.ScopeBlock.ID, li.Name));
					}
				}
			}
		}

		//
		// Internal routine that loads the instance to reach parameter `name'
		//
		void EmitParameterInstance (EmitContext ec, string name)
		{
			CaptureContext cc = ContextForParameter (ec.CurrentBlock.Toplevel, name);
			if (cc != this){
				cc.EmitParameterInstance (ec, name);
				return;
			}

			CapturedParameter par_info = (CapturedParameter) captured_parameters [name];
			if (par_info != null){
				// 
				// FIXME: implementing this.
				//
			}
			ILGenerator ig = ec.ig;

			ScopeInfo si;

			if (ec.CurrentBlock.Toplevel == toplevel_owner) {
				si = (ScopeInfo) scopes [toplevel_owner.ID];
				si.EmitScopeInstance (ec);
			} else {
				si = ec.CurrentAnonymousMethod.Scope;
				ig.Emit (OpCodes.Ldarg_0);
			}

			if (si != null){
				while (si.ParentLink != null) {
					ig.Emit (OpCodes.Ldfld, si.ParentLink.FieldBuilder);
					si = si.ParentScope;
				} 
			}
		}

		//
		// Emits the code necessary to load the parameter named `name' within
		// an anonymous method.
		//
		public void EmitParameter (EmitContext ec, string name, bool leave_copy, bool prepared, ref LocalTemporary temp)
		{
			CaptureContext cc = ContextForParameter (ec.CurrentBlock.Toplevel, name);
			if (cc != this){
				cc.EmitParameter (ec, name, leave_copy, prepared, ref temp);
				return;
			}
			if (!prepared)
				EmitParameterInstance (ec, name);
			CapturedParameter par_info = (CapturedParameter) captured_parameters [name];
			if (par_info != null){
				// 
				// FIXME: implementing this.
				//
			}
			ec.ig.Emit (OpCodes.Ldfld, par_info.Field.FieldBuilder);

			if (leave_copy){
				ec.ig.Emit (OpCodes.Dup);
				temp = new LocalTemporary (par_info.Field.MemberType);
				temp.Store (ec);
			}
		}

		//
		// Implements the assignment of `source' to the paramenter named `name' within
		// an anonymous method.
		//
		public void EmitAssignParameter (EmitContext ec, string name, Expression source, bool leave_copy, bool prepare_for_load, ref LocalTemporary temp)
		{
			CaptureContext cc = ContextForParameter (ec.CurrentBlock.Toplevel, name);
			if (cc != this){
				cc.EmitAssignParameter (ec, name, source, leave_copy, prepare_for_load, ref temp);
				return;
			}
			ILGenerator ig = ec.ig;
			CapturedParameter par_info = (CapturedParameter) captured_parameters [name];

			EmitParameterInstance (ec, name);
			if (prepare_for_load)
				ig.Emit (OpCodes.Dup);
			source.Emit (ec);
			if (leave_copy){
				ig.Emit (OpCodes.Dup);
				temp = new LocalTemporary (par_info.Field.MemberType);
				temp.Store (ec);
			}
			ig.Emit (OpCodes.Stfld, par_info.Field.FieldBuilder);
			if (temp != null)
				temp.Emit (ec);
		}

		//
		// Emits the address for the parameter named `name' within
		// an anonymous method.
		//
		public void EmitAddressOfParameter (EmitContext ec, string name)
		{
			CaptureContext cc = ContextForParameter (ec.CurrentBlock.Toplevel, name);
			if (cc != this){
				cc.EmitAddressOfParameter (ec, name);
				return;
			}
			EmitParameterInstance (ec, name);
			CapturedParameter par_info = (CapturedParameter) captured_parameters [name];
			ec.ig.Emit (OpCodes.Ldflda, par_info.Field.FieldBuilder);
		}

		public void RegisterCaptureContext ()
		{
			toplevel_owner.RegisterCaptureContext (this);
		}

		//
		// Returs true if `probe' is an ancestor of `scope' in the 
		// scope chain
		//
		bool IsAncestor (ScopeInfo probe, ScopeInfo scope)
		{
			for (Block b = scope.ScopeBlock.Parent; b != null; b = b.Parent){
				if (probe.ScopeBlock == b)
					return true;
			}
			return false;
		}

		//
		// Returns an ArrayList of ScopeInfos that enumerates all the ancestors
		// of `scope' found in `scope_list'.
		//
		// The value returned is either a ScopeInfo or an Arraylist of ScopeInfos
		//
		object GetAncestorScopes (ScopeInfo scope, ScopeInfo [] scope_list)
		{
			object ancestors = null;
			
			for (int i = 0; i < scope_list.Length; i++){
				// Ignore the same scope
				if (scope_list [i] == scope)
					continue;
				
				if (IsAncestor (scope_list [i], scope)){
					if (ancestors == null){
						ancestors = scope_list [i];
						continue;
					}
					
					if (ancestors is ScopeInfo){
						object old = ancestors;
						ancestors = new ArrayList (4);
						((ArrayList)ancestors).Add (old);
					} 
					
					((ArrayList)ancestors).Add (scope_list [i]);
				}
			}
			return ancestors;
		}

		//
		// Returns the immediate parent of `scope' from all the captured
		// scopes found in `scope_list', or null if this is a toplevel scope.
		//
		ScopeInfo GetParentScope (ScopeInfo scope, ScopeInfo [] scope_list)
		{
			object ancestors = GetAncestorScopes (scope, scope_list);
			if (ancestors == null)
				return null;

			// Single match, thats the parent.
			if (ancestors is ScopeInfo)
				return (ScopeInfo) ancestors;

			ArrayList candidates = (ArrayList) ancestors;
			ScopeInfo parent = (ScopeInfo) candidates [0];
			for (int i = 1; i < candidates.Count; i++){
				if (IsAncestor (parent, (ScopeInfo) candidates [i]))
					parent = (ScopeInfo) candidates [i];
			}
			return parent;
		}

		//
		// Links all the scopes
		//
		bool linked;
		public void LinkScopes ()
		{
			if (linked)
				return;

			Report.Debug (64, "LINK SCOPES", this, ParentCaptureContext);

			linked = true;
			if (ParentCaptureContext != null)
				ParentCaptureContext.LinkScopes ();

			int scope_count = scopes.Keys.Count;
			ScopeInfo [] scope_list = new ScopeInfo [scope_count];
			scopes.Values.CopyTo (scope_list, 0);

			Report.Debug (64, "LINK SCOPES #1", this, scope_list);

			for (int i = 0; i < scope_count; i++){
				ScopeInfo parent = GetParentScope (scope_list [i], scope_list);

				Report.Debug (64, "LINK SCOPES #2", this, scope_list, i,
					      scope_list [i], parent);

				if (parent == null){
					roots.Add (scope_list [i]);
					continue;
				}

				scope_list [i].ParentScope = parent;
				parent.AddChild (scope_list [i]);
			}

			Report.Debug (64, "LINK SCOPES #3", this, ParentCaptureContext, roots);

			//
			// Link the roots to their parent containers if any.
			//
			if (ParentCaptureContext != null && roots.Count != 0){
				ScopeInfo one_root = (ScopeInfo) roots [0];
				bool found = false;

				Report.Debug (64, "LINK SCOPES #4", this, one_root,
					      ParentCaptureContext.roots);

				foreach (ScopeInfo a_parent_root in ParentCaptureContext.roots){
					Report.Debug (64, "LINK SCOPES #5", this, a_parent_root,
						      one_root);

					if (!IsAncestor (a_parent_root, one_root))
						continue;

					Report.Debug (64, "LINK SCOPES #6", this, a_parent_root,
						      one_root, roots);

					found = true;
					
					// Found, link all the roots to this root
					foreach (ScopeInfo root in roots){
						root.ParentScope = a_parent_root;
						a_parent_root.AddChild (root);
					}
					break;
				}
				if (!found){
					//
					// This is to catch a condition in which it is
					// not possible to determine the containing ScopeInfo
					// from an encapsulating CaptureContext
					//
					throw new Exception ("Internal compiler error: Did not find the parent for the root in the chain");
				}
			}
		}
	}
}
