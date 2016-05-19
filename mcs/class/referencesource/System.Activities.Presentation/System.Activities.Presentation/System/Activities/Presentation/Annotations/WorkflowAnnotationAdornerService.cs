//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation.Annotations
{
    using System.Activities.Presentation.Model;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Xaml;

    internal class WorkflowAnnotationAdornerService : AnnotationAdornerService
    {
        private ScrollViewer scrollViewer;
        private bool enabled;

        internal WorkflowAnnotationAdornerService()
        {
        }

        public override void Show(AnnotationAdorner adorner)
        {
            if (!this.enabled)
            {
                return;
            }

            AnnotationAdorner.SetAnchor(adorner, AdornerLocation.None);
            adorner.ScrollViewer = this.scrollViewer;

            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(adorner.AdornedElement);
            if (adornerLayer != null)
            {
                adornerLayer.Add(adorner);
            }

            return;
        }

        public override void Hide(AnnotationAdorner adorner)
        {
            if (!this.enabled)
            {
                return;
            }

            AnnotationAdorner.SetAnchor(adorner, AdornerLocation.None);

            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(adorner.AdornedElement);
            if (adornerLayer != null)
            {
                adornerLayer.Remove(adorner);
            }
            
            return;
        }

        internal void Initialize(EditingContext editingContext, ScrollViewer scrollViewer)
        {
            this.scrollViewer = scrollViewer;
            this.enabled = editingContext.Services.GetService<DesignerConfigurationService>().AnnotationEnabled;

            if (!this.enabled)
            {
                return;
            }

            AttachedPropertiesService attachedPropertiesService = editingContext.Services.GetService<AttachedPropertiesService>();
            AttachedProperty<string> attachedProperty = new AttachedProperty<string>
            {
                IsBrowsable = false,
                IsVisibleToModelItem = true,
                Name = Annotation.AnnotationTextPropertyName,
                OwnerType = typeof(object),
                Getter = (modelItem) =>
                {
                    string annotation = null;
                    AttachablePropertyServices.TryGetProperty<string>(modelItem.GetCurrentValue(), Annotation.AnnotationTextProperty, out annotation);
                    return annotation;
                },
                Setter = (modelItem, value) =>
                {
                    string oldValue = null;
                    AttachablePropertyServices.TryGetProperty<string>(modelItem.GetCurrentValue(), Annotation.AnnotationTextProperty, out oldValue);
                    if (oldValue == value)
                    {
                        return;
                    }

                    ModelTreeManager treeManager = modelItem.GetEditingContext().Services.GetService<ModelTreeManager>();

                    AttachablePropertyChange change = new AttachablePropertyChange()
                    {
                        Owner = modelItem,
                        AttachablePropertyIdentifier = Annotation.AnnotationTextProperty,
                        OldValue = oldValue,
                        NewValue = value,
                        PropertyName = Annotation.AnnotationTextPropertyName
                    };

                    treeManager.AddToCurrentEditingScope(change);
                }
            };

            attachedPropertiesService.AddProperty(attachedProperty);
        }
    }
}
