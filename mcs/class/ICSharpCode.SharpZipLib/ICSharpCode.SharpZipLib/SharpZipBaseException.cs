// SharpZipBaseException.cs
//
// Copyright 2004 John Reilly
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

namespace ICSharpCode.SharpZipLib
{
	/// <summary>
	/// SharpZipBaseException is the base exception class for the SharpZipLibrary.
	/// All library exceptions are derived from this.
	/// </summary>
	[System.ObsoleteAttribute("This assembly has been deprecated. Please use https://www.nuget.org/packages/SharpZipLib/ instead.")]
	public class SharpZipBaseException : ApplicationException
	{
		/// <summary>
		/// Initializes a new instance of the SharpZipLibraryException class.
		/// </summary>
		public SharpZipBaseException()
		{
		}
		
		/// <summary>
		/// Initializes a new instance of the SharpZipLibraryException class with a specified error message.
		/// </summary>
		public SharpZipBaseException(string msg) : base(msg)
		{
		}

		/// <summary>
		/// Initializes a new instance of the SharpZipLibraryException class with a specified
		/// error message and a reference to the inner exception that is the cause of this exception.
		/// </summary>
		/// <param name="message">Error message string</param>
		/// <param name="innerException">The inner exception</param>
		public SharpZipBaseException(string message, Exception innerException)	: base(message, innerException)
		{
		}
	}
}
