//
// enum.cs: Enum handling.
//
// Author: Miguel de Icaza (miguel@gnu.org)
//         Ravi Pratap     (ravi@ximian.com)
//         Anirban Bhattacharjee (banirban@novell.com)
//         Jambunathan K (kjambunathan@novell.com)
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

	public class EnumMember : MemberCore
	{
		Enum parent_enum;
		Expression expr;
		int index;
		
		FieldBuilder builder;
		object enum_value;

		bool in_transit = false;
		FieldAttributes field_attrs = FieldAttributes.Public | FieldAttributes.Static
						| FieldAttributes.Literal;

		public EnumMember (Enum parent_enum, Expression expr, string name,
				   Location loc, Attributes attrs, int index):
			base (name, attrs, loc)
		{
			this.parent_enum = parent_enum;
			this.expr = expr;
			this.index = index;
		}

		public override void ApplyAttributeBuilder(Attribute a, CustomAttributeBuilder cb)
		{
			builder.SetCustomAttribute (cb);
		}

		public override AttributeTargets AttributeTargets {
			get {
				return AttributeTargets.Field;
			}
		}

		public override bool Define (TypeContainer tc)
		{
			throw new NotImplementedException ();
		}

		public object GetValue ()
		{
			if (builder == null) 
				DefineMember ();

			return enum_value;
		}

		public FieldBuilder DefineMember ()
		{
			if (builder != null)
				return builder;

			DoDefineMember ();

			if (builder == null)
				parent_enum.RemoveEnumMember (this.Name);

			return builder;
		}

		public FieldBuilder DoDefineMember ()
		{
			object default_value = null;
			Constant c = null;
			Type UnderlyingType = parent_enum.UnderlyingType;
			EmitContext ec = parent_enum.EmitContext;
			
			default_value = enum_value;
			
			if (in_transit) {
				Report.Error (30500, Location, "The evaluation of the constant value for `" +
					      parent_enum.Name + "." + Name + "' involves a circular definition.");
				return null;
			}

			//
			// So if the above doesn't happen, we have a member that is undefined
			// We now proceed to define it 
			//
			Expression val = expr;
			int idx = index;

			if (val == null) {
				if (idx == 0)
					default_value = 0;
				else {
					int i = idx - 1;
					EnumMember em = parent_enum [i];

					in_transit = true;
					default_value = em.GetValue ();
					in_transit = false;

					if (default_value == null)
						return null;
					
					default_value = Enum.GetNextDefaultValue (default_value, UnderlyingType);
				}
				
			} else {
				bool old = ec.InEnumContext;
				ec.InEnumContext = true;
				in_transit = true;
				val = val.Resolve (ec);
				in_transit = false;
				ec.InEnumContext = old;

				if (val == null)
					return null;

				if (! Enum.IsValidEnumConstant (val)) {
					Report.Error (
						30650, Location,
						"Type byte, sbyte, short, ushort, int, uint, long, or " +
						"ulong expected (have: " + val + ")");
					return null;
				}

				c = (Constant) val;
			
				default_value = c.GetValue ();

				if (default_value == null) {
					Enum.Error_ConstantValueCannotBeConverted (c, Location, UnderlyingType);
					return null;
				}
			}

			builder = parent_enum.TypeBuilder.DefineField (Name, parent_enum.TypeBuilder, field_attrs);

			try {
				//FXME: ChangeType is not the right thing to do. 
				default_value = TypeManager.ChangeType (default_value, UnderlyingType);
			} catch {
				Enum.Error_ConstantValueCannotBeConverted (c, Location, UnderlyingType);
				return null;
			}

			enum_value = default_value;

			builder.SetConstant (enum_value);

			if (!TypeManager.RegisterFieldValue (builder, enum_value))
				return null;

			if (OptAttributes != null)
				OptAttributes.Emit (ec, this);

			return builder;
		}

	}
	

	/// <summary>
	///   Enumeration container
	/// </summary>
	public class Enum : DeclSpace {
		ArrayList ordered_enums;
		public Expression BaseType;
		public Type UnderlyingType;
		ArrayList field_builders;
		
		public const int AllowedModifiers =
			Modifiers.NEW |
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.PRIVATE;

		EmitContext emit_context;

		public Enum (TypeContainer parent, Expression type, int mod_flags, string name, Attributes attrs, Location l)
			: base (parent, name, attrs, l)
		{
			this.BaseType = type;
			ModFlags = Modifiers.Check (AllowedModifiers, mod_flags,
						    IsTopLevel ? Modifiers.INTERNAL : Modifiers.PUBLIC, l);

			ordered_enums = new ArrayList ();
			field_builders = new ArrayList ();
		}

		public override AttributeTargets AttributeTargets {
			get {
				return AttributeTargets.Enum;
			}
		}

		/// <summary>
		///   Adds @name to the enumeration space, with @expr
		///   being its definition.  
		/// </summary>
		public AdditionResult AddEnumMember (string name, Expression expr, Location loc,
						     Attributes opt_attrs)
		{
			int index;
			EnumMember em;
			
			if (defined_names.Contains (name))
				return AdditionResult.NameExists;

			index = ordered_enums.Add (name);
			em = new EnumMember (this, expr, name, loc, opt_attrs, index);
			DefineName (name, em);

			return AdditionResult.Success;
		}

		public void RemoveEnumMember (string name)
		{
			defined_names.Remove (name);
		}

		public override TypeBuilder DefineType ()
		{
			if (TypeBuilder != null)
				return TypeBuilder;

			emit_context = new EmitContext (Parent, this, Location, null,
							  UnderlyingType, ModFlags, false);

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

			TypeBuilder.DefineField ("value__", UnderlyingType,
						 FieldAttributes.Public | FieldAttributes.SpecialName
						 | FieldAttributes.RTSpecialName);

			TypeManager.AddEnumType (Name, TypeBuilder, this);

			return TypeBuilder;
		}

	    	public static bool IsValidEnumConstant (Expression e)
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

		static public object GetNextDefaultValue (object default_value, Type UnderlyingType)
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

		public static void Error_ConstantValueCannotBeConverted (object val, Location loc, Type UnderlyingType)
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
		///  This is used to lookup the value of an enum member. If the member is undefined,
		///  it attempts to define it and return its value
		/// </summary>
		public object LookupEnumValue (string name)
		{
			object default_value = null;
			Constant c = null;
			EnumMember em;

			// first check whether the requested name is there
			// in the member list of enum
			bool found = false;


			em = this [name];

			if (em == null)
				return null;
			
			name = em.Name;
			FieldBuilder fb = em.DefineMember ();
			
			if (fb == null) {
				return null;
			} 

			if (! field_builders.Contains (fb)) {
				field_builders.Add (fb);
			}

			return em.GetValue ();
		}

		public override bool DefineMembers (TypeContainer parent)
		{
			return true;
		}
		
		public override bool Define (TypeContainer parent)
		{
			if (TypeBuilder == null)
				return false;
			
			EmitContext ec = new EmitContext (parent, this, Location, null,
							  UnderlyingType, ModFlags, false);
			
			foreach (string name in ordered_enums) {
				EnumMember em = this [name];
				FieldBuilder fb = em.DefineMember ();

				if (fb == null) {
					return false;
				} 

				if (!field_builders.Contains (fb)) {
					field_builders.Add (fb);
				}
			}

			if (OptAttributes != null)
				OptAttributes.Emit (EmitContext, this);
			
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
				
				if (criteria is string){
					LookupEnumValue ((string) criteria);
				}
				
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
		public EnumMember this [string name] {
			get {
				EnumMember em = (EnumMember) defined_names [name];

				if (em != null)
					return em;

				name = name.ToLower();
				foreach (string nm in ordered_enums) {
					if (nm.ToLower() == name) {
						em = (EnumMember) defined_names [name];
						break;
					}
				}

				return em;
			}
		}

		public EnumMember this[int mem_index] {
			get {
				string mem_name = (string) ordered_enums [mem_index];;
				return (EnumMember) defined_names[mem_name];
			}
		}

		public EmitContext EmitContext {
			get {
				return emit_context;
			}
		}
	}
}
