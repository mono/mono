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
	//   This is used to map defined FQN to Types
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
		types = new Hashtable ();
		user_types = new ArrayList ();
	}

	// <summary>
	//   Registers a single type with the Type Manager.  This is
	//   an interface for our type builder. 
	// </summary>
	public void AddType (string name, Type t)
	{
		types.Add (t.FullName, t);
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
		foreach (Type t in a.GetExportedTypes ()){
			AddType (t.FullName, t);
		}
	}

	// <summary>
	//   Returns the Type associated with @name
	// </summary>
	public Type LookupType (string name)
	{
		Type t = (Type) types [name];
		
		return t;
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

