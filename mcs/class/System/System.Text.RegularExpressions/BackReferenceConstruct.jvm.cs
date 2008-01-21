//
// BackReferenceConstruct.jvm.cs
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
	sealed class BackReferenceConstruct : IConstructType
	{
		private const string NUMBER_BACK_REFERENCE_PATTERN = @"(?<=(?:[^\\]|\A)(?:[\\]{2}){0,1073741823})\\(\d+)";
		private const string NAME_1_BACK_REFERENCE_PATTERN = @"(?<=(?:[^\\]|\A)(?:[\\]{2}){0,1073741823})\\k<(\w+)>";
		private const string NAME_2_BACK_REFERENCE_PATTERN = @"(?<=(?:[^\\]|\A)(?:[\\]{2}){0,1073741823})\\k'(\w+)'";
		private const string NUMBER = @"\d+";


		public bool HasConstruct (string pattern, RegexOptions options) {
			if (JavaUtils.IsMatch (pattern, NUMBER_BACK_REFERENCE_PATTERN)) {
				return true;
			}

			if (JavaUtils.IsMatch (pattern, NAME_1_BACK_REFERENCE_PATTERN)) {
				return true;
			}

			return JavaUtils.IsMatch (pattern, NAME_2_BACK_REFERENCE_PATTERN);
		}

		public string Reformat (RegexOptions options,
			string reformattedPattern,
			PatternGrouping patternGrouping) {
			if (!HasConstruct (reformattedPattern, options)) {
				return reformattedPattern;
			}

			if (patternGrouping.GroupCount >= 0 && patternGrouping.SameGroupsFlag) {
				return null;
			}

			Matcher m = JavaUtils.Matcher (reformattedPattern, NUMBER_BACK_REFERENCE_PATTERN);
			if (m.find ()) {
				reformattedPattern = ReplaceGroupNumber (m, reformattedPattern, patternGrouping, options);
				if (reformattedPattern == null)
					return null;
			}

			m = JavaUtils.Matcher(reformattedPattern, NAME_1_BACK_REFERENCE_PATTERN);
			if (m.find ()) {
				reformattedPattern = ReplaceGroupName (m, reformattedPattern, patternGrouping, options);
				if (reformattedPattern == null)
					return null;
			}

			m = JavaUtils.Matcher(reformattedPattern, NAME_2_BACK_REFERENCE_PATTERN);
			if (m.find ()) {
				reformattedPattern = ReplaceGroupName (m, reformattedPattern, patternGrouping, options);
				if (reformattedPattern == null)
					return null;
			}

			return reformattedPattern;
		}

		private string ReplaceGroupNumber (Matcher match,
			string reformattedPattern,
			PatternGrouping patternGrouping,
			RegexOptions options) {
			int groupNumber = int.Parse (match.group (1));
			int javaGroupNumber = groupNumber;
			int groupCount = patternGrouping.GroupCount;

			if (groupCount == -1) {
				if ((options & RegexOptions.ExplicitCapture) == RegexOptions.ExplicitCapture) {
					groupCount = 0;
				}
				else {
					groupCount = JavaUtils.GroupCount (reformattedPattern);
				}
			}
			else {
				javaGroupNumber = patternGrouping.NetToJavaNumbersMap [groupNumber];
			}

			if (groupNumber > groupCount) {
				return null;
			}

			return match.replaceFirst (@"\\" + javaGroupNumber);
		}

		private string ReplaceGroupName (Matcher match,
			string reformattedPattern,
			PatternGrouping patternGrouping,
			RegexOptions options) {

			if (patternGrouping.GroupCount == -1){
				return null;
			}

			string groupName = match.group (1);
			Pattern p = Pattern.compile (NUMBER);
			Matcher m = p.matcher ((CharSequence) (object) groupName);
			if (m.matches ()) {
				return ReplaceGroupNumber (match, reformattedPattern, patternGrouping, options);
			}

			if (!patternGrouping.GroupNameToNumberMap.Contains (groupName)) {
				return null;
			}

			int javaGroupNumber = patternGrouping.NetToJavaNumbersMap [(int) patternGrouping.GroupNameToNumberMap [groupName]];
			return match.replaceFirst (@"\\" + javaGroupNumber);
		}
	}
}