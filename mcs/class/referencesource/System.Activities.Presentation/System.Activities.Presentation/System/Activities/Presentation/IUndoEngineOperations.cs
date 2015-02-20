//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation
{
    interface IUndoEngineOperations
    {
        void AddUndoUnitCore(UndoUnit unit);
        bool UndoCore();
        bool RedoCore();
    }
}
