//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Activities.Presentation.Converters
{
    using System.Activities;
    using System.Activities.Presentation.Converters;
    using System.Collections.Generic;
    using System.ServiceModel.Activities;

    class SendParametersContentSearchableStringConverter : SearchableStringConverter
    {
        public override IList<string> Convert(object value)
        {
            List<string> results = new List<string>();
            SendParametersContent content = value as SendParametersContent;
            if (null != content)
            {
                foreach (KeyValuePair<string, InArgument> param in content.Parameters)
                {
                    results.Add(param.Key);
                    results.Add(param.Value.GetType().Name);
                    results.AddRange(new ArgumentSearchableStringConverter().Convert(param.Value));
                }
            }
            return results;
        }
    }
}
