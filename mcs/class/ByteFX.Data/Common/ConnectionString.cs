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
using System.Collections.Specialized;

namespace ByteFX.Data.Common
{
	/// <summary>
	/// Summary description for Utility.
	/// </summary>
	internal class ConnectionString
	{
		private	StringDictionary	elements;
		private string				connString;

		public ConnectionString(string connString)
		{
			this.connString = connString;
			elements = new StringDictionary();
			Parse( connString );
		}

		public string Value
		{
			get { return connString; }
		}

		public string this[string key] 
		{
			get 
			{ 
				string val = elements[key];
				return val;
			}
		}

		public int  GetIntOption( string key, int defaultvalue ) 
		{
			string val = this[ key ];
			if (null == val) return defaultvalue;
			return Convert.ToInt32( val );
		}

		public bool  GetBoolOption( string key, bool defaultvalue ) 
		{
			string val = this[ key ];
			if (null == val) return defaultvalue;
			val = val.ToLower();
			if (val == "true" || val == "yes") return true;
			return false;
		}

		public bool Contains( string key ) 
		{
			return elements.ContainsKey(key);
		}

		public bool Equals( ConnectionString obj ) 
		{
			foreach (string key in elements.Keys) 
			{
				if (! obj.Contains(key)) return false;
				if ( ! this[key].Equals( obj[key] )) return false;
			}
			return true;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public bool PoolingEquals( ConnectionString obj ) 
		{
			foreach (string key in elements.Keys) 
			{
				// these connection string elements only affect pooling
				// so we don't check them when making sure connection strings
				// are alike
				if (key.Equals("connection lifetime")) continue;
				if (key.Equals("connection reset")) continue;
				if (key.Equals("enlist")) continue;
				if (key.Equals("max pool size")) continue;
				if (key.Equals("min pool size")) continue;
				if (key.Equals("pooling")) continue;

				if (! obj.Contains(key)) return false;
				if ( ! this[key].Equals( obj[key] )) return false;
			}
			return true;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="s"></param>
		public void Parse( String s ) 
		{
			String[] keyvalues = s.Split( ';' );
			String[] newkeyvalues = new String[keyvalues.Length];
			int		 x = 0;

			elements.Clear();

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

			// now we run through our normalized key-values, splitting on equals
			for (int y=0; y < x; y++) 
			{
				String[] parts = newkeyvalues[y].Split( '=' );

				// first trim off any space and lowercase the key
				parts[0] = parts[0].Trim().ToLower();
				parts[1] = parts[1].Trim();

				// normalize the keys going in.  We want to support the same synonyms that
				// SqlClient supports
				switch (parts[0]) 
				{
					case "uid": parts[0] = "user id"; break;
					case "pwd": parts[0] = "password"; break;
					case "user": parts[0] = "user id"; break;
					case "initial catalog": parts[0] = "database"; break;
					case "server": parts[0] = "data source"; break;
				}

				// we also want to clear off any quotes
				String newvalue = parts[1].Trim( '\'' );
				if (newvalue.Length == parts[1].Length) 
				{
					newvalue = parts[1].Trim('"');
				}
				parts[1] = newvalue;

				// make sure we don't get dupliate keys
				if (elements.ContainsKey(parts[0])) 
				{
					throw new ArgumentException("Duplicate key in connection string", parts[0]);
				}

				elements.Add( parts[0], parts[1] );

				// now put the correct parsed string into the connection string !! (AG 4/8/2003)
				connString="";
				foreach(string key in elements.Keys)
				{
					connString=connString+key+"="+elements[key]+"; ";
				}
				connString=connString.Substring(0,connString.Length-2);
			}
		}
	}
}
