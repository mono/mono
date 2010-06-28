#if NET_4_0

#define CONTRACTS_FULL
#define DEBUG

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using MonoTests.System.Diagnostics.Contracts.Helpers;
using System.Diagnostics.Contracts;

namespace MonoTests.System.Diagnostics.Contracts {

	[TestFixture]
	public class ContractCollectionMethodsTest {

		/// <summary>
		/// Contract.Exists() determines that at least one element in the collection satisfies the predicate.
		/// </summary>
		[Test, RunAgainstReference]
		public void TestExistsInt ()
		{
			try {
				Contract.Exists (0, 10, null);
				Assert.Fail ("TestExistsInt() no exception #1");
			} catch (Exception ex) {
				Assert.IsInstanceOfType (typeof (ArgumentNullException), ex, "TestExistsInt() wrong exception #1");
			}

			try {
				Contract.Exists (10, 0, i => false);
				Assert.Fail ("TestExistsInt() no exception #2");
			} catch (Exception ex) {
				Assert.IsInstanceOfType (typeof (ArgumentException), ex, "TestExistsInt() wrong exception #2");
			}

			Assert.IsTrue (Contract.Exists (0, 10, i => i <= 0), "TestExistsInt() #1");
			Assert.IsTrue (Contract.Exists (0, 10, i => i >= 9), "TestExistsInt() #2");
			Assert.IsFalse (Contract.Exists (0, 10, i => i < 0), "TestExistsInt() #3");
			Assert.IsFalse (Contract.Exists (0, 10, i => i > 9), "TestExistsInt() #4");
		}

		/// <summary>
		/// Contract.Exists() determines that at least one element in the collection satisfies the predicate.
		/// </summary>
		[Test, RunAgainstReference]
		public void TestExistsEnumeration ()
		{
			try {
				Contract.Exists (Enumerable.Range (0, 10), null);
				Assert.Fail ("TestExistsEnumeration() no exception #1");
			} catch (Exception ex) {
				Assert.IsInstanceOfType (typeof (ArgumentNullException), ex, "TestExistsEnumeration() wrong exception #1");
			}

			try {
				Contract.Exists<int> (null, x => false);
				Assert.Fail ("TestExistsEnumeration() no exception #2");
			} catch (Exception ex) {
				Assert.IsInstanceOfType (typeof (ArgumentNullException), ex, "TestExistsEnumeration() wrong exception #2");
			}

			var en = Enumerable.Range (0, 10);
			Assert.IsTrue (Contract.Exists (en, i => i <= 0), "TestExistsEnumeration() #1");
			Assert.IsTrue (Contract.Exists (en, i => i >= 9), "TestExistsEnumeration() #2");
			Assert.IsFalse (Contract.Exists (en, i => i < 0), "TestExistsEnumeration() #3");
			Assert.IsFalse (Contract.Exists (en, i => i > 9), "TestExistsEnumeration() #4");
		}

		/// <summary>
		/// Contract.ForAll() determines if all elements in the collection satisfy the predicate.
		/// </summary>
		[Test, RunAgainstReference]
		public void TestForAllInt ()
		{
			try {
				Contract.ForAll (0, 10, null);
				Assert.Fail ("TestForAllInt() no exception #1");
			} catch (Exception ex) {
				Assert.IsInstanceOfType (typeof (ArgumentNullException), ex, "TestForAllInt() wrong exception #1");
			}

			try {
				Contract.ForAll (10, 0, i => false);
				Assert.Fail ("TestForAllInt() no exception #2");
			} catch (Exception ex) {
				Assert.IsInstanceOfType (typeof (ArgumentException), ex, "TestForAllInt() wrong exception #2");
			}

			Assert.IsTrue (Contract.ForAll (0, 10, i => i <= 9), "TestForAllInt() #1");
			Assert.IsTrue (Contract.ForAll (0, 10, i => i >= 0), "TestForAllInt() #2");
			Assert.IsFalse (Contract.ForAll (0, 10, i => i < 9), "TestForAllInt() #3");
			Assert.IsFalse (Contract.ForAll (0, 10, i => i > 0), "TestForAllInt() #4");
		}

		/// <summary>
		/// Contract.ForAll() determines if all elements in the collection satisfy the predicate.
		/// </summary>
		[Test, RunAgainstReference]
		public void TestForAllEnumeration ()
		{
			try {
				Contract.ForAll (Enumerable.Range (0, 10), null);
				Assert.Fail ("TestForAllEnumeration() no exception #1");
			} catch (Exception ex) {
				Assert.IsInstanceOfType (typeof (ArgumentNullException), ex, "TestForAllEnumeration() wrong exception #1");
			}

			try {
				Contract.ForAll<int> (null, x => false);
				Assert.Fail ("TestForAllEnumeration() no exception #2");
			} catch (Exception ex) {
				Assert.IsInstanceOfType (typeof (ArgumentNullException), ex, "TestForAllEnumeration() wrong exception #2");
			}

			var en = Enumerable.Range (0, 10);
			Assert.IsTrue (Contract.ForAll (en, i => i <= 9), "TestForAllEnumeration() #1");
			Assert.IsTrue (Contract.ForAll (en, i => i >= 0), "TestForAllEnumeration() #2");
			Assert.IsFalse (Contract.ForAll (en, i => i < 9), "TestForAllEnumeration() #3");
			Assert.IsFalse (Contract.ForAll (en, i => i > 0), "TestForAllEnumeration() #4");
		}

	}

}

#endif
