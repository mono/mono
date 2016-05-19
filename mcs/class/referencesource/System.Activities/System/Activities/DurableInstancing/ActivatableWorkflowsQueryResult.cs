//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.DurableInstancing
{
    using System;
    using System.Linq;
    using System.Runtime;
    using System.Runtime.Collections;
    using System.Runtime.DurableInstancing;
    using System.Xml.Linq;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    [Fx.Tag.XamlVisible(false)]
    public sealed class ActivatableWorkflowsQueryResult : InstanceStoreQueryResult
    {
        static readonly ReadOnlyDictionaryInternal<XName, object> emptyDictionary = new ReadOnlyDictionaryInternal<XName, object>(new Dictionary<XName, object>(0));

        public ActivatableWorkflowsQueryResult()
        {
            ActivationParameters = new List<IDictionary<XName, object>>(0);
        }

        public ActivatableWorkflowsQueryResult(IDictionary<XName, object> parameters)
        {
            ActivationParameters = new List<IDictionary<XName, object>>
                { parameters == null ? ActivatableWorkflowsQueryResult.emptyDictionary : new ReadOnlyDictionaryInternal<XName, object>(new Dictionary<XName, object>(parameters)) };
        }

        public ActivatableWorkflowsQueryResult(IEnumerable<IDictionary<XName, object>> parameters)
        {
            if (parameters == null)
            {
                ActivationParameters = new List<IDictionary<XName, object>>(0);
            }
            else
            {
                ActivationParameters = new List<IDictionary<XName, object>>(parameters.Select(dictionary =>
                    dictionary == null ? ActivatableWorkflowsQueryResult.emptyDictionary : new ReadOnlyDictionaryInternal<XName, object>(new Dictionary<XName, object>(dictionary))));
            }
        }

        public List<IDictionary<XName, object>> ActivationParameters
        {
            get;
            private set;
        }
    }
}
