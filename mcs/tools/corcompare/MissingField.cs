// Mono.Util.CorCompare.MissingField
//
// Author(s):
//   Nick Drochak (ndrochak@gol.com)
//
// (C) 2001-2002 Nick Drochak

using System;
using Mono.Cecil;

namespace Mono.Util.CorCompare {

	/// <summary>
	/// 	Represents a class event that is completely missing
	/// </summary>
	/// <remarks>
	/// 	created by - Nick
	/// 	created on - 2/24/2002 10:43:57 PM
	/// </remarks>
	class MissingField : MissingMember {
		// e.g. <method name="Equals" status="missing"/>
		public MissingField (FieldDefinition infoMono, FieldDefinition infoMS) : base (infoMono, infoMS) { }

		public override string Type {
			get {
				return "field";
			}
		}

		public override CustomAttributeCollection GetCustomAttributes (MemberReference mref) {
			return ((FieldDefinition) mref).CustomAttributes;
		}

		public override Accessibility GetAccessibility (MemberReference mref) {
			FieldDefinition member = (FieldDefinition) mref;
			FieldAttributes maskedMemberAccess = member.Attributes & FieldAttributes.FieldAccessMask;
			if (maskedMemberAccess == FieldAttributes.Public)
				return Accessibility.Public;
			else if (maskedMemberAccess == FieldAttributes.Assembly)
				return Accessibility.Assembly;
			else if (maskedMemberAccess == FieldAttributes.FamORAssem)
				return Accessibility.FamilyOrAssembly;
			else if (maskedMemberAccess == FieldAttributes.Family)
				return Accessibility.Family;
			else if (maskedMemberAccess == FieldAttributes.FamANDAssem)
				return Accessibility.FamilyAndAssembly;
			else if (maskedMemberAccess == FieldAttributes.Private)
				return Accessibility.Private;
			throw new Exception ("Missing handler for Member " + mref.Name);
		}

		public override NodeStatus Analyze ()
		{
			base.Analyze ();

			if (mInfoMono != null && mInfoMS != null)
			{
				FieldDefinition fiMono = (FieldDefinition) mInfoMono;
				FieldDefinition fiMS = (FieldDefinition) mInfoMS;
				bool fiMonoIsNotSerialized = (fiMono.Attributes & FieldAttributes.NotSerialized) != 0;
				bool fiMSIsNotSerialized = (fiMS.Attributes & FieldAttributes.NotSerialized) != 0;
				bool fiMonoIsPinvokeImpl = (fiMono.Attributes & FieldAttributes.PInvokeImpl) != 0;
				bool fiMSIsPinvokeImpl = (fiMS.Attributes & FieldAttributes.PInvokeImpl) != 0;
				bool fiMonoIsInitOnly = (fiMono.Attributes & FieldAttributes.InitOnly) != 0;
				bool fiMSIsInitOnly = (fiMS.Attributes & FieldAttributes.InitOnly) != 0;


				AddFakeAttribute (fiMonoIsNotSerialized, fiMSIsNotSerialized, "System.NonSerializedAttribute");
				AddFakeAttribute (fiMonoIsPinvokeImpl, fiMSIsPinvokeImpl, "System.PInvokeImplAttribute");

				AddFlagWarning (fiMono.IsStatic, fiMS.IsStatic, "static");
				AddFlagWarning (fiMono.IsLiteral, fiMS.IsLiteral, "const");
				AddFlagWarning (fiMonoIsInitOnly, fiMSIsInitOnly, "readonly");

				string strTypeMono = fiMono.FieldType.FullName;
				string strTypeMS   =   fiMS.FieldType.FullName;
				if (strTypeMono != strTypeMS)
				{
					Status.AddWarning ("Invalid type: is '"+strTypeMono+"', should be '"+strTypeMS+"'");
				}

				try
				{
					if (fiMono.IsStatic && fiMS.IsStatic &&
						fiMono.IsLiteral && fiMS.IsLiteral)
					{
						byte[] objMono = fiMono.InitialValue;
						byte[] objMS = fiMS.InitialValue;
						long lMono = Convert.ToInt64 (objMono);
						long lMS = Convert.ToInt64 (objMS);

						if (lMono != lMS)
						{
							string strValMono = ((lMono < 0) ? "-0x" : "0x") + lMono.ToString ("x");
							string strValMS   = ((lMS   < 0) ? "-0x" : "0x") +   lMS.ToString ("x");
							Status.AddWarning ("Invalid value: is '"+strValMono+"', should be '"+strValMS+"'");
						}
					}
				}
				catch (Exception) {}
			}
			return m_nodeStatus;
		}
	}
}
