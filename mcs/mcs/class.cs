//
// class.cs: Class and Struct handlers
//
// Author: Miguel de Icaza (miguel@gnu.org)
//
// Licensed under the terms of the GNU GPL
//
// (C) 2001 Ximian, Inc (http://www.ximian.com)
//
// TODO:
//
//    a. Maybe keep a list of defined names in the order they
//       appeared, so we can walk things in this way to present
//       the users with errors in that order?
//

using System.Collections;
using System;

namespace CIR {
	
	public class TypeContainer : DeclSpace {
		protected int mod_flags;
		Hashtable types, fields, properties;
		Hashtable enums, constants, interfaces, method_groups;

		ArrayList constructor_list;

		//
		// This is the namespace in which this typecontainer
		// was declared.  We use this to resolve names.
		//
		CSC.Namespace my_namespace;
		
		//
		// This one is computed after we can distinguish interfaces
		// from classes from the arraylist `type_bases' 
		//
		string     base_class_name;

		TypeContainer parent;
		ArrayList type_bases;

		public TypeContainer (TypeContainer parent, string name) : base (name)
		{
			types = new Hashtable ();
			this.parent = parent;

			string n;
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
				constants = new Hashtable ();

			constants.Add (name, constant);
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
				enums = new Hashtable ();

			enums.Add (name, e);
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
			types.Add (name, c);

			return AdditionResult.Success;
		}

		public AdditionResult AddStruct (Struct s)
		{
			AdditionResult res;
			string name = s.Name;
			
			if ((res = IsValid (name)) != AdditionResult.Success)
				return res;

			DefineName (name, s);
			types.Add (name, s);

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
			
			if (constructor_list == null)
				constructor_list = new ArrayList ();

			constructor_list.Add (c);
			
			return AdditionResult.Success;
		}
		
		public AdditionResult AddInterface (Interface iface)
		{
			AdditionResult res;
			string name = iface.Name;

			if ((res = IsValid (name)) != AdditionResult.Success)
				return res;
			
			if (interfaces == null)
				interfaces = new Hashtable ();
			interfaces.Add (name, iface);
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
				fields = new Hashtable ();

			fields.Add (name, field);
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
				properties = new Hashtable ();

			properties.Add (name, prop);
			DefineName (name, prop);

			return AdditionResult.Success;
		}
		
		public Constant GetConstant (string name) {
			return (Constant) constants [name];
		}
		
		public TypeContainer Parent {
			get {
				return parent;
			}
		}

		public Hashtable Types {
			get {
				return types;
			}
		}

		public Hashtable MethodGroups {
			get {
				return method_groups;
			}
		}

		public Hashtable Constants {
			get {
				return constants;
			}
		}

		public Hashtable Interfaces {
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

		public Hashtable Fields {
			get {
				return fields;
			}
		}

		public Hashtable Constructors {
			get {
				return null; // constructors;
			}
		}

		public Hashtable Properties {
			get {
				return properties;
			}
		}

		public Hashtable Enums {
			get {
				return enums;
			}
		}

		public CSC.Namespace Namespace {
			get {
				return my_namespace;
			}

			set {
				my_namespace = value;
			}
		}
		
		public int ResolveParents (Tree root)
		{
			if (Bases == null){
				base_class_name = "System.Object";
				return 0;
			}
			
			if (type_bases.Count == 0){
				base_class_name = "System.Object";
				return 0;
			}
			
			return 0;
		}

		override public Type Define (Tree tree)
		{
			return null;
		}
		
		public delegate void VisitContainer (TypeContainer container, object cback_data);

		void VisitTypesAt (TypeContainer root, VisitContainer visit, object cback)
		{
			if (root == null)
				return;
			
			foreach (DictionaryEntry de in root.Types){
				TypeContainer type = (TypeContainer) de.Value;

				visit (type, cback);
				VisitTypesAt (type, visit, cback);
			}
		}

		// <summary>
		//   Use this method to visit all the types in a type container.
		//   You can use cback to pass arbitrary data to your callback.
		// </summary>
		public void VisitTypes (VisitContainer visit, object cback)
		{
			foreach (DictionaryEntry de in types){
				TypeContainer type = (TypeContainer) de.Value;

				VisitTypesAt (type, visit, cback);
			}
			
		}

		internal class VisitExpressions_Lambda {
			VisitExpressionRoot vb;
			object user_data;

			void walk_arguments (ArrayList args)
			{
				if (args == null)
					return;
				
				int top = args.Count;

				for (int i = 0; i < top; i++){
					Argument arg = (Argument) args [i];

					vb (arg.Expr, user_data);
				}
			}

			void walk_block (Block b)
			{
			}
			
			void walk_constructor (Constructor c)
			{
				ConstructorInitializer init = c.Initializer;
				
				if (init != null && init.Arguments != null)
					walk_arguments (init.Arguments);

				walk_block (c.Block);
			}

			void walk_properties (Property p)
			{
			}

			void walk_method (Method m)
			{
			}
				
			void type_walker_1 (TypeContainer type, object cback)
			{
				if (type.Fields != null){
					foreach (DictionaryEntry de in type.Fields){
						Field f = (Field) de.Value;
						
						if (f.Initializer != null){
							if (f.Initializer is Expression)
								vb ((Expression) f.Initializer, user_data);
						}
					}
				}

				if (type.Constructors != null){
					foreach (DictionaryEntry de in type.Constructors)
						walk_constructor ((Constructor) de.Value);
				}

				if (type.Properties != null){
					foreach (DictionaryEntry de in type.Properties)
						walk_properties ((Property) de.Value);
				}

				if (type.MethodGroups != null){
					foreach (DictionaryEntry de in type.MethodGroups){
						Hashtable methods = ((MethodGroup) de.Value).Methods;
						foreach (Method m in methods)
							walk_method (m);
					}
				}
			}

			
			internal VisitExpressions_Lambda (TypeContainer tc,
							  VisitExpressionRoot vb,
							  object user_data)
			{
				this.vb = vb;
				this.user_data = user_data;

				tc.VisitTypes (new VisitContainer (type_walker_1), null);
			}
		}
		
		public delegate void VisitExpressionRoot (Expression e, object cback);
		// <summary>
		//   Use this method to visit all the code blocks in a TypeContainer
		// </summary>
		public void VisitExpressionRoots (VisitExpressionRoot vb, object cback)
		{
			VisitExpressions_Lambda l = new VisitExpressions_Lambda (this, vb, cback);
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

		public Class (TypeContainer parent, string name, int mod)
			: base (parent, name)
		{
			int accmods;

			if (parent.Parent == null)
				accmods = Modifiers.INTERNAL;
			else
				accmods = Modifiers.PRIVATE;
			
			this.mod_flags = Modifiers.Check (AllowedModifiers, mod, accmods);
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

		public Struct (TypeContainer parent, string name, int mod)
			: base (parent, name)
		{
			int accmods;
			
			if (parent.Parent == null)
				accmods = Modifiers.INTERNAL;
			else
				accmods = Modifiers.PRIVATE;
			
			this.mod_flags = Modifiers.Check (AllowedModifiers, mod, accmods);
		}
	}

	public class Method {
		Parameters parameters;
		TypeRef    return_typeref;
		string     name;
		int        modifiers;
		Block      block;

		// return_type can be "null" for VOID values.
		public Method (TypeRef return_typeref, int mod, string name, Parameters parameters)
		{
			this.return_typeref = return_typeref;
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

		public Type ReturnType {
			get {
				return return_typeref.Type;
			}
		}

		public string ArgumentSignature {
			get {
				return ""; // TYPEFIX: Type.MakeParameterSignature (name, parameters);
			}
		}
	}

	public class Field {
		Type type;
		Object expr_or_array_init;
		string name;
		int modifiers;
		
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

		public Field (TypeRef typeref, int mod, string name, Object expr_or_array_init)
		{
			this.type = type;
			this.modifiers = Modifiers.Check (AllowedModifiers, mod, Modifiers.PRIVATE);
			this.name = name;
			this.expr_or_array_init = expr_or_array_init;
		}

		public Type Type {
			get {
				return type;
			}
		}

		public object Initializer {
			get {
				return expr_or_array_init;
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
		ConstructorInitializer init;
		string name;
		Parameters args;
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
			this.name = name;
			this.args = args;
			this.init = init;
		}

		public string Name {
			get {
				return name;
			}
		}

		public ConstructorInitializer Initializer {
			get {
				return init;
			}
		}

		public Parameters Parameters {
			get {
				return args;
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

		public int ModFlags {
			get {
				return mod_flags;
			}

			set {
				mod_flags = Modifiers.Check (AllowedModifiers, value, 0);
			}
		}
	}

	public class Property {
		TypeRef typeref;
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
		
		public Property (TypeRef typeref, string name, int mod_flags, Block get_block, Block set_block)
		{
			this.typeref = typeref;
			this.name = name;
			this.mod_flags = Modifiers.Check (AllowedModifiers, mod_flags, Modifiers.PRIVATE);
			this.get_block = get_block;
			this.set_block = set_block;
		}

		public Type Type {
			get {
				return typeref.Type;
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

