//
// System.Runtime.InteropServices.MarshalAsAttribute.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//

using System;

namespace System.Runtime.InteropServices {

	[AttributeUsage (AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
	public sealed class MarshalAsAttribute : Attribute {
		private UnmanagedType utype;
		public UnmanagedType ArraySubType;
		public string MarshalCookie;
		public string MarshalType;
		public Type MarshalTypeRef;
		public VarEnum SafeArraySubType;
		public int SizeConst;
		public short SizeParamIndex;
		public Type SafeArrayUserDefinedSubType;

		public MarshalAsAttribute (short unmanagedType) {
			utype = (UnmanagedType)unmanagedType;
		}
		public MarshalAsAttribute( UnmanagedType unmanagedType) {
			utype = unmanagedType;
		}
		public UnmanagedType Value {
			get {return utype;}
		}
		
	}
}
