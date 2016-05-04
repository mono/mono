//------------------------------------------------------------------------------
// <copyright file="DisposableCollectionWrapper.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
//------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace System.Data.Common.Utils
{
    internal class DisposableCollectionWrapper<T> : IDisposable, IEnumerable<T> where T : IDisposable
    {
        IEnumerable<T> _enumerable;
        internal DisposableCollectionWrapper(IEnumerable<T> enumerable)
        {
            Debug.Assert(enumerable != null, "don't pass in a null enumerable");
            _enumerable = enumerable;
        }

        public void  Dispose()
        {
            // Technically, calling GC.SuppressFinalize is not required because the class does not
            // have a finalizer, but it does no harm, protects against the case where a finalizer is added
            // in the future, and prevents an FxCop warning.
            GC.SuppressFinalize(this);
            if(_enumerable != null)
            {
                foreach(T item in _enumerable)
                {
                    if(item != null)
                    {
                        item.Dispose();
                    }
                }
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _enumerable.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ((System.Collections.IEnumerable)_enumerable).GetEnumerator();
        }
    }
}
