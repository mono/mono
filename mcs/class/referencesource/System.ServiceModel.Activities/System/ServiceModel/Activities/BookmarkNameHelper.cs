//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Activities
{
    using System.Globalization;
    using System.Runtime;
    using System.Xml.Linq;

    static class BookmarkNameHelper
    {
        public static string CreateBookmarkName(string operationName, XName serviceContractName)
        {
            Fx.Assert(!string.IsNullOrEmpty(operationName), "OperationName cannot be null or empty");
            return string.Format(CultureInfo.InvariantCulture, "{0}|{1}", operationName, serviceContractName);
        }
    }
}
