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
using System.Globalization;
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

		public Enum (TypeContainer parent, string type, int mod_flags, string name, Attributes attrs, Location l)
			: base (parent, name, l)
		{
			this.BaseType = type;
			ModFlags = Modifiers.Check (AllowedModifiers, mod_flags, Modifiers.PUBLIC, l);
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

		//
		// This is used by corlib compilation: we map from our
		// type to a type that is consumable by the DefineField
		//
		Type MapToInternalType (Type t)
		{
			if (t == TypeManager.int32_type)
				return typeof (int);
			if (t == TypeManager.int64_type)
				return typeof (long);
			if (t == TypeManager.uint32_type)
				return typeof (uint);
			if (t == TypeManager.uint64_type)
				return typeof (ulong);
			if (t == TypeManager.float_type)
				return typeof (float);
			if (t == TypeManager.double_type)
				return typeof (double);
			if (t == TypeManager.byte_type)
				return typeof (byte);
			if (t == TypeManager.sbyte_type)
				return typeof (sbyte);
			if (t == TypeManager.char_type)
				return typeof (char);
			if (t == TypeManager.short_type)
				return typeof (short);
			if (t == TypeManager.ushort_type)
				return typeof (ushort);

			throw new Exception ();
		}
		
		public override TypeBuilder DefineType ()
		{
			if (TypeBuilder != null)
				return TypeBuilder;

			TypeAttributes attr = TypeAttributes.Class | TypeAttributes.Sealed;

			UnderlyingType = TypeManager.LookupType (BaseType);

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
					      "long, or ulong expected (got: " +
					      TypeManager.CSharpName (UnderlyingType) + ")");
				return null;
			}

			if (IsTopLevel) {
				ModuleBuilder builder = CodeGen.ModuleBuilder;

				if ((ModFlags & Modifiers.PUBLIC) != 0)
					attr |= TypeAttributes.Public;
				else
					attr |= TypeAttributes.NotPublic;
				
				TypeBuilder = builder.DefineType (Name, attr, TypeManager.enum_type);
			} else {
				TypeBuilder builder = Parent.TypeBuilder;

				if ((ModFlags & Modifiers.PUBLIC) != 0)
					attr |= TypeAttributes.NestedPublic;
				else
					attr |= TypeAttributes.NestedPrivate;

				
				TypeBuilder = builder.DefineNestedType (
					Basename, attr, TypeManager.enum_type);
			}

			//
			// Call MapToInternalType for corlib
			//
			TypeBuilder.DefineField ("value__", UnderlyingType,
						 FieldAttributes.Public | FieldAttributes.SpecialName
						 | FieldAttributes.RTSpecialName);

			TypeManager.AddEnumType (Name, TypeBuilder, this);

			return TypeBuilder;
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

		void Error_ConstantValueCannotBeConverted (object val, Location loc)
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

		// Function to convert an object to another type and return
		// it as an object. In place for the core data types to use
		// when implementing IConvertible. Uses hardcoded indexes in 
		// the conversionTypes array, so if modify carefully.
		private static object ChangeEnumType (object value, Type conversionType)
		{
			if (!(value is IConvertible))
				throw new ArgumentException ();

			IConvertible convertValue = (IConvertible) value;
			CultureInfo ci = CultureInfo.CurrentCulture;
			NumberFormatInfo provider = ci.NumberFormat;

			//
			// We must use Type.Equals() here since `conversionType' is
			// the TypeBuilder created version of a system type and not
			// the system type itself.  You cannot use Type.GetTypeCode()
			// on such a type - it'd always return TypeCode.Object.
			//
			if (conversionType.Equals (typeof (Boolean)))
				return (object)(convertValue.ToBoolean (provider));
			else if (conversionType.Equals (typeof (Byte)))
				return (object)(convertValue.ToByte (provider));
			else if (conversionType.Equals (typeof (Char)))
				return (object)(convertValue.ToChar (provider));
			else if (conversionType.Equals (typeof (DateTime)))
				return (object)(convertValue.ToDateTime (provider));
			else if (conversionType.Equals (typeof (Decimal)))
				return (object)(convertValue.ToDecimal (provider));
			else if (conversionType.Equals (typeof (Double)))
				return (object)(convertValue.ToDouble (provider));
			else if (conversionType.Equals (typeof (Int16)))
				return (object)(convertValue.ToInt16 (provider));
			else if (conversionType.Equals (typeof (Int32)))
				return (object)(convertValue.ToInt32 (provider));
			else if (conversionType.Equals (typeof (Int64)))
				return (object)(convertValue.ToInt64 (provider));
			else if (conversionType.Equals (typeof (SByte)))
				return (object)(convertValue.ToSByte (provider));
			else if (conversionType.Equals (typeof (Single)))
				return (object)(convertValue.ToSingle (provider));
			else if (conversionType.Equals (typeof (String)))
				return (object)(convertValue.ToString (provider));
			else if (conversionType.Equals (typeof (UInt16)))
				return (object)(convertValue.ToUInt16 (provider));
			else if (conversionType.Equals (typeof (UInt32)))
				return (object)(convertValue.ToUInt32 (provider));
			else if (conversionType.Equals (typeof (UInt64)))
				return (object)(convertValue.ToUInt64 (provider));
			else if (conversionType.Equals (typeof (Object)))
				return (object)(value);
			else 
				throw new InvalidCastException ();
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

			//
			// This may happen if we're calling a method in System.Enum, for instance
			// Enum.IsDefined().
			//
			if (!defined_names.Contains (name))
				return null;

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
						Location m_loc = (Mono.CSharp.Location)
							member_to_location [n];
						default_value = LookupEnumValue (ec, n, m_loc);
					}
					
					default_value = GetNextDefaultValue (default_value);
				}
				
			} else {
				bool old = ec.InEnumContext;
				ec.InEnumContext = true;
				val = val.Resolve (ec);
				ec.InEnumContext = old;
				
				if (val == null) {
					Report.Error (-12, loc, "Definition is circular.");
					return null;
				}

				if (IsValidEnumConstant (val)) {
					c = (Constant) val;
					default_value = c.GetValue ();
					
					if (default_value == null) {
						Error_ConstantValueCannotBeConverted (c, loc);
						return null;
					}
					
				} else {
					Report.Error (
						1008, loc,
						"Type byte, sbyte, short, ushort, int, uint, long, or " +
						"ulong expected (have: " + val + ")");
					return null;
				}
			}

			FieldAttributes attr = FieldAttributes.Public | FieldAttributes.Static
					| FieldAttributes.Literal;
			
			FieldBuilder fb = TypeBuilder.DefineField (name, UnderlyingType, attr);

			try {
				if (RootContext.StdLib)
					default_value = Convert.ChangeType (default_value, UnderlyingType);
				else
					default_value = ChangeEnumType (default_value, UnderlyingType);
			} catch {
				Error_ConstantValueCannotBeConverted (c, loc);
				return null;
			}

			fb.SetConstant (default_value);
			field_builders.Add (fb);
			member_to_value [name] = default_value;

			if (!TypeManager.RegisterFieldValue (fb, default_value))
				return null;
			
			return default_value;
		}
		
		public override bool Define (TypeContainer parent)
		{
			//
			// If there was an error during DefineEnum, return
			//
			if (TypeBuilder == null)
				return false;
			
			EmitContext ec = new EmitContext (parent, this, Location, null,
							  UnderlyingType, ModFlags, false);
			
			object default_value = 0;
			
			FieldAttributes attr = FieldAttributes.Public | FieldAttributes.Static
				             | FieldAttributes.Literal;

			
			foreach (string name in ordered_enums) {
				//
				// Have we already been defined, thanks to some cross-referencing ?
				// 
				if (member_to_value.Contains (name))
					continue;
				
				Location loc = (Mono.CSharp.Location) member_to_location [name];

				if (this [name] != null) {
					default_value = LookupEnumValue (ec, name, loc);

					if (default_value == null)
						return true;

				} else {
					FieldBuilder fb = TypeBuilder.DefineField (
						name, UnderlyingType, attr);
					
					if (default_value == null) {
					   Report.Error (543, loc, "Enumerator value for '" + name + "' is too large to " +
							      "fit in its type");
						return false;
					}
					
					try {
						if (RootContext.StdLib)
							default_value = Convert.ChangeType (default_value, UnderlyingType);
						else
							default_value = ChangeEnumType (default_value, UnderlyingType);
					} catch {
						Error_ConstantValueCannotBeConverted (default_value, loc);
						return false;
					}

					fb.SetConstant (default_value);
					field_builders.Add (fb);
					member_to_value [name] = default_value;
					
					if (!TypeManager.RegisterFieldValue (fb, default_value))
						return false;
				}

				default_value = GetNextDefaultValue (default_value);
			}
			
			Attribute.ApplyAttributes (ec, TypeBuilder, this, OptAttributes, Location);

			return true;
		}
		
		//
		// Hack around System.Reflection as found everywhere else
		//
		public MemberInfo [] FindMembers (MemberTypes mt, BindingFlags bf,
						  MemberFilter filter, object criteria)
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
