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

	public TypeManager ()
	{
		assemblies = new ArrayList ();
		user_types = new ArrayList ();
		types = new Hashtable ();
		typecontainers = new Hashtable ();
	}

	public void AddUserType (string name, TypeBuilder t, TypeContainer tc)
	{
		types.Add (t.FullName, t);
		user_types.Add (t);
		typecontainers.Add (t.FullName, tc);
	}

	public void AddUserType (string name, TypeBuilder t)
	{
		this.AddUserType (name, t, null);
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

	// <remarks>
	//   The types have to be initialized after the initial
	//   population of the type has happened (for example, to
	//   bootstrap the corlib.dll
	// </remarks>
	public void InitCoreTypes ()
	{
		object_type  = LookupType ("System.Object");
		string_type  = LookupType ("System.String");
		int32_type   = LookupType ("System.Int32");
		int64_type   = LookupType ("System.Int64");
		uint32_type  = LookupType ("System.UInt32"); 
		uint64_type  = LookupType ("System.UInt64"); 
		float_type   = LookupType ("System.Single");
		double_type  = LookupType ("System.Double");
		char_type    = LookupType ("System.Char");
		short_type   = LookupType ("System.Short");
		decimal_type = LookupType ("System.Decimal");
		bool_type    = LookupType ("System.Bool");
	}
	
	public MemberInfo [] FindMembers (Type t, MemberTypes mt, BindingFlags bf, MemberFilter filter, object criteria)
	{
		TypeContainer tc;

		tc = (TypeContainer) typecontainers [t.FullName];

		if (tc == null)
			return t.FindMembers (mt, bf, filter, criteria);
		else
			return tc.FindMembers (mt, bf, filter, criteria);

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

}

}
