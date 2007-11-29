//
// ReplacementData.jvm.cs
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
	sealed class ReplacementData
	{
		private const string SINGLE_DOLLAR_PATTERN = @"(?<=(?:\A|[^\\])(?:\\{2}){0,1073741823})\$(?:\{(\w+)\}|(\d+))?";
		private const string DOUBLE_DOLLAR_PATTERN =		@"(?<=(?:\A|[^\\])(?:\\{2}){0,1073741823})\$\$";
		private const string BACKSLASH_PATTERN = @"\\";
		private const string COPY_ENTIRE_MATCH_PATTERN =	@"(?<=(?:\A|[^\\])(?:\\{2}){0,1073741823})\\\$\&";
		private const string INPUT_BEFORE_MATCH_PATTERN =	@"(?<=(?:\A|[^\\])(?:\\{2}){0,1073741823})\\\$\`";
		private const string INPUT_AFTER_MATCH_PATTERN =	@"(?<=(?:\A|[^\\])(?:\\{2}){0,1073741823})\\\$\'";
		private const string LAST_CAPTURED_GROUP_PATTERN =	@"(?<=(?:\A|[^\\])(?:\\{2}){0,1073741823})\\\$\+";
		private const string INPUT_PATTERN =				@"(?<=(?:\A|[^\\])(?:\\{2}){0,1073741823})\\\$_";
		private const string JAVA_DOLLAR = @"\\\$";
		private const string JAVA_BACKSLASH = @"\\\\";

		internal static string Reformat (Regex regex, string replacement) {

			replacement = JavaUtils.ReplaceAll (replacement, BACKSLASH_PATTERN, JAVA_BACKSLASH);
			replacement = JavaUtils.ReplaceAll (replacement, DOUBLE_DOLLAR_PATTERN, JAVA_DOLLAR);

			Pattern p = Pattern.compile (SINGLE_DOLLAR_PATTERN);
			Matcher m = p.matcher ((CharSequence) (object) replacement);

			StringBuffer sb = new StringBuffer ();
			while (m.find ()) {
				if (m.start (1) >= 0) {
					int groupNumber = regex.GroupNumberFromName (m.group (1));
					if (groupNumber >= 0) {
						m.appendReplacement (sb, @"\$" + regex.GetJavaNumberByNetNumber (groupNumber));
						continue;
					}
					if (int.TryParse (m.group (1), out groupNumber) && groupNumber <= regex.GroupCount) {
						m.appendReplacement (sb, @"\$" + regex.GetJavaNumberByNetNumber (groupNumber));
						continue;
					}

					m.appendReplacement (sb, JAVA_DOLLAR + "{" + m.group (1) + "}");
					continue;
				}
				if (m.start (2) >= 0) {
					int netGroupNumber = int.Parse (m.group (2));
					if (netGroupNumber > regex.GroupCount) {
						m.appendReplacement (sb, JAVA_DOLLAR + netGroupNumber);
						continue;
					}

					m.appendReplacement (sb, @"\$" + regex.GetJavaNumberByNetNumber (netGroupNumber));
					continue;
				}

				m.appendReplacement (sb, JAVA_DOLLAR);
			}

			m.appendTail (sb);

			return sb.ToString ();
		}

		internal static void ReplaceMatch (string replacementPattern, Matcher match, StringBuffer sb, string input, PatternData patternData) {

			replacementPattern = JavaUtils.ReplaceAll (replacementPattern, COPY_ENTIRE_MATCH_PATTERN, match.group ());
			replacementPattern = JavaUtils.ReplaceAll (replacementPattern, INPUT_BEFORE_MATCH_PATTERN, input.Substring (0, match.start ()));
			replacementPattern = JavaUtils.ReplaceAll (replacementPattern, INPUT_AFTER_MATCH_PATTERN, input.Substring (match.end ()));
			replacementPattern = JavaUtils.ReplaceAll (replacementPattern, INPUT_PATTERN, input);

			int groupsNumber = match.groupCount ();
			if (groupsNumber > 0) {
				Pattern p = Pattern.compile (LAST_CAPTURED_GROUP_PATTERN);
				Matcher m = p.matcher ((CharSequence) (object) replacementPattern);
				if (m.find ()) {
					while (groupsNumber > 0) {
						if (match.start (patternData.NetToJavaNumbersMap [groupsNumber]) >= 0) {
							break;
						}
						--groupsNumber;
					}
					if (groupsNumber > 0) {
						replacementPattern = m.replaceAll (match.group (patternData.NetToJavaNumbersMap [groupsNumber]));
					}
				}
			}

			match.appendReplacement (sb, replacementPattern);
		}
	}
}
