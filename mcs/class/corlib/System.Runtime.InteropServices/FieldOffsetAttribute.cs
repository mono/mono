using System;

namespace System.Runtime.InteropServices {

	[AttributeUsage (AttributeTargets.Field)]
	public sealed class FieldOffsetAttribute : Attribute {
		private int val;
		
		public FieldOffsetAttribute( int offset) {
			val = offset;
		}
		public int Value {
			get {return val;}
		}
		
	}
}
