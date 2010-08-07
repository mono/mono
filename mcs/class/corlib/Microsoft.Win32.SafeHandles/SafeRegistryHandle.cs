//
// Microsoft.Win32.SafeHandles.SafeRegistryHandle
//
// Author
//       Gonzalo Paniagua Javier (gonzalo@novell.com)
//
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

#if NET_4_0
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.ConstrainedExecution;

namespace Microsoft.Win32.SafeHandles {

	public sealed class SafeRegistryHandle : SafeHandleZeroOrMinusOneIsInvalid, IDisposable {
		public SafeRegistryHandle (IntPtr preexistingHandle, bool ownsHandle) : base (ownsHandle)
		{
			handle = preexistingHandle;	
		}

		protected override bool ReleaseHandle ()
		{
			// Need to release an unmanaged pointer *only* in Windows.
			if (Path.DirectorySeparatorChar == '\\')
				return RegCloseKey (handle) == 0;

			return true;
		}

		[DllImport ("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint="RegCloseKey")]
		static extern int RegCloseKey (IntPtr keyHandle);
	}
}
#endif

