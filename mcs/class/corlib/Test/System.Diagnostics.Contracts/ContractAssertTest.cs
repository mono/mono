#if NET_4_0

#define CONTRACTS_FULL
#define DEBUG

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Diagnostics.Contracts;
using System.Diagnostics;
using MonoTests.System.Diagnostics.Contracts.Helpers;

namespace MonoTests.System.Diagnostics.Contracts {

	[TestFixture]
	public class ContractAssertTest : TestContractBase {

		/// <summary>
		/// Ensures that Assert(true) allows execution to continue.
		/// </summary>
		[Test, RunAgainstReference]
		public void TestAssertTrue ()
		{
			Contract.Assert (true);
		}

		/// <summary>
		/// Contract.Assert(false) will cause an assert to be triggered with the correct message.
		/// </summary>
		[Test]
		public void TestAssertNoEventHandler ()
		{
			try {
				Contract.Assert (false);
				Assert.Fail ("TestAssertNoEventHandler() exception not thrown #1");
			} catch (Exception ex) {
				Assert.IsInstanceOfType (typeof (NotImplementedException), ex, "TestAssertNoEventHandler() wrong exception type #1");
			}

			try {
				Contract.Assert (false, "Message");
				Assert.Fail ("TestAssertNoEventHandler() exception not thrown #2");
			} catch (Exception ex) {
				Assert.IsInstanceOfType (typeof (NotImplementedException), ex, "TestAssertNoEventHandler() wrong exception type #2");
			}
		}

		/// <summary>
		/// Contract.Assert(true) will not call the ContractFailed event handler.
		/// Contract.Assert(false) will call the ContractFailed event handler.
		/// Because nothing is done in the event handler, an assert should be triggered.
		/// </summary>
		[Test]
		public void TestAssertEventHandlerNoAction ()
		{
			bool visitedEventHandler = false;
			Contract.ContractFailed += (sender, e) => {
				visitedEventHandler = true;
			};

			Contract.Assert (true);

			Assert.IsFalse (visitedEventHandler, "TestAssertEventHandlerNoAction() handler visited");

			try {
				Contract.Assert (false);
				Assert.Fail ("TestAssertEventHandlerNoAction() exception not thrown");
			} catch (Exception ex) {
				Assert.IsInstanceOfType (typeof (NotImplementedException), ex, "TestAssertEventHandlerNoAction() wrong exception type");
			}

			Assert.IsTrue (visitedEventHandler, "TestAssertEventHandlerNoAction() handler not visited");
		}

		/// <summary>
		/// Event handler calls SetHandled(), so no assert should be triggered.
		/// </summary>
		[Test, RunAgainstReference]
		public void TestAssertEventHandlerSetHandled ()
		{
			Contract.ContractFailed += (sender, e) => {
				e.SetHandled ();
			};

			Contract.Assert (false);
		}

		/// <summary>
		/// Event handler calls SetUnwind(), so exception of type ContractException should be thrown.
		/// </summary>
		[Test, RunAgainstReference]
		public void TestAssertEventHandlerSetUnwind ()
		{
			Contract.ContractFailed += (sender, e) => {
				e.SetUnwind ();
			};

			try {
				Contract.Assert (false);
			} catch (Exception ex) {
				Assert.IsInstanceOfType (base.ContractExceptionType, ex, "TestAssertEventHandlerSetUnwind() wrong exception type");
				Assert.IsNull (ex.InnerException, "TestAssertEventHandlerSetUnwind() inner exception not null");
			}
		}

		/// <summary>
		/// Event handler calls SetHandled() and SetUnwind(), so exception of type ContractException should be thrown,
		/// as SetUnwind overrides SetHandled.
		/// </summary>
		[Test, RunAgainstReference]
		public void TestAssertEventHandlerSetUnwindHandled ()
		{
			Contract.ContractFailed += (sender, e) => {
				e.SetHandled ();
				e.SetUnwind ();
			};

			try {
				Contract.Assert (false);
			} catch (Exception ex) {
				Assert.IsInstanceOfType (base.ContractExceptionType, ex, "TestAssertEventHandlerSetUnwindHandled() wrong exception type");
				Assert.IsNull (ex.InnerException, "TestAssertEventHandlerSetUnwindHandled() inner exception not null");
			}
		}

		/// <summary>
		/// Event handler throws exception.
		/// ContractException is thrown by Contract.Assert(), with InnerException set to the thrown exception.
		/// </summary>
		[Test, RunAgainstReference]
		public void TestAssertEventHandlerThrows ()
		{
			Contract.ContractFailed += (sender, e) => {
				throw new ArgumentNullException ();
			};

			try {
				Contract.Assert (false);
			} catch (Exception ex) {
				Assert.IsInstanceOfType (base.ContractExceptionType, ex, "TestAssertEventHandlerSetUnwindHandled() wrong exception type");
				Assert.IsInstanceOfType (typeof (ArgumentNullException), ex.InnerException, "TestAssertEventHandlerSetUnwindHandled() wrong inner exception type");
			}
		}

		/// <summary>
		/// Multiple event handlers are registered. Check that both are called, and that the SetHandled()
		/// call in one of them is handled correctly.
		/// </summary>
		[Test, RunAgainstReference]
		public void TestAssertMultipleHandlers ()
		{
			bool visited1 = false, visited2 = false;

			Contract.ContractFailed += (sender, e) => {
				visited1 = true;
				Assert.IsFalse (e.Handled, "TestAssertMultipleHandlers() Handled incorrect #1");
				e.SetHandled ();
			};
			Contract.ContractFailed += (sender, e) => {
				visited2 = true;
				Assert.IsTrue (e.Handled, "TestAssertMultipleHandlers() Handled incorrect #2");
			};

			Contract.Assert (false);

			Assert.IsTrue (visited1, "TestAssertMultipleHandlers() visited1 incorrect");
			Assert.IsTrue (visited2, "TestAssertMultipleHandlers() visited2 incorrect");
		}

	}
}

#endif
