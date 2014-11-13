//------------------------------------------------------------------------------
// <copyright file="DoubleLinkListEnumerator.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * DoubleLinkList
 * 
 * Copyright (c) 1998-1999, Microsoft Corporation
 * 
 */

namespace System.Web.Util {
    using System.Runtime.Serialization.Formatters;
    using System.Collections;

    internal class DoubleLinkListEnumerator : IEnumerator {
        private DoubleLinkList  _list;
        private DoubleLink      _current;

        internal DoubleLinkListEnumerator(DoubleLinkList list) {
            _list = list;
            _current = list;
        }

        public void Reset() {
            _current = _list;
        }

        public bool MoveNext() {
            if (_current.Next == _list) {
                _current = null;
                return false;
            }

            _current = _current.Next;
            return true;
        }

        public Object Current {
            get { 
                if (_current == null || _current == _list)
                    throw new InvalidOperationException();
                return _current.Item;
            }
        }

        internal DoubleLink GetDoubleLink() {
            return _current;
        }
    }
}
