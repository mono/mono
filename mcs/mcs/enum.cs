//
// enum.cs: Enum handling.
//
// Author: Miguel de Icaza (miguel@gnu.org)
//
// Licensed under the terms of the GNU GPL
//
// (C) 2001 Ximian, Inc (http://www.ximian.com)
//

using System;
using System.Collections;

namespace CIR {

	public class Enum : DeclSpace {
		ArrayList ordered_enums;
		TypeRef typeref;
		string name;
		int mod_flags;

		public const int AllowedModifiers =
			Modifiers.NEW |
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.PRIVATE;

		public Enum (TypeRef typeref, int mod_flags, string name) : base (name)
		{
			this.typeref = typeref;
			this.name = name;
			this.mod_flags = Modifiers.Check (AllowedModifiers, mod_flags, Modifiers.PUBLIC);

			ordered_enums = new ArrayList ();
		}

		// <summary>
		//   Adds @name to the enumeration space, with @expr
		//   being its definition.  
		// </summary>
		public AdditionResult AddEnum (string name, Expression expr)
		{
			if (defined_names.Contains (name))
				return AdditionResult.NameExists;

			DefineName (name, expr);

			ordered_enums.Add (name);
			return AdditionResult.Success;
		}

		public Type Type {
			get {
				return typeref.Type;
			}
		}

		public ArrayList ValueNames {
			get {
				return ordered_enums;
			}
		}

		public int ModFlags {
			get {
				return mod_flags;
			}
		}
		
		// indexer
		public Expression this [string name] {
			get {
				return (Expression) defined_names [name];
			}
		}

		public override Type Define (Tree tree)
		{
			return null;
		}
	}
}
