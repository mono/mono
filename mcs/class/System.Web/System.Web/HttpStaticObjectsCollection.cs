using System;
using System.Collections;

namespace System.Web {
   public sealed class HttpStaticObjectsCollection : ICollection, IEnumerable {
      private Hashtable _Objects;

      // Needs to hold object items that can be latebound and can be serialized
      public HttpStaticObjectsCollection() {
         _Objects = new Hashtable();
      }

      public object GetObject(string name) {
         return this[name];
      }

      public IEnumerator GetEnumerator() {
         return _Objects.GetEnumerator ();
      }
      
      public void CopyTo(Array array, int index) {
	 _Objects.CopyTo (array, index);
      }   

      internal IDictionary GetObjects() {
         return _Objects;
      }

      public object this[string name] {
         get {
            return _Objects [name];
         }
      }

      public int Count {
         get {
            return _Objects.Count;
         }
      }

      public bool IsReadOnly {
         get {
            return true;
         }
      }

      public bool IsSynchronized {
         get {
            return false;
         }
      }

      public object SyncRoot {
         get {
            return this;
         }
      }
   }
}
