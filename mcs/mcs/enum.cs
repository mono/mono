//
// enum.cs: Enum handling.
//
// Author: Miguel de Icaza (miguel@gnu.org)
//         Ravi Pratap     (ravi@ximian.com)
//         Marek Safar     (marek.safar@seznam.cz)
//
// Dual licensed under the terms of the MIT X11 or GNU GPL
//
// Copyright 2001 Ximian, Inc (http://www.ximian.com)
// Copyright 2003-2003 Novell, Inc (http://www.novell.com)
//

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Globalization;

namespace Mono.CSharp {

	public class EnumMember : Const
	{
		class EnumTypeExpr : TypeExpr
		{
			protected override TypeExpr DoResolveAsTypeStep (IMemberContext ec)
			{
				type = ec.CurrentType;
				return this;
			}

			public override TypeExpr ResolveAsTypeTerminal (IMemberContext ec, bool silent)
			{
				return DoResolveAsTypeStep (ec);
			}
		}

		public EnumMember (Enum parent, EnumMember prev_member, string name, Expression expr,
				   Attributes attrs, Location loc)
			: base (parent, new EnumTypeExpr (), name, null, Modifiers.PUBLIC,
				attrs, loc)
		{
			initializer = new EnumInitializer (this, expr, prev_member);
		}

		static bool IsValidEnumType (TypeSpec t)
		{
			return (t == TypeManager.int32_type || t == TypeManager.uint32_type || t == TypeManager.int64_type ||
				t == TypeManager.byte_type || t == TypeManager.sbyte_type || t == TypeManager.short_type ||
				t == TypeManager.ushort_type || t == TypeManager.uint64_type || t == TypeManager.char_type ||
				TypeManager.IsEnumType (t));
		}

		public override Constant ConvertInitializer (ResolveContext rc, Constant expr)
		{
			if (expr is EnumConstant)
				expr = ((EnumConstant) expr).Child;

			var underlying = ((Enum) Parent).UnderlyingType;
			if (expr != null) {
				expr = expr.ImplicitConversionRequired (rc, underlying, Location);
				if (expr != null && !IsValidEnumType (expr.Type)) {
					Enum.Error_1008 (Location, Report);
					expr = null;
				}
			}

			if (expr == null)
				expr = New.Constantify (underlying);

			return new EnumConstant (expr, MemberType).Resolve (rc);
		}

		public override bool Define ()
		{
			if (!ResolveMemberType ())
				return false;

			const FieldAttributes attr = FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.Literal;
			FieldBuilder = Parent.TypeBuilder.DefineField (Name, MemberType.GetMetaInfo (), attr);
			spec = new ConstSpec (Parent.Definition, this, MemberType, FieldBuilder, ModFlags, initializer);

			Parent.MemberCache.AddMember (spec);
			return true;
		}
	}

	class EnumInitializer : ConstInitializer
	{
		EnumMember prev;

		public EnumInitializer (Const field, Expression init, EnumMember prev)
			: base (field, init)
		{
			this.prev = prev;
		}

		protected override Expression DoResolveInitializer (ResolveContext rc)
		{
			if (expr != null)
				return base.DoResolveInitializer (rc);

			if (prev == null)
				return field.ConvertInitializer (rc, null);

			try {
				var ec = prev.DefineValue () as EnumConstant;
				expr = ec.Increment ().Resolve (rc);
			} catch (OverflowException) {
				rc.Report.Error (543, field.Location,
					"The enumerator value `{0}' is outside the range of enumerator underlying type `{1}'",
					field.GetSignatureForError (),
					TypeManager.CSharpName (((Enum) field.Parent).UnderlyingType));

				expr = field.ConvertInitializer (rc, null);
			}		

			return expr;
		}
	}

	/// <summary>
	///   Enumeration container
	/// </summary>
	public class Enum : TypeContainer
	{
		public static readonly string UnderlyingValueField = "value__";

		TypeExpr base_type;

		const Modifiers AllowedModifiers =
			Modifiers.NEW |
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.PRIVATE;

		public Enum (NamespaceEntry ns, DeclSpace parent, TypeExpr type,
			     Modifiers mod_flags, MemberName name, Attributes attrs)
			: base (ns, parent, name, attrs, MemberKind.Enum)
		{
			this.base_type = type;
			var accmods = IsTopLevel ? Modifiers.INTERNAL : Modifiers.PRIVATE;
			ModFlags = ModifiersExtensions.Check (AllowedModifiers, mod_flags, accmods, Location, Report);
			spec = new EnumSpec (null, this, null, null, ModFlags);
		}

		public void AddEnumMember (EnumMember em)
		{
			if (em.Name == UnderlyingValueField) {
				Report.Error (76, em.Location, "An item in an enumeration cannot have an identifier `{0}'",
					UnderlyingValueField);
				return;
			}

			AddConstant (em);
		}

		public static void Error_1008 (Location loc, Report Report)
		{
			Report.Error (1008, loc, "Type byte, sbyte, short, ushort, " +
				      "int, uint, long or ulong expected");
		}

		protected override bool DefineNestedTypes ()
		{
			if (!base.DefineNestedTypes ())
				return false;

			((EnumSpec) spec).UnderlyingType = UnderlyingType;

			TypeBuilder.DefineField (UnderlyingValueField, UnderlyingType.GetMetaInfo (),
				FieldAttributes.Public | FieldAttributes.SpecialName | FieldAttributes.RTSpecialName);

			if (!RootContext.StdLib)
				RootContext.hack_corlib_enums.Add (this);

			return true;
		}

		protected override bool DoDefineMembers ()
		{
			DefineContainerMembers (constants);
			return true;
		}

		public override bool IsUnmanagedType ()
		{
			return true;
		}

		public TypeSpec UnderlyingType {
			get {
				return base_type.Type;
			}
		}

		protected override bool VerifyClsCompliance ()
		{
			if (!base.VerifyClsCompliance ())
				return false;

			if (UnderlyingType == TypeManager.uint32_type ||
				UnderlyingType == TypeManager.uint64_type ||
				UnderlyingType == TypeManager.ushort_type) {
				Report.Warning (3009, 1, Location, "`{0}': base type `{1}' is not CLS-compliant", GetSignatureForError (), TypeManager.CSharpName (UnderlyingType));
			}

			return true;
		}	

		public override AttributeTargets AttributeTargets {
			get {
				return AttributeTargets.Enum;
			}
		}

		protected override TypeAttributes TypeAttr {
			get {
				return ModifiersExtensions.TypeAttr (ModFlags, IsTopLevel) |
					TypeAttributes.Class | TypeAttributes.Sealed | base.TypeAttr;
			}
		}
	}

	class EnumSpec : TypeSpec
	{
		TypeSpec underlying;

		public EnumSpec (TypeSpec declaringType, ITypeDefinition definition, TypeSpec underlyingType, Type info, Modifiers modifiers)
			: base (MemberKind.Enum, declaringType, definition, info, modifiers | Modifiers.SEALED)
		{
			this.underlying = underlyingType;
		}

		public TypeSpec UnderlyingType {
			get {
				return underlying;
			}
			set {
				if (underlying != null)
					throw new InternalErrorException ("UnderlyingType reset");

				underlying = value;
			}
		}

		public static TypeSpec GetUnderlyingType (TypeSpec t)
		{
			return ((EnumSpec) t.GetDefinition ()).UnderlyingType;
		}
	}
}
