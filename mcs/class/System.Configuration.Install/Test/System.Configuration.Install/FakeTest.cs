//
// MonoTests.System.Configuration.Install.FakeTest
//
// Authors:
// 	Martin Willemoes Hansen <mwh@sysrq.dk>
//
// (C) 2003 Martin Willemoes Hansen
// 
// This test should be sent to /dev/null when real
// tests arrive.

using System;
using NUnit.Framework;

namespace MonoTests.System.Configuration.Install {

	[TestFixture]
        public class FakeTest {

		[SetUp]
		public void GetReady () {}

		[TearDown]
		public void Clear () {}

		[Test]
		public void Fake () {}
        }
}

