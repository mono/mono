//
// System.Runtime.InteropServices.ClassInterfaceAttribute.cs
//
// Author:
//   Nick Drochak (ndrochak@gol.com)
//
// (C) 2002 Nick Drochak
//

using System;

namespace System.Runtime.InteropServices {

	[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class)]
	public sealed class ClassInterfaceAttribute : Attribute {
		private ClassInterfaceType ciType;
		
		public ClassInterfaceAttribute ( short classInterfaceType ) {
			ciType = (ClassInterfaceType)classInterfaceType;
		}

		public ClassInterfaceAttribute ( ClassInterfaceType classInterfaceType ) {
			ciType = classInterfaceType;
		}

		public ClassInterfaceType Value {
			get {return ciType;}
		}
	}
}
