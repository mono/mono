// ZipFile.cs created with MonoDevelop
// User: alan at 11:54 13/10/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.Runtime.InteropServices;

namespace zipsharp
{
	class ZipHandle : SafeHandle
	{
		public override bool IsInvalid {
			get {
				return handle == IntPtr.Zero;
			}
		}

		public ZipHandle ()
			: base (IntPtr.Zero, true)
		{
			
		}

		protected override bool ReleaseHandle()
		{
			NativeZip.CloseArchive (this);
			return true;
		}
	}
}
