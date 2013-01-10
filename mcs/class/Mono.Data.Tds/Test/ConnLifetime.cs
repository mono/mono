//
// ConnLifetime.cs
//
// Authors:
//      James Lewis <james.lewis@7digital.com>
//      Andres G. Aragoneses <andres@7digital.com>
//
// Copyright (C) 2012 7digital Media Ltd (http://www.7digital.com)
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

using NUnit.Framework;

using Mono.Data.Tds.Protocol;


namespace Mono.Data.Tds.Tests
{

	[TestFixture]
	public class ConnLifetime
	{
		[Test]
		public void LifeTimeIsTakenInAccount ()
		{
			var SMALLEST_LIFETIME_TO_TEST = 1;
			var WAIT_TO_MAKE_LIFETIME_PASSED = 2;

			TdsConnectionPoolManager sqlConnectionPools = new FakeConnectionPoolManager ();
			TdsConnectionInfo info = new TdsConnectionInfo ("dummy", 0, 0, 0,
			                                               1 /*minpoolsize*/,
			                                               1 /*maxpoolsize*/,
			                                               SMALLEST_LIFETIME_TO_TEST/*lifetime*/);

			TdsConnectionPool pool = sqlConnectionPools.GetConnectionPool ("test",info);
			Mono.Data.Tds.Protocol.Tds tds, tds2 = null;

			tds = pool.GetConnection();

			System.Threading.Thread.Sleep (TimeSpan.FromSeconds (WAIT_TO_MAKE_LIFETIME_PASSED));
			pool.ReleaseConnection (tds);

			tds2 = pool.GetConnection ();


			Assert.IsFalse (object.ReferenceEquals (tds, tds2));
			pool.ReleaseConnection(tds2);
		}

		class FakeConnectionPoolManager : TdsConnectionPoolManager {

			internal FakeConnectionPoolManager () : base (Mono.Data.Tds.Protocol.TdsVersion.tds90)
			{
			}

			public override Mono.Data.Tds.Protocol.Tds CreateConnection (TdsConnectionInfo info)
			{
	 			return new FakeTds (info.LifeTime);
			}
		}

		class FakeTds : Mono.Data.Tds.Protocol.Tds {
			internal FakeTds(int lifetime) : base (null, 0, 0, 0, lifetime, Mono.Data.Tds.Protocol.TdsVersion.tds90){
			}

			public override bool Connect (TdsConnectionParameters connectionParameters)
			{
				throw new NotImplementedException ();
			}

			protected override void ProcessColumnInfo ()
			{
				throw new NotImplementedException ();
			}

			protected override void InitComm (int port, int timeout)
			{
				//do nothing, not relevant for the test
			}

			public override bool IsConnected {
				get { return true; }
				set { }
			}

			public override void Disconnect ()
			{
				// do nothing, not relevant for the test
			}
		}
	}
}

