// Mono.Util.CorCompare.MissingField
//
// Author(s):
//   Nick Drochak (ndrochak@gol.com)
//
// (C) 2001-2002 Nick Drochak

using System;
using System.Reflection;

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
		public MissingField (MemberInfo infoMono, MemberInfo infoMS) : base (infoMono, infoMS) {}

		public override string Type {
			get {
				return "field";
			}
		}

		public override NodeStatus Analyze ()
		{
			base.Analyze ();

			if (mInfoMono != null && mInfoMS != null)
			{
				FieldInfo fiMono = (FieldInfo) mInfoMono;
				FieldInfo fiMS   = (FieldInfo) mInfoMS;

				AddFakeAttribute (fiMono.IsNotSerialized, fiMS.IsNotSerialized, "System.NonSerializedAttribute");
				AddFakeAttribute (fiMono.IsPinvokeImpl, fiMS.IsPinvokeImpl, "System.PInvokeImplAttribute");

				AddFlagWarning (fiMono.IsStatic, fiMS.IsStatic, "static");
				AddFlagWarning (fiMono.IsLiteral, fiMS.IsLiteral, "const");
				AddFlagWarning (fiMono.IsInitOnly, fiMS.IsInitOnly, "readonly");

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
						object objMono = fiMono.GetValue (null);
						object objMS = fiMS.GetValue (null);
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
