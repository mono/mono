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
    using System.Web;

    public class FileContentResult : FileResult {

        public FileContentResult(byte[] fileContents, string contentType)
            : base(contentType) {
            if (fileContents == null) {
                throw new ArgumentNullException("fileContents");
            }

            FileContents = fileContents;
        }

        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays",
            Justification = "There's no reason to tamper-proof this array since it's supplied to the type's constructor.")]
        public byte[] FileContents {
            get;
            private set;
        }

        protected override void WriteFile(HttpResponseBase response) {
            response.OutputStream.Write(FileContents, 0, FileContents.Length);
        }

    }
}
