//
// interface.cs: Interface handler
//
// Author: Miguel de Icaza (miguel@gnu.org)
//		   Anirban Bhattacharjee (banirban@novell.com)
//
// Licensed under the terms of the GNU GPL
//
// (C) 2001 Ximian, Inc (http://www.ximian.com)
//

/*This file will go off shortly
 * after copying the interface class 
 * in class.cs file
 */

#define CACHE
using System.Collections;
using System;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;

namespace Mono.MonoBASIC {

	/// <summary>
	///   Interfaces
	/// </summary>


	/// <summary>
	///   Interfaces
	/// </summary>
	public class Interface : Mono.MonoBASIC.Class /*TypeContainer , IMemberContainer */
	{
		/// <summary>
		///   Modifiers allowed in a class declaration
		/// </summary>
		public new const int AllowedModifiers =
			Modifiers.NEW       |
			Modifiers.PUBLIC    |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL  |
			Modifiers.PRIVATE;

		public Interface (TypeContainer parent, string name, int mod,
							Attributes attrs, Location l)
			: base (parent, name, 0, attrs, l)
		{
			int accmods;

			if (parent.Parent == null)
				accmods = Modifiers.INTERNAL;
			else
				accmods = Modifiers.PUBLIC;

			this.ModFlags = Modifiers.Check (AllowedModifiers, mod, accmods, l);
			this.ModFlags |= Modifiers.ABSTRACT;
		}

		public override AttributeTargets AttributeTargets {
			get {
				return AttributeTargets.Interface;
			}
		}

		public override TypeAttributes TypeAttr 
		{
			get 
			{
				return base.TypeAttr |
					TypeAttributes.AutoLayout |
					TypeAttributes.Abstract |
					TypeAttributes.Interface;
			}
		}
	}

/*
	public class InterfaceMemberBase {
		public readonly string Name;
		public readonly bool IsNew;
		public Attributes OptAttributes;
		
		public InterfaceMemberBase (string name, bool is_new, Attributes attrs)
		{
			Name = name;
			IsNew = is_new;
			OptAttributes = attrs;
		}
	}
	
	public class InterfaceProperty : InterfaceMemberBase {
		public readonly bool HasSet;
		public readonly bool HasGet;
		public readonly Location Location;
		public Expression Type;
		
		public InterfaceProperty (Expression type, string name,
					  bool is_new, bool has_get, bool has_set,
					  Attributes attrs, Location loc)
			: base (name, is_new, attrs)
		{
			Type = type;
			HasGet = has_get;
			HasSet = has_set;
			Location = loc;
		}
	}
*/
/*	public class InterfaceEvent : InterfaceMemberBase {
		public readonly Location Location;
		public Expression Type;
		
		public InterfaceEvent (Expression type, string name, bool is_new, Attributes attrs,
				       Location loc)
			: base (name, is_new, attrs)
		{
			Type = type;
			Location = loc;
		}
	}
/*	
	public class InterfaceMethod : InterfaceMemberBase {
		public readonly Expression ReturnType;
		public readonly Parameters Parameters;
		public readonly Location Location;
		
		public InterfaceMethod (Expression return_type, string name, bool is_new, Parameters args,
					Attributes attrs, Location l)
			: base (name, is_new, attrs)
		{
			this.ReturnType = return_type;
			this.Parameters = args;
			Location = l;
		}

		/// <summary>
		///   Returns the signature for this interface method
		/// </summary>
		public string GetSignature (DeclSpace ds)
		{
			Type ret = ds.ResolveType (ReturnType, false, Location);
			string args = Parameters.GetSignature (ds);

			if ((ret == null) || (args == null))
				return null;
			
			return (IsNew ? "new-" : "") + ret.FullName + "(" + args + ")";
		}

		public Type [] ParameterTypes (DeclSpace ds)
		{
			return Parameters.GetParameterInfo (ds);
		}
	}

	public class InterfaceIndexer : InterfaceMemberBase {
		public readonly bool HasGet, HasSet;
		public readonly Parameters Parameters;
		public readonly Location Location;
		public Expression Type;
		
		public InterfaceIndexer (Expression type, Parameters args, bool do_get, bool do_set,
					 bool is_new, Attributes attrs, Location loc)
			: base ("", is_new, attrs)
		{
			Type = type;
			Parameters = args;
			HasGet = do_get;
			HasSet = do_set;
			Location = loc;
		}

		public Type [] ParameterTypes (DeclSpace ds)
		{
			return Parameters.GetParameterInfo (ds);
		}
	}*/
}
