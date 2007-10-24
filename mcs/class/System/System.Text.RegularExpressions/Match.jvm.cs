//
// Match.jvm.cs
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

	public partial class Match : Group
	{
		#region Fields

		GroupCollection _monoGroups;
		readonly object _monoGroupsLock = new object ();

		#endregion Fields

		#region Ctors

		internal Match (Regex regex, IMachine machine, string text, int text_length, int n_groups,
		 int index, int length, int n_caps)
			: this (regex, machine, new GroupCollection (n_groups), text,
			text_length, index, length, n_caps) { }

		internal Match (Regex regex, IMachine machine,
			GroupCollection groups,
			string text, int text_length,
			int index, int length)
			: this (regex, machine, groups, text, text_length, index, length, 1) { }

		private Match (Regex regex, IMachine machine,
						GroupCollection groups,
						string text, int text_length,
						int index, int length, int n_caps)
			: base (text, index, length, n_caps) {
			this.regex = regex;
			this.machine = machine;
			this.text_length = text_length;

			this.groups = groups;
			groups.SetValue (this, 0);
		}

		#endregion Ctors

		#region Properties
		private GroupCollection MonoGroups {
			get {
				lock (_monoGroupsLock) {
					if (_monoGroups != null)
						return _monoGroups;

					Match monoMatch = regex.GetMonoMachine ().Scan (regex, text, index, index + length);
					_monoGroups = monoMatch.Groups;
				}

				return _monoGroups;
			}
		}

		#endregion Properties

		internal void FillMonoCaptures (Group group) {
			group.Captures = MonoGroups [group.GroupNumber].Captures;
		}
	}
}