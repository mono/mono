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
			// TODO: remove after MemberCache fix
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

			// TODO: Remove completely
			IMemberDetails details;
			var gfd = TypeManager.GetGenericFieldDefinition (fi);
			fb = TypeManager.GetFieldCore (gfd);
			if (fb != null) {
				details = fb;
			} else {
				cs = TypeManager.GetConstant (gfd);
				if (cs != null)
					details = cs.MemberDetails;
				else
					details = new ImportedMemberDetails (fi);
			}

			if ((fa & FieldAttributes.Literal) != 0) {
				Expression c;
				if (gfd is System.Reflection.Emit.FieldBuilder) {
					// TODO: Remove after MemberCache
					c = TypeManager.GetConstant (gfd).Value;
				} else {
					c = Constant.CreateConstantFromValue (fi.FieldType, gfd.GetValue (gfd), Location.Null);
				}

				return new ConstSpec (details, fi, mod, c);
			}

			if ((fa & FieldAttributes.InitOnly) != 0) {
				if (fi.FieldType == TypeManager.decimal_type) {
					var dc = ReadDecimalConstant (gfd);
					if (dc != null)
						return new ConstSpec (details, fi, mod, dc);
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
						return new FixedFieldSpec (details, fi, element_field, mod);
					}
				}
			}

			// TODO: volatile

			return new FieldSpec (details, fi, mod);
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

	class ImportedMemberDetails : IMemberDetails
	{
		readonly ICustomAttributeProvider provider;

		public ImportedMemberDetails (ICustomAttributeProvider provider)
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
	}
}