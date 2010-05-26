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

    [Serializable]
    public class ModelError {

        public ModelError(Exception exception)
            : this(exception, null /* errorMessage */) {
        }

        public ModelError(Exception exception, string errorMessage)
            : this(errorMessage) {
            if (exception == null) {
                throw new ArgumentNullException("exception");
            }

            Exception = exception;
        }

        public ModelError(string errorMessage) {
            ErrorMessage = errorMessage ?? String.Empty;
        }

        public Exception Exception {
            get;
            private set;
        }

        public string ErrorMessage {
            get;
            private set;
        }
    }
}
