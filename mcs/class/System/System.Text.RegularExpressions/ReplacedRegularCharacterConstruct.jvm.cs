//
// ReplacedRegularCharacterConstruct.jvm.cs
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
	sealed class ReplacedRegularCharacterConstruct : IConstructType
	{
		private const string BACKSLASH_IN_CC_PATTERN = @"(?<=(?:[^\\]|\A)(?:[\\]{2})*(?:\[|\[[^\[\]]*[^\[\]\\])(?:[\\]{2})*)\\b(?=[^\[\]]*\])";
		private const string BRACKET_IN_CC_PATTERN = @"(?<=(?:[^\\]|\A)(?:[\\]{2})*(?:\[|\[[^\[\]]*[^\[\]\\])(?:[\\]{2})*)\[(?=[^\[\]]*\])";
		private const string BACKSLASH_PATTERN = @"\\b(?=[^\[\]]*\])";
		private const string BRACKET_PATTERN = @"\[(?=[^\[\]]*\])";
		private const string BACKSLASH_BEHIND_PATTERN = @"(?:[^\\]|\A)(?:[\\]{2})*\[(?:[^\[\]\\]*(\\[^b])*)*(?:[\\]{2})*$";
		private const string BRACKET_BEHIND_PATTERN = @"(?:[^\\]|\A)(?:[\\]{2})*(?:\[(?:[^\[\]]*[^\[\]\\])?)(?:[\\]{2})*$";
		private const string BACKSLASH = @"\\u0008";
		private const string BRACKET = @"\\[";

		private const string LEFT_FIGURE_PAREN = @"(?<=(?:[^\\\{]|\A)(?:[\\]{2})*)(?<!(?:[^\\]|\A)(?:[\\]{2})*\\[pP])\{(?!\d\d*(,(\d\d*)?)?\})";
		private const string RIGHT_FIGURE_PAREN = @"(?<!(?:[^\\]|\A)(\\{2})*(?:\\(?:[pP]\{\w\w*)?|(?:\{\d\d*(,(\d\d*)?)?)))\}";
		private const string ESC_LEFT_FIGURE = @"\\{";
		private const string ESC_RIGHT_FIGURE = @"\\}";
		private const string NULL_PATTERN = @"(?<=(?:[^\\]|\A)(?:[\\]{2})*)\\0(?!\d)";
		private const string JAVA_NULL = @"\\u0000";

		public bool HasConstruct (string pattern, RegexOptions options) {
			if (JavaUtils.IsMatch (pattern, BACKSLASH_IN_CC_PATTERN)) {
				//TODO Store result 
				return true;
			}
			if (JavaUtils.IsMatch (pattern, LEFT_FIGURE_PAREN)) {
				//TODO Store result 
				return true;
			}
			if (JavaUtils.IsMatch (pattern, RIGHT_FIGURE_PAREN)) {
				//TODO Store result 
				return true;
			}
			if (JavaUtils.IsMatch (pattern, BRACKET_IN_CC_PATTERN)) {
				//TODO Store result 
				return true;
			}
			if (JavaUtils.IsMatch (pattern, NULL_PATTERN)) {
				//TODO Store result 
				return true;
			}

			return false;
		}


		public string Reformat (RegexOptions options,
			string reformattedPattern,
			PatternGrouping patternGrouping) {
			reformattedPattern = JavaUtils.ReplaceWithLookBehind (reformattedPattern, BACKSLASH_PATTERN, BACKSLASH_BEHIND_PATTERN, BACKSLASH);
			reformattedPattern = JavaUtils.ReplaceAllAdvanced (reformattedPattern, LEFT_FIGURE_PAREN, ESC_LEFT_FIGURE);
			reformattedPattern = JavaUtils.ReplaceAllAdvanced (reformattedPattern, RIGHT_FIGURE_PAREN, ESC_RIGHT_FIGURE);
			reformattedPattern = JavaUtils.ReplaceWithLookBehind (reformattedPattern, BRACKET_PATTERN, BRACKET_BEHIND_PATTERN, BRACKET);
			reformattedPattern = JavaUtils.ReplaceAllAdvanced (reformattedPattern, NULL_PATTERN, JAVA_NULL);

			return reformattedPattern;
		}
	}
}
