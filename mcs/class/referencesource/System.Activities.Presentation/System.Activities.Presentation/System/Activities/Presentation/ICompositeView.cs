//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System.Collections.Generic;
    using System.Windows;
    using System.Activities.Presentation.Model;
using System.Activities.Presentation.View;

    public interface ICompositeView
    {
        void OnItemMoved(ModelItem modelItem);
        object OnItemsCut(List<ModelItem> itemsToCut);
        object OnItemsCopied(List<ModelItem> itemsToCopy);
        void OnItemsPasted(List<object> itemsToPaste, List<object> metadata, Point pastePoint, WorkflowViewElement pastePointReference);
        void OnItemsDelete(List<ModelItem> itemsToDelete);
        bool CanPasteItems(List<object> itemsToPaste);

        bool IsDefaultContainer { get; }
        TypeResolvingOptions DroppingTypeResolvingOptions { get; }
    }
}
