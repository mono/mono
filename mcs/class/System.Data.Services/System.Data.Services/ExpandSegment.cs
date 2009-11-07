//
// ExpandSegment.cs
//
// Author:
//   Eric Maupin  <me@ermau.com>
//
// Copyright (c) 2009 Eric Maupin (http://www.ermau.com)
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

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace System.Data.Services {
	public class ExpandSegment {
		public ExpandSegment (string name, Expression filter)
		{
			if (name == null)
				throw new ArgumentNullException ("name");

			this.Name = name;
			this.Filter = filter;
		}

		public string Name {
			get;
			private set;
		}

		public Expression Filter {
			get;
			private set;
		}

		public bool HasFilter {
			get { return (this.Filter != null); }
		}

		public int MaxResultsExpected {
			get { return this.max_results_expected; }
		}

		private int max_results_expected = Int32.MaxValue;

		public static bool PathHasFilter (IEnumerable<ExpandSegment> segments)
		{
			if (segments == null)
				throw new ArgumentNullException ("segments");

			return segments.Any (s => s.HasFilter);
		}
	}
}