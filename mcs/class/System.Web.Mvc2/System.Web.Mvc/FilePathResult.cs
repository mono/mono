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
    using System.Web;
    using System.Web.Mvc.Resources;

    public class FilePathResult : FileResult {

        public FilePathResult(string fileName, string contentType)
            : base(contentType) {
            if (String.IsNullOrEmpty(fileName)) {
                throw new ArgumentException(MvcResources.Common_NullOrEmpty, "fileName");
            }

            FileName = fileName;
        }

        public string FileName {
            get;
            private set;
        }

        protected override void WriteFile(HttpResponseBase response) {
            response.TransmitFile(FileName);
        }

    }
}
