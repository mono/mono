// SharpZipBaseException.cs
//
// Copyright 2004 John Reilly
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

namespace ICSharpCode.SharpZipLib
{
	/// <summary>
	/// SharpZipBaseException is the base exception class for the SharpZipLibrary.
	/// All library exceptions are derived from this.
	/// </summary>
	/// <remarks>NOTE: Not all exceptions thrown will be derived from this class.
	/// A variety of other exceptions are possible for example <see cref="ArgumentNullException"></see></remarks>
#if !NETCF_1_0 && !NETCF_2_0
	[Serializable]
#endif
	public class SharpZipBaseException : ApplicationException
	{
#if !NETCF_1_0 && !NETCF_2_0
		/// <summary>
		/// Deserialization constructor
		/// </summary>
		/// <param name="info"><see cref="System.Runtime.Serialization.SerializationInfo"/> for this constructor</param>
		/// <param name="context"><see cref="StreamingContext"/> for this constructor</param>
		protected SharpZipBaseException(SerializationInfo info, StreamingContext context )
			: base( info, context )
		{
		}
#endif

		/// <summary>
		/// Initializes a new instance of the SharpZipBaseException class.
		/// </summary>
		public SharpZipBaseException()
		{
		}
		
		/// <summary>
		/// Initializes a new instance of the SharpZipBaseException class with a specified error message.
		/// </summary>
		/// <param name="message">A message describing the exception.</param>
		public SharpZipBaseException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the SharpZipBaseException class with a specified
		/// error message and a reference to the inner exception that is the cause of this exception.
		/// </summary>
		/// <param name="message">A message describing the exception.</param>
		/// <param name="innerException">The inner exception</param>
		public SharpZipBaseException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}
