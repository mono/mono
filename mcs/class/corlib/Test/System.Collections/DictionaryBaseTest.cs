//
// System.Collections.DictionaryBase
// Test suite for System.Collections.DictionaryBase
//
// Authors:
//	Carlos Alberto Barcenilla (barce@frlp.utn.edu.ar)
//

using System;
using System.Collections;
using NUnit.Framework;

namespace MonoTests.System.Collections 
{

	[TestFixture]
	public class DictionaryBaseTest: Assertion
	{
		static void Main(string[] args)
		{
		}

		public class ConcreteDictionary : DictionaryBase
		{
			public bool onInsertFired;
			public bool onInsertCompleteFired;
			public bool onValidateFired;
			public bool onRemoveFired;
			public bool onRemoveCompleteFired;
			public bool onClearFired;
			public bool onClearCompleteFired;
			public bool onSetFired;
			public bool onSetCompleteFired;
			public bool onGetFired;

			public bool onInsertMustThrowException;
			public bool onInsertCompleteMustThrowException;
			public bool onValidateMustThrowException;
			public bool onRemoveMustThrowException;
			public bool onRemoveCompleteMustThrowException;
			public bool onClearMustThrowException;
			public bool onClearCompleteMustThrowException;
			public bool onSetMustThrowException;
			public bool onSetCompleteMustThrowException;
			public bool onGetMustThrowException;

			public ConcreteDictionary() 
			{
			}

			public ConcreteDictionary(int i) 
			{
				for (int j = 0; j < i; j++) 
				{
					((IDictionary) this).Add(j, j*2);
				}

				ClearFlags();
			}

            public IDictionary BaseDictionary 
			{
				get { return this.Dictionary; }
			}

			public void ClearFlags() 
			{
				onInsertFired = false;
				onInsertCompleteFired = false;
				onValidateFired = false;
				onRemoveFired = false;
				onRemoveCompleteFired = false;
				onClearFired = false;
				onClearCompleteFired = false;
				onSetFired = false;
				onSetCompleteFired = false;
				onGetFired = false;
			}

			protected override void OnValidate(object key, object value)
			{
				onValidateFired = true;
				if (onValidateMustThrowException)
					throw new Exception();
				base.OnValidate (key, value);
			}

			protected override void OnInsert(object key, object value)
			{
				onInsertFired = true;
				if (onInsertMustThrowException)
					throw new Exception();
				base.OnInsert (key, value);
			}

			protected override void OnInsertComplete(object key, object value)
			{
				onInsertCompleteFired = true;
				if (onInsertCompleteMustThrowException)
					throw new Exception();
				base.OnInsertComplete (key, value);
			}

			protected override void OnRemove(object key, object value)
			{
				onRemoveFired = true;
				if (onRemoveMustThrowException)
					throw new Exception();
				base.OnRemove (key, value);
			}

			protected override void OnRemoveComplete(object key, object value)
			{
				onRemoveCompleteFired = true;
				if (onRemoveCompleteMustThrowException)
					throw new Exception();
				base.OnRemoveComplete (key, value);
			}


			protected override void OnClear()
			{
				onClearFired = true;
				if (onClearMustThrowException)
					throw new Exception();
				base.OnClear ();
			}

			protected override void OnClearComplete()
			{
				onClearCompleteFired = true;
				if (onClearCompleteMustThrowException)
					throw new Exception();
				base.OnClearComplete ();
			}

			protected override object OnGet(object key, object currentValue)
			{
				onGetFired = true;
				if (onGetMustThrowException)
					throw new Exception();
				return base.OnGet (key, currentValue);
			}

			protected override void OnSet(object key, object oldValue, object newValue)
			{
				onSetFired = true;
				if (onSetMustThrowException)
					throw new Exception();
				base.OnSet (key, oldValue, newValue);
			}

			protected override void OnSetComplete(object key, object oldValue, object newValue)
			{
				onSetCompleteFired = true;
				if (onSetCompleteMustThrowException)
					throw new Exception();
				base.OnSetComplete (key, oldValue, newValue);
			}
		}

		[Test]
		public void Add() 
		{
			ConcreteDictionary myDictionary = new ConcreteDictionary(10);
			myDictionary.BaseDictionary.Add(100, 1);

			Assert("OnValidate must be fired", myDictionary.onValidateFired);
			Assert("OnInsert must be fired", myDictionary.onInsertFired);
			Assert("OnInsertComplete must be fired", myDictionary.onInsertCompleteFired);
			AssertEquals("Count", 11, myDictionary.Count);
			AssertEquals(1, myDictionary.BaseDictionary[100]);
		}

		[Test]
		public void AddOnValidateExcept() 
		{
			bool exceptionThrown = false;

			ConcreteDictionary myDictionary = new ConcreteDictionary(30);
			myDictionary.onValidateMustThrowException = true;

			try {
				myDictionary.BaseDictionary.Add(111,222);
			} catch {
				exceptionThrown = true;
			} finally {
				Assert("Exception must be thrown", exceptionThrown);
				Assert("OnValidate must be fired", myDictionary.onValidateFired);
				Assert("OnInsert must not be fired", !myDictionary.onInsertFired);
				Assert("OnInsertComplete must not be fired", !myDictionary.onInsertCompleteFired);
				AssertEquals("Count", 30, myDictionary.Count);
			}

		}

		[Test]
		public void AddOnInsertExcept() 
		{
			bool exceptionThrown = false;

			ConcreteDictionary myDictionary = new ConcreteDictionary(30);
			myDictionary.onInsertMustThrowException = true;

			try {
				myDictionary.BaseDictionary.Add(666,222);
			} catch {
				exceptionThrown = true;
			} finally {
				Assert("Exception must be thrown", exceptionThrown);
				Assert("OnValidate must be fired", myDictionary.onValidateFired);
				Assert("OnInsert must be fired", myDictionary.onInsertFired);
				Assert("OnInsertComplete must not be fired", !myDictionary.onInsertCompleteFired);
				AssertEquals("Count", 30, myDictionary.Count);
			}

		}

		[Test]
		public void AddOnInsertCompleteExcept() 
		{
			bool exceptionThrown = false;

			ConcreteDictionary myDictionary = new ConcreteDictionary(5);
			myDictionary.onInsertCompleteMustThrowException = true;

			try {
				myDictionary.BaseDictionary.Add(888,999);
			} catch {
				exceptionThrown = true;
			} finally {
				Assert("Exception must be thrown", exceptionThrown);
				Assert("OnValidate must be fired", myDictionary.onValidateFired);
				Assert("OnInsert must be fired", myDictionary.onInsertFired);
				Assert("OnInsertComplete must be fired", myDictionary.onInsertCompleteFired);
				AssertEquals("Count", 5, myDictionary.Count);
			}

		}

		[Test]
		public void AddNullKey() 
		{
			bool exceptionThrown = false;

			ConcreteDictionary myDictionary = new ConcreteDictionary();
			try {
				myDictionary.BaseDictionary.Add(null, 11);
			} catch (ArgumentNullException) {
				exceptionThrown = true;
			} finally {
				Assert("OnValidate must be fired", myDictionary.onValidateFired);
				Assert("OnInsert must be fired", myDictionary.onInsertFired);
				Assert("OnInsertComplete must not be fired", !myDictionary.onInsertCompleteFired);
				Assert("ArgumentNullException must be thrown", exceptionThrown);
			}
		}

		[Test]
		public void Clear() 
		{
			ConcreteDictionary myDictionary = new ConcreteDictionary(30);
			myDictionary.Clear();

			Assert("OnClear must be fired", myDictionary.onClearFired);
			Assert("OnClearComplete must be fired", myDictionary.onClearCompleteFired);
			AssertEquals("Count", 0, myDictionary.Count);
		}

		[Test]
		public void ClearOnClearExcept() 
		{
			bool exceptionThrown = false;

			ConcreteDictionary myDictionary = new ConcreteDictionary(30);
			myDictionary.onClearMustThrowException = true;

			try {
				myDictionary.Clear();
			} catch {
				exceptionThrown = true;
			} finally {
				Assert("Exception must be thrown", exceptionThrown);
				Assert("OnClear must be fired", myDictionary.onClearFired);
				Assert("OnClearComplete must not be fired", !myDictionary.onClearCompleteFired);
				AssertEquals("Count", 30, myDictionary.Count);
			}

		}

		[Test]
		public void ClearOnClearCompleteExcept() 
		{
			bool exceptionThrown = false;

			ConcreteDictionary myDictionary = new ConcreteDictionary(30);
			myDictionary.onClearCompleteMustThrowException = true;

			try {
				myDictionary.Clear();
			} catch {
				exceptionThrown = true;
			} finally {
				Assert("Exception must be thrown", exceptionThrown);
				Assert("OnClear must be fired", myDictionary.onClearFired);
				Assert("OnClearComplete must be fired", myDictionary.onClearCompleteFired);
				AssertEquals("Count", 0, myDictionary.Count);
			}

		}

		[Test]
		public void Count() 
		{
			ConcreteDictionary myDictionary = new ConcreteDictionary(19);
			AssertEquals(19, myDictionary.Count);
		}

		[Test]
		public void Remove() 
		{
			ConcreteDictionary myDictionary = new ConcreteDictionary(8);
			myDictionary.BaseDictionary.Remove(5);

			Assert("OnValidate must be fired", myDictionary.onValidateFired);
			Assert("OnRemove must be fired", myDictionary.onRemoveFired);
			Assert("OnRemoveComplete must be fired", myDictionary.onRemoveCompleteFired);
			AssertEquals("Count", 7, myDictionary.Count);
			AssertEquals(null, myDictionary.BaseDictionary[5]);
		}

		[Test]
		public void RemoveOnValidateExcept() 
		{
			bool exceptionThrown = false;

			ConcreteDictionary myDictionary = new ConcreteDictionary(28);
			myDictionary.onValidateMustThrowException = true;

			try {
				myDictionary.BaseDictionary.Remove(11);
			} catch {
				exceptionThrown = true;
			} finally {
				Assert("Exception must be thrown in this test", exceptionThrown);
				Assert("OnValidate must be fired", myDictionary.onValidateFired);
				Assert("OnRemove must not be fired", !myDictionary.onRemoveFired);
				Assert("OnRemoveComplete must not be fired", !myDictionary.onRemoveCompleteFired);
				AssertEquals("Count", 28, myDictionary.Count);
				AssertEquals(22, myDictionary.BaseDictionary[11]);
			}

		}

		[Test]
		public void RemoveOnRemoveExcept() 
		{
			bool exceptionThrown = false;

			ConcreteDictionary myDictionary = new ConcreteDictionary(28);
			myDictionary.onRemoveMustThrowException = true;

			try {
				myDictionary.BaseDictionary.Remove(11);
			} catch {
				exceptionThrown = true;
			} finally {
				Assert("Exception must be thrown", exceptionThrown);
				Assert("OnValidate must be fired", myDictionary.onValidateFired);
				Assert("OnRemove must be fired", myDictionary.onRemoveFired);
				Assert("OnRemoveComplete must not be fired", !myDictionary.onRemoveCompleteFired);
				AssertEquals("Count", 28, myDictionary.Count);
				AssertEquals(22, myDictionary.BaseDictionary[11]);
			}

		}

		[Test]
		public void RemoveOnRemoveCompleteExcept() 
		{
			bool exceptionThrown = false;

			ConcreteDictionary myDictionary = new ConcreteDictionary(28);
			myDictionary.onRemoveCompleteMustThrowException = true;

			try 
			{
				myDictionary.BaseDictionary.Remove(11);
			} 
			catch 
			{
				exceptionThrown = true;
			} 
			finally 
			{
				Assert("Exception must be thrown", exceptionThrown);
				Assert("OnValidate must be fired", myDictionary.onValidateFired);
				Assert("OnRemove must be fired", myDictionary.onRemoveFired);
				Assert("OnRemoveComplete must be fired", myDictionary.onRemoveCompleteFired);
				AssertEquals("Count", 27, myDictionary.Count);
				AssertEquals(null, myDictionary.BaseDictionary[11]);
			}

		}

		[Test]
		public void RemoveKeyNotInDictionary() 
		{
		
			ConcreteDictionary myDictionary = new ConcreteDictionary(28);

			myDictionary.BaseDictionary.Remove(80);
			Assert("OnValidate must be fired", myDictionary.onValidateFired);
			Assert("OnRemove must be fired", myDictionary.onRemoveFired);
			Assert("OnRemoveComplete must be fired", myDictionary.onRemoveCompleteFired);
                }

		[Test]
		public void Items() 
		{
			ConcreteDictionary myDictionary = new ConcreteDictionary(19);
			for (int i = 0; i < 19; i++) 
			{
				AssertEquals(i*2, (int) myDictionary.BaseDictionary[i]);
			}
		}

		[Test]
		public void Contains() 
		{
			ConcreteDictionary myDictionary = new ConcreteDictionary(14);
			for (int i = 0; i < 14; i++) 
			{
				Assert("Must contain " + i, myDictionary.BaseDictionary.Contains(i));
			}
			for (int i = 14; i < 34; i++) 
			{
				Assert("Must not contain " + i, !myDictionary.BaseDictionary.Contains(i));
			}
		}

		[Test]
		public void GetEnumerator() 
		{
			ConcreteDictionary myDictionary = new ConcreteDictionary(4);
			
			AssertNotNull(myDictionary.GetEnumerator());
		}

		[Test]
		public void Keys() 
		{
			ConcreteDictionary myDictionary = new ConcreteDictionary(5);
			ICollection keys = myDictionary.BaseDictionary.Keys;

			int total = 0;
			foreach (int i in keys) 
				total += i;
			AssertEquals(10, total);
			AssertEquals(5, keys.Count);
		}

		[Test]
		public void Values() 
		{
			ConcreteDictionary myDictionary = new ConcreteDictionary(5);
			ICollection values = myDictionary.BaseDictionary.Values;

			int total = 0;
			foreach (int i in values) 
				total += i;
			AssertEquals(20, total);
			AssertEquals(5, values.Count);
		}

		[Test]
		public void Get() 
		{
			ConcreteDictionary myDictionary = new ConcreteDictionary(18);
			int v = (int) myDictionary.BaseDictionary[10];

			Assert("OnGet must be fired", myDictionary.onGetFired);
			AssertEquals(v, 20);
		}

		[Test]
		public void GetOnGetExcept() 
		{
			bool exceptionThrown = false;

			ConcreteDictionary myDictionary = new ConcreteDictionary(18);
			myDictionary.onGetMustThrowException = true;

			try {
				int v = (int) myDictionary.BaseDictionary[10];
			} catch {
				exceptionThrown = true;
			} finally {
				Assert("Exception must be thrown", exceptionThrown);
				Assert("OnGet must be fired", myDictionary.onGetFired);
			}
		}

		[Test]
		public void GetNoKey() 
		{
			bool exceptionThrown = false;

			ConcreteDictionary myDictionary = new ConcreteDictionary(18);
			AssertNull(myDictionary.BaseDictionary[100]);
		}

		[Test]
		public void Set() 
		{
			ConcreteDictionary myDictionary = new ConcreteDictionary(18);
			myDictionary.BaseDictionary[10] = 50;

			Assert("OnValidate must be fired", myDictionary.onValidateFired);
			Assert("OnSet must be fired", myDictionary.onSetFired);
			Assert("OnSetComplete must be fired", myDictionary.onSetCompleteFired);
			AssertEquals(50, myDictionary.BaseDictionary[10]);
		}

		[Test]
		public void SetNewKey() 
		{
			ConcreteDictionary myDictionary = new ConcreteDictionary(18);
			myDictionary.BaseDictionary[111] = 222;

			Assert("OnValidate must be fired", myDictionary.onValidateFired);
			Assert("OnSet must be fired", myDictionary.onSetFired);
			Assert("OnSetComplete must be fired", myDictionary.onSetCompleteFired);
			Assert("OnInsert must not be fired", !myDictionary.onInsertFired);
			Assert("OnInsertComplete must not be fired", !myDictionary.onInsertCompleteFired);
			AssertEquals(222, myDictionary.BaseDictionary[111]);
			AssertEquals(19, myDictionary.Count);
		}

		[Test]
		public void SetOnValidateExcept() 
		{
			bool exceptionThrown = false;

			ConcreteDictionary myDictionary = new ConcreteDictionary(18);
			myDictionary.onValidateMustThrowException = true;
			
			try {
				myDictionary.BaseDictionary[10] = 50;
			} catch {
				exceptionThrown = true;
			} finally {
				Assert("Exception must be thrown", exceptionThrown);
				Assert("OnValidate must be fired", myDictionary.onValidateFired);
				Assert("OnSet must not be fired", !myDictionary.onSetFired);
				Assert("OnSetComplete not must be fired", !myDictionary.onSetCompleteFired);
				AssertEquals(20, myDictionary.BaseDictionary[10]);
			}
		}

		[Test]
		public void SetOnSetExcept() 
		{
			bool exceptionThrown = false;

			ConcreteDictionary myDictionary = new ConcreteDictionary(18);
			myDictionary.onSetMustThrowException = true;
			
			try {
				myDictionary.BaseDictionary[10] = 50;
			} catch {
				exceptionThrown = true;
			} finally {
				Assert("Exception must be thrown", exceptionThrown);
				Assert("OnValidate must be fired", myDictionary.onValidateFired);
				Assert("OnSet must be fired", myDictionary.onSetFired);
				Assert("OnSetComplete must not be fired", !myDictionary.onSetCompleteFired);
				AssertEquals(20, myDictionary.BaseDictionary[10]);
			}
		}

		[Test]
		public void SetOnSetCompleteExcept() 
		{
			bool exceptionThrown = false;

			ConcreteDictionary myDictionary = new ConcreteDictionary(18);
			myDictionary.onSetCompleteMustThrowException = true;
			
			try {
				myDictionary.BaseDictionary[10] = 50;
			} catch {
				exceptionThrown = true;
			} finally {
				Assert("Exception must be thrown", exceptionThrown);
				Assert("OnValidate must be fired", myDictionary.onValidateFired);
				Assert("OnSet must be fired", myDictionary.onSetFired);
				Assert("OnSetComplete must be fired", myDictionary.onSetCompleteFired);
				AssertEquals(20, myDictionary.BaseDictionary[10]);
			}
		}

		[Test]
		public void IsReadOnly() 
		{
			ConcreteDictionary myDictionary = new ConcreteDictionary(1);
			Assert(!myDictionary.BaseDictionary.IsReadOnly);
		}

		[Test]
		public void IsFixedSize() 
		{
			ConcreteDictionary myDictionary = new ConcreteDictionary(1);
			Assert(!myDictionary.BaseDictionary.IsFixedSize);
		}

		[Test]
		public void DictionaryProperty()
		{
			ConcreteDictionary myDictionary = new ConcreteDictionary(1);
			AssertEquals(myDictionary, myDictionary.BaseDictionary);
		}
	}
}
