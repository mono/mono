using System;
using System.DirectoryServices;
using System.Collections.Specialized;
using System.Configuration;

namespace MonoTests.System.DirectoryServices 
{
	public class TestConfiguration
	{
		#region Fields

		private string _serverRoot;
		private string _username;
		private string _password;
		private string _baseDn;
		private AuthenticationTypes _authenticationType;

		#endregion // Fields

		#region Constructors

		public TestConfiguration()
		{
			NameValueCollection config = (NameValueCollection) ConfigurationSettings.GetConfig ("System.DirectoryServices.Test/Settings");
			if (config != null) {
				string servername = config ["servername"];
				string port = config ["port"];
				_serverRoot = "LDAP://" + servername + (port.Equals ("389") ? String.Empty : (":" + port)) + "/";

				_baseDn = config ["basedn"];

				_username = config ["username"];
				_password = config ["password"];

				string authType = config ["authenticationtype"];

				string [] authTypes = authType.Split (new char[] { '|' });

				_authenticationType = (AuthenticationTypes) 0;

				foreach (string s in authTypes)
					foreach (AuthenticationTypes type in Enum.GetValues (typeof (AuthenticationTypes)))
						if (s.Trim ().Equals (type.ToString ()))
							_authenticationType |= type;	
			}

			//Console.WriteLine ("Connecting to {0} with credentials {1}:{2} and security {3}",ConnectionString,Username,Password,AuthenticationType);
		}

		#endregion // Constructors

		#region Properties

		public string ServerRoot
		{
			get { return _serverRoot; }
		}

		public string BaseDn
		{
			get { return ((_baseDn == null) ? String.Empty : _baseDn); }
		}

		public string ConnectionString
		{ 
			get { return ServerRoot + ((BaseDn.Length == 0) ? String.Empty : BaseDn); }
		}

		public string Username
		{
			get{ return _username; }
		}

		public string Password
		{
			get { return _password; }
		}

		public AuthenticationTypes AuthenticationType
		{
			get { return _authenticationType; }
		}

		#endregion // Properties
	}
}
