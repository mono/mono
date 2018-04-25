// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.Activities.DynamicUpdate
{
    using System;
    using System.Xaml;

    public static class DynamicUpdateInfo
    {
        private static AttachableMemberIdentifier mapItemProperty = new AttachableMemberIdentifier(typeof(DynamicUpdateInfo), "MapItem");
        private static AttachableMemberIdentifier originalDefinitionProperty = new AttachableMemberIdentifier(typeof(DynamicUpdateInfo), "OriginalDefinition");
        private static AttachableMemberIdentifier originalActivityBuilderProperty = new AttachableMemberIdentifier(typeof(DynamicUpdateInfo), "OriginalActivityBuilder");

        public static void SetMapItem(object instance, DynamicUpdateMapItem mapItem)
        {
            if (mapItem != null)
            {
                AttachablePropertyServices.SetProperty(instance, mapItemProperty, mapItem);
            }
            else
            {
                AttachablePropertyServices.RemoveProperty(instance, mapItemProperty);
            }
        }

        public static DynamicUpdateMapItem GetMapItem(object instance)
        {
            DynamicUpdateMapItem result;
            if (AttachablePropertyServices.TryGetProperty(instance, mapItemProperty, out result))
            {
                return result;
            }
            else
            {
                return null;
            }
        }

        public static void SetOriginalDefinition(object instance, Activity originalDefinition)
        {
            if (originalDefinition != null)
            {
                AttachablePropertyServices.SetProperty(instance, originalDefinitionProperty, originalDefinition);
            }
            else
            {
                AttachablePropertyServices.RemoveProperty(instance, originalDefinitionProperty);
            }
        }

        public static Activity GetOriginalDefinition(object instance)
        {
            Activity result;
            if (AttachablePropertyServices.TryGetProperty(instance, originalDefinitionProperty, out result))
            {
                return result;
            }
            else
            {
                return null;
            }
        }

        public static void SetOriginalActivityBuilder(object instance, ActivityBuilder originalActivityBuilder)
        {
            if (originalActivityBuilder != null)
            {
                AttachablePropertyServices.SetProperty(instance, originalActivityBuilderProperty, originalActivityBuilder);
            }
            else
            {
                AttachablePropertyServices.RemoveProperty(instance, originalActivityBuilderProperty);
            }
        }

        public static ActivityBuilder GetOriginalActivityBuilder(object instance)
        {
            ActivityBuilder result;
            if (AttachablePropertyServices.TryGetProperty(instance, originalActivityBuilderProperty, out result))
            {
                return result;
            }
            else
            {
                return null;
            }
        }       
    }
}
