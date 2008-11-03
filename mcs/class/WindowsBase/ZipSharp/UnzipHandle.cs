// UnzipArchive.cs created with MonoDevelop
// User: alan at 13:13 20/10/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.Runtime.InteropServices;

namespace zipsharp
{
	class UnzipHandle : SafeHandle
	{
		public override bool IsInvalid {
			get {
				return handle == IntPtr.Zero;
			}
		}

		public UnzipHandle ()
			: base (IntPtr.Zero, true)
		{
			
		}

		protected override bool ReleaseHandle ()
		{
			NativeUnzip.CloseArchive (this);
			return true;
		}
	}
}
