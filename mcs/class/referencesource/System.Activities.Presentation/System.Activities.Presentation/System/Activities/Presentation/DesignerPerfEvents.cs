//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation
{
    enum DesignerPerfEvents
    {
        WorkflowDesignerLoadStart = 0,
        WorkflowDesignerLoadComplete,
        WorkflowDesignerDeserializeStart,
        WorkflowDesignerDeserializeEnd,
        WorkflowDesignerApplicationIdleAfterLoad,
        WorkflowDesignerSerializeStart,
        WorkflowDesignerSerializeEnd,
        WorkflowDesignerDrop,
        WorkflowDesignerIdleAfterDrop,
        WorkflowDesignerExpressionEditorLoadStart,
        WorkflowDesignerExpressionEditorLoaded,
        WorkflowDesignerExpressionEditorCompilationStart,
        WorkflowDesignerExpressionEditorCompilationEnd,
        WorkflowDesignerValidationStart,
        WorkflowDesignerValidationEnd,
        FlowchartDesignerLoadEnd,
        FlowchartDesignerLoadStart,
        FreeFormPanelMeasureStart,
        FreeFormPanelMeasureEnd,
        WorkflowDesignerCopyStart,
        WorkflowDesignerCopyEnd,
        WorkflowDesignerPasteStart,
        WorkflowDesignerPasteEnd,
        DesignerTreeViewLoadChildrenStart,
        DesignerTreeViewLoadChildrenEnd,
        DesignerTreeViewUpdateStart,
        DesignerTreeViewUpdateEnd,
        DesignerTreeViewExpandStart,
        DesignerTreeViewExpandEnd,
        TypeBrowserApplicationIdleAfterShowDialog,
        TypeBrowserOkPressed,
        SelectionChangedStart,
        PropertyInspectorUpdatePropertyListStart,
        PropertyInspectorUpdatePropertyListEnd,
    };
}
