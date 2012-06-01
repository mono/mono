// System.EnterpriseServices.Internal.IClrObjectFactory.cs
//
// Author:
//   Alejandro Sánchez Acosta (raciel@es.gnu.org)
//
// Copyright (C) Alejandro Sánchez Acosta
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

namespace System.EnterpriseServices.Internal
{
	[Guid("ecabafd2-7f19-11d2-978e-0000f8757e2a")]
	public interface IClrObjectFactory
	{
		[DispId(1)]
		[return:MarshalAs (UnmanagedType.IDispatch)]
		object CreateFromAssembly (string assembly, string type, string mode);
		[DispId(4)]
		[return:MarshalAs (UnmanagedType.IDispatch)]
		object CreateFromMailbox (string Mailbox, string Mode);
		[DispId(2)]
		[return:MarshalAs (UnmanagedType.IDispatch)]
		object CreateFromVroot (string VrootUrl, string Mode);
		[DispId(3)]
		[return:MarshalAs (UnmanagedType.IDispatch)]
		object CreateFromWsdl (string WsdlUrl, string Mode);
	}
}
