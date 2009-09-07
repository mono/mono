//
// const.cs: Constant declarations.
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Marek Safar (marek.safar@seznam.cz)
//
// Copyright 2001-2003 Ximian, Inc.
// Copyright 2003-2008 Novell, Inc.
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
		Constant CreateConstantReference (Location loc);
	}

	public class Const : FieldBase, IConstant {
		protected Constant value;
		bool in_transit;
		bool resolved;
		bool define_called;

		public const int AllowedModifiers =
			Modifiers.NEW |
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.PRIVATE;

		public Const (DeclSpace parent, FullNamedExpression type, string name,
			      Expression expr, int mod_flags, Attributes attrs, Location loc)
			: base (parent, type, mod_flags, AllowedModifiers,
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
			if (Parent.PartialContainer.BaseCache == null)
				return true;
			return base.CheckBase ();
		}

		/// <summary>
		///   Defines the constant in the @parent
		/// </summary>
		public override bool Define ()
		{
			// Because constant define can be called from other class
			if (define_called) {
				CheckBase ();
				return FieldBuilder != null;
			}

			define_called = true;

			if (!base.Define ())
				return false;

			Type ttype = MemberType;
			if (!IsConstantTypeValid (ttype)) {
				Error_InvalidConstantType (ttype, Location, Report);
			}

			// If the constant is private then we don't need any field the
			// value is already inlined and cannot be referenced
			//if ((ModFlags & Modifiers.PRIVATE) != 0 && RootContext.Optimize)
			//	return true;

			FieldAttributes field_attr = FieldAttributes.Static | Modifiers.FieldAttr (ModFlags);
			// Decimals cannot be emitted into the constant blob.  So, convert to 'readonly'.
			if (ttype == TypeManager.decimal_type) {
				field_attr |= FieldAttributes.InitOnly;
			} else {
				field_attr |= FieldAttributes.Literal;
			}

			FieldBuilder = Parent.TypeBuilder.DefineField (Name, MemberType, field_attr);
			TypeManager.RegisterConstant (FieldBuilder, this);
			Parent.MemberCache.AddMember (FieldBuilder, this);

			if ((field_attr & FieldAttributes.InitOnly) != 0)
				Parent.PartialContainer.RegisterFieldForInitialization (this,
					new FieldInitializer (FieldBuilder, initializer, this));

			return true;
		}

		public static bool IsConstantTypeValid (Type t)
		{
			if (TypeManager.IsBuiltinOrEnum (t))
				return true;

			if (TypeManager.IsGenericParameter (t) || t.IsPointer)
				return false;

			return TypeManager.IsReferenceType (t);
		}

		/// <summary>
		///  Emits the field value by evaluating the expression
		/// </summary>
		public override void Emit ()
		{
			if (!ResolveValue ())
				return;

			if (FieldBuilder == null)
				return;

			if (value.Type == TypeManager.decimal_type) {
				FieldBuilder.SetCustomAttribute (CreateDecimalConstantAttribute (value));
			} else{
				FieldBuilder.SetConstant (value.GetTypedValue ());
			}

			base.Emit ();
		}

		public static CustomAttributeBuilder CreateDecimalConstantAttribute (Constant c)
		{
			PredefinedAttribute pa = PredefinedAttributes.Get.DecimalConstant;
			if (pa.Constructor == null &&
				!pa.ResolveConstructor (c.Location, TypeManager.byte_type, TypeManager.byte_type,
					TypeManager.uint32_type, TypeManager.uint32_type, TypeManager.uint32_type))
				return null;

			Decimal d = (Decimal) c.GetValue ();
			int [] bits = Decimal.GetBits (d);
			object [] args = new object [] { 
				(byte) (bits [3] >> 16),
				(byte) (bits [3] >> 31),
				(uint) bits [2], (uint) bits [1], (uint) bits [0]
			};

			return new CustomAttributeBuilder (pa.Constructor, args);
		}

		public static void Error_ExpressionMustBeConstant (Location loc, string e_name, Report Report)
		{
			Report.Error (133, loc, "The expression being assigned to `{0}' must be constant", e_name);
		}

		public static void Error_CyclicDeclaration (MemberCore mc, Report Report)
		{
			Report.Error (110, mc.Location, "The evaluation of the constant value for `{0}' involves a circular definition",
				mc.GetSignatureForError ());
		}

		public static void Error_ConstantCanBeInitializedWithNullOnly (Type type, Location loc, string name, Report Report)
		{
			Report.Error (134, loc, "A constant `{0}' of reference type `{1}' can only be initialized with null",
				name, TypeManager.CSharpName (type));
		}

		public static void Error_InvalidConstantType (Type t, Location loc, Report Report)
		{
			if (TypeManager.IsGenericParameter (t)) {
				Report.Error (1959, loc,
					"Type parameter `{0}' cannot be declared const", TypeManager.CSharpName (t));
			} else {
				Report.Error (283, loc,
					"The type `{0}' cannot be declared const", TypeManager.CSharpName (t));
			}
		}

		#region IConstant Members

		public bool ResolveValue ()
		{
			if (resolved)
				return value != null;

			SetMemberIsUsed ();
			if (in_transit) {
				Error_CyclicDeclaration (this, Report);
				// Suppress cyclic errors
				value = New.Constantify (MemberType);
				resolved = true;
				return false;
			}

			in_transit = true;

			ResolveContext.Options opt = ResolveContext.Options.ConstantScope;
			if (this is EnumMember)
				opt |= ResolveContext.Options.EnumScope;

			ResolveContext rc = new ResolveContext (this, opt);
			value = DoResolveValue (rc);

			in_transit = false;
			resolved = true;
			return value != null;
		}

		protected virtual Constant DoResolveValue (ResolveContext ec)
		{
			Constant value = initializer.ResolveAsConstant (ec, this);
			if (value == null)
				return null;

			Constant c = value.ConvertImplicitly (MemberType);
			if (c == null) {
				if (TypeManager.IsReferenceType (MemberType))
					Error_ConstantCanBeInitializedWithNullOnly (MemberType, Location, GetSignatureForError (), Report);
				else
					value.Error_ValueCannotBeConverted (ec, Location, MemberType, false);
			}

			return c;
		}

		public virtual Constant CreateConstantReference (Location loc)
		{
			if (value == null)
				return null;

			return Constant.CreateConstant (value.Type, value.GetValue(), loc);
		}

		#endregion
	}

	public class ExternalConstant : IConstant
	{
		FieldInfo fi;
		object value;

		public ExternalConstant (FieldInfo fi)
		{
			this.fi = fi;
		}

		private ExternalConstant (FieldInfo fi, object value):
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

			PredefinedAttribute pa = PredefinedAttributes.Get.DecimalConstant;
			if (!pa.IsDefined)
				return null;

			object[] attrs = fi.GetCustomAttributes (pa.Type, false);
			if (attrs.Length != 1)
				return null;

			IConstant ic = new ExternalConstant (fi,
				((System.Runtime.CompilerServices.DecimalConstantAttribute) attrs [0]).Value);

			return ic;
		}

		#region IConstant Members

		public void CheckObsoleteness (Location loc)
		{
			ObsoleteAttribute oa = AttributeTester.GetMemberObsoleteAttribute (fi);
			if (oa == null) {
				return;
			}

			AttributeTester.Report_ObsoleteMessage (oa, TypeManager.GetFullNameSignature (fi), loc, RootContext.ToplevelTypes.Compiler.Report);
		}

		public bool ResolveValue ()
		{
			if (value != null)
				return true;

			value = fi.GetValue (fi);
			return true;
		}

		public Constant CreateConstantReference (Location loc)
		{
			return Constant.CreateConstant (TypeManager.TypeToCoreType (fi.FieldType), value, loc);
		}

		#endregion
	}

}
