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
    using System.Linq;
    using System.Text;

    internal sealed class NullViewLocationCache : IViewLocationCache {

        #region IViewLocationCache Members
        public string GetViewLocation(HttpContextBase httpContext, string key) {
            return null;
        }

        public void InsertViewLocation(HttpContextBase httpContext, string key, string virtualPath) {
        }
        #endregion
    }
}
