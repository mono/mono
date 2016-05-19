//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System;
    using System.Activities.Debugger;
    using System.Activities.Debugger.Symbol;
    using System.Activities.Presentation.View;
    using System.Activities.Presentation.Xaml;
    using System.Activities.XamlIntegration;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Runtime;
    using System.Runtime.Versioning;
    using System.Xaml;
    using System.Xml;
    using Microsoft.Activities.Presentation.Xaml;
    
    public partial class WorkflowDesigner : IWorkflowDesignerXamlHelperExecutionContext
    {
        FrameworkName IWorkflowDesignerXamlHelperExecutionContext.FrameworkName
        {
            get { return WorkflowDesigner.GetTargetFramework(this.context); }
        }

        WorkflowDesignerXamlSchemaContext IWorkflowDesignerXamlHelperExecutionContext.XamlSchemaContext
        {
            get { return this.XamlSchemaContext; }
        }

        ViewStateIdManager IWorkflowDesignerXamlHelperExecutionContext.IdManager
        {
            get { return this.idManager; }
        }

        WorkflowSymbol IWorkflowDesignerXamlHelperExecutionContext.LastWorkflowSymbol
        {
            get { return this.lastWorkflowSymbol; }
            set { this.lastWorkflowSymbol = value; }
        }

        void IWorkflowDesignerXamlHelperExecutionContext.OnSerializationCompleted(Dictionary<object, object> sourceLocationObjectToModelItemObjectMapping)
        {
            this.ObjectToSourceLocationMapping.SourceLocationObjectToModelItemObjectMapping = sourceLocationObjectToModelItemObjectMapping;
            this.ObjectReferenceService.OnSaveCompleted();
        }

        void IWorkflowDesignerXamlHelperExecutionContext.OnBeforeDeserialize()
        {
            this.ObjectToSourceLocationMapping.Clear();
        }

        void IWorkflowDesignerXamlHelperExecutionContext.OnSourceLocationFound(object target, SourceLocation sourceLocationFound)
        {
            this.ObjectToSourceLocationMapping.UpdateMap(target, sourceLocationFound);
        }

        void IWorkflowDesignerXamlHelperExecutionContext.OnAfterDeserialize(Dictionary<string, SourceLocation> viewStateDataSourceLocationMapping)
        {
            this.ObjectToSourceLocationMapping.ViewStateDataSourceLocationMapping = viewStateDataSourceLocationMapping;
        }

        string IWorkflowDesignerXamlHelperExecutionContext.LocalAssemblyName
        {
            get { return this.GetLocalAssemblyName(); }
        }

        WorkflowDesignerXamlSchemaContext XamlSchemaContext
        {
            get
            {
                if (this.workflowDesignerXamlSchemaContext == null)
                {
                    this.workflowDesignerXamlSchemaContext = new WorkflowDesignerXamlSchemaContext(this.GetLocalAssemblyName(), this.Context);
                }

                return this.workflowDesignerXamlSchemaContext;
            }
        }

        internal object DeserializeString(string text)
        {
            return new WorkflowDesignerXamlHelper(this).DeserializeString(text);
        }

        internal object DeserializeString(string text, out IList<XamlLoadErrorInfo> loadErrors, out Dictionary<object, SourceLocation> sourceLocations)
        {
            return new WorkflowDesignerXamlHelper(this).DeserializeString(text, out loadErrors, out sourceLocations);
        }

        internal string SerializeToString(object obj, string fileName = null)
        {
            return new WorkflowDesignerXamlHelper(this).SerializeToString(obj, fileName);
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DoNotCatchGeneralExceptionTypes,
            Justification = "Serializer might throw if it fails to serialize. Catching all exceptions to avoid VS Crash.")]
        [SuppressMessage("Reliability", "Reliability108",
            Justification = "Serializer might throw if it fails to serialize. Catching all exceptions to avoid VS crash.")]
        void WriteModelToText(string fileName)
        {
            this.perfEventProvider.WorkflowDesignerSerializeStart();
            object rootModelObject = this.modelTreeManager.Root.GetCurrentValue();
            // if we are serializing a activity schema type, remove the namespace in the Name property.
            ActivityBuilder activityBuilderType = rootModelObject as ActivityBuilder;

            // now try to serialize
            try
            {
                string newText = SerializeToString(rootModelObject, fileName);
                if (string.IsNullOrEmpty(this.Text) ||
                    (this.isModelChanged && !string.Equals(newText, this.Text, StringComparison.Ordinal)))
                {
                    this.Text = newText;
                    if (this.TextChanged != null)
                    {
                        this.TextChanged.Invoke(this, null);
                    }
                }
                this.isModelChanged = false;
            }
            catch (Exception e)
            {
                this.Context.Items.SetValue(new ErrorItem() { Message = e.Message, Details = e.ToString() });
            }
            this.perfEventProvider.WorkflowDesignerSerializeEnd();
        }

        void RaiseLoadError(Exception e)
        {
            if (this.xamlLoadErrorService != null)
            {
                XamlLoadErrorInfo errorInfo = null;
                XamlException xamlEx = e as XamlException;
                if (xamlEx != null)
                {
                    errorInfo = new XamlLoadErrorInfo(xamlEx.Message, xamlEx.LineNumber, xamlEx.LinePosition);
                }
                else
                {
                    XmlException xmlEx = e as XmlException;
                    if (xmlEx != null)
                    {
                        errorInfo = new XamlLoadErrorInfo(xmlEx.Message, xmlEx.LineNumber, xmlEx.LinePosition);
                    }
                }
                if (errorInfo != null)
                {
                    var errors = new XamlLoadErrorInfo[] { errorInfo };
                    xamlLoadErrorService.ShowXamlLoadErrors(errors);
                }
            }
        }

        void RaiseLoadErrors(IList<XamlLoadErrorInfo> loadErrors)
        {
            if (this.xamlLoadErrorService != null)
            {
                this.xamlLoadErrorService.ShowXamlLoadErrors(loadErrors);
            }
        }
    }
}
