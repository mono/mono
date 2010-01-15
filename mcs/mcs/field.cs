//
// field.cs: All field handlers
//
// Authors: Miguel de Icaza (miguel@gnu.org)
//          Martin Baulig (martin@ximian.com)
//          Marek Safar (marek.safar@seznam.cz)
//
// Dual licensed under the terms of the MIT X11 or GNU GPL
//
// Copyright 2001, 2002, 2003 Ximian, Inc (http://www.ximian.com)
// Copyright 2004-2008 Novell, Inc
//

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace Mono.CSharp
{
	//
	// Abstract class for all fields
	//
	abstract public class FieldBase : MemberBase
	{
		public FieldBuilder FieldBuilder;
		protected FieldSpec spec;
		public Status status;
		protected Expression initializer;

		[Flags]
		public enum Status : byte {
			HAS_OFFSET = 4		// Used by FieldMember.
		}

		static readonly string[] attribute_targets = new string [] { "field" };

		protected FieldBase (DeclSpace parent, FullNamedExpression type, Modifiers mod,
				     Modifiers allowed_mod, MemberName name, Attributes attrs)
			: base (parent, null, type, mod, allowed_mod | Modifiers.ABSTRACT, Modifiers.PRIVATE,
				name, attrs)
		{
			if ((mod & Modifiers.ABSTRACT) != 0)
				Report.Error (681, Location, "The modifier 'abstract' is not valid on fields. Try using a property instead");
		}

		public override AttributeTargets AttributeTargets {
			get {
				return AttributeTargets.Field;
			}
		}

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb, PredefinedAttributes pa)
		{
			if (a.Type == pa.FieldOffset) {
				status |= Status.HAS_OFFSET;

				if (!Parent.PartialContainer.HasExplicitLayout) {
					Report.Error (636, Location, "The FieldOffset attribute can only be placed on members of types marked with the StructLayout(LayoutKind.Explicit)");
					return;
				}

				if ((ModFlags & Modifiers.STATIC) != 0 || this is Const) {
					Report.Error (637, Location, "The FieldOffset attribute is not allowed on static or const fields");
					return;
				}
			}

			if (a.Type == pa.FixedBuffer) {
				Report.Error (1716, Location, "Do not use 'System.Runtime.CompilerServices.FixedBuffer' attribute. Use the 'fixed' field modifier instead");
				return;
			}

#if false
			if (a.Type == pa.MarshalAs) {
				UnmanagedMarshal marshal = a.GetMarshal (this);
				if (marshal != null) {
					FieldBuilder.SetMarshal (marshal);
				}
				return;
			}
#endif
			if ((a.HasSecurityAttribute)) {
				a.Error_InvalidSecurityParent ();
				return;
			}

			if (a.Type == pa.Dynamic) {
				a.Error_MisusedDynamicAttribute ();
				return;
			}

			FieldBuilder.SetCustomAttribute (cb);
		}

 		protected override bool CheckBase ()
		{
 			if (!base.CheckBase ())
 				return false;
 
 			MemberInfo conflict_symbol = Parent.PartialContainer.FindBaseMemberWithSameName (Name, false);
 			if (conflict_symbol == null) {
 				if ((ModFlags & Modifiers.NEW) != 0) {
 					Report.Warning (109, 4, Location, "The member `{0}' does not hide an inherited member. The new keyword is not required", GetSignatureForError ());
 				}
 				return true;
 			}
 
 			if ((ModFlags & (Modifiers.NEW | Modifiers.OVERRIDE | Modifiers.BACKING_FIELD)) == 0) {
				Report.SymbolRelatedToPreviousError (conflict_symbol);
				Report.Warning (108, 2, Location, "`{0}' hides inherited member `{1}'. Use the new keyword if hiding was intended",
					GetSignatureForError (), TypeManager.GetFullNameSignature (conflict_symbol));
			}
 
 			return true;
 		}

		public virtual Constant ConvertInitializer (ResolveContext rc, Constant expr)
		{
			return expr.ConvertImplicitly (rc, MemberType);
		}

		protected override void DoMemberTypeDependentChecks ()
		{
			base.DoMemberTypeDependentChecks ();

			if (TypeManager.IsGenericParameter (MemberType))
				return;

			if (MemberType.IsSealed && MemberType.IsAbstract) {
				Error_VariableOfStaticClass (Location, GetSignatureForError (), MemberType, Report);
			}

			CheckBase ();
			IsTypePermitted ();
		}

		//
		//   Represents header string for documentation comment.
		//
		public override string DocCommentHeader {
			get { return "F:"; }
		}

		public override void Emit ()
		{
			if (TypeManager.IsDynamicType (member_type)) {
				PredefinedAttributes.Get.Dynamic.EmitAttribute (FieldBuilder);
			} else {
				var trans_flags = TypeManager.HasDynamicTypeUsed (member_type);
				if (trans_flags != null) {
					var pa = PredefinedAttributes.Get.DynamicTransform;
					if (pa.Constructor != null || pa.ResolveConstructor (Location, TypeManager.bool_type.MakeArrayType ())) {
						FieldBuilder.SetCustomAttribute (new CustomAttributeBuilder (pa.Constructor, new object[] { trans_flags }));
					}
				}
			}

			if ((ModFlags & Modifiers.COMPILER_GENERATED) != 0 && !Parent.IsCompilerGenerated)
				PredefinedAttributes.Get.CompilerGenerated.EmitAttribute (FieldBuilder);

			if (OptAttributes != null) {
				OptAttributes.Emit ();
			}

			if (((status & Status.HAS_OFFSET) == 0) && (ModFlags & (Modifiers.STATIC | Modifiers.BACKING_FIELD)) == 0 && Parent.PartialContainer.HasExplicitLayout) {
				Report.Error (625, Location, "`{0}': Instance field types marked with StructLayout(LayoutKind.Explicit) must have a FieldOffset attribute", GetSignatureForError ());
			}

			base.Emit ();
		}

		public static void Error_VariableOfStaticClass (Location loc, string variable_name, Type static_class, Report Report)
		{
			Report.SymbolRelatedToPreviousError (static_class);
			Report.Error (723, loc, "`{0}': cannot declare variables of static types",
				variable_name);
		}

		public Expression Initializer {
			get {
				return initializer;
			}
			set {
				if (value != null) {
					this.initializer = value;
				}
			}
		}

		protected virtual bool IsFieldClsCompliant {
			get {
				if (FieldBuilder == null)
					return true;

				return AttributeTester.IsClsCompliant (FieldBuilder.FieldType);
			}
		}

		public FieldSpec Spec {
			get { return spec; }
		}

		public override string[] ValidAttributeTargets 
		{
			get {
				return attribute_targets;
			}
		}

		protected override bool VerifyClsCompliance ()
		{
			if (!base.VerifyClsCompliance ())
				return false;

			if (!IsFieldClsCompliant) {
				Report.Warning (3003, 1, Location, "Type of `{0}' is not CLS-compliant",
					GetSignatureForError ());
			}
			return true;
		}

		public void SetAssigned ()
		{
			caching_flags |= Flags.IsAssigned;
		}
	}

	//
	// Field specification
	//
	public class FieldSpec : MemberSpec
	{
		FieldInfo info;

		public FieldSpec (IMemberDefinition definition, FieldInfo info, Modifiers modifiers)
			: base (MemberKind.Field, definition, info.Name, modifiers)
		{
			this.info = info;
		}

		public bool IsReadOnly {
			get { return (Modifiers & Modifiers.READONLY) != 0; }
		}

		public FieldInfo MetaInfo {
			get {
				return info;
			}
			set {
				info = value;
			}
		}

		public override Type DeclaringType {
			get {
				return MetaInfo.DeclaringType;
			}
		}

		// Obsolete
		public Type FieldType {
			get {
				 return MetaInfo.FieldType;
			}
		}
	}

	/// <summary>
	/// Fixed buffer implementation
	/// </summary>
	public class FixedField : FieldBase
	{
		public const string FixedElementName = "FixedElementField";
		static int GlobalCounter = 0;
		static object[] ctor_args = new object[] { (short)LayoutKind.Sequential };
		static FieldInfo[] fi;

		TypeBuilder fixed_buffer_type;

		const Modifiers AllowedModifiers =
			Modifiers.NEW |
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.PRIVATE |
			Modifiers.UNSAFE;

		public FixedField (DeclSpace parent, FullNamedExpression type, Modifiers mod, string name,
			Expression size_expr, Attributes attrs, Location loc):
			base (parent, type, mod, AllowedModifiers, new MemberName (name, loc), attrs)
		{
			if (RootContext.Version < LanguageVersion.ISO_2)
				Report.FeatureIsNotAvailable (loc, "fixed size buffers");

			initializer = new ConstInitializer (this, size_expr);
		}

		public override Constant ConvertInitializer (ResolveContext rc, Constant expr)
		{
			return expr.ImplicitConversionRequired (rc, TypeManager.int32_type, Location);
		}

		public override bool Define ()
		{
			if (!base.Define ())
				return false;

			if (!TypeManager.IsPrimitiveType (MemberType)) {
				Report.Error (1663, Location,
					"`{0}': Fixed size buffers type must be one of the following: bool, byte, short, int, long, char, sbyte, ushort, uint, ulong, float or double",
					GetSignatureForError ());
			}			
			
			// Create nested fixed buffer container
			string name = String.Format ("<{0}>__FixedBuffer{1}", Name, GlobalCounter++);
			fixed_buffer_type = Parent.TypeBuilder.DefineNestedType (name, Parent.Module.DefaultCharSetType |
				TypeAttributes.NestedPublic | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit, TypeManager.value_type);
			
			var element = fixed_buffer_type.DefineField (FixedElementName, MemberType, FieldAttributes.Public);
			RootContext.RegisterCompilerGeneratedType (fixed_buffer_type);
			
			FieldBuilder = Parent.TypeBuilder.DefineField (Name, fixed_buffer_type, ModifiersExtensions.FieldAttr (ModFlags));
			spec = new FixedFieldSpec (this, FieldBuilder, element, ModFlags);

			Parent.MemberCache.AddMember (FieldBuilder, spec);
			TypeManager.RegisterFieldBase (FieldBuilder, this);

			return true;
		}

		protected override void DoMemberTypeIndependentChecks ()
		{
			base.DoMemberTypeIndependentChecks ();

			if (!IsUnsafe)
				Expression.UnsafeError (Report, Location);

			if (Parent.PartialContainer.Kind != Kind.Struct) {
				Report.Error (1642, Location, "`{0}': Fixed size buffer fields may only be members of structs",
					GetSignatureForError ());
			}
		}

		public override void Emit()
		{
			ResolveContext rc = new ResolveContext (this);
			IntConstant buffer_size_const = initializer.Resolve (rc) as IntConstant;
			if (buffer_size_const == null)
				return;

			int buffer_size = buffer_size_const.Value;

			if (buffer_size <= 0) {
				Report.Error (1665, Location, "`{0}': Fixed size buffers must have a length greater than zero", GetSignatureForError ());
				return;
			}

			int type_size = Expression.GetTypeSize (MemberType);

			if (buffer_size > int.MaxValue / type_size) {
				Report.Error (1664, Location, "Fixed size buffer `{0}' of length `{1}' and type `{2}' exceeded 2^31 limit",
					GetSignatureForError (), buffer_size.ToString (), TypeManager.CSharpName (MemberType));
				return;
			}

			buffer_size *= type_size;
			EmitFieldSize (buffer_size);

			PredefinedAttributes.Get.UnsafeValueType.EmitAttribute (fixed_buffer_type);

			base.Emit ();
		}

		void EmitFieldSize (int buffer_size)
		{
			CustomAttributeBuilder cab;
			PredefinedAttribute pa;

			pa = PredefinedAttributes.Get.StructLayout;
			if (pa.Constructor == null &&
				!pa.ResolveConstructor (Location, TypeManager.short_type))
					return;

			// TODO: It's not cleared
			if (fi == null)
				fi = new FieldInfo[] { pa.Type.GetField ("Size") };

			object[] fi_val = new object[] { buffer_size };
			cab = new CustomAttributeBuilder (pa.Constructor,
				ctor_args, fi, fi_val);
			fixed_buffer_type.SetCustomAttribute (cab);
			
			//
			// Don't emit FixedBufferAttribute attribute for private types
			//
			if ((ModFlags & Modifiers.PRIVATE) != 0)
				return;

			pa = PredefinedAttributes.Get.FixedBuffer;
			if (pa.Constructor == null &&
				!pa.ResolveConstructor (Location, TypeManager.type_type, TypeManager.int32_type))
				return;

			cab = new CustomAttributeBuilder (pa.Constructor, new object[] { MemberType, buffer_size });
			FieldBuilder.SetCustomAttribute (cab);
		}

		protected override bool IsFieldClsCompliant {
			get {
				return false;
			}
		}

		public void SetCharSet (TypeAttributes ta)
		{
			TypeAttributes cta = fixed_buffer_type.Attributes;
			if ((cta & TypeAttributes.UnicodeClass) != (ta & TypeAttributes.UnicodeClass))
				SetTypeBuilderCharSet ((cta & ~TypeAttributes.AutoClass) | TypeAttributes.UnicodeClass);
			else if ((cta & TypeAttributes.AutoClass) != (ta & TypeAttributes.AutoClass))
				SetTypeBuilderCharSet ((cta & ~TypeAttributes.UnicodeClass) | TypeAttributes.AutoClass);
			else if (cta == 0 && ta != 0)
				SetTypeBuilderCharSet (cta & ~(TypeAttributes.UnicodeClass | TypeAttributes.AutoClass));
		}

		void SetTypeBuilderCharSet (TypeAttributes ta)
		{
			MethodInfo mi = typeof (TypeBuilder).GetMethod ("SetCharSet", BindingFlags.Instance | BindingFlags.NonPublic);
			if (mi == null) {
				Report.RuntimeMissingSupport (Location, "TypeBuilder::SetCharSet");
			} else {
				mi.Invoke (fixed_buffer_type, new object [] { ta });
			}
		}
	}

	class FixedFieldSpec : FieldSpec
	{
		readonly FieldInfo element;

		public FixedFieldSpec (IMemberDefinition definition, FieldInfo info, FieldInfo element, Modifiers modifiers)
			 : base (definition, info, modifiers)
		{
			this.element = element;
		}

		public FieldInfo Element {
			get {
				return element;
			}
		}

		public Type ElementType {
			get {
				return element.FieldType;
			}
		}
	}

	//
	// The Field class is used to represents class/struct fields during parsing.
	//
	public class Field : FieldBase {
		// <summary>
		//   Modifiers allowed in a class declaration
		// </summary>
		const Modifiers AllowedModifiers =
			Modifiers.NEW |
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.PRIVATE |
			Modifiers.STATIC |
			Modifiers.VOLATILE |
			Modifiers.UNSAFE |
			Modifiers.READONLY;

		public Field (DeclSpace parent, FullNamedExpression type, Modifiers mod, MemberName name,
			      Attributes attrs)
			: base (parent, type, mod, AllowedModifiers, name, attrs)
		{
		}

		bool CanBeVolatile ()
		{
			if (TypeManager.IsReferenceType (MemberType))
				return true;

			if (MemberType == TypeManager.bool_type || MemberType == TypeManager.char_type ||
				MemberType == TypeManager.sbyte_type || MemberType == TypeManager.byte_type ||
				MemberType == TypeManager.short_type || MemberType == TypeManager.ushort_type ||
				MemberType == TypeManager.int32_type || MemberType == TypeManager.uint32_type ||
				MemberType == TypeManager.float_type ||
				MemberType == TypeManager.intptr_type || MemberType == TypeManager.uintptr_type)
				return true;

			if (TypeManager.IsEnumType (MemberType))
				return true;

			return false;
		}

		bool CheckStructLayout (Type type, bool isStatic)
		{
			if (TypeManager.IsBuiltinType (type))
				return true;

			if (isStatic) {
				if (!TypeManager.IsValueType (type) || TypeManager.IsEqual (type, Parent.TypeBuilder))
					return true;
			}

			if (!TypeManager.IsEqual (TypeManager.DropGenericTypeArguments (type), Parent.TypeBuilder)) {
				if (!TypeManager.IsGenericType (type))
					return true;

				foreach (Type t in TypeManager.GetTypeArguments (type)) {
					if (!CheckStructLayout (t, false))
						return false;
				}
				return true;
			}
			
			Report.Error (523, Location,
				"Struct member `{0}' of type `{1}' causes a cycle in the struct layout",
				GetSignatureForError (), TypeManager.CSharpName (MemberType));
			return false;
		}

		public override bool Define ()
		{
			if (!base.Define ())
				return false;

			try {
				Type[] required_modifier = null;
				if ((ModFlags & Modifiers.VOLATILE) != 0) {
					if (TypeManager.isvolatile_type == null)
						TypeManager.isvolatile_type = TypeManager.CoreLookupType (Compiler,
							"System.Runtime.CompilerServices", "IsVolatile", Kind.Class, true);

					if (TypeManager.isvolatile_type != null)
						required_modifier = new Type [] { TypeManager.isvolatile_type };
				}

				FieldBuilder = Parent.TypeBuilder.DefineField (
					Name, MemberType, required_modifier, null, ModifiersExtensions.FieldAttr (ModFlags));

				spec = new FieldSpec (this, FieldBuilder, ModFlags);

				// Don't cache inaccessible fields
				if ((ModFlags & Modifiers.BACKING_FIELD) == 0) {
					Parent.MemberCache.AddMember (FieldBuilder, spec);
				}

				TypeManager.RegisterFieldBase (FieldBuilder, this);
			}
			catch (ArgumentException) {
				Report.RuntimeMissingSupport (Location, "`void' or `void*' field type");
				return false;
			}

			if (initializer != null) {
				((TypeContainer) Parent).RegisterFieldForInitialization (this,
					new FieldInitializer (this, initializer, this));
			} else {
				if (Parent.PartialContainer.Kind == Kind.Struct)
					CheckStructLayout (member_type, (ModFlags & Modifiers.STATIC) != 0);
			}

			return true;
		}

		protected override void DoMemberTypeDependentChecks ()
		{
			base.DoMemberTypeDependentChecks ();

			if ((ModFlags & Modifiers.VOLATILE) != 0) {
				if (!CanBeVolatile ()) {
					Report.Error (677, Location, "`{0}': A volatile field cannot be of the type `{1}'",
						GetSignatureForError (), TypeManager.CSharpName (MemberType));
				}

				if ((ModFlags & Modifiers.READONLY) != 0) {
					Report.Error (678, Location, "`{0}': A field cannot be both volatile and readonly",
						GetSignatureForError ());
				}
			}
		}

		protected override bool VerifyClsCompliance ()
		{
			if (!base.VerifyClsCompliance ())
				return false;

			if ((ModFlags & Modifiers.VOLATILE) != 0) {
				Report.Warning (3026, 1, Location, "CLS-compliant field `{0}' cannot be volatile", GetSignatureForError ());
			}

			return true;
		}
	}
}
