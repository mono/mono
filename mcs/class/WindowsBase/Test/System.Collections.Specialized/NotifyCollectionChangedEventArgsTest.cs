// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// Authors:
//	Brian O'Keefe (zer0keefie@gmail.com)
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using NUnit.Framework;

namespace MonoTests.System.Collections.Specialized {
	[TestFixture]
	public class NotifyCollectionChangedEventArgsTest {
		public NotifyCollectionChangedEventArgsTest()
		{
		}

		[Test]
		public void NotifyCollectionChangedEventArgsConstructor1Test()
		{
			/* Expected Behavior:
			 *
			 * If action is Reset, success.
			 * If action is not Reset, throw an ArgumentException
			 */

			// Trying with Reset
			NotifyCollectionChangedEventArgs args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Reset);

			CollectionChangedEventValidators.ValidateResetOperation (args, "#A01");

			// Trying with Add
			try {
				args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Add);
				Assert.Fail ("Should not be able to call .ctor with NotifyCollectionChangedAction.Add.");
			} catch (ArgumentException) {
			}

			// Trying with Remove
			try {
				args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Remove);
				Assert.Fail ("Should not be able to call .ctor with NotifyCollectionChangedAction.Remove.");
			} catch (ArgumentException) {
			}

			// Trying with Move
			try {
				args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Move);
				Assert.Fail ("Should not be able to call .ctor with NotifyCollectionChangedAction.Move.");
			} catch (ArgumentException) {
			}

			// Trying with Replace
			try {
				args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Replace);
				Assert.Fail ("Should not be able to call .ctor with NotifyCollectionChangedAction.Replace.");
			} catch (ArgumentException) {
			}
		}

		[Test]
		public void NotifyCollectionChangedEventArgsConstructor2Test()
		{
			/* Expected Behavior:
			 * 
			 * If action is Add, success.
			 * If action is Remove, success.
			 * If action is Reset:
			 *    If changedItems is null, success.
			 *    If changedItems is non-null, throw an Argument Exception
			 * If action is Move or Replace, throw an Argument Exception
			 */

			IList changedItems = new List<object> ();

			// Trying with Add
			NotifyCollectionChangedEventArgs args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Add, changedItems);

			CollectionChangedEventValidators.ValidateAddOperation (args, changedItems, "#B01");

			// Trying to add a null array
			try {
				args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Add, (IList)null);
				Assert.Fail ("Cannot call .ctor if changedItems is null.");
			} catch (ArgumentNullException) {
			}

			// Trying with Remove
			args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Remove, changedItems);

			CollectionChangedEventValidators.ValidateRemoveOperation (args, changedItems, "#B02");

			// Trying with Reset (works if changedItems is null)
			args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Reset, (IList)null);

			CollectionChangedEventValidators.ValidateResetOperation (args, "#B03");

			try {
				args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Reset, changedItems);
				Assert.Fail ("Should not be able to call .ctor with NotifyCollectionChangedAction.Reset.");
			} catch (ArgumentException) {
			}

			// Trying with Move
			try {
				args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Move, changedItems);
				Assert.Fail ("Should not be able to call .ctor with NotifyCollectionChangedAction.Move.");
			} catch (ArgumentException) {
			}

			// Trying with Replace
			try {
				args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Replace, changedItems);
				Assert.Fail ("Should not be able to call .ctor with NotifyCollectionChangedAction.Replace.");
			} catch (ArgumentException) {
			}

			// Add some items, and repeat
			changedItems.Add (new object ());
			changedItems.Add (new object ());
			changedItems.Add (new object ());

			// Trying with Add
			args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Add, changedItems);

			CollectionChangedEventValidators.ValidateAddOperation (args, changedItems, "#B04");

			// Trying with Remove
			args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Remove, changedItems);

			CollectionChangedEventValidators.ValidateRemoveOperation (args, changedItems, "#B05");
		}

		[Test]
		public void NotifyCollectionChangedEventArgsConstructor3Test()
		{
			/* Expected Behavior:
			 * 
			 * If action is Add, success.
			 * If action is Remove, success.
			 * If action is Reset:
			 *    If changedItem is null, success.
			 *    If changedItem is non-null, throw an Argument Exception
			 * If action is Move or Replace, throw an Argument Exception
			 */

			object changedItem = new object ();

			// Trying with Add
			NotifyCollectionChangedEventArgs args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Add, changedItem);

			CollectionChangedEventValidators.ValidateAddOperation (args, new object [] { changedItem }, "#C01");

			// Trying with Remove
			args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Remove, changedItem);

			CollectionChangedEventValidators.ValidateRemoveOperation (args, new object [] { changedItem }, "#C02");

			// Trying with Reset

			args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Reset, (object)null);

			CollectionChangedEventValidators.ValidateResetOperation (args, "#C03");

			try {
				args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Reset, changedItem);
				Assert.Fail ("Should not be able to call .ctor with NotifyCollectionChangedAction.Reset.");
			} catch (ArgumentException) {
			}

			// Trying with Move
			try {
				args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Move, changedItem);
				Assert.Fail ("Should not be able to call .ctor with NotifyCollectionChangedAction.Move.");
			} catch (ArgumentException) {
			}

			// Trying with Replace
			try {
				args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Replace, changedItem);
				Assert.Fail ("Should not be able to call .ctor with NotifyCollectionChangedAction.Replace.");
			} catch (ArgumentException) {
			}
		}

		[Test]
		public void NotifyCollectionChangedEventArgsConstructor4Test()
		{
			/* Expected Behavior:
			 * 
			 * If action is Replace:
			 *    If newItems is null, throw an ArgumentNullException.
			 *    If oldItems is null, throw an ArgumentNullException
			 *    Otherwise, success.
			 * If action is not Replace, throw an ArgumentException
			 */

			IList newItems = new List<object> ();
			IList oldItems = new List<object> ();

			// Trying with Replace
			NotifyCollectionChangedEventArgs args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Replace, newItems, oldItems);

			CollectionChangedEventValidators.ValidateReplaceOperation (args, oldItems, newItems, "#D01");

			// Add some items to test this one.
			newItems.Add (new object ());
			newItems.Add (new object ());
			newItems.Add (new object ());

			// Trying with Replace again
			args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Replace, newItems, oldItems);

			CollectionChangedEventValidators.ValidateReplaceOperation (args, oldItems, newItems, "#D02");

			// Add some more items to test this one.
			oldItems.Add (new object ());
			oldItems.Add (new object ());
			oldItems.Add (new object ());

			// Trying with Replace again
			args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Replace, newItems, oldItems);

			CollectionChangedEventValidators.ValidateReplaceOperation (args, oldItems, newItems, "#D03");

			// Trying with null arguments.
			try {
				args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Replace, (IList)null, oldItems);
				Assert.Fail ("The newItems argument cannot be null.");
			} catch (ArgumentNullException) {
			}

			try {
				args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Replace, newItems, (IList)null);
				Assert.Fail ("The oldItems argument cannot be null.");
			} catch (ArgumentNullException) {
			}

			// Trying with Reset
			try {
				args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Reset, newItems, oldItems);
				Assert.Fail ("Should not be able to call .ctor with NotifyCollectionChangedAction.Reset.");
			} catch (ArgumentException) {
			}

			// Trying with Move
			try {
				args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Move, newItems, oldItems);
				Assert.Fail ("Should not be able to call .ctor with NotifyCollectionChangedAction.Move.");
			} catch (ArgumentException) {
			}

			// Trying with Add
			try {
				args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Add, newItems, oldItems);
				Assert.Fail ("Should not be able to call .ctor with NotifyCollectionChangedAction.Add.");
			} catch (ArgumentException) {
			}

			// Trying with Remove
			try {
				args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Remove, newItems, oldItems);
				Assert.Fail ("Should not be able to call .ctor with NotifyCollectionChangedAction.Remove.");
			} catch (ArgumentException) {
			}
		}

		[Test]
		public void NotifyCollectionChangedEventArgsConstructor5Test()
		{
			/* Expected Behavior:
			 * 
			 * If action is Add or Remove:
			 *    If changedItems is null, throw an ArgumentNullException.
			 *    If startingIndex < -1, throw an ArgumentException
			 *    Otherwise, success.
			 * If action is Reset:
			 *    If changedItems is non-null, throw an ArgumentException
			 *    If startingIndex != 0, throw an ArgumentException
			 *    Otherwise, success.
			 * If action is Move or Replace, throw an ArgumentException
			 */

			IList changedItems = new List<object> ();
			int startingIndex = 5; // Doesn't matter what the value of this is.

			// Trying with Add
			NotifyCollectionChangedEventArgs args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Add, changedItems, startingIndex);

			CollectionChangedEventValidators.ValidateAddOperation (args, changedItems, startingIndex, "#E01");

			// Trying with Remove
			args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Remove, changedItems, startingIndex);

			CollectionChangedEventValidators.ValidateRemoveOperation (args, changedItems, startingIndex, "#E02");

			// Add some items to test this one.
			changedItems.Add (new object ());
			changedItems.Add (new object ());
			changedItems.Add (new object ());

			// Trying with Add
			args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Add, changedItems, startingIndex);

			CollectionChangedEventValidators.ValidateAddOperation (args, changedItems, startingIndex, "#E03");

			try {
				args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Add, changedItems, -5);
				Assert.Fail ("Should not be able to call .ctor with NotifyCollectionChangedAction.Add if startingIndex < -1.");
			} catch (ArgumentException) {
			}

			try {
				args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Add, (IList)null, startingIndex);
				Assert.Fail ("Should not be able to call .ctor with NotifyCollectionChangedAction.Add if changedItems is null.");
			} catch (ArgumentNullException) {
			}

			// Trying with Remove
			args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Remove, changedItems, startingIndex);

			CollectionChangedEventValidators.ValidateRemoveOperation (args, changedItems, startingIndex, "#E04");

			try {
				args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Remove, changedItems, -5);
				Assert.Fail ("Should not be able to call .ctor with NotifyCollectionChangedAction.Remove if startingIndex < -1.");
			} catch (ArgumentException) {
			}

			try {
				args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Remove, (IList)null, startingIndex);
				Assert.Fail ("Should not be able to call .ctor with NotifyCollectionChangedAction.Remove if changedItems is null.");
			} catch (ArgumentNullException) {
			}

			// Trying with Reset
			args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Reset, (IList)null, -1);

			try {
				args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Reset, changedItems, -1);
				Assert.Fail ("Should not be able to call .ctor with NotifyCollectionChangedAction.Reset unless changeItems is null");
			} catch (ArgumentException) {
			}

			try {
				args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Reset, (IList)null, 1);
				Assert.Fail ("Should not be able to call .ctor with NotifyCollectionChangedAction.Reset unless startingIndex is -1");
			} catch (ArgumentException) {
			}

			try {
				args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Reset, changedItems, startingIndex);
				Assert.Fail ("Should not be able to call .ctor with NotifyCollectionChangedAction.Reset.");
			} catch (ArgumentException) {
			}

			// Trying with Move
			try {
				args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Move, changedItems, startingIndex);
				Assert.Fail ("Should not be able to call .ctor with NotifyCollectionChangedAction.Move.");
			} catch (ArgumentException) {
			}

			// Trying with Replace
			try {
				args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Replace, changedItems, startingIndex);
				Assert.Fail ("Should not be able to call .ctor with NotifyCollectionChangedAction.Replace.");
			} catch (ArgumentException) {
			}
		}

		[Test]
		public void NotifyCollectionChangedEventArgsConstructor6Test()
		{
			object changedItem = new object ();
			int startingIndex = 5; // Doesn't matter what the value of this is.

			// Trying with Add
			NotifyCollectionChangedEventArgs args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Add, changedItem, startingIndex);

			CollectionChangedEventValidators.ValidateAddOperation (args, new object [] { changedItem }, startingIndex, "#F01");

			// Trying with Remove
			args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Remove, changedItem, startingIndex);

			CollectionChangedEventValidators.ValidateRemoveOperation (args, new object [] { changedItem }, startingIndex, "#F02");

			// Trying with Reset
			args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Reset, (object)null, -1);

			CollectionChangedEventValidators.ValidateResetOperation (args, "#F03");

			try {
				args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Reset, changedItem, -1);
				Assert.Fail ("Should not be able to call .ctor with NotifyCollectionChangedAction.Reset unless changeItems is null");
			} catch (ArgumentException) {
			}

			try {
				args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Reset, (object)null, 1);
				Assert.Fail ("Should not be able to call .ctor with NotifyCollectionChangedAction.Reset unless startingIndex is -1");
			} catch (ArgumentException) {
			}

			try {
				args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Reset, changedItem, startingIndex);
				Assert.Fail ("Should not be able to call .ctor with NotifyCollectionChangedAction.Reset.");
			} catch (ArgumentException) {
			}

			// Trying with Move
			try {
				args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Move, changedItem, startingIndex);
				Assert.Fail ("Should not be able to call .ctor with NotifyCollectionChangedAction.Move.");
			} catch (ArgumentException) {
			}

			// Trying with Replace
			try {
				args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Replace, changedItem, startingIndex);
				Assert.Fail ("Should not be able to call .ctor with NotifyCollectionChangedAction.Replace.");
			} catch (ArgumentException) {
			}
		}

		[Test]
		public void NotifyCollectionChangedEventArgsConstructor7Test()
		{
			object oldItem = new object ();
			object newItem = new object (); // Doesn't matter what the value of this is.

			// Trying with Add
			NotifyCollectionChangedEventArgs args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Replace, newItem, oldItem);
			CollectionChangedEventValidators.ValidateReplaceOperation (args, new object [] { oldItem }, new object [] { newItem }, "#G01");

			// Trying null items
			args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Replace, (object)null, oldItem);
			CollectionChangedEventValidators.ValidateReplaceOperation (args, new object [] { oldItem }, new object [] { null }, "#G02");

			args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Replace, newItem, (object)null);
			CollectionChangedEventValidators.ValidateReplaceOperation (args, new object [] { null }, new object [] { newItem }, "#G03");

			// Trying with Reset
			try {
				args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Reset, newItem, oldItem);
				Assert.Fail ("Should not be able to call .ctor with NotifyCollectionChangedAction.Reset.");
			} catch (ArgumentException) {
			}

			// Trying with Move
			try {
				args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Move, newItem, oldItem);
				Assert.Fail ("Should not be able to call .ctor with NotifyCollectionChangedAction.Move.");
			} catch (ArgumentException) {
			}

			// Trying with Add
			try {
				args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Add, newItem, oldItem);
				Assert.Fail ("Should not be able to call .ctor with NotifyCollectionChangedAction.Add.");
			} catch (ArgumentException) {
			}

			// Trying with Remove
			try {
				args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Remove, newItem, oldItem);
				Assert.Fail ("Should not be able to call .ctor with NotifyCollectionChangedAction.Remove.");
			} catch (ArgumentException) {
			}
		}

		[Test]
		public void NotifyCollectionChangedEventArgsConstructor8Test()
		{
			IList newItems = new List<object> ();
			IList oldItems = new List<object> ();
			int startIndex = 5;

			// Trying with Replace
			NotifyCollectionChangedEventArgs args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Replace, newItems, oldItems, startIndex);

			CollectionChangedEventValidators.ValidateReplaceOperation (args, oldItems, newItems, startIndex, "#H01");

			// Add some items to test this one.
			newItems.Add (new object ());
			newItems.Add (new object ());
			newItems.Add (new object ());

			// Trying with Replace again
			args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Replace, newItems, oldItems, startIndex);

			CollectionChangedEventValidators.ValidateReplaceOperation (args, oldItems, newItems, startIndex, "#H02");

			// Add some more items to test this one.
			oldItems.Add (new object ());
			oldItems.Add (new object ());
			oldItems.Add (new object ());

			// Trying with Replace again
			args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Replace, newItems, oldItems, startIndex);

			CollectionChangedEventValidators.ValidateReplaceOperation (args, oldItems, newItems, startIndex, "#H03");

			// Trying with null arguments.
			try {
				args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Replace, (IList)null, oldItems, startIndex);
				Assert.Fail ("The newItems argument cannot be null.");
			} catch (ArgumentNullException) {
			}

			try {
				args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Replace, newItems, (IList)null, startIndex);
				Assert.Fail ("The oldItems argument cannot be null.");
			} catch (ArgumentNullException) {
			}

			// Trying with Reset
			try {
				args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Reset, newItems, oldItems, startIndex);
				Assert.Fail ("Should not be able to call .ctor with NotifyCollectionChangedAction.Reset.");
			} catch (ArgumentException) {
			}

			// Trying with Move
			try {
				args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Move, newItems, oldItems, startIndex);
				Assert.Fail ("Should not be able to call .ctor with NotifyCollectionChangedAction.Move.");
			} catch (ArgumentException) {
			}

			// Trying with Add
			try {
				args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Add, newItems, oldItems, startIndex);
				Assert.Fail ("Should not be able to call .ctor with NotifyCollectionChangedAction.Add.");
			} catch (ArgumentException) {
			}

			// Trying with Remove
			try {
				args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Remove, newItems, oldItems);
				Assert.Fail ("Should not be able to call .ctor with NotifyCollectionChangedAction.Remove.");
			} catch (ArgumentException) {
			}
		}

		[Test]
		public void NotifyCollectionChangedEventArgsConstructor9Test()
		{
			IList changed = new List<object> ();
			int newIndex = 2;
			int oldIndex = 5;

			// Trying with Replace
			NotifyCollectionChangedEventArgs args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Move, changed, newIndex, oldIndex);

			CollectionChangedEventValidators.ValidateMoveOperation (args, changed, newIndex, oldIndex, "#I01");

			// Add some items to test this one.
			changed.Add (new object ());
			changed.Add (new object ());
			changed.Add (new object ());

			// Trying with Replace again
			args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Move, changed, newIndex, oldIndex);

			CollectionChangedEventValidators.ValidateMoveOperation (args, changed, newIndex, oldIndex, "#I02");

			// Trying with newIndex < 0.
			try {
				args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Move, changed, -5, oldIndex);
				Assert.Fail ("The index argument cannot be less than 0.");
			} catch (ArgumentException) {
			}

			// Trying with Reset
			try {
				args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Reset, changed, newIndex, oldIndex);
				Assert.Fail ("Should not be able to call .ctor with NotifyCollectionChangedAction.Reset.");
			} catch (ArgumentException) {
			}

			// Trying with Replace
			try {
				args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Replace, changed, newIndex, oldIndex);
				Assert.Fail ("Should not be able to call .ctor with NotifyCollectionChangedAction.Replace.");
			} catch (ArgumentException) {
			}

			// Trying with Add
			try {
				args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Add, changed, newIndex, oldIndex);
				Assert.Fail ("Should not be able to call .ctor with NotifyCollectionChangedAction.Add.");
			} catch (ArgumentException) {
			}

			// Trying with Remove
			try {
				args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Remove, changed, newIndex, oldIndex);
				Assert.Fail ("Should not be able to call .ctor with NotifyCollectionChangedAction.Remove.");
			} catch (ArgumentException) {
			}
		}

		[Test]
		public void NotifyCollectionChangedEventArgsConstructor10Test()
		{
			object changed = new object ();
			int newIndex = 2;
			int oldIndex = 5;

			// Trying with Replace
			NotifyCollectionChangedEventArgs args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Move, changed, newIndex, oldIndex);

			CollectionChangedEventValidators.ValidateMoveOperation (args, new object [] { changed }, newIndex, oldIndex, "#J01");

			// Trying with newIndex < 0.
			try {
				args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Move, changed, -5, oldIndex);
				Assert.Fail ("The newIndex argument cannot be less than 0.");
			} catch (ArgumentException) {
			}

			// Trying with Reset
			try {
				args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Reset, changed, newIndex, oldIndex);
				Assert.Fail ("Should not be able to call .ctor with NotifyCollectionChangedAction.Reset.");
			} catch (ArgumentException) {
			}

			// Trying with Replace
			try {
				args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Replace, changed, newIndex, oldIndex);
				Assert.Fail ("Should not be able to call .ctor with NotifyCollectionChangedAction.Replace.");
			} catch (ArgumentException) {
			}

			// Trying with Add
			try {
				args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Add, changed, newIndex, oldIndex);
				Assert.Fail ("Should not be able to call .ctor with NotifyCollectionChangedAction.Add.");
			} catch (ArgumentException) {
			}

			// Trying with Remove
			try {
				args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Remove, changed, newIndex, oldIndex);
				Assert.Fail ("Should not be able to call .ctor with NotifyCollectionChangedAction.Remove.");
			} catch (ArgumentException) {
			}
		}

		[Test]
		public void NotifyCollectionChangedEventArgsConstructor11Test()
		{
			object newItem = new object ();
			object oldItem = new object ();
			int startIndex = 5;

			// Trying with Replace
			NotifyCollectionChangedEventArgs args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Replace, newItem, oldItem, startIndex);

			CollectionChangedEventValidators.ValidateReplaceOperation (args, new object [] { oldItem }, new object [] { newItem }, startIndex, "#K01");

			// Trying with Reset
			try {
				args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Reset, newItem, oldItem, startIndex);
				Assert.Fail ("Should not be able to call .ctor with NotifyCollectionChangedAction.Reset.");
			} catch (ArgumentException) {
			}

			// Trying with Move
			try {
				args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Move, newItem, oldItem, startIndex);
				Assert.Fail ("Should not be able to call .ctor with NotifyCollectionChangedAction.Move.");
			} catch (ArgumentException) {
			}

			// Trying with Add
			try {
				args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Add, newItem, oldItem, startIndex);
				Assert.Fail ("Should not be able to call .ctor with NotifyCollectionChangedAction.Add.");
			} catch (ArgumentException) {
			}

			// Trying with Remove
			try {
				args = new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Remove, newItem, oldItem);
				Assert.Fail ("Should not be able to call .ctor with NotifyCollectionChangedAction.Remove.");
			} catch (ArgumentException) {
			}
		}
	}
}
