//
// System.Net.Authorization.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Lawrence Pit (loz@cable.a2000.nl)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

namespace System.Net {

	public class Authorization {
		string token;
		bool complete;
		string connectionGroupId;
		string [] protectionRealm;
		IAuthenticationModule module;
		
		public Authorization (string token) : this (token, true)
		{
		}

		public Authorization (string token, bool complete) 
			: this (token, complete, null)
		{
		}
		
		public Authorization (string token, bool complete, string connectionGroupId)
		{
			this.token = token;
			this.complete = complete;
			this.connectionGroupId = connectionGroupId;
		}

		public string Message {
			get { return token; }
		}

		public bool Complete {
			get { return complete; }
		}

		public string ConnectionGroupId {
			get { return connectionGroupId; }
		}	
		
		public string[] ProtectionRealm {
			get { return protectionRealm; }
			set { protectionRealm = value; }
		}

		internal IAuthenticationModule Module {
			get { return module; }
			set { module = value; }
		}
	}
}
