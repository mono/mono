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
	internal abstract class DBConnectionString
	{
		protected Hashtable	keyValues = new Hashtable();
		protected string	connectionName = String.Empty;
		protected string	connectString;

		public DBConnectionString()
		{	
			keyValues = GetDefaultValues();
		}

		public void SetConnectionString(string value)
		{
			Hashtable ht = Parse( value );			
			connectString = value;
			keyValues = ht;
		}

		protected string GetString(string name) 
		{
			if (! keyValues.ContainsKey(name)) return String.Empty;
			return (keyValues[name] as string);
		}

		protected int GetInt( string name ) 
		{
			return Convert.ToInt32(keyValues[name]);
		}

		protected bool GetBool( string name ) 
		{
			return Convert.ToBoolean(keyValues[name]);
		}

		protected virtual bool ConnectionParameterParsed(Hashtable hash, string key, string value)
		{
			switch (key.ToLower()) 
			{
				case "persist security info":
					hash["persist security info"] = 
						value.ToLower() == "yes" || value.ToLower() == "true";
					return true;

				case "uid":
				case "username":
				case "user id":
				case "user name": 
				case "userid":
					hash["user id"] = value;
					return true;

				case "password": 
				case "pwd":
					hash["password"] = value;
					return true;

				case "host":
				case "server":
				case "data source":
				case "datasource":
				case "address":
				case "addr":
				case "network address":
					hash["host"] = value;
					return true;
				
				case "initial catalog":
				case "database":
					hash["database"] = value;
					return true;

				case "connection timeout":
				case "connect timeout":
					hash["connect timeout"] = Int32.Parse( value );
					return true;

				case "port":
					hash["port"] = Int32.Parse( value );
					return true;

				case "pooling":
					hash["pooling"] = 
						value.ToLower() == "yes" || value.ToLower() == "true";
					return true;

				case "min pool size":
					hash["min pool size"] = Int32.Parse(value);
					return true;

				case "max pool size":
					hash["max pool size"] = Int32.Parse(value);
					return true;

				case "connection lifetime":
					hash["connect lifetime"] = Int32.Parse(value);
					return true;
			}
			return false;
		}

		protected virtual Hashtable GetDefaultValues()
		{
			return null;
		}

		protected Hashtable ParseKeyValuePairs( string src )
		{
			String[] keyvalues = src.Split( ';' );
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

			Hashtable hash = new Hashtable();

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

				hash.Add( parts[0], parts[1] );
			}
			return hash;
		}

		protected virtual Hashtable Parse(string newConnectString) 
		{
			Hashtable hash = ParseKeyValuePairs( newConnectString );
			Hashtable newHash = GetDefaultValues();

			foreach (object key in hash.Keys)
				ConnectionParameterParsed( newHash, (string)key, (string)hash[key] );
			return newHash;
		}


	}
}
