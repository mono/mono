//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation.Model
{
    using System.Activities.Presentation.Hosting;
    using System.Runtime;

    internal class ImmediateEditingScope : EditingScope
    {
        private ModelTreeManager modelTreeManager;
        private UndoEngine.Bookmark undoEngineBookmark;

        public ImmediateEditingScope(ModelTreeManager modelTreeManager, UndoEngine.Bookmark undoEngineBookmark)
            : base(modelTreeManager, null)
        {
            Fx.Assert(modelTreeManager != null, "modelTreeManager should never be null!");
            Fx.Assert(undoEngineBookmark != null, "undoEngineBookmark should never be null!");
            this.modelTreeManager = modelTreeManager;
            this.undoEngineBookmark = undoEngineBookmark;
        }

        protected override void OnComplete()
        {
            this.undoEngineBookmark.CommitBookmark();
            this.undoEngineBookmark = null;
            this.modelTreeManager.OnEditingScopeCompleted(this);
        }

        protected override void OnRevert(bool finalizing)
        {
            this.undoEngineBookmark.RollbackBookmark();
            this.undoEngineBookmark = null;
            this.modelTreeManager.OnEditingScopeReverted(this);
        }

        protected override bool CanComplete()
        {
            ReadOnlyState readOnlyState = this.modelTreeManager.Context.Items.GetValue<ReadOnlyState>();
            return readOnlyState == null || !readOnlyState.IsReadOnly;
        }
    }
}
