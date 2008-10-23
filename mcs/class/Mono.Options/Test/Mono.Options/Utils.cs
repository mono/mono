//
// Utils.cs
//
// Authors:
//  Jonathan Pryor <jpryor@novell.com>
//
// Copyright (C) 2008 Novell (http://www.novell.com)
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

#if NDESK_OPTIONS
namespace Tests.NDesk.Options
#else
namespace Tests.Mono.Options
#endif
{
	static class Utils {
		public static void AssertException<T> (Type exception, string message, T a, Action<T> action)
		{
			Type actualType = null;
			string stack = null;
			string actualMessage = null;
			try {
				action (a);
			}
			catch (Exception e) {
				actualType    = e.GetType ();
				actualMessage = e.Message;
				if (!object.Equals (actualType, exception))
					stack = e.ToString ();
			}
			if (!object.Equals (actualType, exception)) {
				throw new InvalidOperationException (
					string.Format ("Assertion failed: Expected Exception Type {0}, got {1}.\n" +
						"Actual Exception: {2}", exception, actualType, stack));
			}
			if (!object.Equals (actualMessage, message))
				throw new InvalidOperationException (
					string.Format ("Assertion failed:\n\tExpected: {0}\n\t  Actual: {1}",
						message, actualMessage));
		}

	}
}

