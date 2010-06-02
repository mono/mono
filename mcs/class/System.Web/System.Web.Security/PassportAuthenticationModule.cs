//
// System.Web.Security.PassportAuthenticationModule
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
// Copyright (C) 2005-2010 Novell, Inc (http://www.novell.com)
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

namespace System.Web.Security
{
	// CAS - no InheritanceDemand here as the class is sealed
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#if NET_4_0
	[Obsolete ("This type is obsolete. The Passport authentication product is no longer supported and has been superseded by Live ID.")]
#endif
	public sealed class PassportAuthenticationModule : IHttpModule
	{
		static readonly object authenticateEvent = new object ();

		EventHandlerList events = new EventHandlerList ();
		[SecurityPermission (SecurityAction.Demand, UnmanagedCode = true)]
		public PassportAuthenticationModule ()
		{
		}

		public event PassportAuthenticationEventHandler Authenticate {
			add { events.AddHandler (authenticateEvent, value); }
			remove { events.RemoveHandler (authenticateEvent, value); }
		}

		public void Dispose ()
		{
			events.Dispose ();
		}

		[MonoTODO("Will we ever implement this? :-)")]
		public void Init (HttpApplication app)
		{
			throw new NotImplementedException ();
		}
	}
}

