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

      [MonoTODO()]
      public IEnumerator GetEnumerator() {
         throw new NotImplementedException();
      }
      
      public void CopyTo(Array array, int index) {
         IEnumerator Enum = GetEnumerator();
         while (Enum.MoveNext()) {
            array.SetValue(Enum.Current, ++index);
         }
      }   

      internal IDictionary GetObjects() {
         return _Objects;
      }

      [MonoTODO()]
      public object this[string name] {
         get {
            throw new NotImplementedException();
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
