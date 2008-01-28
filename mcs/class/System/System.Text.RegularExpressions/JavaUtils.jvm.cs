//
// JavaUtils.jvm.cs
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
	sealed class JavaUtils
	{
		private const string ASTERISK_PATTERN = @"\*";
		private static readonly Pattern _asteriskPattern = Pattern.compile (ASTERISK_PATTERN);

		internal static bool IsMatch (string input, string pattern) {
			Pattern p = Pattern.compile (pattern);
			Matcher m = p.matcher ((CharSequence) (object) input);

			return m.find ();
		}

		internal static Matcher Matcher (string input, string pattern) {
			Pattern p = Pattern.compile (pattern);
			return p.matcher ((CharSequence) (object) input);
		}

		internal static string ReplaceAll (string input, string pattern, string replacement) {
			Pattern p = Pattern.compile (pattern);
			Matcher m = p.matcher ((CharSequence) (object) input);
			return m.replaceAll (replacement);
		}

		internal static string ReplaceAllAdvanced (string input, string pattern, string replacement) {
			Matcher m = _asteriskPattern.matcher ((CharSequence) (object) pattern);
			string readyPattern = m.replaceAll ("{0," + pattern.Length + "}");

			return ReplaceAll (input, readyPattern, replacement);
		}

		internal static int GroupCount (string pattern) {
			Pattern javaPattern = Pattern.compile (pattern);
			Matcher emptyMatcher = javaPattern.matcher ((CharSequence) (object) String.Empty);
			return emptyMatcher.groupCount ();
		}

		internal static string ReplaceWithLookBehind (string input, string pattern, string lookBehindPattern, string replacement) {
			Pattern p = Pattern.compile (pattern);
			Pattern behindPattern = Pattern.compile (lookBehindPattern);
	
			Matcher m = p.matcher ((CharSequence) (object) input);
			StringBuffer sb = new StringBuffer ();

			while (m.find ()) {
				Matcher pm = behindPattern.matcher ((CharSequence) (object) input.Substring (0, m.start()));
				if(pm.find())
					m.appendReplacement(sb, replacement);
			}

			m.appendTail (sb);
			return sb.ToString();
		}
	}
}