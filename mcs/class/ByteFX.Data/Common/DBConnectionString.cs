// ByteFX.Data data access components for .Net
// Copyright (C) 2002-2003  ByteFX, Inc.
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

using System;
using System.Collections;
using System.Text;

namespace ByteFX.Data.Common
{
	/// <summary>
	/// Summary description for Utility.
	/// </summary>
	internal class DBConnectionString
	{
		protected Hashtable	keyValues = new Hashtable();
		protected string	connectString;
		protected string	host;
		protected string	username;
		protected string	password;
		protected string	database;
		protected int		connectTimeout;
		protected int		port;
		protected int		maxPoolSize;
		protected int		minPoolSize;
		protected int		connectLifetime;
		protected bool		pooling;
		protected bool		persistSecurityInfo;

		public DBConnectionString()
		{	
			persistSecurityInfo = false;
		}

		public DBConnectionString(string connectString) : this()
		{
			this.connectString = connectString;
		}

		#region Properties
		public string Host 
		{
			get { return host; }
		}

		public string Username 
		{
			get { return username; }
		}

		public string Password 
		{
			get { return password; }
		}

		public int ConnectTimeout
		{
			get { return connectTimeout; }
		}
		
		public string Database 
		{
			get { return database; }
		}

		public int Port 
		{
			get { return port; }
		}

		public int MaxPoolSize 
		{
			get { return maxPoolSize; }
		}

		public int MinPoolSize 
		{
			get { return minPoolSize; }
		}

		public bool Pooling 
		{
			get { return pooling; }
		}

		public int ConnectionLifetime 
		{
			get { return connectLifetime; }
		}

		public string ConnectString 
		{
			get { return GetConnectionString(); }
			set { connectString = value; Parse(); }
		}

		#endregion

		private string GetConnectionString()
		{
			StringBuilder str = new StringBuilder();

			foreach ( string key in keyValues.Keys)
			{
				if ((key.ToLower() == "pwd" || key.ToLower() == "password") &&
					!persistSecurityInfo) continue;
				str.AppendFormat("{0}={1};", key, keyValues[key]);
			}

			if (str.Length > 0)
				str.Remove( str.Length-1, 1 );

			return str.ToString();
		}

		protected virtual void ConnectionParameterParsed(string key, string value)
		{
			switch (key.ToLower()) 
			{
				case "persist security info":
					if (value.ToLower() == "no" || value.ToLower() == "false")
						persistSecurityInfo = false;
					else
						persistSecurityInfo = true;
					break;

				case "uid":
				case "username":
				case "user id":
				case "user name": 
				case "userid":
					username = value;
					break;

				case "password": 
				case "pwd":
					password = value;
					break;

				case "host":
				case "server":
				case "data source":
				case "datasource":
				case "address":
				case "addr":
				case "network address":
					host = value;
					break;
				
				case "initial catalog":
				case "database":
					database = value;
					break;

				case "connection timeout":
				case "connect timeout":
					connectTimeout = Int32.Parse( value );
					break;

				case "port":
					port = Int32.Parse( value );
					break;

				case "pooling":
					if (value.ToLower() == "no" || value.ToLower() == "false")
						pooling = false;
					else
						pooling = true;
					break;

				case "min pool size":
					minPoolSize = Int32.Parse(value);
					break;

				case "max pool size":
					maxPoolSize = Int32.Parse(value);
					break;

				case "connection lifetime":
					connectLifetime = Int32.Parse(value);
					break;
			}
		}

		protected void Parse() 
		{
			String[] keyvalues = connectString.Split( ';' );
			String[] newkeyvalues = new String[keyvalues.Length];
			int		 x = 0;

			// first run through the array and check for any keys that
			// have ; in their value
			foreach (String keyvalue in keyvalues) 
			{
				// check for trailing ; at the end of the connection string
				if (keyvalue.Length == 0) continue;

				// this value has an '=' sign so we are ok
				if (keyvalue.IndexOf('=') >= 0) 
				{
					newkeyvalues[x++] = keyvalue;
				}
				else 
				{
					newkeyvalues[x-1] += ";";
					newkeyvalues[x-1] += keyvalue;
				}
			}

			keyValues.Clear();

			// now we run through our normalized key-values, splitting on equals
			for (int y=0; y < x; y++) 
			{
				String[] parts = newkeyvalues[y].Split( '=' );

				// first trim off any space and lowercase the key
				parts[0] = parts[0].Trim().ToLower();
				parts[1] = parts[1].Trim();

				// we also want to clear off any quotes
				parts[0] = parts[0].Trim('\'', '"');
				parts[1] = parts[1].Trim('\'', '"');

				ConnectionParameterParsed( parts[0], parts[1] );
				keyValues.Add( parts[0], parts[1] );
			}
		}


	}
}
