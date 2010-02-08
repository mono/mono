//
// anonymous.cs: Support for anonymous methods and types
//
// Author:
//   Miguel de Icaza (miguel@ximain.com)
//   Marek Safar (marek.safar@gmail.com)
//
// Dual licensed under the terms of the MIT X11 or GNU GPL
// Copyright 2003-2008 Novell, Inc.
//

using System;
using System.Text;
using System.Collections;
using System.Collections.Specialized;
using System.Reflection;
using System.Reflection.Emit;

namespace Mono.CSharp {

	public abstract class CompilerGeneratedClass : Class
	{
		public static string MakeName (string host, string typePrefix, string name, int id)
		{
			return "<" + host + ">" + typePrefix + "__" + name + id.ToString ("X");
		}
		
		protected CompilerGeneratedClass (DeclSpace parent, MemberName name, int mod)
			: base (parent.NamespaceEntry, parent, name, mod | Modifiers.COMPILER_GENERATED | Modifiers.SEALED, null)
		{
		}

		protected CompilerGeneratedClass (DeclSpace parent, GenericMethod generic, MemberName name, int mod)
			: this (parent, name, mod)
		{
			if (generic != null) {
				ArrayList list = new ArrayList ();
				foreach (TypeParameter tparam in generic.TypeParameters) {
					if (tparam.Constraints != null)
						list.Add (tparam.Constraints.Clone ());
				}
				SetParameterInfo (list);
			}
		}

		protected void CheckMembersDefined ()
		{
			if (members_defined)
				throw new InternalErrorException ("Helper class already defined!");
		}
	}

	//
	// Anonymous method storey is created when an anonymous method uses
	// variable or parameter from outer scope. They are then hoisted to
	// anonymous method storey (captured)
	//
	public class AnonymousMethodStorey : CompilerGeneratedClass
	{
		class StoreyFieldPair {
			public readonly AnonymousMethodStorey Storey;
			public readonly Field Field;

			public StoreyFieldPair (AnonymousMethodStorey storey, Field field)
			{
				this.Storey = storey;
				this.Field = field;
			}

			public override int GetHashCode ()
			{
				return Storey.ID.GetHashCode ();
			}

			public override bool Equals (object obj)
			{
				return (AnonymousMethodStorey)obj == Storey;
			}
		}

		sealed class HoistedGenericField : Field
		{
			public HoistedGenericField (DeclSpace parent, FullNamedExpression type, int mod, string name,
				  Attributes attrs, Location loc)
				: base (parent, type, mod, new MemberName (name, loc), attrs)
			{
			}

			protected override bool ResolveMemberType ()
			{
				if (!base.ResolveMemberType ())
					return false;

				AnonymousMethodStorey parent = ((AnonymousMethodStorey) Parent).GetGenericStorey ();
				if (parent != null)
					member_type = parent.MutateType (member_type);

				return true;
			}
		}

		//
		// Needed to delay hoisted _this_ initialization. When an anonymous
		// method is used inside ctor and _this_ is hoisted, base ctor has to
		// be called first, otherwise _this_ will be initialized with 
		// uninitialized value.
		//
		sealed class ThisInitializer : Statement
		{
			readonly HoistedThis hoisted_this;

			public ThisInitializer (HoistedThis hoisted_this)
			{
				this.hoisted_this = hoisted_this;
			}

			protected override void DoEmit (EmitContext ec)
			{
				hoisted_this.EmitHoistingAssignment (ec);
			}

			protected override void CloneTo (CloneContext clonectx, Statement target)
			{
				// Nothing to clone
			}

			public override void MutateHoistedGenericType (AnonymousMethodStorey storey)
			{
				// Nothing to mutate
			}
		}

		// Unique storey ID
		public readonly int ID;
		static int unique_id;

		public readonly Block OriginalSourceBlock;

		// A list of StoreyFieldPair with local field keeping parent storey instance
		ArrayList used_parent_storeys;
		ArrayList children_references;

		// A list of hoisted parameters
		protected ArrayList hoisted_params;
		protected ArrayList hoisted_locals;

		// Hoisted this
		protected HoistedThis hoisted_this;

		// Local variable which holds this storey instance
		public LocalTemporary Instance;

		public AnonymousMethodStorey (Block block, TypeContainer parent, MemberBase host, GenericMethod generic, string name)
			: base (parent, generic, MakeMemberName (host, name, generic, block.StartLocation), Modifiers.PRIVATE)
		{
			Parent = parent;
			OriginalSourceBlock = block;
			ID = unique_id++;
		}

		static MemberName MakeMemberName (MemberBase host, string name, GenericMethod generic, Location loc)
		{
			string host_name = host == null ? null : host.Name;
			string tname = MakeName (host_name, "c", name, unique_id);
			TypeArguments args = null;
			if (generic != null) {
				args = new TypeArguments ();
				foreach (TypeParameter tparam in generic.CurrentTypeParameters)
					args.Add (new TypeParameterName (tparam.Name, null, loc));
			}

			return new MemberName (tname, args, loc);
		}

		public void AddCapturedThisField (EmitContext ec)
		{
			TypeExpr type_expr = new TypeExpression (ec.CurrentType, Location);
			Field f = AddCompilerGeneratedField ("<>f__this", type_expr);
			f.Define ();
			hoisted_this = new HoistedThis (this, f);
		}

		public Field AddCapturedVariable (string name, Type type)
		{
			CheckMembersDefined ();

			FullNamedExpression field_type = new TypeExpression (type, Location);
			if (!IsGeneric)
				return AddCompilerGeneratedField (name, field_type);

			const int mod = Modifiers.INTERNAL | Modifiers.COMPILER_GENERATED;
			Field f = new HoistedGenericField (this, field_type, mod, name, null, Location);
			AddField (f);
			return f;
		}

		protected Field AddCompilerGeneratedField (string name, FullNamedExpression type)
		{
			const int mod = Modifiers.INTERNAL | Modifiers.COMPILER_GENERATED;
			Field f = new Field (this, type, mod, new MemberName (name, Location), null);
			AddField (f);
			return f;
		}

		//
		// Creates a link between block and the anonymous method storey
		//
		// An anonymous method can reference variables from any outer block, but they are
		// hoisted in their own ExplicitBlock. When more than one block is referenced we
		// need to create another link between those variable storeys
		//
		public void AddReferenceFromChildrenBlock (ExplicitBlock block)
		{
			if (children_references == null)
				children_references = new ArrayList ();

			if (!children_references.Contains (block))
				children_references.Add (block);
		}

		public void AddParentStoreyReference (AnonymousMethodStorey storey)
		{
			CheckMembersDefined ();

			if (used_parent_storeys == null)
				used_parent_storeys = new ArrayList ();
			else if (used_parent_storeys.IndexOf (storey) != -1)
				return;

			TypeExpr type_expr = new TypeExpression (storey.TypeBuilder, Location);
			Field f = AddCompilerGeneratedField ("<>f__ref$" + storey.ID, type_expr);
			used_parent_storeys.Add (new StoreyFieldPair (storey, f));
		}

		public void CaptureLocalVariable (ResolveContext ec, LocalInfo local_info)
		{
			ec.CurrentBlock.Explicit.HasCapturedVariable = true;
			if (ec.CurrentBlock.Explicit != local_info.Block.Explicit)
				AddReferenceFromChildrenBlock (ec.CurrentBlock.Explicit);

			if (local_info.HoistedVariableReference != null)
				return;

			HoistedVariable var = new HoistedLocalVariable (this, local_info, GetVariableMangledName (local_info));
			local_info.HoistedVariableReference = var;

			if (hoisted_locals == null)
				hoisted_locals = new ArrayList ();

			hoisted_locals.Add (var);
		}

		public void CaptureParameter (ResolveContext ec, ParameterReference param_ref)
		{
			ec.CurrentBlock.Explicit.HasCapturedVariable = true;
			AddReferenceFromChildrenBlock (ec.CurrentBlock.Explicit);

			if (param_ref.GetHoistedVariable (ec) != null)
				return;

			if (hoisted_params == null)
				hoisted_params = new ArrayList (2);

			HoistedVariable expr = new HoistedParameter (this, param_ref);
			param_ref.Parameter.HoistedVariableReference = expr;
			hoisted_params.Add (expr);
		}

		public void ChangeParentStorey (AnonymousMethodStorey parentStorey)
		{
			Parent = parentStorey;
			type_params = null;
		}

		//
		// Initializes all hoisted variables
		//
		public void EmitStoreyInstantiation (EmitContext ec)
		{
			// There can be only one instance variable for each storey type
			if (Instance != null)
				throw new InternalErrorException ();

			SymbolWriter.OpenCompilerGeneratedBlock (ec.ig);

			//
			// Create an instance of storey type
			//
			Expression storey_type_expr;
			if (is_generic) {
				//
				// Use current method type parameter (MVAR) for top level storey only. All
				// nested storeys use class type parameter (VAR)
				//
				TypeParameter[] tparams = ec.CurrentAnonymousMethod != null && ec.CurrentAnonymousMethod.Storey != null ?
					ec.CurrentAnonymousMethod.Storey.TypeParameters :
					ec.CurrentTypeParameters;

				TypeArguments targs = new TypeArguments ();

				if (tparams.Length < CountTypeParameters) {
					TypeParameter[] parent_tparams = ec.MemberContext.CurrentTypeDefinition.TypeParameters;
					for (int i = 0; i < parent_tparams.Length; ++i)
						targs.Add (new TypeParameterExpr (parent_tparams[i], Location));
				}
				
				for (int i = 0; i < tparams.Length; ++i)
					targs.Add (new TypeParameterExpr (tparams[i], Location));

				storey_type_expr = new GenericTypeExpr (TypeBuilder, targs, Location);
			} else {
				storey_type_expr = new TypeExpression (TypeBuilder, Location);
			}

			ResolveContext rc = new ResolveContext (this);
			Expression e = new New (storey_type_expr, null, Location).Resolve (rc);
			e.Emit (ec);

			Instance = new LocalTemporary (storey_type_expr.Type);
			Instance.Store (ec);

			EmitHoistedFieldsInitialization (ec);

			SymbolWriter.DefineScopeVariable (ID, Instance.Builder);
			SymbolWriter.CloseCompilerGeneratedBlock (ec.ig);
		}

		void EmitHoistedFieldsInitialization (EmitContext ec)
		{
			//
			// Initialize all storey reference fields by using local or hoisted variables
			//
			if (used_parent_storeys != null) {
				foreach (StoreyFieldPair sf in used_parent_storeys) {
					//
					// Setting local field
					//
					Expression instace_expr = GetStoreyInstanceExpression (ec);
					FieldExpr f_set_expr = TypeManager.IsGenericType (instace_expr.Type) ?
						new FieldExpr (sf.Field.FieldBuilder, instace_expr.Type, Location) :
						new FieldExpr (sf.Field.FieldBuilder, Location);
					f_set_expr.InstanceExpression = instace_expr;

					SimpleAssign a = new SimpleAssign (f_set_expr, sf.Storey.GetStoreyInstanceExpression (ec));
					if (a.Resolve (new ResolveContext (ec.MemberContext)) != null)
						a.EmitStatement (ec);
				}
			}

			//
			// Define hoisted `this' in top-level storey only 
			//
			if (OriginalSourceBlock.Explicit.HasCapturedThis && !(Parent is AnonymousMethodStorey)) {
				AddCapturedThisField (ec);
				OriginalSourceBlock.AddScopeStatement (new ThisInitializer (hoisted_this));
			}

			//
			// Setting currect anonymous method to null blocks any further variable hoisting
			//
			AnonymousExpression ae = ec.CurrentAnonymousMethod;
			ec.CurrentAnonymousMethod = null;

			if (hoisted_params != null) {
				EmitHoistedParameters (ec, hoisted_params);
			}

			ec.CurrentAnonymousMethod = ae;
		}

		protected virtual void EmitHoistedParameters (EmitContext ec, ArrayList hoisted)
		{
			foreach (HoistedParameter hp in hoisted) {
				hp.EmitHoistingAssignment (ec);
			}
		}

		public override void EmitType ()
		{
			SymbolWriter.DefineAnonymousScope (ID);

			if (hoisted_this != null)
				hoisted_this.EmitSymbolInfo ();

			if (hoisted_locals != null) {
				foreach (HoistedVariable local in hoisted_locals)
					local.EmitSymbolInfo ();
			}

			if (hoisted_params != null) {
				foreach (HoistedParameter param in hoisted_params)
					param.EmitSymbolInfo ();
			}

			if (used_parent_storeys != null) {
				foreach (StoreyFieldPair sf in used_parent_storeys) {
					SymbolWriter.DefineCapturedScope (ID, sf.Storey.ID, sf.Field.Name);
				}
			}

			base.EmitType ();
		}

		public AnonymousMethodStorey GetGenericStorey ()
		{
			DeclSpace storey = this;
			while (storey != null && storey.CurrentTypeParameters == null)
				storey = storey.Parent;

			return storey as AnonymousMethodStorey;
		}

		//
		// Returns a field which holds referenced storey instance
		//
		Field GetReferencedStoreyField (AnonymousMethodStorey storey)
		{
			if (used_parent_storeys == null)
				return null;

			foreach (StoreyFieldPair sf in used_parent_storeys) {
				if (sf.Storey == storey)
					return sf.Field;
			}

			return null;
		}

		//
		// Creates storey instance expression regardless of currect IP
		//
		public Expression GetStoreyInstanceExpression (EmitContext ec)
		{
			AnonymousExpression am = ec.CurrentAnonymousMethod;

			//
			// Access from original block -> storey
			//
			if (am == null)
				return Instance;

			//
			// Access from anonymous method implemented as a static -> storey
			//
			if (am.Storey == null)
				return Instance;

			Field f = am.Storey.GetReferencedStoreyField (this);
			if (f == null) {
				if (am.Storey == this) {
					//
					// Access inside of same storey (S -> S)
					//
					return new CompilerGeneratedThis (TypeBuilder, Location);
				}
				//
				// External field access
				//
				return Instance;
			}

			//
			// Storey was cached to local field
			//
			FieldExpr f_ind = new FieldExpr (f.FieldBuilder, Location);
			f_ind.InstanceExpression = new CompilerGeneratedThis (TypeBuilder, Location);
			return f_ind;
		}

		protected virtual string GetVariableMangledName (LocalInfo local_info)
		{
			//
			// No need to mangle anonymous method hoisted variables cause they
			// are hoisted in their own scopes
			//
			return local_info.Name;
		}

		public HoistedThis HoistedThis {
			get { return hoisted_this; }
		}

		//
		// Mutate type dispatcher
		//
		public Type MutateType (Type type)
		{
#if GMCS_SOURCE
			if (TypeManager.IsGenericType (type))
				return MutateGenericType (type);

			if (TypeManager.IsGenericParameter (type))
				return MutateGenericArgument (type);

			if (type.IsArray)
				return MutateArrayType (type);
#endif
			return type;
		}

		//
		// Changes method type arguments (MVAR) to storey (VAR) type arguments
		//
		public MethodInfo MutateGenericMethod (MethodInfo method)
		{
#if GMCS_SOURCE
			Type [] t_args = TypeManager.GetGenericArguments (method);
			if (TypeManager.IsGenericType (method.DeclaringType)) {
				Type t = MutateGenericType (method.DeclaringType);
				if (t != method.DeclaringType) {
					method = (MethodInfo) TypeManager.DropGenericMethodArguments (method);
					if (method.Module == Module.Builder)
						method = TypeBuilder.GetMethod (t, method);
					else
						method = (MethodInfo) MethodInfo.GetMethodFromHandle (method.MethodHandle, t.TypeHandle);
				}				
			}

			if (t_args == null || t_args.Length == 0)
				return method;

			for (int i = 0; i < t_args.Length; ++i)
				t_args [i] = MutateType (t_args [i]);

			return method.GetGenericMethodDefinition ().MakeGenericMethod (t_args);
#else
			throw new NotSupportedException ();
#endif
		}

		public ConstructorInfo MutateConstructor (ConstructorInfo ctor)
		{
#if GMCS_SOURCE		
			if (TypeManager.IsGenericType (ctor.DeclaringType)) {
				Type t = MutateGenericType (ctor.DeclaringType);
				if (t != ctor.DeclaringType) {
					ctor = (ConstructorInfo) TypeManager.DropGenericMethodArguments (ctor);
					if (ctor.Module == Module.Builder)
						return TypeBuilder.GetConstructor (t, ctor);
						
					return (ConstructorInfo) ConstructorInfo.GetMethodFromHandle (ctor.MethodHandle, t.TypeHandle);
				}
			}
#endif
			return ctor;
		}
		
		public FieldInfo MutateField (FieldInfo field)
		{
#if GMCS_SOURCE
			if (TypeManager.IsGenericType (field.DeclaringType)) {
				Type t = MutateGenericType (field.DeclaringType);
				if (t != field.DeclaringType) {
					field = TypeManager.DropGenericTypeArguments (field.DeclaringType).GetField (field.Name, TypeManager.AllMembers);
					if (t.GetType ().FullName == "System.Reflection.MonoGenericClass")
						return TypeBuilder.GetField (t, field);

					return FieldInfo.GetFieldFromHandle (field.FieldHandle, t.TypeHandle);						
				}
			}
#endif
			return field;
		}		

#if GMCS_SOURCE
		protected Type MutateArrayType (Type array)
		{
			Type element = TypeManager.GetElementType (array);
			if (element.IsArray) {
				element = MutateArrayType (element);
			} else if (TypeManager.IsGenericParameter (element)) {
				element = MutateGenericArgument (element);
			} else if (TypeManager.IsGenericType (element)) {
				element = MutateGenericType (element);
			} else {
				return array;
			}

			int rank = array.GetArrayRank ();
			if (rank == 1)
				return element.MakeArrayType ();

			return element.MakeArrayType (rank);
		}

		protected Type MutateGenericType (Type type)
		{
			Type [] t_args = TypeManager.GetTypeArguments (type);
			if (t_args == null || t_args.Length == 0)
				return type;

			for (int i = 0; i < t_args.Length; ++i)
				t_args [i] = MutateType (t_args [i]);

			return TypeManager.DropGenericTypeArguments (type).MakeGenericType (t_args);
		}
#endif

		//
		// Changes method generic argument (MVAR) to type generic argument (VAR)
		//
		public Type MutateGenericArgument (Type type)
		{
			if (CurrentTypeParameters != null) {
				TypeParameter tp = TypeParameter.FindTypeParameter (CurrentTypeParameters, type.Name);
				if (tp != null)
					return tp.Type;
			}

			return type;
		}

		public ArrayList ReferencesFromChildrenBlock {
			get { return children_references; }
		}

		public static void Reset ()
		{
			unique_id = 0;
		}		
	}

	public abstract class HoistedVariable
	{
		class ExpressionTreeProxy : Expression
		{
			readonly HoistedVariable hv;

			public ExpressionTreeProxy (HoistedVariable hv)
			{
				this.hv = hv;
			}

			public override Expression CreateExpressionTree (ResolveContext ec)
			{
				throw new NotSupportedException ("ET");
			}

			public override Expression DoResolve (ResolveContext ec)
			{
				eclass = ExprClass.Value;
				type = TypeManager.expression_type_expr.Type;
				return this;
			}

			public override void Emit (EmitContext ec)
			{
				ResolveContext rc = new ResolveContext (ec.MemberContext);
				Expression e = hv.GetFieldExpression (ec).CreateExpressionTree (rc);
				// This should never fail
				e = e.Resolve (rc);
				if (e != null)
					e.Emit (ec);
			}
		}
	
		protected readonly AnonymousMethodStorey storey;
		protected Field field;
		Hashtable cached_inner_access; // TODO: Hashtable is too heavyweight
		FieldExpr cached_outer_access;

		protected HoistedVariable (AnonymousMethodStorey storey, string name, Type type)
			: this (storey, storey.AddCapturedVariable (name, type))
		{
		}

		protected HoistedVariable (AnonymousMethodStorey storey, Field field)
		{
			this.storey = storey;
			this.field = field;
		}

		public void AddressOf (EmitContext ec, AddressOp mode)
		{
			GetFieldExpression (ec).AddressOf (ec, mode);
		}

		public Expression CreateExpressionTree (ResolveContext ec)
		{
			return new ExpressionTreeProxy (this);
		}

		public void Emit (EmitContext ec)
		{
			GetFieldExpression (ec).Emit (ec);
		}

		//
		// Creates field access expression for hoisted variable
		//
		protected FieldExpr GetFieldExpression (EmitContext ec)
		{
			if (ec.CurrentAnonymousMethod == null || ec.CurrentAnonymousMethod.Storey == null) {
				if (cached_outer_access != null)
					return cached_outer_access;

				//
				// When setting top-level hoisted variable in generic storey
				// change storey generic types to method generic types (VAR -> MVAR)
				//
				cached_outer_access = storey.MemberName.IsGeneric ?
					new FieldExpr (field.FieldBuilder, storey.Instance.Type, field.Location) :
					new FieldExpr (field.FieldBuilder, field.Location);

				cached_outer_access.InstanceExpression = storey.GetStoreyInstanceExpression (ec);
				return cached_outer_access;
			}

			FieldExpr inner_access;
			if (cached_inner_access != null) {
				inner_access = (FieldExpr) cached_inner_access [ec.CurrentAnonymousMethod];
			} else {
				inner_access = null;
				cached_inner_access = new Hashtable (4);
			}

			if (inner_access == null) {
				inner_access = field.Parent.MemberName.IsGeneric ?
					new FieldExpr (field.FieldBuilder, field.Parent.CurrentType, field.Location) :
					new FieldExpr (field.FieldBuilder, field.Location);
							
				inner_access.InstanceExpression = storey.GetStoreyInstanceExpression (ec);
				cached_inner_access.Add (ec.CurrentAnonymousMethod, inner_access);
			}

			return inner_access;
		}

		public abstract void EmitSymbolInfo ();

		public void Emit (EmitContext ec, bool leave_copy)
		{
			GetFieldExpression (ec).Emit (ec, leave_copy);
		}

		public void EmitAssign (EmitContext ec, Expression source, bool leave_copy, bool prepare_for_load)
		{
			GetFieldExpression (ec).EmitAssign (ec, source, leave_copy, false);
		}
	}

	class HoistedParameter : HoistedVariable
	{
		sealed class HoistedFieldAssign : Assign
		{
			public HoistedFieldAssign (Expression target, Expression source)
				: base (target, source, source.Location)
			{
			}

			protected override Expression ResolveConversions (ResolveContext ec)
			{
				//
				// Implicit conversion check fails for hoisted type arguments
				// as they are of different types (!!0 x !0)
				//
				return this;
			}
		}

		readonly ParameterReference parameter;

		public HoistedParameter (AnonymousMethodStorey scope, ParameterReference par)
			: base (scope, par.Name, par.Type)
		{
			this.parameter = par;
		}

		public HoistedParameter (HoistedParameter hp, string name)
			: base (hp.storey, name, hp.parameter.Type)
		{
			this.parameter = hp.parameter;
		}

		public void EmitHoistingAssignment (EmitContext ec)
		{
			//
			// Remove hoisted redirection to emit assignment from original parameter
			//
			HoistedVariable temp = parameter.Parameter.HoistedVariableReference;
			parameter.Parameter.HoistedVariableReference = null;

			Assign a = new HoistedFieldAssign (GetFieldExpression (ec), parameter);
			if (a.Resolve (new ResolveContext (ec.MemberContext)) != null)
				a.EmitStatement (ec);

			parameter.Parameter.HoistedVariableReference = temp;
		}

		public override void EmitSymbolInfo ()
		{
			SymbolWriter.DefineCapturedParameter (storey.ID, field.Name, field.Name);
		}

		public Field Field {
			get { return field; }
		}
	}

	class HoistedLocalVariable : HoistedVariable
	{
		readonly string name;

		public HoistedLocalVariable (AnonymousMethodStorey scope, LocalInfo local, string name)
			: base (scope, name, local.VariableType)
		{
			this.name = local.Name;
		}

		public override void EmitSymbolInfo ()
		{
			SymbolWriter.DefineCapturedLocal (storey.ID, name, field.Name);
		}
	}

	public class HoistedThis : HoistedVariable
	{
		public HoistedThis (AnonymousMethodStorey storey, Field field)
			: base (storey, field)
		{
		}

		public void EmitHoistingAssignment (EmitContext ec)
		{
			SimpleAssign a = new SimpleAssign (GetFieldExpression (ec), new CompilerGeneratedThis (ec.CurrentType, field.Location));
			if (a.Resolve (new ResolveContext (ec.MemberContext)) != null)
				a.EmitStatement (ec);
		}

		public override void EmitSymbolInfo ()
		{
			SymbolWriter.DefineCapturedThis (storey.ID, field.Name);
		}

		public Field Field {
			get { return field; }
		}
	}

	//
	// Anonymous method expression as created by parser
	//
	public class AnonymousMethodExpression : Expression
	{
		ListDictionary compatibles;
		public ToplevelBlock Block;

		public AnonymousMethodExpression (Location loc)
		{
			this.loc = loc;
			this.compatibles = new ListDictionary ();
		}

		public override string ExprClassName {
			get {
				return "anonymous method";
			}
		}

		public virtual bool HasExplicitParameters {
			get {
				return Parameters != ParametersCompiled.Undefined;
			}
		}
		
		public ParametersCompiled Parameters {
			get { return Block.Parameters; }
		}

		//
		// Returns true if the body of lambda expression can be implicitly
		// converted to the delegate of type `delegate_type'
		//
		public bool ImplicitStandardConversionExists (ResolveContext ec, Type delegate_type)
		{
			using (ec.With (ResolveContext.Options.InferReturnType, false)) {
				using (ec.Set (ResolveContext.Options.ProbingMode)) {
					return Compatible (ec, delegate_type) != null;
				}
			}
		}

		protected Type CompatibleChecks (ResolveContext ec, Type delegate_type)
		{
			if (TypeManager.IsDelegateType (delegate_type))
				return delegate_type;

			if (TypeManager.DropGenericTypeArguments (delegate_type) == TypeManager.expression_type) {
				delegate_type = TypeManager.GetTypeArguments (delegate_type) [0];
				if (TypeManager.IsDelegateType (delegate_type))
					return delegate_type;

				ec.Report.Error (835, loc, "Cannot convert `{0}' to an expression tree of non-delegate type `{1}'",
					GetSignatureForError (), TypeManager.CSharpName (delegate_type));
				return null;
			}

			ec.Report.Error (1660, loc, "Cannot convert `{0}' to non-delegate type `{1}'",
				      GetSignatureForError (), TypeManager.CSharpName (delegate_type));
			return null;
		}

		protected bool VerifyExplicitParameters (ResolveContext ec, Type delegate_type, AParametersCollection parameters)
		{
			if (VerifyParameterCompatibility (ec, delegate_type, parameters, ec.IsInProbingMode))
				return true;

			if (!ec.IsInProbingMode)
				ec.Report.Error (1661, loc,
					"Cannot convert `{0}' to delegate type `{1}' since there is a parameter mismatch",
					GetSignatureForError (), TypeManager.CSharpName (delegate_type));

			return false;
		}

		protected bool VerifyParameterCompatibility (ResolveContext ec, Type delegate_type, AParametersCollection invoke_pd, bool ignore_errors)
		{
			if (Parameters.Count != invoke_pd.Count) {
				if (ignore_errors)
					return false;
				
				ec.Report.Error (1593, loc, "Delegate `{0}' does not take `{1}' arguments",
					      TypeManager.CSharpName (delegate_type), Parameters.Count.ToString ());
				return false;
			}

			bool has_implicit_parameters = !HasExplicitParameters;
			bool error = false;

			for (int i = 0; i < Parameters.Count; ++i) {
				Parameter.Modifier p_mod = invoke_pd.FixedParameters [i].ModFlags;
				if (Parameters.FixedParameters [i].ModFlags != p_mod && p_mod != Parameter.Modifier.PARAMS) {
					if (ignore_errors)
						return false;
					
					if (p_mod == Parameter.Modifier.NONE)
						ec.Report.Error (1677, loc, "Parameter `{0}' should not be declared with the `{1}' keyword",
							      (i + 1).ToString (), Parameter.GetModifierSignature (Parameters.FixedParameters [i].ModFlags));
					else
						ec.Report.Error (1676, loc, "Parameter `{0}' must be declared with the `{1}' keyword",
							      (i+1).ToString (), Parameter.GetModifierSignature (p_mod));
					error = true;
				}

				if (has_implicit_parameters)
					continue;

				Type type = invoke_pd.Types [i];
				
				// We assume that generic parameters are always inflated
				if (TypeManager.IsGenericParameter (type))
					continue;
				
				if (TypeManager.HasElementType (type) && TypeManager.IsGenericParameter (TypeManager.GetElementType (type)))
					continue;
				
				if (invoke_pd.Types [i] != Parameters.Types [i]) {
					if (ignore_errors)
						return false;
					
					ec.Report.Error (1678, loc, "Parameter `{0}' is declared as type `{1}' but should be `{2}'",
						      (i+1).ToString (),
						      TypeManager.CSharpName (Parameters.Types [i]),
						      TypeManager.CSharpName (invoke_pd.Types [i]));
					error = true;
				}
			}

			return !error;
		}

		//
		// Infers type arguments based on explicit arguments
		//
		public bool ExplicitTypeInference (ResolveContext ec, TypeInferenceContext type_inference, Type delegate_type)
		{
			if (!HasExplicitParameters)
				return false;

			if (!TypeManager.IsDelegateType (delegate_type)) {
				if (TypeManager.DropGenericTypeArguments (delegate_type) != TypeManager.expression_type)
					return false;

				delegate_type = TypeManager.GetTypeArguments (delegate_type) [0];
				if (!TypeManager.IsDelegateType (delegate_type))
					return false;
			}
			
			AParametersCollection d_params = TypeManager.GetDelegateParameters (ec, delegate_type);
			if (d_params.Count != Parameters.Count)
				return false;

			for (int i = 0; i < Parameters.Count; ++i) {
				Type itype = d_params.Types [i];
				if (!TypeManager.IsGenericParameter (itype)) {
					if (!TypeManager.HasElementType (itype))
						continue;
					
					if (!TypeManager.IsGenericParameter (TypeManager.GetElementType (itype)))
					    continue;
				}
				type_inference.ExactInference (Parameters.Types [i], itype);
			}
			return true;
		}

		public Type InferReturnType (ResolveContext ec, TypeInferenceContext tic, Type delegate_type)
		{
			AnonymousMethodBody am;
			using (ec.Set (ResolveContext.Options.ProbingMode | ResolveContext.Options.InferReturnType)) {
				am = CompatibleMethod (ec, tic, InternalType.Arglist, delegate_type);
			}
			
			if (am == null)
				return null;

			return am.ReturnType;
		}

		//
		// Returns AnonymousMethod container if this anonymous method
		// expression can be implicitly converted to the delegate type `delegate_type'
		//
		public Expression Compatible (ResolveContext ec, Type type)
		{
			Expression am = (Expression) compatibles [type];
			if (am != null)
				return am;

			Type delegate_type = CompatibleChecks (ec, type);
			if (delegate_type == null)
				return null;

			//
			// At this point its the first time we know the return type that is 
			// needed for the anonymous method.  We create the method here.
			//

			MethodInfo invoke_mb = Delegate.GetInvokeMethod (ec.Compiler,
				ec.CurrentType, delegate_type);
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

			try {
				int errors = ec.Report.Errors;
				am = CompatibleMethod (ec, null, return_type, delegate_type);
				if (am != null && delegate_type != type && errors == ec.Report.Errors)
					am = CreateExpressionTree (ec, delegate_type);

				if (!ec.IsInProbingMode)
					compatibles.Add (type, am == null ? EmptyExpression.Null : am);

				return am;
			} catch (CompletionResult){
				throw;
			} catch (Exception e) {
				throw new InternalErrorException (e, loc);
			}
		}

		protected virtual Expression CreateExpressionTree (ResolveContext ec, Type delegate_type)
		{
			return CreateExpressionTree (ec);
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			ec.Report.Error (1946, loc, "An anonymous method cannot be converted to an expression tree");
			return null;
		}

		protected virtual ParametersCompiled ResolveParameters (ResolveContext ec, TypeInferenceContext tic, Type delegate_type)
		{
			AParametersCollection delegate_parameters = TypeManager.GetDelegateParameters (ec, delegate_type);

			if (Parameters == ParametersCompiled.Undefined) {
				//
				// We provide a set of inaccessible parameters
				//
				Parameter[] fixedpars = new Parameter[delegate_parameters.Count];

				for (int i = 0; i < delegate_parameters.Count; i++) {
					Parameter.Modifier i_mod = delegate_parameters.FixedParameters [i].ModFlags;
					if (i_mod == Parameter.Modifier.OUT) {
						ec.Report.Error (1688, loc, "Cannot convert anonymous " +
								  "method block without a parameter list " +
								  "to delegate type `{0}' because it has " +
								  "one or more `out' parameters.",
								  TypeManager.CSharpName (delegate_type));
						return null;
					}
					fixedpars[i] = new Parameter (
						null, null,
						delegate_parameters.FixedParameters [i].ModFlags, null, loc);
				}

				return ParametersCompiled.CreateFullyResolved (fixedpars, delegate_parameters.Types);
			}

			if (!VerifyExplicitParameters (ec, delegate_type, delegate_parameters)) {
				return null;
			}

			return Parameters;
		}

		public override Expression DoResolve (ResolveContext ec)
		{
			if (ec.HasSet (ResolveContext.Options.ConstantScope)) {
				ec.Report.Error (1706, loc, "Anonymous methods and lambda expressions cannot be used in the current context");
				return null;
			}

			//
			// Set class type, set type
			//

			eclass = ExprClass.Value;

			//
			// This hack means `The type is not accessible
			// anywhere', we depend on special conversion
			// rules.
			// 
			type = InternalType.AnonymousMethod;

			if ((Parameters != null) && !Parameters.Resolve (ec))
				return null;

			// FIXME: The emitted code isn't very careful about reachability
			// so, ensure we have a 'ret' at the end
			BlockContext bc = ec as BlockContext;
			if (bc != null && bc.CurrentBranching != null && bc.CurrentBranching.CurrentUsageVector.IsUnreachable)
				bc.NeedReturnLabel ();

			return this;
		}

		public override void Emit (EmitContext ec)
		{
			// nothing, as we only exist to not do anything.
		}

		public static void Error_AddressOfCapturedVar (ResolveContext ec, IVariableReference var, Location loc)
		{
			ec.Report.Error (1686, loc,
				"Local variable or parameter `{0}' cannot have their address taken and be used inside an anonymous method or lambda expression",
				var.Name);
		}

		public override string GetSignatureForError ()
		{
			return ExprClassName;
		}

		protected AnonymousMethodBody CompatibleMethod (ResolveContext ec, TypeInferenceContext tic, Type return_type, Type delegate_type)
		{
			ParametersCompiled p = ResolveParameters (ec, tic, delegate_type);
			if (p == null)
				return null;

			ToplevelBlock b = ec.IsInProbingMode ? (ToplevelBlock) Block.PerformClone () : Block;

			AnonymousMethodBody anonymous = CompatibleMethodFactory (return_type, delegate_type, p, b);
			if (!anonymous.Compatible (ec))
				return null;

			return anonymous;
		}

		protected virtual AnonymousMethodBody CompatibleMethodFactory (Type return_type, Type delegate_type, ParametersCompiled p, ToplevelBlock b)
		{
			return new AnonymousMethodBody (p, b, return_type, delegate_type, loc);
		}

		protected override void CloneTo (CloneContext clonectx, Expression t)
		{
			AnonymousMethodExpression target = (AnonymousMethodExpression) t;

			target.Block = (ToplevelBlock) clonectx.LookupBlock (Block);
		}
	}

	//
	// Abstract expression for any block which requires variables hoisting
	//
	public abstract class AnonymousExpression : Expression
	{
		protected class AnonymousMethodMethod : Method
		{
			public readonly AnonymousExpression AnonymousMethod;
			public readonly AnonymousMethodStorey Storey;
			readonly string RealName;

			public AnonymousMethodMethod (DeclSpace parent, AnonymousExpression am, AnonymousMethodStorey storey,
							  GenericMethod generic, TypeExpr return_type,
							  int mod, string real_name, MemberName name,
							  ParametersCompiled parameters)
				: base (parent, generic, return_type, mod | Modifiers.COMPILER_GENERATED,
						name, parameters, null)
			{
				this.AnonymousMethod = am;
				this.Storey = storey;
				this.RealName = real_name;

				Parent.PartialContainer.AddMethod (this);
				Block = am.Block;
			}

			public override EmitContext CreateEmitContext (ILGenerator ig)
			{
				EmitContext ec = new EmitContext (this, ig, ReturnType);
				ec.CurrentAnonymousMethod = AnonymousMethod;
				if (AnonymousMethod.return_label != null) {
					ec.HasReturnLabel = true;
					ec.ReturnLabel = (Label) AnonymousMethod.return_label;
				}

				return ec;
			}

			protected override bool ResolveMemberType ()
			{
				if (!base.ResolveMemberType ())
					return false;

				if (Storey != null && Storey.IsGeneric) {
					AnonymousMethodStorey gstorey = Storey.GetGenericStorey ();
					if (gstorey != null) {
						if (!Parameters.IsEmpty) {
							Type [] ptypes = Parameters.Types;
							for (int i = 0; i < ptypes.Length; ++i)
								ptypes [i] = gstorey.MutateType (ptypes [i]);
						}

						member_type = gstorey.MutateType (member_type);
					}
				}

				return true;
			}

			public override void Emit ()
			{
				//
				// Before emitting any code we have to change all MVAR references to VAR
				// when the method is of generic type and has hoisted variables
				//
				if (Storey == Parent && Storey.IsGeneric) {
					AnonymousMethodStorey gstorey = Storey.GetGenericStorey ();
					if (gstorey != null) {
						block.MutateHoistedGenericType (gstorey);
					}
				}

				if (MethodBuilder == null) {
					Define ();
				}

				base.Emit ();
			}

			public override void EmitExtraSymbolInfo (SourceMethod source)
			{
				source.SetRealMethodName (RealName);
			}
		}

		//
		// The block that makes up the body for the anonymous method
		//
		protected readonly ToplevelBlock Block;

		public Type ReturnType;

		object return_label;

		protected AnonymousExpression (ToplevelBlock block, Type return_type, Location loc)
		{
			this.ReturnType = return_type;
			this.Block = block;
			this.loc = loc;
		}

		public abstract string ContainerType { get; }
		public abstract bool IsIterator { get; }
		public abstract AnonymousMethodStorey Storey { get; }

		public bool Compatible (ResolveContext ec)
		{
			// TODO: Implement clone
			BlockContext aec = new BlockContext (ec.MemberContext, Block, ReturnType);
			aec.CurrentAnonymousMethod = this;

			IDisposable aec_dispose = null;
			ResolveContext.Options flags = 0;
			if (ec.HasSet (ResolveContext.Options.InferReturnType)) {
				flags |= ResolveContext.Options.InferReturnType;
				aec.ReturnTypeInference = new TypeInferenceContext ();
			}

			if (ec.IsInProbingMode)
				flags |= ResolveContext.Options.ProbingMode;

			if (ec.HasSet (ResolveContext.Options.FieldInitializerScope))
				flags |= ResolveContext.Options.FieldInitializerScope;

			if (ec.IsUnsafe)
				flags |= ResolveContext.Options.UnsafeScope;

			if (ec.HasSet (ResolveContext.Options.CheckedScope))
				flags |= ResolveContext.Options.CheckedScope;

			// HACK: Flag with 0 cannot be set 
			if (flags != 0)
				aec_dispose = aec.Set (flags);

			bool res = Block.Resolve (ec.CurrentBranching, aec, Block.Parameters, null);

			if (aec.HasReturnLabel)
				return_label = aec.ReturnLabel;

			if (ec.HasSet (ResolveContext.Options.InferReturnType)) {
				aec.ReturnTypeInference.FixAllTypes (ec);
				ReturnType = aec.ReturnTypeInference.InferredTypeArguments [0];
			}

			if (aec_dispose != null) {
				aec_dispose.Dispose ();
			}

			return res;
		}

		public void SetHasThisAccess ()
		{
			Block.HasCapturedThis = true;
			ExplicitBlock b = Block.Parent.Explicit;

			while (b != null) {
				if (b.HasCapturedThis)
					return;

				b.HasCapturedThis = true;
				b = b.Parent == null ? null : b.Parent.Explicit;
			}
		}
	}

	public class AnonymousMethodBody : AnonymousExpression
	{
		protected readonly ParametersCompiled parameters;
		AnonymousMethodStorey storey;

		AnonymousMethodMethod method;
		Field am_cache;
		string block_name;

		static int unique_id;

		public AnonymousMethodBody (ParametersCompiled parameters,
					ToplevelBlock block, Type return_type, Type delegate_type,
					Location loc)
			: base (block, return_type, loc)
		{
			this.type = delegate_type;
			this.parameters = parameters;
		}

		public override string ContainerType {
			get { return "anonymous method"; }
		}

		public override AnonymousMethodStorey Storey {
			get { return storey; }
		}

		public override bool IsIterator {
			get { return false; }
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			ec.Report.Error (1945, loc, "An expression tree cannot contain an anonymous method expression");
			return null;
		}

		bool Define (ResolveContext ec)
		{
			if (!Block.Resolved && !Compatible (ec))
				return false;

			if (block_name == null) {
				MemberCore mc = (MemberCore) ec.MemberContext;
				block_name = mc.MemberName.Basename;
			}

			return true;
		}

		//
		// Creates a host for the anonymous method
		//
		AnonymousMethodMethod DoCreateMethodHost (EmitContext ec)
		{
			//
			// Anonymous method body can be converted to
			//
			// 1, an instance method in current scope when only `this' is hoisted
			// 2, a static method in current scope when neither `this' nor any variable is hoisted
			// 3, an instance method in compiler generated storey when any hoisted variable exists
			//

			int modifiers;
			if (Block.HasCapturedVariable || Block.HasCapturedThis) {
				storey = FindBestMethodStorey ();
				modifiers = storey != null ? Modifiers.INTERNAL : Modifiers.PRIVATE;
			} else {
				if (ec.CurrentAnonymousMethod != null)
					storey = ec.CurrentAnonymousMethod.Storey;

				modifiers = Modifiers.STATIC | Modifiers.PRIVATE;
			}

			TypeContainer parent = storey != null ? storey : ec.CurrentTypeDefinition;

			MemberCore mc = ec.MemberContext as MemberCore;
			string name = CompilerGeneratedClass.MakeName (parent != storey ? block_name : null,
				"m", null, unique_id++);

			MemberName member_name;
			GenericMethod generic_method;
			if (storey == null && mc.MemberName.TypeArguments != null) {
				member_name = new MemberName (name, mc.MemberName.TypeArguments.Clone (), Location);

				generic_method = new GenericMethod (parent.NamespaceEntry, parent, member_name,
					new TypeExpression (ReturnType, Location), parameters);

				ArrayList list = new ArrayList ();
				foreach (TypeParameter tparam in ec.CurrentTypeParameters) {
					if (tparam.Constraints != null)
						list.Add (tparam.Constraints.Clone ());
				}
				generic_method.SetParameterInfo (list);
			} else {
				member_name = new MemberName (name, Location);
				generic_method = null;
			}

			string real_name = String.Format (
				"{0}~{1}{2}", mc.GetSignatureForError (), GetSignatureForError (),
				parameters.GetSignatureForError ());

			return new AnonymousMethodMethod (parent,
				this, storey, generic_method, new TypeExpression (ReturnType, Location), modifiers,
				real_name, member_name, parameters);
		}

		public override Expression DoResolve (ResolveContext ec)
		{
			if (eclass == ExprClass.Invalid) {
				if (!Define (ec))
					return null;
			}

			eclass = ExprClass.Value;
			return this;
		}

		public override void Emit (EmitContext ec)
		{
			//
			// Use same anonymous method implementation for scenarios where same
			// code is used from multiple blocks, e.g. field initializers
			//
			if (method == null) {
				//
				// Delay an anonymous method definition to avoid emitting unused code
				// for unreachable blocks or expression trees
				//
				method = DoCreateMethodHost (ec);
				method.Define ();
			}

			bool is_static = (method.ModFlags & Modifiers.STATIC) != 0;
			if (is_static && am_cache == null) {
				//
				// Creates a field cache to store delegate instance if it's not generic
				//
				if (!method.MemberName.IsGeneric) {
					TypeContainer parent = method.Parent.PartialContainer;
					int id = parent.Fields == null ? 0 : parent.Fields.Count;
					am_cache = new Field (parent, new TypeExpression (type, loc),
						Modifiers.STATIC | Modifiers.PRIVATE | Modifiers.COMPILER_GENERATED,
						new MemberName (CompilerGeneratedClass.MakeName (null, "f", "am$cache", id), loc), null);
					am_cache.Define ();
					parent.AddField (am_cache);
				} else {
					// TODO: Implement caching of generated generic static methods
					//
					// Idea:
					//
					// Some extra class is needed to capture variable generic type
					// arguments. Maybe we could re-use anonymous types, with a unique
					// anonymous method id, but they are quite heavy.
					//
					// Consider : "() => typeof(T);"
					//
					// We need something like
					// static class Wrap<Tn, Tm, DelegateType> {
					//		public static DelegateType cache;
					// }
					//
					// We then specialize local variable to capture all generic parameters
					// and delegate type, e.g. "Wrap<Ta, Tb, DelegateTypeInst> cache;"
					//
				}
			}

			ILGenerator ig = ec.ig;
			Label l_initialized = ig.DefineLabel ();

			if (am_cache != null) {
				ig.Emit (OpCodes.Ldsfld, am_cache.FieldBuilder);
				ig.Emit (OpCodes.Brtrue_S, l_initialized);
			}

			//
			// Load method delegate implementation
			//

			if (is_static) {
				ig.Emit (OpCodes.Ldnull);
			} else if (storey != null) {
				Expression e = storey.GetStoreyInstanceExpression (ec).Resolve (new ResolveContext (ec.MemberContext));
				if (e != null)
					e.Emit (ec);
			} else {
				ig.Emit (OpCodes.Ldarg_0);
			}

			MethodInfo delegate_method = method.MethodBuilder;
			if (storey != null && storey.MemberName.IsGeneric) {
				Type t = storey.Instance.Type;
				
				//
				// Mutate anonymous method instance type if we are in nested
				// hoisted generic anonymous method storey
				//
				if (ec.CurrentAnonymousMethod != null &&
					ec.CurrentAnonymousMethod.Storey != null &&
					ec.CurrentAnonymousMethod.Storey.IsGeneric) {
					t = storey.GetGenericStorey ().MutateType (t);
				}

#if GMCS_SOURCE
				delegate_method = TypeBuilder.GetMethod (t, delegate_method);
#else
				throw new NotSupportedException ();
#endif
			}

			ig.Emit (OpCodes.Ldftn, delegate_method);

			ConstructorInfo constructor_method = Delegate.GetConstructor (RootContext.ToplevelTypes.Compiler, ec.CurrentType, type);
#if MS_COMPATIBLE
            if (type.IsGenericType && type is TypeBuilder)
                constructor_method = TypeBuilder.GetConstructor (type, constructor_method);
#endif
			ig.Emit (OpCodes.Newobj, constructor_method);

			if (am_cache != null) {
				ig.Emit (OpCodes.Stsfld, am_cache.FieldBuilder);
				ig.MarkLabel (l_initialized);
				ig.Emit (OpCodes.Ldsfld, am_cache.FieldBuilder);
			}
		}

		//
		// Look for the best storey for this anonymous method
		//
		AnonymousMethodStorey FindBestMethodStorey ()
		{
			//
			// Use the nearest parent block which has a storey
			//
			for (Block b = Block.Parent; b != null; b = b.Parent) {
				AnonymousMethodStorey s = b.Explicit.AnonymousMethodStorey;
				if (s != null)
					return s;
			}
					
			return null;
		}

		public override string GetSignatureForError ()
		{
			return TypeManager.CSharpName (type);
		}

		public override void MutateHoistedGenericType (AnonymousMethodStorey storey)
		{
			type = storey.MutateType (type);
		}

		public static void Reset ()
		{
			unique_id = 0;
		}
	}

	//
	// Anonymous type container
	//
	public class AnonymousTypeClass : CompilerGeneratedClass
	{
		sealed class AnonymousParameters : ParametersCompiled
		{
			public AnonymousParameters (params Parameter[] parameters)
				: base (parameters)
			{
			}

			protected override void ErrorDuplicateName (Parameter p, Report Report)
			{
				Report.Error (833, p.Location, "`{0}': An anonymous type cannot have multiple properties with the same name",
					p.Name);
			}
		}

		static int types_counter;
		public const string ClassNamePrefix = "<>__AnonType";
		public const string SignatureForError = "anonymous type";
		
		readonly ArrayList parameters;

		private AnonymousTypeClass (DeclSpace parent, MemberName name, ArrayList parameters, Location loc)
			: base (parent, name, (RootContext.EvalMode ? Modifiers.PUBLIC : 0) | Modifiers.SEALED)
		{
			this.parameters = parameters;
		}

		public static AnonymousTypeClass Create (CompilerContext ctx, TypeContainer parent, ArrayList parameters, Location loc)
		{
			string name = ClassNamePrefix + types_counter++;

			SimpleName [] t_args = new SimpleName [parameters.Count];
			TypeParameterName [] t_params = new TypeParameterName [parameters.Count];
			Parameter [] ctor_params = new Parameter [parameters.Count];
			for (int i = 0; i < parameters.Count; ++i) {
				AnonymousTypeParameter p = (AnonymousTypeParameter) parameters [i];

				t_args [i] = new SimpleName ("<" + p.Name + ">__T", p.Location);
				t_params [i] = new TypeParameterName (t_args [i].Name, null, p.Location);
				ctor_params [i] = new Parameter (t_args [i], p.Name, 0, null, p.Location);
			}

			//
			// Create generic anonymous type host with generic arguments
			// named upon properties names
			//
			AnonymousTypeClass a_type = new AnonymousTypeClass (parent.NamespaceEntry.SlaveDeclSpace,
				new MemberName (name, new TypeArguments (t_params), loc), parameters, loc);

			if (parameters.Count > 0)
				a_type.SetParameterInfo (null);

			Constructor c = new Constructor (a_type, name, Modifiers.PUBLIC | Modifiers.DEBUGGER_HIDDEN,
				null, new AnonymousParameters (ctor_params), null, loc);
			c.Block = new ToplevelBlock (ctx, c.Parameters, loc);

			// 
			// Create fields and contructor body with field initialization
			//
			bool error = false;
			for (int i = 0; i < parameters.Count; ++i) {
				AnonymousTypeParameter p = (AnonymousTypeParameter) parameters [i];

				Field f = new Field (a_type, t_args [i], Modifiers.PRIVATE | Modifiers.READONLY,
					new MemberName ("<" + p.Name + ">", p.Location), null);

				if (!a_type.AddField (f)) {
					error = true;
					continue;
				}

				c.Block.AddStatement (new StatementExpression (
					new SimpleAssign (new MemberAccess (new This (p.Location), f.Name),
						c.Block.GetParameterReference (p.Name, p.Location))));

				ToplevelBlock get_block = new ToplevelBlock (ctx, p.Location);
				get_block.AddStatement (new Return (
					new MemberAccess (new This (p.Location), f.Name), p.Location));
				Accessor get_accessor = new Accessor (get_block, 0, null, null, p.Location);
				Property prop = new Property (a_type, t_args [i], Modifiers.PUBLIC,
					new MemberName (p.Name, p.Location), null, get_accessor, null, false);
				a_type.AddProperty (prop);
			}

			if (error)
				return null;

			a_type.AddConstructor (c);
			return a_type;
		}
		
		public static void Reset ()
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
				Modifiers.PUBLIC | Modifiers.OVERRIDE | Modifiers.DEBUGGER_HIDDEN, new MemberName ("Equals", loc),
				Mono.CSharp.ParametersCompiled.CreateFullyResolved (new Parameter (null, "obj", 0, null, loc), TypeManager.object_type), null);

			Method tostring = new Method (this, null, TypeManager.system_string_expr,
				Modifiers.PUBLIC | Modifiers.OVERRIDE | Modifiers.DEBUGGER_HIDDEN, new MemberName ("ToString", loc),
				Mono.CSharp.ParametersCompiled.EmptyReadOnlyParameters, null);

			ToplevelBlock equals_block = new ToplevelBlock (Compiler, equals.Parameters, loc);
			TypeExpr current_type;
			if (IsGeneric)
				current_type = new GenericTypeExpr (this, loc);
			else
				current_type = new TypeExpression (TypeBuilder, loc);

			equals_block.AddVariable (current_type, "other", loc);
			LocalVariableReference other_variable = new LocalVariableReference (equals_block, "other", loc);

			MemberAccess system_collections_generic = new MemberAccess (new MemberAccess (
				new QualifiedAliasMember ("global", "System", loc), "Collections", loc), "Generic", loc);

			Expression rs_equals = null;
			Expression string_concat = new StringConstant ("{", loc);
			Expression rs_hashcode = new IntConstant (-2128831035, loc);
			for (int i = 0; i < parameters.Count; ++i) {
				AnonymousTypeParameter p = (AnonymousTypeParameter) parameters [i];
				Field f = (Field) Fields [i];

				MemberAccess equality_comparer = new MemberAccess (new MemberAccess (
					system_collections_generic, "EqualityComparer",
						new TypeArguments (new SimpleName (TypeParameters [i].Name, loc)), loc),
						"Default", loc);

				Arguments arguments_equal = new Arguments (2);
				arguments_equal.Add (new Argument (new MemberAccess (new This (f.Location), f.Name)));
				arguments_equal.Add (new Argument (new MemberAccess (other_variable, f.Name)));

				Expression field_equal = new Invocation (new MemberAccess (equality_comparer,
					"Equals", loc), arguments_equal);

				Arguments arguments_hashcode = new Arguments (1);
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
					new StringConstant (string.Empty, loc));

				if (rs_equals == null) {
					rs_equals = field_equal;
					string_concat = new Binary (Binary.Operator.Addition,
						string_concat,
						new Binary (Binary.Operator.Addition,
							new StringConstant (" " + p.Name + " = ", loc),
							field_to_string));
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

			string_concat = new Binary (Binary.Operator.Addition,
				string_concat,
				new StringConstant (" }", loc));

			//
			// Equals (object obj) override
			//		
			LocalVariableReference other_variable_assign = new LocalVariableReference (equals_block, "other", loc);
			equals_block.AddStatement (new StatementExpression (
				new SimpleAssign (other_variable_assign,
					new As (equals_block.GetParameterReference ("obj", loc),
						current_type, loc), loc)));

			Expression equals_test = new Binary (Binary.Operator.Inequality, other_variable, new NullLiteral (loc));
			if (rs_equals != null)
				equals_test = new Binary (Binary.Operator.LogicalAnd, equals_test, rs_equals);
			equals_block.AddStatement (new Return (equals_test, loc));

			equals.Block = equals_block;
			equals.Define ();
			AddMethod (equals);

			//
			// GetHashCode () override
			//
			Method hashcode = new Method (this, null, TypeManager.system_int32_expr,
				Modifiers.PUBLIC | Modifiers.OVERRIDE | Modifiers.DEBUGGER_HIDDEN,
				new MemberName ("GetHashCode", loc),
				Mono.CSharp.ParametersCompiled.EmptyReadOnlyParameters, null);

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

			ToplevelBlock hashcode_top = new ToplevelBlock (Compiler, loc);
			Block hashcode_block = new Block (hashcode_top);
			hashcode_top.AddStatement (new Unchecked (hashcode_block));

			hashcode_block.AddVariable (TypeManager.system_int32_expr, "hash", loc);
			LocalVariableReference hash_variable = new LocalVariableReference (hashcode_block, "hash", loc);
			LocalVariableReference hash_variable_assign = new LocalVariableReference (hashcode_block, "hash", loc);
			hashcode_block.AddStatement (new StatementExpression (
				new SimpleAssign (hash_variable_assign, rs_hashcode)));

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
			hashcode.Block = hashcode_top;
			hashcode.Define ();
			AddMethod (hashcode);

			//
			// ToString () override
			//

			ToplevelBlock tostring_block = new ToplevelBlock (Compiler, loc);
			tostring_block.AddStatement (new Return (string_concat, loc));
			tostring.Block = tostring_block;
			tostring.Define ();
			AddMethod (tostring);
		}

		public override bool Define ()
		{
			if (!base.Define ())
				return false;

			DefineOverrides ();
			return true;
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
