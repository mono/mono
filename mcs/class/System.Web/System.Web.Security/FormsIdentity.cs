//
// System.Web.Security.FormsIdentity
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//

using System;
using System.Security.Principal;

namespace System.Web.Security
{
	[Serializable]
	public sealed class FormsIdentity : IIdentity
	{
		FormsAuthenticationTicket ticket;

		public FormsIdentity (FormsAuthenticationTicket ticket)
		{
			this.ticket = ticket;
		}

		public string AuthenticationType
		{
			get {
				return "Forms";
			}
		}

		public bool IsAuthenticated
		{
			get {
				return true;
			}
		}

		public string Name
		{
			get {
				return ticket.Name;
			}
		}

		public FormsAuthenticationTicket Ticket
		{
			get {
				return ticket;
			}
		}
	}
}

