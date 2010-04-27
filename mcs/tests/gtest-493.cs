using System;
using System.Collections;
using System.Collections.Generic;

namespace MonoGenericIteratorTest
{
	public class MyType
	{
	}

	public abstract class MyCollectionBase<T> : Dictionary<string, T>
	{
		public new virtual IEnumerator GetEnumerator ()
		{
			return Values.GetEnumerator ();
		}
	}

	public class MyCollection : MyCollectionBase<MyType>
	{
	}

	class MainClass
	{
		public static void Main (string[] args)
		{
			MyCollection myCollection = new MyCollection ();

			foreach (MyType item in myCollection) {
				Console.WriteLine ("Success.");
			}
		}
	}
}