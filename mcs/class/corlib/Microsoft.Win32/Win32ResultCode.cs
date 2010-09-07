//
// Microsoft.Win32/Win32ResultCode.cs: define win32 error values
//
// Authos:
//	Erik LeBel (eriklebel@yahoo.ca)
//
// Copyright (C) Erik LeBel 2004
// 

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

#if !NET_2_1

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.Win32
{
	/// <summary>
	///	These are some values for Win32 result codes.
	///
	///	NOTE: This code could be relocated into a common repository
	///	for error codes, along with a utility to fetch the matching 
	///	error messages. These messages should support globalization.
	///	Maybe the 'glib' libraries provide support for this.
	///	(see System/System.ComponentModel/Win32Exception.cs)
	/// </summary>
	internal class Win32ResultCode
	{
		public const int Success = 0;
		public const int FileNotFound = 2;
		public const int AccessDenied = 5;
		public const int InvalidHandle = 6;
		public const int InvalidParameter = 87;
		public const int MoreData = 234;
		public const int NetworkPathNotFound = 53;
		public const int NoMoreEntries = 259;
		public const int MarkedForDeletion = 1018;
	}
}

#endif // NET_2_1

