//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System;
    using System.Activities.Debugger.Symbol;
    using System.Activities.Presentation.Debug;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.Xaml;
    using System.Runtime;
    using System.Xaml;

    public partial class WorkflowDesigner
    {
        public IDesignerDebugView DebugManagerView
        {
            get
            {
                return this.DebuggerService;
            }
        }

        DebuggerService DebuggerService
        {
            get
            {
                if (this.debuggerService == null)
                {
                    this.debuggerService = new DebuggerService(this.context);
                    this.context.Services.Publish<IDesignerDebugView>(this.debuggerService);
                }
                return this.debuggerService;
            }
        }

        ModelSearchServiceImpl ModelSearchService
        {
            get;
            set;
        }

        internal ObjectToSourceLocationMapping ObjectToSourceLocationMapping
        {
            get
            {
                if (this.objectToSourceLocationMapping == null)
                {
                    this.objectToSourceLocationMapping = new ObjectToSourceLocationMapping(this.ModelSearchService);
                }
                return this.objectToSourceLocationMapping;
            }
        }

        // Get the attached workflow symbol and remove it from the root.
        WorkflowSymbol GetAttachedWorkflowSymbol()
        {
            object rootInstance = this.GetRootInstance();
            WorkflowSymbol wfSymbol = null;

            if (rootInstance != null)
            {
                Activity documentRootElement = GetRootWorkflowElement(rootInstance);
                if (documentRootElement != null)
                {
                    string symbolString;
                    if (AttachablePropertyServices.TryGetProperty<string>(documentRootElement, DebugSymbol.SymbolName, out symbolString))
                    {
                        try
                        {
                            wfSymbol = WorkflowSymbol.Decode(symbolString);
                            // Change the name to the currently loaded file.
                            wfSymbol.FileName = this.Context.Items.GetValue<WorkflowFileItem>().LoadedFile;
                        }
                        catch (Exception ex)
                        {
                            if (Fx.IsFatal(ex))
                            {
                                throw;
                            }
                        }
                        finally
                        {
                            AttachablePropertyServices.RemoveProperty(documentRootElement, DebugSymbol.SymbolName);
                        }
                    }
                }
            }
            return wfSymbol;
        }
    }
}
