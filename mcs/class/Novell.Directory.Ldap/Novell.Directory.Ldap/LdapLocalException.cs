/******************************************************************************
* The MIT License
* Copyright (c) 2003 Novell Inc.  www.novell.com
* 
* Permission is hereby granted, free of charge, to any person obtaining  a copy
* of this software and associated documentation files (the Software), to deal
* in the Software without restriction, including  without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
* copies of the Software, and to  permit persons to whom the Software is 
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in 
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*******************************************************************************/
//
// Novell.Directory.Ldap.LdapLocalException.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;

namespace Novell.Directory.Ldap
{
	
	/// <summary>  Represents an Ldap exception that is not a result of a server response.</summary>
	public class LdapLocalException:LdapException
	{
		/// <summary> Constructs a default exception with no specific error information.</summary>
		public LdapLocalException():base()
		{
			return ;
		}
		
		/// <summary> Constructs a local exception with a detailed message obtained from the
		/// specified <code>MessageOrKey</code> String and the result code.
		/// 
		/// The String is used either as a message key to obtain a localized
		/// messsage from ExceptionMessages, or if there is no key in the
		/// resource matching the text, it is used as the detailed message itself.
		/// 
		/// </summary>
		/// <param name="messageOrKey"> Key to addition result information, a key into
		/// ExceptionMessages, or the information
		/// itself if the key doesn't exist.
		/// 
		/// </param>
		/// <param name="resultCode">   The result code returned.
		/// </param>
		public LdapLocalException(System.String messageOrKey, int resultCode):base(messageOrKey, resultCode, (System.String) null)
		{
			return ;
		}
		
		/// <summary> Constructs a local exception with a detailed message obtained from the
		/// specified <code>MessageOrKey</code> String and modifying arguments.
		/// Additional parameters specify the result code.
		/// 
		/// The String is used either as a message key to obtain a localized
		/// messsage from ExceptionMessages, or if there is no key in the
		/// resource matching the text, it is used as the detailed message itself.
		/// 
		/// The message in the default locale is built with the supplied arguments,
		/// which are saved to be used for building messages for other locales.
		/// 
		/// </summary>
		/// <param name="messageOrKey"> Key to addition result information, a key into
		/// ExceptionMessages, or the information
		/// itself if the key doesn't exist.
		/// 
		/// </param>
		/// <param name="arguments">   The modifying arguments to be included in the
		/// message string.
		/// 
		/// </param>
		/// <param name="resultCode">   The result code returned.
		/// </param>
		public LdapLocalException(System.String messageOrKey, System.Object[] arguments, int resultCode):base(messageOrKey, arguments, resultCode, (System.String) null)
		{
			return ;
		}
		
		/// <summary> Constructs a local exception with a detailed message obtained from the
		/// specified <code>MessageOrKey</code> String.
		/// Additional parameters specify the result code and a rootException which
		/// is the underlying cause of an error on the client.
		/// 
		/// The String is used either as a message key to obtain a localized
		/// messsage from ExceptionMessages, or if there is no key in the
		/// resource matching the text, it is used as the detailed message itself.
		/// 
		/// </summary>
		/// <param name="messageOrKey"> Key to addition result information, a key into
		/// ExceptionMessages, or the information
		/// itself if the key doesn't exist.
		/// 
		/// </param>
		/// <param name="resultCode">   The result code returned.
		/// 
		/// </param>
		/// <param name="rootException"> A throwable which is the underlying cause
		/// of the LdapException.
		/// </param>
		public LdapLocalException(System.String messageOrKey, int resultCode, System.Exception rootException):base(messageOrKey, resultCode, null, rootException)
		{
			return ;
		}
		
		/// <summary> Constructs a local exception with a detailed message obtained from the
		/// specified <code>MessageOrKey</code> String and modifying arguments.
		/// Additional parameters specify the result code
		/// and a rootException which is the underlying cause of an error
		/// on the client.
		/// 
		/// The String is used either as a message key to obtain a localized
		/// messsage from ExceptionMessages, or if there is no key in the
		/// resource matching the text, it is used as the detailed message itself.
		/// 
		/// The message in the default locale is built with the supplied arguments,
		/// which are saved to be used for building messages for other locales.
		/// 
		/// </summary>
		/// <param name="messageOrKey"> Key to addition result information, a key into
		/// ExceptionMessages, or the information
		/// itself if the key doesn't exist.
		/// 
		/// </param>
		/// <param name="arguments">   The modifying arguments to be included in the
		/// message string.
		/// 
		/// </param>
		/// <param name="resultCode">   The result code returned.
		/// 
		/// </param>
		/// <param name="rootException"> A throwable which is the underlying cause
		/// of the LdapException.
		/// </param>
		public LdapLocalException(System.String messageOrKey, System.Object[] arguments, int resultCode, System.Exception rootException):base(messageOrKey, arguments, resultCode, null, rootException)
		{
			return ;
		}
		
		/// <summary> returns a string of information about the exception and the
		/// the nested exceptions, if any.
		/// </summary>
		public override System.String ToString()
		{
			// Format the basic exception information
			return getExceptionString("LdapLocalException");
		}
	}
}
