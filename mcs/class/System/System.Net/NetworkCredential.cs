//
// System.Net.NetworkCredential.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//
// (C) Ximian, Inc.
//

namespace System.Net
{
	public class NetworkCredential : ICredentials
	{
		// Fields
		string userName;
		string password;
		string domain;
		
		// Constructors
		public NetworkCredential ()
			: base ()
		{
		}

		public NetworkCredential (string userName, string password)
		{
			this.userName = userName;
			this.password = password;
		}

		public NetworkCredential (string userName, string password, string domain)
		{
			this.userName = userName;
			this.password = password;
			this.domain = domain;
		}

		// Properties

		public string Domain
		{
			get { return domain; }
			set { domain = value; }
		}

		public string UserName
		{
			get { return userName; }
			set { userName = value; }			
		}

		public string Password
		{
			get { return password; }
			set { password = value; }
		}

		public NetworkCredential GetCredential (Uri uri, string authType)
		{
			return this;
		}					
	}
}
