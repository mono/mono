//
// typegen.cs: type generation 
//
// Author: Miguel de Icaza (miguel@gnu.org)
//
// Licensed under the terms of the GNU GPL
//
// (C) 2001 Ximian, Inc (http://www.ximian.com)
//
//

using System;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;

namespace CIR {

public class TypeManager {
	static public Type object_type;
	static public Type value_type;
	static public Type string_type;
	static public Type int32_type;
	static public Type uint32_type;
	static public Type int64_type;
	static public Type uint64_type;
	static public Type float_type;
	static public Type double_type;
	static public Type char_type;
	static public Type short_type;
	static public Type decimal_type;
	static public Type bool_type;
	static public Type sbyte_type;
	static public Type byte_type;
	static public Type ushort_type;
	static public Type enum_type;
	static public Type delegate_type;
	static public Type void_type;
	static public Type enumeration_type;
	static public Type array_type;
	static public Type runtime_handle_type;
	static public Type icloneable_type;
	static public Type type_type;
	static public Type ienumerator_type;
	static public Type idisposable_type;
	static public Type iasyncresult_type;
	static public Type asynccallback_type;

	static public Type intptr_type;
	
	static public MethodInfo string_concat_string_string;
	static public MethodInfo string_concat_object_object;
	static public MethodInfo system_type_get_type_from_handle;
	static public MethodInfo object_getcurrent_void;
	static public MethodInfo bool_movenext_void;
	static public MethodInfo void_dispose_void;
	
	// <remarks>
	//   Holds the Array of Assemblies that have been loaded
	//   (either because it is the default or the user used the
	//   -r command line option)
	// </remarks>
	ArrayList assemblies;

	// <remarks>
	//   This is the type_cache from the assemblies to avoid
	//   hitting System.Reflection on every lookup.
	// </summary>
	Hashtable types;

	// <remarks>
	//  This is used to hotld the corresponding TypeContainer objects
	//  since we need this in FindMembers
	// </remarks>
	Hashtable typecontainers;

	// <remarks>
	//   Keeps track of those types that are defined by the
	//   user's program
	// </remarks>
	ArrayList user_types;

	// <remarks>
	//   Keeps a mapping between TypeBuilders and their TypeContainers
	// </remarks>
	static Hashtable builder_to_container;

	// <remarks>
	//   Maps MethodBase.RuntimeTypeHandle to a Type array that contains
	//   the arguments to the method
	// </remarks>
	static Hashtable method_arguments;

	static Hashtable builder_to_interface;

	// <remarks>
	//  Keeps track of delegate types
	// </remarks>

	static Hashtable delegate_types;
	
	public TypeManager ()
	{
		assemblies = new ArrayList ();
		user_types = new ArrayList ();
		types = new Hashtable ();
		typecontainers = new Hashtable ();
		builder_to_interface = new Hashtable ();
		delegate_types = new Hashtable ();
	}

	static TypeManager ()
	{
		method_arguments = new Hashtable ();
		builder_to_container = new Hashtable ();
	}
	
	public void AddUserType (string name, TypeBuilder t)
	{
		types.Add (name, t);
		user_types.Add (t);
	}
	
	public void AddUserType (string name, TypeBuilder t, TypeContainer tc)
	{
		AddUserType (name, t);
		builder_to_container.Add (t, tc);
		typecontainers.Add (name, tc);
	}

	public void AddDelegateType (string name, TypeBuilder t, Delegate del)
	{
		types.Add (name, t);
		delegate_types.Add (t, del);
	}

	public void AddUserInterface (string name, TypeBuilder t, Interface i)
	{
		AddUserType (name, t);
		builder_to_interface.Add (t, i);
	}
		
	// <summary>
	//   Returns the TypeContainer whose Type is `t' or null if there is no
	//   TypeContainer for `t' (ie, the Type comes from a library)
	// </summary>
	public static TypeContainer LookupTypeContainer (Type t)
	{
		return (TypeContainer) builder_to_container [t];
	}

	public Interface LookupInterface (Type t)
	{
		return (Interface) builder_to_interface [t];
	}

	public static Delegate LookupDelegate (Type t)
	{
		return (Delegate) delegate_types [t];
	}
	
	// <summary>
	//   Registers an assembly to load types from.
	// </summary>
	public void AddAssembly (Assembly a)
	{
		assemblies.Add (a);
	}

	// <summary>
	//   Returns the Type associated with @name
	// </summary>
	public Type LookupType (string name)
	{
		Type t;

		//
		// First lookup in user defined and cached values
		//

		t = (Type) types [name];
		if (t != null)
			return t;

		foreach (Assembly a in assemblies){
			t = a.GetType (name);
			if (t != null){
				types [name] = t;

				return t;
			}
		}

		return null;
	}

	// <summary>
	//   Returns the C# name of a type if possible, or the full type name otherwise
	// </summary>
	static public string CSharpName (Type t)
	{
		if (t == int32_type)
			return "int";
		else if (t == uint32_type)
			return "uint";
		else if (t == int64_type)
			return "long";
		else if (t == uint64_type)
			return "ulong";
		else if (t == float_type)
			return "float";
		else if (t == double_type)
			return "double";
		else if (t == char_type)
			return "char";
		else if (t == short_type)
			return "short";
		else if (t == decimal_type)
			return "decimal";
		else if (t == bool_type)
			return "bool";
		else if (t == sbyte_type)
			return "sbyte";
		else if (t == byte_type)
			return "byte";
		else if (t == short_type)
			return "short";
		else if (t == ushort_type)
			return "ushort";
		else if (t == string_type)
			return "string";
		else if (t == object_type)
			return "object";
		else
			return t.FullName;
	}

	Type CoreLookupType (string name)
	{
		Type t = LookupType (name);

		if (t == null)
			throw new Exception ("Can not find core type " + name);

		return t;
	}

	MethodInfo GetMethod (Type t, string name, Type [] args)
	{
		MethodInfo mi = t.GetMethod (name, args);

		if (mi == null)
			throw new Exception ("Can not find the core function `" + name + "'");

		return mi;
	}
	
	// <remarks>
	//   The types have to be initialized after the initial
	//   population of the type has happened (for example, to
	//   bootstrap the corlib.dll
	// </remarks>
	public void InitCoreTypes ()
	{
		object_type   = CoreLookupType ("System.Object");
		value_type    = CoreLookupType ("System.ValueType");
		string_type   = CoreLookupType ("System.String");
		int32_type    = CoreLookupType ("System.Int32");
		int64_type    = CoreLookupType ("System.Int64");
		uint32_type   = CoreLookupType ("System.UInt32"); 
		uint64_type   = CoreLookupType ("System.UInt64"); 
		float_type    = CoreLookupType ("System.Single");
		double_type   = CoreLookupType ("System.Double");
		byte_type     = CoreLookupType ("System.Byte");
		sbyte_type    = CoreLookupType ("System.SByte");
		char_type     = CoreLookupType ("System.Char");
		short_type    = CoreLookupType ("System.Int16");
		ushort_type   = CoreLookupType ("System.UInt16");
		decimal_type  = CoreLookupType ("System.Decimal");
		bool_type     = CoreLookupType ("System.Boolean");
		enum_type     = CoreLookupType ("System.Enum");
		delegate_type = CoreLookupType ("System.MulticastDelegate");
		array_type    = CoreLookupType ("System.Array");
		void_type     = CoreLookupType ("System.Void");
		type_type     = CoreLookupType ("System.Type");
		
		runtime_handle_type = CoreLookupType ("System.RuntimeTypeHandle");
		ienumerator_type    = CoreLookupType ("System.Collections.IEnumerator");
		icloneable_type     = CoreLookupType ("System.ICloneable");
		idisposable_type    = CoreLookupType ("System.IDisposable");
		iasyncresult_type   = CoreLookupType ("System.IAsyncResult");
		asynccallback_type = CoreLookupType  ("System.AsyncCallback");
		
		intptr_type = CoreLookupType ("System.IntPtr");
		
		//
		// Now load the default methods that we use.
		//
		Type [] string_string = { string_type, string_type };
		Type [] object_object = { object_type, object_type };
		Type [] runtime_type_handle = { runtime_handle_type };
		Type [] void_arg = {  };
		
		string_concat_string_string = GetMethod (
			string_type, "Concat", string_string);
		string_concat_object_object = GetMethod (
			string_type, "Concat", object_object);
		system_type_get_type_from_handle = GetMethod (
			type_type, "GetTypeFromHandle", runtime_type_handle);
		object_getcurrent_void = GetMethod (
			ienumerator_type, "get_Current", void_arg);
		bool_movenext_void = GetMethod (
			ienumerator_type, "MoveNext", void_arg);
		void_dispose_void = GetMethod (
			idisposable_type, "Dispose", void_arg);
	}
	
	public MemberInfo [] FindMembers (Type t, MemberTypes mt, BindingFlags bf, MemberFilter filter, object criteria)
	{
		TypeContainer tc;

		if (IsDelegateType (t))
		        return null;
		
		tc = (TypeContainer) builder_to_container [t];
		
		if (tc == null) 
		        return t.FindMembers (mt, bf, filter, criteria);
		else 
		        return tc.FindMembers (mt, bf, filter, criteria);
		
	}

	public static bool IsDelegateType (Type t)
	{
		Delegate del = (Delegate) delegate_types [t];

		if (del != null)
			return true;
		else
			return false;
	}
	
	// <summary>
	//   Returns the User Defined Types
	// </summary>
	public ArrayList UserTypes {
		get {
			return user_types;
		}
	}

	public Hashtable TypeContainers {
		get {
			return typecontainers;
		}
	}

	static string GetSig (MethodBase mb)
	{
		if (mb is MethodBuilder || mb is ConstructorBuilder)
			return mb.ReflectedType.FullName + ":" + mb;
		else
			return mb.MethodHandle.ToString ();
	}
	
	//
	// Gigantic work around for stupidity in System.Reflection.Emit follows
	//
	// Since System.Reflection.Emit can not return MethodBase.GetParameters
	// for anything which is dynamic, and we need this in a number of places,
	// we register this information here, and use it afterwards.
	//
	static public void RegisterMethod (MethodBase mb, Type [] args)
	{
		string s;
		
		s = GetSig (mb);
		method_arguments.Add (s, args);
	}

	// <summary>
	//    Returns the argument types for a method based on its methodbase
	//
	//    For dynamic methods, we use the compiler provided types, for
	//    methods from existing assemblies we load them from GetParameters,
	//    and insert them into the cache
	// </summary>
	static public Type [] GetArgumentTypes (MethodBase mb)
	{
		string sig = GetSig (mb);
		object o = method_arguments [sig];

		if (method_arguments.Contains (sig))
			return (Type []) method_arguments [sig];
		else {
			ParameterInfo [] pi = mb.GetParameters ();
			int c = pi.Length;
			Type [] types = new Type [c];
			
			for (int i = 0; i < c; i++)
				types [i] = pi [i].ParameterType;

			method_arguments.Add (sig, types);
			return types;
		}
	}

	static Hashtable properties;
	
	static public void RegisterProperty (PropertyBuilder pb, MethodBase get, MethodBase set)
	{
		if (properties == null)
			properties = new Hashtable ();
		
		properties.Add (pb, new DictionaryEntry (get, set));
	}
	
	static public MethodInfo [] GetAccessors (PropertyInfo pi)
	{
		MethodInfo [] ret;
			
		if (pi is PropertyBuilder){
			DictionaryEntry de = (DictionaryEntry) properties [pi];

			ret = new MethodInfo [2];
			ret [0] = (MethodInfo) de.Key;
			ret [1] = (MethodInfo) de.Value;

			return ret;
		} else
			return pi.GetAccessors ();
	}
		

}

}
