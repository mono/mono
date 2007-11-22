//
// WebPageTraceListener.cs
//
// Author:
//	Daniel Nauck  <dna(at)mono-project(dot)de>
//
// Copyright (C) 2007 Daniel Nauck
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Permissions;
using System.Text;
using System.Web;

namespace System.Web
{
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal),
	AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal),
	HostProtection (SecurityAction.LinkDemand, Synchronization = true)]
	public class WebPageTraceListener : TraceListener
	{
		public override void TraceEvent (TraceEventCache eventCache, string source, TraceEventType severity, int id, string message)
		{
			throw new NotImplementedException ();
		}

		public override void TraceEvent (TraceEventCache eventCache, string source, TraceEventType severity, int id, string format, params object[] args)
		{
			throw new NotImplementedException ();
		}

		public override void Write (string message)
		{
			throw new NotImplementedException ();
		}

		public override void Write (string message, string category)
		{
			throw new NotImplementedException ();
		}

		public override void WriteLine (string message)
		{
			throw new NotImplementedException ();
		}

		public override void WriteLine (string message, string category)
		{
			throw new NotImplementedException ();
		}
	}
}
#endif