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

#if NET_4_0

using System.Collections;
using NUnit.Framework;
using System.Collections.Specialized;
namespace MonoTests.System.Collections.Specialized {

	internal static class CollectionChangedEventValidators {
		#region Validators

		internal static void AssertEquivalentLists(IList expected, IList actual, string message)
		{
			if (expected == null) {
				Assert.IsNull (actual, "LISTEQ_1A::" + message);
				return;
			} else
				Assert.IsNotNull (actual, "LISTEQ_1B::" + message);

			Assert.AreEqual (expected.Count, actual.Count, "LISTEQ_2::" + message);

			for (int i = 0; i < expected.Count; i++)
				Assert.AreEqual (expected [i], actual [i], "LISTEQ_3::" + message);
		}

		private static void ValidateCommon(NotifyCollectionChangedEventArgs args, NotifyCollectionChangedAction action, IList newItems, IList oldItems, int newIndex, int oldIndex, string message)
		{
			Assert.IsNotNull (args, "NCCVAL_1::" + message);

			Assert.AreEqual (action, args.Action, "NCCVAL_2::" + message);

			AssertEquivalentLists (newItems, args.NewItems, "NCCVAL_3::" + message);
			AssertEquivalentLists (oldItems, args.OldItems, "NCCVAL_4::" + message);

			Assert.AreEqual (newIndex, args.NewStartingIndex, "NCCVAL_5::" + message);
			Assert.AreEqual (oldIndex, args.OldStartingIndex, "NCCVAL_6::" + message);
		}

		internal static void ValidateResetOperation(NotifyCollectionChangedEventArgs args, string message)
		{
			ValidateCommon (args, NotifyCollectionChangedAction.Reset, null, null, -1, -1, message);
		}

		internal static void ValidateAddOperation(NotifyCollectionChangedEventArgs args, IList newItems, string message)
		{
			ValidateAddOperation (args, newItems, -1, message);
		}

		internal static void ValidateAddOperation(NotifyCollectionChangedEventArgs args, IList newItems, int startIndex, string message)
		{
			ValidateCommon (args, NotifyCollectionChangedAction.Add, newItems, null, startIndex, -1, message);
		}

		internal static void ValidateRemoveOperation(NotifyCollectionChangedEventArgs args, IList oldItems, string message)
		{
			ValidateRemoveOperation (args, oldItems, -1, message);
		}

		internal static void ValidateRemoveOperation(NotifyCollectionChangedEventArgs args, IList oldItems, int startIndex, string message)
		{
			ValidateCommon (args, NotifyCollectionChangedAction.Remove, null, oldItems, -1, startIndex, message);
		}

		internal static void ValidateReplaceOperation(NotifyCollectionChangedEventArgs args, IList oldItems, IList newItems, string message)
		{
			ValidateReplaceOperation (args, oldItems, newItems, -1, message);
		}

		internal static void ValidateReplaceOperation(NotifyCollectionChangedEventArgs args, IList oldItems, IList newItems, int startIndex, string message)
		{
			ValidateCommon (args, NotifyCollectionChangedAction.Replace, newItems, oldItems, startIndex, startIndex, message);
		}

		internal static void ValidateMoveOperation(NotifyCollectionChangedEventArgs args, IList changedItems, int newIndex, int oldIndex, string message)
		{
			ValidateCommon (args, NotifyCollectionChangedAction.Move, changedItems, changedItems, newIndex, oldIndex, message);
		}

		#endregion
	}
}

#endif