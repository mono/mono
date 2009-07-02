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
    using System.Web.Mvc.Resources;

    public class HandleErrorInfo {

        public HandleErrorInfo(Exception exception, string controllerName, string actionName) {
            if (exception == null) {
                throw new ArgumentNullException("exception");
            }
            if (String.IsNullOrEmpty(controllerName)) {
                throw new ArgumentException(MvcResources.Common_NullOrEmpty, "controllerName");
            }
            if (string.IsNullOrEmpty(actionName)) {
                throw new ArgumentException(MvcResources.Common_NullOrEmpty, "actionName");
            }

            Exception = exception;
            ControllerName = controllerName;
            ActionName = actionName;
        }

        public string ActionName {
            get;
            private set;
        }

        public string ControllerName {
            get;
            private set;
        }

        public Exception Exception {
            get;
            private set;
        }

    }
}
