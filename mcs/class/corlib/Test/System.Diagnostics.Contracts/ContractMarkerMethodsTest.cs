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
	public class ContractMarkerMethodsTest : TestContractBase {

		/// <summary>
		/// Contract.EndContractBlock() has no effects.
		/// </summary>
		[Test, RunAgainstReference]
		public void TestEndContractBlock ()
		{
			Contract.EndContractBlock ();
		}

		/// <summary>
		/// Contract.OldValue() has no effect, and always returns the default value for the type.
		/// </summary>
		[Test, RunAgainstReference]
		public void TestOldValue ()
		{
			int i = Contract.OldValue<int> (8);
			object o = Contract.OldValue<object> (new object ());

			Assert.AreEqual (0, i, "TestOldValue() int value wrong");
			Assert.IsNull (o, "TestOldValue() object value wrong");
		}

		/// <summary>
		/// Contract.Result() has no effect, and always returns the default value for the type.
		/// </summary>
		[Test, RunAgainstReference]
		public void TestResult ()
		{
			int i = Contract.Result<int> ();
			object o = Contract.Result<object> ();

			Assert.AreEqual (0, i, "TestResult() int value wrong");
			Assert.IsNull (o, "TestResult() object value wrong");
		}

		/// <summary>
		/// Contract.ValueAtReturn() has no effect, and always returns the default value for the type.
		/// </summary>
		[Test, RunAgainstReference]
		public void TestValueAtReturn ()
		{
			int iOut, i;
			object oOut, o;

			i = Contract.ValueAtReturn (out iOut);
			o = Contract.ValueAtReturn (out oOut);

			Assert.AreEqual (0, i, "TestValueAtReturn() int return value wrong");
			Assert.IsNull (o, "TestValueAtReturn() object return value wrong");
			Assert.AreEqual (0, iOut, "TestValueAtReturn() int out value wrong");
			Assert.IsNull (oOut, "TestValueAtReturn() object out value wrong");
		}

	}

}

#endif
