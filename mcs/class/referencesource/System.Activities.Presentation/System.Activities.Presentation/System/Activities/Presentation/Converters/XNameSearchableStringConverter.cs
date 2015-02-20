//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.Converters
{
    using System.Collections.Generic;
    using System.Xml.Linq;

    class XNameSearchableStringConverter : SearchableStringConverter
    {
        public override IList<string> Convert(object value)
        {
            IList<string> results = new List<string>();
            if (value is XName)
            {
                results.Add(new XNameConverter().ConvertToString(value));
            }
            return results;
        }
    }
}
