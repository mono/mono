//------------------------------------------------------------------------------
// <copyright file="IHttpResponseInternal.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web {
    using System.IO;

    internal abstract class HttpResponseInternalBase : HttpResponseBase {
        public virtual TextWriter SwitchWriter(TextWriter writer) {
            throw new NotImplementedException();
        }
    }
}
