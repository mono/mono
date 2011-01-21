//
// SessionInstanceContextProvider.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2009 Novell, Inc.  http://www.novell.com
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
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml;

namespace System.ServiceModel.Dispatcher
{
	internal class SessionInstanceContextProvider : IInstanceContextProvider
	{
		ServiceHostBase host;
		Dictionary<string,InstanceContext> pool = new Dictionary<string,InstanceContext> ();

		public SessionInstanceContextProvider (ServiceHostBase host)
		{
			this.host = host;
		}

		public InstanceContext GetExistingInstanceContext (Message message, IContextChannel channel)
		{
			InstanceContext ctx;
			var key = channel.SessionId ?? String.Empty;
			return pool.TryGetValue (key, out ctx) ? ctx : null;
		}

		public void InitializeInstanceContext (InstanceContext instanceContext, Message message, IContextChannel channel)
		{
			var key = channel.SessionId ?? String.Empty;
			pool [key] = instanceContext;
		}

		public bool IsIdle (InstanceContext instanceContext)
		{
			// FIXME: implement
			return false;
		}

		public void NotifyIdle (InstanceContextIdleCallback callback, InstanceContext instanceContext)
		{
			// FIXME: implement
		}
	}
}
