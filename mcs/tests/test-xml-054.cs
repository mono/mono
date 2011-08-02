// Compiler options: -doc:xml-054.xml
using System;
using System.Collections;
using System.Collections.Generic;

public class A {
	public interface I {
		void Foo ();
		void Bar<T>(T value);
	}
}

/// <summary>Container`2</summary>
public class Container<TKey, TValue> : IList<TValue>, A.I {

	/// <summary>Container`2.#ctor</summary>
	public Container ()
	{
	}

	/// <summary>Container`2.System#Collections#Generic#ICollection&lt;TValue&gt;#Count</summary>
	int ICollection<TValue>.Count {get {return 0;}}

	/// <summary>Container`2.System#Collections#Generic#ICollection&lt;TValue&gt;#IsReadOnly</summary>
	bool ICollection<TValue>.IsReadOnly {get {return true;}}

	/// <summary>Container`2.System#Collections#Generic#ICollection&lt;TValue&gt;#Add(`1)</summary>
	void ICollection<TValue>.Add (TValue value) {}

	/// <summary>Container`2.System#Collections#Generic#ICollection&lt;TValue&gt;#Remove(`1)</summary>
	bool ICollection<TValue>.Remove (TValue value) {return false;}

	/// <summary>Container`2.System#Collections#Generic#ICollection&lt;TValue&gt;#Clear</summary>
	void ICollection<TValue>.Clear () {}

	/// <summary>Container`2.System#Collections#Generic#ICollection&lt;TValue&gt;#Contains(`1)</summary>
	bool ICollection<TValue>.Contains (TValue value) {return false;}

	/// <summary>Container`2.System#Collections#Generic#ICollection&lt;TValue&gt;#CopyTo(`1[],System.Int32)</summary>
	void ICollection<TValue>.CopyTo (TValue[] array, int arrayIndex) {}

	/// <summary>Container`2.System#Collections#Generic#IList&lt;TValue&gt;#IndexOf(`1)</summary>
	int IList<TValue>.IndexOf (TValue value) {return -1;}

	/// <summary>Container`2.System#Collections#Generic#IList&lt;TValue&gt;#IndexOf(System.Int32,`1)</summary>
	void IList<TValue>.Insert (int index, TValue item) {}

	/// <summary>Container`2.System#Collections#Generic#IList&lt;TValue&gt;#RemoveAt(System.Int32)</summary>
	void IList<TValue>.RemoveAt (int index) {}

	/// <summary>Container`2.System#Collections#Generic#IList&lt;TValue&gt;#Item(System.Int32)</summary>
	TValue IList<TValue>.this [int index] {
		get {return default (TValue);}
		set {}
	}

	/// <summary>Container`2.System#Collections#IEnumerable#GetEnumerator</summary>
	IEnumerator IEnumerable.GetEnumerator ()
	{
		return GetEnumerator ();
	}

	/// <summary>Container`2.GetEnumerator</summary>
	public IEnumerator<TValue> GetEnumerator ()
	{
		yield break;
	}

	/// <summary>Container`2.A#I#Foo</summary>
	void A.I.Foo ()
	{
	}

	/// <summary>Container`2.A#I#Bar``1(``0)</summary>
	void A.I.Bar<T> (T value)
	{
	}

	/// <summary>Container`2.Element</summary>
	public class Element : ICloneable {

		/// <summary>Container`2.Element.System#ICloneable#Clone</summary>
		object ICloneable.Clone ()
		{
			return Clone ();
		}

		/// <summary>Container`2.Element.Clone</summary>
		public Element Clone ()
		{
			return (Element) MemberwiseClone ();
		}
	}
}

class Test {
	public static void Main ()
	{
	}
}

