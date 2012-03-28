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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using MonoTests.System.Collections.Specialized;
using NUnit.Framework;

namespace MonoTests.System.Collections.ObjectModel {

	[TestFixture]
	public class ReadOnlyObservableCollectionTest {
		[Test]
		public void ClassTest()
		{
			// Because the collection cannot change, check to make sure exceptions are thrown for modifications.

			List<char> initial = new List<char> ();
			initial.Add ('A');
			initial.Add ('B');
			initial.Add ('C');

			ObservableCollection<char> collection = new ObservableCollection<char> (initial);
			ReadOnlyObservableCollection<char> readOnlyCollection = new ReadOnlyObservableCollection<char> (collection);

			// Test the events

			PropertyChangedEventHandler pceh = delegate (object sender, PropertyChangedEventArgs e) {
				Assert.Fail ("No properties should change.");
			};

			NotifyCollectionChangedEventHandler ncceh = delegate (object sender, NotifyCollectionChangedEventArgs e) {
				Assert.Fail ("The collection should not change.");
			};

			((INotifyPropertyChanged)readOnlyCollection).PropertyChanged += pceh;

			((INotifyCollectionChanged)readOnlyCollection).CollectionChanged += ncceh;

			// Done with the events.
			((INotifyPropertyChanged)readOnlyCollection).PropertyChanged -= pceh;

			((INotifyCollectionChanged)readOnlyCollection).CollectionChanged -= ncceh;

			Assert.AreEqual (3, readOnlyCollection.Count, "RO_1");
			CollectionChangedEventValidators.AssertEquivalentLists (initial, readOnlyCollection, "RO_2");

			// Modifying the underlying collection

			bool propChanged = false;

			pceh = delegate (object sender, PropertyChangedEventArgs e) {
				propChanged = true;
			};

			ncceh = delegate (object sender, NotifyCollectionChangedEventArgs e) {
				CollectionChangedEventValidators.ValidateAddOperation (e, new char [] { 'I' }, 3, "RO_3");
			};

			((INotifyPropertyChanged)readOnlyCollection).PropertyChanged += pceh;

			((INotifyCollectionChanged)readOnlyCollection).CollectionChanged += ncceh;

			// In theory, this will cause the properties to change.
			collection.Add ('I');

			Assert.IsTrue (propChanged, "RO_4");
		}
	}
}

#endif