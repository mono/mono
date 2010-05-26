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
    using System.Web.Mvc.Async;

    [SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes",
        Justification = "Unsealed so that subclassed types can set properties in the default constructor.")]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class AsyncTimeoutAttribute : ActionFilterAttribute {

        // duration is specified in milliseconds
        public AsyncTimeoutAttribute(int duration) {
            if (duration < -1) {
                throw Error.AsyncCommon_InvalidTimeout("duration");
            }

            Duration = duration;
        }

        public int Duration {
            get;
            private set;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext) {
            if (filterContext == null) {
                throw new ArgumentNullException("filterContext");
            }

            IAsyncManagerContainer container = filterContext.Controller as IAsyncManagerContainer;
            if (container == null) {
                throw Error.AsyncCommon_ControllerMustImplementIAsyncManagerContainer(filterContext.Controller.GetType());
            }

            container.AsyncManager.Timeout = Duration;

            base.OnActionExecuting(filterContext);
        }

    }
}
