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

namespace CIR {
	
	public class TypeContainer : DeclSpace {
		protected int mod_flags;

		// Holds a list of classes and structures
		ArrayList types;

		// Holds the list of properties
		ArrayList properties;

		// Holds the list of enumerations
		ArrayList enums;

		// Holds the list of delegates
		ArrayList delegates;
		
		// Holds the list of constructors
		ArrayList constructors;

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

		// Maps MethodBuilders to Methods
		static Hashtable method_builders_to_parameters;
		
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
		// This is the namespace in which this typecontainer
		// was declared.  We use this to resolve names.
		//
		Namespace my_namespace;
		
		//
		// This one is computed after we can distinguish interfaces
		// from classes from the arraylist `type_bases' 
		//
		string     base_class_name;

		TypeContainer parent;
		ArrayList type_bases;

		//
		// This behaves like a property ;-)
		//
		public readonly RootContext RootContext;

		// Attributes for this type
		protected Attributes attributes;

		public TypeContainer (RootContext rc, TypeContainer parent, string name, Location l)
			: base (name, l)
		{
			string n;
			types = new ArrayList ();
			this.parent = parent;
			RootContext = rc;

			if (parent == null)
				n = "";
			else
				n = parent.Name;

			base_class_name = null;
			
			//Console.WriteLine ("New class " + name + " inside " + n);
		}

		public AdditionResult AddConstant (Constant constant)
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

		public AdditionResult AddEnum (CIR.Enum e)
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

			if (constructors == null)
				constructors = new ArrayList ();

			constructors.Add (c);

			bool is_static = (c.ModFlags & Modifiers.STATIC) != 0;
			
			if (is_static)
				have_static_constructor = true;

			if (c.IsDefault ()) {
				if (is_static)
					default_static_constructor = c;
				else
					default_constructor = c;
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
		
		public TypeContainer Parent {
			get {
				return parent;
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
		
		public int ModFlags {
			get {
				return mod_flags;
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

		public ArrayList Constructors {
			get {
				return constructors;
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
		
		public Namespace Namespace {
			get {
				return my_namespace;
			}

			set {
				my_namespace = value;
			}
		}

		// 
		// root_types contains all the types.  All TopLevel types
		// hence have a parent that points to `root_types', that is
		// why there is a non-obvious test down here.
		//
		public bool IsTopLevel {
			get {
				if (parent != null){
					if (parent.Parent == null)
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
				return Modifiers.TypeAttr (mod_flags, this);
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

				if (init is Expression){
					Expression e = (Expression) init;

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

			c = new Constructor (Basename, new Parameters (null, null),
					     new ConstructorBaseInitializer (null, new Location (-1)),
					     new Location (-1));
			
			AddConstructor (c);
			
			c.Block = new Block (null);
			
			if (is_static)
				mods = Modifiers.STATIC;

			c.ModFlags = mods;

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

		public void RegisterRequiredImplementations ()
		{
			Type [] ifaces = TypeBuilder.GetInterfaces ();
			Type b = TypeBuilder.BaseType;
			
			if (ifaces != null)
				SetRequiredInterfaces (ifaces);

			if (b.IsAbstract){
				MemberInfo [] abstract_methods;

				abstract_methods = FindMembers (
					TypeBuilder.BaseType,
					MemberTypes.Method, BindingFlags.Public,
					abstract_method_filter, null);

				if (abstract_methods != null){
					MethodInfo [] mi = new MethodInfo [abstract_methods.Length];

					abstract_methods.CopyTo (mi, 0);
					RequireMethods (mi, b);
				}
			}
			
		}

		static object MakeKey (MethodBase mb)
		{
			if (mb is MethodBuilder || mb is ConstructorBuilder)
				return mb.ReflectedType.FullName + ":" + mb;
			else
				return mb.MethodHandle.ToString ();
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

			if (is_class){
				parent = (Class) RootContext.Tree.Classes [name];
			} else {
				parent = (Struct) RootContext.Tree.Structs [name];
			}

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

		//
		// This function computes the Base class and also the
		// list of interfaces that the class or struct @c implements.
		//
		// The return value is an array (might be null) of
		// interfaces implemented (as Types).
		//
		// The @parent argument is set to the parent object or null
		// if this is `System.Object'. 
		//
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
				TypeBuilder builder = (TypeBuilder) parent_builder;
				
				//
				// Structs with no fields need to have a ".size 1"
				// appended
				//
				if (!is_class && Fields == null)
					TypeBuilder = builder.DefineNestedType (Name,
										TypeAttr,
										parent, 
										PackingSize.Unspecified);
				else
				//
				// classes or structs with fields
				//
					TypeBuilder = builder.DefineNestedType (Name,
										TypeAttr,
										parent,
										ifaces);
			}

			RootContext.TypeManager.AddUserType (Name, TypeBuilder, this);

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
		
		//
		// Populates our TypeBuilder with fields and methods
		//
		public void Populate ()
		{
			if (Constants != null){
				foreach (Constant c in Constants)
					c.EmitConstant (RootContext, this);
			}

			if (Fields != null){
				foreach (Field f in Fields)
					f.Define (this);
			} 

			if (this is Class && constructors == null){
				if (default_constructor == null) 
					DefineDefaultConstructor (false);

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
			
			ArrayList remove_list = new ArrayList ();

			if (constructors != null || methods != null ||
			    properties != null || operators != null){
				if (method_builders_to_parameters == null)
					method_builders_to_parameters = new Hashtable ();
			}
			
			if (constructors != null){
				foreach (Constructor c in constructors){
					MethodBase builder = c.Define (this);
					
					if (builder == null)
						remove_list.Add (c);
					else {
						InternalParameters ip = c.ParameterInfo;
						
						method_builders_to_parameters.Add (MakeKey (builder), ip);
					}
				}

				foreach (object o in remove_list)
					constructors.Remove (o);
				
				remove_list.Clear ();
			} 

			if (Methods != null){
				foreach (Method m in methods){
					MethodBase key = m.Define (this);

					//
					// FIXME:
					// The following key is not enoug
					// class x { public void X ()  {} }
					// class y : x { public void X () {}}
					// fails
					
					if (key == null)
						remove_list.Add (m);
					else {
						InternalParameters ip = m.ParameterInfo;
						method_builders_to_parameters.Add (MakeKey (key), ip);
					}
				}
				foreach (object o in remove_list)
					methods.Remove (o);
				
				remove_list.Clear ();
			}

			if (Properties != null) {
				foreach (Property p in Properties)
					p.Define (this);
			}

			if (Events != null) {
				foreach (Event e in Events)
					e.Define (this);
			}

			if (Indexers != null) {
				foreach (Indexer i in Indexers)
					i.Define (this);
			}

			if (Operators != null) {
				foreach (Operator o in Operators) {
					o.Define (this);

					InternalParameters ip = o.OperatorMethod.ParameterInfo;
					
					method_builders_to_parameters.Add (
						MakeKey (o.OperatorMethodBuilder), ip);
				}
			}

			if (Delegates != null) {
				foreach (Delegate d in Delegates) 
					d.Populate (this);
			}
			
			if (Types != null) {
				foreach (TypeContainer tc in Types)
					tc.Populate ();
			}

		
		}

		// <summary>
		//   Since System.Reflection.Emit can not retrieve parameter information
		//   from methods that are dynamically defined, we have to look those
		//   up ourselves using this
		// </summary>
		static public ParameterData LookupParametersByBuilder (MethodBase mb)
		{
			return (ParameterData) method_builders_to_parameters [MakeKey (mb)];
		}

		// <summary>
		//   Indexers and properties can register more than one method at once,
		//   so we need to provide a mechanism for those to register their
		//   various methods to parameter info mappers.
		// </summary>
		static public void RegisterParameterForBuilder (MethodBase mb, InternalParameters pi)
		{
			method_builders_to_parameters.Add (MakeKey (mb), pi);
		}
		
		public Type LookupType (string name, bool silent)
		{
			return RootContext.LookupType (this, name, silent);
		}

		public string LookupAlias (string Name)
		{
			//
			// Read the comments on `mcs/mcs/TODO' for details
			// 
			return null;
		}
		
		//
		// This function is based by a delegate to the FindMembers routine
		//
		static bool AlwaysAccept (MemberInfo m, object filterCriteria)
		{
			return true;
		}
		
		//
		static bool IsAbstractMethod (MemberInfo m, object filter_criteria)
		{
			MethodInfo mi = (MethodInfo) m;

			return mi.IsAbstract;
		}

		// This filter is used by FindMembers, and we just keep
		// a global for the filter to `AlwaysAccept'
		//
		static MemberFilter accepting_filter;
		
		// <summary>
		//    This delegate is a MemberFilter used to extract the 
		//    abstact methods from a type.  
		// </summary>
		static MemberFilter abstract_method_filter;

		static TypeContainer ()
		{
			abstract_method_filter = new MemberFilter (IsAbstractMethod);
			accepting_filter = new MemberFilter (AlwaysAccept);
		}
		
		// <summary>
		//   This method returns the members of this type just like Type.FindMembers would
		//   Only, we need to use this for types which are _being_ defined because MS' 
		//   implementation can't take care of that.
		// </summary>
		public MemberInfo [] FindMembers (MemberTypes mt, BindingFlags bf,
						  MemberFilter filter, object criteria)
		{
			ArrayList members = new ArrayList ();

			if (filter == null)
				filter = accepting_filter; 
			
			if ((mt & MemberTypes.Field) != 0 && Fields != null) {
				foreach (Field f in Fields) {
					if (filter (f.FieldBuilder, criteria) == true)
						members.Add (f.FieldBuilder);
				}
			}

			if ((mt & MemberTypes.Method) != 0) {
				if (Methods != null){
					foreach (Method m in Methods) {
						MethodBuilder mb = m.MethodBuilder;

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

			// FIXME : This ain't right because EventBuilder is not a
			// MemberInfo. What do we do ?
			
			if ((mt & MemberTypes.Event) != 0 && Events != null) {
				//foreach (Event e in Events) {
				//	if (filter (e.EventBuilder, criteria) == true)
				//		mi [i++] = e.EventBuilder;
				//}
			}

			if ((mt & MemberTypes.Property) != 0){
				if (Properties != null)
					foreach (Property p in Properties) {
						if (filter (p.PropertyBuilder, criteria) == true)
							members.Add (p.PropertyBuilder);
					}

				if (Indexers != null)
					foreach (Indexer ix in Indexers){
						if (filter (ix.PropertyBuilder, criteria) == true)
							members.Add (ix.PropertyBuilder);
					}
			}
			
			if ((mt & MemberTypes.NestedType) != 0 && Types != null) {
				foreach (TypeContainer t in Types) { 
					if (filter (t.TypeBuilder, criteria) == true)
						members.Add (t.TypeBuilder);
				}
			}

			if ((mt & MemberTypes.Constructor) != 0){
				if (Constructors != null){
					foreach (Constructor c in Constructors){
						ConstructorBuilder cb = c.ConstructorBuilder;

						if (filter (cb, criteria) == true)
							members.Add (cb);
					}
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

		public static MemberInfo [] FindMembers (Type t, MemberTypes mt, BindingFlags bf,
							 MemberFilter filter, object criteria)
		{
			TypeContainer tc = TypeManager.LookupTypeContainer (t);

			if (tc != null)
				return tc.FindMembers (mt, bf, filter, criteria);
			else
				return t.FindMembers (mt, bf, filter, criteria);
		}
		

		Hashtable pending_implementations;
		
		// <summary>
		//   Requires that the methods in `mi' be implemented for this
		//   class
		// </summary>
		public void RequireMethods (MethodInfo [] mi, object data)
		{
			if (pending_implementations == null)
				pending_implementations = new Hashtable ();

			foreach (MethodInfo m in mi){
				Type [] types = TypeManager.GetArgumentTypes (m);

				pending_implementations.Add (
					new MethodSignature 
						(m.Name, m.ReturnType, types), data);
			}
		}

		// <summary>
		//   Used to set the list of interfaces that this typecontainer
		//   must implement.
		// </summary>
		//
		// <remarks>
		//   For each element exposed by the type, we create a MethodSignature
		//   struct that we will label as `implemented' as we define the various
		//   methods.
		// </remarks>
		public void SetRequiredInterfaces (Type [] ifaces)
		{
			foreach (Type t in ifaces){
				MethodInfo [] mi;

				if (t is TypeBuilder){
					Interface iface = RootContext.TypeManager.LookupInterface (t);

					mi = iface.GetMethods ();
				} else
					mi = t.GetMethods ();

				RequireMethods (mi, t);
			}
		}

		// <summary>
		//   If a method with name `Name', return type `ret_type' and
		//   arguments `args' implements an interface, this method will
		//   return true.
		//
		//   This will remove the method from the list of "pending" methods
		//   that are required to be implemented for this class as a side effect.
		// 
		// </summary>
		public bool IsInterfaceMethod (string Name, Type ret_type, Type [] args)
		{
			MethodSignature query;

			if (pending_implementations == null)
				return false;

			query = new MethodSignature (Name, ret_type, args);

			if (pending_implementations.Contains (query)){
				pending_implementations.Remove (query);
				return true;
			}

			return false;
		}

		// <summary>
		//   Verifies that any pending abstract methods or interface methods
		//   were implemented.
		// </summary>
		void VerifyPendingMethods ()
		{
			int pending = 0;
			
			foreach (object m in pending_implementations){
				DictionaryEntry de = (DictionaryEntry) m;
				Type t = (Type) de.Value;
				pending++;

				MethodSignature method = (MethodSignature) de.Key;

				if (t.IsInterface)
					Report.Error (
						536, Location,
						"`" + Name + "' does not implement interface member `" +
						t.FullName + "." + method.Name + "'");
				else
					Report.Error (
						534, Location,
						"`" + Name + "' does not implement inherited abstract " +
						"member `" + t.FullName + "." + method.Name + "'");
			}
		}
		
		// <summary>
		//   Emits the code, this step is performed after all
		//   the types, enumerations, constructors
		// </summary>
		public void Emit ()
		{
			if (Constructors != null)
				foreach (Constructor c in Constructors)
					c.Emit (this);
			
			if (methods != null)
				foreach (Method m in methods)
					m.Emit (this);

			if (operators != null)
				foreach (Operator o in operators)
					o.Emit (this);

			if (properties != null)
				foreach (Property p in properties)
					p.Emit (this);

			if (indexers != null)
				foreach (Indexer ix in indexers)
					ix.Emit (this);

			if (fields != null)
				foreach (Field f in fields)
					f.Emit (this);

			if (events != null)
				foreach (Event e in Events)
					e.Emit (this);

			if (enums != null)
				foreach (Enum en in enums)
					en.Emit (this);
			
			if (pending_implementations != null)
				VerifyPendingMethods ();

			if (OptAttributes != null) {
				EmitContext ec = new EmitContext (this, null, null, ModFlags, false);
				
				if (OptAttributes.AttributeSections != null) {
					foreach (AttributeSection asec in OptAttributes.AttributeSections) {
						if (asec.Attributes != null) {
							foreach (Attribute a in asec.Attributes) {
								CustomAttributeBuilder cb = a.Resolve (ec);
								if (cb != null)
									TypeBuilder.SetCustomAttribute (cb);
							}
						}
					}
				}
			}
			
			if (types != null)
				foreach (TypeContainer tc in types)
					tc.Emit ();
		}
		
		public void CloseType ()
		{
			try {
				TypeBuilder.CreateType ();
			} catch (InvalidOperationException e){
				Console.WriteLine ("Exception while creating class: " + TypeBuilder.Name);
				Console.WriteLine ("Message:" + e.Message);
			}
			
			if (Types != null)
				foreach (TypeContainer tc in Types)
					tc.CloseType ();

			if (Enums != null)
				foreach (Enum en in Enums)
					en.CloseEnum ();
			
			if (Delegates != null)
				foreach (Delegate d in Delegates)
					d.CloseDelegate ();
		}

		string MakeName (string n)
		{
			return "`" + Name + "." + n + "'";
		}
		
		public static int CheckMember (string name, MemberInfo mi, int ModFlags)
		{
			return 0;
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

		public Class (RootContext rc, TypeContainer parent, string name, int mod,
			      Attributes attrs, Location l)
			: base (rc, parent, name, l)
		{
			int accmods;

			if (parent.Parent == null)
				accmods = Modifiers.INTERNAL;
			else
				accmods = Modifiers.PRIVATE;
			
			this.mod_flags = Modifiers.Check (AllowedModifiers, mod, accmods);
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

		public Struct (RootContext rc, TypeContainer parent, string name, int mod,
			       Attributes attrs, Location l)
			: base (rc, parent, name, l)
		{
			int accmods;
			
			if (parent.Parent == null)
				accmods = Modifiers.INTERNAL;
			else
				accmods = Modifiers.PRIVATE;
			
			this.mod_flags = Modifiers.Check (AllowedModifiers, mod, accmods);

			this.mod_flags |= Modifiers.SEALED;
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

	public class MethodCore {
		public readonly Parameters Parameters;
		public readonly string Name;
		public int ModFlags;
		Block block;
		public readonly Location Location;
		
		//
		// Parameters, cached for semantic analysis.
		//
		InternalParameters parameter_info;
		
		public MethodCore (string name, Parameters parameters, Location l)
		{
			Name = name;
			Parameters = parameters;
			Location = l;
		}
		
		//
		//  Returns the System.Type array for the parameters of this method
		//
		Type [] parameter_types;
		public Type [] ParameterTypes (TypeContainer parent)
		{
			if (Parameters == null)
				return null;
			
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
			Modifiers.VIRTUAL |
			Modifiers.SEALED |
			Modifiers.OVERRIDE |
			Modifiers.ABSTRACT |
			Modifiers.EXTERN;
		
		// return_type can be "null" for VOID values.
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

			//
			// FIXME: remove this assumption if we manage to
			// not enter a null as a the Parameters in TypeManager's 
			// MethodBase to Type argument mapper.
			//
			if (args == null){
				if (sigp != null)
					return false;
				else
					return true;
			} else if (sigp == null)
				return false;
			
			if (args.Length != sigp.Length)
				return false;

			for (int i = args.Length; i > 0; ){
				i--;
				if (args [i] != sigp [i])
					return false;
			}
			return true;
		}
		
		// <summary>
		//    This delegate is used to extract methods which have the
		//    same signature as the argument
		// </summary>
		static MemberFilter method_signature_filter;
		
		static Method ()
		{
			method_signature_filter = new MemberFilter (MemberSignatureCompare);
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
				type_return_type = parent.LookupType (ReturnType, false);
			
			return type_return_type;
		}

		void WarningNotHiding (TypeContainer parent)
		{
			Report.Warning (
				109, Location,
				"The member `" + parent.Name + "." + Name + "' does not hide an " +
				"inherited member.  The keyword new is not required");
							   
		}

		string MakeName (TypeContainer parent)
		{
			return "`" + parent.Name + "." + Name + "'";
		}

		string MethodBaseName (MethodBase mb)
		{
			return "`" + mb.ReflectedType.Name + "." + mb.Name + "'";
		}
		
		bool CheckMethod (TypeContainer parent, MemberInfo [] mi)
		{
			MethodInfo mb = (MethodInfo) mi [0];

			if ((ModFlags & (Modifiers.NEW | Modifiers.OVERRIDE)) == 0){
				Report.Warning (
					108, Location, "The keyword new is required on " + 
					MakeName (parent) + " because it hides `" +
					mb.ReflectedType.Name + "." +
					mb.Name + "'");
			}

			if (mb.IsVirtual || mb.IsAbstract){
				if ((ModFlags & (Modifiers.NEW | Modifiers.OVERRIDE)) == 0){
					Report.Warning (
						114, Location, MakeName (parent) + 
						"hides inherited member " + MethodBaseName (mb) +
						".  To make the current member override that " +
						"implementation, add the override keyword, " +
						"otherwise use the new keyword");
				}
				
			}

			return true;
		}
		
		//
		// Creates the type
		// 
		public MethodBuilder Define (TypeContainer parent)
		{
			Type ret_type = GetReturnType (parent);
			Type [] parameters = ParameterTypes (parent);
			MethodAttributes flags;
			bool error = false;
			
			//
			// Verify if the parent has a type with the same name, and then
			// check whether we have to create a new slot for it or not.
			//
			Type ptype = parent.TypeBuilder.BaseType;

			// ptype is only null for System.Object while compiling corlib.
			if (ptype != null){
				MethodSignature ms = new MethodSignature (Name, ret_type, parameters);
				MemberInfo [] mi;
				
				mi = TypeContainer.FindMembers (
					ptype, MemberTypes.Method,
					BindingFlags.Public, method_signature_filter,
					ms);

				if (mi != null && mi.Length > 0){
					CheckMethod (parent, mi);
				} else {
					if ((ModFlags & Modifiers.NEW) != 0)
						WarningNotHiding (parent);
					
					if ((ModFlags & Modifiers.OVERRIDE) != 0)
						Report.Error (115, Location,
							      MakeName (parent) +
							      " no suitable methods found to override");
				}
			} else if ((ModFlags & Modifiers.NEW) != 0)
				WarningNotHiding (parent);

			//
			// If we implement an interface, then set the proper flags.
			//
			flags = Modifiers.MethodAttr (ModFlags);

			if (parent.IsInterfaceMethod (Name, ret_type, parameters))
				flags |= MethodAttributes.Virtual | MethodAttributes.Final |
					 MethodAttributes.NewSlot | MethodAttributes.HideBySig;

			//
			// Catch invalid uses of virtual and abtract modifiers
			//
			const int va = (Modifiers.VIRTUAL | Modifiers.ABSTRACT);
			const int vao = (Modifiers.VIRTUAL | Modifiers.ABSTRACT | Modifiers.OVERRIDE);
			const int nv = (Modifiers.NEW | Modifiers.VIRTUAL);

			if ((ModFlags & va) == va){
				if ((ModFlags & va) == va){
					Report.Error (
						503, Location, "The abstract method " +
						MakeName (parent) + "can not be marked virtual");
					error = true;
				}
			}

			if ((ModFlags & Modifiers.ABSTRACT) != 0){
				if ((parent.ModFlags & Modifiers.ABSTRACT) == 0){
					Report.Error (
						513, Location, MakeName (parent) +
						" is abstract but its container class is not");
					error = true;
				}
			}

			if ((ModFlags & va) != 0 && ((ModFlags & Modifiers.PRIVATE) != 0)){
				Report.Error (
					621, Location, MakeName (parent) +
					" virtual or abstract members can not be private");
				error = true;
			}

			if ((ModFlags & Modifiers.STATIC) != 0){
				if ((ModFlags & vao) != 0){
					Report.Error (
						112, Location, "static method " + MakeName (parent) +
						" can not be marked as virtual, abstract or override");

					error = true;
				}
			}
			
			if ((ModFlags & Modifiers.OVERRIDE) != 0 && ((ModFlags & nv) != 0)){
				Report.Error (
					113, Location, MakeName (parent) +
					"marked as override cannot be marked as new or virtual");
				error = true;
			}

			if (error)
				return null;

			//
			// Finally, define the method
			//
			
			MethodBuilder = parent.TypeBuilder.DefineMethod (
				Name, flags,
				GetCallingConvention (parent is Class),
				ret_type, parameters);

			//
			// HACK because System.Reflection.Emit is lame
			//
			if (!TypeManager.RegisterMethod (MethodBuilder, parameters)) {
				Report.Error (111, Location,
					      "Class `" + parent.Name + "' already contains a definition with the " +
					      "same return value and parameter types for method `" + Name + "'");
				return null;
			}
			
			ParameterInfo = new InternalParameters (parent, Parameters);

			//
			// This is used to track the Entry Point,
			//
			// FIXME: Allow pluggable entry point, check arguments, etc.
			//
			if (Name == "Main"){
				if ((ModFlags & Modifiers.STATIC) != 0){
					parent.RootContext.EntryPoint = MethodBuilder;
				}
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
					Parameter array_param = Parameters.ArrayParameter;
					MethodBuilder.DefineParameter (i + 1, array_param.Attributes,
								       array_param.Name);
				}
			}

			return MethodBuilder;
		}

		//
		// Emits the code
		// 
		public void Emit (TypeContainer parent)
		{
			ILGenerator ig = MethodBuilder.GetILGenerator ();
			EmitContext ec = new EmitContext (parent, ig, GetReturnType (parent), ModFlags);

			if (OptAttributes != null) {
				if (OptAttributes.AttributeSections != null) {
					foreach (AttributeSection asec in OptAttributes.AttributeSections) {
						if (asec.Attributes != null) {
							foreach (Attribute a in asec.Attributes) {
								CustomAttributeBuilder cb = a.Resolve (ec);
								if (cb != null)
									MethodBuilder.SetCustomAttribute (cb);
							}
						}
					}
				}
			}
			
			ec.EmitTopBlock (Block);
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
			
			if (argument_list != null){
				for (int i = argument_list.Count; i > 0; ){
					--i;

					Argument a = (Argument) argument_list [i];
					if (!a.Resolve (ec))
						return false;
				}
			}

			parent_constructor_group = Expression.MemberLookup (
				ec,
				ec.TypeContainer.TypeBuilder.BaseType, ".ctor", true,
				MemberTypes.Constructor,
				BindingFlags.Public | BindingFlags.Instance, location);
			
			if (parent_constructor_group == null){
				Console.WriteLine ("Could not find a constructor in our parent");
				return false;
			}
			
			parent_constructor = (ConstructorInfo) Invocation.OverloadResolve (ec, 
				(MethodGroupExpr) parent_constructor_group, argument_list, location);
			
			if (parent_constructor == null)
				return false;
			
			return true;
		}

		public void Emit (EmitContext ec)
		{
			ec.ig.Emit (OpCodes.Ldarg_0);
			if (argument_list != null)
				Invocation.EmitArguments (ec, argument_list);
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
			return  (Parameters.FixedParameters == null ? true : Parameters.Empty) &&
				(Parameters.ArrayParameter == null ? true : Parameters.Empty) &&
				(Initializer is ConstructorBaseInitializer) &&
				(Initializer.Arguments == null);
		}

		//
		// Creates the ConstructorBuilder
		//
		public ConstructorBuilder Define (TypeContainer parent)
		{
			MethodAttributes ca = (MethodAttributes.RTSpecialName |
					       MethodAttributes.SpecialName);

			Type [] parameters = ParameterTypes (parent);

			if ((ModFlags & Modifiers.STATIC) != 0)
				ca |= MethodAttributes.Static;
			else {
				if (parent is Struct && parameters == null){
					Report.Error (
						568, Location, 
						"Structs can not contain explicit parameterless " +
						"constructors");
					return null;
				}
			}

			ConstructorBuilder = parent.TypeBuilder.DefineConstructor (
				ca, GetCallingConvention (parent is Class),
				parameters);
			//
			// HACK because System.Reflection.Emit is lame
			//
			if (!TypeManager.RegisterMethod (ConstructorBuilder, parameters)) {
				Report.Error (111, Location,
					      "Class `" + parent.Name + "' already contains a definition with the " +
					      "same return value and parameter types for constructor `" + Name + "'");
				return null;
			}
				

			ParameterInfo = new InternalParameters (parent, Parameters);

			return ConstructorBuilder;
		}

		//
		// Emits the code
		//
		public void Emit (TypeContainer parent)
		{
			ILGenerator ig = ConstructorBuilder.GetILGenerator ();
			EmitContext ec = new EmitContext (parent, ig, null, ModFlags, true);

			if (parent is Class){
				if (Initializer == null)
					Initializer = new ConstructorBaseInitializer (null, parent.Location);

				if (!Initializer.Resolve (ec))
					return;
			}

			//
			// Classes can have base initializers and instance field initializers.
			//
			if (parent is Class){
				if ((ModFlags & Modifiers.STATIC) == 0)
					Initializer.Emit (ec);
				parent.EmitFieldInitializers (ec, false);
			}
			
			if ((ModFlags & Modifiers.STATIC) != 0)
				parent.EmitFieldInitializers (ec, true);

			if (OptAttributes != null) {
				if (OptAttributes.AttributeSections != null) {
					foreach (AttributeSection asec in OptAttributes.AttributeSections) {
						if (asec.Attributes != null) {
							foreach (Attribute a in asec.Attributes) {
								CustomAttributeBuilder cb = a.Resolve (ec);
								if (cb != null)
									ConstructorBuilder.SetCustomAttribute (cb);
							}
						}
					}
				}
			}

			ec.EmitTopBlock (Block);
		}
	}
	
	public class Field {
		public readonly string Type;
		public readonly Object Initializer;
		public readonly string Name;
		public readonly int    ModFlags;
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
			Modifiers.READONLY;

		public Field (string type, int mod, string name, Object expr_or_array_init, Attributes attrs)
		{
			Type = type;
			ModFlags = Modifiers.Check (AllowedModifiers, mod, Modifiers.PRIVATE);
			Name = name;
			Initializer = expr_or_array_init;
			OptAttributes = attrs;
		}

		public void Define (TypeContainer parent)
		{
			Type t = parent.LookupType (Type, false);

			if (t == null)
				return;
			
			FieldBuilder = parent.TypeBuilder.DefineField (
				Name, t, Modifiers.FieldAttr (ModFlags));
		}

		public void Emit (TypeContainer tc)
		{
			EmitContext ec = new EmitContext (tc, null, FieldBuilder.FieldType, ModFlags);
			
			if (OptAttributes == null)
				return;

			if (OptAttributes.AttributeSections == null)
				return;
			
			foreach (AttributeSection asec in OptAttributes.AttributeSections) {
				if (asec.Attributes == null)
					continue;
				
				foreach (Attribute a in asec.Attributes) {
					CustomAttributeBuilder cb = a.Resolve (ec);
					if (cb == null)
						continue;
					
					FieldBuilder.SetCustomAttribute (cb);
				}
			}
		}
	}

	public class Property {
		
		public readonly string Type;
		public readonly string Name;
		public readonly int    ModFlags;
		public Block           Get, Set;
		public PropertyBuilder PropertyBuilder;
		public Attributes OptAttributes;
		MethodBuilder GetBuilder, SetBuilder;

		Location Location;

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
		{
			Type = type;
			Name = name;
			ModFlags = Modifiers.Check (AllowedModifiers, mod_flags, Modifiers.PRIVATE);
			Get = get_block;
			Set = set_block;
			OptAttributes = attrs;
			Location = loc;
		}

		public void Define (TypeContainer parent)
		{

			MethodAttributes method_attr = Modifiers.MethodAttr(ModFlags);
					
			// FIXME - PropertyAttributes.HasDefault ?

			PropertyAttributes prop_attr = PropertyAttributes.RTSpecialName |
				                       PropertyAttributes.SpecialName;
		
		
			PropertyType = parent.LookupType (Type, false);
			Type [] parameters = new Type [1];
			parameters [0] = PropertyType;

			PropertyBuilder = parent.TypeBuilder.DefineProperty (
				Name, prop_attr, PropertyType, null);

			if (Get != null)
			{
				GetBuilder = parent.TypeBuilder.DefineMethod (
					"get_" + Name, method_attr, PropertyType, null);
				PropertyBuilder.SetGetMethod (GetBuilder);
				//
				// HACK because System.Reflection.Emit is lame
				//
				if (!TypeManager.RegisterMethod (GetBuilder, null)) {
					Report.Error (111, Location,
					       "Class `" + parent.Name + "' already contains a definition with the " +
					       "same return value and parameter types as the " +
					       "'get' method of property `" + Name + "'");
					return;
				}
				
				
			}
			
			if (Set != null)
			{
				SetBuilder = parent.TypeBuilder.DefineMethod (
					"set_" + Name, method_attr, null, parameters);
				SetBuilder.DefineParameter (1, ParameterAttributes.None, "value"); 
				PropertyBuilder.SetSetMethod (SetBuilder);
				//
				// HACK because System.Reflection.Emit is lame
				//
				if (!TypeManager.RegisterMethod (SetBuilder, parameters)) {
					Report.Error (111, Location,
					       "Class `" + parent.Name + "' already contains a definition with the " +
					       "same return value and parameter types as the " +
					       "'set' method of property `" + Name + "'");
					return;
				}
				
				//
				// HACK for the reasons exposed above
				//
				if (!TypeManager.RegisterProperty (PropertyBuilder, GetBuilder, SetBuilder)) {
					Report.Error (111, Location,
					       "Class `" + parent.Name + "' already contains a definition for the " +
					       " property `" + Name + "'");
					return;
				}
					
			}

		}
		
		public void Emit (TypeContainer tc)
		{
			ILGenerator ig;
			EmitContext ec;

			if (OptAttributes != null) {
				ec = new EmitContext (tc, null, PropertyType, ModFlags);
				if (OptAttributes.AttributeSections != null) {
					foreach (AttributeSection asec in OptAttributes.AttributeSections) {
						if (asec.Attributes != null) {
							foreach (Attribute a in asec.Attributes) {
								CustomAttributeBuilder cb = a.Resolve (ec);
								if (cb != null)
									PropertyBuilder.SetCustomAttribute (cb);
							}
						}
					}
				}
			}
			
			if (Get != null){
				ig = GetBuilder.GetILGenerator ();
				ec = new EmitContext (tc, ig, PropertyType, ModFlags);
				
				ec.EmitTopBlock (Get);
			}

			if (Set != null){
				ig = SetBuilder.GetILGenerator ();
				ec = new EmitContext (tc, ig, null, ModFlags);
				
				ec.EmitTopBlock (Set);
			}
		}
	}

	public class Event {
		
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
		public readonly string    Name;
		public readonly Object    Initializer;
		public readonly int       ModFlags;
		public readonly Block     Add;
		public readonly Block     Remove;
		public EventBuilder       EventBuilder;
		public Attributes         OptAttributes;

		Type EventType;

		Location Location;
		
		public Event (string type, string name, Object init, int flags, Block add_block, Block rem_block,
			      Attributes attrs, Location loc)
		{
			Type = type;
			Name = name;
			Initializer = init;
			ModFlags = Modifiers.Check (AllowedModifiers, flags, Modifiers.PRIVATE);  
			Add = add_block;
			Remove = rem_block;
			OptAttributes = attrs;
			Location = loc;
		}

		public void Define (TypeContainer parent)
		{
			MethodAttributes m_attr = Modifiers.MethodAttr (ModFlags);

			EventAttributes e_attr = EventAttributes.RTSpecialName | EventAttributes.SpecialName;
			
			MethodBuilder mb;

			EventType = parent.LookupType (Type, false);
			Type [] parameters = new Type [1];
			parameters [0] = EventType;
			
			EventBuilder = parent.TypeBuilder.DefineEvent (Name, e_attr, EventType);
			
			if (Add != null) {
				mb = parent.TypeBuilder.DefineMethod ("add_" + Name, m_attr, null,
								      parameters);
				mb.DefineParameter (1, ParameterAttributes.None, "value");
				EventBuilder.SetAddOnMethod (mb);
				//
				// HACK because System.Reflection.Emit is lame
				//
				if (!TypeManager.RegisterMethod (mb, parameters)) {
					Report.Error (111, Location,
					       "Class `" + parent.Name + "' already contains a definition with the " +
					       "same return value and parameter types for the " +
					       "'add' method of event `" + Name + "'");
					return;
				}
			}

			if (Remove != null) {
				mb = parent.TypeBuilder.DefineMethod ("remove_" + Name, m_attr, null,
								      parameters);
				mb.DefineParameter (1, ParameterAttributes.None, "value");
				EventBuilder.SetRemoveOnMethod (mb);

				//
				// HACK because System.Reflection.Emit is lame
				//
				if (!TypeManager.RegisterMethod (mb, parameters)) {
					Report.Error (111, Location,	
				       "Class `" + parent.Name + "' already contains a definition with the " +
					       "same return value and parameter types for the " +
					       "'remove' method of event `" + Name + "'");
					return;
				}
			}

		
		}

		public void Emit (TypeContainer tc)
		{
			EmitContext ec = new EmitContext (tc, null, EventType, ModFlags);

			if (OptAttributes != null) {
				if (OptAttributes.AttributeSections != null) {
					foreach (AttributeSection asec in OptAttributes.AttributeSections) {
						if (asec.Attributes != null) {
							foreach (Attribute a in asec.Attributes) {
								CustomAttributeBuilder cb = a.Resolve (ec);
								if (cb != null)
									EventBuilder.SetCustomAttribute (cb);
							}
						}
					}
				}
			}
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
 
	public class Indexer {

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
		public readonly int        ModFlags;
		public readonly Block      Get;
		public readonly Block      Set;
		public Attributes          OptAttributes;
		public MethodBuilder       GetBuilder;
		public MethodBuilder       SetBuilder;
		public PropertyBuilder PropertyBuilder;
	        public Type IndexerType;

		Location Location;
			
		public Indexer (string type, string int_type, int flags, Parameters parms,
				Block get_block, Block set_block, Attributes attrs, Location loc)
		{

			Type = type;
			InterfaceType = int_type;
			ModFlags = Modifiers.Check (AllowedModifiers, flags, Modifiers.PRIVATE);
			FormalParameters = parms;
			Get = get_block;
			Set = set_block;
			OptAttributes = attrs;
			Location = loc;
		}

		public void Define (TypeContainer parent)
		{
			MethodAttributes attr = Modifiers.MethodAttr (ModFlags);
			PropertyAttributes prop_attr =
				PropertyAttributes.RTSpecialName |
				PropertyAttributes.SpecialName;
			
			IndexerType = parent.LookupType (Type, false);
			Type [] parameters = FormalParameters.GetParameterInfo (parent);

			PropertyBuilder = parent.TypeBuilder.DefineProperty (
				TypeManager.IndexerPropertyName (parent.TypeBuilder),
				prop_attr, IndexerType, parameters);
				
			if (Get != null){
				GetBuilder = parent.TypeBuilder.DefineMethod (
					"get_Item", attr, IndexerType, parameters);

				if (!TypeManager.RegisterMethod (GetBuilder, parameters)) {
					Report.Error (111, Location,
					       "Class `" + parent.Name + "' already contains a definition with the " +
					       "same return value and parameter types for the " +
					       "'get' indexer");
					return;
				}
					
				TypeContainer.RegisterParameterForBuilder (
					      GetBuilder, new InternalParameters (parent, FormalParameters));
			}
			
			if (Set != null){
				int top = parameters.Length;
				Type [] set_pars = new Type [top + 1];
				parameters.CopyTo (set_pars, 0);
				set_pars [top] = IndexerType;

				Parameter [] fixed_parms = FormalParameters.FixedParameters;

				Parameter [] tmp = new Parameter [fixed_parms.Length + 1];

				fixed_parms.CopyTo (tmp, 0);
				tmp [fixed_parms.Length] = new Parameter (Type, "value", Parameter.Modifier.NONE, null);

				Parameters set_formal_params = new Parameters (tmp, null);
				
				SetBuilder = parent.TypeBuilder.DefineMethod (
					"set_Item", attr, null, set_pars);

				if (!TypeManager.RegisterMethod (SetBuilder, set_pars)) {
					Report.Error (111, Location,
					       "Class `" + parent.Name + "' already contains a definition with the " +
					       "same return value and parameter types for the " +
					       "'set' indexer");
					return;
				}

				TypeContainer.RegisterParameterForBuilder (
					SetBuilder, new InternalParameters (parent, set_formal_params));
			}

			PropertyBuilder.SetGetMethod (GetBuilder);
			PropertyBuilder.SetSetMethod (SetBuilder);
			
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
		}

		public void Emit (TypeContainer tc)
		{
			ILGenerator ig;
			EmitContext ec;

			if (OptAttributes != null) {
				ec = new EmitContext (tc, null, IndexerType, ModFlags);
				if (OptAttributes.AttributeSections != null) {
					foreach (AttributeSection asec in OptAttributes.AttributeSections) {
						if (asec.Attributes != null) {
							foreach (Attribute a in asec.Attributes) {
								CustomAttributeBuilder cb = a.Resolve (ec);
								if (cb != null)
									PropertyBuilder.SetCustomAttribute (cb);
							}
						}
					}
				}
			}

			if (Get != null){
				ig = GetBuilder.GetILGenerator ();
				ec = new EmitContext (tc, ig, IndexerType, ModFlags);
				
				ec.EmitTopBlock (Get);
			}

			if (Set != null){
				ig = SetBuilder.GetILGenerator ();
				ec = new EmitContext (tc, ig, null, ModFlags);
				
				ec.EmitTopBlock (Set);
			}
		}
	}

	public class Operator {

		const int AllowedModifiers =
			Modifiers.PUBLIC |
			Modifiers.STATIC;

		const int RequiredModifiers =
			Modifiers.PUBLIC |
			Modifiers.STATIC;

		public enum OpType {

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
		public readonly int    ModFlags;
		public readonly Block  Block;
		public Attributes      OptAttributes;
		public MethodBuilder   OperatorMethodBuilder;
		public Location        Location;
		
		public string MethodName;
		public Method OperatorMethod;

		public Operator (OpType type, string ret_type, int flags, string arg1type, string arg1name,
				 string arg2type, string arg2name, Block block, Attributes attrs, Location loc)
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
			Location = loc;
		}

		string Prototype (TypeContainer parent)
		{
			return parent.Name + ".operator " + OperatorType + " (" + FirstArgType + "," +
				SecondArgType + ")";
		}
		
		public void Define (TypeContainer parent)
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
			}

			param_list[0] = new Parameter (FirstArgType, FirstArgName,
						       Parameter.Modifier.NONE, null);
			if (SecondArgType != null)
				param_list[1] = new Parameter (SecondArgType, SecondArgName,
							       Parameter.Modifier.NONE, null);
			
			OperatorMethod = new Method (ReturnType, ModFlags, MethodName,
						     new Parameters (param_list, null),
						     OptAttributes, Location.Null);
			
			OperatorMethod.Define (parent);
			OperatorMethodBuilder = OperatorMethod.MethodBuilder;

			Type [] param_types = OperatorMethod.ParameterTypes (parent);
			Type declaring_type = OperatorMethodBuilder.DeclaringType;
			Type return_type = OperatorMethod.GetReturnType (parent);
			Type first_arg_type = param_types [0];

			// Rules for conversion operators
			
			if (OperatorType == OpType.Implicit || OperatorType == OpType.Explicit) {
				
				if (first_arg_type == return_type && first_arg_type == declaring_type)
					Report.Error (555, Location,
					       "User-defined conversion cannot take an object of the enclosing type " +
					       "and convert to an object of the enclosing type");
				
				if (first_arg_type != declaring_type && return_type != declaring_type)
					Report.Error (556, Location, 
					       "User-defined conversion must convert to or from the enclosing type");
				
				if (first_arg_type == TypeManager.object_type || return_type == TypeManager.object_type)
					Report.Error (-8, Location,
					       "User-defined conversion cannot convert to or from object type");
				
				if (first_arg_type.IsInterface || return_type.IsInterface)
					Report.Error (-9, Location,
					       "User-defined conversion cannot convert to or from an interface type");	 
				
				if (first_arg_type.IsSubclassOf (return_type) || return_type.IsSubclassOf (first_arg_type))
					Report.Error (-10, Location,
						"User-defined conversion cannot convert between types that " +
						"derive from each other"); 
				
			} else if (SecondArgType == null) {
				// Checks for Unary operators
				
				if (first_arg_type != declaring_type) 
					Report.Error (562, Location,
						   "The parameter of a unary operator must be the containing type");
				
				
				if (OperatorType == OpType.Increment || OperatorType == OpType.Decrement) {
					if (return_type != declaring_type)
						Report.Error (559, Location,
						       "The parameter and return type for ++ and -- " +
						       "must be the containing type");
					
				}
				
				if (OperatorType == OpType.True || OperatorType == OpType.False) {
					if (return_type != TypeManager.bool_type)
						Report.Error (215, Location,
						       "The return type of operator True or False " +
						       "must be bool");
				}
				
			} else {
				// Checks for Binary operators
				
				if (first_arg_type != declaring_type &&
				    param_types [1] != declaring_type)
					Report.Error (563, Location,
					       "One of the parameters of a binary operator must be the containing type");
			}
			
		
			
		}
		
		public void Emit (TypeContainer parent)
		{
			if (OptAttributes != null) {
				EmitContext ec = new EmitContext (parent, null, null, ModFlags);
				if (OptAttributes.AttributeSections != null) {
					foreach (AttributeSection asec in OptAttributes.AttributeSections) {
						if (asec.Attributes != null) {
							foreach (Attribute a in asec.Attributes) {
								CustomAttributeBuilder cb = a.Resolve (ec);
								if (cb != null)
									OperatorMethodBuilder.SetCustomAttribute (cb);
							}
						}
					}
				}
			}
			
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

			if (parameters != null){
				if (parameters.Length == 0)
					Parameters = null;
			} 
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
