// 
// System.Web.Services.Protocols.MatchAttribute.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
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

namespace System.Web.Services.Protocols {
	[AttributeUsage (AttributeTargets.All, Inherited = true)]
	public sealed class MatchAttribute : Attribute {

		#region Fields

		int capture;
		int group;
		bool ignoreCase;
		int maxRepeats;
		string pattern;

		#endregion

		#region Constructors

		public MatchAttribute (string pattern) 
		{
			ignoreCase = false;
			maxRepeats = -1;
			this.pattern = pattern;
			group = 1;
		}

		#endregion // Constructors

		#region Properties

		public int Capture {
			get { return capture; }
			set { capture = value; }
		}

		public int Group {
			get { return group; }
			set { group = value; }
		}

		public bool IgnoreCase {
			get { return ignoreCase; }
			set { ignoreCase = value; }
		}

		public int MaxRepeats {
			get { return maxRepeats; }
			set { maxRepeats = value; }
		}

		public string Pattern {
			get { return pattern; }
			set { pattern = value; }
		}

		#endregion // Properties
	}
}
