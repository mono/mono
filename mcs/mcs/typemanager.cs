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

public class TypeManager {

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
	//   Keeps track of those types that are defined by the
	//   user's program
	// </remarks>
	ArrayList user_types;

	public TypeManager ()
	{
		assemblies = new ArrayList ();
		user_types = new ArrayList ();
		types = new Hashtable ();
	}

	public void AddUserType (string name, TypeBuilder t)
	{
		types.Add (t.FullName, t);
		user_types.Add (t);
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
	//   Returns the User Defined Types
	// </summary>
	public ArrayList UserTypes {
		get {
			return user_types;
		}
	}
}

