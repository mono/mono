//
// System.Data.ObjectSpaces.ObjectReader.cs
//
// Author:
//   Mark Easton (mark.easton@blinksoftware.co.uk)
//   Tim Coleman (tim@timcoleman.com)
//
// (C) BLiNK Software Ltd.  http://www.blinksoftware.co.uk
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System.Collections;

namespace System.Data.ObjectSpaces
{
        public abstract class ObjectReader : IDisposable,  IEnumerable
        {
                //Inform listeners when a ValueRecord is being merged
                public event ValueRecordMergeEventHandler ValueMerging; 
                
                bool isClosed = true; 
		bool disposed;

                [MonoTODO]
                public object Current { 
                        get { return null; }
                }

                public abstract bool HasObjects { get; }

                [MonoTODO]
                public virtual bool IsClosed { 
                        get { return this.isClosed; } 
                } 


                [MonoTODO]
                public ObjectContext ObjectContext { 
                        get { return null; } 
		}


                [MonoTODO]
                public Type ObjectType { 
                        get { return null; }                 
		}


                [MonoTODO]
                public virtual void Close ()
                { 
                        this.isClosed = true;
                }     

                protected virtual void Dispose (bool disposing) 
		{
			if (!disposed) {
				if (disposing) {
					Close ();
				}
				disposed = true;
			}
		}

                [MonoTODO]
                public IEnumerator GetEnumerator ()
                {
                        return null;
                }

                void IDisposable.Dispose ()
                {
			Dispose (true);
			GC.SuppressFinalize (this);
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
