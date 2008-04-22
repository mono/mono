//
// anonymous.cs: Support for anonymous methods
//
// Author:
//   Miguel de Icaza (miguel@ximain.com)
//   Marek Safar (marek.safar@gmail.com)
//
// Dual licensed under the terms of the MIT X11 or GNU GPL
// Copyright 2003-2008 Novell, Inc.
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
			string name = MakeName (null, "CompilerGenerated");
			if (generic != null) {
				TypeArguments args = new TypeArguments (loc);
				foreach (TypeParameter tparam in generic.CurrentTypeParameters)
					args.Add (new SimpleName (tparam.Name, loc));
				return new MemberName (name, args, loc);
			} else
				return new MemberName (name, loc);
		}

		public static string MakeName (string host, string prefix)
		{
			return "<" + host + ">c__" + prefix + next_index++;
		}
		
		public static void Reset ()
		{
			next_index = 0;
		}

		protected CompilerGeneratedClass (DeclSpace parent,
					MemberName name, int mod, Location loc) :
			base (parent.NamespaceEntry, parent, name, mod | Modifiers.COMPILER_GENERATED, null)
		{
			parent.PartialContainer.AddCompilerGeneratedClass (this);
		}

		protected CompilerGeneratedClass (DeclSpace parent, GenericMethod generic,
						  int mod, Location loc)
			: this (parent, MakeProxyName (generic, loc), mod, loc)
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
			return Parameters.CreateFullyResolved (inflated_params, inflated_types);
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
		public readonly Block ScopeBlock;
		protected ScopeInitializer scope_initializer;

		readonly Hashtable locals = new Hashtable ();
		readonly Hashtable captured_scopes = new Hashtable ();
		Hashtable captured_params;

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
				      ac.Container,
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

		public override void EmitType ()
		{
			SymbolWriter.DefineAnonymousScope (ID);
			foreach (CapturedLocal local in locals.Values)
				local.EmitSymbolInfo ();

			if (captured_params != null) {
				foreach (CapturedParameter param in captured_params.Values)
					param.EmitSymbolInfo ();
			}

			foreach (CapturedScope scope in CapturedScopes) {
				scope.EmitSymbolInfo ();
			}

			base.EmitType ();
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

			public abstract void EmitSymbolInfo ();

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

			public override void EmitSymbolInfo ()
			{
				SymbolWriter.DefineCapturedParameter (
					Scope.ID, Parameter.Name, Field.Name);
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

			public override void EmitSymbolInfo ()
			{
				SymbolWriter.DefineCapturedLocal (
					Scope.ID, Local.Name, Field.Name);
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

			public override void EmitSymbolInfo ()
			{
				SymbolWriter.DefineCapturedThis (Scope.ID, Field.Name);
			}
		}

		protected class CapturedScope : CapturedVariable {
			public readonly ScopeInfo ChildScope;

			public CapturedScope (ScopeInfo root, ScopeInfo child)
				: base (root, "scope" + child.ID)
			{
				this.ChildScope = child;
			}

			public override void EmitSymbolInfo ()
			{
				SymbolWriter.DefineCapturedScope (Scope.ID, ChildScope.ID, Field.Name);
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
				MethodGroupExpr mg = (MethodGroupExpr) MemberLookupFinal (
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
				} else {
					scope_instance = ec.ig.DeclareLocal (type);
					if (!Scope.RootScope.IsIterator)
						SymbolWriter.DefineScopeVariable (Scope.ID, scope_instance);
				}

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

		public override void EmitType ()
		{
			base.EmitType ();
			if (THIS != null)
				THIS.EmitSymbolInfo ();
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
		public readonly Parameters Parameters;

		public ToplevelBlock Block;

		protected Block container;
		protected readonly GenericMethod generic;

		public Block Container {
			get { return container; }
		}

		public GenericMethod GenericMethod {
			get { return generic; }
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
			// FIXME: this polutes expression trees implementation

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

		public virtual bool HasExplicitParameters {
			get {
				return Parameters != null;
			}
		}

		//
		// Returns true if the body of lambda expression can be implicitly
		// converted to the delegate of type `delegate_type'
		//
		public bool ImplicitStandardConversionExists (Type delegate_type)
		{
			EmitContext ec = EmitContext.TempEc;
			using (ec.Set (EmitContext.Flags.ProbingMode)) {
				return Compatible (ec, delegate_type) != null;
			}
		}

		protected Type CompatibleChecks (EmitContext ec, Type delegate_type)
		{
			if (!ec.IsAnonymousMethodAllowed) {
				Report.Error (1706, loc, "Anonymous methods and lambda expressions cannot be used in the current context");
				return null;
			}
			
			if (TypeManager.IsDelegateType (delegate_type))
				return delegate_type;

#if GMCS_SOURCE
			if (TypeManager.DropGenericTypeArguments (delegate_type) == TypeManager.expression_type) {
				delegate_type = TypeManager.GetTypeArguments (delegate_type) [0];
				if (TypeManager.IsDelegateType (delegate_type))
					return delegate_type;

				Report.Error (835, loc, "Cannot convert `{0}' to an expression tree of non-delegate type `{1}'",
					GetSignatureForError (), TypeManager.CSharpName (delegate_type));
				return null;
			}
#endif

			Report.Error (1660, loc, "Cannot convert `{0}' to non-delegate type `{1}'",
				      GetSignatureForError (), TypeManager.CSharpName (delegate_type));
			return null;
		}

		protected bool VerifyExplicitParameters (Type delegate_type, ParameterData parameters, bool ignore_error)
		{
			if (VerifyParameterCompatibility (delegate_type, parameters, ignore_error))
				return true;

			if (!ignore_error)
				Report.Error (1661, loc,
					"Cannot convert `{0}' to delegate type `{1}' since there is a parameter mismatch",
					GetSignatureForError (), TypeManager.CSharpName (delegate_type));

			return false;
		}

		protected bool VerifyParameterCompatibility (Type delegate_type, ParameterData invoke_pd, bool ignore_errors)
		{
			if (Parameters.Count != invoke_pd.Count) {
				if (ignore_errors)
					return false;
				
				Report.Error (1593, loc, "Delegate `{0}' does not take `{1}' arguments",
					      TypeManager.CSharpName (delegate_type), Parameters.Count.ToString ());
				return false;
			}
			
			if (!HasExplicitParameters)
				return true;			

			bool error = false;
			for (int i = 0; i < Parameters.Count; ++i) {
				Parameter.Modifier p_mod = invoke_pd.ParameterModifier (i);
				if (Parameters.ParameterModifier (i) != p_mod && p_mod != Parameter.Modifier.PARAMS) {
					if (ignore_errors)
						return false;
					
					if (p_mod == Parameter.Modifier.NONE)
						Report.Error (1677, loc, "Parameter `{0}' should not be declared with the `{1}' keyword",
							      (i + 1).ToString (), Parameter.GetModifierSignature (Parameters.ParameterModifier (i)));
					else
						Report.Error (1676, loc, "Parameter `{0}' must be declared with the `{1}' keyword",
							      (i+1).ToString (), Parameter.GetModifierSignature (p_mod));
					error = true;
					continue;
				}

				Type type = invoke_pd.Types [i];
				
				// We assume that generic parameters are always inflated
				if (TypeManager.IsGenericParameter (type))
					continue;
				
				if (TypeManager.HasElementType (type) && TypeManager.IsGenericParameter (TypeManager.GetElementType (type)))
					continue;
				
				if (invoke_pd.ParameterType (i) != Parameters.ParameterType (i)) {
					if (ignore_errors)
						return false;
					
					Report.Error (1678, loc, "Parameter `{0}' is declared as type `{1}' but should be `{2}'",
						      (i+1).ToString (),
						      TypeManager.CSharpName (Parameters.ParameterType (i)),
						      TypeManager.CSharpName (invoke_pd.ParameterType (i)));
					error = true;
				}
			}

			return !error;
		}

		//
		// Infers type arguments based on explicit arguments
		//
		public bool ExplicitTypeInference (TypeInferenceContext type_inference, Type delegate_type)
		{
			if (!HasExplicitParameters)
				return false;

			if (!TypeManager.IsDelegateType (delegate_type)) {
#if GMCS_SOURCE
				if (TypeManager.DropGenericTypeArguments (delegate_type) != TypeManager.expression_type)
					return false;

				delegate_type = delegate_type.GetGenericArguments () [0];
				if (!TypeManager.IsDelegateType (delegate_type))
					return false;
#else
				return false;
#endif
			}
			
			ParameterData d_params = TypeManager.GetDelegateParameters (delegate_type);
			if (d_params.Count != Parameters.Count)
				return false;

			for (int i = 0; i < Parameters.Count; ++i) {
				Type itype = d_params.Types [i];
				if (!TypeManager.IsGenericParameter (itype)) {
					if (!TypeManager.HasElementType (itype))
						continue;
					
					if (!TypeManager.IsGenericParameter (itype.GetElementType ()))
					    continue;
				}
				type_inference.ExactInference (Parameters.FixedParameters[i].ParameterType, itype);
			}
			return true;
		}

		public Type InferReturnType (EmitContext ec, TypeInferenceContext tic, Type delegate_type)
		{
			AnonymousMethod am;
			using (ec.Set (EmitContext.Flags.ProbingMode | EmitContext.Flags.InferReturnType)) {
				am = CompatibleMethod (ec, tic, GetType (), delegate_type);
			}
			
			if (am == null)
				return null;

			if (am.ReturnType == TypeManager.null_type)
				am.ReturnType = null;

			return am.ReturnType;
		}

		//
		// Returns AnonymousMethod container if this anonymous method
		// expression can be implicitly converted to the delegate type `delegate_type'
		//
		public Expression Compatible (EmitContext ec, Type type)
		{
			Type delegate_type = CompatibleChecks (ec, type);
			if (delegate_type == null)
				return null;

			//
			// At this point its the first time we know the return type that is 
			// needed for the anonymous method.  We create the method here.
			//

			MethodInfo invoke_mb = Delegate.GetInvokeMethod (
				ec.ContainerType, delegate_type);
			Type return_type = TypeManager.TypeToCoreType (invoke_mb.ReturnType);

#if MS_COMPATIBLE
			Type[] g_args = delegate_type.GetGenericArguments ();
			if (return_type.IsGenericParameter)
				return_type = g_args [return_type.GenericParameterPosition];
#endif

			//
			// Second: the return type of the delegate must be compatible with 
			// the anonymous type.   Instead of doing a pass to examine the block
			// we satisfy the rule by setting the return type on the EmitContext
			// to be the delegate type return type.
			//

			Report.Debug (64, "COMPATIBLE", this, Parent, GenericMethod, Host,
				      Container, Block, return_type, delegate_type,
				      TypeManager.IsGenericType (delegate_type), loc);

			try {
				int errors = Report.Errors;
				AnonymousMethod am = CompatibleMethod (ec, null, return_type, delegate_type);
				if (am != null && delegate_type != type && errors == Report.Errors)
					return CreateExpressionTree (ec, delegate_type);

				return am;
			} catch (Exception e) {
				throw new InternalErrorException (e, loc);
			}
		}

		protected virtual Expression CreateExpressionTree (EmitContext ec, Type delegate_type)
		{
			Report.Error (1946, loc, "An anonymous method cannot be converted to an expression tree");
			return null;
		}

		protected virtual Parameters ResolveParameters (EmitContext ec, TypeInferenceContext tic, Type delegate_type)
		{
			ParameterData delegate_parameters = TypeManager.GetDelegateParameters (delegate_type);

			if (Parameters == null) {
				//
				// We provide a set of inaccessible parameters
				//
				Parameter[] fixedpars = new Parameter[delegate_parameters.Count];

				for (int i = 0; i < delegate_parameters.Count; i++) {
					Parameter.Modifier i_mod = delegate_parameters.ParameterModifier (i);
					if ((i_mod & Parameter.Modifier.OUTMASK) != 0) {
						Report.Error (1688, loc, "Cannot convert anonymous " +
								  "method block without a parameter list " +
								  "to delegate type `{0}' because it has " +
								  "one or more `out' parameters.",
								  TypeManager.CSharpName (delegate_type));
						return null;
					}
					fixedpars[i] = new Parameter (
						delegate_parameters.ParameterType (i), "+" + (++next_index),
						delegate_parameters.ParameterModifier (i), null, loc);
				}

				return Parameters.CreateFullyResolved (fixedpars, delegate_parameters.Types);
			}

			if (!VerifyExplicitParameters (delegate_type, delegate_parameters, ec.IsInProbingMode)) {
				return null;
			}

			return Parameters;
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

			// FIXME: The emitted code isn't very careful about reachability
			// so, ensure we have a 'ret' at the end
			if (ec.CurrentBranching != null &&
			    ec.CurrentBranching.CurrentUsageVector.IsUnreachable)
				ec.NeedReturnLabel ();

			return this;
		}

		public override void Emit (EmitContext ec)
		{
			// nothing, as we only exist to not do anything.
		}

		public override string GetSignatureForError ()
		{
			return ExprClassName;
		}

		public bool IsIterator {
			get { return false; }
		}

		protected AnonymousMethod CompatibleMethod (EmitContext ec, TypeInferenceContext tic, Type return_type, Type delegate_type)
		{
			Parameters p = ResolveParameters (ec, tic, delegate_type);
			if (p == null)
				return null;

			ToplevelBlock b = ec.IsInProbingMode ? (ToplevelBlock) Block.PerformClone () : Block;

			AnonymousMethod anonymous = CompatibleMethodFactory (return_type, delegate_type, p, b);
			if (!anonymous.Compatible (ec))
				return null;

			return anonymous;
		}

		protected virtual AnonymousMethod CompatibleMethodFactory (Type return_type, Type delegate_type, Parameters p, ToplevelBlock b)
		{
			return new AnonymousMethod (RootScope, Host,
				GenericMethod, p, Container, b, return_type,
				delegate_type, loc);
		}

		protected override void CloneTo (CloneContext clonectx, Expression t)
		{
			AnonymousMethodExpression target = (AnonymousMethodExpression) t;

			target.Block = (ToplevelBlock) clonectx.LookupBlock (Block);
			target.container = clonectx.LookupBlock (Block);
		}
	}

	public abstract class AnonymousContainer : Expression, IAnonymousContainer
	{
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

		protected AnonymousContainer (DeclSpace host,
					      GenericMethod generic, Parameters parameters,
					      Block container, ToplevelBlock block,
					      Type return_type, int mod, Location loc)
		{
			this.ReturnType = return_type;
			this.ModFlags = mod | Modifiers.COMPILER_GENERATED;
			this.Host = host;

			this.container = container;
			this.generic = generic;
			this.Parameters = parameters;
			this.Block = block;
			this.loc = loc;

			block.AnonymousContainer = this;
		}

		public Method Method {
			get { return method; }
		}

		public abstract string ContainerType {
			get; 
		}

		public abstract RootScopeInfo RootScope {
			get;
		}

		public abstract ScopeInfo Scope {
			get;
		}

		public bool Compatible (EmitContext ec)
		{
			// REFACTOR: The method should be refactor, many of the
			// hacks can be handled in better way

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

			// Linq type inference is done differently
			if (RootScope != null && RootContext.Version != LanguageVersion.LINQ)
				Parameters = RootScope.InflateParameters (Parameters);

			aec = new EmitContext (
				ec.ResolveContext, ec.TypeContainer,
				RootScope != null ? RootScope : Host, Location, null, ReturnType,
				/* REVIEW */ (ec.InIterator ? Modifiers.METHOD_YIELDS : 0) |
				(ec.InUnsafe ? Modifiers.UNSAFE : 0), /* No constructor */ false);

			aec.CurrentAnonymousMethod = this;
			aec.IsStatic = ec.IsStatic;
			
			//
			// HACK: Overwrite parent declaration container to currently resolved.
			// It's required for an anonymous container inside partial class.
			//
			if (RootScope != null)
				aec.DeclContainer.Parent = ec.TypeContainer;

			IDisposable aec_dispose = null;
			EmitContext.Flags flags = 0;
			if (ec.InferReturnType)
				flags |= EmitContext.Flags.InferReturnType;
			
			if (ec.IsInProbingMode)
				flags |= EmitContext.Flags.ProbingMode;
			
			if (ec.IsInFieldInitializer)
				flags |= EmitContext.Flags.InFieldInitializer;
			
			// HACK: Flag with 0 cannot be set 
			if (flags != 0)
				aec_dispose = aec.Set (flags);

			Report.Debug (64, "RESOLVE ANONYMOUS METHOD #1", this, Location, ec, aec,
				      RootScope, Parameters, Block);

			bool unreachable;
			bool res = aec.ResolveTopBlock (ec, Block, Parameters, null, out unreachable);

			if (ec.InferReturnType)
				ReturnType = aec.ReturnType;

			if (aec_dispose != null) {
				aec_dispose.Dispose ();
			}

			return res;
		}

		public virtual bool Define (EmitContext ec)
		{
			Report.Debug (64, "DEFINE ANONYMOUS METHOD #3", this, ec, aec, Block);

			if (aec == null && !Compatible (ec))
				return false;

			// Don't define anything when we are in probing scope (nested anonymous methods)
			if (ec.IsInProbingMode)
				return true;

			method = DoCreateMethodHost (ec);

			if (Scope != null)
				return true;

			if (!method.ResolveMembers ())
				return false;
			return method.Define ();
		}

		protected abstract Method DoCreateMethodHost (EmitContext ec);

		public override void Emit (EmitContext ec)
		{
			throw new NotSupportedException ();
		}

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
			public readonly string RealName;

			public AnonymousMethodMethod (AnonymousContainer am, ScopeInfo scope,
						      GenericMethod generic, TypeExpr return_type,
						      int mod, string real_name, MemberName name,
						      Parameters parameters)
				: base (scope != null ? scope : am.Host,
					generic, return_type, mod | Modifiers.COMPILER_GENERATED, false, name, parameters, null)
			{
				this.AnonymousMethod = am;
				this.Scope = scope;
				this.RealName = real_name;

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

			public override void EmitExtraSymbolInfo ()
			{
				SymbolWriter.SetRealMethodName (RealName);
			}
		}
	}

	public class AnonymousMethod : AnonymousContainer
	{
		Type DelegateType;

		//
		// The value return by the Compatible call, this ensure that
		// the code works even if invoked more than once (Resolve called
		// more than once, due to the way Convert.ImplicitConversion works
		//
		RootScopeInfo root_scope;
		ScopeInfo scope;

		public AnonymousMethod (RootScopeInfo root_scope,
					DeclSpace host, GenericMethod generic,
					Parameters parameters, Block container,
					ToplevelBlock block, Type return_type, Type delegate_type,
					Location loc)
			: base (host, generic, parameters, container, block,
				return_type, 0, loc)
		{
			this.DelegateType = delegate_type;
			this.root_scope = root_scope;
		}

		public override string ContainerType {
			get { return "anonymous method"; }
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

		public override string GetSignatureForError ()
		{
			return TypeManager.CSharpName (DelegateType);
		}

		//
		// Creates the host for the anonymous method
		//
		protected override Method DoCreateMethodHost (EmitContext ec)
		{
			MemberCore mc = ec.ResolveContext as MemberCore;
			string name = CompilerGeneratedClass.MakeName (mc.Name, null);
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
				      RootScope, scope, scopes, Location);

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

			string real_name = String.Format (
				"{0}~{1}{2}", mc.GetSignatureForError (), GetSignatureForError (),
				Parameters.GetSignatureForError ());

			return new AnonymousMethodMethod (
				this, scope, generic_method, new TypeExpression (ReturnType, Location),
				scope == null ? Modifiers.PRIVATE : Modifiers.INTERNAL,
				real_name, member_name, Parameters);
		}

		public override Expression DoResolve (EmitContext ec)
		{
			if (!Define (ec))
				return null;

			return new AnonymousDelegate (this, DelegateType, Location).Resolve (ec);
		}

		public MethodInfo GetMethodBuilder (EmitContext ec)
		{
			MethodInfo builder = method.MethodBuilder;
			if ((Scope != null) && Scope.IsGeneric) {
				Type scope_type = Scope.GetScopeType (ec);
				if (scope_type == null)
					throw new InternalErrorException ();

				MethodGroupExpr mg = (MethodGroupExpr) Expression.MemberLookup (
					ec.ContainerType, scope_type, builder.Name,
					MemberTypes.Method, Expression.AllBindingFlags | BindingFlags.NonPublic, Location);
				
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
		readonly AnonymousMethod am;

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

			constructor_method = Delegate.GetConstructor (ec.ContainerType, type);
#if MS_COMPATIBLE
			if (type.IsGenericType && type is TypeBuilder)
				constructor_method = TypeBuilder.GetConstructor (type, (ConstructorInfo)constructor_method);
#endif
			
			delegate_method = am.GetMethodBuilder (ec);
			base.Emit (ec);

			//ec.ig.Emit (OpCodes.Ldstr, "EMIT ANONYMOUS DELEGATE DONE");
			//ec.ig.Emit (OpCodes.Pop);

			Report.Debug (128, "EMIT ANONYMOUS DELEGATE DONE", this, am, am.Scope, loc);
		}
	}

	//
	// Anonymous type container
	//
	public class AnonymousTypeClass : CompilerGeneratedClass
	{
		static int types_counter;
		public const string ClassNamePrefix = "<>__AnonType";
		public const string SignatureForError = "anonymous type";
		
		readonly ArrayList parameters;

		private AnonymousTypeClass (DeclSpace parent, MemberName name, ArrayList parameters, Location loc)
			: base (parent, name, Modifiers.SEALED, loc)
		{
			this.parameters = parameters;
		}

		public static AnonymousTypeClass Create (TypeContainer parent, ArrayList parameters, Location loc)
		{
			if (RootContext.Version <= LanguageVersion.ISO_2)
				Report.FeatureIsNotAvailable (loc, "anonymous types");
			
			string name = ClassNamePrefix + types_counter++;

			SimpleName [] t_args = new SimpleName [parameters.Count];
			Parameter [] ctor_params = new Parameter [parameters.Count];
			for (int i = 0; i < parameters.Count; ++i) {
				AnonymousTypeParameter p = (AnonymousTypeParameter) parameters [i];

				t_args [i] = new SimpleName ("<" + p.Name + ">__T", p.Location);
				ctor_params [i] = new Parameter (t_args [i], p.Name, 0, null, p.Location);
			}

			//
			// Create generic anonymous type host with generic arguments
			// named upon properties names
			//
			AnonymousTypeClass a_type = new AnonymousTypeClass (parent.NamespaceEntry.SlaveDeclSpace,
				new MemberName (name, new TypeArguments (loc, t_args), loc), parameters, loc);

			if (parameters.Count > 0)
				a_type.SetParameterInfo (null);

			Constructor c = new Constructor (a_type, name, Modifiers.PUBLIC,
				new Parameters (ctor_params), null, loc);
			c.OptAttributes = a_type.GetDebuggerHiddenAttribute ();
			c.Block = new ToplevelBlock (c.Parameters, loc);

			// 
			// Create fields and contructor body with field initialization
			//
			bool error = false;
			for (int i = 0; i < parameters.Count; ++i) {
				AnonymousTypeParameter p = (AnonymousTypeParameter) parameters [i];

				Field f = new Field (a_type, t_args [i], Modifiers.PRIVATE | Modifiers.READONLY,
					"<" + p.Name + ">", null, p.Location);

				if (!a_type.AddField (f)) {
					error = true;
					Report.Error (833, p.Location, "`{0}': An anonymous type cannot have multiple properties with the same name",
						p.Name);
					continue;
				}

				c.Block.AddStatement (new StatementExpression (
					new Assign (new MemberAccess (new This (p.Location), f.Name),
						c.Block.GetParameterReference (p.Name, p.Location))));

				ToplevelBlock get_block = new ToplevelBlock (p.Location);
				get_block.AddStatement (new Return (
					new MemberAccess (new This (p.Location), f.Name), p.Location));
				Accessor get_accessor = new Accessor (get_block, 0, null, p.Location);
				Property prop = new Property (a_type, t_args [i], Modifiers.PUBLIC, false,
					new MemberName (p.Name, p.Location), null, get_accessor, null, false);
				a_type.AddProperty (prop);
			}

			if (error)
				return null;

			a_type.AddConstructor (c);
			return a_type;
		}
		
		public new static void Reset ()
		{
			types_counter = 0;
		}

		protected override bool AddToContainer (MemberCore symbol, string name)
		{
			MemberCore mc = (MemberCore) defined_names [name];

			if (mc == null) {
				defined_names.Add (name, symbol);
				return true;
			}

			Report.SymbolRelatedToPreviousError (mc);
			return false;
		}

		void DefineOverrides ()
		{
			Location loc = Location;

			Method equals = new Method (this, null, TypeManager.system_boolean_expr,
				Modifiers.PUBLIC | Modifiers.OVERRIDE, false, new MemberName ("Equals", loc),
				new Parameters (new Parameter (TypeManager.system_object_expr, "obj", 0, null, loc)),
				GetDebuggerHiddenAttribute ());

			Method tostring = new Method (this, null, TypeManager.system_string_expr,
				Modifiers.PUBLIC | Modifiers.OVERRIDE, false, new MemberName ("ToString", loc),
				Mono.CSharp.Parameters.EmptyReadOnlyParameters, GetDebuggerHiddenAttribute ());

			ToplevelBlock equals_block = new ToplevelBlock (equals.Parameters, loc);
			TypeExpr current_type;
			if (IsGeneric)
				current_type = new ConstructedType (TypeBuilder, TypeParameters, loc);
			else
				current_type = new TypeExpression (TypeBuilder, loc);

			equals_block.AddVariable (current_type, "other", loc);
			LocalVariableReference other_variable = new LocalVariableReference (equals_block, "other", loc);

			MemberAccess system_collections_generic = new MemberAccess (new MemberAccess (
				new SimpleName ("System", loc), "Collections", loc), "Generic", loc);

			Expression rs_equals = null;
			Expression string_concat = new StringConstant ("<empty type>", loc);
			Expression rs_hashcode = new IntConstant (-2128831035, loc);
			for (int i = 0; i < parameters.Count; ++i) {
				AnonymousTypeParameter p = (AnonymousTypeParameter) parameters [i];
				Field f = (Field) Fields [i];

				MemberAccess equality_comparer = new MemberAccess (new MemberAccess (
					system_collections_generic, "EqualityComparer",
						new TypeArguments (loc, new SimpleName (TypeParameters [i].Name, loc)), loc),
						"Default", loc);

				ArrayList arguments_equal = new ArrayList (2);
				arguments_equal.Add (new Argument (new MemberAccess (new This (f.Location), f.Name)));
				arguments_equal.Add (new Argument (new MemberAccess (other_variable, f.Name)));

				Expression field_equal = new Invocation (new MemberAccess (equality_comparer,
					"Equals", loc), arguments_equal);

				ArrayList arguments_hashcode = new ArrayList (1);
				arguments_hashcode.Add (new Argument (new MemberAccess (new This (f.Location), f.Name)));
				Expression field_hashcode = new Invocation (new MemberAccess (equality_comparer,
					"GetHashCode", loc), arguments_hashcode);

				IntConstant FNV_prime = new IntConstant (16777619, loc);				
				rs_hashcode = new Binary (Binary.Operator.Multiply,
					new Binary (Binary.Operator.ExclusiveOr, rs_hashcode, field_hashcode),
					FNV_prime);

				Expression field_to_string = new Conditional (new Binary (Binary.Operator.Inequality,
					new MemberAccess (new This (f.Location), f.Name), new NullLiteral (loc)),
					new Invocation (new MemberAccess (
						new MemberAccess (new This (f.Location), f.Name), "ToString"), null),
					new StringConstant ("<null>", loc));

				if (rs_equals == null) {
					rs_equals = field_equal;
					string_concat = new Binary (Binary.Operator.Addition,
						new StringConstant (p.Name + " = ", loc),
						field_to_string);
					continue;
				}

				//
				// Implementation of ToString () body using string concatenation
				//				
				string_concat = new Binary (Binary.Operator.Addition,
					new Binary (Binary.Operator.Addition,
						string_concat,
						new StringConstant (", " + p.Name + " = ", loc)),
					field_to_string);

				rs_equals = new Binary (Binary.Operator.LogicalAnd, rs_equals, field_equal);
			}

			//
			// Equals (object obj) override
			//
			equals_block.AddStatement (new StatementExpression (
				new Assign (other_variable,
					new As (equals_block.GetParameterReference ("obj", loc),
						current_type, loc), loc)));

			Expression equals_test = new Binary (Binary.Operator.Inequality, other_variable, new NullLiteral (loc));
			if (rs_equals != null)
				equals_test = new Binary (Binary.Operator.LogicalAnd, equals_test, rs_equals);
			equals_block.AddStatement (new Return (equals_test, loc));

			equals.Block = equals_block;
			equals.ResolveMembers ();
			AddMethod (equals);

			//
			// GetHashCode () override
			//
			Method hashcode = new Method (this, null, TypeManager.system_int32_expr,
				Modifiers.PUBLIC | Modifiers.OVERRIDE, false, new MemberName ("GetHashCode", loc),
				Mono.CSharp.Parameters.EmptyReadOnlyParameters, GetDebuggerHiddenAttribute ());

			//
			// Modified FNV with good avalanche behavior and uniform
			// distribution with larger hash sizes.
			//
			// const int FNV_prime = 16777619;
			// int hash = (int) 2166136261;
			// foreach (int d in data)
			//     hash = (hash ^ d) * FNV_prime;
			// hash += hash << 13;
			// hash ^= hash >> 7;
			// hash += hash << 3;
			// hash ^= hash >> 17;
			// hash += hash << 5;

			ToplevelBlock hashcode_block = new ToplevelBlock (loc);
			hashcode_block.AddVariable (TypeManager.system_int32_expr, "hash", loc);
			LocalVariableReference hash_variable = new LocalVariableReference (hashcode_block, "hash", loc);
			hashcode_block.AddStatement (new StatementExpression (
				new Assign (hash_variable, rs_hashcode)));

			hashcode_block.AddStatement (new StatementExpression (
				new CompoundAssign (Binary.Operator.Addition, hash_variable,
					new Binary (Binary.Operator.LeftShift, hash_variable, new IntConstant (13, loc)))));
			hashcode_block.AddStatement (new StatementExpression (
				new CompoundAssign (Binary.Operator.ExclusiveOr, hash_variable,
					new Binary (Binary.Operator.RightShift, hash_variable, new IntConstant (7, loc)))));
			hashcode_block.AddStatement (new StatementExpression (
				new CompoundAssign (Binary.Operator.Addition, hash_variable,
					new Binary (Binary.Operator.LeftShift, hash_variable, new IntConstant (3, loc)))));
			hashcode_block.AddStatement (new StatementExpression (
				new CompoundAssign (Binary.Operator.ExclusiveOr, hash_variable,
					new Binary (Binary.Operator.RightShift, hash_variable, new IntConstant (17, loc)))));
			hashcode_block.AddStatement (new StatementExpression (
				new CompoundAssign (Binary.Operator.Addition, hash_variable,
					new Binary (Binary.Operator.LeftShift, hash_variable, new IntConstant (5, loc)))));

			hashcode_block.AddStatement (new Return (hash_variable, loc));
			hashcode.Block = hashcode_block;
			hashcode.ResolveMembers ();
			AddMethod (hashcode);

			//
			// ToString () override
			//

			ToplevelBlock tostring_block = new ToplevelBlock (loc);
			tostring_block.AddStatement (new Return (string_concat, loc));
			tostring.Block = tostring_block;
			tostring.ResolveMembers ();
			AddMethod (tostring);
		}

		public override bool DefineMembers ()
		{
			DefineOverrides ();

			return base.DefineMembers ();
		}

		Attributes GetDebuggerHiddenAttribute ()
		{
			return new Attributes (new Attribute (null, null,
				"System.Diagnostics.DebuggerHiddenAttribute", null, Location, false));
		}

		public override string GetSignatureForError ()
		{
			return SignatureForError;
		}

		public ArrayList Parameters {
			get {
				return parameters;
			}
		}
	}
}
