//
// System.Runtime.InteropServices.ImportedFromTypeLibAttribute.cs
//
// Name: Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.
//

using System;

namespace System.Runtime.InteropServices {
	
	[AttributeUsage (AttributeTargets.Assembly)]
	public sealed class ImportedFromTypeLibAttribute : Attribute
	{
		string TlbFile;
		public ImportedFromTypeLibAttribute (string tlbFile)
		{
			TlbFile = tlbFile;
		}

		public string Value {
			get { return TlbFile; }
		}
	} 
}
