//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.View
{
    using System.Activities.Presentation;
    using System.Activities.Presentation.Internal.PropertyEditing;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.Services;
    using System.Activities.XamlIntegration;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Runtime;
    using System.ServiceModel.Activities;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Collections.Generic;

    [Fx.Tag.XamlVisible(false)]
    public class WorkflowViewService : ViewService
    {
        EditingContext context;

        public WorkflowViewService(EditingContext context)
        {
            Fx.Assert(context != null, "The passed in EditingContext is null");
            this.context = context;
        }
        public event EventHandler<ViewCreatedEventArgs> ViewCreated;

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "Catch all exceptions to prevent crash and always return the error view of the designer with the error message")]
        [SuppressMessage("Reliability", "Reliability108:IsFatalRule",
            Justification = "Catch all exceptions to prevent crash and always return the error view of the designer with the error message")]
        public WorkflowViewElement GetViewElement(ModelItem modelItem)
        {
            WorkflowViewElement viewElement = null;
            string errorString = string.Empty;
            if (modelItem == null)
            {
                return null;
            }

            try
            {
                // try to make one from the type specified in Designer attribute.
                // reuse existing views that are not currently parented
                if (modelItem.View != null && ((WorkflowViewElement)modelItem.View).Parent == null)
                {
                    viewElement = (WorkflowViewElement)modelItem.View;
                }
                else
                {
                    viewElement = CreateViewElement(modelItem);
                }

                // now we successfully got a viewElement, lets initialize it with ModelItem;
                if (viewElement != null)
                {
                    viewElement.Context = this.context;
                    viewElement.ModelItem = modelItem;
                    ((IModelTreeItem)modelItem).SetCurrentView(viewElement);
                    viewElement.DataContext = viewElement;

                    // Generate an event that we created a new view element.  This could be used by 
                    // the Debugger Service to insert a breakpoint on the view element.
                    if (this.ViewCreated != null)
                    {
                        this.ViewCreated(this, new ViewCreatedEventArgs(viewElement));
                    }
                }
            }
            // never crash here
            // always report error to the customer.
            catch (Exception e)
            {
                errorString = e.ToString();
            }
            if (viewElement == null || !(string.IsNullOrEmpty(errorString)))
            {
                viewElement = GenerateErrorElement(modelItem, errorString);
            }
            return viewElement;
        }

        internal static void ShowErrorInViewElement(WorkflowViewElement errorElement, string windowText, string toolTipText)
        {
            Grid errorGrid = new Grid();
            errorGrid.Background = Brushes.Red;
            errorGrid.Margin = new Thickness(20.0);
            TextBlock text = new TextBlock();
            text.Text = windowText;
            text.Foreground = SystemColors.WindowBrush;
            errorGrid.Children.Add(text);
            errorGrid.ToolTip = toolTipText;
            errorElement.Content = errorGrid;
        }

        private WorkflowViewElement GenerateErrorElement(ModelItem modelItem, string errorString)
        {
            WorkflowViewElement errorElement = new WorkflowViewElement();
            string errorText = string.Format(CultureInfo.CurrentCulture, SR.CouldNotGenerateView, modelItem.ItemType.Name);
            ShowErrorInViewElement(errorElement, errorText, errorString);
            errorElement.Context = this.context;
            errorElement.ModelItem = modelItem;
            ((IModelTreeItem)modelItem).SetCurrentView(errorElement);
            errorElement.DataContext = errorElement;
            return errorElement;
        }

        DesignerAttribute GetDesignerAttribute(Type type)
        {
            DesignerAttribute designerAttribute = null;
            //do not return designers for IValueSerializableExpression (i.e. VisualBasicValue or VisualBasicReference
            if (!typeof(IValueSerializableExpression).IsAssignableFrom(type))
            {
                designerAttribute = GetAttribute<DesignerAttribute>(type);
            }
            return designerAttribute;
        }

        internal static T GetAttribute<T>(Type type) where T : Attribute
        {
            T attribute = ExtensibilityAccessor.GetAttribute<T>(type);
            if (attribute == null && type.IsGenericType)
            {
                attribute = ExtensibilityAccessor.GetAttribute<T>(type.GetGenericTypeDefinition());
            }
            return attribute;
        }

        //Returns the designer type based on the DesignerAttribute associated with the passed in type.
        internal Type GetDesignerType(Type type)
        {
            return GetDesignerType(type, false);
        }

        internal Type GetDesignerType(Type type, bool throwOnFailure)
        {
            Type designerType = null;
            // Try to identify a designer using the DesignerAttribute, either on the type or from MetaDataStore
            DesignerAttribute designerAttribute = GetDesignerAttribute(type);
            if (designerAttribute != null && !String.IsNullOrEmpty(designerAttribute.DesignerTypeName))
            {
                designerType = Type.GetType(designerAttribute.DesignerTypeName, throwOnFailure);

                //if we have generic activity, check if there is a designer defined at type definition i.e. Assign<T>,
                //rather then using a default one (which happens to be ActivityDesigner)
                if (type.IsGenericType && Type.Equals(designerType, typeof(ActivityDesigner)))
                {
                    Type genericType = type.GetGenericTypeDefinition();
                    DesignerAttribute genericDesignerAttribute =
                        TypeDescriptor.GetAttributes(genericType)[typeof(DesignerAttribute)] as DesignerAttribute;
                    designerType =
                        (null == genericDesignerAttribute ?
                        designerType : Type.GetType(genericDesignerAttribute.DesignerTypeName, throwOnFailure));
                }
            }
            return designerType;
        }

        protected WorkflowViewElement CreateViewElement(ModelItem modelItem)
        {
            Fx.Assert(modelItem != null, "The passed in ModelItem is null");
            WorkflowViewElement viewElement = null;

            Type designerType = GetDesignerType(modelItem.ItemType, true);

            if (designerType != null && typeof(WorkflowViewElement).IsAssignableFrom(designerType))
            {
                viewElement = (WorkflowViewElement)Activator.CreateInstance(designerType);
            }
            return viewElement;
        }

        internal WorkflowViewElement CreateDetachedViewElement(ModelItem modelItem)
        {
            WorkflowViewElement viewElement = CreateViewElement(modelItem);

            // now we successfully got a viewElement, lets initialize it with ModelItem;
            if (viewElement != null)
            {
                viewElement.Context = this.context;
                viewElement.ModelItem = modelItem;
                viewElement.DataContext = viewElement;
            }

            return viewElement;
        }

        public override ModelItem GetModel(DependencyObject view)
        {
            if (view == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("view"));
            }

            if (view is WorkflowViewElement)
            {
                return ((WorkflowViewElement)view).ModelItem;
            }

            Fx.Assert("we should know if somebody is trying to get model item from thing other than WorkflowViewElement");
            return null;
        }

        public override DependencyObject GetView(ModelItem model)
        {
            return GetViewElement(model);
        }

        internal bool ShouldAppearOnBreadCrumb(ModelItem modelItem, bool checkIfCanBeMadeRoot)
        {
            bool shouldAppearOnBreadCrumb = false;
            if (modelItem != null)
            {
                Type designerType = this.GetDesignerType(modelItem.ItemType);
                if (null != designerType)
                {
                    if (checkIfCanBeMadeRoot)
                    {
                        ActivityDesignerOptionsAttribute options = WorkflowViewService.GetAttribute<ActivityDesignerOptionsAttribute>(modelItem.ItemType);
                        shouldAppearOnBreadCrumb = (typeof(WorkflowViewElement).IsAssignableFrom(designerType) &&
                                                   (typeof(ActivityDesigner) != designerType || ActivityDelegateUtilities.HasActivityDelegate(modelItem.ItemType)) &&
                                                   typeof(WorkflowService) != designerType &&
                                                   (options == null || options.AllowDrillIn));
                    }
                    else
                    {
                        shouldAppearOnBreadCrumb = typeof(WorkflowViewElement).IsAssignableFrom(designerType);
                    }
                }
            }
            return shouldAppearOnBreadCrumb;

        }   
    }
}
