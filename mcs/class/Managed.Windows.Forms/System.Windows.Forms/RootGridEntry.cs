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
// Copyright (c) 2006 Novell, Inc.
//
// Authors:
//	Chris Toshok (toshok@novell.com)
//

using System;
using System.Drawing;

namespace System.Windows.Forms.PropertyGridInternal
{
	/// <summary>
	/// Summary description for PropertyGridRootGridItem
	/// </summary>
	[MonoInternalNote ("needs to implement IRootGridEntry")]
	internal class RootGridEntry : GridEntry /*, IRootGridEntry */
	{
		object[] val;

		public RootGridEntry (PropertyGrid owner, object[] obj)
			: base (owner, null)
		{
			if (obj == null || obj.Length == 0)
				throw new ArgumentNullException ("obj");
			val = obj;
		}

		public override bool Expandable {
			get { return true; }
		}

		public override GridItemType GridItemType {
			get { return GridItemType.Root; }
		}

		public override string Label {
			get { return val.Length > 1 ? val.GetType().ToString() : val[0].GetType().ToString(); }
		}

		public override object Value {
			get { return val.Length > 1 ? val : val[0]; }
		}

		public override object[] Values {
			get { return val; }
		}

		public override bool Select ()
		{
			return false; /* root entries aren't selectable */
		}

		public override bool IsReadOnly {
			get { return true; }
		}

		public override bool IsEditable {
			get { return false; }
		}

		public override bool IsResetable {
			get { return false; }
		}

		public override bool IsMerged {
			get { return val.Length > 1; }
		}
	}
}
