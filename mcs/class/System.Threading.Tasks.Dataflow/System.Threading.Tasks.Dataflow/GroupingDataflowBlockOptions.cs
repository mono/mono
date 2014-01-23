// GroupingDataflowBlockOptions.cs
//
// Copyright (c) 2011 Jérémie "garuma" Laval
// Copyright (c) 2012 Petr Onderka
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

namespace System.Threading.Tasks.Dataflow
{
	public class GroupingDataflowBlockOptions : DataflowBlockOptions {
		static readonly GroupingDataflowBlockOptions DefaultOptions =
			new GroupingDataflowBlockOptions ();

		long maxNumberOfGroups;

		/// <summary>
		/// Cached default block options
		/// </summary>
		internal static new GroupingDataflowBlockOptions Default {
			get { return DefaultOptions; }
		}

		public GroupingDataflowBlockOptions ()
		{
			Greedy = true;
			MaxNumberOfGroups = -1;
		}

		public bool Greedy { get; set; }

		public long MaxNumberOfGroups {
			get { return maxNumberOfGroups; }
			set {
				if (value < -1)
					throw new ArgumentOutOfRangeException("value");
				
				maxNumberOfGroups = value;
			}
		}
	}
}