//
// System.Web.IisTraceListener.cs 
//
// Author:
//	Marek Habersack (mhabersack@novell.com)
//
// Copyright (C) 2009 Novell, Inc (http://www.novell.com)
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
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.Web
{
	//
	// Since we don't have a use for this class right now and we don't run it under IIS 7, we
	// will just call the base class to do the work in some cases and for the remaining methods
	// we will mimic the behavior of WebPageTraceListener.
	//
	[AspNetHostingPermissionAttribute(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[HostProtectionAttribute(SecurityAction.LinkDemand, Synchronization = true)]
	public sealed class IisTraceListener : TraceListener
	{
		public IisTraceListener ()
		{
		}

		public override void TraceData (TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
		{
			base.TraceData (eventCache, source, eventType, id, data);
		}

		public override void TraceData (TraceEventCache eventCache, string source, TraceEventType eventType, int id, params object[] data)
		{
			base.TraceData (eventCache, source, eventType, id, data);
		}

		public override void TraceEvent (TraceEventCache eventCache, string source, TraceEventType severity, int id, string message)
		{
			base.TraceEvent (eventCache, source, severity, id, message);
		}

		public override void TraceEvent (TraceEventCache eventCache, string source, TraceEventType severity, int id, string format, params object[] args)
		{
			base.TraceEvent (eventCache, source, severity, id, format, args);
		}
		
		public override void Write (string message)
		{
			HttpContext.Current.Trace.Write (message);
		}

		public override void Write (string message, string category)
		{
			Write (message);
		}

		public override void WriteLine (string message)
		{
			HttpContext.Current.Trace.Write (message + Environment.NewLine);
		}

		public override void WriteLine (string message, string category)
		{
			WriteLine (message);
		}
	}
}
#endif