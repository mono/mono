//
// class.cs: Class and Struct handlers
//
// Author: Miguel de Icaza (miguel@gnu.org)
//
// Licensed under the terms of the GNU GPL
//
// (C) 2001 Ximian, Inc (http://www.ximian.com)
//
//

using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using System;

namespace Mono.CSharp {

	/// <summary>
	///   This is the base class for structs and classes.  
	/// </summary>
	public class TypeContainer : DeclSpace {
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
		// This one is computed after we can distinguish interfaces
		// from classes from the arraylist `type_bases' 
		//
		string     base_class_name;

		ArrayList type_bases;

		// Attributes for this type
		protected Attributes attributes;

		// Information in the case we are an attribute type

		public AttributeTargets Targets;
		public bool AllowMultiple;
		public bool Inherited;
		

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
			string name = constant.Name;

			if ((res = IsValid (name)) != AdditionResult.Success)
				return res;
			
			if (constants == null)
				constants = new ArrayList ();

			constants.Add (constant);
			DefineName (name, constant);

			return AdditionResult.Success;
		}

		public AdditionResult AddEnum (Mono.CSharp.Enum e)
		{
			AdditionResult res;
			string name = e.Name;

			if ((res = IsValid (name)) != AdditionResult.Success)
				return res;

			if (enums == null)
				enums = new ArrayList ();

			enums.Add (e);
			DefineName (name, e);

			return AdditionResult.Success;
		}
		
		public AdditionResult AddClass (Class c)
		{
			AdditionResult res;
			string name = c.Name;


			if ((res = IsValid (name)) != AdditionResult.Success)
				return res;

			DefineName (name, c);
			types.Add (c);

			return AdditionResult.Success;
		}

		public AdditionResult AddStruct (Struct s)
		{
			AdditionResult res;
			string name = s.Name;
			
			if ((res = IsValid (name)) != AdditionResult.Success)
				return res;

			DefineName (name, s);
			types.Add (s);

			return AdditionResult.Success;
		}

		public AdditionResult AddDelegate (Delegate d)
		{
			AdditionResult res;
			string name = d.Name;

			if ((res = IsValid (name)) != AdditionResult.Success)
				return res;

			if (delegates == null)
				delegates = new ArrayList ();
			
			DefineName (name, d);
			delegates.Add (d);

			return AdditionResult.Success;
		}

		public AdditionResult AddMethod (Method method)
		{
			string name = method.Name;
			Object value = defined_names [name];
			
			if (value != null && (!(value is Method)))
				return AdditionResult.NameExists;

			if (methods == null)
				methods = new ArrayList ();

			methods.Add (method);
			if (value != null)
				DefineName (name, method);

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
			string name = iface.Name;

			if ((res = IsValid (name)) != AdditionResult.Success)
				return res;
			
			if (interfaces == null)
				interfaces = new ArrayList ();
			interfaces.Add (iface);
			DefineName (name, iface);
			
			return AdditionResult.Success;
		}

		public AdditionResult AddField (Field field)
		{
			AdditionResult res;
			string name = field.Name;
			
			if ((res = IsValid (name)) != AdditionResult.Success)
				return res;

			if (fields == null)
				fields = new ArrayList ();

			fields.Add (field);
			if (field.Initializer != null){
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
			
			DefineName (name, field);
			return AdditionResult.Success;
		}

		public AdditionResult AddProperty (Property prop)
		{
			AdditionResult res;
			string name = prop.Name;

			if ((res = IsValid (name)) != AdditionResult.Success)
				return res;

			if (properties == null)
				properties = new ArrayList ();

			properties.Add (prop);
			DefineName (name, prop);

			return AdditionResult.Success;
		}

		public AdditionResult AddEvent (Event e)
		{
			AdditionResult res;
			string name = e.Name;

			if ((res = IsValid (name)) != AdditionResult.Success)
				return res;

			if (events == null)
				events = new ArrayList ();
			
			events.Add (e);
			DefineName (name, e);

			return AdditionResult.Success;
		}

		public AdditionResult AddIndexer (Indexer i)
		{
			if (indexers == null)
				indexers = new ArrayList ();

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
		
		// 
		// root_types contains all the types.  All TopLevel types
		// hence have a parent that points to `root_types', that is
		// why there is a non-obvious test down here.
		//
		public bool IsTopLevel {
			get {
				if (Parent != null){
					if (Parent.Parent == null)
						return true;
				}
				return false;
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
		public bool EmitFieldInitializers (EmitContext ec, bool is_static)
		{
			ArrayList fields;
			ILGenerator ig = ec.ig;

			if (is_static)
				fields = initialized_static_fields;
			else
				fields = initialized_fields;

			if (fields == null)
				return true;
			
			foreach (Field f in fields){
				Object init = f.Initializer;

				Expression e;
				if (init is Expression)
					e = (Expression) init;
				else {
					string base_type = f.Type.Substring (0, f.Type.IndexOf ("["));
					string rank = f.Type.Substring (f.Type.IndexOf ("["));
					e = new ArrayCreation (base_type, rank, (ArrayList) init, f.Location); 
				}
				
				e = e.Resolve (ec);
				if (e == null)
					return false;
				
				if (!is_static)
					ig.Emit (OpCodes.Ldarg_0);
				
				e.Emit (ec);
				
				if (is_static)
					ig.Emit (OpCodes.Stsfld, f.FieldBuilder);
				else
					ig.Emit (OpCodes.Stfld, f.FieldBuilder);
				
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

			c = new Constructor (Basename, Parameters.GetEmptyReadOnlyParameters (),
					     new ConstructorBaseInitializer (null, new Location (-1)),
					     new Location (-1));
			
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

		struct TypeAndMethods {
			public Type          type;
			public MethodInfo [] methods;

			// Far from ideal, but we want to avoid creating a copy
			// of methods above.
			public Type [][]     args;

			//
			// This flag on the method says `We found a match, but
			// because it was private, we could not use the match
			//
			public bool []       found;
		}

		//
		// This array keeps track of the pending implementations
		// 
		TypeAndMethods [] pending_implementations;
		
		//
		// Registers the required method implementations for this class
		//
		// Register method implementations are either abstract methods
		// flagged as such on the base class or interface methods
		//
		public void RegisterRequiredImplementations ()
		{
			Type [] ifaces = TypeBuilder.GetInterfaces ();
			Type b = TypeBuilder.BaseType;
			int icount = 0;
			
			if (ifaces != null)
				icount = ifaces.Length;

			if (icount == 0)
				return;
			
			pending_implementations = new TypeAndMethods [icount + (b.IsAbstract ? 1 : 0)];
			
			int i = 0;
			if (ifaces != null){
				foreach (Type t in ifaces){
					MethodInfo [] mi;

					if (t is TypeBuilder){
						Interface iface;

						iface = RootContext.TypeManager.LookupInterface (t);
						
						mi = iface.GetMethods ();
					} else
						mi = t.GetMethods ();

					int count = mi.Length;
					pending_implementations [i].type = t;
					pending_implementations [i].methods = mi;
					pending_implementations [i].args = new Type [count][];
					pending_implementations [i].found = new bool [count];

					int j = 0;
					foreach (MethodInfo m in mi){
						Type [] types = TypeManager.GetArgumentTypes (m);

						pending_implementations [i].args [j] = types;
						j++;
					}
					i++;
				}
			}

			if (b.IsAbstract){
				MemberInfo [] abstract_methods;

				abstract_methods = FindMembers (
					TypeBuilder.BaseType,
					MemberTypes.Method, BindingFlags.Public | BindingFlags.Instance,
					abstract_method_filter, null);

				if (abstract_methods != null){
					int count = abstract_methods.Length;
					pending_implementations [i].methods = new MethodInfo [count];
					
					abstract_methods.CopyTo (pending_implementations [i].methods, 0);
					pending_implementations [i].found = new bool [count];
					pending_implementations [i].args = new Type [count][];
					pending_implementations [i].type = TypeBuilder;

					int j = 0;
					foreach (MemberInfo m in abstract_methods){
						MethodInfo mi = (MethodInfo) m;
						
						Type [] types = TypeManager.GetArgumentTypes (mi);

						pending_implementations [i].args [j] = types;
						j++;
					}
				}
			}
			
		}

		public static string MakeFQN (string nsn, string name)
		{
			string prefix = (nsn == "" ? "" : nsn + ".");

			return prefix + name;
		}
		       
		Type LookupInterfaceOrClass (object builder, string ns, string name, bool is_class, out bool error)
		{
			TypeContainer parent;
			Type t;

			error = false;
			name = MakeFQN (ns, name);

			t  = RootContext.TypeManager.LookupType (name);
			if (t != null)
				return t;

			if (is_class)
				parent = (Class) RootContext.Tree.Classes [name];
			else 
				parent = (Struct) RootContext.Tree.Structs [name];
			

			if (parent != null){
				t = parent.DefineType (builder);
				if (t == null){
					Report.Error (146, "Class definition is circular: `"+name+"'");
					error = true;
					return null;
				}

				return t;
			}

			return null;
		}
		
		//
		// returns the type for an interface or a class, this will recursively
		// try to define the types that it depends on.
		//
		Type GetInterfaceOrClass (object builder, string name, bool is_class)
		{
			Type t;
			bool error;

			//
			// Attempt to lookup the class on our namespace
			//
			t = LookupInterfaceOrClass (builder, Namespace.Name, name, is_class, out error);
			if (error)
				return null;
			
			if (t != null) 
				return t;

			//
			// Attempt to do a direct unqualified lookup
			//
			t = LookupInterfaceOrClass (builder, "", name, is_class, out error);
			if (error)
				return null;
			
			if (t != null)
				return t;
			
			//
			// Attempt to lookup the class on any of the `using'
			// namespaces
			//

			for (Namespace ns = Namespace; ns != null; ns = ns.Parent){
				ArrayList using_list = ns.UsingTable;
				
				if (using_list == null)
					continue;

				foreach (string n in using_list){
					t = LookupInterfaceOrClass (builder, n, name, is_class, out error);
					if (error)
						return null;

					if (t != null)
						return t;
				}
				
			}
			Report.Error (246, "Can not find type `"+name+"'");
			return null;
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
		Type [] GetClassBases (object builder, bool is_class, out Type parent, out bool error)
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
				string name = (string) bases [0];
				Type first = GetInterfaceOrClass (builder, name, is_class);

				if (first == null){
					error = true;
					return null;
				}
				
				if (first.IsClass){
					parent = first;
					start = 1;
				} else {
					parent = TypeManager.object_type;
					start = 0;
				}
			} else {
				start = 0;
			}

			Type [] ifaces = new Type [count-start];
			
			for (i = start, j = 0; i < count; i++, j++){
				string name = (string) bases [i];
				Type t = GetInterfaceOrClass (builder, name, is_class);
				
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
				
				if (t.IsSealed) {
					string detail = "";
					
					if (t.IsValueType)
						detail = " (a class can not inherit from a struct)";
							
					Report.Error (509, "class `"+ Name +
						      "': Cannot inherit from sealed class `"+
						      bases [i]+"'"+detail);
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
				
				ifaces [j] = t;
			}

			return ifaces;
		}
		
		//
		// Defines the type in the appropriate ModuleBuilder or TypeBuilder.
		//
		public TypeBuilder DefineType (object parent_builder)
		{
			Type parent;
			Type [] ifaces;
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

			ifaces = GetClassBases (parent_builder, is_class, out parent, out error); 
			
			if (error)
				return null;

			if (parent_builder is ModuleBuilder) {
				ModuleBuilder builder = (ModuleBuilder) parent_builder;
				//
				// Structs with no fields need to have a ".size 1"
				// appended
				//

				if (!is_class && Fields == null)
					TypeBuilder = builder.DefineType (Name,
									  TypeAttr,
									  parent, 
									  PackingSize.Unspecified, 1);
				else
				//
				// classes or structs with fields
				//
					TypeBuilder = builder.DefineType (Name,
									  TypeAttr,
									  parent,
									  ifaces);
			} else {
				TypeBuilder builder = (System.Reflection.Emit.TypeBuilder) parent_builder;
				
				//
				// Structs with no fields need to have a ".size 1"
				// appended
				//
				if (!is_class && Fields == null)
					TypeBuilder = builder.DefineNestedType (Basename,
										TypeAttr,
										parent, 
										PackingSize.Unspecified);
				else
				//
				// classes or structs with fields
				//
					TypeBuilder = builder.DefineNestedType (Basename,
										TypeAttr,
										parent,
										ifaces);
			}

			RootContext.TypeManager.AddUserType (Name, TypeBuilder, this);
			RootContext.RegisterOrder (this);
			
			if (Types != null) {
				foreach (TypeContainer tc in Types)
					tc.DefineType (TypeBuilder);
			}

			if (Delegates != null) {
				foreach (Delegate d in Delegates)
					d.DefineDelegate (TypeBuilder);
			}

			if (Enums != null) {
				foreach (Enum en in Enums)
					en.DefineEnum (TypeBuilder);
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
							Report109 (mc.Location, mc);
					}
					continue;
				}

				if (defined_names [idx] is PropertyInfo &&
				    ((mc.ModFlags & Modifiers.OVERRIDE) != 0)){
					continue;
				}
				    
#if WANT_TO_VERIFY_SIGNATURES_HERE
				if (defined_names [idx] is MethodBase && mc is MethodCore){
					MethodBase mb = (MethodBase) defined_names [idx];
					MethodCore met = (MethodCore) mc;
					
					if ((mb.IsVirtual || mb.IsAbstract) &&
					    (mc.ModFlags & Modifiers.OVERRIDE) != 0)
						continue;

					//
					// FIXME: Compare the signatures here.  If they differ,
					// then: `continue;' 
				}
#endif
				Report108 (mc.Location, defined_names [idx]);
			}
			
			foreach (object o in remove_list)
				list.Remove (o);
			
			remove_list.Clear ();
		}
		
		/// <summary>
		///   Populates our TypeBuilder with fields and methods
		/// </summary>
		public override bool Define (TypeContainer parent)
		{
			MemberInfo [] defined_names = null;

			if (RootContext.WarningLevel > 1){
				Type ptype;

				//
				// This code throws an exception in the comparer
				// I guess the string is not an object?
				//
				ptype = TypeBuilder.BaseType;
				if (ptype != null){
					defined_names = FindMembers (
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

			RegisterRequiredImplementations ();

			//
			// Constructors are not in the defined_names array
			//
			if (instance_constructors != null)
				DefineMembers (instance_constructors, null);
		
			if (default_static_constructor != null)
				default_static_constructor.Define (this);
			
			if (methods != null)
				DefineMembers (methods, null);

			if (properties != null)
				DefineMembers (properties, defined_names);

			if (events != null)
				DefineMembers (events, defined_names);

			if (indexers != null) {
				foreach (Indexer i in Indexers)
					i.Define (this);
			}

			if (operators != null)
				DefineMembers (operators, null);

			if (enums != null)
				DefineMembers (enums, defined_names);
			
			if (delegates != null)
				DefineMembers (delegates, defined_names);

			return true;
		}

		/// <summary>
		///   Looks up the alias for the name
		/// </summary>
		public string LookupAlias (string name)
		{
			if (Namespace != null)
				return Namespace.LookupAlias (name);
			else
				return null;
		}
		
		/// <summary>
		///   This function is based by a delegate to the FindMembers routine
		/// </summary>
		static bool AlwaysAccept (MemberInfo m, object filterCriteria)
		{
			return true;
		}
		
		static bool IsAbstractMethod (MemberInfo m, object filter_criteria)
		{
			MethodInfo mi = (MethodInfo) m;

			return mi.IsAbstract;
		}

		/// <summary>
		///   This filter is used by FindMembers, and we just keep
		///   a global for the filter to `AlwaysAccept'
		/// </summary>
		static MemberFilter accepting_filter;
		
		/// <summary>
		///    This delegate is a MemberFilter used to extract the 
		///    abstact methods from a type.  
		/// </summary>
		static MemberFilter abstract_method_filter;

		/// <summary>
		///   A member comparission method based on name only
		/// </summary>
		static IComparer mif_compare;

		static TypeContainer ()
		{
			abstract_method_filter = new MemberFilter (IsAbstractMethod);
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
		public MemberInfo [] FindMembers (MemberTypes mt, BindingFlags bf,
						  MemberFilter filter, object criteria)
		{
			ArrayList members = new ArrayList ();

			if (filter == null)
				filter = accepting_filter; 
			
			if ((mt & MemberTypes.Field) != 0) {
				if (Fields != null) {
					foreach (Field f in Fields) {
						FieldBuilder fb = f.FieldBuilder;
						if (filter (fb, criteria) == true)
							members.Add (fb);
					}
				}

				if (Constants != null) {
					foreach (Const con in Constants) {
						FieldBuilder fb = con.FieldBuilder;
						if (filter (fb, criteria) == true)
							members.Add (fb);
					}
				}
			}

			if ((mt & MemberTypes.Method) != 0) {
				if (Methods != null) {
					foreach (Method m in Methods) {
						MethodBuilder mb = m.MethodBuilder;

						// If we are in transit, ignore
						// This case arises when we are still defining a PInvoke method
						// and we hit FindMembers because of the need to resolve named
						// arguments inside of Attribute.DefinePInvokeMethod
						if (mb == null)
						        continue;

						if (filter (mb, criteria) == true)
							members.Add (mb);
					}
				}

				if (Operators != null){
					foreach (Operator o in Operators) {
						MethodBuilder ob = o.OperatorMethodBuilder;

						if (filter (ob, criteria) == true)
							members.Add (ob);
					}
				}
			}

			if ((mt & MemberTypes.Event) != 0) {
				if (Events != null)
				        foreach (Event e in Events) {
						if (filter (e.EventBuilder, criteria) == true)
						        members.Add (e.EventBuilder);
					}
			}
			
			if ((mt & MemberTypes.Property) != 0){
				if (Properties != null)
					foreach (Property p in Properties) {
						if (filter (p.PropertyBuilder, criteria) == true) {
							members.Add (p.PropertyBuilder);
						}
					}

				if (Indexers != null)
					foreach (Indexer ix in Indexers) {
						if (filter (ix.PropertyBuilder, criteria) == true) {
							members.Add (ix.PropertyBuilder);
						}
					}
			}
			
			if ((mt & MemberTypes.NestedType) != 0) {

				if (Types != null)
					foreach (TypeContainer t in Types)  
						if (filter (t.TypeBuilder, criteria) == true)
							members.Add (t.TypeBuilder);

				if (Enums != null)
					foreach (Enum en in Enums)
						if (filter (en.TypeBuilder, criteria) == true)
							members.Add (en.TypeBuilder);
				
			}

			if ((mt & MemberTypes.Constructor) != 0){
				if (instance_constructors != null){
					foreach (Constructor c in instance_constructors){
						ConstructorBuilder cb = c.ConstructorBuilder;

						if (filter (cb, criteria) == true)
							members.Add (cb);
					}
				}

				if (default_static_constructor != null){
					ConstructorBuilder cb =
						default_static_constructor.ConstructorBuilder;
					
					if (filter (cb, criteria) == true)
						members.Add (cb);
				}
			}

			//
			// Lookup members in parent if requested.
			//
			if ((bf & BindingFlags.DeclaredOnly) == 0){
				MemberInfo [] mi;

				mi = FindMembers (TypeBuilder.BaseType, mt, bf, filter, criteria);
				if (mi != null)
					members.AddRange (mi);
			}
			
			int count = members.Count;
			if (count > 0){
				MemberInfo [] mi = new MemberInfo [count];
				members.CopyTo (mi);
				return mi;
			}

			return null;
		}

		public MemberInfo GetFieldFromEvent (EventExpr event_expr)
		{
			EventInfo ei = event_expr.EventInfo;

			foreach (Event e in events) { 

				if (e.FieldBuilder == null)
					continue;
				
				if (Type.FilterName (e.FieldBuilder, ei.Name))
					return e.FieldBuilder;
			}

			return null;
		}

		public static MemberInfo [] FindMembers (Type t, MemberTypes mt, BindingFlags bf,
							 MemberFilter filter, object criteria)
		{
			TypeContainer tc = TypeManager.LookupTypeContainer (t);

			if (tc != null)
				return tc.FindMembers (mt, bf, filter, criteria);
			else
				return t.FindMembers (mt, bf, filter, criteria);
		}
		
		/// <summary>
		///   Whether the specified method is an interface method implementation
		/// </summary>
		///
		/// <remarks>
		///   If a method in Type `t' (or null to look in all interfaces
		///   and the base abstract class) with name `Name', return type `ret_type' and
		///   arguments `args' implements an interface, this method will
		///   return the MethodInfo that this method implements.
		///
		///   This will remove the method from the list of "pending" methods
		///   that are required to be implemented for this class as a side effect.
		/// 
		/// </remarks>
		public MethodInfo IsInterfaceMethod (Type t, string Name, Type ret_type, Type [] args,
						     bool clear)
		{
			if (pending_implementations == null)
				return null;

			foreach (TypeAndMethods tm in pending_implementations){
				if (!(t == null || tm.type == t))
					continue;

				int i = 0;
				foreach (MethodInfo m in tm.methods){
					if (m == null){
						i++;
						continue;
					}

					if (Name != m.Name){
						i++;
						continue;
					}

					if (ret_type != m.ReturnType){
						i++;
						continue;
					}

					if (args == null){
						if (tm.args [i] == null || tm.args [i].Length == 0){
							if (clear)
								tm.methods [i] = null;
							tm.found [i] = true;
							return m;
						} 
						i++;
						continue;
					}

					if (tm.args == null){
						Console.WriteLine ("Type:    " + tm.type);
						Console.WriteLine ("method:  " + tm.methods [i]);
					}
					
					if (tm.args [i] == null){
						i++;
						continue;
					}

					//
					// Check if we have the same parameters
					//
					if (tm.args [i].Length != args.Length){
						i++;
						continue;
					}
					
					int j, top = args.Length;
					bool fail = false;
					
					for (j = 0; j < top; j++){
						if (tm.args [i][j] != args[j]){
							fail = true;
							break;
						}
					}
					if (fail){
						i++;
						continue;
					}

					if (clear)
						tm.methods [i] = null;
					tm.found [i] = true;
					return m;
				}

				// If a specific type was requested, we can stop now.
				if (tm.type == t)
					return null;
			}
			return null;
		}

		/// <summary>
		///   Verifies that any pending abstract methods or interface methods
		///   were implemented.
		/// </summary>
		bool VerifyPendingMethods ()
		{
			int top = pending_implementations.Length;
			bool errors = false;
			int i;
			
			for (i = 0; i < top; i++){
				Type type = pending_implementations [i].type;
				int j = 0;
				
				foreach (MethodInfo mi in pending_implementations [i].methods){
					if (mi == null)
						continue;

					if (type.IsInterface){
						string extra = "";
						
						if (pending_implementations [i].found [j])
							extra = ".  (method might be private or static)";
						Report.Error (
							536, Location,
							"`" + Name + "' does not implement " +
							"interface member `" +
							type.FullName + "." + mi.Name + "'" + extra);
					} else {
						Report.Error (
							534, Location,
							"`" + Name + "' does not implement " +
							"inherited abstract member `" +
							type.FullName + "." + mi.Name + "'");
					}
					errors = true;
					j++;
				}
			}
			return errors;
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

			if (indexers != null) {
				foreach (Indexer ix in indexers)
					ix.Emit (this);

				CustomAttributeBuilder cb = Interface.EmitDefaultMemberAttr (this, ModFlags, Location);

				TypeBuilder.SetCustomAttribute (cb);
			}
			
			if (fields != null)
				foreach (Field f in fields)
					f.Emit (this);

			if (events != null){
				foreach (Event e in Events)
					e.Emit (this);
			}

			if (pending_implementations != null)
				if (!VerifyPendingMethods ())
					return;

			EmitContext ec = new EmitContext (
						  this, Mono.CSharp.Location.Null, null, null,
						  ModFlags, false);

			Attribute.ApplyAttributes (ec, TypeBuilder, this, OptAttributes, Location);
			
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
			}
			
			if (Enums != null)
				foreach (Enum en in Enums)
					en.CloseType ();
			
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
					d.CloseDelegate ();
		}

		public string MakeName (string n)
		{
			return "`" + Name + "." + n + "'";
		}

		public void Report108 (Location l, MemberInfo mi)
		{
			Report.Warning (
				108, l, "The keyword new is required on " + 
				MakeName (mi.Name) + " because it hides `" +
				mi.ReflectedType.Name + "." + mi.Name + "'");
		}

		public void Report109 (Location l, MemberCore mc)
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
			const int nv = (Modifiers.NEW | Modifiers.VIRTUAL);
			bool ok = true;
			string name = MakeName (n);
			
			//
			// At most one of static, virtual or override
			//
			if ((flags & Modifiers.STATIC) != 0){
				if ((flags & vao) != 0){
					Report.Error (
						112, loc, "static method " + name + "can not be marked " +
						"as virtual, abstract or override");
					ok = false;
				}
			}

			if ((flags & Modifiers.OVERRIDE) != 0 && (flags & nv) != 0){
				Report.Error (
					113, loc, name +
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
						180, loc, name + " can not be both abstract and extern");
					ok = false;
				}

				if ((flags & Modifiers.VIRTUAL) != 0){
					Report.Error (
						503, loc, name + " can not be both abstract and virtual");
					ok = false;
				}

				if ((ModFlags & Modifiers.ABSTRACT) == 0){
					Report.Error (
						513, loc, name +
						" is abstract but its container class is not");
					ok = false;

				}
			}

			if ((flags & Modifiers.PRIVATE) != 0){
				if ((flags & vao) != 0){
					Report.Error (
						621, loc, name +
						" virtual or abstract members can not be private");
					ok = false;
				}
			}

			if ((flags & Modifiers.SEALED) != 0){
				if ((flags & Modifiers.OVERRIDE) == 0){
					Report.Error (
						238, loc, name +
						" cannot be sealed because it is not an override");
					ok = false;
				}
			}

			return ok;
		}

		//
		// Returns true if `type' is as accessible as the flags `flags'
		// given for this member
		//
		static public bool AsAccessible (Type type, int flags)
		{
			// FIXME: Implement me
			return true;
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
			Modifiers.SEALED;

		public Class (TypeContainer parent, string name, int mod, Attributes attrs, Location l)
			: base (parent, name, l)
		{
			int accmods;

			if (parent.Parent == null)
				accmods = Modifiers.INTERNAL;
			else
				accmods = Modifiers.PRIVATE;

			this.ModFlags = Modifiers.Check (AllowedModifiers, mod, accmods);
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
			Modifiers.NEW |
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.PRIVATE;

		public Struct (TypeContainer parent, string name, int mod, Attributes attrs, Location l)
			: base (parent, name, l)
		{
			int accmods;
			
			if (parent.Parent == null)
				accmods = Modifiers.INTERNAL;
			else
				accmods = Modifiers.PRIVATE;
			
			this.ModFlags = Modifiers.Check (AllowedModifiers, mod, accmods);

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

	public abstract class MethodCore : MemberCore {
		public readonly Parameters Parameters;
		Block block;
		
		//
		// Parameters, cached for semantic analysis.
		//
		InternalParameters parameter_info;
		
		public MethodCore (string name, Parameters parameters, Location l)
			: base (name, l)
		{
			Name = name;
			Parameters = parameters;
		}
		
		//
		//  Returns the System.Type array for the parameters of this method
		//
		Type [] parameter_types;
		static Type [] no_types = new Type [0];
		public Type [] ParameterTypes (TypeContainer parent)
		{
			if (Parameters == null)
				return no_types;
			
			if (parameter_types == null)
				parameter_types = Parameters.GetParameterInfo (parent);

			return parameter_types;
		}

		public InternalParameters ParameterInfo
		{
			get {
				return parameter_info;
			}

			set {
				parameter_info = value;
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
	}
	
	public class Method : MethodCore {
		public readonly string ReturnType;
		public MethodBuilder MethodBuilder;
		public readonly Attributes OptAttributes;

		MethodAttributes flags;

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
			Modifiers.EXTERN;

		//
		// return_type can be "null" for VOID values.
		//
		public Method (string return_type, int mod, string name, Parameters parameters,
			       Attributes attrs, Location l)
			: base (name, parameters, l)
		{
			ReturnType = return_type;
			ModFlags = Modifiers.Check (AllowedModifiers, mod, Modifiers.PRIVATE);
			OptAttributes = attrs;
		}

		static bool MemberSignatureCompare (MemberInfo m, object filter_criteria)
		{
			MethodInfo mi;

			if (! (m is MethodInfo))
				return false;

			MethodSignature sig = (MethodSignature) filter_criteria;

			if (m.Name != sig.Name)
				return false;
			
			mi = (MethodInfo) m;

			if (mi.ReturnType != sig.RetType)
				return false;

			Type [] args = TypeManager.GetArgumentTypes (mi);
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
		
		/// <summary>
		///    This delegate is used to extract methods which have the
		///    same signature as the argument
		/// </summary>
		static MemberFilter method_signature_filter;
		
		static Method ()
		{
			method_signature_filter = new MemberFilter (MemberSignatureCompare);
			return;
		}
		
		//
		// Returns the `System.Type' for the ReturnType of this
		// function.  Provides a nice cache.  (used between semantic analysis
		// and actual code generation
		//
		Type type_return_type;
		
		public Type GetReturnType (TypeContainer parent)
		{
			if (type_return_type == null)
				type_return_type = RootContext.LookupType (
					parent, ReturnType, false, Location);
			
			return type_return_type;
		}

		void DuplicatEntryPoint (MethodInfo b)
		{
			Report.Error (
				17, Location,
				"Program `" + RootContext.CodeGen.FileName +
				"'  has more than one entry point defined: `" +
				b.DeclaringType.Name + "." + b.Name + "'");
		}
		
		//
		// Creates the type
		//
		public override bool Define (TypeContainer parent)
		{
			Type ret_type = GetReturnType (parent);
			Type [] parameters = ParameterTypes (parent);
			bool error = false;
			MethodInfo implementing;
			Type iface_type = null;
			string iface = "", short_name;

			// Check if the return type and arguments were correct
			if (ret_type == null || parameters == null)
				return false;
			
			if (!parent.MethodModifiersValid (ModFlags, Name, Location))
				return false;

			//
			// verify accessibility
			//
			if (!TypeContainer.AsAccessible (ret_type, ModFlags))
				return false;

			foreach (Type partype in parameters)
				if (!TypeContainer.AsAccessible (partype, ModFlags))
					error = true;

			if (error)
				return false;

			//
			// Verify if the parent has a type with the same name, and then
			// check whether we have to create a new slot for it or not.
			//
			Type ptype = parent.TypeBuilder.BaseType;

			// ptype is only null for System.Object while compiling corlib.
			if (ptype != null){
				MethodSignature ms = new MethodSignature (Name, ret_type, parameters);
				MemberInfo [] mi, mi_static, mi_instance;

				mi_static = TypeContainer.FindMembers (
					ptype, MemberTypes.Method,
					BindingFlags.Public | BindingFlags.Static, method_signature_filter,
					ms);

				mi_instance = TypeContainer.FindMembers (
					ptype, MemberTypes.Method,
					BindingFlags.Public | BindingFlags.Instance, method_signature_filter,
					ms);

				if (mi_instance != null && mi_instance.Length > 0){
					mi = mi_instance;
				} else if (mi_static != null && mi_static.Length > 0)
					mi = mi_static;
				else
					mi = null;
				
				if (mi != null && mi.Length > 0){
					if (!CheckMethodAgainstBase (parent, (MethodInfo) mi [0])){
						return false;
					}
				} else {
					if ((ModFlags & Modifiers.NEW) != 0)
						WarningNotHiding (parent);
					
					if ((ModFlags & Modifiers.OVERRIDE) != 0)
						Report.Error (115, Location,
							      parent.MakeName (Name) +
							      " no suitable methods found to override");
				}
			} else if ((ModFlags & Modifiers.NEW) != 0)
				WarningNotHiding (parent);

			//
			// If we implement an interface, extract the interface name.
			//
			flags = Modifiers.MethodAttr (ModFlags);

			if (Name.IndexOf (".") != -1){
				int pos = Name.LastIndexOf (".");
				iface = Name.Substring (0, pos);

				iface_type = RootContext.LookupType (parent, iface, false, Location);
				short_name = Name.Substring (pos + 1);

				if (iface_type == null)
					return false;

				// Compute the full name that we need to export
				Name = iface_type.FullName + "." + short_name;
			} else
				short_name = Name;

			//
			// Check if we are an implementation of an interface method or
			// a method
			//
			implementing = parent.IsInterfaceMethod (
				iface_type, short_name, ret_type, parameters, false);
				
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
				if (iface_type == null){
					if ((ModFlags & Modifiers.PUBLIC) == 0)
						implementing = null;

					if ((ModFlags & Modifiers.STATIC) != 0)
						implementing = null;
				} else {
					if ((ModFlags & (Modifiers.PUBLIC | Modifiers.ABSTRACT)) != 0){
						Report.Error (
							106, Location, "`public' or `abstract' modifiers "+
							"are not allowed in explicit interface declarations"
							);
						implementing = null;
					}
				}
			}
			
			//
			// If implementing is still valid, set flags
			//
			if (implementing != null){
				flags |=
					MethodAttributes.Virtual |
					MethodAttributes.NewSlot |
					MethodAttributes.HideBySig;

				// If not abstract, then we can set Final.
				if (((flags & MethodAttributes.Abstract) == 0) &&
				    implementing.DeclaringType.IsInterface)
					flags |= MethodAttributes.Final;

				//
				// clear the flag
				//
				parent.IsInterfaceMethod (
					iface_type, short_name, ret_type, parameters, true);
			} 

			Attribute dllimport_attr = null;
			if (OptAttributes != null && OptAttributes.AttributeSections != null) {
				foreach (AttributeSection asec in OptAttributes.AttributeSections) {
				 	if (asec.Attributes == null)
						continue;
					
					foreach (Attribute a in asec.Attributes)
						if (a.Name.IndexOf ("DllImport") != -1) {
							flags |= MethodAttributes.PinvokeImpl;
							dllimport_attr = a;
						}
				}
			}

			//
			// Finally, define the method
			//

			if ((flags & MethodAttributes.PinvokeImpl) != 0) {
				EmitContext ec = new EmitContext (
					parent, Location, null, GetReturnType (parent), ModFlags);
				
				MethodBuilder = dllimport_attr.DefinePInvokeMethod (
					ec, parent.TypeBuilder,
					Name, flags, ret_type, parameters);
			} else {
				MethodBuilder = parent.TypeBuilder.DefineMethod (
					Name, flags,
					GetCallingConvention (parent is Class),
					ret_type, parameters);

				if (implementing != null){
					parent.TypeBuilder.DefineMethodOverride (
						MethodBuilder, implementing);
				}
			}

			if (MethodBuilder == null)
				return false;

			//
			// HACK because System.Reflection.Emit is lame
			//
			ParameterInfo = new InternalParameters (parent, Parameters);

			if (!TypeManager.RegisterMethod (MethodBuilder, ParameterInfo,
							 parameters)) {
				Report.Error (
					111, Location,
					"Class `" + parent.Name + "' already contains a definition with " +
					" the same return value and parameter types for method `" +
					Name + "'");
				return false;
			}
			
			//
			// This is used to track the Entry Point,
			//
			// FIXME: Allow pluggable entry point, check arguments, etc.
			//
			if (Name == "Main" &&
			    ((ModFlags & Modifiers.STATIC) != 0) && 
			    (RootContext.MainClass == null ||
			     RootContext.MainClass == parent.TypeBuilder.FullName)){
				if (RootContext.EntryPoint != null){
					DuplicatEntryPoint (MethodBuilder);
					DuplicatEntryPoint (RootContext.EntryPoint);
				} else 
					RootContext.EntryPoint = MethodBuilder;
				
				//
				// FIXME: Verify that the method signature
				// is valid for an entry point, and report
				// error 28 if not.
				//
			}
			
			//
			// Define each type attribute (in/out/ref) and
			// the argument names.
			//
			Parameter [] p = Parameters.FixedParameters;
			if (p != null){
				int i;
				
				for (i = 0; i < p.Length; i++) 
					MethodBuilder.DefineParameter (
						      i + 1, p [i].Attributes, p [i].Name);
					
				if (i != parameters.Length) {
					ParameterBuilder pb;
					
					Parameter array_param = Parameters.ArrayParameter;
					pb = MethodBuilder.DefineParameter (
						i + 1, array_param.Attributes,
						array_param.Name);

					CustomAttributeBuilder a = new CustomAttributeBuilder (
						TypeManager.cons_param_array_attribute, new object [0]);

					pb.SetCustomAttribute (a);
				}
			}

			return true;
		}

		//
		// Emits the code
		// 
		public void Emit (TypeContainer parent)
		{
			if ((flags & MethodAttributes.PinvokeImpl) != 0)
				return;

			ILGenerator ig = MethodBuilder.GetILGenerator ();
			EmitContext ec = new EmitContext (parent, Location, ig,
							  GetReturnType (parent), ModFlags);

			Attribute.ApplyAttributes (ec, MethodBuilder, this, OptAttributes, Location);
			
			
			//
			// abstract or extern methods have no bodies
			//
			if ((ModFlags & (Modifiers.ABSTRACT | Modifiers.EXTERN)) != 0)
				return;

			//
			// Handle destructors specially
			//
			// FIXME: This code generates buggy code
			//
			if (Name == "Finalize" && type_return_type == TypeManager.void_type)
				EmitDestructor (ec);
			else
				ec.EmitTopBlock (Block, Location);
		}

		void EmitDestructor (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			
			Label finish = ig.DefineLabel ();
			bool old_in_try = ec.InTry;
			Expression member_lookup;
			
			ig.BeginExceptionBlock ();
			ec.InTry = true;
			ec.ReturnLabel = finish;
			ec.EmitTopBlock (Block, Location);
			ec.InTry = old_in_try;
			
			ig.MarkLabel (finish);
			bool old_in_finally = ec.InFinally;
			ec.InFinally = true;
			ig.BeginFinallyBlock ();
			
			member_lookup = Expression.MemberLookup (
				ec, ec.TypeContainer.TypeBuilder.BaseType, "Finalize",
				MemberTypes.Method, Expression.AllBindingFlags, Location);

			if (member_lookup != null){
				MethodGroupExpr parent_destructor = ((MethodGroupExpr) member_lookup);
				
				ig.Emit (OpCodes.Ldarg_0);
				ig.Emit (OpCodes.Call, (MethodInfo) parent_destructor.Methods [0]);
			}
			ec.InFinally = old_in_finally;
			
			ig.EndExceptionBlock ();
			//ig.MarkLabel (ec.ReturnLabel);
			ig.Emit (OpCodes.Ret);
		}
	}

	public abstract class ConstructorInitializer {
		ArrayList argument_list;
		ConstructorInfo parent_constructor;
		Location location;
		
		public ConstructorInitializer (ArrayList argument_list, Location location)
		{
			this.argument_list = argument_list;
			this.location = location;
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
			
			if (argument_list != null){
				for (int i = argument_list.Count; i > 0; ){
					--i;

					Argument a = (Argument) argument_list [i];
					if (!a.Resolve (ec, location))
						return false;
				}
			}

			if (this is ConstructorBaseInitializer)
				t = ec.TypeContainer.TypeBuilder.BaseType;
			else
				t = ec.TypeContainer.TypeBuilder;
			
			parent_constructor_group = Expression.MemberLookup (
				ec, t, ".ctor", 
				MemberTypes.Constructor,
				BindingFlags.Public | BindingFlags.Instance, location);
			
			if (parent_constructor_group == null){
				Console.WriteLine ("Could not find a constructor in our parent");
				return false;
			}
			
			parent_constructor = (ConstructorInfo) Invocation.OverloadResolve (ec, 
				(MethodGroupExpr) parent_constructor_group, argument_list, location);
			
			if (parent_constructor == null){
				Console.WriteLine ("Could not locate a proper overload function");
				return false;
			}
			
			return true;
		}

		public void Emit (EmitContext ec)
		{
			ec.ig.Emit (OpCodes.Ldarg_0);
			if (argument_list != null)
				Invocation.EmitArguments (ec, null, argument_list);
			ec.ig.Emit (OpCodes.Call, parent_constructor);
		}
	}

	public class ConstructorBaseInitializer : ConstructorInitializer {
		public ConstructorBaseInitializer (ArrayList argument_list, Location l) : base (argument_list, l)
		{
		}
	}

	public class ConstructorThisInitializer : ConstructorInitializer {
		public ConstructorThisInitializer (ArrayList argument_list, Location l) : base (argument_list, l)
		{
		}
	}
	
	public class Constructor : MethodCore {
		public ConstructorBuilder ConstructorBuilder;
		public ConstructorInitializer Initializer;
		public Attributes OptAttributes;

		// <summary>
		//   Modifiers allowed for a constructor.
		// </summary>
		const int AllowedModifiers =
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.STATIC |
			Modifiers.PRIVATE;

		//
		// The spec claims that static is not permitted, but
		// my very own code has static constructors.
		//
		public Constructor (string name, Parameters args, ConstructorInitializer init, Location l)
			: base (name, args, l)
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

			Type [] parameters = ParameterTypes (parent);

			if ((ModFlags & Modifiers.STATIC) != 0)
				ca |= MethodAttributes.Static;
			else {
				if (parent is Struct && parameters.Length == 0){
					Report.Error (
						568, Location, 
						"Structs can not contain explicit parameterless " +
						"constructors");
					return false;
				}
				ca |= MethodAttributes.Public | MethodAttributes.HideBySig;
			}

			foreach (Type partype in parameters)
				if (!TypeContainer.AsAccessible (partype, ModFlags))
					return false;

			ConstructorBuilder = parent.TypeBuilder.DefineConstructor (
				ca, GetCallingConvention (parent is Class), parameters);

			//
			// HACK because System.Reflection.Emit is lame
			//
			ParameterInfo = new InternalParameters (parent, Parameters);

			if (!TypeManager.RegisterMethod (ConstructorBuilder, ParameterInfo, parameters)) {
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

			if (parent is Class && ((ModFlags & Modifiers.STATIC) == 0)){
				if (Initializer == null)
					Initializer = new ConstructorBaseInitializer (null, parent.Location);
				
				if (!Initializer.Resolve (ec)){
					Console.WriteLine ("Could not resolve initializer: " + parent.Name);
				}
			}

			//
			// Classes can have base initializers and instance field initializers.
			//
			if (parent is Class){
				if ((ModFlags & Modifiers.STATIC) == 0){
					Initializer.Emit (ec);
					parent.EmitFieldInitializers (ec, false);
				}
			}
			
			if ((ModFlags & Modifiers.STATIC) != 0)
				parent.EmitFieldInitializers (ec, true);

			Attribute.ApplyAttributes (ec, ConstructorBuilder, this, OptAttributes, Location);

			ec.EmitTopBlock (Block, Location);
		}
	}
	
	public class Field : MemberCore {
		public readonly string Type;
		public readonly Object Initializer;
		public readonly Attributes OptAttributes;
		public FieldBuilder  FieldBuilder;
		
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
			Modifiers.READONLY;

		public Field (string type, int mod, string name, Object expr_or_array_init,
			      Attributes attrs, Location loc)
			: base (name, loc)
		{
			Type = type;
			ModFlags = Modifiers.Check (AllowedModifiers, mod, Modifiers.PRIVATE);
			Initializer = expr_or_array_init;
			OptAttributes = attrs;
		}

		public override bool Define (TypeContainer parent)
		{
			Type t = RootContext.LookupType (parent, Type, false, Location);

			if (t == null)
				return false;

			if (!TypeContainer.AsAccessible (t, ModFlags))
				return false;

			if (RootContext.WarningLevel > 1){
				Type ptype = parent.TypeBuilder.BaseType;

				// ptype is only null for System.Object while compiling corlib.
				if (ptype != null){
					MemberInfo [] mi;
					
					mi = TypeContainer.FindMembers (
						ptype, MemberTypes.Method,
						BindingFlags.Public |
						BindingFlags.Static | BindingFlags.Instance,
						System.Type.FilterName, Name);
				}
			}

			if ((ModFlags & Modifiers.VOLATILE) != 0){
				if (!t.IsClass){
					if (TypeManager.IsEnumType (t))
						t = TypeManager.EnumToUnderlying (t);

					if (!((t == TypeManager.bool_type) ||
					      (t == TypeManager.sbyte_type) ||
					      (t == TypeManager.byte_type) ||
					      (t == TypeManager.short_type) ||    
					      (t == TypeManager.ushort_type) ||
					      (t == TypeManager.int32_type) ||    
					      (t == TypeManager.uint32_type) ||    
					      (t == TypeManager.char_type) ||    
					      (t == TypeManager.float_type))){
						Report.Error (
							677, Location, parent.MakeName (Name) +
							" A volatile field can not be of type `" +
							TypeManager.CSharpName (t) + "'");
						return false;
					}
				}
			}
			
			FieldBuilder = parent.TypeBuilder.DefineField (
				Name, t, Modifiers.FieldAttr (ModFlags));

			TypeManager.RegisterField (FieldBuilder, this);
			return true;
		}

		public void Emit (TypeContainer tc)
		{
			EmitContext ec = new EmitContext (tc, Location, null, FieldBuilder.FieldType, ModFlags);

			Attribute.ApplyAttributes (ec, FieldBuilder, this, OptAttributes, Location); 
		}
	}

	public class Property : MemberCore {
		public readonly string Type;
		public Block           Get, Set;
		public PropertyBuilder PropertyBuilder;
		public Attributes OptAttributes;
		MethodBuilder GetBuilder, SetBuilder;

		//
		// The type, once we compute it.
		
		Type PropertyType;

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
			Modifiers.VIRTUAL;

		public Property (string type, string name, int mod_flags, Block get_block, Block set_block,
				 Attributes attrs, Location loc)
			: base (name, loc)
		{
			Type = type;
			ModFlags = Modifiers.Check (AllowedModifiers, mod_flags, Modifiers.PRIVATE);
			Get = get_block;
			Set = set_block;
			OptAttributes = attrs;
		}

		//
		// Checks our base implementation if any
		//
		bool CheckBase (TypeContainer parent)
		{
			//
			// Find properties with the same name on the base class
			//
			MemberInfo [] props;
			MemberInfo [] props_static = TypeContainer.FindMembers (
				parent.TypeBuilder.BaseType,
				MemberTypes.All, BindingFlags.Public | BindingFlags.Static,
				System.Type.FilterName, Name);

			MemberInfo [] props_instance = TypeContainer.FindMembers (
				parent.TypeBuilder.BaseType,
				MemberTypes.All, BindingFlags.Public | BindingFlags.Instance,
				System.Type.FilterName, Name);

			//
			// Find if we have anything
			//
			if (props_static != null && props_static.Length > 0)
				props = props_static;
			else if (props_instance != null && props_instance.Length > 0)
				props = props_instance;
			else
				props = null;

			//
			// If we have something on the base.
			if (props != null && props.Length > 0){
				//
				// FIXME:
				// Currently we expect only to get 1 match at most from our
				// base class, maybe we can get more than one, investigate
				// whether this is possible
				//
				if (props.Length > 1)
					throw new Exception ("How do we handle this?");
				
				PropertyInfo pi = (PropertyInfo) props [0];

				MethodInfo inherited_get = TypeManager.GetPropertyGetter (pi);
				MethodInfo inherited_set = TypeManager.GetPropertySetter (pi);

				MethodInfo reference = inherited_get == null ?
					inherited_set : inherited_get;
				
				if (!CheckMethodAgainstBase (parent, reference))
					return false;
			} else {
				if ((ModFlags & Modifiers.NEW) != 0)
					WarningNotHiding (parent);
				
				if ((ModFlags & Modifiers.OVERRIDE) != 0){
					Report.Error (115, Location,
						      parent.MakeName (Name) +
						      " no suitable methods found to override");
					return false;
				}
			}
			return true;
		}

		bool DefineMethod (TypeContainer parent, Type iface_type, string short_name, bool is_get)
		{
			MethodAttributes flags = Modifiers.MethodAttr (ModFlags);
			Type [] parameters = null;
			MethodInfo implementing;
			Type fn_type;
			string name;

			flags |= MethodAttributes.HideBySig |
				MethodAttributes.SpecialName;
			
			if (is_get){
				fn_type = PropertyType;
				name = "get_" + short_name;
			} else {
				name = "set_" + short_name;
				parameters = new Type [1];
				parameters [0] = PropertyType;
				fn_type = TypeManager.void_type;
			}

			implementing = parent.IsInterfaceMethod (
				iface_type, name, fn_type, parameters, false);

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
				if (iface_type == null){
					if ((ModFlags & Modifiers.PUBLIC) == 0)
						implementing = null;
					if ((ModFlags & Modifiers.STATIC) != 0)
						implementing = null;
				} else {
					if ((ModFlags & (Modifiers.PUBLIC | Modifiers.ABSTRACT)) != 0){
						Report.Error (
							106, Location, "`public' or `abstract' modifiers "+
							"are not allowed in explicit interface declarations"
							);
						implementing = null;
					}
				}
			}
			
			//
			// If implementing is still valid, set flags
			//
			if (implementing != null){
				flags |=
					MethodAttributes.Virtual |
					MethodAttributes.NewSlot |
					MethodAttributes.HideBySig;

				// If an interface implementation, then we can set Final.
				if (((flags & MethodAttributes.Abstract) == 0) &&
				    implementing.DeclaringType.IsInterface)
					flags |= MethodAttributes.Final;
				
				//
				// clear the pending flag
				//
				parent.IsInterfaceMethod (
					iface_type, name, fn_type, parameters, true);
			} 
			
			if (is_get){
				GetBuilder = parent.TypeBuilder.DefineMethod (
					name, flags, PropertyType, null);
				PropertyBuilder.SetGetMethod (GetBuilder);
			
				if (implementing != null)
					parent.TypeBuilder.DefineMethodOverride (
						GetBuilder, implementing);
				
				//
				// HACK because System.Reflection.Emit is lame
				//
				InternalParameters ip = new InternalParameters (
					parent, Parameters.GetEmptyReadOnlyParameters ());
				
				if (!TypeManager.RegisterMethod (GetBuilder, ip, null)) {
					Report.Error (111, Location,
						      "Class `" + parent.Name +
						      "' already contains a definition with the " +
						      "same return value and parameter types as the " +
						      "'get' method of property `" + Name + "'");
					return false;
				}
			} else {
				SetBuilder = parent.TypeBuilder.DefineMethod (
					name, flags, null, parameters);
				
				if (implementing != null)
					parent.TypeBuilder.DefineMethodOverride (
						SetBuilder, implementing);
				
				SetBuilder.DefineParameter (1, ParameterAttributes.None, "value"); 
				PropertyBuilder.SetSetMethod (SetBuilder);

				//
				// HACK because System.Reflection.Emit is lame
				//
				Parameter [] parms = new Parameter [1];
				parms [0] = new Parameter (Type, "value", Parameter.Modifier.NONE, null);
				InternalParameters ip = new InternalParameters (
					parent, new Parameters (parms, null, Location));

				if (!TypeManager.RegisterMethod (SetBuilder, ip, parameters)) {
					Report.Error (
						111, Location,
						"Class `" + parent.Name +
						"' already contains a definition with the " +
						"same return value and parameter types as the " +
						"'set' method of property `" + Name + "'");
					return false;
				}
			}

			return true;
		}

		public override bool Define (TypeContainer parent)
		{
			Type iface_type = null;
			string short_name;
			
			if (!parent.MethodModifiersValid (ModFlags, Name, Location))
				return false;

			// Lookup Type, verify validity
			PropertyType = RootContext.LookupType (parent, Type, false, Location);
			if (PropertyType == null)
				return false;

			// verify accessibility
			if (!TypeContainer.AsAccessible (PropertyType, ModFlags))
				return false;
			
			if (!CheckBase (parent))
				return false;

			//
			// Check for explicit interface implementation
			//
			if (Name.IndexOf (".") != -1){
				int pos = Name.LastIndexOf (".");
				string iface = Name.Substring (0, pos);

				iface_type = RootContext.LookupType (parent, iface, false, Location);
				if (iface_type == null)
					return false;

				short_name = Name.Substring (pos + 1);

				// Compute the full name that we need to export.
				Name = iface_type.FullName + "." + short_name;
			} else
				short_name = Name;

			// FIXME - PropertyAttributes.HasDefault ?

			PropertyAttributes prop_attr = PropertyAttributes.RTSpecialName |
				                       PropertyAttributes.SpecialName;
		
			PropertyBuilder = parent.TypeBuilder.DefineProperty (
				Name, prop_attr, PropertyType, null);

			if (Get != null)
				if (!DefineMethod (parent, iface_type, short_name, true))
					return false;
			
			if (Set != null)
				if (!DefineMethod (parent, iface_type, short_name, false))
					return false;
			
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

			return true;
		}
		
		public void Emit (TypeContainer tc)
		{
			ILGenerator ig;
			EmitContext ec;

			ec = new EmitContext (tc, Location, null, PropertyType, ModFlags);
			Attribute.ApplyAttributes (ec, PropertyBuilder, this, OptAttributes, Location);
			

			//
			// abstract or extern properties have no bodies
			//
			if ((ModFlags & (Modifiers.ABSTRACT | Modifiers.EXTERN)) != 0)
				return;

			if (Get != null){
				ig = GetBuilder.GetILGenerator ();
				ec = new EmitContext (tc, Location, ig, PropertyType, ModFlags);
				
				ec.EmitTopBlock (Get, Location);
			}

			if (Set != null){
				ig = SetBuilder.GetILGenerator ();
				ec = new EmitContext (tc, Location, ig, null, ModFlags);
				
				ec.EmitTopBlock (Set, Location);
			}
		}
	}


	/// </summary>
	///  Gigantic workaround  for lameness in SRE follows :
	///  This class derived from EventInfo and attempts to basically
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

		public MyEventBuilder (TypeBuilder type_builder, string name, EventAttributes event_attr, Type event_type)
		{
			MyBuilder = type_builder.DefineEvent (name, event_attr, event_type);

			// And now store the values in our own fields.
			
			declaring_type = type_builder;

			// FIXME : This is supposed to be MyBuilder but since that doesn't
			// derive from Type, I have no clue what to do with this.
			reflected_type = null;
			
			attributes = event_attr;
			this.name = name;
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
	}
	
	public class Event : MemberCore {
		
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
			Modifiers.ABSTRACT;

		public readonly string    Type;
		public readonly Object    Initializer;
		public readonly Block     Add;
		public readonly Block     Remove;
		public MyEventBuilder     EventBuilder;
		public FieldBuilder       FieldBuilder;
		public Attributes         OptAttributes;

		Type EventType;
		MethodBuilder AddBuilder, RemoveBuilder;
		

		public Event (string type, string name, Object init, int flags, Block add_block,
			      Block rem_block, Attributes attrs, Location loc)
			: base (name, loc)
		{
			Type = type;
			Initializer = init;
			ModFlags = Modifiers.Check (AllowedModifiers, flags, Modifiers.PRIVATE);  
			Add = add_block;
			Remove = rem_block;
			OptAttributes = attrs;
		}

		public override bool Define (TypeContainer parent)
		{
			if (!parent.MethodModifiersValid (ModFlags, Name, Location))
				return false;

			MethodAttributes m_attr = Modifiers.MethodAttr (ModFlags);
			EventAttributes e_attr = EventAttributes.RTSpecialName | EventAttributes.SpecialName;

			EventType = RootContext.LookupType (parent, Type, false, Location);
			if (EventType == null)
				return false;

			if (!TypeContainer.AsAccessible (EventType, ModFlags))
				return false;
			

			if (!EventType.IsSubclassOf (TypeManager.delegate_type)) {
				Report.Error (66, Location, "'" + parent.Name + "." + Name +
					      "' : event must be of a delegate type");
				return false;
			}
			
			Type [] parameters = new Type [1];
			parameters [0] = EventType;

			EventBuilder = new MyEventBuilder (parent.TypeBuilder, Name, e_attr, EventType);

			if (Add == null && Remove == null)
				FieldBuilder = parent.TypeBuilder.DefineField (Name, EventType, FieldAttributes.Private);
			
			//
			// Now define the accessors
			//
			
			AddBuilder = parent.TypeBuilder.DefineMethod (
							 "add_" + Name, m_attr, null, parameters);
			AddBuilder.DefineParameter (1, ParameterAttributes.None, "value");
			EventBuilder.SetAddOnMethod (AddBuilder);

			//
			// HACK because System.Reflection.Emit is lame
			//
			Parameter [] parms = new Parameter [1];
			parms [0] = new Parameter (Type, "value", Parameter.Modifier.NONE, null);
			InternalParameters ip = new InternalParameters (
						            parent, new Parameters (parms, null, Location)); 
			
			if (!TypeManager.RegisterMethod (AddBuilder, ip, parameters)) {
				Report.Error (111, Location,
					      "Class `" + parent.Name + "' already contains a definition with the " +
					      "same return value and parameter types for the " +
					      "'add' method of event `" + Name + "'");
				return false;
			}
		
			RemoveBuilder = parent.TypeBuilder.DefineMethod (
							    "remove_" + Name, m_attr, null, parameters);
			RemoveBuilder.DefineParameter (1, ParameterAttributes.None, "value");
			EventBuilder.SetRemoveOnMethod (RemoveBuilder);

			//
			// HACK because System.Reflection.Emit is lame
			//

			if (!TypeManager.RegisterMethod (RemoveBuilder, ip, parameters)) {
				Report.Error (111, Location,	
					      "Class `" + parent.Name + "' already contains a definition with the " +
					      "same return value and parameter types for the " +
					      "'remove' method of event `" + Name + "'");
				return false;
			}
			
			if (!TypeManager.RegisterEvent (EventBuilder, AddBuilder, RemoveBuilder)) {
				Report.Error (111, Location,
					"Class `" + parent.Name +
					"' already contains a definition for the event `" +
					Name + "'");
				return false;
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
			
			ig.Emit (OpCodes.Ldarg_0);
			ig.Emit (OpCodes.Ldarg_0);
			ig.Emit (OpCodes.Ldfld, (FieldInfo) FieldBuilder);
			ig.Emit (OpCodes.Ldarg_1);
			ig.Emit (OpCodes.Call, method);
			ig.Emit (OpCodes.Castclass, EventType);
			ig.Emit (OpCodes.Stfld, (FieldInfo) FieldBuilder);
			ig.Emit (OpCodes.Ret);
		}

		public void Emit (TypeContainer tc)
		{
			EmitContext ec;
			ILGenerator ig;

			ig = AddBuilder.GetILGenerator ();
			ec = new EmitContext (tc, Location, ig, TypeManager.void_type, ModFlags);

			if (Add != null)
				ec.EmitTopBlock (Add, Location);
			else
				EmitDefaultMethod (ec, true);

			ig = RemoveBuilder.GetILGenerator ();
			ec = new EmitContext (tc, Location, ig, TypeManager.void_type, ModFlags);
			
			if (Remove != null)
				ec.EmitTopBlock (Remove, Location);
			else
				EmitDefaultMethod (ec, false);

			ec = new EmitContext (tc, Location, null, EventType, ModFlags);
			Attribute.ApplyAttributes (ec, EventBuilder, this, OptAttributes, Location);
			
		}
		
	}

	//
	// FIXME: This does not handle:
	//
	//   int INTERFACENAME [ args ]
	//
	// Only:
	// 
	// int this [ args ]
 
	public class Indexer : MemberCore {

		const int AllowedModifiers =
			Modifiers.NEW |
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.PRIVATE |
			Modifiers.VIRTUAL |
			Modifiers.SEALED |
			Modifiers.OVERRIDE |
			Modifiers.ABSTRACT;

		public readonly string     Type;
		public readonly string     InterfaceType;
		public readonly Parameters FormalParameters;
		public readonly Block      Get;
		public readonly Block      Set;
		public Attributes          OptAttributes;
		public MethodBuilder       GetBuilder;
		public MethodBuilder       SetBuilder;
		public PropertyBuilder PropertyBuilder;
	        public Type IndexerType;

		public Indexer (string type, string int_type, int flags, Parameters parms,
				Block get_block, Block set_block, Attributes attrs, Location loc)
			: base ("", loc)
		{

			Type = type;
			InterfaceType = int_type;
			ModFlags = Modifiers.Check (AllowedModifiers, flags, Modifiers.PRIVATE);
			FormalParameters = parms;
			Get = get_block;
			Set = set_block;
			OptAttributes = attrs;
		}

		void DefineMethod (TypeContainer parent, Type iface_type,
				   Type ret_type, string name,
				   Type [] parameters, bool is_get)
		{
			MethodAttributes attr = Modifiers.MethodAttr (ModFlags);
			MethodInfo implementing;

			implementing = parent.IsInterfaceMethod (
				iface_type, name, ret_type, parameters, false);

			//
			// Setting null inside this block will trigger a more
			// verbose error reporting for missing interface implementations
			//
			// The "candidate" function has been flagged already
			// but it wont get cleared
			//
			if (implementing != null){
				if (iface_type == null){
					if ((ModFlags & Modifiers.PUBLIC) == 0)
						implementing = null;
					if ((ModFlags & Modifiers.STATIC) != 0)
						implementing = null;
				} else {
					if((ModFlags&(Modifiers.PUBLIC | Modifiers.ABSTRACT)) != 0){
						Report.Error (
							106, Location,
							"`public' or `abstract' modifiers are not "+
							"allowed in explicit interface declarations"
							);
						implementing = null;
					}
				}
			}
			if (implementing != null){
				attr |=
					MethodAttributes.Virtual |
					MethodAttributes.NewSlot |
					MethodAttributes.HideBySig;

				// If an interface implementing, then we can set final.
				if (((attr & MethodAttributes.Abstract) == 0) &&
				    implementing.DeclaringType.IsInterface)
					attr |= MethodAttributes.Final;
				
				//
				// clear the pending flag
				//
				parent.IsInterfaceMethod (
					iface_type, name, ret_type, parameters, true);
			}

			if (is_get){

				string meth_name = "get_Item";
				if (iface_type != null)
 					meth_name = iface_type + ".get_Item";
				
				GetBuilder = parent.TypeBuilder.DefineMethod (
					meth_name, attr, IndexerType, parameters);

				if (implementing != null) 
					parent.TypeBuilder.DefineMethodOverride (
						GetBuilder, implementing);
				
				
				PropertyBuilder.SetGetMethod (GetBuilder);
			} else {

				string meth_name = "set_Item";

				if (iface_type != null)
					meth_name = iface_type + ".set_Item";
				
				SetBuilder = parent.TypeBuilder.DefineMethod (
				        meth_name, attr, null, parameters);
				if (implementing != null)
					parent.TypeBuilder.DefineMethodOverride (
						SetBuilder, implementing);
					
				PropertyBuilder.SetSetMethod (SetBuilder);
			}
		}
			
		public override bool Define (TypeContainer parent)
		{
			PropertyAttributes prop_attr =
				PropertyAttributes.RTSpecialName |
				PropertyAttributes.SpecialName;
			bool error = false;
			
			IndexerType = RootContext.LookupType (parent, Type, false, Location);
			Type [] parameters = FormalParameters.GetParameterInfo (parent);

			// Check if the return type and arguments were correct
			if (IndexerType == null || parameters == null)
				return false;

			if (!parent.MethodModifiersValid (ModFlags, InterfaceType == null ?
							  "this" : InterfaceType, Location))
				return false;

			//
			// verify accessibility
			//
			if (!TypeContainer.AsAccessible (IndexerType, ModFlags))
				return false;

			foreach (Type partype in parameters)
				if (!TypeContainer.AsAccessible (partype, ModFlags))
					error = true;

			if (error)
				return false;
			
			Type iface_type = null;

			if (InterfaceType != null){
				iface_type = RootContext.LookupType (parent, InterfaceType, false, Location);
				if (iface_type == null)
					return false;
			} 
				
			
			PropertyBuilder = parent.TypeBuilder.DefineProperty (
				TypeManager.IndexerPropertyName (parent.TypeBuilder),
				prop_attr, IndexerType, parameters);

			if (Get != null){
				DefineMethod (parent, iface_type, IndexerType, "get_Item", parameters, true);
                                InternalParameters pi = new InternalParameters (parent, FormalParameters);
				if (!TypeManager.RegisterMethod (GetBuilder, pi, parameters)) {
					Report.Error (111, Location,
						      "Class `" + parent.Name +
						      "' already contains a definition with the " +
						      "same return value and parameter types for the " +
						      "'get' indexer");
					return false;
				}
			}
			
			if (Set != null){
				int top = parameters.Length;
				Type [] set_pars = new Type [top + 1];
				parameters.CopyTo (set_pars, 0);
				set_pars [top] = IndexerType;

				Parameter [] fixed_parms = FormalParameters.FixedParameters;

				Parameter [] tmp = new Parameter [fixed_parms.Length + 1];

				fixed_parms.CopyTo (tmp, 0);
				tmp [fixed_parms.Length] = new Parameter (
					Type, "value", Parameter.Modifier.NONE, null);

				Parameters set_formal_params = new Parameters (tmp, null, Location);
				
				DefineMethod (
					parent, iface_type, TypeManager.void_type,
					"set_Item", set_pars, false);

				InternalParameters ip = new InternalParameters (parent, set_formal_params);
				
				if (!TypeManager.RegisterMethod (SetBuilder, ip, set_pars)) {
					Report.Error (
						111, Location,
						"Class `" + parent.Name + "' already contains a " +
						"definition with the " +
						"same return value and parameter types for the " +
						"'set' indexer");
					return false;
				}
			}

			//
			// Now name the parameters
			//
			Parameter [] p = FormalParameters.FixedParameters;
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
					
				if (i != parameters.Length) {
					Parameter array_param = FormalParameters.ArrayParameter;
					SetBuilder.DefineParameter (i + 1, array_param.Attributes,
								    array_param.Name);
				}
			}

			TypeManager.RegisterProperty (PropertyBuilder, GetBuilder, SetBuilder);

			return true;
		}

		public void Emit (TypeContainer tc)
		{
			ILGenerator ig;
			EmitContext ec;

			ec = new EmitContext (tc, Location, null, IndexerType, ModFlags);
			Attribute.ApplyAttributes (ec, PropertyBuilder, this, OptAttributes, Location);

			if (Get != null){
				ig = GetBuilder.GetILGenerator ();
				ec = new EmitContext (tc, Location, ig, IndexerType, ModFlags);
				
				ec.EmitTopBlock (Get, Location);
			}

			if (Set != null){
				ig = SetBuilder.GetILGenerator ();
				ec = new EmitContext (tc, Location, ig, null, ModFlags);
				
				ec.EmitTopBlock (Set, Location);
			}
		}
	}

	public class Operator : MemberCore {

		const int AllowedModifiers =
			Modifiers.PUBLIC |
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
		public readonly string ReturnType;
		public readonly string FirstArgType;
		public readonly string FirstArgName;
		public readonly string SecondArgType;
		public readonly string SecondArgName;
		public readonly Block  Block;
		public Attributes      OptAttributes;
		public MethodBuilder   OperatorMethodBuilder;
		
		public string MethodName;
		public Method OperatorMethod;

		public Operator (OpType type, string ret_type, int flags, string arg1type, string arg1name,
				 string arg2type, string arg2name, Block block, Attributes attrs, Location loc)
			: base ("", loc)
		{
			OperatorType = type;
			ReturnType = ret_type;
			ModFlags = Modifiers.Check (AllowedModifiers, flags, Modifiers.PUBLIC);
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
			
			OperatorMethod.Define (parent);

			if (OperatorMethod.MethodBuilder == null)
				return false;
			
			OperatorMethodBuilder = OperatorMethod.MethodBuilder;

			Type [] param_types = OperatorMethod.ParameterTypes (parent);
			Type declaring_type = OperatorMethodBuilder.DeclaringType;
			Type return_type = OperatorMethod.GetReturnType (parent);
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
			Attribute.ApplyAttributes (ec, OperatorMethodBuilder, this, OptAttributes, Location);
			
			OperatorMethod.Block = Block;
			OperatorMethod.Emit (parent);
		}
	}

	//
	// This is used to compare method signatures
	//
	struct MethodSignature {
		public string Name;
		public Type RetType;
		public Type [] Parameters;
		
		public MethodSignature (string name, Type ret_type, Type [] parameters)
		{
			Name = name;
			RetType = ret_type;
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
	}		
}
