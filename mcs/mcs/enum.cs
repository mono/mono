//
// enum.cs: Enum handling.
//
// Author: Miguel de Icaza (miguel@gnu.org)
//         Ravi Pratap     (ravi@ximian.com)
//
// Licensed under the terms of the GNU GPL
//
// (C) 2001 Ximian, Inc (http://www.ximian.com)
//

using System;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;

namespace Mono.CSharp {

	/// <summary>
	///   Enumeration container
	/// </summary>
	public class Enum : DeclSpace {

		ArrayList ordered_enums;
		public readonly string BaseType;
		public readonly string EnumName;
		int mod_flags;
		public TypeBuilder EnumBuilder;
		public Attributes  OptAttributes;
		
		public Type UnderlyingType;

		public readonly RootContext RootContext;

		Hashtable member_to_location;
		ArrayList field_builders;
		Location loc;
		
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
			loc = l;

			ordered_enums = new ArrayList ();
			member_to_location = new Hashtable ();
			field_builders = new ArrayList ();
		}

		/// <summary>
		///   Adds @name to the enumeration space, with @expr
		///   being its definition.  
		/// </summary>
		public AdditionResult AddEnumMember (string name, Expression expr, Location loc)
		{
			if (defined_names.Contains (name))
				return AdditionResult.NameExists;

			DefineName (name, expr);

			ordered_enums.Add (name);
			member_to_location.Add (name, loc);
			
			return AdditionResult.Success;
		}

		public void DefineEnum (object parent_builder)
		{
			TypeAttributes attr = TypeAttributes.Class | TypeAttributes.Sealed;

			UnderlyingType = RootContext.TypeManager.LookupType (BaseType);

			if (UnderlyingType != TypeManager.int32_type &&
			    UnderlyingType != TypeManager.uint32_type &&
			    UnderlyingType != TypeManager.int64_type &&
			    UnderlyingType != TypeManager.uint64_type &&
			    UnderlyingType != TypeManager.short_type &&
			    UnderlyingType != TypeManager.ushort_type &&
			    UnderlyingType != TypeManager.byte_type  &&
			    UnderlyingType != TypeManager.sbyte_type) {
				Report.Error (1008, Location,
					      "Type byte, sbyte, short, ushort, int, uint, " +
					      "long, or ulong expected");
				return;
			}

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

			return;
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

		object GetNextDefaultValue (object default_value)
		{
			if (UnderlyingType == TypeManager.int32_type) {
				int i = (int) default_value;
				
				if (i < System.Int32.MaxValue)
					return ++i;
				else
					return null;
			} else if (UnderlyingType == TypeManager.uint32_type) {
				uint i = (uint) default_value;

				if (i < System.UInt32.MaxValue)
					return ++i;
				else
					return null;
			} else if (UnderlyingType == TypeManager.int64_type) {
				long i = (long) default_value;

				if (i < System.Int64.MaxValue)
					return ++i;
				else
					return null;
			} else if (UnderlyingType == TypeManager.uint64_type) {
				ulong i = (ulong) default_value;

				if (i < System.UInt64.MaxValue)
					return ++i;
				else
					return null;
			} else if (UnderlyingType == TypeManager.short_type) {
				short i = (short) default_value;

				if (i < System.Int16.MaxValue)
					return ++i;
				else
					return null;
			} else if (UnderlyingType == TypeManager.ushort_type) {
				ushort i = (ushort) default_value;

				if (i < System.UInt16.MaxValue)
					return ++i;
				else
					return null;
			} else if (UnderlyingType == TypeManager.byte_type) {
				byte i = (byte) default_value;

				if (i < System.Byte.MaxValue)
					return ++i;
				else
					return null;
			} else if (UnderlyingType == TypeManager.sbyte_type) {
				sbyte i = (sbyte) default_value;

				if (i < System.SByte.MaxValue)
					return ++i;
				else
					return null;
			}

			return null;
		}

		void error31 (Literal l, Location loc)
		{
			Report.Error (31, loc, "Constant value '" + l.AsString () +
				      "' cannot be converted" +
				      " to a " + TypeManager.CSharpName (UnderlyingType));
			return;
		}
		
		public void Populate (TypeContainer tc)
		{
			//
			// If there was an error during DefineEnum, return
			//
			if (EnumBuilder == null)
				return;
			
			EmitContext ec = new EmitContext (tc, Location, null, UnderlyingType, ModFlags);
			
			object default_value = 0;
			
			FieldAttributes attr = FieldAttributes.Public | FieldAttributes.Static
				             | FieldAttributes.Literal;

			
			foreach (string name in ordered_enums) {
				Expression e = this [name];
				Location loc = (Location) member_to_location [name];

				if (e != null) {
					e = Expression.Reduce (ec, e);

					if (IsValidEnumLiteral (e)) {
						Literal l = (Literal) e;
						default_value = l.GetValue ();

						if (default_value == null) {
							error31 (l, loc);
							return;
						}
						
					} else {
						Report.Error (1008, loc,
					          "Type byte, sbyte, short, ushort, int, uint, long, or ulong expected");
						return;
					}
				}
				
				FieldBuilder fb = EnumBuilder.DefineField (name, UnderlyingType, attr);

				if (default_value == null) {
					Report.Error (543, loc, "Enumerator value for '" + name + "' is too large to " +
						      "fit in its type");
					return;
				}

				try {
					default_value = Convert.ChangeType (default_value, UnderlyingType);
				} catch {
					error31 ((Literal) e, loc);
					return;
				}

				fb.SetConstant (default_value);
				field_builders.Add (fb);

				if (!TypeManager.RegisterField (fb, default_value))
					return;

				default_value = GetNextDefaultValue (default_value);
			}

			if (OptAttributes == null)
				return;
			
			if (OptAttributes.AttributeSections == null)
				return;
			
			foreach (AttributeSection asec in OptAttributes.AttributeSections) {
				if (asec.Attributes == null)
					continue;
				
				foreach (Attribute a in asec.Attributes) {
					CustomAttributeBuilder cb = a.Resolve (ec);

					if (cb == null)
						continue;
					
					EnumBuilder.SetCustomAttribute (cb);
				}
			}
		}
		
		//
		// Hack around System.Reflection as found everywhere else
		//
		public MemberInfo [] FindMembers (MemberTypes mt, BindingFlags bf, MemberFilter filter, object criteria)
		{
			ArrayList members = new ArrayList ();

			if ((mt & MemberTypes.Field) != 0) {
				foreach (FieldBuilder fb in field_builders)
					if (filter (fb, criteria) == true)
						members.Add (fb);
			}

			int count = members.Count;

			if (count > 0) {
				MemberInfo [] mi = new MemberInfo [count];
				members.CopyTo (mi, 0);
				return mi;
			}

			return null;
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
