//
// System.Web.Hosting.IAppDomainFactory.cs
//
// Author:
//   Bob Smith <bob@thestuff.net>
//
// (C) Bob Smith
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

using System;
using System.Runtime.InteropServices;

namespace System.Web.Hosting
{
	[Guid ("e6e21054-a7dc-4378-877d-b7f4a2d7e8ba")]
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	[ComImportAttribute]
	public interface IAppDomainFactory
	{
		[return: MarshalAs (UnmanagedType.Interface)]
		object Create ([In, MarshalAs(UnmanagedType.BStr)] string module,
			       [In, MarshalAs(UnmanagedType.BStr)] string typeName,
			       [In, MarshalAs(UnmanagedType.BStr)] string appId,
			       [In, MarshalAs(UnmanagedType.BStr)] string appPath,
			       [In, MarshalAs(UnmanagedType.BStr)] string strUrlOfAppOrigin,
			       [In, MarshalAs(UnmanagedType.I4)] int iZone);
	}
}

