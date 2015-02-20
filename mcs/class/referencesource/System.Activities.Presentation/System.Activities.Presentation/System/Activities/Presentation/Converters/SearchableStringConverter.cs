//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.Converters
{
    using System.Collections.Generic;

    abstract class SearchableStringConverter
    {
        public abstract IList<string> Convert(object value);
    }
}
