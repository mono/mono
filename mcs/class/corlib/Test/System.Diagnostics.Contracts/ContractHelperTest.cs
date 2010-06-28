#if NET_4_0

#define CONTRACTS_FULL
#define DEBUG

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Diagnostics.Contracts.Internal;
using System.Diagnostics.Contracts;
using MonoTests.System.Diagnostics.Contracts.Helpers;
using NUnit.Framework.Constraints;

namespace MonoTests.System.Diagnostics.Contracts {

	[TestFixture]
	public class ContractHelperTest : TestContractBase {

		private void CheckAllMessages (ContractFailureKind kind, string messageStart, Action<string, Exception, string, ContractFailureKind, Func<string>> fnAssert)
		{

			foreach (Exception ex in new [] { null, new ArgumentNullException () }) {
				fnAssert (messageStart + ".", ex, null, kind, () => {
					return ContractHelper.RaiseContractFailedEvent (kind, null, null, ex);
				});

				fnAssert (messageStart + ".  Message", ex, null, kind, () => {
					return ContractHelper.RaiseContractFailedEvent (kind, "Message", null, ex);
				});

				fnAssert (messageStart + ": Condition", ex, "Condition", kind, () => {
					return ContractHelper.RaiseContractFailedEvent (kind, null, "Condition", ex);
				});

				fnAssert (messageStart + ": Condition  Message", ex, "Condition", kind, () => {
					return ContractHelper.RaiseContractFailedEvent (kind, "Message", "Condition", ex);
				});
			}

		}

		private void CheckAllKinds (Action<string, Exception, string, ContractFailureKind, Func<string>> fnAssert)
		{
			this.CheckAllMessages (ContractFailureKind.Assert, "Assertion failed", fnAssert);
			this.CheckAllMessages (ContractFailureKind.Assume, "Assumption failed", fnAssert);
			this.CheckAllMessages (ContractFailureKind.Invariant, "Invariant failed", fnAssert);
			this.CheckAllMessages (ContractFailureKind.Postcondition, "Postcondition failed", fnAssert);
			this.CheckAllMessages (ContractFailureKind.PostconditionOnException, "Postcondition failed after throwing an exception", fnAssert);
			this.CheckAllMessages (ContractFailureKind.Precondition, "Precondition failed", fnAssert);
		}

		private void CheckAllKinds (Action<string, Exception, Func<string>> fnAssert)
		{
			this.CheckAllKinds ((expected, ex, condition, kind, fnTest) => fnAssert (expected, ex, fnTest));
		}

		/// <summary>
		/// If no event handler is present, then the string returned describes the condition failure.
		/// </summary>
		[Test, RunAgainstReference]
		public void TestRaiseContractFailedEventNoHandler ()
		{
			this.CheckAllKinds ((expected, ex, fnTest) => {
				string msg = fnTest ();
				Assert.AreEqual (expected, msg, "TestRaiseContractFailedEventNoHandler() incorrect message");
			});
		}

		/// <summary>
		/// When SetHandled() is called, null is returned.
		/// The event args are also checked.
		/// </summary>
		[Test, RunAgainstReference]
		public void TestRaiseContractFailedEventHandled ()
		{
			string expectedMsg = null;
			Exception originalException = null;
			string expectedCondition = null;
			ContractFailureKind expectedKind = ContractFailureKind.Assert;
			Contract.ContractFailed += (sender, e) => {
				Assert.AreEqual (expectedMsg, e.Message, "TestRaiseContractFailedEventHandled() event message wrong");
				Assert.AreSame (originalException, e.OriginalException, "TestRaiseContractFailedEventHandled() event exception wrong");
				Assert.AreEqual (expectedCondition, e.Condition, "TestRaiseContractFailedEventHandled() event condition wrong");
				Assert.AreEqual (expectedKind, e.FailureKind, "TestRaiseContractFailedEventHandled() event failure kind wrong");
				e.SetHandled ();
			};

			this.CheckAllKinds ((expected, ex, condition, kind, fnTest) => {
				expectedMsg = expected;
				originalException = ex;
				expectedCondition = condition;
				expectedKind = kind;
				string msg = fnTest ();
				Assert.IsNull (msg, "TestRaiseContractFailedEventHandled() expected null message");
			});
		}

		/// <summary>
		/// When SetUnwind() is called, a ContractException is thrown. If an innerException is passed, then
		/// it is put in the InnerException of the ContractException. Otherwise, the InnerException is set to null.
		/// </summary>
		[Test, RunAgainstReference]
		public void TestRaiseContractFailedEventUnwind ()
		{
			Contract.ContractFailed += (sender, e) => {
				e.SetUnwind ();
			};

			this.CheckAllKinds ((expected, expectedEx, fnTest) => {
				try {
					fnTest ();
					Assert.Fail ("TestRaiseContractFailedEventUnwind() exception not thrown");
				} catch (Exception ex) {
					Assert.IsInstanceOfType (base.ContractExceptionType, ex, "TestRaiseContractFailedEventUnwind() wrong exception type");
					if (expectedEx == null) {
						Assert.IsNull (ex.InnerException, "TestRaiseContractFailedEventUnwind() inner exception should be null");
					} else {
						Assert.AreSame (expectedEx, ex.InnerException, "TestRaiseContractFailedEventUnwind() wrong inner exception type");
					}
				}
			});
		}

		/// <summary>
		/// When the ContractFailed event throws an exception, then it becomes the inner exception of the
		/// ContractException. Except if an exception is passed in to the call, then that exception is put
		/// in the InnerException.
		/// </summary>
		[Test, RunAgainstReference]
		public void TestRaiseContractFailedEventThrows ()
		{
			Contract.ContractFailed += (sender, e) => {
				throw new InvalidOperationException ();
			};

			this.CheckAllKinds ((expected, expectedEx, fnTest) => {
				try {
					fnTest ();
					Assert.Fail ("TestRaiseContractFailedEventThrows() exception not thrown");
				} catch (Exception ex) {
					Assert.IsInstanceOfType (base.ContractExceptionType, ex, "TestRaiseContractFailedEventThrows() wrong exception type");
					Type expectedInnerExceptionType = expectedEx == null ? typeof (InvalidOperationException) : expectedEx.GetType ();
					Assert.IsInstanceOfType (expectedInnerExceptionType, ex.InnerException, "TestRaiseContractFailedEventThrows() wrong inner exception type");
				}
			});
		}

		/// <summary>
		/// Both event handlers should be called, constraint is not handled.
		/// </summary>
		[Test, RunAgainstReference]
		public void TestRaiseContractMultipleHandlers1 ()
		{
			bool visited1, visited2;
			Contract.ContractFailed += (sender, e) => {
				visited1 = true;
			};
			Contract.ContractFailed += (sender, e) => {
				visited2 = true;
			};

			this.CheckAllKinds ((expected, ex, fnTest) => {
				visited1 = visited2 = false;
				string msg = fnTest ();
				Assert.AreEqual (expected, msg, "TestRaiseContractMultipleHandlers1() msg not as expected");
				Assert.IsTrue (visited1, "TestRaiseContractMultipleHandlers1() handler 1 not visited");
				Assert.IsTrue (visited2, "TestRaiseContractMultipleHandlers1() handler 2 not visited");
			});
		}

		/// <summary>
		/// Both event handlers should be called. SetUnwind() takes precedent over SetHandled().
		/// </summary>
		[Test, RunAgainstReference]
		public void TestRaiseContractMultipleHandlers2 ()
		{
			bool visited1, visited2;
			Contract.ContractFailed += (sender, e) => {
				visited1 = true;
				e.SetHandled ();
			};
			Contract.ContractFailed += (sender, e) => {
				visited2 = true;
				e.SetUnwind ();
			};

			this.CheckAllKinds ((expected, expectedEx, fnTest) => {
				visited1 = visited2 = false;
				try {
					fnTest ();
					Assert.Fail ("TestRaiseContractMultipleHandlers2() exception not thrown");
				} catch (Exception ex) {
					Assert.IsInstanceOfType (base.ContractExceptionType, ex, "TestRaiseContractMultipleHandlers2() wrong exception type");
					if (expectedEx == null) {
						Assert.IsNull (ex.InnerException, "TestRaiseContractMultipleHandlers2() inner exception not null");
					} else {
						Assert.AreSame (expectedEx, ex.InnerException, "TestRaiseContractMultipleHandlers2() wrong inner exception");
					}
					Assert.IsTrue (visited1, "TestRaiseContractMultipleHandlers2() handler 1 not visited");
					Assert.IsTrue (visited2, "TestRaiseContractMultipleHandlers2() handler 2 not visited");
				}
			});
		}

		/// <summary>
		/// Both event handlers should be called. The exceptions are treated as calls to SetUnwind(), with
		/// the exception being put in the InnerException.
		/// </summary>
		[Test, RunAgainstReference]
		public void TestRaiseContractMultipleHandlers3 ()
		{
			bool visited1, visited2;
			Contract.ContractFailed += (sender, e) => {
				visited1 = true;
				throw new InvalidOperationException ();
			};
			Contract.ContractFailed += (sender, e) => {
				visited2 = true;
				throw new InvalidOperationException ();
			};

			this.CheckAllKinds ((expected, expectedEx, fnTest) => {
				visited1 = visited2 = false;
				try {
					fnTest ();
					Assert.Fail ("TestRaiseContractMultipleHandlers3() failed to throw exception");
				} catch (Exception ex) {
					Type expectedInnerExceptionType = expectedEx == null ? typeof (InvalidOperationException) : expectedEx.GetType ();
					Assert.IsInstanceOfType (base.ContractExceptionType, ex, "TestRaiseContractMultipleHandlers3() wrong exception type");
					Assert.IsInstanceOfType (expectedInnerExceptionType, ex.InnerException, "TestRaiseContractMultipleHandlers3() wrong inner exception type");
					Assert.IsTrue (visited1, "TestRaiseContractMultipleHandlers3() handler 1 not visited");
					Assert.IsTrue (visited2, "TestRaiseContractMultipleHandlers3() handler 2 not visited");
				}
			});
		}

		/// <summary>
		/// Contract.TriggerFailure() triggers the assert. Check that the assert is triggered, with the correct text.
		/// </summary>
		[Test]
		public void TestTriggerFailure ()
		{
			try {
				ContractHelper.TriggerFailure (ContractFailureKind.Assert, "Display", null, "Condition", null);
				Assert.Fail ("TestTriggerFailure() failed to throw exception");
			} catch (Exception ex) {
				Assert.IsInstanceOfType(typeof(NotImplementedException), ex, "TestTriggerFailure() wrong exception type");
				//Assert.AreEqual ("Expression: Condition" + Environment.NewLine + "Description: Display", ex.Message, "TestTriggerFailure() wrong message");
			}
		}

	}

}

#endif
