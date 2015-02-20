//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;

    [Serializable]
    public class ClipboardData
    {
        [SuppressMessage(FxCop.Category.Usage, FxCop.Rule.CollectionPropertiesShouldBeReadOnly,
            Justification = "The setter of this property is used several places.")]
        public List<object> Data { get; set;  }

        [SuppressMessage(FxCop.Category.Usage, FxCop.Rule.CollectionPropertiesShouldBeReadOnly,
            Justification = "The setter of this property is used several places.")]
        public List<object> Metadata { get; set;  }

        public string Version { get; set; }
    }


}
