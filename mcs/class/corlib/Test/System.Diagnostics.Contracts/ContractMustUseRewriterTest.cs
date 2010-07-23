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
	public class ContractMustUseRewriterTest : TestContractBase {

		private void CheckMustUseRewriter (string expectedMsg, params Action[] fnTests)
		{
			foreach (var fnTest in fnTests) {
				try {
					fnTest ();
					Assert.Fail ("CheckMustUseRewriter() exception not thrown");
				} catch (Exception ex) {
					Assert.IsInstanceOfType (typeof (NotImplementedException), ex, "CheckMustUseRewriter() wrong exception thrown");
				}
			}

			bool handlerVisited = false;
			Contract.ContractFailed += (sender, e) => {
				handlerVisited = true;
			};

			foreach (var fnTest in fnTests) {
				try {
					fnTest ();
					Assert.Fail ("CheckMustUseRewriter() exception not thrown");
				} catch (Exception ex) {
					Assert.IsInstanceOfType (typeof (NotImplementedException), ex, "CheckMustUseRewriter() wrong exception thrown");
				}
			}

			Assert.IsFalse (handlerVisited, "CheckMustUseRewriter() handled visited");
		}

		/// <summary>
		/// Contract.Requires() ALWAYS triggers an assert, regardless of any other factors.
		/// </summary>
		[Test]
		[Ignore ("This causes NUnit crash on .NET 4.0")]
		public void TestRequires ()
		{
			CheckMustUseRewriter ("Description: Must use the rewriter when using Contract.Requires",
				() => Contract.Requires (true),
				() => Contract.Requires (false),
				() => Contract.Requires (true, "Message"),
				() => Contract.Requires (false, "Message")
			);
		}

		/// <summary>
		/// Contract.Requires() ALWAYS triggers an assert, regardless of any other factors.
		/// </summary>
		[Test]
		[Ignore ("This causes NUnit crash on .NET 4.0")]
		public void TestRequiresTException ()
		{
			CheckMustUseRewriter ("Description: Must use the rewriter when using Contract.Requires<TException>",
				() => Contract.Requires<Exception> (true),
				() => Contract.Requires<Exception> (false),
				() => Contract.Requires<Exception> (true, "Message"),
				() => Contract.Requires<Exception> (false, "Message")
			);
		}

		/// <summary>
		/// Contract.Ensures() ALWAYS triggers an assert, regardless of any other factors.
		/// </summary>
		[Test]
		[Ignore ("This causes NUnit crash on .NET 4.0")]
		public void TestEnsures ()
		{
			CheckMustUseRewriter ("Description: Must use the rewriter when using Contract.Ensures",
				() => Contract.Ensures (true),
				() => Contract.Ensures (false),
				() => Contract.Ensures (true, "Message"),
				() => Contract.Ensures (false, "Message")
			);
		}

		/// <summary>
		/// Contract.Ensures() ALWAYS triggers an assert, regardless of any other factors.
		/// </summary>
		[Test]
		[Ignore ("This causes NUnit crash on .NET 4.0")]
		public void TestEnsuresOnThrow ()
		{
			CheckMustUseRewriter ("Description: Must use the rewriter when using Contract.EnsuresOnThrow",
				() => Contract.EnsuresOnThrow<Exception> (true),
				() => Contract.EnsuresOnThrow<Exception> (false),
				() => Contract.EnsuresOnThrow<Exception> (true, "Message"),
				() => Contract.EnsuresOnThrow<Exception> (false, "Message")
			);
		}

		/// <summary>
		/// Contract.Ensures() ALWAYS triggers an assert, regardless of any other factors.
		/// </summary>
		[Test]
		[Ignore ("This causes NUnit crash on .NET 4.0")]
		public void TestInvariant ()
		{
			CheckMustUseRewriter ("Description: Must use the rewriter when using Contract.Invariant",
				() => Contract.Invariant (true),
				() => Contract.Invariant (false),
				() => Contract.Invariant (true, "Message"),
				() => Contract.Invariant (false, "Message")
			);
		}

	}

}

#endif
