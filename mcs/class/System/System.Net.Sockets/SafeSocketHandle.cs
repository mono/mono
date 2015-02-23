//
// System.Net.Sockets.SafeSocketHandle
//
// Authors:
//	Marcos Henrich  <marcos.henrich@xamarin.com>
//

using System;
using System.IO;
using Microsoft.Win32.SafeHandles;

namespace System.Net.Sockets {

	sealed class SafeSocketHandle : SafeHandleZeroOrMinusOneIsInvalid {
		public SafeSocketHandle (IntPtr preexistingHandle, bool ownsHandle) : base (ownsHandle)
		{
			SetHandle (preexistingHandle);
		}

		// This is just for marshalling
		internal SafeSocketHandle () : base (true)
		{
		}

		protected override bool ReleaseHandle ()
		{
			int error;
			
			Socket.Close_internal (handle, out error);

			return error == 0;
		}

	}
}

