//
// class.cs: Class and Struct handlers
//
// Author: Miguel de Icaza (miguel@gnu.org)
//
// Licensed under the terms of the GNU GPL
//
// (C) 2001, 2002 Ximian, Inc (http://www.ximian.com)
//
//

using System;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Diagnostics.SymbolStore;

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

			if ((field.ModFlags & Modifiers.STATIC) == 0)
				have_nonstatic_fields = true;
			
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
				Object init = f.Initializer;

				Expression e;
				if (init is Expression)
					e = (Expression) init;
				else {
					string base_type = f.Type.Substring (0, f.Type.IndexOf ("["));
					string rank = f.Type.Substring (f.Type.IndexOf ("["));
					e = new ArrayCreation (base_type, rank, (ArrayList)init, f.Location);
				}

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
				string name = (string) bases [0];
				Type first = FindType (name);

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
				Type t = FindType (name);

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
						detail = " (a class can not inherit from a struct/enum)";
							
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
				
				//
				// Structs with no fields need to have a ".size 1"
				// appended
				//

				if (!is_class && !have_nonstatic_fields)
					TypeBuilder = builder.DefineType (Name,
									  type_attributes,
									  parent, 
									  PackingSize.Unspecified, 1);
				else
				//
				// classes or structs with fields
				//
					TypeBuilder = builder.DefineType (Name,
									  type_attributes,
									  parent,
									  ifaces);
			} else {
				TypeBuilder builder = Parent.TypeBuilder;
				
				//
				// Structs with no fields need to have a ".size 1"
				// appended
				//
				if (!is_class && !have_nonstatic_fields)
					TypeBuilder = builder.DefineNestedType (Basename,
										type_attributes,
										parent, 
										PackingSize.Unspecified);
				else {
					//
					// classes or structs with fields
					//
					TypeBuilder = builder.DefineNestedType (Basename,
										type_attributes,
										parent,
										ifaces);
				}
			}

			//
			// Finish the setup for the EmitContext
			//
			ec.ContainerType = TypeBuilder;

			TypeManager.AddUserType (Name, TypeBuilder, this, ifaces);

			if (parent == TypeManager.attribute_type ||
			    parent.IsSubclassOf (TypeManager.attribute_type)) {
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
		public MemberInfo [] FindMembers (MemberTypes mt, BindingFlags bf,
						  MemberFilter filter, object criteria)
		{
			ArrayList members = new ArrayList ();
			bool priv = (bf & BindingFlags.NonPublic) != 0;

			if (filter == null)
				filter = accepting_filter; 
			
			if ((mt & MemberTypes.Field) != 0) {
				if (fields != null) {
					foreach (Field f in fields) {
						if ((f.ModFlags & Modifiers.PRIVATE) != 0)
							if (!priv)
								continue;

						FieldBuilder fb = f.FieldBuilder;
						if (fb != null && filter (fb, criteria) == true)
							members.Add (fb);
					}
				}

				if (constants != null) {
					foreach (Const con in constants) {
						if ((con.ModFlags & Modifiers.PRIVATE) != 0)
							if (!priv)
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
						if ((m.ModFlags & Modifiers.PRIVATE) != 0)
							if (!priv)
								continue;
						
						MethodBuilder mb = m.MethodBuilder;

						if (mb != null && filter (mb, criteria) == true)
							members.Add (mb);
					}
				}

				if (operators != null){
					foreach (Operator o in operators) {
						if ((o.ModFlags & Modifiers.PRIVATE) != 0)
							if (!priv)
								continue;
						
						MethodBuilder ob = o.OperatorMethodBuilder;
						if (ob != null && filter (ob, criteria) == true)
							members.Add (ob);
					}
				}

				if (properties != null){
					foreach (Property p in properties){
						if ((p.ModFlags & Modifiers.PRIVATE) != 0)
							if (!priv)
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
			}

			if ((mt & MemberTypes.Event) != 0) {
				if (events != null)
				        foreach (Event e in events) {
						if ((e.ModFlags & Modifiers.PRIVATE) != 0)
							if (!priv)
								continue;

						MemberInfo eb = e.EventBuilder;
						if (eb != null && filter (eb, criteria) == true)
						        members.Add (e.EventBuilder);
					}
			}
			
			if ((mt & MemberTypes.Property) != 0){
				if (properties != null)
					foreach (Property p in properties) {
						if ((p.ModFlags & Modifiers.PRIVATE) != 0)
							if (!priv)
								continue;

						MemberInfo pb = p.PropertyBuilder;
						if (pb != null && filter (pb, criteria) == true) {
							members.Add (p.PropertyBuilder);
						}
					}

				if (indexers != null)
					foreach (Indexer ix in indexers) {
						if ((ix.ModFlags & Modifiers.PRIVATE) != 0)
							if (!priv)
								continue;

						MemberInfo ib = ix.PropertyBuilder;
						if (ib != null && filter (ib, criteria) == true) {
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

						if (cb != null)
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
			if (((bf & BindingFlags.DeclaredOnly) == 0) && (TypeBuilder.BaseType != null)) {
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
			if (events == null)
				return null;
			
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
			
			Attribute.ApplyAttributes (ec, TypeBuilder, this, OptAttributes, Location);

			//
			// Check for internal or private fields that were never assigned
			//
			if (fields != null && RootContext.WarningLevel >= 3) {
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
					d.CloseDelegate ();
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
			return true;
		}

		Hashtable builder_and_args;
		
		public bool RegisterMethod (MethodBuilder mb, InternalParameters ip, Type [] args)
		{
			if (builder_and_args == null)
				builder_and_args = new Hashtable ();
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
		public Type [] ParameterTypes (TypeContainer parent)
		{
			if (Parameters == null)
				return TypeManager.NoTypes;
			
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

		public void LabelParameters (EmitContext ec, Type [] parameters, MethodBase builder)
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
					
					if (mb == null)
						pb = cb.DefineParameter (
							i + 1, p [i].Attributes, p [i].Name);
					else 
						pb = mb.DefineParameter (
							i + 1, p [i].Attributes, p [i].Name);
					
					Attributes attr = p [i].OptAttributes;
					if (attr != null)
						Attribute.ApplyAttributes (ec, pb, pb, attr, Location);
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
		        Modifiers.UNSAFE |
			Modifiers.EXTERN;

		//
		// return_type can be "null" for VOID values.
		//
		public Method (string return_type, int mod, string name, Parameters parameters,
			       Attributes attrs, Location l)
			: base (name, parameters, l)
		{
			ReturnType = return_type;
			ModFlags = Modifiers.Check (AllowedModifiers, mod, Modifiers.PRIVATE, l);
			OptAttributes = attrs;
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
		// Creates the type
		//
		public override bool Define (TypeContainer parent)
		{
			Type ret_type = GetReturnType (parent);
			Type [] parameters = ParameterTypes (parent);
			bool error = false;
			MethodInfo implementing = null;
			Type iface_type = null;
			string iface = "", short_name;
			bool explicit_impl = false;

			// Check if the return type and arguments were correct
			if (ret_type == null || parameters == null)
				return false;
			
			if (!parent.MethodModifiersValid (ModFlags, Name, Location))
				return false;

			flags = Modifiers.MethodAttr (ModFlags);

			//
			// verify accessibility
			//
			if (!TypeContainer.AsAccessible (ret_type, ModFlags))
				return false;

			if (ret_type.IsPointer && !UnsafeOK (parent))
				return false;
			
			foreach (Type partype in parameters){
				if (!TypeContainer.AsAccessible (partype, ModFlags))
					error = true;
				if (partype.IsPointer && !UnsafeOK (parent))
					error = true;
			}

			if (error)
				return false;

			//
			// Verify if the parent has a type with the same name, and then
			// check whether we have to create a new slot for it or not.
			//
			Type ptype = parent.TypeBuilder.BaseType;

			// ptype is only null for System.Object while compiling corlib.
			if (ptype != null){
				MethodSignature ms = new MethodSignature (Name, null, parameters);
				MemberInfo [] mi, mi_static, mi_instance;

				mi_static = TypeContainer.FindMembers (
					ptype, MemberTypes.Method,
					BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static,
					MethodSignature.inheritable_method_signature_filter, ms);

				mi_instance = TypeContainer.FindMembers (
					ptype, MemberTypes.Method,
					BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance,
					MethodSignature.inheritable_method_signature_filter,
					ms);

				if (mi_instance != null && mi_instance.Length > 0){
					mi = mi_instance;
				} else if (mi_static != null && mi_static.Length > 0)
					mi = mi_static;
				else
					mi = null;

				if (mi != null && mi.Length > 0){
					if (!CheckMethodAgainstBase (parent, flags, (MethodInfo) mi [0])){
						return false;
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

			//
			// If we implement an interface, extract the interface name.
			//

			if (Name.IndexOf (".") != -1){
				int pos = Name.LastIndexOf (".");
				iface = Name.Substring (0, pos);

				iface_type = RootContext.LookupType (parent, iface, false, Location);
				short_name = Name.Substring (pos + 1);

				if (iface_type == null)
					return false;

				// Compute the full name that we need to export
				Name = iface_type.FullName + "." + short_name;
				explicit_impl = true;
			} else
				short_name = Name;

			//
			// Check if we are an implementation of an interface method or
			// a method
			//
			if (parent.Pending != null)
				implementing = parent.Pending.IsInterfaceMethod (
					iface_type, short_name, ret_type, parameters);
				
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
					//
					// We already catch different accessibility settings
					// so we just need to check that we are not private
					//
					if ((ModFlags & Modifiers.PRIVATE) != 0)
						implementing = null;

					//
					// Static is not allowed
					//
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
				if (implementing.DeclaringType.IsInterface)
					flags |= MethodAttributes.NewSlot;
				
				flags |=
					MethodAttributes.Virtual |
					MethodAttributes.HideBySig;

				//
				// clear the pending implementation flag
				//
				parent.Pending.ImplementMethod (
					iface_type, short_name, ret_type, parameters, explicit_impl);
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

				if ((ModFlags & Modifiers.STATIC) == 0) {
					Report.Error (601, Location, "The DllImport attribute must be specified on " +
						      "a method marked 'static' and 'extern'.");
					return false;
				}
				
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

				if (implementing != null && explicit_impl)
					parent.TypeBuilder.DefineMethodOverride (
						MethodBuilder, implementing);
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
			ILGenerator ig;
			EmitContext ec;

			if ((flags & MethodAttributes.PinvokeImpl) == 0)
				ig = MethodBuilder.GetILGenerator ();
			else
				ig = null;

			ec = new EmitContext (parent, Location, ig, GetReturnType (parent), ModFlags);

			if (OptAttributes != null)
				Attribute.ApplyAttributes (ec, MethodBuilder, this, OptAttributes, Location);
			

			LabelParameters (ec, ParameterTypes (parent), MethodBuilder);
			
			//
			// abstract or extern methods have no bodies
			//
			if ((ModFlags & (Modifiers.ABSTRACT | Modifiers.EXTERN)) != 0){
				if (Block != null){
					if ((ModFlags & Modifiers.ABSTRACT) != 0){
						Report.Error (
							500, "Abstract method `" +
							TypeManager.CSharpSignature (MethodBuilder) +
							"' can not have a body");
					}
				}
				return;
			}

			//
			// Handle destructors specially
			//
			// FIXME: This code generates buggy code
			//
			if (Name == "Finalize" && type_return_type == TypeManager.void_type)
				EmitDestructor (ec);
			else {
				ISymbolWriter sw = CodeGen.SymbolWriter;

				if ((sw != null) && (!Location.IsNull (Location))) {
					MethodToken token = MethodBuilder.GetToken ();
					sw.OpenMethod (new SymbolToken (token.Token));
					sw.SetMethodSourceRange (Location.SymbolDocument,
								 Location.Row, 0,
								 Block.EndLocation.SymbolDocument,
								 Block.EndLocation.Row, 0);

					ec.EmitTopBlock (Block, Location);

					sw.CloseMethod ();
				} else
					ec.EmitTopBlock (Block, Location);
			}
		}

		void EmitDestructor (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			
			Label finish = ig.DefineLabel ();
			bool old_in_try = ec.InTry;
			
			ig.BeginExceptionBlock ();
			ec.InTry = true;
			ec.ReturnLabel = finish;
			ec.EmitTopBlock (Block, Location);
			ec.InTry = old_in_try;
			
			ig.MarkLabel (finish);
			bool old_in_finally = ec.InFinally;
			ec.InFinally = true;
			ig.BeginFinallyBlock ();
			
			if (ec.ContainerType.BaseType != null) {
				Expression member_lookup = Expression.MemberLookup (
					ec, ec.ContainerType.BaseType, "Finalize",
					MemberTypes.Method, Expression.AllBindingFlags, Location);

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
				foreach (Argument a in argument_list){
					if (!a.Resolve (ec, location))
						return false;
				}
			}

			if (this is ConstructorBaseInitializer) {
				if (ec.ContainerType.BaseType == null)
					return true;

				t = ec.ContainerType.BaseType;
			} else
				t = ec.ContainerType;
			
			parent_constructor_group = Expression.MemberLookup (
				ec, t, ".ctor", 
				MemberTypes.Constructor,
				BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly,
				location);
			
			if (parent_constructor_group == null){
				Report.Error (1501, location,
				       "Can not find a constructor for this argument list");
				return false;
			}
			
			parent_constructor = (ConstructorInfo) Invocation.OverloadResolve (ec, 
				(MethodGroupExpr) parent_constructor_group, argument_list, location);
			
			if (parent_constructor == null){
				Report.Error (1501, location,
				       "Can not find a constructor for this argument list");
				return false;
			}
			
			return true;
		}

		public void Emit (EmitContext ec)
		{
			ec.ig.Emit (OpCodes.Ldarg_0);
			if (argument_list != null)
				Invocation.EmitArguments (ec, null, argument_list);
			if (parent_constructor != null)
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
			Modifiers.UNSAFE |
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

			if (parameters == null)
				return false;
			
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


				//
				// Spec mandates that Initializers will not have
				// `this' access
				//
				ec.IsStatic = true;
				if (!Initializer.Resolve (ec))
					return;
				ec.IsStatic = false;
			}

			LabelParameters (ec, ParameterTypes (parent), ConstructorBuilder);
			
			//
			// Classes can have base initializers and instance field initializers.
			//
			if (parent is Class){
				if ((ModFlags & Modifiers.STATIC) == 0){
					parent.EmitFieldInitializers (ec);

					Initializer.Emit (ec);
				}
			}
			
			if ((ModFlags & Modifiers.STATIC) != 0)
				parent.EmitFieldInitializers (ec);

			Attribute.ApplyAttributes (ec, ConstructorBuilder, this, OptAttributes, Location);

			ec.EmitTopBlock (Block, Location);
		}
	}

	//
	// Fields and Events both generate FieldBuilders, we use this to share 
	// their common bits.  This is also used to flag usage of the field
	//
	abstract public class FieldBase : MemberCore {
		public readonly string Type;
		public readonly Object Initializer;
		public readonly Attributes OptAttributes;
		public FieldBuilder  FieldBuilder;
		public Status status;

		[Flags]
		public enum Status : byte { ASSIGNED = 1, USED = 2 }

		//
		// The constructor is only exposed to our children
		//
		protected FieldBase (string type, int mod, int allowed_mod, string name,
				     object init, Attributes attrs, Location loc)
			: base (name, loc)
		{
			Type = type;
			ModFlags = Modifiers.Check (allowed_mod, mod, Modifiers.PRIVATE, loc);
			Initializer = init;
			OptAttributes = attrs;
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

		public Field (string type, int mod, string name, Object expr_or_array_init,
			      Attributes attrs, Location loc)
			: base (type, mod, AllowedModifiers, name, expr_or_array_init, attrs, loc)
		{
		}

		public override bool Define (TypeContainer parent)
		{
			Type t = RootContext.LookupType (parent, Type, false, Location);
			
			if (t == null)
				return false;

			if (!TypeContainer.AsAccessible (t, ModFlags))
				return false;

			if (t.IsPointer && !UnsafeOK (parent))
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

			TypeManager.RegisterFieldBase (FieldBuilder, this);
			return true;
		}

		public void Emit (TypeContainer tc)
		{
			EmitContext ec = new EmitContext (tc, Location, null,
							  FieldBuilder.FieldType, ModFlags);

			Attribute.ApplyAttributes (ec, FieldBuilder, this, OptAttributes, Location);
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
			
	public class Property : MemberCore {
		public readonly string Type;
		public Accessor Get, Set;
		public PropertyBuilder PropertyBuilder;
		public Attributes OptAttributes;
		public MethodBuilder GetBuilder, SetBuilder;

		//
		// The type, once we compute it.
		Type PropertyType;

		bool explicit_impl;

		//
		// If true, the interface type we are explicitly implementing
		//
		Type explicit_iface_type = null;

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

		public Property (string type, string name, int mod_flags,
				 Accessor get_block, Accessor set_block,
				 Attributes attrs, Location loc)
			: base (name, loc)
		{
			Type = type;
			ModFlags = Modifiers.Check (AllowedModifiers, mod_flags, Modifiers.PRIVATE, loc);
			Get = get_block;
			Set = set_block;
			OptAttributes = attrs;
		}

		//
		// Checks our base implementation if any
		//
		bool CheckBase (MethodAttributes flags, TypeContainer parent)
		{
			//
			// Find properties with the same name on the base class
			//

			MemberInfo [] props;
			MemberInfo [] props_static = TypeManager.MemberLookup (
				parent.TypeBuilder, 
				parent.TypeBuilder.BaseType,
				MemberTypes.Property, BindingFlags.Public | BindingFlags.Static,
				Name);

			MemberInfo [] props_instance = TypeManager.MemberLookup (
				parent.TypeBuilder, 
				parent.TypeBuilder.BaseType,
				MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance,
				Name);

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
				if (props.Length > 1)
					throw new Exception ("Should not happen");
				
				PropertyInfo pi = (PropertyInfo) props [0];

				MethodInfo inherited_get = TypeManager.GetPropertyGetter (pi);
				MethodInfo inherited_set = TypeManager.GetPropertySetter (pi);

				MethodInfo reference = inherited_get == null ?
					inherited_set : inherited_get;
				
				if (reference != null)
					if (!CheckMethodAgainstBase (parent, flags, reference))
						return false;
				
			} else {
				if ((ModFlags & Modifiers.NEW) != 0)
					WarningNotHiding (parent);
				
				if ((ModFlags & Modifiers.OVERRIDE) != 0){
					Report.Error (115, Location,
						      parent.MakeName (Name) +
						      " no suitable properties found to override");
					return false;
				}
			}
			return true;
		}

		bool DefineMethod (TypeContainer parent, string short_name,
				   MethodAttributes flags, bool is_get, ref bool is_implementing)
		{
			Type [] parameters = TypeManager.NoTypes;
			MethodInfo implementing = null;
			Type fn_type;
			string name, prefix;

			if (explicit_impl)
				prefix = explicit_iface_type.FullName + ".";
			else
				prefix = "";
				
			if (is_get){
				fn_type = PropertyType;
				name = "get_" + short_name;
			} else {
				name = "set_" + short_name;
				parameters = new Type [1];
				parameters [0] = PropertyType;
				fn_type = TypeManager.void_type;
			}

			if (parent.Pending != null)
				implementing = parent.Pending.IsInterfaceMethod (
					explicit_iface_type, name, fn_type, parameters);

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
				if (explicit_iface_type == null){
					//
					// We already catch different accessibility settings
					// so we just need to check that we are not private
					//
					if ((ModFlags & Modifiers.PRIVATE) != 0)
						implementing = null;
					
					//
					// Static is not allowed
					//
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
				//
				// When implementing interface methods, set NewSlot.
				//
				if (implementing.DeclaringType.IsInterface)
					flags |= MethodAttributes.NewSlot;
				
				flags |=
					MethodAttributes.Virtual |
					MethodAttributes.HideBySig;

				//
				// clear the pending implemntation flag
				//
				parent.Pending.ImplementMethod (
					explicit_iface_type, name, fn_type, parameters, explicit_impl);

				is_implementing = true;
			}

			//
			// If this is not an explicit interface implementation,
			// clear implementing, as it is only used for explicit
			// interface implementation
			//
			if (Name.IndexOf (".") == -1)
				implementing = null;
			
			if (is_get){
				GetBuilder = parent.TypeBuilder.DefineMethod (
					prefix + name, flags, PropertyType, null);
			
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
					prefix + name, flags, null, parameters);
				
				if (implementing != null)
					parent.TypeBuilder.DefineMethodOverride (
						SetBuilder, implementing);
				
				SetBuilder.DefineParameter (1, ParameterAttributes.None, "value"); 

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
			string short_name;
			
			if (!parent.MethodModifiersValid (ModFlags, Name, Location))
				return false;

			MethodAttributes flags = Modifiers.MethodAttr (ModFlags);
			
			flags |= MethodAttributes.HideBySig | MethodAttributes.SpecialName;

			// Lookup Type, verify validity
			PropertyType = RootContext.LookupType (parent, Type, false, Location);
			if (PropertyType == null)
				return false;

			// verify accessibility
			if (!TypeContainer.AsAccessible (PropertyType, ModFlags))
				return false;

			if (PropertyType.IsPointer && !UnsafeOK (parent))
				return false;
			
			if (!CheckBase (flags, parent))
				return false;

			//
			// Check for explicit interface implementation
			//
			if (Name.IndexOf (".") != -1){
				int pos = Name.LastIndexOf (".");
				string iface = Name.Substring (0, pos);

				explicit_iface_type = RootContext.LookupType (parent, iface, false, Location);
				if (explicit_iface_type == null)
					return false;

				short_name = Name.Substring (pos + 1);

				// Compute the full name that we need to export.
				Name = explicit_iface_type.FullName + "." + short_name;
				explicit_impl = true;
			} else {
				explicit_impl = false;
				short_name = Name;
			}

			bool is_implementing = false;
			if (Get != null)
				if (!DefineMethod (parent, short_name, flags, true, ref is_implementing))
					return false;
			
			if (Set != null)
				if (!DefineMethod (parent, short_name, flags, false, ref is_implementing))
					return false;

			// FIXME - PropertyAttributes.HasDefault ?
			
			PropertyAttributes prop_attr =
			PropertyAttributes.RTSpecialName |
			PropertyAttributes.SpecialName;

			if (!explicit_impl){
				PropertyBuilder = parent.TypeBuilder.DefineProperty (
					Name, prop_attr, PropertyType, null);
				
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
		
		public void Emit (TypeContainer tc)
		{
			ILGenerator ig;
			EmitContext ec;

			ec = new EmitContext (tc, Location, null, PropertyType, ModFlags);

			//
			// The PropertyBuilder can be null for explicit implementations, in that
			// case, we do not actually emit the ".property", so there is nowhere to
			// put the attribute
			//
			if (PropertyBuilder != null)
				Attribute.ApplyAttributes (ec, PropertyBuilder, this, OptAttributes, Location);
			

			//
			// abstract or extern properties have no bodies
			//
			if ((ModFlags & (Modifiers.ABSTRACT | Modifiers.EXTERN)) != 0)
				return;

			if (Get != null){
				ig = GetBuilder.GetILGenerator ();
				ec = new EmitContext (tc, Location, ig, PropertyType, ModFlags);

				Attribute.ApplyAttributes (ec, GetBuilder, Get, Get.OptAttributes, Location);
				ec.EmitTopBlock (Get.Block, Location);
			}

			if (Set != null){
				ig = SetBuilder.GetILGenerator ();
				ec = new EmitContext (tc, Location, ig, null, ModFlags);

				Attribute.ApplyAttributes (ec, SetBuilder, Set, Set.OptAttributes, Location);
				ec.EmitTopBlock (Set.Block, Location);
			}
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

		public MyEventBuilder (TypeBuilder type_builder, string name, EventAttributes event_attr, Type event_type)
		{
			MyBuilder = type_builder.DefineEvent (name, event_attr, event_type);

			// And now store the values in our own fields.
			
			declaring_type = type_builder;

			reflected_type = type_builder;
			
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

		Type EventType;
		MethodBuilder AddBuilder, RemoveBuilder;
		
		public Event (string type, string name, Object init, int mod, Accessor add,
			      Accessor remove, Attributes attrs, Location loc)
			: base (type, mod, AllowedModifiers, name, init, attrs, loc)
		{
			Add = add;
			Remove = remove;
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

			if (EventType.IsPointer && !UnsafeOK (parent))
				return false;

			if (!EventType.IsSubclassOf (TypeManager.delegate_type)) {
				Report.Error (66, Location, "'" + parent.Name + "." + Name +
					      "' : event must be of a delegate type");
				return false;
			}
			
			Type [] parameters = new Type [1];
			parameters [0] = EventType;

			EventBuilder = new MyEventBuilder (parent.TypeBuilder, Name, e_attr, EventType);

			if (Add == null && Remove == null){
				FieldBuilder = parent.TypeBuilder.DefineField (
					Name, EventType, FieldAttributes.Private);
				TypeManager.RegisterFieldBase (FieldBuilder, this);
			}
			
			//
			// Now define the accessors
			//
			string add_name = "add_" + Name;
			
			AddBuilder = parent.TypeBuilder.DefineMethod (
				add_name, m_attr, null, parameters);
			AddBuilder.DefineParameter (1, ParameterAttributes.None, "value");
			EventBuilder.SetAddOnMethod (AddBuilder);
			if (parent.Pending != null)
				parent.Pending.ImplementMethod (null, add_name, null, parameters, false);
			
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

			string remove_name = "remove_" + Name;
			RemoveBuilder = parent.TypeBuilder.DefineMethod (
				remove_name, m_attr, null, parameters);
			RemoveBuilder.DefineParameter (1, ParameterAttributes.None, "value");
			EventBuilder.SetRemoveOnMethod (RemoveBuilder);

			//
			// This looks like dead code
			//
			//if (parent.Pending != null)
			// parent.Pending.IsInterfaceMethod (null, remove_name, null, parameters, false);

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

			if (Add != null) {
				Attribute.ApplyAttributes (ec, AddBuilder, Add, Add.OptAttributes, Location);
				ec.EmitTopBlock (Add.Block, Location);
			} else
				EmitDefaultMethod (ec, true);

			ig = RemoveBuilder.GetILGenerator ();
			ec = new EmitContext (tc, Location, ig, TypeManager.void_type, ModFlags);
			
			if (Remove != null) {
				Attribute.ApplyAttributes (ec, RemoveBuilder, Remove, Remove.OptAttributes, Location);
				ec.EmitTopBlock (Remove.Block, Location);
			} else
				EmitDefaultMethod (ec, false);

			ec = new EmitContext (tc, Location, null, EventType, ModFlags);
			Attribute.ApplyAttributes (ec, EventBuilder, this, OptAttributes, Location);
			
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
			Modifiers.UNSAFE |
			Modifiers.EXTERN |
			Modifiers.ABSTRACT;

		public readonly string     Type;
		public readonly string     InterfaceType;
		public readonly Parameters FormalParameters;
		public readonly Accessor   Get, Set;
		public Attributes          OptAttributes;
		public MethodBuilder       GetBuilder;
		public MethodBuilder       SetBuilder;
		public PropertyBuilder PropertyBuilder;
	        public Type IndexerType;
		public string IndexerName;
		
		EmitContext ec;
		
		public Indexer (string type, string int_type, int flags, Parameters parms,
				Accessor get_block, Accessor set_block, Attributes attrs, Location loc)
			: base ("", loc)
		{

			Type = type;
			InterfaceType = int_type;
			ModFlags = Modifiers.Check (AllowedModifiers, flags, Modifiers.PRIVATE, loc);
			FormalParameters = parms;
			Get = get_block;
			Set = set_block;
			OptAttributes = attrs;
		}

		bool DefineMethod (TypeContainer parent, Type explicit_iface_type, 
				   Type ret_type, string name,
				   Type [] parameters, MethodAttributes attr, bool is_get)
		{
			MethodInfo implementing = null;
			bool is_implementation;

			if (parent.Pending != null)
				implementing = parent.Pending.IsInterfaceMethod (
					explicit_iface_type, name, ret_type, parameters);

			is_implementation = implementing != null;
			
			//
			// Setting null inside this block will trigger a more
			// verbose error reporting for missing interface implementations
			//
			// The "candidate" function has been flagged already
			// but it wont get cleared
			//
			if (implementing != null){
				if (explicit_iface_type == null){
					//
					// We already catch different accessibility settings
					// so we just need to check that we are not private
					//
					if ((ModFlags & Modifiers.PRIVATE) != 0)
						implementing = null;
					
					//
					// Static is not allowed
					//
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
				//
				// When implementing interface methods, set NewSlot.
				//
				if (implementing.DeclaringType.IsInterface)
					attr |= MethodAttributes.NewSlot;
				
				attr |=
					MethodAttributes.Virtual |
					MethodAttributes.HideBySig;

				//
				// clear the pending implementing flag
				//
				parent.Pending.ImplementMethod (
					explicit_iface_type, name, ret_type, parameters, true);
			}

			//
			// If this is not an explicit interface implementation,
			// clear implementing, as it is only used for explicit
			// interface implementation
			//
			if (InterfaceType == null)
				implementing = null;

			string prefix;
			if (explicit_iface_type == null)
				prefix = "";
			else
				prefix = explicit_iface_type.FullName + ".";
			
			if (is_get){
				string meth_name = prefix + "get_" + IndexerName;
				
				GetBuilder = parent.TypeBuilder.DefineMethod (
					meth_name, attr, IndexerType, parameters);

				if (implementing != null) 
					parent.TypeBuilder.DefineMethodOverride (
						GetBuilder, implementing);
			} else {
				string meth_name = prefix + "set_" + IndexerName;

				SetBuilder = parent.TypeBuilder.DefineMethod (
				        meth_name, attr, null, parameters);
				if (implementing != null)
					parent.TypeBuilder.DefineMethodOverride (
						SetBuilder, implementing);
			}

			return is_implementation;
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
			// verify accessibility and unsafe pointers
			//
			if (!TypeContainer.AsAccessible (IndexerType, ModFlags))
				return false;

			if (IndexerType.IsPointer && !UnsafeOK (parent))
				return false;

			foreach (Type partype in parameters){
				if (!TypeContainer.AsAccessible (partype, ModFlags))
					error = true;
				if (partype.IsPointer && !UnsafeOK (parent))
					error = true;
			}

			if (error)
				return false;
			
			Type explicit_iface_type = null;

			if (InterfaceType != null){
				explicit_iface_type = RootContext.LookupType (parent, InterfaceType, false, Location);
				if (explicit_iface_type == null)
					return false;
			} 

			ec = new EmitContext (parent, Location, null, IndexerType, ModFlags);

			IndexerName = Attribute.ScanForIndexerName (ec, OptAttributes);
			if (IndexerName == null)
				IndexerName = "Item";
			
			MethodAttributes attr = Modifiers.MethodAttr (ModFlags);

			bool is_implementing = false;
			
			if (Get != null){
				is_implementing = DefineMethod (
					parent, explicit_iface_type, IndexerType, "get_" + IndexerName,
					parameters, attr, true);
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
				
				is_implementing = DefineMethod (
					parent, explicit_iface_type, TypeManager.void_type,
					"set_" + IndexerName, set_pars, attr, false);

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


			//
			// Only define the PropertyBuilder if we are not implementing
			// an interface property.
			//
			if (!is_implementing){
				PropertyBuilder = parent.TypeBuilder.DefineProperty (
					IndexerName, prop_attr, IndexerType, parameters);

				if (GetBuilder != null)
					PropertyBuilder.SetGetMethod (GetBuilder);

				if (SetBuilder != null)
					PropertyBuilder.SetSetMethod (SetBuilder);
				
				TypeManager.RegisterProperty (PropertyBuilder, GetBuilder, SetBuilder);
			}

			return true;
		}

		public void Emit (TypeContainer tc)
		{
			ILGenerator ig;

			//
			// The PropertyBuilder can be null for explicit implementations, in that
			// case, we do not actually emit the ".property", so there is nowhere to
			// put the attribute
			//
			if (PropertyBuilder != null)
				Attribute.ApplyAttributes (
					ec, PropertyBuilder, this, OptAttributes, Location);

			if ((ModFlags & (Modifiers.ABSTRACT | Modifiers.EXTERN)) != 0)
				return;
			
			if (Get != null){
				ig = GetBuilder.GetILGenerator ();
				ec = new EmitContext (tc, Location, ig, IndexerType, ModFlags);
				
				ec.EmitTopBlock (Get.Block, Location);
			}

			if (Set != null){
				ig = SetBuilder.GetILGenerator ();
				ec = new EmitContext (tc, Location, ig, null, ModFlags);
				
				ec.EmitTopBlock (Set.Block, Location);
			}
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

			//
			// abstract or extern methods have no bodies
			//
			if ((ModFlags & (Modifiers.ABSTRACT | Modifiers.EXTERN)) != 0)
				return;
			
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
			MethodInfo mi;

			if (! (m is MethodInfo))
				return false;

			MethodSignature sig = (MethodSignature) filter_criteria;

			if (m.Name != sig.Name)
				return false;
			
			mi = (MethodInfo) m;

			//
			// we use sig.RetType == null to mean `do not check the
			// method return value.  
			//
			if (sig.RetType != null)
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
		        if (MemberSignatureCompare (m, filter_criteria)){
				MethodInfo mi = (MethodInfo) m;
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
			return false;
		}
	}		
}
