//
// System.Web.Configuration.PropertyHelper
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.ComponentModel;
using System.Configuration;

namespace System.Web.Configuration
{
	static class PropertyHelper
	{
		internal static WhiteSpaceTrimStringConverter WhiteSpaceTrimStringConverter = new WhiteSpaceTrimStringConverter ();
		internal static InfiniteTimeSpanConverter InfiniteTimeSpanConverter = new InfiniteTimeSpanConverter ();
		internal static InfiniteIntConverter InfiniteIntConverter = new InfiniteIntConverter ();
		internal static TimeSpanMinutesConverter TimeSpanMinutesConverter = new TimeSpanMinutesConverter ();
		internal static TimeSpanSecondsOrInfiniteConverter TimeSpanSecondsOrInfiniteConverter = new TimeSpanSecondsOrInfiniteConverter ();
		internal static TimeSpanSecondsConverter TimeSpanSecondsConverter = new TimeSpanSecondsConverter ();
		internal static CommaDelimitedStringCollectionConverter CommaDelimitedStringCollectionConverter = new CommaDelimitedStringCollectionConverter();
		internal static DefaultValidator DefaultValidator = new DefaultValidator ();
		internal static NullableStringValidator NonEmptyStringValidator = new NullableStringValidator (1);
		internal static PositiveTimeSpanValidator PositiveTimeSpanValidator = new PositiveTimeSpanValidator ();
		internal static TimeSpanMinutesOrInfiniteConverter TimeSpanMinutesOrInfiniteConverter = new TimeSpanMinutesOrInfiniteConverter ();

		internal static IntegerValidator IntFromZeroToMaxValidator = new IntegerValidator (0, Int32.MaxValue);
		internal static IntegerValidator IntFromOneToMax_1Validator = new IntegerValidator (1, Int32.MaxValue - 1);
#if NET_4_0
		internal static VersionConverter VersionConverter = new VersionConverter ();
#endif
	}
}

