using System;
using System.Collections;
using System.ComponentModel;
using System.Text;
using ByteFX.Data.Common;

namespace ByteFX.Data.MySqlClient
{
	internal enum ConnectionProtocol 
	{
		Sockets, NamedPipe, UnixSocket
	}

	/// <summary>
	/// Summary description for MySqlConnectionString.
	/// </summary>
	internal sealed class MySqlConnectionString : DBConnectionString
	{
		private Hashtable	defaults;

		public MySqlConnectionString() : base()
		{
		}

		public MySqlConnectionString(string connectString) : this()
		{
			SetConnectionString( connectString );
		}

		#region Server Properties
		[Browsable(false)]
		public string Name 
		{
			get { return connectionName; }
			set { connectionName = value; }
		}


		[Category("Connection")]
		[Description("The name or IP address of the server to use")]
		public string Server 
		{
			get { return GetString("host"); }
			set { keyValues["host"] = value; }
		}

		[Category("Connection")]
		[Description("Port to use when connecting with sockets")]
		[DefaultValue(3306)]
		public int Port 
		{
			get { return GetInt("port"); }
			set { keyValues["port"] = value; }
		}

		[Category("Connection")]
		[Description("Protocol to use for connection to MySQL")]
		[DefaultValue(ConnectionProtocol.Sockets)]
		public ConnectionProtocol Protocol
		{
			get { return (ConnectionProtocol)keyValues["protocol"]; }
			set { keyValues["protocol"] = value; }
		}

		[Category("Connection")]
		[Description("Name of pipe to use when connecting with named pipes (Win32 only)")]
		public string PipeName 
		{
			get { return GetString("use pipe"); }
			set { keyValues["use pipe"] = value; }
		}

		[Category("Connection")]
		[Description("Should the connection ues compression")]
		[DefaultValue(false)]
		public bool UseCompression 
		{
			get { return GetBool("compress"); }
			set { keyValues["compress"] = value; }
		}

		[Category("Connection")]
		[Description("Database to use initially")]
		[Editor("ByteFX.Data.MySqlClient.Design.DatabaseTypeEditor,MySqlClient.Design", typeof(System.Drawing.Design.UITypeEditor))]
		public string Database
		{
			get { return GetString("database"); }
			set { keyValues["database"] = value; }
		}

		[Category("Connection")]
		[Description("Number of seconds to wait for the connection to succeed")]
		[DefaultValue(15)]
		public int ConnectionTimeout
		{
			get { return GetInt("connect timeout"); }
			set { keyValues["connect timeout"] = value; }
		}
		#endregion

		#region Authentication Properties

		[Category("Authentication")]
		[Description("The username to connect as")]
		public string UserId 
		{
			get { return GetString("user id"); }
			set { keyValues["user id"] = value; }
		}

		[Category("Authentication")]
		[Description("The password to use for authentication")]
		public string Password 
		{
			get { return GetString("password"); }
			set { keyValues["password"] = value; }
		}

		[Category("Authentication")]
		[Description("Should the connection use SSL.  This currently has no effect.")]
		[DefaultValue(false)]
		public bool UseSSL
		{
			get { return GetBool("use ssl"); }
			set { keyValues["use ssl"] = value; }
		}

		[Category("Authentication")]
		[Description("Show user password in connection string")]
		[DefaultValue(false)]
		public bool PersistSecurityInfo 
		{
			get { return GetBool("persist security info"); }
			set { keyValues["persist security info"] = value; }
		}
		#endregion

		#region Pooling Properties

		[Category("Pooling")]
		[Description("Should the connection support pooling")]
		[DefaultValue(true)]
		public bool Pooling 
		{
			get { return GetBool("pooling"); }
			set { keyValues["pooling"] = value; }
		}

		[Category("Pooling")]
		[Description("Minimum number of connections to have in this pool")]
		[DefaultValue(0)]
		public int MinPoolSize 
		{
			get { return GetInt("min pool size"); }
			set { keyValues["min pool size"] = value; }
		}

		[Category("Pooling")]
		[Description("Maximum number of connections to have in this pool")]
		[DefaultValue(100)]
		public int MaxPoolSize 
		{
			get { return GetInt("max pool size"); }
			set { keyValues["max pool size"] = value; }
		}

		[Category("Pooling")]
		[Description("Maximum number of seconds a connection should live.  This is checked when a connection is returned to the pool.")]
		[DefaultValue(0)]
		public int ConnectionLifetime 
		{
			get { return GetInt("connection lifetime"); }
			set { keyValues["connection lifetime"] = value; }
		}

		#endregion


		/// <summary>
		/// Takes a given connection string and returns it, possible
		/// stripping out the password info
		/// </summary>
		/// <returns></returns>
		public string GetConnectionString()
		{
			if (connectString == null) return CreateConnectionString();

			StringBuilder str = new StringBuilder();
			Hashtable ht = ParseKeyValuePairs( connectString );

			if (! PersistSecurityInfo) 
				ht.Remove("password");

			foreach( string key in ht.Keys)
				str.AppendFormat("{0}={1};", key, ht[key]);

			if (str.Length > 0)
				str.Remove( str.Length-1, 1 );

			return str.ToString();
		}

		/// <summary>
		/// Uses the values in the keyValues hash to create a
		/// connection string
		/// </summary>
		/// <returns></returns>
		public string CreateConnectionString()
		{
			string cStr = String.Empty;

			Hashtable values = (Hashtable)keyValues.Clone();
			Hashtable defaultValues = GetDefaultValues();

			if (!PersistSecurityInfo && values.Contains("password") )
				values.Remove( "password" );

			// we always return the server key.  It's not needed but 
			// seems weird for it not to be there.
			cStr = "server=" + values["host"] + ";";
			values.Remove("server");

			foreach (string key in values.Keys)
			{
				if (!values[key].Equals( defaultValues[key]))
					cStr += key + "=" + values[key] + ";";
			}

			return cStr;
		}

		protected override Hashtable GetDefaultValues()
		{
			defaults = base.GetDefaultValues();
			if (defaults == null)
			{
				defaults = new Hashtable();
				defaults["host"] = "localhost";
				defaults["connect lifetime"] = 0;
				defaults["user id"] = String.Empty;
				defaults["password"] = String.Empty;
				defaults["pooling"] = true;
				defaults["min pool size"] = 0;
				defaults["protocol"] = ConnectionProtocol.Sockets;
				defaults["max pool size"] = 100;
				defaults["connect timeout"] = 15;
				defaults["port"] = 3306;
				defaults["useSSL"] = false;
				defaults["compress"] = false;
				defaults["persist Security Info"] = false;
			}
			return (Hashtable)defaults.Clone();
		}

		protected override bool ConnectionParameterParsed(Hashtable hash, string key, string value)
		{
			switch (key.ToLower()) 
			{
				case "use compression":
				case "compress":
					hash["compress"] = 
						value.ToLower() == "yes" || value.ToLower() == "true";
					return true;

				case "protocol":
					if (value == "socket" || value == "tcp")
						hash["protocol"] = ConnectionProtocol.Sockets;
					else if (value == "pipe")
						hash["protocol"] = ConnectionProtocol.NamedPipe;
					else if (value == "unix")
						hash["protocol"] = ConnectionProtocol.UnixSocket;
					return true;

				case "use pipe":
				case "pipe":
					hash["use pipe"] = value;
					return true;
			}

			if (! base.ConnectionParameterParsed(hash, key, value))
				throw new ArgumentException("Keyword not supported: '" + key + "'");
			return true;
		}

	}
}
