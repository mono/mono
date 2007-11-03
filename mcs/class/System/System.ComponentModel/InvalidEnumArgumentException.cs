//
// System.ComponentModel.InvalidEnumArgumentException.cs 
//
// Authors:
//	Duncan Mak (duncan@ximian.com)
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2002 Ximian, Inc.		http://www.ximian.com
// (C) 2003 Andreas Nahr
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.ComponentModel
{
#if NET_2_0
	[Serializable]
#endif
	public class InvalidEnumArgumentException : ArgumentException
	{

		public InvalidEnumArgumentException () : this ((string) null)
		{
		}

		public InvalidEnumArgumentException (string message) : base (message)
		{
		}

		public InvalidEnumArgumentException (string argumentName, int invalidValue, Type enumClass) :
#if NET_2_0
			base (string.Format (CultureInfo.CurrentCulture, "The value "
					+ "of argument '{0}' ({1}) is invalid for "
					+ "Enum type '{2}'.", argumentName, invalidValue,
					enumClass.Name), argumentName)
#else
			base (string.Format (CultureInfo.CurrentCulture, "Enum argument value"
					+ " {0} is not valid for {1}. {1} should be a value from {2}.", 
					invalidValue, argumentName, enumClass.Name), argumentName)
#endif
		{
		}
#if NET_2_0
		public InvalidEnumArgumentException (string message, Exception innerException)
			: base (message, innerException)
		{
		}

		protected InvalidEnumArgumentException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}
#endif
	}
}
