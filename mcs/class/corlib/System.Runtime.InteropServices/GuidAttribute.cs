//
// System.Runtime.InteropServices.InAttribute.cs
//
// Author:
//   Kevin Winchester (kwin@ns.sympatico.ca)
//
// (C) 2002 Kevin Winchester
//

namespace System.Runtime.InteropServices {

	[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Interface | AttributeTargets.Delegate)]
	public sealed class GuidAttribute : Attribute {
		
		private string guidValue;
		
		public GuidAttribute (string guid) {
			guidValue = guid;	
		}
		
		public string Value {
			get {return guidValue;}
		}
	}
}
