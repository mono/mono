//
// RegexRunnerCas.cs 
//	- CAS unit tests for System.Text.RegularExpressions.RegexRunner
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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


#if !MOBILE

using NUnit.Framework;

using System;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Text.RegularExpressions;

namespace MonoCasTests.System.Text.RegularExpressions {

	class TestRegexRunner : RegexRunner {

		public TestRegexRunner ()
		{
		}

		protected override bool FindFirstChar ()
		{
			return false;
		}

		protected override void Go ()
		{
		}

		protected override void InitTrackCount ()
		{
		}

		// easier to test from inside
		public void Test ()
		{
			// abstract (and protected) stuff
			Assert.IsFalse (this.FindFirstChar (), "FindFirstChar");
			Go ();
			InitTrackCount ();

			// protected stuff
			Assert.IsNull (runcrawl, "runcrawl");
			runcrawl = new int[3] { 0, 0, 0 };
			Assert.AreEqual (0, runcrawlpos, "runcrawlpos");
			runcrawlpos = 1;
			Assert.IsNull (runmatch, "runmatch");
			runmatch = Match.Empty;
			Assert.IsNull (runregex, "runregex");
			runregex = new Regex (String.Empty);
			Assert.IsNull (runstack, "runstack");
			runstack = new int[3] { 0, 0, 0 };
			Assert.AreEqual (0, runstackpos, "runstackpos");
			runstackpos = 1;
			Assert.IsNull (runtext, "runtext");
			runtext = "mono";
			Assert.AreEqual (0, runtextbeg, "runtextbeg");
			runtextbeg = 1;
			Assert.AreEqual (0, runtextend, "runtextend");
			runtextend = 1;
			Assert.AreEqual (0, runtextpos, "runtextpos");
			runtextpos = 1;
			Assert.AreEqual (0, runtextstart, "runtextstart");
			runtextstart = 1;
			Assert.IsNull (runtrack, "runtrack");
			runtrack = new int[3] { 0, 0, 0 };
			Assert.AreEqual (0, runtrackcount, "runtrackcount");
			runtrackcount = 1;
			Assert.AreEqual (0, runtrackpos, "runtrackpos");
			runtrackpos = 1;

			Capture (0, 0, 0);
			Assert.IsTrue (CharInSet ('a', "a", ""), "CharInSet");
			Crawl (1);
			Assert.AreEqual (4, Crawlpos (), "Crawlpos");
			DoubleCrawl ();
			DoubleStack ();
			DoubleTrack ();
			EnsureStorage ();
			Assert.IsFalse (IsBoundary (0, 0, 0), "IsBoundary");
			Assert.IsFalse (IsECMABoundary (0, 0, 0), "IsECMABoundary");
			Assert.IsTrue (IsMatched (0), "IsMatched");
			Assert.AreEqual (0, MatchIndex (0), "MatchIndex");
			Assert.AreEqual (0, MatchLength (0), "MatchLength");
			Assert.AreEqual (1, Popcrawl (), "Popcrawl");
			TransferCapture (0, 0, 0, 0);
			Uncapture ();
			Assert.IsNotNull (Scan (new Regex (String.Empty), "mono", 0, 0, 0, 0, true), "Scan");
#if NET_2_0
			Assert.IsTrue (CharInSet ('a', "a", String.Empty), "CharInSet");
#endif
		}
	}

	[TestFixture]
	[Category ("CAS")]
	public class RegexRunnerCas {

		[SetUp]
		public void SetUp ()
		{
			if (!SecurityManager.SecurityEnabled)
				Assert.Ignore ("SecurityManager.SecurityEnabled is OFF");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		[Category ("NotWorking")]
		public void Deny_Unrestricted ()
		{
			TestRegexRunner runner = new TestRegexRunner ();
			runner.Test ();
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void LinkDemand_Deny_Unrestricted ()
		{
			ConstructorInfo ci = typeof (TestRegexRunner).GetConstructor (new Type[0]);
			Assert.IsNotNull (ci, "default .ctor");
			Assert.IsNotNull (ci.Invoke (null), "invoke");
		}
	}
}

#endif
