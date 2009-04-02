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

    public class HttpUnauthorizedResult : ActionResult {

        public override void ExecuteResult(ControllerContext context) {
            if (context == null) {
                throw new ArgumentNullException("context");
            }

            // 401 is the HTTP status code for unauthorized access - setting this
            // will cause the active authentication module to execute its default
            // unauthorized handler
            context.HttpContext.Response.StatusCode = 401;
        }
    }
}
