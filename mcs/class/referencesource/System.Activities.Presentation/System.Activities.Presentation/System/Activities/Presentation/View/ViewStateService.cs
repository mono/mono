//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.View
{
    using System.Activities.Presentation.Model;
    using System.Collections.Generic;

    public abstract class ViewStateService
    {
        public abstract event ViewStateChangedEventHandler ViewStateChanged;
        public abstract event ViewStateChangedEventHandler UndoableViewStateChanged;
        public abstract object RetrieveViewState(ModelItem modelItem, string key);
        public abstract void StoreViewState(ModelItem modelItem, string key, object value);
        public abstract void StoreViewStateWithUndo(ModelItem modelItem, string key, object value);
        public abstract Dictionary<string, object> RetrieveAllViewState(ModelItem modelItem);
        public abstract bool RemoveViewState(ModelItem modelItem, string key);
    }
}
