//
// AssertExtensions.cs
//
// Author:
//	Marek Habersack <mhabersack@novell.com>
//
// Copyright (C) 2009 Novell Inc. http://novell.com
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
using System;
using System.Web;
using System.Web.Routing;
using NUnit.Framework;

namespace MonoTests.System.Web.Routing
{
	public delegate void AssertThrowsDelegate ();
	
	public static class AssertExtensions
	{
		public static void Throws <EX> (AssertThrowsDelegate code)
		{
			Throws (typeof (EX), code);
		}

		public static void Throws <EX> (AssertThrowsDelegate code, string message)
		{
			Throws (typeof (EX), code, message);
		}

		public static void Throws (Type exceptionType, AssertThrowsDelegate code)
		{
			Throws (exceptionType, code, String.Empty);
		}

		public static void Throws (Type exceptionType, AssertThrowsDelegate code, string message)
		{
			if (code == null)
				throw new ArgumentNullException ("code");
			if (exceptionType == null)
				throw new ArgumentNullException ("exceptionType");
			
			try {
				code ();
			} catch (Exception ex) {
				if (ex.GetType () != exceptionType)
					Assert.Fail (message + " (got exception of type '" + ex.GetType () + "')");
				
				// good!
				return;
			}

			Assert.Fail (message);
		}
	}
}
