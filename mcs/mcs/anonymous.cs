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
			string name = MakeName ("CompilerGenerated");
			if (generic != null) {
				TypeArguments args = new TypeArguments (loc);
				foreach (TypeParameter tparam in generic.CurrentTypeParameters)
					args.Add (new SimpleName (tparam.Name, loc));
				return new MemberName (name, args, loc);
			} else
				return new MemberName (name, loc);
		}

		public static string MakeName (string prefix)
		{
			return "<>c__" + prefix + next_index++;
		}

		protected CompilerGeneratedClass (DeclSpace parent, GenericMethod generic,
						  int mod, Location loc)
			: base (parent.NamespaceEntry, parent,
				MakeProxyName (generic, loc), mod | Modifiers.COMPILER_GENERATED, null)
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

			parent.PartialContainer.AddCompilerGeneratedClass (this);
		}

		protected override bool DefineNestedTypes ()
		{
			RootContext.RegisterCompilerGeneratedType (TypeBuilder);
			return base.DefineNestedTypes ();
		}

		protected override bool DoDefineMembers ()
		{
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

		protected override bool DoResolveMembers ()
		{
			if (CompilerGenerated != null) {
				foreach (CompilerGeneratedClass c in CompilerGenerated) {
					if (!c.ResolveMembers ())
						return false;
				}
			}

			return base.DoResolveMembers ();
		}

		public GenericMethod GenericMethod {
			get { return generic_method; }
		}

		public Parameters InflateParameters (Parameters ps)
		{
			if (generic_method == null)
				return ps;

			int n = ps.Count;
			if (n == 0)
				return ps;

			Parameter[] inflated_params = new Parameter [n];
			Type[] inflated_types = new Type [n];

			for (int i = 0; i < n; ++i) {
				Parameter p = ps [i];
				Type it = InflateType (p.ExternalType ()).ResolveAsTypeTerminal (this, false).Type;
				inflated_types [i] = it;
				inflated_params [i] = new Parameter (it, p.Name, p.ModFlags, p.OptAttributes, p.Location);
			}
			return new Parameters (inflated_params, inflated_types);
		}

		public TypeExpr InflateType (Type it)
		{
#if GMCS_SOURCE
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
#endif

			return new TypeExpression (it, Location);
		}

		public Field CaptureVariable (string name, TypeExpr type)
		{
			if (members_defined)
				throw new InternalErrorException ("Helper class already defined!");
			if (type == null)
				throw new ArgumentNullException ();

			return new CapturedVariableField (this, name, type);
		}

		bool members_defined;

		internal void CheckMembersDefined ()
		{
			if (members_defined)
				throw new InternalErrorException ("Helper class already defined!");
		}

		protected class CapturedVariableField : Field
		{
			public CapturedVariableField (CompilerGeneratedClass helper, string name,
						      TypeExpr type)
				: base (helper, type, Modifiers.INTERNAL, name, null, helper.Location)
			{
				helper.AddField (this);
			}
		}
	}

	public class ScopeInfo : CompilerGeneratedClass
	{
		protected readonly RootScopeInfo RootScope;
		new public readonly DeclSpace Parent;
		public readonly int ID = ++next_id;
		public Block ScopeBlock;

		static int next_id;

		public static ScopeInfo CreateScope (Block block)
		{
			ToplevelBlock toplevel = block.Toplevel;
			AnonymousContainer ac = toplevel.AnonymousContainer;

			Report.Debug (128, "CREATE SCOPE", block, block.ScopeInfo, toplevel, ac);

			if (ac == null)
				return new ScopeInfo (block, toplevel.RootScope.Parent,
						      toplevel.RootScope.GenericMethod);

			Report.Debug (128, "CREATE SCOPE #1", ac, ac.Host, ac.Scope, ac.Block,
				      ac.Container, ac.ContainerAnonymousMethod,
				      ac.Location);

			Block b;
			ScopeInfo parent = null;

			for (b = ac.Block; b != null; b = b.Parent) {
				if (b.ScopeInfo != null) {
					parent = b.ScopeInfo;
					break;
				}
			}

			Report.Debug (128, "CREATE SCOPE #2", parent);

			ScopeInfo new_scope = new ScopeInfo (block, parent, null);

			Report.Debug (128, "CREATE SCOPE #3", new_scope);

			return new_scope;
		}

		private static int default_modflags (DeclSpace parent)
		{
			return parent is CompilerGeneratedClass ? Modifiers.PUBLIC : Modifiers.PRIVATE;
		}

		protected ScopeInfo (Block block, DeclSpace parent, GenericMethod generic)
			: base (parent, generic, default_modflags (parent), block.StartLocation)
		{
			Parent = parent;
			RootScope = block.Toplevel.RootScope;
			ScopeBlock = block;

			Report.Debug (128, "NEW SCOPE", this, block,
				      block.Parent, block.Toplevel);

			RootScope.AddScope (this);
		}

		protected ScopeInfo (ToplevelBlock toplevel, DeclSpace parent,
				     GenericMethod generic, Location loc)
			: base (parent, generic, default_modflags (parent), loc)
		{
			Parent = parent;
			RootScope = (RootScopeInfo) this;
			ScopeBlock = toplevel;

			Report.Debug (128, "NEW ROOT SCOPE", this, toplevel, loc);
		}

		protected ScopeInitializer scope_initializer;

		Hashtable locals = new Hashtable ();
		Hashtable captured_scopes = new Hashtable ();
		Hashtable captured_params;

		protected CapturedScope[] CapturedScopes {
			get {
				CapturedScope[] list = new CapturedScope [captured_scopes.Count];
				captured_scopes.Values.CopyTo (list, 0);
				return list;
			}
		}

		protected CapturedVariable GetCapturedScope (ScopeInfo scope)
		{
			return (CapturedVariable) captured_scopes [scope];
		}

		protected void EmitScopeInstance (EmitContext ec)
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
			Report.Debug (128, "GET SCOPE INITIALIZER",
				      this, GetType (), scope_initializer, ScopeBlock);

			if (scope_initializer == null) {
				scope_initializer = CreateScopeInitializer ();
				if (scope_initializer.Resolve (ec) == null)
					throw new InternalErrorException ();
			}

			return scope_initializer;
		}

		public Type GetScopeType (EmitContext ec)
		{
			if (!IsGeneric)
				return TypeBuilder;

			TypeArguments targs = new TypeArguments (Location);

			if (ec.DeclContainer.Parent.IsGeneric)
				foreach (TypeParameter t in ec.DeclContainer.Parent.TypeParameters)
					targs.Add (new TypeParameterExpr (t, Location));
			if (ec.DeclContainer.IsGeneric)
				foreach (TypeParameter t in ec.DeclContainer.CurrentTypeParameters)
					targs.Add (new TypeParameterExpr (t, Location));

			Report.Debug (128, "GET SCOPE TYPE", this, TypeBuilder, targs,
				      ec.DeclContainer, ec.DeclContainer.GetType (),
				      ec.DeclContainer.Parent.Name);

			TypeExpr te = new ConstructedType (TypeBuilder, targs, Location);
			te = te.ResolveAsTypeTerminal (ec, false);
			if ((te == null) || (te.Type == null))
				return null;
			return te.Type;
		}

		protected override bool DoDefineMembers ()
		{
			Report.Debug (64, "SCOPE INFO DEFINE MEMBERS", this, GetType (), IsGeneric,
				      Parent.IsGeneric, GenericMethod);

			foreach (CapturedScope child in CapturedScopes) {
				if (!child.DefineMembers ())
					return false;
			}

			return base.DoDefineMembers ();
		}

		protected override bool DoResolveMembers ()
		{
			Report.Debug (64, "SCOPE INFO RESOLVE MEMBERS", this, GetType (), IsGeneric,
				      Parent.IsGeneric, GenericMethod);

			return base.DoResolveMembers ();
		}

		public Variable CaptureScope (ScopeInfo child)
		{
			CheckMembersDefined ();
			Report.Debug (128, "CAPTURE SCOPE", this, GetType (), child, child.GetType ());
			if (child == this)
				throw new InternalErrorException ();
			CapturedScope captured = (CapturedScope) captured_scopes [child];
			if (captured == null) {
				captured = new CapturedScope (this, child);
				captured_scopes.Add (child, captured);
			}
			return captured;
		}

		public Variable AddLocal (LocalInfo local)
		{
			Report.Debug (128, "CAPTURE LOCAL", this, local);
			Variable var = (Variable) locals [local];
			if (var == null) {
				var = new CapturedLocal (this, local);
				locals.Add (local, var);
				local.IsCaptured = true;
			}
			return var;
		}

		public Variable GetCapturedVariable (LocalInfo local)
		{
			return (Variable) locals [local];
		}

		public bool HostsParameters {
			get { return captured_params != null; }
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
				par.IsCaptured = true;
			}

			return var;
		}

		protected string MakeFieldName (string local_name)
		{
			return "<" + ID + ":" + local_name + ">";
		}

		protected virtual ScopeInitializer CreateScopeInitializer ()
		{
			return new ScopeInitializer (this);
		}

		protected abstract class CapturedVariable : Variable
		{
			public readonly ScopeInfo Scope;
			public readonly string Name;

			public FieldExpr FieldInstance;
			protected Field field;

			protected CapturedVariable (ScopeInfo scope, string name)
			{
				this.Scope = scope;
				this.Name = name;
			}

			protected CapturedVariable (ScopeInfo scope, string name, Type type)
				: this (scope, name)
			{
				this.field = scope.CaptureVariable (
					scope.MakeFieldName (name), scope.RootScope.InflateType (type));
			}

			public Field Field {
				get { return field; }
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
				if ((ec.CurrentBlock != null) &&
				    (ec.CurrentBlock.Toplevel != Scope.ScopeBlock.Toplevel))
					return Field.FieldBuilder;
				else
					return FieldInstance.FieldInfo;
			}

			public override void EmitInstance (EmitContext ec)
			{
				if ((ec.CurrentAnonymousMethod != null) &&
				    (ec.CurrentAnonymousMethod.Scope == Scope)) {
					ec.ig.Emit (OpCodes.Ldarg_0);
					return;
				}

				Scope.EmitScopeInstance (ec);
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
		}

		protected class CapturedParameter : CapturedVariable {
			public readonly Parameter Parameter;
			public readonly int Idx;

			public CapturedParameter (ScopeInfo scope, Parameter par, int idx)
				: base (scope, par.Name, par.ParameterType)
			{
				this.Parameter = par;
				this.Idx = idx;
			}

			public override string ToString ()
			{
				return String.Format ("{0} ({1}:{2}:{3})", GetType (), Field,
						      Parameter.Name, Idx);
			}
		}

		protected class CapturedLocal : CapturedVariable {
			public readonly LocalInfo Local;

			public CapturedLocal (ScopeInfo scope, LocalInfo local)
				: base (scope, local.Name, local.VariableType)
			{
				this.Local = local;
			}

			public override string ToString ()
			{
				return String.Format ("{0} ({1}:{2})", GetType (), Field,
						      Local.Name);
			}
		}

		protected class CapturedThis : CapturedVariable {
			public CapturedThis (RootScopeInfo host)
				: base (host, "<>THIS", host.ParentType)
			{ }
		}

		protected class CapturedScope : CapturedVariable {
			public readonly ScopeInfo ChildScope;

			public CapturedScope (ScopeInfo root, ScopeInfo child)
				: base (root, "scope" + child.ID)
			{
				this.ChildScope = child;
			}

			public bool DefineMembers ()
			{
				Type type = ChildScope.IsGeneric ?
					ChildScope.CurrentType : ChildScope.TypeBuilder;
				Report.Debug (128, "CAPTURED SCOPE DEFINE MEMBERS", this, Scope,
					      ChildScope, Name, type);
				if (type == null)
					throw new InternalErrorException ();
				field = Scope.CaptureVariable (
					Scope.MakeFieldName (Name), Scope.InflateType (type));
				return true;
			}

			public override string ToString ()
			{
				return String.Format ("CapturedScope ({1} captured in {0})",
						      Scope, ChildScope);
			}
		}

		static void DoPath (StringBuilder sb, ScopeInfo start)
		{
			sb.Append ((start.ID).ToString ());
		}
		
		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();
			
			sb.Append ("{");
			DoPath (sb, this);
			sb.Append ("}");

			return sb.ToString ();
		}

		protected class ScopeInitializer : ExpressionStatement
		{
			ScopeInfo scope;
			CapturedVariable captured_scope;
			LocalBuilder scope_instance;
			ConstructorInfo scope_ctor;

			bool initialized;

			public ScopeInitializer (ScopeInfo scope)
			{
				this.scope = scope;
				this.loc = scope.Location;
				eclass = ExprClass.Value;
			}

			public ScopeInfo Scope {
				get { return scope; }
			}

			public override Expression DoResolve (EmitContext ec)
			{
				if (scope_ctor != null)
					return this;

				Report.Debug (64, "RESOLVE SCOPE INITIALIZER BASE", this, Scope,
					      ec, ec.CurrentBlock);

				type = Scope.GetScopeType (ec);
				if (type == null)
					throw new InternalErrorException ();

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

				Report.Debug (128, "RESOLVE THE INIT", this, Scope, Scope.RootScope,
					      Scope.RootScope.GetType ());

				ScopeInfo host = Scope.RootScope;
				if ((Scope != host) && (Scope.RootScope is IteratorHost)) {
					captured_scope = host.GetCapturedScope (Scope);
					Type root = host.GetScopeType (ec);
					FieldExpr fe = (FieldExpr) Expression.MemberLookup (
						type, root, captured_scope.Field.Name, loc);
					if (fe == null)
						throw new InternalErrorException ();

					fe.InstanceExpression = this;
					captured_scope.FieldInstance = fe;

					Report.Debug (128, "RESOLVE THE INIT #1", this,
						      captured_scope, fe);
				} else
					scope_instance = ec.ig.DeclareLocal (type);

				foreach (CapturedLocal local in Scope.locals.Values) {
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

				if (Scope.HostsParameters) {
					foreach (CapturedParameter cp in Scope.captured_params.Values) {
						FieldExpr fe = (FieldExpr) Expression.MemberLookup (
							ec.ContainerType, type, cp.Field.Name, loc);
						if (fe == null)
							throw new InternalErrorException ();

						fe.InstanceExpression = this;
						cp.FieldInstance = fe;
					}
				}

				foreach (CapturedScope scope in Scope.CapturedScopes) {
					FieldExpr fe = (FieldExpr) Expression.MemberLookup (
						ec.ContainerType, type, scope.Field.Name, loc);
					Report.Debug (64, "RESOLVE SCOPE INITIALIZER #3", this, Scope,
						      scope, ec, ec.ContainerType, type,
						      scope.Field, scope.Field.Name, loc, fe);
					if (fe == null)
						throw new InternalErrorException ();

					fe.InstanceExpression = this;
					scope.FieldInstance = fe;
				}

				return true;
			}

			protected virtual void EmitParameterReference (EmitContext ec,
								       CapturedParameter cp)
			{
				int extra = ec.MethodIsStatic ? 0 : 1;
				ParameterReference.EmitLdArg (ec.ig, cp.Idx + extra);
			}

			static int next_id;
			int id = ++next_id;

			protected virtual void DoEmit (EmitContext ec)
			{
				if ((ec.CurrentBlock != null) &&
				    (ec.CurrentBlock.Toplevel != Scope.ScopeBlock.Toplevel)) {
					ec.ig.Emit (OpCodes.Ldarg_0);

					if (ec.CurrentAnonymousMethod != null) {
						ScopeInfo host = ec.CurrentAnonymousMethod.Scope;
						Variable captured = host.GetCapturedScope (scope);
						Report.Debug (128, "EMIT SCOPE INSTANCE #2",
							      ec.CurrentAnonymousMethod, host,
							      scope, captured);
						if (captured != null)
							captured.Emit (ec);
					}
				} else if (scope_instance != null)
					ec.ig.Emit (OpCodes.Ldloc, scope_instance);
				else {
					Report.Debug (128, "DO EMIT", this, Scope, ec,
						      scope_instance, captured_scope);
					captured_scope.EmitInstance (ec);
					captured_scope.Emit (ec);
				}
			}

			protected void DoEmitInstance (EmitContext ec)
			{
				Report.Debug (128, "DO EMIT INSTANCE", this, Scope, ec,
					      scope_instance, captured_scope);

				if (scope_instance != null)
					ec.ig.Emit (OpCodes.Ldloc, scope_instance);
				else
					captured_scope.EmitInstance (ec);
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
				Report.Debug (128, "EMIT SCOPE INITIALIZER STATEMENT", this, id,
					      Scope, scope_instance, ec);

				ec.ig.Emit (OpCodes.Nop);
				ec.ig.Emit (OpCodes.Ldc_I4, id);
				ec.ig.Emit (OpCodes.Pop);
				ec.ig.Emit (OpCodes.Nop);

				if (scope_instance == null)
					ec.ig.Emit (OpCodes.Ldarg_0);
				EmitScopeConstructor (ec);
				if (scope_instance != null)
					ec.ig.Emit (OpCodes.Stloc, scope_instance);
				else
					captured_scope.EmitAssign (ec);

				if (Scope.HostsParameters) {
					foreach (CapturedParameter cp in Scope.captured_params.Values) {
						Report.Debug (128, "EMIT SCOPE INIT #6", this,
							      ec, ec.IsStatic, Scope, cp, cp.Field.Name);
						DoEmitInstance (ec);
						EmitParameterReference (ec, cp);
						ec.ig.Emit (OpCodes.Stfld, cp.FieldInstance.FieldInfo);
					}
				}

				if (Scope is IteratorHost)
					return;

				foreach (CapturedScope scope in Scope.CapturedScopes) {
					ScopeInfo child = scope.ChildScope;

					Report.Debug (128, "EMIT SCOPE INIT #5", this, Scope,
						      scope.Scope, scope.ChildScope);

					ExpressionStatement init = child.GetScopeInitializer (ec);
					init.EmitStatement (ec);

					DoEmit (ec);
					scope.ChildScope.EmitScopeInstance (ec);
					scope.EmitAssign (ec);
				}
			}
		}
	}

	public class RootScopeInfo : ScopeInfo
	{
		public RootScopeInfo (ToplevelBlock toplevel, DeclSpace parent,
				      GenericMethod generic, Location loc)
			: base (toplevel, parent, generic, loc)
		{
			scopes = new ArrayList ();
		}

		TypeExpr parent_type;
		CapturedVariableField parent_link;
		CapturedThis this_variable;
		protected ArrayList scopes;

		public virtual bool IsIterator {
			get { return false; }
		}

		public RootScopeInfo ParentHost {
			get { return Parent.PartialContainer as RootScopeInfo; }
		}

		public Type ParentType {
			get { return parent_type.Type; }
		}

		public Field ParentLink {
			get { return parent_link; }
		}

		protected CapturedThis THIS {
			get { return this_variable; }
		}

		public Variable CaptureThis ()
		{
			if (ParentHost != null)
				return ParentHost.CaptureThis ();

			CheckMembersDefined ();
			if (this_variable == null)
				this_variable = new CapturedThis (this);
			return this_variable;
		}

		public void AddScope (ScopeInfo scope)
		{
			scopes.Add (scope);
		}

		bool linked;
		public void LinkScopes ()
		{
			Report.Debug (128, "LINK SCOPES", this, linked, scopes);

			if (linked)
				return;

			linked = true;
			if (ParentHost != null)
				ParentHost.LinkScopes ();

			foreach (ScopeInfo si in scopes) {
				if (!si.Define ())
					throw new InternalErrorException ();
				if (si.DefineType () == null)
					throw new InternalErrorException ();
				if (!si.ResolveType ())
					throw new InternalErrorException ();
			}

			foreach (ScopeInfo si in scopes) {
				if (!si.ResolveMembers ())
					throw new InternalErrorException ();
				if (!si.DefineMembers ())
					throw new InternalErrorException ();
			}
		}

		protected override ScopeInitializer CreateScopeInitializer ()
		{
			return new RootScopeInitializer (this);
		}

		protected override bool DefineNestedTypes ()
		{
			if (Parent.IsGeneric) {
				parent_type = new ConstructedType (
					Parent.TypeBuilder, Parent.TypeParameters, Location);
				parent_type = parent_type.ResolveAsTypeTerminal (this, false);
				if ((parent_type == null) || (parent_type.Type == null))
					return false;
			} else {
				parent_type = new TypeExpression (Parent.TypeBuilder, Location);
			}

			CompilerGeneratedClass parent = Parent.PartialContainer as CompilerGeneratedClass;
			if (parent != null)
				parent_link = new CapturedVariableField (this, "<>parent", parent_type);

			return base.DefineNestedTypes ();
		}

		protected override bool DoDefineMembers ()
		{
			ArrayList args = new ArrayList ();
			if (this is IteratorHost)
				args.Add (new Parameter (
					TypeManager.int32_type, "$PC", Parameter.Modifier.NONE,
					null, Location));

			Field pfield;
			if (Parent is CompilerGeneratedClass)
				pfield = parent_link;
			else
				pfield = this_variable !=  null ? this_variable.Field : null;
			if (pfield != null)
				args.Add (new Parameter (
					pfield.MemberType, "parent", Parameter.Modifier.NONE,
					null, Location));

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

			Field pfield;
			if (Parent is CompilerGeneratedClass)
				pfield = parent_link;
			else
				pfield = this_variable !=  null ? this_variable.Field : null;

			if (pfield != null) {
				ec.ig.Emit (OpCodes.Ldarg_0);
				ec.ig.Emit (OpCodes.Ldarg, pos);
				ec.ig.Emit (OpCodes.Stfld, pfield.FieldBuilder);
				pos++;
			}
		}

		protected class TheCtor : Statement
		{
			RootScopeInfo host;

			public TheCtor (RootScopeInfo host)
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

		protected class RootScopeInitializer : ScopeInitializer
		{
			RootScopeInfo host;

			public RootScopeInitializer (RootScopeInfo host)
				: base (host)
			{
				this.host = host;
			}

			public RootScopeInfo Host {
				get { return host; }
			}

			protected override bool DoResolveInternal (EmitContext ec)
			{
				Report.Debug (64, "RESOLVE ANONYMOUS METHOD HOST INITIALIZER",
					      this, Host, Host.ParentType, loc);

				if (Host.THIS != null) {
					FieldExpr fe = (FieldExpr) Expression.MemberLookup (
						ec.ContainerType, type, Host.THIS.Field.Name, loc);
					if (fe == null)
						throw new InternalErrorException ();

					fe.InstanceExpression = this;
					Host.THIS.FieldInstance = fe;
				}

				return base.DoResolveInternal (ec);
			}

			protected virtual bool IsGetEnumerator {
				get { return false; }
			}

			protected override void EmitScopeConstructor (EmitContext ec)
			{
				if (host.THIS != null) {
					ec.ig.Emit (OpCodes.Ldarg_0);
					if (IsGetEnumerator)
						ec.ig.Emit (OpCodes.Ldfld, host.THIS.Field.FieldBuilder);
					else if (host.THIS.Type.IsValueType)
						Expression.LoadFromPtr (ec.ig, host.THIS.Type);
				} else if (host.ParentLink != null)
					ec.ig.Emit (OpCodes.Ldarg_0);

				base.EmitScopeConstructor (ec);
			}
		}

	}

	public interface IAnonymousContainer
	{
		Block Container {
			get;
		}

		GenericMethod GenericMethod {
			get;
		}

		RootScopeInfo RootScope {
			get;
		}

		bool IsIterator {
			get;
		}
	}

	public interface IAnonymousHost
	{
		//
		// Invoked if a yield statement is found in the body
		//
		void SetYields ();

		//
		// Invoked if an anonymous method is found in the body
		//
		void AddAnonymousMethod (AnonymousMethodExpression anonymous);
	}

	public class AnonymousMethodExpression : Expression, IAnonymousContainer, IAnonymousHost
	{
		public readonly AnonymousMethodExpression Parent;
		public readonly TypeContainer Host;
		public Parameters Parameters;

		public ToplevelBlock Block;
		protected AnonymousMethod anonymous;

		protected Block container;
		protected readonly GenericMethod generic;

		public Block Container {
			get { return container; }
		}

		public GenericMethod GenericMethod {
			get { return generic; }
		}

		public AnonymousMethod AnonymousMethod {
			get { return anonymous; }
		}

		public RootScopeInfo RootScope {
			get { return root_scope; }
		}

		public AnonymousMethodExpression (AnonymousMethodExpression parent,
						  GenericMethod generic, TypeContainer host,
						  Parameters parameters, Block container,
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

			if (parent != null)
				parent.AddAnonymousMethod (this);
		}

		ArrayList children;
		RootScopeInfo root_scope;

		static int next_index;

		void IAnonymousHost.SetYields ()
		{
			throw new InvalidOperationException ();
		}

		public void AddAnonymousMethod (AnonymousMethodExpression anonymous)
		{
			if (children == null)
				children = new ArrayList ();
			children.Add (anonymous);
		}

		public bool CreateAnonymousHelpers ()
		{
			Report.Debug (64, "ANONYMOUS METHOD EXPRESSION CREATE ROOT SCOPE",
				      this, Host, container, loc);

			if (container != null)
				root_scope = container.Toplevel.CreateRootScope (Host);

			if (children != null) {
				foreach (AnonymousMethodExpression child in children) {
					if (!child.CreateAnonymousHelpers ())
						return false;
				}
			}

			return true;
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

		public virtual bool ImplicitStandardConversionExists (Type delegate_type)
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

		protected Expression CompatibleChecks (EmitContext ec, Type delegate_type)
		{
			if (!ec.IsAnonymousMethodAllowed) {
				Report.Error (1706, loc,
					      "Anonymous methods are not allowed in the " +
					      "attribute declaration");
				return null;
			}
			
			if (!TypeManager.IsDelegateType (delegate_type)){
				Report.Error (1660, loc,
					      "Cannot convert anonymous method block to type " +
					      "`{0}' because it is not a delegate type",
					      TypeManager.CSharpName (delegate_type));
				return null;
			}
			return this;
		}

		protected bool VerifyExplicitParameterCompatibility (Type delegate_type, ParameterData invoke_pd)
		{
			if (Parameters.Count != invoke_pd.Count) {
				Report.SymbolRelatedToPreviousError (delegate_type);
				Report.Error (1593, loc, "Delegate `{0}' does not take `{1}' arguments",
					      TypeManager.CSharpName (delegate_type), Parameters.Count.ToString ());
				Error_ParameterMismatch (delegate_type);
				return false;
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
					return false;
				}

				// We assume that generic parameters are always inflated
				if (TypeManager.IsGenericParameter (invoke_pd.Types[i]))
					continue;
				
				if (invoke_pd.ParameterType (i) != Parameters.ParameterType (i)) {
					Report.Error (1678, loc, "Parameter `{0}' is declared as type `{1}' but should be `{2}'",
						      (i+1).ToString (),
						      TypeManager.CSharpName (Parameters.ParameterType (i)),
						      TypeManager.CSharpName (invoke_pd.ParameterType (i)));
					Error_ParameterMismatch (delegate_type);
					return false;
				}
			}
			return true;
		}
		
		//
		// Returns true if this anonymous method can be implicitly
		// converted to the delegate type `delegate_type'
		//
		public virtual Expression Compatible (EmitContext ec, Type delegate_type)
		{
			if (anonymous != null)
				return anonymous.AnonymousDelegate;

			if (CompatibleChecks (ec, delegate_type) == null)
				return null;

			
			//
			// At this point its the first time we know the return type that is 
			// needed for the anonymous method.  We create the method here.
			//

			MethodGroupExpr invoke_mg = Delegate.GetInvokeMethod (
				ec.ContainerType, delegate_type, loc);
			MethodInfo invoke_mb = (MethodInfo) invoke_mg.Methods [0];
			ParameterData invoke_pd = TypeManager.GetParameterData (invoke_mb);

#if GMCS_SOURCE
			Type[] infered_arguments = null;
			if (TypeManager.IsGenericType (delegate_type)) {

				Type[] g_arguments = delegate_type.GetGenericArguments ();
				infered_arguments = new Type[g_arguments.Length];
				for (int i = 0; i < g_arguments.Length; ++i) {
					infered_arguments [i] = g_arguments[i];
				}

				for (int i = 0; i < invoke_pd.Count; ++i) {
					if (!invoke_pd.Types[i].IsGenericParameter)
						continue;

					infered_arguments [invoke_pd.Types[i].GenericParameterPosition] = Parameters.Types[i];
				}
			}

			int return_type_pos = -1;
			if (TypeManager.IsGenericParameter (invoke_mb.ReturnType)) {
				ec.InferReturnType = true;
				return_type_pos = invoke_mb.ReturnType.GenericParameterPosition;
			}
#endif

			Parameters parameters;
			if (Parameters == null) {
				//
				// We provide a set of inaccessible parameters
				//
				Parameter [] fixedpars = new Parameter [invoke_pd.Count];
								
				for (int i = 0; i < invoke_pd.Count; i++) {
					Parameter.Modifier i_mod = invoke_pd.ParameterModifier (i);
					if ((i_mod & Parameter.Modifier.OUTMASK) != 0) {
						Report.Error (1688, loc, "Cannot convert anonymous " +
							      "method block without a parameter list " +
							      "to delegate type `{0}' because it has " +
							      "one or more `out' parameters.",
							      TypeManager.CSharpName (delegate_type));
						return null;
					}
					fixedpars [i] = new Parameter (
						invoke_pd.ParameterType (i), "+" + (++next_index),
						invoke_pd.ParameterModifier (i), null, loc);
				}
								
				parameters = new Parameters (fixedpars);
				if (!parameters.Resolve (ec))
					return null;
			} else {
				if (!VerifyExplicitParameterCompatibility (delegate_type, invoke_pd))
					return null;

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
				      TypeManager.IsGenericType (delegate_type), loc);

			anonymous = new AnonymousMethod (
				Parent != null ? Parent.AnonymousMethod : null, RootScope, Host,
				GenericMethod, parameters, Container, Block, invoke_mb.ReturnType,
				delegate_type, loc);

			if (!anonymous.Resolve (ec))
				return null;

#if GMCS_SOURCE
			if (return_type_pos != -1) {
				if (infered_arguments == null)
					infered_arguments = new Type [delegate_type.GetGenericArguments ().Length];

				infered_arguments [return_type_pos] = anonymous.ReturnType;
			}

			if (infered_arguments != null & TypeManager.IsGenericType (delegate_type)) {
				anonymous.AnonymousDelegate.Type = delegate_type.GetGenericTypeDefinition ().MakeGenericType (infered_arguments);
				anonymous.DelegateType = anonymous.AnonymousDelegate.Type;
			}
#endif
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

		protected override void CloneTo (CloneContext clonectx, Expression t)
		{
			AnonymousMethodExpression target = (AnonymousMethodExpression) t;

			target.Block = (ToplevelBlock) clonectx.LookupBlock (Block);
			target.container = clonectx.LookupBlock (Block);
			target.Parameters = Parameters.Clone ();
		}
	}

	public abstract class AnonymousContainer : IAnonymousContainer
	{
		public readonly Location Location;

		public Parameters Parameters;

		//
		// The block that makes up the body for the anonymous mehtod
		//
		public readonly ToplevelBlock Block;

		public readonly int ModFlags;
		public Type ReturnType;
		public readonly DeclSpace Host;

		//
		// The implicit method we create
		//
		protected Method method;
		protected EmitContext aec;

		// The emit context for the anonymous method
		protected bool unreachable;
		protected readonly Block container;
		protected readonly GenericMethod generic;

		//
		// Points to our container anonymous method if its present
		//
		public readonly AnonymousContainer ContainerAnonymousMethod;

		protected AnonymousContainer (AnonymousContainer parent, DeclSpace host,
					      GenericMethod generic, Parameters parameters,
					      Block container, ToplevelBlock block,
					      Type return_type, int mod, Location loc)
		{
			this.ContainerAnonymousMethod = parent;
			this.ReturnType = return_type;
			this.ModFlags = mod;
			this.Host = host;

			this.container = container;
			this.generic = parent != null ? null : generic;
			this.Parameters = parameters;
			this.Block = block;
			this.Location = loc;

			block.AnonymousContainer = this;
		}

		public Method Method {
			get { return method; }
		}

		public abstract RootScopeInfo RootScope {
			get;
		}

		public abstract ScopeInfo Scope {
			get;
		}

		public abstract string GetSignatureForError ();

		public virtual bool ResolveNoDefine (EmitContext ec)
		{
			Report.Debug (64, "RESOLVE ANONYMOUS METHOD", this, Location, ec,
				      RootScope, Parameters, ec.IsStatic);

			if (ReturnType != null) {
				TypeExpr return_type_expr;
				if (RootScope != null)
					return_type_expr = RootScope.InflateType (ReturnType);
				else
					return_type_expr = new TypeExpression (ReturnType, Location);
				return_type_expr = return_type_expr.ResolveAsTypeTerminal (ec, false);
				if ((return_type_expr == null) || (return_type_expr.Type == null))
					return false;
				ReturnType = return_type_expr.Type;
			}

			if (RootScope != null)
				Parameters = RootScope.InflateParameters (Parameters);

			aec = new EmitContext (
				ec.ResolveContext, ec.TypeContainer,
				RootScope != null ? RootScope : Host, Location, null, ReturnType,
				/* REVIEW */ (ec.InIterator ? Modifiers.METHOD_YIELDS : 0) |
				(ec.InUnsafe ? Modifiers.UNSAFE : 0), /* No constructor */ false);

			aec.CurrentAnonymousMethod = this;
			aec.IsFieldInitializer = ec.IsFieldInitializer;
			aec.IsStatic = ec.IsStatic;
			aec.InferReturnType = ec.InferReturnType;

			Report.Debug (64, "RESOLVE ANONYMOUS METHOD #1", this, Location, ec, aec,
				      RootScope, Parameters, Block);

			bool unreachable;
			if (!aec.ResolveTopBlock (ec, Block, Parameters, null, out unreachable))
				return false;

			return true;
		}

		public virtual bool Resolve (EmitContext ec)
		{
			if (!ResolveNoDefine (ec))
				return false;
			
			Report.Debug (64, "RESOLVE ANONYMOUS METHOD #3", this, ec, aec, Block);

			if (aec.InferReturnType)
				ReturnType = aec.ReturnType;

			method = DoCreateMethodHost (ec);

			if (Scope != null)
				return true;

			if (!method.ResolveMembers ())
				return false;
			return method.Define ();
		}

		protected abstract Method DoCreateMethodHost (EmitContext ec);

		public Block Container {
			get { return container; }
		}

		public GenericMethod GenericMethod {
			get { return generic; }
		}

		public abstract bool IsIterator {
			get;
		}

		protected class AnonymousMethodMethod : Method
		{
			public readonly AnonymousContainer AnonymousMethod;
			public readonly ScopeInfo Scope;

			public AnonymousMethodMethod (AnonymousContainer am, ScopeInfo scope,
						      GenericMethod generic, TypeExpr return_type,
						      int mod, MemberName name, Parameters parameters)
				: base (scope != null ? scope : am.Host,
					generic, return_type, mod | Modifiers.COMPILER_GENERATED, false, name, parameters, null)
			{
				this.AnonymousMethod = am;
				this.Scope = scope;

				if (scope != null) {
					scope.CheckMembersDefined ();
					scope.AddMethod (this);
				} else {
					ModFlags |= Modifiers.STATIC;
					am.Host.PartialContainer.AddMethod (this);
				}
				Block = am.Block;
			}

			public override EmitContext CreateEmitContext (DeclSpace tc, ILGenerator ig)
			{
				EmitContext aec = AnonymousMethod.aec;
				aec.ig = ig;
				aec.MethodIsStatic = Scope == null;
				return aec;
			}
		}
	}

	public class AnonymousMethod : AnonymousContainer
	{
		public Type DelegateType;

		//
		// The value return by the Compatible call, this ensure that
		// the code works even if invoked more than once (Resolve called
		// more than once, due to the way Convert.ImplicitConversion works
		//
		Expression anonymous_delegate;
		RootScopeInfo root_scope;
		ScopeInfo scope;

		public AnonymousMethod (AnonymousMethod parent, RootScopeInfo root_scope,
					DeclSpace host, GenericMethod generic,
					Parameters parameters, Block container,
					ToplevelBlock block, Type return_type, Type delegate_type,
					Location loc)
			: base (parent, host, generic, parameters, container, block,
				return_type, 0, loc)
		{
			this.DelegateType = delegate_type;
			this.root_scope = root_scope;
		}

		public override RootScopeInfo RootScope {
			get { return root_scope; }
		}

		public override ScopeInfo Scope {
			get { return scope; }
		}

		public override bool IsIterator {
			get { return false; }
		}

		public Expression AnonymousDelegate {
			get { return anonymous_delegate; }
		}

		public override string GetSignatureForError ()
		{
			return TypeManager.CSharpName (DelegateType);
		}

		//
		// Creates the host for the anonymous method
		//
		protected override Method DoCreateMethodHost (EmitContext ec)
		{
			string name = CompilerGeneratedClass.MakeName ("AnonymousMethod");
			MemberName member_name;

			Report.Debug (128, "CREATE METHOD HOST #0", RootScope);

			Block b;
			scope = RootScope;

			Report.Debug (128, "CREATE METHOD HOST #1", this, Block, Block.ScopeInfo,
				      RootScope, Location);

			for (b = Block.Parent; b != null; b = b.Parent) {
				Report.Debug (128, "CREATE METHOD HOST #2", this, Block,
					      b, b.ScopeInfo);
				if (b.ScopeInfo != null) {
					scope = b.ScopeInfo;
					break;
				}
			}

			if (scope != null)
				scope.CheckMembersDefined ();

			ArrayList scopes = new ArrayList ();
			if (b != null) {
				for (b = b.Parent; b != null; b = b.Parent) {
					if (b.ScopeInfo != null)
						scopes.Add (b.ScopeInfo);
				}
			}

			Report.Debug (128, "CREATE METHOD HOST #1", this, scope, scopes);

			foreach (ScopeInfo si in scopes)
				scope.CaptureScope (si);

			Report.Debug (128, "CREATE METHOD HOST", this, Block, container,
				      RootScope, scope, scopes, Location,
				      ContainerAnonymousMethod);

			GenericMethod generic_method = null;
#if GMCS_SOURCE
			if (TypeManager.IsGenericType (DelegateType)) {
				TypeArguments args = new TypeArguments (Location);

				Type dt = DelegateType.GetGenericTypeDefinition ();

				Type[] tparam = TypeManager.GetTypeArguments (dt);
				for (int i = 0; i < tparam.Length; i++)
					args.Add (new SimpleName (tparam [i].Name, Location));

				member_name = new MemberName (name, args, Location);

				Report.Debug (128, "CREATE METHOD HOST #5", this, DelegateType,
					      TypeManager.GetTypeArguments (DelegateType),
					      dt, tparam, args);

				generic_method = new GenericMethod (
					Host.NamespaceEntry, scope, member_name,
					new TypeExpression (ReturnType, Location), Parameters);

				generic_method.SetParameterInfo (null);
			} else
#endif
				member_name = new MemberName (name, Location);

			return new AnonymousMethodMethod (
				this, scope, generic_method, new TypeExpression (ReturnType, Location),
				Modifiers.INTERNAL, member_name, Parameters);
		}

		bool ResolveAnonymousDelegate (EmitContext ec)
                {
			// If we are inferring the return type, set it to the discovered value.
			if (DelegateType == null){
				DelegateType = aec.ReturnType;
				
				// The special value pointing to our internal type means it failed.
				if (DelegateType == typeof (AnonymousDelegate))
					return false;
			}

                        anonymous_delegate = new AnonymousDelegate (
				this, DelegateType, Location).Resolve (ec);
                        if (anonymous_delegate == null)
                                return false;
			return true;
		}

		public override bool Resolve (EmitContext ec)
		{
			if (!base.Resolve (ec))
				return false;

			return ResolveAnonymousDelegate (ec);
		}

		public override bool ResolveNoDefine (EmitContext ec)
		{
			if (!base.ResolveNoDefine (ec))
				return false;

			return ResolveAnonymousDelegate (ec);
		}

		public MethodInfo GetMethodBuilder (EmitContext ec)
		{
			MethodInfo builder = method.MethodBuilder;
			if ((Scope != null) && Scope.IsGeneric) {
				Type scope_type = Scope.GetScopeType (ec);
				if (scope_type == null)
					throw new InternalErrorException ();

				MethodGroupExpr mg = (MethodGroupExpr) Expression.MemberLookup (
					ec.ContainerType, scope_type, builder.Name, Location);

				if (mg == null)
					throw new InternalErrorException ();
				builder = (MethodInfo) mg.Methods [0];
			}

#if GMCS_SOURCE
			if (!DelegateType.IsGenericType)
				return builder;

			Type[] targs = TypeManager.GetTypeArguments (DelegateType);
			return builder.MakeGenericMethod (targs);
#else
			return builder;
#endif
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

		//
		// if target_type is null, this means that we do not know the type
		// for this delegate, and we want to infer it from the various 
		// returns (implicit and explicit) from the body of this anonymous
		// method.
		//
		// for example, the lambda: x => 1
		//
		public AnonymousDelegate (AnonymousMethod am, Type target_type, Location l)
		{
			type = target_type;
			loc = l;
			this.am = am;
		}

		public override Expression DoResolve (EmitContext ec)
		{
			eclass = ExprClass.Value;

			//
			// If we are inferencing
			//
			if (type == null){
				type = ec.ReturnType;

				// No type was infered
				if (type == null)
					return null;
			}

			return this;
		}
		
		public override void Emit (EmitContext ec)
		{
			//ec.ig.Emit (OpCodes.Ldstr, "EMIT ANONYMOUS DELEGATE");
			//ec.ig.Emit (OpCodes.Pop);

			//
			// Now emit the delegate creation.
			//
			if ((am.Method.ModFlags & Modifiers.STATIC) == 0) {
				Report.Debug (128, "EMIT ANONYMOUS DELEGATE", this, am, am.Scope, loc);
				delegate_instance_expression = am.Scope.GetScopeInitializer (ec);

				if (delegate_instance_expression == null)
					throw new InternalErrorException ();
			}

			Expression ml = Expression.MemberLookup (
				ec.ContainerType, type, ".ctor", MemberTypes.Constructor,
				BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly,
				loc);

			constructor_method = ((MethodGroupExpr) ml).Methods [0];
#if MS_COMPATIBLE
			if (type.IsGenericType)
				constructor_method = TypeBuilder.GetConstructor (type, (ConstructorInfo)constructor_method);
#endif
			
			delegate_method = am.GetMethodBuilder (ec);
			base.Emit (ec);

			//ec.ig.Emit (OpCodes.Ldstr, "EMIT ANONYMOUS DELEGATE DONE");
			//ec.ig.Emit (OpCodes.Pop);

			Report.Debug (128, "EMIT ANONYMOUS DELEGATE DONE", this, am, am.Scope, loc);
		}
	}
	
	public class AnonymousClass : CompilerGeneratedClass
	{
		public AnonymousClass (TypeContainer parent, Location loc)
			: base (parent, null, 0, loc)
		{
		}
	}
}
