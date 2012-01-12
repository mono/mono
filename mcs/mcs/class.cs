//
// class.cs: Class and Struct handlers
//
// Authors: Miguel de Icaza (miguel@gnu.org)
//          Martin Baulig (martin@ximian.com)
//          Marek Safar (marek.safar@seznam.cz)
//
// Dual licensed under the terms of the MIT X11 or GNU GPL
//
// Copyright 2001, 2002, 2003 Ximian, Inc (http://www.ximian.com)
// Copyright 2004-2008 Novell, Inc
//

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Text;

#if BOOTSTRAP_WITH_OLDLIB || NET_2_1
using XmlElement = System.Object;
#else
using System.Xml;
#endif

using Mono.CompilerServices.SymbolWriter;

namespace Mono.CSharp {

	public enum Kind {
		Root,
		Struct,
		Class,
		Interface,
		Enum,
		Delegate
	}

	/// <summary>
	///   This is the base class for structs and classes.  
	/// </summary>
	public abstract class TypeContainer : DeclSpace, IMemberContainer {

 		public class MemberCoreArrayList: ArrayList
 		{
			/// <summary>
			///   Defines the MemberCore objects that are in this array
			/// </summary>
			public virtual void DefineContainerMembers ()
			{
				foreach (MemberCore mc in this) {
					try {
						mc.Define ();
					} catch (Exception e) {
						throw new InternalErrorException (mc, e);
					}
				}
			}

			public virtual void Emit ()
			{
				foreach (MemberCore mc in this)
					mc.Emit ();
			}
 		}

 		public class OperatorArrayList: MemberCoreArrayList
		{
			TypeContainer container;

			public OperatorArrayList (TypeContainer container)
			{
				this.container = container;
			}

			//
			// Checks that some operators come in pairs:
			//  == and !=
			// > and <
			// >= and <=
			// true and false
			//
			// They are matched based on the return type and the argument types
			//
			void CheckPairedOperators ()
			{
				bool has_equality_or_inequality = false;
				Operator[] operators = (Operator[]) ToArray (typeof (Operator));
				bool [] has_pair = new bool [operators.Length];

				for (int i = 0; i < Count; ++i) {
					if (operators [i] == null)
						continue;

					Operator o_a = operators [i];
					Operator.OpType o_type = o_a.OperatorType;
					if (o_type == Operator.OpType.Equality || o_type == Operator.OpType.Inequality)
						has_equality_or_inequality = true;

					Operator.OpType matching_type = o_a.GetMatchingOperator ();
					if (matching_type == Operator.OpType.TOP) {
						operators [i] = null;
						continue;
					}
	
					for (int ii = 0; ii < Count; ++ii) {
						Operator o_b = operators [ii];
						if (o_b == null || o_b.OperatorType != matching_type)
							continue;

						if (!TypeManager.IsEqual (o_a.ReturnType, o_b.ReturnType))
							continue;

						if (!TypeManager.IsEqual (o_a.ParameterTypes, o_b.ParameterTypes))
							continue;

						operators [i] = null;

						//
						// Used to ignore duplicate user conversions
						//
						has_pair [ii] = true;
					}
				}

				for (int i = 0; i < Count; ++i) {
					if (operators [i] == null || has_pair [i])
						continue;

					Operator o = operators [i];
					container.Report.Error (216, o.Location,
						"The operator `{0}' requires a matching operator `{1}' to also be defined",
						o.GetSignatureForError (), Operator.GetName (o.GetMatchingOperator ()));
				}

 				if (has_equality_or_inequality && container.Report.WarningLevel > 2) {
 					if (container.Methods == null || !container.HasEquals)
 						container.Report.Warning (660, 2, container.Location, "`{0}' defines operator == or operator != but does not override Object.Equals(object o)", container.GetSignatureForError ());
 
 					if (container.Methods == null || !container.HasGetHashCode)
 						container.Report.Warning (661, 2, container.Location, "`{0}' defines operator == or operator != but does not override Object.GetHashCode()", container.GetSignatureForError ());
 				}
			}

	 		public override void DefineContainerMembers ()
	 		{
	 			base.DefineContainerMembers ();
	 			CheckPairedOperators ();
			}
		}

		//
		// Different context is needed when resolving type container base
		// types. Type names come from the parent scope but type parameter
		// names from the container scope.
		//
		struct BaseContext : IMemberContext
		{
			TypeContainer tc;

			public BaseContext (TypeContainer tc)
			{
				this.tc = tc;
			}

			#region IMemberContext Members

			public CompilerContext Compiler {
				get { return tc.Compiler; }
			}

			public Type CurrentType {
				get { return tc.Parent.CurrentType; }
			}

			public TypeParameter[] CurrentTypeParameters {
				get { return tc.PartialContainer.CurrentTypeParameters; }
			}

			public TypeContainer CurrentTypeDefinition {
				get { return tc.Parent.CurrentTypeDefinition; }
			}

			public bool IsObsolete {
				get { return tc.IsObsolete; }
			}

			public bool IsUnsafe {
				get { return tc.IsUnsafe; }
			}

			public bool IsStatic {
				get { return tc.IsStatic; }
			}

			public string GetSignatureForError ()
			{
				throw new NotImplementedException ();
			}

			public ExtensionMethodGroupExpr LookupExtensionMethod (Type extensionType, string name, Location loc)
			{
				return null;
			}

			public FullNamedExpression LookupNamespaceAlias (string name)
			{
				return tc.Parent.LookupNamespaceAlias (name);
			}

			public FullNamedExpression LookupNamespaceOrType (string name, Location loc, bool ignore_cs0104)
			{
				TypeParameter[] tp = CurrentTypeParameters;
				if (tp != null) {
					TypeParameter t = TypeParameter.FindTypeParameter (tp, name);
					if (t != null)
						return new TypeParameterExpr (t, loc);
				}

				return tc.Parent.LookupNamespaceOrType (name, loc, ignore_cs0104);
			}

			#endregion
		}

		[Flags]
		enum CachedMethods
		{
			Equals				= 1,
			GetHashCode			= 1 << 1,
			HasStaticFieldInitializer	= 1 << 2
		}


		// Whether this is a struct, class or interface
		public readonly Kind Kind;

		// Holds a list of classes and structures
		protected ArrayList types;

		MemberCoreArrayList ordered_explicit_member_list;
		MemberCoreArrayList ordered_member_list;

		// Holds the list of properties
		MemberCoreArrayList properties;

		// Holds the list of delegates
		MemberCoreArrayList delegates;
		
		// Holds the list of constructors
		protected MemberCoreArrayList instance_constructors;

		// Holds the list of fields
		protected MemberCoreArrayList fields;

		// Holds a list of fields that have initializers
		protected ArrayList initialized_fields;

		// Holds a list of static fields that have initializers
		protected ArrayList initialized_static_fields;

		// Holds the list of constants
		protected MemberCoreArrayList constants;

		// Holds the methods.
		MemberCoreArrayList methods;

		// Holds the events
		protected MemberCoreArrayList events;

		// Holds the indexers
		ArrayList indexers;

		// Holds the operators
		MemberCoreArrayList operators;

		// Holds the compiler generated classes
		ArrayList compiler_generated;

		//
		// Pointers to the default constructor and the default static constructor
		//
		protected Constructor default_constructor;
		protected Constructor default_static_constructor;

		//
		// Points to the first non-static field added to the container.
		//
		// This is an arbitrary choice.  We are interested in looking at _some_ non-static field,
		// and the first one's as good as any.
		//
		FieldBase first_nonstatic_field = null;

		//
		// This one is computed after we can distinguish interfaces
		// from classes from the arraylist `type_bases' 
		//
		TypeExpr base_type;
		TypeExpr[] iface_exprs;
		Type GenericType;
		GenericTypeParameterBuilder[] nested_gen_params;

		protected ArrayList type_bases;

		protected bool members_defined;
		bool members_defined_ok;

		// The interfaces we implement.
		protected Type[] ifaces;

		// The base member cache and our member cache
		MemberCache base_cache;
		protected MemberCache member_cache;

		public const string DefaultIndexerName = "Item";

		private bool seen_normal_indexers = false;
		private string indexer_name = DefaultIndexerName;
		protected bool requires_delayed_unmanagedtype_check;

		private CachedMethods cached_method;

		ArrayList partial_parts;

		/// <remarks>
		///  The pending methods that need to be implemented
		//   (interfaces or abstract methods)
		/// </remarks>
		PendingImplementation pending;

		public TypeContainer (NamespaceEntry ns, DeclSpace parent, MemberName name,
				      Attributes attrs, Kind kind)
			: base (ns, parent, name, attrs)
		{
			if (parent != null && parent.NamespaceEntry != ns)
				throw new InternalErrorException ("A nested type should be in the same NamespaceEntry as its enclosing class");

			this.Kind = kind;
			this.PartialContainer = this;
		}

		public bool AddMember (MemberCore symbol)
		{
			return AddToContainer (symbol, symbol.MemberName.Basename);
		}

		protected virtual bool AddMemberType (DeclSpace ds)
		{
			return AddToContainer (ds, ds.Basename);
		}

		protected virtual void RemoveMemberType (DeclSpace ds)
		{
			RemoveFromContainer (ds.Basename);
		}

		public void AddConstant (Const constant)
		{
			if (!AddMember (constant))
				return;

			if (constants == null)
				constants = new MemberCoreArrayList ();
			
			constants.Add (constant);
		}

		public TypeContainer AddTypeContainer (TypeContainer tc)
		{
			if (!AddMemberType (tc))
				return tc;

			if (types == null)
				types = new MemberCoreArrayList ();
			types.Add (tc);

			return tc;
		}

		public virtual TypeContainer AddPartial (TypeContainer next_part)
		{
			return AddPartial (next_part, next_part.Basename);
		}

		protected TypeContainer AddPartial (TypeContainer next_part, string name)
		{
			next_part.ModFlags |= Modifiers.PARTIAL;
			TypeContainer tc = defined_names [name] as TypeContainer;

			if (tc == null)
				return AddTypeContainer (next_part);

			if ((tc.ModFlags & Modifiers.PARTIAL) == 0) {
				Report.SymbolRelatedToPreviousError (next_part);
				Error_MissingPartialModifier (tc);
			}

			if (tc.Kind != next_part.Kind) {
				Report.SymbolRelatedToPreviousError (tc);
				Report.Error (261, next_part.Location,
					"Partial declarations of `{0}' must be all classes, all structs or all interfaces",
					next_part.GetSignatureForError ());
			}

			if ((tc.ModFlags & Modifiers.Accessibility) != (next_part.ModFlags & Modifiers.Accessibility) &&
				((tc.ModFlags & Modifiers.DEFAULT_ACCESS_MODIFER) == 0 &&
				 (next_part.ModFlags & Modifiers.DEFAULT_ACCESS_MODIFER) == 0)) {
				Report.SymbolRelatedToPreviousError (tc);
				Report.Error (262, next_part.Location,
					"Partial declarations of `{0}' have conflicting accessibility modifiers",
					next_part.GetSignatureForError ());
			}

			if (tc.partial_parts == null)
				tc.partial_parts = new ArrayList (1);

			if ((next_part.ModFlags & Modifiers.DEFAULT_ACCESS_MODIFER) != 0) {
				tc.ModFlags |= next_part.ModFlags & ~(Modifiers.DEFAULT_ACCESS_MODIFER | Modifiers.Accessibility);
			} else if ((tc.ModFlags & Modifiers.DEFAULT_ACCESS_MODIFER) != 0) {
				tc.ModFlags &= ~(Modifiers.DEFAULT_ACCESS_MODIFER | Modifiers.Accessibility);
				tc.ModFlags |= next_part.ModFlags;
			} else {
				tc.ModFlags |= next_part.ModFlags;
			}

			if (next_part.attributes != null) {
				if (tc.attributes == null)
					tc.attributes = next_part.attributes;
				else
					tc.attributes.AddAttributes (next_part.attributes.Attrs);
			}

			next_part.PartialContainer = tc;
			tc.partial_parts.Add (next_part);
			return tc;
		}

		public virtual void RemoveTypeContainer (TypeContainer next_part)
		{
			if (types != null)
				types.Remove (next_part);
			RemoveMemberType (next_part);
		}
		
		public void AddDelegate (Delegate d)
		{
			if (!AddMemberType (d))
				return;

			if (delegates == null)
				delegates = new MemberCoreArrayList ();

			delegates.Add (d);
		}

		private void AddMemberToList (MemberCore mc, ArrayList alist, bool isexplicit)
		{
			if (ordered_explicit_member_list == null)  {
				ordered_explicit_member_list = new MemberCoreArrayList ();
				ordered_member_list = new MemberCoreArrayList ();
			}

			if (isexplicit) {
				if (Kind == Kind.Interface) {
					Report.Error (541, mc.Location,
						"`{0}': explicit interface declaration can only be declared in a class or struct",
						mc.GetSignatureForError ());
				}

				ordered_explicit_member_list.Add (mc);
				alist.Insert (0, mc);
			} else {
				ordered_member_list.Add (mc);
				alist.Add (mc);
			}

		}
		
		public void AddMethod (MethodOrOperator method)
		{
			if (!AddToContainer (method, method.MemberName.Basename))
				return;
			
			if (methods == null)
				methods = new MemberCoreArrayList ();

			if (method.MemberName.Left != null) 
				AddMemberToList (method, methods, true);
			else 
				AddMemberToList (method, methods, false);
		}

		public void AddConstructor (Constructor c)
		{
			bool is_static = (c.ModFlags & Modifiers.STATIC) != 0;
			if (!AddToContainer (c, is_static ?
				ConstructorBuilder.ConstructorName : ConstructorBuilder.TypeConstructorName))
				return;
			
			if (is_static && c.Parameters.IsEmpty){
				if (default_static_constructor != null) {
				    Report.SymbolRelatedToPreviousError (default_static_constructor);
					Report.Error (111, c.Location,
						"A member `{0}' is already defined. Rename this member or use different parameter types",
						c.GetSignatureForError ());
				    return;
				}

				default_static_constructor = c;
			} else {
				if (c.Parameters.IsEmpty)
					default_constructor = c;
				
				if (instance_constructors == null)
					instance_constructors = new MemberCoreArrayList ();
				
				instance_constructors.Add (c);
			}
		}

		public bool AddField (FieldBase field)
		{
			if (!AddMember (field))
				return false;

			if (fields == null)
				fields = new MemberCoreArrayList ();

			fields.Add (field);

			if ((field.ModFlags & Modifiers.STATIC) != 0)
				return true;

			if (first_nonstatic_field == null) {
				first_nonstatic_field = field;
				return true;
			}

			if (Kind == Kind.Struct && first_nonstatic_field.Parent != field.Parent) {
				Report.SymbolRelatedToPreviousError (first_nonstatic_field.Parent);
				Report.Warning (282, 3, field.Location,
					"struct instance field `{0}' found in different declaration from instance field `{1}'",
					field.GetSignatureForError (), first_nonstatic_field.GetSignatureForError ());
			}
			return true;
		}

		public void AddProperty (Property prop)
		{
			if (!AddMember (prop) || 
				!AddMember (prop.Get) || !AddMember (prop.Set))
				return;

			if (properties == null)
				properties = new MemberCoreArrayList ();

			if (prop.MemberName.Left != null)
				AddMemberToList (prop, properties, true);
			else 
				AddMemberToList (prop, properties, false);
		}

		public void AddEvent (Event e)
		{
			if (!AddMember (e))
				return;

			if (e is EventProperty) {
				if (!AddMember (e.Add))
					return;

				if (!AddMember (e.Remove))
					return;
			}

			if (events == null)
				events = new MemberCoreArrayList ();

			events.Add (e);
		}

		/// <summary>
		/// Indexer has special handling in constrast to other AddXXX because the name can be driven by IndexerNameAttribute
		/// </summary>
		public void AddIndexer (Indexer i)
		{
			if (indexers == null)
				indexers = new ArrayList ();

			if (i.IsExplicitImpl)
				AddMemberToList (i, indexers, true);
			else 
				AddMemberToList (i, indexers, false);
		}

		public void AddOperator (Operator op)
		{
			if (!AddMember (op))
				return;

			if (operators == null)
				operators = new OperatorArrayList (this);

			operators.Add (op);
		}

		public void AddCompilerGeneratedClass (CompilerGeneratedClass c)
		{
			Report.Debug (64, "ADD COMPILER GENERATED CLASS", this, c);

			if (compiler_generated == null)
				compiler_generated = new ArrayList ();

			compiler_generated.Add (c);
		}

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb, PredefinedAttributes pa)
		{
			if (a.Type == pa.DefaultMember) {
				if (Indexers != null) {
					Report.Error (646, a.Location, "Cannot specify the `DefaultMember' attribute on type containing an indexer");
					return;
				}
			}
			
			base.ApplyAttributeBuilder (a, cb, pa);
		} 

		public override AttributeTargets AttributeTargets {
			get {
				throw new NotSupportedException ();
			}
		}

		public ArrayList Types {
			get {
				return types;
			}
		}

		public MemberCoreArrayList Methods {
			get {
				return methods;
			}
		}

		public ArrayList Constants {
			get {
				return constants;
			}
		}

		protected Type BaseType {
			get {
				return TypeBuilder.BaseType;
			}
		}

		public ArrayList Fields {
			get {
				return fields;
			}
		}

		public ArrayList InstanceConstructors {
			get {
				return instance_constructors;
			}
		}

		public ArrayList Properties {
			get {
				return properties;
			}
		}

		public ArrayList Events {
			get {
				return events;
			}
		}
		
		public ArrayList Indexers {
			get {
				return indexers;
			}
		}

		public ArrayList Operators {
			get {
				return operators;
			}
		}

		public ArrayList Delegates {
			get {
				return delegates;
			}
		}
		
		protected override TypeAttributes TypeAttr {
			get {
				return Modifiers.TypeAttr (ModFlags, IsTopLevel) | base.TypeAttr;
			}
		}

		public string IndexerName {
			get {
				return indexers == null ? DefaultIndexerName : indexer_name;
			}
		}

		public bool IsComImport {
			get {
				if (OptAttributes == null)
					return false;

				return OptAttributes.Contains (PredefinedAttributes.Get.ComImport);
			}
		}

		public virtual void RegisterFieldForInitialization (MemberCore field, FieldInitializer expression)
		{
			if ((field.ModFlags & Modifiers.STATIC) != 0){
				if (initialized_static_fields == null) {
					PartialContainer.HasStaticFieldInitializer = true;
					initialized_static_fields = new ArrayList (4);
				}

				initialized_static_fields.Add (expression);
			} else {
				if (initialized_fields == null)
					initialized_fields = new ArrayList (4);

				initialized_fields.Add (expression);
			}
		}

		public void ResolveFieldInitializers (BlockContext ec)
		{
			if (partial_parts != null) {
				foreach (TypeContainer part in partial_parts) {
					part.DoResolveFieldInitializers (ec);
				}
			}
			DoResolveFieldInitializers (ec);
		}

		void DoResolveFieldInitializers (BlockContext ec)
		{
			if (ec.IsStatic) {
				if (initialized_static_fields == null)
					return;

				bool has_complex_initializer = !RootContext.Optimize;
				int i;
				ExpressionStatement [] init = new ExpressionStatement [initialized_static_fields.Count];
				for (i = 0; i < initialized_static_fields.Count; ++i) {
					FieldInitializer fi = (FieldInitializer) initialized_static_fields [i];
					ExpressionStatement s = fi.ResolveStatement (ec);
					if (s == null) {
						s = EmptyExpressionStatement.Instance;
					} else if (fi.IsComplexInitializer) {
						has_complex_initializer |= true;
					}

					init [i] = s;
				}

				for (i = 0; i < initialized_static_fields.Count; ++i) {
					FieldInitializer fi = (FieldInitializer) initialized_static_fields [i];
					//
					// Need special check to not optimize code like this
					// static int a = b = 5;
					// static int b = 0;
					//
					if (!has_complex_initializer && fi.IsDefaultInitializer)
						continue;

					ec.CurrentBlock.AddScopeStatement (new StatementExpression (init [i]));
				}

				return;
			}

			if (initialized_fields == null)
				return;

			for (int i = 0; i < initialized_fields.Count; ++i) {
				FieldInitializer fi = (FieldInitializer) initialized_fields [i];
				ExpressionStatement s = fi.ResolveStatement (ec);
				if (s == null)
					continue;

				//
				// Field is re-initialized to its default value => removed
				//
				if (fi.IsDefaultInitializer && RootContext.Optimize)
					continue;

				ec.CurrentBlock.AddScopeStatement (new StatementExpression (s));
			}
		}

		public override string DocComment {
			get {
				return comment;
			}
			set {
				if (value == null)
					return;

				comment += value;
			}
		}

		public PendingImplementation PendingImplementations {
			get { return pending; }
		}

		public override bool GetClsCompliantAttributeValue ()
		{
			if (PartialContainer != this)
				return PartialContainer.GetClsCompliantAttributeValue ();

			return base.GetClsCompliantAttributeValue ();
		}

		public virtual void AddBasesForPart (DeclSpace part, ArrayList bases)
		{
			// FIXME: get rid of partial_parts and store lists of bases of each part here
			// assumed, not verified: 'part' is in 'partial_parts' 
			((TypeContainer) part).type_bases = bases;
		}

		/// <summary>
		///   This function computes the Base class and also the
		///   list of interfaces that the class or struct @c implements.
		///   
		///   The return value is an array (might be null) of
		///   interfaces implemented (as Types).
		///   
		///   The @base_class argument is set to the base object or null
		///   if this is `System.Object'. 
		/// </summary>
		protected virtual TypeExpr[] ResolveBaseTypes (out TypeExpr base_class)
		{
			base_class = null;
			if (type_bases == null)
				return null;

			int count = type_bases.Count;
			TypeExpr [] ifaces = null;
			IMemberContext base_context = new BaseContext (this);
			for (int i = 0, j = 0; i < count; i++){
				FullNamedExpression fne = (FullNamedExpression) type_bases [i];

				//
				// Standard ResolveAsTypeTerminal cannot be used in this case because
				// it does ObsoleteAttribute and constraint checks which require
				// base type to be set
				//
				TypeExpr fne_resolved = fne.ResolveAsBaseTerminal (base_context, false);
				if (fne_resolved == null)
					continue;

				if (i == 0 && Kind == Kind.Class && !fne_resolved.Type.IsInterface) {
					if (fne_resolved is DynamicTypeExpr)
						Report.Error (1965, Location, "Class `{0}' cannot derive from the dynamic type",
							GetSignatureForError ());
					else
						base_class = fne_resolved;
					continue;
				}

				if (ifaces == null)
					ifaces = new TypeExpr [count - i];

				if (fne_resolved.Type.IsInterface) {
					for (int ii = 0; ii < j; ++ii) {
						if (TypeManager.IsEqual (fne_resolved.Type, ifaces [ii].Type)) {
							Report.Error (528, Location, "`{0}' is already listed in interface list",
								fne_resolved.GetSignatureForError ());
							break;
						}
					}

					if (Kind == Kind.Interface && !IsAccessibleAs (fne_resolved.Type)) {
						Report.Error (61, fne.Location,
							"Inconsistent accessibility: base interface `{0}' is less accessible than interface `{1}'",
							fne_resolved.GetSignatureForError (), GetSignatureForError ());
					}
				} else {
					Report.SymbolRelatedToPreviousError (fne_resolved.Type);
					if (Kind != Kind.Class) {
						Report.Error (527, fne.Location, "Type `{0}' in interface list is not an interface", fne_resolved.GetSignatureForError ());
					} else if (base_class != null)
						Report.Error (1721, fne.Location, "`{0}': Classes cannot have multiple base classes (`{1}' and `{2}')",
							GetSignatureForError (), base_class.GetSignatureForError (), fne_resolved.GetSignatureForError ());
					else {
						Report.Error (1722, fne.Location, "`{0}': Base class `{1}' must be specified as first",
							GetSignatureForError (), fne_resolved.GetSignatureForError ());
					}
				}

				ifaces [j++] = fne_resolved;
			}

			return ifaces;
		}

		TypeExpr[] GetNormalPartialBases (ref TypeExpr base_class)
		{
			ArrayList ifaces = new ArrayList (0);
			if (iface_exprs != null)
				ifaces.AddRange (iface_exprs);

			foreach (TypeContainer part in partial_parts) {
				TypeExpr new_base_class;
				TypeExpr[] new_ifaces = part.ResolveBaseTypes (out new_base_class);
				if (new_base_class != TypeManager.system_object_expr) {
					if (base_class == TypeManager.system_object_expr)
						base_class = new_base_class;
					else {
						if (new_base_class != null && !TypeManager.IsEqual (new_base_class.Type, base_class.Type)) {
							Report.SymbolRelatedToPreviousError (base_class.Location, "");
							Report.Error (263, part.Location,
								"Partial declarations of `{0}' must not specify different base classes",
								part.GetSignatureForError ());

							return null;
						}
					}
				}

				if (new_ifaces == null)
					continue;

				foreach (TypeExpr iface in new_ifaces) {
					if (ifaces.Contains (iface))
						continue;

					ifaces.Add (iface);
				}
			}

			if (ifaces.Count == 0)
				return null;

			return (TypeExpr[])ifaces.ToArray (typeof (TypeExpr));
		}

		bool CheckGenericInterfaces (Type[] ifaces)
		{
#if GMCS_SOURCE
			ArrayList already_checked = new ArrayList ();

			for (int i = 0; i < ifaces.Length; i++) {
				Type iface = ifaces [i];
				foreach (Type t in already_checked) {
					if (iface == t)
						continue;

					Type[] inferred = new Type [CountTypeParameters];
					if (!TypeManager.MayBecomeEqualGenericInstances (iface, t, inferred, null))
						continue;

					Report.Error (695, Location,
						"`{0}' cannot implement both `{1}' and `{2}' " +
						"because they may unify for some type parameter substitutions",
						TypeManager.CSharpName (TypeBuilder), TypeManager.CSharpName (iface),
						TypeManager.CSharpName (t));
					return false;
				}

				already_checked.Add (iface);
			}
#endif

			return true;
		}

		bool error = false;
		
		bool CreateTypeBuilder ()
		{
			try {
				Type default_parent = null;
				if (Kind == Kind.Struct)
					default_parent = TypeManager.value_type;
				else if (Kind == Kind.Enum)
					default_parent = TypeManager.enum_type;
				else if (Kind == Kind.Delegate)
					default_parent = TypeManager.multicast_delegate_type;

				//
				// Sets .size to 1 for structs with no instance fields
				//
				int type_size = Kind == Kind.Struct && first_nonstatic_field == null ? 1 : 0;

				if (IsTopLevel){
					if (GlobalRootNamespace.Instance.IsNamespace (Name)) {
						Report.Error (519, Location, "`{0}' clashes with a predefined namespace", Name);
						return false;
					}

					ModuleBuilder builder = Module.Builder;
					TypeBuilder = builder.DefineType (
						Name, TypeAttr, default_parent, type_size);
				} else {
					TypeBuilder builder = Parent.TypeBuilder;

					TypeBuilder = builder.DefineNestedType (
						Basename, TypeAttr, default_parent, type_size);
				}
			} catch (ArgumentException) {
				Report.RuntimeMissingSupport (Location, "static classes");
				return false;
			}

			TypeManager.AddUserType (this);

			if (IsGeneric) {
				string[] param_names = new string [TypeParameters.Length];
				for (int i = 0; i < TypeParameters.Length; i++)
					param_names [i] = TypeParameters [i].Name;

#if GMCS_SOURCE
				GenericTypeParameterBuilder[] gen_params = TypeBuilder.DefineGenericParameters (param_names);

				int offset = CountTypeParameters;
				if (CurrentTypeParameters != null)
					offset -= CurrentTypeParameters.Length;

				if (offset > 0) {
					nested_gen_params = new GenericTypeParameterBuilder [offset];
					Array.Copy (gen_params, nested_gen_params, offset);
				}

				for (int i = offset; i < gen_params.Length; i++)
					CurrentTypeParameters [i - offset].Define (gen_params [i]);
#else
				nested_gen_params = null;
				throw new NotSupportedException ();
#endif
			}

			return true;
		}

		bool DefineBaseTypes ()
		{
			iface_exprs = ResolveBaseTypes (out base_type);
			if (partial_parts != null) {
				iface_exprs = GetNormalPartialBases (ref base_type);
			}

			//
			// GetClassBases calls ResolveBaseTypeExpr() on the various type expressions involved,
			// which in turn should have called DefineType()s on base types if necessary.
			//
			// None of the code below should trigger DefineType()s on classes that we depend on.
			// Thus, we are eligible to be on the topological sort `type_container_resolve_order'.
			//
			// Let's do it as soon as possible, since code below can call DefineType() on classes
			// that depend on us to be populated before they are.
			//
			if (!(this is CompilerGeneratedClass) && !(this is Delegate))
				RootContext.RegisterOrder (this); 

			if (!CheckRecursiveDefinition (this))
				return false;

			if (base_type != null && base_type.Type != null) {
				TypeBuilder.SetParent (base_type.Type);
			}

			// add interfaces that were not added at type creation
			if (iface_exprs != null) {
				ifaces = TypeManager.ExpandInterfaces (iface_exprs);
				if (ifaces == null)
					return false;

				foreach (Type itype in ifaces)
 					TypeBuilder.AddInterfaceImplementation (itype);

				if (!CheckGenericInterfaces (ifaces))
					return false;

				TypeManager.RegisterBuilder (TypeBuilder, ifaces);
			}

			return true;
		}

		//
		// Defines the type in the appropriate ModuleBuilder or TypeBuilder.
		//
		public TypeBuilder CreateType ()
		{
			if (TypeBuilder != null)
				return TypeBuilder;

			if (error)
				return null;

			if (!CreateTypeBuilder ()) {
				error = true;
				return null;
			}

			if (partial_parts != null) {
				foreach (TypeContainer part in partial_parts)
					part.TypeBuilder = TypeBuilder;
			}

			if (Types != null) {
				foreach (TypeContainer tc in Types) {
					if (tc.CreateType () == null) {
						error = true;
						return null;
					}
				}
			}

			return TypeBuilder;
		}

		bool type_defined;

		public override TypeBuilder DefineType ()
		{
			if (error)
				return null;
			if (type_defined)
				return TypeBuilder;

			type_defined = true;

			if (CreateType () == null) {
				error = true;
				return null;
			}

			if (!DefineBaseTypes ()) {
				error = true;
				return null;
			}

			if (!DefineNestedTypes ()) {
				error = true;
				return null;
			}

			return TypeBuilder;
		}

		public override void SetParameterInfo (ArrayList constraints_list)
		{
			base.SetParameterInfo (constraints_list);

			if (!is_generic || PartialContainer == this)
				return;

			TypeParameter[] tc_names = PartialContainer.TypeParameters;
			for (int i = 0; i < tc_names.Length; ++i) {
				if (tc_names [i].Name != type_params [i].Name) {
					Report.SymbolRelatedToPreviousError (PartialContainer.Location, "");
					Report.Error (264, Location, "Partial declarations of `{0}' must have the same type parameter names in the same order",
						GetSignatureForError ());
					break;
				}
			}
		}

		void UpdateTypeParameterConstraints (TypeContainer part)
		{
			TypeParameter[] current_params = type_params;
			for (int i = 0; i < current_params.Length; i++) {
				Constraints c = part.type_params [i].Constraints;
				if (c == null)
					continue;

				if (current_params [i].UpdateConstraints (part, c))
					continue;

				Report.SymbolRelatedToPreviousError (Location, "");
				Report.Error (265, part.Location,
					"Partial declarations of `{0}' have inconsistent constraints for type parameter `{1}'",
					GetSignatureForError (), current_params [i].GetSignatureForError ());
			}
		}

		public bool ResolveType ()
		{
			if (!DoResolveType ())
				return false;

			if (compiler_generated != null) {
				foreach (CompilerGeneratedClass c in compiler_generated)
					if (!c.ResolveType ())
						return false;
			}

			return true;
		}

		protected virtual bool DoResolveType ()
		{
			if (!IsGeneric)
				return true;

			if (PartialContainer != this)
				throw new InternalErrorException ();

			TypeExpr current_type = null;
			if (CurrentTypeParameters != null) {
				foreach (TypeParameter type_param in CurrentTypeParameters) {
					if (!type_param.Resolve (this)) {
						error = true;
						return false;
					}
				}

				if (partial_parts != null) {
					foreach (TypeContainer part in partial_parts)
						UpdateTypeParameterConstraints (part);
				}
			}

			for (int i = 0; i < TypeParameters.Length; ++i) {
				//
				// FIXME: Same should be done for delegates
				// TODO: Quite ugly way how to propagate constraints to
				// nested types
				//
				if (nested_gen_params != null && i < nested_gen_params.Length) {
					TypeParameters [i].SetConstraints (nested_gen_params [i]);
				} else {
					if (!TypeParameters [i].DefineType (this)) {
						error = true;
						return false;
					}
				}
			}

			// TODO: Very strange, why not simple make generic type from
			// current type parameters
			current_type = new GenericTypeExpr (this, Location);
			current_type = current_type.ResolveAsTypeTerminal (this, false);
			if (current_type == null) {
				error = true;
				return false;
			}

			currentType = current_type.Type;
			return true;
		}

		protected virtual bool DefineNestedTypes ()
		{
			if (Types != null) {
				foreach (TypeContainer tc in Types)
					if (tc.DefineType () == null)
						return false;
			}

			if (Delegates != null) {
				foreach (Delegate d in Delegates)
					if (d.DefineType () == null)
						return false;
			}

			return true;
		}

		TypeContainer InTransit;

		protected bool CheckRecursiveDefinition (TypeContainer tc)
		{
			if (InTransit != null) {
				Report.SymbolRelatedToPreviousError (this);
				if (this is Interface)
					Report.Error (
						529, tc.Location, "Inherited interface `{0}' causes a " +
						"cycle in the interface hierarchy of `{1}'",
						GetSignatureForError (), tc.GetSignatureForError ());
				else
					Report.Error (
						146, tc.Location, "Circular base class dependency " +
						"involving `{0}' and `{1}'",
						tc.GetSignatureForError (), GetSignatureForError ());
				return false;
			}

			InTransit = tc;

			if (base_type != null && base_type.Type != null) {
				Type t = TypeManager.DropGenericTypeArguments (base_type.Type);
				TypeContainer ptc = TypeManager.LookupTypeContainer (t);
				if ((ptc != null) && !ptc.CheckRecursiveDefinition (this))
					return false;
			}

			if (iface_exprs != null) {
				foreach (TypeExpr iface in iface_exprs) {
					Type itype = TypeManager.DropGenericTypeArguments (iface.Type);
					TypeContainer ptc = TypeManager.LookupTypeContainer (itype);
					if ((ptc != null) && !ptc.CheckRecursiveDefinition (this))
						return false;
				}
			}

			if (!IsTopLevel && !Parent.PartialContainer.CheckRecursiveDefinition (this))
				return false;

			InTransit = null;
			return true;
		}

		public override TypeParameter[] CurrentTypeParameters {
			get {
				return PartialContainer.type_params;
			}
		}

		/// <summary>
		///   Populates our TypeBuilder with fields and methods
		/// </summary>
		public override bool Define ()
		{
			if (members_defined)
				return members_defined_ok;

			members_defined_ok = DoDefineMembers ();
			members_defined = true;

			return members_defined_ok;
		}

		protected virtual bool DoDefineMembers ()
		{
			if (iface_exprs != null) {
				foreach (TypeExpr iface in iface_exprs) {
					ObsoleteAttribute oa = AttributeTester.GetObsoleteAttribute (iface.Type);
					if ((oa != null) && !IsObsolete)
						AttributeTester.Report_ObsoleteMessage (
							oa, iface.GetSignatureForError (), Location, Report);

					GenericTypeExpr ct = iface as GenericTypeExpr;
					if (ct != null) {
						// TODO: passing `this' is wrong, should be base type iface instead
						TypeManager.CheckTypeVariance (ct.Type, Variance.Covariant, this);

						if (!ct.CheckConstraints (this))
							return false;
					}
				}
			}

			if (base_type != null) {
				ObsoleteAttribute obsolete_attr = AttributeTester.GetObsoleteAttribute (base_type.Type);
				if (obsolete_attr != null && !IsObsolete)
					AttributeTester.Report_ObsoleteMessage (obsolete_attr, base_type.GetSignatureForError (), Location, Report);

				GenericTypeExpr ct = base_type as GenericTypeExpr;
				if ((ct != null) && !ct.CheckConstraints (this))
					return false;
				
				TypeContainer baseContainer = TypeManager.LookupTypeContainer(base_type.Type);
				if (baseContainer != null)
					baseContainer.Define();				
				
				member_cache = new MemberCache (base_type.Type, this);
			} else if (Kind == Kind.Interface) {
				member_cache = new MemberCache (null, this);
				Type [] ifaces = TypeManager.GetInterfaces (TypeBuilder);
				for (int i = 0; i < ifaces.Length; ++i)
					member_cache.AddInterface (TypeManager.LookupMemberCache (ifaces [i]));
			} else {
				member_cache = new MemberCache (null, this);
			}

			if (types != null)
				foreach (TypeContainer tc in types)
					member_cache.AddNestedType (tc);

			if (delegates != null)
				foreach (Delegate d in delegates)
					member_cache.AddNestedType (d);

			if (partial_parts != null) {
				foreach (TypeContainer part in partial_parts)
					part.member_cache = member_cache;
			}

			if (!IsTopLevel) {
				MemberInfo conflict_symbol = Parent.PartialContainer.FindBaseMemberWithSameName (Basename, false);
				if (conflict_symbol == null) {
					if ((ModFlags & Modifiers.NEW) != 0)
						Report.Warning (109, 4, Location, "The member `{0}' does not hide an inherited member. The new keyword is not required", GetSignatureForError ());
				} else {
					if ((ModFlags & Modifiers.NEW) == 0) {
						Report.SymbolRelatedToPreviousError (conflict_symbol);
						Report.Warning (108, 2, Location, "`{0}' hides inherited member `{1}'. Use the new keyword if hiding was intended",
							GetSignatureForError (), TypeManager.GetFullNameSignature (conflict_symbol));
					}
				}
			}

			DefineContainerMembers (constants);
			DefineContainerMembers (fields);

			if (Kind == Kind.Struct || Kind == Kind.Class) {
				pending = PendingImplementation.GetPendingImplementations (this);

				if (requires_delayed_unmanagedtype_check) {
					requires_delayed_unmanagedtype_check = false;
					foreach (FieldBase f in fields) {
						if (f.MemberType != null && f.MemberType.IsPointer)
							TypeManager.VerifyUnManaged (f.MemberType, f.Location);
					}
				}
			}
		
			//
			// Constructors are not in the defined_names array
			//
			DefineContainerMembers (instance_constructors);
		
			DefineContainerMembers (events);
			DefineContainerMembers (ordered_explicit_member_list);
			DefineContainerMembers (ordered_member_list);

			DefineContainerMembers (operators);
			DefineContainerMembers (delegates);

			ComputeIndexerName();
			CheckEqualsAndGetHashCode();

			if (CurrentType != null) {
				GenericType = CurrentType;
			}

			//
			// FIXME: This hack is needed because member cache does not work
			// with generic types, we rely on runtime to inflate dynamic types.
			// TODO: This hack requires member cache refactoring to be removed
			//
			if (TypeManager.IsGenericType (TypeBuilder))
				member_cache = new MemberCache (this);

			return true;
		}

		protected virtual void DefineContainerMembers (MemberCoreArrayList mcal)
		{
			if (mcal != null)
				mcal.DefineContainerMembers ();
		}
		
		protected virtual void ComputeIndexerName ()
		{
			if (indexers == null)
				return;

			string class_indexer_name = null;

			//
			// If there's both an explicit and an implicit interface implementation, the
			// explicit one actually implements the interface while the other one is just
			// a normal indexer.  See bug #37714.
			//

			// Invariant maintained by AddIndexer(): All explicit interface indexers precede normal indexers
			foreach (Indexer i in indexers) {
				if (i.InterfaceType != null) {
					if (seen_normal_indexers)
						throw new Exception ("Internal Error: 'Indexers' array not sorted properly.");
					continue;
				}

				seen_normal_indexers = true;

				if (class_indexer_name == null) {
					class_indexer_name = i.ShortName;
					continue;
				}

				if (i.ShortName != class_indexer_name)
					Report.Error (668, i.Location, "Two indexers have different names; the IndexerName attribute must be used with the same name on every indexer within a type");
			}

			if (class_indexer_name != null)
				indexer_name = class_indexer_name;
		}

		protected virtual void EmitIndexerName ()
		{
			if (!seen_normal_indexers)
				return;

			PredefinedAttribute pa = PredefinedAttributes.Get.DefaultMember;
			if (pa.Constructor == null &&
				!pa.ResolveConstructor (Location, TypeManager.string_type))
				return;

			CustomAttributeBuilder cb = new CustomAttributeBuilder (pa.Constructor, new string [] { IndexerName });
			TypeBuilder.SetCustomAttribute (cb);
		}

		protected virtual void CheckEqualsAndGetHashCode ()
		{
			if (methods == null)
				return;

			if (HasEquals && !HasGetHashCode) {
				Report.Warning (659, 3, this.Location, "`{0}' overrides Object.Equals(object) but does not override Object.GetHashCode()", this.GetSignatureForError ());
			}
		}

		public MemberInfo FindBaseMemberWithSameName (string name, bool ignore_methods)
		{
			return BaseCache == null ? null : BaseCache.FindMemberWithSameName (name, ignore_methods, null);
		}

		/// <summary>
		///   This function is based by a delegate to the FindMembers routine
		/// </summary>
		static bool AlwaysAccept (MemberInfo m, object filterCriteria)
		{
			return true;
		}

		/// <summary>
		///   This filter is used by FindMembers, and we just keep
		///   a global for the filter to `AlwaysAccept'
		/// </summary>
		static MemberFilter accepting_filter;

		
		static TypeContainer ()
		{
			accepting_filter = new MemberFilter (AlwaysAccept);
		}

		public MethodInfo[] GetMethods ()
		{
			ArrayList members = new ArrayList ();

			Define ();

			if (methods != null) {
				int len = methods.Count;
				for (int i = 0; i < len; i++) {
					Method m = (Method) methods [i];

					members.Add (m.MethodBuilder);
				}
			}

			if (operators != null) {
				int len = operators.Count;
				for (int i = 0; i < len; i++) {
					Operator o = (Operator) operators [i];

					members.Add (o.MethodBuilder);
				}
			}

			if (properties != null) {
				int len = properties.Count;
				for (int i = 0; i < len; i++) {
					Property p = (Property) properties [i];

					if (p.GetBuilder != null)
						members.Add (p.GetBuilder);
					if (p.SetBuilder != null)
						members.Add (p.SetBuilder);
				}
			}
				
			if (indexers != null) {
				int len = indexers.Count;
				for (int i = 0; i < len; i++) {
					Indexer ix = (Indexer) indexers [i];

					if (ix.GetBuilder != null)
						members.Add (ix.GetBuilder);
					if (ix.SetBuilder != null)
						members.Add (ix.SetBuilder);
				}
			}

			if (events != null) {
				int len = events.Count;
				for (int i = 0; i < len; i++) {
					Event e = (Event) events [i];

					if (e.AddBuilder != null)
						members.Add (e.AddBuilder);
					if (e.RemoveBuilder != null)
						members.Add (e.RemoveBuilder);
				}
			}

			MethodInfo[] retMethods = new MethodInfo [members.Count];
			members.CopyTo (retMethods, 0);
			return retMethods;
		}
		
		// Indicated whether container has StructLayout attribute set Explicit
		public bool HasExplicitLayout {
			get { return (caching_flags & Flags.HasExplicitLayout) != 0; }
			set { caching_flags |= Flags.HasExplicitLayout; }
		}

		public bool HasStructLayout {
			get { return (caching_flags & Flags.HasStructLayout) != 0; }
			set { caching_flags |= Flags.HasStructLayout; }
		}

		//
		// Return the nested type with name @name.  Ensures that the nested type
		// is defined if necessary.  Do _not_ use this when you have a MemberCache handy.
		//
		public Type FindNestedType (string name)
		{
			if (PartialContainer != this)
				throw new InternalErrorException ("should not happen");

			ArrayList [] lists = { types, delegates };

			for (int j = 0; j < lists.Length; ++j) {
				ArrayList list = lists [j];
				if (list == null)
					continue;
				
				int len = list.Count;
				for (int i = 0; i < len; ++i) {
					DeclSpace ds = (DeclSpace) list [i];
					if (ds.Basename == name) {
						return ds.DefineType ();
					}
				}
			}

			return null;
		}

		private void FindMembers_NestedTypes (int modflags,
						      BindingFlags bf, MemberFilter filter, object criteria,
						      ref ArrayList members)
		{
			ArrayList [] lists = { types, delegates };

			for (int j = 0; j < lists.Length; ++j) {
				ArrayList list = lists [j];
				if (list == null)
					continue;
			
				int len = list.Count;
				for (int i = 0; i < len; i++) {
					DeclSpace ds = (DeclSpace) list [i];
					
					if ((ds.ModFlags & modflags) == 0)
						continue;
					
					TypeBuilder tb = ds.TypeBuilder;
					if (tb == null) {
						if (!(criteria is string) || ds.Basename.Equals (criteria))
							tb = ds.DefineType ();
					}
					
					if (tb != null && (filter (tb, criteria) == true)) {
						if (members == null)
							members = new ArrayList ();
						
						members.Add (tb);
					}
				}
			}
		}
		
		/// <summary>
		///   This method returns the members of this type just like Type.FindMembers would
		///   Only, we need to use this for types which are _being_ defined because MS' 
		///   implementation can't take care of that.
		/// </summary>
		//
		// FIXME: return an empty static array instead of null, that cleans up
		// some code and is consistent with some coding conventions I just found
		// out existed ;-)
		//
		//
		// Notice that in various cases we check if our field is non-null,
		// something that would normally mean that there was a bug elsewhere.
		//
		// The problem happens while we are defining p-invoke methods, as those
		// will trigger a FindMembers, but this happens before things are defined
		//
		// Since the whole process is a no-op, it is fine to check for null here.
		//
		// TODO: This approach will be one day completely removed, it's already
		// used at few places only
		//
		//
		public override MemberList FindMembers (MemberTypes mt, BindingFlags bf,
							MemberFilter filter, object criteria)
		{
			ArrayList members = null;

			int modflags = 0;
			if ((bf & BindingFlags.Public) != 0)
				modflags |= Modifiers.PUBLIC | Modifiers.PROTECTED |
					Modifiers.INTERNAL;
			if ((bf & BindingFlags.NonPublic) != 0)
				modflags |= Modifiers.PRIVATE;

			int static_mask = 0, static_flags = 0;
			switch (bf & (BindingFlags.Static | BindingFlags.Instance)) {
			case BindingFlags.Static:
				static_mask = static_flags = Modifiers.STATIC;
				break;

			case BindingFlags.Instance:
				static_mask = Modifiers.STATIC;
				static_flags = 0;
				break;

			default:
				static_mask = static_flags = 0;
				break;
			}

			Timer.StartTimer (TimerType.TcFindMembers);

			if (filter == null)
				filter = accepting_filter; 

			if ((mt & MemberTypes.Field) != 0) {
				if (fields != null) {
					int len = fields.Count;
					for (int i = 0; i < len; i++) {
						FieldBase f = (FieldBase) fields [i];
						
						if ((f.ModFlags & modflags) == 0)
							continue;
						if ((f.ModFlags & static_mask) != static_flags)
							continue;

						FieldBuilder fb = f.FieldBuilder;
						if (fb != null && filter (fb, criteria) == true) {
							if (members == null)
								members = new ArrayList ();
							
							members.Add (fb);
						}
					}
				}

				if (constants != null) {
					int len = constants.Count;
					for (int i = 0; i < len; i++) {
						Const con = (Const) constants [i];
						
						if ((con.ModFlags & modflags) == 0)
							continue;
						if ((con.ModFlags & static_mask) != static_flags)
							continue;

						FieldBuilder fb = con.FieldBuilder;
						if (fb == null) {
							// Define parent and not member, otherwise membercache can be null
							if (con.Parent.Define ())
								fb = con.FieldBuilder;
						}
						if (fb != null && filter (fb, criteria) == true) {
							if (members == null)
								members = new ArrayList ();
							
							members.Add (fb);
						}
					}
				}
			}

			if ((mt & MemberTypes.Method) != 0) {
				if (methods != null) {
					int len = methods.Count;
					for (int i = 0; i < len; i++) {
						MethodOrOperator m = (MethodOrOperator) methods [i];
						
						if ((m.ModFlags & modflags) == 0)
							continue;
						if ((m.ModFlags & static_mask) != static_flags)
							continue;
						
						MethodBuilder mb = m.MethodBuilder;

						if (mb != null && filter (mb, criteria) == true) {
							if (members == null)
								members = new ArrayList ();
							
                                                        members.Add (mb);
						}
					}
				}

				if (operators != null) {
					int len = operators.Count;
					for (int i = 0; i < len; i++) {
						Operator o = (Operator) operators [i];
						
						if ((o.ModFlags & modflags) == 0)
							continue;
						if ((o.ModFlags & static_mask) != static_flags)
							continue;
						
						MethodBuilder ob = o.MethodBuilder;
						if (ob != null && filter (ob, criteria) == true) {
							if (members == null)
								members = new ArrayList ();
							
							members.Add (ob);
						}
					}
				}

				if (events != null) {
					foreach (Event e in events) {
						if ((e.ModFlags & modflags) == 0)
							continue;
						if ((e.ModFlags & static_mask) != static_flags)
							continue;

						MethodBuilder b = e.AddBuilder;
						if (b != null && filter (b, criteria)) {
							if (members == null)
								members = new ArrayList (4);

							members.Add (b);
						}

						b = e.RemoveBuilder;
						if (b != null && filter (b, criteria)) {
							if (members == null) 
								members = new ArrayList (4);

							members.Add (b);
						}
					}
				}

				if (properties != null) {
					int len = properties.Count;
					for (int i = 0; i < len; i++) {
						Property p = (Property) properties [i];
						
						if ((p.ModFlags & modflags) == 0)
							continue;
						if ((p.ModFlags & static_mask) != static_flags)
							continue;
						
						MethodBuilder b;

						b = p.GetBuilder;
						if (b != null && filter (b, criteria) == true) {
							if (members == null)
								members = new ArrayList ();
							
							members.Add (b);
						}

						b = p.SetBuilder;
						if (b != null && filter (b, criteria) == true) {
							if (members == null)
								members = new ArrayList ();
							
							members.Add (b);
						}
					}
				}
				
				if (indexers != null) {
					int len = indexers.Count;
					for (int i = 0; i < len; i++) {
						Indexer ix = (Indexer) indexers [i];
						
						if ((ix.ModFlags & modflags) == 0)
							continue;
						if ((ix.ModFlags & static_mask) != static_flags)
							continue;
						
						MethodBuilder b;

						b = ix.GetBuilder;
						if (b != null && filter (b, criteria) == true) {
							if (members == null)
								members = new ArrayList ();
							
							members.Add (b);
						}

						b = ix.SetBuilder;
						if (b != null && filter (b, criteria) == true) {
							if (members == null)
								members = new ArrayList ();
							
							members.Add (b);
						}
					}
				}
			}

			if ((mt & MemberTypes.Event) != 0) {
				if (events != null) {
					int len = events.Count;
					for (int i = 0; i < len; i++) {
						Event e = (Event) events [i];
						
						if ((e.ModFlags & modflags) == 0)
							continue;
						if ((e.ModFlags & static_mask) != static_flags)
							continue;

						MemberInfo eb = e.EventBuilder;
						if (eb != null && filter (eb, criteria) == true) {
							if (members == null)
								members = new ArrayList ();
							
						        members.Add (e.EventBuilder);
						}
					}
				}
			}
			
			if ((mt & MemberTypes.Property) != 0){
				if (properties != null) {
					int len = properties.Count;
					for (int i = 0; i < len; i++) {
						Property p = (Property) properties [i];
						
						if ((p.ModFlags & modflags) == 0)
							continue;
						if ((p.ModFlags & static_mask) != static_flags)
							continue;

						MemberInfo pb = p.PropertyBuilder;
						if (pb != null && filter (pb, criteria) == true) {
							if (members == null)
								members = new ArrayList ();
							
							members.Add (p.PropertyBuilder);
						}
					}
				}

				if (indexers != null) {
					int len = indexers.Count;
					for (int i = 0; i < len; i++) {
						Indexer ix = (Indexer) indexers [i];
						
						if ((ix.ModFlags & modflags) == 0)
							continue;
						if ((ix.ModFlags & static_mask) != static_flags)
							continue;

						MemberInfo ib = ix.PropertyBuilder;
						if (ib != null && filter (ib, criteria) == true) {
							if (members == null)
								members = new ArrayList ();
							
							members.Add (ix.PropertyBuilder);
						}
					}
				}
			}
			
			if ((mt & MemberTypes.NestedType) != 0)
				FindMembers_NestedTypes (modflags, bf, filter, criteria, ref members);

			if ((mt & MemberTypes.Constructor) != 0){
				if (((bf & BindingFlags.Instance) != 0) && (instance_constructors != null)){
					int len = instance_constructors.Count;
					for (int i = 0; i < len; i++) {
						Constructor c = (Constructor) instance_constructors [i];
						
						ConstructorBuilder cb = c.ConstructorBuilder;
						if (cb != null && filter (cb, criteria) == true) {
							if (members == null)
								members = new ArrayList ();

							members.Add (cb);
						}
					}
				}

				if (((bf & BindingFlags.Static) != 0) && (default_static_constructor != null)){
					ConstructorBuilder cb =
						default_static_constructor.ConstructorBuilder;
					
					if (cb != null && filter (cb, criteria) == true) {
						if (members == null)
							members = new ArrayList ();
						
						members.Add (cb);
					}
				}
			}

			//
			// Lookup members in base if requested.
			//
			if ((bf & BindingFlags.DeclaredOnly) == 0) {
				if (TypeBuilder.BaseType != null) {
					MemberList list = FindMembers (TypeBuilder.BaseType, mt, bf, filter, criteria);
					if (list.Count > 0) {
						if (members == null)
							members = new ArrayList ();
					
						members.AddRange (list);
					}
				}
			}

			Timer.StopTimer (TimerType.TcFindMembers);

			if (members == null)
				return MemberList.Empty;
			else
				return new MemberList (members);
		}

		public override MemberCache MemberCache {
			get {
				return member_cache;
			}
		}

		public static MemberList FindMembers (Type t, MemberTypes mt, BindingFlags bf,
						      MemberFilter filter, object criteria)
		{
			DeclSpace ds = TypeManager.LookupDeclSpace (t);

			if (ds != null)
				return ds.FindMembers (mt, bf, filter, criteria);
			else
				return new MemberList (t.FindMembers (mt, bf, filter, criteria));
                }

		/// <summary>
		///   Emits the values for the constants
		/// </summary>
		public void EmitConstants ()
		{
			if (constants != null)
				foreach (Const con in constants)
					con.Emit ();
			return;
		}

		void CheckMemberUsage (MemberCoreArrayList al, string member_type)
		{
			if (al == null)
				return;

			foreach (MemberCore mc in al) {
				if ((mc.ModFlags & Modifiers.Accessibility) != Modifiers.PRIVATE)
					continue;

				if (!mc.IsUsed && (mc.caching_flags & Flags.Excluded) == 0) {
					Report.Warning (169, 3, mc.Location, "The private {0} `{1}' is never used", member_type, mc.GetSignatureForError ());
				}
			}
		}

		public virtual void VerifyMembers ()
		{
			//
			// Check for internal or private fields that were never assigned
			//
			if (Report.WarningLevel >= 3) {
				if (RootContext.EnhancedWarnings) {
					CheckMemberUsage (properties, "property");
					CheckMemberUsage (methods, "method");
					CheckMemberUsage (constants, "constant");
				}

				if (fields != null){
					bool is_type_exposed = Kind == Kind.Struct || IsExposedFromAssembly ();
					foreach (FieldBase f in fields) {
						if ((f.ModFlags & Modifiers.Accessibility) != Modifiers.PRIVATE) {
							if (is_type_exposed)
								continue;

							f.SetMemberIsUsed ();
						}				
						
						if (!f.IsUsed){
							if ((f.caching_flags & Flags.IsAssigned) == 0)
								Report.Warning (169, 3, f.Location, "The private field `{0}' is never used", f.GetSignatureForError ());
							else {
#if NET_2_0
								const int error_code = 414;
#else
								const int error_code = 169;
#endif
								Report.Warning (error_code, 3, f.Location, "The private field `{0}' is assigned but its value is never used",
									f.GetSignatureForError ());
							}
							continue;
						}
						
						//
						// Only report 649 on level 4
						//
						if (Report.WarningLevel < 4)
							continue;
						
						if ((f.caching_flags & Flags.IsAssigned) != 0)
							continue;

						//
						// Don't be pendatic over serializable attributes
						//
						if (f.OptAttributes != null || PartialContainer.HasStructLayout)
							continue;
						
						Constant c = New.Constantify (f.MemberType);
						Report.Warning (649, 4, f.Location, "Field `{0}' is never assigned to, and will always have its default value `{1}'",
							f.GetSignatureForError (), c == null ? "null" : c.AsString ());
					}
				}
			}
		}

		// TODO: move to ClassOrStruct
		void EmitConstructors ()
		{
			if (instance_constructors == null)
				return;

			if (TypeBuilder.IsSubclassOf (TypeManager.attribute_type) && RootContext.VerifyClsCompliance && IsClsComplianceRequired ()) {
				bool has_compliant_args = false;

				foreach (Constructor c in instance_constructors) {
					try {
						c.Emit ();
					}
					catch (Exception e) {
						throw new InternalErrorException (c, e);
					}

					if (has_compliant_args)
						continue;

					has_compliant_args = c.HasCompliantArgs;
				}
				if (!has_compliant_args)
					Report.Warning (3015, 1, Location, "`{0}' has no accessible constructors which use only CLS-compliant types", GetSignatureForError ());
			} else {
				foreach (Constructor c in instance_constructors) {
					try {
						c.Emit ();
					}
					catch (Exception e) {
						throw new InternalErrorException (c, e);
					}
				}
			}
		}

		/// <summary>
		///   Emits the code, this step is performed after all
		///   the types, enumerations, constructors
		/// </summary>
		public virtual void EmitType ()
		{
			if (OptAttributes != null)
				OptAttributes.Emit ();

			Emit ();

			EmitConstructors ();

			// Can not continue if constants are broken
			EmitConstants ();
			if (Report.Errors > 0)
				return;

			if (default_static_constructor != null)
				default_static_constructor.Emit ();
			
			if (operators != null)
				foreach (Operator o in operators)
					o.Emit ();

			if (properties != null)
				foreach (Property p in properties)
					p.Emit ();

			if (indexers != null) {
				foreach (Indexer indx in indexers)
					indx.Emit ();
				EmitIndexerName ();
			}

			if (events != null){
				foreach (Event e in Events)
					e.Emit ();
			}

			if (methods != null) {
				for (int i = 0; i < methods.Count; ++i)
					((MethodOrOperator) methods [i]).Emit ();
			}
			
			if (fields != null)
				foreach (FieldBase f in fields)
					f.Emit ();

			if (delegates != null) {
				foreach (Delegate d in Delegates) {
					d.Emit ();
				}
			}

			if (pending != null)
				pending.VerifyPendingMethods (Report);

			if (Report.Errors > 0)
				return;

			if (compiler_generated != null) {
				for (int i = 0; i < compiler_generated.Count; ++i)
					((CompilerGeneratedClass) compiler_generated [i]).EmitType ();
			}
		}
		
		public override void CloseType ()
		{
			if ((caching_flags & Flags.CloseTypeCreated) != 0)
				return;

			try {
				caching_flags |= Flags.CloseTypeCreated;
				TypeBuilder.CreateType ();
			} catch (TypeLoadException){
				//
				// This is fine, the code still created the type
				//
//				Report.Warning (-20, "Exception while creating class: " + TypeBuilder.Name);
//				Console.WriteLine (e.Message);
			} catch (Exception e) {
				throw new InternalErrorException (this, e);
			}
			
			if (Types != null){
				foreach (TypeContainer tc in Types)
					if (tc.Kind == Kind.Struct)
						tc.CloseType ();

				foreach (TypeContainer tc in Types)
					if (tc.Kind != Kind.Struct)
						tc.CloseType ();
			}

			if (Delegates != null)
				foreach (Delegate d in Delegates)
					d.CloseType ();

			if (compiler_generated != null)
				foreach (CompilerGeneratedClass c in compiler_generated)
					c.CloseType ();
			
			PartialContainer = null;
			types = null;
//			properties = null;
			delegates = null;
			fields = null;
			initialized_fields = null;
			initialized_static_fields = null;
			constants = null;
			ordered_explicit_member_list = null;
			ordered_member_list = null;
			methods = null;
			events = null;
			indexers = null;
			operators = null;
			compiler_generated = null;
			default_constructor = null;
			default_static_constructor = null;
			type_bases = null;
			OptAttributes = null;
			ifaces = null;
			base_cache = null;
			member_cache = null;
		}

		//
		// Performs the validation on a Method's modifiers (properties have
		// the same properties).
		//
		public bool MethodModifiersValid (MemberCore mc)
		{
			const int vao = (Modifiers.VIRTUAL | Modifiers.ABSTRACT | Modifiers.OVERRIDE);
			const int va = (Modifiers.VIRTUAL | Modifiers.ABSTRACT);
			const int nv = (Modifiers.NEW | Modifiers.VIRTUAL);
			bool ok = true;
			int flags = mc.ModFlags;
			
			//
			// At most one of static, virtual or override
			//
			if ((flags & Modifiers.STATIC) != 0){
				if ((flags & vao) != 0){
					Report.Error (112, mc.Location, "A static member `{0}' cannot be marked as override, virtual or abstract",
						mc.GetSignatureForError ());
					ok = false;
				}
			}

			if (Kind == Kind.Struct){
				if ((flags & va) != 0){
					Modifiers.Error_InvalidModifier (mc.Location, "virtual or abstract", Report);
					ok = false;
				}
			}

			if ((flags & Modifiers.OVERRIDE) != 0 && (flags & nv) != 0){
				Report.Error (113, mc.Location, "A member `{0}' marked as override cannot be marked as new or virtual",
					mc.GetSignatureForError ());
				ok = false;
			}

			//
			// If the declaration includes the abstract modifier, then the
			// declaration does not include static, virtual or extern
			//
			if ((flags & Modifiers.ABSTRACT) != 0){
				if ((flags & Modifiers.EXTERN) != 0){
					Report.Error (
						180, mc.Location, "`{0}' cannot be both extern and abstract", mc.GetSignatureForError ());
					ok = false;
				}

				if ((flags & Modifiers.SEALED) != 0) {
					Report.Error (502, mc.Location, "`{0}' cannot be both abstract and sealed", mc.GetSignatureForError ());
					ok = false;
				}

				if ((flags & Modifiers.VIRTUAL) != 0){
					Report.Error (503, mc.Location, "The abstract method `{0}' cannot be marked virtual", mc.GetSignatureForError ());
					ok = false;
				}

				if ((ModFlags & Modifiers.ABSTRACT) == 0){
					Report.SymbolRelatedToPreviousError (this);
					Report.Error (513, mc.Location, "`{0}' is abstract but it is declared in the non-abstract class `{1}'",
						mc.GetSignatureForError (), GetSignatureForError ());
					ok = false;
				}
			}

			if ((flags & Modifiers.PRIVATE) != 0){
				if ((flags & vao) != 0){
					Report.Error (621, mc.Location, "`{0}': virtual or abstract members cannot be private", mc.GetSignatureForError ());
					ok = false;
				}
			}

			if ((flags & Modifiers.SEALED) != 0){
				if ((flags & Modifiers.OVERRIDE) == 0){
					Report.Error (238, mc.Location, "`{0}' cannot be sealed because it is not an override", mc.GetSignatureForError ());
					ok = false;
				}
			}

			return ok;
		}

		public Constructor DefaultStaticConstructor {
			get { return default_static_constructor; }
		}

		protected override bool VerifyClsCompliance ()
		{
			if (!base.VerifyClsCompliance ())
				return false;

			VerifyClsName ();

			Type base_type = TypeBuilder.BaseType;
			if (base_type != null && !AttributeTester.IsClsCompliant (base_type)) {
				Report.Warning (3009, 1, Location, "`{0}': base type `{1}' is not CLS-compliant", GetSignatureForError (), TypeManager.CSharpName (base_type));
			}
			return true;
		}


		/// <summary>
		/// Checks whether container name is CLS Compliant
		/// </summary>
		void VerifyClsName ()
		{
			Hashtable base_members = base_cache == null ? 
				new Hashtable () :
				base_cache.GetPublicMembers ();
			Hashtable this_members = new Hashtable ();

			foreach (DictionaryEntry entry in defined_names) {
				MemberCore mc = (MemberCore)entry.Value;
				if (!mc.IsClsComplianceRequired ())
					continue;

				string name = (string) entry.Key;
				string basename = name.Substring (name.LastIndexOf ('.') + 1);

				string lcase = basename.ToLower (System.Globalization.CultureInfo.InvariantCulture);
				object found = base_members [lcase];
				if (found == null) {
					found = this_members [lcase];
					if (found == null) {
						this_members.Add (lcase, mc);
						continue;
					}
				}

				if ((mc.ModFlags & Modifiers.OVERRIDE) != 0)
					continue;					

				if (found is MemberInfo) {
					if (basename == ((MemberInfo) found).Name)
						continue;
					Report.SymbolRelatedToPreviousError ((MemberInfo) found);
				} else {
					Report.SymbolRelatedToPreviousError ((MemberCore) found);
				}

				Report.Warning (3005, 1, mc.Location, "Identifier `{0}' differing only in case is not CLS-compliant", mc.GetSignatureForError ());
			}
		}


		/// <summary>
		///   Performs checks for an explicit interface implementation.  First it
		///   checks whether the `interface_type' is a base inteface implementation.
		///   Then it checks whether `name' exists in the interface type.
		/// </summary>
		public bool VerifyImplements (InterfaceMemberBase mb)
		{
			if (ifaces != null) {
				foreach (Type t in ifaces){
					if (TypeManager.IsEqual (t, mb.InterfaceType))
						return true;
				}
			}
			
			Report.SymbolRelatedToPreviousError (mb.InterfaceType);
			Report.Error (540, mb.Location, "`{0}': containing type does not implement interface `{1}'",
				mb.GetSignatureForError (), TypeManager.CSharpName (mb.InterfaceType));
			return false;
		}

		public override Type LookupAnyGeneric (string typeName)
		{
			if (types != null) {
				foreach (TypeContainer tc in types) {
					if (!tc.IsGeneric)
						continue;

					int pos = tc.Basename.LastIndexOf ('`');
					if (pos == typeName.Length && String.Compare (typeName, 0, tc.Basename, 0, pos) == 0)
						return tc.TypeBuilder;
				}
			}

			return base.LookupAnyGeneric (typeName);
		}

		public void Mark_HasEquals ()
		{
			cached_method |= CachedMethods.Equals;
		}

		public void Mark_HasGetHashCode ()
		{
			cached_method |= CachedMethods.GetHashCode;
		}

		/// <summary>
		/// Method container contains Equals method
		/// </summary>
		public bool HasEquals {
			get {
				return (cached_method & CachedMethods.Equals) != 0;
			}
		}
 
		/// <summary>
		/// Method container contains GetHashCode method
		/// </summary>
		public bool HasGetHashCode {
			get {
				return (cached_method & CachedMethods.GetHashCode) != 0;
			}
		}

		public bool HasStaticFieldInitializer {
			get {
				return (cached_method & CachedMethods.HasStaticFieldInitializer) != 0;
			}
			set {
				if (value)
					cached_method |= CachedMethods.HasStaticFieldInitializer;
				else
					cached_method &= ~CachedMethods.HasStaticFieldInitializer;
			}
		}

		//
		// IMemberContainer
		//

		string IMemberContainer.Name {
			get {
				return Name;
			}
		}

		Type IMemberContainer.Type {
			get {
				return TypeBuilder;
			}
		}

		bool IMemberContainer.IsInterface {
			get {
				return Kind == Kind.Interface;
			}
		}

		MemberList IMemberContainer.GetMembers (MemberTypes mt, BindingFlags bf)
		{
			BindingFlags new_bf = bf | BindingFlags.DeclaredOnly;

			if (GenericType != null)
				return TypeManager.FindMembers (GenericType, mt, new_bf,
								null, null);
			else
				return FindMembers (mt, new_bf, null, null);
		}

		//
		// Generates xml doc comments (if any), and if required,
		// handle warning report.
		//
		internal override void GenerateDocComment (DeclSpace ds)
		{
			DocUtil.GenerateTypeDocComment (this, ds, Report);
		}

		public override string DocCommentHeader {
			get { return "T:"; }
		}

		public MemberCache BaseCache {
			get {
				if (base_cache != null)
					return base_cache;
				if (TypeBuilder.BaseType != null)
					base_cache = TypeManager.LookupMemberCache (TypeBuilder.BaseType);
				if (TypeBuilder.IsInterface)
					base_cache = TypeManager.LookupBaseInterfacesCache (TypeBuilder);
				return base_cache;
			}
		}
	}

	public abstract class ClassOrStruct : TypeContainer {
		ListDictionary declarative_security;

		public ClassOrStruct (NamespaceEntry ns, DeclSpace parent,
				      MemberName name, Attributes attrs, Kind kind)
			: base (ns, parent, name, attrs, kind)
		{
		}

		protected override bool AddToContainer (MemberCore symbol, string name)
		{
			if (name == MemberName.Name) {
				if (symbol is TypeParameter) {
					Report.Error (694, symbol.Location,
						"Type parameter `{0}' has same name as containing type, or method",
						symbol.GetSignatureForError ());
					return false;
				}

				InterfaceMemberBase imb = symbol as InterfaceMemberBase;
				if (imb == null || !imb.IsExplicitImpl) {
					Report.SymbolRelatedToPreviousError (this);
					Report.Error (542, symbol.Location, "`{0}': member names cannot be the same as their enclosing type",
						symbol.GetSignatureForError ());
					return false;
				}
			}

			return base.AddToContainer (symbol, name);
		}

		public override void VerifyMembers ()
		{
			base.VerifyMembers ();

			if ((events != null) && Report.WarningLevel >= 3) {
				foreach (Event e in events){
					// Note: The event can be assigned from same class only, so we can report
					// this warning for all accessibility modes
					if ((e.caching_flags & Flags.IsUsed) == 0)
						Report.Warning (67, 3, e.Location, "The event `{0}' is never used", e.GetSignatureForError ());
				}
			}
		}

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb, PredefinedAttributes pa)
		{
			if (a.IsValidSecurityAttribute ()) {
				if (declarative_security == null)
					declarative_security = new ListDictionary ();

				a.ExtractSecurityPermissionSet (declarative_security);
				return;
			}

			if (a.Type == pa.StructLayout) {
				PartialContainer.HasStructLayout = true;

				if (a.GetLayoutKindValue () == LayoutKind.Explicit)
					PartialContainer.HasExplicitLayout = true;
			}

			base.ApplyAttributeBuilder (a, cb, pa);
		}

		/// <summary>
		/// Defines the default constructors 
		/// </summary>
		protected void DefineDefaultConstructor (bool is_static)
		{
			// The default instance constructor is public
			// If the class is abstract, the default constructor is protected
			// The default static constructor is private

			int mods;
			if (is_static) {
				mods = Modifiers.STATIC | Modifiers.PRIVATE;
			} else {
				mods = ((ModFlags & Modifiers.ABSTRACT) != 0) ? Modifiers.PROTECTED : Modifiers.PUBLIC;
			}

			Constructor c = new Constructor (this, MemberName.Name, mods,
				null, ParametersCompiled.EmptyReadOnlyParameters,
				new GeneratedBaseInitializer (Location),
				Location);
			
			AddConstructor (c);
			c.Block = new ToplevelBlock (Compiler, ParametersCompiled.EmptyReadOnlyParameters, Location);
		}

		public override bool Define ()
		{
			CheckProtectedModifier ();

			base.Define ();

			if (default_static_constructor != null)
				default_static_constructor.Define ();

			return true;
		}

		public override void Emit ()
		{
			if (default_static_constructor == null && PartialContainer.HasStaticFieldInitializer) {
				DefineDefaultConstructor (true);
				default_static_constructor.Define ();
			}

			base.Emit ();

			if (declarative_security != null) {
				foreach (DictionaryEntry de in declarative_security) {
					TypeBuilder.AddDeclarativeSecurity ((SecurityAction)de.Key, (PermissionSet)de.Value);
				}
			}
		}

		public override ExtensionMethodGroupExpr LookupExtensionMethod (Type extensionType, string name, Location loc)
		{
			DeclSpace top_level = Parent;
			if (top_level != null) {
				while (top_level.Parent != null)
					top_level = top_level.Parent;

				ArrayList candidates = NamespaceEntry.NS.LookupExtensionMethod (extensionType, this, name);
				if (candidates != null)
					return new ExtensionMethodGroupExpr (candidates, NamespaceEntry, extensionType, loc);
			}

			return NamespaceEntry.LookupExtensionMethod (extensionType, name, loc);
		}

		protected override TypeAttributes TypeAttr {
			get {
				if (default_static_constructor == null)
					return base.TypeAttr | TypeAttributes.BeforeFieldInit;

				return base.TypeAttr;
			}
		}
	}


	// TODO: should be sealed
	public class Class : ClassOrStruct {
		const int AllowedModifiers =
			Modifiers.NEW |
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.PRIVATE |
			Modifiers.ABSTRACT |
			Modifiers.SEALED |
			Modifiers.STATIC |
			Modifiers.UNSAFE;

		public const TypeAttributes StaticClassAttribute = TypeAttributes.Abstract | TypeAttributes.Sealed;

		public Class (NamespaceEntry ns, DeclSpace parent, MemberName name, int mod,
			      Attributes attrs)
			: base (ns, parent, name, attrs, Kind.Class)
		{
			int accmods = Parent.Parent == null ? Modifiers.INTERNAL : Modifiers.PRIVATE;
			this.ModFlags = Modifiers.Check (AllowedModifiers, mod, accmods, Location, Report);

			if (IsStatic && RootContext.Version == LanguageVersion.ISO_1) {
				Report.FeatureIsNotAvailable (Location, "static classes");
			}
		}

		public override void AddBasesForPart (DeclSpace part, ArrayList bases)
		{
			if (part.Name == "System.Object")
				Report.Error (537, part.Location,
					"The class System.Object cannot have a base class or implement an interface.");
			base.AddBasesForPart (part, bases);
		}

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb, PredefinedAttributes pa)
		{
			if (a.Type == pa.AttributeUsage) {
				if (!TypeManager.IsAttributeType (BaseType) &&
					TypeBuilder.FullName != "System.Attribute") {
					Report.Error (641, a.Location, "Attribute `{0}' is only valid on classes derived from System.Attribute", a.GetSignatureForError ());
				}
			}

			if (a.Type == pa.Conditional && !TypeManager.IsAttributeType (BaseType)) {
				Report.Error (1689, a.Location, "Attribute `System.Diagnostics.ConditionalAttribute' is only valid on methods or attribute classes");
				return;
			}

			if (a.Type == pa.ComImport && !attributes.Contains (pa.Guid)) {
				a.Error_MissingGuidAttribute ();
				return;
			}

			if (a.Type == pa.Extension) {
				a.Error_MisusedExtensionAttribute ();
				return;
			}

			if (AttributeTester.IsAttributeExcluded (a.Type, Location))
				return;

			base.ApplyAttributeBuilder (a, cb, pa);
		}

		public override AttributeTargets AttributeTargets {
			get {
				return AttributeTargets.Class;
			}
		}

		protected override void DefineContainerMembers (MemberCoreArrayList list)
		{
			if (list == null)
				return;

			if (!IsStatic) {
				base.DefineContainerMembers (list);
				return;
			}

			foreach (MemberCore m in list) {
				if (m is Operator) {
					Report.Error (715, m.Location, "`{0}': Static classes cannot contain user-defined operators", m.GetSignatureForError ());
					continue;
				}

				if (m is Destructor) {
					Report.Error (711, m.Location, "`{0}': Static classes cannot contain destructor", GetSignatureForError ());
					continue;
				}

				if (m is Indexer) {
					Report.Error (720, m.Location, "`{0}': cannot declare indexers in a static class", m.GetSignatureForError ());
					continue;
				}

				if ((m.ModFlags & Modifiers.STATIC) != 0 || m is Enum || m is Delegate)
					continue;

				if (m is Constructor) {
					Report.Error (710, m.Location, "`{0}': Static classes cannot have instance constructors", GetSignatureForError ());
					continue;
				}

				Method method = m as Method;
				if (method != null && method.Parameters.HasExtensionMethodType) {
					Report.Error (1105, m.Location, "`{0}': Extension methods must be declared static", m.GetSignatureForError ());
					continue;
				}

				Report.Error (708, m.Location, "`{0}': cannot declare instance members in a static class", m.GetSignatureForError ());
			}

			base.DefineContainerMembers (list);
		}

		public override bool Define ()
		{
			if ((ModFlags & Modifiers.ABSTRACT) == Modifiers.ABSTRACT && (ModFlags & (Modifiers.SEALED | Modifiers.STATIC)) != 0) {
				Report.Error (418, Location, "`{0}': an abstract class cannot be sealed or static", GetSignatureForError ());
			}

			if ((ModFlags & (Modifiers.SEALED | Modifiers.STATIC)) == (Modifiers.SEALED | Modifiers.STATIC)) {
				Report.Error (441, Location, "`{0}': a class cannot be both static and sealed", GetSignatureForError ());
			}

			return base.Define ();
		}

		protected override bool DoDefineMembers ()
		{
			if (InstanceConstructors == null && !IsStatic)
				DefineDefaultConstructor (false);

			return base.DoDefineMembers ();
		}

		public override void Emit ()
		{
			base.Emit ();

			if ((ModFlags & Modifiers.METHOD_EXTENSION) != 0)
				PredefinedAttributes.Get.Extension.EmitAttribute (TypeBuilder);
		}

		protected override TypeExpr[] ResolveBaseTypes (out TypeExpr base_class)
		{
			TypeExpr[] ifaces = base.ResolveBaseTypes (out base_class);

			if (base_class == null) {
				if (RootContext.StdLib)
					base_class = TypeManager.system_object_expr;
				else if (Name != "System.Object")
					base_class = TypeManager.system_object_expr;
			} else {
				if (Kind == Kind.Class && TypeManager.IsGenericParameter (base_class.Type)){
					Report.Error (
						689, base_class.Location,
						"Cannot derive from `{0}' because it is a type parameter",
						base_class.GetSignatureForError ());
					return ifaces;
				}

				if (IsGeneric && TypeManager.IsAttributeType (base_class.Type)) {
					Report.Error (698, base_class.Location,
						"A generic type cannot derive from `{0}' because it is an attribute class",
						base_class.GetSignatureForError ());
				}

				if (base_class.IsSealed){
					Report.SymbolRelatedToPreviousError (base_class.Type);
					if (base_class.Type.IsAbstract) {
						Report.Error (709, Location, "`{0}': Cannot derive from static class `{1}'",
							GetSignatureForError (), TypeManager.CSharpName (base_class.Type));
					} else {
						Report.Error (509, Location, "`{0}': cannot derive from sealed type `{1}'",
							GetSignatureForError (), TypeManager.CSharpName (base_class.Type));
					}
					return ifaces;
				}

				if (!base_class.CanInheritFrom ()){
					Report.Error (644, Location, "`{0}' cannot derive from special class `{1}'",
						GetSignatureForError (), base_class.GetSignatureForError ());
					return ifaces;
				}

				if (!IsAccessibleAs (base_class.Type)) {
					Report.SymbolRelatedToPreviousError (base_class.Type);
					Report.Error (60, Location, "Inconsistent accessibility: base class `{0}' is less accessible than class `{1}'", 
						TypeManager.CSharpName (base_class.Type), GetSignatureForError ());
				}
			}

			if (PartialContainer.IsStaticClass) {
				if (base_class.Type != TypeManager.object_type) {
					Report.Error (713, Location, "Static class `{0}' cannot derive from type `{1}'. Static classes must derive from object",
						GetSignatureForError (), base_class.GetSignatureForError ());
					return ifaces;
				}

				if (ifaces != null) {
					foreach (TypeExpr t in ifaces)
						Report.SymbolRelatedToPreviousError (t.Type);
					Report.Error (714, Location, "Static class `{0}' cannot implement interfaces", GetSignatureForError ());
				}
			}

			return ifaces;
		}

		/// Search for at least one defined condition in ConditionalAttribute of attribute class
		/// Valid only for attribute classes.
		public bool IsExcluded ()
		{
			if ((caching_flags & Flags.Excluded_Undetected) == 0)
				return (caching_flags & Flags.Excluded) != 0;

			caching_flags &= ~Flags.Excluded_Undetected;

			if (OptAttributes == null)
				return false;

			Attribute[] attrs = OptAttributes.SearchMulti (PredefinedAttributes.Get.Conditional);
			if (attrs == null)
				return false;

			foreach (Attribute a in attrs) {
				string condition = a.GetConditionalAttributeValue ();
				if (Location.CompilationUnit.IsConditionalDefined (condition))
					return false;
			}

			caching_flags |= Flags.Excluded;
			return true;
		}

		//
		// FIXME: How do we deal with the user specifying a different
		// layout?
		//
		protected override TypeAttributes TypeAttr {
			get {
				TypeAttributes ta = base.TypeAttr | TypeAttributes.AutoLayout | TypeAttributes.Class;
				if (IsStatic)
					ta |= StaticClassAttribute;
				return ta;
			}
		}
	}

	public sealed class Struct : ClassOrStruct {

		bool is_unmanaged, has_unmanaged_check_done;

		// <summary>
		//   Modifiers allowed in a struct declaration
		// </summary>
		const int AllowedModifiers =
			Modifiers.NEW       |
			Modifiers.PUBLIC    |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL  |
			Modifiers.UNSAFE    |
			Modifiers.PRIVATE;

		public Struct (NamespaceEntry ns, DeclSpace parent, MemberName name,
			       int mod, Attributes attrs)
			: base (ns, parent, name, attrs, Kind.Struct)
		{
			int accmods;
			
			if (parent.Parent == null)
				accmods = Modifiers.INTERNAL;
			else
				accmods = Modifiers.PRIVATE;
			
			this.ModFlags = Modifiers.Check (AllowedModifiers, mod, accmods, Location, Report);

			this.ModFlags |= Modifiers.SEALED;
		}

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb, PredefinedAttributes pa)
		{
			base.ApplyAttributeBuilder (a, cb, pa);

			//
			// When struct constains fixed fixed and struct layout has explicitly
			// set CharSet, its value has to be propagated to compiler generated
			// fixed field types
			//
			if (a.Type == pa.StructLayout && Fields != null && a.HasField ("CharSet")) {
				for (int i = 0; i < Fields.Count; ++i) {
					FixedField ff = Fields [i] as FixedField;
					if (ff != null)
						ff.SetCharSet (TypeBuilder.Attributes);
				}
			}
		}

		public override AttributeTargets AttributeTargets {
			get {
				return AttributeTargets.Struct;
			}
		}

		public override bool IsUnmanagedType ()
		{
			if (fields == null)
				return true;

			if (requires_delayed_unmanagedtype_check)
				return true;

			if (has_unmanaged_check_done)
				return is_unmanaged;

			has_unmanaged_check_done = true;

			foreach (FieldBase f in fields) {
				if ((f.ModFlags & Modifiers.STATIC) != 0)
					continue;

				// It can happen when recursive unmanaged types are defined
				// struct S { S* s; }
				Type mt = f.MemberType;
				if (mt == null) {
					has_unmanaged_check_done = false;
					requires_delayed_unmanagedtype_check = true;
					return true;
				}

				// TODO: Remove when pointer types are under mcs control
				while (mt.IsPointer)
					mt = TypeManager.GetElementType (mt);
				if (TypeManager.IsEqual (mt, TypeBuilder))
					continue;

				if (TypeManager.IsUnmanagedType (mt))
					continue;

				return false;
			}

			is_unmanaged = true;
			return true;
		}

		protected override TypeExpr[] ResolveBaseTypes (out TypeExpr base_class)
		{
			TypeExpr[] ifaces = base.ResolveBaseTypes (out base_class);
			//
			// If we are compiling our runtime,
			// and we are defining ValueType, then our
			// base is `System.Object'.
			//
			if (base_class == null) {
				if (!RootContext.StdLib && Name == "System.ValueType")
					base_class = TypeManager.system_object_expr;
				else
					base_class = TypeManager.system_valuetype_expr;
			}

			return ifaces;
		}

		//
		// FIXME: Allow the user to specify a different set of attributes
		// in some cases (Sealed for example is mandatory for a class,
		// but what SequentialLayout can be changed
		//
		protected override TypeAttributes TypeAttr {
			get {
				const TypeAttributes DefaultTypeAttributes =
					TypeAttributes.SequentialLayout |
					TypeAttributes.Sealed;

				return base.TypeAttr | DefaultTypeAttributes;
			}
		}

		public override void RegisterFieldForInitialization (MemberCore field, FieldInitializer expression)
		{
			if ((field.ModFlags & Modifiers.STATIC) == 0) {
				Report.Error (573, field.Location, "`{0}': Structs cannot have instance field initializers",
					field.GetSignatureForError ());
				return;
			}
			base.RegisterFieldForInitialization (field, expression);
		}

	}

	/// <summary>
	///   Interfaces
	/// </summary>
	public sealed class Interface : TypeContainer, IMemberContainer {

		/// <summary>
		///   Modifiers allowed in a class declaration
		/// </summary>
		public const int AllowedModifiers =
			Modifiers.NEW       |
			Modifiers.PUBLIC    |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL  |
		 	Modifiers.UNSAFE    |
			Modifiers.PRIVATE;

		public Interface (NamespaceEntry ns, DeclSpace parent, MemberName name, int mod,
				  Attributes attrs)
			: base (ns, parent, name, attrs, Kind.Interface)
		{
			int accmods;

			if (parent.Parent == null)
				accmods = Modifiers.INTERNAL;
			else
				accmods = Modifiers.PRIVATE;

			this.ModFlags = Modifiers.Check (AllowedModifiers, mod, accmods, name.Location, Report);
		}

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb, PredefinedAttributes pa)
		{
			if (a.Type == pa.ComImport && !attributes.Contains (pa.Guid)) {
				a.Error_MissingGuidAttribute ();
				return;
			}

			base.ApplyAttributeBuilder (a, cb, pa);
		}


		public override AttributeTargets AttributeTargets {
			get {
				return AttributeTargets.Interface;
			}
		}

		protected override TypeAttributes TypeAttr {
			get {
				const TypeAttributes DefaultTypeAttributes =
					TypeAttributes.AutoLayout |
					TypeAttributes.Abstract |
					TypeAttributes.Interface;

				return base.TypeAttr | DefaultTypeAttributes;
			}
		}

		protected override bool VerifyClsCompliance ()
		{
			if (!base.VerifyClsCompliance ())
				return false;

			if (ifaces != null) {
				foreach (Type t in ifaces) {
					if (AttributeTester.IsClsCompliant (t))
						continue;

					Report.SymbolRelatedToPreviousError (t);
					Report.Warning (3027, 1, Location, "`{0}' is not CLS-compliant because base interface `{1}' is not CLS-compliant",
						GetSignatureForError (), TypeManager.CSharpName (t));
				}
			}

			return true;
		}
	}

	// It is used as a base class for all property based members
	// This includes properties, indexers, and events
	public abstract class PropertyBasedMember : InterfaceMemberBase
	{
		public PropertyBasedMember (DeclSpace parent, GenericMethod generic,
			FullNamedExpression type, int mod, int allowed_mod,
			MemberName name, Attributes attrs)
			: base (parent, generic, type, mod, allowed_mod, name, attrs)
		{
		}

		protected override bool VerifyClsCompliance ()
		{
			if (!base.VerifyClsCompliance ())
				return false;

			if (!AttributeTester.IsClsCompliant (MemberType)) {
				Report.Warning (3003, 1, Location, "Type of `{0}' is not CLS-compliant",
					GetSignatureForError ());
			}
			return true;
		}

	}


	public abstract class MethodCore : InterfaceMemberBase
	{
		public readonly ParametersCompiled Parameters;
		protected ToplevelBlock block;

		public MethodCore (DeclSpace parent, GenericMethod generic,
			FullNamedExpression type, int mod, int allowed_mod,
			MemberName name, Attributes attrs, ParametersCompiled parameters)
			: base (parent, generic, type, mod, allowed_mod, name, attrs)
		{
			Parameters = parameters;
		}

		//
		//  Returns the System.Type array for the parameters of this method
		//
		public Type [] ParameterTypes {
			get {
				return Parameters.Types;
			}
		}

		public ParametersCompiled ParameterInfo {
			get {
				return Parameters;
			}
		}
		
		public ToplevelBlock Block {
			get {
				return block;
			}

			set {
				block = value;
			}
		}

		public CallingConventions CallingConventions {
			get {
				CallingConventions cc = Parameters.CallingConvention;
				if (!IsInterface)
					if ((ModFlags & Modifiers.STATIC) == 0)
						cc |= CallingConventions.HasThis;

				// FIXME: How is `ExplicitThis' used in C#?
			
				return cc;
			}
		}

		protected override bool CheckBase ()
		{
			// Check whether arguments were correct.
			if (!DefineParameters (Parameters))
				return false;

			return base.CheckBase ();
		}

		//
		// Returns a string that represents the signature for this 
		// member which should be used in XML documentation.
		//
		public override string GetDocCommentName (DeclSpace ds)
		{
			return DocUtil.GetMethodDocCommentName (this, Parameters, ds);
		}

		//
		// Raised (and passed an XmlElement that contains the comment)
		// when GenerateDocComment is writing documentation expectedly.
		//
		// FIXME: with a few effort, it could be done with XmlReader,
		// that means removal of DOM use.
		//
		internal override void OnGenerateDocComment (XmlElement el)
		{
			DocUtil.OnMethodGenerateDocComment (this, el, Report);
		}

		//
		//   Represents header string for documentation comment.
		//
		public override string DocCommentHeader 
		{
			get { return "M:"; }
		}

		public override bool EnableOverloadChecks (MemberCore overload)
		{
			if (overload is MethodCore || overload is AbstractPropertyEventMethod) {
				caching_flags |= Flags.MethodOverloadsExist;
				return true;
			}

			return base.EnableOverloadChecks (overload);
		}

		protected override bool VerifyClsCompliance ()
		{
			if (!base.VerifyClsCompliance ())
				return false;

			if (Parameters.HasArglist) {
				Report.Warning (3000, 1, Location, "Methods with variable arguments are not CLS-compliant");
			}

			if (!AttributeTester.IsClsCompliant (MemberType)) {
				Report.Warning (3002, 1, Location, "Return type of `{0}' is not CLS-compliant",
					GetSignatureForError ());
			}

			Parameters.VerifyClsCompliance ();
			return true;
		}

	}

	public abstract class InterfaceMemberBase : MemberBase {
		//
		// Whether this is an interface member.
		//
		public bool IsInterface;

		//
		// If true, this is an explicit interface implementation
		//
		public bool IsExplicitImpl;

		protected bool is_external_implementation;

		//
		// The interface type we are explicitly implementing
		//
		public Type InterfaceType;

		//
		// The method we're overriding if this is an override method.
		//
		protected MethodInfo base_method;

		readonly int explicit_mod_flags;
		public MethodAttributes flags;

		public InterfaceMemberBase (DeclSpace parent, GenericMethod generic,
				   FullNamedExpression type, int mod, int allowed_mod,
				   MemberName name, Attributes attrs)
			: base (parent, generic, type, mod, allowed_mod, Modifiers.PRIVATE,
				name, attrs)
		{
			IsInterface = parent.PartialContainer.Kind == Kind.Interface;
			IsExplicitImpl = (MemberName.Left != null);
			explicit_mod_flags = mod;
		}
		
		protected override bool CheckBase ()
		{
			if (!base.CheckBase ())
				return false;

			if ((caching_flags & Flags.MethodOverloadsExist) != 0)
				CheckForDuplications ();
			
			if (IsExplicitImpl)
				return true;

			// Is null for System.Object while compiling corlib and base interfaces
			if (Parent.PartialContainer.BaseCache == null) {
				if ((ModFlags & Modifiers.NEW) != 0) {
					Report.Warning (109, 4, Location, "The member `{0}' does not hide an inherited member. The new keyword is not required", GetSignatureForError ());
				}
				return true;
			}

			Type base_ret_type = null;
			base_method = FindOutBaseMethod (ref base_ret_type);

			// method is override
			if (base_method != null) {
				if (!CheckMethodAgainstBase (base_ret_type))
					return false;

				if ((ModFlags & Modifiers.OVERRIDE) != 0) {
					ObsoleteAttribute oa = AttributeTester.GetMethodObsoleteAttribute (base_method);
					if (oa != null) {
						if (OptAttributes == null || !OptAttributes.Contains (PredefinedAttributes.Get.Obsolete)) {
							Report.SymbolRelatedToPreviousError (base_method);
								Report.Warning (672, 1, Location, "Member `{0}' overrides obsolete member `{1}'. Add the Obsolete attribute to `{0}'",
									GetSignatureForError (), TypeManager.CSharpSignature (base_method));
						}
					} else {
						if (OptAttributes != null && OptAttributes.Contains (PredefinedAttributes.Get.Obsolete)) {
							Report.SymbolRelatedToPreviousError (base_method);
							Report.Warning (809, 1, Location, "Obsolete member `{0}' overrides non-obsolete member `{1}'",
								GetSignatureForError (), TypeManager.CSharpSignature (base_method));
						}
					}
				}
				return true;
			}

			MemberInfo conflict_symbol = Parent.PartialContainer.FindBaseMemberWithSameName (Name, !((this is Event) || (this is Property)));
			if ((ModFlags & Modifiers.OVERRIDE) != 0) {
				if (conflict_symbol != null) {
					Report.SymbolRelatedToPreviousError (conflict_symbol);
					if (this is Event)
						Report.Error (72, Location, "`{0}': cannot override because `{1}' is not an event", GetSignatureForError (), TypeManager.GetFullNameSignature (conflict_symbol));
					else if (this is PropertyBase)
						Report.Error (544, Location, "`{0}': cannot override because `{1}' is not a property", GetSignatureForError (), TypeManager.GetFullNameSignature (conflict_symbol));
					else
						Report.Error (505, Location, "`{0}': cannot override because `{1}' is not a method", GetSignatureForError (), TypeManager.GetFullNameSignature (conflict_symbol));
				} else {
					Report.Error (115, Location, "`{0}' is marked as an override but no suitable {1} found to override",
						GetSignatureForError (), SimpleName.GetMemberType (this));
				}
				return false;
			}

			if (conflict_symbol == null) {
				if ((ModFlags & Modifiers.NEW) != 0) {
					Report.Warning (109, 4, Location, "The member `{0}' does not hide an inherited member. The new keyword is not required", GetSignatureForError ());
				}
				return true;
			}

			if ((ModFlags & Modifiers.NEW) == 0) {
				if (this is MethodOrOperator && conflict_symbol.MemberType == MemberTypes.Method)
					return true;

				Report.SymbolRelatedToPreviousError (conflict_symbol);
				Report.Warning (108, 2, Location, "`{0}' hides inherited member `{1}'. Use the new keyword if hiding was intended",
					GetSignatureForError (), TypeManager.GetFullNameSignature (conflict_symbol));
			}

			return true;
		}

		protected virtual bool CheckForDuplications ()
		{
			return Parent.MemberCache.CheckExistingMembersOverloads (
				this, GetFullName (MemberName), ParametersCompiled.EmptyReadOnlyParameters, Report);
		}

		//
		// Performs various checks on the MethodInfo `mb' regarding the modifier flags
		// that have been defined.
		//
		// `name' is the user visible name for reporting errors (this is used to
		// provide the right name regarding method names and properties)
		//
		bool CheckMethodAgainstBase (Type base_method_type)
		{
			bool ok = true;

			if ((ModFlags & Modifiers.OVERRIDE) != 0){
				if (!(base_method.IsAbstract || base_method.IsVirtual)){
					Report.SymbolRelatedToPreviousError (base_method);
					Report.Error (506, Location,
						"`{0}': cannot override inherited member `{1}' because it is not marked virtual, abstract or override",
						 GetSignatureForError (), TypeManager.CSharpSignature (base_method));
					ok = false;
				}
				
				// Now we check that the overriden method is not final
				
				if (base_method.IsFinal) {
					Report.SymbolRelatedToPreviousError (base_method);
					Report.Error (239, Location, "`{0}': cannot override inherited member `{1}' because it is sealed",
							      GetSignatureForError (), TypeManager.CSharpSignature (base_method));
					ok = false;
				}
				//
				// Check that the permissions are not being changed
				//
				MethodAttributes thisp = flags & MethodAttributes.MemberAccessMask;
				MethodAttributes base_classp = base_method.Attributes & MethodAttributes.MemberAccessMask;

				if (!CheckAccessModifiers (thisp, base_classp, base_method)) {
					Error_CannotChangeAccessModifiers (Location, base_method, base_classp, null);
					ok = false;
				}

				if (!TypeManager.IsEqual (MemberType, TypeManager.TypeToCoreType (base_method_type))) {
					Report.SymbolRelatedToPreviousError (base_method);
					if (this is PropertyBasedMember) {
						Report.Error (1715, Location, "`{0}': type must be `{1}' to match overridden member `{2}'", 
							GetSignatureForError (), TypeManager.CSharpName (base_method_type), TypeManager.CSharpSignature (base_method));
					}
					else {
						Report.Error (508, Location, "`{0}': return type must be `{1}' to match overridden member `{2}'",
							GetSignatureForError (), TypeManager.CSharpName (base_method_type), TypeManager.CSharpSignature (base_method));
					}
					ok = false;
				}
			}

			if ((ModFlags & Modifiers.NEW) == 0) {
				if ((ModFlags & Modifiers.OVERRIDE) == 0) {
					ModFlags |= Modifiers.NEW;
					Report.SymbolRelatedToPreviousError (base_method);
					if (!IsInterface && (base_method.IsVirtual || base_method.IsAbstract)) {
						Report.Warning (114, 2, Location, "`{0}' hides inherited member `{1}'. To make the current member override that implementation, add the override keyword. Otherwise add the new keyword",
							GetSignatureForError (), TypeManager.CSharpSignature (base_method));
						if (base_method.IsAbstract){
							Report.Error (533, Location, "`{0}' hides inherited abstract member `{1}'",
								      GetSignatureForError (), TypeManager.CSharpSignature (base_method));
							ok = false;
						}
					} else {
						Report.Warning (108, 2, Location, "`{0}' hides inherited member `{1}'. Use the new keyword if hiding was intended",
							GetSignatureForError (), TypeManager.CSharpSignature (base_method));
					}
				}
			} else {
				if (base_method.IsAbstract && !IsInterface) {
					Report.SymbolRelatedToPreviousError (base_method);
					Report.Error (533, Location, "`{0}' hides inherited abstract member `{1}'",
						GetSignatureForError (), TypeManager.CSharpSignature (base_method));
					return ok = false;
				}
			}

			return ok;
		}
		
		protected bool CheckAccessModifiers (MethodAttributes thisp, MethodAttributes base_classp, MethodInfo base_method)
		{
			if ((base_classp & MethodAttributes.FamORAssem) == MethodAttributes.FamORAssem){
				//
				// when overriding protected internal, the method can be declared
				// protected internal only within the same assembly or assembly
				// which has InternalsVisibleTo
				//
				if ((thisp & MethodAttributes.FamORAssem) == MethodAttributes.FamORAssem){
					return TypeManager.IsThisOrFriendAssembly (base_method.DeclaringType.Assembly);
				} else if ((thisp & MethodAttributes.Family) != MethodAttributes.Family) {
					//
					// if it's not "protected internal", it must be "protected"
					//

					return false;
				} else if (Parent.TypeBuilder.Assembly == base_method.DeclaringType.Assembly) {
					//
					// protected within the same assembly - an error
					//
					return false;
				} else if ((thisp & ~(MethodAttributes.Family | MethodAttributes.FamORAssem)) != 
					   (base_classp & ~(MethodAttributes.Family | MethodAttributes.FamORAssem))) {
					//
					// protected ok, but other attributes differ - report an error
					//
					return false;
				}
				return true;
			} else {
				return (thisp == base_classp);
			}
		}

		public override bool Define ()
		{
			if (IsInterface) {
				ModFlags = Modifiers.PUBLIC | Modifiers.ABSTRACT |
					Modifiers.VIRTUAL | (ModFlags & (Modifiers.UNSAFE | Modifiers.NEW));

				flags = MethodAttributes.Public |
					MethodAttributes.Abstract |
					MethodAttributes.HideBySig |
					MethodAttributes.NewSlot |
					MethodAttributes.Virtual;
			} else {
				Parent.PartialContainer.MethodModifiersValid (this);

				flags = Modifiers.MethodAttr (ModFlags);
			}

			if (IsExplicitImpl) {
				TypeExpr iface_texpr = MemberName.Left.GetTypeExpression ().ResolveAsTypeTerminal (this, false);
				if (iface_texpr == null)
					return false;

				if ((ModFlags & Modifiers.PARTIAL) != 0) {
					Report.Error (754, Location, "A partial method `{0}' cannot explicitly implement an interface",
						GetSignatureForError ());
				}

				InterfaceType = iface_texpr.Type;

				if (!InterfaceType.IsInterface) {
					Report.SymbolRelatedToPreviousError (InterfaceType);
					Report.Error (538, Location, "The type `{0}' in explicit interface declaration is not an interface",
						TypeManager.CSharpName (InterfaceType));
				} else {
					Parent.PartialContainer.VerifyImplements (this);
				}

				Modifiers.Check (Modifiers.AllowedExplicitImplFlags, explicit_mod_flags, 0, Location, Report);
			}

			return base.Define ();
		}

		protected bool DefineParameters (ParametersCompiled parameters)
		{
			if (!parameters.Resolve (this))
				return false;

			bool error = false;
			for (int i = 0; i < parameters.Count; ++i) {
				Parameter p = parameters [i];

				if (p.HasDefaultValue && (IsExplicitImpl || this is Operator || (this is Indexer && parameters.Count == 1)))
					p.Warning_UselessOptionalParameter (Report);

				if (p.CheckAccessibility (this))
					continue;

				Type t = parameters.Types [i];
				Report.SymbolRelatedToPreviousError (t);
				if (this is Indexer)
					Report.Error (55, Location,
						      "Inconsistent accessibility: parameter type `{0}' is less accessible than indexer `{1}'",
						      TypeManager.CSharpName (t), GetSignatureForError ());
				else if (this is Operator)
					Report.Error (57, Location,
						      "Inconsistent accessibility: parameter type `{0}' is less accessible than operator `{1}'",
						      TypeManager.CSharpName (t), GetSignatureForError ());
				else
					Report.Error (51, Location,
						"Inconsistent accessibility: parameter type `{0}' is less accessible than method `{1}'",
						TypeManager.CSharpName (t), GetSignatureForError ());
				error = true;
			}
			return !error;
		}

		public override void Emit()
		{
			// for extern static method must be specified either DllImport attribute or MethodImplAttribute.
			// We are more strict than csc and report this as an error because SRE does not allow emit that
			if ((ModFlags & Modifiers.EXTERN) != 0 && !is_external_implementation) {
				if (this is Constructor) {
					Report.Error (824, Location,
						"Constructor `{0}' is marked `external' but has no external implementation specified", GetSignatureForError ());
				} else {
					Report.Error (626, Location,
						"`{0}' is marked as an external but has no DllImport attribute. Consider adding a DllImport attribute to specify the external implementation",
						GetSignatureForError ());
				}
			}

			base.Emit ();
		}

		public override bool EnableOverloadChecks (MemberCore overload)
		{
			//
			// Two members can differ in their explicit interface
			// type parameter only
			//
			InterfaceMemberBase imb = overload as InterfaceMemberBase;
			if (imb != null && imb.IsExplicitImpl) {
				if (IsExplicitImpl) {
					caching_flags |= Flags.MethodOverloadsExist;
				}
				return true;
			}

			return IsExplicitImpl;
		}

		protected void Error_CannotChangeAccessModifiers (Location loc, MemberInfo base_method, MethodAttributes ma, string suffix)
		{
			Report.SymbolRelatedToPreviousError (base_method);
			string base_name = TypeManager.GetFullNameSignature (base_method);
			string this_name = GetSignatureForError ();
			if (suffix != null) {
				base_name += suffix;
				this_name += suffix;
			}

			Report.Error (507, loc, "`{0}': cannot change access modifiers when overriding `{1}' inherited member `{2}'",
				this_name, Modifiers.GetDescription (ma), base_name);
		}

		protected static string Error722 {
			get {
				return "`{0}': static types cannot be used as return types";
			}
		}

		/// <summary>
		/// Gets base method and its return type
		/// </summary>
		protected abstract MethodInfo FindOutBaseMethod (ref Type base_ret_type);

		//
		// The "short" name of this property / indexer / event.  This is the
		// name without the explicit interface.
		//
		public string ShortName 
		{
			get { return MemberName.Name; }
			set { SetMemberName (new MemberName (MemberName.Left, value, Location)); }
		}
		
		//
		// Returns full metadata method name
		//
		public string GetFullName (MemberName name)
		{
			if (!IsExplicitImpl)
				return name.Name;

			//
			// When dealing with explicit members a full interface type
			// name is added to member name to avoid possible name conflicts
			//
			// We use CSharpName which gets us full name with benefit of
			// replacing predefined names which saves some space and name
			// is still unique
			//
			return TypeManager.CSharpName (InterfaceType) + "." + name.Name;
		}

		protected override bool VerifyClsCompliance ()
		{
			if (!base.VerifyClsCompliance ()) {
				if (IsInterface && HasClsCompliantAttribute && Parent.IsClsComplianceRequired ()) {
					Report.Warning (3010, 1, Location, "`{0}': CLS-compliant interfaces must have only CLS-compliant members", GetSignatureForError ());
				}

				if ((ModFlags & Modifiers.ABSTRACT) != 0 && Parent.TypeBuilder.IsClass && IsExposedFromAssembly () && Parent.IsClsComplianceRequired ()) {
					Report.Warning (3011, 1, Location, "`{0}': only CLS-compliant members can be abstract", GetSignatureForError ());
				}
				return false;
			}

			if (GenericMethod != null)
				GenericMethod.VerifyClsCompliance ();

			return true;
		}

		public override bool IsUsed 
		{
			get { return IsExplicitImpl || base.IsUsed; }
		}

	}

	public abstract class MethodOrOperator : MethodCore, IMethodData
	{
		public MethodBuilder MethodBuilder;
		ReturnParameter return_attributes;
		ListDictionary declarative_security;
		protected MethodData MethodData;

		static string[] attribute_targets = new string [] { "method", "return" };

		protected MethodOrOperator (DeclSpace parent, GenericMethod generic, FullNamedExpression type, int mod,
				int allowed_mod, MemberName name,
				Attributes attrs, ParametersCompiled parameters)
			: base (parent, generic, type, mod, allowed_mod, name,
					attrs, parameters)
		{
		}

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb, PredefinedAttributes pa)
		{
			if (a.Target == AttributeTargets.ReturnValue) {
				if (return_attributes == null)
					return_attributes = new ReturnParameter (MethodBuilder, Location);

				return_attributes.ApplyAttributeBuilder (a, cb, pa);
				return;
			}

			if (a.IsInternalMethodImplAttribute) {
				is_external_implementation = true;
			}

			if (a.Type == pa.DllImport) {
				const int extern_static = Modifiers.EXTERN | Modifiers.STATIC;
				if ((ModFlags & extern_static) != extern_static) {
					Report.Error (601, a.Location, "The DllImport attribute must be specified on a method marked `static' and `extern'");
				}
				is_external_implementation = true;
			}

			if (a.IsValidSecurityAttribute ()) {
				if (declarative_security == null)
					declarative_security = new ListDictionary ();
				a.ExtractSecurityPermissionSet (declarative_security);
				return;
			}

			if (MethodBuilder != null)
				MethodBuilder.SetCustomAttribute (cb);
		}

		public override AttributeTargets AttributeTargets {
			get {
				return AttributeTargets.Method; 
			}
		}

		protected override bool CheckForDuplications ()
		{
			string name = GetFullName (MemberName);
			if (MemberName.IsGeneric)
				name = MemberName.MakeName (name, MemberName.TypeArguments);

			return Parent.MemberCache.CheckExistingMembersOverloads (this, name, Parameters, Report);
		}

		public virtual EmitContext CreateEmitContext (ILGenerator ig)
		{
			return new EmitContext (
				this, ig, MemberType);
		}

		protected override bool ResolveMemberType ()
		{
#if GMCS_SOURCE
			if (GenericMethod != null) {
				MethodBuilder = Parent.TypeBuilder.DefineMethod (GetFullName (MemberName), flags);
				if (!GenericMethod.Define (this))
					return false;
			}
#endif

			return base.ResolveMemberType ();
		}

		public override bool Define ()
		{
			if (!base.Define ())
				return false;

			if (!CheckBase ())
				return false;

			if (block != null && block.IsIterator && !(Parent is IteratorStorey)) {
				//
				// Current method is turned into automatically generated
				// wrapper which creates an instance of iterator
				//
				Iterator.CreateIterator (this, Parent.PartialContainer, ModFlags, Compiler);
				ModFlags |= Modifiers.DEBUGGER_HIDDEN;
			}

			if (IsPartialDefinition) {
				caching_flags &= ~Flags.Excluded_Undetected;
				caching_flags |= Flags.Excluded;
				// Add to member cache only when a partial method implementation is not there
				if ((caching_flags & Flags.MethodOverloadsExist) == 0) {
					MethodBase mb = new PartialMethodDefinitionInfo (this);
					Parent.MemberCache.AddMember (mb, this);
					TypeManager.AddMethod (mb, this);
				}

				return true;
			}

			MethodData = new MethodData (
				this, ModFlags, flags, this, MethodBuilder, GenericMethod, base_method);

			if (!MethodData.Define (Parent.PartialContainer, GetFullName (MemberName), Report))
				return false;
					
			MethodBuilder = MethodData.MethodBuilder;

			if (TypeManager.IsGenericMethod (MethodBuilder))
				Parent.MemberCache.AddGenericMember (MethodBuilder, this);
			
			Parent.MemberCache.AddMember (MethodBuilder, this);

			return true;
		}

		protected override void DoMemberTypeIndependentChecks ()
		{
			base.DoMemberTypeIndependentChecks ();

			CheckAbstractAndExtern (block != null);

			if ((ModFlags & Modifiers.PARTIAL) != 0) {
				for (int i = 0; i < Parameters.Count; ++i) {
					IParameterData p = Parameters.FixedParameters [i];
					if (p.ModFlags == Parameter.Modifier.OUT) {
						Report.Error (752, Location, "`{0}': A partial method parameters cannot use `out' modifier",
							GetSignatureForError ());
					}

					if (p.HasDefaultValue && IsPartialImplementation)
						((Parameter) p).Warning_UselessOptionalParameter (Report);
				}
			}
		}

		protected override void DoMemberTypeDependentChecks ()
		{
			base.DoMemberTypeDependentChecks ();

			if (!TypeManager.IsGenericParameter (MemberType)) {
				if (MemberType.IsAbstract && MemberType.IsSealed) {
					Report.Error (722, Location, Error722, TypeManager.CSharpName (MemberType));
				}
			}
		}

		public override void Emit ()
		{
			if ((ModFlags & Modifiers.COMPILER_GENERATED) != 0 && !Parent.IsCompilerGenerated)
				PredefinedAttributes.Get.CompilerGenerated.EmitAttribute (MethodBuilder);
			if ((ModFlags & Modifiers.DEBUGGER_HIDDEN) != 0)
				PredefinedAttributes.Get.DebuggerHidden.EmitAttribute (MethodBuilder);

			if (TypeManager.IsDynamicType (ReturnType)) {
				return_attributes = new ReturnParameter (MethodBuilder, Location);
				return_attributes.EmitPredefined (PredefinedAttributes.Get.Dynamic, Location);
			}

			if (OptAttributes != null)
				OptAttributes.Emit ();

			if (declarative_security != null) {
				foreach (DictionaryEntry de in declarative_security) {
					MethodBuilder.AddDeclarativeSecurity ((SecurityAction)de.Key, (PermissionSet)de.Value);
				}
			}

			if (MethodData != null)
				MethodData.Emit (Parent);

			base.Emit ();

			Block = null;
			MethodData = null;
		}

		protected void Error_ConditionalAttributeIsNotValid ()
		{
			Report.Error (577, Location,
				"Conditional not valid on `{0}' because it is a constructor, destructor, operator or explicit interface implementation",
				GetSignatureForError ());
		}

		public bool IsPartialDefinition {
			get {
				return (ModFlags & Modifiers.PARTIAL) != 0 && Block == null;
			}
		}

		public bool IsPartialImplementation {
			get {
				return (ModFlags & Modifiers.PARTIAL) != 0 && Block != null;
			}
		}

		public override string[] ValidAttributeTargets {
			get {
				return attribute_targets;
			}
		}

		#region IMethodData Members

		public Type ReturnType {
			get {
				return MemberType;
			}
		}

		public MemberName MethodName {
			get {
				return MemberName;
			}
		}

		/// <summary>
		/// Returns true if method has conditional attribute and the conditions is not defined (method is excluded).
		/// </summary>
		public bool IsExcluded () {
			if ((caching_flags & Flags.Excluded_Undetected) == 0)
				return (caching_flags & Flags.Excluded) != 0;

			caching_flags &= ~Flags.Excluded_Undetected;

			if (base_method == null) {
				if (OptAttributes == null)
					return false;

				Attribute[] attrs = OptAttributes.SearchMulti (PredefinedAttributes.Get.Conditional);

				if (attrs == null)
					return false;

				foreach (Attribute a in attrs) {
					string condition = a.GetConditionalAttributeValue ();
					if (condition == null)
						return false;

					if (Location.CompilationUnit.IsConditionalDefined (condition))
						return false;
				}

				caching_flags |= Flags.Excluded;
				return true;
			}

			IMethodData md = TypeManager.GetMethod (TypeManager.DropGenericMethodArguments (base_method));
			if (md == null) {
				if (AttributeTester.IsConditionalMethodExcluded (base_method, Location)) {
					caching_flags |= Flags.Excluded;
					return true;
				}
				return false;
			}

			if (md.IsExcluded ()) {
				caching_flags |= Flags.Excluded;
				return true;
			}
			return false;
		}

		GenericMethod IMethodData.GenericMethod {
			get {
				return GenericMethod;
			}
		}

		public virtual void EmitExtraSymbolInfo (SourceMethod source)
		{ }

		#endregion

	}

	public class SourceMethod : IMethodDef
	{
		MethodBase method;
		SourceMethodBuilder builder;

		protected SourceMethod (DeclSpace parent, MethodBase method, ICompileUnit file)
		{
			this.method = method;
			
			builder = SymbolWriter.OpenMethod (file, parent.NamespaceEntry.SymbolFileID, this);
		}

		public string Name {
			get { return method.Name; }
		}

		public int Token {
			get {
				if (method is MethodBuilder)
					return ((MethodBuilder) method).GetToken ().Token;
				else if (method is ConstructorBuilder)
					return ((ConstructorBuilder) method).GetToken ().Token;
				else
					throw new NotSupportedException ();
			}
		}

		public void CloseMethod ()
		{
			SymbolWriter.CloseMethod ();
		}

		public void SetRealMethodName (string name)
		{
			if (builder != null)
				builder.SetRealMethodName (name);
		}

		public static SourceMethod Create (DeclSpace parent, MethodBase method, Block block)
		{
			if (!SymbolWriter.HasSymbolWriter)
				return null;
			if (block == null)
				return null;

			Location start_loc = block.StartLocation;
			if (start_loc.IsNull)
				return null;

			ICompileUnit compile_unit = start_loc.CompilationUnit;
			if (compile_unit == null)
				return null;

			return new SourceMethod (parent, method, compile_unit);
		}
	}

	public class Method : MethodOrOperator {

		/// <summary>
		///   Modifiers allowed in a class declaration
		/// </summary>
		const int AllowedModifiers =
			Modifiers.NEW |
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.PRIVATE |
			Modifiers.STATIC |
			Modifiers.VIRTUAL |
			Modifiers.SEALED |
			Modifiers.OVERRIDE |
			Modifiers.ABSTRACT |
			Modifiers.UNSAFE |
			Modifiers.EXTERN;

		const int AllowedInterfaceModifiers =
			Modifiers.NEW | Modifiers.UNSAFE;

		Method partialMethodImplementation;

		public Method (DeclSpace parent, GenericMethod generic,
			       FullNamedExpression return_type, int mod,
			       MemberName name, ParametersCompiled parameters, Attributes attrs)
			: base (parent, generic, return_type, mod,
				parent.PartialContainer.Kind == Kind.Interface ? AllowedInterfaceModifiers : AllowedModifiers,
				name, attrs, parameters)
		{
		}

		protected Method (DeclSpace parent, FullNamedExpression return_type, int mod, int amod,
					MemberName name, ParametersCompiled parameters, Attributes attrs)
			: base (parent, null, return_type, mod, amod, name, attrs, parameters)
		{
		}
		
		public override string GetSignatureForError()
		{
			return base.GetSignatureForError () + Parameters.GetSignatureForError ();
		}

		void Error_DuplicateEntryPoint (Method b)
		{
			Report.Error (17, b.Location,
				"Program `{0}' has more than one entry point defined: `{1}'",
				CodeGen.FileName, b.GetSignatureForError ());
		}

		bool IsEntryPoint ()
		{
			if (ReturnType != TypeManager.void_type &&
				ReturnType != TypeManager.int32_type)
				return false;

			if (Parameters.Count == 0)
				return true;

			if (Parameters.Count > 1)
				return false;

			Type t = Parameters.Types [0];
			return t.IsArray && t.GetArrayRank () == 1 &&
					TypeManager.GetElementType (t) == TypeManager.string_type &&
					(Parameters[0].ModFlags & ~Parameter.Modifier.PARAMS) == Parameter.Modifier.NONE;
		}

		public override FullNamedExpression LookupNamespaceOrType (string name, Location loc, bool ignore_cs0104)
		{
			TypeParameter[] tp = CurrentTypeParameters;
			if (tp != null) {
				TypeParameter t = TypeParameter.FindTypeParameter (tp, name);
				if (t != null)
					return new TypeParameterExpr (t, loc);
			}

			return base.LookupNamespaceOrType (name, loc, ignore_cs0104);
		}

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb, PredefinedAttributes pa)
		{
			if (a.Type == pa.Conditional) {
				if (IsExplicitImpl) {
					Error_ConditionalAttributeIsNotValid ();
					return;
				}

				if (ReturnType != TypeManager.void_type) {
					Report.Error (578, Location, "Conditional not valid on `{0}' because its return type is not void", GetSignatureForError ());
					return;
				}

				if ((ModFlags & Modifiers.OVERRIDE) != 0) {
					Report.Error (243, Location, "Conditional not valid on `{0}' because it is an override method", GetSignatureForError ());
					return;
				}

				if (IsInterface) {
					Report.Error (582, Location, "Conditional not valid on interface members");
					return;
				}

				if (MethodData.implementing != null) {
					Report.SymbolRelatedToPreviousError (MethodData.implementing.DeclaringType);
					Report.Error (629, Location, "Conditional member `{0}' cannot implement interface member `{1}'",
						GetSignatureForError (), TypeManager.CSharpSignature (MethodData.implementing));
					return;
				}

				for (int i = 0; i < Parameters.Count; ++i) {
					if (Parameters.FixedParameters [i].ModFlags == Parameter.Modifier.OUT) {
						Report.Error (685, Location, "Conditional method `{0}' cannot have an out parameter", GetSignatureForError ());
						return;
					}
				}
			}

			if (a.Type == pa.Extension) {
				a.Error_MisusedExtensionAttribute ();
				return;
			}

			base.ApplyAttributeBuilder (a, cb, pa);
		}

  		protected override bool CheckForDuplications ()
   		{
			if (!base.CheckForDuplications ())
				return false;

			ArrayList ar = Parent.PartialContainer.Properties;
			if (ar != null) {
				for (int i = 0; i < ar.Count; ++i) {
					PropertyBase pb = (PropertyBase) ar [i];
					if (pb.AreAccessorsDuplicateImplementation (this))
						return false;
				}
			}

			ar = Parent.PartialContainer.Indexers;
			if (ar != null) {
				for (int i = 0; i < ar.Count; ++i) {
					PropertyBase pb = (PropertyBase) ar [i];
					if (pb.AreAccessorsDuplicateImplementation (this))
						return false;
				}
			}

			return true;
		}

		protected override bool CheckBase ()
		{
			if (!base.CheckBase ())
				return false;

			if (base_method != null && (ModFlags & Modifiers.OVERRIDE) != 0 && Name == Destructor.MetadataName) {
				Report.Error (249, Location, "Do not override `{0}'. Use destructor syntax instead",
					TypeManager.CSharpSignature (base_method));
			}

			return true;
		}

		public override TypeParameter[] CurrentTypeParameters {
			get {
				if (GenericMethod != null)
					return GenericMethod.CurrentTypeParameters;

				return null;
			}
		}

		//
		// Creates the type
		//
		public override bool Define ()
		{
			if (type_name == TypeManager.system_void_expr && Parameters.IsEmpty && Name == Destructor.MetadataName) {
				Report.Warning (465, 1, Location, "Introducing `Finalize' method can interfere with destructor invocation. Did you intend to declare a destructor?");
			}

			if (!base.Define ())
				return false;

			if (partialMethodImplementation != null && IsPartialDefinition)
				MethodBuilder = partialMethodImplementation.MethodBuilder;

			if (RootContext.StdLib && TypeManager.IsSpecialType (ReturnType)) {
				Error1599 (Location, ReturnType, Report);
				return false;
			}

			if (base_method != null && (ModFlags & Modifiers.NEW) == 0) {
				if (Parameters.Count == 1 && ParameterTypes [0] == TypeManager.object_type && Name == "Equals")
					Parent.PartialContainer.Mark_HasEquals ();
				else if (Parameters.IsEmpty && Name == "GetHashCode")
					Parent.PartialContainer.Mark_HasGetHashCode ();
			}

			if ((ModFlags & Modifiers.STATIC) == 0)
				return true;

			if (Parameters.HasExtensionMethodType) {
				if (Parent.PartialContainer.IsStaticClass && !Parent.IsGeneric) {
					if (!Parent.IsTopLevel)
						Report.Error (1109, Location, "`{0}': Extension methods cannot be defined in a nested class",
							GetSignatureForError ());

					PredefinedAttribute pa = PredefinedAttributes.Get.Extension;
					if (!pa.IsDefined) {
						Report.Error (1110, Location,
							"`{0}': Extension methods cannot be declared without a reference to System.Core.dll assembly. Add the assembly reference or remove `this' modifer from the first parameter",
							GetSignatureForError ());
					}

					ModFlags |= Modifiers.METHOD_EXTENSION;
					Parent.PartialContainer.ModFlags |= Modifiers.METHOD_EXTENSION;
					CodeGen.Assembly.HasExtensionMethods = true;
				} else {
					Report.Error (1106, Location, "`{0}': Extension methods must be defined in a non-generic static class",
						GetSignatureForError ());
				}
			}

			//
			// This is used to track the Entry Point,
			//
			if (RootContext.NeedsEntryPoint &&
				Name == "Main" &&
				(RootContext.MainClass == null ||
				RootContext.MainClass == Parent.TypeBuilder.FullName)){
				if (IsEntryPoint ()) {

					if (RootContext.EntryPoint == null) {
						if (Parent.IsGeneric || MemberName.IsGeneric) {
							Report.Warning (402, 4, Location, "`{0}': an entry point cannot be generic or in a generic type",
								GetSignatureForError ());
						} else {
							SetMemberIsUsed ();
							RootContext.EntryPoint = this;
						}
					} else {
						Error_DuplicateEntryPoint (RootContext.EntryPoint);
						Error_DuplicateEntryPoint (this);
					}
				} else {
					Report.Warning (28, 4, Location, "`{0}' has the wrong signature to be an entry point",
						GetSignatureForError ());
				}
			}

			return true;
		}

		//
		// Emits the code
		// 
		public override void Emit ()
		{
			try {
				Report.Debug (64, "METHOD EMIT", this, MethodBuilder, Location, Block, MethodData);
				if (IsPartialDefinition) {
					//
					// Use partial method implementation builder for partial method declaration attributes
					//
					if (partialMethodImplementation != null) {
						MethodBuilder = partialMethodImplementation.MethodBuilder;
						return;
					}
				} else if ((ModFlags & Modifiers.PARTIAL) != 0 && (caching_flags & Flags.PartialDefinitionExists) == 0) {
					Report.Error (759, Location, "A partial method `{0}' implementation is missing a partial method declaration",
						GetSignatureForError ());
				}

				base.Emit ();
				
				if ((ModFlags & Modifiers.METHOD_EXTENSION) != 0)
					PredefinedAttributes.Get.Extension.EmitAttribute (MethodBuilder);
			} catch {
				Console.WriteLine ("Internal compiler error at {0}: exception caught while emitting {1}",
						   Location, MethodBuilder);
				throw;
			}
		}

		public override bool EnableOverloadChecks (MemberCore overload)
		{
			// TODO: It can be deleted when members will be defined in correct order
			if (overload is Operator)
				return overload.EnableOverloadChecks (this);

			if (overload is Indexer)
				return false;

			return base.EnableOverloadChecks (overload);
		}

		public static void Error1599 (Location loc, Type t, Report Report)
		{
			Report.Error (1599, loc, "Method or delegate cannot return type `{0}'", TypeManager.CSharpName (t));
		}

		protected override MethodInfo FindOutBaseMethod (ref Type base_ret_type)
		{
			MethodInfo mi = (MethodInfo) Parent.PartialContainer.BaseCache.FindMemberToOverride (
				Parent.TypeBuilder, Name, Parameters, GenericMethod, false);

			if (mi == null)
				return null;

			if (mi.IsSpecialName)
				return null;

			base_ret_type = TypeManager.TypeToCoreType (mi.ReturnType);
			return mi;
		}

		public void SetPartialDefinition (Method methodDefinition)
		{
			caching_flags |= Flags.PartialDefinitionExists;
			methodDefinition.partialMethodImplementation = this;

			for (int i = 0; i < methodDefinition.Parameters.Count; ++i ) {
				Parameters [i].DefaultValue = methodDefinition.Parameters [i].DefaultValue;
			}

			if (methodDefinition.attributes == null)
				return;

			if (attributes == null) {
				attributes = methodDefinition.attributes;
			} else {
				attributes.Attrs.AddRange (methodDefinition.attributes.Attrs);
			}
		}

		protected override bool VerifyClsCompliance ()
		{
			if (!base.VerifyClsCompliance ())
				return false;

			if (!Parameters.IsEmpty) {
				ArrayList al = (ArrayList)Parent.PartialContainer.MemberCache.Members [Name];
				if (al.Count > 1)
					MemberCache.VerifyClsParameterConflict (al, this, MethodBuilder, Report);
			}

			return true;
		}
	}

	public abstract class ConstructorInitializer : ExpressionStatement
	{
		Arguments argument_list;
		MethodGroupExpr base_constructor_group;

		public ConstructorInitializer (Arguments argument_list, Location loc)
		{
			this.argument_list = argument_list;
			this.loc = loc;
		}

		public Arguments Arguments {
			get {
				return argument_list;
			}
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			throw new NotSupportedException ("ET");
		}

		public override Expression DoResolve (ResolveContext ec)
		{
			eclass = ExprClass.Value;

			// TODO: ec.GetSignatureForError ()
			ConstructorBuilder caller_builder = ((Constructor) ec.MemberContext).ConstructorBuilder;

			if (argument_list != null) {
				bool dynamic;

				//
				// Spec mandates that constructor initializer will not have `this' access
				//
				using (ec.Set (ResolveContext.Options.BaseInitializer)) {
					argument_list.Resolve (ec, out dynamic);
				}

				if (dynamic) {
					SimpleName ctor = new SimpleName (ConstructorBuilder.ConstructorName, loc);
					return new DynamicInvocation (ctor, argument_list, loc).Resolve (ec) as ExpressionStatement;
				}
			}

			type = ec.CurrentType;
			if (this is ConstructorBaseInitializer) {
				if (ec.CurrentType.BaseType == null)
					return this;

				type = ec.CurrentType.BaseType;
				if (TypeManager.IsStruct (ec.CurrentType)) {
					ec.Report.Error (522, loc,
						"`{0}': Struct constructors cannot call base constructors", TypeManager.CSharpSignature (caller_builder));
					return this;
				}
			} else {
				//
				// It is legal to have "this" initializers that take no arguments
				// in structs, they are just no-ops.
				//
				// struct D { public D (int a) : this () {}
				//
				if (TypeManager.IsStruct (ec.CurrentType) && argument_list == null)
					return this;			
			}

			base_constructor_group = MemberLookupFinal (
				ec, null, type, ConstructorBuilder.ConstructorName, MemberTypes.Constructor,
				BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly,
				loc) as MethodGroupExpr;
			
			if (base_constructor_group == null)
				return this;
			
			base_constructor_group = base_constructor_group.OverloadResolve (
				ec, ref argument_list, false, loc);
			
			if (base_constructor_group == null)
				return this;

			if (!ec.IsStatic)
				base_constructor_group.InstanceExpression = ec.GetThis (loc);
			
			ConstructorInfo base_ctor = (ConstructorInfo)base_constructor_group;

			if (base_ctor == caller_builder){
				ec.Report.Error (516, loc, "Constructor `{0}' cannot call itself", TypeManager.CSharpSignature (caller_builder));
			}
						
			return this;
		}

		public override void Emit (EmitContext ec)
		{
			// It can be null for static initializers
			if (base_constructor_group == null)
				return;
			
			ec.Mark (loc);

			base_constructor_group.EmitCall (ec, argument_list);
		}

		public override void EmitStatement (EmitContext ec)
		{
			Emit (ec);
		}
	}

	public class ConstructorBaseInitializer : ConstructorInitializer {
		public ConstructorBaseInitializer (Arguments argument_list, Location l) :
			base (argument_list, l)
		{
		}
	}

	class GeneratedBaseInitializer: ConstructorBaseInitializer {
		public GeneratedBaseInitializer (Location loc):
			base (null, loc)
		{
		}
	}

	public class ConstructorThisInitializer : ConstructorInitializer {
		public ConstructorThisInitializer (Arguments argument_list, Location l) :
			base (argument_list, l)
		{
		}
	}
	
	public class Constructor : MethodCore, IMethodData {
		public ConstructorBuilder ConstructorBuilder;
		public ConstructorInitializer Initializer;
		ListDictionary declarative_security;
		bool has_compliant_args;

		// <summary>
		//   Modifiers allowed for a constructor.
		// </summary>
		public const int AllowedModifiers =
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.STATIC |
			Modifiers.UNSAFE |
			Modifiers.EXTERN |		
			Modifiers.PRIVATE;

		static readonly string[] attribute_targets = new string [] { "method" };

		//
		// The spec claims that static is not permitted, but
		// my very own code has static constructors.
		//
		public Constructor (DeclSpace parent, string name, int mod, Attributes attrs, ParametersCompiled args,
				    ConstructorInitializer init, Location loc)
			: base (parent, null, null, mod, AllowedModifiers,
				new MemberName (name, loc), attrs, args)
		{
			Initializer = init;
		}

		public bool HasCompliantArgs {
			get { return has_compliant_args; }
		}

		public override AttributeTargets AttributeTargets {
			get { return AttributeTargets.Constructor; }
		}

		//
		// Returns true if this is a default constructor
		//
		public bool IsDefault ()
		{
			if ((ModFlags & Modifiers.STATIC) != 0)
				return Parameters.IsEmpty;
			
			return Parameters.IsEmpty &&
					(Initializer is ConstructorBaseInitializer) &&
					(Initializer.Arguments == null);
		}

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb, PredefinedAttributes pa)
		{
			if (a.IsValidSecurityAttribute ()) {
				if (declarative_security == null) {
					declarative_security = new ListDictionary ();
				}
				a.ExtractSecurityPermissionSet (declarative_security);
				return;
			}

			if (a.IsInternalMethodImplAttribute) {
				is_external_implementation = true;
			}

			ConstructorBuilder.SetCustomAttribute (cb);
		}

		protected override bool CheckBase ()
		{
			if ((ModFlags & Modifiers.STATIC) != 0) {
				if (!Parameters.IsEmpty) {
					Report.Error (132, Location, "`{0}': The static constructor must be parameterless",
						GetSignatureForError ());
					return false;
				}

				// the rest can be ignored
				return true;
			}

			// Check whether arguments were correct.
			if (!DefineParameters (Parameters))
				return false;

			if ((caching_flags & Flags.MethodOverloadsExist) != 0)
				Parent.MemberCache.CheckExistingMembersOverloads (this, ConstructorInfo.ConstructorName,
					Parameters, Report);

			if (Parent.PartialContainer.Kind == Kind.Struct) {
				if (Parameters.Count == 0) {
					Report.Error (568, Location, 
						"Structs cannot contain explicit parameterless constructors");
					return false;
				}
			}

			CheckProtectedModifier ();
			
			return true;
		}
		
		//
		// Creates the ConstructorBuilder
		//
		public override bool Define ()
		{
			if (ConstructorBuilder != null)
				return true;

			MethodAttributes ca = (MethodAttributes.RTSpecialName |
					       MethodAttributes.SpecialName);
			
			if ((ModFlags & Modifiers.STATIC) != 0) {
				ca |= MethodAttributes.Static | MethodAttributes.Private;
			} else {
				ca |= MethodAttributes.HideBySig;

				if ((ModFlags & Modifiers.PUBLIC) != 0)
					ca |= MethodAttributes.Public;
				else if ((ModFlags & Modifiers.PROTECTED) != 0){
					if ((ModFlags & Modifiers.INTERNAL) != 0)
						ca |= MethodAttributes.FamORAssem;
					else 
						ca |= MethodAttributes.Family;
				} else if ((ModFlags & Modifiers.INTERNAL) != 0)
					ca |= MethodAttributes.Assembly;
				else
					ca |= MethodAttributes.Private;
			}

			if (!CheckAbstractAndExtern (block != null))
				return false;
			
			// Check if arguments were correct.
			if (!CheckBase ())
				return false;

			ConstructorBuilder = Parent.TypeBuilder.DefineConstructor (
				ca, CallingConventions,
				Parameters.GetEmitTypes ());

			if (Parent.PartialContainer.IsComImport) {
				if (!IsDefault ()) {
					Report.Error (669, Location, "`{0}': A class with the ComImport attribute cannot have a user-defined constructor",
						Parent.GetSignatureForError ());
				}
				ConstructorBuilder.SetImplementationFlags (MethodImplAttributes.InternalCall);
			}
			
			Parent.MemberCache.AddMember (ConstructorBuilder, this);
			TypeManager.AddMethod (ConstructorBuilder, this);
			
			// It's here only to report an error
			if (block != null && block.IsIterator) {
				member_type = TypeManager.void_type;
				Iterator.CreateIterator (this, Parent.PartialContainer, ModFlags, Compiler);
			}

			return true;
		}

		//
		// Emits the code
		//
		public override void Emit ()
		{
			if ((ModFlags & Modifiers.DEBUGGER_HIDDEN) != 0)
				PredefinedAttributes.Get.DebuggerHidden.EmitAttribute (ConstructorBuilder);

			if (OptAttributes != null)
				OptAttributes.Emit ();

			base.Emit ();

			//
			// If we use a "this (...)" constructor initializer, then
			// do not emit field initializers, they are initialized in the other constructor
			//
			bool emit_field_initializers = ((ModFlags & Modifiers.STATIC) != 0) ||
				!(Initializer is ConstructorThisInitializer);

			BlockContext bc = new BlockContext (this, block, TypeManager.void_type);
			bc.Set (ResolveContext.Options.ConstructorScope);

			if (emit_field_initializers)
				Parent.PartialContainer.ResolveFieldInitializers (bc);

			if (block != null) {
				// If this is a non-static `struct' constructor and doesn't have any
				// initializer, it must initialize all of the struct's fields.
				if ((Parent.PartialContainer.Kind == Kind.Struct) &&
					((ModFlags & Modifiers.STATIC) == 0) && (Initializer == null))
					block.AddThisVariable (Parent, Location);

				if (block != null && (ModFlags & Modifiers.STATIC) == 0){
					if (Parent.PartialContainer.Kind == Kind.Class && Initializer == null)
						Initializer = new GeneratedBaseInitializer (Location);

					if (Initializer != null) {
						block.AddScopeStatement (new StatementExpression (Initializer));
					}
				}
			}

			Parameters.ApplyAttributes (ConstructorBuilder);

			SourceMethod source = SourceMethod.Create (Parent, ConstructorBuilder, block);

			if (block != null) {
				if (block.Resolve (null, bc, Parameters, this)) {
					EmitContext ec = new EmitContext (this, ConstructorBuilder.GetILGenerator (), bc.ReturnType);
					ec.With (EmitContext.Options.ConstructorScope, true);

					if (!ec.HasReturnLabel && bc.HasReturnLabel) {
						ec.ReturnLabel = bc.ReturnLabel;
						ec.HasReturnLabel = true;
					}

					block.Emit (ec);
				}
			}

			if (source != null)
				source.CloseMethod ();

			if (declarative_security != null) {
				foreach (DictionaryEntry de in declarative_security) {
					ConstructorBuilder.AddDeclarativeSecurity ((SecurityAction)de.Key, (PermissionSet)de.Value);
				}
			}

			block = null;
		}

		// Is never override
		protected override MethodInfo FindOutBaseMethod (ref Type base_ret_type)
		{
			return null;
		}

		public override string GetSignatureForError()
		{
			return base.GetSignatureForError () + Parameters.GetSignatureForError ();
		}

		public override string[] ValidAttributeTargets {
			get {
				return attribute_targets;
			}
		}

		protected override bool VerifyClsCompliance ()
		{
			if (!base.VerifyClsCompliance () || !IsExposedFromAssembly ()) {
				return false;
			}
			
 			if (!Parameters.IsEmpty) {
 				ArrayList al = (ArrayList)Parent.MemberCache.Members [ConstructorInfo.ConstructorName];
 				if (al.Count > 2)
 					MemberCache.VerifyClsParameterConflict (al, this, ConstructorBuilder, Report);
 
				if (TypeManager.IsSubclassOf (Parent.TypeBuilder, TypeManager.attribute_type)) {
					foreach (Type param in Parameters.Types) {
						if (param.IsArray) {
							return true;
						}
					}
				}
			}
			has_compliant_args = true;
			return true;
		}

		#region IMethodData Members

		public MemberName MethodName {
			get {
				return MemberName;
			}
		}

		public Type ReturnType {
			get {
				return MemberType;
			}
		}

		public EmitContext CreateEmitContext (ILGenerator ig)
		{
			throw new NotImplementedException ();
		}

		public bool IsExcluded()
		{
			return false;
		}

		GenericMethod IMethodData.GenericMethod {
			get {
				return null;
			}
		}

		void IMethodData.EmitExtraSymbolInfo (SourceMethod source)
		{ }

		#endregion
	}

	/// <summary>
	/// Interface for MethodData class. Holds links to parent members to avoid member duplication.
	/// </summary>
	public interface IMethodData
	{
		CallingConventions CallingConventions { get; }
		Location Location { get; }
		MemberName MethodName { get; }
		Type ReturnType { get; }
		GenericMethod GenericMethod { get; }
		ParametersCompiled ParameterInfo { get; }

		Attributes OptAttributes { get; }
		ToplevelBlock Block { get; set; }

		EmitContext CreateEmitContext (ILGenerator ig);
		ObsoleteAttribute GetObsoleteAttribute ();
		string GetSignatureForError ();
		bool IsExcluded ();
		bool IsClsComplianceRequired ();
		void SetMemberIsUsed ();
		void EmitExtraSymbolInfo (SourceMethod source);
	}

	//
	// Encapsulates most of the Method's state
	//
	public class MethodData {
		static FieldInfo methodbuilder_attrs_field;
		public readonly IMethodData method;

		public readonly GenericMethod GenericMethod;

		//
		// Are we implementing an interface ?
		//
		public MethodInfo implementing;

		//
		// Protected data.
		//
		protected InterfaceMemberBase member;
		protected int modifiers;
		protected MethodAttributes flags;
		protected Type declaring_type;
		protected MethodInfo parent_method;

		MethodBuilder builder = null;
		public MethodBuilder MethodBuilder {
			get {
				return builder;
			}
		}

		public Type DeclaringType {
			get {
				return declaring_type;
			}
		}

		public MethodData (InterfaceMemberBase member,
				   int modifiers, MethodAttributes flags, IMethodData method)
		{
			this.member = member;
			this.modifiers = modifiers;
			this.flags = flags;

			this.method = method;
		}

		public MethodData (InterfaceMemberBase member, 
				   int modifiers, MethodAttributes flags, 
				   IMethodData method, MethodBuilder builder,
				   GenericMethod generic, MethodInfo parent_method)
			: this (member, modifiers, flags, method)
		{
			this.builder = builder;
			this.GenericMethod = generic;
			this.parent_method = parent_method;
		}

		public bool Define (DeclSpace parent, string method_full_name, Report Report)
		{
			string name = method.MethodName.Basename;

			TypeContainer container = parent.PartialContainer;

			PendingImplementation pending = container.PendingImplementations;
			if (pending != null){
				implementing = pending.IsInterfaceMethod (name, member.InterfaceType, this);

				if (member.InterfaceType != null){
					if (implementing == null){
						if (member is PropertyBase) {
							Report.Error (550, method.Location, "`{0}' is an accessor not found in interface member `{1}{2}'",
								      method.GetSignatureForError (), TypeManager.CSharpName (member.InterfaceType),
								      member.GetSignatureForError ().Substring (member.GetSignatureForError ().LastIndexOf ('.')));

						} else {
							Report.Error (539, method.Location,
								      "`{0}.{1}' in explicit interface declaration is not a member of interface",
								      TypeManager.CSharpName (member.InterfaceType), member.ShortName);
						}
						return false;
					}
					if (implementing.IsSpecialName && !(method is AbstractPropertyEventMethod)) {
						Report.SymbolRelatedToPreviousError (implementing);
						Report.Error (683, method.Location, "`{0}' explicit method implementation cannot implement `{1}' because it is an accessor",
							member.GetSignatureForError (), TypeManager.CSharpSignature (implementing));
						return false;
					}
				} else {
					if (implementing != null) {
						AbstractPropertyEventMethod prop_method = method as AbstractPropertyEventMethod;
						if (prop_method == null) {
							if (TypeManager.IsSpecialMethod (implementing)) {
								Report.SymbolRelatedToPreviousError (implementing);
								Report.Error (470, method.Location, "Method `{0}' cannot implement interface accessor `{1}.{2}'",
									method.GetSignatureForError (), TypeManager.CSharpSignature (implementing),
									implementing.Name.StartsWith ("get_") ? "get" : "set");
							}
						} else if (implementing.DeclaringType.IsInterface) {
							if (!implementing.IsSpecialName) {
								Report.SymbolRelatedToPreviousError (implementing);
								Report.Error (686, method.Location, "Accessor `{0}' cannot implement interface member `{1}' for type `{2}'. Use an explicit interface implementation",
									method.GetSignatureForError (), TypeManager.CSharpSignature (implementing), container.GetSignatureForError ());
								return false;
							}
							PropertyBase.PropertyMethod pm = prop_method as PropertyBase.PropertyMethod;
							if (pm != null && pm.HasCustomAccessModifier && (pm.ModFlags & Modifiers.PUBLIC) == 0) {
								Report.SymbolRelatedToPreviousError (implementing);
								Report.Error (277, method.Location, "Accessor `{0}' must be declared public to implement interface member `{1}'",
									method.GetSignatureForError (), TypeManager.CSharpSignature (implementing, true));
								return false;
							}
						}
					}
				}
			}

			//
			// For implicit implementations, make sure we are public, for
			// explicit implementations, make sure we are private.
			//
			if (implementing != null){
				//
				// Setting null inside this block will trigger a more
				// verbose error reporting for missing interface implementations
				//
				// The "candidate" function has been flagged already
				// but it wont get cleared
				//
				if (member.IsExplicitImpl){
					if (method.ParameterInfo.HasParams && !TypeManager.GetParameterData (implementing).HasParams) {
						Report.SymbolRelatedToPreviousError (implementing);
						Report.Error (466, method.Location, "`{0}': the explicit interface implementation cannot introduce the params modifier",
							method.GetSignatureForError ());
						return false;
					}
				} else {
					if (implementing.DeclaringType.IsInterface) {
						//
						// If this is an interface method implementation,
						// check for public accessibility
						//
						if ((flags & MethodAttributes.MemberAccessMask) != MethodAttributes.Public)
						{
							implementing = null;
						}
					} else if ((flags & MethodAttributes.MemberAccessMask) == MethodAttributes.Private){
						// We may never be private.
						implementing = null;

					} else if ((modifiers & Modifiers.OVERRIDE) == 0){
						//
						// We may be protected if we're overriding something.
						//
						implementing = null;
					}
				}
					
				//
				// Static is not allowed
				//
				if ((modifiers & Modifiers.STATIC) != 0){
					implementing = null;
				}
			}
			
			//
			// If implementing is still valid, set flags
			//
			if (implementing != null){
				//
				// When implementing interface methods, set NewSlot
				// unless, we are overwriting a method.
				//
				if (implementing.DeclaringType.IsInterface){
					if ((modifiers & Modifiers.OVERRIDE) == 0)
						flags |= MethodAttributes.NewSlot;
				}
				flags |=
					MethodAttributes.Virtual |
					MethodAttributes.HideBySig;

				// Set Final unless we're virtual, abstract or already overriding a method.
				if ((modifiers & (Modifiers.VIRTUAL | Modifiers.ABSTRACT | Modifiers.OVERRIDE)) == 0)
					flags |= MethodAttributes.Final;
			}

			DefineMethodBuilder (container, method_full_name, method.ParameterInfo);

			if (builder == null)
				return false;

			if (container.CurrentType != null)
				declaring_type = container.CurrentType;
			else
				declaring_type = container.TypeBuilder;

			if (implementing != null && member.IsExplicitImpl) {
					container.TypeBuilder.DefineMethodOverride (builder, implementing);
			}

			TypeManager.AddMethod (builder, method);

			if (GenericMethod != null) {
				bool is_override = member.IsExplicitImpl |
					((modifiers & Modifiers.OVERRIDE) != 0);

				if (implementing != null)
					parent_method = implementing;

				if (!GenericMethod.DefineType (GenericMethod, builder, parent_method, is_override))
					return false;
			}

			return true;
		}


		/// <summary>
		/// Create the MethodBuilder for the method 
		/// </summary>
		void DefineMethodBuilder (TypeContainer container, string method_name, ParametersCompiled param)
		{
			if (builder == null) {
				builder = container.TypeBuilder.DefineMethod (
					method_name, flags, method.CallingConventions,
					TypeManager.TypeToReflectionType (method.ReturnType),
					param.GetEmitTypes ());
				return;
			}

#if GMCS_SOURCE
			//
			// Generic method has been already defined to resolve method parameters
			// correctly when they use type parameters
			//
			builder.SetParameters (param.GetEmitTypes ());
			builder.SetReturnType (method.ReturnType);
#endif
			if (builder.Attributes != flags) {
				try {
					if (methodbuilder_attrs_field == null)
						methodbuilder_attrs_field = typeof (MethodBuilder).GetField ("attrs", BindingFlags.NonPublic | BindingFlags.Instance);
					methodbuilder_attrs_field.SetValue (builder, flags);
				} catch {
					RootContext.ToplevelTypes.Compiler.Report.RuntimeMissingSupport (method.Location, "Generic method MethodAttributes");
				}
			}
		}

		//
		// Emits the code
		// 
		public void Emit (DeclSpace parent)
		{
			method.ParameterInfo.ApplyAttributes (MethodBuilder);

			if (GenericMethod != null)
				GenericMethod.EmitAttributes ();

			//
			// clear the pending implementation flag
			//
			if (implementing != null)
				parent.PartialContainer.PendingImplementations.ImplementMethod (method.MethodName.Basename,
					member.InterfaceType, this, member.IsExplicitImpl);

			SourceMethod source = SourceMethod.Create (parent, MethodBuilder, method.Block);

			ToplevelBlock block = method.Block;
			if (block != null) {
				BlockContext bc = new BlockContext ((IMemberContext) method, block, method.ReturnType);
				if (block.Resolve (null, bc, method.ParameterInfo, method)) {
					EmitContext ec = method.CreateEmitContext (MethodBuilder.GetILGenerator ());
					if (!ec.HasReturnLabel && bc.HasReturnLabel) {
						ec.ReturnLabel = bc.ReturnLabel;
						ec.HasReturnLabel = true;
					}

					block.Emit (ec);
				}
			}

			if (source != null) {
				method.EmitExtraSymbolInfo (source);
				source.CloseMethod ();
			}
		}
	}

	public class Destructor : MethodOrOperator
	{
		const int AllowedModifiers =
			Modifiers.UNSAFE |
			Modifiers.EXTERN;

		static readonly string[] attribute_targets = new string [] { "method" };

		public static readonly string MetadataName = "Finalize";

		public Destructor (DeclSpace parent, int mod, ParametersCompiled parameters, Attributes attrs, Location l)
			: base (parent, null, TypeManager.system_void_expr, mod, AllowedModifiers,
				new MemberName (MetadataName, l), attrs, parameters)
		{
			ModFlags &= ~Modifiers.PRIVATE;
			ModFlags |= Modifiers.PROTECTED;
		}

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb, PredefinedAttributes pa)
		{
			if (a.Type == pa.Conditional) {
				Error_ConditionalAttributeIsNotValid ();
				return;
			}

			base.ApplyAttributeBuilder (a, cb, pa);
		}

		protected override bool CheckBase ()
		{
			flags |= MethodAttributes.Virtual;

			if (!base.CheckBase ())
				return false;

			if (Parent.PartialContainer.BaseCache == null)
				return true;

			Type base_type = Parent.PartialContainer.BaseCache.Container.Type;
			if (base_type != null && Block != null) {
				MethodGroupExpr method_expr = Expression.MethodLookup (Parent.Module.Compiler, Parent.TypeBuilder, base_type, MetadataName, Location);
				if (method_expr == null)
					throw new NotImplementedException ();

				method_expr.IsBase = true;
				method_expr.InstanceExpression = new CompilerGeneratedThis (Parent.TypeBuilder, Location);

				ToplevelBlock new_block = new ToplevelBlock (Compiler, Block.StartLocation);
				new_block.EndLocation = Block.EndLocation;

				Block finaly_block = new ExplicitBlock (new_block, Location, Location);
				Block try_block = new Block (new_block, block);

				//
				// 0-size arguments to avoid CS0250 error
				// TODO: Should use AddScopeStatement or something else which emits correct
				// debugger scope
				//
				finaly_block.AddStatement (new StatementExpression (new Invocation (method_expr, new Arguments (0))));
				new_block.AddStatement (new TryFinally (try_block, finaly_block, Location));

				block = new_block;
			}

			return true;
		}

		public override string GetSignatureForError ()
		{
			return Parent.GetSignatureForError () + ".~" + Parent.MemberName.Name + "()";
		}

		protected override MethodInfo FindOutBaseMethod (ref Type base_ret_type)
		{
			return null;
		}

		public override string[] ValidAttributeTargets {
			get {
				return attribute_targets;
			}
		}
	}
	
	public abstract class MemberBase : MemberCore
	{
		protected FullNamedExpression type_name;
		protected Type member_type;

		public readonly DeclSpace ds;
		public readonly GenericMethod GenericMethod;

		protected MemberBase (DeclSpace parent, GenericMethod generic,
				      FullNamedExpression type, int mod, int allowed_mod, int def_mod,
				      MemberName name, Attributes attrs)
			: base (parent, name, attrs)
		{
			this.ds = generic != null ? generic : (DeclSpace) parent;
			this.type_name = type;
			ModFlags = Modifiers.Check (allowed_mod, mod, def_mod, Location, Report);
			GenericMethod = generic;
			if (GenericMethod != null)
				GenericMethod.ModFlags = ModFlags;
		}

		//
		// Main member define entry
		//
		public override bool Define ()
		{
			DoMemberTypeIndependentChecks ();

			//
			// Returns false only when type resolution failed
			//
			if (!ResolveMemberType ())
				return false;

			DoMemberTypeDependentChecks ();
			return true;
		}

		//
		// Any type_name independent checks
		//
		protected virtual void DoMemberTypeIndependentChecks ()
		{
			if ((Parent.ModFlags & Modifiers.SEALED) != 0 &&
				(ModFlags & (Modifiers.VIRTUAL | Modifiers.ABSTRACT)) != 0) {
				Report.Error (549, Location, "New virtual member `{0}' is declared in a sealed class `{1}'",
					GetSignatureForError (), Parent.GetSignatureForError ());
			}
		}

		//
		// Any type_name dependent checks
		//
		protected virtual void DoMemberTypeDependentChecks ()
		{
			// verify accessibility
			if (!IsAccessibleAs (MemberType)) {
				Report.SymbolRelatedToPreviousError (MemberType);
				if (this is Property)
					Report.Error (53, Location,
						      "Inconsistent accessibility: property type `" +
						      TypeManager.CSharpName (MemberType) + "' is less " +
						      "accessible than property `" + GetSignatureForError () + "'");
				else if (this is Indexer)
					Report.Error (54, Location,
						      "Inconsistent accessibility: indexer return type `" +
						      TypeManager.CSharpName (MemberType) + "' is less " +
						      "accessible than indexer `" + GetSignatureForError () + "'");
				else if (this is MethodCore) {
					if (this is Operator)
						Report.Error (56, Location,
							      "Inconsistent accessibility: return type `" +
							      TypeManager.CSharpName (MemberType) + "' is less " +
							      "accessible than operator `" + GetSignatureForError () + "'");
					else
						Report.Error (50, Location,
							      "Inconsistent accessibility: return type `" +
							      TypeManager.CSharpName (MemberType) + "' is less " +
							      "accessible than method `" + GetSignatureForError () + "'");
				} else {
					Report.Error (52, Location,
						      "Inconsistent accessibility: field type `" +
						      TypeManager.CSharpName (MemberType) + "' is less " +
						      "accessible than field `" + GetSignatureForError () + "'");
				}
			}

			Variance variance = this is Event ? Variance.Contravariant : Variance.Covariant;
			TypeManager.CheckTypeVariance (MemberType, variance, this);
		}

		protected bool IsTypePermitted ()
		{
			if (TypeManager.IsSpecialType (MemberType)) {
				Report.Error (610, Location, "Field or property cannot be of type `{0}'", TypeManager.CSharpName (MemberType));
				return false;
			}
			return true;
		}

		protected virtual bool CheckBase ()
		{
			CheckProtectedModifier ();

			return true;
		}

		public Type MemberType {
			get { return member_type; }
		}

		protected virtual bool ResolveMemberType ()
		{
			if (member_type != null)
				throw new InternalErrorException ("Multi-resolve");

			TypeExpr te = type_name.ResolveAsTypeTerminal (this, false);
			if (te == null)
				return false;
			
			//
			// Replace original type name, error reporting can use fully resolved name
			//
			type_name = te;

			member_type = te.Type;
			return true;
		}
	}

	//
	// Abstract class for all fields
	//
	abstract public class FieldBase : MemberBase {
		public FieldBuilder FieldBuilder;
		public Status status;
		protected Expression initializer;

		[Flags]
		public enum Status : byte {
			HAS_OFFSET = 4		// Used by FieldMember.
		}

		static readonly string[] attribute_targets = new string [] { "field" };

		protected FieldBase (DeclSpace parent, FullNamedExpression type, int mod,
				     int allowed_mod, MemberName name, Attributes attrs)
			: base (parent, null, type, mod, allowed_mod | Modifiers.ABSTRACT, Modifiers.PRIVATE,
				name, attrs)
		{
			if ((mod & Modifiers.ABSTRACT) != 0)
				Report.Error (681, Location, "The modifier 'abstract' is not valid on fields. Try using a property instead");
		}

		public override AttributeTargets AttributeTargets {
			get {
				return AttributeTargets.Field;
			}
		}

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb, PredefinedAttributes pa)
		{
			if (a.Type == pa.FieldOffset) {
				status |= Status.HAS_OFFSET;

				if (!Parent.PartialContainer.HasExplicitLayout) {
					Report.Error (636, Location, "The FieldOffset attribute can only be placed on members of types marked with the StructLayout(LayoutKind.Explicit)");
					return;
				}

				if ((ModFlags & Modifiers.STATIC) != 0 || this is Const) {
					Report.Error (637, Location, "The FieldOffset attribute is not allowed on static or const fields");
					return;
				}
			}

#if NET_2_0
			if (a.Type == pa.FixedBuffer) {
				Report.Error (1716, Location, "Do not use 'System.Runtime.CompilerServices.FixedBuffer' attribute. Use the 'fixed' field modifier instead");
				return;
			}
#endif

#if !NET_2_0
			if (a.Type == pa.MarshalAs) {
				UnmanagedMarshal marshal = a.GetMarshal (this);
				if (marshal != null) {
					FieldBuilder.SetMarshal (marshal);
				}
				return;
			}
#endif
			if ((a.HasSecurityAttribute)) {
				a.Error_InvalidSecurityParent ();
				return;
			}

			FieldBuilder.SetCustomAttribute (cb);
		}

 		protected override bool CheckBase ()
		{
 			if (!base.CheckBase ())
 				return false;
 
 			MemberInfo conflict_symbol = Parent.PartialContainer.FindBaseMemberWithSameName (Name, false);
 			if (conflict_symbol == null) {
 				if ((ModFlags & Modifiers.NEW) != 0) {
 					Report.Warning (109, 4, Location, "The member `{0}' does not hide an inherited member. The new keyword is not required", GetSignatureForError ());
 				}
 				return true;
 			}
 
 			if ((ModFlags & (Modifiers.NEW | Modifiers.OVERRIDE | Modifiers.BACKING_FIELD)) == 0) {
				Report.SymbolRelatedToPreviousError (conflict_symbol);
				Report.Warning (108, 2, Location, "`{0}' hides inherited member `{1}'. Use the new keyword if hiding was intended",
					GetSignatureForError (), TypeManager.GetFullNameSignature (conflict_symbol));
			}
 
 			return true;
 		}

		protected override void DoMemberTypeDependentChecks ()
		{
			base.DoMemberTypeDependentChecks ();

			if (TypeManager.IsGenericParameter (MemberType))
				return;

			if (MemberType.IsSealed && MemberType.IsAbstract) {
				Error_VariableOfStaticClass (Location, GetSignatureForError (), MemberType, Report);
			}

			CheckBase ();
			IsTypePermitted ();
		}

		//
		//   Represents header string for documentation comment.
		//
		public override string DocCommentHeader {
			get { return "F:"; }
		}

		public override void Emit ()
		{
			if (TypeManager.IsDynamicType (member_type))
				PredefinedAttributes.Get.Dynamic.EmitAttribute (FieldBuilder);

			if ((ModFlags & Modifiers.COMPILER_GENERATED) != 0 && !Parent.IsCompilerGenerated)
				PredefinedAttributes.Get.CompilerGenerated.EmitAttribute (FieldBuilder);

			if (OptAttributes != null) {
				OptAttributes.Emit ();
			}

			if (((status & Status.HAS_OFFSET) == 0) && (ModFlags & (Modifiers.STATIC | Modifiers.BACKING_FIELD)) == 0 && Parent.PartialContainer.HasExplicitLayout) {
				Report.Error (625, Location, "`{0}': Instance field types marked with StructLayout(LayoutKind.Explicit) must have a FieldOffset attribute", GetSignatureForError ());
			}

			base.Emit ();
		}

		public static void Error_VariableOfStaticClass (Location loc, string variable_name, Type static_class, Report Report)
		{
			Report.SymbolRelatedToPreviousError (static_class);
			Report.Error (723, loc, "`{0}': cannot declare variables of static types",
				variable_name);
		}

		public Expression Initializer {
			set {
				if (value != null) {
					this.initializer = value;
				}
			}
		}

		protected virtual bool IsFieldClsCompliant {
			get {
				if (FieldBuilder == null)
					return true;

				return AttributeTester.IsClsCompliant (FieldBuilder.FieldType);
			}
		}

		public override string[] ValidAttributeTargets 
		{
			get {
				return attribute_targets;
			}
		}

		protected override bool VerifyClsCompliance ()
		{
			if (!base.VerifyClsCompliance ())
				return false;

			if (!IsFieldClsCompliant) {
				Report.Warning (3003, 1, Location, "Type of `{0}' is not CLS-compliant",
					GetSignatureForError ());
			}
			return true;
		}

		public void SetAssigned ()
		{
			caching_flags |= Flags.IsAssigned;
		}
	}

	interface IFixedBuffer
	{
		FieldInfo Element { get; }
		Type ElementType { get; }
	}

	public class FixedFieldExternal: IFixedBuffer
	{
		FieldInfo element_field;

		public FixedFieldExternal (FieldInfo fi)
		{
			element_field = fi.FieldType.GetField (FixedField.FixedElementName);
		}

		#region IFixedField Members

		public FieldInfo Element {
			get {
				return element_field;
			}
		}

		public Type ElementType {
			get {
				return element_field.FieldType;
			}
		}

		#endregion
	}

	/// <summary>
	/// Fixed buffer implementation
	/// </summary>
	public class FixedField : FieldBase, IFixedBuffer
	{
		public const string FixedElementName = "FixedElementField";
		static int GlobalCounter = 0;
		static object[] ctor_args = new object[] { (short)LayoutKind.Sequential };
		static FieldInfo[] fi;

		TypeBuilder fixed_buffer_type;
		FieldBuilder element;
		Expression size_expr;

		const int AllowedModifiers =
			Modifiers.NEW |
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.PRIVATE |
			Modifiers.UNSAFE;

		public FixedField (DeclSpace parent, FullNamedExpression type, int mod, string name,
			Expression size_expr, Attributes attrs, Location loc):
			base (parent, type, mod, AllowedModifiers, new MemberName (name, loc), attrs)
		{
			if (RootContext.Version < LanguageVersion.ISO_2)
				Report.FeatureIsNotAvailable (loc, "fixed size buffers");

			this.size_expr = size_expr;
		}

		public override bool Define()
		{
			if (!base.Define ())
				return false;

			if (!TypeManager.IsPrimitiveType (MemberType)) {
				Report.Error (1663, Location, "`{0}': Fixed size buffers type must be one of the following: bool, byte, short, int, long, char, sbyte, ushort, uint, ulong, float or double",
					GetSignatureForError ());
			}			
			
			// Create nested fixed buffer container
			string name = String.Format ("<{0}>__FixedBuffer{1}", Name, GlobalCounter++);
			fixed_buffer_type = Parent.TypeBuilder.DefineNestedType (name, Parent.Module.DefaultCharSetType |
				TypeAttributes.NestedPublic | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit, TypeManager.value_type);
			
			element = fixed_buffer_type.DefineField (FixedElementName, MemberType, FieldAttributes.Public);
			RootContext.RegisterCompilerGeneratedType (fixed_buffer_type);
			
			FieldBuilder = Parent.TypeBuilder.DefineField (Name, fixed_buffer_type, Modifiers.FieldAttr (ModFlags));
			Parent.MemberCache.AddMember (FieldBuilder, this);
			TypeManager.RegisterFieldBase (FieldBuilder, this);

			return true;
		}

		protected override void DoMemberTypeIndependentChecks ()
		{
			base.DoMemberTypeIndependentChecks ();

			if (!IsUnsafe)
				Expression.UnsafeError (Report, Location);

			if (Parent.PartialContainer.Kind != Kind.Struct) {
				Report.Error (1642, Location, "`{0}': Fixed size buffer fields may only be members of structs",
					GetSignatureForError ());
			}
		}

		public override void Emit()
		{
			ResolveContext rc = new ResolveContext (this);
			Constant c = size_expr.ResolveAsConstant (rc, this);
			if (c == null)
				return;
			
			IntConstant buffer_size_const = c.ImplicitConversionRequired (rc, TypeManager.int32_type, Location) as IntConstant;
			if (buffer_size_const == null)
				return;

			int buffer_size = buffer_size_const.Value;

			if (buffer_size <= 0) {
				Report.Error (1665, Location, "`{0}': Fixed size buffers must have a length greater than zero", GetSignatureForError ());
				return;
			}

			int type_size = Expression.GetTypeSize (MemberType);

			if (buffer_size > int.MaxValue / type_size) {
				Report.Error (1664, Location, "Fixed size buffer `{0}' of length `{1}' and type `{2}' exceeded 2^31 limit",
					GetSignatureForError (), buffer_size.ToString (), TypeManager.CSharpName (MemberType));
				return;
			}

			buffer_size *= type_size;
			EmitFieldSize (buffer_size);

			PredefinedAttributes.Get.UnsafeValueType.EmitAttribute (fixed_buffer_type);

			base.Emit ();
		}

		void EmitFieldSize (int buffer_size)
		{
			CustomAttributeBuilder cab;
			PredefinedAttribute pa;

			pa = PredefinedAttributes.Get.StructLayout;
			if (pa.Constructor == null &&
				!pa.ResolveConstructor (Location, TypeManager.short_type))
					return;

			// TODO: It's not cleared
			if (fi == null)
				fi = new FieldInfo[] { pa.Type.GetField ("Size") };

			object[] fi_val = new object[] { buffer_size };
			cab = new CustomAttributeBuilder (pa.Constructor,
				ctor_args, fi, fi_val);
			fixed_buffer_type.SetCustomAttribute (cab);
			
			//
			// Don't emit FixedBufferAttribute attribute for private types
			//
			if ((ModFlags & Modifiers.PRIVATE) != 0)
				return;

			pa = PredefinedAttributes.Get.FixedBuffer;
			if (pa.Constructor == null &&
				!pa.ResolveConstructor (Location, TypeManager.type_type, TypeManager.int32_type))
				return;

			cab = new CustomAttributeBuilder (pa.Constructor, new object[] { MemberType, buffer_size });
			FieldBuilder.SetCustomAttribute (cab);
		}

		protected override bool IsFieldClsCompliant {
			get {
				return false;
			}
		}

		public void SetCharSet (TypeAttributes ta)
		{
			TypeAttributes cta = fixed_buffer_type.Attributes;
			if ((cta & TypeAttributes.UnicodeClass) != (ta & TypeAttributes.UnicodeClass))
				SetTypeBuilderCharSet ((cta & ~TypeAttributes.AutoClass) | TypeAttributes.UnicodeClass);
			else if ((cta & TypeAttributes.AutoClass) != (ta & TypeAttributes.AutoClass))
				SetTypeBuilderCharSet ((cta & ~TypeAttributes.UnicodeClass) | TypeAttributes.AutoClass);
			else if (cta == 0 && ta != 0)
				SetTypeBuilderCharSet (cta & ~(TypeAttributes.UnicodeClass | TypeAttributes.AutoClass));
		}

		void SetTypeBuilderCharSet (TypeAttributes ta)
		{
			MethodInfo mi = typeof (TypeBuilder).GetMethod ("SetCharSet", BindingFlags.Instance | BindingFlags.NonPublic);
			if (mi == null) {
				Report.RuntimeMissingSupport (Location, "TypeBuilder::SetCharSet");
			} else {
				mi.Invoke (fixed_buffer_type, new object [] { ta });
			}
		}

		#region IFixedField Members

		public FieldInfo Element {
			get {
				return element;
			}
		}

		public Type ElementType {
			get {
				return MemberType;
			}
		}

		#endregion
	}

	//
	// The Field class is used to represents class/struct fields during parsing.
	//
	public class Field : FieldBase {
		// <summary>
		//   Modifiers allowed in a class declaration
		// </summary>
		const int AllowedModifiers =
			Modifiers.NEW |
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.PRIVATE |
			Modifiers.STATIC |
			Modifiers.VOLATILE |
			Modifiers.UNSAFE |
			Modifiers.READONLY;

		public Field (DeclSpace parent, FullNamedExpression type, int mod, MemberName name,
			      Attributes attrs)
			: base (parent, type, mod, AllowedModifiers, name, attrs)
		{
		}

		bool CanBeVolatile ()
		{
			if (TypeManager.IsReferenceType (MemberType))
				return true;

			if (MemberType == TypeManager.bool_type || MemberType == TypeManager.char_type ||
				MemberType == TypeManager.sbyte_type || MemberType == TypeManager.byte_type ||
				MemberType == TypeManager.short_type || MemberType == TypeManager.ushort_type ||
				MemberType == TypeManager.int32_type || MemberType == TypeManager.uint32_type ||
				MemberType == TypeManager.float_type ||
				MemberType == TypeManager.intptr_type || MemberType == TypeManager.uintptr_type)
				return true;

			if (TypeManager.IsEnumType (MemberType))
				return true;

			return false;
		}

		bool CheckStructLayout (Type type, bool isStatic)
		{
			if (TypeManager.IsBuiltinType (type))
				return true;

			if (isStatic) {
				if (!TypeManager.IsValueType (type) || TypeManager.IsEqual (type, Parent.TypeBuilder))
					return true;
			}

			if (!TypeManager.IsEqual (TypeManager.DropGenericTypeArguments (type), Parent.TypeBuilder)) {
				if (!TypeManager.IsGenericType (type))
					return true;

				foreach (Type t in TypeManager.GetTypeArguments (type)) {
					if (!CheckStructLayout (t, false))
						return false;
				}
				return true;
			}
			
			Report.Error (523, Location,
				"Struct member `{0}' of type `{1}' causes a cycle in the struct layout",
				GetSignatureForError (), TypeManager.CSharpName (MemberType));
			return false;
		}

		public override bool Define ()
		{
			if (!base.Define ())
				return false;

			try {
#if GMCS_SOURCE
				Type[] required_modifier = null;
				if ((ModFlags & Modifiers.VOLATILE) != 0) {
					if (TypeManager.isvolatile_type == null)
						TypeManager.isvolatile_type = TypeManager.CoreLookupType (Compiler,
							"System.Runtime.CompilerServices", "IsVolatile", Kind.Class, true);

					if (TypeManager.isvolatile_type != null)
						required_modifier = new Type [] { TypeManager.isvolatile_type };
				}

				FieldBuilder = Parent.TypeBuilder.DefineField (
					Name, MemberType, required_modifier, null, Modifiers.FieldAttr (ModFlags));
#else
				FieldBuilder = Parent.TypeBuilder.DefineField (
					Name, MemberType, Modifiers.FieldAttr (ModFlags));
#endif
				// Don't cache inaccessible fields
				if ((ModFlags & Modifiers.BACKING_FIELD) == 0) {
					Parent.MemberCache.AddMember (FieldBuilder, this);
				}

				TypeManager.RegisterFieldBase (FieldBuilder, this);
			}
			catch (ArgumentException) {
				Report.RuntimeMissingSupport (Location, "`void' or `void*' field type");
				return false;
			}

			if (initializer != null) {
				((TypeContainer) Parent).RegisterFieldForInitialization (this,
					new FieldInitializer (FieldBuilder, initializer, this));
			} else {
				if (Parent.PartialContainer.Kind == Kind.Struct)
					CheckStructLayout (member_type, (ModFlags & Modifiers.STATIC) != 0);
			}

			return true;
		}

		protected override void DoMemberTypeDependentChecks ()
		{
			base.DoMemberTypeDependentChecks ();

			if ((ModFlags & Modifiers.VOLATILE) != 0) {
				if (!CanBeVolatile ()) {
					Report.Error (677, Location, "`{0}': A volatile field cannot be of the type `{1}'",
						GetSignatureForError (), TypeManager.CSharpName (MemberType));
				}

				if ((ModFlags & Modifiers.READONLY) != 0) {
					Report.Error (678, Location, "`{0}': A field cannot be both volatile and readonly",
						GetSignatureForError ());
				}
			}
		}

		protected override bool VerifyClsCompliance ()
		{
			if (!base.VerifyClsCompliance ())
				return false;

			if ((ModFlags & Modifiers.VOLATILE) != 0) {
				Report.Warning (3026, 1, Location, "CLS-compliant field `{0}' cannot be volatile", GetSignatureForError ());
			}

			return true;
		}
	}

	//
	// `set' and `get' accessors are represented with an Accessor.
	// 
	public class Accessor {
		//
		// Null if the accessor is empty, or a Block if not
		//
		public const int AllowedModifiers = 
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.PRIVATE;
		
		public ToplevelBlock Block;
		public Attributes Attributes;
		public Location Location;
		public int ModFlags;
		public ParametersCompiled Parameters;
		
		public Accessor (ToplevelBlock b, int mod, Attributes attrs, ParametersCompiled p, Location loc)
		{
			Block = b;
			Attributes = attrs;
			Location = loc;
			Parameters = p;
			ModFlags = Modifiers.Check (AllowedModifiers, mod, 0, loc, RootContext.ToplevelTypes.Compiler.Report);
		}
	}

	// Ooouh Martin, templates are missing here.
	// When it will be possible move here a lot of child code and template method type.
	public abstract class AbstractPropertyEventMethod : MemberCore, IMethodData {
		protected MethodData method_data;
		protected ToplevelBlock block;
		protected ListDictionary declarative_security;

		// The accessor are created even if they are not wanted.
		// But we need them because their names are reserved.
		// Field says whether accessor will be emited or not
		public readonly bool IsDummy;

		protected readonly string prefix;

		ReturnParameter return_attributes;

		public AbstractPropertyEventMethod (PropertyBasedMember member, string prefix)
			: base (member.Parent, SetupName (prefix, member, member.Location), null)
		{
			this.prefix = prefix;
			IsDummy = true;
		}

		public AbstractPropertyEventMethod (InterfaceMemberBase member, Accessor accessor,
						    string prefix)
			: base (member.Parent, SetupName (prefix, member, accessor.Location),
				accessor.Attributes)
		{
			this.prefix = prefix;
			this.block = accessor.Block;
		}

		static MemberName SetupName (string prefix, InterfaceMemberBase member, Location loc)
		{
			return new MemberName (member.MemberName.Left, prefix + member.ShortName, loc);
		}

		public void UpdateName (InterfaceMemberBase member)
		{
			SetMemberName (SetupName (prefix, member, Location));
		}

		#region IMethodData Members

		public ToplevelBlock Block {
			get {
				return block;
			}

			set {
				block = value;
			}
		}

		public CallingConventions CallingConventions {
			get {
				return CallingConventions.Standard;
			}
		}

		public EmitContext CreateEmitContext (ILGenerator ig)
		{
			return new EmitContext (this, ig, ReturnType);
		}

		public bool IsExcluded ()
		{
			return false;
		}

		GenericMethod IMethodData.GenericMethod {
			get {
				return null;
			}
		}

		public MemberName MethodName {
			get {
				return MemberName;
			}
		}

		public Type[] ParameterTypes { 
			get {
				return ParameterInfo.Types;
			}
		}

		public abstract ParametersCompiled ParameterInfo { get ; }
		public abstract Type ReturnType { get; }

		#endregion

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb, PredefinedAttributes pa)
		{
			if (a.Type == pa.CLSCompliant || a.Type == pa.Obsolete || a.Type == pa.Conditional) {
				Report.Error (1667, a.Location,
					"Attribute `{0}' is not valid on property or event accessors. It is valid on `{1}' declarations only",
					TypeManager.CSharpName (a.Type), a.GetValidTargets ());
				return;
			}

			if (a.IsValidSecurityAttribute ()) {
				if (declarative_security == null)
					declarative_security = new ListDictionary ();
				a.ExtractSecurityPermissionSet (declarative_security);
				return;
			}

			if (a.Target == AttributeTargets.Method) {
				method_data.MethodBuilder.SetCustomAttribute (cb);
				return;
			}

			if (a.Target == AttributeTargets.ReturnValue) {
				if (return_attributes == null)
					return_attributes = new ReturnParameter (method_data.MethodBuilder, Location);

				return_attributes.ApplyAttributeBuilder (a, cb, pa);
				return;
			}

			ApplyToExtraTarget (a, cb, pa);
		}

		protected virtual void ApplyToExtraTarget (Attribute a, CustomAttributeBuilder cb, PredefinedAttributes pa)
		{
			throw new NotSupportedException ("You forgot to define special attribute target handling");
		}

		// It is not supported for the accessors
		public sealed override bool Define()
		{
			throw new NotSupportedException ();
		}

		public virtual void Emit (DeclSpace parent)
		{
			method_data.Emit (parent);

			if ((ModFlags & Modifiers.COMPILER_GENERATED) != 0 && !Parent.IsCompilerGenerated)
				PredefinedAttributes.Get.CompilerGenerated.EmitAttribute (method_data.MethodBuilder);
			if (((ModFlags & Modifiers.DEBUGGER_HIDDEN) != 0))
				PredefinedAttributes.Get.DebuggerHidden.EmitAttribute (method_data.MethodBuilder);

			if (TypeManager.IsDynamicType (ReturnType)) {
				return_attributes = new ReturnParameter (method_data.MethodBuilder, Location);
				return_attributes.EmitPredefined (PredefinedAttributes.Get.Dynamic, Location);
			}

			if (OptAttributes != null)
				OptAttributes.Emit ();

			if (declarative_security != null) {
				foreach (DictionaryEntry de in declarative_security) {
					method_data.MethodBuilder.AddDeclarativeSecurity ((SecurityAction)de.Key, (PermissionSet)de.Value);
				}
			}

			block = null;
		}

		public override bool EnableOverloadChecks (MemberCore overload)
		{
			// This can only happen with indexers and it will
			// be catched as indexer difference
			if (overload is AbstractPropertyEventMethod)
				return true;

			if (overload is MethodCore) {
				caching_flags |= Flags.MethodOverloadsExist;
				return true;
			}
			return false;
		}

		public override bool IsClsComplianceRequired()
		{
			return false;
		}

		public bool IsDuplicateImplementation (MethodCore method)
		{
			if (!MemberName.Equals (method.MemberName))
				return false;

			Type[] param_types = method.ParameterTypes;

			if (param_types == null || param_types.Length != ParameterTypes.Length)
				return false;

			for (int i = 0; i < param_types.Length; i++)
				if (param_types [i] != ParameterTypes [i])
					return false;

			Report.SymbolRelatedToPreviousError (method);
			Report.Error (82, Location, "A member `{0}' is already reserved",
				method.GetSignatureForError ());
			return true;
		}

		public override bool IsUsed
		{
			get {
				if (IsDummy)
					return false;

				return base.IsUsed;
			}
		}

		//
		//   Represents header string for documentation comment.
		//
		public override string DocCommentHeader {
			get { throw new InvalidOperationException ("Unexpected attempt to get doc comment from " + this.GetType () + "."); }
		}

		void IMethodData.EmitExtraSymbolInfo (SourceMethod source)
		{ }
	}

	//
	// Properties and Indexers both generate PropertyBuilders, we use this to share 
	// their common bits.
	//
	abstract public class PropertyBase : PropertyBasedMember {

		public class GetMethod : PropertyMethod
		{
			static string[] attribute_targets = new string [] { "method", "return" };

			public GetMethod (PropertyBase method):
				base (method, "get_")
			{
			}

			public GetMethod (PropertyBase method, Accessor accessor):
				base (method, accessor, "get_")
			{
			}

			public override MethodBuilder Define (DeclSpace parent)
			{
				base.Define (parent);

				if (IsDummy)
					return null;
				
				method_data = new MethodData (method, ModFlags, flags, this);

				if (!method_data.Define (parent, method.GetFullName (MemberName), Report))
					return null;

				return method_data.MethodBuilder;
			}

			public override Type ReturnType {
				get {
					return method.MemberType;
				}
			}

			public override ParametersCompiled ParameterInfo {
				get {
					return ParametersCompiled.EmptyReadOnlyParameters;
				}
			}

			public override string[] ValidAttributeTargets {
				get {
					return attribute_targets;
				}
			}
		}

		public class SetMethod : PropertyMethod {

			static string[] attribute_targets = new string [] { "method", "param", "return" };
			ImplicitParameter param_attr;
			protected ParametersCompiled parameters;

			public SetMethod (PropertyBase method) :
				base (method, "set_")
			{
				parameters = new ParametersCompiled (
					new Parameter (method.type_name, "value", Parameter.Modifier.NONE, null, Location));
			}

			public SetMethod (PropertyBase method, Accessor accessor):
				base (method, accessor, "set_")
			{
				this.parameters = accessor.Parameters;
			}

			protected override void ApplyToExtraTarget (Attribute a, CustomAttributeBuilder cb, PredefinedAttributes pa)
			{
				if (a.Target == AttributeTargets.Parameter) {
					if (param_attr == null)
						param_attr = new ImplicitParameter (method_data.MethodBuilder);

					param_attr.ApplyAttributeBuilder (a, cb, pa);
					return;
				}

				base.ApplyAttributeBuilder (a, cb, pa);
			}

			public override ParametersCompiled ParameterInfo {
			    get {
			        return parameters;
			    }
			}

			public override MethodBuilder Define (DeclSpace parent)
			{
				parameters.Resolve (this);
				
				base.Define (parent);

				if (IsDummy)
					return null;

				method_data = new MethodData (method, ModFlags, flags, this);

				if (!method_data.Define (parent, method.GetFullName (MemberName), Report))
					return null;

				return method_data.MethodBuilder;
			}

			public override Type ReturnType {
				get {
					return TypeManager.void_type;
				}
			}

			public override string[] ValidAttributeTargets {
				get {
					return attribute_targets;
				}
			}
		}

		static string[] attribute_targets = new string [] { "property" };

		public abstract class PropertyMethod : AbstractPropertyEventMethod
		{
			protected readonly PropertyBase method;
			protected MethodAttributes flags;

			public PropertyMethod (PropertyBase method, string prefix)
				: base (method, prefix)
			{
				this.method = method;
				this.ModFlags = method.ModFlags & (Modifiers.STATIC | Modifiers.UNSAFE);				
			}

			public PropertyMethod (PropertyBase method, Accessor accessor,
					       string prefix)
				: base (method, accessor, prefix)
			{
				this.method = method;
				this.ModFlags = accessor.ModFlags | (method.ModFlags & (Modifiers.STATIC | Modifiers.UNSAFE));

				if (accessor.ModFlags != 0 && RootContext.Version == LanguageVersion.ISO_1) {
					Report.FeatureIsNotAvailable (Location, "access modifiers on properties");
				}
			}

			public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb, PredefinedAttributes pa)
			{
				if (a.IsInternalMethodImplAttribute) {
					method.is_external_implementation = true;
				}

				base.ApplyAttributeBuilder (a, cb, pa);
			}

			public override AttributeTargets AttributeTargets {
				get {
					return AttributeTargets.Method;
				}
			}

			public override bool IsClsComplianceRequired ()
			{
				return method.IsClsComplianceRequired ();
			}

			public virtual MethodBuilder Define (DeclSpace parent)
			{
				CheckForDuplications ();

				if (IsDummy) {
					if (method.InterfaceType != null && parent.PartialContainer.PendingImplementations != null) {
						MethodInfo mi = parent.PartialContainer.PendingImplementations.IsInterfaceMethod (
							MethodName.Name, method.InterfaceType, new MethodData (method, ModFlags, flags, this));
						if (mi != null) {
							Report.SymbolRelatedToPreviousError (mi);
							Report.Error (551, Location, "Explicit interface implementation `{0}' is missing accessor `{1}'",
								method.GetSignatureForError (), TypeManager.CSharpSignature (mi, true));
						}
					}
					return null;
				}

				TypeContainer container = parent.PartialContainer;

				//
				// Check for custom access modifier
				//
				if ((ModFlags & Modifiers.Accessibility) == 0) {
					ModFlags |= method.ModFlags;
					flags = method.flags;
				} else {
					if (container.Kind == Kind.Interface)
						Report.Error (275, Location, "`{0}': accessibility modifiers may not be used on accessors in an interface",
							GetSignatureForError ());

					if ((method.ModFlags & Modifiers.ABSTRACT) != 0 && (ModFlags & Modifiers.PRIVATE) != 0) {
						Report.Error (442, Location, "`{0}': abstract properties cannot have private accessors", GetSignatureForError ());
					}

					CheckModifiers (ModFlags);
					ModFlags |= (method.ModFlags & (~Modifiers.Accessibility));
					ModFlags |= Modifiers.PROPERTY_CUSTOM;
					flags = Modifiers.MethodAttr (ModFlags);
					flags |= (method.flags & (~MethodAttributes.MemberAccessMask));
				}

				CheckAbstractAndExtern (block != null);
				CheckProtectedModifier ();

				if (block != null && block.IsIterator)
					Iterator.CreateIterator (this, Parent.PartialContainer, ModFlags, Compiler);

				return null;
			}

			public bool HasCustomAccessModifier {
				get {
					return (ModFlags & Modifiers.PROPERTY_CUSTOM) != 0;
				}
			}

			public PropertyBase Property {
				get {
					return method;
				}
			}

			public override ObsoleteAttribute GetObsoleteAttribute ()
			{
				return method.GetObsoleteAttribute ();
			}

			public override string GetSignatureForError()
			{
				return method.GetSignatureForError () + '.' + prefix.Substring (0, 3);
			}
			
			void CheckModifiers (int modflags)
			{
				modflags &= Modifiers.Accessibility;
				int flags = 0;
				int mflags = method.ModFlags & Modifiers.Accessibility;

				if ((mflags & Modifiers.PUBLIC) != 0) {
					flags |= Modifiers.PROTECTED | Modifiers.INTERNAL | Modifiers.PRIVATE;
				}
				else if ((mflags & Modifiers.PROTECTED) != 0) {
					if ((mflags & Modifiers.INTERNAL) != 0)
						flags |= Modifiers.PROTECTED | Modifiers.INTERNAL;

					flags |= Modifiers.PRIVATE;
				}
				else if ((mflags & Modifiers.INTERNAL) != 0)
					flags |= Modifiers.PRIVATE;

				if ((mflags == modflags) || (modflags & (~flags)) != 0) {
					Report.Error (273, Location,
						"The accessibility modifier of the `{0}' accessor must be more restrictive than the modifier of the property or indexer `{1}'",
						GetSignatureForError (), method.GetSignatureForError ());
				}
			}

			protected bool CheckForDuplications ()
			{
				if ((caching_flags & Flags.MethodOverloadsExist) == 0)
					return true;

				return Parent.MemberCache.CheckExistingMembersOverloads (this, Name, ParameterInfo, Report);
			}
		}

		public PropertyMethod Get, Set;
		public PropertyBuilder PropertyBuilder;
		public MethodBuilder GetBuilder, SetBuilder;

		protected bool define_set_first = false;

		public PropertyBase (DeclSpace parent, FullNamedExpression type, int mod_flags,
				     int allowed_mod, MemberName name,
				     Attributes attrs, bool define_set_first)
			: base (parent, null, type, mod_flags, allowed_mod, name, attrs)
		{
			 this.define_set_first = define_set_first;
		}

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb, PredefinedAttributes pa)
		{
			if (a.HasSecurityAttribute) {
				a.Error_InvalidSecurityParent ();
				return;
			}

			PropertyBuilder.SetCustomAttribute (cb);
		}

		public override AttributeTargets AttributeTargets {
			get {
				return AttributeTargets.Property;
			}
		}

		protected override void DoMemberTypeDependentChecks ()
		{
			base.DoMemberTypeDependentChecks ();

			IsTypePermitted ();
#if MS_COMPATIBLE
			if (MemberType.IsGenericParameter)
				return;
#endif

			if ((MemberType.Attributes & Class.StaticClassAttribute) == Class.StaticClassAttribute) {
				Report.Error (722, Location, Error722, TypeManager.CSharpName (MemberType));
			}
		}

		protected override void DoMemberTypeIndependentChecks ()
		{
			base.DoMemberTypeIndependentChecks ();

			//
			// Accessors modifiers check
			//
			if ((Get.ModFlags & Modifiers.Accessibility) != 0 &&
				(Set.ModFlags & Modifiers.Accessibility) != 0) {
				Report.Error (274, Location, "`{0}': Cannot specify accessibility modifiers for both accessors of the property or indexer",
						GetSignatureForError ());
			}

			if ((ModFlags & Modifiers.OVERRIDE) == 0 && 
				(Get.IsDummy && (Set.ModFlags & Modifiers.Accessibility) != 0) ||
				(Set.IsDummy && (Get.ModFlags & Modifiers.Accessibility) != 0)) {
				Report.Error (276, Location, 
					      "`{0}': accessibility modifiers on accessors may only be used if the property or indexer has both a get and a set accessor",
					      GetSignatureForError ());
			}
		}

		bool DefineGet ()
		{
			GetBuilder = Get.Define (Parent);
			return (Get.IsDummy) ? true : GetBuilder != null;
		}

		bool DefineSet (bool define)
		{
			if (!define)
				return true;

			SetBuilder = Set.Define (Parent);
			return (Set.IsDummy) ? true : SetBuilder != null;
		}

		protected bool DefineAccessors ()
		{
			return DefineSet (define_set_first) &&
				DefineGet () &&
				DefineSet (!define_set_first);
		}

		protected abstract PropertyInfo ResolveBaseProperty ();

		// TODO: rename to Resolve......
 		protected override MethodInfo FindOutBaseMethod (ref Type base_ret_type)
 		{
 			PropertyInfo base_property = ResolveBaseProperty ();
 			if (base_property == null)
 				return null;

 			base_ret_type = base_property.PropertyType;
			MethodInfo get_accessor = base_property.GetGetMethod (true);
			MethodInfo set_accessor = base_property.GetSetMethod (true);
			MethodAttributes get_accessor_access = 0, set_accessor_access = 0;

			//
			// Check base property accessors conflict
			//
			if ((ModFlags & (Modifiers.OVERRIDE | Modifiers.NEW)) == Modifiers.OVERRIDE) {
				if (get_accessor == null) {
					if (Get != null && !Get.IsDummy) {
						Report.SymbolRelatedToPreviousError (base_property);
						Report.Error (545, Location,
							"`{0}.get': cannot override because `{1}' does not have an overridable get accessor",
							GetSignatureForError (), TypeManager.GetFullNameSignature (base_property));
					}
				} else {
					get_accessor_access = get_accessor.Attributes & MethodAttributes.MemberAccessMask;

					if (!Get.IsDummy && !CheckAccessModifiers (
						Modifiers.MethodAttr (Get.ModFlags) & MethodAttributes.MemberAccessMask, get_accessor_access, get_accessor))
						Error_CannotChangeAccessModifiers (Get.Location, get_accessor, get_accessor_access, ".get");
				}

				if (set_accessor == null) {
					if (Set != null && !Set.IsDummy) {
						Report.SymbolRelatedToPreviousError (base_property);
						Report.Error (546, Location,
							"`{0}.set': cannot override because `{1}' does not have an overridable set accessor",
							GetSignatureForError (), TypeManager.GetFullNameSignature (base_property));
					}
				} else {
					set_accessor_access = set_accessor.Attributes & MethodAttributes.MemberAccessMask;

					if (!Set.IsDummy && !CheckAccessModifiers (
						Modifiers.MethodAttr (Set.ModFlags) & MethodAttributes.MemberAccessMask, set_accessor_access, set_accessor))
						Error_CannotChangeAccessModifiers (Set.Location, set_accessor, set_accessor_access, ".set");
				}
			}

			// When one accessor does not exist and property hides base one
			// we need to propagate this upwards
			if (set_accessor == null)
				set_accessor = get_accessor;

			//
			// Get the less restrictive access
			//
			return get_accessor_access > set_accessor_access ? get_accessor : set_accessor;
		}

		public override void Emit ()
		{
			//
			// The PropertyBuilder can be null for explicit implementations, in that
			// case, we do not actually emit the ".property", so there is nowhere to
			// put the attribute
			//
			if (PropertyBuilder != null) {
				if (OptAttributes != null)
					OptAttributes.Emit ();

				if (TypeManager.IsDynamicType (member_type))
					PredefinedAttributes.Get.Dynamic.EmitAttribute (PropertyBuilder);
			}

			if (!Get.IsDummy)
				Get.Emit (Parent);

			if (!Set.IsDummy)
				Set.Emit (Parent);

			base.Emit ();
		}

		/// <summary>
		/// Tests whether accessors are not in collision with some method (CS0111)
		/// </summary>
		public bool AreAccessorsDuplicateImplementation (MethodCore mc)
		{
			return Get.IsDuplicateImplementation (mc) || Set.IsDuplicateImplementation (mc);
		}

		public override bool IsUsed
		{
			get {
				if (IsExplicitImpl)
					return true;

				return Get.IsUsed | Set.IsUsed;
			}
		}

		protected override void SetMemberName (MemberName new_name)
		{
			base.SetMemberName (new_name);

			Get.UpdateName (this);
			Set.UpdateName (this);
		}

		public override string[] ValidAttributeTargets {
			get {
				return attribute_targets;
			}
		}

		//
		//   Represents header string for documentation comment.
		//
		public override string DocCommentHeader {
			get { return "P:"; }
		}
	}
			
	public class Property : PropertyBase
	{
		public sealed class BackingField : Field
		{
			readonly Property property;

			public BackingField (Property p)
				: base (p.Parent, p.type_name,
				Modifiers.BACKING_FIELD | Modifiers.COMPILER_GENERATED | Modifiers.PRIVATE | (p.ModFlags & (Modifiers.STATIC | Modifiers.UNSAFE)),
				new MemberName ("<" + p.GetFullName (p.MemberName) + ">k__BackingField", p.Location), null)
			{
				this.property = p;
			}

			public override string GetSignatureForError ()
			{
				return property.GetSignatureForError ();
			}
		}

		const int AllowedModifiers =
			Modifiers.NEW |
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.PRIVATE |
			Modifiers.STATIC |
			Modifiers.SEALED |
			Modifiers.OVERRIDE |
			Modifiers.ABSTRACT |
			Modifiers.UNSAFE |
			Modifiers.EXTERN |
			Modifiers.VIRTUAL;

		const int AllowedInterfaceModifiers =
			Modifiers.NEW;

		public Property (DeclSpace parent, FullNamedExpression type, int mod,
				 MemberName name, Attributes attrs, Accessor get_block,
				 Accessor set_block, bool define_set_first)
			: this (parent, type, mod, name, attrs, get_block, set_block,
				define_set_first, null)
		{
		}
		
		public Property (DeclSpace parent, FullNamedExpression type, int mod,
				 MemberName name, Attributes attrs, Accessor get_block,
				 Accessor set_block, bool define_set_first, Block current_block)
			: base (parent, type, mod,
				parent.PartialContainer.Kind == Kind.Interface ? AllowedInterfaceModifiers : AllowedModifiers,
				name, attrs, define_set_first)
		{
			if (get_block == null)
				Get = new GetMethod (this);
			else
				Get = new GetMethod (this, get_block);

			if (set_block == null)
				Set = new SetMethod (this);
			else
				Set = new SetMethod (this, set_block);

			if (!IsInterface && (mod & (Modifiers.ABSTRACT | Modifiers.EXTERN)) == 0 &&
				get_block != null && get_block.Block == null &&
				set_block != null && set_block.Block == null) {
				if (RootContext.Version <= LanguageVersion.ISO_2)
					Report.FeatureIsNotAvailable (Location, "automatically implemented properties");

				Get.ModFlags |= Modifiers.COMPILER_GENERATED;
				Set.ModFlags |= Modifiers.COMPILER_GENERATED;
			}
		}

		void CreateAutomaticProperty ()
		{
			// Create backing field
			Field field = new BackingField (this);
			if (!field.Define ())
				return;

			Parent.PartialContainer.AddField (field);

			FieldExpr fe = new FieldExpr (field.FieldBuilder, Location);
			if ((field.ModFlags & Modifiers.STATIC) == 0)
				fe.InstanceExpression = new CompilerGeneratedThis (fe.Type, Location);

			// Create get block
			Get.Block = new ToplevelBlock (Compiler, ParametersCompiled.EmptyReadOnlyParameters, Location);
			Return r = new Return (fe, Location);
			Get.Block.AddStatement (r);

			// Create set block
			Set.Block = new ToplevelBlock (Compiler, Set.ParameterInfo, Location);
			Assign a = new SimpleAssign (fe, new SimpleName ("value", Location));
			Set.Block.AddStatement (new StatementExpression (a));
		}

		public override bool Define ()
		{
			if (!base.Define ())
				return false;

			flags |= MethodAttributes.HideBySig | MethodAttributes.SpecialName;

			if ((Get.ModFlags & Modifiers.COMPILER_GENERATED) != 0)
				CreateAutomaticProperty ();

			if (!DefineAccessors ())
				return false;

			if (!CheckBase ())
				return false;

			// FIXME - PropertyAttributes.HasDefault ?

			PropertyBuilder = Parent.TypeBuilder.DefineProperty (
				GetFullName (MemberName), PropertyAttributes.None, MemberType, null);

			if (!Get.IsDummy) {
				PropertyBuilder.SetGetMethod (GetBuilder);
				Parent.MemberCache.AddMember (GetBuilder, Get);
			}

			if (!Set.IsDummy) {
				PropertyBuilder.SetSetMethod (SetBuilder);
				Parent.MemberCache.AddMember (SetBuilder, Set);
			}
			
			TypeManager.RegisterProperty (PropertyBuilder, this);
			Parent.MemberCache.AddMember (PropertyBuilder, this);
			return true;
		}

		public override void Emit ()
		{
			if (((Set.ModFlags | Get.ModFlags) & (Modifiers.STATIC | Modifiers.COMPILER_GENERATED)) == Modifiers.COMPILER_GENERATED && Parent.PartialContainer.HasExplicitLayout) {
				Report.Error (842, Location,
					"Automatically implemented property `{0}' cannot be used inside a type with an explicit StructLayout attribute",
					GetSignatureForError ());
			}

			base.Emit ();
		}

		protected override PropertyInfo ResolveBaseProperty ()
		{
			return Parent.PartialContainer.BaseCache.FindMemberToOverride (
				Parent.TypeBuilder, Name, ParametersCompiled.EmptyReadOnlyParameters, null, true) as PropertyInfo;
		}
	}

	/// </summary>
	///  Gigantic workaround  for lameness in SRE follows :
	///  This class derives from EventInfo and attempts to basically
	///  wrap around the EventBuilder so that FindMembers can quickly
	///  return this in it search for members
	/// </summary>
	public class MyEventBuilder : EventInfo {
		
		//
		// We use this to "point" to our Builder which is
		// not really a MemberInfo
		//
		EventBuilder MyBuilder;
		
		//
		// We "catch" and wrap these methods
		//
		MethodInfo raise, remove, add;

		EventAttributes attributes;
		Type declaring_type, reflected_type, event_type;
		string name;

		Event my_event;

		public MyEventBuilder (Event ev, TypeBuilder type_builder, string name, EventAttributes event_attr, Type event_type)
		{
			MyBuilder = type_builder.DefineEvent (name, event_attr, event_type);

			// And now store the values in our own fields.
			
			declaring_type = type_builder;

			reflected_type = type_builder;
			
			attributes = event_attr;
			this.name = name;
			my_event = ev;
			this.event_type = event_type;
		}
		
		//
		// Methods that you have to override.  Note that you only need 
		// to "implement" the variants that take the argument (those are
		// the "abstract" methods, the others (GetAddMethod()) are 
		// regular.
		//
		public override MethodInfo GetAddMethod (bool nonPublic)
		{
			return add;
		}
		
		public override MethodInfo GetRemoveMethod (bool nonPublic)
		{
			return remove;
		}
		
		public override MethodInfo GetRaiseMethod (bool nonPublic)
		{
			return raise;
		}
		
		//
		// These methods make "MyEventInfo" look like a Builder
		//
		public void SetRaiseMethod (MethodBuilder raiseMethod)
		{
			raise = raiseMethod;
			MyBuilder.SetRaiseMethod (raiseMethod);
		}

		public void SetRemoveOnMethod (MethodBuilder removeMethod)
		{
			remove = removeMethod;
			MyBuilder.SetRemoveOnMethod (removeMethod);
		}

		public void SetAddOnMethod (MethodBuilder addMethod)
		{
			add = addMethod;
			MyBuilder.SetAddOnMethod (addMethod);
		}

		public void SetCustomAttribute (CustomAttributeBuilder cb)
		{
			MyBuilder.SetCustomAttribute (cb);
		}
		
		public override object [] GetCustomAttributes (bool inherit)
		{
			// FIXME : There's nothing which can be seemingly done here because
			// we have no way of getting at the custom attribute objects of the
			// EventBuilder !
			return null;
		}

		public override object [] GetCustomAttributes (Type t, bool inherit)
		{
			// FIXME : Same here !
			return null;
		}

		public override bool IsDefined (Type t, bool b)
		{
			return true;
		}

		public override EventAttributes Attributes {
			get {
				return attributes;
			}
		}

		public override string Name {
			get {
				return name;
			}
		}

		public override Type DeclaringType {
			get {
				return declaring_type;
			}
		}

		public override Type ReflectedType {
			get {
				return reflected_type;
			}
		}

		public Type EventType {
			get {
				return event_type;
			}
		}
		
		public void SetUsed ()
		{
			if (my_event != null) {
//				my_event.SetAssigned ();
				my_event.SetMemberIsUsed ();
			}
		}
	}
	
	/// <summary>
	/// For case when event is declared like property (with add and remove accessors).
	/// </summary>
	public class EventProperty: Event {
		abstract class AEventPropertyAccessor : AEventAccessor
		{
			protected AEventPropertyAccessor (Event method, Accessor accessor, string prefix):
				base (method, accessor, prefix)
			{
			}

			public override MethodBuilder Define (DeclSpace ds)
			{
				CheckAbstractAndExtern (block != null);
				return base.Define (ds);
			}
			
			public override string GetSignatureForError ()
			{
				return method.GetSignatureForError () + "." + prefix.Substring (0, prefix.Length - 1);
			}
		}

		sealed class AddDelegateMethod: AEventPropertyAccessor
		{
			public AddDelegateMethod (Event method, Accessor accessor):
				base (method, accessor, AddPrefix)
			{
			}
		}

		sealed class RemoveDelegateMethod: AEventPropertyAccessor
		{
			public RemoveDelegateMethod (Event method, Accessor accessor):
				base (method, accessor, RemovePrefix)
			{
			}
		}


		static readonly string[] attribute_targets = new string [] { "event" }; // "property" target was disabled for 2.0 version

		public EventProperty (DeclSpace parent, FullNamedExpression type, int mod_flags,
				      MemberName name,
				      Attributes attrs, Accessor add, Accessor remove)
			: base (parent, type, mod_flags, name, attrs)
		{
			Add = new AddDelegateMethod (this, add);
			Remove = new RemoveDelegateMethod (this, remove);
		}

		public override bool Define()
		{
			if (!base.Define ())
				return false;

			SetMemberIsUsed ();
			return true;
		}

		public override string[] ValidAttributeTargets {
			get {
				return attribute_targets;
			}
		}
	}

	/// <summary>
	/// Event is declared like field.
	/// </summary>
	public class EventField : Event {
		abstract class EventFieldAccessor : AEventAccessor
		{
			protected EventFieldAccessor (Event method, string prefix)
				: base (method, prefix)
			{
			}

			public override void Emit (DeclSpace parent)
			{
				if ((method.ModFlags & (Modifiers.ABSTRACT | Modifiers.EXTERN)) == 0) {
					if (parent is Class) {
						MethodBuilder mb = method_data.MethodBuilder;
						mb.SetImplementationFlags (mb.GetMethodImplementationFlags () | MethodImplAttributes.Synchronized);
					}

					// TODO: because we cannot use generics yet
					FieldInfo field_info = ((EventField) method).BackingField.FieldBuilder;
					FieldExpr f_expr = new FieldExpr (field_info, Location);
					if ((method.ModFlags & Modifiers.STATIC) == 0)
						f_expr.InstanceExpression = new CompilerGeneratedThis (field_info.FieldType, Location);

					block = new ToplevelBlock (Compiler, ParameterInfo, Location);
					block.AddStatement (new StatementExpression (
						new CompoundAssign (Operation,
							f_expr,
							block.GetParameterReference (ParameterInfo[0].Name, Location))));
				}

				base.Emit (parent);
			}

			protected abstract Binary.Operator Operation { get; }
		}

		sealed class AddDelegateMethod: EventFieldAccessor
		{
			public AddDelegateMethod (Event method):
				base (method, AddPrefix)
			{
			}

			protected override Binary.Operator Operation {
				get { return Binary.Operator.Addition; }
			}
		}

		sealed class RemoveDelegateMethod: EventFieldAccessor
		{
			public RemoveDelegateMethod (Event method):
				base (method, RemovePrefix)
			{
			}

			protected override Binary.Operator Operation {
				get { return Binary.Operator.Subtraction; }
			}
		}


		static readonly string[] attribute_targets = new string [] { "event", "field", "method" };
		static readonly string[] attribute_targets_interface = new string[] { "event", "method" };

		public Field BackingField;
		public Expression Initializer;

		public EventField (DeclSpace parent, FullNamedExpression type, int mod_flags, MemberName name, Attributes attrs)
			: base (parent, type, mod_flags, name, attrs)
		{
			Add = new AddDelegateMethod (this);
			Remove = new RemoveDelegateMethod (this);
		}

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb, PredefinedAttributes pa)
		{
			if (a.Target == AttributeTargets.Field) {
				BackingField.ApplyAttributeBuilder (a, cb, pa);
				return;
			}

			if (a.Target == AttributeTargets.Method) {
				int errors = Report.Errors;
				Add.ApplyAttributeBuilder (a, cb, pa);
				if (errors == Report.Errors)
					Remove.ApplyAttributeBuilder (a, cb, pa);
				return;
			}

			base.ApplyAttributeBuilder (a, cb, pa);
		}

		public override bool Define()
		{
			if (!base.Define ())
				return false;

			if (Initializer != null && (ModFlags & Modifiers.ABSTRACT) != 0) {
				Report.Error (74, Location, "`{0}': abstract event cannot have an initializer",
					GetSignatureForError ());
			}

			if (!HasBackingField) {
				SetMemberIsUsed ();
				return true;
			}

			// FIXME: We are unable to detect whether generic event is used because
			// we are using FieldExpr instead of EventExpr for event access in that
			// case.  When this issue will be fixed this hack can be removed.
			if (TypeManager.IsGenericType (MemberType))
				SetMemberIsUsed ();

			if (Add.IsInterfaceImplementation)
				SetMemberIsUsed ();

			TypeManager.RegisterEventField (EventBuilder, this);

			BackingField = new Field (Parent,
				new TypeExpression (MemberType, Location),
				Modifiers.BACKING_FIELD | Modifiers.COMPILER_GENERATED | Modifiers.PRIVATE | (ModFlags & (Modifiers.STATIC | Modifiers.UNSAFE)),
				MemberName, null);

			Parent.PartialContainer.AddField (BackingField);
			BackingField.Initializer = Initializer;
			BackingField.ModFlags &= ~Modifiers.COMPILER_GENERATED;

			// Call define because we passed fields definition
			return BackingField.Define ();
		}

		bool HasBackingField {
			get {
				return !IsInterface && (ModFlags & Modifiers.ABSTRACT) == 0;
			}
		}

		public override string[] ValidAttributeTargets 
		{
			get {
				return HasBackingField ? attribute_targets : attribute_targets_interface;
			}
		}
	}

	public abstract class Event : PropertyBasedMember {
		public abstract class AEventAccessor : AbstractPropertyEventMethod
		{
			protected readonly Event method;
			ImplicitParameter param_attr;

			static readonly string[] attribute_targets = new string [] { "method", "param", "return" };

			public const string AddPrefix = "add_";
			public const string RemovePrefix = "remove_";

			protected AEventAccessor (Event method, string prefix)
				: base (method, prefix)
			{
				this.method = method;
				this.ModFlags = method.ModFlags;
			}

			protected AEventAccessor (Event method, Accessor accessor, string prefix)
				: base (method, accessor, prefix)
			{
				this.method = method;
				this.ModFlags = method.ModFlags;
			}

			public bool IsInterfaceImplementation {
				get { return method_data.implementing != null; }
			}

			public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb, PredefinedAttributes pa)
			{
				if (a.IsInternalMethodImplAttribute) {
					method.is_external_implementation = true;
				}

				base.ApplyAttributeBuilder (a, cb, pa);
			}

			protected override void ApplyToExtraTarget (Attribute a, CustomAttributeBuilder cb, PredefinedAttributes pa)
			{
				if (a.Target == AttributeTargets.Parameter) {
					if (param_attr == null)
						param_attr = new ImplicitParameter (method_data.MethodBuilder);

					param_attr.ApplyAttributeBuilder (a, cb, pa);
					return;
				}

				base.ApplyAttributeBuilder (a, cb, pa);
			}

			public override AttributeTargets AttributeTargets {
				get {
					return AttributeTargets.Method;
				}
			}

			public override bool IsClsComplianceRequired ()
			{
				return method.IsClsComplianceRequired ();
			}

			public virtual MethodBuilder Define (DeclSpace parent)
			{
				method_data = new MethodData (method, method.ModFlags,
					method.flags | MethodAttributes.HideBySig | MethodAttributes.SpecialName, this);

				if (!method_data.Define (parent, method.GetFullName (MemberName), Report))
					return null;

				MethodBuilder mb = method_data.MethodBuilder;
				ParameterInfo.ApplyAttributes (mb);
				return mb;
			}

			public override Type ReturnType {
				get {
					return TypeManager.void_type;
				}
			}

			public override ObsoleteAttribute GetObsoleteAttribute ()
			{
				return method.GetObsoleteAttribute ();
			}

			public override string[] ValidAttributeTargets {
				get {
					return attribute_targets;
				}
			}

			public override ParametersCompiled ParameterInfo {
				get {
					return method.parameters;
				}
			}
		}


		const int AllowedModifiers =
			Modifiers.NEW |
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.PRIVATE |
			Modifiers.STATIC |
			Modifiers.VIRTUAL |
			Modifiers.SEALED |
			Modifiers.OVERRIDE |
			Modifiers.UNSAFE |
			Modifiers.ABSTRACT |
			Modifiers.EXTERN;

		const int AllowedInterfaceModifiers =
			Modifiers.NEW;

		public AEventAccessor Add, Remove;
		public MyEventBuilder     EventBuilder;
		public MethodBuilder AddBuilder, RemoveBuilder;

		ParametersCompiled parameters;

		protected Event (DeclSpace parent, FullNamedExpression type, int mod_flags, MemberName name, Attributes attrs)
			: base (parent, null, type, mod_flags,
				parent.PartialContainer.Kind == Kind.Interface ? AllowedInterfaceModifiers : AllowedModifiers,
				name, attrs)
		{
		}

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb, PredefinedAttributes pa)
		{
			if ((a.HasSecurityAttribute)) {
				a.Error_InvalidSecurityParent ();
				return;
			}
			
			EventBuilder.SetCustomAttribute (cb);
		}

		public bool AreAccessorsDuplicateImplementation (MethodCore mc)
		{
			return Add.IsDuplicateImplementation (mc) || Remove.IsDuplicateImplementation (mc);
		}

		public override AttributeTargets AttributeTargets {
			get {
				return AttributeTargets.Event;
			}
		}

		public override bool Define ()
		{
			if (!base.Define ())
				return false;

			if (!TypeManager.IsDelegateType (MemberType)) {
				Report.Error (66, Location, "`{0}': event must be of a delegate type", GetSignatureForError ());
			}

			parameters = ParametersCompiled.CreateFullyResolved (
				new Parameter (null, "value", Parameter.Modifier.NONE, null, Location), MemberType);

			if (!CheckBase ())
				return false;

			if (TypeManager.delegate_combine_delegate_delegate == null) {
				TypeManager.delegate_combine_delegate_delegate = TypeManager.GetPredefinedMethod (
					TypeManager.delegate_type, "Combine", Location,
					TypeManager.delegate_type, TypeManager.delegate_type);
			}
			if (TypeManager.delegate_remove_delegate_delegate == null) {
				TypeManager.delegate_remove_delegate_delegate = TypeManager.GetPredefinedMethod (
					TypeManager.delegate_type, "Remove", Location,
					TypeManager.delegate_type, TypeManager.delegate_type);
			}

			//
			// Now define the accessors
			//

			AddBuilder = Add.Define (Parent);
			if (AddBuilder == null)
				return false;

			RemoveBuilder = Remove.Define (Parent);
			if (RemoveBuilder == null)
				return false;

			EventBuilder = new MyEventBuilder (this, Parent.TypeBuilder, Name, EventAttributes.None, MemberType);						
			EventBuilder.SetAddOnMethod (AddBuilder);
			EventBuilder.SetRemoveOnMethod (RemoveBuilder);

			Parent.MemberCache.AddMember (EventBuilder, this);
			Parent.MemberCache.AddMember (AddBuilder, Add);
			Parent.MemberCache.AddMember (RemoveBuilder, Remove);
			
			return true;
		}

		public override void Emit ()
		{
			if (OptAttributes != null) {
				OptAttributes.Emit ();
			}

			Add.Emit (Parent);
			Remove.Emit (Parent);

			base.Emit ();
		}

		protected override MethodInfo FindOutBaseMethod (ref Type base_ret_type)
		{
			MethodInfo mi = (MethodInfo) Parent.PartialContainer.BaseCache.FindBaseEvent (
				Parent.TypeBuilder, Name);

			if (mi == null)
				return null;

			AParametersCollection pd = TypeManager.GetParameterData (mi);
			base_ret_type = pd.Types [0];
			return mi;
		}

		//
		//   Represents header string for documentation comment.
		//
		public override string DocCommentHeader {
			get { return "E:"; }
		}
	}

 
	public class Indexer : PropertyBase
	{
		public class GetIndexerMethod : GetMethod
		{
			ParametersCompiled parameters;

			public GetIndexerMethod (Indexer method):
				base (method)
			{
				this.parameters = method.parameters;
			}

			public GetIndexerMethod (PropertyBase method, Accessor accessor):
				base (method, accessor)
			{
				parameters = accessor.Parameters;
			}

			public override MethodBuilder Define (DeclSpace parent)
			{
				parameters.Resolve (this);
				return base.Define (parent);
			}
			
			public override bool EnableOverloadChecks (MemberCore overload)
			{
				if (base.EnableOverloadChecks (overload)) {
					overload.caching_flags |= Flags.MethodOverloadsExist;
					return true;
				}

				return false;
			}			

			public override ParametersCompiled ParameterInfo {
				get {
					return parameters;
				}
			}
		}

		public class SetIndexerMethod: SetMethod
		{
			public SetIndexerMethod (Indexer method):
				base (method)
			{
				parameters = ParametersCompiled.MergeGenerated (method.parameters, false, parameters [0], null);
			}

			public SetIndexerMethod (PropertyBase method, Accessor accessor):
				base (method, accessor)
			{
				parameters = method.Get.IsDummy ? accessor.Parameters : accessor.Parameters.Clone ();			
			}

			public override bool EnableOverloadChecks (MemberCore overload)
			{
				if (base.EnableOverloadChecks (overload)) {
					overload.caching_flags |= Flags.MethodOverloadsExist;
					return true;
				}

				return false;
			}
		}

		const int AllowedModifiers =
			Modifiers.NEW |
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.PRIVATE |
			Modifiers.VIRTUAL |
			Modifiers.SEALED |
			Modifiers.OVERRIDE |
			Modifiers.UNSAFE |
			Modifiers.EXTERN |
			Modifiers.ABSTRACT;

		const int AllowedInterfaceModifiers =
			Modifiers.NEW;

		public readonly ParametersCompiled parameters;

		public Indexer (DeclSpace parent, FullNamedExpression type, MemberName name, int mod,
				ParametersCompiled parameters, Attributes attrs,
				Accessor get_block, Accessor set_block, bool define_set_first)
			: base (parent, type, mod,
				parent.PartialContainer.Kind == Kind.Interface ? AllowedInterfaceModifiers : AllowedModifiers,
				name, attrs, define_set_first)
		{
			this.parameters = parameters;

			if (get_block == null)
				Get = new GetIndexerMethod (this);
			else
				Get = new GetIndexerMethod (this, get_block);

			if (set_block == null)
				Set = new SetIndexerMethod (this);
			else
				Set = new SetIndexerMethod (this, set_block);
		}

		protected override bool CheckForDuplications ()
		{
			return Parent.MemberCache.CheckExistingMembersOverloads (this, GetFullName (MemberName), parameters, Report);
		}
		
		public override bool Define ()
		{
			if (!base.Define ())
				return false;

			if (!DefineParameters (parameters))
				return false;

			if (OptAttributes != null) {
				Attribute indexer_attr = OptAttributes.Search (PredefinedAttributes.Get.IndexerName);
				if (indexer_attr != null) {
					// Remove the attribute from the list because it is not emitted
					OptAttributes.Attrs.Remove (indexer_attr);

					string name = indexer_attr.GetIndexerAttributeValue ();
					if (name == null)
						return false;

					ShortName = name;

					if (IsExplicitImpl) {
						Report.Error (415, indexer_attr.Location,
							      "The `IndexerName' attribute is valid only on an " +
							      "indexer that is not an explicit interface member declaration");
						return false;
					}

					if ((ModFlags & Modifiers.OVERRIDE) != 0) {
						Report.Error (609, indexer_attr.Location,
							      "Cannot set the `IndexerName' attribute on an indexer marked override");
						return false;
					}
				}
			}

			if (InterfaceType != null) {
				string base_IndexerName = TypeManager.IndexerPropertyName (InterfaceType);
				if (base_IndexerName != Name)
					ShortName = base_IndexerName;
			}

			if (!Parent.PartialContainer.AddMember (this) ||
				!Parent.PartialContainer.AddMember (Get) || !Parent.PartialContainer.AddMember (Set))
				return false;

			flags |= MethodAttributes.HideBySig | MethodAttributes.SpecialName;
			
			if (!DefineAccessors ())
				return false;

			if (!CheckBase ())
				return false;

			//
			// Now name the parameters
			//
			PropertyBuilder = Parent.TypeBuilder.DefineProperty (
				GetFullName (MemberName), PropertyAttributes.None, MemberType, parameters.GetEmitTypes ());

			if (!Get.IsDummy) {
				PropertyBuilder.SetGetMethod (GetBuilder);
				Parent.MemberCache.AddMember (GetBuilder, Get);
			}

			if (!Set.IsDummy) {
				PropertyBuilder.SetSetMethod (SetBuilder);
				Parent.MemberCache.AddMember (SetBuilder, Set);
			}
				
			TypeManager.RegisterIndexer (PropertyBuilder, parameters);
			Parent.MemberCache.AddMember (PropertyBuilder, this);
			return true;
		}

		public override bool EnableOverloadChecks (MemberCore overload)
		{
			if (overload is Indexer) {
				caching_flags |= Flags.MethodOverloadsExist;
				return true;
			}

			return base.EnableOverloadChecks (overload);
		}

		public override string GetDocCommentName (DeclSpace ds)
		{
			return DocUtil.GetMethodDocCommentName (this, parameters, ds);
		}

		public override string GetSignatureForError ()
		{
			StringBuilder sb = new StringBuilder (Parent.GetSignatureForError ());
			if (MemberName.Left != null) {
				sb.Append ('.');
				sb.Append (MemberName.Left.GetSignatureForError ());
			}

			sb.Append (".this");
			sb.Append (parameters.GetSignatureForError ().Replace ('(', '[').Replace (')', ']'));
			return sb.ToString ();
		}

		protected override PropertyInfo ResolveBaseProperty ()
		{
			return Parent.PartialContainer.BaseCache.FindMemberToOverride (
				Parent.TypeBuilder, Name, parameters, null, true) as PropertyInfo;
		}

		protected override bool VerifyClsCompliance ()
		{
			if (!base.VerifyClsCompliance ())
				return false;

			parameters.VerifyClsCompliance ();
			return true;
		}
	}

	public class Operator : MethodOrOperator {

		const int AllowedModifiers =
			Modifiers.PUBLIC |
			Modifiers.UNSAFE |
			Modifiers.EXTERN |
			Modifiers.STATIC;

		public enum OpType : byte {

			// Unary operators
			LogicalNot,
			OnesComplement,
			Increment,
			Decrement,
			True,
			False,

			// Unary and Binary operators
			Addition,
			Subtraction,

			UnaryPlus,
			UnaryNegation,
			
			// Binary operators
			Multiply,
			Division,
			Modulus,
			BitwiseAnd,
			BitwiseOr,
			ExclusiveOr,
			LeftShift,
			RightShift,
			Equality,
			Inequality,
			GreaterThan,
			LessThan,
			GreaterThanOrEqual,
			LessThanOrEqual,

			// Implicit and Explicit
			Implicit,
			Explicit,

			// Just because of enum
			TOP
		};

		public readonly OpType OperatorType;

		static readonly string [] [] names;

		static Operator ()
		{
			names = new string[(int)OpType.TOP][];
			names [(int) OpType.LogicalNot] = new string [] { "!", "op_LogicalNot" };
			names [(int) OpType.OnesComplement] = new string [] { "~", "op_OnesComplement" };
			names [(int) OpType.Increment] = new string [] { "++", "op_Increment" };
			names [(int) OpType.Decrement] = new string [] { "--", "op_Decrement" };
			names [(int) OpType.True] = new string [] { "true", "op_True" };
			names [(int) OpType.False] = new string [] { "false", "op_False" };
			names [(int) OpType.Addition] = new string [] { "+", "op_Addition" };
			names [(int) OpType.Subtraction] = new string [] { "-", "op_Subtraction" };
			names [(int) OpType.UnaryPlus] = new string [] { "+", "op_UnaryPlus" };
			names [(int) OpType.UnaryNegation] = new string [] { "-", "op_UnaryNegation" };
			names [(int) OpType.Multiply] = new string [] { "*", "op_Multiply" };
			names [(int) OpType.Division] = new string [] { "/", "op_Division" };
			names [(int) OpType.Modulus] = new string [] { "%", "op_Modulus" };
			names [(int) OpType.BitwiseAnd] = new string [] { "&", "op_BitwiseAnd" };
			names [(int) OpType.BitwiseOr] = new string [] { "|", "op_BitwiseOr" };
			names [(int) OpType.ExclusiveOr] = new string [] { "^", "op_ExclusiveOr" };
			names [(int) OpType.LeftShift] = new string [] { "<<", "op_LeftShift" };
			names [(int) OpType.RightShift] = new string [] { ">>", "op_RightShift" };
			names [(int) OpType.Equality] = new string [] { "==", "op_Equality" };
			names [(int) OpType.Inequality] = new string [] { "!=", "op_Inequality" };
			names [(int) OpType.GreaterThan] = new string [] { ">", "op_GreaterThan" };
			names [(int) OpType.LessThan] = new string [] { "<", "op_LessThan" };
			names [(int) OpType.GreaterThanOrEqual] = new string [] { ">=", "op_GreaterThanOrEqual" };
			names [(int) OpType.LessThanOrEqual] = new string [] { "<=", "op_LessThanOrEqual" };
			names [(int) OpType.Implicit] = new string [] { "implicit", "op_Implicit" };
			names [(int) OpType.Explicit] = new string [] { "explicit", "op_Explicit" };
		}
		
		public Operator (DeclSpace parent, OpType type, FullNamedExpression ret_type,
				 int mod_flags, ParametersCompiled parameters,
				 ToplevelBlock block, Attributes attrs, Location loc)
			: base (parent, null, ret_type, mod_flags, AllowedModifiers,
				new MemberName (GetMetadataName (type), loc), attrs, parameters)
		{
			OperatorType = type;
			Block = block;
		}

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb, PredefinedAttributes pa)
		{
			if (a.Type == pa.Conditional) {
				Error_ConditionalAttributeIsNotValid ();
				return;
			}

			base.ApplyAttributeBuilder (a, cb, pa);
		}
		
		public override bool Define ()
		{
			const int RequiredModifiers = Modifiers.PUBLIC | Modifiers.STATIC;
			if ((ModFlags & RequiredModifiers) != RequiredModifiers){
				Report.Error (558, Location, "User-defined operator `{0}' must be declared static and public", GetSignatureForError ());
			}

			if (!base.Define ())
				return false;

			// imlicit and explicit operator of same types are not allowed
			if (OperatorType == OpType.Explicit)
				Parent.MemberCache.CheckExistingMembersOverloads (this, GetMetadataName (OpType.Implicit), Parameters, Report);
			else if (OperatorType == OpType.Implicit)
				Parent.MemberCache.CheckExistingMembersOverloads (this, GetMetadataName (OpType.Explicit), Parameters, Report);

			Type declaring_type = MethodData.DeclaringType;
			Type return_type = MemberType;
			Type first_arg_type = ParameterTypes [0];
			
			Type first_arg_type_unwrap = first_arg_type;
			if (TypeManager.IsNullableType (first_arg_type))
				first_arg_type_unwrap = TypeManager.TypeToCoreType (TypeManager.GetTypeArguments (first_arg_type) [0]);
			
			Type return_type_unwrap = return_type;
			if (TypeManager.IsNullableType (return_type))
				return_type_unwrap = TypeManager.TypeToCoreType (TypeManager.GetTypeArguments (return_type) [0]);

			if (TypeManager.IsDynamicType (return_type) || TypeManager.IsDynamicType (first_arg_type)) {
				Report.Error (1964, Location,
					"User-defined operator `{0}' cannot convert to or from the dynamic type",
					GetSignatureForError ());

				return false;
			}

			//
			// Rules for conversion operators
			//
			if (OperatorType == OpType.Implicit || OperatorType == OpType.Explicit) {
				if (first_arg_type_unwrap == return_type_unwrap && first_arg_type_unwrap == declaring_type){
					Report.Error (555, Location,
						"User-defined operator cannot take an object of the enclosing type and convert to an object of the enclosing type");
					return false;
				}
				
				Type conv_type;
				if (TypeManager.IsEqual (declaring_type, return_type) || declaring_type == return_type_unwrap) {
					conv_type = first_arg_type;
				} else if (TypeManager.IsEqual (declaring_type, first_arg_type) || declaring_type == first_arg_type_unwrap) {
					conv_type = return_type;
				} else {
					Report.Error (556, Location, 
						"User-defined conversion must convert to or from the enclosing type");
					return false;
				}

				//
				// Because IsInterface and IsClass are not supported
				//
				if (!TypeManager.IsGenericParameter (conv_type)) {
					if (conv_type.IsInterface) {
						Report.Error (552, Location, "User-defined conversion `{0}' cannot convert to or from an interface type",
							GetSignatureForError ());
						return false;
					}

					if (conv_type.IsClass) {
						if (TypeManager.IsSubclassOf (declaring_type, conv_type)) {
							Report.Error (553, Location, "User-defined conversion `{0}' cannot convert to or from a base class",
								GetSignatureForError ());
							return false;
						}

						if (TypeManager.IsSubclassOf (conv_type, declaring_type)) {
							Report.Error (554, Location, "User-defined conversion `{0}' cannot convert to or from a derived class",
								GetSignatureForError ());
							return false;
						}
					}
				}
			} else if (OperatorType == OpType.LeftShift || OperatorType == OpType.RightShift) {
				if (first_arg_type != declaring_type || Parameters.Types [1] != TypeManager.int32_type) {
					Report.Error (564, Location, "Overloaded shift operator must have the type of the first operand be the containing type, and the type of the second operand must be int");
					return false;
				}
			} else if (Parameters.Count == 1) {
				// Checks for Unary operators

				if (OperatorType == OpType.Increment || OperatorType == OpType.Decrement) {
					if (return_type != declaring_type && !TypeManager.IsSubclassOf (return_type, declaring_type)) {
						Report.Error (448, Location,
							"The return type for ++ or -- operator must be the containing type or derived from the containing type");
						return false;
					}
					if (first_arg_type != declaring_type) {
						Report.Error (
							559, Location, "The parameter type for ++ or -- operator must be the containing type");
						return false;
					}
				}
				
				if (!TypeManager.IsEqual (first_arg_type_unwrap, declaring_type)){
					Report.Error (562, Location,
						"The parameter type of a unary operator must be the containing type");
					return false;
				}
				
				if (OperatorType == OpType.True || OperatorType == OpType.False) {
					if (return_type != TypeManager.bool_type){
						Report.Error (
							215, Location,
							"The return type of operator True or False " +
							"must be bool");
						return false;
					}
				}
				
			} else {
				// Checks for Binary operators
				
				if (first_arg_type != declaring_type &&
				    Parameters.Types [1] != declaring_type){
					Report.Error (
						563, Location,
						"One of the parameters of a binary operator must " +
						"be the containing type");
					return false;
				}
			}

			return true;
		}

		protected override bool ResolveMemberType ()
		{
			if (!base.ResolveMemberType ())
				return false;

			flags |= MethodAttributes.SpecialName | MethodAttributes.HideBySig;
			return true;
		}

		// Operator cannot be override
		protected override MethodInfo FindOutBaseMethod (ref Type base_ret_type)
		{
			return null;
		}

		public static string GetName (OpType ot)
		{
			return names [(int) ot] [0];
		}

		public static string GetName (string metadata_name)
		{
			for (int i = 0; i < names.Length; ++i) {
				if (names [i] [1] == metadata_name)
					return names [i] [0];
			}
			return null;
		}

		public static string GetMetadataName (OpType ot)
		{
			return names [(int) ot] [1];
		}

		public static string GetMetadataName (string name)
		{
			for (int i = 0; i < names.Length; ++i) {
				if (names [i] [0] == name)
					return names [i] [1];
			}
			return null;
		}

		public OpType GetMatchingOperator ()
		{
			switch (OperatorType) {
			case OpType.Equality:
				return OpType.Inequality;
			case OpType.Inequality:
				return OpType.Equality;
			case OpType.True:
				return OpType.False;
			case OpType.False:
				return OpType.True;
			case OpType.GreaterThan:
				return OpType.LessThan;
			case OpType.LessThan:
				return OpType.GreaterThan;
			case OpType.GreaterThanOrEqual:
				return OpType.LessThanOrEqual;
			case OpType.LessThanOrEqual:
				return OpType.GreaterThanOrEqual;
			default:
				return OpType.TOP;
			}
		}

		public override string GetSignatureForError ()
		{
			StringBuilder sb = new StringBuilder ();
			if (OperatorType == OpType.Implicit || OperatorType == OpType.Explicit) {
				sb.AppendFormat ("{0}.{1} operator {2}",
					Parent.GetSignatureForError (), GetName (OperatorType), type_name.GetSignatureForError ());
			}
			else {
				sb.AppendFormat ("{0}.operator {1}", Parent.GetSignatureForError (), GetName (OperatorType));
			}

			sb.Append (Parameters.GetSignatureForError ());
			return sb.ToString ();
		}
	}

	//
	// This is used to compare method signatures
	//
	struct MethodSignature {
		public string Name;
		public Type RetType;
		public Type [] Parameters;
		
		/// <summary>
		///    This delegate is used to extract methods which have the
		///    same signature as the argument
		/// </summary>
		public static MemberFilter method_signature_filter = new MemberFilter (MemberSignatureCompare);
		
		public MethodSignature (string name, Type ret_type, Type [] parameters)
		{
			Name = name;
			RetType = ret_type;

			if (parameters == null)
				Parameters = Type.EmptyTypes;
			else
				Parameters = parameters;
		}

		public override string ToString ()
		{
			string pars = "";
			if (Parameters.Length != 0){
				System.Text.StringBuilder sb = new System.Text.StringBuilder ();
				for (int i = 0; i < Parameters.Length; i++){
					sb.Append (Parameters [i]);
					if (i+1 < Parameters.Length)
						sb.Append (", ");
				}
				pars = sb.ToString ();
			}

			return String.Format ("{0} {1} ({2})", RetType, Name, pars);
		}
		
		public override int GetHashCode ()
		{
			return Name.GetHashCode ();
		}

		public override bool Equals (Object o)
		{
			MethodSignature other = (MethodSignature) o;

			if (other.Name != Name)
				return false;

			if (other.RetType != RetType)
				return false;
			
			if (Parameters == null){
				if (other.Parameters == null)
					return true;
				return false;
			}

			if (other.Parameters == null)
				return false;
			
			int c = Parameters.Length;
			if (other.Parameters.Length != c)
				return false;

			for (int i = 0; i < c; i++)
				if (other.Parameters [i] != Parameters [i])
					return false;

			return true;
		}

		static bool MemberSignatureCompare (MemberInfo m, object filter_criteria)
		{
			MethodSignature sig = (MethodSignature) filter_criteria;

			if (m.Name != sig.Name)
				return false;

			Type ReturnType;
			MethodInfo mi = m as MethodInfo;
			PropertyInfo pi = m as PropertyInfo;

			if (mi != null)
				ReturnType = mi.ReturnType;
			else if (pi != null)
				ReturnType = pi.PropertyType;
			else
				return false;

			//
			// we use sig.RetType == null to mean `do not check the
			// method return value.  
			//
			if (sig.RetType != null) {
				if (!TypeManager.IsEqual (ReturnType, sig.RetType))
					return false;
			}

			Type [] args;
			if (mi != null)
				args = TypeManager.GetParameterData (mi).Types;
			else
				args = TypeManager.GetParameterData (pi).Types;
			Type [] sigp = sig.Parameters;

			if (args.Length != sigp.Length)
				return false;

			for (int i = args.Length - 1; i >= 0; i--)
				if (!TypeManager.IsEqual (args [i], sigp [i]))
					return false;

			return true;
		}
	}
}

