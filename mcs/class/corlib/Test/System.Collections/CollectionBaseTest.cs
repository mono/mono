//
// System.Collections.CollectionBase
// Test suite for System.Collections.CollectionBase
//
// Author:
//    Nick D. Drochak II
//
// (C) 2001 Nick D. Drochak II
//


using System;
using System.Collections;
using NUnit.Framework;

namespace MonoTests.System.Collections
{

public class CollectionBaseTest : TestCase 	
{
	// We need a concrete class to test the abstract base class
	public class ConcreteCollection : CollectionBase 
	{
		// These fields are used as markers to test the On* hooks.
		public bool onClearFired;
		public bool onClearCompleteFired;

		public bool onInsertFired;
		public int onInsertIndex;
		public bool onInsertCompleteFired;
		public int onInsertCompleteIndex;

		public bool onRemoveFired;
		public int onRemoveIndex;
		public bool onRemoveCompleteFired;
		public int onRemoveCompleteIndex;

		public bool onSetFired;
		public int onSetOldValue;
		public int onSetNewValue;
		public bool onSetCompleteFired;
		public int onSetCompleteOldValue;
		public int onSetCompleteNewValue;

		// This constructor is used to test OnValid()
		public ConcreteCollection()	
		{
			IList listObj;
			listObj = this;
			listObj.Add(null);
		}

		// This constructor puts consecutive integers into the list
		public ConcreteCollection(int i) {
			IList listObj;
			listObj = this;

			int j;
			for (j = 0; j< i; j++) {
				listObj.Add(j);
			}
		}

		// A helper method to look at a value in the list at a specific index
		public int PeekAt(int index)
		{
			IList listObj;
			listObj = this;
			return (int) listObj[index];
		}

		// Mark the flag if this hook is fired
		protected override void OnClear() {
			this.onClearFired = true;
		}

		// Mark the flag if this hook is fired
		protected override void OnClearComplete() 
		{
			this.onClearCompleteFired = true;
		}

		// Mark the flag, and save the paramter if this hook is fired
		protected override void OnInsert(int index, object value) 
		{
			this.onInsertFired = true;
			this.onInsertIndex = index;
		}

		// Mark the flag, and save the paramter if this hook is fired
		protected override void OnInsertComplete(int index, object value) 
		{
			this.onInsertCompleteFired = true;
			this.onInsertCompleteIndex = index;
		}
		
		// Mark the flag, and save the paramter if this hook is fired
		protected override void OnRemove(int index, object value) 
		{
			this.onRemoveFired = true;
			this.onRemoveIndex = index;
		}
		
		// Mark the flag, and save the paramter if this hook is fired
		protected override void OnRemoveComplete(int index, object value) 
		{
			this.onRemoveCompleteFired = true;
			this.onRemoveCompleteIndex = index;
		}
		
		// Mark the flag, and save the paramters if this hook is fired
		protected override void OnSet(int index, object oldValue, object newValue) 
		{
			this.onSetFired = true;
			this.onSetOldValue = (int) oldValue;
			this.onSetNewValue = (int) newValue;
		}
		
		// Mark the flag, and save the paramters if this hook is fired
		protected override void OnSetComplete(int index, object oldValue, object newValue) 
		{
			this.onSetCompleteFired = true;
			this.onSetCompleteOldValue = (int) oldValue;
			this.onSetCompleteNewValue = (int) newValue;
		}
	}  // public class ConcreteCollection

	// Check the count property
	public void TestCount() {
		ConcreteCollection myCollection;
		myCollection = new ConcreteCollection(4);
		Assert(4 == myCollection.Count);
	}

	// Make sure GetEnumerator returns an object
	public void TestGetEnumerator() {
		ConcreteCollection myCollection;
		myCollection = new ConcreteCollection(4);
		Assert(null != myCollection.GetEnumerator());
	}

	// OnValid disallows nulls
	public void TestOnValid() {
		ConcreteCollection myCollection;
		try {
			myCollection = new ConcreteCollection();
		}
		catch (ArgumentNullException) {
		}
	}

	// Test various Insert paths
	public void TestInsert() {
		ConcreteCollection myCollection;
		int numberOfItems;
		numberOfItems = 3;
		// The constructor inserts
		myCollection = new ConcreteCollection(numberOfItems);
		Assert(myCollection.onInsertFired);
		Assert(myCollection.onInsertCompleteFired);

		// Using the IList interface, check inserts in the middle
		IList listObj = myCollection;
		listObj.Insert(1, 9);
		Assert(myCollection.onInsertIndex == 1);
		Assert(myCollection.onInsertCompleteIndex == 1);
		Assert(myCollection.PeekAt(1) == 9);
	}

	// Test Clear and it's hooks
	public void TestClear() 
	{
		ConcreteCollection myCollection;
		int numberOfItems;
		numberOfItems = 1;
		myCollection = new ConcreteCollection(numberOfItems);
		myCollection.Clear();
		Assert(myCollection.Count == 0);
		Assert(myCollection.onClearFired);
		Assert(myCollection.onClearCompleteFired);
	}

	// Test RemoveAt, other removes and the hooks
	public void TestRemove() 
	{
		ConcreteCollection myCollection;
		int numberOfItems;
		numberOfItems = 3;
		// Set up a test collection
		myCollection = new ConcreteCollection(numberOfItems);

		// The list is 0-based.  So if we remove the second one
		myCollection.RemoveAt(1);

		// We should see the original third one in it's place
		Assert(myCollection.PeekAt(1) == 2);
		Assert(myCollection.onRemoveFired);
		Assert(myCollection.onRemoveIndex == 1);
		Assert(myCollection.onRemoveCompleteFired);
		Assert(myCollection.onRemoveCompleteIndex == 1);
		IList listObj = myCollection;
		listObj.Remove(0);
		// Confirm parameters are being passed to the hooks
		Assert(myCollection.onRemoveIndex == 0);
		Assert(myCollection.onRemoveCompleteIndex == 0);
	}

	// Test the random access feature
	public void TestSet() 
	{
		ConcreteCollection myCollection;
		int numberOfItems;
		numberOfItems = 3;
		myCollection = new ConcreteCollection(numberOfItems);
		IList listObj = myCollection;
		listObj[0] = 99;
		Assert((int) listObj[0] == 99);
		Assert(myCollection.onSetFired);
		Assert(myCollection.onSetCompleteFired);
		Assert(myCollection.onSetOldValue == 0);
		Assert(myCollection.onSetCompleteOldValue == 0);
		Assert(myCollection.onSetNewValue == 99);
		Assert(myCollection.onSetCompleteNewValue == 99);
	}
}

}
