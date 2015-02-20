//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.Converters
{
    using System;
    using System.Activities.Presentation.View;
    using System.Collections.Generic;

    class TypeSearchableStringConverter : SearchableStringConverter
    {
        public override IList<string> Convert(object value)
        {
            IList<string> results = new List<string>();
            if (value is Type)
            {
                results.Add(TypePresenter.ResolveTypeName(value as Type));
            }
            return results;
        }
    }
}
