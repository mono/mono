//
// enum.cs: Enum handling.
//
// Author: Miguel de Icaza (miguel@gnu.org)
//         Ravi Pratap     (ravi@ximian.com)
//         Anirban Bhattacharjee (banirban@novell.com)
//
// Licensed under the terms of the GNU GPL
//
// (C) 2001 Ximian, Inc (http://www.ximian.com)
//

using System;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;

namespace Mono.MonoBASIC {

	/// <summary>
	///   Enumeration container
	/// </summary>
	public class Enum : DeclSpace {
		ArrayList ordered_enums;
		
		public Expression BaseType;
		public Attributes  OptAttributes;
		
		public Type UnderlyingType;

		Hashtable member_to_location;
		Hashtable member_to_attributes;

		//
		// This is for members that have been defined
		//
		Hashtable member_to_value;

		//
		// This is used to mark members we're currently defining
		//
		Hashtable in_transit;
		
		ArrayList field_builders;
		
		public const int AllowedModifiers =
			Modifiers.NEW |
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.PRIVATE;

		public Enum (TypeContainer parent, Expression type, int mod_flags, string name, Attributes attrs, Location l)
			: base (parent, name, l)
		{
			this.BaseType = type;
			ModFlags = Modifiers.Check (AllowedModifiers, mod_flags,
						    IsTopLevel ? Modifiers.INTERNAL : Modifiers.PUBLIC, l);
			OptAttributes = attrs;

			ordered_enums = new ArrayList ();
			member_to_location = new Hashtable ();
			member_to_value = new Hashtable ();
			in_transit = new Hashtable ();
			field_builders = new ArrayList ();
		}

		/// <summary>
		///   Adds @name to the enumeration space, with @expr
		///   being its definition.  
		/// </summary>
		public AdditionResult AddEnumMember (string name, Expression expr, Location loc,
						     Attributes opt_attrs)
		{
			if (defined_names.Contains (name))
				return AdditionResult.NameExists;

			DefineName (name, expr);

			ordered_enums.Add (name);
			member_to_location.Add (name, loc);

			if (member_to_attributes == null)
				member_to_attributes = new Hashtable ();

			member_to_attributes.Add (name, opt_attrs);
			
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

			TypeAttributes attr = Modifiers.TypeAttr (ModFlags, IsTopLevel);

			attr |= TypeAttributes.Class | TypeAttributes.Sealed;

			UnderlyingType = ResolveType (BaseType, false, Location);

			if (UnderlyingType != TypeManager.int32_type &&
			    UnderlyingType != TypeManager.uint32_type &&
			    UnderlyingType != TypeManager.int64_type &&
			    UnderlyingType != TypeManager.uint64_type &&
			    UnderlyingType != TypeManager.short_type &&
			    UnderlyingType != TypeManager.ushort_type &&
			    UnderlyingType != TypeManager.byte_type  &&
			    UnderlyingType != TypeManager.sbyte_type) {
				Report.Error (30650, Location,
					      "Type byte, sbyte, short, ushort, int, uint, " +
					      "long, or ulong expected (got: " +
					      TypeManager.MonoBASIC_Name (UnderlyingType) + ")");
				return null;
			}

			if (IsTopLevel) {
				ModuleBuilder builder = CodeGen.ModuleBuilder;

				TypeBuilder = builder.DefineType (Name, attr, TypeManager.enum_type);
			} else {
				TypeBuilder builder = Parent.TypeBuilder;

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
				Report.Error (30439, loc, "Constant value '" + ((Constant) val).AsString () +
					      "' cannot be converted" +
					      " to a " + TypeManager.MonoBASIC_Name (UnderlyingType));
			else 
				Report.Error (30439, loc, "Constant value '" + val +
					      "' cannot be converted" +
					      " to a " + TypeManager.MonoBASIC_Name (UnderlyingType));
			return;
		}

		/// <summary>
		///  Determines if a standard implicit conversion exists from
		///  expr_type to target_type
		/// </summary>
		public static bool ImplicitConversionExists (Type expr_type, Type target_type)
		{
			expr_type = TypeManager.TypeToCoreType (expr_type);

			if (expr_type == TypeManager.void_type)
				return false;
			
			if (expr_type == target_type)
				return true;

			// First numeric conversions 

			if (expr_type == TypeManager.sbyte_type){
				//
				// From sbyte to short, int, long, float, double.
				//
				if ((target_type == TypeManager.int32_type) || 
				    (target_type == TypeManager.int64_type) ||
				    (target_type == TypeManager.double_type) ||
				    (target_type == TypeManager.float_type)  ||
				    (target_type == TypeManager.short_type) ||
				    (target_type == TypeManager.decimal_type))
					return true;
				
			} else if (expr_type == TypeManager.byte_type){
				//
				// From byte to short, ushort, int, uint, long, ulong, float, double
				// 
				if ((target_type == TypeManager.short_type) ||
				    (target_type == TypeManager.ushort_type) ||
				    (target_type == TypeManager.int32_type) ||
				    (target_type == TypeManager.uint32_type) ||
				    (target_type == TypeManager.uint64_type) ||
				    (target_type == TypeManager.int64_type) ||
				    (target_type == TypeManager.float_type) ||
				    (target_type == TypeManager.double_type) ||
				    (target_type == TypeManager.decimal_type))
					return true;
	
			} else if (expr_type == TypeManager.short_type){
				//
				// From short to int, long, float, double
				// 
				if ((target_type == TypeManager.int32_type) ||
				    (target_type == TypeManager.int64_type) ||
				    (target_type == TypeManager.double_type) ||
				    (target_type == TypeManager.float_type) ||
				    (target_type == TypeManager.decimal_type))
					return true;
					
			} else if (expr_type == TypeManager.ushort_type){
				//
				// From ushort to int, uint, long, ulong, float, double
				//
				if ((target_type == TypeManager.uint32_type) ||
				    (target_type == TypeManager.uint64_type) ||
				    (target_type == TypeManager.int32_type) ||
				    (target_type == TypeManager.int64_type) ||
				    (target_type == TypeManager.double_type) ||
				    (target_type == TypeManager.float_type) ||
				    (target_type == TypeManager.decimal_type))
					return true;
				    
			} else if (expr_type == TypeManager.int32_type){
				//
				// From int to long, float, double
				//
				if ((target_type == TypeManager.int64_type) ||
				    (target_type == TypeManager.double_type) ||
				    (target_type == TypeManager.float_type) ||
				    (target_type == TypeManager.decimal_type))
					return true;
					
			} else if (expr_type == TypeManager.uint32_type){
				//
				// From uint to long, ulong, float, double
				//
				if ((target_type == TypeManager.int64_type) ||
				    (target_type == TypeManager.uint64_type) ||
				    (target_type == TypeManager.double_type) ||
				    (target_type == TypeManager.float_type) ||
				    (target_type == TypeManager.decimal_type))
					return true;
					
			} else if ((expr_type == TypeManager.uint64_type) ||
				   (expr_type == TypeManager.int64_type)) {
				//
				// From long/ulong to float, double
				//
				if ((target_type == TypeManager.double_type) ||
				    (target_type == TypeManager.float_type) ||
				    (target_type == TypeManager.decimal_type))
					return true;
				    
			} else if (expr_type == TypeManager.char_type){
				//
				// From char to ushort, int, uint, long, ulong, float, double
				// 
				if ((target_type == TypeManager.ushort_type) ||
				    (target_type == TypeManager.int32_type) ||
				    (target_type == TypeManager.uint32_type) ||
				    (target_type == TypeManager.uint64_type) ||
				    (target_type == TypeManager.int64_type) ||
				    (target_type == TypeManager.float_type) ||
				    (target_type == TypeManager.double_type) ||
				    (target_type == TypeManager.decimal_type))
					return true;

			} else if (expr_type == TypeManager.float_type){
				//
				// float to double
				//
				if (target_type == TypeManager.double_type)
					return true;
			}	
			
			return false;
		}

		/// <summary>
		///  This is used to lookup the value of an enum member. If the member is undefined,
		///  it attempts to define it and return its value
		/// </summary>
		public object LookupEnumValue (EmitContext ec, string name, Location loc)
		{
			object default_value = null;
			Constant c = null;
			TypeContainer parent = ec.TypeContainer;

			// first check whether the requested name is there
			// in the member list of enum
			bool found = false;
			name = name.ToLower();
			foreach (string nm in ordered_enums) {
				if (nm.ToLower() == name) {
					name = nm;
					found = true;
					break;
				}
			}

			if (!found)
				Report.Error (30456, loc,
					name + " is not found in member list of enum " + this.Name);

			default_value = member_to_value [name];
			
			if (default_value != null)
				return default_value;

			//
			// This may happen if we're calling a method in System.Enum, for instance
			// Enum.IsDefined().
			//
			if (!defined_names.Contains (name))
				return null;

			if (in_transit.Contains (name)) {
				Report.Error (110, loc, "The evaluation of the constant value for `" +
					      Name + "." + name + "' involves a circular definition.");
				return null;
			}

			//
			// So if the above doesn't happen, we have a member that is undefined
			// We now proceed to define it 
			//
			Expression val = this [name];
			int idx = ordered_enums.IndexOf (name);

			if (val == null) {
				
				//int idx = ordered_enums.IndexOf (name);

				if (idx == 0)
					default_value = 0;
				else {
					for (int i = 0; i < idx; ++i) {
						string n = (string) ordered_enums [i];
						Location m_loc = (Mono.MonoBASIC.Location)
							member_to_location [n];
						in_transit.Add (name, true);
						default_value = LookupEnumValue (ec, n, m_loc);
						in_transit.Remove (name);
						if (default_value == null)
							return null;
					}
					
					default_value = GetNextDefaultValue (default_value);
				}
				
			} else {
				// check for any cyclic dependency
				if (val is Mono.MonoBASIC.SimpleName) {
					int var_idx = ordered_enums.IndexOf (val.ToString());

					if (idx <= var_idx)
						Report.Error(30500, loc,
							"The evaluation of the constant value for `" +
							Name + "." + name + "' involves a circular definition");
					else
						default_value = member_to_value [val.ToString()];
				}
				else {
					bool old = ec.InEnumContext;
					ec.InEnumContext = true;
					in_transit.Add (name, true);
					val = val.Resolve (ec);
					in_transit.Remove (name);
					ec.InEnumContext = old;

					if (val == null)
						return null;

					if (!IsValidEnumConstant (val)) {
						Report.Error (
							30650, loc,
							"Type byte, sbyte, short, ushort, int, uint, long, or " +
							"ulong expected (have: " + val + ")");
						return null;
					}

					c = (Constant) val;
				
					default_value = c.GetValue ();
				}

				if (default_value == null) {
					Error_ConstantValueCannotBeConverted (c, loc);
					return null;
				}

				if (val is EnumConstant) {
					Type etype = TypeManager.EnumToUnderlying (c.Type);
					
					if ( (!ImplicitConversionExists (etype, UnderlyingType)) &&
						(!Expression.NarrowingConversionExists (ec, val, UnderlyingType)) ){
						Expression.Error_CannotConvertImplicit (
							loc, c.Type, UnderlyingType);
						return null;
					}
				}
			}

			FieldAttributes attr = FieldAttributes.Public | FieldAttributes.Static
					| FieldAttributes.Literal;
			
			FieldBuilder fb = TypeBuilder.DefineField (name, TypeBuilder, attr);

			try {
				default_value = TypeManager.ChangeType (default_value, UnderlyingType);
			} catch {
				Error_ConstantValueCannotBeConverted (c, loc);
				return null;
			}

			fb.SetConstant (default_value);
			field_builders.Add (fb);
			member_to_value [name] = default_value;

			if (!TypeManager.RegisterFieldValue (fb, default_value))
				return null;

			//
			// Now apply attributes
			//
			Attribute.ApplyAttributes (ec, fb, fb, (Attributes) member_to_attributes [name], loc); 
			
			return default_value;
		}

		public override bool DefineMembers (TypeContainer parent)
		{
			return true;
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
			
			foreach (string name in ordered_enums) {
				if (member_to_value.Contains (name))
					continue;
				
				Location loc = (Mono.MonoBASIC.Location) member_to_location [name];

				if (this [name] != null) {
					default_value = LookupEnumValue (ec, name, loc);

					if (default_value == null)
						return true;
				} else {
					FieldAttributes attr = FieldAttributes.Public | FieldAttributes.Static
						| FieldAttributes.Literal;
			
					FieldBuilder fb = TypeBuilder.DefineField (name, TypeBuilder, attr);

					if (default_value == null) {
						Report.Error (30439, loc, "Enumerator value for '" + name + "' is too large to " +
							"fit in its type");
						return false;
					}

					try	{
						default_value = TypeManager.ChangeType (default_value, UnderlyingType);
					} catch	{
						Error_ConstantValueCannotBeConverted (default_value, loc);
						return false;
					}

					fb.SetConstant (default_value);
					field_builders.Add (fb);
					member_to_value [name] = default_value;

					if (!TypeManager.RegisterFieldValue (fb, default_value))
						return false;

					//
					// Now apply attributes
					//
					Attribute.ApplyAttributes (ec, fb, fb, (Attributes) member_to_attributes [name], loc); 
				}

				default_value = GetNextDefaultValue (default_value);
			}
			
			Attribute.ApplyAttributes (ec, TypeBuilder, this, OptAttributes, Location);
			return true;
		}
		
		//
		// IMemberFinder
		//
		public override MemberList FindMembers (MemberTypes mt, BindingFlags bf,
							MemberFilter filter, object criteria)
		{
			ArrayList members = new ArrayList ();

			if ((mt & MemberTypes.Field) != 0) {
				foreach (FieldBuilder fb in field_builders)
					if (filter (fb, criteria) == true)
						members.Add (fb);
			}

			return new MemberList (members);
		}

		public override MemberCache MemberCache {
			get {
				return null;
			}
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
