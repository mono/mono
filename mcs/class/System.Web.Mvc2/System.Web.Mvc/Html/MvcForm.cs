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
    using System.IO;

    public class MvcForm : IDisposable {

        private bool _disposed;
        private readonly FormContext _originalFormContext;
        private readonly ViewContext _viewContext;
        private readonly TextWriter _writer;

        [Obsolete("The recommended alternative is the constructor MvcForm(ViewContext viewContext).", true /* error */)]
        public MvcForm(HttpResponseBase httpResponse) {
            if (httpResponse == null) {
                throw new ArgumentNullException("httpResponse");
            }

            _writer = httpResponse.Output;
        }

        public MvcForm(ViewContext viewContext) {
            if (viewContext == null) {
                throw new ArgumentNullException("viewContext");
            }

            _viewContext = viewContext;
            _writer = viewContext.Writer;

            // push the new FormContext
            _originalFormContext = viewContext.FormContext;
            viewContext.FormContext = new FormContext();
        }

        public void Dispose() {
            Dispose(true /* disposing */);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (!_disposed) {
                _disposed = true;
                _writer.Write("</form>");

                // output client validation and restore the original form context
                if (_viewContext != null) {
                    _viewContext.OutputClientValidation();
                    _viewContext.FormContext = _originalFormContext;
                }
            }
        }

        public void EndForm() {
            Dispose(true);
        }

    }
}
