//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.Model
{

    using System;
    using System.Activities;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime;
    using System.Activities.Presentation.View;
    using System.Windows.Threading;

    class GenericArgumentUpdater
    {
        EditingContext context;
        const string TypeArgumentPropertyName = "TypeArgument";

        public GenericArgumentUpdater(EditingContext context)
        {
            this.context = context;
        }

        public void AddSupportForUpdatingTypeArgument(Type modelItemType)
        {
            AttachedProperty<Type> typeArgumentProperty = new AttachedProperty<Type>
            {
                Name = TypeArgumentPropertyName,
                OwnerType = modelItemType,
                Getter = (modelItem) => modelItem.Parent == null ? null : GetTypeArgument(modelItem),
                Setter = (modelItem, value) => UpdateTypeArgument(modelItem, value),
                IsBrowsable = true
            };
            this.context.Services.GetService<AttachedPropertiesService>().AddProperty(typeArgumentProperty);
        }

        private static void UpdateTypeArgument(ModelItem modelItem, Type value)
        {
            if (value != null)
            {
                Type oldModelItemType = modelItem.ItemType;
                Fx.Assert(oldModelItemType.GetGenericArguments().Count() == 1, "we only support changing a single type parameter ?");
                Type newModelItemType = oldModelItemType.GetGenericTypeDefinition().MakeGenericType(value);
                Fx.Assert(newModelItemType != null, "New model item type needs to be non null or we cannot proceed further");
                ModelItem newModelItem = ModelFactory.CreateItem(modelItem.GetEditingContext(), Activator.CreateInstance(newModelItemType));
                MorphHelper.MorphObject(modelItem, newModelItem);
                MorphHelper.MorphProperties(modelItem, newModelItem);

                if (oldModelItemType.IsSubclassOf(typeof(Activity)) && newModelItemType.IsSubclassOf(typeof(Activity)))
                {
                    if (string.Equals((string)modelItem.Properties["DisplayName"].ComputedValue, GetActivityDefaultName(oldModelItemType), StringComparison.Ordinal))
                    {
                        newModelItem.Properties["DisplayName"].SetValue(GetActivityDefaultName(newModelItemType));
                    }
                }

                DesignerView designerView = modelItem.GetEditingContext().Services.GetService<DesignerView>();
                if (designerView != null)
                {
                    Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Render, (Action)(() =>
                    {
                        if (designerView.RootDesigner != null && ((WorkflowViewElement)designerView.RootDesigner).ModelItem == modelItem)
                        {
                            designerView.MakeRootDesigner(newModelItem, true);
                        }
                        Selection.SelectOnly(modelItem.GetEditingContext(), newModelItem);
                    }));
                }
            }
        }

        private static Type GetTypeArgument(ModelItem modelItem)
        {
            Fx.Assert(modelItem.ItemType.GetGenericArguments().Count() == 1, "we only support changing a single type parameter ?");
            return modelItem.ItemType.GetGenericArguments()[0];
        }

        private static string GetActivityDefaultName(Type activityType)
        {
            Fx.Assert(activityType.IsSubclassOf(typeof(Activity)), "activityType is not a subclass of System.Activities.Activity");
            Activity activity = (Activity)Activator.CreateInstance(activityType);
            return activity.DisplayName;
        }
    }



}
