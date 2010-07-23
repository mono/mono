#if NET_4_0

#define CONTRACTS_FULL
#define DEBUG

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Diagnostics.Contracts;
using MonoTests.System.Diagnostics.Contracts.Helpers;

namespace MonoTests.System.Diagnostics.Contracts {

	[TestFixture]
	public class ContractAssumeTest : TestContractBase {

		/// <summary>
		/// At runtime Contract.Assume() acts just like a Contract.Assert(), except the exact message in the assert
		/// or exception is slightly different.
		/// </summary>
		[Test]
		[Ignore ("This causes NUnit crash on .NET 4.0")]
		public void TestAssumeMessage ()
		{
			try {
				Contract.Assume (false);
				Assert.Fail ("TestAssumeMessage() exception not thrown #1");
			} catch (Exception ex) {
				Assert.IsInstanceOfType (typeof(NotImplementedException), ex, "TestAssumeMessage() wrong exception type #1");
			}

			try {
				Contract.Assume (false, "Message");
				Assert.Fail ("TestAssumeMessage() exception not thrown #1");
			} catch (Exception ex) {
				Assert.IsInstanceOfType (typeof(NotImplementedException), ex, "TestAssumeMessage() wrong exception type #1");
			}
		}

		// Identical to Contract.Assert, so no more testing required.

	}

}

#endif
