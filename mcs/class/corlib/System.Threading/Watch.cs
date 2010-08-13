// Task.cs
//
// Copyright (c) 2008 Jérémie "Garuma" Laval
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
//
//

#if NET_4_0 || BOOTSTRAP_NET_4_0
using System;

namespace System.Threading
{
	internal class Watch
	{
		public Watch ()
		{
			startTicks = TicksNow ();
		}
		
		long startTicks;
		
		public static Watch StartNew ()
		{
			Watch temp = new Watch ();
			temp.Start ();
			return temp;
		}
		
		public void Start ()
		{
			startTicks = TicksNow ();
		}
		
		public void Stop ()
		{
			
		}
		
		public long ElapsedMilliseconds {
			get {
				return (TicksNow () - startTicks) / TimeSpan.TicksPerMillisecond;
			}
		}
		
		static long TicksNow ()
		{
			return DateTime.GetTimeMonotonic ();
		}
	}
}
#endif
