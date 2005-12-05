//
// enum.cs: Enum handling.
//
// Author: Miguel de Icaza (miguel@gnu.org)
//         Ravi Pratap     (ravi@ximian.com)
//         Marek Safar     (marek.safar@seznam.cz)
//
// Licensed under the terms of the GNU GPL
//
// (C) 2001 Ximian, Inc (http://www.ximian.com)
//

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Reflection;
using System.Reflection.Emit;
using System.Globalization;

namespace Mono.CSharp {

	public class EnumMember : MemberCore, IConstant {
		static string[] attribute_targets = new string [] { "field" };

		public FieldBuilder builder;

		readonly Enum parent_enum;
		readonly Expression ValueExpr;
		readonly EnumMember prev_member;

		Constant value;
		bool in_transit;

		public EnumMember (Enum parent_enum, EnumMember prev_member, Expression expr,
				MemberName name, Attributes attrs):
			base (parent_enum.Parent, name, attrs)
		{
			this.parent_enum = parent_enum;
			this.ModFlags = parent_enum.ModFlags;
			this.ValueExpr = expr;
			this.prev_member = prev_member;
		}

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb)
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

		bool IsValidEnumType (Type t)
		{
			return (t == TypeManager.int32_type || t == TypeManager.uint32_type || t == TypeManager.int64_type ||
				t == TypeManager.byte_type || t == TypeManager.sbyte_type || t == TypeManager.short_type ||
				t == TypeManager.ushort_type || t == TypeManager.uint64_type || t == TypeManager.char_type ||
				t.IsEnum);
		}
	
		public override bool Define ()
		{
			const FieldAttributes attr = FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.Literal;
			TypeBuilder tb = parent_enum.TypeBuilder;
			builder = tb.DefineField (Name, tb, attr);

			TypeManager.RegisterConstant (builder, this);
			return true;
		}

		// Because parent is TypeContainer and we have DeclSpace only
		public override void CheckObsoleteness (Location loc)
		{
			parent_enum.CheckObsoleteness (loc);

			ObsoleteAttribute oa = GetObsoleteAttribute ();
			if (oa == null) {
				return;
			}

			AttributeTester.Report_ObsoleteMessage (oa, GetSignatureForError (), loc);
		}

		public bool ResolveValue ()
		{
			if (value != null)
				return true;

			if (in_transit) {
				// suppress cyclic errors
				value = new EnumConstant (New.Constantify (parent_enum.UnderlyingType), parent_enum.TypeBuilder);
				Const.Error_CyclicDeclaration (this);
				return false;
			}

			if (ValueExpr != null) {
				in_transit = true;
				Constant c = ValueExpr.ResolveAsConstant (parent_enum.EmitContext, this);
				in_transit = false;

				if (c == null)
					return false;

				if (c is EnumConstant)
					c = ((EnumConstant)c).Child;
					
				c = c.ToType (parent_enum.UnderlyingType, Location);
				if (c == null)
					return false;

				if (!IsValidEnumType (c.Type)) {
					Report.Error (1008, Location, "Type byte, sbyte, short, ushort, int, uint, long or ulong expected");
					return false;
				}

				in_transit = false;
				value = new EnumConstant (c, parent_enum.TypeBuilder);
				return true;
			}

			if (prev_member == null) {
				value = new EnumConstant (New.Constantify (parent_enum.UnderlyingType), parent_enum.TypeBuilder);
				return true;
			}

			if (!prev_member.ResolveValue ())
				return false;

			in_transit = true;

			try {
				value = prev_member.value.Increment ();
			}
			catch (OverflowException) {
				Report.Error (543, Location, "The enumerator value `{0}' is too large to fit in its type `{1}'",
					GetSignatureForError (), TypeManager.CSharpName (parent_enum.UnderlyingType));
				return false;
			}
			in_transit = false;

			return true;
		}

		public bool Emit (EmitContext ec)
		{
			if (OptAttributes != null)
				OptAttributes.Emit (ec, this); 

			if (!ResolveValue ())
				return false;

			builder.SetConstant (value.GetValue ());
			Emit ();
			return true;
		}

		public override string GetSignatureForError()
		{
			return String.Concat (parent_enum.GetSignatureForError (), '.', Name);
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

		public override string DocCommentHeader {
			get { return "F:"; }
		}

		#region IConstant Members

		public Constant Value {
			get {
				return value;
			}
		}

		#endregion
	}

	/// <summary>
	///   Enumeration container
	/// </summary>
	public class Enum : DeclSpace {
		Expression BaseType;
		public Type UnderlyingType;

		static MemberList no_list = new MemberList (new object[0]);
		
		public const int AllowedModifiers =
			Modifiers.NEW |
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.PRIVATE;

		public Enum (NamespaceEntry ns, TypeContainer parent, Expression type,
			     int mod_flags, MemberName name, Attributes attrs)
			: base (ns, parent, name, attrs)
		{
			this.BaseType = type;
			ModFlags = Modifiers.Check (AllowedModifiers, mod_flags,
						    IsTopLevel ? Modifiers.INTERNAL : Modifiers.PRIVATE, name.Location);
		}

		public void AddEnumMember (EnumMember em)
		{
			if (em.Name == "value__") {
				Report.Error (76, em.Location, "An item in an enumeration cannot have an identifier `value__'");
				return;
			}

			if (!AddToContainer (em, em.Name))
				return;
		}
		
		public override TypeBuilder DefineType ()
		{
			if (TypeBuilder != null)
				return TypeBuilder;

			ec = new EmitContext (this, this, Location, null, null, ModFlags, false);
			ec.InEnumContext = true;

			if (!(BaseType is TypeLookupExpression)) {
				Report.Error (1008, Location,
					"Type byte, sbyte, short, ushort, int, uint, long or ulong expected");
				return null;
			}

			TypeExpr ute = ResolveBaseTypeExpr (BaseType, false, Location);
			UnderlyingType = ute.Type;

			if (UnderlyingType != TypeManager.int32_type &&
			    UnderlyingType != TypeManager.uint32_type &&
			    UnderlyingType != TypeManager.int64_type &&
			    UnderlyingType != TypeManager.uint64_type &&
			    UnderlyingType != TypeManager.short_type &&
			    UnderlyingType != TypeManager.ushort_type &&
			    UnderlyingType != TypeManager.byte_type  &&
			    UnderlyingType != TypeManager.sbyte_type) {
				Report.Error (1008, Location,
					"Type byte, sbyte, short, ushort, int, uint, long or ulong expected");
				return null;
			}

			if (IsTopLevel) {
				if (TypeManager.NamespaceClash (Name, Location))
					return null;
				
				ModuleBuilder builder = CodeGen.Module.Builder;

				TypeBuilder = builder.DefineType (Name, TypeAttr, TypeManager.enum_type);
			} else {
				TypeBuilder builder = Parent.TypeBuilder;

				TypeBuilder = builder.DefineNestedType (
					Basename, TypeAttr, TypeManager.enum_type);
			}

			ec.ContainerType = TypeBuilder;

			//
			// Call MapToInternalType for corlib
			//
			TypeBuilder.DefineField ("value__", UnderlyingType,
						 FieldAttributes.Public | FieldAttributes.SpecialName
						 | FieldAttributes.RTSpecialName);

			TypeManager.AddUserType (this);

			foreach (EnumMember em in defined_names.Values) {
				if (!em.Define ())
					return null;
			}

			return TypeBuilder;
		}
		
		public override bool Define ()
		{
			if (GetObsoleteAttribute () != null || Parent.GetObsoleteAttribute () != null)
				ec.TestObsoleteMethodUsage = false;

			return true;
		}

		public override void Emit ()
		{
			if (OptAttributes != null) {
				OptAttributes.Emit (ec, this);
			}

			foreach (EnumMember em in defined_names.Values) {
				if (!em.Emit (ec))
					return;
			}

			base.Emit ();
		}

		//
		// IMemberFinder
		//
		public override MemberList FindMembers (MemberTypes mt, BindingFlags bf,
			MemberFilter filter, object criteria)
		{
			if ((mt & MemberTypes.Field) == 0)
				return no_list;

			EnumMember em = defined_names [criteria] as EnumMember;
			if (em == null)
				return no_list;

			FieldBuilder[] fb = new FieldBuilder[] { em.builder };
			return new MemberList (fb);
		}

 		void VerifyClsName ()
  		{
			HybridDictionary dict = new HybridDictionary (defined_names.Count, true);
			foreach (EnumMember em in defined_names.Values) {
				if (!em.IsClsComplianceRequired (this))
					continue;

				try {
					dict.Add (em.Name, em);
				}
				catch (ArgumentException) {
					Report.SymbolRelatedToPreviousError (em);
					MemberCore col = (MemberCore)dict [em.Name];
					Report.Error (3005, col.Location, "Identifier `{0}' differing only in case is not CLS-compliant", col.GetSignatureForError ());
				}
			}
  		}

		protected override bool VerifyClsCompliance (DeclSpace ds)
		{
			if (!base.VerifyClsCompliance (ds))
				return false;

			VerifyClsName ();

			if (UnderlyingType == TypeManager.uint32_type ||
				UnderlyingType == TypeManager.uint64_type ||
				UnderlyingType == TypeManager.ushort_type) {
				Report.Error (3009, Location, "`{0}': base type `{1}' is not CLS-compliant", GetSignatureForError (), TypeManager.CSharpName (UnderlyingType));
			}

			return true;
		}
	

		public override MemberCache MemberCache {
			get {
				return null;
			}
		}

		public override AttributeTargets AttributeTargets {
			get {
				return AttributeTargets.Enum;
			}
		}

		protected override TypeAttributes TypeAttr {
			get {
				return Modifiers.TypeAttr (ModFlags, IsTopLevel) |
				TypeAttributes.Class | TypeAttributes.Sealed |
				base.TypeAttr;
			}
		}

		//
		// Generates xml doc comments (if any), and if required,
		// handle warning report.
		//
		internal override void GenerateDocComment (DeclSpace ds)
		{
			base.GenerateDocComment (ds);

			foreach (EnumMember em in defined_names.Values) {
				em.GenerateDocComment (this);
			}
		}

		//
		//   Represents header string for documentation comment.
		//
		public override string DocCommentHeader {
			get { return "T:"; }
		}
	}
}
