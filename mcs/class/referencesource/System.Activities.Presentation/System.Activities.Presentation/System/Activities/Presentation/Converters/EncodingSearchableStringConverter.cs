//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation.Converters
{
    using System.Collections.Generic;
    using System.Text;

    internal class EncodingSearchableStringConverter : SearchableStringConverter
    {
        public override IList<string> Convert(object value)
        {
            IList<string> results = new List<string>();
            if (value is Encoding)
            {
                results.Add(((Encoding)value).EncodingName.ToString());
            }

            return results;
        }
    }
}
