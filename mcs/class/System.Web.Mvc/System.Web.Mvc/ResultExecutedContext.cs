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
    using System.Diagnostics.CodeAnalysis;

    public class ResultExecutedContext : ControllerContext {

        // parameterless constructor used for mocking
        public ResultExecutedContext() {
        }

        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors",
            Justification = "The virtual property setters are only to support mocking frameworks, in which case this constructor shouldn't be called anyway.")]
        public ResultExecutedContext(ControllerContext controllerContext, ActionResult result, bool canceled, Exception exception)
            : base(controllerContext) {
            if (result == null) {
                throw new ArgumentNullException("result");
            }

            Result = result;
            Canceled = canceled;
            Exception = exception;
        }

        public virtual bool Canceled {
            get;
            set;
        }

        public virtual Exception Exception {
            get;
            set;
        }

        public bool ExceptionHandled {
            get;
            set;
        }

        public virtual ActionResult Result {
            get;
            set;
        }

    }
}
