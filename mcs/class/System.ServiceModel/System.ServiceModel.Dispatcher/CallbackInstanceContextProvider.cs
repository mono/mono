//
// CallbackInstanceContextProvider.cs
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
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace System.ServiceModel.Dispatcher
{
	internal class CallbackInstanceContextProvider : IInstanceContextProvider
	{
		InstanceContext ctx;

		public CallbackInstanceContextProvider (InstanceContext context)
		{
			this.ctx = context;
		}

		public InstanceContext GetExistingInstanceContext (Message message, IContextChannel channel)
		{
			return ctx;
		}

		public void InitializeInstanceContext (InstanceContext instanceContext, Message message, IContextChannel channel)
		{
			// FIXME: what to do here?
		}

		public bool IsIdle (InstanceContext instanceContext)
		{
			// FIXME: implement
			throw new NotImplementedException ();
		}

		public void NotifyIdle (InstanceContextIdleCallback callback, InstanceContext instanceContext)
		{
			// FIXME: implement
			throw new NotImplementedException ();
		}
	}
}
