//
// NamingGroupsConstruct.jvm.cs
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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using java.util.regex;
using java.lang;

namespace System.Text.RegularExpressions
{
	sealed class NamingGroupsConstruct : IConstructType
	{
		private const string NAMED_GROUP_PATTERN_1 = @"\(\?<[A-Za-z]\w*>.*\)";
		private const string NAMED_GROUP_PATTERN_2 = @"\(\?'[A-Za-z]\w*'.*\)";
		private const string NUMBERED_GROUP_PATTERN_1 = @"\(\?<\d+>.*\)";
		private const string NUMBERED_GROUP_PATTERN_2 = @"\(\?'\d+'.*\)";
		private const string LEFT_PAREN = @"\(";
		private const string ESCAPED_LEFT_PAREN_TEMPL = @"(?<=(?:[^\\]|\A)(?:[\\]{2}){0,1073741823})\\\(";
		private const string NON_CAPTURED_GROUP_PATTERN = @"(?:^\?[:imnsx=!>-]|^\?<[!=])";
		private const string NAMED_GROUP_PATTERN1 = @"^\?<([A-Za-z]\w*)>";
		private const string NAMED_GROUP_PATTERN2 = @"^\?'([A-Za-z]\w*)'";
		private const string NUMBERED_GROUP_PATTERN1 = @"^\?<(\d+)>";
		private const string NUMBERED_GROUP_PATTERN2 = @"^\?'(\d+)'";
		private const string QUESTION = "?";
		private const string REMOVED_NAME_PATTERN_TEMPL1 = @"(?<=(?:[^\\]|\A)(?:[\\]{2}){0,1073741823}\()\?<[A-Za-z]\w*>";
		private const string REMOVED_NAME_PATTERN_TEMPL2 = @"(?<=(?:[^\\]|\A)(?:[\\]{2}){0,1073741823}\()\?'[A-Za-z]\w*'";
		private const string REMOVED_NUMBERED_PATTERN_TEMPL1 = @"(?<=(?:[^\\]|\A)(?:[\\]{2}){0,1073741823}\()\?<\d+>";
		private const string REMOVED_NUMBERED_PATTERN_TEMPL2 = @"(?<=(?:[^\\]|\A)(?:[\\]{2}){0,1073741823}\()\?'\d+'";


		public bool HasConstruct (string pattern, RegexOptions options) {
			if (JavaUtils.IsMatch (pattern, NAMED_GROUP_PATTERN_1)) {
				return true;
			}
			if (JavaUtils.IsMatch (pattern, NAMED_GROUP_PATTERN_2)) {
				return true;
			}
			if (JavaUtils.IsMatch (pattern, NUMBERED_GROUP_PATTERN_1)) {
				return true;
			}
			if (JavaUtils.IsMatch (pattern, NUMBERED_GROUP_PATTERN_2)) {
				return true;
			}
			return false;
		}


		public string Reformat (RegexOptions options,
			string reformattedPattern,
			PatternGrouping patternGrouping) {
			if (!HasConstruct (reformattedPattern, options)) {
				return reformattedPattern;
			}

			UpdateGroupMapping (reformattedPattern, options, patternGrouping);

			return ReformatPattern (reformattedPattern);
		}

		private static string ReformatPattern (string reformattedPattern) {
			//Reformat pattern
			reformattedPattern = JavaUtils.ReplaceAll (reformattedPattern, REMOVED_NAME_PATTERN_TEMPL1, String.Empty);
			reformattedPattern = JavaUtils.ReplaceAll (reformattedPattern, REMOVED_NAME_PATTERN_TEMPL2, String.Empty);
			reformattedPattern = JavaUtils.ReplaceAll (reformattedPattern, REMOVED_NUMBERED_PATTERN_TEMPL1, String.Empty);
			reformattedPattern = JavaUtils.ReplaceAll (reformattedPattern, REMOVED_NUMBERED_PATTERN_TEMPL2, String.Empty);

			return reformattedPattern;
		}

		private static void UpdateGroupMapping (string reformattedPattern,
			RegexOptions options,
			PatternGrouping patternGrouping) {
		
			CharSequence workString = (CharSequence) (object) JavaUtils.ReplaceAll (reformattedPattern, ESCAPED_LEFT_PAREN_TEMPL, String.Empty);

			//Split pattern by left parenthesis
			Pattern p = Pattern.compile (LEFT_PAREN);
			string [] parts = p.split (workString);

			Pattern nonCapturedGroupPattern = Pattern.compile (NON_CAPTURED_GROUP_PATTERN);
			Pattern groupNamePattern1 = Pattern.compile (NAMED_GROUP_PATTERN1);
			Pattern groupNamePattern2 = Pattern.compile (NAMED_GROUP_PATTERN2);
			Pattern groupNumPattern1 = Pattern.compile (NUMBERED_GROUP_PATTERN1);
			Pattern groupNumPattern2 = Pattern.compile (NUMBERED_GROUP_PATTERN2);

			int enoughLength = parts.Length;
			string [] namedGroups = new string [enoughLength];
			int [] javaGroupNumberToNetGroupNumber = new int [enoughLength];
			int capturedGroupsCount = 0;
			int namedGroupsCount = 0;
			int nonamedGroupsCount = 0;
			int sameGroupsCounter = 0;

			//Scan of groups
			for (int i = 1; i < parts.Length; ++i) {
				//nonamed group            
				if (parts [i].StartsWith (QUESTION) == false) {
					javaGroupNumberToNetGroupNumber [++capturedGroupsCount] = ++nonamedGroupsCount;
					continue;
				}

				//Skip non captured groups
				Matcher partMatcher =
						nonCapturedGroupPattern.matcher ((CharSequence) (object) parts [i]);
				if (partMatcher.find ()) {
					continue;
				}

				//Find named groups by 2 patterns
				partMatcher = groupNamePattern1.matcher ((CharSequence) (object) parts [i]);
				if (partMatcher.find ()) {
					namedGroups [namedGroupsCount++] = partMatcher.group (1);
					javaGroupNumberToNetGroupNumber [++capturedGroupsCount] = -1;
					continue;
				}
				partMatcher = groupNamePattern2.matcher ((CharSequence) (object) parts [i]);
				if (partMatcher.find ()) {
					namedGroups [namedGroupsCount++] = partMatcher.group (1);
					javaGroupNumberToNetGroupNumber [++capturedGroupsCount] = -1;
					continue;
				}

				//Find explicitly numbered groups by 2 patterns
				partMatcher = groupNumPattern1.matcher ((CharSequence) (object) parts [i]);
				if (partMatcher.find ()) {
					int netGroupNumber = int.Parse (partMatcher.group (1));
					if ((options & RegexOptions.ExplicitCapture) == RegexOptions.ExplicitCapture) {
						namedGroups [namedGroupsCount++] = partMatcher.group (1);
						javaGroupNumberToNetGroupNumber [++capturedGroupsCount] = -1;						
					}
					else {
						javaGroupNumberToNetGroupNumber [++capturedGroupsCount] = netGroupNumber;
						if (javaGroupNumberToNetGroupNumber [capturedGroupsCount] != netGroupNumber) {
							++sameGroupsCounter;
						}
					}
					continue;
				}
				partMatcher = groupNumPattern2.matcher ((CharSequence) (object) parts [i]);
				if (partMatcher.find ()) {
					int netGroupNumber = int.Parse (partMatcher.group (1));
					if ((options & RegexOptions.ExplicitCapture) == RegexOptions.ExplicitCapture) {
						namedGroups [namedGroupsCount++] = partMatcher.group (1);
						javaGroupNumberToNetGroupNumber [++capturedGroupsCount] = -1;
					}
					else {
						javaGroupNumberToNetGroupNumber [++capturedGroupsCount] = netGroupNumber;
						if (javaGroupNumberToNetGroupNumber [capturedGroupsCount] != netGroupNumber) {
							++sameGroupsCounter;
						}
					}
					continue;
				}
			}

			//Filling grouping
			patternGrouping.SetGroups (namedGroups,
				javaGroupNumberToNetGroupNumber,
				nonamedGroupsCount,
				capturedGroupsCount,
				sameGroupsCounter,
				options);

			return;
		}
	}
}