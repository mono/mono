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
		public readonly string BaseType;
		public readonly string EnumName;
		int mod_flags;
		public TypeBuilder EnumBuilder;
		public Attributes  OptAttributes;

		Type UnderlyingType;

		public readonly RootContext RootContext;
		
		public const int AllowedModifiers =
			Modifiers.NEW |
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.PRIVATE;

		public Enum (RootContext rc, string type, int mod_flags, string name, Attributes attrs, Location l)
			: base (name, l)
		{
			RootContext = rc;
			this.BaseType = type;
			this.EnumName = name;
			this.mod_flags = Modifiers.Check (AllowedModifiers, mod_flags, Modifiers.PUBLIC);
			OptAttributes = attrs;
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

		public void DefineEnum (object parent_builder)
		{
			TypeAttributes attr = TypeAttributes.Class | TypeAttributes.Sealed;

			UnderlyingType = RootContext.TypeManager.LookupType (BaseType);

			if (parent_builder is ModuleBuilder) {
				ModuleBuilder builder = (ModuleBuilder) parent_builder;

				if ((ModFlags & Modifiers.PUBLIC) != 0)
					attr |= TypeAttributes.Public;
				else
					attr |= TypeAttributes.NotPublic;
				
				EnumBuilder = builder.DefineType (EnumName, attr, TypeManager.enum_type);

			} else {
				TypeBuilder builder = (TypeBuilder) parent_builder;

				if ((ModFlags & Modifiers.PUBLIC) != 0)
					attr |= TypeAttributes.NestedPublic;
				else
					attr |= TypeAttributes.NestedPrivate;
				
				EnumBuilder = builder.DefineNestedType (EnumName, attr, TypeManager.enum_type);
			}

			EnumBuilder.DefineField ("value__", UnderlyingType,
						 FieldAttributes.Public | FieldAttributes.SpecialName);
			
			RootContext.TypeManager.AddEnumType (EnumName, EnumBuilder, this);
		}

	        bool IsValidEnumLiteral (Expression e)
		{
			if (!(e is Literal))
				return false;

			if (e is IntLiteral || e is UIntLiteral || e is LongLiteral || e is ULongLiteral)
				return true;
			else
				return false;
		}
		
		public void Emit (TypeContainer tc)
		{
			EmitContext ec = new EmitContext (tc, null, UnderlyingType, ModFlags);

			int default_value = 0;

			FieldAttributes attr = FieldAttributes.Public | FieldAttributes.Static
				             | FieldAttributes.Literal;

			foreach (string name in ordered_enums) {
				Expression e = this [name];

				if (e != null) {
					e = Expression.Reduce (ec, e);

					if (IsValidEnumLiteral (e))
						default_value = (int) ((Literal) e).GetValue ();
					else {
						Report.Error (1008, Location,
					          "Type byte, sbyte, short, ushort, int, uint, long, or ulong expected");
						return;
					}
				}
				
				FieldBuilder fb = EnumBuilder.DefineField (name, UnderlyingType, attr);

				fb.SetConstant (default_value++);
			}

			if (OptAttributes != null) {
				if (OptAttributes.AttributeSections != null) {
					foreach (AttributeSection asec in OptAttributes.AttributeSections) {
						if (asec.Attributes != null) {
							foreach (Attribute a in asec.Attributes) {
								CustomAttributeBuilder cb = a.Resolve (ec);
								if (cb != null)
									EnumBuilder.SetCustomAttribute (cb);
							}
						}
					}
				}
			}
			
		}

		public void CloseEnum ()
		{
			EnumBuilder.CreateType ();
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
