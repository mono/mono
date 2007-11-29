//
// PatternDataBuilder.jvm.cs
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

namespace System.Text.RegularExpressions
{
	sealed class PatternDataBuilder
	{
		private static readonly List<IConstructType> _monoConstructTypesList = new List<IConstructType>();
		private static readonly List<IConstructType> _jvmOrderedConstructTypesList = new List<IConstructType> ();

		static PatternDataBuilder () {
			FillMonoConstructTypesList ();
			FillJvmOrderedConstructTypesList ();
		}

		internal static PatternData GetPatternData (string pattern, RegexOptions options) {

			foreach (IConstructType construct in _monoConstructTypesList) {
				if (construct.HasConstruct (pattern, options)) {
					return null;
				}
			}

			PatternGrouping patternGrouping = new PatternGrouping ();
			string reformattedPattern = pattern;

			foreach (IConstructType construct in _jvmOrderedConstructTypesList) {
				
				reformattedPattern = construct.Reformat (options, reformattedPattern, patternGrouping);
				
				if (reformattedPattern == null) 
					return null;
			}

			return new PatternData (options, reformattedPattern, patternGrouping);
		}

		private static void FillMonoConstructTypesList () {
			_monoConstructTypesList.Add (new CategoryConstruct ());
			_monoConstructTypesList.Add (new BalancingGroupConstruct ());
			_monoConstructTypesList.Add (new AlternationBackReferenceConstruct ());
			_monoConstructTypesList.Add (new AlternationExpressionConstruct ());
			_monoConstructTypesList.Add (new RightToLeftOptionConstruct ());
			_monoConstructTypesList.Add (new InlineExplicitCaptureConstruct ());
			_monoConstructTypesList.Add (new LookBehindWithUndefinedLength ());
			_monoConstructTypesList.Add (new NotAllowedConstruct ());
		}

		private static void FillJvmOrderedConstructTypesList () {

			//The order is meaningful
			_jvmOrderedConstructTypesList.Add (new CommentsConstruct ());
			_jvmOrderedConstructTypesList.Add (new NamingGroupsConstruct ());
			_jvmOrderedConstructTypesList.Add (new ReplacedRegularCharacterConstruct ());
			_jvmOrderedConstructTypesList.Add (new BackReferenceConstruct ());
		}
	}
}