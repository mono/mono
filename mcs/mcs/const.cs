//
// const.cs: Constant declarations.
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Marek Safar (marek.safar@seznam.cz)
//
// (C) 2001 Ximian, Inc.
//
//

namespace Mono.CSharp {

	using System;
	using System.Reflection;
	using System.Reflection.Emit;
	using System.Collections;

	public interface IConstant
	{
		void CheckObsoleteness (Location loc);
		bool ResolveValue ();
		Constant Value { get; }
	}

	public class Const : FieldMember, IConstant {
		Constant value;
		bool in_transit;

		public const int AllowedModifiers =
			Modifiers.NEW |
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.PRIVATE;

		public Const (DeclSpace parent, Expression constant_type, string name,
			      Expression expr, int mod_flags, Attributes attrs, Location loc)
			: base (parent, constant_type, mod_flags, AllowedModifiers,
				new MemberName (name, loc), attrs)
		{
			initializer = expr;
			ModFlags |= Modifiers.STATIC;
		}

		protected override bool CheckBase ()
		{
			// Constant.Define can be called when the parent type hasn't yet been populated
			// and it's base types need not have been populated.  So, we defer this check
			// to the second time Define () is called on this member.
			if (ParentContainer.BaseCache == null)
				return true;
			return base.CheckBase ();
		}

		/// <summary>
		///   Defines the constant in the @parent
		/// </summary>
		public override bool Define ()
		{
			// Make Define () idempotent, but ensure that the error check happens.
			if (FieldBuilder != null)
				return base.CheckBase ();

			if (!base.Define ())
				return false;

			Type ttype = MemberType;
			while (ttype.IsArray)
			    ttype = TypeManager.GetElementType (ttype);

			FieldAttributes field_attr = FieldAttributes.Static | Modifiers.FieldAttr (ModFlags);
			// Decimals cannot be emitted into the constant blob.  So, convert to 'readonly'.
			if (ttype == TypeManager.decimal_type) {
				field_attr |= FieldAttributes.InitOnly;
				ParentContainer.RegisterFieldForInitialization (this);
			}
			else {
				field_attr |= FieldAttributes.Literal;
			}

			FieldBuilder = Parent.TypeBuilder.DefineField (Name, MemberType, field_attr);

			TypeManager.RegisterConstant (FieldBuilder, this);

			return true;
		}
		
		/// <summary>
		///  Emits the field value by evaluating the expression
		/// </summary>
		public override void Emit ()
		{
			if (!ResolveValue ())
				return;

			if (value.Type == TypeManager.decimal_type) {
				Decimal d = ((DecimalConstant)value).Value;
				int[] bits = Decimal.GetBits (d);
				object[] args = new object[] { (byte)(bits [3] >> 16), (byte)(bits [3] >> 31), (uint)bits [2], (uint)bits [1], (uint)bits [0] };
				CustomAttributeBuilder cab = new CustomAttributeBuilder (TypeManager.decimal_constant_attribute_ctor, args);
				FieldBuilder.SetCustomAttribute (cab);
			}
			else{
				FieldBuilder.SetConstant (value.GetTypedValue ());
			}

			base.Emit ();
		}

		public static void Error_ExpressionMustBeConstant (Location loc, string e_name)
		{
			Report.Error (133, loc, "The expression being assigned to `{0}' must be constant", e_name);
		}

		public static void Error_CyclicDeclaration (MemberCore mc)
		{
			Report.Error (110, mc.Location, "The evaluation of the constant value for `{0}' involves a circular definition",
				mc.GetSignatureForError ());
		}

		public static void Error_ConstantCanBeInitializedWithNullOnly (Location loc, string name)
		{
			Report.Error (134, loc, "`{0}': the constant of reference type other than string can only be initialized with null",
				name);
		}

		#region IConstant Members

		public bool ResolveValue ()
		{
			if (value != null)
				return true;

			SetMemberIsUsed ();
			if (in_transit) {
				Error_CyclicDeclaration (this);
				// Suppress cyclic errors
				value = New.Constantify (MemberType);
				return false;
			}

			in_transit = true;
			// TODO: IResolveContext here
			EmitContext ec = new EmitContext (this, Parent, Location, null, MemberType, ModFlags);
			value = initializer.ResolveAsConstant (ec, this);
			in_transit = false;

			if (value == null)
				return false;

			value = value.ToType (MemberType, Location);
			if (value == null)
				return false;

			if (!MemberType.IsValueType && MemberType != TypeManager.string_type && !value.IsDefaultValue) {
				Error_ConstantCanBeInitializedWithNullOnly (Location, GetSignatureForError ());
				return false;
			}

			return true;
		}

		public Constant Value {
			get {
				return value;
			}
		}

		#endregion
	}

	public class ExternalConstant : IConstant
	{
		FieldInfo fi;
		Constant value;

		public ExternalConstant (FieldInfo fi)
		{
			this.fi = fi;
		}

		private ExternalConstant (FieldInfo fi, Constant value):
			this (fi)
		{
			this.value = value;
		}

		//
		// Decimal constants cannot be encoded in the constant blob, and thus are marked
		// as IsInitOnly ('readonly' in C# parlance).  We get its value from the 
		// DecimalConstantAttribute metadata.
		//
		public static IConstant CreateDecimal (FieldInfo fi)
		{
			if (fi is FieldBuilder)
				return null;
			
			object[] attrs = fi.GetCustomAttributes (TypeManager.decimal_constant_attribute_type, false);
			if (attrs.Length != 1)
				return null;

			IConstant ic = new ExternalConstant (fi,
				new DecimalConstant (((System.Runtime.CompilerServices.DecimalConstantAttribute) attrs [0]).Value, Location.Null));

			return ic;
		}

		#region IConstant Members

		public void CheckObsoleteness (Location loc)
		{
			ObsoleteAttribute oa = AttributeTester.GetMemberObsoleteAttribute (fi);
			if (oa == null) {
				return;
			}

			AttributeTester.Report_ObsoleteMessage (oa, TypeManager.GetFullNameSignature (fi), loc);
		}

		public bool ResolveValue ()
		{
			if (value != null)
				return true;

			if (fi.DeclaringType.IsEnum) {
				value = Expression.Constantify (fi.GetValue (fi), TypeManager.EnumToUnderlying (fi.FieldType));
				value = new EnumConstant (value, fi.DeclaringType);
				return true;
			}

			value = Expression.Constantify (fi.GetValue (fi), fi.FieldType);
			return true;
		}

		public Constant Value {
			get {
				return value;
			}
		}

		#endregion
	}

}
