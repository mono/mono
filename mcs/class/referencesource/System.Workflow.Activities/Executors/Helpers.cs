using System;
using System.Diagnostics;
using System.Reflection;
using System.Collections;
using System.Workflow.ComponentModel;
using System.Workflow.Runtime;
using System.Xml;
using System.Workflow.Activities.Common;

namespace System.Workflow.Activities
{
    internal static class ActivityHelpers
    {
        internal static void InitializeCorrelationTokenCollection(Activity activity, CorrelationToken correlator)
        {
            if (correlator != null && !String.IsNullOrEmpty(correlator.OwnerActivityName))
            {
                string ownerActivityId = correlator.OwnerActivityName;
                Activity owner = activity.GetActivityByName(ownerActivityId);
                if (owner == null)
                    owner = System.Workflow.Activities.Common.Helpers.ParseActivityForBind(activity, ownerActivityId);
                if (owner == null)
                    throw new ArgumentException("ownerActivity");

                CorrelationTokenCollection collection = owner.GetValue(CorrelationTokenCollection.CorrelationTokenCollectionProperty) as CorrelationTokenCollection;
                if (collection == null)
                {
                    collection = new CorrelationTokenCollection();
                    owner.SetValue(CorrelationTokenCollection.CorrelationTokenCollectionProperty, collection);
                }

                if (!collection.Contains(correlator.Name))
                {
                    collection.Add(correlator);
                }
            }
        }
    }
}
