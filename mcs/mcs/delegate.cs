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
		public Parameters parameters;
		public Attributes OptAttributes;

		const int AllowedModifiers =
			Modifiers.NEW |
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.PRIVATE;

		public Delegate (string type, int mod_flags, string name, Parameters param_list,
				 Attributes attrs) : base (name)
		{
			this.name       = name;
			this.type       = type;
			this.mod_flags  = Modifiers.Check (AllowedModifiers, mod_flags, Modifiers.PUBLIC);
			parameters      = param_list;
			OptAttributes   = attrs;
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
