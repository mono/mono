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
using System.Reflection;
using System.Reflection.Emit;

namespace CIR {

	public class Enum : DeclSpace {

		ArrayList ordered_enums;
		string type;
		string name;
		int mod_flags;
		public EnumBuilder EnumBuilder;
		
		public const int AllowedModifiers =
			Modifiers.NEW |
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.PRIVATE;

		public Enum (string type, int mod_flags, string name) : base (name)
		{
			this.type = type;
			this.name = name;
			this.mod_flags = Modifiers.Check (AllowedModifiers, mod_flags, Modifiers.PUBLIC);

			ordered_enums = new ArrayList ();
		}

		// <summary>
		//   Adds @name to the enumeration space, with @expr
		//   being its definition.  
		// </summary>
		public AdditionResult AddEnumMember (string name, Expression expr)
		{
			if (defined_names.Contains (name))
				return AdditionResult.NameExists;

			DefineName (name, expr);

			ordered_enums.Add (name);
			return AdditionResult.Success;
		}

		public void Define (TypeContainer parent)
		{
			TypeAttributes attr = Modifiers.TypeAttr (ModFlags);

			Type t = System.Type.GetType (type);

			EnumBuilder = parent.RootContext.CodeGen.ModuleBuilder.DefineEnum (name, attr, t);
		}

		public string Type {
			get {
				return type;
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
	}
}
