//
// JvmReMachine.jvm.cs
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
using System.Collections;
using System.Text;
using java.util.regex;
using java.lang;

namespace System.Text.RegularExpressions
{
	sealed class JvmReMachine : BaseMachine
	{
		readonly PatternData _patternData;

		internal JvmReMachine (PatternData patternData) {
			this._patternData = patternData;
		}

		#region Properties

		internal PatternData PatternData {
			get { return _patternData; }
		}

		internal Pattern JavaPattern {
			get { return _patternData.JavaPattern; }
		}

		internal IDictionary Mapping {
			get { return _patternData.GroupNameToNumberMap; }
			set { throw new NotImplementedException ("Mapping setter of JvmReMachine should not be called."); }//We must implement the setter of interface but it is not in use
		}

		internal string [] NamesMapping {
			get { return _patternData.GroupNumberToNameMap; }
			set { throw new NotImplementedException ("NamesMapping setter of JvmReMachine should not be called."); }//We must implement the setter of interface but it is not in use
		}

		internal int GroupCount {
			get { return _patternData.GroupCount; }
		}

		#endregion Properties

		#region Implementations of IMachine Interface

		public override Match Scan (Regex regex, string text, int start, int end) {

			if (start > end)
				return Match.Empty;

			Matcher m = JavaPattern.matcher (((CharSequence) (object) text).subSequence(0, end));

			if (!m.find (start)) {
				return System.Text.RegularExpressions.Match.Empty;
			}

			GroupCollection groups = new GroupCollection (regex.GroupCount + 1);
			Match match = new Match (regex, this, groups, text, text.Length,
										m.start (), m.end () - m.start ());
			for (int javaGroupNumber = 1; javaGroupNumber <= m.groupCount (); ++javaGroupNumber) {
				AddGroup (m, groups, javaGroupNumber, text, match);
			}

			return match;
		}

		public override string [] Split (Regex regex, string input, int count, int startat) {
			
			string [] splitArray = JavaPattern.split ((CharSequence) (object) input.Substring (startat), count);
			
			if (regex.GroupCount == 0 || splitArray.Length == 1) {
				return splitArray;
			}

			if (count == 0)
				count = Int32.MaxValue;

			Matcher m = JavaPattern.matcher ((CharSequence) (object) input.Substring (startat));

			int splitArrayIndex = 1;

			List<string> splits = new List<string> (splitArray.Length * (1 + regex.GroupCount));
			splits.Add (splitArray [0]);

			for (int number = 0; number < count; ++number) {
				
				if (!m.find ())
					break;
				
				for (int i = 1; i <= m.groupCount (); ++i) {
					splits.Add (m.group (i));
				}

				splits.Add (splitArray [splitArrayIndex++]);
			}

			return splits.ToArray ();
		}

		public override string Replace (Regex regex, string input, string replacement, int count, int startat) {
			
			if (regex.SameGroupNamesFlag) {
				return base.Replace (regex, input, replacement, count, startat);
			}

			if (count < 0) {
				count = Int32.MaxValue;
			}

			string replacementPattern = ReplacementData.Reformat (regex, replacement);
			Matcher m = JavaPattern.matcher ((CharSequence) (object) input);

			StringBuffer sb = new StringBuffer ();
			if (count > 0 && m.find (startat)) {
				ReplacementData.ReplaceMatch (replacementPattern, m, sb, input, _patternData);
			}

			for (int matchesCounter = 1; matchesCounter < count; ++matchesCounter) {
				if (!m.find ()) {
					break;
				}
				ReplacementData.ReplaceMatch (replacementPattern, m, sb, input, _patternData);
			}


			m.appendTail (sb);
			return sb.ToString ();

		}

		public override string Result (string replacement, Match match) {
			if (match.Length == 0)
				return String.Empty;

			string result = Replace (match.Regex, match.Text, replacement, 1, 0);
			return result.Substring (match.Index, result.Length - (match.Text.Length - match.Length));

		}

		#endregion Implementations of IMachine Interface

		private void AddGroup (Matcher m, GroupCollection groups, int javaGroupNumber, string text, Match match) {
			int netGroupNumber = _patternData.JavaToNetGroupNumbersMap [javaGroupNumber];
			if (netGroupNumber == -1) {
				return;
			}

			int index = m.start (javaGroupNumber);

			if (index < 0){
				if(groups[netGroupNumber] == null) 
					groups.SetValue (new Group (), netGroupNumber);
				return;
			}

			Group group = new Group (text, index, m.end (javaGroupNumber) - index, match, netGroupNumber);

			groups.SetValue (group, netGroupNumber);
		}
	}
}