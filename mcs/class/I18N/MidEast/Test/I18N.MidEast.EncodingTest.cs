//
// I18N.MidEast.EncodingTest.cs
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

namespace MonoTests.I18N.MidEast
{
	[TestFixture]
	public class EncodingTest : EncodingTestBase
	{
		static CodePageTestInfo[] codepageTestInfos = new CodePageTestInfo[]
		{
			new CodePageTestInfo { CodePage = 1254,  IsBrowserDisplay = true, IsBrowserSave = true, IsMailNewsDisplay = true, IsMailNewsSave = true, SuperscriptFiveReplacementChar = 63, InfinityReplacementChar = 63, FFReplacementChar = 63, A0Char = '\u00a0', A8Char = '\u00a8' },
			new CodePageTestInfo { CodePage = 1255,  IsBrowserDisplay = true, IsBrowserSave = true, IsMailNewsDisplay = true, IsMailNewsSave = true, SuperscriptFiveReplacementChar = 63, InfinityReplacementChar = 63, FFReplacementChar = 63, A0Char = '\u00a0', A8Char = '\u00a8' },
			new CodePageTestInfo { CodePage = 1256,  IsBrowserDisplay = true, IsBrowserSave = true, IsMailNewsDisplay = true, IsMailNewsSave = true, SuperscriptFiveReplacementChar = 63, InfinityReplacementChar = 63, FFReplacementChar = 63, A0Char = '\u00a0', A8Char = '\u00a8' },
			new CodePageTestInfo { CodePage = 28596, IsBrowserDisplay = true, IsBrowserSave = true, IsMailNewsDisplay = true, IsMailNewsSave = true, SuperscriptFiveReplacementChar = 63, InfinityReplacementChar = 63, FFReplacementChar = 65, A0Char = '\u00a0', A8Char = '\uf7ce' },
			new CodePageTestInfo { CodePage = 28598, IsBrowserDisplay = true, IsBrowserSave = true, IsMailNewsDisplay = true, IsMailNewsSave = true, SuperscriptFiveReplacementChar = 63, InfinityReplacementChar = 63, FFReplacementChar = 63, A0Char = '\u00a0', A8Char = '\u00a8' },
			new CodePageTestInfo { CodePage = 28599, IsBrowserDisplay = true, IsBrowserSave = true, IsMailNewsDisplay = true, IsMailNewsSave = true, SuperscriptFiveReplacementChar = 63, InfinityReplacementChar = 63, FFReplacementChar = 63, A0Char = '\u00a0', A8Char = '\u00a8' },
			new CodePageTestInfo { CodePage = 38598, IsBrowserDisplay = true, IsBrowserSave = true, IsMailNewsDisplay = true, IsMailNewsSave = true, SuperscriptFiveReplacementChar = 63, InfinityReplacementChar = 63, FFReplacementChar = 63, A0Char = '\u00a0', A8Char = '\u00a8' }
		};
	}
}
