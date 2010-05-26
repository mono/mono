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

    public class AuthorizationContext : ControllerContext {

        // parameterless constructor used for mocking
        public AuthorizationContext() {
        }

        [Obsolete("The recommended alternative is the constructor AuthorizationContext(ControllerContext controllerContext, ActionDescriptor actionDescriptor).")]
        public AuthorizationContext(ControllerContext controllerContext)
            : base(controllerContext) {
        }

        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors",
            Justification = "The virtual property setters are only to support mocking frameworks, in which case this constructor shouldn't be called anyway.")]
        public AuthorizationContext(ControllerContext controllerContext, ActionDescriptor actionDescriptor)
            : base(controllerContext) {
            if (actionDescriptor == null) {
                throw new ArgumentNullException("actionDescriptor");
            }

            ActionDescriptor = actionDescriptor;
        }

        public virtual ActionDescriptor ActionDescriptor {
            get;
            set;
        }

        public ActionResult Result {
            get;
            set;
        }

    }
}
