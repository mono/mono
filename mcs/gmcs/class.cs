//
// class.cs: Class and Struct handlers
//
// Authors: Miguel de Icaza (miguel@gnu.org)
//          Martin Baulig (martin@gnome.org)
//          Marek Safar (marek.safar@seznam.cz)
//
// Licensed under the terms of the GNU GPL
//
// (C) 2001, 2002, 2003 Ximian, Inc (http://www.ximian.com)
//
//
//  2002-10-11  Miguel de Icaza  <miguel@ximian.com>
//
//	* class.cs: Following the comment from 2002-09-26 to AddMethod, I
//	have fixed a remaining problem: not every AddXXXX was adding a
//	fully qualified name.  
//
//	Now everyone registers a fully qualified name in the DeclSpace as
//	being defined instead of the partial name.  
//
//	Downsides: we are slower than we need to be due to the excess
//	copies and the names being registered this way.  
//
//	The reason for this is that we currently depend (on the corlib
//	bootstrap for instance) that types are fully qualified, because
//	we dump all the types in the namespace, and we should really have
//	types inserted into the proper namespace, so we can only store the
//	basenames in the defined_names array.
//
//
#define CACHE
using System;
using System.Text;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using Mono.CompilerServices.SymbolWriter;

namespace Mono.CSharp {

	public enum Kind {
		Root,
		Struct,
		Class,
		Interface
	}

	/// <summary>
	///   This is the base class for structs and classes.  
	/// </summary>
	public abstract class TypeContainer : DeclSpace, IMemberContainer {

		// Whether this is a struct, class or interface
		public readonly Kind Kind;

		// Holds a list of classes and structures
		ArrayList types;

		// Holds the list of properties
		ArrayList properties;

		// Holds the list of enumerations
		ArrayList enums;

		// Holds the list of delegates
		ArrayList delegates;
		
		// Holds the list of constructors
		ArrayList instance_constructors;

		// Holds the list of fields
		ArrayList fields;

		// Holds a list of fields that have initializers
		ArrayList initialized_fields;

		// Holds a list of static fields that have initializers
		ArrayList initialized_static_fields;

		// Holds the list of constants
		ArrayList constants;

		// Holds the list of
		ArrayList interfaces;

		// Holds the methods.
		ArrayList methods;

		// Holds the events
		ArrayList events;

		// Holds the indexers
		ArrayList indexers;

		// Holds the operators
		ArrayList operators;

		// Holds the iterators
		ArrayList iterators;

		// Holds the parts of a partial class;
		ArrayList parts;

		// The emit context for toplevel objects.
		EmitContext ec;
		
		//
		// Pointers to the default constructor and the default static constructor
		//
		Constructor default_constructor;
		Constructor default_static_constructor;

		//
		// Whether we have seen a static constructor for this class or not
		//
		public bool UserDefinedStaticConstructor = false;

		//
		// Whether we have at least one non-static field
		//
		bool have_nonstatic_fields = false;
		
		//
		// This one is computed after we can distinguish interfaces
		// from classes from the arraylist `type_bases' 
		//
		string     base_class_name;
		TypeExpr   parent_type;

		ArrayList type_bases;

		bool members_defined;
		bool members_defined_ok;

		// The interfaces we implement.
		Type[] ifaces;

		// The parent member container and our member cache
		IMemberContainer parent_container;
		MemberCache member_cache;

		//
		// The indexer name for this class
		//
		public string IndexerName;

		Type GenericType;

		public TypeContainer (NamespaceEntry ns, TypeContainer parent,
				      MemberName name, Attributes attrs, Kind kind, Location l)
			: base (ns, parent, name, attrs, l)
		{
			this.Kind = kind;

			types = new ArrayList ();

			base_class_name = null;
		}

		// <summary>
		//   Used to report back to the user the result of a declaration
		//   in the current declaration space
		// </summary>
		public void CheckDef (AdditionResult result, string name, Location loc)
		{
			if (result == AdditionResult.Success)
				return;

			switch (result){
			case AdditionResult.NameExists:
				Report.Error (102, loc, "The container `{0}' already " +
					      "contains a definition for `{1}'",
					      Name, name);
				break;

				//
				// This is handled only for static Constructors, because
				// in reality we handle these by the semantic analysis later
				//
			case AdditionResult.MethodExists:
				Report.Error (111, loc, "Class `{0}' already defines a " +
					      "member called '{1}' with the same parameter " +
					      "types (more than one default constructor)",
					      Name, name);
				break;

			case AdditionResult.EnclosingClash:
				Report.Error (542, loc, "Member names cannot be the same " +
					      "as their enclosing type");
				break;
		
			case AdditionResult.NotAConstructor:
				Report.Error (1520, loc, "Class, struct, or interface method " +
					      "must have a return type");
				break;

			case AdditionResult.Error:
				// Error has already been reported.
				break;
			}
		}

		public AdditionResult AddConstant (Const constant)
		{
			AdditionResult res;
			string basename = constant.Name;
			string fullname = Name + "." + basename;

			if ((res = IsValid (basename, fullname)) != AdditionResult.Success)
				return res;
			
			if (constants == null)
				constants = new ArrayList ();

			constants.Add (constant);
			DefineName (fullname, constant);

			return AdditionResult.Success;
		}

		public AdditionResult AddEnum (Mono.CSharp.Enum e)
		{
			AdditionResult res;

			if ((res = IsValid (e.Basename, e.Name)) != AdditionResult.Success)
				return res;

			if (enums == null)
				enums = new ArrayList ();

			enums.Add (e);
			DefineName (e.Name, e);

			return AdditionResult.Success;
		}
		
		public AdditionResult AddClass (TypeContainer c)
		{
			AdditionResult res;
			string name = c.Basename;
			
			if ((res = IsValid (name, c.Name)) != AdditionResult.Success)
				return res;

			DefineName (c.Name, c);
			types.Add (c);

			return AdditionResult.Success;
		}

		public AdditionResult AddStruct (TypeContainer s)
		{
			AdditionResult res;
			string name = s.Basename;
			
			if ((res = IsValid (name, s.Name)) != AdditionResult.Success)
				return res;

			DefineName (s.Name, s);
			types.Add (s);

			return AdditionResult.Success;
		}

		public AdditionResult AddDelegate (Delegate d)
		{
			AdditionResult res;
			string name = d.Basename;
			
			if ((res = IsValid (name, d.Name)) != AdditionResult.Success)
				return res;

			if (delegates == null)
				delegates = new ArrayList ();
			
			DefineName (d.Name, d);
			delegates.Add (d);

			return AdditionResult.Success;
		}

		public AdditionResult AddMethod (Method method)
		{
			string basename = method.Name;
			string fullname = Name + "." + basename;

			Object value = defined_names [fullname];

			if (value != null && (!(value is Method)))
				return AdditionResult.NameExists;

			if (basename == Basename)
				return AdditionResult.EnclosingClash;

			if (methods == null)
				methods = new ArrayList ();

			if (method.Name.IndexOf ('.') != -1)
				methods.Insert (0, method);
			else 
				methods.Add (method);
			
			if (value == null)
				DefineName (fullname, method);

			return AdditionResult.Success;
		}

		public AdditionResult AddConstructor (Constructor c)
		{
			if (c.Name != Basename) 
				return AdditionResult.NotAConstructor;

			bool is_static = (c.ModFlags & Modifiers.STATIC) != 0;
			
			if (is_static){
				UserDefinedStaticConstructor = true;
				if (default_static_constructor != null)
					return AdditionResult.MethodExists;

				default_static_constructor = c;
			} else {
				if (c.IsDefault ()){
					if (default_constructor != null)
						return AdditionResult.MethodExists;
					default_constructor = c;
				}
				
				if (instance_constructors == null)
					instance_constructors = new ArrayList ();
				
				instance_constructors.Add (c);
			}
			
			return AdditionResult.Success;
		}
		
		public AdditionResult AddInterface (TypeContainer iface)
		{
			AdditionResult res;
			string name = iface.Basename;
			
			if ((res = IsValid (name, iface.Name)) != AdditionResult.Success)
				return res;
			
			if (interfaces == null)
				interfaces = new ArrayList ();
			interfaces.Add (iface);
			DefineName (iface.Name, iface);
			
			return AdditionResult.Success;
		}

		public AdditionResult AddField (Field field)
		{
			AdditionResult res;
			string basename = field.Name;
			string fullname = Name + "." + basename;

			if ((res = IsValid (basename, fullname)) != AdditionResult.Success)
				return res;
			
			if (fields == null)
				fields = new ArrayList ();
			
			fields.Add (field);
			
			if (field.HasInitializer){
				if ((field.ModFlags & Modifiers.STATIC) != 0){
					if (initialized_static_fields == null)
						initialized_static_fields = new ArrayList ();

					initialized_static_fields.Add (field);

				} else {
					if (initialized_fields == null)
						initialized_fields = new ArrayList ();
				
					initialized_fields.Add (field);
				}
			}

			if ((field.ModFlags & Modifiers.STATIC) == 0)
				have_nonstatic_fields = true;

			DefineName (fullname, field);
			return AdditionResult.Success;
		}

		public AdditionResult AddProperty (Property prop)
		{
			AdditionResult res;

			if ((res = AddProperty (prop, prop.Name)) != AdditionResult.Success)
				return res;

			if (prop.Get != null) {
				if ((res = AddProperty (prop, "get_" + prop.Name)) != AdditionResult.Success)
					return res;
			}

			if (prop.Set != null) {
				if ((res = AddProperty (prop, "set_" + prop.Name)) != AdditionResult.Success)
				return res;
			}

			if (properties == null)
				properties = new ArrayList ();

			if (prop.Name.IndexOf ('.') != -1)
				properties.Insert (0, prop);
			else
				properties.Add (prop);

			return AdditionResult.Success;
		}

		AdditionResult AddProperty (Property prop, string basename)
		{
			AdditionResult res;
			string fullname = Name + "." + basename;

			if ((res = IsValid (basename, fullname)) != AdditionResult.Success)
				return res;

			DefineName (fullname, prop);

			return AdditionResult.Success;
		}

		public AdditionResult AddEvent (Event e)
		{
			AdditionResult res;
			string basename = e.Name;
			string fullname = Name + "." + basename;

			if ((res = IsValid (basename, fullname)) != AdditionResult.Success)
				return res;

			if (events == null)
				events = new ArrayList ();
			
			events.Add (e);
			DefineName (fullname, e);

			return AdditionResult.Success;
		}

		public void AddIndexer (Indexer i)
		{
			if (indexers == null)
				indexers = new ArrayList ();

			if (i.MemberName.Left != null)
				indexers.Insert (0, i);
			else
				indexers.Add (i);
		}

		public AdditionResult AddOperator (Operator op)
		{
			if (operators == null)
				operators = new ArrayList ();

			operators.Add (op);

			string basename = op.Name;
			string fullname = Name + "." + basename;
			if (!defined_names.Contains (fullname))
			{
				DefineName (fullname, op);
			}
			return AdditionResult.Success;
		}

		public void AddIterator (Iterator i)
		{
			if (iterators == null)
				iterators = new ArrayList ();

			iterators.Add (i);
		}

		public void AddType (TypeContainer tc)
		{
			types.Add (tc);
		}

		public void AddPart (ClassPart part)
		{
			if (parts == null)
				parts = new ArrayList ();

			parts.Add (part);
		}

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb)
		{
			if (a.Type == TypeManager.default_member_type) {
				if (Indexers != null) {
					Report.Error (646, a.Location,
						      "Cannot specify the DefaultMember attribute on" +
						      " a type containing an indexer");
					return;
				}
			}
			
			base.ApplyAttributeBuilder (a, cb);
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

		public ArrayList Methods {
			get {
				return methods;
			}
		}

		public ArrayList Constants {
			get {
				return constants;
			}
		}

		public ArrayList Interfaces {
			get {
				return interfaces;
			}
		}

		public ArrayList Iterators {
			get {
				return iterators;
			}
		}
		
		public string Base {
			get {
				return base_class_name;
			}
		}
		
		public ArrayList Bases {
			get {
				return type_bases;
			}

			set {
				type_bases = value;
			}
		}

		public ArrayList Fields {
			get {
				return fields;
			}

			set {
				fields = value;
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
		
		public ArrayList Enums {
			get {
				return enums;
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
		
		public ArrayList Parts {
			get {
				return parts;
			}
		}

		public virtual TypeAttributes TypeAttr {
			get {
				return Modifiers.TypeAttr (ModFlags, this);
			}
		}

		//
		// Emits the instance field initializers
		//
		public bool EmitFieldInitializers (EmitContext ec)
		{
			ArrayList fields;
			Expression instance_expr;
			
			if (ec.IsStatic){
				fields = initialized_static_fields;
				instance_expr = null;
			} else {
				fields = initialized_fields;
				instance_expr = new This (Location.Null).Resolve (ec);
			}

			if (fields == null)
				return true;

			foreach (Field f in fields){
				Expression e = f.GetInitializerExpression (ec);
				if (e == null)
					return false;

				Location l = f.Location;
				FieldExpr fe = new FieldExpr (f.FieldBuilder, l);
				fe.InstanceExpression = instance_expr;
				fe.IsFieldInitializer = true;

				ExpressionStatement a = new Assign (fe, e, l);

				a = a.ResolveStatement (ec);
				if (a == null)
					return false;

				a.EmitStatement (ec);
			}

			return true;
		}
		
		//
		// Defines the default constructors
		//
		void DefineDefaultConstructor (bool is_static)
		{
			Constructor c;

			// The default constructor is public
			// If the class is abstract, the default constructor is protected
			// The default static constructor is private

			int mods = Modifiers.PUBLIC;
			if (is_static)
				mods = Modifiers.STATIC | Modifiers.PRIVATE;
			else if ((ModFlags & Modifiers.ABSTRACT) != 0)
				mods = Modifiers.PROTECTED;

			c = new Constructor (this, Basename, mods, Parameters.EmptyReadOnlyParameters,
					     new ConstructorBaseInitializer (
						     null, Parameters.EmptyReadOnlyParameters,
						     Location),
					     Location);
			
			AddConstructor (c);
			
			c.Block = new ToplevelBlock (null, Location);
			
		}

		public void ReportStructInitializedInstanceError ()
		{
			string n = TypeBuilder.FullName;
			
			foreach (Field f in initialized_fields){
				Report.Error (
					573, Location,
					"`" + n + "." + f.Name + "': can not have " +
					"instance field initializers in structs");
			}
		}

		/// <remarks>
		///  The pending methods that need to be implemented
		//   (interfaces or abstract methods)
		/// </remarks>
		public PendingImplementation Pending;

		public abstract void Register ();

		public abstract PendingImplementation GetPendingImplementations ();

		TypeExpr[] GetPartialBases (out TypeExpr parent, out bool error)
		{
			ArrayList ifaces = new ArrayList ();

			parent = null;
			Location parent_loc = Location.Null;

			foreach (ClassPart part in parts) {
				TypeExpr new_parent;
				TypeExpr[] new_ifaces;

				new_ifaces = part.GetClassBases (out new_parent, out error);
				if (error)
					return null;

				if ((parent != null) && (new_parent != null) &&
				    !parent.Equals (new_parent)) {
					Report.Error (263, part.Location,
						      "Partial declarations of `{0}' must " +
						      "not specify different base classes",
						      Name);

					if (!Location.IsNull (parent_loc))
						Report.LocationOfPreviousError (parent_loc);

					error = true;
					return null;
				}

				if ((parent == null) && (new_parent != null)) {
					parent = new_parent;
					parent_loc = part.Location;
				}

				if (new_ifaces == null)
					continue;

				foreach (TypeExpr iface in new_ifaces) {
					bool found = false;
					foreach (TypeExpr old_iface in ifaces) {
						if (old_iface.Equals (iface)) {
							found = true;
							break;
						}
					}

					if (!found)
						ifaces.Add (iface);
				}
			}

			error = false;

			TypeExpr[] retval = new TypeExpr [ifaces.Count];
			ifaces.CopyTo (retval, 0);
			return retval;
		}

		TypeExpr[] GetNormalBases (out TypeExpr parent, out bool error)
		{
			parent = null;

			int count = Bases.Count;
			int start, i, j;

			if (Kind == Kind.Class){
				TypeExpr name = ResolveTypeExpr (
					(Expression) Bases [0], false, Location);

				if (name == null){
					error = true;
					return null;
				}

				if (name.IsClass){
					parent = name;
					start = 1;
				} else {
					start = 0;
				}
			} else {
				start = 0;
			}

			TypeExpr [] ifaces = new TypeExpr [count-start];
			
			for (i = start, j = 0; i < count; i++, j++){
				Expression name = (Expression) Bases [i];
				TypeExpr resolved = ResolveTypeExpr (name, false, Location);
				if (resolved == null) {
					error = true;
					return null;
				}
				
				ifaces [j] = resolved;
			}

			error = false;
			return ifaces;
		}

		/// <summary>
		///   This function computes the Base class and also the
		///   list of interfaces that the class or struct @c implements.
		///   
		///   The return value is an array (might be null) of
		///   interfaces implemented (as Types).
		///   
		///   The @parent argument is set to the parent object or null
		///   if this is `System.Object'. 
		/// </summary>
		TypeExpr [] GetClassBases (out TypeExpr parent, out bool error)
		{
			ArrayList bases = Bases;
			int start, j, i;

			error = false;

			TypeExpr[] ifaces;

			if (parts != null)
				ifaces = GetPartialBases (out parent, out error);
			else if (Bases == null){
				parent = null;
				return null;
			} else
				ifaces = GetNormalBases (out parent, out error);

			if (error)
				return null;

			if ((parent != null) && (Kind == Kind.Class)){
				if (parent is TypeParameterExpr){
					Report.Error (
						689, parent.Location,
						"Type parameter `{0}' can not be used as a " +
						"base class or interface", parent.Name);
					error = true;
					return null;
				}

				if (IsGeneric && parent.IsAttribute){
					Report.Error (
						698, parent.Location,
						"A generic type cannot derive from `{0}' " +
						"because it is an attribute class",
						parent.Name);
					error = true;
					return null;
				}

				if (parent.IsSealed){
					string detail = "";
  					
					if (parent.IsValueType)
						detail = " (a class can not inherit from a struct/enum)";
					
					Report.Error (509, "class `"+ Name +
						      "': Cannot inherit from sealed class `"+
						      parent.Name + "'" + detail);
					error = true;
					return null;
				}

				if (!parent.CanInheritFrom ()){
					Report.Error (644, Location,
						      "`{0}' cannot inherit from special class `{1}'",
						      Name, parent_type.Name);
					error = true;
					return null;
				}

				if (!parent.AsAccessible (this, ModFlags))
					Report.Error (60, Location,
						      "Inconsistent accessibility: base class `" +
						      parent.Name + "' is less accessible than class `" +
						      Name + "'");
			}

			if (parent != null)
				base_class_name = parent.Name;

			if (ifaces == null)
				return null;

			int count = ifaces != null ? ifaces.Length : 0;

			for (i = 0; i < count; i++) {
				TypeExpr iface = (TypeExpr) ifaces [i];

				if ((Kind != Kind.Class) && !iface.IsInterface){
					string what = Kind == Kind.Struct ?
						"Struct" : "Interface";

					Report.Error (527, Location,
						      "In {0} `{1}', type `{2}' is not "+
						      "an interface", what, Name, iface.Name);
					error = true;
					return null;
				}

				if (iface.IsClass) {
					if (parent != null){
						Report.Error (527, Location,
							      "In Class `{0}', `{1}' is not " +
							      "an interface", Name, iface.Name);
						error = true;
						return null;
					}
				}
  
				for (int x = 0; x < i; x++) {
					if (iface.Equals (ifaces [x])) {
						Report.Error (528, Location,
							      "`{0}' is already listed in " +
							      "interface list", iface.Name);
						error = true;
						return null;
					}
				}

				if ((Kind == Kind.Interface) &&
				    !iface.AsAccessible (Parent, ModFlags))
					Report.Error (61, Location,
						      "Inconsistent accessibility: base " +
						      "interface `{0}' is less accessible " +
						      "than interface `{1}'", iface.Name,
						      Name);
			}

			return ifaces;
		}

		bool CheckGenericInterfaces (Type[] ifaces)
		{
			ArrayList already_checked = new ArrayList ();

			for (int i = 0; i < ifaces.Length; i++) {
				Type iface = ifaces [i];
				foreach (Type t in already_checked) {
					if (iface == t)
						continue;

					if (!TypeManager.MayBecomeEqualGenericInstances (iface, t))
						continue;

					Report.Error (
						695, Location,
						"`{0}' cannot implement both `{1}' and `{2}' " +
						"because they may unify for some type " +
						"parameter substitutions",
						TypeManager.GetFullName (TypeBuilder),
						iface, t);
					return false;
				}

				already_checked.Add (iface);
			}

			return true;
		}

		bool error = false;
		
		//
		// Defines the type in the appropriate ModuleBuilder or TypeBuilder.
		//
		public override TypeBuilder DefineType ()
		{
			TypeExpr parent;

			if (TypeBuilder != null)
				return TypeBuilder;

			if (error)
				return null;
			
			if (InTransit) {
				Report.Error (146, Location, "Class definition is circular: `{0}'", Name);
				error = true;
				return null;
			}
			
			InTransit = true;

			ec = new EmitContext (this, Mono.CSharp.Location.Null, null, null, ModFlags);

			TypeAttributes type_attributes = TypeAttr;

			if (IsTopLevel){
				if (TypeManager.NamespaceClash (Name, Location)) {
					error = true;
					return null;
				}

				ModuleBuilder builder = CodeGen.Module.Builder;
				TypeBuilder = builder.DefineType (
					Name, type_attributes, null, null);
			} else {
				TypeBuilder builder = Parent.DefineType ();
				if (builder == null) {
					error = true;
					return null;
				}
				
				TypeBuilder = builder.DefineNestedType (
					MemberName.Basename, type_attributes, null, null);
			}

			TypeManager.AddUserType (Name, TypeBuilder, this);

			if (IsGeneric) {
				foreach (TypeParameter type_param in TypeParameters) {
					if (!type_param.Resolve (this)) {
						error = true;
						return null;
					}
				}

				CurrentType = new ConstructedType (
					Name, TypeParameters, Location);

				string[] param_names = new string [TypeParameters.Length];
				for (int i = 0; i < TypeParameters.Length; i++)
					param_names [i] = TypeParameters [i].Name;

				GenericTypeParameterBuilder[] gen_params;
				
				gen_params = TypeBuilder.DefineGenericParameters (param_names);

				for (int i = 0; i < gen_params.Length; i++)
					TypeParameters [i].Define (gen_params [i]);
			}

			if (IsGeneric) {
				foreach (TypeParameter type_param in TypeParameters)
					if (!type_param.DefineType (ec)) {
						error = true;
						return null;
					}
			}

			if ((Kind == Kind.Struct) && TypeManager.value_type == null)
				throw new Exception ();

			TypeExpr[] iface_exprs = GetClassBases (out parent_type, out error); 
			if (error)
				return null;

			if (parent_type == null) {
				if (Kind == Kind.Class){
					if (RootContext.StdLib)
						parent_type = TypeManager.system_object_expr;
					else if (Name != "System.Object")
						parent_type = TypeManager.system_object_expr;
				} else if (Kind == Kind.Struct){
					//
					// If we are compiling our runtime,
					// and we are defining ValueType, then our
					// parent is `System.Object'.
					//
					if (!RootContext.StdLib && Name == "System.ValueType")
						parent_type = TypeManager.system_object_expr;
					else if (Kind == Kind.Struct)
						parent_type = TypeManager.system_valuetype_expr;
				}
			}

			Type ptype;
			ConstructedType constructed = parent_type as ConstructedType;
			if ((constructed == null) && (parent_type != null))
				ptype = parent_type.ResolveType (ec);
			else
				ptype = null;

			if (constructed != null) {
				ptype = constructed.ResolveType (ec);
				if (ptype == null) {
					error = true;
					return null;
				}
			}

			if (ptype != null)
				TypeBuilder.SetParent (ptype);

			//
			// Structs with no fields need to have at least one byte.
			// The right thing would be to set the PackingSize in a DefineType
			// but there are no functions that allow interfaces *and* the size to
			// be specified.
			//

			if ((Kind == Kind.Struct) && !have_nonstatic_fields){
				TypeBuilder.DefineField ("$PRIVATE$", TypeManager.byte_type,
							 FieldAttributes.Private);
			}

			// add interfaces that were not added at type creation
			if (iface_exprs != null) {
				ifaces = TypeManager.ExpandInterfaces (ec, iface_exprs);
				if (ifaces == null) {
					error = true;
					return null;
				}

				foreach (Type itype in ifaces)
 					TypeBuilder.AddInterfaceImplementation (itype);

				if (!CheckGenericInterfaces (ifaces)) {
					error = true;
					return null;
				}

				TypeManager.RegisterBuilder (TypeBuilder, ifaces);
			}

			if (IsGeneric) {
				foreach (TypeParameter type_param in TypeParameters)
					if (!type_param.CheckDependencies (ec)) {
						error = true;
						return null;
					}
			}

			//
			// Finish the setup for the EmitContext
			//
			ec.ContainerType = TypeBuilder;

			if ((parent_type != null) && parent_type.IsAttribute) {
				RootContext.RegisterAttribute (this);
			} else if (!(this is Iterator))
				RootContext.RegisterOrder (this); 

			if (!DefineNestedTypes ()) {
				error = true;
				return null;
			}

			InTransit = false;
			return TypeBuilder;
					}

		protected virtual bool DefineNestedTypes ()
		{
			if (Interfaces != null) {
				foreach (TypeContainer iface in Interfaces)
					if (iface.DefineType () == null)
						return false;
			}
			
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

			if (Enums != null) {
				foreach (Enum en in Enums)
					if (en.DefineType () == null)
						return false;
			}

			if (Parts != null) {
				foreach (ClassPart part in Parts) {
					part.TypeBuilder = TypeBuilder;
					part.parent_type = parent_type;
				}
		}

			return true;
		}


		/// <summary>
		///   Defines the MemberCore objects that are in the `list' Arraylist
		///
		///   The `defined_names' array contains a list of members defined in
		///   a base class
		/// </summary>
		static ArrayList remove_list = new ArrayList ();
		void DefineMembers (ArrayList list, MemberInfo [] defined_names)
		{
			int idx;
			
			remove_list.Clear ();

			foreach (MemberCore mc in list){

				if (defined_names != null)
					idx = Array.BinarySearch (defined_names, mc.Name, mif_compare);
				else
					idx = -1;

				if (idx < 0){
					if (RootContext.WarningLevel >= 4){
						if ((mc.ModFlags & Modifiers.NEW) != 0)
							Warning_KeywordNewNotRequired (mc.Location, mc);
					}
				} else if (mc is MethodCore)
					((MethodCore) mc).OverridesSomething = true;

				if (!mc.Define ()){
					remove_list.Add (mc);
					continue;
				}
						
				if (idx < 0)
					continue;

				MemberInfo match = defined_names [idx];

				if (match is PropertyInfo && ((mc.ModFlags & Modifiers.OVERRIDE) != 0))
					continue;

				//
				// If we are both methods, let the method resolution emit warnings
				//
				if (match is MethodBase && mc is MethodCore)
					continue; 

				if ((mc.ModFlags & Modifiers.NEW) == 0) {
					if (mc is Event) {
						if (!(match is EventInfo)) {
							Error_EventCanOnlyOverrideEvent (mc.Location, defined_names [idx]);
							return;
						}

						if ((mc.ModFlags & Modifiers.OVERRIDE) != 0)
							continue;
					}

					Warning_KeywordNewRequired (mc.Location, defined_names [idx]);
				}
			}
			
			foreach (object o in remove_list)
				list.Remove (o);
			
			remove_list.Clear ();
		}

		//
		// Defines the indexers, and also verifies that the IndexerNameAttribute in the
		// class is consistent.  Either it is `Item' or it is the name defined by all the
		// indexers with the `IndexerName' attribute.
		//
		// Turns out that the IndexerNameAttribute is applied to each indexer,
		// but it is never emitted, instead a DefaultMember attribute is attached
		// to the class.
		//
		void DefineIndexers ()
		{
			string class_indexer_name = null;

			//
			// If there's both an explicit and an implicit interface implementation, the
			// explicit one actually implements the interface while the other one is just
			// a normal indexer.  See bug #37714.
			//

			// Invariant maintained by AddIndexer(): All explicit interface indexers precede normal indexers
			bool seen_normal_indexers = false;
			foreach (Indexer i in Indexers) {
				string name;

				i.Define ();

				name = i.IndexerName;

				if (i.InterfaceType != null) {
					if (seen_normal_indexers)
						throw new Exception ("Internal Error: 'Indexers' array not sorted properly.");
					continue;
				}

				seen_normal_indexers = true;

				if (class_indexer_name == null)
					class_indexer_name = name;
				else if (name != class_indexer_name)
					Report.Error (668, i.Location, "Two indexers have different names, " +
						      " you should use the same name for all your indexers");
			}

			if (seen_normal_indexers && class_indexer_name == null)
				class_indexer_name = "Item";
			IndexerName = class_indexer_name;
		}

		static void Error_KeywordNotAllowed (Location loc)
		{
			Report.Error (1530, loc, "Keyword new not allowed for namespace elements");
		}

		/// <summary>
		///   Populates our TypeBuilder with fields and methods
		/// </summary>
		public override bool DefineMembers (TypeContainer container)
		{
			if (members_defined)
				return members_defined_ok;

			members_defined_ok = DoDefineMembers ();
			members_defined = true;

			return members_defined_ok;
		}

		bool DoDefineMembers ()
		{
			MemberInfo [] defined_names = null;

			//
			// We need to be able to use the member cache while we are checking/defining
			//
#if CACHE
			if (TypeBuilder.BaseType != null)
				parent_container = TypeManager.LookupMemberContainer (TypeBuilder.BaseType);
#endif

			if (RootContext.WarningLevel > 1){
				Type ptype;

				//
				// This code throws an exception in the comparer
				// I guess the string is not an object?
				//
				ptype = TypeBuilder.BaseType;
				if (ptype != null){
					defined_names = (MemberInfo []) FindMembers (
						ptype, MemberTypes.All & ~MemberTypes.Constructor,
						BindingFlags.Public | BindingFlags.Instance |
						BindingFlags.Static, null, null);

					Array.Sort (defined_names, mif_compare);
				}
			}

			Class pclass = Parent as Class;
			if (pclass != null) {
				string pname = null;
				TypeExpr ptype = null;
				Type t = pclass.TypeBuilder.BaseType;
				while ((t != null) && (ptype == null)) {
					pname = t.FullName + "." + Basename;
					ptype = RootContext.LookupType (this, pname, true, Location.Null);
					t = t.BaseType;
				}

				if ((ModFlags & Modifiers.NEW) != 0) {
					if (ptype == null)
						Report.Warning (109, Location, "The member '" + Name + "' does not hide an " +
								"inherited member. The keyword new is not required.");
				} else if (ptype != null) {
					Report.Warning (108, Location, "The keyword new is required on `" +
							Name + "' because it hides inherited member '" +
							pname + "'.");
				}
			} else if ((ModFlags & Modifiers.NEW) != 0)
				Error_KeywordNotAllowed (Location);

			if (constants != null)
				DefineMembers (constants, defined_names);

			if (fields != null)
				DefineMembers (fields, defined_names);

			if ((Kind == Kind.Class) && !(this is ClassPart)){
				if (instance_constructors == null){
					if (default_constructor == null)
						DefineDefaultConstructor (false);
				}

				if (initialized_static_fields != null &&
				    default_static_constructor == null)
					DefineDefaultConstructor (true);
			}

			if (Kind == Kind.Struct){
				//
				// Structs can not have initialized instance
				// fields
				//
				if (initialized_static_fields != null &&
				    default_static_constructor == null)
					DefineDefaultConstructor (true);

				if (initialized_fields != null)
					ReportStructInitializedInstanceError ();
			}

			Pending = GetPendingImplementations ();

			if (parts != null) {
				foreach (ClassPart part in parts) {
					if (!part.DefineMembers (this))
						return false;
				}
			}
			
			//
			// Constructors are not in the defined_names array
			//
			if (instance_constructors != null)
				DefineMembers (instance_constructors, null);
		
			if (default_static_constructor != null)
				default_static_constructor.Define ();

			if (methods != null)
				DefineMembers (methods, defined_names);

			if (properties != null)
				DefineMembers (properties, defined_names);

			if (events != null)
				DefineMembers (events, defined_names);

			if (indexers != null)
				DefineIndexers ();

			if (operators != null){
				DefineMembers (operators, null);

				CheckPairedOperators ();
			}

			if (enums != null)
				DefineMembers (enums, defined_names);
			
			if (delegates != null)
				DefineMembers (delegates, defined_names);

			if (CurrentType != null) {
				GenericType = CurrentType.ResolveType (ec);

				ec.ContainerType = GenericType;
			}


#if CACHE
			if (!(this is ClassPart))
			member_cache = new MemberCache (this);
#endif

			if (parts != null) {
				foreach (ClassPart part in parts)
					part.member_cache = member_cache;
			}

			if (iterators != null) {
				foreach (Iterator iterator in iterators) {
					if (iterator.DefineType () == null)
						return false;
				}

				foreach (Iterator iterator in iterators) {
					if (!iterator.DefineMembers (this))
						return false;
				}
			}

			return true;
		}

		public override bool Define ()
		{
			if (parts != null) {
				foreach (ClassPart part in parts) {
					if (!part.Define ())
						return false;
				}
			}

			return true;
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

		
		/// <summary>
		///   A member comparission method based on name only
		/// </summary>
		static IComparer mif_compare;

		static TypeContainer ()
		{
			accepting_filter = new MemberFilter (AlwaysAccept);
			mif_compare = new MemberInfoCompare ();
		}
		
		public MethodInfo[] GetMethods ()
		{
			ArrayList members = new ArrayList ();

			DefineMembers (null);

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

					members.Add (o.OperatorMethodBuilder);
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
						Field f = (Field) fields [i];
						
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
						Method m = (Method) methods [i];
						
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
						
						MethodBuilder ob = o.OperatorMethodBuilder;
						if (ob != null && filter (ob, criteria) == true) {
							if (members == null)
								members = new ArrayList ();
							
							members.Add (ob);
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
			
			if ((mt & MemberTypes.NestedType) != 0) {
				if (types != null) {
					int len = types.Count;
					for (int i = 0; i < len; i++) {
						TypeContainer t = (TypeContainer) types [i];
						
						if ((t.ModFlags & modflags) == 0)
							continue;

						TypeBuilder tb = t.TypeBuilder;
						if (tb != null && (filter (tb, criteria) == true)) {
							if (members == null)
								members = new ArrayList ();
							
								members.Add (tb);
					}
				}
				}

				if (enums != null) {
					int len = enums.Count;
					for (int i = 0; i < len; i++) {
						Enum en = (Enum) enums [i];

						if ((en.ModFlags & modflags) == 0)
							continue;

						TypeBuilder tb = en.TypeBuilder;
						if (tb != null && (filter (tb, criteria) == true)) {
							if (members == null)
								members = new ArrayList ();
							
							members.Add (tb);
					}
				}
				}
				
				if (delegates != null) {
					int len = delegates.Count;
					for (int i = 0; i < len; i++) {
						Delegate d = (Delegate) delegates [i];
				
						if ((d.ModFlags & modflags) == 0)
							continue;

						TypeBuilder tb = d.TypeBuilder;
						if (tb != null && (filter (tb, criteria) == true)) {
							if (members == null)
								members = new ArrayList ();
							
							members.Add (tb);
					}
				}
				}

				if (interfaces != null) {
					int len = interfaces.Count;
					for (int i = 0; i < len; i++) {
						TypeContainer iface = (TypeContainer) interfaces [i];

						if ((iface.ModFlags & modflags) == 0)
							continue;

						TypeBuilder tb = iface.TypeBuilder;
						if (tb != null && (filter (tb, criteria) == true)) {
							if (members == null)
								members = new ArrayList ();
							
							members.Add (tb);
					}
				}
			}
			}

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
			// Lookup members in parent if requested.
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

		//
		// FindMethods will look for methods not only in the type `t', but in
		// any interfaces implemented by the type.
		//
		public static MethodInfo [] FindMethods (Type t, BindingFlags bf,
							 MemberFilter filter, object criteria)
		{
			return null;
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

		protected virtual void VerifyMembers (EmitContext ec)
		{
			//
			// Check for internal or private fields that were never assigned
			//
			if (RootContext.WarningLevel >= 3) {
				if (fields != null){
					foreach (Field f in fields) {
						if ((f.ModFlags & Modifiers.Accessibility) != Modifiers.PRIVATE)
							continue;
						
						if ((f.status & Field.Status.USED) == 0){
							Report.Warning (
								169, f.Location, "Private field " +
								MakeName (f.Name) + " is never used");
							continue;
						}
						
						//
						// Only report 649 on level 4
						//
						if (RootContext.WarningLevel < 4)
							continue;
						
						if ((f.status & Field.Status.ASSIGNED) != 0)
							continue;
						
						Report.Warning (
							649, f.Location,
							"Field " + MakeName (f.Name) + " is never assigned " +
							" to and will always have its default value");
					}
				}

				if (events != null){
					foreach (Event e in events){
						if (e.status == 0)
							Report.Warning (67, "The event " + MakeName (e.Name) + " is never used");
					}
				}
			}
		}

		/// <summary>
		///   Emits the code, this step is performed after all
		///   the types, enumerations, constructors
		/// </summary>
		public void EmitType ()
		{
			if (OptAttributes != null)
				OptAttributes.Emit (ec, this);

			Emit ();

			if (instance_constructors != null) {
				if (TypeBuilder.IsSubclassOf (TypeManager.attribute_type) && IsClsCompliaceRequired (this)) {
					bool has_compliant_args = false;

					foreach (Constructor c in instance_constructors) {
						c.Emit ();

						if (has_compliant_args)
							continue;

						has_compliant_args = c.HasCompliantArgs;
					}
					if (!has_compliant_args)
						Report.Error_T (3015, Location, GetSignatureForError ());
				} else {
				foreach (Constructor c in instance_constructors)
						c.Emit ();
				}
			}

			if (default_static_constructor != null)
				default_static_constructor.Emit ();
			
			if (methods != null)
				foreach (Method m in methods)
					m.Emit ();

			if (operators != null)
				foreach (Operator o in operators)
					o.Emit ();

			if (properties != null)
				foreach (Property p in properties)
					p.Emit ();

			if (indexers != null){
				foreach (Indexer ix in indexers)
					ix.Emit ();
				if (IndexerName != null) {
					CustomAttributeBuilder cb = EmitDefaultMemberAttr ();
					TypeBuilder.SetCustomAttribute (cb);
				}
			}
			
			if (fields != null)
				foreach (Field f in fields)
					f.Emit ();

			if (events != null){
				foreach (Event e in Events)
					e.Emit ();
			}

			if (delegates != null) {
				foreach (Delegate d in Delegates) {
					d.Emit ();
				}
			}

			if (enums != null) {
				foreach (Enum e in enums) {
					e.Emit ();
				}
			}

			if (parts != null) {
				foreach (ClassPart part in parts)
					part.EmitType ();
			}

			if ((Pending != null) && !(this is ClassPart))
				if (Pending.VerifyPendingMethods ())
					return;

			VerifyMembers (ec);

			if (iterators != null)
				foreach (Iterator iterator in iterators)
					iterator.EmitType ();
			
//			if (types != null)
//				foreach (TypeContainer tc in types)
//					tc.Emit ();
		}
		
		CustomAttributeBuilder EmitDefaultMemberAttr ()
		{
			EmitContext ec = new EmitContext (this, Location, null, null, ModFlags);

			Expression ml = Expression.MemberLookup (ec, TypeManager.default_member_type,
								 ".ctor", MemberTypes.Constructor,
								 BindingFlags.Public | BindingFlags.Instance,
								 Location.Null);
			
			MethodGroupExpr mg = (MethodGroupExpr) ml;

			MethodBase constructor = mg.Methods [0];

			string [] vals = { IndexerName };

			CustomAttributeBuilder cb = null;
			try {
				cb = new CustomAttributeBuilder ((ConstructorInfo) constructor, vals);
			} catch {
				Report.Warning (-100, "Can not set the indexer default member attribute");
			}

			return cb;
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
			} catch {
				Console.WriteLine ("In type: " + Name);
				throw;
			}
			
			if (Enums != null)
				foreach (Enum en in Enums)
					en.CloseType ();

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

			if (Iterators != null)
				foreach (Iterator i in Iterators)
					i.CloseType ();
			
			types = null;
			properties = null;
			enums = null;
			delegates = null;
			fields = null;
			initialized_fields = null;
			initialized_static_fields = null;
			constants = null;
			interfaces = null;
			methods = null;
			events = null;
			indexers = null;
			operators = null;
			iterators = null;
			ec = null;
			default_constructor = null;
			default_static_constructor = null;
			type_bases = null;
			OptAttributes = null;
			ifaces = null;
			parent_container = null;
			member_cache = null;
		}

		public string MakeName (string n)
		{
			return "`" + Name + "." + n + "'";
		}

		public void Warning_KeywordNewRequired (Location l, MemberInfo mi)
		{
			Report.Warning (
				108, l, "The keyword new is required on " + 
				MakeName (mi.Name) + " because it hides `" +
				mi.ReflectedType.Name + "." + mi.Name + "'");
		}

		public void Warning_KeywordNewNotRequired (Location l, MemberCore mc)
		{
			Report.Warning (
				109, l, "The member " + MakeName (mc.Name) + " does not hide an " +
				"inherited member, the keyword new is not required");
		}

		public void Error_EventCanOnlyOverrideEvent (Location l, MemberInfo mi)
		{
			Report.Error (
				72, l, MakeName (mi.Name) + " : cannot override; `" +
				mi.ReflectedType.Name + "." + mi.Name + "' is not an event");
		}
		
		public static int CheckMember (string name, MemberInfo mi, int ModFlags)
		{
			return 0;
		}

		//
		// Performs the validation on a Method's modifiers (properties have
		// the same properties).
		//
		public bool MethodModifiersValid (int flags, string n, Location loc)
		{
			const int vao = (Modifiers.VIRTUAL | Modifiers.ABSTRACT | Modifiers.OVERRIDE);
			const int va = (Modifiers.VIRTUAL | Modifiers.ABSTRACT);
			const int nv = (Modifiers.NEW | Modifiers.VIRTUAL);
			bool ok = true;
			
			//
			// At most one of static, virtual or override
			//
			if ((flags & Modifiers.STATIC) != 0){
				if ((flags & vao) != 0){
					Report.Error (
						112, loc, "static method " + MakeName (n) + "can not be marked " +
						"as virtual, abstract or override");
					ok = false;
				}
			}

			if (Kind == Kind.Struct){
				if ((flags & va) != 0){
					Modifiers.Error_InvalidModifier (loc, "virtual or abstract");
					ok = false;
				}
			}

			if ((flags & Modifiers.OVERRIDE) != 0 && (flags & nv) != 0){
				Report.Error (
					113, loc, MakeName (n) +
					" marked as override cannot be marked as new or virtual");
				ok = false;
			}

			//
			// If the declaration includes the abstract modifier, then the
			// declaration does not include static, virtual or extern
			//
			if ((flags & Modifiers.ABSTRACT) != 0){
				if ((flags & Modifiers.EXTERN) != 0){
					Report.Error (
						180, loc, MakeName (n) + " can not be both abstract and extern");
					ok = false;
				}

				if ((flags & Modifiers.VIRTUAL) != 0){
					Report.Error (
						503, loc, MakeName (n) + " can not be both abstract and virtual");
					ok = false;
				}

				if ((ModFlags & Modifiers.ABSTRACT) == 0){
					Report.Error (
						513, loc, MakeName (n) +
						" is abstract but its container class is not");
					ok = false;

				}
			}

			if ((flags & Modifiers.PRIVATE) != 0){
				if ((flags & vao) != 0){
					Report.Error (
						621, loc, MakeName (n) +
						" virtual or abstract members can not be private");
					ok = false;
				}
			}

			if ((flags & Modifiers.SEALED) != 0){
				if ((flags & Modifiers.OVERRIDE) == 0){
					Report.Error (
						238, loc, MakeName (n) +
						" cannot be sealed because it is not an override");
					ok = false;
				}
			}

			return ok;
		}

		Hashtable builder_and_args;
		
		public bool RegisterMethod (MethodBuilder mb, InternalParameters ip, Type [] args)
		{
			if (builder_and_args == null)
				builder_and_args = new Hashtable ();
			return true;
		}

		protected override bool VerifyClsCompliance (DeclSpace ds)
		{
			if (!base.VerifyClsCompliance (ds))
				return false;

			// parent_container is null for System.Object
			if (parent_container != null && !AttributeTester.IsClsCompliant (parent_container.Type)) {
				Report.Error_T (3009, Location, GetSignatureForError (),  TypeManager.CSharpName (parent_container.Type));
			}
			return true;
		}


		/// <summary>
		///   Performs checks for an explicit interface implementation.  First it
		///   checks whether the `interface_type' is a base inteface implementation.
		///   Then it checks whether `name' exists in the interface type.
		/// </summary>
		public virtual bool VerifyImplements (Type interface_type, string full,
						      string name, Location loc)
		{
			bool found = false;

			if (ifaces != null){
				foreach (Type t in ifaces){
					if (t == interface_type){
						found = true;
						break;
					}
				}
			}
			
			if (!found){
				Report.Error (540, "`" + full + "': containing class does not implement interface `" + interface_type.FullName + "'");
				return false;
			}

			return true;
		}

		protected override void VerifyObsoleteAttribute()
		{
			CheckUsageOfObsoleteAttribute (TypeBuilder.BaseType);

			if (ifaces == null)
				return;

			foreach (Type iface in ifaces) {
				CheckUsageOfObsoleteAttribute (iface);
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

		IMemberContainer IMemberContainer.Parent {
			get {
				return parent_container;
			}
		}

		MemberCache IMemberContainer.MemberCache {
			get {
				return member_cache;
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
		// Operator pair checking
		//

		class OperatorEntry {
			public int flags;
			public Type ret_type;
			public Type type1, type2;
			public Operator op;
			public Operator.OpType ot;
			
			public OperatorEntry (int f, Operator o)
			{
				flags = f;

				ret_type = o.OperatorMethod.GetReturnType ();
				Type [] pt = o.OperatorMethod.ParameterTypes;
				type1 = pt [0];
				type2 = pt [1];
				op = o;
				ot = o.OperatorType;
			}

			public override int GetHashCode ()
			{	
				return ret_type.GetHashCode ();
			}

			public override bool Equals (object o)
			{
				OperatorEntry other = (OperatorEntry) o;

				if (other.ret_type != ret_type)
					return false;
				if (other.type1 != type1)
					return false;
				if (other.type2 != type2)
					return false;
				return true;
			}
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
			Hashtable pairs = new Hashtable (null, null);
			Operator true_op = null;
			Operator false_op = null;
			bool has_equality_or_inequality = false;
			
			// Register all the operators we care about.
			foreach (Operator op in operators){
				int reg = 0;
				
				switch (op.OperatorType){
				case Operator.OpType.Equality:
					reg = 1;
					has_equality_or_inequality = true;
					break;
				case Operator.OpType.Inequality:
					reg = 2;
					has_equality_or_inequality = true;
					break;

				case Operator.OpType.True:
					true_op = op;
					break;
				case Operator.OpType.False:
					false_op = op;
					break;
					
				case Operator.OpType.GreaterThan:
					reg = 1; break;
				case Operator.OpType.LessThan:
					reg = 2; break;
					
				case Operator.OpType.GreaterThanOrEqual:
					reg = 1; break;
				case Operator.OpType.LessThanOrEqual:
					reg = 2; break;
				}
				if (reg == 0)
					continue;

				OperatorEntry oe = new OperatorEntry (reg, op);

				object o = pairs [oe];
				if (o == null)
					pairs [oe] = oe;
				else {
					oe = (OperatorEntry) o;
					oe.flags |= reg;
				}
			}

			if (true_op != null){
				if (false_op == null)
					Report.Error (216, true_op.Location, "operator true requires a matching operator false");
			} else if (false_op != null)
				Report.Error (216, false_op.Location, "operator false requires a matching operator true");
			
			//
			// Look for the mistakes.
			//
			foreach (DictionaryEntry de in pairs){
				OperatorEntry oe = (OperatorEntry) de.Key;

				if (oe.flags == 3)
					continue;

				string s = "";
				switch (oe.ot){
				case Operator.OpType.Equality:
					s = "!=";
					break;
				case Operator.OpType.Inequality: 
					s = "==";
					break;
				case Operator.OpType.GreaterThan: 
					s = "<";
					break;
				case Operator.OpType.LessThan:
					s = ">";
					break;
				case Operator.OpType.GreaterThanOrEqual:
					s = "<=";
					break;
				case Operator.OpType.LessThanOrEqual:
					s = ">=";
					break;
				}
				Report.Error (216, oe.op.Location,
					      "The operator `" + oe.op + "' requires a matching operator `" + s + "' to also be defined");
			}

			if ((has_equality_or_inequality) && (RootContext.WarningLevel >= 2)) {
				MethodSignature equals_ms = new MethodSignature (
					"Equals", TypeManager.bool_type, new Type [] { TypeManager.object_type });
				MethodSignature hash_ms = new MethodSignature (
					"GetHashCode", TypeManager.int32_type, new Type [0]);

				MemberList equals_ml = FindMembers (MemberTypes.Method, BindingFlags.Public | BindingFlags.Instance |
								    BindingFlags.DeclaredOnly, MethodSignature.method_signature_filter,
								    equals_ms);
				MemberList hash_ml = FindMembers (MemberTypes.Method, BindingFlags.Public | BindingFlags.Instance |
								  BindingFlags.DeclaredOnly, MethodSignature.method_signature_filter,
								  hash_ms);

				bool equals_ok = false;
				if ((equals_ml != null) && (equals_ml.Count == 1))
					equals_ok = equals_ml [0].DeclaringType == TypeBuilder;
				bool hash_ok = false;
				if ((hash_ml != null) && (hash_ml.Count == 1))
					hash_ok = hash_ml [0].DeclaringType == TypeBuilder;

				if (!equals_ok)
					Report.Warning (660, Location, "`" + Name + "' defines operator == or operator != but does " +
							"not override Object.Equals (object o)");
				if (!hash_ok)
					Report.Warning (661, Location, "`" + Name + "' defines operator == or operator != but does " +
							"not override Object.GetHashCode ()");
			}
		}
		
	}

	public class PartialContainer : TypeContainer {

		public readonly Namespace Namespace;
		public readonly int OriginalModFlags;
		public readonly int AllowedModifiers;
		public readonly TypeAttributes DefaultTypeAttributes;

		static PartialContainer Create (NamespaceEntry ns, TypeContainer parent,
						MemberName name, int mod_flags, Kind kind,
						Location loc)
		{
			PartialContainer pc;
			string full_name = name.GetName (true);
			DeclSpace ds = (DeclSpace) RootContext.Tree.Decls [full_name];
			if (ds != null) {
				pc = ds as PartialContainer;

				if (pc == null) {
					Report.Error (
						260, ds.Location, "Missing partial modifier " +
						"on declaration of type `{0}'; another " +
						"partial implementation of this type exists",
						name);

					Report.LocationOfPreviousError (loc);
					return null;
				}

				if (pc.Kind != kind) {
					Report.Error (
						261, loc, "Partial declarations of `{0}' " +
						"must be all classes, all structs or " +
						"all interfaces", name);
					return null;
				}

				if (pc.OriginalModFlags != mod_flags) {
					Report.Error (
						262, loc, "Partial declarations of `{0}' " +
						"have conflicting accessibility modifiers",
						name);
					return null;
				}

				return pc;
			}

			pc = new PartialContainer (ns, parent, name, mod_flags, kind, loc);
			RootContext.Tree.RecordDecl (full_name, pc);
			parent.AddType (pc);
			pc.Register ();
			return pc;
		}

		public static ClassPart CreatePart (NamespaceEntry ns, TypeContainer parent,
						    MemberName name, int mod, Attributes attrs,
						    Kind kind, Location loc)
		{
			PartialContainer pc = Create (ns, parent, name, mod, kind, loc);
			if (pc == null) {
				// An error occured; create a dummy container, but don't
				// register it.
				pc = new PartialContainer (ns, parent, name, mod, kind, loc);
			}

			ClassPart part = new ClassPart (ns, pc, mod, attrs, kind, loc);
			pc.AddPart (part);
			return part;
		}

		protected PartialContainer (NamespaceEntry ns, TypeContainer parent,
					    MemberName name, int mod, Kind kind, Location l)
			: base (ns, parent, name, null, kind, l)
		{
			this.Namespace = ns.NS;

			switch (kind) {
			case Kind.Class:
				AllowedModifiers = Class.AllowedModifiers;
				DefaultTypeAttributes = Class.DefaultTypeAttributes;
				break;

			case Kind.Struct:
				AllowedModifiers = Struct.AllowedModifiers;
				DefaultTypeAttributes = Struct.DefaultTypeAttributes;
				break;

			case Kind.Interface:
				AllowedModifiers = Interface.AllowedModifiers;
				DefaultTypeAttributes = Interface.DefaultTypeAttributes;
				break;

			default:
				throw new InvalidOperationException ();
			}

			int accmods;
			if (parent.Parent == null)
				accmods = Modifiers.INTERNAL;
			else
				accmods = Modifiers.PRIVATE;

			this.ModFlags = Modifiers.Check (AllowedModifiers, mod, accmods, l);
			this.OriginalModFlags = mod;
		}

		public override void Register ()
		{
			if (Kind == Kind.Interface)
				Parent.AddInterface (this);
			else if (Kind == Kind.Class)
				Parent.AddClass (this);
			else if (Kind == Kind.Struct)
				Parent.AddStruct (this);
			else
				throw new InvalidOperationException ();
		}

		public override PendingImplementation GetPendingImplementations ()
		{
			return PendingImplementation.GetPendingImplementations (this);
		}

		public ClassPart AddPart (NamespaceEntry ns, int mod, Attributes attrs,
					  Location l)
		{
			ClassPart part = new ClassPart (ns, this, mod, attrs, Kind, l);
			AddPart (part);
			return part;
		}

		public override TypeAttributes TypeAttr {
			get {
				return base.TypeAttr | DefaultTypeAttributes;
			}
		}
	}

	public class ClassPart : TypeContainer {
		public readonly PartialContainer PartialContainer;
		public readonly bool IsPartial;

		public ClassPart (NamespaceEntry ns, PartialContainer parent,
				  int mod, Attributes attrs, Kind kind, Location l)
			: base (ns, parent.Parent, parent.MemberName, attrs, kind, l)
		{
			this.PartialContainer = parent;
			this.IsPartial = true;

			int accmods;
			if (parent.Parent == null)
				accmods = Modifiers.INTERNAL;
			else
				accmods = Modifiers.PRIVATE;

			this.ModFlags = Modifiers.Check (
				parent.AllowedModifiers, mod, accmods, l);
		}

		public override void Register ()
		{
		}

		public override PendingImplementation GetPendingImplementations ()
		{
			return PartialContainer.Pending;
		}

		public override bool VerifyImplements (Type interface_type, string full,
						       string name, Location loc)
		{
			return PartialContainer.VerifyImplements (
				interface_type, full, name, loc);
		}
	}

	public abstract class ClassOrStruct : TypeContainer {
		bool hasExplicitLayout = false;

		public ClassOrStruct (NamespaceEntry ns, TypeContainer parent,
				      MemberName name, Attributes attrs, Kind kind,
				      Location l)
			: base (ns, parent, name, attrs, kind, l)
		{
		}

		public override PendingImplementation GetPendingImplementations ()
		{
			return PendingImplementation.GetPendingImplementations (this);
		}

		protected override void VerifyMembers (EmitContext ec) 
		{
			if (Fields != null) {
				foreach (Field f in Fields) {
					if ((f.ModFlags & Modifiers.STATIC) != 0)
						continue;
					if (hasExplicitLayout) {
						if (f.OptAttributes == null 
						    || !f.OptAttributes.Contains (TypeManager.field_offset_attribute_type, ec)) {
							Report.Error (625, f.Location,
								      "Instance field of type marked with" 
								      + " StructLayout(LayoutKind.Explicit) must have a"
								      + " FieldOffset attribute.");
						}
					}
					else {
						if (f.OptAttributes != null 
						    && f.OptAttributes.Contains (TypeManager.field_offset_attribute_type, ec)) {
							Report.Error (636, f.Location,
								      "The FieldOffset attribute can only be placed on members of "
								      + "types marked with the StructLayout(LayoutKind.Explicit)");
						}
					}
				}
			}
			base.VerifyMembers (ec);
		}

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb)
		{
			if (a.Type == TypeManager.struct_layout_attribute_type
			    && (LayoutKind) a.GetPositionalValue (0) == LayoutKind.Explicit)
				hasExplicitLayout = true;

			base.ApplyAttributeBuilder (a, cb);
		}
	}

	public class Class : ClassOrStruct {
		// <summary>
		//   Modifiers allowed in a class declaration
		// </summary>
		public const int AllowedModifiers =
			Modifiers.NEW |
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.PRIVATE |
			Modifiers.ABSTRACT |
			Modifiers.SEALED |
			Modifiers.UNSAFE;

		// Information in the case we are an attribute type
		AttributeUsageAttribute attribute_usage;

		public Class (NamespaceEntry ns, TypeContainer parent, MemberName name,
			      int mod, Attributes attrs, Location l)
			: base (ns, parent, name, attrs, Kind.Class, l)
		{
			int accmods;

			if (parent.Parent == null)
				accmods = Modifiers.INTERNAL;
			else
				accmods = Modifiers.PRIVATE;

			this.ModFlags = Modifiers.Check (AllowedModifiers, mod, accmods, l);
			attribute_usage = new AttributeUsageAttribute (AttributeTargets.All);
		}

		public override AttributeTargets AttributeTargets {
			get {
				return AttributeTargets.Class;
			}
		}

		public override void ApplyAttributeBuilder(Attribute a, CustomAttributeBuilder cb)
		{
			if (a.UsageAttribute != null)
				attribute_usage = a.UsageAttribute;

			base.ApplyAttributeBuilder (a, cb);
		}

		public AttributeUsageAttribute AttributeUsage {
			get {
				return attribute_usage;
			}
		}

		public override void Register ()
		{
			CheckDef (Parent.AddClass (this), Name, Location);
		}

		public const TypeAttributes DefaultTypeAttributes =
			TypeAttributes.AutoLayout | TypeAttributes.Class;

		//
		// FIXME: How do we deal with the user specifying a different
		// layout?
		//
		public override TypeAttributes TypeAttr {
			get {
				return base.TypeAttr | DefaultTypeAttributes;
			}
		}
	}

	public class Struct : ClassOrStruct {
		// <summary>
		//   Modifiers allowed in a struct declaration
		// </summary>
		public const int AllowedModifiers =
			Modifiers.NEW       |
			Modifiers.PUBLIC    |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL  |
			Modifiers.UNSAFE    |
			Modifiers.PRIVATE;

		public Struct (NamespaceEntry ns, TypeContainer parent, MemberName name,
			       int mod, Attributes attrs, Location l)
			: base (ns, parent, name, attrs, Kind.Struct, l)
		{
			int accmods;
			
			if (parent.Parent == null)
				accmods = Modifiers.INTERNAL;
			else
				accmods = Modifiers.PRIVATE;
			
			this.ModFlags = Modifiers.Check (AllowedModifiers, mod, accmods, l);

			this.ModFlags |= Modifiers.SEALED;
		}

		public override AttributeTargets AttributeTargets {
			get {
				return AttributeTargets.Struct;
			}
		}

		public override void Register ()
		{
			CheckDef (Parent.AddStruct (this), Name, Location);
		}

		public const TypeAttributes DefaultTypeAttributes =
			TypeAttributes.SequentialLayout |
			TypeAttributes.Sealed |
			TypeAttributes.BeforeFieldInit;

		//
		// FIXME: Allow the user to specify a different set of attributes
		// in some cases (Sealed for example is mandatory for a class,
		// but what SequentialLayout can be changed
		//
		public override TypeAttributes TypeAttr {
			get {
				return base.TypeAttr | DefaultTypeAttributes;
			}
		}
	}

	/// <summary>
	///   Interfaces
	/// </summary>
	public class Interface : TypeContainer, IMemberContainer {
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

		public Interface (NamespaceEntry ns, TypeContainer parent, MemberName name,
				  int mod, Attributes attrs, Location l)
			: base (ns, parent, name, attrs, Kind.Interface, l)
		{
			int accmods;

			if (parent.Parent == null)
				accmods = Modifiers.INTERNAL;
			else
				accmods = Modifiers.PRIVATE;

			this.ModFlags = Modifiers.Check (AllowedModifiers, mod, accmods, l);
		}

		public override void Register ()
		{
			CheckDef (Parent.AddInterface (this), Name, Location);
		}

		public override PendingImplementation GetPendingImplementations ()
		{
			return null;
		}

		public override AttributeTargets AttributeTargets {
			get {
				return AttributeTargets.Interface;
			}
		}

		public const TypeAttributes DefaultTypeAttributes =
					TypeAttributes.AutoLayout |
					TypeAttributes.Abstract |
					TypeAttributes.Interface;

		public override TypeAttributes TypeAttr {
			get {
				return base.TypeAttr | DefaultTypeAttributes;
			}
		}
	}

	public abstract class MethodCore : MemberBase {
		public readonly Parameters Parameters;
		public readonly GenericMethod GenericMethod;
		public readonly DeclSpace ds;
		protected Block block;
		
		//
		// Parameters, cached for semantic analysis.
		//
		protected InternalParameters parameter_info;
		protected Type [] parameter_types;

		// <summary>
		//   This is set from TypeContainer.DefineMembers if this method overrides something.
		// </summary>
		public bool OverridesSomething;

		// Whether this is an operator method.
		public bool IsOperator;

		static string[] attribute_targets = new string [] { "method", "return" };

		public MethodCore (TypeContainer parent, GenericMethod generic,
				   Expression type, int mod, int allowed_mod, bool is_iface,
				   MemberName name, Attributes attrs, Parameters parameters,
				   Location loc)
			: base (parent, type, mod, allowed_mod, Modifiers.PRIVATE, name,
				attrs, loc)
		{
			Parameters = parameters;
			IsInterface = is_iface;
			this.GenericMethod = generic;

			if (generic != null)
				ds = generic;
			else
				ds = parent;
		}
		
		//
		//  Returns the System.Type array for the parameters of this method
		//
		public Type [] ParameterTypes {
			get {
				return parameter_types;
			}
		}

		public InternalParameters ParameterInfo
		{
			get {
				return parameter_info;
			}
		}
		
		public Block Block {
			get {
				return block;
			}

			set {
				block = value;
			}
		}

		protected virtual bool DoDefineParameters ()
		{
			// Check if arguments were correct
			parameter_types = Parameters.GetParameterInfo (ds);
			if ((parameter_types == null) ||
			    !CheckParameters (ds, parameter_types))
				return false;

			parameter_info = new InternalParameters (ds, Parameters);

			Parameter array_param = Parameters.ArrayParameter;
			if ((array_param != null) &&
			    (!array_param.ParameterType.IsArray ||
			     (array_param.ParameterType.GetArrayRank () != 1))) {
				Report.Error (225, Location, "params parameter has to be a single dimensional array");
				return false;
			}

			return true;
		}

		void error_425 (Type old, Type t, string name)
		{
			Report.Error (425, Location,
				      "The constraints of type parameter `{0}' " +
				      "of method `{1}' must match the constraints for " +
				      "type parameter `{2}' of method `{3}'",
				      TypeManager.CSharpName (old), Name,
				      TypeManager.CSharpName (t), name);
		}

		protected override bool CheckGenericOverride (MethodInfo method, string name)
		{
			ParameterData pd = Invocation.GetParameterData (method);

			for (int i = 0; i < ParameterTypes.Length; i++) {
				GenericConstraints ogc = pd.GenericConstraints (i);
				GenericConstraints gc = ParameterInfo.GenericConstraints (i);

				if ((gc == null) && (ogc == null))
					continue;

				Type ot = pd.ParameterType (i);
				Type t = ParameterTypes [i];

				if (!((gc != null) && (ogc != null))) {
					error_425 (ot, t, name);
					return false;
				}

				if (gc.HasConstructor != ogc.HasConstructor) {
					error_425 (ot, t, name);
					return false;
				}

				if (ogc.HasClassConstraint != gc.HasClassConstraint) {
					error_425 (ot, t, name);
					return false;
				}

				if (ogc.HasClassConstraint &&
				    !ogc.ClassConstraint.Equals (gc.ClassConstraint)) {
					error_425 (ot, t, name);
					return false;
				}

				Type[] oct = ogc.InterfaceConstraints;
				Type[] ct = gc.InterfaceConstraints;

				if (oct.Length != ct.Length) {
					error_425 (ot, t, name);
					return false;
				}

				for (int j = 0; j < oct.Length; j++)
					if (!oct [j].Equals (ct [j])) {
						error_425 (ot, t, name);
						return false;
					}
			}

			return true;
		}

		protected override string[] ValidAttributeTargets {
			get {
				return attribute_targets;
			}
		}

		protected override bool VerifyClsCompliance (DeclSpace ds)
		{
			if (!base.VerifyClsCompliance (ds)) {
				if ((ModFlags & Modifiers.ABSTRACT) != 0 && IsExposedFromAssembly (ds) && ds.IsClsCompliaceRequired (ds)) {
					Report.Error_T (3011, Location, GetSignatureForError ());
				}
				return false;
			}

			if (Parameters.HasArglist) {
				// "Methods with variable arguments are not CLS-compliant"
				Report.Error_T (3000, Location);
			}

			AttributeTester.AreParametersCompliant (Parameters.FixedParameters, Location);

			if (!AttributeTester.IsClsCompliant (MemberType)) {
				Report.Error_T (3002, Location, GetSignatureForError ());
			}

			return true;
		}

		protected bool IsDuplicateImplementation (TypeContainer tc, MethodCore method)
		{
			if ((method == this) || (method.Name != Name))
				return false;

			Type[] param_types = method.ParameterTypes;
			if (param_types == null)
				return false;

			if (param_types.Length != ParameterTypes.Length)
				return false;

			int type_params = 0;
			if (GenericMethod != null)
				type_params = GenericMethod.CountTypeParameters;

			int m_type_params = 0;
			if (method.GenericMethod != null)
				m_type_params = method.GenericMethod.CountTypeParameters;

			if (type_params != m_type_params)
				return false;

			bool equal = true;
			bool may_unify;

			Type[] infered_types;
			if (type_params > 0)
				infered_types = new Type [type_params];
			else
				infered_types = null;

			may_unify = Invocation.InferTypeArguments (
				param_types, ParameterTypes, ref infered_types);

			if (!may_unify) {
				if (type_params > 0)
					infered_types = new Type [type_params];
				else
					infered_types = null;

				may_unify = Invocation.InferTypeArguments (
					ParameterTypes, param_types, ref infered_types);
			}

			for (int i = 0; i < param_types.Length; i++) {
				Type a = param_types [i];
				Type b = ParameterTypes [i];

				if (a != b)
					equal = false;
			}

			if (equal) {
				//
				// Try to report 663: method only differs on out/ref
				//
				ParameterData info = ParameterInfo;
				ParameterData other_info = method.ParameterInfo;
				for (int i = 0; i < info.Count; i++){
					if (info.ParameterModifier (i) != other_info.ParameterModifier (i)){
						Report.Error (663, Location,
							      "Overload method only differs " +
							      "in parameter modifier");
						return false;
					}
				}

				Report.Error (111, Location,
					      "Class `{0}' already defines a member called " +
					      "`{1}' with the same parameter types",
					      tc.Name, Name);
				return true;
			} else if (may_unify) {
				Report.Error (408, Location,
					      "`{0}' cannot define overload members that " +
					      "may unify for some type parameter substitutions",
					      tc.Name);
				return true;
			}

			return false;
		}

		public CallingConventions GetCallingConvention (bool is_class)
		{
			CallingConventions cc = 0;
			
			cc = Parameters.GetCallingConvention ();

			if (is_class)
				if ((ModFlags & Modifiers.STATIC) == 0)
					cc |= CallingConventions.HasThis;

			// FIXME: How is `ExplicitThis' used in C#?
			
			return cc;
		}

		protected override void VerifyObsoleteAttribute()
		{
			base.VerifyObsoleteAttribute ();

			if (parameter_types == null)
				return;

			foreach (Type type in parameter_types) {
				CheckUsageOfObsoleteAttribute (type);
			}
		}
	}

	public class SourceMethod : ISourceMethod
	{
		TypeContainer container;
		MethodBase builder;

		protected SourceMethod (TypeContainer container, MethodBase builder,
					ISourceFile file, Location start, Location end)
		{
			this.container = container;
			this.builder = builder;
			
			CodeGen.SymbolWriter.OpenMethod (
				file, this, start.Row, 0, end.Row, 0);
		}

		public string Name {
			get { return builder.Name; }
		}

		public int NamespaceID {
			get { return container.NamespaceEntry.SymbolFileID; }
		}

		public int Token {
			get {
				if (builder is MethodBuilder)
					return ((MethodBuilder) builder).GetToken ().Token;
				else if (builder is ConstructorBuilder)
					return ((ConstructorBuilder) builder).GetToken ().Token;
				else
					throw new NotSupportedException ();
			}
		}

		public void CloseMethod ()
		{
			if (CodeGen.SymbolWriter != null)
				CodeGen.SymbolWriter.CloseMethod ();
		}

		public static SourceMethod Create (TypeContainer parent,
						   MethodBase builder, Block block)
		{
			if (CodeGen.SymbolWriter == null)
				return null;
			if (block == null)
				return null;

			Location start_loc = block.StartLocation;
			if (Location.IsNull (start_loc))
				return null;

			Location end_loc = block.EndLocation;
			if (Location.IsNull (end_loc))
				return null;

			ISourceFile file = start_loc.SourceFile;
			if (file == null)
				return null;

			return new SourceMethod (
				parent, builder, file, start_loc, end_loc);
		}
	}

	public class Method : MethodCore, IIteratorContainer, IMethodData {
		public MethodBuilder MethodBuilder;
		public MethodData MethodData;
		ReturnParameter return_attributes;

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
			Modifiers.METHOD_YIELDS | 
			Modifiers.EXTERN;

		const int AllowedInterfaceModifiers =
			Modifiers.NEW | Modifiers.UNSAFE;

		//
		// return_type can be "null" for VOID values.
		//
		public Method (TypeContainer parent, GenericMethod generic,
			       Expression return_type, int mod, bool is_iface,
			       MemberName name, Parameters parameters, Attributes attrs,
			       Location l)
			: base (parent, generic, return_type, mod,
				is_iface ? AllowedInterfaceModifiers : AllowedModifiers,
				is_iface, name, attrs, parameters, l)
		{
		}

		public override AttributeTargets AttributeTargets {
			get {
				return AttributeTargets.Method | AttributeTargets.ReturnValue;
			}
		}
		
		//
		// Returns the `System.Type' for the ReturnType of this
		// function.  Provides a nice cache.  (used between semantic analysis
		// and actual code generation
		//
		public Type GetReturnType ()
		{
			return MemberType;
		}

		public override string GetSignatureForError()
		{
			return TypeManager.CSharpSignature (MethodBuilder);
		}

		/// <summary>
		/// Use this method when MethodBuilder is null
		/// </summary>
		public override string GetSignatureForError (TypeContainer tc)
		{
			// TODO: move to parameters
			System.Text.StringBuilder args = new System.Text.StringBuilder ();
			if (parameter_info.Parameters.FixedParameters != null) {
				for (int i = 0; i < parameter_info.Parameters.FixedParameters.Length; ++i) {
					Parameter p = parameter_info.Parameters.FixedParameters [i];
					args.Append (p.GetSignatureForError ());

					if (i < parameter_info.Parameters.FixedParameters.Length - 1)
						args.Append (',');
				}
			}

			return String.Concat (base.GetSignatureForError (tc), "(", args.ToString (), ")");
		}

                void DuplicateEntryPoint (MethodInfo b, Location location)
                {
                        Report.Error (
                                17, location,
                                "Program `" + CodeGen.FileName +
                                "'  has more than one entry point defined: `" +
                                TypeManager.CSharpSignature(b) + "'");
                }

                void Report28 (MethodInfo b)
                {
                        Report.Warning (
                                28, Location,
                                "`" + TypeManager.CSharpSignature(b) +
                                "' has the wrong signature to be an entry point");
                }

                public bool IsEntryPoint (MethodBuilder b, InternalParameters pinfo)
                {
                        if (b.ReturnType != TypeManager.void_type &&
                            b.ReturnType != TypeManager.int32_type)
                                return false;

                        if (pinfo.Count == 0)
                                return true;

                        if (pinfo.Count > 1)
                                return false;

                        Type t = pinfo.ParameterType(0);
                        if (t.IsArray &&
                            (t.GetArrayRank() == 1) &&
                            (TypeManager.GetElementType(t) == TypeManager.string_type) &&
                            (pinfo.ParameterModifier(0) == Parameter.Modifier.NONE))
                                return true;
                        else
                                return false;
                }

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb)
		{
			if (a.Target == "return") {
				if (return_attributes == null)
					return_attributes = new ReturnParameter (MethodBuilder, Location);

				return_attributes.ApplyAttributeBuilder (a, cb);
				return;
			}

			if (a.Type == TypeManager.methodimpl_attr_type && a.IsInternalCall) {
				MethodBuilder.SetImplementationFlags (MethodImplAttributes.InternalCall | MethodImplAttributes.Runtime);
			}

			if (a.Type == TypeManager.dllimport_type) {
				const int extern_static = Modifiers.EXTERN | Modifiers.STATIC;
				if ((ModFlags & extern_static) != extern_static) {
					//"The DllImport attribute must be specified on a method marked `static' and `extern'"
					Report.Error_T (601, a.Location);
				}

				return;
			}

			if (a.Type == TypeManager.conditional_attribute_type) {
				if (IsOperator || IsExplicitImpl) {
					// Conditional not valid on '{0}' because it is a destructor, operator, or explicit interface implementation
					Report.Error_T (577, Location, GetSignatureForError ());
					return;
				}

				if (ReturnType != TypeManager.void_type) {
					// Conditional not valid on '{0}' because its return type is not void
					Report.Error_T (578, Location, GetSignatureForError ());
					return;
				}

				if ((ModFlags & Modifiers.OVERRIDE) != 0) {
					// Conditional not valid on '{0}' because it is an override method
					Report.Error_T (243, Location, GetSignatureForError ());
					return;
				}

				if (IsInterface) {
					// Conditional not valid on interface members
					Report.Error_T (582, Location);
					return;
				}

				if (MethodData.IsImplementing) {
					// Conditional member '{0}' cannot implement interface member
					Report.Error_T (629, Location, GetSignatureForError ());
					return;
				}
			}

			MethodBuilder.SetCustomAttribute (cb);
		}

		//
		// Checks our base implementation if any
		//
		protected override bool CheckBase ()
		{
			base.CheckBase ();
			
			// Check whether arguments were correct.
			if (!DoDefineParameters ())
				return false;

			MethodSignature ms = new MethodSignature (Name, null, ParameterTypes);
			if (IsOperator) {
				flags |= MethodAttributes.SpecialName | MethodAttributes.HideBySig;
			} else {
				//
				// Check in our class for dups
				//
				ArrayList ar = Parent.Methods;
				if (ar != null) {
					int arLen = ar.Count;

					for (int i = 0; i < arLen; i++) {
						Method m = (Method) ar [i];
						if (IsDuplicateImplementation (Parent, m))
 							return false;
					}
				}
			}


			//
			// Verify if the parent has a type with the same name, and then
			// check whether we have to create a new slot for it or not.
			//
			Type ptype = Parent.TypeBuilder.BaseType;

			// ptype is only null for System.Object while compiling corlib.
			if (ptype != null) {
				
				//
				// Explicit implementations do not have `parent' methods, however,
				// the member cache stores them there. Without this check, we get
				// an incorrect warning in corlib.
				//
				if (! IsExplicitImpl) {
					parent_method = (MethodInfo)((IMemberContainer)Parent).Parent.MemberCache.FindMemberToOverride (
						Parent.TypeBuilder, Name, ParameterTypes, false);
				}
				
				if (parent_method != null) {
					string name = parent_method.DeclaringType.Name + "." +
						parent_method.Name;

					if (!CheckMethodAgainstBase (Parent, flags, parent_method, name))
						return false;

					if ((ModFlags & Modifiers.NEW) == 0) {
						Type parent_ret = TypeManager.TypeToCoreType (
							parent_method.ReturnType);

						if (!parent_ret.Equals (MemberType)) {
							Report.Error (
								508, Location, Parent.MakeName (Name) + ": cannot " +
								"change return type when overriding " +
								"inherited member " + name);
							return false;
						}
					}

					ObsoleteAttribute oa = AttributeTester.GetMethodObsoleteAttribute (parent_method);
					if (oa != null) {
						Report.SymbolRelatedToPreviousError (parent_method);
						Report.Warning_T (672, Location, GetSignatureForError (Parent));
					}
				} else {
					if (!OverridesSomething && ((ModFlags & Modifiers.NEW) != 0))
						WarningNotHiding (Parent);

					if ((ModFlags & Modifiers.OVERRIDE) != 0){
						Report.Error (115, Location,
							      Parent.MakeName (Name) +
							      " no suitable methods found to override");
					}
				}
			} else if ((ModFlags & Modifiers.NEW) != 0)
				WarningNotHiding (Parent);

			return true;
		}

		//
		// Creates the type
		//
		public override bool Define ()
		{
			if (!DoDefineBase ())
				return false;

			MethodBuilder mb = null;
			if (GenericMethod != null) {
				string mname = MemberName.GetMethodName ();
				mb = Parent.TypeBuilder.DefineGenericMethod (mname, flags);
				if (!GenericMethod.Define (mb))
					return false;
			}

			if (!DoDefine (ds))
				return false;

			if (!CheckBase ())
				return false;

			MethodData = new MethodData (this, ParameterInfo, ModFlags, flags,
						     this, mb, GenericMethod);

			if (!MethodData.Define (Parent))
				return false;

			//
			// Setup iterator if we are one
			//
			if ((ModFlags & Modifiers.METHOD_YIELDS) != 0){
				Iterator iterator = new Iterator (
					Parent, Name, MemberType, ParameterTypes,
					ParameterInfo, ModFlags, block, Location);

				if (!iterator.DefineIterator ())
					return false;

				block = iterator.Block;
			}

			MethodBuilder = MethodData.MethodBuilder;

			//
			// This is used to track the Entry Point,
			//
			if (Name == "Main" &&
			    ((ModFlags & Modifiers.STATIC) != 0) && RootContext.NeedsEntryPoint && 
			    (RootContext.MainClass == null ||
			     RootContext.MainClass == Parent.TypeBuilder.FullName)){
                                if (IsEntryPoint (MethodBuilder, ParameterInfo)) {
                                        if (RootContext.EntryPoint == null) {
						if (Parent.IsGeneric){
							Report.Error (-201, Location,
								      "Entry point can not be defined in a generic class");
						}
						
                                                RootContext.EntryPoint = MethodBuilder;
                                                RootContext.EntryPointLocation = Location;
                                        } else {
                                                DuplicateEntryPoint (RootContext.EntryPoint, RootContext.EntryPointLocation);
                                                DuplicateEntryPoint (MethodBuilder, Location);
                                        }
                                } else                                 	
                               	        Report28(MethodBuilder);
			}

			return true;
		}

		//
		// Emits the code
		// 
		public override void Emit ()
		{
			MethodData.Emit (Parent, this);
			base.Emit ();
			Block = null;
			MethodData = null;
		}

		void IIteratorContainer.SetYields ()
		{
			ModFlags |= Modifiers.METHOD_YIELDS;
		}
	
		protected override bool IsIdentifierClsCompliant (DeclSpace ds)
		{
			return IsIdentifierAndParamClsCompliant (ds, Name, MethodBuilder, parameter_types);
		}

		#region IMethodData Members

		public CallingConventions CallingConventions {
			get {
				CallingConventions cc = Parameters.GetCallingConvention ();
				if (Parameters.HasArglist)
					block.HasVarargs = true;

				if (!IsInterface)
					if ((ModFlags & Modifiers.STATIC) == 0)
						cc |= CallingConventions.HasThis;

				// FIXME: How is `ExplicitThis' used in C#?
			
				return cc;
			}
		}

		public Type ReturnType {
			get {
				return MemberType;
			}
		}

		public string MethodName {
			get {
				return ShortName;
			}
		}

		public new Location Location {
			get {
				return base.Location;
			}
		}

		public EmitContext CreateEmitContext (TypeContainer tc, ILGenerator ig)
		{
			return new EmitContext (
				tc, ds, Location, ig, ReturnType, ModFlags, false);
		}

		public ObsoleteAttribute GetObsoleteAttribute ()
		{
			return GetObsoleteAttribute (ds);
		}

		/// <summary>
		/// Returns true if method has conditional attribute and the conditions is not defined (method is excluded).
		/// </summary>
		public bool IsExcluded (EmitContext ec)
		{
			if ((caching_flags & Flags.Excluded_Undetected) == 0)
				return (caching_flags & Flags.Excluded) != 0;

			caching_flags &= ~Flags.Excluded_Undetected;

			if (parent_method == null) {
				if (OptAttributes == null)
					return false;

				Attribute[] attrs = OptAttributes.SearchMulti (TypeManager.conditional_attribute_type, ec);

				if (attrs == null)
					return false;

				foreach (Attribute a in attrs) {
					string condition = a.GetConditionalAttributeValue (ds);
					if (RootContext.AllDefines.Contains (condition))
						return false;
				}

				caching_flags |= Flags.Excluded;
				return true;
			}

			IMethodData md = TypeManager.GetMethod (parent_method);
			if (md == null) {
				if (AttributeTester.IsConditionalMethodExcluded (parent_method)) {
					caching_flags |= Flags.Excluded;
					return true;
				}
				return false;
			}

			if (md.IsExcluded (ec)) {
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
		#endregion
	}

	public abstract class ConstructorInitializer {
		ArrayList argument_list;
		protected ConstructorInfo parent_constructor;
		Parameters parameters;
		Location loc;
		
		public ConstructorInitializer (ArrayList argument_list, Parameters parameters,
					       Location loc)
		{
			this.argument_list = argument_list;
			this.parameters = parameters;
			this.loc = loc;
		}

		public ArrayList Arguments {
			get {
				return argument_list;
			}
		}

		public bool Resolve (ConstructorBuilder caller_builder, EmitContext ec)
		{
			Expression parent_constructor_group;
			Type t;

			ec.CurrentBlock = new ToplevelBlock (Block.Flags.Implicit, parameters, loc);

			if (argument_list != null){
				foreach (Argument a in argument_list){
					if (!a.Resolve (ec, loc))
						return false;
				}
			}
			ec.CurrentBlock = null;

			if (this is ConstructorBaseInitializer) {
				if (ec.ContainerType.BaseType == null)
					return true;

				t = ec.ContainerType.BaseType;
				if (ec.ContainerType.IsValueType) {
					Report.Error (522, loc,
						"structs cannot call base class constructors");
					return false;
				}
			} else
				t = ec.ContainerType;

			parent_constructor_group = Expression.MemberLookup (
				ec, t, ".ctor", MemberTypes.Constructor,
				BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly,
				loc);
			
			if (parent_constructor_group == null){
				parent_constructor_group = Expression.MemberLookup (
					ec, t, ".ctor", MemberTypes.Constructor,
					BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly,
					loc);

				if (parent_constructor_group != null)
					Report.Error (
						112, loc, "`{0}.{1}' is inaccessible due to " +
						"its protection level", t.FullName, t.Name);
				else
					Report.Error (
						1501, loc, "Can not find a constructor for " +
						"this argument list");
				return false;
			}
			
			parent_constructor = (ConstructorInfo) Invocation.OverloadResolve (
				ec, (MethodGroupExpr) parent_constructor_group, argument_list,
				false, loc);
			
			if (parent_constructor == null){
				Report.Error (1501, loc,
				       "Can not find a constructor for this argument list");
				return false;
			}
			
			if (parent_constructor == caller_builder){
				Report.Error (515, String.Format ("Constructor `{0}' can not call itself", TypeManager.CSharpSignature (caller_builder)));
				return false;
			}
			
			return true;
		}

		public void Emit (EmitContext ec)
		{
			if (parent_constructor != null){
				ec.Mark (loc, false);
				if (ec.IsStatic)
					Invocation.EmitCall (ec, true, true, null, parent_constructor, argument_list, loc);
				else
					Invocation.EmitCall (ec, true, false, ec.GetThis (loc), parent_constructor, argument_list, loc);
			}
		}

		/// <summary>
		/// Method search for base ctor. (We do not cache it).
		/// </summary>
		Constructor GetOverloadedConstructor (TypeContainer tc)
		{
			if (tc.InstanceConstructors == null)
				return null;

			foreach (Constructor c in tc.InstanceConstructors) {
				if (Arguments == null) {
					if (c.ParameterTypes.Length == 0)
						return c;

					continue;
				}

				bool ok = true;

				int count = c.ParameterInfo.Count;
				if ((count > 0) &&
				    c.ParameterInfo.ParameterModifier (count - 1) == Parameter.Modifier.PARAMS) {
					for (int i = 0; i < count-1; i++)
						if (c.ParameterTypes [i] != ((Argument)Arguments [i]).Type) {
							ok = false;
							break;
						}
				} else {
					if (c.ParameterTypes.Length != Arguments.Count)
						continue;

					for (int i = 0; i < Arguments.Count; ++i)
						if (c.ParameterTypes [i] != ((Argument)Arguments [i]).Type) {
							ok = false;
							break;
						}
				}

				if (!ok)
					continue;

				return c;
			}

			return null;
		}

		//TODO: implement caching when it will be necessary
		public virtual void CheckObsoleteAttribute (TypeContainer tc, Location loc)
		{
			Constructor ctor = GetOverloadedConstructor (tc);
			if (ctor == null)
				return;

			ObsoleteAttribute oa = ctor.GetObsoleteAttribute (tc);
			if (oa == null)
				return;

			AttributeTester.Report_ObsoleteMessage (oa, ctor.GetSignatureForError (), loc);
		}
	}

	public class ConstructorBaseInitializer : ConstructorInitializer {
		public ConstructorBaseInitializer (ArrayList argument_list, Parameters pars, Location l) :
			base (argument_list, pars, l)
		{
		}

		public override void CheckObsoleteAttribute(TypeContainer tc, Location loc) {
			if (parent_constructor == null)
				return;

			TypeContainer type_ds = TypeManager.LookupTypeContainer (tc.TypeBuilder.BaseType);
			if (type_ds == null) {
				ObsoleteAttribute oa = AttributeTester.GetMemberObsoleteAttribute (parent_constructor);

				if (oa != null)
					AttributeTester.Report_ObsoleteMessage (oa, TypeManager.CSharpSignature (parent_constructor), loc);

				return;
			}

			base.CheckObsoleteAttribute (type_ds, loc);
		}

	}

	public class ConstructorThisInitializer : ConstructorInitializer {
		public ConstructorThisInitializer (ArrayList argument_list, Parameters pars, Location l) :
			base (argument_list, pars, l)
		{
		}
	}
	
	public class Constructor : MethodCore {
		public ConstructorBuilder ConstructorBuilder;
		public ConstructorInitializer Initializer;

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

		bool has_compliant_args = false;
		//
		// The spec claims that static is not permitted, but
		// my very own code has static constructors.
		//
		public Constructor (TypeContainer ds, string name, int mod, Parameters args,
				    ConstructorInitializer init, Location l)
			: base (ds, null, null, mod, AllowedModifiers, false,
				new MemberName (name), null, args, l)
		{
			Initializer = init;
		}

		public override string GetSignatureForError()
		{
			return TypeManager.CSharpSignature (ConstructorBuilder);
		}

		public bool HasCompliantArgs {
			get {
				return has_compliant_args;
			}
		}

		public override AttributeTargets AttributeTargets {
			get {
				return AttributeTargets.Constructor;
			}
		}


		//
		// Returns true if this is a default constructor
		//
		public bool IsDefault ()
		{
			if ((ModFlags & Modifiers.STATIC) != 0)
				return  (Parameters.FixedParameters == null ? true : Parameters.Empty) &&
					(Parameters.ArrayParameter == null ? true : Parameters.Empty);
			
			else
				return  (Parameters.FixedParameters == null ? true : Parameters.Empty) &&
					(Parameters.ArrayParameter == null ? true : Parameters.Empty) &&
					(Initializer is ConstructorBaseInitializer) &&
					(Initializer.Arguments == null);
		}

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb)
		{
			ConstructorBuilder.SetCustomAttribute (cb);
		}

		protected override bool CheckBase ()
		{
			base.CheckBase ();
			
			// Check whether arguments were correct.
			if (!DoDefineParameters ())
				return false;
			
			if ((ModFlags & Modifiers.STATIC) != 0)
				return true;
			
			if (Parent.Kind == Kind.Struct && ParameterTypes.Length == 0) {
				Report.Error (568, Location, 
					"Structs can not contain explicit parameterless " +
					"constructors");
				return false;
			}
				
			//
			// Check in our class for dups
			//
			ArrayList ar = Parent.InstanceConstructors;
			if (ar != null) {
				int arLen = ar.Count;
					
				for (int i = 0; i < arLen; i++) {
					Constructor m = (Constructor) ar [i];
					if (IsDuplicateImplementation (Parent, m))
						return false;
				}
			}
			
			return true;
		}
		
		//
		// Creates the ConstructorBuilder
		//
		public override bool Define ()
		{
			MethodAttributes ca = (MethodAttributes.RTSpecialName |
					       MethodAttributes.SpecialName);

			if ((ModFlags & Modifiers.STATIC) != 0){
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
				else if (IsDefault ())
					ca |= MethodAttributes.Public;
				else
					ca |= MethodAttributes.Private;
			}

			// Check if arguments were correct.
			if (!CheckBase ())
				return false;

			ConstructorBuilder = Parent.TypeBuilder.DefineConstructor (
				ca, GetCallingConvention (Parent.Kind == Kind.Class),
				ParameterTypes);

			if ((ModFlags & Modifiers.UNSAFE) != 0)
				ConstructorBuilder.InitLocals = false;
			
			//
			// HACK because System.Reflection.Emit is lame
			//
			TypeManager.RegisterMethod (ConstructorBuilder, ParameterInfo, ParameterTypes);

			return true;
		}

		//
		// Emits the code
		//
		public override void Emit ()
		{
			ILGenerator ig = ConstructorBuilder.GetILGenerator ();
			EmitContext ec = new EmitContext (Parent, Location, ig, null, ModFlags, true);

			//
			// extern methods have no bodies
			//
			if ((ModFlags & Modifiers.EXTERN) != 0) {
				if ((block != null) && ((ModFlags & Modifiers.EXTERN) != 0)) {
					Report.Error (
						179, Location, "External constructor `" +
						TypeManager.CSharpSignature (ConstructorBuilder) +
						"' can not have a body");
					return;
				}
			} else if (block == null) {
				Report.Error (
					501, Location, "Constructor `" +
					TypeManager.CSharpSignature (ConstructorBuilder) +
					"' must declare a body since it is not marked extern");
				return;
			}

			if ((ModFlags & Modifiers.STATIC) == 0){
				if (Parent.Kind == Kind.Class && Initializer == null)
					Initializer = new ConstructorBaseInitializer (
						null, Parameters.EmptyReadOnlyParameters, Location);


				//
				// Spec mandates that Initializers will not have
				// `this' access
				//
				ec.IsStatic = true;
				if (Initializer != null && !Initializer.Resolve (ConstructorBuilder, ec))
					return;
				ec.IsStatic = false;
			}

			Parameters.LabelParameters (ec, ConstructorBuilder, Location);
			
			SourceMethod source = SourceMethod.Create (
				Parent, ConstructorBuilder, block);

			//
			// Classes can have base initializers and instance field initializers.
			//
			if (Parent.Kind == Kind.Class){
				if ((ModFlags & Modifiers.STATIC) == 0){

					//
					// If we use a "this (...)" constructor initializer, then
					// do not emit field initializers, they are initialized in the other constructor
					//
					if (!(Initializer != null && Initializer is ConstructorThisInitializer))
						Parent.EmitFieldInitializers (ec);
				}
			}
			if (Initializer != null) {
				Initializer.CheckObsoleteAttribute (Parent, Location);
				Initializer.Emit (ec);
			}
			
			if ((ModFlags & Modifiers.STATIC) != 0)
				Parent.EmitFieldInitializers (ec);

			if (OptAttributes != null) 
				OptAttributes.Emit (ec, this);

			// If this is a non-static `struct' constructor and doesn't have any
			// initializer, it must initialize all of the struct's fields.
			if ((Parent.Kind == Kind.Struct) &&
			    ((ModFlags & Modifiers.STATIC) == 0) && (Initializer == null))
				Block.AddThisVariable (Parent, Location);

			ec.EmitTopBlock (block, ParameterInfo, Location);

			if (source != null)
				source.CloseMethod ();

			base.Emit ();

			block = null;
		}

		// For constructors is needed to test only parameters
		protected override bool IsIdentifierClsCompliant (DeclSpace ds)
		{
			if (parameter_types == null || parameter_types.Length == 0)
				return true;

			TypeContainer tc = ds as TypeContainer;

			for (int i = 0; i < tc.InstanceConstructors.Count; i++) {
				Constructor c = (Constructor) tc.InstanceConstructors [i];
						
				if (c == this || c.ParameterTypes.Length == 0)
					continue;

				if (!c.IsClsCompliaceRequired (ds))
					continue;
				
				if (!AttributeTester.AreOverloadedMethodParamsClsCompliant (parameter_types, c.ParameterTypes)) {
					Report.Error_T (3006, Location, GetSignatureForError ());
					return false;
				}
			}

			if (tc.TypeBuilder.BaseType == null)
				return true;

			DeclSpace temp_ds = TypeManager.LookupDeclSpace (tc.TypeBuilder.BaseType);
			if (temp_ds != null)
				return IsIdentifierClsCompliant (temp_ds);

			MemberInfo[] ml = tc.TypeBuilder.BaseType.FindMembers (MemberTypes.Constructor, BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance, null, null);
			// Skip parameter-less ctor
			if (ml.Length < 2)
				return true;

			foreach (ConstructorInfo ci in ml) {
				object[] cls_attribute = ci.GetCustomAttributes (TypeManager.cls_compliant_attribute_type, false);
				if (cls_attribute.Length == 1 && (!((CLSCompliantAttribute)cls_attribute[0]).IsCompliant))
					continue;

				if (!AttributeTester.AreOverloadedMethodParamsClsCompliant (parameter_types, TypeManager.GetArgumentTypes (ci))) {
					Report.Error_T (3006, Location, GetSignatureForError ());
					return false;
				}
			}
			
			return true;
		}

		protected override bool VerifyClsCompliance (DeclSpace ds)
		{
			if (!base.VerifyClsCompliance (ds) || !IsExposedFromAssembly (ds)) {
				return false;
			}
			
			if (ds.TypeBuilder.IsSubclassOf (TypeManager.attribute_type)) {
				foreach (Type param in parameter_types) {
					if (param.IsArray) {
						return false;
					}
				}
			}
			has_compliant_args = true;
			return true;
		}

	}

	/// <summary>
	/// Interface for MethodData class. Holds links to parent members to avoid member duplication.
	/// </summary>
	public interface IMethodData
	{
		CallingConventions CallingConventions { get; }
		Location Location { get; }
		string MethodName { get; }
		Type[] ParameterTypes { get; }
		Type ReturnType { get; }
		GenericMethod GenericMethod { get; }

		Attributes OptAttributes { get; }
		Block Block { get; }

		EmitContext CreateEmitContext (TypeContainer tc, ILGenerator ig);
		ObsoleteAttribute GetObsoleteAttribute ();
		string GetSignatureForError (TypeContainer tc);
		bool IsExcluded (EmitContext ec);
	}

	//
	// Encapsulates most of the Method's state
	//
	public class MethodData {

		readonly IMethodData method;

		//
		// The return type of this method
		//
		public readonly GenericMethod GenericMethod;
		public readonly InternalParameters ParameterInfo;

		//
		// Are we implementing an interface ?
		//
		public bool IsImplementing = false;

		//
		// Protected data.
		//
		protected MemberBase member;
		protected int modifiers;
		protected MethodAttributes flags;
		protected Type declaring_type;

		EmitContext ec;

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

		public MethodData (MemberBase member, InternalParameters parameters,
				   int modifiers, MethodAttributes flags, IMethodData method)
		{
			this.member = member;
			this.ParameterInfo = parameters;
			this.modifiers = modifiers;
			this.flags = flags;

			this.method = method;
		}

		public MethodData (MemberBase member, InternalParameters parameters,
				   int modifiers, MethodAttributes flags, 
				   IMethodData method, MethodBuilder builder,
				   GenericMethod generic)
			: this (member, parameters, modifiers, flags, method)
		{
			this.builder = builder;
			this.GenericMethod = generic;
		}

		static string RemoveArity (string name)
		{
			int start = 0;
			StringBuilder sb = new StringBuilder ();
			while (start < name.Length) {
				int pos = name.IndexOf ('`', start);
				if (pos < 0) {
					sb.Append (name.Substring (start));
					break;
				}

				sb.Append (name.Substring (start, pos-start));

				pos++;
				while ((pos < name.Length) && Char.IsNumber (name [pos]))
					pos++;

				start = pos;
			}

			return sb.ToString ();
		}

		public bool Define (TypeContainer container)
		{
			MethodInfo implementing = null;
			string prefix;

			if (member.IsExplicitImpl)
				prefix = RemoveArity (member.InterfaceType.FullName) + ".";
			else
				prefix = "";

			string name = method.MethodName;
			string method_name = prefix + name;
			Type[] ParameterTypes = method.ParameterTypes;

			if (container.Pending != null){
				if (member is Indexer)
					implementing = container.Pending.IsInterfaceIndexer (
						member.InterfaceType, method.ReturnType, ParameterTypes);
				else
					implementing = container.Pending.IsInterfaceMethod (
						member.InterfaceType, name, method.ReturnType, ParameterTypes);

				if (member.InterfaceType != null && implementing == null){
					Report.Error (539, method.Location, "'{0}' in explicit interface declaration is not an interface", method_name);
					return false;
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
					if ((modifiers & (Modifiers.PUBLIC | Modifiers.ABSTRACT | Modifiers.VIRTUAL)) != 0){
						Modifiers.Error_InvalidModifier (method.Location, "public, virtual or abstract");
						implementing = null;
					}
				} else if ((flags & MethodAttributes.MemberAccessMask) != MethodAttributes.Public){
					if (TypeManager.IsInterfaceType (implementing.DeclaringType)){
						//
						// If this is an interface method implementation,
						// check for public accessibility
						//
						implementing = null;
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
					Modifiers.Error_InvalidModifier (method.Location, "static");
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

				// Get the method name from the explicit interface.
				if (member.InterfaceType != null) {
					name = implementing.Name;
					method_name = prefix + name;
				}

				IsImplementing = true;
			}

			EmitContext ec = method.CreateEmitContext (container, null);

			DefineMethodBuilder (ec, container, method_name, ParameterTypes);

			if (builder == null)
				return false;

			if (GenericMethod != null) {
				if (!GenericMethod.DefineType (ec, builder))
					return false;
			}

			if (container.CurrentType != null)
				declaring_type = container.CurrentType.ResolveType (ec);
			else
				declaring_type = container.TypeBuilder;

			if ((modifiers & Modifiers.UNSAFE) != 0)
				builder.InitLocals = false;

			if (IsImplementing){
				//
				// clear the pending implemntation flag
				//
				if (member is Indexer) {
					container.Pending.ImplementIndexer (
						member.InterfaceType, builder, method.ReturnType,
						ParameterTypes, true);
				} else
					container.Pending.ImplementMethod (
						member.InterfaceType, name, method.ReturnType,
						ParameterTypes, member.IsExplicitImpl);

				if (member.IsExplicitImpl)
					container.TypeBuilder.DefineMethodOverride (
						builder, implementing);

			}

			if (!TypeManager.RegisterMethod (builder, ParameterInfo, ParameterTypes)) {
				Report.Error (111, method.Location,
					      "Class `" + container.Name +
					      "' already contains a definition with the " +
					      "same return value and parameter types as the " +
					      "'get' method of property `" + member.Name + "'");
				return false;
			}

			TypeManager.AddMethod (builder, method);

			return true;
		}

		/// <summary>
		/// Create the MethodBuilder for the method 
		/// </summary>
		void DefineMethodBuilder (EmitContext ec, TypeContainer container, string method_name, Type[] ParameterTypes)
		{
			const int extern_static = Modifiers.EXTERN | Modifiers.STATIC;

			if ((modifiers & extern_static) == extern_static) {

				if (method.OptAttributes != null) {
					Attribute dllimport_attribute = method.OptAttributes.Search (TypeManager.dllimport_type, ec);
					if (dllimport_attribute != null) {
						flags |= MethodAttributes.PinvokeImpl;
						builder = dllimport_attribute.DefinePInvokeMethod (
							ec, container.TypeBuilder, method_name, flags,
							method.ReturnType, ParameterTypes);

						return;
					}
				}

				// for extern static method must be specified either DllImport attribute or MethodImplAttribute.
				// We are more strict than Microsoft and report CS0626 like error
				if (method.OptAttributes == null ||
					!method.OptAttributes.Contains (TypeManager.methodimpl_attr_type, ec)) {
					//"Method, operator, or accessor '{0}' is marked external and has no attributes on it. Consider adding a DllImport attribute to specify the external implementation"
					Report.Error_T (626, method.Location, method.GetSignatureForError (container));
					return;
				}
			}

			if (builder == null)
				builder = container.TypeBuilder.DefineMethod (
					method_name, flags, method.CallingConventions,
					method.ReturnType, ParameterTypes);
			else
				builder.SetGenericMethodSignature (
					flags, method.CallingConventions,
					method.ReturnType, ParameterTypes);
		}

		//
		// Emits the code
		// 
		public void Emit (TypeContainer container, Attributable kind)
		{
			EmitContext ec;
			if ((flags & MethodAttributes.PinvokeImpl) == 0)
				ec = method.CreateEmitContext (container, builder.GetILGenerator ());
			else
				ec = method.CreateEmitContext (container, null);

			Location loc = method.Location;
			Attributes OptAttributes = method.OptAttributes;

			if (OptAttributes != null)
				OptAttributes.Emit (ec, kind);

			if (member is MethodCore)
				((MethodCore) member).Parameters.LabelParameters (ec, MethodBuilder, loc);
                        
			Block block = method.Block;
			
			//
			// abstract or extern methods have no bodies
			//
			if ((modifiers & (Modifiers.ABSTRACT | Modifiers.EXTERN)) != 0){
				if (block == null)
					return;

				//
				// abstract or extern methods have no bodies.
				//
				if ((modifiers & Modifiers.ABSTRACT) != 0)
					Report.Error (
						500, method.Location, "Abstract method `" +
						TypeManager.CSharpSignature (builder) +
						"' can not have a body");

				if ((modifiers & Modifiers.EXTERN) != 0)
					Report.Error (
						179, method.Location, "External method `" +
						TypeManager.CSharpSignature (builder) +
						"' can not have a body");

				return;
			}

			//
			// Methods must have a body unless they're extern or abstract
			//
			if (block == null) {
				Report.Error (
					501, method.Location, "Method `" +
					TypeManager.CSharpSignature (builder) +
					"' must declare a body since it is not marked " +
					"abstract or extern");
				return;
			}

			SourceMethod source = SourceMethod.Create (
				container, MethodBuilder, method.Block);

			//
			// Handle destructors specially
			//
			// FIXME: This code generates buggy code
			//
			if (member is Destructor)
				EmitDestructor (ec, block);
			else
				ec.EmitTopBlock (block, ParameterInfo, loc);

			if (source != null)
				source.CloseMethod ();
		}

		void EmitDestructor (EmitContext ec, Block block)
		{
			ILGenerator ig = ec.ig;
			
			Label finish = ig.DefineLabel ();

			block.SetDestructor ();
			
			ig.BeginExceptionBlock ();
			ec.ReturnLabel = finish;
			ec.HasReturnLabel = true;
			ec.EmitTopBlock (block, null, method.Location);
			
			// ig.MarkLabel (finish);
			ig.BeginFinallyBlock ();
			
			if (ec.ContainerType.BaseType != null) {
				Expression member_lookup = Expression.MemberLookup (
					ec, ec.ContainerType.BaseType, null, ec.ContainerType.BaseType,
					"Finalize", MemberTypes.Method, Expression.AllBindingFlags, method.Location);

				if (member_lookup != null){
					MethodGroupExpr parent_destructor = ((MethodGroupExpr) member_lookup);
				
					ig.Emit (OpCodes.Ldarg_0);
					ig.Emit (OpCodes.Call, (MethodInfo) parent_destructor.Methods [0]);
				}
			}
			
			ig.EndExceptionBlock ();
			//ig.MarkLabel (ec.ReturnLabel);
			ig.Emit (OpCodes.Ret);
		}
	}

	public class Destructor : Method {

		public Destructor (TypeContainer ds, Expression return_type, int mod, string name,
				   Parameters parameters, Attributes attrs, Location l)
			: base (ds, null, return_type, mod, false, new MemberName (name),
				parameters, attrs, l)
		{ }

		public override void ApplyAttributeBuilder(Attribute a, CustomAttributeBuilder cb)
		{
			if (a.Type == TypeManager.conditional_attribute_type) {
				// Conditional not valid on '{0}' because it is a destructor, operator, or explicit interface implementation
				Report.Error_T (577, Location, GetSignatureForError ());
				return;
			}

			base.ApplyAttributeBuilder (a, cb);
		}
	}
	
	abstract public class MemberBase : MemberCore {
		public Expression Type;

		public MethodAttributes flags;

		protected readonly int explicit_mod_flags;

		//
		// The "short" name of this property / indexer / event.  This is the
		// name without the explicit interface.
		//
		public string ShortName;

		//
		// The type of this property / indexer / event
		//
		public Type MemberType;

		//
		// If true, this is an explicit interface implementation
		//
		public bool IsExplicitImpl = false;

		//
		// The name of the interface we are explicitly implementing
		//
		public Expression ExplicitInterfaceName = null;

		//
		// Whether this is an interface member.
		//
		public bool IsInterface;

		//
		// If true, the interface type we are explicitly implementing
		//
		public Type InterfaceType = null;

		//
		// The method we're overriding if this is an override method.
		//
		protected MethodInfo parent_method = null;
		public MethodInfo ParentMethod {
			get {
				return parent_method;
			}
		}

		//
		// The constructor is only exposed to our children
		//
		protected MemberBase (TypeContainer parent, Expression type, int mod,
				      int allowed_mod, int def_mod, MemberName name,
				      Attributes attrs, Location loc)
			: base (parent, name, attrs, loc)
		{
			explicit_mod_flags = mod;
			Type = type;
			ModFlags = Modifiers.Check (allowed_mod, mod, def_mod, loc);
		}

		protected virtual bool CheckBase ()
		{
			if ((Parent.Kind == Kind.Struct) || (RootContext.WarningLevel > 3)){
				if ((ModFlags & Modifiers.PROTECTED) != 0 && (Parent.ModFlags & Modifiers.SEALED) != 0){
					if (Parent.Kind == Kind.Struct){
						Report.Error (666, Location, "Protected member in struct declaration");
						return false;
					} else
						Report.Warning (628, Location, "Member " + Parent.MakeName (Name) + " protected in sealed class");
				}
			}
			return true;
		}

		protected void WarningNotHiding (TypeContainer parent)
		{
			Report.Warning (
				109, Location,
				"The member " + parent.MakeName (Name) + " does not hide an " +
				"inherited member.  The keyword new is not required");
							   
		}

		void Error_CannotChangeAccessModifiers (TypeContainer parent, MethodInfo parent_method,
							string name)
		{
			//
			// FIXME: report the old/new permissions?
			//
			Report.Error (
				507, Location, parent.MakeName (Name) +
				": can't change the access modifiers when overriding inherited " +
				"member `" + name + "'");
		}

		protected abstract bool CheckGenericOverride (MethodInfo method, string name);
		
		//
		// Performs various checks on the MethodInfo `mb' regarding the modifier flags
		// that have been defined.
		//
		// `name' is the user visible name for reporting errors (this is used to
		// provide the right name regarding method names and properties)
		//
		protected bool CheckMethodAgainstBase (TypeContainer parent, MethodAttributes my_attrs,
						       MethodInfo mb, string name)
		{
			bool ok = true;
			
			if ((ModFlags & Modifiers.OVERRIDE) != 0){
				if (!(mb.IsAbstract || mb.IsVirtual)){
					Report.Error (
						506, Location, parent.MakeName (Name) +
						": cannot override inherited member `" +
						name + "' because it is not " +
						"virtual, abstract or override");
					ok = false;
				}
				
				// Now we check that the overriden method is not final
				
				if (mb.IsFinal) {
					// This happens when implementing interface methods.
					if (mb.IsHideBySig && mb.IsVirtual) {
						Report.Error (
							506, Location, parent.MakeName (Name) +
							": cannot override inherited member `" +
							name + "' because it is not " +
							"virtual, abstract or override");
					} else
						Report.Error (239, Location, parent.MakeName (Name) + " : cannot " +
							      "override inherited member `" + name +
							      "' because it is sealed.");
					ok = false;
				}

				//
				// Check that the constraints match when overriding a
				// generic method.
				//

				if (!CheckGenericOverride (mb, name))
					ok = false;

				//
				// Check that the permissions are not being changed
				//
				MethodAttributes thisp = my_attrs & MethodAttributes.MemberAccessMask;
				MethodAttributes parentp = mb.Attributes & MethodAttributes.MemberAccessMask;

				//
				// special case for "protected internal"
				//

				if ((parentp & MethodAttributes.FamORAssem) == MethodAttributes.FamORAssem){
					//
					// when overriding protected internal, the method can be declared
					// protected internal only within the same assembly
					//

					if ((thisp & MethodAttributes.FamORAssem) == MethodAttributes.FamORAssem){
						if (parent.TypeBuilder.Assembly != mb.DeclaringType.Assembly){
							//
							// assemblies differ - report an error
							//
							
							Error_CannotChangeAccessModifiers (parent, mb, name);
						    ok = false;
						} else if (thisp != parentp) {
							//
							// same assembly, but other attributes differ - report an error
							//
							
							Error_CannotChangeAccessModifiers (parent, mb, name);
							ok = false;
						};
					} else if ((thisp & MethodAttributes.Family) != MethodAttributes.Family) {
						//
						// if it's not "protected internal", it must be "protected"
						//

						Error_CannotChangeAccessModifiers (parent, mb, name);
						ok = false;
					} else if (parent.TypeBuilder.Assembly == mb.DeclaringType.Assembly) {
						//
						// protected within the same assembly - an error
						//
						Error_CannotChangeAccessModifiers (parent, mb, name);
						ok = false;
					} else if ((thisp & ~(MethodAttributes.Family | MethodAttributes.FamORAssem)) != 
						   (parentp & ~(MethodAttributes.Family | MethodAttributes.FamORAssem))) {
						//
						// protected ok, but other attributes differ - report an error
						//
						Error_CannotChangeAccessModifiers (parent, mb, name);
						ok = false;
					}
				} else {
					if (thisp != parentp){
						Error_CannotChangeAccessModifiers (parent, mb, name);
						ok = false;
					}
				}
			}

			if (mb.IsVirtual || mb.IsAbstract){
				if ((ModFlags & (Modifiers.NEW | Modifiers.OVERRIDE)) == 0){
					if (Name != "Finalize"){
						Report.Warning (
							114, 2, Location, parent.MakeName (Name) + 
							" hides inherited member `" + name +
							"'.  To make the current member override that " +
							"implementation, add the override keyword, " +
							"otherwise use the new keyword");
						ModFlags |= Modifiers.NEW;
					}
				}
			} else {
				if ((ModFlags & (Modifiers.NEW | Modifiers.OVERRIDE)) == 0){
					if (Name != "Finalize"){
						Report.Warning (
							108, 1, Location, "The keyword new is required on " +
							parent.MakeName (Name) + " because it hides " +
							"inherited member `" + name + "'");
						ModFlags |= Modifiers.NEW;
					}
				}
			}

			return ok;
		}

		protected virtual bool CheckParameters (DeclSpace ds, Type [] parameters)
		{
			bool error = false;

			foreach (Type partype in parameters){
				if (partype == TypeManager.void_type) {
					Report.Error (
						1547, Location, "Keyword 'void' cannot " +
						"be used in this context");
					return false;
				}

				if (partype.IsPointer){
					if (!UnsafeOK (ds))
						error = true;
					if (!TypeManager.VerifyUnManaged (TypeManager.GetElementType (partype), Location))
						error = true;
				}

				if (ds.AsAccessible (partype, ModFlags))
					continue;

				if (this is Indexer)
					Report.Error (55, Location,
						      "Inconsistent accessibility: parameter type `" +
						      TypeManager.CSharpName (partype) + "' is less " +
						      "accessible than indexer `" + Name + "'");
				else if ((this is Method) && ((Method) this).IsOperator)
					Report.Error (57, Location,
						      "Inconsistent accessibility: parameter type `" +
						      TypeManager.CSharpName (partype) + "' is less " +
						      "accessible than operator `" + Name + "'");
				else
					Report.Error (51, Location,
						      "Inconsistent accessibility: parameter type `" +
						      TypeManager.CSharpName (partype) + "' is less " +
						      "accessible than method `" + Name + "'");
				error = true;
			}

			return !error;
		}

		protected virtual bool DoDefineBase ()
		{
			if (Name == null)
				Name = "this";

			if (IsInterface) {
				ModFlags = Modifiers.PUBLIC |
					Modifiers.ABSTRACT |
					Modifiers.VIRTUAL | (ModFlags & Modifiers.UNSAFE);

				flags = MethodAttributes.Public |
					MethodAttributes.Abstract |
					MethodAttributes.HideBySig |
					MethodAttributes.NewSlot |
					MethodAttributes.Virtual;
			} else {
				if (!Parent.MethodModifiersValid (ModFlags, Name, Location))
					return false;

				flags = Modifiers.MethodAttr (ModFlags);
			}

			return true;
		}

		protected virtual bool DoDefine (DeclSpace decl)
		{
			// Lookup Type, verify validity
			MemberType = decl.ResolveType (Type, false, Location);
			if (MemberType == null)
				return false;

			if ((Parent.ModFlags & Modifiers.SEALED) != 0){
				if ((ModFlags & (Modifiers.VIRTUAL|Modifiers.ABSTRACT)) != 0){
					Report.Error (549, Location, "Virtual method can not be contained in sealed class");
					return false;
				}
			}
			
			// verify accessibility
			if (!Parent.AsAccessible (MemberType, ModFlags)) {
				if (this is Property)
					Report.Error (53, Location,
						      "Inconsistent accessibility: property type `" +
						      TypeManager.CSharpName (MemberType) + "' is less " +
						      "accessible than property `" + Name + "'");
				else if (this is Indexer)
					Report.Error (54, Location,
						      "Inconsistent accessibility: indexer return type `" +
						      TypeManager.CSharpName (MemberType) + "' is less " +
						      "accessible than indexer `" + Name + "'");
				else if (this is Method) {
					if (((Method) this).IsOperator)
						Report.Error (56, Location,
							      "Inconsistent accessibility: return type `" +
							      TypeManager.CSharpName (MemberType) + "' is less " +
							      "accessible than operator `" + Name + "'");
					else
						Report.Error (50, Location,
							      "Inconsistent accessibility: return type `" +
							      TypeManager.CSharpName (MemberType) + "' is less " +
							      "accessible than method `" + Name + "'");
				} else
					Report.Error (52, Location,
						      "Inconsistent accessibility: field type `" +
						      TypeManager.CSharpName (MemberType) + "' is less " +
						      "accessible than field `" + Name + "'");
				return false;
			}

			if (MemberType.IsPointer && !UnsafeOK (Parent))
				return false;

			//
			// Check for explicit interface implementation
			//
			if (MemberName.Left != null) {
				ExplicitInterfaceName = MemberName.Left.GetTypeExpression (Location);
				ShortName = MemberName.Name;
			} else
				ShortName = Name;

			if (ExplicitInterfaceName != null) {
				InterfaceType = Parent.ResolveType (
					ExplicitInterfaceName, false, Location);
				if (InterfaceType == null)
					return false;

				if (InterfaceType.IsClass) {
					Report.Error (538, Location, "'{0}' in explicit interface declaration is not an interface", ExplicitInterfaceName);
					return false;
				}

				// Compute the full name that we need to export.
				Name = InterfaceType.FullName + "." + ShortName;
				
				if (!Parent.VerifyImplements (InterfaceType, ShortName, Name, Location))
					return false;
				
				Modifiers.Check (Modifiers.AllowedExplicitImplFlags, explicit_mod_flags, 0, Location);
				
				IsExplicitImpl = true;
			} else
				IsExplicitImpl = false;

			return true;
		}

		/// <summary>
		/// Use this method when MethodBuilder is null
		/// </summary>
		public virtual string GetSignatureForError (TypeContainer tc)
		{
			return String.Concat (tc.Name, '.', Name);
		}

		protected override bool IsIdentifierClsCompliant (DeclSpace ds)
		{
			return IsIdentifierAndParamClsCompliant (ds, Name, null, null);
		}

		protected override bool VerifyClsCompliance(DeclSpace ds)
		{
			if (base.VerifyClsCompliance (ds)) {
				return true;
			}

			if (IsInterface && HasClsCompliantAttribute && ds.IsClsCompliaceRequired (ds)) {
				Report.Error_T (3010, Location, GetSignatureForError ());
			}
			return false;
		}

		protected override void VerifyObsoleteAttribute()
		{
			CheckUsageOfObsoleteAttribute (MemberType);
		}
	}

	//
	// Fields and Events both generate FieldBuilders, we use this to share 
	// their common bits.  This is also used to flag usage of the field
	//
	abstract public class FieldBase : MemberBase {
		public FieldBuilder  FieldBuilder;
		public Status status;

		[Flags]
		public enum Status : byte { ASSIGNED = 1, USED = 2 }

		static string[] attribute_targets = new string [] { "field" };

		//
		// The constructor is only exposed to our children
		//
		protected FieldBase (TypeContainer parent, Expression type, int mod,
				     int allowed_mod, MemberName name, object init,
				     Attributes attrs, Location loc)
			: base (parent, type, mod, allowed_mod, Modifiers.PRIVATE,
				name, attrs, loc)
		{
			this.init = init;
		}

		public override AttributeTargets AttributeTargets {
			get {
				return AttributeTargets.Field;
			}
		}

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb)
		{
			if (a.Type == TypeManager.marshal_as_attr_type) {
				UnmanagedMarshal marshal = a.GetMarshal ();
				if (marshal != null) {
					FieldBuilder.SetMarshal (marshal);
					return;
				}
				Report.Warning_T (-24, a.Location);
				return;
			}

			
			FieldBuilder.SetCustomAttribute (cb);
		}

		//
		// Whether this field has an initializer.
		//
		public bool HasInitializer {
			get {
				return init != null;
			}
		}

		protected readonly Object init;
		// Private.
		Expression init_expr;
		bool init_expr_initialized = false;

		protected override bool CheckGenericOverride (MethodInfo method, string name)
		{
			return true;
		}

		//
		// Resolves and returns the field initializer.
		//
		public Expression GetInitializerExpression (EmitContext ec)
		{
			if (init_expr_initialized)
				return init_expr;

			Expression e;
			if (init is Expression)
				e = (Expression) init;
			else
				e = new ArrayCreation (Type, "", (ArrayList)init, Location);

			ec.IsFieldInitializer = true;
			e = e.DoResolve (ec);
			ec.IsFieldInitializer = false;

			init_expr = e;
			init_expr_initialized = true;

			return init_expr;
		}

		protected override bool DoDefine (DeclSpace ds)
		{
			if (!base.DoDefine (ds))
				return false;

			if (MemberType == TypeManager.void_type) {
				Report.Error (1547, Location,
					      "Keyword 'void' cannot be used in this context");
				return false;
			}

			if (MemberType == TypeManager.arg_iterator_type || MemberType == TypeManager.typed_reference_type) {
				// "Field or property cannot be of type '{0}'";
				Report.Error_T (610, Location, TypeManager.CSharpName (MemberType));
				return false;
			}

			return true;
		}

		public override string GetSignatureForError ()
		{
			return TypeManager.GetFullNameSignature (FieldBuilder);
		}

		protected override string[] ValidAttributeTargets {
			get {
				return attribute_targets;
			}
		}

		protected override bool VerifyClsCompliance (DeclSpace ds)
		{
			if (!base.VerifyClsCompliance (ds))
				return false;

			if (FieldBuilder == null) {
				return true;
			}

			if (!AttributeTester.IsClsCompliant (FieldBuilder.FieldType)) {
				Report.Error_T (3003, Location, GetSignatureForError ());
			}
			return true;
		}


		public void SetAssigned ()
		{
			status |= Status.ASSIGNED;
		}
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

		public Field (TypeContainer parent, Expression type, int mod, string name,
			      Object expr_or_array_init, Attributes attrs, Location loc)
			: base (parent, type, mod, AllowedModifiers, new MemberName (name),
				expr_or_array_init, attrs, loc)
		{
		}

		public override bool Define ()
		{
			MemberType = Parent.ResolveType (Type, false, Location);

			if (MemberType == null)
				return false;

			CheckBase ();
			
			if (!Parent.AsAccessible (MemberType, ModFlags)) {
				Report.Error (52, Location,
					      "Inconsistent accessibility: field type `" +
					      TypeManager.CSharpName (MemberType) + "' is less " +
					      "accessible than field `" + Name + "'");
				return false;
			}

			if (MemberType.IsPointer && !UnsafeOK (Parent))
				return false;
			
			if (RootContext.WarningLevel > 1){
				Type ptype = Parent.TypeBuilder.BaseType;

				// ptype is only null for System.Object while compiling corlib.
				if (ptype != null){
					TypeContainer.FindMembers (
						ptype, MemberTypes.Method,
						BindingFlags.Public |
						BindingFlags.Static | BindingFlags.Instance,
						System.Type.FilterName, Name);
				}
			}

			if ((ModFlags & Modifiers.VOLATILE) != 0){
				if (!MemberType.IsClass){
					Type vt = MemberType;
					
					if (TypeManager.IsEnumType (vt))
						vt = TypeManager.EnumToUnderlying (MemberType);

					if (!((vt == TypeManager.bool_type) ||
					      (vt == TypeManager.sbyte_type) ||
					      (vt == TypeManager.byte_type) ||
					      (vt == TypeManager.short_type) ||
					      (vt == TypeManager.ushort_type) ||
					      (vt == TypeManager.int32_type) ||
					      (vt == TypeManager.uint32_type) ||    
					      (vt == TypeManager.char_type) ||
					      (vt == TypeManager.float_type) ||
					      (!vt.IsValueType))){
						Report.Error (
							677, Location, Parent.MakeName (Name) +
							" A volatile field can not be of type `" +
							TypeManager.CSharpName (vt) + "'");
						return false;
					}
				}

				if ((ModFlags & Modifiers.READONLY) != 0){
					Report.Error (
						      678, Location,
						      "A field can not be both volatile and readonly");
					return false;
				}
			}

			FieldAttributes fa = Modifiers.FieldAttr (ModFlags);

			if (Parent.Kind == Kind.Struct && 
			    ((fa & FieldAttributes.Static) == 0) &&
			    MemberType == Parent.TypeBuilder &&
			    !TypeManager.IsBuiltinType (MemberType)){
				Report.Error (523, Location, "Struct member `" + Parent.Name + "." + Name + 
					      "' causes a cycle in the structure layout");
				return false;
			}

			try {
				FieldBuilder = Parent.TypeBuilder.DefineField (
					Name, MemberType, Modifiers.FieldAttr (ModFlags));

			TypeManager.RegisterFieldBase (FieldBuilder, this);
			}
			catch (ArgumentException) {
				Report.Warning (-24, Location, "The Microsoft runtime is unable to use [void|void*] as a field type, try using the Mono runtime.");
				return false;
			}

			return true;
		}

		public override void Emit ()
		{
			if (OptAttributes != null) {
				EmitContext ec = new EmitContext (
					Parent, Location, null, FieldBuilder.FieldType,
					ModFlags);
				OptAttributes.Emit (ec, this);
		}

			base.Emit ();
		}
	}

	//
	// `set' and `get' accessors are represented with an Accessor.
	// 
	public class Accessor {
		//
		// Null if the accessor is empty, or a Block if not
		//
		public Block Block;
		public Attributes Attributes;
		
		public Accessor (Block b, Attributes attrs)
		{
			Block = b;
			Attributes = attrs;
		}
	}


	// Ooouh Martin, templates are missing here.
	// When it will be possible move here a lot of child code and template method type.
	public abstract class AbstractPropertyEventMethod: Attributable, IMethodData
	{
		protected MethodData method_data;
		protected Block block;

		ReturnParameter return_attributes;

		public AbstractPropertyEventMethod ():
			base (null)
		{
		}

		public AbstractPropertyEventMethod (Accessor accessor):
			base (accessor.Attributes)
		{
			this.block = accessor.Block;
		}

		#region IMethodData Members

		public Block Block {
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

		public bool IsExcluded (EmitContext ec)
		{
			return false;
		}

		GenericMethod IMethodData.GenericMethod {
			get {
				return null;
			}
		}

		public abstract ObsoleteAttribute GetObsoleteAttribute ();
		public abstract string GetSignatureForError (TypeContainer tc);
		public abstract Location Location { get; }
		public abstract string MethodName { get; }
		public abstract Type[] ParameterTypes { get; }
		public abstract Type ReturnType { get; }
		public abstract EmitContext CreateEmitContext(TypeContainer tc, ILGenerator ig);

		#endregion

		public override void ApplyAttributeBuilder(Attribute a, CustomAttributeBuilder cb)
		{
			if (a.Type == TypeManager.cls_compliant_attribute_type || a.Type == TypeManager.obsolete_attribute_type ||
					a.Type == TypeManager.conditional_attribute_type) {
				//"'{0}' is not valid on property or event accessors. It is valid on '{1}' declarations only"
				Report.Error_T (1667, a.Location, TypeManager.CSharpName (a.Type), a.GetValidTargets ());
				return;
			}

			if (a.Target == "method") {
				method_data.MethodBuilder.SetCustomAttribute (cb);
				return;
			}

			if (a.Target == "return") {
				if (return_attributes == null)
					return_attributes = new ReturnParameter (method_data.MethodBuilder, Location);

				return_attributes.ApplyAttributeBuilder (a, cb);
				return;
			}

			ApplyToExtraTarget (a, cb);
		}

		virtual protected void ApplyToExtraTarget (Attribute a, CustomAttributeBuilder cb)
		{
			System.Diagnostics.Debug.Fail ("You forgot to define special attribute target handling");
		}

		public virtual void Emit (TypeContainer container)
		{
			method_data.Emit (container, this);
			block = null;
		}
	}

	//
	// Properties and Indexers both generate PropertyBuilders, we use this to share 
	// their common bits.
	//
	abstract public class PropertyBase : MethodCore {

		public class GetMethod: PropertyMethod
		{
			static string[] attribute_targets = new string [] { "method", "return" };

			public GetMethod (MethodCore method, Accessor accessor):
				base (method, accessor)
			{
			}

			public override MethodBuilder Define(TypeContainer container)
			{
				method_data = new MethodData (method, method.ParameterInfo, method.ModFlags, method.flags, this);

				if (!method_data.Define (container))
					return null;

				return method_data.MethodBuilder;
			}

			public override string GetSignatureForError (TypeContainer tc)
			{
				return String.Concat (base.GetSignatureForError (tc), ".get");
			}

			public override string MethodName 
			{
				get {
					return "get_" + method.ShortName;
				}
			}

			public override Type ReturnType {
				get {
					return method.MemberType;
				}
			}

			protected override string[] ValidAttributeTargets {
				get {
					return attribute_targets;
				}
			}
		}

		public class SetMethod: PropertyMethod {

			static string[] attribute_targets = new string [] { "method", "param", "return" };
                       ImplicitParameter param_attr;

			public SetMethod (MethodCore method, Accessor accessor):
				base (method, accessor)
			{
			}

			protected override void ApplyToExtraTarget(Attribute a, CustomAttributeBuilder cb)
			{
				if (a.Target == "param") {
					if (param_attr == null)
                                               param_attr = new ImplicitParameter (method_data.MethodBuilder);

					param_attr.ApplyAttributeBuilder (a, cb);
					return;
				}

				base.ApplyAttributeBuilder (a, cb);
			}

			protected virtual InternalParameters GetParameterInfo (TypeContainer container)
			{
				Parameter [] parms = new Parameter [1];
				parms [0] = new Parameter (method.Type, "value", Parameter.Modifier.NONE, null);
				return new InternalParameters (
					container, new Parameters (parms, null, method.Location));
			}

			public override MethodBuilder Define(TypeContainer container)
			{
				method_data = new MethodData (method, GetParameterInfo (container), method.ModFlags, method.flags, this);

				if (!method_data.Define (container))
					return null;

				return method_data.MethodBuilder;
			}

			public override string GetSignatureForError (TypeContainer tc)
			{
				return String.Concat (base.GetSignatureForError (tc), ".set");
			}

			public override string MethodName {
				get {
					return "set_" + method.ShortName;
				}
			}

			public override Type[] ParameterTypes {
				get {
					return new Type[] { method.MemberType };
				}
			}

			public override Type ReturnType {
				get {
					return TypeManager.void_type;
				}
			}

			protected override string[] ValidAttributeTargets {
				get {
					return attribute_targets;
				}
			}
		}

		static string[] attribute_targets = new string [] { "property" };

		public abstract class PropertyMethod: AbstractPropertyEventMethod {
			protected readonly MethodCore method;

			public PropertyMethod (MethodCore method, Accessor accessor):
 				base (accessor)
			{
				this.method = method;
			}

			public override AttributeTargets AttributeTargets {
				get {
					return AttributeTargets.Method | AttributeTargets.ReturnValue;
				}
			}

			public override bool IsClsCompliaceRequired(DeclSpace ds)
			{
				return method.IsClsCompliaceRequired (ds);
			}

			public InternalParameters ParameterInfo 
			{
				get {
					return method_data.ParameterInfo;
				}
			}

			public abstract MethodBuilder Define (TypeContainer container);

			public override Type[] ParameterTypes {
				get {
					return TypeManager.NoTypes;
				}
			}

			public override Location Location {
				get {
					return method.Location;
				}
			}

			public override EmitContext CreateEmitContext (TypeContainer tc,
								       ILGenerator ig)
			{
				return new EmitContext (
					tc, method.ds, method.Location, ig, ReturnType,
					method.ModFlags, false);
			}

			public override ObsoleteAttribute GetObsoleteAttribute ()
			{
				return method.GetObsoleteAttribute (method.ds);
			}

			public override string GetSignatureForError (TypeContainer tc)
  			{
				return String.Concat (tc.Name, '.', method.Name);
  			}
		}

		public PropertyMethod Get, Set;
		public PropertyBuilder PropertyBuilder;
		public MethodBuilder GetBuilder, SetBuilder;

		protected EmitContext ec;

		public PropertyBase (TypeContainer parent, Expression type, int mod_flags,
				     int allowed_mod, bool is_iface, MemberName name,
				     Parameters parameters, Attributes attrs,
				     Location loc)
			: base (parent, null, type, mod_flags, allowed_mod, is_iface, name,
				attrs, parameters, loc)
		{
		}

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb)
		{
			PropertyBuilder.SetCustomAttribute (cb);
		}

		public override AttributeTargets AttributeTargets {
			get {
				return AttributeTargets.Property;
			}
		}

		protected override bool DoDefine (DeclSpace decl)
		{
			if (!base.DoDefine (decl))
				return false;

			if (MemberType == TypeManager.arg_iterator_type || MemberType == TypeManager.typed_reference_type) {
				// "Field or property cannot be of type '{0}'";
				Report.Error_T (610, Location, TypeManager.CSharpName (MemberType));
				return false;
			}

			ec = new EmitContext (Parent, Location, null, MemberType, ModFlags);

			return true;
		}

		public override string GetSignatureForError()
		{
			return TypeManager.CSharpSignature (PropertyBuilder, false);
		}

		protected virtual string RealMethodName {
			get {
				return Name;
			}
		}

		protected override bool IsIdentifierClsCompliant (DeclSpace ds)
		{
			if (!IsIdentifierAndParamClsCompliant (ds, RealMethodName, null, null))
				return false;

			if (Get != null && !IsIdentifierAndParamClsCompliant (ds, "get_" + RealMethodName, null, null))
				return false;

			if (Set != null && !IsIdentifierAndParamClsCompliant (ds, "set_" + RealMethodName, null, null))
				return false;

			return true;
		}


		//
		// Checks our base implementation if any
		//
		protected override bool CheckBase ()
		{
			base.CheckBase ();
			
			// Check whether arguments were correct.
			if (!DoDefineParameters ())
				return false;

			if (IsExplicitImpl)
				return true;

			//
			// Check in our class for dups
			//
			ArrayList ar = Parent.Properties;
			if (ar != null) {
				int arLen = ar.Count;
					
				for (int i = 0; i < arLen; i++) {
					Property m = (Property) ar [i];
					if (IsDuplicateImplementation (Parent, m))
						return false;
				}
			}

			if (IsInterface)
				return true;

			string report_name;
			MethodSignature ms, base_ms;
			if (this is Indexer) {
				string name, base_name;

				report_name = "this";
				name = TypeManager.IndexerPropertyName (Parent.TypeBuilder);
				ms = new MethodSignature (name, null, ParameterTypes);
				base_name = TypeManager.IndexerPropertyName (Parent.TypeBuilder.BaseType);
				base_ms = new MethodSignature (base_name, null, ParameterTypes);
			} else {
				report_name = Name;
				ms = base_ms = new MethodSignature (Name, null, ParameterTypes);
			}

			//
			// Verify if the parent has a type with the same name, and then
			// check whether we have to create a new slot for it or not.
			//
			Type ptype = Parent.TypeBuilder.BaseType;

			// ptype is only null for System.Object while compiling corlib.
			if (ptype == null) {
				if ((ModFlags & Modifiers.NEW) != 0)
					WarningNotHiding (Parent);

				return true;
			}

			MemberInfo parent_member = null;

			//
			// Explicit implementations do not have `parent' methods, however,
			// the member cache stores them there. Without this check, we get
			// an incorrect warning in corlib.
			//
			if (! IsExplicitImpl) {
				parent_member = ((IMemberContainer)Parent).Parent.MemberCache.FindMemberToOverride (
					Parent.TypeBuilder, Name, ParameterTypes, true);
			}

			if (parent_member is PropertyInfo) {
				PropertyInfo parent_property = (PropertyInfo)parent_member;

				string name = parent_property.DeclaringType.Name + "." +
					parent_property.Name;

				MethodInfo get, set, parent_method;
				get = parent_property.GetGetMethod (true);
				set = parent_property.GetSetMethod (true);

				if (get != null)
					parent_method = get;
				else if (set != null)
					parent_method = set;
				else
					throw new Exception ("Internal error!");

				if (!CheckMethodAgainstBase (Parent, flags, parent_method, name))
					return false;

				if ((ModFlags & Modifiers.NEW) == 0) {
					Type parent_type = TypeManager.TypeToCoreType (
						parent_property.PropertyType);

					if (parent_type != MemberType) {
						Report.Error (
							508, Location, Parent.MakeName (Name) + ": cannot " +
							"change return type when overriding " +
							"inherited member " + name);
						return false;
					}
				}
			} else if (parent_member == null){
				if ((ModFlags & Modifiers.NEW) != 0)
					WarningNotHiding (Parent);

				if ((ModFlags & Modifiers.OVERRIDE) != 0){
					if (this is Indexer)
						Report.Error (115, Location,
							      Parent.MakeName (Name) +
							      " no suitable indexers found to override");
					else
						Report.Error (115, Location,
							      Parent.MakeName (Name) +
							      " no suitable properties found to override");
					return false;
				}
			}
			return true;
		}

		public override void Emit ()
		{
			//
			// The PropertyBuilder can be null for explicit implementations, in that
			// case, we do not actually emit the ".property", so there is nowhere to
			// put the attribute
			//
			if (PropertyBuilder != null && OptAttributes != null)
				OptAttributes.Emit (ec, this);

			if (Get != null)
				Get.Emit (Parent);

			if (Set != null)
				Set.Emit (Parent);

			base.Emit ();
		}

		protected override string[] ValidAttributeTargets {
			get {
				return attribute_targets;
			}
		}
	}
			
	public class Property : PropertyBase, IIteratorContainer {
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
			Modifiers.METHOD_YIELDS |
			Modifiers.VIRTUAL;

		const int AllowedInterfaceModifiers =
			Modifiers.NEW;

		public Property (TypeContainer parent, Expression type, int mod_flags,
				 bool is_iface, MemberName name, Attributes attrs,
				 Accessor get_block, Accessor set_block, Location loc)
			: base (parent, type, mod_flags,
				is_iface ? AllowedInterfaceModifiers : AllowedModifiers,
				is_iface, name, Parameters.EmptyReadOnlyParameters, attrs,
				loc)
		{
			if (get_block != null)
				Get = new GetMethod (this, get_block);

			if (set_block != null)
				Set = new SetMethod (this, set_block);
		}

		public override bool Define ()
		{
			if (!DoDefineBase ())
				return false;

			if (!DoDefine (Parent))
				return false;

			if (!CheckBase ())
				return false;

			flags |= MethodAttributes.HideBySig | MethodAttributes.SpecialName;

			if (Get != null) {

				GetBuilder = Get.Define (Parent);
				if (GetBuilder == null)
					return false;

				//
				// Setup iterator if we are one
				//
				if ((ModFlags & Modifiers.METHOD_YIELDS) != 0){
					Iterator iterator = new Iterator (
						Parent, "get", MemberType,
						TypeManager.NoTypes, Get.ParameterInfo,
						ModFlags, Get.Block, Location);
					
					if (!iterator.DefineIterator ())
						return false;
					Get.Block = iterator.Block;
				}
			}

			if (Set != null) {
				SetBuilder = Set.Define (Parent);
				if (SetBuilder == null)
					return false;

				SetBuilder.DefineParameter (1, ParameterAttributes.None, "value"); 
			}

			// FIXME - PropertyAttributes.HasDefault ?
			
			PropertyAttributes prop_attr = PropertyAttributes.None;
			if (!IsInterface)
				prop_attr |= PropertyAttributes.RTSpecialName |
			PropertyAttributes.SpecialName;

			if (!IsExplicitImpl){
				PropertyBuilder = Parent.TypeBuilder.DefineProperty (
					Name, prop_attr, MemberType, null);
				
				if (Get != null)
					PropertyBuilder.SetGetMethod (GetBuilder);
				
				if (Set != null)
					PropertyBuilder.SetSetMethod (SetBuilder);

				//
				// HACK for the reasons exposed above
				//
				if (!TypeManager.RegisterProperty (PropertyBuilder, GetBuilder, SetBuilder)) {
					Report.Error (
						111, Location,
						"Class `" + Parent.Name +
						"' already contains a definition for the property `" +
						Name + "'");
					return false;
				}
			}
			return true;
		}

		public void SetYields ()
		{
			ModFlags |= Modifiers.METHOD_YIELDS;
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
			if (my_event != null)
				my_event.status = (FieldBase.Status.ASSIGNED | FieldBase.Status.USED);
		}
	}
	
	/// <summary>
	/// For case when event is declared like property (with add and remove accessors).
	/// </summary>
	public class EventProperty: Event {

		static string[] attribute_targets = new string [] { "event", "property" };

		public EventProperty (TypeContainer parent, Expression type, int mod_flags,
				      bool is_iface, MemberName name, Object init,
				      Attributes attrs, Accessor add, Accessor remove,
				      Location loc)
			: base (parent, type, mod_flags, is_iface, name, init, attrs, loc)
		{
			Add = new AddDelegateMethod (this, add);
			Remove = new RemoveDelegateMethod (this, remove);
		}

		protected override string[] ValidAttributeTargets {
			get {
				return attribute_targets;
			}
		}
	}

	/// <summary>
	/// Event is declared like field.
	/// </summary>
	public class EventField: Event {

		static string[] attribute_targets = new string [] { "event", "field", "method" };

		public EventField (TypeContainer parent, Expression type, int mod_flags,
				   bool is_iface, MemberName name, Object init,
				   Attributes attrs, Location loc)
			: base (parent, type, mod_flags, is_iface, name, init, attrs, loc)
		{
			Add = new AddDelegateMethod (this);
			Remove = new RemoveDelegateMethod (this);
		}

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb)
		{
			if (a.Target == "field") {
				FieldBuilder.SetCustomAttribute (cb);
				return;
			}

			if (a.Target == "method") {
				AddBuilder.SetCustomAttribute (cb);
				RemoveBuilder.SetCustomAttribute (cb);
				return;
			}

			base.ApplyAttributeBuilder (a, cb);
		}

		protected override string[] ValidAttributeTargets {
			get {
				return attribute_targets;
			}
		}
	}

	public abstract class Event : FieldBase {

		protected sealed class AddDelegateMethod: DelegateMethod
		{
			public AddDelegateMethod (Event method):
				base (method)
			{
			}

			public AddDelegateMethod (Event method, Accessor accessor):
				base (method, accessor)
			{
			}

			public override string MethodName {
				get {
					return "add_" + method.ShortName;
				}
			}

			protected override MethodInfo DelegateMethodInfo {
				get {
					return TypeManager.delegate_combine_delegate_delegate;
				}
			}

		}

		protected sealed class RemoveDelegateMethod: DelegateMethod
		{
			public RemoveDelegateMethod (Event method):
				base (method)
			{
			}

			public RemoveDelegateMethod (Event method, Accessor accessor):
				base (method, accessor)
			{
			}

			public override string MethodName {
				get {
					return "remove_" + method.ShortName;
				}
			}

			protected override MethodInfo DelegateMethodInfo {
				get {
					return TypeManager.delegate_remove_delegate_delegate;
				}
			}

		}

		public abstract class DelegateMethod: AbstractPropertyEventMethod
		{
			protected readonly Event method;
                       ImplicitParameter param_attr;

			static string[] attribute_targets = new string [] { "method", "param", "return" };

			public DelegateMethod (Event method)
			{
				this.method = method;
			}

			public DelegateMethod (Event method, Accessor accessor):
				base (accessor)
			{
				this.method = method;
			}

			protected override void ApplyToExtraTarget(Attribute a, CustomAttributeBuilder cb)
			{
				if (a.Target == "param") {
					if (param_attr == null)
                                               param_attr = new ImplicitParameter (method_data.MethodBuilder);

					param_attr.ApplyAttributeBuilder (a, cb);
					return;
				}

				base.ApplyAttributeBuilder (a, cb);
			}

			public override AttributeTargets AttributeTargets {
				get {
					return AttributeTargets.Method;
				}
			}

			public override bool IsClsCompliaceRequired(DeclSpace ds)
			{
				return method.IsClsCompliaceRequired (ds);
			}

			public MethodBuilder Define (TypeContainer container, InternalParameters ip)
			{
				method_data = new MethodData (method, ip, method.ModFlags,
					method.flags | MethodAttributes.HideBySig | MethodAttributes.SpecialName, this);

				if (!method_data.Define (container))
					return null;

				MethodBuilder mb = method_data.MethodBuilder;
				mb.DefineParameter (1, ParameterAttributes.None, "value");
				return mb;
			}


			public override void Emit (TypeContainer tc)
			{
				if (block != null) {
					base.Emit (tc);
					return;
				}

				ILGenerator ig = method_data.MethodBuilder.GetILGenerator ();
				EmitContext ec = CreateEmitContext (tc, ig);
				FieldInfo field_info = (FieldInfo)method.FieldBuilder;

				method_data.MethodBuilder.SetImplementationFlags (MethodImplAttributes.Synchronized);
				if ((method.ModFlags & Modifiers.STATIC) != 0) {
					ig.Emit (OpCodes.Ldsfld, field_info);
					ig.Emit (OpCodes.Ldarg_0);
					ig.Emit (OpCodes.Call, DelegateMethodInfo);
					ig.Emit (OpCodes.Castclass, method.MemberType);
					ig.Emit (OpCodes.Stsfld, field_info);
				} else {
					ig.Emit (OpCodes.Ldarg_0);
					ig.Emit (OpCodes.Ldarg_0);
					ig.Emit (OpCodes.Ldfld, field_info);
					ig.Emit (OpCodes.Ldarg_1);
					ig.Emit (OpCodes.Call, DelegateMethodInfo);
					ig.Emit (OpCodes.Castclass, method.MemberType);
					ig.Emit (OpCodes.Stfld, field_info);
				}
				ig.Emit (OpCodes.Ret);
			}

			protected abstract MethodInfo DelegateMethodInfo { get; }

			public override Type[] ParameterTypes {
				get {
					return new Type[] { method.MemberType };
				}
			}

			public override Type ReturnType {
				get {
					return TypeManager.void_type;
				}
			}

			public override Location Location {
				get {
					return method.Location;
				}
			}

			public override EmitContext CreateEmitContext (TypeContainer tc,
								       ILGenerator ig)
			{
				return new EmitContext (
					tc, method.Parent, Location, ig, ReturnType,
					method.ModFlags, false);
			}

			public override string GetSignatureForError (TypeContainer tc)
			{
				return String.Concat (tc.Name, '.', method.Name);
			}

			public override ObsoleteAttribute GetObsoleteAttribute ()
			{
				return method.GetObsoleteAttribute (method.Parent);
			}

			protected override string[] ValidAttributeTargets {
				get {
					return attribute_targets;
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
			Modifiers.ABSTRACT;

		const int AllowedInterfaceModifiers =
			Modifiers.NEW;

		protected DelegateMethod Add, Remove;
		public MyEventBuilder     EventBuilder;
		public MethodBuilder AddBuilder, RemoveBuilder;

		MethodData AddData, RemoveData;
		
		public Event (TypeContainer parent, Expression type, int mod_flags,
			      bool is_iface, MemberName name, Object init, Attributes attrs,
			      Location loc)
			: base (parent, type, mod_flags,
				is_iface ? AllowedInterfaceModifiers : AllowedModifiers,
				name, init, attrs, loc)
		{
			IsInterface = is_iface;
		}

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb)
		{
			EventBuilder.SetCustomAttribute (cb);
		}

		public override AttributeTargets AttributeTargets {
			get {
				return AttributeTargets.Event;
			}
		}
  
		public override bool Define ()
		{
			EventAttributes e_attr;
			e_attr = EventAttributes.None;
;
			if (!DoDefineBase ())
				return false;

			if (!DoDefine (Parent))
				return false;

			if (init != null && ((ModFlags & Modifiers.ABSTRACT) != 0)){
				Report.Error (74, Location, "'" + Parent.Name + "." + Name +
					      "': abstract event can not have an initializer");
				return false;
			}

			if (!TypeManager.IsDelegateType (MemberType)) {
				Report.Error (66, Location, "'" + Parent.Name + "." + Name +
					      "' : event must be of a delegate type");
				return false;
			}

			Parameter [] parms = new Parameter [1];
			parms [0] = new Parameter (Type, "value", Parameter.Modifier.NONE, null);
			InternalParameters ip = new InternalParameters (
				Parent, new Parameters (parms, null, Location)); 

			if (!CheckBase ())
				return false;

			//
			// Now define the accessors
			//

			AddBuilder = Add.Define (Parent, ip);
			if (AddBuilder == null)
				return false;

			RemoveBuilder = Remove.Define (Parent, ip);
			if (RemoveBuilder == null)
				return false;

			if (!IsExplicitImpl){
				EventBuilder = new MyEventBuilder (this,
					Parent.TypeBuilder, Name, e_attr, MemberType);
					
				if (Add.Block == null && Remove.Block == null &&
				    !IsInterface) {
					FieldBuilder = Parent.TypeBuilder.DefineField (
						Name, MemberType,
						FieldAttributes.Private | ((ModFlags & Modifiers.STATIC) != 0 ? FieldAttributes.Static : 0));
					TypeManager.RegisterPrivateFieldOfEvent (
						(EventInfo) EventBuilder, FieldBuilder);
					TypeManager.RegisterFieldBase (FieldBuilder, this);
				}
			
				EventBuilder.SetAddOnMethod (AddBuilder);
				EventBuilder.SetRemoveOnMethod (RemoveBuilder);

				if (!TypeManager.RegisterEvent (EventBuilder, AddBuilder, RemoveBuilder)) {
					Report.Error (111, Location,
						      "Class `" + Parent.Name +
						      "' already contains a definition for the event `" +
						      Name + "'");
					return false;
				}
			}
			
			return true;
		}

		public override void Emit ()
		{
			if (OptAttributes != null) {
				EmitContext ec = new EmitContext (
					Parent, Location, null, MemberType, ModFlags);
				OptAttributes.Emit (ec, this);
			}

			if (!IsInterface) {
				Add.Emit (Parent);
				Remove.Emit (Parent);
			}

			base.Emit ();
		}

		public override string GetSignatureForError ()
		{
			return TypeManager.GetFullNameSignature (EventBuilder);
		}
	}

	//
	// FIXME: This does not handle:
	//
	//   int INTERFACENAME [ args ]
	//   Does not 
	//
	// Only:
	// 
	// int this [ args ]
 
	public class Indexer : PropertyBase {

		class GetIndexerMethod: GetMethod
		{
			public GetIndexerMethod (MethodCore method, Accessor accessor):
				base (method, accessor)
			{
			}

			public override Type[] ParameterTypes {
				get {
					return method.ParameterTypes;
				}
			}
		}

		class SetIndexerMethod: SetMethod
		{
			readonly Parameters parameters;

			public SetIndexerMethod (MethodCore method, Parameters parameters, Accessor accessor):
				base (method, accessor)
			{
				this.parameters = parameters;
			}

			public override Type[] ParameterTypes {
				get {
					int top = method.ParameterTypes.Length;
					Type [] set_pars = new Type [top + 1];
					method.ParameterTypes.CopyTo (set_pars, 0);
					set_pars [top] = method.MemberType;
					return set_pars;
				}
			}

			protected override InternalParameters GetParameterInfo (TypeContainer container)
			{
				Parameter [] fixed_parms = parameters.FixedParameters;

				if (fixed_parms == null){
					throw new Exception ("We currently do not support only array arguments in an indexer at: " + method.Location);
					// BUG BUG BUG BUG BUG BUG BUG BUG BUG BUG
					// BUG BUG BUG BUG BUG BUG BUG BUG BUG BUG
					//
					// Here is the problem: the `value' parameter has
					// to come *after* the array parameter in the declaration
					// like this:
					// X (object [] x, Type value)
					// .param [0]
					//
					// BUG BUG BUG BUG BUG BUG BUG BUG BUG BUG
					// BUG BUG BUG BUG BUG BUG BUG BUG BUG BUG
					
				}
				
				Parameter [] tmp = new Parameter [fixed_parms.Length + 1];

				fixed_parms.CopyTo (tmp, 0);
				tmp [fixed_parms.Length] = new Parameter (
					method.Type, "value", Parameter.Modifier.NONE, null);

				Parameters set_formal_params = new Parameters (tmp, null, method.Location);
				
				return new InternalParameters (container, set_formal_params);
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

		public string IndexerName = "Item";
		public string InterfaceIndexerName;

		//
		// Are we implementing an interface ?
		//
		public Indexer (TypeContainer parent, Expression type, int mod_flags,
				bool is_iface, MemberName name, Parameters parameters,
				Attributes attrs, Accessor get_block, Accessor set_block,
				Location loc)
			: base (parent, type, mod_flags,
				is_iface ? AllowedInterfaceModifiers : AllowedModifiers,
				is_iface, name, parameters, attrs, loc)
		{
			if (get_block != null)
				Get = new GetIndexerMethod (this, get_block);

			if (set_block != null)
				Set = new SetIndexerMethod (this, parameters, set_block);
		}

		public override bool Define ()
		{
			PropertyAttributes prop_attr =
				PropertyAttributes.RTSpecialName |
				PropertyAttributes.SpecialName;
			
			if (!DoDefineBase ())
				return false;

			if (!DoDefine (Parent))
				return false;

			if (OptAttributes != null) {
				Attribute indexer_attr = OptAttributes.GetIndexerNameAttribute (ec);
				if (indexer_attr != null) {
					IndexerName = indexer_attr.GetIndexerAttributeValue (ec);
					if (IsExplicitImpl) {
						// The 'IndexerName' attribute is valid only on an indexer that is not an explicit interface member declaration
						Report.Error_T (415, indexer_attr.Location);
						return false;
					}
				
					if (IsExplicitImpl) {
						// The 'IndexerName' attribute is valid only on an indexer that is not an explicit interface member declaration
						Report.Error_T (415, indexer_attr.Location);
						return false;
					}

					if (!Tokenizer.IsValidIdentifier (IndexerName)) {
						// The argument to the 'IndexerName' attribute must be a valid identifier
						Report.Error_T (633, indexer_attr.Location);
						return false;
					}
				}
			}

			ShortName = IndexerName;
			if (IsExplicitImpl) {
				InterfaceIndexerName = TypeManager.IndexerPropertyName (InterfaceType);
				Name = InterfaceType.FullName + "." + IndexerName;
			} else {
				InterfaceIndexerName = IndexerName;
				Name = ShortName;
			}

			if (!CheckNameCollision (Parent))
				return false;

			if (!CheckBase ())
				return false;

			flags |= MethodAttributes.HideBySig | MethodAttributes.SpecialName;
			if (Get != null){
				GetBuilder = Get.Define (Parent);
				if (GetBuilder == null)
					return false;
			}
			
			if (Set != null){
				SetBuilder = Set.Define (Parent);
				if (SetBuilder == null)
					return false;
			}

			//
			// Now name the parameters
			//
			Parameter [] p = Parameters.FixedParameters;
			if (p != null) {
				int i;
				
				for (i = 0; i < p.Length; ++i) {
					if (Get != null)
						GetBuilder.DefineParameter (
							i + 1, p [i].Attributes, p [i].Name);

					if (Set != null)
						SetBuilder.DefineParameter (
							i + 1, p [i].Attributes, p [i].Name);
				}

				if (Set != null)
					SetBuilder.DefineParameter (
						i + 1, ParameterAttributes.None, "value");
					
				if (i != ParameterTypes.Length) {
					Parameter array_param = Parameters.ArrayParameter;

					SetBuilder.DefineParameter (
						i + 1, array_param.Attributes, array_param.Name);
				}
			}

			//
			// Define the PropertyBuilder if one of the following conditions are met:
			// a) we're not implementing an interface indexer.
			// b) the indexer has a different IndexerName and this is no
			//    explicit interface implementation.
			//
			if (!IsExplicitImpl) {
				PropertyBuilder = Parent.TypeBuilder.DefineProperty (
					IndexerName, prop_attr, MemberType, ParameterTypes);

				if (Get != null)
					PropertyBuilder.SetGetMethod (GetBuilder);

				if (Set != null)
					PropertyBuilder.SetSetMethod (SetBuilder);
				
				TypeManager.RegisterIndexer (PropertyBuilder, GetBuilder, SetBuilder,
							     ParameterTypes);
			}

			return true;
		}

		bool CheckNameCollision (TypeContainer container) {
			switch (VerifyName (container)){
				case DeclSpace.AdditionResult.NameExists:
					Report.Error (102, Location, "The container '{0}' already contains a definition for '{1}'", container.GetSignatureForError (), Name);
					return false;

				case DeclSpace.AdditionResult.Success:
					return true;
			}
			throw new NotImplementedException ();
		}

		DeclSpace.AdditionResult VerifyName (TypeContainer container) {
			if (!AddIndexer (container, container.Name + "." + Name))
				return DeclSpace.AdditionResult.NameExists;

			if (Get != null) {
				if (!AddIndexer (container, container.Name + ".get_" + Name))
					return DeclSpace.AdditionResult.NameExists;
			}

			if (Set != null) {
				if (!AddIndexer (container, container.Name + ".set_" + Name))
					return DeclSpace.AdditionResult.NameExists;
			}
			return DeclSpace.AdditionResult.Success;
		}

		bool AddIndexer (TypeContainer container, string fullname)
		{
			object value = container.GetDefinition (fullname);

			if (value != null) {
				return value.GetType () != GetType () ? false : true;
			}

			container.DefineName (fullname, this);
			return true;
		}

		public override string GetSignatureForError ()
		{
			return TypeManager.CSharpSignature (PropertyBuilder, true);
		}

		protected override string RealMethodName {
			get {
				return IndexerName;
			}
		}
	}

	public class Operator : MemberBase, IIteratorContainer {

		const int AllowedModifiers =
			Modifiers.PUBLIC |
			Modifiers.UNSAFE |
			Modifiers.EXTERN |
			Modifiers.STATIC;

		const int RequiredModifiers =
			Modifiers.PUBLIC |
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
			Explicit
		};

		public readonly OpType OperatorType;
		public readonly Expression ReturnType;
		public readonly Expression FirstArgType, SecondArgType;
		public readonly string FirstArgName, SecondArgName;
		public Block           Block;
		public MethodBuilder   OperatorMethodBuilder;
		
		public string MethodName;
		public Method OperatorMethod;

		static string[] attribute_targets = new string [] { "method", "return" };

		public Operator (TypeContainer parent, OpType type, Expression ret_type,
				 int mod_flags, Expression arg1type, string arg1name,
				 Expression arg2type, string arg2name,
				 Block block, Attributes attrs, Location loc)
			: base (parent, ret_type, mod_flags, AllowedModifiers,
				Modifiers.PUBLIC, MemberName.Null, attrs, loc)
		{
			OperatorType = type;
			Name = "op_" + OperatorType;
			ReturnType = ret_type;
			FirstArgType = arg1type;
			FirstArgName = arg1name;
			SecondArgType = arg2type;
			SecondArgName = arg2name;
			Block = block;
		}

		string Prototype (TypeContainer container)
		{
			return container.Name + ".operator " + OperatorType + " (" + FirstArgType + "," +
				SecondArgType + ")";
		}

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb) 
		{
			OperatorMethod.ApplyAttributeBuilder (a, cb);
		}

		public override AttributeTargets AttributeTargets {
			get {
				return AttributeTargets.Method; 
			}
		}
		
		protected override bool CheckGenericOverride (MethodInfo method,  string name)
		{
			return true;
		}

		public override bool Define ()
		{
			int length = 1;
			MethodName = "op_" + OperatorType;
			
			if (SecondArgType != null)
				length = 2;
			
			Parameter [] param_list = new Parameter [length];

			if ((ModFlags & RequiredModifiers) != RequiredModifiers){
				Report.Error (
					558, Location, 
					"User defined operators `" +
					Prototype (Parent) +
					"' must be declared static and public");
				return false;
			}

			param_list[0] = new Parameter (FirstArgType, FirstArgName,
						       Parameter.Modifier.NONE, null);
			if (SecondArgType != null)
				param_list[1] = new Parameter (SecondArgType, SecondArgName,
							       Parameter.Modifier.NONE, null);
			
			OperatorMethod = new Method (
				Parent, null, ReturnType, ModFlags, false,
				new MemberName (MethodName),
				new Parameters (param_list, null, Location),
				OptAttributes, Location);

			OperatorMethod.Block = Block;
			OperatorMethod.IsOperator = true;			
			OperatorMethod.Define ();

			if (OperatorMethod.MethodBuilder == null)
				return false;
			
			OperatorMethodBuilder = OperatorMethod.MethodBuilder;

			Type [] param_types = OperatorMethod.ParameterTypes;
			Type declaring_type = OperatorMethod.MethodData.DeclaringType;
			Type return_type = OperatorMethod.GetReturnType ();
			Type first_arg_type = param_types [0];

			// Rules for conversion operators
			
			if (OperatorType == OpType.Implicit || OperatorType == OpType.Explicit) {
				if (first_arg_type == return_type && first_arg_type == declaring_type){
					Report.Error (
						555, Location,
						"User-defined conversion cannot take an object of the " +
						"enclosing type and convert to an object of the enclosing" +
						" type");
					return false;
				}
				
				if (first_arg_type != declaring_type && return_type != declaring_type){
					Report.Error (
						556, Location, 
						"User-defined conversion must convert to or from the " +
						"enclosing type");
					return false;
				}
				
				if (first_arg_type == TypeManager.object_type ||
				    return_type == TypeManager.object_type){
					Report.Error (
						-8, Location,
						"User-defined conversion cannot convert to or from " +
						"object type");
					return false;
				}

				if (first_arg_type.IsInterface || return_type.IsInterface){
					Report.Error (
						552, Location,
						"User-defined conversion cannot convert to or from an " +
						"interface type");
					return false;
				}
				
				if (first_arg_type.IsSubclassOf (return_type)
					|| return_type.IsSubclassOf (first_arg_type)){
					if (declaring_type.IsSubclassOf (return_type)) {
						// '{0}' : user defined conversion to/from base class
						Report.Error_T (553, Location, GetSignatureForError ());
						return false;
					}
					// '{0}' : user defined conversion to/from derived class
					Report.Error_T (554, Location, GetSignatureForError ());
					return false;
				}
			} else if (SecondArgType == null) {
				// Checks for Unary operators
				
				if (first_arg_type != declaring_type){
					Report.Error (
						562, Location,
						"The parameter of a unary operator must be the " +
						"containing type");
					return false;
				}
				
				if (OperatorType == OpType.Increment || OperatorType == OpType.Decrement) {
					if (return_type != declaring_type){
						Report.Error (
							559, Location,
							"The parameter and return type for ++ and -- " +
							"must be the containing type");
						return false;
					}
					
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
				    param_types [1] != declaring_type){
					Report.Error (
						563, Location,
						"One of the parameters of a binary operator must " +
						"be the containing type");
					return false;
				}
			}

			return true;
		}
		
		public override void Emit ()
		{
			//
			// abstract or extern methods have no bodies
			//
			if ((ModFlags & (Modifiers.ABSTRACT | Modifiers.EXTERN)) != 0)
				return;
			
			OperatorMethod.Emit ();
			Block = null;
		}

		public static string GetName (OpType ot)
		{
			switch (ot){
			case OpType.LogicalNot:
				return "!";
			case OpType.OnesComplement:
				return "~";
			case OpType.Increment:
				return "++";
			case OpType.Decrement:
				return "--";
			case OpType.True:
				return "true";
			case OpType.False:
				return "false";
			case OpType.Addition:
				return "+";
			case OpType.Subtraction:
				return "-";
			case OpType.UnaryPlus:
				return "+";
			case OpType.UnaryNegation:
				return "-";
			case OpType.Multiply:
				return "*";
			case OpType.Division:
				return "/";
			case OpType.Modulus:
				return "%";
			case OpType.BitwiseAnd:
				return "&";
			case OpType.BitwiseOr:
				return "|";
			case OpType.ExclusiveOr:
				return "^";
			case OpType.LeftShift:
				return "<<";
			case OpType.RightShift:
				return ">>";
			case OpType.Equality:
				return "==";
			case OpType.Inequality:
				return "!=";
			case OpType.GreaterThan:
				return ">";
			case OpType.LessThan:
				return "<";
			case OpType.GreaterThanOrEqual:
				return ">=";
			case OpType.LessThanOrEqual:
				return "<=";
			case OpType.Implicit:
				return "implicit";
			case OpType.Explicit:
				return "explicit";
			default: return "";
			}
		}

		public override string GetSignatureForError(TypeContainer tc)
		{
			return ToString ();
		}

		public override string GetSignatureForError()
		{
			return ToString ();
		}
		
		public override string ToString ()
		{
			Type return_type = OperatorMethod.GetReturnType();
			Type [] param_types = OperatorMethod.ParameterTypes;
			
			if (SecondArgType == null)
				return String.Format (
					"{0} operator {1}({2})",
					TypeManager.CSharpName (return_type),
					GetName (OperatorType),
					param_types [0]);
			else
				return String.Format (
					"{0} operator {1}({2}, {3})",
					TypeManager.CSharpName (return_type),
					GetName (OperatorType),
					param_types [0], param_types [1]);
		}

		protected override string[] ValidAttributeTargets {
			get {
				return attribute_targets;
			}
		}

		public void SetYields ()
		{
			ModFlags |= Modifiers.METHOD_YIELDS;
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
				Parameters = TypeManager.NoTypes;
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
			if (sig.RetType != null)
				if (ReturnType != sig.RetType)
					return false;

			Type [] args;
			if (mi != null)
				args = TypeManager.GetArgumentTypes (mi);
			else
				args = TypeManager.GetArgumentTypes (pi);
			Type [] sigp = sig.Parameters;

			if (args.Length != sigp.Length)
				return false;

			for (int i = args.Length; i > 0; ){
				i--;
				if (args [i] != sigp [i])
					return false;
			}
			return true;
		}
	}
}
