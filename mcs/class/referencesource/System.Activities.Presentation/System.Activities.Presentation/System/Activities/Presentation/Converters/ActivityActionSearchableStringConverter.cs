//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.Converters
{
    using System.Collections.Generic;

    class ActivityActionSearchableStringConverter<T> : SearchableStringConverter
    {
        public override IList<string> Convert(object value)
        {
            ActivityAction<T> action = value as ActivityAction<T>;
            IList<string> results = new List<string>();
            if (action != null)
            {
                results.Add(action.Argument.Name);
            }
            return results;
        }
    }
}
