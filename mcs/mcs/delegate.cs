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
	
	public class Delegate : DeclSpace {

		public string name;
		public string type;
		public int    mod_flags;
		public Parameters Parameters;
		public Attributes OptAttributes;
		public TypeBuilder DelegateBuilder;

		const int AllowedModifiers =
			Modifiers.NEW |
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.PRIVATE;

		public Delegate (string type, int mod_flags, string name, Parameters param_list,
				 Attributes attrs, Location l) : base (name, l)
		{
			this.name       = name;
			this.type       = type;
			this.mod_flags  = Modifiers.Check (AllowedModifiers, mod_flags, Modifiers.PUBLIC);
			Parameters      = param_list;
			OptAttributes   = attrs;
		}

		public void Define (TypeContainer parent)
		{
			TypeAttributes attr = Modifiers.TypeAttr (ModFlags, parent);

			Type t = parent.LookupType (type, false);
			Type [] param_types = Parameters.GetParameterInfo (parent);
			Type base_type = System.Type.GetType ("System.MulticastDelegate");

			DelegateBuilder = parent.TypeBuilder.DefineNestedType (name, attr, base_type);

			// FIXME : Need to figure out how to proceed from here. 

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
