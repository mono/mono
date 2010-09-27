//
// HttpChannelListenerEntry.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2010 Novell, Inc.  http://www.novell.com
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
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Security;
using System.Text;
using System.Threading;

namespace System.ServiceModel.Channels.Http
{
	class HttpChannelListenerEntry
	{
		public HttpChannelListenerEntry (ChannelDispatcher channel, EventWaitHandle waitHandle)
		{
			ChannelDispatcher = channel;
			WaitHandle = waitHandle;
			ContextQueue = new Queue<HttpContextInfo> ();
			RetrieverLock = new object ();
		}
		
		public object RetrieverLock { get; private set; }
		public ChannelDispatcher ChannelDispatcher { get; private set; }
		public EventWaitHandle WaitHandle { get; private set; }
		public Queue<HttpContextInfo> ContextQueue { get; private set; }

		internal static int CompareEntries (HttpChannelListenerEntry e1, HttpChannelListenerEntry e2)
		{
			if (e1.ChannelDispatcher.Endpoints.Count == 0)
				return 1;
			if (e2.ChannelDispatcher.Endpoints.Count == 0)
				return -1;
			// select the highest filter priority value in the Endpoints.
			int p1 = e1.ChannelDispatcher.Endpoints.OrderByDescending (e => e.FilterPriority).First ().FilterPriority;
			int p2 = e2.ChannelDispatcher.Endpoints.OrderByDescending (e => e.FilterPriority).First ().FilterPriority;
			// then return the channel dispatcher with higher priority first.
			return p2 - p1;
		}

		const UriComponents cmpflag = UriComponents.Path;
		const UriFormat fmtflag = UriFormat.SafeUnescaped;

		internal bool FilterHttpContext (HttpContextInfo ctx)
		{
			if (ChannelDispatcher == null)
				return true; // no mex can be involved.
			if (ctx.Request.HttpMethod.ToUpper () != "GET")
				return !ChannelDispatcher.IsMex; // non-GET request never matches mex channel dispatcher.
			var sme = ChannelDispatcher.Host.Extensions.Find<ServiceMetadataExtension> ();
			if (sme == null)
				return true; // no mex can be involved.

			var listener = ChannelDispatcher.Listener;
			var mex = sme.Instance;

			// now the request is GET, and we have to return true or false based on the matrix below:
			// matches wsdl or help| yes      |  no      |
			// mex                 | yes | no | yes | no |
			// --------------------+-----+----+-----+----+
			//                     |  T  | F  |  F  |  T |

			bool match =
				(mex.WsdlUrl != null && Uri.Compare (ctx.Request.Url, mex.WsdlUrl, cmpflag, fmtflag, StringComparison.Ordinal) == 0) ||
				(mex.HelpUrl != null && Uri.Compare (ctx.Request.Url, mex.HelpUrl, cmpflag, fmtflag, StringComparison.Ordinal) == 0);

			return !(match ^ ChannelDispatcher.IsMex);
		}
	}
}
