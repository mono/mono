//
// System.Security.Principal.GenericIdentity.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

namespace System.Security.Principal {

	[Serializable]
	public class GenericIdentity : IIdentity {
		string user_name;
		string authentication_type;
		
		public GenericIdentity (string user_name, string authentication_type)
		{
			if (user_name == null)
				throw new ArgumentNullException ("user_name");

			if (authentication_type == null)
				throw new ArgumentNullException ("authentication_type");

			this.user_name = user_name;
			this.authentication_type = authentication_type;
		}

		public GenericIdentity (string name) : this (name, "")
		{
		}

		public virtual string AuthenticationType {
			get {
				return authentication_type;
			}
		}

		public virtual string Name {
			get {
				return user_name;
			}
		}

		public virtual bool IsAuthenticated {
			get {
				return (user_name != "");
			}
		}
	}
}
