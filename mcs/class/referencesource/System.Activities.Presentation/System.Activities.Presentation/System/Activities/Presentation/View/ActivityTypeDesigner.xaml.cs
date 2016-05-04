//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.View
{
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.Services;
    using System.Activities.Presentation.Toolbox;
    using System.Activities.Presentation.Xaml;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Threading;

    partial class ActivityTypeDesigner : IExpandChild
    {
        public ActivityTypeDesigner()
        {
            this.InitializeComponent();
            this.DragHandle = null;
        }

        protected override void OnModelItemChanged(object newItem)
        {
            base.OnModelItemChanged(newItem);
            if (this.Context.Services.GetService<DisplayNameUpdater>() == null)
            {
                this.Context.Services.Publish<DisplayNameUpdater>(new DisplayNameUpdater(this.Context));
            }
        }

        protected override void OnContextMenuLoaded(ContextMenu menu)
        {
            base.OnContextMenuLoaded(menu);
            if (null == this.ModelItem.Properties["Implementation"].Value)
            {
                var toHide = menu.Items.OfType<MenuItem>().Where(p =>
                    p.Command == DesignerView.GoToParentCommand ||
                    p.Command == DesignerView.ExpandCommand);

                foreach (var item in toHide)
                {
                    item.Visibility = Visibility.Collapsed;
                }
            }
        }

        void OnAddAnnotationCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (EditingContextUtilities.GetSingleSelectedModelItem(this.Context) == this.ModelItem)
            {
                e.CanExecute = false;
                e.Handled = true;
            }
        }

        void OnEditAnnotationCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (EditingContextUtilities.GetSingleSelectedModelItem(this.Context) == this.ModelItem)
            {
                e.CanExecute = false;
                e.Handled = true;
            }
        }

        void OnDeleteAnnotationCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (EditingContextUtilities.GetSingleSelectedModelItem(this.Context) == this.ModelItem)
            {
                e.CanExecute = false;
                e.Handled = true;
            }
        }

        // the job of this service is to add an attached property on ActivityBuilder called displayName.
        // this name will be shown in breadcrumb bar.
        // also whenever the Name property changes
        // we want to refresh the DisplayName property too ( by calling displayNameProperty.NotifyPropertyChanged())
        class DisplayNameUpdater
        {
            AttachedProperty<string> activityBuilderDisplayNameProperty;
            AttachedProperty<string> activityTemplateFactoryBuilderDisplayNameProperty;

            public DisplayNameUpdater(EditingContext context)
            {
                activityBuilderDisplayNameProperty = new AttachedProperty<string>
                {
                    Name = "DisplayName",
                    OwnerType = typeof(ActivityBuilder),
                    Getter = (modelItem) => ViewUtilities.GetActivityBuilderDisplayName(modelItem)
                };
                activityTemplateFactoryBuilderDisplayNameProperty = new AttachedProperty<string>
                {
                    Name = "DisplayName",
                    OwnerType = typeof(ActivityTemplateFactoryBuilder),
                    Getter = (modelItem) => ViewUtilities.GetActivityBuilderDisplayName(modelItem)
                };
                AttachedPropertiesService attachedPropertiesService = context.Services.GetService<AttachedPropertiesService>();
                attachedPropertiesService.AddProperty(activityBuilderDisplayNameProperty);
                attachedPropertiesService.AddProperty(activityTemplateFactoryBuilderDisplayNameProperty);
                context.Services.GetService<ModelService>().ModelChanged += new EventHandler<ModelChangedEventArgs>(ModelChanged);
            }

            void ModelChanged(object sender, ModelChangedEventArgs e)
            {
                ModelChangeInfo changeInfo = e.ModelChangeInfo;

                if (changeInfo != null && changeInfo.ModelChangeType == ModelChangeType.PropertyChanged)
                {
                    Type propertyType = changeInfo.Subject.ItemType;
                    if (changeInfo.PropertyName == "Name")
                    {
                        if (propertyType.Equals(typeof(ActivityBuilder)))
                        {
                            activityBuilderDisplayNameProperty.NotifyPropertyChanged(changeInfo.Subject);
                        }
                        else if (propertyType.Equals(typeof(ActivityTemplateFactoryBuilder)))
                        {
                            activityTemplateFactoryBuilderDisplayNameProperty.NotifyPropertyChanged(changeInfo.Subject);
                        }
                    }
                }
            }
        }


        public ModelItem ExpandedChild
        {
            get
            {
                ModelItem modelItemToSelect = null;
                if (this.ModelItem != null)
                {
                    modelItemToSelect = this.ModelItem.Properties["Implementation"].Value;
                }
                return modelItemToSelect;
            }
        }

    }
}
