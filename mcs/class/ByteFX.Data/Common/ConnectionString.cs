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
	public class ConnectionString
	{
		public ConnectionString()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		public static StringDictionary ParseConnectString( String s ) 
		{
			String[] keyvalues = s.Split( ';' );
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

			StringDictionary dict = new StringDictionary();

			// now we run through our normalized key-values, splitting on equals
			for (int y=0; y < x; y++) 
			{
				String[] parts = newkeyvalues[y].Split( '=' );
				parts[0] = parts[0].ToLower();

				// normalize the keys going in
				if (parts[0].Equals("pwd")) parts[0] = "password";
				if (parts[0].Equals("uid")) parts[0] = "user id";
				if (parts[0].Equals("initial catalog")) parts[0] = "database";

				// we also want to clear off any quotes
				// first trim off any space
				parts[1] = parts[1].Trim();
				String newvalue = parts[1].Trim( '\'' );
				if (newvalue.Length == parts[1].Length) 
				{
					newvalue = parts[1].Trim('"');
				}
				parts[1] = newvalue;

				// make sure we don't get dupliate keys
				if (dict.ContainsKey(parts[0])) 
				{
					throw new ArgumentException("Duplicate key in connection string", parts[0]);
				}

				dict.Add( parts[0], parts[1] );
			}

			// now make sure we have some reasonable defaults for keys not present
			if (! dict.ContainsKey("connection timeout")) 
			{
				dict.Add("connection timeout", "15");
			}

			return dict;
		}
	}
}
