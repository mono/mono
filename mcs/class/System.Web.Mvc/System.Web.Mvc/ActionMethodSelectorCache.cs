/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This software is subject to the Microsoft Public License (Ms-PL). 
 * A copy of the license can be found in the license.htm file included 
 * in this distribution.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

namespace System.Web.Mvc {
    using System;
    using System.Collections.Generic;
    using System.Threading;

    internal sealed class ActionMethodSelectorCache {

        private ReaderWriterLock _rwLock = new ReaderWriterLock();
        private Dictionary<Type, ActionMethodSelector> _selectorDictionary = new Dictionary<Type, ActionMethodSelector>();

        // This method could potentially return multiple selectors for the same controllerType due to
        // upgrading the lock from reader to writer, but the dictionary won't be corrupted as a result.
        public ActionMethodSelector GetSelector(Type controllerType) {
            ActionMethodSelector selector;

            _rwLock.AcquireReaderLock(Timeout.Infinite);
            try {
                if (_selectorDictionary.TryGetValue(controllerType, out selector)) {
                    return selector;
                }
            }
            finally {
                _rwLock.ReleaseReaderLock();
            }

            // if we got this far, the selector was not in the cache
            selector = new ActionMethodSelector(controllerType);
            _rwLock.AcquireWriterLock(Timeout.Infinite);
            try {
                _selectorDictionary[controllerType] = selector;
                return selector;
            }
            finally {
                _rwLock.ReleaseWriterLock();
            }
        }

    }
}
