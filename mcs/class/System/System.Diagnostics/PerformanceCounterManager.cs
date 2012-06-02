//
// System.Diagnostics.PerformanceCounterManager.cs
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2002
//

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

using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.Diagnostics {

	[ComVisible(true)]
	[Guid("82840be1-d273-11d2-b94a-00600893b17a")]
	[PermissionSet (SecurityAction.LinkDemand, Unrestricted = true)]
	[Obsolete ("use PerformanceCounter")]
	[MonoTODO ("not implemented")]
	public sealed class PerformanceCounterManager : ICollectData {

		[Obsolete ("use PerformanceCounter")]
		public PerformanceCounterManager ()
		{
		}

		void ICollectData.CloseData ()
		{
			throw new NotImplementedException ();
		}

		void ICollectData.CollectData (
			int callIdx,
			IntPtr valueNamePtr,
			IntPtr dataPtr,
			int totalBytes,
			out IntPtr res)
		{
			throw new NotImplementedException ();
		}
	}
}

