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
using System.Globalization;

namespace Mono.CSharp {

	class EnumMember: MemberCore {
		static string[] attribute_targets = new string [] { "field" };

		Enum parent_enum;
		public FieldBuilder builder;
		internal readonly Expression Type;

		public EnumMember (Enum parent_enum, Expression expr, string name,
				   Location loc, Attributes attrs):
			base (null, new MemberName (name), attrs, loc)
		{
			this.parent_enum = parent_enum;
			this.ModFlags = parent_enum.ModFlags;
			this.Type = expr;
		}

		public override void ApplyAttributeBuilder(Attribute a, CustomAttributeBuilder cb)
		{
			if (a.Type == TypeManager.marshal_as_attr_type) {
				UnmanagedMarshal marshal = a.GetMarshal (this);
				if (marshal != null) {
					builder.SetMarshal (marshal);
				}
				return;
			}

			if (a.Type.IsSubclassOf (TypeManager.security_attr_type)) {
				a.Error_InvalidSecurityParent ();
				return;
			}

			builder.SetCustomAttribute (cb);
		}

		public override AttributeTargets AttributeTargets {
			get {
				return AttributeTargets.Field;
			}
		}

		public void DefineMember (TypeBuilder tb)
		{
			FieldAttributes attr = FieldAttributes.Public | FieldAttributes.Static
				| FieldAttributes.Literal;
			
			builder = tb.DefineField (Name, tb, attr);
		}

		public override bool Define ()
		{
			throw new NotImplementedException ();
		}

		public void Emit (EmitContext ec)
		{
			if (OptAttributes != null)
				OptAttributes.Emit (ec, this); 

			Emit ();
		}

		public override string GetSignatureForError()
		{
			return String.Concat (parent_enum.GetSignatureForError (), '.', base.GetSignatureForError ());
		}

		public override string[] ValidAttributeTargets {
			get {
				return attribute_targets;
			}
		}

		protected override bool VerifyClsCompliance(DeclSpace ds)
		{
			// Because parent is TypeContainer and we have only DeclSpace parent.
			// Parameter replacing is required
			return base.VerifyClsCompliance (parent_enum);
		}

		// There is no base type
		protected override void VerifyObsoleteAttribute()
		{
		}

		public override string DocCommentHeader {
			get { return "F:"; }
		}
	}

	/// <summary>
	///   Enumeration container
	/// </summary>
	public class Enum : DeclSpace {
		public ArrayList ordered_enums;
		
		public Expression BaseType;
		
		public Type UnderlyingType;

		Hashtable member_to_location;

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

		public Enum (NamespaceEntry ns, TypeContainer parent, Expression type,
			     int mod_flags, MemberName name, Attributes attrs, Location l)
			: base (ns, parent, name, attrs, l)
		{
			this.BaseType = type;
			ModFlags = Modifiers.Check (AllowedModifiers, mod_flags,
						    IsTopLevel ? Modifiers.INTERNAL : Modifiers.PRIVATE, l);

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
		public void AddEnumMember (string name, Expression expr, Location loc, Attributes opt_attrs, string documentation)
		{
			if (name == "value__") {
				Report.Error (76, loc, "An item in an enumeration can't have an identifier `value__'");
				return;
			}

			EnumMember em = new EnumMember (this, expr, name, loc, opt_attrs);
			em.DocComment = documentation;
			if (!AddToContainer (em, false, name, ""))
				return;


			// TODO: can be almost deleted
			ordered_enums.Add (name);
			member_to_location.Add (name, loc);
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

			if (!(BaseType is TypeLookupExpression)) {
				Report.Error (1008, Location,
					      "Type byte, sbyte, short, ushort, int, uint, " +
					      "long, or ulong expected (got: `{0}')", BaseType);
				return null;
			}

			UnderlyingType = ResolveType (BaseType, false, Location);

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
				if (TypeManager.NamespaceClash (Name, Location))
					return null;
				
				ModuleBuilder builder = CodeGen.Module.Builder;

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
			    e is UShortConstant || e is ULongConstant || e is EnumConstant ||
			    e is CharConstant)
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

		//
		// Horrible, horrible.  But there is no other way we can pass the EmitContext
		// to the recursive definition triggered by the evaluation of a forward
		// expression
		//
		static EmitContext current_ec = null;
		
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

			if (val == null) {
				
				int idx = ordered_enums.IndexOf (name);

				if (idx == 0)
					default_value = 0;
				else {
					for (int i = 0; i < idx; ++i) {
						string n = (string) ordered_enums [i];
						Location m_loc = (Mono.CSharp.Location)
							member_to_location [n];
						in_transit.Add (name, true);

						EmitContext old_ec = current_ec;
						current_ec = ec;
			
						default_value = LookupEnumValue (ec, n, m_loc);

						current_ec = old_ec;
						
						in_transit.Remove (name);
						if (default_value == null)
							return null;
					}
					
					default_value = GetNextDefaultValue (default_value);
				}
				
			} else {
				bool old = ec.InEnumContext;
				ec.InEnumContext = true;
				in_transit.Add (name, true);

				EmitContext old_ec = current_ec;
				current_ec = ec;
				val = val.Resolve (ec);
				current_ec = old_ec;
				
				in_transit.Remove (name);
				ec.InEnumContext = old;

				if (val == null)
					return null;

				if (!IsValidEnumConstant (val)) {
					Report.Error (
						1008, loc,
						"Type byte, sbyte, short, ushort, int, uint, long, or " +
						"ulong expected (have: " + val + ")");
					return null;
				}

				c = (Constant) val;
				default_value = c.GetValue ();

				if (default_value == null) {
					Error_ConstantValueCannotBeConverted (c, loc);
					return null;
				}

				if (val is EnumConstant){
					Type etype = TypeManager.EnumToUnderlying (c.Type);
					
					if (!ImplicitConversionExists (etype, UnderlyingType)){
						Convert.Error_CannotImplicitConversion (
							loc, c.Type, UnderlyingType);
						return null;
					}
				}
			}

			EnumMember em = (EnumMember) defined_names [name];
			em.DefineMember (TypeBuilder);

			bool fail;
			default_value = TypeManager.ChangeType (default_value, UnderlyingType, out fail);
			if (fail){
				Error_ConstantValueCannotBeConverted (c, loc);
				return null;
			}

			em.builder.SetConstant (default_value);
			field_builders.Add (em.builder);
			member_to_value [name] = default_value;

			if (!TypeManager.RegisterFieldValue (em.builder, default_value))
				return null;

			return default_value;
		}

		public override bool DefineMembers (TypeContainer parent)
		{
			return true;
		}
		
		public override bool Define ()
		{
			//
			// If there was an error during DefineEnum, return
			//
			if (TypeBuilder == null)
				return false;

			ec = new EmitContext (this, this, Location, null, UnderlyingType, ModFlags, false);
			
			object default_value = 0;
			
		
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
					if (name == "value__"){
						Report.Error (76, loc, "The name `value__' is reserved for enumerations");
						return false;
					}

					EnumMember em = (EnumMember) defined_names [name];

					em.DefineMember (TypeBuilder);
					FieldBuilder fb = em.builder;
					
					if (default_value == null) {
					   Report.Error (543, loc, "Enumerator value for '" + name + "' is too large to " +
							      "fit in its type");
						return false;
					}

					bool fail;
					default_value = TypeManager.ChangeType (default_value, UnderlyingType, out fail);
					if (fail){
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

			return true;
		}

		public override void Emit ()
		{
			if (OptAttributes != null) {
				OptAttributes.Emit (ec, this);
			}

			foreach (EnumMember em in defined_names.Values) {
				em.Emit (ec);
			}

			base.Emit ();
		}
		
 		void VerifyClsName ()
  		{
 			Hashtable ht = new Hashtable ();
 			foreach (string name in ordered_enums) {
 				string locase = name.ToLower (System.Globalization.CultureInfo.InvariantCulture);
 				if (!ht.Contains (locase)) {
 					ht.Add (locase, defined_names [name]);
 					continue;
 				}
 
 				MemberCore conflict = (MemberCore)ht [locase];
 				Report.SymbolRelatedToPreviousError (conflict);
 				conflict = GetDefinition (name);
 				Report.Error (3005, conflict.Location, "Identifier '{0}' differing only in case is not CLS-compliant", conflict.GetSignatureForError ());
  			}
  		}

		protected override bool VerifyClsCompliance (DeclSpace ds)
		{
			if (!base.VerifyClsCompliance (ds))
				return false;

			VerifyClsName ();

			if (!AttributeTester.IsClsCompliant (UnderlyingType)) {
				Report.Error (3009, Location, "'{0}': base type '{1}' is not CLS-compliant", GetSignatureForError (), TypeManager.CSharpName (UnderlyingType));
			}

			return true;
		}
		
		/// <summary>
		/// Returns full enum name.
		/// </summary>
		string GetEnumeratorName (string valueName)
		{
			return String.Concat (Name, ".", valueName);
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
					if (member_to_value [criteria] == null && current_ec != null){
						LookupEnumValue (current_ec, (string) criteria, Location.Null);
					}
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
		public Expression this [string name] {
			get {
				return ((EnumMember) defined_names [name]).Type;
			}
		}

		public override AttributeTargets AttributeTargets {
			get {
				return AttributeTargets.Enum;
			}
		}

		protected override void VerifyObsoleteAttribute()
		{
			// UnderlyingType is never obsolete
		}

		//
		// Generates xml doc comments (if any), and if required,
		// handle warning report.
		//
		internal override void GenerateDocComment (DeclSpace ds)
		{
			DocUtil.GenerateEnumDocComment (this, ds);
		}

		//
		//   Represents header string for documentation comment.
		//
		public override string DocCommentHeader {
			get { return "T:"; }
		}
	}
}
