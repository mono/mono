// ByteFX.Data data access components for .Net
// Copyright (C) 2002-2004  ByteFX, Inc.
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

namespace ByteFX.Data.Common
{
	/// <summary>
	/// Summary description for Version.
	/// </summary>
	internal struct DBVersion
	{
		private int	major;
		private int minor;
		private int build;

		public DBVersion( int major, int minor, int build)
		{
			this.major = major;
			this.minor = minor;
			this.build = build;
		}

		public static DBVersion Parse( string versionString )
		{
			int start = 0;
			int index = versionString.IndexOf('.', start);
			if (index == -1) throw new Exception("Version string not in acceptable format");
			int major = Convert.ToInt32( versionString.Substring(start, index-start).Trim());

			start = index+1;
			index = versionString.IndexOf('.', start);
			if (index == -1) throw new Exception("Version string not in acceptable format");
			int minor = Convert.ToInt32( versionString.Substring(start, index-start).Trim());

			start = index+1;
			int i = start;
			while (i < versionString.Length && Char.IsDigit( versionString, i ))
				i++;
			int build = Convert.ToInt32( versionString.Substring(start, i-start).Trim());

			return new DBVersion( major, minor, build );
		}

		public bool isAtLeast(int major, int minor, int build)
		{
			if (this.major > major) return true;
			if (this.major == major && this.minor > minor) return true;
			if (this.major == major && this.minor == minor && this.build >= build) return true;
			return false;
		}

	}
}
