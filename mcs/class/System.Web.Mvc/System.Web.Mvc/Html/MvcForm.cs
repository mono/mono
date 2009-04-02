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

namespace System.Web.Mvc.Html {
    using System;
    using System.Diagnostics.CodeAnalysis;

    public class MvcForm : IDisposable {
        private bool _disposed;
        private readonly HttpResponseBase _httpResponse;

        public MvcForm(HttpResponseBase httpResponse) {
            if (httpResponse == null) {
                throw new ArgumentNullException("httpResponse");
            }
            _httpResponse = httpResponse;
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public void Dispose() {
            Dispose(true /* disposing */);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (!_disposed) {
                _disposed = true;
                _httpResponse.Write("</form>");
            }
        }

        public void EndForm() {
            Dispose(true);
        }
    }
}
