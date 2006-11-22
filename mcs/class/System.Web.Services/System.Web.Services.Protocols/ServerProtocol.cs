// 
// ServerProtocol.cs
//
// Author:
//   Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.
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

#if NET_2_0

namespace System.Web.Services.Protocols
{
	public abstract class ServerProtocol
	{
		protected ServerProtocol ()
		{
		}

		[MonoTODO]
		protected HttpContext Context {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		protected HttpRequest Request {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		protected HttpResponse Response {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		protected virtual object Target {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		protected void AddToCache (Type protocolType, Type serverType, object value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected object GetFromCache (Type protocolType, Type serverType)
		{
			throw new NotImplementedException ();
		}
	}
}

#endif
