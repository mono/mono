//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.View
{
    using System.Windows;

    using System.Windows.Media;
    using System.Windows.Media.Effects;
    using System.Windows.Documents;

    using System.Activities.Presentation;
    using System.Activities.Presentation.View;
    using System.Activities.Presentation.Hosting;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.Documents;
    using System.Activities.Presentation.Services;
    using System.Collections.ObjectModel;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime;
    using System.ComponentModel.Design;
    using System.Activities.Presentation.Annotations;


    internal class WorkflowViewManager : ViewManager
    {
        const string KeywordForWorkflowDesignerHomePage = "DefaultWorkflowDesigner";
        EditingContext context;
        Selection oldSelection;
        ModelService modelService;
        System.Activities.Presentation.View.DesignerView view;
        AttachedProperty<bool> isPrimarySelectionProperty;
        AttachedProperty<bool> isSelectionProperty;
        IIntegratedHelpService helpService;

        public override System.Windows.Media.Visual View
        {
            get
            {
                return view;
            }
        }
        public override void Initialize(EditingContext context)
        {
            this.context = context;
            AttachedPropertiesService propertiesService = this.context.Services.GetService<AttachedPropertiesService>();
            helpService = this.context.Services.GetService<IIntegratedHelpService>();

            oldSelection = this.context.Items.GetValue<Selection>();
            isPrimarySelectionProperty = new AttachedProperty<bool>()
                {
                    Getter = (modelItem) => (this.context.Items.GetValue<Selection>().PrimarySelection == modelItem),
                    Name = "IsPrimarySelection",
                    OwnerType = typeof(Object)
                };

            isSelectionProperty = new AttachedProperty<bool>()
            {
                Getter = (modelItem) => (((IList)this.context.Items.GetValue<Selection>().SelectedObjects).Contains(modelItem)),
                Name = "IsSelection",
                OwnerType = typeof(Object)
            };


            propertiesService.AddProperty(isPrimarySelectionProperty);
            propertiesService.AddProperty(isSelectionProperty);
            



            if (this.context.Services.GetService<ViewService>() == null)
            {
                view = new System.Activities.Presentation.View.DesignerView(this.context);
                WorkflowViewService viewService = new WorkflowViewService(context);
                WorkflowViewStateService viewStateService = new WorkflowViewStateService(context);
                this.context.Services.Publish<ViewService>(viewService);
                this.context.Services.Publish<VirtualizedContainerService>(new VirtualizedContainerService(this.context));
                this.context.Services.Publish<ViewStateService>(viewStateService);
                this.context.Services.Publish<DesignerView>(view);

                WorkflowAnnotationAdornerService annotationService = new WorkflowAnnotationAdornerService();
                annotationService.Initialize(this.context, view.scrollViewer);
                this.context.Services.Publish<AnnotationAdornerService>(annotationService);

                this.context.Services.Subscribe<ModelService>(delegate(ModelService modelService)
                {
                    this.modelService = modelService;
                    if (modelService.Root != null)
                    {
                        view.MakeRootDesigner(modelService.Root);
                    }
                    view.RestoreDesignerStates();
                    this.context.Items.Subscribe<Selection>(new SubscribeContextCallback<Selection>(OnItemSelected));
                });
            }

            if (helpService != null)
            {
                helpService.AddContextAttribute(string.Empty, KeywordForWorkflowDesignerHomePage, HelpKeywordType.F1Keyword); 
            }
        }

        internal static string GetF1HelpTypeKeyword(Type type)
        {
            Fx.Assert(type != null, "type is null");
            if (type.IsGenericType)
            {
                Type genericTypeDefinition = type.GetGenericTypeDefinition();
                return genericTypeDefinition.FullName;
            }
            return type.FullName;
        }

        void OnItemSelected(Selection newSelection)
        {
            Fx.Assert(newSelection != null, "newSelection is null");
            IList<ModelItem> newSelectionObjects = newSelection.SelectedObjects as IList<ModelItem>;
            IList<ModelItem> oldSelectionObjects = oldSelection.SelectedObjects as IList<ModelItem>;

            //Call notifyPropertyChanged for IsPrimarySelection attached property.
            if (newSelection.PrimarySelection != null && !newSelection.PrimarySelection.Equals(oldSelection.PrimarySelection))
            {
                isPrimarySelectionProperty.NotifyPropertyChanged(oldSelection.PrimarySelection);
                isPrimarySelectionProperty.NotifyPropertyChanged(newSelection.PrimarySelection);
            }
            else if (newSelection.PrimarySelection == null)
            {
                isPrimarySelectionProperty.NotifyPropertyChanged(oldSelection.PrimarySelection);
            }


            //call NotifyPropertyChanged for IsSelection property on ModelItems that were added or removed from selection.
            HashSet<ModelItem> selectionChangeSet = new HashSet<ModelItem>(oldSelectionObjects);
            selectionChangeSet.SymmetricExceptWith(newSelectionObjects);
            foreach (ModelItem selectionChangeMI in selectionChangeSet)
            {
                isSelectionProperty.NotifyPropertyChanged(selectionChangeMI);
            }
                        
            if (helpService != null)
            {
                if (oldSelection.PrimarySelection != null)
                {         
                    helpService.RemoveContextAttribute(string.Empty, GetF1HelpTypeKeyword(oldSelection.PrimarySelection.ItemType));
                }

                if (newSelection.PrimarySelection != null)
                {
                    helpService.AddContextAttribute(string.Empty, GetF1HelpTypeKeyword(newSelection.PrimarySelection.ItemType), HelpKeywordType.F1Keyword);
                }
            }
            oldSelection = newSelection;
        }


    }


}
