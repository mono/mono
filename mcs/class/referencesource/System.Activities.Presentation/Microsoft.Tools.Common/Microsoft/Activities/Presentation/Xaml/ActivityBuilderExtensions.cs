// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace Microsoft.Activities.Presentation.Xaml
{
    using System;
    using System.Activities;
    using Microsoft.VisualBasic.Activities;

    internal static class ActivityBuilderExtensions
    {
        internal static DynamicActivity ConvertToDynamicActivity(this ActivityBuilder activityBuilder)
        {
            DynamicActivity result = new DynamicActivity();
            ActivityBuilderExtensions.ConvertActivityBuilderToDynamicActivity(activityBuilder, result);
            return result;
        }

        internal static void ConvertActivityBuilderToDynamicActivity(ActivityBuilder activityBuilder, DynamicActivity bodyPlaceholder)
        {
            bodyPlaceholder.Name = activityBuilder.Name;
            bodyPlaceholder.Implementation = () => activityBuilder.Implementation;

            if (activityBuilder.Implementation != null)
            {
                VisualBasic.SetSettings(bodyPlaceholder, VisualBasic.GetSettings(activityBuilder));
            }

            bodyPlaceholder.Attributes.Clear();
            foreach (Attribute attribute in activityBuilder.Attributes)
            {
                bodyPlaceholder.Attributes.Add(attribute);
            }

            bodyPlaceholder.Properties.Clear();
            foreach (DynamicActivityProperty property in activityBuilder.Properties)
            {
                bodyPlaceholder.Properties.Add(property);
            }
        }
    }
}
