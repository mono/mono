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

using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Mono.CSharp {

	public class Const : FieldBase
	{
		bool define_called;

		public const Modifiers AllowedModifiers =
			Modifiers.NEW |
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.PRIVATE;

		public Const (DeclSpace parent, FullNamedExpression type, string name,
			      Expression expr, Modifiers mod_flags, Attributes attrs, Location loc)
			: base (parent, type, mod_flags, AllowedModifiers,
				new MemberName (name, loc), attrs)
		{
			if (expr != null)
				initializer = new ConstInitializer (this, expr);

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

			FieldAttributes field_attr = FieldAttributes.Static | ModifiersExtensions.FieldAttr (ModFlags);
			// Decimals cannot be emitted into the constant blob.  So, convert to 'readonly'.
			if (ttype == TypeManager.decimal_type) {
				field_attr |= FieldAttributes.InitOnly;
			} else {
				field_attr |= FieldAttributes.Literal;
			}

			FieldBuilder = Parent.TypeBuilder.DefineField (Name, MemberType, field_attr);
			spec = new ConstSpec (this, FieldBuilder, ModFlags, initializer);

			TypeManager.RegisterConstant (FieldBuilder, (ConstSpec) spec);
			Parent.MemberCache.AddMember (FieldBuilder, this);

			if ((field_attr & FieldAttributes.InitOnly) != 0)
				Parent.PartialContainer.RegisterFieldForInitialization (this,
					new FieldInitializer (this, initializer, this));

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
			var value = initializer.Resolve (new ResolveContext (this)) as Constant;
			if (value == null || FieldBuilder == null)
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
	}

	public class ConstSpec : FieldSpec
	{
		Expression value;

		public ConstSpec (IMemberDefinition definition, FieldInfo fi, Modifiers mod, Expression value)
			: base (definition, fi, mod)
		{
			this.value = value;
		}

		public Expression Value {
			get {
				return value;
			}
			set {
				this.value = value;
			}
		}
	}

	class ConstInitializer : ShimExpression
	{
		bool in_transit;
		protected readonly FieldBase field;

		public ConstInitializer (FieldBase field, Expression value)
			: base (value)
		{
			if (value != null)
				this.loc = value.Location;

			this.field = field;
		}

		protected override Expression DoResolve (ResolveContext unused)
		{
			if (type != null)
				return expr;

			var opt = ResolveContext.Options.ConstantScope;
			if (field is EnumMember)
				opt |= ResolveContext.Options.EnumScope;

			//
			// Use a context in which the constant was declared and
			// not the one in which is referenced
			//
			var rc = new ResolveContext (field, opt);
			expr = DoResolveInitializer (rc);
			type = expr.Type;

			return expr;
		}

		protected virtual Expression DoResolveInitializer (ResolveContext rc)
		{
			if (in_transit) {
				field.Compiler.Report.Error (110, field.Location,
					"The evaluation of the constant value for `{0}' involves a circular definition",
					field.GetSignatureForError ());

				expr = null;
			} else {
				in_transit = true;
				expr = expr.Resolve (rc);
			}

			in_transit = false;

			if (expr != null) {
				Constant c = expr as Constant;
				if (c != null)
					c = field.ConvertInitializer (rc, c);

				if (c == null) {
					if (TypeManager.IsReferenceType (field.MemberType))
						Error_ConstantCanBeInitializedWithNullOnly (rc, field.MemberType, loc, field.GetSignatureForError ());
					else if (!(expr is Constant))
						Error_ExpressionMustBeConstant (rc, field.Location, field.GetSignatureForError ());
					else
						expr.Error_ValueCannotBeConverted (rc, loc, field.MemberType, false);
				}

				expr = c;
			}

			if (expr == null) {
				expr = New.Constantify (field.MemberType);
				if (expr == null)
					expr = Constant.CreateConstantFromValue (field.MemberType, null, Location);
				expr = expr.Resolve (rc);
			}


			return expr;
		}
	}
}
