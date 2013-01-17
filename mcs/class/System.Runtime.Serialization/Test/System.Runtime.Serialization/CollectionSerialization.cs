//
// CollectionSerialization
//
// Authors:
//      Martin Baulig (martin.baulig@xamarin.com)
//
// Copyright 2012 Xamarin Inc. (http://www.xamarin.com)
//
//
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
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.ServiceModel;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using NUnit.Framework.SyntaxHelpers;

namespace MonoTests.System.Runtime.Serialization
{
	[TestFixture]
	public class CollectionSerialization
	{
		[DataContract]
		class Foo
		{
			[DataMember]
			public int Hello;
		}
		
		class MyList<T> : List<T>, IMyList<T>
		{
		}
		
		interface IMyList<T> : IList
		{
		}

		[Serializable]
		class CustomList<T> : IList<T>
		{
			List<T> list;

			public CustomList (IList<T> elements)
			{
				list = new List<T> ();
				if (elements != null)
					list.AddRange (elements);
			}

			#region IList implementation
			public int IndexOf (T item)
			{
				return list.IndexOf (item);
			}
			public void Insert (int index, T item)
			{
				list.Insert (index, item);
			}
			public void RemoveAt (int index)
			{
				list.RemoveAt (index);
			}
			public T this [int index] {
				get {
					return list [index];
				}
				set {
					list [index] = value;
				}
			}
			#endregion
			#region ICollection implementation
			public void Add (T item)
			{
				list.Add (item);
			}
			public void Clear ()
			{
				list.Clear ();
			}
			public bool Contains (T item)
			{
				return list.Contains (item);
			}
			public void CopyTo (T[] array, int arrayIndex)
			{
				list.CopyTo (array, arrayIndex);
			}
			public bool Remove (T item)
			{
				return list.Remove (item);
			}
			#endregion
			#region IEnumerable implementation
			public IEnumerator<T> GetEnumerator ()
			{
				return list.GetEnumerator ();
			}
			#endregion
			#region IEnumerable implementation
			IEnumerator IEnumerable.GetEnumerator ()
			{
				return GetEnumerator ();
			}
			#endregion
			#region ICollection<T> implementation
			int ICollection<T>.Count {
				get {
					return list.Count;
				}
			}
			bool ICollection<T>.IsReadOnly {
				get {
					return ((ICollection<T>)list).IsReadOnly;
				}
			}
			#endregion

			public override int GetHashCode ()
			{
				return list.GetHashCode ();
			}

			public override bool Equals (object obj)
			{
				var custom = obj as CustomList<T>;
				if (custom == null)
					return false;

				if (list.Count != custom.list.Count)
					return false;

				for (int i = 0; i < list.Count; i++)
					if (!list [i].Equals (custom.list [i]))
						return false;

				return true;
			}
		}

		class CustomCollection<T> : CustomList<T>
		{
			public CustomCollection ()
				: base (null)
			{
			}

			public CustomCollection (IList<T> elements)
				: base (elements)
			{
			}
		}

		static object Serialize<T> (object arg)
		{
			using (var ms = new MemoryStream ()) {
				try {
					var serializer = new DataContractSerializer (typeof(T));
					serializer.WriteObject (ms, arg);
				} catch (Exception ex) {
					return ex;
				}

				return new UTF8Encoding ().GetString (ms.GetBuffer (), 0, (int)ms.Position);
			}
		}

		static T Deserialize<T> (string text)
		{
			var buffer = new UTF8Encoding ().GetBytes (text);
			using (var ms = new MemoryStream (buffer)) {
				var serializer = new DataContractSerializer (typeof(T));
				return (T)serializer.ReadObject (ms);
			}
		}

		[Test]
		public void CollectionInterfaceContract ()
		{
			var array = new object[3] { 1, 2, 3 };
			var arrayResult = (string)Serialize<object[]> (array);

			var list = new List<int> (new[] { 1, 2, 3 });
			
			Assert.That (Serialize<IList> (array), Is.EqualTo (arrayResult), "#1");
			Assert.That (Serialize<IList> (list), Is.EqualTo (arrayResult), "#2");
			Assert.That (Serialize<IEnumerable> (list), Is.EqualTo (arrayResult), "#3");
			Assert.That (Serialize<ICollection> (list), Is.EqualTo (arrayResult), "#4");

			var alist = new ArrayList ();
			alist.AddRange (array);

			Assert.That (Serialize<IList> (alist), Is.EqualTo (arrayResult), "#5");

			Assert.That (Deserialize<IList> (arrayResult), Is.EqualTo (list), "#6");
			Assert.That (Deserialize<IEnumerable> (arrayResult), Is.EqualTo (list), "#7");
			Assert.That (Deserialize<ICollection> (arrayResult), Is.EqualTo (list), "#8");
		}

		[Test]
		public void GenericCollectionInterfaceContract ()
		{
			var array = new[] { 1, 2, 3 };
			var arrayResult = (string)Serialize<int[]> (array);
			
			var list = new List<int> (array);
			var mylist = new MyList<int> ();
			mylist.AddRange (array);

			var custom = new CustomList<int> (array);

			Assert.That (Serialize<IList<int>> (list), Is.EqualTo (arrayResult), "#1");
			Assert.That (Serialize<IEnumerable<int>> (list), Is.EqualTo (arrayResult), "#2");
			Assert.That (Serialize<ICollection<int>> (list), Is.EqualTo (arrayResult), "#3");

			Assert.That (Serialize<IList<object>> (list),
			             Is.InstanceOfType (typeof (InvalidCastException)), "#4");

			Assert.That (Serialize<IList<int>> (mylist), Is.EqualTo (arrayResult), "#5");
			Assert.That (Serialize<IList<int>> (list.AsReadOnly ()), Is.EqualTo (arrayResult), "#6");
			Assert.That (Serialize<IList<int>> (custom), Is.EqualTo (arrayResult), "#7");

			Assert.That (Deserialize<IList<int>> (arrayResult), Is.EqualTo (list), "#8");
			Assert.That (Deserialize<List<int>> (arrayResult), Is.EqualTo (list), "#9");
		}

		[Test]
		public void CustomCollectionInterfaceContract ()
		{
			var array = new[] { 1, 2, 3 };
			var arrayResult = Serialize<int[]> (array);
			
			var mylist = new MyList<int> ();
			mylist.AddRange (array);

			Assert.That (Serialize<IList<int>> (mylist), Is.EqualTo (arrayResult), "#1");
			Assert.That (Serialize<List<int>> (mylist), Is.EqualTo (arrayResult), "#2");
			Assert.That (Serialize<IMyList<int>> (mylist),
			             Is.InstanceOfType (typeof (SerializationException)), "#3");
			Assert.That (Serialize<MyList<int>> (mylist), Is.EqualTo (arrayResult), "#4");
		}

		[Test]
		public void CustomCollectionTypeContract ()
		{
			var array = new[] { 1, 2, 3 };
			var arrayResult = (string)Serialize<int[]> (array);

			var custom = new CustomList<int> (array);

			var result = (string)Serialize<CustomList<int>> (custom);
			Assert.That (result.Contains ("CustomListOfint"), Is.True, "#1");
			Assert.That (Deserialize<CustomList<int>> (result), Is.EqualTo (custom), "#2");

			var ro = array.ToList ().AsReadOnly ();
			var result2 = (string)Serialize<ReadOnlyCollection<int>> (ro);
			Assert.That (result2.Contains ("ReadOnlyCollectionOfint"), Is.True, "#3");
			Assert.That (Deserialize<ReadOnlyCollection<int>> (result2), Is.EqualTo (ro), "#4");

			/*
			 * CustomList<T> implements one of the collection interfaces, but does not have
			 * a public parameterless constructor.  It is therefor treated like a normal
			 * [Serializable] type and can not be deserialized from an array.
			 * 
			 * The same also applies to ReadOnlyCollection<T>.
			 * 
			 */

			try {
				Deserialize<CustomList<int>> (arrayResult);
				Assert.Fail ("#5");
			} catch (Exception ex) {
				Assert.That (ex, Is.InstanceOfType (typeof (SerializationException)), "#6");
			}

			try {
				Deserialize<ReadOnlyCollection<int>> (arrayResult);
				Assert.Fail ("#7");
			} catch (Exception ex) {
				Assert.That (ex, Is.InstanceOfType (typeof (SerializationException)), "#8");
			}

			/*
			 * CustomCollection<T> does have the required public parameterless constructor,
			 * so it is treated as custom collection type and serialized as array.
			 * 
			 */

			var collection = new CustomCollection<int> (array);
			var result3 = (string)Serialize<CustomCollection<int>> (collection);
			Assert.That (result3, Is.EqualTo (arrayResult), "#9");
			Assert.That (Deserialize<CustomCollection<int>> (result3), Is.EqualTo (collection), "#10");
		}

		[Test]
		public void ArrayContract ()
		{
			var array = new[] { 1, 2, 3 };
			var list = new List<int> (array);

			Assert.That (Serialize<int[]> (list),
			             Is.InstanceOfType (typeof (InvalidCastException)), "#1");
			Assert.That (Serialize<object[]> (array),
			             Is.InstanceOfType (typeof (InvalidCastException)), "#2");
		}

		[Test]
		public void ListOfArrays ()
		{
			var water = new[] { "Fish", "Mermaid" };
			var land = new[] { "Horse", "Human", "Lion" };
			var air = new[] { "Bird", "Drake" };
			var species = new[] { water, land, air };
			var serialized = (string)Serialize<string[][]> (species);

			var list = new List<string[]> (species);
			Assert.That (Serialize<IList<string[]>> (species), Is.EqualTo (serialized), "#1");
			Assert.That (Serialize<IList<string[]>> (list), Is.EqualTo (serialized), "#2");
		}

		[CollectionDataContract (Name = "MyCollection")]
		class MissingAddMethod<T> : IEnumerable<T>
		{
			#region IEnumerable implementation
			public IEnumerator<T> GetEnumerator ()
			{
				throw new InvalidOperationException ();
			}
#endregion
			#region IEnumerable implementation
			IEnumerator IEnumerable.GetEnumerator ()
			{
				throw new InvalidOperationException ();
			}
#endregion
		}
		
		[CollectionDataContract (Name = "MyCollection")]
		class MissingEnumerable<T>
		{
			public void Add (T item)
			{
				throw new NotImplementedException ();
			}
		}
		
		[CollectionDataContract (Name = "MyCollection")]
		class MyDataContractCollection<T> : IEnumerable<T>
		{
			List<T> list;
			
			public MyDataContractCollection ()
			{
				list = new List<T> ();
			}
			
			public MyDataContractCollection (IList<T> elements)
			{
				list = new List<T> ();
				list.AddRange (elements);
			}
			
			#region IEnumerable implementation
			public IEnumerator<T> GetEnumerator ()
			{
				return list.GetEnumerator ();
			}
#endregion
			#region IEnumerable implementation
			IEnumerator IEnumerable.GetEnumerator ()
			{
				return GetEnumerator ();
			}
#endregion
			
			public void Add (T item)
			{
				list.Add (item);
			}
		}

		class MyDerivedDataContract<T> : MyDataContractCollection<T>
		{
		}
		
		[Test]
		public void TestCollectionDataContract ()
		{
			Assert.That (Serialize<MissingAddMethod<int>> (new MissingAddMethod<int> ()),
			             Is.InstanceOfType (typeof (InvalidDataContractException)), "#1");
			Assert.That (Serialize<MissingEnumerable<int>> (new MissingEnumerable<int> ()),
			             Is.InstanceOfType (typeof (InvalidDataContractException)), "#2");

			var array = new[] { 1, 2, 3 };
			var arrayResult = (string)Serialize<int[]> (array);
			var collection = new MyDataContractCollection<int> (array);
			
			var result = Serialize<MyDataContractCollection<int>> (collection);
			Assert.That (result, Is.InstanceOfType (typeof(string)), "#3");

			Assert.That (Serialize<MyDataContractCollection<int>> (array),
			             Is.InstanceOfType (typeof (SerializationException)), "#4");

			var derived = new MyDerivedDataContract<int> ();
			Assert.That (Serialize<MyDataContractCollection<int>> (derived),
			             Is.InstanceOfType (typeof (SerializationException)), "#5");

			try {
				Deserialize<MyDataContractCollection<int>> (arrayResult);
				Assert.Fail ("#6");
			} catch (Exception ex) {
				Assert.That (ex, Is.InstanceOfType (typeof(SerializationException)), "#7");
			}
			
			var deserialized = Deserialize<MyDataContractCollection<int>> ((string)result);
			Assert.That (deserialized, Is.InstanceOfType (typeof (MyDataContractCollection<int>)), "#8");
		}

		[Test]
		public void Test ()
		{
			var derived = new MyDerivedDataContract<int> ();
			Assert.That (Serialize<MyDataContractCollection<int>> (derived),
			             Is.InstanceOfType (typeof (SerializationException)), "#5");
		}

	}
}

