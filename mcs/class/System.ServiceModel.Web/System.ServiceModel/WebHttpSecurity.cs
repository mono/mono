//
// WebHttpSecurity.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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
using System.ComponentModel;

namespace System.ServiceModel
{
	public sealed class WebHttpSecurity
	{
#if NET_4_0
		public WebHttpSecurity ()
#else
		internal WebHttpSecurity ()
#endif
		{
			// there is no public constructor for transport ...
#if !NET_2_1
			Transport = new BasicHttpBinding ().Security.Transport;
#endif
		}

		WebHttpSecurityMode mode;

		public WebHttpSecurityMode Mode {
			get { return mode; }
			set { mode = value; }
		}

#if NET_4_0
		public HttpTransportSecurity Transport { get; set; }
#elif !NET_2_1
		public HttpTransportSecurity Transport { get; private set; }
#endif


#if NET_4_0
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public bool ShouldSerializeMode ()
		{
			return false;
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public bool ShouldSerializeTransport ()
		{
			return false;
		}
#endif
	}
}
