//
// System.Web.Security.FormsAuthenticationTicket
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//

using System;

namespace System.Web.Security
{
	[Serializable]
	public sealed class FormsAuthenticationTicket
	{
		int version;
		string name;
		DateTime issueDate;
		DateTime expiration;
		bool isPersistent;
		string userData;
		string cookiePath;

		public FormsAuthenticationTicket (int version,
						  string name,
						  DateTime issueDate,
						  DateTime expiration,
						  bool isPersistent,
						  string userData)
		{
			this.version = version;
			this.name = name;
			this.issueDate = issueDate;
			this.expiration = expiration;
			this.isPersistent = isPersistent;
			this.userData = userData;
			this.cookiePath = "/";
		}

		public FormsAuthenticationTicket (int version,
						  string name,
						  DateTime issueDate,
						  DateTime expiration,
						  bool isPersistent,
						  string userData,
						  string cookiePath)
		{
			this.version = version;
			this.name = name;
			this.issueDate = issueDate;
			this.expiration = expiration;
			this.isPersistent = isPersistent;
			this.userData = userData;
			this.cookiePath = cookiePath;
		}

		public FormsAuthenticationTicket (string name, bool isPersistent, int timeout)
		{
			this.version = 1;
			this.name = name;
			this.issueDate = DateTime.Now;
			this.isPersistent = isPersistent;
			if (isPersistent)
				expiration = issueDate.AddYears (50);
			else
				expiration = issueDate.AddMinutes ((double) timeout);

			this.userData = String.Empty;
			this.cookiePath = "/";
		}

		internal void SetDates (DateTime issueDate, DateTime expiration)
		{
			this.issueDate = issueDate;
			this.expiration = expiration;
		}
		
		internal FormsAuthenticationTicket Clone ()
		{
			return new FormsAuthenticationTicket   (version,
								name,
								issueDate,
								expiration,
								isPersistent,
								userData,
								cookiePath);
		}

		public string CookiePath
		{
			get {
				return cookiePath;
			}
		}

		public DateTime Expiration
		{
			get {
				return expiration;
			}
		}

		public bool Expired
		{
			get {
				return DateTime.Now > expiration;
			}
		}

		public bool IsPersistent
		{
			get {
				return isPersistent;
			}
		}

		public DateTime IssueDate
		{
			get {
				return issueDate;
			}
		}

		public string Name
		{
			get {
				return name;
			}
		}

		public string UserData
		{
			get {
				return userData;
			}
		}

		public int Version
		{
			get {
				return version;
			}
		}
	}
}

