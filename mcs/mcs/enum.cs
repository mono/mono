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
		public TypeBuilder EnumBuilder;
		public Attributes  OptAttributes;
		
		public Type UnderlyingType;

		Hashtable member_to_location;

		//
		// This is for members that have been defined
		//
		Hashtable member_to_value;
		
		ArrayList field_builders;
		
		public const int AllowedModifiers =
			Modifiers.NEW |
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.PRIVATE;

		public Enum (string type, int mod_flags, string name, Attributes attrs, Location l)
			: base (name, l)
		{
			this.BaseType = type;
			ModFlags = Modifiers.Check (AllowedModifiers, mod_flags, Modifiers.PUBLIC);
			OptAttributes = attrs;

			ordered_enums = new ArrayList ();
			member_to_location = new Hashtable ();
			member_to_value = new Hashtable ();
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
				
				EnumBuilder = builder.DefineType (Name, attr, TypeManager.enum_type);

			} else {
				TypeBuilder builder = (TypeBuilder) parent_builder;

				if ((ModFlags & Modifiers.PUBLIC) != 0)
					attr |= TypeAttributes.NestedPublic;
				else
					attr |= TypeAttributes.NestedPrivate;

				
				EnumBuilder = builder.DefineNestedType (
					Basename, attr, TypeManager.enum_type);
			}

			EnumBuilder.DefineField ("value__", UnderlyingType,
						 FieldAttributes.Public | FieldAttributes.SpecialName
						 | FieldAttributes.RTSpecialName);

			RootContext.TypeManager.AddEnumType (Name, EnumBuilder, this);

			return;
		}

	        bool IsValidEnumConstant (Expression e)
		{
			if (!(e is Constant))
				return false;

			if (e is IntConstant || e is UIntConstant || e is LongConstant ||
			    e is ByteConstant || e is SByteConstant || e is ShortConstant ||
			    e is UShortConstant || e is ULongConstant || e is EnumConstant)
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

		void error31 (object val, Location loc)
		{
			if (val is Constant)
				Report.Error (31, loc, "Constant value '" + ((Constant) val).AsString () +
					      "' cannot be converted" +
					      " to a " + TypeManager.CSharpName (UnderlyingType));
			else 
				Report.Error (31, loc, "Constant value '" + val +
					      "' cannot be converted" +
					      " to a " + TypeManager.CSharpName (UnderlyingType));
			return;
		}

		/// <summary>
		///  This is used to lookup the value of an enum member. If the member is undefined,
		///  it attempts to define it and return its value
		/// </summary>
		public object LookupEnumValue (EmitContext ec, string name, Location loc)
		{
			object default_value = null;
			Constant c = null;

			default_value = member_to_value [name];

			if (default_value != null)
				return default_value;

			if (!defined_names.Contains (name)) {
				Report.Error (117, loc, "'"+ Name + "' does not contain a definition for '"
					      + name + "'");
				return null;
			}

			//
			// So if the above doesn't happen, we have a member that is undefined
			// We now proceed to define it 
			//
			Expression val = this [name];

			if (val == null) {
				
				int idx = ordered_enums.IndexOf (name);

				if (idx == 0)
					default_value = 0;
				else {
					for (int i = 0; i < idx; ++i) {
						string n = (string) ordered_enums [i];
						Location m_loc = (Location) member_to_location [n];
						default_value = LookupEnumValue (ec, n, m_loc);
					}
					
					default_value = GetNextDefaultValue (default_value);
				}
				
			} else {
				val = val.Resolve (ec);
				
				if (val == null) {
					Report.Error (-12, loc, "Definition is circular.");
					return null;
				}	
				
				if (IsValidEnumConstant (val)) {
					c = (Constant) val;
					default_value = c.GetValue ();
					
					if (default_value == null) {
						error31 (c, loc);
						return null;
					}
					
				} else {
					Report.Error (
						1008, loc,
						"Type byte, sbyte, short, ushort, int, uint, long, or " +
						"ulong expected");
					return null;
				}
			}

			FieldAttributes attr = FieldAttributes.Public | FieldAttributes.Static
					| FieldAttributes.Literal;
			
			FieldBuilder fb = EnumBuilder.DefineField (name, UnderlyingType, attr);
			
			try {
				default_value = Convert.ChangeType (default_value, UnderlyingType);
			} catch {
				error31 (c, loc);
				return null;
			}

			fb.SetConstant (default_value);
			field_builders.Add (fb);
			member_to_value [name] = default_value;

			if (!TypeManager.RegisterField (fb, default_value))
				return null;
			
			return default_value;
		}
		
		public override bool Define (TypeContainer parent)
		{
			//
			// If there was an error during DefineEnum, return
			//
			if (EnumBuilder == null)
				return false;
			
			EmitContext ec = new EmitContext (parent, Location, null, UnderlyingType, ModFlags);
			
			object default_value = 0;
			
			FieldAttributes attr = FieldAttributes.Public | FieldAttributes.Static
				             | FieldAttributes.Literal;

			
			foreach (string name in ordered_enums) {
				//
				// Have we already been defined, thanks to some cross-referencing ?
				// 
				if (member_to_value.Contains (name))
					continue;
				
				Location loc = (Location) member_to_location [name];

				if (this [name] != null) {
					default_value = LookupEnumValue (ec, name, loc);

					if (default_value == null)
						return true;

				} else {
					
					FieldBuilder fb = EnumBuilder.DefineField (name, UnderlyingType, attr);
					
					if (default_value == null) {
					   Report.Error (543, loc, "Enumerator value for '" + name + "' is too large to " +
							      "fit in its type");
						return false;
					}
					
					try {
						default_value = Convert.ChangeType (default_value, UnderlyingType);
					} catch {
						error31 (default_value, loc);
						return false;
					}

					fb.SetConstant (default_value);
					field_builders.Add (fb);
					member_to_value [name] = default_value;
					
					if (!TypeManager.RegisterField (fb, default_value))
						return false;
				}

				default_value = GetNextDefaultValue (default_value);
			}
			
			if (OptAttributes == null)
				return true;
			
			if (OptAttributes.AttributeSections == null)
				return true;
			
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

			return true;
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

		// indexer
		public Expression this [string name] {
			get {
				return (Expression) defined_names [name];
			}
		}
	}
}
