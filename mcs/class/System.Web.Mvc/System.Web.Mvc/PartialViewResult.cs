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
    using System.Globalization;
    using System.Text;
    using System.Web.Mvc.Resources;

    public class PartialViewResult : ViewResultBase {

        protected override ViewEngineResult FindView(ControllerContext context) {
            ViewEngineResult result = ViewEngineCollection.FindPartialView(context, ViewName);
            if (result.View != null) {
                return result;
            }

            // we need to generate an exception containing all the locations we searched
            StringBuilder locationsText = new StringBuilder();
            foreach (string location in result.SearchedLocations) {
                locationsText.AppendLine();
                locationsText.Append(location);
            }
            throw new InvalidOperationException(String.Format(CultureInfo.CurrentUICulture,
                MvcResources.Common_PartialViewNotFound, ViewName, locationsText));
        }
    }
}
