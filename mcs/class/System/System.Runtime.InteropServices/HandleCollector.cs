//
// HandleCollector.cs
//
// Author: Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
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

namespace System.Runtime.InteropServices
{
	public sealed class HandleCollector
	{
		int count;
		readonly int init, max;
		readonly string name;
		DateTime previous_collection = DateTime.MinValue;

		public HandleCollector (string name, int initialThreshold)
			: this (name, initialThreshold, int.MaxValue)
		{
		}

		public HandleCollector (string name, int initialThreshold, int maximumThreshold)
		{
			if (initialThreshold < 0)
				throw new ArgumentOutOfRangeException ("initialThreshold", "initialThreshold must not be less than zero");
			if (maximumThreshold < 0)
				throw new ArgumentOutOfRangeException ("maximumThreshold", "maximumThreshold must not be less than zero");
			if (maximumThreshold < initialThreshold)
				throw new ArgumentException ("maximumThreshold must not be less than initialThreshold");

			this.name = name;
			init = initialThreshold;
			max = maximumThreshold;
		}

		public int Count {
			get { return count; }
		}

		public int InitialThreshold {
			get { return init; }
		}

		public int MaximumThreshold {
			get { return max; }
		}

		public string Name {
			get { return name; }
		}

		public void Add ()
		{
/* NET_3_5
			if (++count >= max)
				GC.Collect (GC.MaxGeneration, GCCollectionMode.Forced);
			else if (count >= init)
				GC.Collect (GC.MaxGeneration, GCCollectionMode.Optimized);
*/
			if (++count >= max)
				GC.Collect (GC.MaxGeneration);
			else if (count >= init && DateTime.Now - previous_collection > TimeSpan.FromSeconds (5)) { // some arbitrary criteria
				GC.Collect (GC.MaxGeneration);
				previous_collection = DateTime.Now;
			}
		}

		public void Remove ()
		{
			if (count == 0)
				throw new InvalidOperationException ("Cannot call Remove method when Count is 0");
			count--;
		}
	}
}

