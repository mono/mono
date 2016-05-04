//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation.Converters
{
    using System;
    using System.Collections.Generic;

    internal class EmptySearchableStringConverter : SearchableStringConverter
    {
        public override IList<string> Convert(object value)
        {
            return new List<string>();
        }
    }
}
