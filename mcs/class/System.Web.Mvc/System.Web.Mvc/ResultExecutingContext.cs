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

    public class ResultExecutingContext : ControllerContext {

        // parameterless constructor used for mocking
        public ResultExecutingContext() {
        }

        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors",
            Justification = "The virtual property setters are only to support mocking frameworks, in which case this constructor shouldn't be called anyway.")]
        public ResultExecutingContext(ControllerContext controllerContext, ActionResult result)
            : base(controllerContext) {
            if (result == null) {
                throw new ArgumentNullException("result");
            }

            Result = result;
        }

        public bool Cancel {
            get;
            set;
        }

        public virtual ActionResult Result {
            get;
            set;
        }

    }
}
