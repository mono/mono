//
// delegate.cs: Delegate Handler
//
// Author: Ravi Pratap (ravi@ximian.com)
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
	
	public class Delegate {

		public string Name;
		public string type;
		public int    mod_flags;
		public Parameters Parameters;
		public Attributes OptAttributes;
		public TypeBuilder DelegateBuilder;

		Location loc;

		const int AllowedModifiers =
			Modifiers.NEW |
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.PRIVATE;

		public Delegate (string type, int mod_flags, string name, Parameters param_list,
				 Attributes attrs, Location loc)
		{
			this.Name       = name;
			this.type       = type;
			this.mod_flags  = Modifiers.Check (AllowedModifiers, mod_flags, Modifiers.PUBLIC);
			Parameters      = param_list;
			OptAttributes   = attrs;
			this.loc        = loc;
		}

		public void Define (TypeContainer parent)
		{
			TypeAttributes attr;
			
			if (parent.IsTopLevel)
				attr = TypeAttributes.NestedPublic | TypeAttributes.Class;
			else
				attr = TypeAttributes.Public | TypeAttributes.Class;
			
			Type t = parent.LookupType (type, false);
			Type [] param_types = Parameters.GetParameterInfo (parent);
			Type base_type = System.Type.GetType ("System.MulticastDelegate");

			DelegateBuilder = parent.TypeBuilder.DefineNestedType (Name, attr, base_type);

			//DelegateBuilder.CreateType ();

		}
		
		
		public string Type {
			get {
				return type;
			}
		}

		public int ModFlags {
			get {
				return mod_flags;
			}
		}
		

	}
	
}
