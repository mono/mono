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
using System.Text;
using System.Collections;
using System.Collections.Specialized;

namespace ByteFX.Data.Common
{
	/// <summary>
	/// Summary description for StringUtility.
	/// </summary>
	public class StringUtility
	{
		public StringUtility()
		{
		}

		public static string[] Split( string src, char delimiter, params char[] quotedelims )
		{
			ArrayList		strings = new ArrayList();
			StringBuilder	sb = new StringBuilder();
			ArrayList		ar = new ArrayList(quotedelims);
			char			quote_open = Char.MinValue;

			foreach (char c in src) 
			{
				if (c == delimiter && quote_open == Char.MinValue) 
				{
					strings.Add( sb.ToString() );
					sb.Remove( 0, sb.Length );
				}
					
				else if (ar.Contains(c)) 
				{
					if (quote_open == Char.MinValue)
						quote_open = c;
					else if (quote_open == c)
						quote_open = Char.MinValue;
					sb.Append(c);
				}
				else
					sb.Append( c );
			}

			if (sb.Length > 0)
				strings.Add( sb.ToString());

			return (string[])strings.ToArray(typeof(string));
		}
	}
}
