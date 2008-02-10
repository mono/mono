//
// assembly:	System
// namespace:	System.Text.RegularExpressions
// file:	Group.cs
//
// author:	Dan Lewis (dlewis@gmx.co.uk)
// 		(c) 2002
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

namespace System.Text.RegularExpressions {

	[Serializable]
	public partial class Group : Capture {

		[MonoTODO ("not thread-safe")]
		public static Group Synchronized (Group inner)
		{
			if (inner == null)
				throw new ArgumentNullException ("inner");
			return inner;
		}

		internal static Group Fail = new Group ();
#if !TARGET_JVM
		public CaptureCollection Captures {
			get { return captures; }
		}
#endif
		public bool Success {
			get { return success; }
		}

		// internal
		internal Group (string text, int index, int length, int n_caps) : base (text, index, length)
		{
			success = true;
			captures = new CaptureCollection (n_caps);
			captures.SetValue (this, n_caps - 1);
		}

		internal Group (string text, int index, int length) : base (text, index, length)
		{
			success = true;
		}
		
		internal Group () : base ("")
		{
			success = false;
			captures = new CaptureCollection (0);
		}

		private bool success;
		private CaptureCollection captures;
	}
}
