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

		// Holds the method groups.
		Hashtable method_groups;

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

		public AdditionResult AddMethod (Method method)
		{
			string name = method.Name;
			Object value = defined_names [name];
			
			if (value != null && (!(value is MethodGroup)))
				return AdditionResult.NameExists;

			if (method_groups == null)
				method_groups = new Hashtable ();

			MethodGroup mg = (MethodGroup) method_groups [name];
			if (mg == null){
				mg = new MethodGroup (name);

				mg.Add (method);
				method_groups.Add (name, mg);

				return AdditionResult.Success;
			}
			mg.Add (method);

			if (value == null)
				DefineName (name, mg);
			
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

		public Hashtable MethodGroups {
			get {
				return method_groups;
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
				return null; // constructors;
			}
		}

		public ArrayList Properties {
			get {
				return properties;
			}
		}

		public ArrayList Enums {
			get {
				return enums;
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
			ConstructorBuilder cb;
			MethodAttributes ca = (MethodAttributes.RTSpecialName |
					       MethodAttributes.SpecialName);
			bool is_static = (c.ModFlags & Modifiers.STATIC) != 0;
			
			if (is_static)
				ca |= MethodAttributes.Static;

			ca |= Modifiers.MethodAttr (c.ModFlags);

			cb = TypeBuilder.DefineConstructor (
				ca, c.CallingConvention, c.ParameterTypes (this));
			
			if (is_static){
				if (initialized_static_fields != null)
					EmitStaticFieldInitializers (cb);
			} else {
				if (initialized_fields != null)
					EmitFieldInitializers (cb);
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
					EmitConstructor (c);
			}

			if (default_constructor == null)
				EmitDefaultConstructor (false);

			if (initialized_static_fields != null && default_static_constructor == null)
				EmitDefaultConstructor (true);
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

		public Class (RootContext rc, TypeContainer parent, string name, int mod)
			: base (rc, parent, name)
		{
			int accmods;

			if (parent.Parent == null)
				accmods = Modifiers.INTERNAL;
			else
				accmods = Modifiers.PRIVATE;
			
			this.mod_flags = Modifiers.Check (AllowedModifiers, mod, accmods);
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

		public Struct (RootContext rc, TypeContainer parent, string name, int mod)
			: base (rc, parent, name)
		{
			int accmods;
			
			if (parent.Parent == null)
				accmods = Modifiers.INTERNAL;
			else
				accmods = Modifiers.PRIVATE;
			
			this.mod_flags = Modifiers.Check (AllowedModifiers, mod, accmods);

			this.mod_flags |= Modifiers.SEALED;
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
		Parameters parameters;
		string     return_type;
		string     name;
		int        modifiers;
		Block      block;

		// return_type can be "null" for VOID values.
		public Method (string return_type, int mod, string name, Parameters parameters)
		{
			this.return_type = return_type;
			this.name = name;
			this.parameters = parameters;
			this.modifiers = Modifiers.Check (AllowedModifiers, mod, Modifiers.PRIVATE);
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

		public string Name {
			get {
				return name;
			}
		}

		public int ModFlags {
			get {
				return modifiers;
			}
		}

		public Parameters Parameters {
			get {
				return parameters;
			}
		}

		public string ReturnType {
			get {
				return return_type;
			}
		}

		public string ArgumentSignature {
			get {
				return ""; // TYPEFIX: Type.MakeParameterSignature (name, parameters);
			}
		}
	}

	public class Field {
		public readonly string Type;
		public readonly Object Initializer;
		public readonly string Name;
		public readonly int    ModFlags;
		
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

		public Field (string type, int mod, string name, Object expr_or_array_init)
		{
			Type = type;
			ModFlags = Modifiers.Check (AllowedModifiers, mod, Modifiers.PRIVATE);
			Name = name;
			Initializer = expr_or_array_init;
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

		public CallingConventions CallingConvention {
			get {
				CallingConventions cc = 0;
				
				if (Parameters.ArrayParameter != null)
					cc |= CallingConventions.VarArgs;
				else
					cc |= CallingConventions.Standard;

				if ((ModFlags & Modifiers.STATIC) != 0)
					cc |= CallingConventions.HasThis;

				// FIXME: How is `ExplicitThis' used in C#?
				
				return cc;
			}
		}

		public Type [] ParameterTypes (TypeContainer tc)
		{
			return Parameters.GetParameterInfo (tc);
		}
	}

	public class Property {
		string type;
		string name;
		int mod_flags;
		Block get_block, set_block;

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
		
		public Property (string type, string name, int mod_flags, Block get_block, Block set_block)
		{
			this.type = type;
			this.name = name;
			this.mod_flags = Modifiers.Check (AllowedModifiers, mod_flags, Modifiers.PRIVATE);
			this.get_block = get_block;
			this.set_block = set_block;
		}

		public string Type {
			get {
				return type;
			}
		}

		public string Name {
			get {
				return name;
			}
		}

		public int ModFlags {
			get {
				return mod_flags;
			}
		}

		public Block Get {
			get {
				return get_block;
			}
		}

		public Block Set {
			get {
				return set_block;
			}
		}
	}
}

