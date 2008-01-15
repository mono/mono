//
// System.Net.MonoHttpDate
//
// Author:
//   Lawrence Pit (loz@cable.a2000.nl)
//

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

using System;
using System.Globalization;

namespace System.Net 
{
	/// <summary>
	/// See RFC 2068 3.3.1
	/// </summary>
	internal class MonoHttpDate
	{
		private static readonly string rfc1123_date = "r";
		private static readonly string rfc850_date = "dddd, dd-MMM-yy HH:mm:ss G\\MT";
		private static readonly string asctime_date = "ddd MMM d HH:mm:ss yyyy";
		private static readonly string [] formats = 
			new string [] {rfc1123_date, rfc850_date, asctime_date};
		
		internal static DateTime Parse (string dateStr)
		{			
			 return DateTime.ParseExact (dateStr, 
			                             formats, 
			                             CultureInfo.InvariantCulture, 
			                             DateTimeStyles.AllowWhiteSpaces).ToLocalTime ();
		}
	}
}
