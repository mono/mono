//
// System.Runtime.InteropServices.InterfaceTypeAttribute.cs
//
// Author:
//   Kevin Winchester (kwin@ns.sympatico.ca)
//
// (C) 2002 Kevin Winchester
//

namespace System.Runtime.InteropServices {

	[AttributeUsage(AttributeTargets.Interface)]
	public sealed class InterfaceTypeAttribute : Attribute {
		
		private ComInterfaceType intType;
		
		public InterfaceTypeAttribute (ComInterfaceType interfaceType){
			intType = interfaceType;
		}

		public InterfaceTypeAttribute (short interfaceType) {
			intType = (ComInterfaceType)interfaceType;
		}
		
		public ComInterfaceType Value {
			get {return intType;}
		}
	}
}
