using System;

namespace System.Runtime.InteropServices {

	[AttributeUsage (AttributeTargets.Class | AttributeTargets.Struct)]
	public sealed class StructLayoutAttribute : Attribute {
		public CharSet CharSet = CharSet.Auto;
		public int Pack = 8;
		public int Size = 0;
		private LayoutKind lkind;
		
		public StructLayoutAttribute( short layoutKind) {
			lkind = (LayoutKind)layoutKind;
		}
		public StructLayoutAttribute( LayoutKind layoutKind) {
			lkind = layoutKind;
		}
		public LayoutKind Value {
			get {return lkind;}
		}
		
	}
}
