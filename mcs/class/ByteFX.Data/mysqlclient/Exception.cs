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
using System.Runtime.Serialization;

namespace ByteFX.Data.MySqlClient
{
	/// <summary>
	/// The exception that is thrown when MySQL returns an error. This class cannot be inherited.
	/// </summary>
	/// <include file='docs/MySqlException.xml' path='MyDocs/MyMembers[@name="Class"]/*'/>
	[Serializable]
	public sealed class MySqlException : SystemException
	{
		private int	errorCode;

		internal MySqlException(string msg) : base(msg)
		{
		}
		
		internal MySqlException( string msg, Exception ex ) : base(msg, ex)
		{
		}

		internal MySqlException() 
		{
		}

		internal MySqlException(string msg, int errno) : base(msg)
		{
			errorCode = errno;	
		}

		internal MySqlException(SerializationInfo info,
					StreamingContext context) : base(info, context)
		{
		}

		/// <summary>
		/// Gets a number that identifies the type of error.
		/// </summary>
		public int Number 
		{
			get { return errorCode; }
		}

	}
}
