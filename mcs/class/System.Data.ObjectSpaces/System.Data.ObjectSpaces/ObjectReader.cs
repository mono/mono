//
// System.Data.ObjectSpaces.ObjectReader.cs
//
// Author:
//   Mark Easton (mark.easton@blinksoftware.co.uk)
//   Tim Coleman (tim@timcoleman.com)
//
// (C) BLiNK Software Ltd.  http://www.blinksoftware.co.uk
// Copyright (C) Tim Coleman, 2003-2004
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

#if NET_2_0

using System.Collections;

namespace System.Data.ObjectSpaces
{
        public abstract class ObjectReader : IDisposable,  IEnumerable
        {
		#region Fields

                bool isClosed = true; 
		bool disposed;

		#endregion // Fields

		#region Properties

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

		#endregion // Properties

		#region Events and Delegates

                //Inform listeners when a ValueRecord is being merged
                public event ValueRecordMergeEventHandler ValueMerging; 

		#endregion // Events and Delegates

		#region Methods

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

		#endregion // Methods
        }
}

#endif
