//
// PatternData.jvm.cs
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
using System.Collections;
using java.util.regex;
using java.lang;

namespace System.Text.RegularExpressions
{

	sealed class PatternData
	{
		readonly Pattern _javaPattern;
		readonly PatternGrouping _patternGrouping;

		#region Properties

		internal int [] NetToJavaNumbersMap {
			get { return _patternGrouping.NetToJavaNumbersMap; }
		}

		internal IDictionary GroupNameToNumberMap {
			get { return _patternGrouping.GroupNameToNumberMap; }
		}

		internal string [] GroupNumberToNameMap {
			get { return _patternGrouping.GroupNumberToNameMap; }
		}

		internal int [] JavaToNetGroupNumbersMap {
			get { return _patternGrouping.JavaToNetGroupNumbersMap; }
		}

		internal int GroupCount {
			get { return _patternGrouping.GroupCount; }
		}

		internal Pattern JavaPattern {
			get { return _javaPattern; }
		}

		internal bool SameGroupsFlag {
			get { return _patternGrouping.SameGroupsFlag; }
		}

		#endregion Properties

		#region Ctors

		internal PatternData (
			RegexOptions options,
			string reformattedPattern,
			PatternGrouping patternGrouping) {

			this._patternGrouping = patternGrouping;

			_javaPattern = Pattern.compile (reformattedPattern, GetJavaFlags (options));

			FillGroups (options);
		}


		#endregion Ctors

		#region Private methods

		private void FillGroups (RegexOptions options) {
			if (_patternGrouping.GroupCount >= 0) {
				return;
			}

			Matcher m = JavaPattern.matcher ((CharSequence) (object) String.Empty);

			if ((options & RegexOptions.ExplicitCapture) == RegexOptions.ExplicitCapture) {
				_patternGrouping.SetGroups (0, m.groupCount());
			}
			else {
				_patternGrouping.SetGroups (m.groupCount(), m.groupCount ());
			}
		}

		private int GetJavaFlags (RegexOptions options) {
			int flags = Pattern.UNIX_LINES; // .NET treats only the "\n" character as newline, UNIX_LINES implies the same behavior in Java.

			if ((options & RegexOptions.IgnoreCase) == RegexOptions.IgnoreCase) {
				flags |= Pattern.CASE_INSENSITIVE;
			}
			if ((options & RegexOptions.Multiline) == RegexOptions.Multiline) {
				flags |= Pattern.MULTILINE;
			}
			if ((options & RegexOptions.Singleline) == RegexOptions.Singleline) {
				flags |= Pattern.DOTALL;
			}
			if ((options & RegexOptions.IgnorePatternWhitespace) == RegexOptions.IgnorePatternWhitespace) {
				flags |= Pattern.COMMENTS;
			}
			return flags;
		}

		#endregion Private methods
	}
}
