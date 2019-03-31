#region License, Terms and Author(s)
//
// BackLINQ
// Copyright (c) 2008 Atif Aziz. All rights reserved.
//
//  Author(s):
//
//      Dominik Hug, http://www.dominikhug.ch
//
// This library is free software; you can redistribute it and/or modify it
// under the terms of the New BSD License, a copy of which should have
// been delivered along with this distribution.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
// "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A
// PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
// OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
// LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
// DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
// THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using System.Threading;
using NUnit.Framework;
using System.Linq;
using NUnit.Framework.Constraints;
using System.Diagnostics;

namespace MonoTests.System.Linq
{
	[TestFixture]
	public sealed class EnumerableFixture {
		private CultureInfo initialCulture; // Thread culture saved during Setup to be undone in TearDown.
		private AssertionHandler tearDownAssertions;

		private delegate void AssertionHandler ();

		[SetUp]
		public void SetUp ()
		{
			tearDownAssertions = null;
			initialCulture = Thread.CurrentThread.CurrentCulture;
			Thread.CurrentThread.CurrentCulture = new CultureInfo ("de-CH");
		}

		[TearDown]
		public void TearDown ()
		{
			if (tearDownAssertions != null)
				tearDownAssertions ();
			Thread.CurrentThread.CurrentCulture = initialCulture;
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void Aggregate_EmptySource_ThrowsInvalidOperationException ()
		{
			var source = Read<object> ();
			source.Aggregate (delegate { throw new NotImplementedException (); });
		}

		[Test]
		public void Aggregate_AddFuncOnIntegers_ReturnsTotal ()
		{
			var source = Read (12, 34, 56, 78, 910, 1112, 1314, 1516, 1718, 1920);
			var result = source.Aggregate ((a, b) => a + b);
			Assert.That (result, Is.EqualTo (8670));
		}

		[Test]
		public void Aggregate_AddFuncOnIntegersWithSeed_ReturnsTotal ()
		{
			var source = Read (12, 34, 56, 78, 910, 1112, 1314, 1516, 1718, 1920);
			var result = source.Aggregate (100, (a, b) => a + b);
			Assert.That (result, Is.EqualTo (8770));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Aggregate_NullSource_ThrowsArgumentNullException ()
		{
			Enumerable.Aggregate<object> (null, delegate { throw new NotImplementedException (); });
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Aggregate_NullFunc_ThrowsArgumentNullException ()
		{
			Read<object> ().Aggregate (null);
		}

		[Test]
		public void Empty_YieldsEmptySource ()
		{
			var source = Enumerable.Empty<string> ();
			Assert.That (source, Is.Not.Null);
			var e = source.GetEnumerator ();
			Assert.That (e, Is.Not.Null);
			Assert.That (e.MoveNext (), Is.False);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Cast_NullSource_ThrowsArgumentNullException ()
		{
			Enumerable.Cast<object> (null);
		}

		[Test]
		[ExpectedException (typeof (InvalidCastException))]
		public void Cast_InvalidSource_ThrowsInvalidCastException ()
		{
			var source = Read (1000, "hello", new object ());
			var e = source.Cast<byte> ().GetEnumerator ();
			e.MoveNext (); // Do something so Cast will really run (deferred execution)
		}

		[Test]
		public void Cast_ObjectSourceContainingIntegers_YieldsDowncastedIntegers ()
		{
			var source = Read<object> (1, 10, 100);
			source.Cast<int> ().AssertEquals (1, 10, 100);
		}

		[Test]
		public void Cast_Integers_YieldsUpcastedObjects ()
		{
#if false
			// shouldn't this be inferred?
			Read (1, 10, 100).Cast<object> ().AssertEquals (1, 10, 100);
#else
			Read (1, 10, 100).Cast<object> ().AssertEquals<object> (1, 10, 100);
#endif
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void All_NullSource_ThrowsArgumentNullException ()
		{
			Enumerable.All (null, (int i) => { throw new NotImplementedException (); });
		}

		[Test]
		public void All_SomeSourceElementsNotSatifyingPredicate_ReturnsFalse ()
		{
			var source = Read (-100, -1, 0, 1, 100);
			Assert.That (source.All (i => i < 0), Is.False);
		}

		[Test]
		public void All_SourceElementsSatisfyingPredicate_ReturnsTrue ()
		{
			var source = Read (-100, -1, 0, 1, 100);
			Assert.That (source.All (i => i >= -100), Is.True);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Any_NullSource_ThrowsArgumentNullException ()
		{
			Enumerable.Any<object> (null);
		}

		[Test]
		public void Any_EmptySource_ReturnsFalse ()
		{
			var source = Read<object> ();
			Assert.That (source.Any (), Is.False);
		}

		[Test]
		public void Any_NonEmptySource_ReturnsTrue ()
		{
			var source = Read (new object ());
			Assert.That (source.Any (), Is.True);
		}

		[Test]
		public void Any_PredicateArg_EmptySource_ReturnsFalse ()
		{
			var source = Read (new int [0]);
			Assert.That (source.Any (delegate { throw new NotImplementedException (); }), Is.False);
		}

		[Test]
		public void Any_PredicateArg_NonEmptySource_ReturnsTrue ()
		{
			Assert.That (Read (1, 2, 3, 4, 5).Any (i => i > 2), Is.True);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void Average_EmptyLongSource_ThrowsInvalidOperationException ()
		{
			Read<long> ().Average ();
		}

		[Test]
		public void Average_Longs_ReturnsAverage ()
		{
			Assert.That (Read (25L, 75L).Average (), Is.EqualTo (50));
		}

		[Test]
		public void Average_SelectorArg_Longs_ReturnsAverage ()
		{
			Assert.That (Read (25L, 75L).Average (n => n * 2L), Is.EqualTo (100));
		}

		[Test]
		public void Average_EmptyNullableLongSource_Null ()
		{
			Assert.That (Read<long?> ().Average (), Is.Null);
		}

		[Test]
		public void Average_NullableLongsWithSomeNull_ReturnsAverage ()
		{
			Assert.That (Read<long?> (12L, null, 34L, null, 56L).Average (), Is.EqualTo (34.0));
		}

		[Test]
		public void Average_SelectorArg_NullableLongsWithSomeNull_ReturnsAverage ()
		{
			Assert.That (Read<long?> (12L, null, 34L, null, 56L).Average (n => n * 2L), Is.EqualTo (68.0));
		}

		[Test]
		public void Average_EmptyNullableIntegerSource_Null ()
		{
			Assert.That (Read<int?> ().Average (), Is.Null);
		}

		[Test]
		public void Average_NullableIntegersWithSomeNull_ReturnsAverage ()
		{
			Assert.That (Read<int?> (12, null, 34, null, 56).Average (), Is.EqualTo (34.0));
		}

		[Test]
		public void Average_SelectorArg_NullableIntegersWithSomeNull_ReturnsAverage ()
		{
			Assert.That (Read<int?> (12, null, 34, null, 56).Average (n => n * 2), Is.EqualTo (68.0));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void Average_EmptyDecimalSource_ThrowsInvalidOperationException ()
		{
			Read<decimal> ().Average ();
		}

		[Test]
		public void Average_Decimals_ReturnsAverage ()
		{
			var source = Read (-10000m, 2.0001m, 50m);
			Assert.That (source.Average (), Is.EqualTo (-3315.999966).Within (0.00001));
		}

		[Test]
		public void Average_SelectorArg_Decimals_ReturnsAverage ()
		{
			var source = Read (-10000m, 2.0001m, 50m);
			Assert.That (source.Average (n => n * 2m), Is.EqualTo (-6631.999933).Within (0.00001));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void Average_EmptySource_ThrowsInvalidOperationException ()
		{
			Read<int> ().Average ();
		}

		[Test]
		public void Average_EmptyNullableIntegerSource_ReturnsNull ()
		{
			Assert.That (Read<int?> ().Average (), Is.Null);
		}

		[Test]
		public void Average_SelectorArg_Integers_ReturnsAverage ()
		{
			Assert.That (Read (21, 22, 23, 24).Average (n => n * 2).Equals (45));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void Average_EmptyDoubleSource_ThrowsInvalidOperationException ()
		{
			Read<double> ().Average ();
		}

		[Test]
		public void Average_Doubles_ReturnsAverage ()
		{
			var source = Read (-3.45, 9.001, 10000.01);
			Assert.That (source.Average (), Is.EqualTo (3335.187).Within (0.01));
		}

		[Test]
		public void Average_SelectorArg_Doubles_ReturnsAverage ()
		{
			var source = Read (-3.45, 9.001, 10000.01);
			Assert.That (source.Average (n => n * 2.0), Is.EqualTo (6670.374).Within (0.01));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void Average_EmptyFloatSource_ThrowsInvalidOperationException ()
		{
			Read<float> ().Average ();
		}

		[Test]
		public void Average_Floats_ReturnsAverage ()
		{
			var source = Read (-3.45F, 9.001F, 10000.01F);
			Assert.That (source.Average (), Is.EqualTo (3335.187).Within (0.01));
		}

		[Test]
		public void Average_SelectorArg_Floats_ReturnsAverage ()
		{
			var source = Read (-3.45F, 9.001F, 10000.01F);
			Assert.That (source.Average (n => n * 2F), Is.EqualTo (6670.37354).Within (0.01));
		}

		[Test]
		public void Average_EmptyNullableFloatSource_Null ()
		{
			Assert.That (Read<float?> ().Average (), Is.Null);
		}

		[Test]
		public void Average_NullableFloatsWithSomeNulls_ReturnsAverage ()
		{
			var source = Read<float?> (-3.45F, null, 9.001F, null, 10000.01F);
			Assert.That (source.Average (), Is.EqualTo (3335.187).Within (0.01));
		}

		[Test]
		public void Average_SelectorArg_NullableFloatsWithSomeNulls_ReturnsAverage ()
		{
			var source = Read<float?> (-3.45F, null, 9.001F, null, 10000.01F);
			Assert.That (source.Average (n => n * 2F), Is.EqualTo (6670.37354).Within (0.01));
		}

		[Test]
		public void Average_EmptyNullableDoubleSource_Null ()
		{
			Assert.That (Read<double?> ().Average (), Is.Null);
		}

		[Test]
		public void Average_NullableDoublesWithSomeNulls_ReturnsAverage ()
		{
			var source = Read<double?> (-3.45, null, 9.001, null, 10000.01);
			Assert.That (source.Average (), Is.EqualTo (3335.187).Within (0.01));
		}

		[Test]
		public void Average_SelectorArg_NullableDoublesWithSomeNulls_ReturnsAverage ()
		{
			var source = Read<double?> (-3.45, null, 9.001, null, 10000.01);
			Assert.That (source.Average (n => n * 2.0), Is.EqualTo (6670.374).Within (0.01));
		}

		[Test]
		public void Average_EmptyNullableDecimalSource_Null ()
		{
			Assert.That (Read<decimal?> ().Average (), Is.Null);
		}

		[Test]
		public void Average_NullableDecimalsWithSomeNulls_ReturnsAverage ()
		{
			var source = Read<decimal?> (-3.45m, null, 9.001m, null, 10000.01m);
			Assert.That (source.Average (), Is.EqualTo (3335.187).Within (0.01));
		}

		[Test]
		public void Average_SelectorArg_NullableDecimalsWithSomeNulls_ReturnsAverage ()
		{
			var source = Read<decimal?> (-3.45m, null, 9.001m, null, 10000.01m);
			Assert.That (source.Average (n => n * 2m), Is.EqualTo (6670.374m).Within (0.01));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Concat_FirstSourceNull_ThrowsArgumentNullException ()
		{
			Enumerable.Concat (null, new object [0]);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Concat_SecondSourceNull_ThrowsArgumentNullException ()
		{
			new object [0].Concat (null);
		}

		[Test]
		public void Concat_TwoSequences_CombinedSequenceWhereElementsOfSecondFollowFirst ()
		{
			var first = Read (12, 34, 56);
			var second = Read (78, 910, 1112);
			first.Concat (second).AssertEquals (12, 34, 56, 78, 910, 1112);
		}

		[Test]
		public void Contains_SequenceContainingSoughtValue_ReturnsTrue ()
		{
			var source = Read (12, -15, 21);
			Assert.That (source.Contains (21), Is.True);
		}

		[Test]
		public void Contains_SequenceWithoutSoughtValue_ReturnsFalse ()
		{
			var source = Read (-2, 4, 8);
			Assert.That (source.Contains (9), Is.False);
		}

		[Test]
		public void Contains_CollectionOptimization_ReturnsTrueWithoutEnumerating ()
		{
			var source = new NonEnumerableList<int> (new [] { 1, 2, 3 });

			// IMPORTANT! Use the non-extension invocation style below
			//            to avoid calling List<T>.Contains instead of
			//            Enumerable.Contains.

			Assert.That (Enumerable.Contains (source, 3), Is.True);
		}

		[Test]
		public void Count_Integers_ReturnsNumberOfElements ()
		{
			Assert.That (Read (12, 34, 56).Count (), Is.EqualTo (3));
		}

		[Test]
		public void Count_PredicateArg_Strings_CountsOnlyStringsWithEvenLength ()
		{
			var source = Read ("A", "AB", "ABC", "ABCD");
			Assert.That (source.Count (s => s.Length % 2 == 0), Is.EqualTo (2));
		}

		[Test]
		public void DefaultIfEmpty_Integers_YieldsIntegersInOrder ()
		{
			var source = Read (12, 34, 56);
			source.DefaultIfEmpty (1).AssertEquals (12, 34, 56);
		}

		[Test]
		public void DefaultIfEmpty_EmptyIntegerSequence_ReturnsZero ()
		{
			var source = Read (new int [0]);
			source.DefaultIfEmpty ().AssertEquals (0);
		}

		[Test]
		public void DefaultIfEmpty_DefaultValueArg_EmptyIntegerSequenceAndNonZeroDefault_ReturnNonZeroDefault ()
		{
			var source = Read (new int [0]);
			source.DefaultIfEmpty (5).AssertEquals (5);
		}

		[Test]
		public void DefaultIfEmpty_DefaultValueArg_Integers_YieldsIntegersInOrder ()
		{
			var source = Read (12, 34, 56);
			source.DefaultIfEmpty (5).AssertEquals (12, 34, 56);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Distinct_NullSource_ThrowsArgumentNullException ()
		{
			Enumerable.Distinct<object> (null);
		}

		[Test]
		public void Distinct_IntegersWithSomeDuplicates_YieldsIntegersInSourceOrderWithoutDuplicates ()
		{
			var source = Read (12, 34, 34, 56, 78, 78, 78, 910, 78);
			source.Distinct ().AssertEquals (12, 34, 56, 78, 910);
		}

		[Test]
		[Category ("ManagedCollator")]
		public void Distinct_MixedCaseStringsWithCaseIgnoringComparer_YieldsFirstCaseOfEachDistinctStringInSourceOrder ()
		{
			var source = Read ("Foo Bar BAZ BaR baz FOo".Split ());
			source.Distinct (StringComparer.InvariantCultureIgnoreCase).AssertEquals ("Foo", "Bar", "BAZ");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ElementAt_IndexOutOfRange_ThrowsArgumentOutOfRangeException ()
		{
			var source = Read (3, 5, 7);
			source.ElementAt (3);
		}

		[Test]
		public void ElementAt_Integers_ReturnsValueAtGivenIndex ()
		{
			var source = new [] { 15, 2, 7 };
			Assert.That (Read (source).ElementAt (0), Is.EqualTo (15));
			Assert.That (Read (source).ElementAt (1), Is.EqualTo (2));
			Assert.That (Read (source).ElementAt (2), Is.EqualTo (7));
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ElementAt_NegativeIndex_ThrowsArgumentOutOfRangeException ()
		{
			Read<int> ().ElementAt (-1);
		}

		[Test]
		public void ElementAt_ListOptimization_ReturnsValueAtGivenIndex ()
		{
			var source = new NonEnumerableList<int> (new [] { 1, 2, 3, 4, 5, 6 });
			Assert.That (source.ElementAt (2), Is.EqualTo (3));
		}

		[Test]
		public void ElementAt_ReadOnlyListOptimization_ReturnsValueAtGivenIndex()
		{
			var source = new NonEnumerableReadOnlyList<int> (new List<int> (new[] { 1, 2, 3, 4, 5, 6 }));
			Assert.That(source.ElementAt (2), Is.EqualTo (3));
		}

		[Test]
		public void ElementAtOrDefault_IntegersWithOutOfRangeIndex_ReturnsDefault ()
		{
			var source = Read (3, 6, 8);
			Assert.That (source.ElementAtOrDefault (3), Is.EqualTo (0));
		}

		[Test]
		public void ElementAtOrDefault_Integers_ReturnsValueAtGivenIndex ()
		{
			var source = Read (3, 6, 9);
			Assert.That (source.ElementAtOrDefault (2), Is.EqualTo (9));
		}

		[Test]
		public void ElementAtOrDefault_ListOptimization_ReturnsValueAtGivenIndex ()
		{
			var source = new NonEnumerableList<int> (new [] { 1, 2, 3, 4, 5, 6 });
			Assert.That (source.ElementAtOrDefault (2), Is.EqualTo (3));
		}

		[Test]
		public void ElementAtOrDefault_ReadOnlyListOptimization_ReturnsValueAtGivenIndex()
		{
			var source = new NonEnumerableReadOnlyList<int>(new List<int> (new[] { 1, 2, 3, 4, 5, 6 }));
			Assert.That(source.ElementAtOrDefault (2), Is.EqualTo (3));
		}

		[Test]
		public void ElementAtOrDefault_BooleansAndNegativeIndex_ReturnsDefault ()
		{
			var source = Read (true, false, true, false);
			Assert.That (source.ElementAtOrDefault (-3), Is.False);
		}

		[Test]
		public void ElementAtOrDefault_ObjectsWithOutOfRangeIndex_ReturnsNull ()
		{
			var source = Read (new object (), new object ());
			Assert.That (source.ElementAtOrDefault (2), Is.EqualTo (null));
		}

		[Test]
		public void ElementAtOrDefault_Objects_ReturnsValueAtGivenIndex ()
		{
			var second = new object ();
			var source = Read (new object (), second, new object ());
			Assert.That (source.ElementAt (1), Is.EqualTo (second));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Except_SecondArg_ArgumentNull_ThrowsArgumentNullException ()
		{
			Read<object> ().Except (null);
		}

		[Test]
		public void Except_SecondArg_ValidArgument_ReturnsDifference ()
		{
			var first = Read (1, 2, 3, 4, 5, 6, 7, 8, 9, 10);
			var second = Read (1, 3, 5, 7, 9);
			first.Except (second).AssertEquals (2, 4, 6, 8, 10);
		}

		[Test]
		[Category ("ManagedCollator")]
		public void Except_SecondArgComparerArg_ComparerIsUsed ()
		{
			var first = Read ("albert", "john", "simon");
			var second = Read ("ALBERT");
			first.Except (second, StringComparer.CurrentCultureIgnoreCase).AssertEquals ("john", "simon");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void First_EmptySource_ThrowsInvalidOperationException ()
		{
			Read<int> ().First ();
		}

		[Test]
		public void First_Integers_ReturnsFirst ()
		{
			var source = Read (12, 34, 56);
			Assert.That (source.First (), Is.EqualTo (12));
		}

		[Test]
		public void First_IntegersWithPredicateForEvens_FirstEvenInteger ()
		{
			var source = Read (15, 20, 25, 30);
			Assert.That (source.First (i => i % 2 == 0), Is.EqualTo (20));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void First_IntegerSequenceWithNoneMatchingPredicate_ThrowsInvalidOperationException ()
		{
			var source = Read (12, 34, 56, 78);
			Assert.That (source.First (i => i > 100), Is.EqualTo (0));
		}

		[Test]
		public void FirstOrDefault_EmptyBooleanSource_ReturnsFalse ()
		{
			Assert.That (Read<bool> ().FirstOrDefault (), Is.False);
		}

		[Test]
		public void FirstOrDefault_Objects_ReturnsFirstReference ()
		{
			var first = new object ();
			var source = Read (first, new object ());
			Assert.That (source.FirstOrDefault (), Is.SameAs (first));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void FirstOrDefault_PredicateArg_NullPredicate_ThrowsArgumentNullException ()
		{
			Read<int> ().FirstOrDefault (null);
		}

		[Test]
		public void FirstOrDefault_PredicateArg_NonNullPredicate_ReturnsFirstMatchingItem ()
		{
			var source = Read (1, 4, 8);
			Assert.That (source.FirstOrDefault (i => i % 2 == 0), Is.EqualTo (4));
		}

		[Test]
		public void FirstOrDefault_PredicateArg_IntegerSequenceWithNonMatchingPredicate_ReturnsDefaultValue ()
		{
			var source = Read (1, 4, 6);
			Assert.That (source.FirstOrDefault (i => i > 10), Is.EqualTo (0));
		}

		[Test]
		public void First_IntegerListOptimization_ReturnsFirstElementWithoutEnumerating ()
		{
			var source = new NonEnumerableList<int> (new [] { 123, 456, 789 });
			Assert.That (source.First (), Is.EqualTo (123));
		}

		private class Person {
			public string FirstName { get; set; }
			public string LastName { get; set; }
			public int Age { get; set; }

			public static Person [] CreatePersons ()
			{
				return new []
                {
                    new Person { LastName = "M\u00FCller", FirstName = "Peter",   Age = 21 },
                    new Person { LastName = "M\u00FCller", FirstName = "Herbert", Age = 22 },
                    new Person { LastName = "Meier",       FirstName = "Hubert",  Age = 23 },
                    new Person { LastName = "Meier",       FirstName = "Isidor",  Age = 24 }
                };
			}

			public static Person [] CreatePersonsWithNamesUsingMixedCase ()
			{
				var persons = CreatePersons ();
				var herbert = persons [1];
				herbert.LastName = herbert.LastName.ToLower ();
				var isidor = persons [3];
				isidor.LastName = isidor.LastName.ToLower ();
				return persons;
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GroupBy_KeySelectorArg_NullAsKeySelector_ThrowsArgumentNullException ()
		{
			Read<object> ().GroupBy<object, object> (null);
		}

		[Test]
		public void GroupBy_KeySelectorArg_ValidArguments_CorrectGrouping ()
		{
			var persons = Read (Person.CreatePersons ());
			var result = new Reader<IGrouping<string, Person>> (persons.GroupBy (person => person.LastName));

			var group1 = result.Read ();
			Assert.That (group1.Key, Is.EqualTo ("M\u00FCller"));
			var muellers = new Reader<Person> (group1);
			Assert.That (muellers.Read ().FirstName, Is.EqualTo ("Peter"));
			Assert.That (muellers.Read ().FirstName, Is.EqualTo ("Herbert"));

			var group2 = result.Read ();
			Assert.That (group2.Key, Is.EqualTo ("Meier"));
			var meiers = new Reader<Person> (group2);
			Assert.That (meiers.Read ().FirstName, Is.EqualTo ("Hubert"));
			Assert.That (meiers.Read ().FirstName, Is.EqualTo ("Isidor"));

			result.AssertEnded ();
		}

		[Test]
		public void GroupBy_KeySelectorArg_ValidArguments_CorrectCaseSensitiveGrouping ()
		{
			var persons = Read (Person.CreatePersonsWithNamesUsingMixedCase ());

			var result = persons.GroupBy (person => person.LastName);

			var e = result.GetEnumerator ();
			Func<IGrouping<string, Person>, Person> first = g => new Reader<Person> (g).Read ();

			e.MoveNext ();
			Assert.That (e.Current.Key, Is.EqualTo ("M\u00FCller"));
			Assert.That (first (e.Current).FirstName, Is.EqualTo ("Peter"));

			e.MoveNext ();
			Assert.That (e.Current.Key, Is.EqualTo ("m\u00FCller"));
			Assert.That (first (e.Current).FirstName, Is.EqualTo ("Herbert"));

			e.MoveNext ();
			Assert.That (e.Current.Key, Is.EqualTo ("Meier"));
			Assert.That (first (e.Current).FirstName, Is.EqualTo ("Hubert"));

			e.MoveNext ();
			Assert.That (e.Current.Key, Is.EqualTo ("meier"));
			Assert.That (first (e.Current).FirstName, Is.EqualTo ("Isidor"));

			Assert.That (e.MoveNext (), Is.False);
		}

		[Test]
		[Category ("ManagedCollator")]
		public void GroupBy_KeySelectorArgComparerArg_KeysThatDifferInCasingNonCaseSensitiveStringComparer_CorrectGrouping ()
		{
			var persons = Read (Person.CreatePersonsWithNamesUsingMixedCase ());

			var result = new Reader<IGrouping<string, Person>> (
				persons.GroupBy (person => person.LastName, StringComparer.CurrentCultureIgnoreCase));

			var group1 = result.Read ();
			Assert.That (group1.Key, Is.EqualTo ("M\u00FCller"));
			var muellers = new Reader<Person> (group1);
			Assert.That (muellers.Read ().FirstName, Is.EqualTo ("Peter"));
			Assert.That (muellers.Read ().FirstName, Is.EqualTo ("Herbert"));

			var group2 = result.Read ();
			Assert.That (group2.Key, Is.EqualTo ("Meier"));
			var meiers = new Reader<Person> (group2);
			Assert.That (meiers.Read ().FirstName, Is.EqualTo ("Hubert"));
			Assert.That (meiers.Read ().FirstName, Is.EqualTo ("Isidor"));

			result.AssertEnded ();
		}

		[Test]
		public void GroupBy_KeySelectorArgElementSelectorArg_ValidArguments_CorrectGroupingAndProjection ()
		{
			var persons = Read (Person.CreatePersons ());

			var result = new Reader<IGrouping<string, int>> (
				persons.GroupBy (person => person.LastName, person => person.Age));

			var group1 = result.Read ();
			Assert.That (group1.Key, Is.EqualTo ("M\u00FCller"));
			var muellers = new Reader<int> (group1);
			Assert.That (muellers.Read (), Is.EqualTo (21));
			Assert.That (muellers.Read (), Is.EqualTo (22));

			var group2 = result.Read ();
			Assert.That (group2.Key, Is.EqualTo ("Meier"));
			var meiers = new Reader<int> (group2);
			Assert.That (meiers.Read (), Is.EqualTo (23));
			Assert.That (meiers.Read (), Is.EqualTo (24));
		}

		[Test]
		public void GroupBy_KeySelectorArgResultSelectorArg_ValidArguments_CorrectGroupingProcessing ()
		{
			var persons = Read (Person.CreatePersons ());

			var result = persons.GroupBy (
							 p => p.LastName,
							 (key, group) => {
								 var total = 0;
								 foreach (var p in group)
									 total += p.Age;
								 return key + ":" + total;
							 });

			result.AssertEquals ("M\u00FCller:43", "Meier:47");
		}

		[Test]
		[Category ("ManagedCollator")]
		public void GroupBy_KeySelectorArgElementSelectorArgComparerArg_ValidArguments_CorrectGroupingAndProcessing ()
		{
			var persons = Read (Person.CreatePersonsWithNamesUsingMixedCase ());

			var result = new Reader<IGrouping<string, int>> (
				persons.GroupBy (p => p.LastName, p => p.Age, StringComparer.CurrentCultureIgnoreCase));

			var group1 = result.Read ();
			Assert.That (group1.Key, Is.EqualTo ("M\u00FCller"));
			var muellers = new Reader<int> (group1);
			Assert.That (muellers.Read (), Is.EqualTo (21));
			Assert.That (muellers.Read (), Is.EqualTo (22));

			var group2 = result.Read ();
			Assert.That (group2.Key, Is.EqualTo ("Meier"));
			var meiers = new Reader<int> (group2);
			Assert.That (meiers.Read (), Is.EqualTo (23));
			Assert.That (meiers.Read (), Is.EqualTo (24));
		}

		[Test]
		public void GroupBy_KeySelectorArgElementSelectorArgResultSelectorArg_ValidArguments_CorrectGroupingAndTransforming ()
		{
			var persons = Read (Person.CreatePersons ());

			var result = persons.GroupBy (
							 p => p.LastName,
							 p => p.Age,
							 (key, ages) => {
								 var total = 0;
								 foreach (var age in ages)
									 total += age;
								 return key + ":" + total;
							 });

			result.AssertEquals ("M\u00FCller:43", "Meier:47");
		}

		[Test]
		[Category ("ManagedCollator")]
		public void GroupBy_KeySelectorArgResultSelectorArgComparerArg_ValidArguments_CorrectGroupingAndTransforming ()
		{
			var persons = Read (Person.CreatePersonsWithNamesUsingMixedCase ());

			var result = persons.GroupBy (
							 p => p.LastName,
							 (key, values) => {
								 var total = 0;
								 foreach (var person in values)
									 total += person.Age;
								 return key + ":" + total;
							 },
							 StringComparer.CurrentCultureIgnoreCase);

			result.AssertEquals ("M\u00FCller:43", "Meier:47");
		}

		[Test]
		[Category ("ManagedCollator")]
		public void GroupBy_KeySelectorArgElementSelectorArgResultSelectorArgComparerArg_ValidArguments_CorrectGroupingAndTransforming ()
		{
			var persons = Read (Person.CreatePersonsWithNamesUsingMixedCase ());

			var result = persons.GroupBy (
							 p => p.LastName,
							 p => p.Age,
							 (key, ages) => {
								 var total = 0;
								 foreach (var age in ages)
									 total += age;
								 return key + ":" + total;
							 },
							 StringComparer.CurrentCultureIgnoreCase);

			result.AssertEquals ("M\u00FCller:43", "Meier:47");
		}

		class Pet {
			public string Name { get; set; }
			public string Owner { get; set; }
		}

		[Test]
		public void GroupJoin_InnerArgOuterKeySelectorArgInnerKeySelectorArgResultSelectorArg_ValidArguments_CorrectGroupingAndJoining ()
		{
			var persons = Read (Person.CreatePersons ());

			var barley = new Pet { Name = "Barley", Owner = "Peter" };
			var boots = new Pet { Name = "Boots", Owner = "Herbert" };
			var whiskers = new Pet { Name = "Whiskers", Owner = "Herbert" };
			var daisy = new Pet { Name = "Daisy", Owner = "Isidor" };

			var pets = Read (barley, boots, whiskers, daisy);

			var result = persons.GroupJoin (pets, person => person.FirstName, pet => pet.Owner,
							  (person, ppets) => new { Owner = person, Pets = ppets });

			using (var e = result.GetEnumerator ()) {
				e.MoveNext (); Assert.That (e.Current.Owner.FirstName, Is.EqualTo ("Peter"));
				e.Current.Pets.AssertThat (Is.SameAs, barley);

				e.MoveNext (); Assert.That (e.Current.Owner.FirstName, Is.EqualTo ("Herbert"));
				e.Current.Pets.AssertThat (Is.SameAs, boots, whiskers);

				e.MoveNext (); Assert.That (e.Current.Owner.FirstName, Is.EqualTo ("Hubert"));
				e.Current.Pets.AssertThat (Is.SameAs); // empty

				e.MoveNext (); Assert.That (e.Current.Owner.FirstName, Is.EqualTo ("Isidor"));
				e.Current.Pets.AssertThat (Is.SameAs, daisy);

				Assert.That (e.MoveNext (), Is.False);
			}
		}

		[Test]
		[Category ("ManagedCollator")]
		public void GroupJoin_InnerArgOuterKeySelectorArgInnerKeySelectorArgResultSelectorArgComparerArg_ValidArguments_CorrectGroupingAndJoining ()
		{
			var persons = Read (Person.CreatePersons ());

			var barley = new Pet { Name = "Barley", Owner = "Peter" };
			var boots = new Pet { Name = "Boots", Owner = "Herbert" };
			var whiskers = new Pet { Name = "Whiskers", Owner = "HeRbErT" };
			var daisy = new Pet { Name = "Daisy", Owner = "Isidor" };

			var pets = Read (barley, boots, whiskers, daisy);

			var result = persons.GroupJoin (pets, person => person.FirstName, pet => pet.Owner,
							  (person, ppets) => new { Owner = person, Pets = ppets },
							  StringComparer.CurrentCultureIgnoreCase);

			using (var e = result.GetEnumerator ()) {
				e.MoveNext (); Assert.That (e.Current.Owner.FirstName, Is.EqualTo ("Peter"));
				e.Current.Pets.AssertThat (Is.SameAs, barley);

				e.MoveNext (); Assert.That (e.Current.Owner.FirstName, Is.EqualTo ("Herbert"));
				e.Current.Pets.AssertThat (Is.SameAs, boots, whiskers);

				e.MoveNext (); Assert.That (e.Current.Owner.FirstName, Is.EqualTo ("Hubert"));
				e.Current.Pets.AssertThat (Is.SameAs); // empty

				e.MoveNext (); Assert.That (e.Current.Owner.FirstName, Is.EqualTo ("Isidor"));
				e.Current.Pets.AssertThat (Is.SameAs, daisy);

				Assert.That (e.MoveNext (), Is.False);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GroupJoin_InnerArgOuterKeySelectorArgInnerKeySelectorArgResultSelectorArg_NullOuterKeySelector_ThrowsArgumentNullException ()
		{
			new object [0].GroupJoin<object, object, object, object> (
				new object [0], null,
				delegate { throw new NotImplementedException (); },
				delegate { throw new NotImplementedException (); });
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Intersect_NullSecondSource_ThrowsArgumentNullException ()
		{
			Read<object> ().Intersect (null);
		}

		[Test]
		public void Intersect_IntegerSources_YieldsCommonSet ()
		{
			var first = Read (1, 2, 3);
			var second = Read (2, 3, 4);
			first.Intersect (second).AssertEquals (2, 3);
		}

		[Test]
		[Category ("ManagedCollator")]
		public void Intersect_MixedStringsAndCaseInsensitiveComparer_YieldsCommonSetFromFirstSource ()
		{
			var first = Read ("Heinrich", "Hubert", "Thomas");
			var second = Read ("Heinrich", "hubert", "Joseph");
			var result = first.Intersect (second, StringComparer.CurrentCultureIgnoreCase);
			result.AssertEquals ("Heinrich", "Hubert");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Join_InnerArgOuterKeySelectorArgInnerKeySelectorArgResultSelectorArg_PassNullAsArgument_ThrowsArgumentNullException ()
		{
			Read<object> ().Join<object, object, object, object> (null, null, null, null);
		}

		[Test]
		public void Join_InnerArgOuterKeySelectorArgInnerKeySelectorArgResultSelectorArg_PassingPetsAndOwners_PetsAreCorrectlyAssignedToOwners ()
		{
			var persons = Read (Person.CreatePersons ());

			var pets = new Reader<Pet> (new []
                           {
                               new Pet {Name = "Barley", Owner = "Peter"},
                               new Pet {Name = "Boots", Owner = "Herbert"},
                               new Pet {Name = "Whiskers", Owner = "Herbert"},
                               new Pet {Name = "Daisy", Owner = "Isidor"}
                           });

			var result = persons.Join (pets, aPerson => aPerson.FirstName, aPet => aPet.Owner,
						 (aPerson, aPet) => new { Owner = aPerson.FirstName, Pet = aPet.Name });

			var e = result.GetEnumerator ();

			Assert.That (e.MoveNext (), Is.True);
			Assert.That (e.Current.Owner, Is.EqualTo ("Peter"));
			Assert.That (e.Current.Pet, Is.EqualTo ("Barley"));

			Assert.That (e.MoveNext (), Is.True);
			Assert.That (e.Current.Owner, Is.EqualTo ("Herbert"));
			Assert.That (e.Current.Pet, Is.EqualTo ("Boots"));

			Assert.That (e.MoveNext (), Is.True);
			Assert.That (e.Current.Owner, Is.EqualTo ("Herbert"));
			Assert.That (e.Current.Pet, Is.EqualTo ("Whiskers"));

			Assert.That (e.MoveNext (), Is.True);
			Assert.That (e.Current.Owner, Is.EqualTo ("Isidor"));
			Assert.That (e.Current.Pet, Is.EqualTo ("Daisy"));

			Assert.That (e.MoveNext (), Is.False);
		}

		[Test]
		[Category ("ManagedCollator")]
		public void Join_InnerArgOuterKeySelectorArgInnerKeySelectorArgResultSelectorArgComparerArg_PetOwnersNamesCasingIsInconsistent_CaseInsensitiveJoinIsPerformed ()
		{
			var persons = Read (Person.CreatePersons ());

			var pets = new Reader<Pet> (new []
                           {
                               new Pet {Name = "Barley", Owner = "Peter"},
                               new Pet {Name = "Boots", Owner = "Herbert"},
                               new Pet {Name = "Whiskers", Owner = "herbert"},
                               new Pet {Name = "Daisy", Owner = "Isidor"}
                           });
			var result = persons.Join (pets, aPerson => aPerson.FirstName, aPet => aPet.Owner,
						 (aPerson, aPet) => new { Owner = aPerson.FirstName, Pet = aPet.Name },
						 StringComparer.CurrentCultureIgnoreCase);

			var e = result.GetEnumerator ();

			Assert.That (e.MoveNext (), Is.True);
			Assert.That (e.Current.Owner, Is.EqualTo ("Peter"));
			Assert.That (e.Current.Pet, Is.EqualTo ("Barley"));

			Assert.That (e.MoveNext (), Is.True);
			Assert.That (e.Current.Owner, Is.EqualTo ("Herbert"));
			Assert.That (e.Current.Pet, Is.EqualTo ("Boots"));

			Assert.That (e.MoveNext (), Is.True);
			Assert.That (e.Current.Owner, Is.EqualTo ("Herbert"));
			Assert.That (e.Current.Pet, Is.EqualTo ("Whiskers"));

			Assert.That (e.MoveNext (), Is.True);
			Assert.That (e.Current.Owner, Is.EqualTo ("Isidor"));
			Assert.That (e.Current.Pet, Is.EqualTo ("Daisy"));

			Assert.That (e.MoveNext (), Is.False);
		}

		[Test]
		public void Last_Integers_ReturnsLastElement ()
		{
			var source = Read (1, 2, 3);
			Assert.That (source.Last (), Is.EqualTo (3));
		}

		[Test]
		public void Last_IntegerListOptimization_ReturnsLastElementWithoutEnumerating ()
		{
			var source = new NonEnumerableList<int> (new [] { 1, 2, 3 });
			Assert.That (source.Last (), Is.EqualTo (3));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void Last_EmptyIntegerListOptimization_ThrowsInvalidOperationException ()
		{
			new NonEnumerableList<int> ().Last ();
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Last_PredicateArg_NullAsPredicate_ThrowsArgumentNullException ()
		{
			Read<object> ().Last (null);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void Last_PredicateArg_NoMatchingElement_ThrowsInvalidOperationException ()
		{
			var source = Read (1, 2, 3, 4, 5);
			source.Last (i => i > 10);
		}

		[Test]
		public void Last_PredicateArg_ListOfInts_ReturnsLastMatchingElement ()
		{
			var source = Read (1, 2, 3, 4, 5);
			Assert.That (source.Last (i => i % 2 == 0), Is.EqualTo (4));
		}

		[Test]
		public void LastOrDefault_EmptySource_ReturnsZero ()
		{
			var source = Read (new int [0]);
			Assert.That (source.LastOrDefault (), Is.EqualTo (0));
		}

		[Test]
		public void LastOrDefault_NonEmptyList_ReturnsLastElement ()
		{
			var source = Read (1, 2, 3, 4, 5);
			Assert.That (source.LastOrDefault (), Is.EqualTo (5));
		}

		[Test]
		public void LastOrDefault_PredicateArg_ValidArguments_RetunsLastMatchingElement ()
		{
			var source = Read (1, 2, 3, 4, 5);
			Assert.That (source.LastOrDefault (i => i % 2 == 0), Is.EqualTo (4));
		}

		[Test]
		public void LastOrDefault_PredicateArg_NoMatchingElement_ReturnsZero ()
		{
			var source = Read (1, 3, 5, 7);
			Assert.That (source.LastOrDefault (i => i % 2 == 0), Is.EqualTo (0));
		}

		[Test]
		public void LongCount_ValidArgument_ReturnsCorrectNumberOfElements ()
		{
			var source = Read (1, 4, 7, 10);
			Assert.That (source.LongCount (), Is.EqualTo (4));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void LongCount_PredicateArg_NullAsPredicate_ThrowsArgumentNullException ()
		{
			Read<object> ().LongCount (null);
		}

		[Test]
		public void LongCount_PredicateArg_ValidArguments_ReturnsCorrectNumerOfMatchingElements ()
		{
			var source = Read (1, 2, 3, 4, 5);
			Assert.That (source.LongCount (i => i % 2 == 0), Is.EqualTo (2));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void Max_EmptyList_ThrowsInvalidOperationException ()
		{
			var source = Read (new int [0]);
			source.Max ();
		}

		[Test]
		public void Max_EmptyNullableIntegerArray_ReturnsNull ()
		{
			Assert.That (Read (new int? [0]).Max (), Is.Null);
		}

		[Test]
		public void Max_NullableIntegerArrayWithNullsOnly_ReturnsNull ()
		{
			Assert.That (Read<int?> (null, null, null).Max (), Is.Null);
		}

		[Test]
		public void Max_Integers_ReturnsMaxValue ()
		{
			var source = Read (1000, 203, -9999);
			Assert.That (source.Max (), Is.EqualTo (1000));
		}

		[Test]
		public void Max_NullableLongs_ReturnsMaxValue ()
		{
			Assert.That (Read<long?> (1L, 2L, 3L, null).Max (), Is.EqualTo (3));
		}

		[Test]
		public void Max_NullableDoubles_ReturnsMaxValue ()
		{
			Assert.That (Read<double?> (1.0, 2.0, 3.0, null).Max (), Is.EqualTo (3));
		}

		[Test]
		public void Max_NullableDecimals_ReturnsMaxValue ()
		{
			Assert.That (Read<decimal?> (1m, 2m, 3m, null).Max (), Is.EqualTo (3));
		}

		[Test]
		public void Max_NullableFloats_ReturnsMaxValue ()
		{
			Assert.That (Read<float?> (-1000F, -100F, -1F, null).Max (), Is.EqualTo (-1));
		}

		[Test]
		public void Max_ListWithNullableType_ReturnsMaximum ()
		{
			var source = Read<int?> (1, 4, null, 10);
			Assert.That (source.Max (), Is.EqualTo (10));
		}

		[Test]
		public void Max_NullableList_ReturnsMaxNonNullValue ()
		{
			var source = Read<int?> (-5, -2, null);
			Assert.That (source.Max (), Is.EqualTo (-2));
		}

		[Test]
		public void Max_SelectorArg_ListOfObjects_ReturnsMaxSelectedValue ()
		{
			var persons = Read (Person.CreatePersons ());
			Assert.That (persons.Max (p => p.Age), Is.EqualTo (24));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void Min_EmptyList_ThrowsInvalidOperationException ()
		{
			var source = Read<int> ();
			source.Min ();
		}

		[Test]
		public void Min_IntegersWithSomeNull_ReturnsMinimumNonNullValue ()
		{
			var source = Read<int?> (199, 15, null, 30);
			Assert.That (source.Min (), Is.EqualTo (15));
		}

		[Test]
		public void Min_NullableLongs_ReturnsMinimumNonNullValue ()
		{
			var source = Read<long?> (199L, 15L, null, 30L);
			Assert.That (source.Min (), Is.EqualTo (15));
		}

		[Test]
		public void Min_NullableFloats_ReturnsMinimumNonNullValue ()
		{
			var source = Read<float?> (1.111F, null, 2.222F); // TODO Improve test data
			Assert.That (source.Min (), Is.EqualTo (1.111F).Within (0.01));
		}

		[Test]
		public void Min_NullableDoubles_ReturnsMinimumNonNullValue ()
		{
			var source = Read<double?> (1.111, null, 2.222); // TODO Improve test data
			Assert.That (source.Min (), Is.EqualTo (1.111).Within (0.01));
		}

		[Test]
		public void Min_NullableDecimals_ReturnsMinimumNonNullValue ()
		{
			var source = Read<decimal?> (1.111m, null, 2.222m);  // TODO Improve test data
			Assert.That (source.Min (), Is.EqualTo (1.111m).Within (0.01));
		}

		[Test]
		public void Min_Chars_ReturnsMinimumBySortOrder ()
		{
			Assert.That ("qwertzuioplkjhgfdsayxcvbnm".ToCharArray ().Min (), Is.EqualTo ('a'));
		}

		[Test]
		public void Min_StringsWithLengthSelector_ReturnsMinimumNonNullStringLength ()
		{
			var strings = Read ("five", "four", null, "three", null, "two", "one", "zero");
			Assert.That (strings.Min (s => s != null ? s.Length : (int?) null), Is.EqualTo (3));
		}

		[Test]
		public void OfType_EnumerableWithElementsOfDifferentTypes_OnlyDecimalsAreReturned ()
		{
			var source = Read<object> (1, "Hello", 1.234m, new object ());
			var result = source.OfType<decimal> ();
			result.AssertEquals (1.234m);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void OrderBy_KeySelectorArg_NullAsKeySelector_ThrowsArgumentNullException ()
		{
			Read<object> ().OrderBy<object, object> (null);
		}

		[Test]
		public void OrderBy_KeySelector_ArrayOfPersons_PersonsAreOrderedByAge ()
		{
			var persons = Person.CreatePersons ();
			var reversePersons = (Person []) persons.Clone ();
			Array.Reverse (reversePersons);
			var source = Read (reversePersons);
			var result = source.OrderBy (p => p.Age);

			var age = 21;
			foreach (var person in result)
				Assert.That (person.Age, Is.EqualTo (age++));

			Assert.That (age, Is.EqualTo (25));
		}

		[Test]
		public void OrderBy_KeySelector_DataWithDuplicateKeys_YieldsStablySortedData ()
		{
			var data = new []
            {
                new { Number = 4, Text = "four" },
                new { Number = 4, Text = "quatre" },
                new { Number = 4, Text = "vier" },
                new { Number = 4, Text = "quattro" },
                new { Number = 1, Text = "one" },
                new { Number = 2, Text = "two" },
                new { Number = 2, Text = "deux" },
                new { Number = 3, Text = "three" },
                new { Number = 3, Text = "trois" },
                new { Number = 3, Text = "drei" },
            };

			var result = Read (data).OrderBy (e => e.Number);
			using (var e = result.GetEnumerator ()) {
				e.MoveNext (); Assert.That (e.Current.Text, Is.EqualTo ("one"));
				e.MoveNext (); Assert.That (e.Current.Text, Is.EqualTo ("two"));
				e.MoveNext (); Assert.That (e.Current.Text, Is.EqualTo ("deux"));
				e.MoveNext (); Assert.That (e.Current.Text, Is.EqualTo ("three"));
				e.MoveNext (); Assert.That (e.Current.Text, Is.EqualTo ("trois"));
				e.MoveNext (); Assert.That (e.Current.Text, Is.EqualTo ("drei"));
				e.MoveNext (); Assert.That (e.Current.Text, Is.EqualTo ("four"));
				e.MoveNext (); Assert.That (e.Current.Text, Is.EqualTo ("quatre"));
				e.MoveNext (); Assert.That (e.Current.Text, Is.EqualTo ("vier"));
				e.MoveNext (); Assert.That (e.Current.Text, Is.EqualTo ("quattro"));
				Assert.That (e.MoveNext (), Is.False);
			}
		}

		[Test]
		public void ThenBy_KeySelector_DataWithDuplicateKeys_YieldsStablySortedData ()
		{
			var data = new []
            {
                new { Position = 1, LastName = "Smith", FirstName = "John" },
                new { Position = 2, LastName = "Smith", FirstName = "Jack" },
                new { Position = 3, LastName = "Smith", FirstName = "John" },
                new { Position = 4, LastName = "Smith", FirstName = "Jack" },
                new { Position = 5, LastName = "Smith", FirstName = "John" },
                new { Position = 6, LastName = "Smith", FirstName = "Jack" },
            };

			var result = Read (data).OrderBy (e => e.LastName).ThenBy (e => e.FirstName);
			using (var e = result.GetEnumerator ()) {
				e.MoveNext (); Assert.That (e.Current.Position, Is.EqualTo (2));
				e.MoveNext (); Assert.That (e.Current.Position, Is.EqualTo (4));
				e.MoveNext (); Assert.That (e.Current.Position, Is.EqualTo (6));
				e.MoveNext (); Assert.That (e.Current.Position, Is.EqualTo (1));
				e.MoveNext (); Assert.That (e.Current.Position, Is.EqualTo (3));
				e.MoveNext (); Assert.That (e.Current.Position, Is.EqualTo (5));
				Assert.That (e.MoveNext (), Is.False);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ThenBy_NullSource_ThrowsArgumentNullException ()
		{
			Enumerable.ThenBy<object, object> (null, delegate { throw new NotImplementedException (); });
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ThenBy_NullKeySelector_ThrowsArgumentNullException ()
		{
			Read<object> ().OrderBy<object, object> (e => { throw new NotImplementedException (); }).ThenBy<object, object> (null);
		}

		[Test]
		public void ThenByDescending_KeySelectorArgComparerArg_StringArray_CorrectOrdering ()
		{
			var source = Read ("AA", "AB", "AC", "-BA", "-BB", "-BC");
			var result = source.OrderBy (s => s.ToCharArray () [s.ToCharArray ().Length - 1]).ThenByDescending (s => s.Length); /*.AssertEquals("butterfly", "elephant", "dog", "snake", "ape"); */
			result.AssertEquals ("-BA", "AA", "-BB", "AB", "-BC", "AC");
		}

		class ReverseComparer<T> : IComparer<T> where T : IComparable<T> {
			public int Compare (T x, T y)
			{
				return -1 * x.CompareTo (y);
			}
		}

		[Test]
		public void OrderBy_KeySelectorArgComparerArg_ArrayOfPersonsAndReversecomparer_PersonsAreOrderedByAgeUsingReversecomparer ()
		{
			var persons = Read (Person.CreatePersons ());
			var result = persons.OrderBy (p => p.Age, new ReverseComparer<int> ());
			var age = 25;
			foreach (var person in result) {
				age--;
				Assert.That (person.Age, Is.EqualTo (age));
			}
			Assert.That (age, Is.EqualTo (21));

		}

		[Test]
		public void OrderByDescending_KeySelectorArg_ArrayOfPersons_PersonsAreOrderedByAgeDescending ()
		{
			var persons = Read (Person.CreatePersons ());
			var result = persons.OrderByDescending (p => p.Age);
			int age = 25;
			foreach (var person in result) {
				age--;
				Assert.That (person.Age, Is.EqualTo (age));
			}
			Assert.That (age, Is.EqualTo (21));
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Range_ProduceRangeThatLeadsToOverflow_ThrowsArgumentOutOfRangeException ()
		{
			Enumerable.Range (int.MaxValue - 3, 5);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Range_NegativeCount_ThrowsArgumentOutOfRangeException ()
		{
			Enumerable.Range (0, -1);
		}

		[Test]
		public void Range_Start10Count5_IntsFrom10To14 ()
		{
			var result = Enumerable.Range (10, 5);
			result.AssertEquals (10, 11, 12, 13, 14);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Repeat_PassNegativeCount_ThrowsArgumentOutOfRangeException ()
		{
			Enumerable.Repeat ("Hello World", -2);
		}

		[Test]
		public void Repeat_StringArgumentCount2_ReturnValueContainsStringArgumentTwice ()
		{
			var result = Enumerable.Repeat ("Hello World", 2);
			result.AssertEquals ("Hello World", "Hello World");
		}

		[Test]
		public void Reverse_SeriesOfInts_IntsAreCorrectlyReversed ()
		{
			var source = Read (1, 2, 3, 4, 5);
			source.Reverse ().AssertEquals (5, 4, 3, 2, 1);
		}

		[Test]
		public void Select_ArrayOfPersons_AgeOfPersonsIsSelectedAccordingToPassedLambdaExpression ()
		{
			var persons = Read (Person.CreatePersons ());
			persons.Select (p => p.Age).AssertEquals (21, 22, 23, 24);
		}

		[Test]
		public void Select_SelectorArg_LambdaThatTakesIndexAsArgument_ReturnValueContainsElementsMultipliedByIndex ()
		{
			var source = Read (0, 1, 2, 3);
			source.Select ((i, index) => i * index).AssertEquals (0, 1, 4, 9);
		}

		[Test]
		public void SelectMany_SelectorArg_ArrayOfPersons_ReturnsASequenceWithAllLettersOfFirstnames ()
		{
			var persons = Read (Person.CreatePersons ());
			var result = persons.SelectMany (p => p.FirstName.ToCharArray ());
			var check = "PeterHerbertHubertIsidor".ToCharArray ();
			int count = 0; // BUGBUG Collapse loop-based check with array assertion!
			foreach (var c in result) {
				Assert.That (c, Is.EqualTo (check [count]));
				count++;
			}
		}

		class PetOwner {
			public string Name { get; set; }
			public IList<string> Pets { get; set; }
		}

		[Test]
		public void SelectMany_Selector3Arg_ArrayOfPetOwners_SelectorUsesElementIndexArgument ()
		{
			var petOwners = Read (new [] {
                  new PetOwner { Name = "Higa, Sidney",     Pets = new[] { "Scruffy", "Sam" } },
                  new PetOwner { Name = "Ashkenazi, Ronen", Pets = new[] { "Walker", "Sugar" } },
                  new PetOwner { Name = "Price, Vernette",  Pets = new[] { "Scratches", "Diesel" } },
                  new PetOwner { Name = "Hines, Patrick",   Pets = new[] { "Dusty" } } });

			var result = petOwners.SelectMany ((po, index) => po.Pets.Select (pet => index + pet));

			result.AssertEquals ("0Scruffy", "0Sam", "1Walker", "1Sugar", "2Scratches", "2Diesel", "3Dusty");
		}

		[Test]
		public void SelectMany_CollectionSelectorArgResultSelectorArg_ArrayOfPetOwner_ResultContainsElementForEachPetAPetOwnerHas ()
		{
			var petOwners = Read (new [] {
                  new PetOwner { Name = "Higa",      Pets = new[] { "Scruffy", "Sam" } },
                  new PetOwner { Name = "Ashkenazi", Pets = new[] { "Walker", "Sugar" } },
                  new PetOwner { Name = "Price",     Pets = new[] { "Scratches", "Diesel" } },
                  new PetOwner { Name = "Hines",     Pets = new[] { "Dusty" } } });

			var result = petOwners.SelectMany (po => po.Pets, (po, pet) => po.Name + "+" + pet);

			result.AssertEquals (
				"Higa+Scruffy", "Higa+Sam",
				"Ashkenazi+Walker", "Ashkenazi+Sugar",
				"Price+Scratches", "Price+Diesel",
				"Hines+Dusty");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void SequenceEqual_NullFirstSequence_ThrowsArgumentNullException ()
		{
			Enumerable.SequenceEqual (null, Read<object> ());
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void SequenceEqual_NullSecondSequence_ThrowsArgumentNullException ()
		{
			Read<object> ().SequenceEqual (null);
		}

		[Test]
		public void SequenceEqual_EqualSequences_ReturnsTrue ()
		{
			var source = Read (1, 2, 3);
			var argument = Read (1, 2, 3);
			Assert.That (source.SequenceEqual (argument), Is.True);
		}

		[Test]
		public void SequenceEqual_DifferentSequences_ReturnsFalse ()
		{
			var source = Read (1, 2, 3);
			var argument = Read (1, 3, 2);
			Assert.That (source.SequenceEqual (argument), Is.False);
		}

		[Test]
		public void SequenceEqual_LongerSecondSequence_ReturnsFalse ()
		{
			var source = Read (1, 2, 3);
			var argument = Read (1, 2, 3, 4);
			Assert.That (source.SequenceEqual (argument), Is.False);
		}

		[Test]
		public void SequenceEqual_ShorterSecondSequence_ReturnsFalse ()
		{
			var first = Read (1, 2, 3, 4);
			var second = Read (1, 2, 3);
			Assert.That (first.SequenceEqual (second), Is.False);
		}

		[Test]
		public void SequenceEqual_FloatsWithTolerantComparer_ComparerIsUsed ()
		{
			var source = Read (1F, 2F, 3F);
			var argument = Read (1.03F, 1.99F, 3.02F);
			Assert.That (source.SequenceEqual (argument, new FloatComparer ()), Is.True);
		}

		private sealed class FloatComparer : IEqualityComparer<float> {
			public bool Equals (float x, float y)
			{
				return Math.Abs (x - y) < 0.1f;
			}
			public int GetHashCode (float x)
			{
				throw new NotImplementedException ();
			}
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void Single_EmptySource_ThrowsInvalidOperationException ()
		{
			var source = Read<int> ();
			source.Single ();
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void Single_SourceWithMoreThanOneElement_ThrowsInvalidOperationException ()
		{
			var source = Read (3, 6);
			source.Single ();
		}

		[Test]
		public void Single_SourceWithOneElement_ReturnsSingleElement ()
		{
			var source = Read (1);
			Assert.That (source.Single (), Is.EqualTo (1));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Single_PredicateArg_PassNullAsPredicate_ThrowsArgumentNullException ()
		{
			Read<object> ().Single (null);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void Single_PredicateArg_NoElementSatisfiesCondition_ThrowsInvalidOperationException ()
		{
			var source = Read (1, 3, 5);
			source.Single (i => i % 2 == 0);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void Single_PredicateArg_MoreThanOneElementSatisfiedCondition_ThrowsInvalidOperationException ()
		{
			var source = Read (1, 2, 3, 4);
			source.Single (i => i % 2 == 0);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void Single_PredicateArg_SourceIsEmpty_ThrowsInvalidOperationException ()
		{
			var source = Read<int> ();
			source.Single (i => i % 2 == 0);
		}

		[Test]
		public void Single_PredicateArg_ArrayOfIntWithOnlyOneElementSatisfyingCondition_ReturnsOnlyThisElement ()
		{
			var source = Read (1, 2, 3);
			Assert.That (source.Single (i => i % 2 == 0), Is.EqualTo (2));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void SingleOrDefault_MoreThanOneElementInSource_ThrowsInvalidOperationException ()
		{
			var source = Read (1, 2, 3);
			source.SingleOrDefault ();
		}

		[Test]
		public void SingleOrDefault_EmptySource_ReturnsZero ()
		{
			var source = Read<int> ();
			Assert.That (source.SingleOrDefault (), Is.EqualTo (0));
		}

		[Test]
		public void SingleOrDefault_SourceWithOneElement_ReturnsSingleElement ()
		{
			var source = Read (5);
			Assert.That (source.SingleOrDefault (), Is.EqualTo (5));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void SingleOrDefault_PredicateArg_PassNullAsPredicate_ThrowsArgumentNullException ()
		{
			Read<object> ().SingleOrDefault (null);
		}

		[Test]
		public void SingleOrDefault_PredicateArg_EmptySource_ReturnsZero ()
		{
			var source = Read<int> ();
			Assert.That (source.SingleOrDefault (i => i % 2 == 0), Is.EqualTo (0));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void SingleOrDefault_PredicateArg_MoreThanOneElementSatisfiesCondition_ThrowsInvalidOperationException ()
		{
			var source = Read (1, 2, 3, 4, 5);
			source.SingleOrDefault (i => i % 2 == 0);
		}

		[Test]
		public void SingleOrDefault_PredicateArg_NoElementSatisfiesCondition_ReturnsZero ()
		{
			var source = Read (1, 3, 5);
			Assert.That (source.SingleOrDefault (i => i % 2 == 0), Is.EqualTo (0));
		}

		[Test]
		public void SingleOrDefault_PredicateArg_OneElementSatisfiesCondition_ReturnsCorrectElement ()
		{
			var source = Read (1, 2, 3);
			Assert.That (source.SingleOrDefault (i => i % 2 == 0), Is.EqualTo (2));
		}

		[Test]
		public void Skip_IntsFromOneToTenAndFifeAsSecondArg_IntsFromSixToTen ()
		{
			var source = Read (1, 2, 3, 4, 5, 6, 7, 8, 9, 10);
			source.Skip (5).AssertEquals (6, 7, 8, 9, 10);
		}

		[Test]
		public void Skip_PassNegativeValueAsCount_SameBehaviorAsMicrosoftImplementation ()
		{
			var source = Read (1, 2, 3, 4, 5);
			source.Skip (-5).AssertEquals (1, 2, 3, 4, 5);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void SkipWhile_PredicateArg_PassNullAsPredicate_ThrowsArgumentNullException ()
		{
			Read<object> ().SkipWhile ((Func<object, bool>) null);
		}

		[Test]
		public void SkipWhile_PredicateArg_IntsFromOneToFive_ElementsAreSkippedAsLongAsConditionIsSatisfied ()
		{
			var source = Read (1, 2, 3, 4, 5);
			source.SkipWhile (i => i < 3).AssertEquals (3, 4, 5);
		}

		[Test]
		public void SkipWhile_PredicateArg_ArrayOfIntsWithElementsNotSatisfyingConditionAtTheEnd_IntsAtTheEndArePartOfResult ()
		{
			var source = Read (1, 2, 3, 4, 5, 1, 2, 3);
			source.SkipWhile (i => i < 3).AssertEquals (3, 4, 5, 1, 2, 3);
		}

		[Test]
		public void SkipWhile_PredicateArg_PredicateAlwaysTrue_EmptyResult ()
		{
			var source = Read (1, 2, 3);
			var result = source.SkipWhile (i => true);
			Assert.That (result.GetEnumerator ().MoveNext (), Is.False);
		}

		[Test]
		public void SkipWhile_Predicate3Arg_IntsFromOneToNine_ElementsAreSkippedWhileIndexLessThanFive ()
		{
			var source = Read (1, 2, 3, 4, 5, 6, 7, 8, 9);
			source.SkipWhile ((i, index) => index < 5).AssertEquals (6, 7, 8, 9);
		}

		[Test]
		[ExpectedException (typeof (OverflowException))]
		public void Sum_SumOfArgumentsCausesOverflow_ThrowsOverflowException ()
		{
			var source = Read (int.MaxValue - 1, 2);
			source.Sum ();
		}

		[Test]
		public void Sum_IntsFromOneToTen_ResultIsFiftyFive ()
		{
			var source = Read (1, 2, 3, 4, 5, 6, 7, 8, 9, 10);
			Assert.That (source.Sum (), Is.EqualTo (55));
		}

		[Test]
		public void Sum_Longs_ReturnsSum ()
		{
			Assert.That (Read (1L, 2L, 3L).Sum (), Is.EqualTo (6));
		}

		[Test]
		public void Sum_SelectorArg_Longs_ReturnsSum ()
		{
			Assert.That (Read (123L, 456L, 789L).Sum (n => n * 2L), Is.EqualTo (2736L));
		}

		[Test]
		public void Sum_SelectorArg_NullableLongsWithSomeNulls_ReturnsSum ()
		{
			Assert.That (Read<long?> (123L, null, 456L, null, 789L).Sum (n => n * 2L), Is.EqualTo (2736L));
		}

		[Test]
		public void Sum_Floats_ReturnsSum ()
		{
			Assert.That (Read (1F, 2F, 3F).Sum (), Is.EqualTo (6));
		}

		[Test]
		public void Sum_SelectorArg_Floats_ReturnsSum ()
		{
			Assert.That (Read (123.4F, 567.8F, 91011.12F).Sum (n => n * 2.5F), Is.EqualTo (229255.8F));
		}

		[Test]
		public void Sum_NullableFloats_ReturnsSum ()
		{
			Assert.That (Read<float?> (1F, 2F, 3F, null).Sum (), Is.EqualTo (6));
		}

		[Test]
		public void Sum_SelectorArg_NullableFloatsWithSomeNulls_ReturnsSum ()
		{
			Assert.That (Read<float?> (123.4F, null, 567.8F, null, 91011.12F).Sum (n => n * 2.5F), Is.EqualTo (229255.8F));
		}

		[Test]
		public void Sum_Doubles_ReturnsSum ()
		{
			Assert.That (Read (1.0, 2.0, 3.0).Sum (), Is.EqualTo (6));
		}

		[Test]
		public void Sum_SelectorArg_Doubles_ReturnsSum ()
		{
			Assert.That (Read (123.4, 567.8, 91011.12).Sum (n => n * 2.5), Is.EqualTo (229255.8));
		}

		[Test]
		public void Sum_NullableDoubles_ReturnsSum ()
		{
			Assert.That (Read<double?> (1.0, 2.0, 3.0, null).Sum (), Is.EqualTo (6)); // TODO Improve test data
		}

		[Test]
		public void Sum_SelectorArg_NullableDoublesWithSomeNulls_ReturnsSum ()
		{
			Assert.That (Read<double?> (123.4, null, 567.8, null, 91011.12).Sum (n => n * 2.5), Is.EqualTo (229255.8));
		}

		[Test]
		public void Sum_Decimals_ReturnsSum ()
		{
			Assert.That (Read (1m, 2m, 3m).Sum (), Is.EqualTo (6));
		}

		[Test]
		public void Sum_SelectorArg_Decimals_ReturnsSum ()
		{
			Assert.That (Read (123.4m, 567.8m, 91011.12m).Sum (n => n * 2.5m), Is.EqualTo (229255.8m));
		}

		[Test]
		public void Sum_NullableDecimals_ReturnsSum ()
		{
			Assert.That (Read<decimal?> (1m, 2m, 3m, null).Sum (), Is.EqualTo (6)); // TODO Improve test data
		}

		[Test]
		public void Sum_SelectorArg_NullableDecimalsWithSomeNulls_ReturnsSum ()
		{
			Assert.That (Read<decimal?> (123.4m, null, 567.8m, null, 91011.12m).Sum (n => n * 2.5m), Is.EqualTo (229255.8m));
		}

		[Test]
		public void Sum_NullableLongs_ReturnsSum ()
		{
			Assert.That (Read<long?> (1L, 2L, 3L, null).Sum (), Is.EqualTo (6)); // TODO Improve test data
		}

		[Test]
		public void Sum_NullableIntsAsArguments_ReturnsCorrectSum () // TODO Improve test data
		{
			var source = Read<int?> (1, 2, null);
			Assert.That (source.Sum (), Is.EqualTo (3));
		}

		[Test]
		public void Sum_SelectorArgNullableIntegersWithSomeNulls_ReturnsSum ()
		{
			var source = Read<int?> (123, null, 456, null, 789);
			Assert.That (source.Sum (n => n * 2), Is.EqualTo (2736));
		}

		[Test]
		public void Sum_SelectorArg_StringArray_ResultIsSumOfStringLengths ()
		{
			var source = Read ("dog", "cat", "eagle");
			Assert.That (source.Sum (s => s.Length), Is.EqualTo (11));
		}

		[Test]
		public void Take_IntsFromOneToSixAndThreeAsCount_IntsFromOneToThreeAreReturned ()
		{
			var source = Read (1, 2, 3, 4, 5, 6);
			source.Take (3).AssertEquals (1, 2, 3);
		}

		[Test]
		public void Take_CountBiggerThanList_ReturnsAllElements ()
		{
			var source = Read (1, 2, 3, 4, 5);
			source.Take (10).AssertEquals (1, 2, 3, 4, 5);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TakeWhile_PassNullAsPredicate_ThrowsArgumentNullException ()
		{
			new object [0].TakeWhile ((Func<object, bool>) null);
		}

		[Test]
		public void TakeWhile_IntsFromOneToTenAndConditionThatSquareIsSmallerThan50_IntsFromOneToSeven ()
		{
			var source = Read (1, 2, 3, 4, 5, 6, 7, 8, 9, 10);
			source.TakeWhile (i => i * i < 50).AssertEquals (1, 2, 3, 4, 5, 6, 7);
		}

		[Test]
		public void ToArray_IntsFromOneToTen_ResultIsIntArrayContainingAllElements ()
		{
			var source = Read (1, 2, 3, 4, 5, 6, 7, 8, 9, 10);
			var result = source.ToArray ();
			Assert.That (result, Is.TypeOf (typeof (int [])));
			result.AssertEquals (1, 2, 3, 4, 5, 6, 7, 8, 9, 10);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ToDictionary_KeySelectorArg_KeySelectorYieldsNull_ThrowsArgumentNullException ()
		{
			var source = new [] { "eagle", "deer" };
			source.ToDictionary<string, string> (s => null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ToDictionary_KeySelectorArg_DuplicateKeys_ThrowsArgumentException ()
		{
			var source = new [] { "eagle", "deer", "cat", "dog" };
			source.ToDictionary (s => s.Length);
		}

		[Test]
		public void ToDictionary_KeySelectorArg_ValidArguments_KeySelectorIsUsedForKeysInDictionary ()
		{
			var source = Read ("1", "2", "3");
			var result = source.ToDictionary (s => int.Parse (s));
			int check = 1;
			foreach (var pair in result) {
				Assert.That (pair.Key, Is.EqualTo (check));
				Assert.That (pair.Value, Is.EqualTo (check.ToString ()));
				check++;
			}
			Assert.That (check, Is.EqualTo (4));
		}

		[Test]
		public void ToDictionary_KeySelectorArgElementSelectorArg_IntsFromOneToTen_KeySelectorAndElementSelectorAreUsedForDictionaryElements ()
		{
			var source = Read (1, 2, 3, 4, 5, 6, 7, 8, 9, 10);
			var result = source.ToDictionary (i => i.ToString (), i => Math.Sqrt (double.Parse (i.ToString ())));
			int check = 1;
			foreach (var pair in result) {
				Assert.That (pair.Key, Is.EqualTo (check.ToString ()));
				Assert.That (pair.Value, Is.EqualTo (Math.Sqrt (double.Parse (check.ToString ()))).Within (0.00001));
				check++;
			}
		}

		[Test]
		public void ToList_IntsFromOneToTen_ReturnsListOfIntsContainingAllElements ()
		{
			var source = Read (1, 2, 3, 4, 5, 6, 7, 8, 9, 10);
			var result = source.ToList ();
			Assert.That (result, Is.TypeOf (typeof (List<int>)));
			result.AssertEquals (1, 2, 3, 4, 5, 6, 7, 8, 9, 10);
		}

		[Test]
		public void ToLookup_KeySelectorArg_Strings_StringsByLength ()
		{
			var source = Read ("eagle", "dog", "cat", "bird", "camel");
			var lookup = source.ToLookup (s => s.Length);

			Assert.That (lookup.Count, Is.EqualTo (3));

			Assert.That (lookup.Contains (3), Is.True);
			lookup [3].AssertEquals ("dog", "cat");

			Assert.That (lookup.Contains (4), Is.True);
			lookup [4].AssertEquals ("bird");

			Assert.That (lookup.Contains (5), Is.True);
			lookup [5].AssertEquals ("eagle", "camel");
		}

		[Test]
		public void ToLookup_KeySelectorArgElementSelectorArg_Strings_ProjecetedStringsByLength ()
		{
			var source = Read ("eagle", "dog", "cat", "bird", "camel");
			var lookup = source.ToLookup (s => s.Length, str => str.ToUpperInvariant ());

			Assert.That (lookup.Count, Is.EqualTo (3));

			Assert.That (lookup.Contains (3), Is.True);
			var e = lookup [3].GetEnumerator ();
			e.MoveNext (); Assert.That (e.Current, Is.EqualTo ("DOG"));
			e.MoveNext (); Assert.That (e.Current, Is.EqualTo ("CAT"));
			Assert.That (e.MoveNext (), Is.False);

			Assert.That (lookup.Contains (4), Is.True);
			e = lookup [4].GetEnumerator ();
			e.MoveNext (); Assert.That (e.Current, Is.EqualTo ("BIRD"));
			Assert.That (e.MoveNext (), Is.False);

			Assert.That (lookup.Contains (5), Is.True);
			e = lookup [5].GetEnumerator ();
			e.MoveNext (); Assert.That (e.Current, Is.EqualTo ("EAGLE"));
			e.MoveNext (); Assert.That (e.Current, Is.EqualTo ("CAMEL"));
			Assert.That (e.MoveNext (), Is.False);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Union_SecondArg_PassNullAsArgument_ThrowsArgumentNullException ()
		{
			Read<object> ().Union (null);
		}

		[Test]
		public void Union_SecondArg_ValidIntArguments_NoDuplicatesAndInSourceOrder ()
		{
			var source = Read (5, 3, 9, 7, 5, 9, 3, 7);
			var argument = Read (8, 3, 6, 4, 4, 9, 1, 0);
			source.Union (argument).AssertEquals (5, 3, 9, 7, 8, 6, 4, 1, 0);
		}

		[Test]
		[Category ("ManagedCollator")]
		public void Union_SecondArgComparerArg_UpperCaseAndLowerCaseStrings_PassedComparerIsUsed ()
		{
			var source = Read ("A", "B", "C", "D", "E", "F");
			var argument = Read ("a", "b", "c", "d", "e", "f");
			source.Union (argument, StringComparer.CurrentCultureIgnoreCase).AssertEquals ("A", "B", "C", "D", "E", "F");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Where_NullPredicate_ThrowsArgumentNullException ()
		{
			Read<object> ().Where ((Func<object, bool>) null);
		}

		[Test]
		public void Where_IntegersWithEvensPredicate_YieldsEvenIntegers ()
		{
			var source = Read (1, 2, 3, 4, 5);
			source.Where (i => i % 2 == 0).AssertEquals (2, 4);
		}

		[Test]
		public void Where_StringsWithEvenIndexPredicate_YieldsElementsWithEvenIndex ()
		{
			var source = Read ("Camel", "Marlboro", "Parisienne", "Lucky Strike");
			source.Where ((s, i) => i % 2 == 0).AssertEquals ("Camel", "Parisienne");
		}

		[Test]
		public void AsEnumerable_NonNullSource_ReturnsSourceReference ()
		{
			var source = new object [0];
			Assert.That (Enumerable.AsEnumerable (source), Is.SameAs (source));
		}

		[Test]
		public void AsEnumerable_NullSource_ReturnsNull ()
		{
			Assert.That (Enumerable.AsEnumerable<object> (null), Is.Null);
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Zip_FirstSourceNull_ThrowsArgumentNullException ()
		{
			Enumerable.Zip<object, object, object> (null, new object [0], null);
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Zip_SecondSourceNull_ThrowsArgumentNullException ()
		{
			new object [0].Zip<object, object, object> (null, null);
		}		

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Zip_ResultNull_ThrowsArgumentNullException ()
		{
			new object [0].Zip<object, object, object> (new object [0], null);
		}		

		[Test]
		public void Zip ()
		{
			var a = new [] { 'a', 'b', 'c' };
			var b = new [] { 1, 2, 3 };
			a.Zip (b, (f, s) => f + s.ToString ()).AssertEquals ("a1", "b2", "c3");
			
			a = new [] { 'a' };
			b = new [] { 100, 200, 300 };
			a.Zip (b, (f, s) => f + s.ToString ()).AssertEquals ("a100");
		}


		private Reader<T> Read<T> (params T [] source)
		{
			Debug.Assert (source != null);

			var reader = new Reader<T> (source);

			//
			// If the calling test method is not expecting an exception
			// then check that the source enumerator will be disposed
			// by the time the test is torn.
			//
/*
			var disposed = false;
			var enumerated = false;
			reader.Disposed += delegate { disposed = true; };
			reader.Enumerated += delegate { enumerated = true; };
			AssertionHandler assertion = () => Assert.That (!enumerated || disposed, Is.True, "Enumerator not disposed.");
			tearDownAssertions = (AssertionHandler) Delegate.Combine (tearDownAssertions, assertion);
*/
			return reader;
		}
	}

	[Serializable]
	internal sealed class NonEnumerableList<T> : List<T>, IEnumerable<T> {
		public NonEnumerableList () { }

		public NonEnumerableList (IEnumerable<T> collection) :
			base (collection) { }

		// Re-implement GetEnumerator to be undefined.

		IEnumerator<T> IEnumerable<T>.GetEnumerator ()
		{
			throw new NotImplementedException ();
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return ((IEnumerable<T>) this).GetEnumerator ();
		}
	}

	[Serializable]
	internal sealed class NonEnumerableReadOnlyList<T> : ReadOnlyCollection<T>, IEnumerable<T> {
		public NonEnumerableReadOnlyList () : 
			this (new List<T>()) { }

		public NonEnumerableReadOnlyList (IList<T> collection) :
			base (collection) { }

		// Re-implement GetEnumerator to be undefined.

		IEnumerator<T> IEnumerable<T>.GetEnumerator ()
		{
			throw new NotImplementedException ();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<T>) this).GetEnumerator();
		}
	}

    internal sealed class Reader<T> : IEnumerable<T>, IEnumerator<T>
    {
        public event EventHandler Disposed;
        public event EventHandler Enumerated;

        private IEnumerable<T> source;
        private IEnumerator<T> cursor;

        public Reader(IEnumerable<T> values)
        {
            Debug.Assert(values != null);
            source = values;
        }

        private IEnumerator<T> Enumerator
        {
            get
            {
                if (cursor == null)
                    GetEnumerator();
                return this;
            }
        }

        public object EOF
        {
            get { return Enumerator.MoveNext(); }
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (source == null) throw new Exception("A LINQ Operator called GetEnumerator() twice.");
            cursor = source.GetEnumerator();
            source = null;

            var handler = Enumerated;
            if (handler != null)
                handler(this, EventArgs.Empty);

            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public T Read()
        {
            if (!Enumerator.MoveNext())
                throw new InvalidOperationException("No more elements in the source sequence.");
            return Enumerator.Current;
        }

        void IDisposable.Dispose()
        {
            source = null;
            var e = cursor;
            cursor = null;

            if (e != null)
            {
                e.Dispose();

                var handler = Disposed;
                if (handler != null)
                    handler(this, EventArgs.Empty);
            }
        }

        private IEnumerator<T> GetSourceEnumerator()
        {
            if (source != null && cursor == null)
                throw new InvalidOperationException(/* GetEnumerator not called yet */);
            if (source == null && cursor == null)
                throw new ObjectDisposedException(GetType().FullName);

            return cursor;
        }

        bool IEnumerator.MoveNext()
        {
            return GetSourceEnumerator().MoveNext();
        }

        void IEnumerator.Reset()
        {
            GetSourceEnumerator().Reset();
        }

        T IEnumerator<T>.Current
        {
            get { return GetSourceEnumerator().Current; }
        }

        object IEnumerator.Current
        {
            get { return ((IEnumerator<T>) this).Current; }
        }
    }

    internal static class ReaderTestExtensions
    {
        public static void AssertEnded<T>(this Reader<T> reader)
        {
            Debug.Assert(reader != null);

            Assert.That(reader.EOF, Is.False, "Too many elements in source.");
        }

        public static Reader<T> AssertNext<T>(this Reader<T> reader, Constraint constraint)
        {
            Debug.Assert(reader != null);
            Debug.Assert(constraint != null);

            Assert.That(reader.Read(), constraint);
            return reader;
        }
    }

	internal static class Tester {

		public static void AssertEquals<T> (this IEnumerable<T> actuals, params T [] expectations)
		{
			actuals.AssertThat (a => Is.EqualTo (a), expectations);
		}

		public static void AssertThat<T> (this IEnumerable<T> actuals, Func<T, Constraint> constrainer, params T [] expectations)
		{
			using (var e = actuals.GetEnumerator ()) {
				foreach (var expected in expectations) {
					e.MoveNext ();
					Assert.That (e.Current, constrainer (expected));
				}

				Assert.That (e.MoveNext (), Is.False);
			}
		}
	}
}
