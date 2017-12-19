//
// I18N.CJK.EncodingTest.cs
//
// Author:
//	Alexander Köplinger (alexander.koeplinger@xamarin.com)
//
// Copyright (C) 2017 Xamarin, Inc.
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
using System.Text;

using NUnit.Framework;
using NUnit.Framework.Constraints;

using MonoTests.I18N;

namespace MonoTests.I18N.CJK
{
	[TestFixture]
	public class EncodingTest : EncodingTestBase
	{
		static CodePageTestInfo[] codepageTestInfos = new CodePageTestInfo[]
		{
			new CodePageTestInfo { CodePage = 932,   IsBrowserDisplay = true,  IsBrowserSave = true,  IsMailNewsDisplay = true,  IsMailNewsSave = true,  SuperscriptFiveReplacementChar = 63, InfinityReplacementChar = 129, FFReplacementChar = 63, A0Char = '\u003f', A8Char = '\uff68', SupplementChar = 135, SkipGetBytes7Test = true, SkipEncoderFallback2Test = true },  // FIXME: SkipGetBytes7Test, SkipEncoderFallback2Test
			new CodePageTestInfo { CodePage = 936,   IsBrowserDisplay = true,  IsBrowserSave = true,  IsMailNewsDisplay = true,  IsMailNewsSave = true,  SuperscriptFiveReplacementChar = 63, InfinityReplacementChar = 161, FFReplacementChar = 63, A0Char = '\u71c4', A8Char = '\u0036', SupplementChar = 222, CharsWritten = 2, OneChar = '\u0037', SkipEncoderFallback2Test = true },  // FIXME: CharsWritten==1 on .NET, SkipEncoderFallback2Test
			new CodePageTestInfo { CodePage = 949,   IsBrowserDisplay = true,  IsBrowserSave = true,  IsMailNewsDisplay = true,  IsMailNewsSave = true,  SuperscriptFiveReplacementChar = 63, InfinityReplacementChar = 161, FFReplacementChar = 63, A0Char = '\u003f', A8Char = '\u0036', SupplementChar = 196, CharsWritten = 2, OneChar = '\u0037', SkipEncoderFallback2Test = true },  // FIXME: CharsWritten==1 on .NET, SkipEncoderFallback2Test
			new CodePageTestInfo { CodePage = 950,   IsBrowserDisplay = true,  IsBrowserSave = true,  IsMailNewsDisplay = true,  IsMailNewsSave = true,  SuperscriptFiveReplacementChar = 63, InfinityReplacementChar = 161, FFReplacementChar = 63, A0Char = '\u003f', A8Char = '\u0036', SupplementChar = 219, CharsWritten = 3, SkipEncoderFallback2Test = true },                      // FIXME: CharsWritten==1 on .NET, SkipEncoderFallback2Test
			new CodePageTestInfo { CodePage = 51932, IsBrowserDisplay = true,  IsBrowserSave = true,  IsMailNewsDisplay = true,  IsMailNewsSave = true,  SuperscriptFiveReplacementChar = 63, InfinityReplacementChar = 161, FFReplacementChar = 63, A0Char = '\u30fb', A8Char = '\u30fb', SupplementChar = 231, CharsWritten = 4, SkipEncoderFallbackTest = true, SkipEncoderFallback2Test = true },  // FIXME: CharsWritten==3 on .NET, SkipEncoderFallbackTest, SkipEncoderFallback2Test
			new CodePageTestInfo { CodePage = 54936, IsBrowserDisplay = true,  IsBrowserSave = true,  IsMailNewsDisplay = true,  IsMailNewsSave = true,  SuperscriptFiveReplacementChar = 63, InfinityReplacementChar = 161, FFReplacementChar = 63, A0Char = '\u0038', A8Char = '\u0036', SupplementChar = 222, CharsWritten = 1, OneChar = '\u0037', SkipGetBytes7Test = true, SkipEncoderFallbackTest = true, SkipEncoderFallback2Test = true },  // FIXME: CharsWritten==1 on .NET, SkipGetBytes7Test, SkipEncoderFallbackTest, SkipEncoderFallback2Test
			new CodePageTestInfo { CodePage = 50220, IsBrowserDisplay = false, IsBrowserSave = false, IsMailNewsDisplay = true,  IsMailNewsSave = true,  SuperscriptFiveReplacementChar = 63, InfinityReplacementChar = 63,  FFReplacementChar = 63, A0Char = '\u00a0', A8Char = '\uff68', SkipGetBytes7Test = true, SkipEncoderFallbackTest = true, SkipEncoderFallback2Test = true },  // FIXME: SkipGetBytes7Test, SkipEncoderFallbackTest, SkipEncoderFallback2Test
			new CodePageTestInfo { CodePage = 50221, IsBrowserDisplay = false, IsBrowserSave = true,  IsMailNewsDisplay = true,  IsMailNewsSave = true,  SuperscriptFiveReplacementChar = 63, InfinityReplacementChar = 63,  FFReplacementChar = 63, A0Char = '\u00a0', A8Char = '\uff68', SkipGetBytes7Test = true, SkipEncoderFallbackTest = true, SkipEncoderFallback2Test = true },  // FIXME: SkipGetBytes7Test, SkipEncoderFallbackTest, SkipEncoderFallback2Test
			new CodePageTestInfo { CodePage = 50222, IsBrowserDisplay = false, IsBrowserSave = false, IsMailNewsDisplay = false, IsMailNewsSave = false, SuperscriptFiveReplacementChar = 63, InfinityReplacementChar = 63,  FFReplacementChar = 63, A0Char = '\u00a0', A8Char = '\uff68', SkipGetBytes7Test = true, SkipEncoderFallbackTest = true, SkipEncoderFallback2Test = true }   // FIXME: SkipGetBytes7Test, SkipEncoderFallbackTest, SkipEncoderFallback2Test
		};
	}
}
