//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Activities.Presentation.View
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Windows.Controls;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.Hosting;
    using System.Windows;

    public interface IExpressionEditorService
    {
        IExpressionEditorInstance CreateExpressionEditor(AssemblyContextControlItem assemblies, ImportedNamespaceContextItem importedNamespaces, List<ModelItem> variables, string text, Type expressionType);
        IExpressionEditorInstance CreateExpressionEditor(AssemblyContextControlItem assemblies, ImportedNamespaceContextItem importedNamespaces, List<ModelItem> variables, string text, Type expressionType, Size initialSize);
        IExpressionEditorInstance CreateExpressionEditor(AssemblyContextControlItem assemblies, ImportedNamespaceContextItem importedNamespaces, List<ModelItem> variables, string text);
        IExpressionEditorInstance CreateExpressionEditor(AssemblyContextControlItem assemblies, ImportedNamespaceContextItem importedNamespaces, List<ModelItem> variables, string text, Size initialSize);
        void CloseExpressionEditors(); // Closes all editors
        void UpdateContext(AssemblyContextControlItem assemblies, ImportedNamespaceContextItem importedNamespaces);
    }
}
