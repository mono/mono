//
// CairoDebug.cs
//
// Author:
//   Michael Hutchinson (mhutch@xamarin.com)
//
// Copyright (C) 2013 Xamarin Inc. (http://www.xamarin.com)
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

namespace Cairo {

	static class CairoDebug
	{
		static System.Collections.Generic.Dictionary<IntPtr,string> traces;

		public static readonly bool Enabled;

		static CairoDebug ()
		{
			var dbg = Environment.GetEnvironmentVariable ("MONO_CAIRO_DEBUG_DISPOSE");
			if (dbg == null)
				return;
			Enabled = true;
			traces = new System.Collections.Generic.Dictionary<IntPtr,string> ();
		}

		public static void OnAllocated (IntPtr obj)
		{
			if (!Enabled)
				throw new InvalidOperationException ();

			traces[obj] = Environment.StackTrace;
		}

		public static void OnDisposed<T> (IntPtr obj, bool disposing)
		{
			if (disposing && !Enabled)
				throw new InvalidOperationException ();

			if (Environment.HasShutdownStarted)
				return;

			if (!disposing) {
				Console.Error.WriteLine ("{0} is leaking, programmer is missing a call to Dispose", typeof(T).FullName);
				if (Enabled) {
					string val;
					if (traces.TryGetValue (obj, out val)) {
						Console.Error.WriteLine ("Allocated from:");
						Console.Error.WriteLine (val);
					}
				} else {
					Console.Error.WriteLine ("Set MONO_CAIRO_DEBUG_DISPOSE to track allocation traces");
				}
			}

			if (Enabled)
				traces.Remove (obj);
		}
	}

}
