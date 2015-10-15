//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Activities.Presentation.FreeFormEditing
{
    interface INestedFreeFormPanelContainer
    {
        FreeFormPanel GetChildFreeFormPanel();
        FreeFormPanel GetOutmostFreeFormPanel();
    }
}