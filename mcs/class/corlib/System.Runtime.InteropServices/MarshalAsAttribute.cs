using System;

namespace System.Runtime.InteropServices {
	public sealed class MarshalAsAttribute : Attribute {
		private UnmanagedType utype;
		public UnmanagedType ArraySubType;
		public string MarshalCookie;
		public string MarshalType;
		public Type MarshalTypeRef;
		public VarEnum SafeArraySubType;
		public int SizeConst;
		public short SizeParamIndex;

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
