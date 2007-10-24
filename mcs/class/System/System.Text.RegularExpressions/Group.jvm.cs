//
// Group.jvm.cs
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

namespace System.Text.RegularExpressions
{
	public partial class Group : Capture
	{
		readonly Match _match;
		readonly object _capturesLock = new object ();
		readonly int _groupNumber;

		internal int GroupNumber {
			get { return _groupNumber; }
		}

		public CaptureCollection Captures {
			get {
				lock (_capturesLock) {
					if (captures != null)
						return captures;
					_match.FillMonoCaptures (this);
				}

				return captures;
			}

			internal set { captures = value; }
		}

		// internal
		internal Group (string text, int index, int length, Match match, int groupNumber)
			: base (text, index, length) {
			success = true;
			_match = match;
			_groupNumber = groupNumber;
		}

	}
}