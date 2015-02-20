//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.Converters
{
    using System;
    using System.Collections.Generic;
    using Microsoft.VisualBasic.Activities;

    class VisualBasicValueSearchableStringConverter : SearchableStringConverter
    {
        public override IList<string> Convert(object value)
        {
            if (value == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("value"));
            }

            IList<string> results = new List<string>();
            Type t = value.GetType();
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(VisualBasicValue<>))
            {
                results.Add(ExpressionHelper.GetExpressionString(value as Activity));
            }
            return results;
        }
    }
}
