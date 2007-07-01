//
// System.Data.SqlClient.SqlNotificationInfo.cs
//
// Author:
//   Umadevi S <sumadevi@novell.com>
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

#if NET_2_0
namespace System.Data.SqlClient
{
	/// <summary>
	/// Provides additional infoatmion about the different notifications that can be 
	/// received by the dependency event handler
	/// </summary>
	public enum SqlNotificationInfo
	{
		AlreadyChanged = -2,
		Alter = 5,
		Delete = 3,
		Drop = 4,
		Error = 7,
		Expired = 12,
		Insert = 1,
		Invalid = 9,
		Isolation = 11,
		Options = 10,
		PreviousFire = 14,
		Query = 8,
		Resource = 13,
		Restart = 6,
		TemplateLimit = 15,
		Truncate = 0,
		Unknown = -1,
		Update = 2
	}
}
#endif
