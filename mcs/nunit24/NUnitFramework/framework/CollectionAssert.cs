// ****************************************************************
// This is free software licensed under the NUnit license. You
// may obtain a copy of the license as well as information regarding
// copyright ownership at http://nunit.org/?p=license&r=2.4.
// ****************************************************************

using System;
using System.Collections;
using System.ComponentModel;
using NUnit.Framework.Constraints;

namespace NUnit.Framework
{
	/// <summary>
	/// A set of Assert methods operationg on one or more collections
	/// </summary>
	public class CollectionAssert
	{
		#region Equals and ReferenceEquals

		/// <summary>
		/// The Equals method throws an AssertionException. This is done 
		/// to make sure there is no mistake by calling this function.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static new bool Equals(object a, object b)
		{
			throw new AssertionException("Assert.Equals should not be used for Assertions");
		}

		/// <summary>
		/// override the default ReferenceEquals to throw an AssertionException. This 
		/// implementation makes sure there is no mistake in calling this function 
		/// as part of Assert. 
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		public static new void ReferenceEquals(object a, object b)
		{
			throw new AssertionException("Assert.ReferenceEquals should not be used for Assertions");
		}

		#endregion
				
		#region AllItemsAreInstancesOfType
		/// <summary>
		/// Asserts that all items contained in collection are of the type specified by expectedType.
		/// </summary>
		/// <param name="collection">IEnumerable containing objects to be considered</param>
		/// <param name="expectedType">System.Type that all objects in collection must be instances of</param>
		public static void AllItemsAreInstancesOfType (IEnumerable collection, Type expectedType)
		{
			AllItemsAreInstancesOfType(collection, expectedType, string.Empty, null);
		}

		/// <summary>
		/// Asserts that all items contained in collection are of the type specified by expectedType.
		/// </summary>
		/// <param name="collection">IEnumerable containing objects to be considered</param>
		/// <param name="expectedType">System.Type that all objects in collection must be instances of</param>
		/// <param name="message">The message that will be displayed on failure</param>
		public static void AllItemsAreInstancesOfType (IEnumerable collection, Type expectedType, string message)
		{
			AllItemsAreInstancesOfType(collection, expectedType, message, null);
		}

		/// <summary>
		/// Asserts that all items contained in collection are of the type specified by expectedType.
		/// </summary>
		/// <param name="collection">IEnumerable containing objects to be considered</param>
		/// <param name="expectedType">System.Type that all objects in collection must be instances of</param>
		/// <param name="message">The message that will be displayed on failure</param>
		/// <param name="args">Arguments to be used in formatting the message</param>
		public static void AllItemsAreInstancesOfType (IEnumerable collection, Type expectedType, string message, params object[] args)
		{
            Assert.That(collection, new AllItemsConstraint(new InstanceOfTypeConstraint(expectedType)), message, args);
		}
		#endregion

		#region AllItemsAreNotNull

		/// <summary>
		/// Asserts that all items contained in collection are not equal to null.
		/// </summary>
		/// <param name="collection">IEnumerable containing objects to be considered</param>
		public static void AllItemsAreNotNull (IEnumerable collection) 
		{
			AllItemsAreNotNull(collection, string.Empty, null);
		}

		/// <summary>
		/// Asserts that all items contained in collection are not equal to null.
		/// </summary>
		/// <param name="collection">IEnumerable containing objects to be considered</param>
		/// <param name="message">The message that will be displayed on failure</param>
		public static void AllItemsAreNotNull (IEnumerable collection, string message) 
		{
			AllItemsAreNotNull(collection, message, null);
		}

		/// <summary>
		/// Asserts that all items contained in collection are not equal to null.
		/// </summary>
		/// <param name="collection">IEnumerable of objects to be considered</param>
		/// <param name="message">The message that will be displayed on failure</param>
		/// <param name="args">Arguments to be used in formatting the message</param>
		public static void AllItemsAreNotNull (IEnumerable collection, string message, params object[] args) 
		{
            Assert.That(collection, new AllItemsConstraint(new NotConstraint(new EqualConstraint(null))), message, args);
		}
		#endregion

		#region AllItemsAreUnique

		/// <summary>
		/// Ensures that every object contained in collection exists within the collection
		/// once and only once.
		/// </summary>
		/// <param name="collection">IEnumerable of objects to be considered</param>
		public static void AllItemsAreUnique (IEnumerable collection) 
		{
			AllItemsAreUnique(collection, string.Empty, null);
		}

		/// <summary>
		/// Ensures that every object contained in collection exists within the collection
		/// once and only once.
		/// </summary>
		/// <param name="collection">IEnumerable of objects to be considered</param>
		/// <param name="message">The message that will be displayed on failure</param>
		public static void AllItemsAreUnique (IEnumerable collection, string message) 
		{
			AllItemsAreUnique(collection, message, null);
		}
		
		/// <summary>
		/// Ensures that every object contained in collection exists within the collection
		/// once and only once.
		/// </summary>
		/// <param name="collection">IEnumerable of objects to be considered</param>
		/// <param name="message">The message that will be displayed on failure</param>
		/// <param name="args">Arguments to be used in formatting the message</param>
		public static void AllItemsAreUnique (IEnumerable collection, string message, params object[] args) 
		{
            Assert.That(collection, new UniqueItemsConstraint(), message, args);
		}
		#endregion

		#region AreEqual

		/// <summary>
		/// Asserts that expected and actual are exactly equal.  The collections must have the same count, 
		/// and contain the exact same objects in the same order.
		/// </summary>
		/// <param name="expected">The first IEnumerable of objects to be considered</param>
		/// <param name="actual">The second IEnumerable of objects to be considered</param>
		public static void AreEqual (IEnumerable expected, IEnumerable actual) 
		{
			//AreEqual(expected, actual, null, string.Empty, null);
            Assert.That(actual, new EqualConstraint(expected));
		}

		/// <summary>
		/// Asserts that expected and actual are exactly equal.  The collections must have the same count, 
		/// and contain the exact same objects in the same order.
		/// If comparer is not null then it will be used to compare the objects.
		/// </summary>
		/// <param name="expected">The first IEnumerable of objects to be considered</param>
		/// <param name="actual">The second IEnumerable of objects to be considered</param>
		/// <param name="comparer">The IComparer to use in comparing objects from each IEnumerable</param>
		public static void AreEqual (IEnumerable expected, IEnumerable actual, IComparer comparer) 
		{
			AreEqual(expected, actual, comparer, string.Empty, null);
		}

		/// <summary>
		/// Asserts that expected and actual are exactly equal.  The collections must have the same count, 
		/// and contain the exact same objects in the same order.
		/// </summary>
		/// <param name="expected">The first IEnumerable of objects to be considered</param>
		/// <param name="actual">The second IEnumerable of objects to be considered</param>
		/// <param name="message">The message that will be displayed on failure</param>
		public static void AreEqual (IEnumerable expected, IEnumerable actual, string message) 
		{
			//AreEqual(expected, actual, null, message, null);
            Assert.That(actual, new EqualConstraint(expected), message);
		}

		/// <summary>
		/// Asserts that expected and actual are exactly equal.  The collections must have the same count, 
		/// and contain the exact same objects in the same order.
		/// If comparer is not null then it will be used to compare the objects.
		/// </summary>
		/// <param name="expected">The first IEnumerable of objects to be considered</param>
		/// <param name="actual">The second IEnumerable of objects to be considered</param>
		/// <param name="comparer">The IComparer to use in comparing objects from each IEnumerable</param>
		/// <param name="message">The message that will be displayed on failure</param>
		public static void AreEqual (IEnumerable expected, IEnumerable actual, IComparer comparer, string message) 
		{
			AreEqual(expected, actual, comparer, message, null);
		}

		/// <summary>
		/// Asserts that expected and actual are exactly equal.  The collections must have the same count, 
		/// and contain the exact same objects in the same order.
		/// </summary>
		/// <param name="expected">The first IEnumerable of objects to be considered</param>
		/// <param name="actual">The second IEnumerable of objects to be considered</param>
		/// <param name="message">The message that will be displayed on failure</param>
		/// <param name="args">Arguments to be used in formatting the message</param>
		public static void AreEqual (IEnumerable expected, IEnumerable actual, string message, params object[] args) 
		{
			//AreEqual(expected, actual, null, message, args);
            Assert.That(actual, new EqualConstraint(expected), message, args);
		}

		/// <summary>
		/// Asserts that expected and actual are exactly equal.  The collections must have the same count, 
		/// and contain the exact same objects in the same order.
		/// If comparer is not null then it will be used to compare the objects.
		/// </summary>
		/// <param name="expected">The first IEnumerable of objects to be considered</param>
		/// <param name="actual">The second IEnumerable of objects to be considered</param>
		/// <param name="comparer">The IComparer to use in comparing objects from each IEnumerable</param>
		/// <param name="message">The message that will be displayed on failure</param>
		/// <param name="args">Arguments to be used in formatting the message</param>
		public static void AreEqual (IEnumerable expected, IEnumerable actual, IComparer comparer, string message, params object[] args) 
		{
            Assert.That(actual, new EqualConstraint(expected).Comparer(comparer), message, args);
		}
		#endregion

		#region AreEquivalent

		/// <summary>
		/// Asserts that expected and actual are equivalent, containing the same objects but the match may be in any order.
		/// </summary>
		/// <param name="expected">The first IEnumerable of objects to be considered</param>
		/// <param name="actual">The second IEnumerable of objects to be considered</param>
		public static void AreEquivalent (IEnumerable expected, IEnumerable actual) 
		{
			AreEquivalent(expected, actual, string.Empty, null);
		}

		/// <summary>
		/// Asserts that expected and actual are equivalent, containing the same objects but the match may be in any order.
		/// </summary>
		/// <param name="expected">The first IEnumerable of objects to be considered</param>
		/// <param name="actual">The second IEnumerable of objects to be considered</param>
		/// <param name="message">The message that will be displayed on failure</param>
		public static void AreEquivalent (IEnumerable expected, IEnumerable actual, string message) 
		{
			AreEquivalent(expected, actual, message, null);
		}

		/// <summary>
		/// Asserts that expected and actual are equivalent, containing the same objects but the match may be in any order.
		/// </summary>
		/// <param name="expected">The first IEnumerable of objects to be considered</param>
		/// <param name="actual">The second IEnumerable of objects to be considered</param>
		/// <param name="message">The message that will be displayed on failure</param>
		/// <param name="args">Arguments to be used in formatting the message</param>
		public static void AreEquivalent (IEnumerable expected, IEnumerable actual, string message, params object[] args) 
		{
            Assert.That(actual, new CollectionEquivalentConstraint(expected), message, args);
		}
		#endregion

		#region AreNotEqual

		/// <summary>
		/// Asserts that expected and actual are not exactly equal.
		/// </summary>
		/// <param name="expected">The first IEnumerable of objects to be considered</param>
		/// <param name="actual">The second IEnumerable of objects to be considered</param>
		public static void AreNotEqual (IEnumerable expected, IEnumerable actual)
		{
            Assert.That(actual, new NotConstraint(new EqualConstraint(expected)));
		}

		/// <summary>
		/// Asserts that expected and actual are not exactly equal.
		/// If comparer is not null then it will be used to compare the objects.
		/// </summary>
		/// <param name="expected">The first IEnumerable of objects to be considered</param>
		/// <param name="actual">The second IEnumerable of objects to be considered</param>
		/// <param name="comparer">The IComparer to use in comparing objects from each IEnumerable</param>
		public static void AreNotEqual (IEnumerable expected, IEnumerable actual, IComparer comparer)
		{
			AreNotEqual(expected, actual, comparer, string.Empty, null);
		}

		/// <summary>
		/// Asserts that expected and actual are not exactly equal.
		/// </summary>
		/// <param name="expected">The first IEnumerable of objects to be considered</param>
		/// <param name="actual">The second IEnumerable of objects to be considered</param>
		/// <param name="message">The message that will be displayed on failure</param>
		public static void AreNotEqual (IEnumerable expected, IEnumerable actual, string message)
		{
			//AreNotEqual(expected, actual, null, message, null);
			//Assert.AreNotEqual( expected, actual, message );
            Assert.That(actual, new NotConstraint(new EqualConstraint(expected)), message);
		}

		/// <summary>
		/// Asserts that expected and actual are not exactly equal.
		/// If comparer is not null then it will be used to compare the objects.
		/// </summary>
		/// <param name="expected">The first IEnumerable of objects to be considered</param>
		/// <param name="actual">The second IEnumerable of objects to be considered</param>
		/// <param name="comparer">The IComparer to use in comparing objects from each IEnumerable</param>
		/// <param name="message">The message that will be displayed on failure</param>
		public static void AreNotEqual (IEnumerable expected, IEnumerable actual, IComparer comparer, string message)
		{
			AreNotEqual(expected, actual, comparer, message, null);
		}

		/// <summary>
		/// Asserts that expected and actual are not exactly equal.
		/// </summary>
		/// <param name="expected">The first IEnumerable of objects to be considered</param>
		/// <param name="actual">The second IEnumerable of objects to be considered</param>
		/// <param name="message">The message that will be displayed on failure</param>
		/// <param name="args">Arguments to be used in formatting the message</param>
		public static void AreNotEqual (IEnumerable expected, IEnumerable actual, string message, params object[] args) 
		{
			//AreNotEqual(expected, actual, null, message, args);
			//Assert.AreNotEqual( expected, actual, message, args );
            Assert.That(actual, new NotConstraint(new EqualConstraint(expected)), message, args);
		}

		/// <summary>
		/// Asserts that expected and actual are not exactly equal.
		/// If comparer is not null then it will be used to compare the objects.
		/// </summary>
		/// <param name="expected">The first IEnumerable of objects to be considered</param>
		/// <param name="actual">The second IEnumerable of objects to be considered</param>
		/// <param name="comparer">The IComparer to use in comparing objects from each IEnumerable</param>
		/// <param name="message">The message that will be displayed on failure</param>
		/// <param name="args">Arguments to be used in formatting the message</param>
		public static void AreNotEqual (IEnumerable expected, IEnumerable actual, IComparer comparer, string message, params object[] args)
		{
			Assert.That(actual, new NotConstraint(new EqualConstraint(expected).Comparer(comparer)), message, args);
		}
		#endregion

		#region AreNotEquivalent

		/// <summary>
		/// Asserts that expected and actual are not equivalent.
		/// </summary>
		/// <param name="expected">The first IEnumerable of objects to be considered</param>
		/// <param name="actual">The second IEnumerable of objects to be considered</param>
		public static void AreNotEquivalent (IEnumerable expected, IEnumerable actual)
		{
			AreNotEquivalent(expected, actual, string.Empty, null);
		}

		/// <summary>
		/// Asserts that expected and actual are not equivalent.
		/// </summary>
		/// <param name="expected">The first IEnumerable of objects to be considered</param>
		/// <param name="actual">The second IEnumerable of objects to be considered</param>
		/// <param name="message">The message that will be displayed on failure</param>
		public static void AreNotEquivalent (IEnumerable expected, IEnumerable actual, string message)
		{
			AreNotEquivalent(expected, actual, message, null);
		}

		/// <summary>
		/// Asserts that expected and actual are not equivalent.
		/// </summary>
		/// <param name="expected">The first IEnumerable of objects to be considered</param>
		/// <param name="actual">The second IEnumerable of objects to be considered</param>
		/// <param name="message">The message that will be displayed on failure</param>
		/// <param name="args">Arguments to be used in formatting the message</param>
		public static void AreNotEquivalent (IEnumerable expected, IEnumerable actual, string message, params object[] args)
		{
            Assert.That(actual, new NotConstraint(new CollectionEquivalentConstraint(expected)), message, args);
		}
		#endregion

		#region Contains
		/// <summary>
		/// Asserts that collection contains actual as an item.
		/// </summary>
		/// <param name="collection">IEnumerable of objects to be considered</param>
		/// <param name="actual">Object to be found within collection</param>
		public static void Contains (IEnumerable collection, Object actual)
		{
			Contains(collection, actual, string.Empty, null);
		}

		/// <summary>
		/// Asserts that collection contains actual as an item.
		/// </summary>
		/// <param name="collection">IEnumerable of objects to be considered</param>
		/// <param name="actual">Object to be found within collection</param>
		/// <param name="message">The message that will be displayed on failure</param>
		public static void Contains (IEnumerable collection, Object actual, string message)
		{
			Contains(collection, actual, message, null);
		}

		/// <summary>
		/// Asserts that collection contains actual as an item.
		/// </summary>
		/// <param name="collection">IEnumerable of objects to be considered</param>
		/// <param name="actual">Object to be found within collection</param>
		/// <param name="message">The message that will be displayed on failure</param>
		/// <param name="args">Arguments to be used in formatting the message</param>
		public static void Contains (IEnumerable collection, Object actual, string message, params object[] args)
		{
            Assert.That(collection, new CollectionContainsConstraint(actual), message, args);
		}
		#endregion

		#region DoesNotContain

		/// <summary>
		/// Asserts that collection does not contain actual as an item.
		/// </summary>
		/// <param name="collection">IEnumerable of objects to be considered</param>
		/// <param name="actual">Object that cannot exist within collection</param>
		public static void DoesNotContain (IEnumerable collection, Object actual)
		{
			DoesNotContain(collection, actual, string.Empty, null);
		}

		/// <summary>
		/// Asserts that collection does not contain actual as an item.
		/// </summary>
		/// <param name="collection">IEnumerable of objects to be considered</param>
		/// <param name="actual">Object that cannot exist within collection</param>
		/// <param name="message">The message that will be displayed on failure</param>
		public static void DoesNotContain (IEnumerable collection, Object actual, string message)
		{
			DoesNotContain(collection, actual, message, null);
		}

		/// <summary>
		/// Asserts that collection does not contain actual as an item.
		/// </summary>
		/// <param name="collection">IEnumerable of objects to be considered</param>
		/// <param name="actual">Object that cannot exist within collection</param>
		/// <param name="message">The message that will be displayed on failure</param>
		/// <param name="args">Arguments to be used in formatting the message</param>
		public static void DoesNotContain (IEnumerable collection, Object actual, string message, params object[] args)
		{
            Assert.That(collection, new NotConstraint( new CollectionContainsConstraint( actual ) ), message, args);
		}
		#endregion

		#region IsNotSubsetOf

		/// <summary>
		/// Asserts that superset is not a subject of subset.
		/// </summary>
		/// <param name="subset">The IEnumerable superset to be considered</param>
		/// <param name="superset">The IEnumerable subset to be considered</param>
		public static void IsNotSubsetOf (IEnumerable subset, IEnumerable superset)
		{
			IsNotSubsetOf(subset, superset, string.Empty, null);
		}

		/// <summary>
		/// Asserts that superset is not a subject of subset.
		/// </summary>
		/// <param name="subset">The IEnumerable superset to be considered</param>
		/// <param name="superset">The IEnumerable subset to be considered</param>
		/// <param name="message">The message that will be displayed on failure</param>
		public static void IsNotSubsetOf (IEnumerable subset, IEnumerable superset, string message)
		{
			IsNotSubsetOf(subset, superset, message, null);
		}

		/// <summary>
		/// Asserts that superset is not a subject of subset.
		/// </summary>
		/// <param name="subset">The IEnumerable superset to be considered</param>
		/// <param name="superset">The IEnumerable subset to be considered</param>
		/// <param name="message">The message that will be displayed on failure</param>
		/// <param name="args">Arguments to be used in formatting the message</param>
		public static void IsNotSubsetOf (IEnumerable subset, IEnumerable superset, string message, params object[] args)
		{
            Assert.That(subset, new NotConstraint(new CollectionSubsetConstraint(superset)), message, args);
		}
		#endregion

		#region IsSubsetOf

		/// <summary>
		/// Asserts that superset is a subset of subset.
		/// </summary>
		/// <param name="subset">The IEnumerable superset to be considered</param>
		/// <param name="superset">The IEnumerable subset to be considered</param>
		public static void IsSubsetOf (IEnumerable subset, IEnumerable superset)
		{
			IsSubsetOf(subset, superset, string.Empty, null);
		}

		/// <summary>
		/// Asserts that superset is a subset of subset.
		/// </summary>
		/// <param name="subset">The IEnumerable superset to be considered</param>
		/// <param name="superset">The IEnumerable subset to be considered</param>
		/// <param name="message">The message that will be displayed on failure</param>
		public static void IsSubsetOf (IEnumerable subset, IEnumerable superset, string message)
		{
			IsSubsetOf(subset, superset, message, null);
		}

		/// <summary>
		/// Asserts that superset is a subset of subset.
		/// </summary>
		/// <param name="subset">The IEnumerable superset to be considered</param>
		/// <param name="superset">The IEnumerable subset to be considered</param>
		/// <param name="message">The message that will be displayed on failure</param>
		/// <param name="args">Arguments to be used in formatting the message</param>
		public static void IsSubsetOf (IEnumerable subset, IEnumerable superset, string message, params object[] args)
		{
            Assert.That(subset, new CollectionSubsetConstraint(superset), message, args);
		}
		#endregion

        #region IsEmpty
        /// <summary>
        /// Assert that an array, list or other collection is empty
        /// </summary>
        /// <param name="collection">An array, list or other collection implementing IEnumerable</param>
        /// <param name="message">The message to be displayed on failure</param>
        /// <param name="args">Arguments to be used in formatting the message</param>
        public static void IsEmpty(IEnumerable collection, string message, params object[] args)
        {
            Assert.That(collection, new EmptyConstraint(), message, args);
        }

        /// <summary>
        /// Assert that an array, list or other collection is empty
        /// </summary>
        /// <param name="collection">An array, list or other collection implementing IEnumerable</param>
        /// <param name="message">The message to be displayed on failure</param>
        public static void IsEmpty(IEnumerable collection, string message)
        {
            IsEmpty(collection, message, null);
        }

        /// <summary>
        /// Assert that an array,list or other collection is empty
        /// </summary>
        /// <param name="collection">An array, list or other collection implementing IEnumerable</param>
        public static void IsEmpty(IEnumerable collection)
        {
            IsEmpty(collection, string.Empty, null);
        }
        #endregion

        #region IsNotEmpty
        /// <summary>
        /// Assert that an array, list or other collection is empty
        /// </summary>
        /// <param name="collection">An array, list or other collection implementing IEnumerable</param>
        /// <param name="message">The message to be displayed on failure</param>
        /// <param name="args">Arguments to be used in formatting the message</param>
        public static void IsNotEmpty(IEnumerable collection, string message, params object[] args)
        {
            Assert.That(collection, new NotConstraint(new EmptyConstraint()), message, args);
        }

        /// <summary>
        /// Assert that an array, list or other collection is empty
        /// </summary>
        /// <param name="collection">An array, list or other collection implementing IEnumerable</param>
        /// <param name="message">The message to be displayed on failure</param>
        public static void IsNotEmpty(IEnumerable collection, string message)
        {
            IsNotEmpty(collection, message, null);
        }

        /// <summary>
        /// Assert that an array,list or other collection is empty
        /// </summary>
        /// <param name="collection">An array, list or other collection implementing IEnumerable</param>
        public static void IsNotEmpty(IEnumerable collection)
        {
            IsNotEmpty(collection, string.Empty, null);
        }
        #endregion
    }
}


