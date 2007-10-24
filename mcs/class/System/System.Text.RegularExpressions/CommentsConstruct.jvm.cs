//
// CommentsConstruct.jvm.cs
//
// Author:
//	Arina Itkes  <arinai@mainsoft.com>
//
// Copyright (C) 2007 Mainsoft, Inc.
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
using System.Collections.Generic;
using System.Text;
using java.util.regex;
using java.lang;

namespace System.Text.RegularExpressions
{
	sealed class CommentsConstruct : IConstructType
	{
		private const string INLINE_COMMENT_PATTERN = @"\(\?#[^\)]*\)";
		private const string COMMENT_PATTERN = @"\#[^\n\r]*";

		public bool HasConstruct (string pattern, RegexOptions options) {
			if (JavaUtils.IsMatch (pattern, INLINE_COMMENT_PATTERN)) 
				return true;
			
			if ((options & RegexOptions.IgnorePatternWhitespace) == 0) 
				return false;
		
			return JavaUtils.IsMatch (pattern, COMMENT_PATTERN);
		}

		public string Reformat (RegexOptions options,
			string reformattedPattern,
			PatternGrouping patternGrouping) {
			
			reformattedPattern = JavaUtils.ReplaceAll (reformattedPattern, INLINE_COMMENT_PATTERN, String.Empty);

			if ((options & RegexOptions.IgnorePatternWhitespace) == 0) 
				return reformattedPattern;
			
			reformattedPattern = JavaUtils.ReplaceAll (reformattedPattern, COMMENT_PATTERN, String.Empty);

			return reformattedPattern;
		}
	}
}