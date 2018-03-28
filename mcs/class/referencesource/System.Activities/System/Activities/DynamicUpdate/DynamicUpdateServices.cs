//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.DynamicUpdate
{
    using System;
    using System.IO;
    using System.Activities;
    using System.Activities.Expressions;
    using System.Activities.DynamicUpdate;
    using System.Activities.Hosting;
    using System.Activities.Runtime;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Xaml;
    using System.Activities.Validation;
    using Microsoft.VisualBasic.Activities;

    public static class DynamicUpdateServices
    {
        private static Func<Activity, Exception> onInvalidActivityToBlockUpdate =
            new Func<Activity, Exception>(OnInvalidActivityToBlockUpdate);

        private static Func<Activity, Exception> onInvalidImplementationMapAssociation =
            new Func<Activity, Exception>(OnInvalidImplementationMapAssociation);

        private static AttachableMemberIdentifier implementationMapProperty = new AttachableMemberIdentifier(typeof(DynamicUpdateServices), "ImplementationMap");

        public static void PrepareForUpdate(Activity workflowDefinitionToBeUpdated)
        {
            if (workflowDefinitionToBeUpdated == null)
            {
                throw FxTrace.Exception.ArgumentNull("workflowDefinitionToBeUpdated");
            }

            InternalPrepareForUpdate(workflowDefinitionToBeUpdated, false);
        }

        public static void PrepareForUpdate(ActivityBuilder activityDefinitionToBeUpdated)
        {
            if (activityDefinitionToBeUpdated == null)
            {
                throw FxTrace.Exception.ArgumentNull("activityDefinitionToBeUpdated");
            }

            InternalPrepareForUpdate(activityDefinitionToBeUpdated, true);
        }

        private static void InternalPrepareForUpdate(object definitionToBeUpdated, bool forImplementation)
        {
            // Clone the definition
            object clone;
            using (XamlObjectReader reader = new XamlObjectReader(definitionToBeUpdated))
            {
                using (XamlObjectWriter writer = new XamlObjectWriter(reader.SchemaContext))
                {
                    XamlServices.Transform(reader, writer);
                    clone = writer.Result;
                }
            }

            // Calculate the match info
            // Set the match info as attached properties so it is serializable,
            // and available when the user calls CreateUpdateMap

            IDictionary<object, DynamicUpdateMapItem> mapItems;
            if (!forImplementation)
            {
                DynamicUpdateInfo.SetOriginalDefinition(definitionToBeUpdated, (Activity)clone);
                mapItems = DynamicUpdateMap.CalculateMapItems((Activity)definitionToBeUpdated);
            }
            else
            {
                DynamicUpdateInfo.SetOriginalActivityBuilder(definitionToBeUpdated, (ActivityBuilder)clone);
                mapItems = DynamicUpdateMap.CalculateImplementationMapItems(GetDynamicActivity((ActivityBuilder)definitionToBeUpdated));                
            }

            foreach (KeyValuePair<object, DynamicUpdateMapItem> objectInfo in mapItems)
            {
                DynamicUpdateInfo.SetMapItem(objectInfo.Key, objectInfo.Value);
            }
        }

        public static DynamicUpdateMap CreateUpdateMap(Activity updatedWorkflowDefinition)
        {
            return CreateUpdateMap(updatedWorkflowDefinition, null);
        }

        public static DynamicUpdateMap CreateUpdateMap(Activity updatedWorkflowDefinition, IEnumerable<Activity> disallowUpdateInsideActivities)
        {
            IList<ActivityBlockingUpdate> activitiesBlockingUpdate;
            return CreateUpdateMap(updatedWorkflowDefinition, disallowUpdateInsideActivities, out activitiesBlockingUpdate);
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.AvoidOutParameters, Justification = "Approved Design. Need to return the map and the block list.")]
        public static DynamicUpdateMap CreateUpdateMap(Activity updatedWorkflowDefinition, IEnumerable<Activity> disallowUpdateInsideActivities, out IList<ActivityBlockingUpdate> activitiesBlockingUpdate)
        {
            if (updatedWorkflowDefinition == null)
            {
                throw FxTrace.Exception.ArgumentNull("updatedWorkflowDefinition");
            }

            Activity originalDefinition = DynamicUpdateInfo.GetOriginalDefinition(updatedWorkflowDefinition);
            if (originalDefinition == null)
            {
                throw FxTrace.Exception.Argument("updatedWorkflowDefinition", SR.MustCallPrepareBeforeFinalize);
            }

            DynamicUpdateMap result = InternalTryCreateUpdateMap(updatedWorkflowDefinition, originalDefinition, disallowUpdateInsideActivities, false, out activitiesBlockingUpdate);
            // Remove the DynamicUpdateMapItems now that the update is finalized
            // Calling CalculateMapItems is actually an unnecessary perf hit since it calls CacheMetadata
            // again; but we do it so that Finalize is implemented purely in terms of other public APIs.
            DynamicUpdateInfo.SetOriginalDefinition(updatedWorkflowDefinition, null);
            IDictionary<object, DynamicUpdateMapItem> mapItems = DynamicUpdateMap.CalculateMapItems(updatedWorkflowDefinition);
            foreach (object matchObject in mapItems.Keys)
            {
                DynamicUpdateInfo.SetMapItem(matchObject, null);
            }

            return result;
        }

        public static DynamicUpdateMap CreateUpdateMap(ActivityBuilder updatedActivityDefinition)
        {
            return CreateUpdateMap(updatedActivityDefinition, null);
        }

        public static DynamicUpdateMap CreateUpdateMap(ActivityBuilder updatedActivityDefinition, IEnumerable<Activity> disallowUpdateInsideActivities)
        {
            IList<ActivityBlockingUpdate> activitiesBlockingUpdate;
            return CreateUpdateMap(updatedActivityDefinition, disallowUpdateInsideActivities, out activitiesBlockingUpdate);
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.AvoidOutParameters, Justification = "Approved Design. Need to return the map and the block list.")]
        public static DynamicUpdateMap CreateUpdateMap(ActivityBuilder updatedActivityDefinition, IEnumerable<Activity> disallowUpdateInsideActivities, out IList<ActivityBlockingUpdate> activitiesBlockingUpdate)
        {
            if (updatedActivityDefinition == null)
            {
                throw FxTrace.Exception.ArgumentNull("updatedActivityDefinition");
            }

            ActivityBuilder originalActivityDefinition = DynamicUpdateInfo.GetOriginalActivityBuilder(updatedActivityDefinition);
            if (originalActivityDefinition == null)
            {
                throw FxTrace.Exception.Argument("updatedActivityDefinition", SR.MustCallPrepareBeforeFinalize);
            }

            Activity originalBuiltRoot = GetDynamicActivity(originalActivityDefinition);
            Activity updatedBuiltRoot = GetDynamicActivity(updatedActivityDefinition);

            DynamicUpdateMap result = InternalTryCreateUpdateMap(updatedBuiltRoot, originalBuiltRoot, disallowUpdateInsideActivities, true, out activitiesBlockingUpdate);
            // Remove the DynamicUpdateMapItems now that the update is finalized
            // Calling CalculateMapItems is actually an unnecessary perf hit since it calls CacheMetadata
            // again; but we do it so that Finalize is implemented purely in terms of other public APIs.
            DynamicUpdateInfo.SetOriginalActivityBuilder(updatedActivityDefinition, null);
            IDictionary<object, DynamicUpdateMapItem> mapItems = DynamicUpdateMap.CalculateImplementationMapItems(updatedBuiltRoot);
            foreach (object matchObject in mapItems.Keys)
            {
                DynamicUpdateInfo.SetMapItem(matchObject, null);
            }

            return result;
        }

        private static DynamicUpdateMap InternalTryCreateUpdateMap(Activity updatedDefinition, Activity originalDefinition, IEnumerable<Activity> disallowUpdateInsideActivities, bool forImplementation, out IList<ActivityBlockingUpdate> activitiesBlockingUpdate)
        {
            DynamicUpdateMapBuilder builder = new DynamicUpdateMapBuilder
            {
                ForImplementation = forImplementation,
                LookupMapItem = DynamicUpdateInfo.GetMapItem,
                LookupImplementationMap = GetImplementationMap,
                UpdatedWorkflowDefinition = updatedDefinition,
                OriginalWorkflowDefinition = originalDefinition,
                OnInvalidActivityToBlockUpdate = onInvalidActivityToBlockUpdate,
                OnInvalidImplementationMapAssociation = onInvalidImplementationMapAssociation,
            };
            if (disallowUpdateInsideActivities != null)
            {
                foreach (Activity activity in disallowUpdateInsideActivities)
                {
                    builder.DisallowUpdateInside.Add(activity);
                }
            }

            return builder.CreateMap(out activitiesBlockingUpdate);
        }

        public static DynamicUpdateMap GetImplementationMap(Activity targetActivity)
        {
            DynamicUpdateMap result;
            if (AttachablePropertyServices.TryGetProperty(targetActivity, implementationMapProperty, out result))
            {
                return result;
            }
            else
            {
                return null;
            }
        }

        public static void SetImplementationMap(Activity targetActivity, DynamicUpdateMap implementationMap)
        {
            if (implementationMap != null)
            {
                AttachablePropertyServices.SetProperty(targetActivity, implementationMapProperty, implementationMap);
            }
            else
            {
                AttachablePropertyServices.RemoveProperty(targetActivity, implementationMapProperty);
            }
        }

        static DynamicActivity GetDynamicActivity(ActivityBuilder activityDefinition)
        {
            DynamicActivity result = new DynamicActivity
            {
                Name = activityDefinition.Name
            };
            foreach (DynamicActivityProperty property in activityDefinition.Properties)
            {
                result.Properties.Add(property);
            }
            foreach (Attribute attrib in activityDefinition.Attributes)
            {
                result.Attributes.Add(attrib);
            }
            foreach (Constraint constraint in activityDefinition.Constraints)
            {
                result.Constraints.Add(constraint);
            }
            result.Implementation = () => activityDefinition.Implementation;

            VisualBasicSettings vbsettings = VisualBasic.GetSettings(activityDefinition);
            if (vbsettings != null)
            {
                VisualBasic.SetSettings(result, vbsettings);
            }

            IList<string> namespacesForImplementation = TextExpression.GetNamespacesForImplementation(activityDefinition);
            if (namespacesForImplementation.Count > 0)
            {
                TextExpression.SetNamespacesForImplementation(result, namespacesForImplementation);
            }

            IList<AssemblyReference> referencesForImplementation = TextExpression.GetReferencesForImplementation(activityDefinition);
            if (referencesForImplementation.Count > 0)
            {
                TextExpression.SetReferencesForImplementation(result, referencesForImplementation);
            }

            return result;
        }

        static Exception OnInvalidActivityToBlockUpdate(Activity activity)
        {
            return new ArgumentException(SR.InvalidActivityToBlockUpdateServices(activity), "disallowUpdateInsideActivities");
        }

        static Exception OnInvalidImplementationMapAssociation(Activity activity)
        {
            return new InvalidOperationException(SR.InvalidImplementationMapAssociationServices(activity));
        }
    }
}
