//
// System.Runtime.InteropServices.ProgIdAttribute.cs
//
// Name: Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.
//

using System;

namespace System.Runtime.InteropServices {

	[AttributeUsage (AttributeTargets.Class)]
	public sealed class ProgIdAttribute : Attribute
	{
		string pid;
		
		public ProgIdAttribute (string progId)
		{
			pid = progId;
		}

		public string Value {
			get { return pid; }
		}
	}

}
