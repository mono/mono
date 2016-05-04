//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.Converters
{
    using System.Collections.Generic;

    class ArgumentSearchableStringConverter : SearchableStringConverter
    {
        public override IList<string> Convert(object value)
        {
            IList<string> results = new List<string>();
            if (value is Argument)
            {
                results.Add(ExpressionHelper.GetExpressionString(((Argument)value).Expression));
            }
            return results;
        }
    }
}
