//
// class.cs: Class and Struct handlers
//
// Authors: Miguel de Icaza (miguel@gnu.org)
//          Martin Baulig (martin@gnome.org)
//
// Licensed under the terms of the GNU GPL
//
// (C) 2001, 2002 Ximian, Inc (http://www.ximian.com)
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
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace Mono.CSharp {

	/// <summary>
	///   This is the base class for structs and classes.  
	/// </summary>
	public class TypeContainer : DeclSpace, IMemberContainer {
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

		// Holds order in which interfaces must be closed
		ArrayList interface_order;
		
		// Holds the methods.
		ArrayList methods;

		// Holds the events
		ArrayList events;

		// Holds the indexers
		ArrayList indexers;

		// Holds the operators
		ArrayList operators;

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
		bool have_static_constructor = false;

		//
		// Whether we have at least one non-static field
		//
		bool have_nonstatic_fields = false;
		
		//
		// This one is computed after we can distinguish interfaces
		// from classes from the arraylist `type_bases' 
		//
		string     base_class_name;

		ArrayList type_bases;

		// Attributes for this type
		protected Attributes attributes;

		// Information in the case we are an attribute type

		public AttributeTargets Targets = AttributeTargets.All;
		public bool AllowMultiple = false;
		public bool Inherited;

		// The interfaces we implement.
		Type [] ifaces;

		// The parent member container and our member cache
		IMemberContainer parent_container;
		MemberCache member_cache;
		
		//
		// The indexer name for this class
		//
		public string IndexerName;

		public TypeContainer (TypeContainer parent, string name, Location l)
			: base (parent, name, l)
		{
			string n;
			types = new ArrayList ();

			if (parent == null)
				n = "";
			else
				n = parent.Name;

			base_class_name = null;
			
			//Console.WriteLine ("New class " + name + " inside " + n);
		}

		public AdditionResult AddConstant (Const constant)
		{
			AdditionResult res;
			string basename = constant.Name;
			string fullname = Name + "." + basename;

			if ((res = IsValid (fullname)) != AdditionResult.Success)
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

			if ((res = IsValid (e.Basename)) != AdditionResult.Success)
				return res;

			if (enums == null)
				enums = new ArrayList ();

			enums.Add (e);
			DefineName (e.Name, e);

			return AdditionResult.Success;
		}
		
		public AdditionResult AddClass (Class c)
		{
			AdditionResult res;

			if ((res = IsValid (c.Basename)) != AdditionResult.Success)
				return res;

			DefineName (c.Name, c);
			types.Add (c);

			return AdditionResult.Success;
		}

		public AdditionResult AddStruct (Struct s)
		{
			AdditionResult res;
			
			if ((res = IsValid (s.Basename)) != AdditionResult.Success)
				return res;

			DefineName (s.Name, s);
			types.Add (s);

			return AdditionResult.Success;
		}

		public AdditionResult AddDelegate (Delegate d)
		{
			AdditionResult res;

			if ((res = IsValid (d.Basename)) != AdditionResult.Success)
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

			if (method.Name.IndexOf (".") != -1)
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
				have_static_constructor = true;
				if (default_static_constructor != null){
					Console.WriteLine ("I have a static constructor already");
					Console.WriteLine ("   " + default_static_constructor);
					return AdditionResult.MethodExists;
				}

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
		
		public AdditionResult AddInterface (Interface iface)
		{
			AdditionResult res;

			if ((res = IsValid (iface.Basename)) != AdditionResult.Success)
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

			if ((res = IsValid (fullname)) != AdditionResult.Success)
				return res;
			
			if (fields == null)
				fields = new ArrayList ();
			
			fields.Add (field);
			
			if (field.HasInitializer){
				if ((field.ModFlags & Modifiers.STATIC) != 0){
					if (initialized_static_fields == null)
						initialized_static_fields = new ArrayList ();

					initialized_static_fields.Add (field);

					//
					// We have not seen a static constructor,
					// but we will provide static initialization of fields
					//
					have_static_constructor = true;
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
			string basename = prop.Name;
			string fullname = Name + "." + basename;

			if ((res = IsValid (fullname)) != AdditionResult.Success)
				return res;

			if (properties == null)
				properties = new ArrayList ();

			if (prop.Name.IndexOf (".") != -1)
				properties.Insert (0, prop);
			else
				properties.Add (prop);
			DefineName (fullname, prop);

			return AdditionResult.Success;
		}

		public AdditionResult AddEvent (Event e)
		{
			AdditionResult res;
			string basename = e.Name;
			string fullname = Name + "." + basename;

			if ((res = IsValid (fullname)) != AdditionResult.Success)
				return res;

			if (events == null)
				events = new ArrayList ();
			
			events.Add (e);
			DefineName (fullname, e);

			return AdditionResult.Success;
		}

		public AdditionResult AddIndexer (Indexer i)
		{
			if (indexers == null)
				indexers = new ArrayList ();

			if (i.InterfaceType != null)
				indexers.Insert (0, i);
			else
				indexers.Add (i);

			return AdditionResult.Success;
		}

		public AdditionResult AddOperator (Operator op)
		{
			if (operators == null)
				operators = new ArrayList ();

			operators.Add (op);

			return AdditionResult.Success;
		}

		public void RegisterOrder (Interface iface)
		{
			if (interface_order == null)
				interface_order = new ArrayList ();

			interface_order.Add (iface);
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
		
		public Attributes OptAttributes {
			get {
				return attributes;
			}
		}
		
		public bool HaveStaticConstructor {
			get {
				return have_static_constructor;
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
			ILGenerator ig = ec.ig;
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
				Expression a = new Assign (fe, e, l);

				a = a.Resolve (ec);
				if (a == null)
					return false;

				if (a is ExpressionStatement)
					((ExpressionStatement) a).EmitStatement (ec);
				else {
					throw new Exception ("Assign.Resolve returned a non ExpressionStatement");
				}
			}

			return true;
		}
		
		//
		// Defines the default constructors
		//
		void DefineDefaultConstructor (bool is_static)
		{
			Constructor c;
			int mods = 0;

			c = new Constructor (Basename, Parameters.EmptyReadOnlyParameters,
					     new ConstructorBaseInitializer (
						     null, Parameters.EmptyReadOnlyParameters,
						     Location.Null),
					     Location.Null);
			
			if (is_static)
				mods = Modifiers.STATIC;

			c.ModFlags = mods;

			AddConstructor (c);
			
			c.Block = new Block (null);
			
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
		///  The pending methods that need to be implemented (interfaces or abstract methods)
		/// </remarks>
		public PendingImplementation Pending;

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
		Type [] GetClassBases (bool is_class, out Type parent, out bool error)
		{
			ArrayList bases = Bases;
			int count;
			int start, j, i;

			error = false;

			if (is_class)
				parent = null;
			else
				parent = TypeManager.value_type;

			if (bases == null){
				if (is_class){
					if (RootContext.StdLib)
						parent = TypeManager.object_type;
					else if (Name != "System.Object")
						parent = TypeManager.object_type;
				} else {
					//
					// If we are compiling our runtime,
					// and we are defining ValueType, then our
					// parent is `System.Object'.
					//
					if (!RootContext.StdLib && Name == "System.ValueType")
						parent = TypeManager.object_type;
				}

				return null;
			}

			//
			// Bases should be null if there are no bases at all
			//
			count = bases.Count;

			if (is_class){
				Expression name = (Expression) bases [0];
				name = ResolveTypeExpr (name, false, Location);

				if (name == null){
					error = true;
					return null;
				}

				Type first = name.Type;

				if (first.IsClass){
					parent = first;
					start = 1;
				} else if (first.IsSealed){
					string detail = "";
					
					if (first.IsValueType)
						detail = " (a class can not inherit from a struct/enum)";
					
					Report.Error (509, "class `"+ Name +
						      "': Cannot inherit from sealed class `"+
						      first + "'" + detail);
					error = true;
					return null;
				} else {
					parent = TypeManager.object_type;
					start = 0;
				}

				if (!AsAccessible (parent, ModFlags))
					Report.Error (60, Location,
						      "Inconsistent accessibility: base class `" +
						      TypeManager.CSharpName (parent) + "' is less " +
						      "accessible than class `" +
						      Name + "'");

			} else {
				start = 0;
			}

			Type [] ifaces = new Type [count-start];
			
			for (i = start, j = 0; i < count; i++, j++){
				Expression name = (Expression) bases [i];
				Expression resolved = ResolveTypeExpr (name, false, Location);
				if (resolved == null)
					return null;
				
				bases [i] = resolved;
				Type t = resolved.Type;

				if (t == null){
					error = true;
					return null;
				}

				if (is_class == false && !t.IsInterface){
					Report.Error (527, "In Struct `" + Name + "', type `"+
						      name +"' is not an interface");
					error = true;
					return null;
				}
				
				if (t.IsClass) {
					if (parent != null){
						Report.Error (527, "In Class `" + Name + "', type `"+
							      name+"' is not an interface");
						error = true;
						return null;
					}
				}

				for (int x = 0; x < j; x++) {
					if (t == ifaces [x]) {
						Report.Error (528, "`" + name + "' is already listed in interface list");
						error = true;
						return null;
					}
				}

				ifaces [j] = t;
			}

			return TypeManager.ExpandInterfaces (ifaces);
		}
		
		//
		// Defines the type in the appropriate ModuleBuilder or TypeBuilder.
		//
		public override TypeBuilder DefineType ()
		{
			Type parent;
			bool error;
			bool is_class;

			if (TypeBuilder != null)
				return TypeBuilder;
			
			if (InTransit)
				return null;
			
			InTransit = true;

			if (this is Class)
				is_class = true;
			else
				is_class = false;

			ec = new EmitContext (this, Mono.CSharp.Location.Null, null, null, ModFlags);

			ifaces = GetClassBases (is_class, out parent, out error); 
			
			if (error)
				return null;

			if (is_class && parent != null){
				if (parent == TypeManager.enum_type ||
				    (parent == TypeManager.value_type && RootContext.StdLib) ||
				    parent == TypeManager.delegate_type ||
				    parent == TypeManager.array_type){
					Report.Error (
						644, Location, "`" + Name + "' cannot inherit from " +
						"special class `" + TypeManager.CSharpName (parent) + "'");
					return null;
				}
			}

			if (!is_class && TypeManager.value_type == null)
				throw new Exception ();

			TypeAttributes type_attributes = TypeAttr;

			// if (parent_builder is ModuleBuilder) {
			if (IsTopLevel){
				ModuleBuilder builder = CodeGen.ModuleBuilder;
				TypeBuilder = builder.DefineType (
					Name, type_attributes, parent, ifaces);
				
			} else {
				TypeBuilder builder = Parent.TypeBuilder;
				TypeBuilder = builder.DefineNestedType (
					Basename, type_attributes, parent, ifaces);
			}
				
			//
			// Structs with no fields need to have at least one byte.
			// The right thing would be to set the PackingSize in a DefineType
			// but there are no functions that allow interfaces *and* the size to
			// be specified.
			//

			if (!is_class && !have_nonstatic_fields){
				TypeBuilder.DefineField ("$PRIVATE$", TypeManager.byte_type,
							 FieldAttributes.Private);
				// add interfaces that were not added at type creation
				if (ifaces != null) {
					foreach (Type i in ifaces)
						TypeBuilder.AddInterfaceImplementation (i);
				}
			}

			//
			// Finish the setup for the EmitContext
			//
			ec.ContainerType = TypeBuilder;

			TypeManager.AddUserType (Name, TypeBuilder, this, ifaces);

			if ((parent != null) &&
			    (parent == TypeManager.attribute_type ||
			     parent.IsSubclassOf (TypeManager.attribute_type))) {
				RootContext.RegisterAttribute (this);
				TypeManager.RegisterAttrType (TypeBuilder, this);
			} else
				RootContext.RegisterOrder (this); 
				
			if (Interfaces != null) {
				foreach (Interface iface in Interfaces)
					iface.DefineType ();
			}
			
			if (Types != null) {
				foreach (TypeContainer tc in Types)
					tc.DefineType ();
			}

			if (Delegates != null) {
				foreach (Delegate d in Delegates)
					d.DefineType ();
			}

			if (Enums != null) {
				foreach (Enum en in Enums)
					en.DefineType ();
			}

			InTransit = false;
			return TypeBuilder;
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
				if (!mc.Define (this)){
					remove_list.Add (mc);
					continue;
				}
						
				if (defined_names == null)
					continue;

				idx = Array.BinarySearch (defined_names, mc.Name, mif_compare);
				if (idx < 0){
					if (RootContext.WarningLevel >= 4){
						if ((mc.ModFlags & Modifiers.NEW) != 0)
							Warning_KewywordNewNotRequired (mc.Location, mc);
					}
					continue;
				}

				MemberInfo match = defined_names [idx];

				if (match is PropertyInfo && ((mc.ModFlags & Modifiers.OVERRIDE) != 0))
					continue;

				//
				// If we are both methods, let the method resolution emit warnings
				//
				if (match is MethodBase && mc is MethodCore)
					continue; 
				
				if ((mc.ModFlags & Modifiers.NEW) == 0)
					Warning_KeywordNewRequired (mc.Location, defined_names [idx]);
			}
			
			foreach (object o in remove_list)
				list.Remove (o);
			
			remove_list.Clear ();
		}

		//
		// Defines the indexers, and also verifies that the IndexerNameAttribute in the
		// class is consisten.  Either it is `Item' or it is the name defined by all the
		// indexers with the `IndexerName' attribute.
		//
		// Turns out that the IndexerNameAttribute is applied to each indexer,
		// but it is never emitted, instead a DefaultName attribute is attached
		// to the class.
		//
		void DefineIndexers ()
		{
			string class_indexer_name = null;
			
			foreach (Indexer i in Indexers){
				string name;
				
				i.Define (this);

				name = i.IndexerName;

				if (i.InterfaceType != null)
					continue;

				if (class_indexer_name == null){
					class_indexer_name = name;
					continue;
				}
				
				if (name == class_indexer_name)
					continue;
				
				Report.Error (
					668, "Two indexers have different names, " +
					" you should use the same name for all your indexers");
			}
			if (class_indexer_name == null)
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
		public override bool DefineMembers (TypeContainer parent)
		{
			MemberInfo [] defined_names = null;

			if (interface_order != null){
				foreach (Interface iface in interface_order)
					if ((iface.ModFlags & Modifiers.NEW) == 0)
						iface.DefineMembers (this);
					else
						Error_KeywordNotAllowed (iface.Location);
			}

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
			
			if (constants != null)
				DefineMembers (constants, defined_names);

			if (fields != null)
				DefineMembers (fields, defined_names);

			if (this is Class){
				if (instance_constructors == null){
					if (default_constructor == null)
						DefineDefaultConstructor (false);
				}

				if (initialized_static_fields != null &&
				    default_static_constructor == null)
					DefineDefaultConstructor (true);
			}

			if (this is Struct){
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

			Pending = PendingImplementation.GetPendingImplementations (this);
			
			//
			// Constructors are not in the defined_names array
			//
			if (instance_constructors != null)
				DefineMembers (instance_constructors, null);
		
			if (default_static_constructor != null)
				default_static_constructor.Define (this);
			
			if (methods != null)
				DefineMembers (methods, defined_names);

			if (properties != null)
				DefineMembers (properties, defined_names);

			if (events != null)
				DefineMembers (events, defined_names);

			if (indexers != null) {
				DefineIndexers ();
			} else
				IndexerName = "Item";

			if (operators != null){
				DefineMembers (operators, null);

				CheckPairedOperators ();
			}

			if (enums != null)
				DefineMembers (enums, defined_names);
			
			if (delegates != null)
				DefineMembers (delegates, defined_names);

#if CACHE
			if (TypeBuilder.BaseType != null)
				parent_container = TypeManager.LookupMemberContainer (TypeBuilder.BaseType);

			member_cache = new MemberCache (this);
#endif

			return true;
		}

		public override bool Define (TypeContainer parent)
		{
			if (interface_order != null){
				foreach (Interface iface in interface_order)
					if ((iface.ModFlags & Modifiers.NEW) == 0)
						iface.Define (this);
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
			ArrayList members = new ArrayList ();

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
					foreach (Field f in fields) {
						if ((f.ModFlags & modflags) == 0)
							continue;
						if ((f.ModFlags & static_mask) != static_flags)
							continue;

						FieldBuilder fb = f.FieldBuilder;
						if (fb != null && filter (fb, criteria) == true)
							members.Add (fb);
					}
				}

				if (constants != null) {
					foreach (Const con in constants) {
						if ((con.ModFlags & modflags) == 0)
							continue;
						if ((con.ModFlags & static_mask) != static_flags)
							continue;

						FieldBuilder fb = con.FieldBuilder;
						if (fb != null && filter (fb, criteria) == true)
							members.Add (fb);
					}
				}
			}

			if ((mt & MemberTypes.Method) != 0) {
				if (methods != null) {
					foreach (Method m in methods) {
						if ((m.ModFlags & modflags) == 0)
							continue;
						if ((m.ModFlags & static_mask) != static_flags)
							continue;
						
						MethodBuilder mb = m.MethodBuilder;

						if (mb != null && filter (mb, criteria) == true)
							members.Add (mb);
					}
				}

				if (operators != null){
					foreach (Operator o in operators) {
						if ((o.ModFlags & modflags) == 0)
							continue;
						if ((o.ModFlags & static_mask) != static_flags)
							continue;
						
						MethodBuilder ob = o.OperatorMethodBuilder;
						if (ob != null && filter (ob, criteria) == true)
							members.Add (ob);
					}
				}

				if (properties != null){
					foreach (Property p in properties){
						if ((p.ModFlags & modflags) == 0)
							continue;
						if ((p.ModFlags & static_mask) != static_flags)
							continue;
						
						MethodBuilder b;

						b = p.GetBuilder;
						if (b != null && filter (b, criteria) == true)
							members.Add (b);

						b = p.SetBuilder;
						if (b != null && filter (b, criteria) == true)
							members.Add (b);
					}
				}
				
				if (indexers != null){
					foreach (Indexer ix in indexers){
						if ((ix.ModFlags & modflags) == 0)
							continue;
						if ((ix.ModFlags & static_mask) != static_flags)
							continue;
						
						MethodBuilder b;

						b = ix.GetBuilder;
						if (b != null && filter (b, criteria) == true)
							members.Add (b);

						b = ix.SetBuilder;
						if (b != null && filter (b, criteria) == true)
							members.Add (b);
					}
				}
			}

			if ((mt & MemberTypes.Event) != 0) {
				if (events != null)
				        foreach (Event e in events) {
						if ((e.ModFlags & modflags) == 0)
							continue;
						if ((e.ModFlags & static_mask) != static_flags)
							continue;

						MemberInfo eb = e.EventBuilder;
						if (eb != null && filter (eb, criteria) == true)
						        members.Add (e.EventBuilder);
					}
			}
			
			if ((mt & MemberTypes.Property) != 0){
				if (properties != null)
					foreach (Property p in properties) {
						if ((p.ModFlags & modflags) == 0)
							continue;
						if ((p.ModFlags & static_mask) != static_flags)
							continue;

						MemberInfo pb = p.PropertyBuilder;
						if (pb != null && filter (pb, criteria) == true) {
							members.Add (p.PropertyBuilder);
						}
					}

				if (indexers != null)
					foreach (Indexer ix in indexers) {
						if ((ix.ModFlags & modflags) == 0)
							continue;
						if ((ix.ModFlags & static_mask) != static_flags)
							continue;

						MemberInfo ib = ix.PropertyBuilder;
						if (ib != null && filter (ib, criteria) == true) {
							members.Add (ix.PropertyBuilder);
						}
					}
			}
			
			if ((mt & MemberTypes.NestedType) != 0) {
				if (types != null){
					foreach (TypeContainer t in types) {
						if ((t.ModFlags & modflags) == 0)
							continue;

						TypeBuilder tb = t.TypeBuilder;
						if (tb != null && (filter (tb, criteria) == true))
								members.Add (tb);
					}
				}

				if (enums != null){
					foreach (Enum en in enums){
						if ((en.ModFlags & modflags) == 0)
							continue;

						TypeBuilder tb = en.TypeBuilder;
						if (tb != null && (filter (tb, criteria) == true))
							members.Add (tb);
					}
				}
				
				if (delegates != null){
					foreach (Delegate d in delegates){
						if ((d.ModFlags & modflags) == 0)
							continue;

						TypeBuilder tb = d.TypeBuilder;
						if (tb != null && (filter (tb, criteria) == true))
							members.Add (tb);
					}
				}

				if (interfaces != null){
					foreach (Interface iface in interfaces){
						if ((iface.ModFlags & modflags) == 0)
							continue;

						TypeBuilder tb = iface.TypeBuilder;
						if (tb != null && (filter (tb, criteria) == true))
							members.Add (tb);
					}
				}
			}

			if ((mt & MemberTypes.Constructor) != 0){
				if (((bf & BindingFlags.Instance) != 0) && (instance_constructors != null)){
					foreach (Constructor c in instance_constructors){
						ConstructorBuilder cb = c.ConstructorBuilder;
						if (cb != null)
							if (filter (cb, criteria) == true)
								members.Add (cb);
					}
				}

				if (((bf & BindingFlags.Static) != 0) && (default_static_constructor != null)){
					ConstructorBuilder cb =
						default_static_constructor.ConstructorBuilder;
					
					if (cb != null)
					if (filter (cb, criteria) == true)
						members.Add (cb);
				}
			}

			//
			// Lookup members in parent if requested.
			//
			if (((bf & BindingFlags.DeclaredOnly) == 0) && (TypeBuilder.BaseType != null)) {
				MemberList list = FindMembers (TypeBuilder.BaseType, mt, bf, filter, criteria);
				members.AddRange (list);
			}

			Timer.StopTimer (TimerType.TcFindMembers);

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
			TypeContainer tc = TypeManager.LookupTypeContainer (t);

			if (tc != null)
				return tc.FindMembers (mt, bf, filter, criteria);
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
					con.EmitConstant (this);
			return;
		}

		/// <summary>
		///   Emits the code, this step is performed after all
		///   the types, enumerations, constructors
		/// </summary>
		public void Emit ()
		{
			if (instance_constructors != null)
				foreach (Constructor c in instance_constructors)
					c.Emit (this);

			if (default_static_constructor != null)
				default_static_constructor.Emit (this);
			
			if (methods != null)
				foreach (Method m in methods)
					m.Emit (this);

			if (operators != null)
				foreach (Operator o in operators)
					o.Emit (this);

			if (properties != null)
				foreach (Property p in properties)
					p.Emit (this);

			if (indexers != null){
				foreach (Indexer ix in indexers)
					ix.Emit (this);
				
				CustomAttributeBuilder cb = Interface.EmitDefaultMemberAttr (
					this, IndexerName, ModFlags, Location);
				TypeBuilder.SetCustomAttribute (cb);
			}
			
			if (fields != null)
				foreach (Field f in fields)
					f.Emit (this);

			if (events != null){
				foreach (Event e in Events)
					e.Emit (this);
			}

			if (Pending != null)
				if (Pending.VerifyPendingMethods ())
					return;
			
			Attribute.ApplyAttributes (ec, TypeBuilder, this, OptAttributes);

			//
			// Check for internal or private fields that were never assigned
			//
			if (RootContext.WarningLevel >= 3) {
				if (fields != null){
					foreach (Field f in fields) {
						if ((f.ModFlags & Modifiers.PUBLIC) != 0)
							continue;
						
						if (f.status == 0){
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
			
//			if (types != null)
//				foreach (TypeContainer tc in types)
//					tc.Emit ();
		}
		
		public override void CloseType ()
		{
			try {
				if (!Created){
					Created = true;
					TypeBuilder.CreateType ();
				}
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

			if (interface_order != null){
				foreach (Interface iface in interface_order)
					iface.CloseType ();
			}
			
			if (Types != null){
				foreach (TypeContainer tc in Types)
					if (tc is Struct)
						tc.CloseType ();

				foreach (TypeContainer tc in Types)
					if (!(tc is Struct))
						tc.CloseType ();
			}

			if (Delegates != null)
				foreach (Delegate d in Delegates)
					d.CloseType ();
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

		public void Warning_KewywordNewNotRequired (Location l, MemberCore mc)
		{
			Report.Warning (
				109, l, "The member " + MakeName (mc.Name) + " does not hide an " +
				"inherited member, the keyword new is not required");
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

			if (this is Struct){
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

		// Access level of a type.
		enum AccessLevel {
			Public			= 0,
			ProtectedInternal	= 1,
			Internal		= 2,
			Protected		= 3,
			Private			= 4
		}

		// Check whether `flags' denotes a more restricted access than `level'
		// and return the new level.
		static AccessLevel CheckAccessLevel (AccessLevel level, int flags)
		{
			AccessLevel old_level = level;

			if ((flags & Modifiers.INTERNAL) != 0) {
				if ((flags & Modifiers.PROTECTED) != 0) {
					if ((int) level < (int) AccessLevel.ProtectedInternal)
						level = AccessLevel.ProtectedInternal;
				} else {
					if ((int) level < (int) AccessLevel.Internal)
						level = AccessLevel.Internal;
				}
			} else if ((flags & Modifiers.PROTECTED) != 0) {
				if ((int) level < (int) AccessLevel.Protected)
					level = AccessLevel.Protected;
			} else if ((flags & Modifiers.PRIVATE) != 0)
				level = AccessLevel.Private;

			return level;
		}

		// Return the access level for a new member which is defined in the current
		// TypeContainer with access modifiers `flags'.
		AccessLevel GetAccessLevel (int flags)
		{
			if ((flags & Modifiers.PRIVATE) != 0)
				return AccessLevel.Private;

			AccessLevel level;
			if (!IsTopLevel && (Parent != null))
				level = Parent.GetAccessLevel (flags);
			else
				level = AccessLevel.Public;

			return CheckAccessLevel (CheckAccessLevel (level, flags), ModFlags);
		}

		// Return the access level for type `t', but don't give more access than `flags'.
		static AccessLevel GetAccessLevel (Type t, int flags)
		{
			if (((flags & Modifiers.PRIVATE) != 0) || t.IsNestedPrivate)
				return AccessLevel.Private;

			AccessLevel level;
			if (TypeManager.IsBuiltinType (t))
				return AccessLevel.Public;
			else if ((t.DeclaringType != null) && (t != t.DeclaringType))
				level = GetAccessLevel (t.DeclaringType, flags);
			else {
				level = CheckAccessLevel (AccessLevel.Public, flags);
			}

			if (t.IsNestedPublic)
				return level;

			if (t.IsNestedAssembly || t.IsNotPublic) {
				if ((int) level < (int) AccessLevel.Internal)
					level = AccessLevel.Internal;
			}

			if (t.IsNestedFamily) {
				if ((int) level < (int) AccessLevel.Protected)
					level = AccessLevel.Protected;
			}

			if (t.IsNestedFamORAssem) {
				if ((int) level < (int) AccessLevel.ProtectedInternal)
					level = AccessLevel.ProtectedInternal;
			}

			return level;
		}

		//
		// Returns true if `parent' is as accessible as the flags `flags'
		// given for this member.
		//
		public bool AsAccessible (Type parent, int flags)
		{
			while (parent.IsArray || parent.IsPointer || parent.IsByRef)
				parent = parent.GetElementType ();

			AccessLevel level = GetAccessLevel (flags);
			AccessLevel level2 = GetAccessLevel (parent, flags);

			return (int) level >= (int) level2;
		}

		Hashtable builder_and_args;
		
		public bool RegisterMethod (MethodBuilder mb, InternalParameters ip, Type [] args)
		{
			if (builder_and_args == null)
				builder_and_args = new Hashtable ();
			return true;
		}

		/// <summary>
		///   Performs checks for an explicit interface implementation.  First it
		///   checks whether the `interface_type' is a base inteface implementation.
		///   Then it checks whether `name' exists in the interface type.
		/// </summary>
		public bool VerifyImplements (Type interface_type, string full, string name, Location loc)
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

		public static void Error_ExplicitInterfaceNotMemberInterface (Location loc, string name)
		{
			Report.Error (539, loc, "Explicit implementation: `" + name + "' is not a member of the interface");
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
				return false;
			}
		}

		MemberList IMemberContainer.GetMembers (MemberTypes mt, BindingFlags bf)
		{
			return FindMembers (mt, bf | BindingFlags.DeclaredOnly, null, null);
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
			
			// Register all the operators we care about.
			foreach (Operator op in operators){
				int reg = 0;
				
				switch (op.OperatorType){
				case Operator.OpType.Equality:
					reg = 1; break;
				case Operator.OpType.Inequality:
					reg = 2; break;

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
		}
		
		
	}

	public class Class : TypeContainer {
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

		public Class (TypeContainer parent, string name, int mod, Attributes attrs, Location l)
			: base (parent, name, l)
		{
			int accmods;

			if (parent.Parent == null)
				accmods = Modifiers.INTERNAL;
			else
				accmods = Modifiers.PRIVATE;

			this.ModFlags = Modifiers.Check (AllowedModifiers, mod, accmods, l);
			this.attributes = attrs;
		}

		//
		// FIXME: How do we deal with the user specifying a different
		// layout?
		//
		public override TypeAttributes TypeAttr {
			get {
				return base.TypeAttr | TypeAttributes.AutoLayout | TypeAttributes.Class;
			}
		}
	}

	public class Struct : TypeContainer {
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

		public Struct (TypeContainer parent, string name, int mod, Attributes attrs, Location l)
			: base (parent, name, l)
		{
			int accmods;
			
			if (parent.Parent == null)
				accmods = Modifiers.INTERNAL;
			else
				accmods = Modifiers.PRIVATE;
			
			this.ModFlags = Modifiers.Check (AllowedModifiers, mod, accmods, l);

			this.ModFlags |= Modifiers.SEALED;
			this.attributes = attrs;
			
		}

		//
		// FIXME: Allow the user to specify a different set of attributes
		// in some cases (Sealed for example is mandatory for a class,
		// but what SequentialLayout can be changed
		//
		public override TypeAttributes TypeAttr {
			get {
				return base.TypeAttr |
					TypeAttributes.SequentialLayout |
					TypeAttributes.Sealed |
					TypeAttributes.BeforeFieldInit;
			}
		}
	}

	public abstract class MethodCore : MemberBase {
		public readonly Parameters Parameters;
		protected Block block;
		
		//
		// Parameters, cached for semantic analysis.
		//
		protected InternalParameters parameter_info;
		protected Type [] parameter_types;

		public MethodCore (Expression type, int mod, int allowed_mod, string name,
				   Attributes attrs, Parameters parameters, Location loc)
			: base (type, mod, allowed_mod, name, attrs, loc)
		{
			Parameters = parameters;
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

		protected virtual bool DoDefineParameters (TypeContainer parent)
		{
			// Check if arguments were correct
			parameter_types = Parameters.GetParameterInfo (parent);
			if ((parameter_types == null) || !CheckParameters (parent, parameter_types))
				return false;

			parameter_info = new InternalParameters (parent, Parameters);

			return true;
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

		//
		// The method's attributes are passed in because we need to extract
		// the "return:" attribute from there to apply on the return type
		//
		public void LabelParameters (EmitContext ec, MethodBase builder, Attributes method_attrs)
		{
			//
			// Define each type attribute (in/out/ref) and
			// the argument names.
			//
			Parameter [] p = Parameters.FixedParameters;
			int i = 0;
			
			MethodBuilder mb = null;
			ConstructorBuilder cb = null;

			if (builder is MethodBuilder)
				mb = (MethodBuilder) builder;
			else
				cb = (ConstructorBuilder) builder;

			if (p != null){
				for (i = 0; i < p.Length; i++) {
					ParameterBuilder pb;
					ParameterAttributes par_attr = p [i].Attributes;
					
					if (mb == null)
						pb = cb.DefineParameter (
							i + 1, par_attr, p [i].Name);
					else 
						pb = mb.DefineParameter (
							i + 1, par_attr, p [i].Name);
					
					Attributes attr = p [i].OptAttributes;
					if (attr != null){
						Attribute.ApplyAttributes (ec, pb, pb, attr);

						if (par_attr == ParameterAttributes.Out){
							if (attr.Contains (TypeManager.in_attribute_type))
								Report.Error (36, Location, "Can not use [In] attribute on out parameter");
						}
					}
				}
			}

			if (Parameters.ArrayParameter != null){
				ParameterBuilder pb;
				Parameter array_param = Parameters.ArrayParameter;
				
				if (mb == null)
					pb = cb.DefineParameter (
						i + 1, array_param.Attributes,
						array_param.Name);
				else
					pb = mb.DefineParameter (
						i + 1, array_param.Attributes,
						array_param.Name);
					
				CustomAttributeBuilder a = new CustomAttributeBuilder (
					TypeManager.cons_param_array_attribute, new object [0]);
				
				pb.SetCustomAttribute (a);
			}

			//
			// And now for the return type attribute decoration
			//
			ParameterBuilder ret_pb;
			Attributes ret_attrs = null;
				
			if (mb == null || method_attrs == null)
				return;

			foreach (AttributeSection asec in method_attrs.AttributeSections) {

				if (asec.Target != "return")
					continue;

				if (ret_attrs == null)
					ret_attrs = new Attributes (asec);
				else
					ret_attrs.AddAttributeSection (asec);
			}

			if (ret_attrs != null) {
				try {
				 	ret_pb = mb.DefineParameter (0, ParameterAttributes.None, "");
					Attribute.ApplyAttributes (ec, ret_pb, ret_pb, ret_attrs);
				} catch (ArgumentOutOfRangeException) {
					Report.Warning (
						-24, Location,
						".NET SDK 1.0 does not permit to set custom attributes to the return type of a method");
				}
			}
		}
	}
	
	public class Method : MethodCore {
		public MethodBuilder MethodBuilder;
		public MethodData MethodData;

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

		//
		// return_type can be "null" for VOID values.
		//
		public Method (Expression return_type, int mod, string name, Parameters parameters,
			       Attributes attrs, Location l)
			: base (return_type, mod, AllowedModifiers, name, attrs, parameters, l)
		{ }

		//
		// Returns the `System.Type' for the ReturnType of this
		// function.  Provides a nice cache.  (used between semantic analysis
		// and actual code generation
		//
		public Type GetReturnType ()
		{
			return MemberType;
		}

		// Whether this is an operator method.
		public bool IsOperator;

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
			if (RootContext.WarningLevel < 4) 
				return;
				
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
                            (t.GetElementType() == TypeManager.string_type) &&
                            (pinfo.ParameterModifier(0) == Parameter.Modifier.NONE))
                                return true;
                        else
                                return false;
                }

		//
		// Checks our base implementation if any
		//
		protected override bool CheckBase (TypeContainer parent)
		{
			// Check whether arguments were correct.
			if (!DoDefineParameters (parent))
				return false;

			MethodSignature ms = new MethodSignature (Name, null, ParameterTypes);
			if (IsOperator) {
				flags |= MethodAttributes.SpecialName | MethodAttributes.HideBySig;
			} else {
				MemberList mi_this;

				mi_this = TypeContainer.FindMembers (
					parent.TypeBuilder, MemberTypes.Method,
					BindingFlags.NonPublic | BindingFlags.Public |
					BindingFlags.Static | BindingFlags.Instance |
					BindingFlags.DeclaredOnly,
					MethodSignature.method_signature_filter, ms);

				if (mi_this.Count > 0) {
					Report.Error (111, Location, "Class `" + parent.Name + "' " +
						      "already defines a member called `" + Name + "' " +
						      "with the same parameter types");
					return false;
				}
			} 

			//
			// Verify if the parent has a type with the same name, and then
			// check whether we have to create a new slot for it or not.
			//
			Type ptype = parent.TypeBuilder.BaseType;

			// ptype is only null for System.Object while compiling corlib.
			if (ptype != null){
				MemberList mi, mi_static, mi_instance;

				mi_instance = TypeContainer.FindMembers (
					ptype, MemberTypes.Method,
					BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance,
					MethodSignature.inheritable_method_signature_filter,
					ms);

				if (mi_instance.Count > 0){
					mi = mi_instance;
				} else {
					mi_static = TypeContainer.FindMembers (
						ptype, MemberTypes.Method,
						BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static,
						MethodSignature.inheritable_method_signature_filter, ms);

					if (mi_static.Count > 0)
						mi = mi_static;
					else
						mi = null;
				}

				if (mi != null && mi.Count > 0){
					parent_method = (MethodInfo) mi [0];
					string name = parent_method.DeclaringType.Name + "." +
						parent_method.Name;

					if (!CheckMethodAgainstBase (parent, flags, parent_method, name))
						return false;

					if ((ModFlags & Modifiers.NEW) == 0) {
						Type parent_ret = TypeManager.TypeToCoreType (
							parent_method.ReturnType);

						if (parent_ret != MemberType) {
							Report.Error (
								508, Location, parent.MakeName (Name) + ": cannot " +
								"change return type when overriding " +
								"inherited member " + name);
							return false;
						}
					}
				} else {
					if ((ModFlags & Modifiers.NEW) != 0)
						WarningNotHiding (parent);

					if ((ModFlags & Modifiers.OVERRIDE) != 0){
						Report.Error (115, Location,
							      parent.MakeName (Name) +
							      " no suitable methods found to override");
					}
				}
			} else if ((ModFlags & Modifiers.NEW) != 0)
				WarningNotHiding (parent);

			return true;
		}

		//
		// Creates the type
		//
		public override bool Define (TypeContainer parent)
		{
			if (!DoDefine (parent))
				return false;

			if (!CheckBase (parent))
				return false;

			CallingConventions cc = GetCallingConvention (parent is Class);

			MethodData = new MethodData (this, null, MemberType, ParameterTypes,
						     ParameterInfo, cc, OptAttributes,
						     ModFlags, flags, true);

			if (!MethodData.Define (parent))
				return false;

			MethodBuilder = MethodData.MethodBuilder;
			
			//
			// This is used to track the Entry Point,
			//
			if (Name == "Main" &&
			    ((ModFlags & Modifiers.STATIC) != 0) && 
			    (RootContext.MainClass == null ||
			     RootContext.MainClass == parent.TypeBuilder.FullName)){
                                if (IsEntryPoint (MethodBuilder, ParameterInfo)) {
                                        if (RootContext.EntryPoint == null) {
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
		public void Emit (TypeContainer parent)
		{
			MethodData.Emit (parent, Block, this);
		}
	}

	public abstract class ConstructorInitializer {
		ArrayList argument_list;
		ConstructorInfo parent_constructor;
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

		public bool Resolve (EmitContext ec)
		{
			Expression parent_constructor_group;
			Type t;

			ec.CurrentBlock = new Block (null, true, parameters);

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
				ec, t, null, t, ".ctor", 
				MemberTypes.Constructor,
				BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly,
				loc);
			
			if (parent_constructor_group == null){
				Report.Error (1501, loc,
				       "Can not find a constructor for this argument list");
				return false;
			}
			
			parent_constructor = (ConstructorInfo) Invocation.OverloadResolve (ec, 
				(MethodGroupExpr) parent_constructor_group, argument_list, loc);
			
			if (parent_constructor == null){
				Report.Error (1501, loc,
				       "Can not find a constructor for this argument list");
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
					Invocation.EmitCall (ec, true, false, ec.This, parent_constructor, argument_list, loc);
			}
		}
	}

	public class ConstructorBaseInitializer : ConstructorInitializer {
		public ConstructorBaseInitializer (ArrayList argument_list, Parameters pars, Location l) :
			base (argument_list, pars, l)
		{
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
		new public Attributes OptAttributes;

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

		//
		// The spec claims that static is not permitted, but
		// my very own code has static constructors.
		//
		public Constructor (string name, Parameters args, ConstructorInitializer init, Location l)
			: base (null, 0, AllowedModifiers, name, null, args, l)
		{
			Initializer = init;
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

		//
		// Creates the ConstructorBuilder
		//
		public override bool Define (TypeContainer parent)
		{
			MethodAttributes ca = (MethodAttributes.RTSpecialName |
					       MethodAttributes.SpecialName);

			// Check if arguments were correct.
			if (!DoDefineParameters (parent))
				return false;

			if ((ModFlags & Modifiers.STATIC) != 0)
				ca |= MethodAttributes.Static;
			else {
				if (parent is Struct && ParameterTypes.Length == 0){
					Report.Error (
						568, Location, 
						"Structs can not contain explicit parameterless " +
						"constructors");
					return false;
				}
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

			ConstructorBuilder = parent.TypeBuilder.DefineConstructor (
				ca, GetCallingConvention (parent is Class), ParameterTypes);

			//
			// HACK because System.Reflection.Emit is lame
			//
			if (!TypeManager.RegisterMethod (ConstructorBuilder, ParameterInfo, ParameterTypes)) {
				Report.Error (
					111, Location,
					"Class `" +parent.Name+ "' already contains a definition with the " +
					"same return value and parameter types for constructor `" + Name
					+ "'");
				return false;
			}

			return true;
		}

		//
		// Emits the code
		//
		public void Emit (TypeContainer parent)
		{
			ILGenerator ig = ConstructorBuilder.GetILGenerator ();
			EmitContext ec = new EmitContext (parent, Location, ig, null, ModFlags, true);

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
				if (parent is Class && Initializer == null)
					Initializer = new ConstructorBaseInitializer (
						null, Parameters.EmptyReadOnlyParameters, Location.Null);


				//
				// Spec mandates that Initializers will not have
				// `this' access
				//
				ec.IsStatic = true;
				if (Initializer != null && !Initializer.Resolve (ec))
					return;
				ec.IsStatic = false;
			}

			LabelParameters (ec, ConstructorBuilder, OptAttributes);
			
			SymbolWriter sw = CodeGen.SymbolWriter;
			bool generate_debugging = false;

			if ((sw != null) && (block != null) &&
				!Location.IsNull (Location) &&
				!Location.IsNull (block.EndLocation)) {

				sw.OpenMethod (parent, ConstructorBuilder, Location, block.EndLocation);

				generate_debugging = true;
			}

			//
			// Classes can have base initializers and instance field initializers.
			//
			if (parent is Class){
				if ((ModFlags & Modifiers.STATIC) == 0)
					parent.EmitFieldInitializers (ec);
			}
			if (Initializer != null)
				Initializer.Emit (ec);
			
			if ((ModFlags & Modifiers.STATIC) != 0)
				parent.EmitFieldInitializers (ec);

			Attribute.ApplyAttributes (ec, ConstructorBuilder, this, OptAttributes);

			// If this is a non-static `struct' constructor and doesn't have any
			// initializer, it must initialize all of the struct's fields.
			if ((parent is Struct) && ((ModFlags & Modifiers.STATIC) == 0) &&
			    (Initializer == null))
				Block.AddThisVariable (parent, Location);

			ec.EmitTopBlock (block, ParameterInfo, Location);

			if (generate_debugging)
				sw.CloseMethod ();
		}
	}

	public class MethodData {
		//
		// The return type of this method
		//
		public readonly Type ReturnType;
		public readonly Type[] ParameterTypes;
		public readonly InternalParameters ParameterInfo;
		public readonly CallingConventions CallingConventions;
		public readonly Attributes OptAttributes;
		public readonly Location Location;

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
		protected bool is_method;
		protected string accessor_name;
		ArrayList conditionals;

		MethodBuilder builder = null;
		public MethodBuilder MethodBuilder {
			get {
				return builder;
			}
		}

		public MethodData (MemberBase member, string name, Type return_type,
				   Type [] parameter_types, InternalParameters parameters,
				   CallingConventions cc, Attributes opt_attrs,
				   int modifiers, MethodAttributes flags, bool is_method)
		{
			this.member = member;
			this.accessor_name = name;
			this.ReturnType = return_type;
			this.ParameterTypes = parameter_types;
			this.ParameterInfo = parameters;
			this.CallingConventions = cc;
			this.OptAttributes = opt_attrs;
			this.modifiers = modifiers;
			this.flags = flags;
			this.is_method = is_method;
			this.Location = member.Location;
			this.conditionals = new ArrayList ();
		}

		//
		// Attributes.
		//
		Attribute dllimport_attribute = null;
		string obsolete = null;
		bool obsolete_error = false;

		public virtual bool ApplyAttributes (Attributes opt_attrs, bool is_method)
		{
			if ((opt_attrs == null) || (opt_attrs.AttributeSections == null))
				return true;

			foreach (AttributeSection asec in opt_attrs.AttributeSections) {
				if (asec.Attributes == null)
					continue;
					
				foreach (Attribute a in asec.Attributes) {
					if (a.Name == "Conditional") {
						if (!ApplyConditionalAttribute (a))
							return false;
					} else if (a.Name == "Obsolete") {
						if (!ApplyObsoleteAttribute (a))
							return false;
					} else if (a.Name.IndexOf ("DllImport") != -1) {
						if (!is_method) {
							a.Type = TypeManager.dllimport_type;
							Attribute.Error_AttributeNotValidForElement (a, Location);
							return false;
						}
						if (!ApplyDllImportAttribute (a))
							return false;
					}
				}
			}

			return true;
		}

		//
		// Applies the `DllImport' attribute to the method.
		//
		protected virtual bool ApplyDllImportAttribute (Attribute a)
		{
			const int extern_static = Modifiers.EXTERN | Modifiers.STATIC;
			if ((modifiers & extern_static) != extern_static) {
				Report.Error (601, Location,
					      "The DllImport attribute must be specified on a method " +
					      "marked `static' and `extern'.");
				return false;
			}

			flags |= MethodAttributes.PinvokeImpl;
			dllimport_attribute = a;
			return true;
		}

		//
		// Applies the `Obsolete' attribute to the method.
		//
		protected virtual bool ApplyObsoleteAttribute (Attribute a)
		{
			if (obsolete != null) {
				Report.Error (579, Location, "Duplicate `Obsolete' attribute");
				return false;
			}

			obsolete = a.Obsolete_GetObsoleteMessage (out obsolete_error);
			return obsolete != null;
		}

		//
		// Applies the `Conditional' attribute to the method.
		//
		protected virtual bool ApplyConditionalAttribute (Attribute a)
		{
			// The Conditional attribute is only valid on methods.
			if (!is_method) {
				Attribute.Error_AttributeNotValidForElement (a, Location);
				return false;
			}

			string condition = a.Conditional_GetConditionName ();

			if (condition == null)
				return false;

			if (ReturnType != TypeManager.void_type) {
				Report.Error (578, Location,
					      "Conditional not valid on `" + member.Name + "' " +
					      "because its return type is not void");
				return false;
			}

			if ((modifiers & Modifiers.OVERRIDE) != 0) {
				Report.Error (243, Location,
					      "Conditional not valid on `" + member.Name + "' " +
					      "because it is an override method");
				return false;
			}

			if (member.IsExplicitImpl) {
				Report.Error (577, Location,
					      "Conditional not valid on `" + member.Name + "' " +
					      "because it is an explicit interface implementation");
				return false;
			}

			if (IsImplementing) {
				Report.Error (623, Location,
					      "Conditional not valid on `" + member.Name + "' " +
					      "because it is an interface method");
				return false;
			}

			conditionals.Add (condition);

			return true;
		}

		//
		// Checks whether this method should be ignored due to its Conditional attributes.
		//
		bool ShouldIgnore (Location loc)
		{
			// When we're overriding a virtual method, we implicitly inherit the
			// Conditional attributes from our parent.
			if (member.ParentMethod != null) {
				TypeManager.MethodFlags flags = TypeManager.GetMethodFlags (
					member.ParentMethod, loc);

				if ((flags & TypeManager.MethodFlags.ShouldIgnore) != 0)
					return true;
			}

			foreach (string condition in conditionals)
				if (RootContext.AllDefines [condition] == null)
					return true;

			return false;
		}

		//
		// Returns the TypeManager.MethodFlags for this method.
		// This emits an error 619 / warning 618 if the method is obsolete.
		// In the former case, TypeManager.MethodFlags.IsObsoleteError is returned.
		//
		public virtual TypeManager.MethodFlags GetMethodFlags (Location loc)
		{
			TypeManager.MethodFlags flags = 0;

			if (obsolete != null) {
				if (obsolete_error) {
					Report.Error (619, loc, "Method `" + member.Name +
						      "' is obsolete: `" + obsolete + "'");
					return TypeManager.MethodFlags.IsObsoleteError;
				} else
					Report.Warning (618, loc, "Method `" + member.Name +
							"' is obsolete: `" + obsolete + "'");

				flags |= TypeManager.MethodFlags.IsObsolete;
			}

			if (ShouldIgnore (loc))
			    flags |= TypeManager.MethodFlags.ShouldIgnore;

			return flags;
		}

		public virtual bool Define (TypeContainer parent)
		{
			MethodInfo implementing = null;
			string method_name, name, prefix;

			if (OptAttributes != null)
				if (!ApplyAttributes (OptAttributes, is_method))
					return false;

			if (member.IsExplicitImpl)
				prefix = member.InterfaceType.FullName + ".";
			else
				prefix = "";

			if (accessor_name != null)
				name = accessor_name + "_" + member.ShortName;
			else
				name = member.ShortName;
			method_name = prefix + name;

			if (parent.Pending != null){
				if (member is Indexer)
					implementing = parent.Pending.IsInterfaceIndexer (
						member.InterfaceType, ReturnType, ParameterTypes);
				else
					implementing = parent.Pending.IsInterfaceMethod (
						member.InterfaceType, name, ReturnType, ParameterTypes);

				if (member.InterfaceType != null && implementing == null){
					TypeContainer.Error_ExplicitInterfaceNotMemberInterface (
						Location, name);
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
						Modifiers.Error_InvalidModifier (Location, "public, virtual or abstract");
						implementing = null;
					}
				} else {
					//
					// We already catch different accessibility settings
					// so we just need to check that we are not private
					//
					if ((modifiers & Modifiers.PRIVATE) != 0)
						implementing = null;
				} 
					
				//
				// Static is not allowed
				//
				if ((modifiers & Modifiers.STATIC) != 0){
					implementing = null;
					Modifiers.Error_InvalidModifier (Location, "static");
				}
			}
			
			//
			// If implementing is still valid, set flags
			//
			if (implementing != null){
				//
				// When implementing interface methods, set NewSlot.
				//
				if (implementing.DeclaringType.IsInterface)
					flags |= MethodAttributes.NewSlot;

				flags |=
					MethodAttributes.Virtual |
					MethodAttributes.HideBySig;

				// Get the method name from the explicit interface.
				if (member.InterfaceType != null) {
					name = implementing.Name;
					method_name = prefix + name;
				}

				IsImplementing = true;
			}

			//
			// Create the MethodBuilder for the method
			//
			if ((flags & MethodAttributes.PinvokeImpl) != 0) {
				if ((modifiers & Modifiers.STATIC) == 0) {
					Report.Error (601, Location,
						      "The DllImport attribute must be specified on " +
						      "a method marked 'static' and 'extern'.");
					return false;
				}
				
				EmitContext ec = new EmitContext (
					parent, Location, null, ReturnType, modifiers);
				
				builder = dllimport_attribute.DefinePInvokeMethod (
					ec, parent.TypeBuilder, method_name, flags,
					ReturnType, ParameterTypes);
			} else
				builder = parent.TypeBuilder.DefineMethod (
					method_name, flags, CallingConventions,
					ReturnType, ParameterTypes);

			if (builder == null)
				return false;

			if (IsImplementing) {
				//
				// clear the pending implemntation flag
				//
				if (member is Indexer) {
					parent.Pending.ImplementIndexer (
						member.InterfaceType, builder, ReturnType,
						ParameterTypes, true);
				} else
					parent.Pending.ImplementMethod (
						member.InterfaceType, name, ReturnType,
						ParameterTypes, member.IsExplicitImpl);

				if (member.IsExplicitImpl)
					parent.TypeBuilder.DefineMethodOverride (
						builder, implementing);
			}

			if (!TypeManager.RegisterMethod (builder, ParameterInfo, ParameterTypes)) {
				Report.Error (111, Location,
					      "Class `" + parent.Name +
					      "' already contains a definition with the " +
					      "same return value and parameter types as the " +
					      "'get' method of property `" + member.Name + "'");
				return false;
			}

			TypeManager.AddMethod (builder, this);

			return true;
		}

		//
		// Emits the code
		// 
		public virtual void Emit (TypeContainer parent, Block block, object kind)
		{
			ILGenerator ig;
			EmitContext ec;

			if ((flags & MethodAttributes.PinvokeImpl) == 0)
				ig = builder.GetILGenerator ();
			else
				ig = null;

			ec = new EmitContext (parent, Location, ig, ReturnType, modifiers);

			if (OptAttributes != null)
				Attribute.ApplyAttributes (ec, builder, kind, OptAttributes);

			if (member is MethodCore)
				((MethodCore) member).LabelParameters (ec, MethodBuilder, OptAttributes);

			//
			// abstract or extern methods have no bodies
			//
			if ((modifiers & (Modifiers.ABSTRACT | Modifiers.EXTERN)) != 0){
				if (block == null) {
					SymbolWriter sw = CodeGen.SymbolWriter;

					if ((sw != null) && ((modifiers & Modifiers.EXTERN) != 0)) {
						sw.OpenMethod (parent, MethodBuilder, Location, Location);
						sw.CloseMethod ();
					}

					return;
				}

				//
				// abstract or extern methods have no bodies.
				//
				if ((modifiers & Modifiers.ABSTRACT) != 0)
					Report.Error (
						500, Location, "Abstract method `" +
						TypeManager.CSharpSignature (builder) +
						"' can not have a body");

				if ((modifiers & Modifiers.EXTERN) != 0)
					Report.Error (
						179, Location, "External method `" +
						TypeManager.CSharpSignature (builder) +
						"' can not have a body");

				return;
			}

			//
			// Methods must have a body unless they're extern or abstract
			//
			if (block == null) {
				Report.Error (
					501, Location, "Method `" +
					TypeManager.CSharpSignature (builder) +
					"' must declare a body since it is not marked " +
					"abstract or extern");
				return;
			}

			//
			// Handle destructors specially
			//
			// FIXME: This code generates buggy code
			//
			if (member.Name == "Finalize" && ReturnType == TypeManager.void_type)
				EmitDestructor (ec, block);
			else {
				SymbolWriter sw = CodeGen.SymbolWriter;

				if ((sw != null) && !Location.IsNull (Location) &&
				    !Location.IsNull (block.EndLocation)) {
					sw.OpenMethod (parent, MethodBuilder, Location, block.EndLocation);

					ec.EmitTopBlock (block, ParameterInfo, Location);

					sw.CloseMethod ();
				} else
					ec.EmitTopBlock (block, ParameterInfo, Location);
			}
		}

		void EmitDestructor (EmitContext ec, Block block)
		{
			ILGenerator ig = ec.ig;
			
			Label finish = ig.DefineLabel ();
			bool old_in_try = ec.InTry;
			
			ig.BeginExceptionBlock ();
			ec.InTry = true;
			ec.ReturnLabel = finish;
			ec.HasReturnLabel = true;
			ec.EmitTopBlock (block, null, Location);
			ec.InTry = old_in_try;
			
			// ig.MarkLabel (finish);
			bool old_in_finally = ec.InFinally;
			ec.InFinally = true;
			ig.BeginFinallyBlock ();
			
			if (ec.ContainerType.BaseType != null) {
				Expression member_lookup = Expression.MemberLookup (
					ec, ec.ContainerType.BaseType, null, ec.ContainerType.BaseType,
					"Finalize", MemberTypes.Method, Expression.AllBindingFlags, Location);

				if (member_lookup != null){
					MethodGroupExpr parent_destructor = ((MethodGroupExpr) member_lookup);
				
					ig.Emit (OpCodes.Ldarg_0);
					ig.Emit (OpCodes.Call, (MethodInfo) parent_destructor.Methods [0]);
				}
			}
			ec.InFinally = old_in_finally;
			
			ig.EndExceptionBlock ();
			//ig.MarkLabel (ec.ReturnLabel);
			ig.Emit (OpCodes.Ret);
		}
	}

	abstract public class MemberBase : MemberCore {
		public Expression Type;
		public readonly Attributes OptAttributes;

		protected MethodAttributes flags;

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
		public string ExplicitInterfaceName = null;

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
		protected MemberBase (Expression type, int mod, int allowed_mod, string name,
				      Attributes attrs, Location loc)
			: base (name, loc)
		{
			Type = type;
			ModFlags = Modifiers.Check (allowed_mod, mod, Modifiers.PRIVATE, loc);
			OptAttributes = attrs;
		}

		protected virtual bool CheckBase (TypeContainer parent)
		{
			return true;
		}

		protected virtual bool CheckParameters (TypeContainer parent, Type [] parameters)
		{
			bool error = false;

			foreach (Type partype in parameters){
				if (partype.IsPointer){
					if (!UnsafeOK (parent))
						error = true;
					if (!TypeManager.VerifyUnManaged (partype.GetElementType (), Location))
						error = true;
				}

				if (parent.AsAccessible (partype, ModFlags))
					continue;

				if (this is Indexer)
					Report.Error (55, Location,
						      "Inconsistent accessibility: parameter type `" +
						      TypeManager.CSharpName (partype) + "' is less " +
						      "accessible than indexer `" + Name + "'");
				else
					Report.Error (51, Location,
						      "Inconsistent accessibility: parameter type `" +
						      TypeManager.CSharpName (partype) + "' is less " +
						      "accessible than method `" + Name + "'");
				error = true;
			}

			return !error;
		}

		protected virtual bool DoDefine (TypeContainer parent)
		{
			if (Name == null)
				Name = "this";

			if (!parent.MethodModifiersValid (ModFlags, Name, Location))
				return false;

			flags = Modifiers.MethodAttr (ModFlags);

			// Lookup Type, verify validity
			MemberType = parent.ResolveType (Type, false, Location);
			if (MemberType == null)
				return false;

			if ((parent.ModFlags & Modifiers.SEALED) != 0){
				if ((ModFlags & (Modifiers.VIRTUAL|Modifiers.ABSTRACT)) != 0){
					Report.Error (549, Location, "Virtual method can not be contained in sealed class");
					return false;
				}
			}
			
			// verify accessibility
			if (!parent.AsAccessible (MemberType, ModFlags)) {
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
				else if (this is Method)
					Report.Error (50, Location,
						      "Inconsistent accessibility: return type `" +
						      TypeManager.CSharpName (MemberType) + "' is less " +
						      "accessible than method `" + Name + "'");
				else
					Report.Error (52, Location,
						      "Inconsistent accessibility: field type `" +
						      TypeManager.CSharpName (MemberType) + "' is less " +
						      "accessible than field `" + Name + "'");
				return false;
			}

			if (MemberType.IsPointer && !UnsafeOK (parent))
				return false;
			
			//
			// Check for explicit interface implementation
			//
			if ((ExplicitInterfaceName == null) && (Name.IndexOf (".") != -1)){
				int pos = Name.LastIndexOf (".");

				ExplicitInterfaceName = Name.Substring (0, pos);
				ShortName = Name.Substring (pos + 1);
			} else
				ShortName = Name;

			if (ExplicitInterfaceName != null) {
				InterfaceType  = RootContext.LookupType (
					parent, ExplicitInterfaceName, false, Location);
				if (InterfaceType == null)
					return false;

				// Compute the full name that we need to export.
				Name = InterfaceType.FullName + "." + ShortName;
				
				if (!parent.VerifyImplements (InterfaceType, ShortName, Name, Location))
					return false;
				
				IsExplicitImpl = true;
			} else
				IsExplicitImpl = false;

			return true;
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

		//
		// The constructor is only exposed to our children
		//
		protected FieldBase (Expression type, int mod, int allowed_mod, string name,
				     object init, Attributes attrs, Location loc)
			: base (type, mod, allowed_mod, name, attrs, loc)
		{
			this.init = init;
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

		public Field (Expression type, int mod, string name, Object expr_or_array_init,
			      Attributes attrs, Location loc)
			: base (type, mod, AllowedModifiers, name, expr_or_array_init, attrs, loc)
		{
		}

		public override bool Define (TypeContainer parent)
		{
			Type t = parent.ResolveType (Type, false, Location);
			
			if (t == null)
				return false;

			if (!parent.AsAccessible (t, ModFlags)) {
				Report.Error (52, Location,
					      "Inconsistent accessibility: field type `" +
					      TypeManager.CSharpName (t) + "' is less " +
					      "accessible than field `" + Name + "'");
				return false;
			}

			if (RootContext.WarningLevel > 1){
				Type ptype = parent.TypeBuilder.BaseType;

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
				if (!t.IsClass){
					Type vt = t;
					
					if (TypeManager.IsEnumType (vt))
						vt = TypeManager.EnumToUnderlying (t);

					if (!((vt == TypeManager.bool_type) ||
					      (vt == TypeManager.sbyte_type) ||
					      (vt == TypeManager.byte_type) ||
					      (vt == TypeManager.short_type) ||    
					      (vt == TypeManager.ushort_type) ||
					      (vt == TypeManager.int32_type) ||    
					      (vt == TypeManager.uint32_type) ||    
					      (vt == TypeManager.char_type) ||    
					      (vt == TypeManager.float_type))){
						Report.Error (
							677, Location, parent.MakeName (Name) +
							" A volatile field can not be of type `" +
							TypeManager.CSharpName (vt) + "'");
						return false;
					}
				}
			}

			FieldAttributes fa = Modifiers.FieldAttr (ModFlags);

			if (parent is Struct && 
			    ((fa & FieldAttributes.Static) == 0) &&
			    t == parent.TypeBuilder &&
			    !TypeManager.IsBuiltinType (t)){
				Report.Error (523, Location, "Struct member `" + parent.Name + "." + Name + 
					      "' causes a cycle in the structure layout");
				return false;
			}
			FieldBuilder = parent.TypeBuilder.DefineField (
				Name, t, Modifiers.FieldAttr (ModFlags));

			TypeManager.RegisterFieldBase (FieldBuilder, this);
			return true;
		}

		public void Emit (TypeContainer tc)
		{
			EmitContext ec = new EmitContext (tc, Location, null,
							  FieldBuilder.FieldType, ModFlags);

			Attribute.ApplyAttributes (ec, FieldBuilder, this, OptAttributes);
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
		public Attributes OptAttributes;
		
		public Accessor (Block b, Attributes attrs)
		{
			Block = b;
			OptAttributes = attrs;
		}
	}

	//
	// Properties and Indexers both generate PropertyBuilders, we use this to share 
	// their common bits.
	//
	abstract public class PropertyBase : MethodCore {
		public Accessor Get, Set;
		public PropertyBuilder PropertyBuilder;
		public MethodBuilder GetBuilder, SetBuilder;
		public MethodData GetData, SetData;

		protected EmitContext ec;

		public PropertyBase (Expression type, string name, int mod_flags, int allowed_mod,
				     Parameters parameters, Accessor get_block, Accessor set_block,
				     Attributes attrs, Location loc)
			: base (type, mod_flags, allowed_mod, name, attrs, parameters, loc)
		{
			Get = get_block;
			Set = set_block;
		}

		protected override bool DoDefine (TypeContainer parent)
		{
			if (!base.DoDefine (parent))
				return false;

			ec = new EmitContext (parent, Location, null, MemberType, ModFlags);

			return true;
		}

		//
		// Checks our base implementation if any
		//
		protected override bool CheckBase (TypeContainer parent)
		{
			// Check whether arguments were correct.
			if (!DoDefineParameters (parent))
				return false;

			if (IsExplicitImpl)
				return true;

			string report_name;
			MethodSignature ms, base_ms;
			if (this is Indexer) {
				string name, base_name;

				report_name = "this";
				name = TypeManager.IndexerPropertyName (parent.TypeBuilder);
				ms = new MethodSignature (name, null, ParameterTypes);
				base_name = TypeManager.IndexerPropertyName (parent.TypeBuilder.BaseType);
				base_ms = new MethodSignature (base_name, null, ParameterTypes);
			} else {
				report_name = Name;
				ms = base_ms = new MethodSignature (Name, null, ParameterTypes);
			}

			//
			// Verify if the parent has a type with the same name, and then
			// check whether we have to create a new slot for it or not.
			//
			Type ptype = parent.TypeBuilder.BaseType;

			// ptype is only null for System.Object while compiling corlib.
			if (ptype == null) {
				if ((ModFlags & Modifiers.NEW) != 0)
					WarningNotHiding (parent);

				return true;
			}

			MemberList props_this;

			props_this = TypeContainer.FindMembers (
				parent.TypeBuilder, MemberTypes.Property,
				BindingFlags.NonPublic | BindingFlags.Public |
				BindingFlags.Static | BindingFlags.Instance |
				BindingFlags.DeclaredOnly,
				MethodSignature.method_signature_filter, ms);

			if (props_this.Count > 0) {
				Report.Error (111, Location, "Class `" + parent.Name + "' " +
					      "already defines a member called `" + report_name + "' " +
					      "with the same parameter types");
				return false;
			}

			MemberList mi_props;

			mi_props = TypeContainer.FindMembers (
				ptype, MemberTypes.Property,
				BindingFlags.NonPublic | BindingFlags.Public |
				BindingFlags.Instance | BindingFlags.Static,
				MethodSignature.inheritable_method_signature_filter, base_ms);

			if (mi_props.Count > 0){
				PropertyInfo parent_property = (PropertyInfo) mi_props [0];
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

				if (!CheckMethodAgainstBase (parent, flags, parent_method, name))
					return false;

				if ((ModFlags & Modifiers.NEW) == 0) {
					Type parent_type = TypeManager.TypeToCoreType (
						parent_property.PropertyType);

					if (parent_type != MemberType) {
						Report.Error (
							508, Location, parent.MakeName (Name) + ": cannot " +
							"change return type when overriding " +
							"inherited member " + name);
						return false;
					}
				}
			} else {
				if ((ModFlags & Modifiers.NEW) != 0)
					WarningNotHiding (parent);

				if ((ModFlags & Modifiers.OVERRIDE) != 0){
					if (this is Indexer)
						Report.Error (115, Location,
							      parent.MakeName (Name) +
							      " no suitable indexers found to override");
					else
						Report.Error (115, Location,
							      parent.MakeName (Name) +
							      " no suitable properties found to override");
					return false;
				}
			}
			return true;
		}

		public void Emit (TypeContainer tc)
		{
			//
			// The PropertyBuilder can be null for explicit implementations, in that
			// case, we do not actually emit the ".property", so there is nowhere to
			// put the attribute
			//
			if (PropertyBuilder != null)
				Attribute.ApplyAttributes (ec, PropertyBuilder, this, OptAttributes);

			if (GetData != null)
				GetData.Emit (tc, Get.Block, Get);

			if (SetData != null)
				SetData.Emit (tc, Set.Block, Set);
		}
	}
			
	public class Property : PropertyBase {
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

		public Property (Expression type, string name, int mod_flags,
				 Accessor get_block, Accessor set_block,
				 Attributes attrs, Location loc)
			: base (type, name, mod_flags, AllowedModifiers,
				Parameters.EmptyReadOnlyParameters,
				get_block, set_block, attrs, loc)
		{
		}

		public override bool Define (TypeContainer parent)
		{
			if (!DoDefine (parent))
				return false;

			if (!CheckBase (parent))
				return false;

			flags |= MethodAttributes.HideBySig | MethodAttributes.SpecialName;

			if (Get != null) {
				Type [] parameters = TypeManager.NoTypes;

				InternalParameters ip = new InternalParameters (
					parent, Parameters.EmptyReadOnlyParameters);

				GetData = new MethodData (this, "get", MemberType,
							  parameters, ip, CallingConventions.Standard,
							  Get.OptAttributes, ModFlags, flags, false);
				
				if (!GetData.Define (parent))
					return false;

				GetBuilder = GetData.MethodBuilder;
			}

			if (Set != null) {
				Type [] parameters = new Type [1];
				parameters [0] = MemberType;

				Parameter [] parms = new Parameter [1];
				parms [0] = new Parameter (Type, "value", Parameter.Modifier.NONE, null);
				InternalParameters ip = new InternalParameters (
					parent, new Parameters (parms, null, Location));

				SetData = new MethodData (this, "set", TypeManager.void_type,
							  parameters, ip, CallingConventions.Standard,
							  Set.OptAttributes, ModFlags, flags, false);

				if (!SetData.Define (parent))
					return false;

				SetBuilder = SetData.MethodBuilder;
				SetBuilder.DefineParameter (1, ParameterAttributes.None, "value"); 
			}

			// FIXME - PropertyAttributes.HasDefault ?
			
			PropertyAttributes prop_attr =
			PropertyAttributes.RTSpecialName |
			PropertyAttributes.SpecialName;

			if (!IsExplicitImpl){
				PropertyBuilder = parent.TypeBuilder.DefineProperty (
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
						"Class `" + parent.Name +
						"' already contains a definition for the property `" +
						Name + "'");
					return false;
				}
			}
			return true;
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
	
	public class Event : FieldBase {
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

		public readonly Accessor  Add;
		public readonly Accessor  Remove;
		public MyEventBuilder     EventBuilder;

		MethodBuilder AddBuilder, RemoveBuilder;
		MethodData AddData, RemoveData;
		
		public Event (Expression type, string name, Object init, int mod, Accessor add,
			      Accessor remove, Attributes attrs, Location loc)
			: base (type, mod, AllowedModifiers, name, init, attrs, loc)
		{
			Add = add;
			Remove = remove;
		}

		public override bool Define (TypeContainer parent)
		{
			EventAttributes e_attr = EventAttributes.RTSpecialName | EventAttributes.SpecialName;

			if (!DoDefine (parent))
				return false;

			if (init != null && ((ModFlags & Modifiers.ABSTRACT) != 0)){
				Report.Error (74, Location, "'" + parent.Name + "." + Name +
					      "': abstract event can not have an initializer");
				return false;
			}
			
			if (!MemberType.IsSubclassOf (TypeManager.delegate_type)) {
				Report.Error (66, Location, "'" + parent.Name + "." + Name +
					      "' : event must be of a delegate type");
				return false;
			}

			Type [] parameter_types = new Type [1];
			parameter_types [0] = MemberType;

			Parameter [] parms = new Parameter [1];
			parms [0] = new Parameter (Type, "value", Parameter.Modifier.NONE, null);
			InternalParameters ip = new InternalParameters (
				parent, new Parameters (parms, null, Location)); 

			if (!CheckBase (parent))
				return false;

			//
			// Now define the accessors
			//
			AddData = new MethodData (this, "add", TypeManager.void_type,
						  parameter_types, ip, CallingConventions.Standard,
						  (Add != null) ? Add.OptAttributes : null,
						  ModFlags, flags, false);

			if (!AddData.Define (parent))
				return false;

			AddBuilder = AddData.MethodBuilder;
			AddBuilder.DefineParameter (1, ParameterAttributes.None, "value");

			RemoveData = new MethodData (this, "remove", TypeManager.void_type,
						     parameter_types, ip, CallingConventions.Standard,
						     (Remove != null) ? Remove.OptAttributes : null,
						     ModFlags, flags, false);

			if (!RemoveData.Define (parent))
				return false;

			RemoveBuilder = RemoveData.MethodBuilder;
			RemoveBuilder.DefineParameter (1, ParameterAttributes.None, "value");

			if (!IsExplicitImpl){
				EventBuilder = new MyEventBuilder (this,
					parent.TypeBuilder, Name, e_attr, MemberType);
					
				if (Add == null && Remove == null) {
					FieldBuilder = parent.TypeBuilder.DefineField (
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
						      "Class `" + parent.Name +
						      "' already contains a definition for the event `" +
						      Name + "'");
					return false;
				}
			}
			
			return true;
		}

		void EmitDefaultMethod (EmitContext ec, bool is_add)
		{
			ILGenerator ig = ec.ig;
			MethodInfo method = null;
			
			if (is_add)
				method = TypeManager.delegate_combine_delegate_delegate;
			else
				method = TypeManager.delegate_remove_delegate_delegate;

			if ((ModFlags & Modifiers.STATIC) != 0) {
				ig.Emit (OpCodes.Ldsfld, (FieldInfo) FieldBuilder);
				ig.Emit (OpCodes.Ldarg_0);
				ig.Emit (OpCodes.Call, method);
				ig.Emit (OpCodes.Castclass, MemberType);
				ig.Emit (OpCodes.Stsfld, (FieldInfo) FieldBuilder);
			} else {
				ig.Emit (OpCodes.Ldarg_0);
				ig.Emit (OpCodes.Ldarg_0);
				ig.Emit (OpCodes.Ldfld, (FieldInfo) FieldBuilder);
				ig.Emit (OpCodes.Ldarg_1);
				ig.Emit (OpCodes.Call, method);
				ig.Emit (OpCodes.Castclass, MemberType);
				ig.Emit (OpCodes.Stfld, (FieldInfo) FieldBuilder);
			}
			ig.Emit (OpCodes.Ret);
		}

		public void Emit (TypeContainer tc)
		{
			EmitContext ec;

			ec = new EmitContext (tc, Location, null, MemberType, ModFlags);
			Attribute.ApplyAttributes (ec, EventBuilder, this, OptAttributes);

			if (Add != null)
				AddData.Emit (tc, Add.Block, Add);
			else {
				ILGenerator ig = AddData.MethodBuilder.GetILGenerator ();
				ec = new EmitContext (tc, Location, ig, TypeManager.void_type, ModFlags);
				EmitDefaultMethod (ec, true);
			}

			if (Remove != null)
				RemoveData.Emit (tc, Remove.Block, Remove);
			else {
				ILGenerator ig = RemoveData.MethodBuilder.GetILGenerator ();
				ec = new EmitContext (tc, Location, ig, TypeManager.void_type, ModFlags);
				EmitDefaultMethod (ec, false);
			}
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

		public string IndexerName;
		public string InterfaceIndexerName;

		//
		// Are we implementing an interface ?
		//
		bool IsImplementing = false;
		
		public Indexer (Expression type, string int_type, int flags, Parameters parameters,
				Accessor get_block, Accessor set_block, Attributes attrs, Location loc)
			: base (type, "", flags, AllowedModifiers, parameters, get_block, set_block,
				attrs, loc)
		{
			ExplicitInterfaceName = int_type;
		}

		public override bool Define (TypeContainer parent)
		{
			PropertyAttributes prop_attr =
				PropertyAttributes.RTSpecialName |
				PropertyAttributes.SpecialName;
			
			if (!DoDefine (parent))
				return false;

			IndexerName = Attribute.ScanForIndexerName (ec, OptAttributes);
			if (IndexerName == null)
				IndexerName = "Item";
			else if (IsExplicitImpl)
				Report.Error (592, Location,
					      "Attribute 'IndexerName' is not valid on this declaration " +
					      "type. It is valid on `property' declarations only.");

			ShortName = IndexerName;
			if (IsExplicitImpl) {
				InterfaceIndexerName = TypeManager.IndexerPropertyName (InterfaceType);
				Name = InterfaceType.FullName + "." + IndexerName;
			} else {
				InterfaceIndexerName = IndexerName;
				Name = ShortName;
			}

			if (!CheckBase (parent))
				return false;

			flags |= MethodAttributes.HideBySig | MethodAttributes.SpecialName;
			if (Get != null){
                                InternalParameters ip = new InternalParameters (parent, Parameters);

				GetData = new MethodData (this, "get", MemberType,
							  ParameterTypes, ip, CallingConventions.Standard,
							  Get.OptAttributes, ModFlags, flags, false);

				if (!GetData.Define (parent))
					return false;

				GetBuilder = GetData.MethodBuilder;
			}
			
			if (Set != null){
				int top = ParameterTypes.Length;
				Type [] set_pars = new Type [top + 1];
				ParameterTypes.CopyTo (set_pars, 0);
				set_pars [top] = MemberType;

				Parameter [] fixed_parms = Parameters.FixedParameters;

				if (fixed_parms == null){
					throw new Exception ("We currently do not support only array arguments in an indexer");
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
					Type, "value", Parameter.Modifier.NONE, null);

				Parameters set_formal_params = new Parameters (tmp, null, Location);
				
				InternalParameters ip = new InternalParameters (parent, set_formal_params);

				SetData = new MethodData (this, "set", TypeManager.void_type,
							  set_pars, ip, CallingConventions.Standard,
							  Set.OptAttributes, ModFlags, flags, false);

				if (!SetData.Define (parent))
					return false;

				SetBuilder = SetData.MethodBuilder;
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

			if (GetData != null)
				IsImplementing = GetData.IsImplementing;
			else if (SetData != null)
				IsImplementing = SetData.IsImplementing;

			//
			// Define the PropertyBuilder if one of the following conditions are met:
			// a) we're not implementing an interface indexer.
			// b) the indexer has a different IndexerName and this is no
			//    explicit interface implementation.
			//
			if (!IsExplicitImpl) {
				PropertyBuilder = parent.TypeBuilder.DefineProperty (
					IndexerName, prop_attr, MemberType, ParameterTypes);

				if (GetData != null)
					PropertyBuilder.SetGetMethod (GetBuilder);

				if (SetData != null)
					PropertyBuilder.SetSetMethod (SetBuilder);
				
				TypeManager.RegisterIndexer (PropertyBuilder, GetBuilder, SetBuilder,
							     ParameterTypes);
			}

			return true;
		}
	}

	public class Operator : MemberCore {

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
		public readonly Block  Block;
		public Attributes      OptAttributes;
		public MethodBuilder   OperatorMethodBuilder;
		
		public string MethodName;
		public Method OperatorMethod;

		public Operator (OpType type, Expression ret_type, int flags,
				 Expression arg1type, string arg1name,
				 Expression arg2type, string arg2name,
				 Block block, Attributes attrs, Location loc)
			: base ("", loc)
		{
			OperatorType = type;
			ReturnType = ret_type;
			ModFlags = Modifiers.Check (AllowedModifiers, flags, Modifiers.PUBLIC, loc);
			FirstArgType = arg1type;
			FirstArgName = arg1name;
			SecondArgType = arg2type;
			SecondArgName = arg2name;
			Block = block;
			OptAttributes = attrs;
		}

		string Prototype (TypeContainer parent)
		{
			return parent.Name + ".operator " + OperatorType + " (" + FirstArgType + "," +
				SecondArgType + ")";
		}
		
		public override bool Define (TypeContainer parent)
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
					Prototype (parent) +
					"' must be declared static and public");
				return false;
			}

			param_list[0] = new Parameter (FirstArgType, FirstArgName,
						       Parameter.Modifier.NONE, null);
			if (SecondArgType != null)
				param_list[1] = new Parameter (SecondArgType, SecondArgName,
							       Parameter.Modifier.NONE, null);
			
			OperatorMethod = new Method (ReturnType, ModFlags, MethodName,
						     new Parameters (param_list, null, Location),
						     OptAttributes, Mono.CSharp.Location.Null);

			OperatorMethod.IsOperator = true;			
			OperatorMethod.Define (parent);

			if (OperatorMethod.MethodBuilder == null)
				return false;
			
			OperatorMethodBuilder = OperatorMethod.MethodBuilder;

			Type [] param_types = OperatorMethod.ParameterTypes;
			Type declaring_type = OperatorMethodBuilder.DeclaringType;
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
				
				if (first_arg_type.IsSubclassOf (return_type) ||
				    return_type.IsSubclassOf (first_arg_type)){
					Report.Error (
						-10, Location,
						"User-defined conversion cannot convert between types " +
						"that derive from each other");
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
		
		public void Emit (TypeContainer parent)
		{
			EmitContext ec = new EmitContext (parent, Location, null, null, ModFlags);
			Attribute.ApplyAttributes (ec, OperatorMethodBuilder, this, OptAttributes);

			//
			// abstract or extern methods have no bodies
			//
			if ((ModFlags & (Modifiers.ABSTRACT | Modifiers.EXTERN)) != 0)
				return;
			
			OperatorMethod.Block = Block;
			OperatorMethod.Emit (parent);
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
		public static MemberFilter method_signature_filter;

		/// <summary>
		///   This delegate is used to extract inheritable methods which
		///   have the same signature as the argument.  By inheritable,
		///   this means that we have permissions to override the method
		///   from the current assembly and class
		/// </summary>
		public static MemberFilter inheritable_method_signature_filter;

		static MethodSignature ()
		{
			method_signature_filter = new MemberFilter (MemberSignatureCompare);
			inheritable_method_signature_filter = new MemberFilter (
				InheritableMemberSignatureCompare);
		}
		
		public MethodSignature (string name, Type ret_type, Type [] parameters)
		{
			Name = name;
			RetType = ret_type;

			if (parameters == null)
				Parameters = TypeManager.NoTypes;
			else
				Parameters = parameters;
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

		//
		// This filter should be used when we are requesting methods that
		// we want to override.
		//
		// This makes a number of assumptions, for example
		// that the methods being extracted are of a parent
		// class (this means we know implicitly that we are
		// being called to find out about members by a derived
		// class).
		// 
		static bool InheritableMemberSignatureCompare (MemberInfo m, object filter_criteria)
		{
		        if (!MemberSignatureCompare (m, filter_criteria))
				return false;

			MethodInfo mi;
			PropertyInfo pi = m as PropertyInfo;

			if (pi != null) {
				mi = pi.GetGetMethod (true);
				if (mi == null)
					mi = pi.GetSetMethod (true);
			} else
				mi = m as MethodInfo;

			if (mi == null){
				Console.WriteLine ("Nothing found");
			}
			
			MethodAttributes prot = mi.Attributes & MethodAttributes.MemberAccessMask;

			// If only accessible to the current class.
			if (prot == MethodAttributes.Private)
				return false;

			// If only accessible to the defining assembly or 
			if (prot == MethodAttributes.FamANDAssem ||
			    prot == MethodAttributes.Assembly){
				if (m.DeclaringType.Assembly == CodeGen.AssemblyBuilder)
					return true;
				else
					return false;
			}

			// Anything else (FamOrAssembly and Public) is fine
			return true;
		}
	}
}
