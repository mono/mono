//
// I18N.Rare.EncodingTest.cs
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

namespace MonoTests.I18N.Rare
{
	[TestFixture]
	public class EncodingTest : EncodingTestBase
	{
		static CodePageTestInfo[] codepageTestInfos = new CodePageTestInfo[]
		{
			// FIXME: a lot of the tests fail because of different expectations of the test string (i.e. likely not a product bug), needs some rework of the tests
			// new CodePageTestInfo { CodePage = 37,    IsBrowserDisplay = false, IsBrowserSave = false, IsMailNewsDisplay = false, IsMailNewsSave = false, SuperscriptFiveReplacementChar = 63, InfinityReplacementChar = 63, FFReplacementChar = 63, A0Char = '\u00a0', A8Char = '\u00a8' },
			// new CodePageTestInfo { CodePage = 500,   IsBrowserDisplay = false, IsBrowserSave = false, IsMailNewsDisplay = false, IsMailNewsSave = false, SuperscriptFiveReplacementChar = 63, InfinityReplacementChar = 63, FFReplacementChar = 63, A0Char = '\u00a0', A8Char = '\u00a8' },
			// new CodePageTestInfo { CodePage = 708,   IsBrowserDisplay = false, IsBrowserSave = false, IsMailNewsDisplay = false, IsMailNewsSave = false, SuperscriptFiveReplacementChar = 63, InfinityReplacementChar = 63, FFReplacementChar = 63, A0Char = '\u00a0', A8Char = '\u00a8' },
			// new CodePageTestInfo { CodePage = 852,   IsBrowserDisplay = false, IsBrowserSave = false, IsMailNewsDisplay = false, IsMailNewsSave = false, SuperscriptFiveReplacementChar = 63, InfinityReplacementChar = 63, FFReplacementChar = 63, A0Char = '\u00a0', A8Char = '\u00a8' },
			// new CodePageTestInfo { CodePage = 855,   IsBrowserDisplay = false, IsBrowserSave = false, IsMailNewsDisplay = false, IsMailNewsSave = false, SuperscriptFiveReplacementChar = 63, InfinityReplacementChar = 63, FFReplacementChar = 63, A0Char = '\u00a0', A8Char = '\u00a8' },
			// new CodePageTestInfo { CodePage = 857,   IsBrowserDisplay = false, IsBrowserSave = false, IsMailNewsDisplay = false, IsMailNewsSave = false, SuperscriptFiveReplacementChar = 63, InfinityReplacementChar = 63, FFReplacementChar = 63, A0Char = '\u00a0', A8Char = '\u00a8' },
			// new CodePageTestInfo { CodePage = 858,   IsBrowserDisplay = false, IsBrowserSave = false, IsMailNewsDisplay = false, IsMailNewsSave = false, SuperscriptFiveReplacementChar = 63, InfinityReplacementChar = 63, FFReplacementChar = 63, A0Char = '\u00a0', A8Char = '\u00a8' },
			// new CodePageTestInfo { CodePage = 862,   IsBrowserDisplay = false, IsBrowserSave = false, IsMailNewsDisplay = false, IsMailNewsSave = false, SuperscriptFiveReplacementChar = 63, InfinityReplacementChar = 63, FFReplacementChar = 63, A0Char = '\u00a0', A8Char = '\u00a8' },
			// new CodePageTestInfo { CodePage = 864,   IsBrowserDisplay = false, IsBrowserSave = false, IsMailNewsDisplay = false, IsMailNewsSave = false, SuperscriptFiveReplacementChar = 63, InfinityReplacementChar = 63, FFReplacementChar = 63, A0Char = '\u00a0', A8Char = '\u00a8' },
			// new CodePageTestInfo { CodePage = 866,   IsBrowserDisplay = false, IsBrowserSave = false, IsMailNewsDisplay = false, IsMailNewsSave = false, SuperscriptFiveReplacementChar = 63, InfinityReplacementChar = 63, FFReplacementChar = 63, A0Char = '\u00a0', A8Char = '\u00a8' },
			// new CodePageTestInfo { CodePage = 869,   IsBrowserDisplay = false, IsBrowserSave = false, IsMailNewsDisplay = false, IsMailNewsSave = false, SuperscriptFiveReplacementChar = 63, InfinityReplacementChar = 63, FFReplacementChar = 63, A0Char = '\u00a0', A8Char = '\u00a8' },
			// new CodePageTestInfo { CodePage = 870,   IsBrowserDisplay = false, IsBrowserSave = false, IsMailNewsDisplay = false, IsMailNewsSave = false, SuperscriptFiveReplacementChar = 63, InfinityReplacementChar = 63, FFReplacementChar = 63, A0Char = '\u00a0', A8Char = '\u00a8' },
			// new CodePageTestInfo { CodePage = 875,   IsBrowserDisplay = false, IsBrowserSave = false, IsMailNewsDisplay = false, IsMailNewsSave = false, SuperscriptFiveReplacementChar = 63, InfinityReplacementChar = 63, FFReplacementChar = 63, A0Char = '\u00a0', A8Char = '\u00a8' },
			// new CodePageTestInfo { CodePage = 1026,  IsBrowserDisplay = false, IsBrowserSave = false, IsMailNewsDisplay = false, IsMailNewsSave = false, SuperscriptFiveReplacementChar = 63, InfinityReplacementChar = 63, FFReplacementChar = 63, A0Char = '\u00a0', A8Char = '\u00a8' },
			// new CodePageTestInfo { CodePage = 1047,  IsBrowserDisplay = false, IsBrowserSave = false, IsMailNewsDisplay = false, IsMailNewsSave = false, SuperscriptFiveReplacementChar = 63, InfinityReplacementChar = 63, FFReplacementChar = 63, A0Char = '\u00a0', A8Char = '\u00a8' },
			// new CodePageTestInfo { CodePage = 1140,  IsBrowserDisplay = false, IsBrowserSave = false, IsMailNewsDisplay = false, IsMailNewsSave = false, SuperscriptFiveReplacementChar = 63, InfinityReplacementChar = 63, FFReplacementChar = 63, A0Char = '\u00a0', A8Char = '\u00a8' },
			// new CodePageTestInfo { CodePage = 1141,  IsBrowserDisplay = false, IsBrowserSave = false, IsMailNewsDisplay = false, IsMailNewsSave = false, SuperscriptFiveReplacementChar = 63, InfinityReplacementChar = 63, FFReplacementChar = 63, A0Char = '\u00a0', A8Char = '\u00a8' },
			// new CodePageTestInfo { CodePage = 1142,  IsBrowserDisplay = false, IsBrowserSave = false, IsMailNewsDisplay = false, IsMailNewsSave = false, SuperscriptFiveReplacementChar = 63, InfinityReplacementChar = 63, FFReplacementChar = 63, A0Char = '\u00a0', A8Char = '\u00a8' },
			// new CodePageTestInfo { CodePage = 1143,  IsBrowserDisplay = false, IsBrowserSave = false, IsMailNewsDisplay = false, IsMailNewsSave = false, SuperscriptFiveReplacementChar = 63, InfinityReplacementChar = 63, FFReplacementChar = 63, A0Char = '\u00a0', A8Char = '\u00a8' },
			// new CodePageTestInfo { CodePage = 1144,  IsBrowserDisplay = false, IsBrowserSave = false, IsMailNewsDisplay = false, IsMailNewsSave = false, SuperscriptFiveReplacementChar = 63, InfinityReplacementChar = 63, FFReplacementChar = 63, A0Char = '\u00a0', A8Char = '\u00a8' },
			// new CodePageTestInfo { CodePage = 1145,  IsBrowserDisplay = false, IsBrowserSave = false, IsMailNewsDisplay = false, IsMailNewsSave = false, SuperscriptFiveReplacementChar = 63, InfinityReplacementChar = 63, FFReplacementChar = 63, A0Char = '\u00a0', A8Char = '\u00a8' },
			// new CodePageTestInfo { CodePage = 1146,  IsBrowserDisplay = false, IsBrowserSave = false, IsMailNewsDisplay = false, IsMailNewsSave = false, SuperscriptFiveReplacementChar = 63, InfinityReplacementChar = 63, FFReplacementChar = 63, A0Char = '\u00a0', A8Char = '\u00a8' },
			// new CodePageTestInfo { CodePage = 1147,  IsBrowserDisplay = false, IsBrowserSave = false, IsMailNewsDisplay = false, IsMailNewsSave = false, SuperscriptFiveReplacementChar = 63, InfinityReplacementChar = 63, FFReplacementChar = 63, A0Char = '\u00a0', A8Char = '\u00a8' },
			// new CodePageTestInfo { CodePage = 1148,  IsBrowserDisplay = false, IsBrowserSave = false, IsMailNewsDisplay = false, IsMailNewsSave = false, SuperscriptFiveReplacementChar = 63, InfinityReplacementChar = 63, FFReplacementChar = 63, A0Char = '\u00a0', A8Char = '\u00a8' },
			// new CodePageTestInfo { CodePage = 1149,  IsBrowserDisplay = false, IsBrowserSave = false, IsMailNewsDisplay = false, IsMailNewsSave = false, SuperscriptFiveReplacementChar = 63, InfinityReplacementChar = 63, FFReplacementChar = 63, A0Char = '\u00a0', A8Char = '\u00a8' },
			// new CodePageTestInfo { CodePage = 20273, IsBrowserDisplay = false, IsBrowserSave = false, IsMailNewsDisplay = false, IsMailNewsSave = false, SuperscriptFiveReplacementChar = 63, InfinityReplacementChar = 63, FFReplacementChar = 63, A0Char = '\u00a0', A8Char = '\u00a8' },
			// new CodePageTestInfo { CodePage = 20277, IsBrowserDisplay = false, IsBrowserSave = false, IsMailNewsDisplay = false, IsMailNewsSave = false, SuperscriptFiveReplacementChar = 63, InfinityReplacementChar = 63, FFReplacementChar = 63, A0Char = '\u00a0', A8Char = '\u00a8' },
			// new CodePageTestInfo { CodePage = 20278, IsBrowserDisplay = false, IsBrowserSave = false, IsMailNewsDisplay = false, IsMailNewsSave = false, SuperscriptFiveReplacementChar = 63, InfinityReplacementChar = 63, FFReplacementChar = 63, A0Char = '\u00a0', A8Char = '\u00a8' },
			// new CodePageTestInfo { CodePage = 20280, IsBrowserDisplay = false, IsBrowserSave = false, IsMailNewsDisplay = false, IsMailNewsSave = false, SuperscriptFiveReplacementChar = 63, InfinityReplacementChar = 63, FFReplacementChar = 63, A0Char = '\u00a0', A8Char = '\u00a8' },
			// new CodePageTestInfo { CodePage = 20284, IsBrowserDisplay = false, IsBrowserSave = false, IsMailNewsDisplay = false, IsMailNewsSave = false, SuperscriptFiveReplacementChar = 63, InfinityReplacementChar = 63, FFReplacementChar = 63, A0Char = '\u00a0', A8Char = '\u00a8' },
			// new CodePageTestInfo { CodePage = 20285, IsBrowserDisplay = false, IsBrowserSave = false, IsMailNewsDisplay = false, IsMailNewsSave = false, SuperscriptFiveReplacementChar = 63, InfinityReplacementChar = 63, FFReplacementChar = 63, A0Char = '\u00a0', A8Char = '\u00a8' },
			// new CodePageTestInfo { CodePage = 20290, IsBrowserDisplay = false, IsBrowserSave = false, IsMailNewsDisplay = false, IsMailNewsSave = false, SuperscriptFiveReplacementChar = 63, InfinityReplacementChar = 63, FFReplacementChar = 63, A0Char = '\u00a0', A8Char = '\u00a8' },
			// new CodePageTestInfo { CodePage = 20297, IsBrowserDisplay = false, IsBrowserSave = false, IsMailNewsDisplay = false, IsMailNewsSave = false, SuperscriptFiveReplacementChar = 63, InfinityReplacementChar = 63, FFReplacementChar = 63, A0Char = '\u00a0', A8Char = '\u00a8' },
			// new CodePageTestInfo { CodePage = 20420, IsBrowserDisplay = false, IsBrowserSave = false, IsMailNewsDisplay = false, IsMailNewsSave = false, SuperscriptFiveReplacementChar = 63, InfinityReplacementChar = 63, FFReplacementChar = 63, A0Char = '\u00a0', A8Char = '\u00a8' },
			// new CodePageTestInfo { CodePage = 20424, IsBrowserDisplay = false, IsBrowserSave = false, IsMailNewsDisplay = false, IsMailNewsSave = false, SuperscriptFiveReplacementChar = 63, InfinityReplacementChar = 63, FFReplacementChar = 63, A0Char = '\u00a0', A8Char = '\u00a8' },
			// new CodePageTestInfo { CodePage = 20871, IsBrowserDisplay = false, IsBrowserSave = false, IsMailNewsDisplay = false, IsMailNewsSave = false, SuperscriptFiveReplacementChar = 63, InfinityReplacementChar = 63, FFReplacementChar = 63, A0Char = '\u00a0', A8Char = '\u00a8' },
			// new CodePageTestInfo { CodePage = 21025, IsBrowserDisplay = false, IsBrowserSave = false, IsMailNewsDisplay = false, IsMailNewsSave = false, SuperscriptFiveReplacementChar = 63, InfinityReplacementChar = 63, FFReplacementChar = 63, A0Char = '\u00a0', A8Char = '\u00a8' },
		};
	}
}

