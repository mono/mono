//
// assembly:	System
// namespace:	System.Text.RegularExpressions
// file:	Group.cs
//
// author:	Dan Lewis (dlewis@gmx.co.uk)
// 		(c) 2002

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

namespace System.Text.RegularExpressions {

	[Serializable]
	public class Group : Capture {
		public static Group Synchronized (Group inner) {
			return inner;	// is this enough?
		}

		public CaptureCollection Captures {
			get { return captures; }
		}

		public bool Success {
			get { return success; }
		}

		// internal

		internal Group (string text, int[] caps) : base (text) {
			this.captures = new CaptureCollection ();

			if (caps == null || caps.Length == 0) {
				this.success = false;
				return;
			}

			this.success = true;
			this.index = caps[0];
			this.length = caps[1];
			captures.Add (this);
			for (int i = 2; i < caps.Length; i += 2)
				captures.Add (new Capture (text, caps[i], caps[i + 1]));
			captures.Reverse ();
		}
		
		internal Group (): base ("")
		{
			captures = new CaptureCollection ();
		}

		private bool success;
		private CaptureCollection captures;
	}
}
