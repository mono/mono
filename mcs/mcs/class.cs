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
		readonly public RootContext RootContext;

		// Attributes for this type
		protected ArrayList attributes;
		
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

		public ArrayList OptAttributes {
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
		// The Toplevel is `root_types' which is a containerfor all
		// types defined, hence the non-obviios parent.parent.
		//
		// If we were not tracking Namespaces we could remove this.
		//
		bool IsTopLevel {
			get {
				if (parent != null){
					if (parent.parent == null)
						return true;
				}

				return false;
			}
		}

		// <summary>
		//   Returns the TypeAttributes for this TypeContainer
		// </summary>
		public virtual TypeAttributes TypeAttr {
			get {
				TypeAttributes x = 0;

				//
				// FIXME: Figure out exactly how private, public and protected
				// map to the TypeAttribute flags.
				//
				// FIXME: Figure out what `new' in the context of a class/struct means.
				//
				// FIXME: figure out what `internal' means in the context of class/structs
				//
				if ((mod_flags & Modifiers.PUBLIC) != 0)
					x |= TypeAttributes.Public;

				if ((mod_flags & Modifiers.PRIVATE) != 0)
					x |= TypeAttributes.NotPublic;
				
				if ((mod_flags & Modifiers.ABSTRACT) != 0)
					x |= TypeAttributes.Abstract;
				
				if ((mod_flags & Modifiers.SEALED) != 0)
					x |= TypeAttributes.Sealed;

				if (!IsTopLevel){
					if ((mod_flags & Modifiers.PUBLIC) != 0)
						x |= TypeAttributes.NestedPublic;
					else
						x |= TypeAttributes.NestedPrivate;
				}

				//
				// If we have static constructors, the runtime needs to
				// initialize the class, otherwise we can optimize
				// the case.
				//
				if (!have_static_constructor)
					x |= TypeAttributes.BeforeFieldInit;
				return x;
			}
		}

		void EmitField (Field f)
		{
			Type t = LookupType (f.Type, false);

			if (t == null)
				return;
			
			TypeBuilder.DefineField (f.Name, t, Modifiers.FieldAttr (f.ModFlags));
		}

		//
		// Emits the class field initializers
		//
		void EmitStaticFieldInitializers (ConstructorBuilder cb)
		{
			// FIXME: Implement
		}

		//
		// Emits the instance field initializers
		//
		void EmitFieldInitializers (ConstructorBuilder cb)
		{
			// FIXME: Implement
		}

		//
		// Emits a constructor
		//
		void EmitConstructor (Constructor c)
		{
			if ((c.ModFlags & Modifiers.STATIC) != 0){
				if (initialized_static_fields != null)
					EmitStaticFieldInitializers (c.ConstructorBuilder);
			} else {
				if (initialized_fields != null)
					EmitFieldInitializers (c.ConstructorBuilder);
			}

		}

		//
		// This function is used to emit instance and static constructors
		// when the user did not provide one.
		// 
		void EmitDefaultConstructor (bool is_static)
		{
			ConstructorBuilder cb;
			MethodAttributes ca = (MethodAttributes.RTSpecialName |
					       MethodAttributes.SpecialName);

			if (is_static)
				ca |= MethodAttributes.Static;
			
			//
			// Default constructors provided by the compiler should be `protected'
			// if the class is abstract, otherwise it is public
			//
			if ((mod_flags & Modifiers.ABSTRACT) != 0)
				ca |= MethodAttributes.Family;
			else
				ca |= MethodAttributes.Public;
			
			cb = TypeBuilder.DefineDefaultConstructor (ca);

			if (is_static)
				EmitStaticFieldInitializers (cb);
			else
				EmitFieldInitializers (cb);
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
					EmitField (f);
			}

			if (Constructors != null){
				foreach (Constructor c in Constructors)
					c.Define (this);
			}

			if (Methods != null){
				foreach (Method m in Methods)
					m.Define (this);
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
		}

		//
		// Emits the code, this step is performed after all
		// the types, enumerations, constructors
		//
		public void Emit ()
		{
			if (default_constructor == null)
				EmitDefaultConstructor (false);

			if (initialized_static_fields != null && default_static_constructor == null)
				EmitDefaultConstructor (true);

			if (Constructors != null)
				foreach (Constructor c in Constructors)
					c.Emit ();
			
			if (Methods != null)
				foreach (Method m in Methods)
					m.Emit (this);
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

		public Class (RootContext rc, TypeContainer parent, string name, int mod, ArrayList attrs)
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

		public Struct (RootContext rc, TypeContainer parent, string name, int mod, ArrayList attrs)
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

	public class Method {
		public readonly Parameters Parameters;
		public readonly string     ReturnType;
		public readonly string     Name;
		public readonly int        ModFlags;
		public MethodBuilder MethodBuilder;
		public readonly ArrayList OptAttributes;
		
		Block block;
		
		// return_type can be "null" for VOID values.
		public Method (string return_type, int mod, string name, Parameters parameters, ArrayList attrs)
		{
			Name = name;
			ReturnType = return_type;
			Parameters = parameters;
			ModFlags = Modifiers.Check (AllowedModifiers, mod, Modifiers.PRIVATE);
			OptAttributes = attrs;
		}

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

		public Block Block {
			get {
				return block;
			}

			set {
				block = value;
			}
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

		public CallingConventions GetCallingConvention (bool is_class)
		{
			CallingConventions cc = 0;
			
			cc = Parameters.GetCallingConvention ();

			if (is_class)
				if ((ModFlags & Modifiers.STATIC) == 0)
					cc |= CallingConventions.HasThis;

			return cc;
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
			
			ec.EmitTopBlock (block);
		}
	}

	public class Field {
		public readonly string Type;
		public readonly Object Initializer;
		public readonly string Name;
		public readonly int    ModFlags;
		public readonly ArrayList OptAttributes;
		
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

		public Field (string type, int mod, string name, Object expr_or_array_init, ArrayList attrs)
		{
			Type = type;
			ModFlags = Modifiers.Check (AllowedModifiers, mod, Modifiers.PRIVATE);
			Name = name;
			Initializer = expr_or_array_init;
			OptAttributes = attrs;
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
	
	public class Constructor {
		public ConstructorBuilder ConstructorBuilder;
		public readonly ConstructorInitializer Initializer;
		public readonly Parameters Parameters;
		public readonly string Name;
		Block block;
		int mod_flags;

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
		{
			Name = name;
			Parameters = args;
			Initializer = init;
		}

		//
		// Returns true if this is a default constructor
		//
		public bool IsDefault ()
		{
			return  (Parameters == null) &&
				(Initializer is ConstructorBaseInitializer) &&
				(Initializer.Arguments == null);
		}

		public int ModFlags {
			get {
				return mod_flags;
			}

			set {
				mod_flags = value;
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

		public CallingConventions GetCallingConvention (bool parent_is_class)
		{
			CallingConventions cc = 0;
			
			if (Parameters.ArrayParameter != null)
				cc |= CallingConventions.VarArgs;
			else
				cc |= CallingConventions.Standard;

			if (parent_is_class)
				if ((ModFlags & Modifiers.STATIC) != 0)
					cc |= CallingConventions.HasThis;

				// FIXME: How is `ExplicitThis' used in C#?
				
			return cc;
		}

		//
		// Cached representation
		///
		Type [] parameter_types;
		public Type [] ParameterTypes (TypeContainer tc)
		{
			if (Parameters == null)
				return null;
			
			if (parameter_types == null)
				parameter_types = Parameters.GetParameterInfo (tc);
			
			return parameter_types;
		}

		//
		// Creates the ConstructorBuilder
		//
		public void Define (TypeContainer parent)
		{
			MethodAttributes ca = (MethodAttributes.RTSpecialName |
					       MethodAttributes.SpecialName);
			
			if ((ModFlags & Modifiers.STATIC) != 0)
				ca |= MethodAttributes.Static;
			
			ConstructorBuilder = parent.TypeBuilder.DefineConstructor (
				ca, GetCallingConvention (parent is Class),
				ParameterTypes (parent));
		}

		//
		// Emits the code
		//
		public void Emit ()
		{
		}
	}

	public class Property {
		
		public readonly string Type;
		public readonly string Name;
		public readonly int    ModFlags;
		public Block           Get, Set;
		public PropertyBuilder PropertyBuilder;
		public readonly ArrayList OptAttributes;
		
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
		
		public Property (string type, string name, int mod_flags, Block get_block, Block set_block, ArrayList attrs)
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
					
			// FIXME - Right now using Modifiers.MethodAttributes
			// Guess we need to add a special method for ProprtyAttributes
			// in modifiers.cs - when we get final comments from miguel
			// abt why access modifiers like new internal etc are not handled
			// in modifiers.cs

			// FIXME - how to handle PropertyAttributes.HasDefault

			PropertyAttributes prop_attr = PropertyAttributes.RTSpecialName |
				                       PropertyAttributes.SpecialName;
		
		
			Type tp = System.Type.GetType(Type);
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

		public Event (string type, string name, Object init, int flags, Block add_block, Block rem_block)
		{
			Type = type;
			Name = name;
			Initializer = init;
			ModFlags = Modifiers.Check (AllowedModifiers, flags, Modifiers.PRIVATE);  
			Add = add_block;
			Remove = rem_block;
		}

		public void Define (TypeContainer parent)
		{
			MethodAttributes m_attr = Modifiers.MethodAttr (ModFlags);

			EventAttributes e_attr = EventAttributes.RTSpecialName | EventAttributes.SpecialName;
			
			MethodBuilder mb;

			Type t = System.Type.GetType (Type);
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

		public Indexer (string type, string int_type, int flags, Parameters parms,
				Block get_block, Block set_block)
		{

			Type = type;
			InterfaceType = int_type;
			ModFlags = Modifiers.Check (AllowedModifiers, flags, Modifiers.PRIVATE);
			FormalParameters = parms;
			Get = get_block;
			Set = set_block;
		}
	}

	public class Operator {

		const int AllowedModifiers =
			Modifiers.PUBLIC |
			Modifiers.STATIC;

		public enum OpType {

			// Unary operators
			Bang,
			Tilde,
			Increment,
			Decrement,
			True,
			False,

			// Unary and Binary operators
			Plus,
			Minus,
			
			// Binary operators
			Star,
			Div,
			Percent,
			BitAnd,
			BitOr,
			Carret,
			ShiftLeft,
			ShiftRight,
			Eq,
			NotEq,
			GreaterThan,
			LesserThan,
			GreaterOrEq,
			LesserOrEq,

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

		public Operator (OpType type, string ret_type, int flags, string arg1type, string arg1name,
				 string arg2type, string arg2name, Block block)
		{
			OperatorType = type;
			ReturnType = ret_type;
			ModFlags = Modifiers.Check (AllowedModifiers, flags, Modifiers.PUBLIC);
			FirstArgType = arg1type;
			FirstArgName = arg1name;
			SecondArgType = arg2type;
			SecondArgName = arg2name;
			Block = block;
		}
			      
		public static void CheckUnaryOperator (OpType op)
		{
			switch (op) {

			case OpType.Bang : case OpType.Tilde : case OpType.Increment : case OpType.Decrement :
			case OpType.True : case OpType.False : case OpType.Plus : case OpType.Minus :

				break;
				
			default :
				// We have an error
				
				CSharpParser.error (1019, "Overloadable unary operator expected"); 
				break;
				
			}
		}

		public static void CheckBinaryOperator (OpType op)
		{
			switch (op) {

			case OpType.Plus : case OpType.Minus : case OpType.Star : case OpType.Div :
			case OpType.Percent : case OpType.BitAnd : case OpType.BitOr : case OpType.Carret :
			case OpType.ShiftLeft : case OpType.ShiftRight : case OpType.Eq : case OpType.NotEq :
			case OpType.GreaterThan : case OpType.LesserThan : case OpType.GreaterOrEq :
			case OpType.LesserOrEq :

				break;

			default :

				CSharpParser.error (1020, "Overloadable binary operator expected");
				break;
			}

		}

	}

}

