// ZipException.cs
//
// Copyright (C) 2001 Mike Krueger
//
// This file was translated from java, it was part of the GNU Classpath
// Copyright (C) 1998, 1999, 2000, 2001 Free Software Foundation, Inc.
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
//
// Linking this library statically or dynamically with other modules is
// making a combined work based on this library.  Thus, the terms and
// conditions of the GNU General Public License cover the whole
// combination.
// 
// As a special exception, the copyright holders of this library give you
// permission to link this library with independent modules to produce an
// executable, regardless of the license terms of these independent
// modules, and to copy and distribute the resulting executable under
// terms of your choice, provided that you also meet, for each linked
// independent module, the terms and conditions of the license of that
// module.  An independent module is a module which is not derived from
// or based on this library.  If you modify this library, you may extend
// this exception to your version of the library, but you are not
// obligated to do so.  If you do not wish to do so, delete this
// exception statement from your version.

using System;

#if !NETCF_1_0 && !NETCF_2_0
using System.Runtime.Serialization;
#endif

namespace ICSharpCode.SharpZipLib.Zip 
{
	
	/// <summary>
	/// Represents exception conditions specific to Zip archive handling
	/// </summary>
#if !NETCF_1_0 && !NETCF_2_0
	[Serializable]
#endif
	public class ZipException : SharpZipBaseException
	{
#if !NETCF_1_0 && !NETCF_2_0
		/// <summary>
		/// Deserialization constructor
		/// </summary>
		/// <param name="info"><see cref="SerializationInfo"/> for this constructor</param>
		/// <param name="context"><see cref="StreamingContext"/> for this constructor</param>
		protected ZipException(SerializationInfo info, StreamingContext context )
			: base( info, context )
		{
		}
#endif

		/// <summary>
		/// Initializes a new instance of the ZipException class.
		/// </summary>
		public ZipException()
		{
		}
		
		/// <summary>
		/// Initializes a new instance of the ZipException class with a specified error message.
		/// </summary>
		/// <param name="message">The error message that explains the reason for the exception.</param>
		public ZipException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// Initialise a new instance of ZipException.
		/// </summary>
		/// <param name="message">A message describing the error.</param>
		/// <param name="exception">The exception that is the cause of the current exception.</param>
		public ZipException(string message, Exception exception)
			: base(message, exception)
		{
		}
	}
}
