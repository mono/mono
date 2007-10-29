//
// PatternGrouping.jvm.cs
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
	sealed class PatternGrouping
	{
		int _groupCount;
		IDictionary _groupNameToNumberMap;
		int [] _javaToNetGroupNumbersMap;
		string [] _groupNumberToNameMap;
		int [] _netToJavaNumbersMap;
		bool _sameGroupsFlag;

		internal string [] GroupNumberToNameMap {
			get { return _groupNumberToNameMap; }
		}

		internal int [] JavaToNetGroupNumbersMap {
			get { return _javaToNetGroupNumbersMap; }
		}

		internal IDictionary GroupNameToNumberMap {
			get { return _groupNameToNumberMap; }
		}

		internal int [] NetToJavaNumbersMap {
			get { return _netToJavaNumbersMap; }
		}

		internal bool SameGroupsFlag {
			get { return _sameGroupsFlag; }
		}

		internal int GroupCount {
			get { return _groupCount; }
		}

		internal PatternGrouping () {
			this._groupCount = -1;
		}

		internal void SetGroups (int netGroupCount, int javaGroupCount) {
			this._groupCount = netGroupCount;

			_groupNameToNumberMap = new Hashtable (_groupCount + 1);
			_javaToNetGroupNumbersMap = new int [javaGroupCount + 1];
			_groupNumberToNameMap = new string [_groupCount + 1];
			_netToJavaNumbersMap = new int [_groupCount + 1];

			for (int i = 0; i <= _groupCount; ++i) {
				_groupNameToNumberMap.Add (i.ToString (), i);
				_javaToNetGroupNumbersMap [i] = i;
				_groupNumberToNameMap [i] = i.ToString ();
				_netToJavaNumbersMap [i] = i;
			}
			for (int i = _groupCount + 1; i <= javaGroupCount; ++i) {
				_javaToNetGroupNumbersMap [i] = -1;
			}
		}

		internal void SetGroups (string [] namedGroups,
								int [] javaGroupNumberToNetGroupNumber,
								int noNamedGroupNumber,
								int capturedGroupsCount,
								int sameGroupsCounter,
								RegexOptions options) {
			_groupNumberToNameMap = new string [capturedGroupsCount + 1];
			_groupNameToNumberMap = new Hashtable (capturedGroupsCount + 1);
			_netToJavaNumbersMap = new int [capturedGroupsCount + 1];

			_javaToNetGroupNumbersMap = javaGroupNumberToNetGroupNumber;

			if ((options & RegexOptions.ExplicitCapture) == RegexOptions.ExplicitCapture) {
				FillExplicitGroupMaps (namedGroups,
				capturedGroupsCount,
				sameGroupsCounter,
				options);
				return;
			}
			else {
				FillImplicitGroupMaps (namedGroups,
					noNamedGroupNumber + 1,
					capturedGroupsCount,
					sameGroupsCounter,
					options);
			}

		}

		private void FillImplicitGroupMaps (string [] namedGroups,
			int namedGroupNumber,
			int capturedGroupsCount,
			int sameGroupsCounter,
			RegexOptions options) {
			int addedNamedGroupsCount = 0;

			for (int i = 0; i < capturedGroupsCount + 1; ++i) {
				int netGroupNumber = _javaToNetGroupNumbersMap [i];
				if (netGroupNumber == -1) {
					if (_groupNameToNumberMap.Contains (namedGroups [addedNamedGroupsCount])) {
						_javaToNetGroupNumbersMap [i] = (int) _groupNameToNumberMap [namedGroups [addedNamedGroupsCount]];
						++sameGroupsCounter;
					}
					else {
						_javaToNetGroupNumbersMap [i] = namedGroupNumber;
						_groupNumberToNameMap [namedGroupNumber] = namedGroups [addedNamedGroupsCount];
						++namedGroupNumber;
					}

					netGroupNumber = _javaToNetGroupNumbersMap [i];
					++addedNamedGroupsCount;
				}
				else {
					_groupNumberToNameMap [netGroupNumber] =
						netGroupNumber.ToString ();
				}
				_groupNameToNumberMap [_groupNumberToNameMap [netGroupNumber]] = netGroupNumber;
				_netToJavaNumbersMap [_javaToNetGroupNumbersMap [i]] = i;
			}
			_groupCount = capturedGroupsCount - sameGroupsCounter;
			_sameGroupsFlag = (sameGroupsCounter != 0);

		}

		private void FillExplicitGroupMaps (string [] namedGroups,
			int capturedGroupsCount,
			int sameGroupsCounter,
			RegexOptions options) {
			int addedNamedGroupsCount = 0;
			int namedGroupNumber = 1;
			int nonCapturedGroupsNumber = 0;

			for (int i = 1; i < capturedGroupsCount + 1; ++i) {
				int netGroupNumber = _javaToNetGroupNumbersMap [i];
				if (netGroupNumber >= 0) {
					_javaToNetGroupNumbersMap [i] = -1;
					++nonCapturedGroupsNumber;
					continue;
				}

				if (_groupNameToNumberMap.Contains (namedGroups [addedNamedGroupsCount])) {
					_javaToNetGroupNumbersMap [i] = (int) _groupNameToNumberMap [namedGroups [addedNamedGroupsCount]];
					++sameGroupsCounter;
				}
				else {
					_javaToNetGroupNumbersMap [i] = namedGroupNumber;
					_groupNumberToNameMap [namedGroupNumber] = namedGroups [addedNamedGroupsCount];
					++namedGroupNumber;
				}

				netGroupNumber = _javaToNetGroupNumbersMap [i];
				++addedNamedGroupsCount;

				_groupNameToNumberMap [_groupNumberToNameMap [netGroupNumber]] = netGroupNumber;
				_netToJavaNumbersMap [_javaToNetGroupNumbersMap [i]] = i;
			}
			_groupCount = capturedGroupsCount - sameGroupsCounter - nonCapturedGroupsNumber;
			_sameGroupsFlag = (sameGroupsCounter != 0);
		}
	}
}