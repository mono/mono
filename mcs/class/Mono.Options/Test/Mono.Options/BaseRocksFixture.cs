//
// BaseRocksTestFixture.cs
//
// Author:
//   Jb Evain (jbevain@novell.com)
//
// Copyright (c) 2007 Novell, Inc. (http://www.novell.com)
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
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using Cadenza;

namespace Cadenza.Tests {

	public abstract class BaseRocksFixture {

		public void AssertAreSame<T> (IEnumerable<T> expected, IEnumerable<T> data)
		{
			Assert.IsTrue (data.SequenceEqual (expected));
		}

		public static void AssertThrows<TException> (Action action)
		{
			try {
				action ();
			}
			catch (Exception e)
			{
				if (e.GetType () != typeof (TException))
					throw new InvalidOperationException (
							string.Format ("invalid exception type!  Expected {0}, got {1}.",
								typeof (TException).FullName, e.GetType().FullName),
							e);
			}
		}

		// Exists so that you can use a variable and shut up the compiler.
		public static void Ignore<T>(T value)
		{
		}
	}
}
