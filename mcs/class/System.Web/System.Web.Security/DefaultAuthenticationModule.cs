//
// System.Web.Security.DefaultAuthenticationModule
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.Security.Permissions;
using System.Security.Principal;
using System.Threading;

// more info on the workings of this class can be found in Shackow, p. 55
//
namespace System.Web.Security
{
	// CAS - no InheritanceDemand here as the class is sealed
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public sealed class DefaultAuthenticationModule : IHttpModule
	{
		static readonly object authenticateEvent = new object ();
		static IPrincipal generic_principal = new GenericPrincipal (new GenericIdentity ("", ""), new string [0]);

		EventHandlerList events = new EventHandlerList ();
		
		public event DefaultAuthenticationEventHandler Authenticate {
			add { events.AddHandler (authenticateEvent, value); }
			remove { events.RemoveHandler (authenticateEvent, value); }
		}
		
		[SecurityPermission (SecurityAction.Demand, UnmanagedCode = true)]
		public DefaultAuthenticationModule ()
		{
		}

		public void Dispose ()
		{
		}

		public void Init (HttpApplication app)
		{
			app.DefaultAuthentication += new EventHandler (OnDefaultAuthentication);
		}

		void OnDefaultAuthentication (object sender, EventArgs args)
		{
			HttpApplication app = (HttpApplication) sender;
			HttpContext context = app.Context;

			DefaultAuthenticationEventHandler eh = events [authenticateEvent] as DefaultAuthenticationEventHandler;
			if (context.User == null && eh != null)
				eh (this, new DefaultAuthenticationEventArgs (context));

			if (context.User == null)
				context.User = generic_principal;

			Thread.CurrentPrincipal = context.User;
		}
	}
}

