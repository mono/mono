//
// System.Runtime.InteropServices/DllImportAttribute.cs
//
// Author:
//   Paolo Molaro (lupus@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System;


namespace System.Runtime.InteropServices {
	public sealed class DllImportAttribute: Attribute {
		public CallingConvention CallingConvention;
		public CharSet CharSet;
		public string EntryPoint;
		public bool ExactSpelling;
		public bool PreserveSig;
		public bool SetLastError;
		private string Dll;
		
		public string Value {
			get {return Dll;}
		}
		
		public DllImportAttribute (string dllName) {
			Dll = dllName;
		}
	}
}
