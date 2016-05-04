// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace Microsoft.Activities.Presentation.Xaml
{
    using System.Activities.Debugger;
    using System.Activities.Debugger.Symbol;
    using System.Collections.Generic;
    using System.Runtime.Versioning;

    internal interface IWorkflowDesignerXamlHelperExecutionContext
    {
        FrameworkName FrameworkName { get; }

        WorkflowDesignerXamlSchemaContext XamlSchemaContext { get; }

        ViewStateIdManager IdManager { get; }

        WorkflowSymbol LastWorkflowSymbol { get; set; }

        string LocalAssemblyName { get; }

        void OnSerializationCompleted(Dictionary<object, object> sourceLocationObjectToModelItemObjectMapping);

        void OnBeforeDeserialize();

        void OnSourceLocationFound(object target, SourceLocation sourceLocation);

        void OnAfterDeserialize(Dictionary<string, SourceLocation> viewStateDataSourceLocationMapping);
    }
}
