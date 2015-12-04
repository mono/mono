//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Activities.Presentation.Converters
{
    using System.Activities;
    using System.Activities.Presentation.Converters;
    using System.Collections.Generic;
    using System.ServiceModel;

    class XPathMessageQuerySearchableStringConverter : SearchableStringConverter
    {
        public override IList<string> Convert(object value)
        {
            IList<string> results = new List<string>();
            XPathMessageQuery messageQuery = value as XPathMessageQuery;
            if (messageQuery != null)
            {
                results.Add(messageQuery.Expression);
            }
            return results;
        }
    }
}
