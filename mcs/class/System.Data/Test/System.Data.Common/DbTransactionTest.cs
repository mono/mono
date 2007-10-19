//
// DbTransactionTest.cs - NUnit Test Cases for testing the DbTransaction class
//
// Author:
//	Gert Driesen (drieseng@users.sourceforge.net)
//
// Copyright (c) 2007 Gert Driesen
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

#if NET_2_0
using System;
using System.Data;
using System.Data.Common;

using NUnit.Framework;

namespace MonoTests.System.Data.Common
{
	[TestFixture]
	public class DbTransactionTest
	{
		[Test] // bug #325397
		public void DisposeTest ()
		{
			MockTransaction trans = new MockTransaction ();
			trans.Dispose ();

			Assert.IsFalse (trans.IsCommitted, "#1");
			Assert.IsFalse (trans.IsRolledback, "#2");
			Assert.IsTrue (trans.IsDisposed, "#3");
			Assert.IsTrue (trans.Disposing, "#4");
		}

		class MockTransaction : DbTransaction
		{
			protected override DbConnection DbConnection {
				get { return null; }
			}

			public override IsolationLevel IsolationLevel {
				get { return IsolationLevel.RepeatableRead; }
			}

			public bool IsCommitted {
				get { return _isCommitted; }
			}

			public bool IsRolledback {
				get { return _isRolledback; }
			}

			public bool IsDisposed {
				get { return _isDisposed; }
			}

			public bool Disposing {
				get { return _disposing; }
			}

			public override void Commit ()
			{
				_isCommitted = true;
			}

			public override void Rollback ()
			{
				_isRolledback = true;
			}

			protected override void Dispose (bool disposing)
			{
				_isDisposed = true;
				_disposing = disposing;
				base.Dispose (disposing);
			}

			private bool _isCommitted;
			private bool _isRolledback;
			private bool _isDisposed;
			private bool _disposing;
		}
	}
}
#endif
