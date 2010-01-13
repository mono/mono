//
// import.cs: System.Reflection conversions
//
// Authors: Marek Safar (marek.safar@gmail.com)
//
// Dual licensed under the terms of the MIT X11 or GNU GPL
//
// Copyright 2009 Novell, Inc
//

using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Linq;

namespace Mono.CSharp
{
	static class Import
	{
		public static FieldSpec CreateField (FieldInfo fi)
		{
			// TODO MemberCache: remove
			var cs = TypeManager.GetConstant (fi);
			if (cs != null)
				return cs;
			var fb = TypeManager.GetFieldCore (fi);
			if (fb != null)
				return fb.Spec;
			// End

			Modifiers mod = 0;
			var fa = fi.Attributes;
			switch (fa & FieldAttributes.FieldAccessMask) {
				case FieldAttributes.Public:
					mod = Modifiers.PUBLIC;
					break;
				case FieldAttributes.Assembly:
					mod = Modifiers.INTERNAL;
					break;
				case FieldAttributes.Family:
					mod = Modifiers.PROTECTED;
					break;
				case FieldAttributes.FamORAssem:
					mod = Modifiers.PROTECTED | Modifiers.INTERNAL;
					break;
				default:
					mod = Modifiers.PRIVATE;
					break;
			}

			// TODO MemberCache: Remove completely and use only Imported
			IMemberDefinition definition;
			var gfd = TypeManager.GetGenericFieldDefinition (fi);
			fb = TypeManager.GetFieldCore (gfd);
			if (fb != null) {
				definition = fb;
			} else {
				cs = TypeManager.GetConstant (gfd);
				if (cs != null)
					definition = cs.MemberDefinition;
				else
					definition = new ImportedMemberDefinition (fi);
			}

			if ((fa & FieldAttributes.Literal) != 0) {
				Expression c;
				if (gfd is System.Reflection.Emit.FieldBuilder) {
					// TODO: Remove after MemberCache
					c = TypeManager.GetConstant (gfd).Value;
				} else {
					c = Constant.CreateConstantFromValue (fi.FieldType, gfd.GetValue (gfd), Location.Null);
				}

				return new ConstSpec (definition, fi, mod, c);
			}

			if ((fa & FieldAttributes.InitOnly) != 0) {
				if (fi.FieldType == TypeManager.decimal_type) {
					var dc = ReadDecimalConstant (gfd);
					if (dc != null)
						return new ConstSpec (definition, fi, mod, dc);
				}

				mod |= Modifiers.READONLY;
			}

			if ((fa & FieldAttributes.Static) != 0)
				mod |= Modifiers.STATIC;

			if (!TypeManager.IsReferenceType (fi.FieldType)) {
				PredefinedAttribute pa = PredefinedAttributes.Get.FixedBuffer;
				if (pa.IsDefined) {
					if (gfd is System.Reflection.Emit.FieldBuilder) {
						 // TODO: Remove this after MemberCache fix
					} else if (gfd.IsDefined (pa.Type, false)) {
						var element_field = fi.FieldType.GetField (FixedField.FixedElementName);
						return new FixedFieldSpec (definition, fi, element_field, mod);
					}
				}
			}

			// TODO: volatile

			return new FieldSpec (definition, fi, mod);
		}

		public static MethodSpec CreateMethod (MethodBase mb)
		{
			// TODO MemberCache: Remove
			MethodCore mc = TypeManager.GetMethod (mb) as MethodCore;
			if (mc != null)
				return mc.Spec;

			Modifiers mod = 0;
			var ma = mb.Attributes;
			switch (ma & MethodAttributes.MemberAccessMask) {
				case MethodAttributes.Public:
					mod = Modifiers.PUBLIC;
					break;
				case MethodAttributes.Assembly:
					mod = Modifiers.INTERNAL;
					break;
				case MethodAttributes.Family:
					mod = Modifiers.PROTECTED;
					break;
				case MethodAttributes.FamORAssem:
					mod = Modifiers.PROTECTED | Modifiers.INTERNAL;
					break;
				default:
					mod = Modifiers.PRIVATE;
					break;
			}

			if ((ma & MethodAttributes.Static) != 0)
				mod |= Modifiers.STATIC;
			if ((ma & MethodAttributes.Virtual) != 0)
				mod |= Modifiers.VIRTUAL;
			if ((ma & MethodAttributes.Abstract) != 0)
				mod |= Modifiers.ABSTRACT;
			if ((ma & MethodAttributes.Final) != 0)
				mod |= Modifiers.SEALED;

			IMemberDefinition definition;
			var gmd = mb as MethodInfo;
			if (gmd != null && gmd.IsGenericMethodDefinition) {
				definition = new ImportedGenericMethodDefinition (gmd);
			} else if (mb.IsGenericMethod) {	// TODO MemberCache: Remove me
				definition = new ImportedGenericMethodDefinition ((MethodInfo) TypeManager.DropGenericMethodArguments (mb));
			} else {
				definition = new ImportedMemberDefinition (mb);
			}

			// TODO MemberCache: Use AParametersCollection p = ParametersImported.Create (mb);
			AParametersCollection p = TypeManager.GetParameterData (mb);
			MethodSpec ms = new MethodSpec (definition, mb, p, mod);
			return ms;
		}

		public static PropertySpec CreateProperty (PropertyInfo pi)
		{
			var definition = new ImportedMemberDefinition (pi);
			var mod = Modifiers.PRIVATE;	// TODO: modifiers
			return new PropertySpec (definition, pi, mod);
		}

		//
		// Decimal constants cannot be encoded in the constant blob, and thus are marked
		// as IsInitOnly ('readonly' in C# parlance).  We get its value from the 
		// DecimalConstantAttribute metadata.
		//
		static Constant ReadDecimalConstant (FieldInfo fi)
		{
			PredefinedAttribute pa = PredefinedAttributes.Get.DecimalConstant;
			if (!pa.IsDefined)
				return null;

			object[] attrs = fi.GetCustomAttributes (pa.Type, false);
			if (attrs.Length != 1)
				return null;

			return new DecimalConstant (((DecimalConstantAttribute) attrs [0]).Value, Location.Null);
		}
	}

	class ImportedMemberDefinition : IMemberDefinition
	{
		protected readonly ICustomAttributeProvider provider;

		public ImportedMemberDefinition (ICustomAttributeProvider provider)
		{
			this.provider = provider;
		}

		public ObsoleteAttribute GetObsoleteAttribute ()
		{
			var res = provider.GetCustomAttributes (typeof (ObsoleteAttribute), false);
			if (res == null || res.Length < 1)
				return null;

			return res [0] as ObsoleteAttribute;
		}

		public void SetIsUsed ()
		{
			// Unused for imported members
		}
	}

	class ImportedGenericMethodDefinition : ImportedMemberDefinition, IGenericMethodDefinition
	{
		public ImportedGenericMethodDefinition (MethodInfo provider)
			: base (provider)
		{
		}

		public MethodInfo MakeGenericMethod (Type[] targs)
		{
			return ((MethodInfo) provider).MakeGenericMethod (targs);
		}
	}
}