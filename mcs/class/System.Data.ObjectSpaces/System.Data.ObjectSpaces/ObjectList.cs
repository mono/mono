//
// System.Data.ObjectSpaces.ObjectList.cs
//
// Author:
//   Mark Easton (mark.easton@blinksoftware.co.uk)
//
// (C) BLiNK Software Ltd.  http://www.blinksoftware.co.uk
//

#if NET_1_2

using System.Collections;

namespace System.Data.ObjectSpaces
{
        [MonoTODO]
        public class ObjectList : ICollection, IEnumerable, IList
        {
                
                [MonoTODO]
                public ObjectList () : this (typeof (ArrayList), null) {}

                [MonoTODO]
                public ObjectList (Type type, object[] parameters)
                {
                        if (type == null || !(type is IList))
                                throw ObjectException.CreateObjectException ();
                }

                [MonoTODO]
                public int Count {
                        get { return 0; }
                }     

		[MonoTODO]
		private bool ICollection.IsSynchronized {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		private object ICollection.SyncRoot {
			get { throw new NotImplementedException (); }
		}
                
                [MonoTODO]
                public IList InnerList {
                        get { return null; }
                }        
                
                [MonoTODO]
                public bool IsFixedSize {
                        get { return false; }
                }  

                [MonoTODO]
                public bool IsReadOnly {
                        get { return false; }
                }
                
                [MonoTODO]
                public object this[int index] {
                        get { throw new NotImplementedException (); }       
                        set { throw new NotImplementedException (); }       
                }
                
                [MonoTODO]
                public int Add (object value)
                {
                        return 0;        
                }
                
                [MonoTODO]
                public void Clear () {}
                
                [MonoTODO]
                public bool Contains (object value)
                {
                        return false;
                }
                
                [MonoTODO]
                public void CopyTo (Array array, int index) {}
               
                
                [MonoTODO]
                public IEnumerator GetEnumerator ()
                {
                        return null;
                }
                
                [MonoTODO]
                public int IndexOf (object value)
                {
                        return 0;        
                }
                
                [MonoTODO]
                public void Insert (int index, object value) {}
                
                [MonoTODO]
                public void Remove (object value) {}
                
                [MonoTODO]
                public void RemoveAt (int index) {}
        }
}

#endif
