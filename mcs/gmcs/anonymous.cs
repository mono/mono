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

		protected string MakeName (string prefix)
		{
			return String.Format ("<>c__{0}{1}", prefix, ++next_index);
		}

		protected CompilerGeneratedClass (TypeContainer parent, GenericMethod generic,
						  int mod, Location loc)
			: base (parent.NamespaceEntry, parent,
				MakeProxyName (generic, loc), mod, null)
		{
			this.generic_method = generic;

			if (generic != null) {
				ArrayList list = new ArrayList ();
				foreach (TypeParameter tparam in generic.TypeParameters) {
					if (tparam.Constraints != null)
						list.Add (tparam.Constraints.Clone ());
				}
				SetParameterInfo (list);
			}

			parent.AddCompilerGeneratedClass (this);
		}

		protected override bool DefineNestedTypes ()
		{
			RootContext.RegisterCompilerGeneratedType (TypeBuilder);
			return base.DefineNestedTypes ();
		}

		protected override bool DoDefineMembers ()
		{
			if (!PopulateType ())
				throw new InternalErrorException ();

			members_defined = true;

			if (!base.DoDefineMembers ())
				return false;

			if (CompilerGenerated != null) {
				foreach (CompilerGeneratedClass c in CompilerGenerated) {
					if (!c.DefineMembers ())
						throw new InternalErrorException ();
				}
			}

			return true;
		}

		protected virtual bool PopulateType ()
		{
			if (type_populated)
				return true;

			type_populated = true;

			if (!DoPopulateType ())
				return false;

			if (CompilerGenerated != null) {
				foreach (CompilerGeneratedClass c in CompilerGenerated) {
					if (!c.PopulateType ())
						return false;
				}
			}

			return true;
		}

		protected virtual bool DoPopulateType ()
		{
			return true;
		}

		public GenericMethod GenericMethod {
			get { return generic_method; }
		}

		protected TypeExpr InflateType (Type it)
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

		public Field CaptureVariable (string name, Type type)
		{
			if (members_defined)
				throw new InternalErrorException ("Helper class already defined!");
			if (type == null)
				throw new ArgumentNullException ();

			return new CapturedVariable (this, name, InflateType (type));
		}

		bool type_populated;
		bool members_defined;

		internal void CheckMembersDefined ()
		{
			if (members_defined)
				throw new InternalErrorException ("Helper class already defined!");
		}

		public override void EmitType ()
		{
			Report.Debug (64, "COMPILER GENERATED EMIT TYPE", this, CompilerGenerated);
			base.EmitType ();
		}

		protected class CapturedVariable : Field
		{
			public CapturedVariable (CompilerGeneratedClass helper, string name,
						 TypeExpr type)
				: base (helper, type, Modifiers.INTERNAL, name, null, helper.Location)
			{
				helper.AddField (this);
			}
		}
	}

	public abstract class ScopeInfoBase : CompilerGeneratedClass
	{
		public CaptureContext CaptureContext;
		public Block ScopeBlock;

		protected ScopeInfoBase (TypeContainer parent, GenericMethod generic,
					 int mod, Location loc)
			: base (parent, generic, mod, loc)
		{ }

		TypeExpr scope_type;
		protected Field scope_instance;
		protected ScopeInitializerBase scope_initializer;

		public abstract AnonymousMethodHost Host {
			get;
		}

		public Field ScopeInstance {
			get { return scope_instance; }
		}

		public Type ScopeType {
			get { return scope_type.Type; }
		}

		public void EmitScopeInstance (EmitContext ec)
		{
			if (scope_initializer == null) {
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

			scope_initializer.Emit (ec);
		}

		public ExpressionStatement GetScopeInitializer (EmitContext ec)
		{
			Report.Debug (64, "GET SCOPE INITIALIZER",
				      this, GetType (), scope_initializer, ScopeBlock);

			if (scope_initializer == null) {
				scope_initializer = CreateScopeInitializer ();
				if (scope_initializer.Resolve (ec) == null)
					throw new InternalErrorException ();
			}

			return scope_initializer;
		}

		protected abstract ScopeInitializerBase CreateScopeInitializer ();

		protected override bool DoPopulateType ()
		{
			Report.Debug (64, "SCOPE INFO RESOLVE TYPE", this, GetType (), IsGeneric,
				      Parent.IsGeneric, GenericMethod);

			if (IsGeneric) {
				TypeArguments targs = new TypeArguments (Location);
				if (Parent.IsGeneric)
					foreach (TypeParameter t in Parent.TypeParameters)
						targs.Add (new TypeParameterExpr (t, Location));
				if (GenericMethod != null)
					foreach (TypeParameter t in GenericMethod.TypeParameters)
						targs.Add (new TypeParameterExpr (t, Location));

				scope_type = new ConstructedType (TypeBuilder, targs, Location);
				scope_type = scope_type.ResolveAsTypeTerminal (this, false);
				if ((scope_type == null) || (scope_type.Type == null))
					return false;
			} else {
				scope_type = new TypeExpression (TypeBuilder, Location);
			}

			if (Host != this)
				scope_instance = Host.CaptureScope (this);

			return base.DoPopulateType ();
		}

		protected abstract class ScopeInitializerBase : ExpressionStatement
		{
			ScopeInfoBase scope;
			LocalBuilder scope_instance;
			ConstructorInfo scope_ctor;

			bool initialized;

			protected ScopeInitializerBase (ScopeInfoBase scope)
			{
				this.scope = scope;
				this.loc = scope.Location;
				eclass = ExprClass.Value;
			}

			public ScopeInfoBase Scope {
				get { return scope; }
			}

			public override Expression DoResolve (EmitContext ec)
			{
				if (scope_ctor != null)
					return this;

				Report.Debug (64, "RESOLVE SCOPE INITIALIZER BASE", this, Scope,
					      ec, ec.CurrentBlock);

				type = Scope.ScopeType;

				if (!DoResolveInternal (ec))
					throw new InternalErrorException ();

				return this;
			}

			protected virtual bool DoResolveInternal (EmitContext ec)
			{
				MethodGroupExpr mg = (MethodGroupExpr) Expression.MemberLookupFinal (
					ec, ec.ContainerType, type, ".ctor", MemberTypes.Constructor,
					AllBindingFlags | BindingFlags.DeclaredOnly, loc);
				if (mg == null)
					throw new InternalErrorException ();

				scope_ctor = (ConstructorInfo) mg.Methods [0];

				if (Scope.ScopeInstance == null)
					scope_instance = ec.ig.DeclareLocal (type);

				return true;
			}

			static int next_id;
			int id = ++next_id;

			protected virtual void DoEmit (EmitContext ec)
			{
				if (ec.CurrentBlock.Toplevel == Scope.ScopeBlock.Toplevel)
					DoEmitInstance (ec);
				else
					ec.ig.Emit (OpCodes.Ldarg_0);
			}

			protected void DoEmitInstance (EmitContext ec)
			{
				ec.ig.Emit (OpCodes.Ldloc, scope_instance);
			}

			protected virtual void EmitScopeConstructor (EmitContext ec)
			{
				ec.ig.Emit (OpCodes.Newobj, scope_ctor);
			}

			public override void Emit (EmitContext ec)
			{
				if (!initialized)
					throw new InternalErrorException (
						"Scope {0} not initialized yet", scope);

				DoEmit (ec);
			}

			public override void EmitStatement (EmitContext ec)
			{
				if (initialized)
					return;

				DoEmitStatement (ec);
				initialized = true;
			}

			protected virtual void DoEmitStatement (EmitContext ec)
			{
				Report.Debug (64, "EMIT SCOPE INIT", this, id,
					      Scope, scope_instance, ec);

				ec.ig.Emit (OpCodes.Nop);
				ec.ig.Emit (OpCodes.Ldc_I4, id);
				ec.ig.Emit (OpCodes.Pop);
				ec.ig.Emit (OpCodes.Nop);

				if (Scope != Scope.Host)
					Scope.Host.EmitScopeInstance (ec);
				else if (Scope.ScopeInstance != null)
					ec.ig.Emit (OpCodes.Ldarg_0);
				EmitScopeConstructor (ec);
				if (Scope.ScopeInstance != null)
					ec.ig.Emit (OpCodes.Stfld, Scope.ScopeInstance.FieldBuilder);
				else
					ec.ig.Emit (OpCodes.Stloc, scope_instance);
			}
		}
	}

	public class AnonymousMethodHost : ScopeInfo
	{
		public AnonymousMethodHost (CaptureContext cc, ToplevelBlock toplevel,
					    TypeContainer parent, GenericMethod generic)
			: base (cc, toplevel, parent, generic)
		{ }

		Hashtable scopes;
		TypeExpr parent_type;
		CapturedVariable parent_link;
		CapturedVariable this_field;
		Hashtable captured_params;

		public override AnonymousMethodHost Host {
			get { return this; }
		}

		public AnonymousMethodHost ParentHost {
			get { return Parent as AnonymousMethodHost; }
		}

		public Type ParentType {
			get { return parent_type.Type; }
		}

		public Field ParentLink {
			get { return parent_link; }
		}

		public Field THIS {
			get { return this_field; }
		}

		public bool HostsParameters {
			get { return captured_params != null; }
		}

		public Field CaptureThis ()
		{
			if (ParentHost != null)
				return ParentHost.CaptureThis ();

			CheckMembersDefined ();
			if (this_field == null)
				this_field = new CapturedVariable (this, "<>THIS", parent_type);
			return this_field;
		}

		public Field CaptureScope (ScopeInfoBase scope)
		{
			Field field = CaptureVariable (MakeName ("scope"), scope.ScopeType);
			if (scopes == null)
				scopes = new Hashtable ();
			scopes.Add (scope, field);
			return field;
		}

		public Field GetChildScope (ScopeInfoBase scope)
		{
			return (Field) scopes [scope];
		}

		public Variable GetCapturedParameter (Parameter par)
		{
			if (captured_params != null)
				return (Variable) captured_params [par];
			else
				return null;
		}

		public bool IsParameterCaptured (string name)
		{			
			if (captured_params != null)
				return captured_params [name] != null;
			return false;
		}

		public Variable AddParameter (Parameter par, int idx)
		{
			if (captured_params == null)
				captured_params = new Hashtable ();

			Variable var = (Variable) captured_params [par];
			if (var == null) {
				var = new CapturedParameter (this, par, idx);
				captured_params.Add (par, var);
			}

			return var;
		}

		protected override ScopeInitializerBase CreateScopeInitializer ()
		{
			return new AnonymousMethodHostInitializer (this);
		}

		protected override bool DefineNestedTypes ()
		{
			Report.Debug (64, "ANONYMOUS METHOD HOST NESTED",
				      this, Name, Parent, Parent.Name, Parent.IsGeneric);

			if (Parent.IsGeneric) {
				parent_type = new ConstructedType (
					Parent.TypeBuilder, Parent.TypeParameters, Location);
				parent_type = parent_type.ResolveAsTypeTerminal (this, false);
				if ((parent_type == null) || (parent_type.Type == null))
					return false;
			} else {
				parent_type = new TypeExpression (Parent.TypeBuilder, Location);
			}

			CompilerGeneratedClass parent = Parent as CompilerGeneratedClass;
			if (parent != null)
				parent_link = new CapturedVariable (this, "<>parent", parent_type);

			return base.DefineNestedTypes ();
		}

		protected override bool DoDefineMembers ()
		{
			Report.Debug (64, "ANONYMOUS METHOD HOST DEFINE MEMBERS",
				      this, Name, Parent, CompilerGenerated);

			ArrayList args = new ArrayList ();
			if (this is IteratorHost)
				args.Add (new Parameter (
					TypeManager.int32_type, "$PC", Parameter.Modifier.NONE,
					null, Location));

			Field pfield = Parent is CompilerGeneratedClass ? parent_link : this_field;
			if (pfield != null)
				args.Add (new Parameter (
					pfield.MemberType, "parent", Parameter.Modifier.NONE,
					null, Location));

			if (HostsParameters) {
				foreach (CapturedParameter cp in captured_params.Values) {
					args.Add (new Parameter (
							  cp.Field.MemberType, cp.Field.Name,
							  Parameter.Modifier.NONE, null, Location));
				}
			}

			Parameter[] ctor_params = new Parameter [args.Count];
			args.CopyTo (ctor_params, 0);
			Constructor ctor = new Constructor (
				this, MemberName.Name, Modifiers.PUBLIC,
				new Parameters (ctor_params),
				new GeneratedBaseInitializer (Location),
				Location);
			AddConstructor (ctor);

			ctor.Block = new ToplevelBlock (null, Location);
			ctor.Block.AddStatement (new TheCtor (this));

			return base.DoDefineMembers ();
		}

		protected virtual void EmitScopeConstructor (EmitContext ec)
		{
			int pos = (this is IteratorHost) ? 2 : 1;
			Field pfield = Parent is CompilerGeneratedClass ? parent_link : this_field;
			if (pfield != null) {
				ec.ig.Emit (OpCodes.Ldarg_0);
				ec.ig.Emit (OpCodes.Ldarg, pos);
				ec.ig.Emit (OpCodes.Stfld, pfield.FieldBuilder);
				pos++;
			}

			if (HostsParameters) {
				foreach (CapturedParameter cp in captured_params.Values) {
					ec.ig.Emit (OpCodes.Ldarg_0);
					ParameterReference.EmitLdArg (ec.ig, pos++);
					ec.ig.Emit (OpCodes.Stfld, cp.Field.FieldBuilder);
				}
			}
		}

		protected class TheCtor : Statement
		{
			AnonymousMethodHost host;

			public TheCtor (AnonymousMethodHost host)
			{
				this.host = host;
			}

			public override bool Resolve (EmitContext ec)
			{
				return true;
			}

			protected override void DoEmit (EmitContext ec)
			{
				host.EmitScopeConstructor (ec);
			}
		}

		protected class AnonymousMethodHostInitializer : ScopeInitializer
		{
			AnonymousMethodHost host;
			ExpressionStatement parent_init;

			public AnonymousMethodHostInitializer (AnonymousMethodHost host)
				: base (host)
			{
				this.host = host;
			}

			public AnonymousMethodHost Host {
				get { return host; }
			}

			protected override bool DoResolveInternal (EmitContext ec)
			{
				Report.Debug (64, "RESOLVE ANONYMOUS METHOD HOST INITIALIZER",
					      this, Host, Host.ScopeType, Host.ParentType, loc);

				//
				// Copy the parameter values, if any
				//
				if (Host.HostsParameters) {
					foreach (CapturedParameter cp in Host.captured_params.Values) {
						FieldExpr fe = (FieldExpr) Expression.MemberLookup (
							ec.ContainerType, type, cp.Field.Name, loc);
						if (fe == null)
							throw new InternalErrorException ();

						fe.InstanceExpression = this;
						cp.FieldInstance = fe;
					}
				}

				return base.DoResolveInternal (ec);
			}

			protected override void EmitScopeConstructor (EmitContext ec)
			{
				if ((host.THIS != null) || (host.ParentLink != null))
					ec.ig.Emit (OpCodes.Ldarg_0);

				if (Host.HostsParameters) {
					foreach (CapturedParameter cp in Host.captured_params.Values) {
						EmitParameterReference (ec, cp);
					}
				}

				base.EmitScopeConstructor (ec);
			}
		}

	}

	public interface IAnonymousContainer
	{
		ToplevelBlock Container {
			get;
		}

		GenericMethod GenericMethod {
			get;
		}

		AnonymousMethodHost RootScope {
			get;
		}

		bool IsIterator {
			get;
		}
	}

	public class AnonymousMethodExpression : Expression, IAnonymousContainer
	{
		public readonly AnonymousMethodExpression Parent;
		public readonly TypeContainer Host;
		public readonly Parameters Parameters;

		public ToplevelBlock Block;
		protected AnonymousMethod anonymous;

		protected readonly ToplevelBlock container;
		protected readonly GenericMethod generic;

		public ToplevelBlock Container {
			get { return container; }
		}

		public GenericMethod GenericMethod {
			get { return generic; }
		}

		public AnonymousMethod AnonymousMethod {
			get { return anonymous; }
		}

		public AnonymousMethodHost RootScope {
			get { return container.AnonymousMethodHost; }
		}

		public AnonymousMethodExpression (AnonymousMethodExpression parent,
						  GenericMethod generic, TypeContainer host,
						  Parameters parameters, ToplevelBlock container,
						  Location loc)
		{
			this.Parent = parent;
			this.generic = parent != null ? null : generic;
			this.Host = host;
			this.Parameters = parameters;
			this.container = container;
			this.loc = loc;

			Report.Debug (64, "NEW ANONYMOUS METHOD EXPRESSION", this, parent, host,
				      container, loc);

			if (container != null) {
				container.SetHaveAnonymousMethods (loc, this);
				container.CreateAnonymousMethodHost (Host);
			}
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
				Parent != null ? Parent.AnonymousMethod : null, Host,
				GenericMethod, parameters, Container, Block, invoke_mb.ReturnType,
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

		public bool IsIterator {
			get { return false; }
		}
	}

	public abstract class AnonymousContainer : IAnonymousContainer
	{
		// Used to generate unique method names.
		protected static int anonymous_method_count;

		public readonly Location Location;

		// An array list of AnonymousMethodParameter or null
		public readonly Parameters Parameters;

		//
		// The block that makes up the body for the anonymous mehtod
		//
		public readonly ToplevelBlock Block;

		public readonly Type ReturnType;
		public readonly TypeContainer Host;

		//
		// The implicit method we create
		//
		protected Method method;
		protected EmitContext aec;

		// The emit context for the anonymous method
		protected bool unreachable;
		protected readonly Location loc;
		protected readonly ToplevelBlock container;
		protected readonly GenericMethod generic;
		protected readonly AnonymousMethodHost root_scope;

		//
		// Points to our container anonymous method if its present
		//
		public readonly AnonymousContainer ContainerAnonymousMethod;

		protected AnonymousContainer (AnonymousContainer parent, TypeContainer host,
					      GenericMethod generic, Parameters parameters,
					      ToplevelBlock container, ToplevelBlock block,
					      Type return_type, int mod, Location loc)
		{
			this.ContainerAnonymousMethod = parent;
			this.ReturnType = return_type;
			this.Host = host;

			this.container = container;
			this.generic = parent != null ? null : generic;
			this.root_scope = container.AnonymousMethodHost;
			this.Parameters = parameters;
			this.Block = block;
			this.loc = loc;
		}

		public Method Method {
			get { return method; }
		}

		public virtual AnonymousMethodHost RootScope {
			get { return root_scope; }
		}

		public ScopeInfo Scope {
			get { return ContainerCaptureContext.Scope; }
		}

		public string GetSignatureForError ()
		{
			return RootScope.GetSignatureForError ();
		}

		protected virtual CaptureContext ContainerCaptureContext {
			get { return Container.CaptureContext; }
		}

		protected virtual CaptureContext CaptureContext {
			get { return Block.CaptureContext; }
		}

		public virtual bool Resolve (EmitContext ec)
		{
			if (!ec.IsAnonymousMethodAllowed) {
				Report.Error (1706, loc,
					      "Anonymous methods are not allowed in the " +
					      "attribute declaration");
				return false;
			}

			Report.Debug (64, "RESOLVE ANONYMOUS METHOD", this, loc, ec,
				      RootScope, Parameters, ec.IsStatic);

			aec = new EmitContext (
				ec.ResolveContext, ec.TypeContainer, RootScope, loc, null, ReturnType,
				/* REVIEW */ (ec.InIterator ? Modifiers.METHOD_YIELDS : 0) |
				(ec.InUnsafe ? Modifiers.UNSAFE : 0), /* No constructor */ false);

			aec.capture_context = CaptureContext;
			aec.CurrentAnonymousMethod = this;

			Report.Debug (64, "RESOLVE ANONYMOUS METHOD #1", this, loc, ec, aec,
				      RootScope, Parameters, Block, Block.CaptureContext);

			bool unreachable;
			if (!aec.ResolveTopBlock (ec, Block, Parameters, null, out unreachable))
				return false;

			Report.Debug (64, "RESOLVE ANONYMOUS METHOD #3", this, ec, aec, Block,
				      ec.capture_context);

			ContainerCaptureContext.ComputeMethodHost ();

			Report.Debug (64, "RESOLVE ANONYMOUS METHOD #4", this, Scope);

			if (Scope == null)
				throw new InternalErrorException ();

			method = DoCreateMethodHost (ec);

			return true;
		}

		protected abstract Method DoCreateMethodHost (EmitContext ec);

		public ToplevelBlock Container {
			get { return container; }
		}

		public GenericMethod GenericMethod {
			get { return generic; }
		}

		public abstract bool IsIterator {
			get;
		}

#if FIXME
		public override string ToString ()
		{
			return String.Format ("{0} ({1})", GetType (), Name);
		}
#endif

		protected class AnonymousMethodMethod : Method
		{
			public AnonymousContainer AnonymousMethod;

			public AnonymousMethodMethod (AnonymousContainer am, GenericMethod generic,
						      TypeExpr return_type, int mod, MemberName name,
						      Parameters parameters)
				: base (am.RootScope, generic, return_type, mod, false,
					name, parameters, null)
			{
				this.AnonymousMethod = am;

				am.RootScope.CheckMembersDefined ();
				am.Scope.AddMethod (this);
				Block = am.Block;
			}

			public override EmitContext CreateEmitContext (DeclSpace tc, ILGenerator ig)
			{
				EmitContext aec = AnonymousMethod.aec;
				aec.ig = ig;
				return aec;
			}
		}
	}

	public class AnonymousMethod : AnonymousContainer
	{
		public readonly Type DelegateType;

		//
		// The value return by the Compatible call, this ensure that
		// the code works even if invoked more than once (Resolve called
		// more than once, due to the way Convert.ImplicitConversion works
		//
		Expression anonymous_delegate;

		public AnonymousMethod (AnonymousMethod parent, TypeContainer host,
					GenericMethod generic, Parameters parameters,
					ToplevelBlock container, ToplevelBlock block,
					Type return_type, Type delegate_type, Location loc)
			: base (parent, host, generic, parameters, container, block,
				return_type, 0, loc)
		{
			this.DelegateType = delegate_type;

			// Container.CaptureContext.RegisterScope (RootScope);
		}

		public override bool IsIterator {
			get { return false; }
		}

		public Expression AnonymousDelegate {
			get { return anonymous_delegate; }
		}

		//
		// Creates the host for the anonymous method
		//
		protected override Method DoCreateMethodHost (EmitContext ec)
		{
			string name = "<>c__AnonymousMethod" + anonymous_method_count++;
			MemberName member_name;

			GenericMethod generic_method = null;
			if (DelegateType.IsGenericType) {
				TypeArguments args = new TypeArguments (loc);

				Type[] tparam = TypeManager.GetTypeArguments (DelegateType);
				for (int i = 0; i < tparam.Length; i++)
					args.Add (new SimpleName (tparam [i].Name, loc));

				member_name = new MemberName (name, args, loc);

				generic_method = new GenericMethod (
					Scope.NamespaceEntry, Scope, member_name,
					new TypeExpression (ReturnType, loc), Parameters);

				generic_method.SetParameterInfo (null);
			} else
				member_name = new MemberName (name, loc);

			return new AnonymousMethodMethod (
				this, generic_method, new TypeExpression (ReturnType, loc),
				Modifiers.INTERNAL, member_name, Parameters);
		}

		public override bool Resolve (EmitContext ec)
		{
			if (!base.Resolve (ec))
				return false;

			anonymous_delegate = new AnonymousDelegate (this, DelegateType, loc).Resolve (ec);
			if (anonymous_delegate == null)
				return false;

			return true;
		}

		public MethodInfo GetMethodBuilder (EmitContext ec)
		{
			MethodInfo builder = method.MethodBuilder;
			if (RootScope.IsGeneric) {
				MethodGroupExpr mg = (MethodGroupExpr) Expression.MemberLookup (
					ec.ContainerType, RootScope.ScopeType, builder.Name, loc);

				if (mg == null)
					throw new InternalErrorException ();
				builder = (MethodInfo) mg.Methods [0];
			}

			if (!DelegateType.IsGenericType)
				return builder;

			Type[] targs = TypeManager.GetTypeArguments (DelegateType);
			return builder.MakeGenericMethod (targs);
		}

		public static void Error_AddressOfCapturedVar (string name, Location loc)
		{
			Report.Error (1686, loc,
				      "Local variable `{0}' or its members cannot have their " +
				      "address taken and be used inside an anonymous method block",
				      name);
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

			//
			// Now emit the delegate creation.
			//
			if ((am.Method.ModFlags & Modifiers.STATIC) == 0) {
				delegate_instance_expression = am.RootScope.GetScopeInitializer (ec);
				Report.Debug (64, "ANONYMOUS DELEGATE #0", this,
					      delegate_instance_expression);

				if (delegate_instance_expression == null)
					throw new InternalErrorException ();
			}

			Expression ml = Expression.MemberLookup (ec.ContainerType, type, ".ctor", loc);
			constructor_method = ((MethodGroupExpr) ml).Methods [0];
			delegate_method = am.GetMethodBuilder (ec);
			Report.Debug (64, "ANONYMOUS DELEGATE #1", constructor_method, delegate_method,
				      delegate_method.GetType (), delegate_instance_expression);
			base.Emit (ec);
		}
	}

	//
	// Here we cluster all the variables captured on a given scope, we also
	// keep some extra information that might be required on each scope.
	//
	public class ScopeInfo : ScopeInfoBase {
		public ScopeInfo ParentScope;
		
		// For tracking the number of scopes created.
		public int id;
		static int count;
		
		ArrayList locals = new ArrayList ();
		ArrayList children = new ArrayList ();

		//
		// The types and fields generated
		//
		public readonly Location loc;

		public ScopeInfo (CaptureContext cc, Block block)
			: base (cc.Host.RootScope, cc.Host.GenericMethod,
				Modifiers.PUBLIC, cc.loc)
		{
			CaptureContext = cc;
			ScopeBlock = block;
			loc = cc.loc;
			id = count++;

			Report.Debug (64, "NEW SCOPE", this);

			cc.RegisterCaptureContext ();
		}

		public ScopeInfo (CaptureContext cc, ToplevelBlock toplevel, TypeContainer parent,
				  GenericMethod generic)
			: base (parent, generic, 0, toplevel.StartLocation)
		{
			CaptureContext = cc;
			ScopeBlock = toplevel;
			loc = cc.loc;
			id = count++;

			Report.Debug (64, "NEW ROOT SCOPE", this);

			cc.RegisterCaptureContext ();
		}

		public override AnonymousMethodHost Host {
			get { return CaptureContext.Host.RootScope; }
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

		protected override ScopeInitializerBase CreateScopeInitializer ()
		{
			return new ScopeInitializer (this);
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

		protected class CapturedParameter : Variable {
			public readonly ScopeInfo Scope;
			public readonly Parameter Parameter;
			public readonly Field Field;
			public readonly int Idx;

			public FieldExpr FieldInstance;

			public CapturedParameter (ScopeInfo scope, Parameter par, int idx)
			{
				this.Scope = scope;
				this.Parameter = par;
				this.Idx = idx;
				Field = scope.CaptureVariable (
					"<p:" + par.Name + ">", par.ParameterType);
			}

			public override Type Type {
				get { return Field.MemberType; }
			}

			public override bool HasInstance {
				get { return true; }
			}

			public override bool NeedsTemporary {
				get { return true; }
			}

			protected FieldInfo GetField (EmitContext ec)
			{
				if ((Scope.Host is IteratorHost) ||
				    (ec.capture_context != Scope.CaptureContext))
					return Field.FieldBuilder;
				else
					return FieldInstance.FieldInfo;
			}

			public override void EmitInstance (EmitContext ec)
			{
				CaptureContext.EmitScopeInstance (ec, Scope);
			}

			public override void Emit (EmitContext ec)
			{
				ec.ig.Emit (OpCodes.Ldfld, GetField (ec));
			}

			public override void EmitAssign (EmitContext ec)
			{
				ec.ig.Emit (OpCodes.Stfld, GetField (ec));
			}

			public override void EmitAddressOf (EmitContext ec)
			{
				ec.ig.Emit (OpCodes.Ldflda, GetField (ec));
			}

			public override string ToString ()
			{
				return String.Format ("{0} ({1})", GetType (), Field);
			}
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
				Field = scope.CaptureVariable (
					scope.MakeFieldName (local.Name), local.VariableType);
			}

			public override Type Type {
				get { return Field.MemberType; }
			}

			public override bool HasInstance {
				get { return true; }
			}

			public override bool NeedsTemporary {
				get { return true; }
			}

			public override void EmitInstance (EmitContext ec)
			{
				CaptureContext.EmitScopeInstance (ec, Scope);
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

		protected class ScopeInitializer : ScopeInitializerBase
		{
			ScopeInfo scope;

			public ScopeInitializer (ScopeInfo scope)
				: base (scope)
			{
				this.scope = scope;
			}

			new public ScopeInfo Scope {
				get { return scope; }
			}

			protected override bool DoResolveInternal (EmitContext ec)
			{
				Report.Debug (64, "RESOLVE SCOPE INITIALIZER", this, Scope,
					      Scope.ParentScope, Scope.ScopeType, ec,
					      ec.TypeContainer.Name, ec.DeclContainer,
					      ec.DeclContainer.Name, ec.DeclContainer.IsGeneric);

				foreach (CapturedLocal local in Scope.locals) {
					FieldExpr fe = (FieldExpr) Expression.MemberLookup (
						ec.ContainerType, type, local.Field.Name, loc);
					Report.Debug (64, "RESOLVE SCOPE INITIALIZER #2", this, Scope,
						      Scope, ec, ec.ContainerType, type,
						      local.Field, local.Field.Name, loc, fe);
					if (fe == null)
						throw new InternalErrorException ();

					fe.InstanceExpression = this;
					local.FieldInstance = fe;
				}

				return base.DoResolveInternal (ec);
			}

			protected virtual void EmitParameterReference (EmitContext ec,
								       CapturedParameter cp)
			{
				int extra = ec.IsStatic ? 0 : 1;
				ParameterReference.EmitLdArg (ec.ig, cp.Idx + extra);
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
		public readonly ToplevelBlock ToplevelOwner;
		public readonly IAnonymousContainer Host;

		//
		// All the scopes we capture
		//
		Hashtable scopes = new Hashtable ();

		//
		// All the root scopes
		//
		ArrayList roots = new ArrayList ();
		
		bool have_captured_vars = false;

		//
		// Captured fields
		//
		Hashtable captured_fields = new Hashtable ();
		Hashtable captured_variables = new Hashtable ();

		public CaptureContext (ToplevelBlock toplevel_owner, Location loc,
				       IAnonymousContainer host)
		{
			cc_id = count++;
			this.ToplevelOwner = toplevel_owner;
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
				return ToplevelOwner.Container;
			}
		}

		public CaptureContext ParentCaptureContext {
			get {
				ToplevelBlock parent = ParentToplevel;
				
				return (parent == null) ? null : parent.CaptureContext;
			}
		}

		// The method scope
		ScopeInfo method_scope;
		bool computed_method_scope = false;
		
		//
		// Track the scopes that this method has used.  At the
		// end this is used to determine the ScopeInfo that will
		// host the method
		//
		ArrayList scopes_used = new ArrayList ();
		
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

			Report.Debug (64, "DEEPEST", this, a, a.ParentScope, b, b.ParentScope);

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

			LinkScopes ();
			
			method_scope = null;
			int top = scopes_used.Count;
			computed_method_scope = true;

			if (top == 0)
				return;
			
			method_scope = (ScopeInfo) scopes_used [0];
			if (top == 1)
				return;

			Report.Debug (64, "COMPUTE METHOD HOST #1", this, method_scope,
				      scopes_used);
			
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
				throw new InternalErrorException (
					"AnonymousContainer.Scope is being used before its " +
					"container is computed");
			}
		}

		internal AnonymousMethodHost CreateRootScope (TypeContainer parent, GenericMethod generic)
		{
			Report.Debug (64, "CREATE ROOT SCOPE", this, parent.Name, ToplevelOwner,
				      ToplevelOwner.ScopeInfo);

			AnonymousMethodHost root_scope = new AnonymousMethodHost (
				this, ToplevelOwner, parent, generic);
			AddRootScope (root_scope);
			return root_scope;
		}

		internal void AddRootScope (ScopeInfo root_scope)
		{
			if (ToplevelOwner.ScopeInfo != null)
				throw new InternalErrorException ();

			ToplevelOwner.ScopeInfo = root_scope;
			scopes.Add (ToplevelOwner.ID, root_scope);
			RegisterScope (root_scope);
		}

		internal ScopeInfo GetScopeForBlock (Block block)
		{
			while (block.Implicit)
				block = block.Parent;

			ScopeInfo si = (ScopeInfo) scopes [block.ID];
			Report.Debug (64, "GET SCOPE FOR BLOCK", this, block,
				      block.ScopeInfo, block.Parent, si);
			if (si != null)
				return si;

			block.ScopeInfo = si = new ScopeInfo (this, block);
			scopes [block.ID] = si;
			return si;
		}

		public Variable AddLocal (LocalInfo li)
		{
			Report.Debug (64, "ADD LOCAL", this, li.Name, loc, li.Block,
				      li.Block.Toplevel, ToplevelOwner);

			if (li.Block.Toplevel != ToplevelOwner)
				return ParentCaptureContext.AddLocal (li);

			ScopeInfo scope = GetScopeForBlock (li.Block);

			//
			// Adjust the owner
			//
			RegisterScope (scope);

			Variable var = (Variable) captured_variables [li];
			Report.Debug (64, "ADD LOCAL #1", this, li.Name, scope, var);
			if (var == null) {
				var = scope.AddLocal (li);
				captured_variables.Add (li, var);
			}

			have_captured_vars = true;
			return var;
		}

		//
		// Captured fields are only recorded on the topmost CaptureContext, because that
		// one is the one linked to the owner of instance fields
		//
		public void AddField (EmitContext ec, FieldExpr fe)
		{
			if (fe.FieldInfo.IsStatic)
				throw new InternalErrorException (
					"Attempt to register a static field as a captured field");

			CaptureContext parent = ParentCaptureContext;
			if (parent != null) {
				parent.AddField (ec, fe);
				return;
			}

			ScopeInfo scope = GetScopeForBlock (ToplevelOwner);
			RegisterScope (scope);
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

		public Variable GetCapturedVariable (LocalInfo local)
		{
			return (Variable) captured_variables [local];
		}

		//
		// Returns whether the parameter is captured
		//
		public bool IsParameterCaptured (string name)
		{
			if ((ParentCaptureContext != null) &&
			    ParentCaptureContext.IsParameterCaptured (name))
				return true;

			return ToplevelOwner.AnonymousMethodHost.IsParameterCaptured (name);
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

		public static void EmitScopeInstance (EmitContext ec, ScopeInfo scope)
		{
			AnonymousMethodHost root_scope = ec.CurrentBlock.Toplevel.AnonymousMethodHost;

			Report.Debug (64, "EMIT SCOPE INSTANCE", root_scope, scope, scope.Host);

			root_scope.EmitScopeInstance (ec);
			while (root_scope != scope.Host) {
				ec.ig.Emit (OpCodes.Ldfld, root_scope.ParentLink.FieldBuilder);
				root_scope = root_scope.ParentHost;

				if (root_scope == null)
					throw new InternalErrorException (
						"Never found scope {0} starting at block {1}",
						scope, ec.CurrentBlock.ID);
			}

			if (scope != scope.Host)
				ec.ig.Emit (OpCodes.Ldfld, scope.ScopeInstance.FieldBuilder);
		}

		public void RegisterCaptureContext ()
		{
			ToplevelOwner.RegisterCaptureContext (this);
		}

		//
		// Returs true if `probe' is an ancestor of `scope' in the 
		// scope chain
		//
		bool IsAncestor (ScopeInfo probe, ScopeInfo scope)
		{
			Report.Debug (64, "IS ANCESTOR", scope, scope.ScopeBlock,
				      scope.ScopeBlock.Parent, probe, probe.ScopeBlock);
			for (Block b = scope.ScopeBlock.Parent; b != null; b = b.Parent){
				Report.Debug (64, "IS ANCESTOR #1", b, probe.ScopeBlock);
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

			foreach (ScopeInfo si in scope_list) {
				if (!si.Define ())
					throw new InternalErrorException ();
				if (si.DefineType () == null)
					throw new InternalErrorException ();
			}
		}
	}
}
