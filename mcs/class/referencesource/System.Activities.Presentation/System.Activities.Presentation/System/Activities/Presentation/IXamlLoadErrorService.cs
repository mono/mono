//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System.Collections.Generic;

    public interface IXamlLoadErrorService
    {
        void ShowXamlLoadErrors(IList<XamlLoadErrorInfo> errors);
    }
}
