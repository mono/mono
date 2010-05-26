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
    using System.Diagnostics.CodeAnalysis;

    public class ActionExecutingContext : ControllerContext {

        // parameterless constructor used for mocking
        public ActionExecutingContext() {
        }

        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors",
            Justification = "The virtual property setters are only to support mocking frameworks, in which case this constructor shouldn't be called anyway.")]
        public ActionExecutingContext(ControllerContext controllerContext, ActionDescriptor actionDescriptor, IDictionary<string, object> actionParameters)
            : base(controllerContext) {
            if (actionDescriptor == null) {
                throw new ArgumentNullException("actionDescriptor");
            }
            if (actionParameters == null) {
                throw new ArgumentNullException("actionParameters");
            }

            ActionDescriptor = actionDescriptor;
            ActionParameters = actionParameters;
        }

        public virtual ActionDescriptor ActionDescriptor {
            get;
            set;
        }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly",
            Justification = "The property setter is only here to support mocking this type and should not be called at runtime.")]
        public virtual IDictionary<string, object> ActionParameters {
            get;
            set;
        }

        public ActionResult Result {
            get;
            set;
        }

    }
}
