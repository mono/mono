//
// Collections.Specialized.NameObjectCollectionBase
//
// Author:
//   Nick Drochak II (ndrochak@gol.com)
//
// (C) 2001 Nick Drochak II
//

using System;
using System.Collections;
using System.Runtime.Serialization;

namespace System.Collections.Specialized{
	public abstract class NameObjectCollectionBase  :
		System.Collections.ICollection, 
		System.Collections.IEnumerable, 
		System.Runtime.Serialization.ISerializable, 
		System.Runtime.Serialization.IDeserializationCallback
	{
		private Hashtable theCollection = null;

		//TODO: Implement all the methods that just have the "throw" statement

		protected NameObjectCollectionBase(){throw new Exception("Feature not yet implemented");}
		protected NameObjectCollectionBase(int capacity){throw new Exception("Feature not yet implemented");}
		protected NameObjectCollectionBase(IHashCodeProvider hashProvider, IComparer comparer){throw new Exception("Feature not yet implemented");}
		protected NameObjectCollectionBase(SerializationInfo info, StreamingContext context){throw new Exception("Feature not yet implemented");}
		protected NameObjectCollectionBase(int capacity, IHashCodeProvider hashProvider, IComparer comparer){throw new Exception("Feature not yet implemented");}

		virtual public int Count { get { return theCollection.Count; } }
		virtual public NameObjectCollectionBase.KeysCollection Keys { get {return (KeysCollection) theCollection.Keys; } }

		IEnumerator IEnumerable.GetEnumerator(){throw new Exception("Feature not yet implemented");}
		virtual public void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context){throw new Exception("Feature not yet implemented");}
		virtual public void OnDeserialization(object sender){throw new Exception("Feature not yet implemented");}

		virtual protected bool IsReadOnly {get {throw new Exception("Feature not yet implemented");} set {throw new Exception("Feature not yet implemented");}}

		virtual protected void BaseAdd(string name, object value){throw new Exception("Feature not yet implemented");}
		virtual protected void BaseClear(){throw new Exception("Feature not yet implemented");}
		virtual protected object BaseGet(int index){throw new Exception("Feature not yet implemented");}
		virtual protected object BaseGet(string name){throw new Exception("Feature not yet implemented");}
		virtual protected string[] BaseGetAllKeys(){throw new Exception("Feature not yet implemented");}
		virtual protected object[] BaseGetAllValues(){throw new Exception("Feature not yet implemented");}
		virtual protected object[] BaseGetAllValues(Type type){throw new Exception("Feature not yet implemented");}
		virtual protected string BaseGetKey(int index){throw new Exception("Feature not yet implemented");}
		virtual protected bool BaseHasKeys(){throw new Exception("Feature not yet implemented");}
		virtual protected void BaseRemove(string name){throw new Exception("Feature not yet implemented");}
		virtual protected void BaseRemoveAt(int index){throw new Exception("Feature not yet implemented");}
		virtual protected void BaseSet(int index, object value){throw new Exception("Feature not yet implemented");}
		virtual protected void BaseSet(string name, object value){throw new Exception("Feature not yet implemented");}

		public bool IsSynchronized {get{throw new Exception("Feature not yet implemented");}}
		public object SyncRoot {get{throw new Exception("Feature not yet implemented");}}

		virtual public void CopyTo(Array array, int index){throw new Exception("Feature not yet implemented");}

		public class KeysCollection : ICollection, IEnumerable {
			public int Count {get{throw new Exception("Feature not yet implemented");}}
			public bool IsSynchronized {get{throw new Exception("Feature not yet implemented");}}
			public object SyncRoot {get{throw new Exception("Feature not yet implemented");}}

			public void CopyTo(Array array, int index){throw new Exception("Feature not yet implemented");}

			IEnumerator IEnumerable.GetEnumerator(){throw new Exception("Feature not yet implemented");}
		}
	}
}
