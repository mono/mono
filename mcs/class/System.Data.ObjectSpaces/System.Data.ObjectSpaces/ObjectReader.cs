//
// System.Data.ObjectSpaces.ObjectReader.cs
//
// Author:
//   Mark Easton (mark.easton@blinksoftware.co.uk)
//
// (C) BLiNK Software Ltd.  http://www.blinksoftware.co.uk
//

#if NET_1_2

namespace System.Data.ObjectSpaces
{
        public abstract class ObjectReader : IDisposable,  IEnumerable
        {
                //Inform listeners when a ValueRecord is being merged
                public event ValueRecordMergeEventHandler ValueMerging; 
                
                private bool isClosed = true;           //Is the reader closed


                [MonoTODO]
                public object Current { 
                        get { return null; }
                }

                
                [MonoTODO]
                public abstract bool HasObjects { 
                        get { return false; } 
                } 


                [MonoTODO]
                public virtual bool IsClosed { 
                        get { return this.isClosed; } 
                } 


                [MonoTODO]
                public ObjectContext ObjectContext { 
                        get { return null; } 


                [MonoTODO]
                public Type ObjectType { 
                        get { return null; }                 


                [MonoTODO]
                public virtual void Close ()
                { 
                        this.isClosed = true;
                }     


                [MonoTODO]
                protected virtual void Dispose (bool disposing) {}



                [MonoTODO]
                public IEnumerator GetEnumerator ()
                {
                        return null;
                }

                [MonoTODO]
                private void IDisposable.Dispose ()
                {
                        return false;
                }

                [MonoTODO]
                protected virtual void OnValueMerging (ValueRecordMergeEventArgs e)
                {
                        if (this.ValueMerging != null)
                                this.ValueMerging (this, e);
                }
                
                
                [MonoTODO]
                public virtual bool Read()
                {
                        return false;       
                }
        }
}

#endif
