//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System.Activities.Presentation.Model;
    using System.Runtime;

    internal class EditingScopeUndoUnit : UndoUnit
    {
        private ModelTreeManager modelTreeManager;
        private EditingScope editingScope;
        private EditingContext context;

        public EditingScopeUndoUnit(EditingContext context, ModelTreeManager modelTreeManager, EditingScope editingScope)
            : base(context)
        {
            Fx.Assert(context != null, "context cannot be null");
            Fx.Assert(modelTreeManager != null, "modelTreeManager cannot be null");
            Fx.Assert(editingScope != null, "editingScope cannot be null");

            this.context = context;
            this.modelTreeManager = modelTreeManager;
            this.editingScope = editingScope;
            this.Description = this.editingScope.Description;

            SaveGlobalState();
        }

        public override void Redo()
        {
            this.modelTreeManager.StopTracking();
            try
            {
                EditingScope redoEditingScope = this.modelTreeManager.CreateEditingScope(this.editingScope.Description);
                redoEditingScope.Changes.AddRange(editingScope.Changes);
                redoEditingScope.Complete();
            }
            finally
            {
                this.modelTreeManager.StartTracking();
            }
            ApplyGlobalState();
        }

        public override void Undo()
        {
            this.modelTreeManager.StopTracking();
            try
            {
                EditingScope undoEditingScope = this.modelTreeManager.CreateEditingScope(this.editingScope.Description);
                foreach (Change change in editingScope.Changes)
                {
                    Change inverseChange = change.GetInverse();
                    if (inverseChange != null)
                    {
                        undoEditingScope.Changes.Add(inverseChange);
                    }
                }
                undoEditingScope.Changes.Reverse();
                undoEditingScope.Complete();
            }
            finally
            {
                this.modelTreeManager.StartTracking();
            }
            ApplyGlobalState();
        }
    }
}
