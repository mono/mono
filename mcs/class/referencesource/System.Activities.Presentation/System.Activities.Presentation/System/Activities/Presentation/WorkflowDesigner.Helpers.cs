//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------


namespace System.Activities.Presentation
{
    using System;
    using System.Activities.Presentation.Documents;
    using System.Activities.Presentation.Hosting;
    using System.Activities.Presentation.Internal.PropertyEditing.Resources;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.Validation;
    using System.Activities.Presentation.View;
    using System.ComponentModel;
    using System.Runtime;
    using System.Runtime.Versioning;
    using System.Windows;

    public partial class WorkflowDesigner
    {
        static internal FrameworkName GetTargetFramework(EditingContext context)
        {
            if (context != null)
            {
                DesignerConfigurationService designerConfigurationService = context.Services.GetService<DesignerConfigurationService>();
                if (designerConfigurationService != null)
                {
                    return designerConfigurationService.TargetFrameworkName;
                }
            }

            return DesignerConfigurationService.DefaultTargetFrameworkName;
        }


        string GetLocalAssemblyName()
        {
            AssemblyContextControlItem assemblyItem = this.Context.Items.GetValue<AssemblyContextControlItem>();
            return assemblyItem != null && assemblyItem.LocalAssemblyName != null ? assemblyItem.LocalAssemblyName.Name : null;
        }


        ViewManager GetViewManager(ModelItem modelItem)
        {
            Fx.Assert(modelItem != null, "modelItem cannot be null");
            ViewManager viewManager = null;
            // First we look for a ViewManagerAttribute. for example a ServiceContractRoot tag, could use
            // use its own view manager if it wanted to .
            ViewManagerAttribute viewManagerAttribute = TypeDescriptor.GetAttributes(modelItem.ItemType)[typeof(ViewManagerAttribute)] as ViewManagerAttribute;
            if (viewManagerAttribute != null && viewManagerAttribute.ViewManagerType != null)
            {
                viewManager = (ViewManager)Activator.CreateInstance(viewManagerAttribute.ViewManagerType);

            }
            // If no viewmanager attribute is found we default to the workflowviewmanager
            if (viewManager == null)
            {
                viewManager = new WorkflowViewManager();
            }
            viewManager.Initialize(this.context);
            return viewManager;
        }


        object GetRootInstance()
        {
            return this.modelTreeManager.Root.GetCurrentValue();
        }


        void InitializePropertyInspectorCommandHandling()
        {
        }

        void InitializePropertyInspectorResources()
        {
            this.propertyInspector.Resources.MergedDictionaries.Add(PropertyInspectorResources.GetResources());
        }

        //This is to notify VS that the WF Designer is in Modal state (eg. when a modal dialog is shown).
        void ComponentDispatcher_EnterThreadModal(object sender, EventArgs e)
        {
            IModalService modalService = Context.Services.GetService<IModalService>();
            if (modalService != null)
            {
                modalService.SetModalState(true);
            }
        }

        void ComponentDispatcher_LeaveThreadModal(object sender, EventArgs e)
        {
            IModalService modalService = Context.Services.GetService<IModalService>();
            if (modalService != null)
            {
                modalService.SetModalState(false);
            }
        }

        void OnUndoCompleted(object sender, UndoUnitEventArgs e)
        {
            // If an action had caused the errorview to be shown, and undo was executed after that 
            // try to put back the viewmanagerview back as the rootview of the designer.
            // may be the undo might help recover from the problem.
            if (!this.view.Children.Contains((UIElement)this.viewManager.View))
            {
                this.view.Children.Clear();
                this.view.Children.Add((UIElement)this.viewManager.View);

                if (this.outlineView != null)
                {
                    this.outlineView.Children.Clear();
                    this.AddOutlineView();
                }

                // Clear out the error condition
                ErrorItem errorItem = this.context.Items.GetValue<ErrorItem>();
                errorItem.Message = null;
                errorItem.Details = null;
            }
        }


        void OnViewStateChanged(object sender, ViewStateChangedEventArgs e)
        {
            NotifyModelChanged();
        }

        void OnEditingScopeCompleted(object sender, EditingScopeEventArgs e)
        {
            if (e.EditingScope.HasEffectiveChanges)
            {
                NotifyModelChanged();

                // The undo unit of an ImmediateEditingScope is added into undo engine in ImmediateEditingScope.Complete
                // so we only handle non ImmediateEditingScope here
                if (!this.modelTreeManager.RedoUndoInProgress
                    && !(e.EditingScope is ImmediateEditingScope)
                    && undoEngine != null
                    && !e.EditingScope.SuppressUndo)
                {
                    undoEngine.AddUndoUnit(new EditingScopeUndoUnit(this.Context, this.modelTreeManager, e.EditingScope));
                }
            }
        }


        void NotifyModelChanged()   // Notify text is going to changed
        {
            IDocumentPersistenceService documentPersistenceService = this.Context.Services.GetService<IDocumentPersistenceService>();
            if (documentPersistenceService != null)
            {
                documentPersistenceService.OnModelChanged(this.modelTreeManager.Root.GetCurrentValue());
            }
            else
            {
                this.isModelChanged = true;
                if (this.ModelChanged != null)
                {
                    this.ModelChanged.Invoke(this, null);
                }
            }
        }


        void OnReadonlyStateChanged(ReadOnlyState state)
        {
            if (null != this.propertyInspector)
            {
                this.propertyInspector.IsReadOnly = state.IsReadOnly;
            }
        }
    }
}
