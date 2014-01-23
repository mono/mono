using System;
using System.Collections;
using System.Collections.Generic;

class Program
{
	public static void Main ()
	{
		SerializeDictionary (new SerializerLazyDictionary ());
	}

	static void SerializeDictionary (IDictionary values)
	{
	}

	static void SerializeDictionary (IDictionary<string, object> values)
	{
	}
}

sealed class SerializerLazyDictionary : LazyDictionary
{
	protected override IEnumerator<KeyValuePair<string, object>> GetEnumerator ()
	{
		return null;
	}
}

internal abstract class LazyDictionary : IDictionary<string, object>
{
	void IDictionary<string, object>.Add (string key, object value)
	{
		throw new NotSupportedException ();
	}

	bool IDictionary<string, object>.ContainsKey (string key)
	{
		throw new NotSupportedException ();
	}

	ICollection<string> IDictionary<string, object>.Keys
	{
		get { throw new NotSupportedException (); }
	}

	bool IDictionary<string, object>.Remove (string key)
	{
		throw new NotSupportedException ();
	}

	bool IDictionary<string, object>.TryGetValue (string key, out object value)
	{
		throw new NotSupportedException ();
	}

	ICollection<object> IDictionary<string, object>.Values
	{
		get { throw new NotSupportedException (); }
	}

	object IDictionary<string, object>.this [string key] {
		get {
			throw new NotSupportedException ();
		}
		set {
			throw new NotSupportedException ();
		}
	}

	void ICollection<KeyValuePair<string, object>>.Add (KeyValuePair<string, object> item)
	{
		throw new NotSupportedException ();
	}

	void ICollection<KeyValuePair<string, object>>.Clear ()
	{
		throw new NotSupportedException ();
	}

	bool ICollection<KeyValuePair<string, object>>.Contains (KeyValuePair<string, object> item)
	{
		throw new NotSupportedException ();
	}

	void ICollection<KeyValuePair<string, object>>.CopyTo (KeyValuePair<string, object> [] array, int arrayIndex)
	{
		throw new NotSupportedException ();
	}

	int ICollection<KeyValuePair<string, object>>.Count
	{
		get { throw new NotSupportedException (); }
	}

	bool ICollection<KeyValuePair<string, object>>.IsReadOnly
	{
		get { throw new NotSupportedException (); }
	}

	bool ICollection<KeyValuePair<string, object>>.Remove (KeyValuePair<string, object> item)
	{
		throw new NotSupportedException ();
	}

	IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator ()
	{
		return GetEnumerator ();
	}

	protected abstract IEnumerator<KeyValuePair<string, object>> GetEnumerator ();

	System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
	{
		return ((IEnumerable<KeyValuePair<string, object>>) this).GetEnumerator ();
	}
}
