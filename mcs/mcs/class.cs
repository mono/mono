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
		static Hashtable method_builders_to_methods;
		
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

		public TypeContainer (RootContext rc, TypeContainer parent, string name) : base (name)
		{
			string n;
			types = new ArrayList ();
			this.parent = parent;
			RootContext = rc;

			object a = rc.Report;
			
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

			Console.WriteLine ("Found a constructor for " + Name);
			if (c.IsDefault ()){
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
		// Emits the class field initializers
		//
		public void EmitStaticFieldInitializers (ConstructorBuilder cb)
		{
			if (initialized_static_fields == null)
				return;
			
			// FIXME: Implement
		}

		//
		// Emits the instance field initializers
		//
		public void EmitFieldInitializers (ConstructorBuilder cb)
		{
			if (initialized_fields == null)
				return;
			
			// FIXME: Implement
		}

		//
		// Defines the default constructors
		//
		void DefineDefaultConstructor (bool is_static)
		{
			Constructor c;
			int mods = 0;

			c = new Constructor (Name, new Parameters (null, null),
					     new ConstructorBaseInitializer (null));
			AddConstructor (c);
			c.Block = new Block (null);
			
			if (is_static)
				mods = Modifiers.STATIC;

			c.ModFlags = mods;	
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

			if (default_constructor == null)
				DefineDefaultConstructor (false);

			if (initialized_static_fields != null && default_static_constructor == null)
				DefineDefaultConstructor (true);

			
			if (Constructors != null){
				if (method_builders_to_methods == null)
					method_builders_to_methods = new Hashtable ();
				
				foreach (Constructor c in Constructors){
					c.Define (this);
					method_builders_to_methods.Add (c.ConstructorBuilder, c);
				}
			} 

			if (Methods != null){
				if (method_builders_to_methods == null)
					method_builders_to_methods = new Hashtable ();
				
				foreach (Method m in Methods){
					m.Define (this);
					method_builders_to_methods.Add (m.MethodBuilder, m);
				}
			}

			if (Properties != null) {
				foreach (Property p in Properties)
					p.Define (this);
			}

			if (Enums != null) {
				foreach (Enum e in Enums)
					e.Define (this);
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
				foreach (Operator o in Operators)
					o.Define (this);
			}

			if (Delegates != null) {
				foreach (Delegate d in Delegates)
					d.Define (this);
			}
			
			
		}

		static public MethodCore LookupMethodByBuilder (object mb)
		{
			return (MethodCore) method_builders_to_methods [mb];
		}
		
		//
		// Emits the code, this step is performed after all
		// the types, enumerations, constructors
		//
		public void Emit ()
		{
			if (Constructors != null)
				foreach (Constructor c in Constructors)
					c.Emit (this);
			
			if (Methods != null)
				foreach (Method m in Methods)
					m.Emit (this);

			if (Operators != null)
				foreach (Operator o in Operators)
					o.Emit (this);
		}
		
		public delegate void ExamineType (TypeContainer container, object cback_data);

		void WalkTypesAt (TypeContainer root, ExamineType visit, object cback_data)
		{
			if (root == null)
				return;

			foreach (TypeContainer type in root.Types){
				visit (type, cback_data);
				WalkTypesAt (type, visit, cback_data);
			}
		}

		public void WalkTypes (ExamineType visit, object cback)
		{
			WalkTypesAt (this, visit, cback);
		}

		public Type LookupType (string name, bool silent)
		{
			return RootContext.LookupType (this, name, silent);
		}

		bool AlwaysAccept (MemberInfo m, object filterCriteria)
		{
			return true;
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
				filter = new MemberFilter (AlwaysAccept);
			
			if ((mt & MemberTypes.Field) != 0 && Fields != null) {
				foreach (Field f in Fields) {
					if (filter (f.FieldBuilder, criteria) == true)
						members.Add (f.FieldBuilder);
				}
			}
			
			if ((mt & MemberTypes.Method) != 0 && Methods != null) {
				foreach (Method m in Methods) {
					if (filter (m.MethodBuilder, criteria) == true)
						members.Add (m.MethodBuilder);
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

			if ((mt & MemberTypes.Property) != 0 && Properties != null) {
				foreach (Property p in Properties) {
					if (filter (p.PropertyBuilder, criteria) == true)
						members.Add (p.PropertyBuilder);
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
					foreach (Constructor c in Constructors)
						if (filter (c.ConstructorBuilder, criteria) == true)
							members.Add (c.ConstructorBuilder);
				}
			}

			MemberInfo [] mi = new MemberInfo [members.Count];
			members.CopyTo (mi);

			return mi;
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

		public Class (RootContext rc, TypeContainer parent, string name, int mod, Attributes attrs)
			: base (rc, parent, name)
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
				return base.TypeAttr | TypeAttributes.AutoLayout;
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

		public Struct (RootContext rc, TypeContainer parent, string name, int mod, Attributes attrs)
			: base (rc, parent, name)
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
		
		//
		// Parameters, cached for semantic analysis.
		//
		InternalParameters parameter_info;
		
		public MethodCore (string name, Parameters parameters)
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
		public readonly string     ReturnType;
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
			       Attributes attrs)
			: base (name, parameters)
		{
			ReturnType = return_type;
			ModFlags = Modifiers.Check (AllowedModifiers, mod, Modifiers.PRIVATE);
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
				type_return_type = parent.LookupType (ReturnType, false);
			
			return type_return_type;
		}

		//
		// Creates the type
		// 
		public void Define (TypeContainer parent)
		{
			Type ret_type = GetReturnType (parent);
			Type [] parameters = ParameterTypes (parent);

			//
			// Create the method
			//
			MethodBuilder = parent.TypeBuilder.DefineMethod (
				Name, Modifiers.MethodAttr (ModFlags),
				GetCallingConvention (parent is Class),
				ret_type, parameters);

			ParameterInfo = new InternalParameters (parameters);

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

				if (i != parameters.Length)
					Console.WriteLine ("Implement the type definition for params");
			}
		}

		//
		// Emits the code
		// 
		public void Emit (TypeContainer parent)
		{
			ILGenerator ig = MethodBuilder.GetILGenerator ();
			EmitContext ec = new EmitContext (parent, ig);
			
			ec.EmitTopBlock (Block);
		}
	}

	public abstract class ConstructorInitializer {
		ArrayList argument_list;

		public ConstructorInitializer (ArrayList argument_list)
		{
			this.argument_list = argument_list;
		}

		public ArrayList Arguments {
			get {
				return argument_list;
			}
		}
	}

	public class ConstructorBaseInitializer : ConstructorInitializer {
		public ConstructorBaseInitializer (ArrayList argument_list) : base (argument_list)
		{
		}
	}

	public class ConstructorThisInitializer : ConstructorInitializer {
		public ConstructorThisInitializer (ArrayList argument_list) : base (argument_list)
		{
		}
	}
	
	public class Constructor : MethodCore {
		public ConstructorBuilder ConstructorBuilder;
		public readonly ConstructorInitializer Initializer;

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
		public Constructor (string name, Parameters args, ConstructorInitializer init)
			: base (name, args)
		{
			Initializer = init;
		}

		//
		// Returns true if this is a default constructor
		//
		public bool IsDefault ()
		{
			return  (Parameters == null ? true : Parameters.Empty) &&
				(Initializer is ConstructorBaseInitializer) &&
				(Initializer.Arguments == null);
		}

		//
		// Creates the ConstructorBuilder
		//
		public void Define (TypeContainer parent)
		{
			MethodAttributes ca = (MethodAttributes.RTSpecialName |
					       MethodAttributes.SpecialName);
			Type [] parameters = ParameterTypes (parent);
			
			if ((ModFlags & Modifiers.STATIC) != 0)
				ca |= MethodAttributes.Static;
			
			ConstructorBuilder = parent.TypeBuilder.DefineConstructor (
				ca, GetCallingConvention (parent is Class),
				parameters);

			ParameterInfo = new InternalParameters (parameters);
		}

		//
		// Emits the code
		//
		public void Emit (TypeContainer parent)
		{
			
			if ((ModFlags & Modifiers.STATIC) != 0) 
				parent.EmitStaticFieldInitializers (this.ConstructorBuilder);
			else 
				parent.EmitFieldInitializers (this.ConstructorBuilder);

			ILGenerator ig = ConstructorBuilder.GetILGenerator ();
			EmitContext ec = new EmitContext (parent, ig);

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
	}

	public class Property {
		
		public readonly string Type;
		public readonly string Name;
		public readonly int    ModFlags;
		public Block           Get, Set;
		public PropertyBuilder PropertyBuilder;
		public Attributes OptAttributes;
		
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
		
		public Property (string type, string name, int mod_flags, Block get_block, Block set_block, Attributes attrs)
		{
			Type = type;
			Name = name;
			ModFlags = Modifiers.Check (AllowedModifiers, mod_flags, Modifiers.PRIVATE);
			Get = get_block;
			Set = set_block;
			OptAttributes = attrs;
		}

		public void Define (TypeContainer parent)
		{

			MethodAttributes method_attr = Modifiers.MethodAttr(ModFlags);
					
			// FIXME - PropertyAttributes.HasDefault ?

			PropertyAttributes prop_attr = PropertyAttributes.RTSpecialName |
				                       PropertyAttributes.SpecialName;
		
		
			Type tp = parent.LookupType (Type, false);
			Type [] prop_type = new Type [1];
			prop_type [0] = tp;

			MethodBuilder mb;
			
			PropertyBuilder = parent.TypeBuilder.DefineProperty(Name, prop_attr, tp, null);
					
			if (Get != null)
			{
				mb = parent.TypeBuilder.DefineMethod("get_" + Name, method_attr, tp, null);
				PropertyBuilder.SetGetMethod (mb);
			}
			
			if (Set != null)
			{
				mb = parent.TypeBuilder.DefineMethod("set_" + Name, method_attr, null, prop_type);
				mb.DefineParameter(1, ParameterAttributes.None, "value"); 
				PropertyBuilder.SetSetMethod (mb);
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
		
		public Event (string type, string name, Object init, int flags, Block add_block, Block rem_block,
			      Attributes attrs)
		{
			Type = type;
			Name = name;
			Initializer = init;
			ModFlags = Modifiers.Check (AllowedModifiers, flags, Modifiers.PRIVATE);  
			Add = add_block;
			Remove = rem_block;
			OptAttributes = attrs;
		}

		public void Define (TypeContainer parent)
		{
			MethodAttributes m_attr = Modifiers.MethodAttr (ModFlags);

			EventAttributes e_attr = EventAttributes.RTSpecialName | EventAttributes.SpecialName;
			
			MethodBuilder mb;

			Type t = parent.LookupType (Type, false);
			Type [] p_type = new Type [1];
			p_type [0] = t;
			
			EventBuilder = parent.TypeBuilder.DefineEvent (Name, e_attr, t);
			
			if (Add != null) {
				mb = parent.TypeBuilder.DefineMethod ("add_" + Name, m_attr, null, p_type);
				mb.DefineParameter (1, ParameterAttributes.None, "value");
				EventBuilder.SetAddOnMethod (mb);
			}

			if (Remove != null) {
				mb = parent.TypeBuilder.DefineMethod ("remove_" + Name, m_attr, null, p_type);
				mb.DefineParameter (1, ParameterAttributes.None, "value");
				EventBuilder.SetRemoveOnMethod (mb);
			}
		}
		
	}

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
		public MethodBuilder GetMethodBuilder;
		public MethodBuilder SetMethodBuilder;
		

		public Indexer (string type, string int_type, int flags, Parameters parms,
				Block get_block, Block set_block, Attributes attrs)
		{

			Type = type;
			InterfaceType = int_type;
			ModFlags = Modifiers.Check (AllowedModifiers, flags, Modifiers.PRIVATE);
			FormalParameters = parms;
			Get = get_block;
			Set = set_block;
			OptAttributes = attrs;
		}

		public void Define (TypeContainer parent)
		{
			MethodAttributes attr = Modifiers.MethodAttr (ModFlags);
			
			Type ret_type = parent.LookupType (Type, false);
			Type [] param_types = FormalParameters.GetParameterInfo (parent);

			GetMethodBuilder = parent.TypeBuilder.DefineMethod ("get_Item", attr, ret_type, param_types);
			SetMethodBuilder = parent.TypeBuilder.DefineMethod ("set_Item", attr, ret_type, param_types);
			
			Parameter [] p = FormalParameters.FixedParameters;

			if (p != null) {
				int i;
				
				for (i = 0; i < p.Length; ++i) {
					GetMethodBuilder.DefineParameter (i + 1, p [i].Attributes, p [i].Name);
					SetMethodBuilder.DefineParameter (i + 1, p [i].Attributes, p [i].Name);
				}
				
				if (i != param_types.Length)
					Console.WriteLine ("Implement type definition for params");
			}

		}
		
	}

	public class Operator {

		const int AllowedModifiers =
			Modifiers.PUBLIC |
			Modifiers.STATIC;

		public enum OpType {

			// Unary operators
			Negate,
			BitComplement,
			Increment,
			Decrement,
			True,
			False,

			// Unary and Binary operators
			Plus,
			Minus,
			
			// Binary operators
			Multiply,
			Divide,
			Modulo,
			BitwiseAnd,
			BitwiseOr,
			ExclusiveOr,
			ShiftLeft,
			ShiftRight,
			Equal,
			NotEqual,
			GreaterThan,
			LessThan,
			GreaterOrEqual,
			LesserOrEqual,

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

		public Operator (OpType type, string ret_type, int flags, string arg1type, string arg1name,
				 string arg2type, string arg2name, Block block, Attributes attrs)
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

		public void Define (TypeContainer parent)
		{
			MethodAttributes attr = Modifiers.MethodAttr (ModFlags);

			string name = "Operator" + OperatorType;

			Type ret_type = parent.LookupType (ReturnType, false);

			Type [] param_types = new Type [2];

			param_types [0] = parent.LookupType (FirstArgType, false);
			if (SecondArgType != null)
				param_types [1] = parent.LookupType (SecondArgType, false);
			
			OperatorMethodBuilder = parent.TypeBuilder.DefineMethod (name, attr, ret_type, param_types);

			OperatorMethodBuilder.DefineParameter (1, ParameterAttributes.None, FirstArgName);

			if (SecondArgType != null)
				OperatorMethodBuilder.DefineParameter (2, ParameterAttributes.None, SecondArgName);

		}

		public void Emit (TypeContainer parent)
		{
			ILGenerator ig = OperatorMethodBuilder.GetILGenerator ();
			EmitContext ec = new EmitContext (parent, ig);

			ec.EmitTopBlock (Block);
		}
		

	}

}
