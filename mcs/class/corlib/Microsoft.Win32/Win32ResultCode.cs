//
// Microsoft.Win32/Win32ResultCode.cs: define win32 error values
//
// Authos:
//	Erik LeBel (eriklebel@yahoo.ca)
//
// Copyright (C) Erik LeBel 2004
// 

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
		public const int InvalidParameter = 87;
		public const int MoreData = 234;
		public const int NoMoreEntries = 259;	
	}
}
