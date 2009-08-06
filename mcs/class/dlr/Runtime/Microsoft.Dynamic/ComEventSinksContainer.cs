/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/
using System; using Microsoft;


#if !SILVERLIGHT // ComObject

using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;

#if CODEPLEX_40
namespace System.Dynamic {
#else
namespace Microsoft.Scripting {
#endif
    /// <summary>
    /// ComEventSinksContainer is just a regular list with a finalizer.
    /// This list is usually attached as a custom data for RCW object and 
    /// is finalized whenever RCW is finalized.
    /// </summary>
    internal class ComEventSinksContainer : List<ComEventSink>, IDisposable {
        private ComEventSinksContainer() {
        }

        private static readonly object _ComObjectEventSinksKey = new object();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        [SecurityCritical]
        public static ComEventSinksContainer FromRuntimeCallableWrapper(object rcw, bool createIfNotFound) {
            // !!! Marshal.Get/SetComObjectData has a LinkDemand for UnmanagedCode which will turn into
            // a full demand. We need to avoid this by making this method SecurityCritical
            object data = Marshal.GetComObjectData(rcw, _ComObjectEventSinksKey);
            if (data != null || createIfNotFound == false) {
                return (ComEventSinksContainer)data;
            }

            lock (_ComObjectEventSinksKey) {
                data = Marshal.GetComObjectData(rcw, _ComObjectEventSinksKey);
                if (data != null) {
                    return (ComEventSinksContainer)data;
                }

                ComEventSinksContainer comEventSinks = new ComEventSinksContainer();
                if (!Marshal.SetComObjectData(rcw, _ComObjectEventSinksKey, comEventSinks)) {
                    throw Error.SetComObjectDataFailed();
                }

                return comEventSinks;
            }
        }

        #region IDisposable Members

#if CLR2
        [SecurityCritical, SecurityTreatAsSafe]
#else
        [SecuritySafeCritical]
#endif
        public void Dispose() {
            DisposeAll();
            GC.SuppressFinalize(this);
        }

        #endregion

        [SecurityCritical]
        private void DisposeAll() {
            foreach (ComEventSink sink in this) {
                sink.Dispose();
            }
        }

#if CLR2
        [SecurityCritical, SecurityTreatAsSafe]
#else
        [SecuritySafeCritical]
#endif
        ~ComEventSinksContainer() {
            DisposeAll();
        }
    }
}

#endif
