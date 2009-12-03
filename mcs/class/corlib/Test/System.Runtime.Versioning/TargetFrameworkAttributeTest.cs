//
// System.Runtime.Versioning.TargetFrameworkAttributeTest class
//
// Authors
//	Marek Habersack <mhabersack@novell.com>
//
// Copyright (C) 2009 Novell, Inc (http://novell.com)
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
using System.Runtime.Versioning;
using System.Text;

using NUnit.Framework;

#if NET_4_0
namespace MonoTests.System.Runtime.Versioning
{
	[TestFixture]
	public class TargetFrameworkAttributeTest
	{
		void Throws<TEx> (string message, Action code)
		{
			bool failed = false;
			Exception exception = null;
			try {
				code ();
				failed = true;
			} catch (Exception ex) {
				if (ex.GetType () != typeof (TEx)) {
					failed = true;
					exception = ex;
				}
			}

			if (failed) {
				if (exception != null)
					Assert.Fail ("{0}{1}Expected exception {2}, got {3}",
						     message, Environment.NewLine, typeof (TEx), exception.GetType ());
				else
					Assert.Fail ("{0}{1}Expected exception {2}",
						     message, Environment.NewLine, typeof (TEx));
			}
		}

		[Test]
		public void Constructor_String ()
		{
			TargetFrameworkAttribute tfa;

			Throws<ArgumentNullException> ("#A1-1", () => {
					tfa = new TargetFrameworkAttribute (null);
				});

			tfa = new TargetFrameworkAttribute (String.Empty);
			Assert.AreEqual (String.Empty, tfa.FrameworkName, "#A2-1");
			Assert.AreEqual (null, tfa.FrameworkDisplayName, "#A2-2");

			tfa = new TargetFrameworkAttribute ("identifier,Version=2");
			Assert.AreEqual ("identifier,Version=2", tfa.FrameworkName, "#A3-1");
			Assert.AreEqual (null, tfa.FrameworkDisplayName, "#A3-2");
		}

		[Test]
		public void FrameworkDisplayName ()
		{
			TargetFrameworkAttribute tfa;
			tfa = new TargetFrameworkAttribute (String.Empty);
			tfa.FrameworkDisplayName = null;
			Assert.AreEqual (null, tfa.FrameworkDisplayName, "#A1");

			tfa.FrameworkDisplayName = "test";
			Assert.AreEqual ("test", tfa.FrameworkDisplayName, "#A2");
		}
	}
}
#endif